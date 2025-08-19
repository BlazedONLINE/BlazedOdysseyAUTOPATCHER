using BlazedOdysseyLauncher.Models;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace BlazedOdysseyLauncher.Services;

public class GameLaunchService : IGameLaunchService
{
    private readonly ISettingsService _settingsService;
    private Process? _gameProcess;

    public GameLaunchService(ISettingsService settingsService)
    {
        _settingsService = settingsService;
    }

    public async Task<bool> LaunchGameAsync(string? gamePath = null, string? arguments = null)
    {
        try
        {
            var settings = _settingsService.LoadSettings();
            var gameDir = Path.GetFullPath(gamePath ?? settings.GamePath);
            
            // Find the game executable
            var gameExe = await FindGameExecutableAsync(gameDir);
            if (string.IsNullOrEmpty(gameExe))
            {
                throw new FileNotFoundException("Game executable not found");
            }

            var processInfo = new ProcessStartInfo
            {
                FileName = gameExe,
                WorkingDirectory = gameDir,
                Arguments = arguments ?? "",
                UseShellExecute = false,
                CreateNoWindow = false
            };

            _gameProcess = Process.Start(processInfo);
            
            if (_gameProcess == null)
            {
                throw new InvalidOperationException("Failed to start game process");
            }

            // Update last played version
            var manifest = await LoadLocalManifestAsync(gameDir);
            if (manifest != null)
            {
                settings.LastPlayedVersion = manifest.Version;
                await _settingsService.SaveSettingsAsync(settings);
            }

            return true;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to launch game: {ex.Message}", ex);
        }
    }

    public bool IsGameRunning()
    {
        return _gameProcess != null && !_gameProcess.HasExited;
    }

    public async Task WaitForGameToExitAsync()
    {
        if (_gameProcess != null && !_gameProcess.HasExited)
        {
            await _gameProcess.WaitForExitAsync();
        }
    }

    private async Task<string?> FindGameExecutableAsync(string gameDir)
    {
        // First, try to load manifest and get executable name
        var manifest = await LoadLocalManifestAsync(gameDir);
        if (manifest != null && !string.IsNullOrEmpty(manifest.GameExecutable))
        {
            var manifestExe = Path.Combine(gameDir, manifest.GameExecutable);
            if (File.Exists(manifestExe))
            {
                return manifestExe;
            }
        }

        // Fallback: look for common game executable names
        var commonNames = new[]
        {
            "BlazedOdyssey.exe",
            "BlazedOdysseyMMO.exe", 
            "Game.exe",
            "Launcher.exe"
        };

        foreach (var name in commonNames)
        {
            var exePath = Path.Combine(gameDir, name);
            if (File.Exists(exePath))
            {
                return exePath;
            }
        }

        // Last resort: find any .exe file
        if (Directory.Exists(gameDir))
        {
            var exeFiles = Directory.GetFiles(gameDir, "*.exe", SearchOption.TopDirectoryOnly);
            if (exeFiles.Length > 0)
            {
                // Prefer non-Unity files first, then any exe
                var nonUnityExe = exeFiles.FirstOrDefault(f => !Path.GetFileName(f).StartsWith("Unity", StringComparison.OrdinalIgnoreCase));
                return nonUnityExe ?? exeFiles[0];
            }
        }

        return null;
    }

    private async Task<GameManifest?> LoadLocalManifestAsync(string gameDir)
    {
        try
        {
            var manifestPath = Path.Combine(gameDir, "manifest.json");
            if (!File.Exists(manifestPath))
                return null;

            var json = await File.ReadAllTextAsync(manifestPath);
            return System.Text.Json.JsonSerializer.Deserialize<GameManifest>(json, new System.Text.Json.JsonSerializerOptions
            {
                PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase
            });
        }
        catch
        {
            return null;
        }
    }
}