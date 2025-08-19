using NSec.Cryptography;
using System.CommandLine;
using System.Security.Cryptography;
using System.Text.Json;

namespace ManifestGenerator;

public class Program
{
    public static async Task<int> Main(string[] args)
    {
        var rootCommand = new RootCommand("Blazed Odyssey Manifest Generator");

        var generateCommand = new Command("generate", "Generate a manifest from a game directory");
        var inputOption = new Option<DirectoryInfo>("--input", "Input game directory") { IsRequired = true };
        var outputOption = new Option<FileInfo>("--output", "Output manifest file") { IsRequired = true };
        var versionOption = new Option<string>("--version", "Game version") { IsRequired = true };
        var baseUrlOption = new Option<string>("--base-url", "Base URL for downloads") { IsRequired = true };
        var channelOption = new Option<string>("--channel", () => "stable", "Release channel");
        var executableOption = new Option<string>("--executable", () => "BlazedOdyssey.exe", "Game executable name");
        var signOption = new Option<bool>("--sign", () => false, "Sign the manifest");
        var privateKeyOption = new Option<FileInfo?>("--private-key", "Private key file for signing");

        generateCommand.AddOption(inputOption);
        generateCommand.AddOption(outputOption);
        generateCommand.AddOption(versionOption);
        generateCommand.AddOption(baseUrlOption);
        generateCommand.AddOption(channelOption);
        generateCommand.AddOption(executableOption);
        generateCommand.AddOption(signOption);
        generateCommand.AddOption(privateKeyOption);

        generateCommand.SetHandler(GenerateManifest, inputOption, outputOption, versionOption, 
            baseUrlOption, channelOption, executableOption, signOption, privateKeyOption);

        var verifyCommand = new Command("verify", "Verify a manifest file");
        var manifestOption = new Option<FileInfo>("--manifest", "Manifest file to verify") { IsRequired = true };
        var gameDirectoryOption = new Option<DirectoryInfo>("--game-dir", "Game directory to verify against") { IsRequired = true };

        verifyCommand.AddOption(manifestOption);
        verifyCommand.AddOption(gameDirectoryOption);
        verifyCommand.SetHandler(VerifyManifest, manifestOption, gameDirectoryOption);

        var keygenCommand = new Command("keygen", "Generate signing key pair");
        var keyOutputOption = new Option<string>("--output", () => "signing_key", "Output key file prefix");
        keygenCommand.AddOption(keyOutputOption);
        keygenCommand.SetHandler(GenerateKeyPair, keyOutputOption);

        rootCommand.AddCommand(generateCommand);
        rootCommand.AddCommand(verifyCommand);
        rootCommand.AddCommand(keygenCommand);

        return await rootCommand.InvokeAsync(args);
    }

    private static async Task GenerateManifest(DirectoryInfo input, FileInfo output, string version, 
        string baseUrl, string channel, string executable, bool sign, FileInfo? privateKey)
    {
        try
        {
            Console.WriteLine($"Generating manifest for {input.FullName}...");
            Console.WriteLine($"Version: {version}");
            Console.WriteLine($"Channel: {channel}");
            Console.WriteLine($"Base URL: {baseUrl}");

            if (!input.Exists)
            {
                Console.Error.WriteLine($"Input directory does not exist: {input.FullName}");
                Environment.Exit(1);
            }

            var manifest = new GameManifest
            {
                Version = version,
                Channel = channel,
                BaseUrl = baseUrl.TrimEnd('/'),
                GameExecutable = executable,
                Files = new List<GameFile>()
            };

            var files = input.GetFiles("*", SearchOption.AllDirectories);
            var processedFiles = 0;

            Console.WriteLine($"Processing {files.Length} files...");

            foreach (var file in files)
            {
                var relativePath = Path.GetRelativePath(input.FullName, file.FullName).Replace('\\', '/');
                
                // Skip certain files
                if (ShouldSkipFile(relativePath))
                {
                    continue;
                }

                Console.Write($"\rProcessing: {relativePath} ({++processedFiles}/{files.Length})");

                var hash = await CalculateFileHashAsync(file.FullName);
                var gameFile = new GameFile
                {
                    Path = relativePath,
                    Size = file.Length,
                    Sha256 = hash,
                    Executable = Path.GetExtension(file.Name).ToLower() == ".exe"
                };

                manifest.Files.Add(gameFile);
            }

            Console.WriteLine($"\nGenerated manifest with {manifest.Files.Count} files");

            // Serialize manifest
            var jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            var json = JsonSerializer.Serialize(manifest, jsonOptions);
            await File.WriteAllTextAsync(output.FullName, json);

            Console.WriteLine($"Manifest written to: {output.FullName}");

            // Sign manifest if requested
            if (sign)
            {
                if (privateKey == null || !privateKey.Exists)
                {
                    Console.Error.WriteLine("Private key file required for signing");
                    Environment.Exit(1);
                }

                var signatureFile = Path.ChangeExtension(output.FullName, ".sig");
                await SignManifest(output.FullName, signatureFile, privateKey.FullName);
                Console.WriteLine($"Signature written to: {signatureFile}");
            }

            Console.WriteLine("Manifest generation completed successfully!");
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error generating manifest: {ex.Message}");
            Environment.Exit(1);
        }
    }

    private static async Task VerifyManifest(FileInfo manifestFile, DirectoryInfo gameDirectory)
    {
        try
        {
            Console.WriteLine($"Verifying manifest: {manifestFile.FullName}");
            Console.WriteLine($"Against directory: {gameDirectory.FullName}");

            if (!manifestFile.Exists)
            {
                Console.Error.WriteLine("Manifest file does not exist");
                Environment.Exit(1);
            }

            if (!gameDirectory.Exists)
            {
                Console.Error.WriteLine("Game directory does not exist");
                Environment.Exit(1);
            }

            var json = await File.ReadAllTextAsync(manifestFile.FullName);
            var manifest = JsonSerializer.Deserialize<GameManifest>(json, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            if (manifest == null)
            {
                Console.Error.WriteLine("Failed to parse manifest");
                Environment.Exit(1);
            }

            var errors = new List<string>();
            var verified = 0;

            foreach (var file in manifest.Files)
            {
                var filePath = Path.Combine(gameDirectory.FullName, file.Path.Replace('/', Path.DirectorySeparatorChar));
                
                if (!File.Exists(filePath))
                {
                    errors.Add($"Missing file: {file.Path}");
                    continue;
                }

                var actualSize = new FileInfo(filePath).Length;
                if (actualSize != file.Size)
                {
                    errors.Add($"Size mismatch for {file.Path}: expected {file.Size}, got {actualSize}");
                    continue;
                }

                var actualHash = await CalculateFileHashAsync(filePath);
                if (!string.Equals(actualHash, file.Sha256, StringComparison.OrdinalIgnoreCase))
                {
                    errors.Add($"Hash mismatch for {file.Path}");
                    continue;
                }

                verified++;
                Console.Write($"\rVerified: {verified}/{manifest.Files.Count} files");
            }

            Console.WriteLine();

            if (errors.Count == 0)
            {
                Console.WriteLine("✓ Manifest verification passed!");
            }
            else
            {
                Console.WriteLine($"✗ Verification failed with {errors.Count} errors:");
                foreach (var error in errors.Take(10))
                {
                    Console.WriteLine($"  - {error}");
                }
                if (errors.Count > 10)
                {
                    Console.WriteLine($"  ... and {errors.Count - 10} more errors");
                }
                Environment.Exit(1);
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error verifying manifest: {ex.Message}");
            Environment.Exit(1);
        }
    }

    private static async Task GenerateKeyPair(string outputPrefix)
    {
        try
        {
            Console.WriteLine("Generating Ed25519 key pair for manifest signing...");

            var algorithm = SignatureAlgorithm.Ed25519;
            using var key = Key.Create(algorithm);

            var privateKeyBytes = key.Export(KeyBlobFormat.RawPrivateKey);
            var publicKeyBytes = key.Export(KeyBlobFormat.RawPublicKey);

            var privateKeyFile = $"{outputPrefix}_private.key";
            var publicKeyFile = $"{outputPrefix}_public.key";

            await File.WriteAllBytesAsync(privateKeyFile, privateKeyBytes);
            await File.WriteAllBytesAsync(publicKeyFile, publicKeyBytes);

            Console.WriteLine($"Private key: {privateKeyFile}");
            Console.WriteLine($"Public key: {publicKeyFile}");
            Console.WriteLine();
            Console.WriteLine("⚠️  IMPORTANT: Keep the private key secure and never commit it to version control!");
            Console.WriteLine("   The public key should be embedded in your launcher application.");
            Console.WriteLine();
            Console.WriteLine("Public key (hex) for launcher:");
            Console.WriteLine(Convert.ToHexString(publicKeyBytes));
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error generating key pair: {ex.Message}");
            Environment.Exit(1);
        }
    }

    private static async Task SignManifest(string manifestPath, string signaturePath, string privateKeyPath)
    {
        try
        {
            var manifestData = await File.ReadAllBytesAsync(manifestPath);
            var privateKeyData = await File.ReadAllBytesAsync(privateKeyPath);

            var algorithm = SignatureAlgorithm.Ed25519;
            using var key = Key.Import(algorithm, privateKeyData, KeyBlobFormat.RawPrivateKey);

            var signature = algorithm.Sign(key, manifestData);
            await File.WriteAllBytesAsync(signaturePath, signature);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to sign manifest: {ex.Message}", ex);
        }
    }

    private static async Task<string> CalculateFileHashAsync(string filePath)
    {
        using var sha256 = SHA256.Create();
        using var stream = File.OpenRead(filePath);
        
        var buffer = new byte[81920];
        int bytesRead;
        
        while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
        {
            sha256.TransformBlock(buffer, 0, bytesRead, null, 0);
        }
        
        sha256.TransformFinalBlock(Array.Empty<byte>(), 0, 0);
        return Convert.ToHexString(sha256.Hash!).ToLowerInvariant();
    }

    private static bool ShouldSkipFile(string relativePath)
    {
        var fileName = Path.GetFileName(relativePath);
        var extension = Path.GetExtension(fileName).ToLower();

        // Skip temporary files, logs, and other non-essential files
        var skipPatterns = new[]
        {
            "manifest.json", "manifest.sig", // Don't include previous manifests
            ".tmp", ".temp", ".log", ".bak", ".old",
            "thumbs.db", "desktop.ini", ".ds_store",
            "crashdumps/", "logs/", "temp/", "cache/"
        };

        return skipPatterns.Any(pattern => 
            relativePath.Contains(pattern, StringComparison.OrdinalIgnoreCase) ||
            fileName.StartsWith(".", StringComparison.Ordinal));
    }
}

public class GameManifest
{
    public string Version { get; set; } = string.Empty;
    public string Channel { get; set; } = "stable";
    public string BaseUrl { get; set; } = string.Empty;
    public List<GameFile> Files { get; set; } = new();
    public string? GameExecutable { get; set; } = "BlazedOdyssey.exe";
    public string? LaunchArguments { get; set; }
    public bool ForceUpdate { get; set; } = false;
}

public class GameFile
{
    public string Path { get; set; } = string.Empty;
    public long Size { get; set; }
    public string Sha256 { get; set; } = string.Empty;
    public DeltaPatch? Delta { get; set; }
    public string? Url { get; set; }
    public bool Compressed { get; set; } = false;
    public bool Optional { get; set; } = false;
    public bool Executable { get; set; } = false;
}

public class DeltaPatch
{
    public string From { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string Sha256 { get; set; } = string.Empty;
    public long Size { get; set; }
}