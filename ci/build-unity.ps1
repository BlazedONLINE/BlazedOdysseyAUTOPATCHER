# Unity Build Script for CI/CD
param(
    [Parameter(Mandatory=$true)]
    [string]$Version,
    
    [Parameter(Mandatory=$false)]
    [string]$UnityPath = "C:\Program Files\Unity\Hub\Editor\2023.2.20f1\Editor\Unity.exe",
    
    [Parameter(Mandatory=$false)]
    [string]$ProjectPath = ".",
    
    [Parameter(Mandatory=$false)]
    [string]$BuildPath = "Game",
    
    [Parameter(Mandatory=$false)]
    [string]$LogFile = "unity-build.log"
)

Write-Host "🎮 Building Blazed Odyssey v$Version"
Write-Host "Unity Path: $UnityPath"
Write-Host "Project Path: $ProjectPath"
Write-Host "Build Path: $BuildPath"

# Validate Unity installation
if (-not (Test-Path $UnityPath)) {
    Write-Error "Unity not found at: $UnityPath"
    exit 1
}

# Clean previous build
if (Test-Path $BuildPath) {
    Write-Host "🧹 Cleaning previous build..."
    Remove-Item -Path $BuildPath -Recurse -Force
}

# Create build directory
New-Item -ItemType Directory -Force -Path $BuildPath | Out-Null

# Unity build arguments
$unityArgs = @(
    "-batchmode"
    "-quit"
    "-projectPath", $ProjectPath
    "-buildWindows64Player", "$BuildPath\BlazedOdyssey.exe"
    "-logFile", $LogFile
    "-buildVersion", $Version
)

Write-Host "🚀 Starting Unity build..."
Write-Host "Command: $UnityPath $($unityArgs -join ' ')"

# Execute Unity build
$process = Start-Process -FilePath $UnityPath -ArgumentList $unityArgs -Wait -PassThru -NoNewWindow

# Check build result
if ($process.ExitCode -eq 0) {
    Write-Host "✅ Unity build completed successfully!"
    
    # Verify build output
    $exePath = Join-Path $BuildPath "BlazedOdyssey.exe"
    if (Test-Path $exePath) {
        $fileSize = (Get-Item $exePath).Length
        Write-Host "📁 Build size: $([math]::Round($fileSize / 1MB, 2)) MB"
        
        # List build contents
        Write-Host "📋 Build contents:"
        Get-ChildItem -Path $BuildPath -Recurse | ForEach-Object {
            $size = if ($_.PSIsContainer) { "DIR" } else { "$([math]::Round($_.Length / 1KB, 1)) KB" }
            Write-Host "   $($_.Name) ($size)"
        }
    } else {
        Write-Error "❌ Build completed but executable not found!"
        exit 1
    }
} else {
    Write-Error "❌ Unity build failed with exit code: $($process.ExitCode)"
    
    # Display log if available
    if (Test-Path $LogFile) {
        Write-Host "📄 Unity Log (last 50 lines):"
        Get-Content $LogFile | Select-Object -Last 50 | ForEach-Object {
            Write-Host "   $_"
        }
    }
    
    exit 1
}

Write-Host "🎉 Build process completed successfully!"