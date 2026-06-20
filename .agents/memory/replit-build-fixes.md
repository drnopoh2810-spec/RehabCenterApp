---
name: Replit build fixes for RehabCenterApp
description: Fixes required to compile and run the Windows Avalonia desktop app on Linux/Replit
---

## Key Fixes Applied

**Why:** The project was originally configured for Windows only (win-x64). Running it on Replit's Linux environment required several code and config changes.

**How to apply:** If the project is re-cloned or reset, apply these fixes before building.

### Package Versions
- `AsyncImageLoader.Avalonia`: downgraded to 3.1.0 (3.8.0 requires Avalonia 12)
- `DialogHost.Avalonia`: 0.7.7 (0.12.2 requires Avalonia 12)
- `Material.Icons.Avalonia`: 2.1.0 (matches 11.x Avalonia)
- `MessageBox.Avalonia`: 3.1.5 (API changed to MsBox.Avalonia namespace)

### .csproj Changes
- Removed `<RuntimeIdentifier>win-x64</RuntimeIdentifier>` — cross-platform build
- Changed `<OutputType>WinExe</OutputType>` → `<OutputType>Exe</OutputType>`
- Removed Windows-only publish flags (PublishSingleFile, SelfContained, etc.)
- Removed `<ApplicationManifest>app.manifest</ApplicationManifest>` (Windows-only)
- Removed `<ApplicationIcon>` (icon path was Windows-style)

### Code Fixes
- `DialogService.cs`: Updated to MsBox.Avalonia API (MessageBoxManager.GetMessageBoxStandard)
- `Services/BackupService.cs`: Added `using Microsoft.EntityFrameworkCore` for GetDbConnection
- `Services/PrintService.cs`: Replaced BaseColor.DARK_GRAY/WHITE with new BaseColor(r,g,b)
- `Models/AdvancedFeatures.cs`, `Assessment.cs`, `ParentCommunication.cs`: Added `using System.Collections.Generic`
- Many ViewModels/Helpers: Added `using System.Linq`
- `WaitingList.cs`: Removed stray semicolon after property getter
- `ParentPortalViewModel.cs`: Fixed multi-line string literals (used \n instead of literal newlines)
- `AssessmentsViewModel.cs`, `AnalyticsViewModel.cs`: Fixed SolidColorPaint.SKAlpha → new SKColor(r,g,b,a)
- `BeneficiariesViewModel.cs`: Fixed ExportBeneficiariesToExcelAsync → ExportBeneficiariesAsync
- Many ViewModels: Fixed `ReactiveCommand.Create(() => prop = value)` → `ReactiveCommand.Create(() => { prop = value; })`
- Many ViewModels: Fixed `ReactiveCommand.Create(method)` → `ReactiveCommand.Create(() => method())`
- `HRManagementViewModel.cs`: Added missing CancelCommand property and initialization
- Created `Converters/BoolToStringConverter.cs` (was referenced but missing)

### XAML Fixes
- `App.axaml`: Moved Colors.axaml to Application.Resources (before Styles) to fix StaticResource lookup order
- `App.axaml`: Removed FluentAvaloniaTheme (not available/compatible)
- `App.axaml`: Fixed Material.Icons resource path → `avares://Material.Icons.Avalonia/MaterialIconStyles.axaml`
- `AnalyticsView.axaml`, `GamificationView.axaml`: Escaped `&` → `&amp;`
- Multiple views: Fixed `ComboBox.Text` → `ComboBox.SelectedItem`
- `BeneficiariesView.axaml`: Added `converters:` namespace prefix for BoolToStringConverter
- `FormsView.axaml`: Changed `Kind="FilePdf"` → `Kind="FileExport"` (invalid enum value)
- `RemindersView.axaml`: Changed `Kind="Snooze"` → `Kind="AlarmSnooze"`

### System Dependencies (Nix)
Required: `fontconfig libGL xorg.libX11 xorg.libXext xorg.libXrender xorg.libICE xorg.libSM libxkbcommon`

### Workflow
- Type: VNC (desktop GUI app — no browser preview possible)
- Command: `dotnet run --project RehabCenterApp.csproj`
