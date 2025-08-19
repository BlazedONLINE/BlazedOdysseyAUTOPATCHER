# Integration Guide

This guide explains how to integrate the Blazed Odyssey auto-patcher into your development workflow.

## Unity Project Integration

### 1. Setting Up Your Unity Project

Your Unity project should be organized to work with the auto-patcher:

```
BlazedOdysseyMMOAlpha/
├── Assets/                    # Your Unity assets
├── ProjectSettings/           # Unity project settings
└── Builds/                    # Build output directory
    └── Windows/               # Platform-specific builds
        └── Game/              # Final game files for patcher
```

### 2. Build Script Integration

Create a Unity Editor script to automate builds:

```csharp
// Assets/Editor/BuildScript.cs
using UnityEditor;
using UnityEngine;
using System.IO;

public class BuildScript
{
    [MenuItem("Blazed Odyssey/Build for Patcher")]
    public static void BuildGame()
    {
        string buildPath = Path.Combine(Directory.GetCurrentDirectory(), "Game");
        
        // Clean previous build
        if (Directory.Exists(buildPath))
            Directory.Delete(buildPath, true);
            
        Directory.CreateDirectory(buildPath);
        
        // Build settings
        BuildPlayerOptions buildOptions = new BuildPlayerOptions
        {
            scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray(),
            locationPathName = Path.Combine(buildPath, "BlazedOdyssey.exe"),
            target = BuildTarget.StandaloneWindows64,
            options = BuildOptions.None
        };
        
        // Perform build
        BuildPipeline.BuildPlayer(buildOptions);
        
        Debug.Log($"Build completed: {buildPath}");
    }
}
```

### 3. Version Management

Create a version management system:

```csharp
// Assets/Scripts/Core/GameVersion.cs
using UnityEngine;

[CreateAssetMenu(fileName = "GameVersion", menuName = "Blazed Odyssey/Game Version")]
public class GameVersion : ScriptableObject
{
    [SerializeField] private string version = "1.0.0";
    [SerializeField] private string channel = "stable";
    [SerializeField] private string buildDate;
    
    public string Version => version;
    public string Channel => channel;
    public string BuildDate => buildDate;
    
    [ContextMenu("Update Build Date")]
    public void UpdateBuildDate()
    {
        buildDate = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
    }
}
```

## Development Workflow

### Daily Development
1. Work on your Unity project as normal
2. Use version control (Git) for source code
3. Test locally with Play mode

### Creating Releases
1. **Update Version**: Update your GameVersion ScriptableObject
2. **Commit Changes**: Commit all changes to Git
3. **Create Tag**: `git tag v1.0.0 && git push origin v1.0.0`
4. **Automated Build**: GitHub Actions will automatically:
   - Build Unity project
   - Generate manifest
   - Create GitHub Release
   - Build and publish launcher

### Manual Release Process
If you prefer manual control:

```bash
# 1. Build Unity project (use Unity Cloud Build or local build)
# 2. Generate manifest
ManifestGenerator generate \
  --input "Game/" \
  --output "manifest.json" \
  --version "1.0.0" \
  --base-url "https://github.com/BlazedONLINE/BlazedOdysseyAUTOPATCHER/releases/download/v1.0.0" \
  --channel "stable" \
  --executable "BlazedOdyssey.exe" \
  --sign \
  --private-key "signing_key_private.key"

# 3. Publish release
Publisher github \
  --game-dir "Game/" \
  --manifest "manifest.json" \
  --version "1.0.0" \
  --changelog "CHANGELOG.md" \
  --token "$GITHUB_TOKEN" \
  --repo "BlazedONLINE/BlazedOdysseyAUTOPATCHER"
```

## Server Integration

### API Endpoints

You can integrate with your existing API server to provide additional launcher features:

```csharp
// Optional: Launcher API integration
public class LauncherAPI
{
    private const string API_BASE = "https://api.blazedodyssey.com";
    
    public async Task<string> GetServerStatusAsync()
    {
        using var client = new HttpClient();
        var response = await client.GetStringAsync($"{API_BASE}/status");
        return response;
    }
    
    public async Task<AnnouncementData> GetAnnouncementsAsync()
    {
        using var client = new HttpClient();
        var response = await client.GetStringAsync($"{API_BASE}/announcements");
        return JsonSerializer.Deserialize<AnnouncementData>(response);
    }
}
```

### Database Integration

The launcher can work with your existing player database:

```sql
-- Add launcher-specific columns to your users table
ALTER TABLE users ADD COLUMN launcher_version VARCHAR(20);
ALTER TABLE users ADD COLUMN last_launcher_update TIMESTAMP;

-- Track launcher usage
CREATE TABLE launcher_analytics (
    id INT PRIMARY KEY AUTO_INCREMENT,
    user_id INT REFERENCES users(id),
    launcher_version VARCHAR(20),
    game_version VARCHAR(20),
    action VARCHAR(50),
    timestamp TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);
```

## Testing Strategy

### Local Testing
1. **Unit Tests**: Test individual components
2. **Integration Tests**: Test full update workflow
3. **Manual Testing**: Test UI and user workflows

### Staging Environment
1. Create a staging release channel
2. Test updates before pushing to production
3. Use a separate S3 bucket or GitHub repository for staging

### Production Testing
1. Monitor launcher analytics
2. Track update success rates
3. Monitor crash reports and logs

## Security Considerations

### Code Signing
For production deployments, consider code signing your launcher:

```bash
# Windows Code Signing (requires certificate)
signtool sign /f certificate.pfx /p password /t http://timestamp.digicert.com BlazedOdysseyLauncher.exe
```

### Key Management
- Store private signing keys securely (GitHub Secrets, Azure Key Vault, etc.)
- Rotate keys periodically
- Use separate keys for different environments

### Distribution Security
- Use HTTPS for all downloads
- Verify SSL certificates
- Implement download quotas to prevent abuse

## Performance Optimization

### CDN Setup
Use a CDN for better download performance:

```json
{
  "baseUrl": "https://cdn.blazedodyssey.com/releases/1.0.0",
  "cdnMirrors": [
    "https://eu-cdn.blazedodyssey.com/releases/1.0.0",
    "https://asia-cdn.blazedodyssey.com/releases/1.0.0"
  ]
}
```

### Bandwidth Management
Configure download limits in launcher settings:

```json
{
  "maxConcurrentDownloads": 4,
  "bandwidthLimit": 10485760,  // 10 MB/s
  "downloadRetries": 3,
  "downloadTimeout": 300000    // 5 minutes
}
```

## Monitoring and Analytics

### Launcher Metrics
Track important metrics:
- Update success rate
- Download speeds
- Error frequency
- User retention

### Implementation Example
```csharp
public class LauncherAnalytics
{
    public async Task TrackEvent(string eventName, Dictionary<string, object> properties)
    {
        var payload = new
        {
            Event = eventName,
            Properties = properties,
            Timestamp = DateTime.UtcNow,
            UserId = GetUserId(),
            SessionId = GetSessionId()
        };
        
        await SendToAnalytics(payload);
    }
}
```

## Troubleshooting Integration Issues

### Common Problems

**Build Not Found**
- Check Unity build output path
- Verify file permissions
- Ensure all dependencies are included

**Manifest Generation Fails**
- Check file path separators (use forward slashes in manifest)
- Verify all files are accessible
- Check available disk space

**Download Errors**
- Verify base URL is accessible
- Check file permissions on server
- Ensure HTTPS certificates are valid

### Debug Mode
Enable debug logging in development:

```json
{
  "debugMode": true,
  "logLevel": "Debug",
  "logToFile": true,
  "logPath": "launcher-debug.log"
}
```

## Best Practices

### Release Management
1. Use semantic versioning (1.2.3)
2. Maintain changelog for each release
3. Test releases in staging before production
4. Have a rollback plan for failed releases

### File Organization
```
Game/
├── BlazedOdyssey.exe           # Main executable
├── UnityPlayer.dll             # Unity runtime
├── UnityCrashHandler64.exe     # Crash handler
├── BlazedOdyssey_Data/         # Unity data
│   ├── Managed/                # .NET assemblies
│   ├── Resources/              # Unity resources
│   ├── StreamingAssets/        # Streaming assets
│   └── ...
└── MonoBleedingEdge/           # Mono runtime (if used)
```

### Version Strategy
- **Major.Minor.Patch** format
- Major: Breaking changes, new features
- Minor: New features, backwards compatible
- Patch: Bug fixes only

This integration guide should help you seamlessly incorporate the auto-patcher into your Blazed Odyssey development workflow.