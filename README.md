# Log4Net.AppDynamics.Appender

Installation:
```
dotnet add reference <path of Log4Net.AppDynamics.Appender project file>
```
    OR
```
(Nuget package pending)
```
To use AppDynamicsAppender just add AppDynamics event service, credential and program settings to your log4net config file:

```
<appender name="AppDynamicsAppender" type="Log4net.AppDynamics.Appender.AppDynamicsBufferedAppender, Log4net.AppDynamics.Appender">
    <AppdEndpoint>http://localhost:9080</AppdEndpoint>
    <AppdSchemaName>log4net_entry</AppdSchemaName>
    <AppdGlobalAccount>customer1_xxxx-xxxx-xxxxxx-xxxxxxx</AppdGlobalAccount>
    <AppdApiKey>xxxxx-xxxxx-xxxxx-xxxxx-xxxxx</AppdApiKey>
    <AppdContentType>application/vnd.appd.events+json;v=2</AppdContentType>               
    <ErrorMaxRetries>10</ErrorMaxRetries>
    <ErrorSleepTime>00:00:00.200</ErrorSleepTime>
    <BatchMaxSize>100</BatchMaxSize>
    <BatchSleepTime>00:00:00.200</BatchSleepTime>
</appender>
```

Include appender to your logger:
```
<root>
    <level value="ALL" />
    <appender-ref ref="AppDynamicsAppender" />
</root>
```

See test code: [Log4Net.AppDynamics.Appender.Tests](https://github.com/charleslin-appd/Log4Net.AppDynamics.Appender/tree/master/sources/Log4Net.AppDynamics.Appender.Tests)