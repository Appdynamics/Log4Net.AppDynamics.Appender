using System;
using System.Collections.Generic;

namespace Log4net.AppDynamics.Appender
{
    public class LogEntry
    {
        public DateTime EntryTimestamp { get; set; }
        public string Level { get; set; }
        public string RenderedMessage { get; set; }
        public string MachineName { get; set; }
        public string SourceContext { get; set; }
        public string Properties { get; set; }
        public string Exception { get; set; }
    }
}
