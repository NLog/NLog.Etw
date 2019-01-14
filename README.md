NLog.Etw
============


[![Version](https://badge.fury.io/nu/NLog.Etw.svg)](https://www.nuget.org/packages/NLog.Etw)
[![AppVeyor](https://img.shields.io/appveyor/ci/nlog/nlog-Etw/master.svg)](https://ci.appveyor.com/project/nlog/nlog-Etw/branch/master)
[![codecov.io](https://codecov.io/github/NLog/NLog.Etw/coverage.svg?branch=master)](https://codecov.io/github/NLog/NLog.Etw?branch=master)

This package is an extension to [NLog](https://github.com/NLog/NLog/). 

## Getting started

To add to your own projects do the following.

#### Add NLog.Etw.dll to your project(s) via [NuGet](http://www.nuget.org/packages/NLog.Etw/)

  > install-package NLog.Etw

#### Configure NLog

Add the assembly and new target to NLog.config:

```xml
<?xml version="1.0" encoding="utf-8" ?>
<nlog xmlns="http://www.nlog-project.org/schemas/NLog.xsd"
      xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"
      autoReload="true"
      throwExceptions="false">

    <extensions>
        <add assembly="NLog.Etw" />
    </extensions>

    <targets async="true">
        <target xsi:type="EtwEventSource"
                name="eetw"
				providerName="MyEventSourceName"
				taskName="${level}"
                layout="${message}"
              />
    </targets>
    
    <rules>
      <logger name="*" minlevel="Trace" writeTo="eetw" />
    </rules>
</nlog>
```
