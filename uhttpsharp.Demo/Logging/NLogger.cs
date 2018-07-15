using System;
using NLog;

namespace uhttpsharp.Demo.Logging {
    public class NLogger : ILogger {
        private readonly string _loggerName;

        private Logger _nlog = LogManager.GetCurrentClassLogger();

        public NLogger(string loggerName) {
            _loggerName = loggerName;
        }

        public void Fatal(string message, Exception e = null) {
            Log(LogLevel.Fatal, message, e);
        }

        public void Error(string message, Exception e = null) {
            Log(LogLevel.Error, message, e);
        }

        public void Warn(string message, Exception e = null) {
            Log(LogLevel.Warn, message, e);
        }

        public void Info(string message, Exception e = null) {
            Log(LogLevel.Info, message, e);
        }

        public void Debug(string message, Exception e = null) {
            Log(LogLevel.Debug, message, e);
        }

        public void Trace(string message, Exception e = null) {
            Log(LogLevel.Trace, message, e);
        }

        private void Log(LogLevel level, string message, Exception e = null) {
            var logEvent = new LogEventInfo(level, _loggerName, message) {
                Exception = e
            };
            // set event-specific context parameter this context parameter can be retrieved using ${event-properties:EventID}
            //logEvent.Properties["EventID"] = "id";
            _nlog.Log(typeof(ILogger), logEvent);
        }
    }
}