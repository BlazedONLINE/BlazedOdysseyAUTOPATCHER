using BlazedOdysseyLauncher.Models;
using System;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace BlazedOdysseyLauncher.Services;

public class ManifestService : IManifestService
{
    private readonly IDownloadService _downloadService;
    private readonly ISecurityService _securityService;
    private readonly ISettingsService _settingsService;

    public ManifestService(IDownloadService downloadService, ISecurityService securityService, ISettingsService settingsService)
    {
        _downloadService = downloadService;
        _securityService = securityService;
        _settingsService = settingsService;
    }

    public async Task<GameManifest?> LoadLocalManifestAsync()
    {
        try
        {
            var settings = _settingsService.LoadSettings();
            var manifestPath = Path.Combine(settings.GamePath, "manifest.json");

            if (!File.Exists(manifestPath))
                return null;

            var json = await File.ReadAllTextAsync(manifestPath);
            return JsonSerializer.Deserialize<GameManifest>(json, JsonOptions);
        }
        catch
        {
            return null;
        }
    }

    public async Task SaveLocalManifestAsync(GameManifest manifest)
    {
        try
        {
            var settings = _settingsService.LoadSettings();
            var gameDir = Path.GetFullPath(settings.GamePath);
            Directory.CreateDirectory(gameDir);
            
            var manifestPath = Path.Combine(gameDir, "manifest.json");
            var json = JsonSerializer.Serialize(manifest, JsonOptions);
            await File.WriteAllTextAsync(manifestPath, json);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to save local manifest: {ex.Message}", ex);
        }
    }

    public async Task<GameManifest?> DownloadManifestAsync(string url, CancellationToken cancellationToken = default)
    {
        try
        {
            var manifestBytes = await _downloadService.DownloadBytesAsync(url, cancellationToken);
            
            // Try to download signature for verification
            byte[]? signature = null;
            try
            {
                var signatureUrl = url.Replace("manifest.json", "manifest.sig");
                signature = await _downloadService.DownloadBytesAsync(signatureUrl, cancellationToken);
            }
            catch
            {
                // Signature is optional
            }

            // Verify signature if available
            if (signature != null && !_securityService.VerifyManifestSignature(manifestBytes, signature))
            {
                throw new InvalidOperationException("Manifest signature verification failed");
            }

            var json = System.Text.Encoding.UTF8.GetString(manifestBytes);
            var manifest = JsonSerializer.Deserialize<GameManifest>(json, JsonOptions);

            return manifest;
        }
        catch (Exception ex) when (!(ex is OperationCanceledException))
        {
            throw new InvalidOperationException($"Failed to download manifest: {ex.Message}", ex);
        }
    }

    public bool ValidateManifest(GameManifest manifest)
    {
        try
        {
            // Basic validation
            if (string.IsNullOrEmpty(manifest.Version))
                return false;

            if (string.IsNullOrEmpty(manifest.BaseUrl))
                return false;

            if (manifest.Files == null || manifest.Files.Count == 0)
                return false;

            // Validate each file entry
            foreach (var file in manifest.Files)
            {
                if (string.IsNullOrEmpty(file.Path))
                    return false;

                if (string.IsNullOrEmpty(file.Sha256) || file.Sha256.Length != 64)
                    return false;

                if (file.Size <= 0)
                    return false;
            }

            // Validate version format (semantic versioning)
            if (!IsValidVersion(manifest.Version))
                return false;

            return true;
        }
        catch
        {
            return false;
        }
    }

    private static bool IsValidVersion(string version)
    {
        try
        {
            var parts = version.Split('.');
            if (parts.Length < 2 || parts.Length > 4)
                return false;

            foreach (var part in parts)
            {
                if (!int.TryParse(part, out var number) || number < 0)
                    return false;
            }

            return true;
        }
        catch
        {
            return false;
        }
    }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };
}