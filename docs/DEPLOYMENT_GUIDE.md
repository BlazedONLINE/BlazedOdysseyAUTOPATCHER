# Deployment Guide

This guide covers how to deploy the Blazed Odyssey auto-patcher system to production.

## Prerequisites

### Required Services
- **GitHub Repository**: For source code and releases
- **Domain Name**: For CDN and API endpoints (optional but recommended)
- **SSL Certificate**: For HTTPS downloads
- **Code Signing Certificate**: For launcher executable (Windows)

### Optional Services
- **CDN**: CloudFlare, AWS CloudFront, or similar
- **S3-Compatible Storage**: AWS S3, DigitalOcean Spaces, etc.
- **Monitoring**: Application insights, error tracking

## GitHub Setup

### 1. Repository Configuration

Create the following repository secrets in GitHub:

```
UNITY_LICENSE          # Unity license file content (base64)
UNITY_EMAIL           # Unity account email
UNITY_PASSWORD        # Unity account password
SIGNING_PRIVATE_KEY   # Ed25519 private key for manifest signing (base64)
CODE_SIGNING_CERT     # Code signing certificate (optional)
CODE_SIGNING_PASS     # Code signing certificate password (optional)
S3_ACCESS_KEY         # S3 access key (if using S3)
S3_SECRET_KEY         # S3 secret key (if using S3)
```

### 2. Generate Signing Keys

Run this once to generate your signing keys:

```bash
# Build the tools first
cd tools/ManifestGenerator
dotnet publish -c Release -r win-x64 --self-contained -p:PublishSingleFile=true -o bin

# Generate keys
./bin/ManifestGenerator keygen --output production_signing_key

# Convert to base64 for GitHub secrets
base64 -i production_signing_key_private.key
base64 -i production_signing_key_public.key
```

**Important**: 
- Store the private key in GitHub Secrets as `SIGNING_PRIVATE_KEY`
- Embed the public key in your launcher code (replace the placeholder in SecurityService.cs)
- Keep a secure backup of both keys

### 3. Update Public Key in Launcher

Edit `launcher/Services/SecurityService.cs`:

```csharp
// Replace this with your actual public key
private static readonly byte[] PublicKey = Convert.FromHexString(
    "YOUR_ACTUAL_PUBLIC_KEY_HEX_STRING_HERE");
```

## Domain and CDN Setup

### 1. DNS Configuration

Set up DNS records for your domain:

```
api.blazedodyssey.com        A/CNAME -> Your API server
releases.blazedodyssey.com   A/CNAME -> Your CDN/S3
launcher.blazedodyssey.com   A/CNAME -> GitHub Pages or CDN
```

### 2. SSL Certificates

Obtain SSL certificates for all subdomains:
- Use Let's Encrypt for free certificates
- Or purchase certificates from a CA
- Configure auto-renewal

### 3. CloudFlare Setup (Recommended)

If using CloudFlare:

1. Add your domain to CloudFlare
2. Enable "Always Use HTTPS"
3. Set SSL/TLS mode to "Full (strict)"
4. Enable "Auto Minify" for better performance
5. Configure caching rules for releases:

```
Cache Rule:
- URL pattern: releases.blazedodyssey.com/*
- Cache level: Cache everything
- Edge cache TTL: 1 month
- Browser cache TTL: 1 day
```

## S3/Spaces Deployment

### 1. Bucket Configuration

Create and configure your S3 bucket:

```bash
# AWS CLI example
aws s3 mb s3://blazed-odyssey-releases --region us-east-1

# Set public read policy
aws s3api put-bucket-policy --bucket blazed-odyssey-releases --policy '{
  "Version": "2012-10-17",
  "Statement": [
    {
      "Sid": "PublicReadGetObject",
      "Effect": "Allow",
      "Principal": "*",
      "Action": "s3:GetObject",
      "Resource": "arn:aws:s3:::blazed-odyssey-releases/*"
    }
  ]
}'

# Enable CORS for web access
aws s3api put-bucket-cors --bucket blazed-odyssey-releases --cors-configuration '{
  "CORSRules": [
    {
      "AllowedOrigins": ["*"],
      "AllowedMethods": ["GET", "HEAD"],
      "AllowedHeaders": ["*"],
      "MaxAgeSeconds": 3600
    }
  ]
}'
```

### 2. DigitalOcean Spaces Setup

For DigitalOcean Spaces:

```bash
# Install s3cmd
pip install s3cmd

# Configure s3cmd for DigitalOcean Spaces
s3cmd --configure

# Set public read policy
s3cmd setpolicy public s3://blazed-odyssey-releases/
```

### 3. Directory Structure

Organize your releases in the bucket:

```
blazed-odyssey-releases/
├── manifest.json                    # Latest manifest (symlink)
├── manifest.sig                     # Latest signature
├── v1.0.0/                         # Version-specific files
│   ├── manifest.json
│   ├── manifest.sig
│   ├── changelog.md
│   └── BlazedOdyssey.exe
├── v1.0.1/
│   ├── manifest.json
│   ├── manifest.sig
│   ├── changelog.md
│   └── BlazedOdyssey.exe
└── launcher/                        # Launcher files
    ├── BlazedOdysseyLauncher.exe
    └── versions/
        ├── 1.0.0/
        └── 1.1.0/
```

## GitHub Releases Deployment

### 1. Automated Deployment

Tag a release to trigger automated deployment:

```bash
# Create and push a tag
git tag v1.0.0
git push origin v1.0.0

# This will trigger the GitHub Actions workflow
```

### 2. Manual Deployment

For manual releases:

```bash
# Build tools
cd tools/ManifestGenerator
dotnet publish -c Release -r win-x64 --self-contained -p:PublishSingleFile=true -o bin

cd ../Publisher
dotnet publish -c Release -r win-x64 --self-contained -p:PublishSingleFile=true -o bin

# Generate manifest
../ManifestGenerator/bin/ManifestGenerator generate \
  --input ../../Game/ \
  --output manifest.json \
  --version 1.0.0 \
  --base-url "https://github.com/BlazedONLINE/BlazedOdysseyAUTOPATCHER/releases/download/v1.0.0" \
  --channel stable \
  --sign \
  --private-key production_signing_key_private.key

# Publish to GitHub
../Publisher/bin/Publisher github \
  --game-dir ../../Game/ \
  --manifest manifest.json \
  --version 1.0.0 \
  --changelog ../../CHANGELOG.md \
  --token $GITHUB_TOKEN \
  --repo BlazedONLINE/BlazedOdysseyAUTOPATCHER
```

## Launcher Distribution

### 1. GitHub Releases

The launcher is automatically built and attached to releases.

### 2. Website Integration

Create a download page:

```html
<!DOCTYPE html>
<html>
<head>
    <title>Download Blazed Odyssey</title>
    <meta charset="utf-8">
    <meta name="viewport" content="width=device-width, initial-scale=1">
</head>
<body>
    <div class="container">
        <h1>Download Blazed Odyssey</h1>
        <div class="download-section">
            <h2>Game Launcher</h2>
            <p>Download the official launcher to play Blazed Odyssey</p>
            <a href="https://github.com/BlazedONLINE/BlazedOdysseyAUTOPATCHER/releases/latest/download/BlazedOdysseyLauncher.exe" 
               class="download-btn">
                Download for Windows
            </a>
        </div>
        
        <div class="system-requirements">
            <h3>System Requirements</h3>
            <ul>
                <li>Windows 10 or later</li>
                <li>8 GB RAM</li>
                <li>DirectX 11 compatible graphics card</li>
                <li>2 GB available disk space</li>
                <li>Internet connection</li>
            </ul>
        </div>
    </div>
</body>
</html>
```

### 3. Auto-Update the Launcher

Future enhancement: Make the launcher self-updating by checking GitHub releases.

## Production Monitoring

### 1. Application Insights

Add telemetry to track launcher usage:

```csharp
public class TelemetryService
{
    private readonly TelemetryClient _telemetryClient;
    
    public void TrackLauncherStarted(string version)
    {
        _telemetryClient.TrackEvent("LauncherStarted", new Dictionary<string, string>
        {
            ["Version"] = version,
            ["OS"] = Environment.OSVersion.ToString()
        });
    }
    
    public void TrackUpdateCompleted(string fromVersion, string toVersion, TimeSpan duration)
    {
        _telemetryClient.TrackEvent("UpdateCompleted", new Dictionary<string, string>
        {
            ["FromVersion"] = fromVersion,
            ["ToVersion"] = toVersion,
            ["Duration"] = duration.ToString()
        });
    }
}
```

### 2. Error Tracking

Implement error tracking:

```csharp
public class ErrorReporter
{
    public static void ReportError(Exception ex, string context)
    {
        var errorData = new
        {
            Message = ex.Message,
            StackTrace = ex.StackTrace,
            Context = context,
            Timestamp = DateTime.UtcNow,
            Version = GetLauncherVersion(),
            OS = Environment.OSVersion.ToString()
        };
        
        // Send to your error tracking service
        SendToErrorService(errorData);
    }
}
```

### 3. Download Analytics

Track download performance:

```csharp
public class DownloadMetrics
{
    public void TrackDownloadStarted(string fileName, long fileSize)
    {
        // Track download metrics
    }
    
    public void TrackDownloadCompleted(string fileName, TimeSpan duration, long bytesPerSecond)
    {
        // Track successful downloads
    }
    
    public void TrackDownloadFailed(string fileName, string error)
    {
        // Track failed downloads
    }
}
```

## Security Hardening

### 1. Rate Limiting

Implement rate limiting on your CDN/server:

```yaml
# CloudFlare rate limiting example
Rate Limiting Rules:
- Pattern: api.blazedodyssey.com/*
  Requests: 100 per minute per IP
  
- Pattern: releases.blazedodyssey.com/*
  Requests: 50 per minute per IP
```

### 2. Access Controls

Set up proper access controls:

```bash
# GitHub repository
- Protect main branch
- Require PR reviews
- Enable vulnerability alerts
- Use branch protection rules

# S3 bucket
- Restrict write access to CI/CD
- Enable access logging
- Use IAM roles instead of access keys
```

### 3. Backup Strategy

Implement backups:

```bash
# Backup S3 bucket
aws s3 sync s3://blazed-odyssey-releases s3://blazed-odyssey-releases-backup

# Backup GitHub releases
gh release list --repo BlazedONLINE/BlazedOdysseyAUTOPATCHER
```

## Performance Optimization

### 1. CDN Configuration

Optimize CDN settings:

```yaml
Cache Headers:
- manifest.json: Cache-Control: max-age=300 (5 minutes)
- *.exe, *.dll: Cache-Control: max-age=31536000 (1 year)
- *.sig: Cache-Control: max-age=300 (5 minutes)

Compression:
- Enable Gzip/Brotli for text files
- Disable compression for binary files (.exe, .dll)
```

### 2. Download Optimization

Configure optimal download settings:

```json
{
  "maxConcurrentDownloads": 4,
  "chunkSize": 1048576,
  "retryDelayMs": 1000,
  "maxRetries": 3,
  "connectionTimeoutMs": 30000,
  "readTimeoutMs": 300000
}
```

### 3. Manifest Optimization

Keep manifests small and efficient:

- Use relative paths
- Compress manifest files
- Cache manifest responses
- Use delta updates for large files

## Disaster Recovery

### 1. Rollback Plan

Prepare for rollbacks:

```bash
# Rollback script example
#!/bin/bash
ROLLBACK_VERSION="1.0.0"

# Update manifest to point to previous version
aws s3 cp s3://blazed-odyssey-releases/v${ROLLBACK_VERSION}/manifest.json \
          s3://blazed-odyssey-releases/manifest.json

# Update signature
aws s3 cp s3://blazed-odyssey-releases/v${ROLLBACK_VERSION}/manifest.sig \
          s3://blazed-odyssey-releases/manifest.sig

echo "Rolled back to version ${ROLLBACK_VERSION}"
```

### 2. Incident Response

Document incident response procedures:

1. **Detection**: Monitor error rates and user reports
2. **Assessment**: Determine impact and root cause
3. **Response**: Implement fixes or rollback
4. **Communication**: Notify users via Discord/website
5. **Recovery**: Ensure system stability
6. **Post-mortem**: Document lessons learned

## Scaling Considerations

### 1. Traffic Planning

Plan for traffic spikes:

- CDN can handle high traffic
- Monitor bandwidth usage
- Set up auto-scaling if using custom servers
- Have standby capacity for major releases

### 2. Geographic Distribution

Consider multiple regions:

```yaml
Regions:
- Primary: US East (us-east-1)
- Secondary: EU West (eu-west-1)
- Secondary: Asia Pacific (ap-southeast-1)

CDN: Global edge locations
```

### 3. Load Testing

Test your infrastructure:

```bash
# Load test downloads
ab -n 1000 -c 10 https://releases.blazedodyssey.com/v1.0.0/BlazedOdyssey.exe

# Test manifest endpoint
ab -n 10000 -c 100 https://releases.blazedodyssey.com/manifest.json
```

This deployment guide should help you successfully deploy the auto-patcher system to production with proper security, monitoring, and scalability considerations.