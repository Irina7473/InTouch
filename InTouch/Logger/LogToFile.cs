using System;
using System.IO;
using System.Reflection;

namespace InTouchServer
{
    public class LogToFile
    {
        public static event Action<MessageType, string> Notify;
        private readonly string TotalPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "TotalLog.log");

        public LogToFile() { }

        public LogToFile(string path)
        {
            try
            {
                TotalPath = Path.Combine(path, "TotalLog.log");
            }
            catch
            {
                throw new Exception("Путь к TotalLog.log не найден");
            }
        }

        public void RecordToLog(MessageType type, string message)
        {            
            var text = type + " " + message + " \n";
            try
            {
                File.AppendAllTextAsync(TotalPath, text); //STRIMWRITEASYNC
            }
            catch (Exception e)
            {
                Notify?.Invoke(MessageType.error, e.ToString());
            }
        }

        public string ReadTheLog()
        {
            string log = "";
            if (File.Exists(TotalPath))
            {
                StreamReader reader = new(TotalPath);
                log += reader.ReadToEnd();
                reader.Close();
            }
            return log;
        }

        public void ClearLog()
        {
            File.WriteAllText(TotalPath, null);
        }
    }
}
