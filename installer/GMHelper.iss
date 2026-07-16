; Inno Setup script for GMHelper.
; Built via scripts\Build-InnoInstaller.ps1, which passes -DAppVersion and -DSourceDir.
; AppId is a fixed GUID (do not change) so Inno Setup recognizes future versions as upgrades
; of the same install rather than a separate side-by-side app.

#ifndef AppVersion
  #define AppVersion "0.0.0"
#endif
#ifndef SourceDir
  #define SourceDir "..\publish-inno"
#endif

[Setup]
AppId={{7AF6DD9F-0506-4538-9121-FA99230E3FAB}
AppName=GMHelper
AppVersion={#AppVersion}
AppPublisher=Markus Klatte-Schür
DefaultDirName={localappdata}\Programs\GMHelper
DefaultGroupName=GMHelper
DisableProgramGroupPage=yes
PrivilegesRequired=lowest
OutputDir=..\dist
OutputBaseFilename=GMHelper-Setup-{#AppVersion}
Compression=lzma2/max
SolidCompression=yes
WizardStyle=modern
UninstallDisplayIcon={app}\GMHelper.App.exe
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible

[Languages]
Name: "german"; MessagesFile: "compiler:Languages\German.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
Source: "{#SourceDir}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\GMHelper"; Filename: "{app}\GMHelper.App.exe"
Name: "{group}\{cm:UninstallProgram,GMHelper}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\GMHelper"; Filename: "{app}\GMHelper.App.exe"; Tasks: desktopicon

[Run]
Filename: "{app}\GMHelper.App.exe"; Description: "{cm:LaunchProgram,GMHelper}"; Flags: nowait postinstall skipifsilent
