using BlazedOdysseyLauncher.Models;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace BlazedOdysseyLauncher.Services;

public interface IUpdateService
{
    event EventHandler<UpdateProgress>? ProgressChanged;
    event EventHandler<string>? StatusChanged;
    event EventHandler<Exception>? ErrorOccurred;

    Task<GameManifest?> CheckForUpdatesAsync(CancellationToken cancellationToken = default);
    Task<bool> UpdateGameAsync(GameManifest manifest, CancellationToken cancellationToken = default);
    Task<bool> VerifyGameIntegrityAsync(CancellationToken cancellationToken = default);
    Task<bool> RepairGameAsync(CancellationToken cancellationToken = default);
    string GetCurrentVersion();
    bool IsUpdateRequired(GameManifest manifest);
}

public interface IDownloadService
{
    event EventHandler<UpdateProgress>? ProgressChanged;
    
    Task<bool> DownloadFileAsync(string url, string destination, string expectedHash, 
        long expectedSize = 0, CancellationToken cancellationToken = default);
    Task<bool> DownloadManifestAsync(string url, string destination, 
        CancellationToken cancellationToken = default);
    Task<byte[]> DownloadBytesAsync(string url, CancellationToken cancellationToken = default);
}

public interface IGameLaunchService
{
    Task<bool> LaunchGameAsync(string gamePath, string? arguments = null);
    bool IsGameRunning();
    Task WaitForGameToExitAsync();
}

public interface ISettingsService
{
    LauncherSettings LoadSettings();
    Task SaveSettingsAsync(LauncherSettings settings);
    string GetSettingsPath();
}

public interface IManifestService
{
    Task<GameManifest?> LoadLocalManifestAsync();
    Task SaveLocalManifestAsync(GameManifest manifest);
    Task<GameManifest?> DownloadManifestAsync(string url, CancellationToken cancellationToken = default);
    bool ValidateManifest(GameManifest manifest);
}

public interface ISecurityService
{
    bool VerifyManifestSignature(byte[] manifestData, byte[] signature);
    bool VerifyFileHash(string filePath, string expectedHash);
    string CalculateFileHash(string filePath);
    Task<string> CalculateFileHashAsync(string filePath);
}