# RehabCenterApp — Integrated Rehabilitation Center Management System

## Overview

A C# desktop application built with Avalonia UI (cross-platform) targeting .NET 8. It uses the MVVM architecture pattern and SQLite for local data storage. The app provides a comprehensive management system for rehabilitation centers.

## Architecture

- **Framework**: Avalonia UI 11.2.8 (cross-platform desktop UI)
- **Language**: C# with .NET 8
- **Database**: SQLite via Entity Framework Core 8
- **Pattern**: MVVM with ReactiveUI + CommunityToolkit.Mvvm
- **Charts**: LiveChartsCore with SkiaSharp

## Project Structure

- `Models/` — Data entities (Beneficiary, Session, Payment, Assessment, etc.)
- `Views/` — Avalonia XAML UI definitions (`.axaml` files)
- `ViewModels/` — Application logic and state
- `Services/` — Business logic (DatabaseService, NavigationService, etc.)
- `Converters/` — XAML value converters
- `Helpers/` — Utilities (AuditLogger, ExcelExporter, PasswordHasher)
- `Styles/` — Custom XAML styles and color resources

## Running the App

The app is configured as a VNC workflow (desktop GUI). It builds and runs via:
```
dotnet run --project RehabCenterApp.csproj
```

## Default Login

| Username | Password | Role |
|----------|----------|------|
| admin | admin123 | Administrator |

## Key Features

- 22 integrated management systems
- Standardized assessments (GARS-2, WISC-V, GMFM, PEP-3)
- IEP (Individualized Education Plans) with SMART goals
- Telehealth session management
- AI-powered analytics and predictions
- Gamification system for children
- Parent communication portal
- Government reporting
- HR management
- Inventory tracking

## Build Notes (Replit-specific fixes applied)

- Upgraded from Windows-only (`win-x64`) to cross-platform build
- Fixed package version conflicts: `AsyncImageLoader.Avalonia` → 3.1.0, `DialogHost.Avalonia` → 0.7.7
- Removed `FluentAvaloniaTheme` from App.axaml (not available in this version)
- Fixed `Material.Icons.Avalonia` resource path → `MaterialIconStyles.axaml`
- Added missing `using System.Linq;` and `using System.Collections.Generic;` across Models/ViewModels
- Fixed unescaped `&` in XAML files
- Fixed `ReactiveCommand.Create` lambda return type issues (assignment expressions → block statements)
- Fixed `ComboBox.Text` → `ComboBox.SelectedItem` bindings
- Fixed `BaseColor.DARK_GRAY/WHITE` → `new BaseColor(r,g,b)`
- Created missing `BoolToStringConverter`
- Added missing `CancelCommand` to `HRManagementViewModel`
- Installed native deps: fontconfig, libGL, X11 libs, libxkbcommon

## User Preferences

(none set yet)
