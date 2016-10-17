[Version]
Class=IEXPRESS
SEDVersion=3
[Options]
PackagePurpose=InstallApp
ShowInstallProgramWindow=0
HideExtractAnimation=0
UseLongFileName=1
InsideCompressed=0
CAB_FixedSize=0
CAB_ResvCodeSigning=0
RebootMode=N
InstallPrompt=%InstallPrompt%
DisplayLicense=%DisplayLicense%
FinishMessage=%FinishMessage%
TargetName=%TargetName%
FriendlyName=%FriendlyName%
AppLaunched=%AppLaunched%
PostInstallCmd=%PostInstallCmd%
AdminQuietInstCmd=%AdminQuietInstCmd%
UserQuietInstCmd=%UserQuietInstCmd%
SourceFiles=SourceFiles
[Strings]
InstallPrompt=Register NLog ETW Provider and Logs?
DisplayLicense=
FinishMessage=NLog ETW Provider and Logs registered
TargetName=C:\RegisterEvents.EXE
FriendlyName=Register NLog ETW Provider and Logs
AppLaunched=cmd /c Copy2Sys.bat
PostInstallCmd=cmd /c Register.bat
AdminQuietInstCmd=
UserQuietInstCmd=
FILE0="NLog.Etw.NLog-LogEvents.etwManifest.dll"
FILE1="NLog.Etw.NLog-LogEvents.etwManifest.man"
FILE2="NLog.Etw.NLog-LogEvents.etwManifest.Unregister.bat"
FILE3="NLog.Etw.NLog-LogEvents.etwManifest.Register.bat"
FILE4="Register.bat"
FILE5="Copy2Sys.bat"
[SourceFiles]
SourceFiles0=C:\NLog.Etw\NLog.Etw\RegisterEvents\
[SourceFiles0]
%FILE0%=
%FILE1%=
%FILE2%=
%FILE3%=
%FILE4%=
%FILE5%=
