# Installation Guide - RehabCenterApp

## System Requirements

| Component | Requirement |
|-----------|-------------|
| OS | Windows 10/11 (64-bit) |
| CPU | Intel Core i3 or higher |
| RAM | 4 GB minimum (8 GB recommended) |
| Disk | 500 MB free space |
| .NET | 8.0 SDK (for building) |

## Method 1: Visual Studio (Easiest)

### Step 1: Install Prerequisites
1. Download and install Visual Studio 2022 (Community Edition is free)
2. During installation, select: .NET desktop development workload

### Step 2: Open Project
1. Double-click RehabCenterApp.sln
2. Wait for NuGet packages to restore automatically
3. Press Ctrl+Shift+B to build
4. Press F5 to run

### Step 3: Publish
1. Right-click project in Solution Explorer
2. Select Publish
3. Choose Folder target
4. Click Publish button

## Method 2: Command Line (build.bat)

### Step 1: Install .NET 8.0 SDK
https://dotnet.microsoft.com/download/dotnet/8.0

### Step 2: Navigate to Project Folder
```cmd
cd C:\Path\To\RehabCenterApp
```

### Step 3: Run Build Script
```cmd
build.bat
```

## Creating the Installer (EXE)

### Step 1: Install Inno Setup
https://jrsoftware.org/isdl.php

### Step 2: Open Inno Setup Script
1. Launch Inno Setup Compiler
2. File -> Open -> Select Installer/RehabCenterApp.iss

### Step 3: Compile
1. Press F9 or click Build -> Compile
2. Installer will be created at: Publish/RehabCenterApp_Setup_v1.0.0.exe

## Troubleshooting

### Error: "MSB1003: Specify a project or solution file"
**Cause**: Running build.bat from wrong directory
**Fix**: Make sure you are in the same folder as RehabCenterApp.csproj

### Error: "dotnet is not recognized"
**Cause**: .NET SDK not installed or not in PATH
**Fix**: Install .NET 8.0 SDK and restart Command Prompt

## Default Login Credentials
| Username | Password | Role |
|----------|----------|------|
| admin | admin123 | Administrator |

**WARNING**: Change default password immediately after first login!

## Support
Email: support@rehabcenterapp.com
Website: https://rehabcenterapp.com