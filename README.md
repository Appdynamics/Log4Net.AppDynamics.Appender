# Log4Net.AppDynamics.Appender

This project provides the log4net appender library for AppDynamics. You can use the library to send all of your logs to AppDynamics from log4net calls. The log messages are buffered in memory and sent to AppDynamics with bulk requests. AppDynamics will collect your logs from multiple sources and provide rich powerful search and analytic capabilities.

## Installation:
```
dotnet add reference <path of Log4Net.AppDynamics.Appender project file>
```
    OR
```
dotnet add package AppDynamics.Log4NetAppender --version 1.0.0
```
## Setup:

Use the following command to create the log entry schema in AppDynamics
```
curl "http(s)://<appd_event_host_name>(:<event_services_port>)/events/schema/<schema_name>" -H"X-Events-API-AccountName:<global_account_name>" -H"X-Events-API-Key:<api_key>" -H"Content-type: application/vnd.appd.events+json;v=2" -d "{ \"schema\": { \"EntryTimestamp\": \"date\", \"Level\": \"string\", \"SourceContext\": \"string\", \"MachineName\": \"string\", \"RenderedMessage\": \"string\", \"Properties\": \"string\",\"Exception\": \"string\" } }"

```
## Usage
To use AppDynamicsAppender, just add AppDynamics event service, credential and program settings to your log4net config file:

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

See test code: 
- [Log4Net.AppDynamics.Appender.Tests](https://github.com/Appdynamics/Log4Net.AppDynamics.Appender/tree/master/sources/Log4Net.AppDynamics.Appender.Tests)
- [Log4Net.AppDynamics.Appender.Aspnet.Tests](https://github.com/Appdynamics/Log4Net.AppDynamics.Appender/tree/master/sources/Log4Net.AppDynamics.Appender.Aspnet.Tests)