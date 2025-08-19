# Blazed Odyssey Auto-Patcher

A comprehensive auto-patcher and launcher system for Blazed Odyssey MMO, featuring secure updates, integrity verification, and atomic patching.

## 🌟 Features

### Launcher
- **Modern UI**: Clean, responsive interface built with Avalonia
- **Automatic Updates**: Checks for game updates and downloads only changed files
- **Integrity Verification**: SHA-256 hash verification for all files
- **Resume Downloads**: Interrupted downloads can be resumed
- **Atomic Updates**: Changes are applied atomically with rollback on failure
- **Secure**: Ed25519 signature verification for manifests
- **Cross-Platform**: Windows, Linux, and macOS support

### Tools
- **Manifest Generator**: Creates update manifests from game builds
- **Publisher**: Publishes releases to GitHub Releases or S3-compatible storage
- **Delta Support**: Designed to support delta patches (future feature)

### CI/CD Integration
- **GitHub Actions**: Automated building, testing, and publishing
- **Unity Integration**: Automated Unity builds on tag push
- **Multi-Platform**: Builds launcher for Windows, Linux, and macOS

## 🚀 Quick Start

### For Players

1. Download the latest launcher from [Releases](https://github.com/BlazedONLINE/BlazedOdysseyAUTOPATCHER/releases)
2. Run `BlazedOdysseyLauncher.exe`
3. Click "Check Updates" to download the latest game version
4. Click "Play Game" to launch Blazed Odyssey

### For Developers

1. Clone this repository
2. Build the tools: `dotnet build tools/`
3. Generate a manifest: `ManifestGenerator generate --input GameDir --output manifest.json --version 1.0.0 --base-url https://releases.example.com`
4. Publish to GitHub: `Publisher github --manifest manifest.json --version 1.0.0 --token YOUR_TOKEN --repo owner/repo`

## 📁 Repository Structure

```
BlazedOdysseyAUTOPATCHER/
├── launcher/              # C# Avalonia launcher application
│   ├── Models/           # Data models (GameManifest, Settings, etc.)
│   ├── Services/         # Business logic (UpdateService, DownloadService, etc.)
│   ├── ViewModels/       # MVVM view models
│   └── Views/            # XAML UI definitions
├── tools/                # Build and publishing tools
│   ├── ManifestGenerator/ # Generates manifests from game builds
│   └── Publisher/        # Publishes releases to GitHub/S3
├── manifests/            # Example manifests and schemas
├── docs/                 # Documentation
├── ci/                   # Build scripts and CI helpers
└── .github/workflows/    # GitHub Actions workflows
```

## 🛠️ Building

### Prerequisites
- .NET 7.0 SDK
- Git

### Build Launcher
```bash
cd launcher
dotnet publish -c Release -r win-x64 --self-contained -p:PublishSingleFile=true
```

### Build Tools
```bash
cd tools/ManifestGenerator
dotnet publish -c Release -r win-x64 --self-contained -p:PublishSingleFile=true

cd ../Publisher
dotnet publish -c Release -r win-x64 --self-contained -p:PublishSingleFile=true
```

## 📋 Usage Guide

### 1. Generate Signing Keys
```bash
ManifestGenerator keygen --output signing_key
```
This creates `signing_key_private.key` and `signing_key_public.key`. Keep the private key secure!

### 2. Build Your Unity Game
Build your Unity project for Windows x64 to a directory (e.g., `Game/`).

### 3. Generate Manifest
```bash
ManifestGenerator generate \
  --input Game/ \
  --output manifest.json \
  --version 1.2.3 \
  --base-url https://releases.blazedodyssey.com/1.2.3 \
  --channel stable \
  --executable BlazedOdyssey.exe \
  --sign \
  --private-key signing_key_private.key
```

### 4. Verify Manifest
```bash
ManifestGenerator verify \
  --manifest manifest.json \
  --game-dir Game/
```

### 5. Publish Release

#### To GitHub Releases:
```bash
Publisher github \
  --game-dir Game/ \
  --manifest manifest.json \
  --version 1.2.3 \
  --changelog CHANGELOG.md \
  --token YOUR_GITHUB_TOKEN \
  --repo BlazedONLINE/BlazedOdysseyAUTOPATCHER
```

#### To S3/DigitalOcean Spaces:
```bash
Publisher s3 \
  --game-dir Game/ \
  --manifest manifest.json \
  --version 1.2.3 \
  --bucket my-releases-bucket \
  --region nyc3 \
  --access-key YOUR_ACCESS_KEY \
  --secret-key YOUR_SECRET_KEY \
  --endpoint https://nyc3.digitaloceanspaces.com
```

## 🔧 Configuration

### Launcher Settings
The launcher stores settings in `%LocalAppData%/BlazedOdyssey/Launcher/settings.json`:

```json
{
  "gamePath": "Game",
  "cdnMirrorUrl": "https://releases.blazedodyssey.com",
  "manifestUrl": "https://releases.blazedodyssey.com/manifest.json",
  "channel": "stable",
  "maxConcurrentDownloads": 4,
  "bandwidthLimit": 0,
  "autoLaunch": false,
  "closeOnLaunch": true,
  "checkUpdatesOnStart": true,
  "lastPlayedVersion": ""
}
```

### Manifest Format
```json
{
  "version": "1.0.0",
  "channel": "stable",
  "baseUrl": "https://releases.blazedodyssey.com/1.0.0",
  "gameExecutable": "BlazedOdyssey.exe",
  "launchArguments": "",
  "minimumVersion": "0.9.0",
  "forceUpdate": false,
  "files": [
    {
      "path": "BlazedOdyssey.exe",
      "size": 45678901,
      "sha256": "a1b2c3d4...",
      "delta": null,
      "compressed": false,
      "optional": false,
      "executable": true
    }
  ]
}
```

## 🔐 Security

### Manifest Signing
- Manifests are signed using Ed25519 digital signatures
- Public key is embedded in the launcher application
- Private key is stored securely (GitHub Secrets for CI/CD)
- Signature verification prevents tampering

### File Integrity
- All files are verified using SHA-256 hashes
- Downloads are verified before applying updates
- Corrupted files are re-downloaded automatically

### Secure Transmission
- All downloads use HTTPS
- No sensitive data is transmitted in plain text
- Redirects are restricted to whitelisted domains

## 🤖 CI/CD Setup

### GitHub Secrets Required
- `UNITY_LICENSE`: Unity license file content
- `UNITY_EMAIL`: Unity account email
- `UNITY_PASSWORD`: Unity account password
- `SIGNING_PRIVATE_KEY`: Base64-encoded private signing key
- `GITHUB_TOKEN`: Automatically provided by GitHub

### Workflow Triggers
- **Game Release**: Push a tag like `v1.0.0` to trigger game build and release
- **Launcher**: Push changes to `launcher/` directory to build launcher
- **Tools**: Push changes to `tools/` directory to build and test tools

## 🐛 Troubleshooting

### Common Issues

**"Game executable not found"**
- Ensure your Unity build outputs to a directory named `Game/`
- Check that the executable name matches the manifest

**"Manifest signature verification failed"**
- Verify the public key in the launcher matches your signing key
- Ensure the manifest wasn't corrupted during transfer

**"Download failed"**
- Check your internet connection
- Verify the base URL in the manifest is accessible
- Check GitHub Releases or S3 bucket permissions

### Logging
- Launcher logs are stored in `%LocalAppData%/BlazedOdyssey/Launcher/logs/`
- Tools output verbose logging to console
- Use `--verbose` flag for additional debugging information

## 📈 Roadmap

- [ ] Delta patching support for faster updates
- [ ] Torrent-based distribution for large files
- [ ] Multi-language launcher support
- [ ] Advanced settings UI in launcher
- [ ] Rollback to previous versions
- [ ] Beta/alpha channel support
- [ ] Statistics and analytics integration

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests if applicable
5. Submit a pull request

## 📄 License

This project is licensed under the MIT License - see the LICENSE file for details.

## 🆘 Support

- **Discord**: [Blazed Odyssey Community](https://discord.gg/blazedodyssey)
- **Issues**: [GitHub Issues](https://github.com/BlazedONLINE/BlazedOdysseyAUTOPATCHER/issues)
- **Email**: support@blazedodyssey.com

---

**Built with ❤️ for the Blazed Odyssey community**