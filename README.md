# NLog.Etw

[![Version](https://badge.fury.io/nu/NLog.Etw.svg)](https://www.nuget.org/packages/NLog.Etw)
[![AppVeyor](https://img.shields.io/appveyor/ci/nlog/nlog-Etw/master.svg)](https://ci.appveyor.com/project/nlog/nlog-Etw/branch/master)
[![codecov.io](https://codecov.io/github/NLog/NLog.Etw/coverage.svg?branch=master)](https://codecov.io/github/NLog/NLog.Etw?branch=master)

NLog Target for writing logevents to Event Tracing for Windows (ETW).

## Install nuget-package to your project:

  > dotnet add package NLog.Etw

## Example NLog.config file

Example of `NLog.config`-file that writes to ETW:

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

## Example Custome EventSource

Example of providing own custom EventSource for writing events:

```c#
[EventSource(Name = "MyEventSourceName")]
public class MyEventSource : EventSource, NLog.Etw.INLogEventSource
{
    /// <inheritdoc/>
    [NonEvent]
    public void Write(EventLevel eventLevel, string layoutMessage, LogEventInfo logEvent)
    {
        // TODO Call own custom logging-method
    }

    /// <inheritdoc/>
    EventSource EventSource { get { return this; } }

    internal readonly static MyEventSource Log = new MyEventSource();
}

var config = new NLog.Config.LoggingConfiguration();
config.AddRuleForAllLevels(new NLog.Etw.NLogEtwExtendedTarget(MyEventSource.Log) { Name = "eetw" });
NLog.LogManager.Configuration = config;
```