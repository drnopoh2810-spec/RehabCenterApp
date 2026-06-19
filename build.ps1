#Requires -Version 5.1
# RehabCenterApp Build Script
# Run as: .\build.ps1

[Console]::OutputEncoding = [System.Text.Encoding]::UTF8
$ErrorActionPreference = "Stop"

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location $scriptDir

Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "   RehabCenterApp - Build Script" -ForegroundColor Cyan
Write-Host "   Version 1.0.0" -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Working directory: $PWD" -ForegroundColor Gray
Write-Host ""

# Check .NET SDK
$dotnet = Get-Command dotnet -ErrorAction SilentlyContinue
if (-not $dotnet) {
    Write-Host "[ERROR] .NET SDK not found!" -ForegroundColor Red
    Write-Host "Please install .NET 8.0 SDK from:" -ForegroundColor Yellow
    Write-Host "https://dotnet.microsoft.com/download/dotnet/8.0" -ForegroundColor Yellow
    Read-Host "Press Enter to exit"
    exit 1
}

Write-Host "[1/5] Restoring packages..." -ForegroundColor Green
dotnet restore "RehabCenterApp.csproj"

Write-Host "[2/5] Building project..." -ForegroundColor Green
dotnet build "RehabCenterApp.csproj" -c Release

Write-Host "[3/5] Publishing single-file executable..." -ForegroundColor Green
dotnet publish "RehabCenterApp.csproj" -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:PublishReadyToRun=true

Write-Host "[4/5] Creating output directory..." -ForegroundColor Green
New-Item -ItemType Directory -Force -Path "Publish" | Out-Null

Write-Host "[5/5] Build complete!" -ForegroundColor Green
Write-Host ""
Write-Host "Output: bin\Release\net8.0\win-x64\publish\RehabCenterApp.exe" -ForegroundColor Cyan
Write-Host ""
Write-Host "Next step: Build installer with Inno Setup" -ForegroundColor Yellow
Write-Host "  1. Install Inno Setup from https://jrsoftware.org/isdl.php" -ForegroundColor Yellow
Write-Host "  2. Open Installer\RehabCenterApp.iss" -ForegroundColor Yellow
Write-Host "  3. Click Build -> Compile" -ForegroundColor Yellow
Write-Host ""
Read-Host "Press Enter to exit"