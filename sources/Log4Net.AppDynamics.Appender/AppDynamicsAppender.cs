using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using log4net.Appender;
using log4net.Core;
using Newtonsoft.Json;

namespace Log4net.AppDynamics.Appender
{
    public class AppDynamicsAppender: AppenderSkeleton
    {
        protected static readonly string MachineName = Dns.GetHostName();

        protected static readonly JsonSerializerSettings SerializerSettings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore
        };

        protected static readonly HttpClient Client = new HttpClient();

        /// <summary>
        /// Specifies AppDynamics Event Service endpoint to which send a POST request with collected events
        /// </summary>
        /// <remarks>Default: http://localhost:9080</remarks>
        public string AppdEndpoint { get; set; } = "http://localhost:9080";

        /// <summary>
        /// Specifies the custom event schema for the log entries
        /// </summary>
        /// <remarks>Default: log4net_entry</remarks>
        public string AppdSchemaName { get; set; } = "log4net_entry";

        /// <summary>
        /// Specifies AppDynamics Global AccountName
        /// </summary>
        /// <remarks>Default: ""</remarks>
        public string AppdGlobalAccount { get; set; } = "";

        /// <summary>
        /// Specifies AppDynamics Api-key
        /// </summary>
        /// <remarks>Default: ""</remarks>
        public string AppdApiKey { get; set; } = "";       
        
        /// <summary>
        /// Specifies AppDynamics event content type
        /// </summary>
        /// <remarks>Default: "application/vnd.appd.events+json;v=2"</remarks>
        public string AppdContentType { get; set; } = "application/vnd.appd.events+json;v=2";       
        
        /// <summary>
        /// When HTTP endpoint is not available - how many retries to do before throwing events away
        /// </summary>
        /// <remarks>Default: 100</remarks>
        public int ErrorMaxRetries { get; set; } = 100;

        /// <summary>
        /// Specifies how long to sleep between retries
        /// </summary>
        /// <remarks>Default: 100 milliseconds</remarks>
        public TimeSpan ErrorSleepTime { get; set; } = TimeSpan.FromMilliseconds(100);

        protected override void Append(LoggingEvent loggingEvent)
        {
            IDictionary loggingProperties = loggingEvent.GetProperties();

            Dictionary<string, object> properties = null;
            string machineName = null;
            if (loggingProperties != null && loggingProperties.Count > 0)
            {
                properties = new Dictionary<string, object>();
                foreach (var property in loggingProperties.Keys)
                {
                    var key = property.ToString();
                    if (key == "log4net:HostName")
                    {
                        machineName = loggingProperties[property].ToString();
                        continue;
                    }

                    properties[key] = loggingProperties[property];
                }
            }

            if (string.IsNullOrWhiteSpace(machineName))
                machineName = MachineName;

            var exception = loggingEvent.ExceptionObject?.ToString();

            var entry = new LogEntry
            {
                EntryTimestamp = loggingEvent.TimeStamp.ToUniversalTime(),
                RenderedMessage = loggingEvent.RenderedMessage,
                Level = loggingEvent.Level.Name,
                MachineName = machineName,
                SourceContext = loggingEvent.LoggerName,
                Properties = string.Join(Environment.NewLine, properties),
                Exception = exception
            };

            if (string.IsNullOrWhiteSpace(entry.RenderedMessage) && loggingEvent.ExceptionObject != null)
                entry.RenderedMessage = loggingEvent.ExceptionObject.Message;

            Task.Run(() => EnqueueAsync(entry)).GetAwaiter().GetResult();
        }

        protected virtual Task EnqueueAsync(LogEntry entry)
        {
            return SendEvents(entry);
        }

        protected async Task<bool> SendEvents(params LogEntry [] entries)
        {
            if (entries == null || entries.Length == 0)
                return true;

            var entriesAreSent = false;
            var reties = 0;
            while (true)
            {
                var content = FormatContent(entries);

                HttpResponseMessage response = null;
                try
                {
                    HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Post, AppdEndpoint + "/events/publish/" + AppdSchemaName + "/");
                    
                    // Need to set AppDynamics headers for content
                    MediaTypeHeaderValue AppdJsonMediaType = null;

                    // AppDynamics requires a custom content type 
                    MediaTypeHeaderValue.TryParse(AppdContentType, out AppdJsonMediaType);
                    req.Content = new StringContent(content, Encoding.UTF8 );
                    req.Content.Headers.ContentType = AppdJsonMediaType;     

                    req.Content.Headers.Add("X-Events-API-AccountName", AppdGlobalAccount);
                    req.Content.Headers.Add("X-Events-API-Key", AppdApiKey);
    
                    response = await Client
                        .SendAsync(req)
                        .ConfigureAwait(false);

                    if (response.IsSuccessStatusCode)
                    {
                        entriesAreSent = true;
                        break;
                    }
                }
                catch (Exception e)
                {
                    System.Diagnostics.Trace.WriteLine(e.ToString());
                }

                if (response == null || !response.IsSuccessStatusCode)
                {
                    reties += 1;
                    if (reties == ErrorMaxRetries)
                    {
                        if (response?.Content != null)
                        {
                            System.Diagnostics.Trace.WriteLine(response.Content.ToString() + ": " + response.ReasonPhrase);
                        }
                        break;
                    }

                    if (ErrorSleepTime > TimeSpan.Zero)
                    {
                        await Task.Delay(ErrorSleepTime).ConfigureAwait(false);
                    }
                }
            }

            return entriesAreSent;
        }

        private static string FormatContent(LogEntry[] entries)
        {
            var content = JsonConvert.SerializeObject(entries, Formatting.None, SerializerSettings);
            return content;
        }
    }
}
