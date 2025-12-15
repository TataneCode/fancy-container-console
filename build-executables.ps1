#!/usr/bin/env pwsh

Write-Host "Building FancyContainerConsole executables..." -ForegroundColor Cyan
Write-Host ""

$ProjectPath = "src/FancyContainerConsole/FancyContainerConsole.csproj"
$OutputDir = "dist"

# Clean previous builds
if (Test-Path $OutputDir) {
    Write-Host "Cleaning previous builds..." -ForegroundColor Yellow
    Remove-Item -Recurse -Force $OutputDir
}

# Create output directory
New-Item -ItemType Directory -Path $OutputDir -Force | Out-Null

# Build for Windows x64
Write-Host "Building for Windows (x64)..." -ForegroundColor Yellow
dotnet publish $ProjectPath `
  -c Release `
  -r win-x64 `
  --self-contained true `
  -p:PublishSingleFile=true `
  -p:IncludeNativeLibrariesForSelfExtract=true `
  -o "$OutputDir/win-x64"

if ($LASTEXITCODE -eq 0) {
    Write-Host "✓ Windows build completed" -ForegroundColor Green
} else {
    Write-Host "✗ Windows build failed" -ForegroundColor Red
    exit 1
}

Write-Host ""

# Build for Linux x64
Write-Host "Building for Linux (x64)..." -ForegroundColor Yellow
dotnet publish $ProjectPath `
  -c Release `
  -r linux-x64 `
  --self-contained true `
  -p:PublishSingleFile=true `
  -p:IncludeNativeLibrariesForSelfExtract=true `
  -o "$OutputDir/linux-x64"

if ($LASTEXITCODE -eq 0) {
    Write-Host "✓ Linux build completed" -ForegroundColor Green
} else {
    Write-Host "✗ Linux build failed" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "Build completed successfully!" -ForegroundColor Green
Write-Host ""
Write-Host "Executables location:" -ForegroundColor Cyan
Write-Host "  Windows: $OutputDir/win-x64/FancyContainerConsole.exe"
Write-Host "  Linux:   $OutputDir/linux-x64/FancyContainerConsole"
Write-Host ""
