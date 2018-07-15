using System;

namespace uhttpsharp {
    public interface ILogger {
        void Fatal(string message, Exception e = null);
        void Error(string message, Exception e = null);
        void Warn(string message, Exception e = null);
        void Info(string message, Exception e = null);
        void Debug(string message, Exception e = null);
        void Trace(string message, Exception e = null);
    }
}