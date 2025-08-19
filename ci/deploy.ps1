# Deployment Script for Blazed Odyssey Auto-Patcher
param(
    [Parameter(Mandatory=$true)]
    [string]$Version,
    
    [Parameter(Mandatory=$false)]
    [string]$GamePath = "Game",
    
    [Parameter(Mandatory=$false)]
    [string]$ManifestPath = "manifest.json",
    
    [Parameter(Mandatory=$false)]
    [string]$ChangelogPath = "CHANGELOG.md",
    
    [Parameter(Mandatory=$false)]
    [string]$PrivateKeyPath = "signing_key_private.key",
    
    [Parameter(Mandatory=$false)]
    [string]$BaseUrl = "https://github.com/BlazedONLINE/BlazedOdysseyAUTOPATCHER/releases/download/v$Version",
    
    [Parameter(Mandatory=$false)]
    [string]$Channel = "stable",
    
    [Parameter(Mandatory=$false)]
    [ValidateSet("github", "s3")]
    [string]$Target = "github",
    
    [Parameter(Mandatory=$false)]
    [string]$Repository = "BlazedONLINE/BlazedOdysseyAUTOPATCHER"
)

Write-Host "🚀 Deploying Blazed Odyssey v$Version to $Target"

# Build tools if not exists
$manifestGen = "tools\bin\ManifestGenerator.exe"
$publisher = "tools\bin\Publisher.exe"

if (-not (Test-Path $manifestGen) -or -not (Test-Path $publisher)) {
    Write-Host "🔨 Building deployment tools..."
    
    # Create tools bin directory
    New-Item -ItemType Directory -Force -Path "tools\bin" | Out-Null
    
    # Build ManifestGenerator
    Write-Host "Building ManifestGenerator..."
    Set-Location "tools\ManifestGenerator"
    dotnet publish -c Release -r win-x64 --self-contained -p:PublishSingleFile=true -o "..\bin"
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Failed to build ManifestGenerator"
        exit 1
    }
    Set-Location "..\..\"
    
    # Build Publisher
    Write-Host "Building Publisher..."
    Set-Location "tools\Publisher"
    dotnet publish -c Release -r win-x64 --self-contained -p:PublishSingleFile=true -o "..\bin"
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Failed to build Publisher"
        exit 1
    }
    Set-Location "..\..\"
}

# Validate game directory
if (-not (Test-Path $GamePath)) {
    Write-Error "Game directory not found: $GamePath"
    exit 1
}

$gameExe = Join-Path $GamePath "BlazedOdyssey.exe"
if (-not (Test-Path $gameExe)) {
    Write-Error "Game executable not found: $gameExe"
    exit 1
}

Write-Host "✅ Game build validated"

# Generate signing keys if they don't exist
if (-not (Test-Path $PrivateKeyPath)) {
    Write-Host "🔐 Generating signing keys..."
    & $manifestGen keygen --output "signing_key"
    
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Failed to generate signing keys"
        exit 1
    }
    
    Write-Host "⚠️  IMPORTANT: Store these keys securely!"
    Write-Host "Private key: signing_key_private.key"
    Write-Host "Public key: signing_key_public.key"
    Write-Host ""
    Write-Host "Add the private key to GitHub Secrets as SIGNING_PRIVATE_KEY (base64 encoded)"
    Write-Host "Update the public key in launcher/Services/SecurityService.cs"
}

# Generate manifest
Write-Host "📄 Generating manifest..."
$manifestArgs = @(
    "generate"
    "--input", $GamePath
    "--output", $ManifestPath
    "--version", $Version
    "--base-url", $BaseUrl
    "--channel", $Channel
    "--executable", "BlazedOdyssey.exe"
    "--sign"
    "--private-key", $PrivateKeyPath
)

& $manifestGen @manifestArgs

if ($LASTEXITCODE -ne 0) {
    Write-Error "Failed to generate manifest"
    exit 1
}

Write-Host "✅ Manifest generated successfully"

# Verify manifest
Write-Host "🔍 Verifying manifest..."
& $manifestGen verify --manifest $ManifestPath --game-dir $GamePath

if ($LASTEXITCODE -ne 0) {
    Write-Error "Manifest verification failed"
    exit 1
}

Write-Host "✅ Manifest verification passed"

# Publish release
Write-Host "📦 Publishing release to $Target..."

if ($Target -eq "github") {
    # GitHub release
    if (-not $env:GITHUB_TOKEN) {
        Write-Error "GITHUB_TOKEN environment variable not set"
        exit 1
    }
    
    $publishArgs = @(
        "github"
        "--game-dir", $GamePath
        "--manifest", $ManifestPath
        "--version", $Version
        "--token", $env:GITHUB_TOKEN
        "--repo", $Repository
    )
    
    if (Test-Path $ChangelogPath) {
        $publishArgs += "--changelog", $ChangelogPath
    }
    
    & $publisher @publishArgs
} elseif ($Target -eq "s3") {
    # S3 release
    if (-not $env:S3_ACCESS_KEY -or -not $env:S3_SECRET_KEY -or -not $env:S3_BUCKET -or -not $env:S3_REGION) {
        Write-Error "S3 environment variables not set (S3_ACCESS_KEY, S3_SECRET_KEY, S3_BUCKET, S3_REGION)"
        exit 1
    }
    
    $publishArgs = @(
        "s3"
        "--game-dir", $GamePath
        "--manifest", $ManifestPath
        "--version", $Version
        "--bucket", $env:S3_BUCKET
        "--region", $env:S3_REGION
        "--access-key", $env:S3_ACCESS_KEY
        "--secret-key", $env:S3_SECRET_KEY
    )
    
    if ($env:S3_ENDPOINT) {
        $publishArgs += "--endpoint", $env:S3_ENDPOINT
    }
    
    if (Test-Path $ChangelogPath) {
        $publishArgs += "--changelog", $ChangelogPath
    }
    
    & $publisher @publishArgs
}

if ($LASTEXITCODE -ne 0) {
    Write-Error "Failed to publish release"
    exit 1
}

Write-Host "🎉 Release v$Version published successfully!"
Write-Host ""
Write-Host "📋 Next steps:"
Write-Host "1. Update your launcher to point to the new manifest URL"
Write-Host "2. Test the update process with the launcher"
Write-Host "3. Announce the release to your community"
Write-Host ""
Write-Host "📊 Release summary:"
Write-Host "Version: $Version"
Write-Host "Channel: $Channel"
Write-Host "Target: $Target"
Write-Host "Game files: $($(Get-ChildItem -Path $GamePath -Recurse -File).Count)"
Write-Host "Total size: $([math]::Round($(Get-ChildItem -Path $GamePath -Recurse -File | Measure-Object -Property Length -Sum).Sum / 1MB, 2)) MB"