; PrintAgent Installer Script for Inno Setup
; Requires Inno Setup 6.x or later
; Download from: https://jrsoftware.org/isdl.php

#define MyAppName "PrintAgent"
#define MyAppVersion "1.1.0"
#define MyAppPublisher "Nebulosa"
#define MyAppURL "https://github.com/nebulosa"
#define MyServiceName "PrintAgent.Service"
#define MyServiceExeName "PrintAgent.Service.exe"

[Setup]
; Application info
AppId={{8E6D4F2A-3B5C-4D7E-9F1A-2C8B6D4E5F3A}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}

; Installation directories
DefaultDirName={autopf}\{#MyAppName}
DefaultGroupName={#MyAppName}
DisableProgramGroupPage=yes

; Output settings
OutputDir=..\dist
OutputBaseFilename=PrintAgent-Setup-{#MyAppVersion}
Compression=lzma2
SolidCompression=yes

; Permissions
PrivilegesRequired=admin
PrivilegesRequiredOverridesAllowed=dialog

; Visual settings
WizardStyle=modern
UninstallDisplayIcon={app}\service\{#MyServiceExeName}

; Misc
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible

[Languages]
Name: "spanish"; MessagesFile: "compiler:Languages\Spanish.isl"
Name: "english"; MessagesFile: "compiler:Default.isl"

[Files]
; Service files only
Source: "..\publish\service\*"; DestDir: "{app}\service"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\{cm:UninstallProgram,{#MyAppName}}"; Filename: "{uninstallexe}"

[Run]
; Install and start the Windows service
Filename: "sc.exe"; Parameters: "create ""{#MyServiceName}"" binPath= ""{app}\service\{#MyServiceExeName}"" start= auto DisplayName= ""PrintAgent Service"""; Flags: runhidden waituntilterminated; StatusMsg: "Instalando servicio..."
Filename: "sc.exe"; Parameters: "description ""{#MyServiceName}"" ""Servicio de impresión para aplicaciones web"""; Flags: runhidden waituntilterminated
Filename: "sc.exe"; Parameters: "start ""{#MyServiceName}"""; Flags: runhidden waituntilterminated; StatusMsg: "Iniciando servicio..."
; Configure firewall
Filename: "netsh.exe"; Parameters: "advfirewall firewall add rule name=""PrintAgent"" dir=in action=allow protocol=TCP localport=5123"; Flags: runhidden waituntilterminated; StatusMsg: "Configurando firewall..."

[UninstallRun]
; Stop and remove the Windows service
Filename: "sc.exe"; Parameters: "stop ""{#MyServiceName}"""; Flags: runhidden waituntilterminated
Filename: "sc.exe"; Parameters: "delete ""{#MyServiceName}"""; Flags: runhidden waituntilterminated
; Remove firewall rule
Filename: "netsh.exe"; Parameters: "advfirewall firewall delete rule name=""PrintAgent"""; Flags: runhidden waituntilterminated

[Code]
// Check if service is running and stop it before uninstall
procedure CurUninstallStepChanged(CurUninstallStep: TUninstallStep);
var
  ResultCode: Integer;
begin
  if CurUninstallStep = usUninstall then
  begin
    // Try to stop the service gracefully
    Exec('sc.exe', 'stop "{#MyServiceName}"', '', SW_HIDE, ewWaitUntilTerminated, ResultCode);
    // Give it time to stop
    Sleep(2000);
  end;
end;

// Check if upgrading and stop service first
procedure CurStepChanged(CurStep: TSetupStep);
var
  ResultCode: Integer;
begin
  if CurStep = ssInstall then
  begin
    // Try to stop existing service before upgrade
    Exec('sc.exe', 'stop "{#MyServiceName}"', '', SW_HIDE, ewWaitUntilTerminated, ResultCode);
    Sleep(2000);
    // Delete old service (will be recreated)
    Exec('sc.exe', 'delete "{#MyServiceName}"', '', SW_HIDE, ewWaitUntilTerminated, ResultCode);
    Sleep(1000);
  end;
end;
