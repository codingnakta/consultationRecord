#define MyAppName "학생 상담 관리"
#define MyAppPublisher "codingnakta"
#define MyAppVersion GetEnv("APP_VERSION")
#if MyAppVersion == ""
#define MyAppVersion "1.0.0"
#endif
#define MyAppExeName "StudentCounseling.exe"
#define PublishDir GetEnv("PUBLISH_DIR")
#if PublishDir == ""
#define PublishDir "..\StudentCounseling\bin\Release\net8.0-windows\win-x64\publish"
#endif
#define OutputDir GetEnv("INSTALLER_OUTPUT_DIR")
#if OutputDir == ""
#define OutputDir "..\artifacts"
#endif

[Setup]
AppId={{8F4F0C32-96E2-4A70-83B7-40EB0DAB65CA}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
DefaultDirName={localappdata}\Programs\StudentCounseling
DefaultGroupName={#MyAppName}
DisableProgramGroupPage=yes
OutputDir={#OutputDir}
OutputBaseFilename=StudentCounseling_Setup_{#MyAppVersion}
Compression=lzma
SolidCompression=yes
WizardStyle=modern
PrivilegesRequired=lowest
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
UninstallDisplayName={#MyAppName}

[Languages]
Name: "korean"; MessagesFile: "compiler:Languages\Korean.isl"

[Tasks]
Name: "desktopicon"; Description: "바탕화면 바로가기 만들기"; GroupDescription: "추가 바로가기:"; Flags: checkedonce

[Files]
Source: "{#PublishDir}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{#MyAppName} 실행"; Flags: nowait postinstall skipifsilent
