using Amazon.S3;
using Amazon.S3.Model;
using Octokit;
using System.CommandLine;
using System.IO.Compression;
using System.Text.Json;

namespace Publisher;

public class Program
{
    public static async Task<int> Main(string[] args)
    {
        var rootCommand = new RootCommand("Blazed Odyssey Publisher");

        var githubCommand = new Command("github", "Publish to GitHub Releases");
        var s3Command = new Command("s3", "Publish to S3-compatible storage");

        // Common options
        var gameDirectoryOption = new Option<DirectoryInfo>("--game-dir", "Game directory to publish") { IsRequired = true };
        var manifestOption = new Option<FileInfo>("--manifest", "Manifest file") { IsRequired = true };
        var versionOption = new Option<string>("--version", "Release version") { IsRequired = true };
        var changelogOption = new Option<FileInfo?>("--changelog", "Changelog file");

        // GitHub options
        var githubTokenOption = new Option<string>("--token", "GitHub access token") { IsRequired = true };
        var repositoryOption = new Option<string>("--repo", "GitHub repository (owner/repo)") { IsRequired = true };
        var preReleaseOption = new Option<bool>("--pre-release", () => false, "Mark as pre-release");
        var draftsOption = new Option<bool>("--draft", () => false, "Create as draft");

        githubCommand.AddOption(gameDirectoryOption);
        githubCommand.AddOption(manifestOption);
        githubCommand.AddOption(versionOption);
        githubCommand.AddOption(changelogOption);
        githubCommand.AddOption(githubTokenOption);
        githubCommand.AddOption(repositoryOption);
        githubCommand.AddOption(preReleaseOption);
        githubCommand.AddOption(draftsOption);

        githubCommand.SetHandler(PublishToGitHub, gameDirectoryOption, manifestOption, versionOption, 
            changelogOption, githubTokenOption, repositoryOption, preReleaseOption, draftsOption);

        // S3 options
        var s3BucketOption = new Option<string>("--bucket", "S3 bucket name") { IsRequired = true };
        var s3RegionOption = new Option<string>("--region", "S3 region") { IsRequired = true };
        var s3AccessKeyOption = new Option<string>("--access-key", "S3 access key") { IsRequired = true };
        var s3SecretKeyOption = new Option<string>("--secret-key", "S3 secret key") { IsRequired = true };
        var s3PrefixOption = new Option<string>("--prefix", () => "", "S3 key prefix");
        var s3EndpointOption = new Option<string?>("--endpoint", "Custom S3 endpoint (for DigitalOcean Spaces, etc.)");

        s3Command.AddOption(gameDirectoryOption);
        s3Command.AddOption(manifestOption);
        s3Command.AddOption(versionOption);
        s3Command.AddOption(changelogOption);
        s3Command.AddOption(s3BucketOption);
        s3Command.AddOption(s3RegionOption);
        s3Command.AddOption(s3AccessKeyOption);
        s3Command.AddOption(s3SecretKeyOption);
        s3Command.AddOption(s3PrefixOption);
        s3Command.AddOption(s3EndpointOption);

        s3Command.SetHandler(PublishToS3, gameDirectoryOption, manifestOption, versionOption,
            changelogOption, s3BucketOption, s3RegionOption, s3AccessKeyOption, s3SecretKeyOption,
            s3PrefixOption, s3EndpointOption);

        rootCommand.AddCommand(githubCommand);
        rootCommand.AddCommand(s3Command);

        return await rootCommand.InvokeAsync(args);
    }

    private static async Task PublishToGitHub(DirectoryInfo gameDirectory, FileInfo manifest, string version,
        FileInfo? changelog, string token, string repository, bool preRelease, bool draft)
    {
        try
        {
            Console.WriteLine($"Publishing version {version} to GitHub repository {repository}...");

            var client = new GitHubClient(new ProductHeaderValue("BlazedOdysseyPublisher"))
            {
                Credentials = new Credentials(token)
            };

            var repoParts = repository.Split('/');
            if (repoParts.Length != 2)
            {
                Console.Error.WriteLine("Repository must be in format 'owner/repo'");
                Environment.Exit(1);
            }

            var owner = repoParts[0];
            var repo = repoParts[1];

            // Read changelog
            var releaseNotes = "";
            if (changelog?.Exists == true)
            {
                releaseNotes = await File.ReadAllTextAsync(changelog.FullName);
            }

            // Create release
            var newRelease = new NewRelease($"v{version}")
            {
                Name = $"Blazed Odyssey v{version}",
                Body = releaseNotes,
                Draft = draft,
                Prerelease = preRelease,
                TargetCommitish = "main"
            };

            Console.WriteLine("Creating GitHub release...");
            var release = await client.Repository.Release.Create(owner, repo, newRelease);
            Console.WriteLine($"Created release: {release.HtmlUrl}");

            // Upload manifest
            Console.WriteLine("Uploading manifest...");
            await UploadAssetToRelease(client, owner, repo, release.Id, manifest.FullName, "application/json");

            // Upload manifest signature if it exists
            var signatureFile = Path.ChangeExtension(manifest.FullName, ".sig");
            if (File.Exists(signatureFile))
            {
                Console.WriteLine("Uploading manifest signature...");
                await UploadAssetToRelease(client, owner, repo, release.Id, signatureFile, "application/octet-stream");
            }

            // Create and upload game archive
            var archivePath = await CreateGameArchive(gameDirectory, version);
            try
            {
                Console.WriteLine("Uploading game archive...");
                await UploadAssetToRelease(client, owner, repo, release.Id, archivePath, "application/zip");
                Console.WriteLine("✓ Release published successfully!");
            }
            finally
            {
                // Cleanup
                if (File.Exists(archivePath))
                {
                    File.Delete(archivePath);
                }
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error publishing to GitHub: {ex.Message}");
            Environment.Exit(1);
        }
    }

    private static async Task PublishToS3(DirectoryInfo gameDirectory, FileInfo manifest, string version,
        FileInfo? changelog, string bucket, string region, string accessKey, string secretKey,
        string prefix, string? endpoint)
    {
        try
        {
            Console.WriteLine($"Publishing version {version} to S3 bucket {bucket}...");

            var config = new AmazonS3Config
            {
                RegionEndpoint = Amazon.RegionEndpoint.GetBySystemName(region)
            };

            if (!string.IsNullOrEmpty(endpoint))
            {
                config.ServiceURL = endpoint;
                config.ForcePathStyle = true;
            }

            using var s3Client = new AmazonS3Client(accessKey, secretKey, config);

            var versionPrefix = $"{prefix.TrimEnd('/')}/{version}".TrimStart('/');

            // Upload manifest
            Console.WriteLine("Uploading manifest...");
            await UploadFileToS3(s3Client, bucket, $"{versionPrefix}/manifest.json", manifest.FullName, "application/json");

            // Upload manifest signature if it exists
            var signatureFile = Path.ChangeExtension(manifest.FullName, ".sig");
            if (File.Exists(signatureFile))
            {
                Console.WriteLine("Uploading manifest signature...");
                await UploadFileToS3(s3Client, bucket, $"{versionPrefix}/manifest.sig", signatureFile, "application/octet-stream");
            }

            // Upload changelog if provided
            if (changelog?.Exists == true)
            {
                Console.WriteLine("Uploading changelog...");
                await UploadFileToS3(s3Client, bucket, $"{versionPrefix}/changelog.md", changelog.FullName, "text/markdown");
            }

            // Upload individual game files
            Console.WriteLine("Uploading game files...");
            await UploadGameFiles(s3Client, bucket, versionPrefix, gameDirectory, manifest);

            // Update latest manifest pointer
            Console.WriteLine("Updating latest manifest pointer...");
            await UploadFileToS3(s3Client, bucket, $"{prefix.TrimEnd('/')}/manifest.json".TrimStart('/'), 
                manifest.FullName, "application/json");

            Console.WriteLine("✓ Published to S3 successfully!");
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error publishing to S3: {ex.Message}");
            Environment.Exit(1);
        }
    }

    private static async Task<string> CreateGameArchive(DirectoryInfo gameDirectory, string version)
    {
        var archivePath = Path.Combine(Path.GetTempPath(), $"BlazedOdyssey_v{version}.zip");
        
        Console.WriteLine($"Creating game archive: {archivePath}");
        
        if (File.Exists(archivePath))
        {
            File.Delete(archivePath);
        }

        ZipFile.CreateFromDirectory(gameDirectory.FullName, archivePath, CompressionLevel.Optimal, false);
        
        var fileInfo = new FileInfo(archivePath);
        Console.WriteLine($"Archive created: {fileInfo.Length / 1024 / 1024:F1} MB");
        
        return archivePath;
    }

    private static async Task UploadAssetToRelease(GitHubClient client, string owner, string repo, int releaseId, 
        string filePath, string contentType)
    {
        using var fileStream = File.OpenRead(filePath);
        var fileName = Path.GetFileName(filePath);
        
        var asset = new ReleaseAssetUpload
        {
            FileName = fileName,
            ContentType = contentType,
            RawData = fileStream
        };

        await client.Repository.Release.UploadAsset(owner, repo, releaseId, asset);
        Console.WriteLine($"✓ Uploaded: {fileName}");
    }

    private static async Task UploadFileToS3(AmazonS3Client client, string bucket, string key, string filePath, string contentType)
    {
        var request = new PutObjectRequest
        {
            BucketName = bucket,
            Key = key,
            FilePath = filePath,
            ContentType = contentType,
            CannedACL = S3CannedACL.PublicRead
        };

        await client.PutObjectAsync(request);
        Console.WriteLine($"✓ Uploaded: {key}");
    }

    private static async Task UploadGameFiles(AmazonS3Client client, string bucket, string versionPrefix, 
        DirectoryInfo gameDirectory, FileInfo manifestFile)
    {
        var manifestJson = await File.ReadAllTextAsync(manifestFile.FullName);
        var manifest = JsonSerializer.Deserialize<GameManifest>(manifestJson, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        if (manifest?.Files == null)
        {
            throw new InvalidOperationException("Invalid manifest file");
        }

        var totalFiles = manifest.Files.Count;
        var uploadedFiles = 0;

        foreach (var file in manifest.Files)
        {
            var localPath = Path.Combine(gameDirectory.FullName, file.Path.Replace('/', Path.DirectorySeparatorChar));
            var s3Key = $"{versionPrefix}/{file.Path}";

            if (!File.Exists(localPath))
            {
                Console.WriteLine($"⚠️  File not found: {file.Path}");
                continue;
            }

            // Determine content type
            var contentType = GetContentType(file.Path);

            await UploadFileToS3(client, bucket, s3Key, localPath, contentType);
            
            uploadedFiles++;
            Console.Write($"\rUploaded: {uploadedFiles}/{totalFiles} files");
        }

        Console.WriteLine();
    }

    private static string GetContentType(string filePath)
    {
        var extension = Path.GetExtension(filePath).ToLower();
        return extension switch
        {
            ".exe" => "application/octet-stream",
            ".dll" => "application/octet-stream",
            ".zip" => "application/zip",
            ".json" => "application/json",
            ".txt" => "text/plain",
            ".md" => "text/markdown",
            ".png" => "image/png",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".ogg" => "audio/ogg",
            ".wav" => "audio/wav",
            _ => "application/octet-stream"
        };
    }
}

public class GameManifest
{
    public string Version { get; set; } = string.Empty;
    public string Channel { get; set; } = "stable";
    public string BaseUrl { get; set; } = string.Empty;
    public List<GameFile> Files { get; set; } = new();
}

public class GameFile
{
    public string Path { get; set; } = string.Empty;
    public long Size { get; set; }
    public string Sha256 { get; set; } = string.Empty;
}