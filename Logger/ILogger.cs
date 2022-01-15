using System;

namespace Logger
{
    public interface ILogger
    {
        public void RecordToLog(LogType type, string message) { }
        public string ReadTheLog() { return ""; }
        public void ClearLog() { }
    }

    public enum LogType
    {
        info,
        warn,
        error,
        text,
        img
    }
}