; Inno Setup Script for RehabCenterApp
; Professional installer for Windows

#define MyAppName "RehabCenterApp"
#define MyAppVersion "1.0.0"
#define MyAppPublisher "RehabCenterApp"
#define MyAppURL "https://rehabcenterapp.com"
#define MyAppExeName "RehabCenterApp.exe"
#define MyAppIcon "Assets\Icons\app_icon.ico"

[Setup]
AppId={{A1B2C3D4-E5F6-7890-ABCD-EF1234567890}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppVerName={#MyAppName} {#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
DefaultDirName={autopf}\RehabCenterApp
DisableProgramGroupPage=yes
OutputDir=Publish
OutputBaseFilename=RehabCenterApp_Setup_v1.0.0
Compression=lzma2
SolidCompression=yes
SetupIconFile={#MyAppIcon}
WizardImageFile=Installer\wizard_image.bmp
WizardSmallImageFile=Installer\wizard_small_image.bmp
PrivilegesRequired=lowest
PrivilegesRequiredOverridesAllowed=dialog
ArchitecturesAllowed=x64
ArchitecturesInstallIn64BitMode=x64
UninstallDisplayIcon={app}\app_icon.ico
UninstallDisplayName={#MyAppName}
WizardStyle=modern

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
Source: "bin\Release\net8.0\win-x64\publish\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "Assets\Icons\app_icon.ico"; DestDir: "{app}"; Flags: ignoreversion
Source: "LICENSE.txt"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
Name: "{autoprograms}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; IconFilename: "{app}\app_icon.ico"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; IconFilename: "{app}\app_icon.ico"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#MyAppName}}"; Flags: nowait postinstall skipifsilent

[Registry]
Root: HKCU; Subkey: "Software\RehabCenterApp"; ValueType: string; ValueName: "InstallPath"; ValueData: "{app}"; Flags: uninsdeletekey

[UninstallDelete]
Type: filesandordirs; Name: "{app}\*"
