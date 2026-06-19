@echo off
chcp 65001 >nul
cls

echo ==========================================
echo   RehabCenterApp - Build Script
echo   Version 1.0.0
echo ==========================================
echo.

REM Get the directory where this script is located
set "SCRIPT_DIR=%~dp0"
cd /d "%SCRIPT_DIR%"

echo Working directory: %CD%
echo.

REM Check for .NET SDK
where dotnet >nul 2>nul
if %errorlevel% neq 0 (
    echo [ERROR] .NET SDK not found!
    echo Please install .NET 8.0 SDK from:
    echo https://dotnet.microsoft.com/download/dotnet/8.0
    pause
    exit /b 1
)

echo [1/5] Restoring packages...
dotnet restore "RehabCenterApp.csproj"
if %errorlevel% neq 0 (
    echo [ERROR] Restore failed!
    pause
    exit /b 1
)

echo [2/5] Building project...
dotnet build "RehabCenterApp.csproj" -c Release
if %errorlevel% neq 0 (
    echo [ERROR] Build failed!
    pause
    exit /b 1
)

echo [3/5] Publishing single-file executable...
dotnet publish "RehabCenterApp.csproj" -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:PublishReadyToRun=true
if %errorlevel% neq 0 (
    echo [ERROR] Publish failed!
    pause
    exit /b 1
)

echo [4/5] Creating output directory...
if not exist "Publish" mkdir Publish

echo [5/5] Build complete!
echo.
echo Output: bin\Release\net8.0\win-x64\publish\RehabCenterApp.exe
echo.
echo Next step: Build installer with Inno Setup
echo   1. Install Inno Setup from https://jrsoftware.org/isdl.php
echo   2. Open Installer\RehabCenterApp.iss
echo   3. Click Build -^> Compile
echo.
pause