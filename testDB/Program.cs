using System;
using System.Text;
using DataBaseActions;
using Logger;

namespace testDB
{
    class Program
    {
        static void Main(string[] args)
        {
            DBConnection.Notify += Output;
            var db = new DBConnection();
            /*
            db.RecordToUser("Irina|123|");
            db.RecordToChat("чат 2|");
            var id = db.FindUserName(2);
            Console.WriteLine(id);
            var user = db.FindUserId("Irina");
            Console.WriteLine(user);
            db.RecordToContact("чат 1|Irina");
            Console.WriteLine(db.FindChatName(1));

            var chats = db.ReceiveListChats(1);
            foreach (var chat in chats) Console.Write($"{chat}  ");
            Console.WriteLine();
            var users = db.ReceiveListUsers(1);
            foreach (var us in users) Console.Write($"{us}  ");
            Console.WriteLine();*/
            
            var t = DateTime.Now;
            Console.WriteLine(t.ToUniversalTime());
            db.RecordToMessage($"text|2021-11-10|user 2|чат 1|yra");
            Console.WriteLine();

            var mes = db.FindMessageToChat("чат 1");
            foreach (var m in mes) Console.Write($"{m}  ");
            Console.WriteLine();

        }   

        static void Output(MessageType type, string message)
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
