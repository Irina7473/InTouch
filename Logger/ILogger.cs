using System;

namespace Logger
{
    public interface ILogger
    {
        public void RecordToLog(MessageType type, string message) { }
        public string ReadTheLog() { return ""; }
        public void ClearLog() { }
    }

    public enum MessageType
    {
        info,
        warn,
        error,
        ident,
        text,
        img
    }
}