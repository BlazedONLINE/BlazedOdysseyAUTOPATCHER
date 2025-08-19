using BlazedOdysseyLauncher.Models;
using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace BlazedOdysseyLauncher.Services;

public class SettingsService : ISettingsService
{
    private readonly string _settingsPath;
    private LauncherSettings? _cachedSettings;

    public SettingsService()
    {
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var launcherDir = Path.Combine(appDataPath, "BlazedOdyssey", "Launcher");
        Directory.CreateDirectory(launcherDir);
        _settingsPath = Path.Combine(launcherDir, "settings.json");
    }

    public LauncherSettings LoadSettings()
    {
        if (_cachedSettings != null)
            return _cachedSettings;

        try
        {
            if (File.Exists(_settingsPath))
            {
                var json = File.ReadAllText(_settingsPath);
                _cachedSettings = JsonSerializer.Deserialize<LauncherSettings>(json, JsonOptions) ?? new LauncherSettings();
            }
            else
            {
                _cachedSettings = new LauncherSettings();
            }
        }
        catch
        {
            _cachedSettings = new LauncherSettings();
        }

        return _cachedSettings;
    }

    public async Task SaveSettingsAsync(LauncherSettings settings)
    {
        try
        {
            var json = JsonSerializer.Serialize(settings, JsonOptions);
            await File.WriteAllTextAsync(_settingsPath, json);
            _cachedSettings = settings;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to save settings: {ex.Message}", ex);
        }
    }

    public string GetSettingsPath() => _settingsPath;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };
}