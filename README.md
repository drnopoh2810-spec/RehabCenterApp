# RehabCenterApp
# Integrated Rehabilitation Center Management System

## Features

- 22 integrated systems for center management
- Standardized assessments (GARS-2, WISC-V, GMFM, PEP-3)
- Individualized Education Plans (IEP) with SMART goals
- Telehealth sessions
- AI-powered analytics and predictions
- Gamification system for children
- Parent communication via WhatsApp/SMS
- Government reporting

## System Requirements

| Component | Requirement |
|-----------|-------------|
| OS | Windows 10/11 (64-bit) |
| CPU | Intel Core i3 or higher |
| RAM | 4 GB minimum (8 GB recommended) |
| Disk | 500 MB free space |
| .NET | 8.0 SDK (for building) |

## Building

### Method 1: Visual Studio

1. Install Visual Studio 2022 with .NET desktop development workload
2. Open RehabCenterApp.sln
3. Press Ctrl+Shift+B to build
4. Press F5 to run

### Method 2: Command Line

```bash
cd RehabCenterApp
dotnet restore RehabCenterApp.csproj
dotnet build RehabCenterApp.csproj -c Release
dotnet publish RehabCenterApp.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```

## Creating Installer

1. Install Inno Setup from https://jrsoftware.org/isdl.php
2. Open Installer/RehabCenterApp.iss
3. Click Build -> Compile
4. Output: Publish/RehabCenterApp_Setup_v1.0.0.exe

## Default Login

| Username | Password | Role |
|----------|----------|------|
| admin | admin123 | Administrator |

**WARNING**: Change default password immediately after first login!

## License

MIT License - Copyright (c) 2026 RehabCenterApp