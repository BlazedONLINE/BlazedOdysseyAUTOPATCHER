using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace BlazedOdysseyLauncher.Models;

public class GameManifest
{
    [JsonPropertyName("version")]
    public string Version { get; set; } = string.Empty;

    [JsonPropertyName("channel")]
    public string Channel { get; set; } = "stable";

    [JsonPropertyName("baseUrl")]
    public string BaseUrl { get; set; } = string.Empty;

    [JsonPropertyName("files")]
    public List<GameFile> Files { get; set; } = new();

    [JsonPropertyName("changelog")]
    public string? Changelog { get; set; }

    [JsonPropertyName("gameExecutable")]
    public string? GameExecutable { get; set; } = "BlazedOdyssey.exe";

    [JsonPropertyName("launchArguments")]
    public string? LaunchArguments { get; set; }

    [JsonPropertyName("minimumVersion")]
    public string? MinimumVersion { get; set; }

    [JsonPropertyName("forceUpdate")]
    public bool ForceUpdate { get; set; } = false;
}

public class GameFile
{
    [JsonPropertyName("path")]
    public string Path { get; set; } = string.Empty;

    [JsonPropertyName("size")]
    public long Size { get; set; }

    [JsonPropertyName("sha256")]
    public string Sha256 { get; set; } = string.Empty;

    [JsonPropertyName("delta")]
    public DeltaPatch? Delta { get; set; }

    [JsonPropertyName("url")]
    public string? Url { get; set; }

    [JsonPropertyName("compressed")]
    public bool Compressed { get; set; } = false;

    [JsonPropertyName("optional")]
    public bool Optional { get; set; } = false;

    [JsonPropertyName("executable")]
    public bool Executable { get; set; } = false;
}

public class DeltaPatch
{
    [JsonPropertyName("from")]
    public string From { get; set; } = string.Empty;

    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;

    [JsonPropertyName("sha256")]
    public string Sha256 { get; set; } = string.Empty;

    [JsonPropertyName("size")]
    public long Size { get; set; }
}

public class LauncherSettings
{
    public string GamePath { get; set; } = "Game";
    public string CdnMirrorUrl { get; set; } = "https://releases.blazedodyssey.com";
    public string ManifestUrl { get; set; } = "https://releases.blazedodyssey.com/manifest.json";
    public string Channel { get; set; } = "stable";
    public int MaxConcurrentDownloads { get; set; } = 4;
    public long BandwidthLimit { get; set; } = 0; // 0 = unlimited
    public bool AutoLaunch { get; set; } = false;
    public bool CloseOnLaunch { get; set; } = true;
    public bool CheckUpdatesOnStart { get; set; } = true;
    public string LastPlayedVersion { get; set; } = string.Empty;
}

public class UpdateProgress
{
    public string CurrentFile { get; set; } = string.Empty;
    public long CurrentFileProgress { get; set; }
    public long CurrentFileSize { get; set; }
    public long TotalProgress { get; set; }
    public long TotalSize { get; set; }
    public int FilesCompleted { get; set; }
    public int TotalFiles { get; set; }
    public double OverallProgress => TotalSize > 0 ? (double)TotalProgress / TotalSize * 100 : 0;
    public string Status { get; set; } = string.Empty;
    public bool IsIndeterminate { get; set; } = false;
}