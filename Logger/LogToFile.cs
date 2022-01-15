using System;
using System.IO;
using System.Reflection;

namespace Logger
{
    public class LogToFile: ILogger
    {
        public static event Action<LogType, string> Notify;
        private readonly string TotalPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "TotalLog.log");

        public LogToFile() { }

        public LogToFile(string path)
        {
            try
            { TotalPath = Path.Combine(path, "TotalLog.log"); }
            catch
            { Notify?.Invoke(LogType.error, "Путь к TotalLog.log не найден."); }            
        }

        public async void RecordToLog(LogType type, string message)
        {            
            var text = type + " " + message;
            try
            {
                using (StreamWriter writer = new(TotalPath, true))
                { await writer.WriteLineAsync(text); }
            }
            catch (Exception e) { Notify?.Invoke(LogType.error, e.ToString());}
        }

        public string ReadTheLog()
        {
            string log = "";
            if (File.Exists(TotalPath))
            {
                try
                {
                    using (StreamReader reader = new(TotalPath))
                    { log += reader.ReadToEnd(); }
                }
                catch (OutOfMemoryException)
                { Notify?.Invoke(LogType.error, 
                    "Не хватает памяти для выделения буфера под возвращаемую строку."); }
                catch (IOException) 
                { Notify?.Invoke(LogType.error, "Ошибка ввода-вывода.");}
            }
            return log;
        }

        public void ClearLog()
        {
            try
            {
                using (StreamWriter writer = new(TotalPath, false))
                { writer.Write(""); }
            }
            catch (InvalidOperationException)
            {
                Notify?.Invoke(LogType.error,
                  "Поток в настоящее время используется предыдущей операцией записи.");
            }
            catch (Exception e) { Notify?.Invoke(LogType.error, e.ToString()); }
        }
    }
}