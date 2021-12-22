using System;
using System.IO;
using System.Reflection;

namespace InTouchServer
{
    public class LogToFile
    {
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

        public void RecordToLog(MessageType typeevent, string message)
        {
            var text = typeevent + " " + DateTime.Now + " " + Environment.UserName + " " + message + " \n";
            File.AppendAllTextAsync(TotalPath, text);
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
