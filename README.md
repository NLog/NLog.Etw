NLog.Etw
============


[![Version](https://badge.fury.io/nu/NLog.Etw.svg)](https://www.nuget.org/packages/NLog.Etw)
[![AppVeyor](https://img.shields.io/appveyor/ci/nlog/nlog-Etw/master.svg)](https://ci.appveyor.com/project/nlog/nlog-Etw/branch/master)
[![codecov.io](https://codecov.io/github/NLog/NLog.Etw/coverage.svg?branch=master)](https://codecov.io/github/NLog/NLog.Etw?branch=master)

This package is an extension to [NLog](https://github.com/NLog/NLog/). 

## Getting started

See the included Sample at /src/NLog.Signalr.Sample.Web and /src/NLog.SignalR.Sample.Command for an example of running two clients (web and console) at the same time and having log messages appear on the web log page from both sources.

To add to your own projects do the following.

#### Add NLog.Etw.dll to your project(s) via [NuGet](http://www.nuget.org/packages/NLog.Etw/)

  > install-package NLog.Etw

#### Configure NLog

Add the assembly and new target to NLog.config:

```xml
<?xml version="1.0" encoding="utf-8" ?>
<nlog   xmlns="http://www.nlog-project.org/schemas/NLog.xsd"
        xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"
        autoReload="true"
        throwExceptions="false">

      <!-- extensions is not needed in NLog 4+ -->
      <extensions>
        <add assembly="NLog.Etw" />
      </extensions>

      <targets async="true">
        <target xsi:type="ExtendedEventTracing"
                name="eetw"
                layout="${longdate}|${uppercase:${level}}|${message}${onexception:|Exception occurred\:${exception:format=tostring}}"
              />

    </targets>
    
    <rules>
      <logger name="*" minlevel="Trace" writeTo="eetw" />
    </rules>
</nlog>
```

#### Register Events (optional)

If you desire to leverage the Windows Event Viewer (and channel-related Event Logs), you may install the event manifest on the target machine(s).
1. Copy RegisterEvents.exe to the target machine.
2. Run RegisterEvents.exe as Administrator.
   * This will copy the manifest and resource files to the %SystemRoot%\System32\ folder: 
     * NLog.Etw.NLog-LogEvents.etwManifest.man
     * NLog.Etw.NLog-LogEvents.etwManifest.dll
   * ... and then register the manifest.
   * (Unregister by running %SystemRoot%\System32\NLog.Etw.NLog-LogEvents.etwManifest.Unregister.bat)
3. You may then use your favorite tool to verify that the provider is installed: 

```shell
c:\>logman query providers
```
4. Use Windows Event Viewer to view, enable, or disable channel logs.
   1. Expand Applications and Services Logs
   2. Expand NLog-LogEvents
   3. (Optionally) Enable debug log (disabled by default).
