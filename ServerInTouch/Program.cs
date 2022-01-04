﻿using System;
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
            var server = new TcpServer(8005, 10);
            server.StartTcpServer();
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