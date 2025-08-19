using BlazedOdysseyLauncher.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BlazedOdysseyLauncher.Services;

public class UpdateService : IUpdateService
{
    private readonly IDownloadService _downloadService;
    private readonly IManifestService _manifestService;
    private readonly ISecurityService _securityService;
    private readonly ISettingsService _settingsService;

    public event EventHandler<UpdateProgress>? ProgressChanged;
    public event EventHandler<string>? StatusChanged;
    public event EventHandler<Exception>? ErrorOccurred;

    public UpdateService(
        IDownloadService downloadService,
        IManifestService manifestService,
        ISecurityService securityService,
        ISettingsService settingsService)
    {
        _downloadService = downloadService;
        _manifestService = manifestService;
        _securityService = securityService;
        _settingsService = settingsService;

        _downloadService.ProgressChanged += (s, e) => ProgressChanged?.Invoke(this, e);
    }

    public async Task<GameManifest?> CheckForUpdatesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            OnStatusChanged("Checking for updates...");
            
            var settings = _settingsService.LoadSettings();
            var remoteManifest = await _manifestService.DownloadManifestAsync(settings.ManifestUrl, cancellationToken);
            
            if (remoteManifest == null)
            {
                throw new InvalidOperationException("Failed to download remote manifest");
            }

            if (!_manifestService.ValidateManifest(remoteManifest))
            {
                throw new InvalidOperationException("Remote manifest validation failed");
            }

            var localManifest = await _manifestService.LoadLocalManifestAsync();
            
            // If no local manifest or versions differ, update is available
            if (localManifest == null || localManifest.Version != remoteManifest.Version)
            {
                OnStatusChanged($"Update available: {remoteManifest.Version}");
                return remoteManifest;
            }

            OnStatusChanged("Game is up to date");
            return null;
        }
        catch (Exception ex)
        {
            OnErrorOccurred(ex);
            return null;
        }
    }

    public async Task<bool> UpdateGameAsync(GameManifest manifest, CancellationToken cancellationToken = default)
    {
        try
        {
            OnStatusChanged("Starting game update...");
            
            var settings = _settingsService.LoadSettings();
            var gameDir = Path.GetFullPath(settings.GamePath);
            var tempDir = Path.Combine(gameDir, ".temp_update");
            var backupDir = Path.Combine(gameDir, ".backup");

            // Create directories
            Directory.CreateDirectory(tempDir);
            Directory.CreateDirectory(gameDir);

            var totalSize = manifest.Files.Sum(f => f.Size);
            var totalFiles = manifest.Files.Count;
            var completedFiles = 0;
            var totalProgress = 0L;

            OnProgressChanged(new UpdateProgress
            {
                TotalSize = totalSize,
                TotalFiles = totalFiles,
                Status = "Downloading files..."
            });

            // Download files concurrently
            var semaphore = new SemaphoreSlim(settings.MaxConcurrentDownloads, settings.MaxConcurrentDownloads);
            var tasks = manifest.Files.Select(async file =>
            {
                await semaphore.WaitAsync(cancellationToken);
                try
                {
                    return await DownloadFileWithProgress(file, manifest.BaseUrl, tempDir, 
                        () => Interlocked.Increment(ref completedFiles),
                        (progress) => Interlocked.Add(ref totalProgress, progress),
                        cancellationToken);
                }
                finally
                {
                    semaphore.Release();
                }
            });

            var results = await Task.WhenAll(tasks);
            
            if (results.Any(r => !r))
            {
                throw new InvalidOperationException("Some files failed to download");
            }

            OnStatusChanged("Verifying downloaded files...");
            
            // Verify all files
            foreach (var file in manifest.Files)
            {
                var filePath = Path.Combine(tempDir, file.Path);
                if (!_securityService.VerifyFileHash(filePath, file.Sha256))
                {
                    throw new InvalidOperationException($"File verification failed: {file.Path}");
                }
            }

            OnStatusChanged("Applying update...");

            // Create backup of existing files
            if (Directory.Exists(backupDir))
            {
                Directory.Delete(backupDir, true);
            }

            // Backup existing game files
            foreach (var file in manifest.Files)
            {
                var sourcePath = Path.Combine(gameDir, file.Path);
                if (File.Exists(sourcePath))
                {
                    var backupPath = Path.Combine(backupDir, file.Path);
                    Directory.CreateDirectory(Path.GetDirectoryName(backupPath)!);
                    File.Copy(sourcePath, backupPath, true);
                }
            }

            // Atomic update: move files from temp to game directory
            foreach (var file in manifest.Files)
            {
                var sourcePath = Path.Combine(tempDir, file.Path);
                var destPath = Path.Combine(gameDir, file.Path);
                
                Directory.CreateDirectory(Path.GetDirectoryName(destPath)!);
                File.Move(sourcePath, destPath, true);
            }

            // Save the new manifest
            await _manifestService.SaveLocalManifestAsync(manifest);

            // Cleanup
            Directory.Delete(tempDir, true);
            if (Directory.Exists(backupDir))
            {
                Directory.Delete(backupDir, true);
            }

            OnStatusChanged("Update completed successfully");
            return true;
        }
        catch (Exception ex)
        {
            OnErrorOccurred(ex);
            
            // Rollback on failure
            try
            {
                await RollbackUpdate();
            }
            catch (Exception rollbackEx)
            {
                OnErrorOccurred(new AggregateException("Update failed and rollback failed", ex, rollbackEx));
            }
            
            return false;
        }
    }

    private async Task<bool> DownloadFileWithProgress(GameFile file, string baseUrl, string tempDir, 
        Action onFileCompleted, Action<long> onProgressUpdate, CancellationToken cancellationToken)
    {
        var url = !string.IsNullOrEmpty(file.Url) ? file.Url : $"{baseUrl.TrimEnd('/')}/{file.Path}";
        var destPath = Path.Combine(tempDir, file.Path);
        
        Directory.CreateDirectory(Path.GetDirectoryName(destPath)!);
        
        var result = await _downloadService.DownloadFileAsync(url, destPath, file.Sha256, file.Size, cancellationToken);
        
        if (result)
        {
            onFileCompleted();
            onProgressUpdate(file.Size);
        }
        
        return result;
    }

    public async Task<bool> VerifyGameIntegrityAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            OnStatusChanged("Verifying game integrity...");
            
            var manifest = await _manifestService.LoadLocalManifestAsync();
            if (manifest == null)
            {
                OnStatusChanged("No local manifest found");
                return false;
            }

            var settings = _settingsService.LoadSettings();
            var gameDir = Path.GetFullPath(settings.GamePath);
            var failedFiles = new List<string>();

            for (int i = 0; i < manifest.Files.Count; i++)
            {
                if (cancellationToken.IsCancellationRequested)
                    return false;

                var file = manifest.Files[i];
                var filePath = Path.Combine(gameDir, file.Path);

                OnProgressChanged(new UpdateProgress
                {
                    CurrentFile = file.Path,
                    FilesCompleted = i,
                    TotalFiles = manifest.Files.Count,
                    Status = $"Verifying {file.Path}..."
                });

                if (!File.Exists(filePath) || !_securityService.VerifyFileHash(filePath, file.Sha256))
                {
                    failedFiles.Add(file.Path);
                }
            }

            if (failedFiles.Count == 0)
            {
                OnStatusChanged("Game integrity check passed");
                return true;
            }
            else
            {
                OnStatusChanged($"Integrity check failed: {failedFiles.Count} files corrupted");
                return false;
            }
        }
        catch (Exception ex)
        {
            OnErrorOccurred(ex);
            return false;
        }
    }

    public async Task<bool> RepairGameAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            OnStatusChanged("Repairing game files...");
            
            var manifest = await _manifestService.LoadLocalManifestAsync();
            if (manifest == null)
            {
                OnStatusChanged("No local manifest found - cannot repair");
                return false;
            }

            return await UpdateGameAsync(manifest, cancellationToken);
        }
        catch (Exception ex)
        {
            OnErrorOccurred(ex);
            return false;
        }
    }

    public string GetCurrentVersion()
    {
        try
        {
            var manifest = _manifestService.LoadLocalManifestAsync().Result;
            return manifest?.Version ?? "Unknown";
        }
        catch
        {
            return "Unknown";
        }
    }

    public bool IsUpdateRequired(GameManifest manifest)
    {
        var localManifest = _manifestService.LoadLocalManifestAsync().Result;
        return localManifest == null || localManifest.Version != manifest.Version || manifest.ForceUpdate;
    }

    private async Task RollbackUpdate()
    {
        var settings = _settingsService.LoadSettings();
        var gameDir = Path.GetFullPath(settings.GamePath);
        var backupDir = Path.Combine(gameDir, ".backup");

        if (!Directory.Exists(backupDir))
            return;

        OnStatusChanged("Rolling back update...");

        // Restore files from backup
        foreach (var backupFile in Directory.GetFiles(backupDir, "*", SearchOption.AllDirectories))
        {
            var relativePath = Path.GetRelativePath(backupDir, backupFile);
            var destPath = Path.Combine(gameDir, relativePath);
            
            Directory.CreateDirectory(Path.GetDirectoryName(destPath)!);
            File.Copy(backupFile, destPath, true);
        }

        Directory.Delete(backupDir, true);
    }

    private void OnStatusChanged(string status) => StatusChanged?.Invoke(this, status);
    private void OnProgressChanged(UpdateProgress progress) => ProgressChanged?.Invoke(this, progress);
    private void OnErrorOccurred(Exception ex) => ErrorOccurred?.Invoke(this, ex);
}