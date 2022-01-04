using System;
using InTouchServer;
using Logger;

namespace ServerInTouch
{
    class Program
    {
        static void Main()
        {
            Output(MessageType.info, $"{DateTime.Now} Server start");
            LogToFile.Notify += Output;
            TcpServer.Notify += Output;
            ClientConnection.Notify += Output;
            var log = new LogToFile();
            TcpServer.Notify += log.RecordToLog;
            ClientConnection.Notify += log.RecordToLog;

            int port, amt;
            Console.WriteLine("Введите номер порта");
            var stringPort = Console.ReadLine();
            port = NumberCheck(stringPort);
            Console.WriteLine("Введите максимальное количество одновременных соединений");
            var stringAmt = Console.ReadLine();
            amt = NumberCheck(stringAmt);    
            var server = new TcpServer(port, amt);
            server.StartTcpServer();
        }

        static int NumberCheck(string inputString)
        {            
            bool correct=false;
            int number=-1;            
            do
            {
                correct = Int32.TryParse(inputString, out number);
                if (!correct || number <= 0)
                {
                    Console.WriteLine("Введите только целое положительное число");
                    inputString = Console.ReadLine();
                }                
            }
            while (number<=0);
            return number;
        }

        static void Output(MessageType type,string message)
        {
            switch (type)
            {
                case MessageType.info:
                    Console.ForegroundColor = ConsoleColor.Green;
                    break;
                case MessageType.warn:
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                    break;
                case MessageType.error:
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
                case MessageType.text:
                    Console.ForegroundColor = ConsoleColor.White;
                    break;
            }
            Console.WriteLine($"{type} {message}");
            Console.ResetColor();
        }
    }
}