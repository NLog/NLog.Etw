This [solution] folder contains files used by RegisterEvents.EXE:

NLog.Etw.NLog-LogEvents.etwManifest.man			ETW Manifest for NLog Provider
NLog.Etw.NLog-LogEvents.etwManifest.dll			Resource and Message file for manifest
NLog.Etw.NLog-LogEvents.etwManifest.Register.bat	Registers manifest	
NLog.Etw.NLog-LogEvents.etwManifest.Unregister.bat	Unregisters manifest

Copy2Sys.bat	Copies above files to %SystemRoot%\System32\ for registration
Register.bat	Install-time registration from temp directory