using System;
using System.Text;
using System.Text.Json;
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
           
            var t = DateTime.Now.ToString();
            Console.WriteLine(t);
            //db.RecordToMessage($"text|2021-05-08 10:02:45|user 2|чат 1|yra");
            var dmmes = new DMMessage();
            dmmes.MessageType = "text";
            dmmes.DateTime = DateTime.Now;
            dmmes.SenderId = 1;
            dmmes.ChatId = 1;
            dmmes.Content = "как дела?";
            db.RecordToMessage(dmmes);
            Console.WriteLine();

            var mes = db.FindMessageToChat(1);
            foreach (var m in mes) Console.Write($"{m}  ");
            Console.WriteLine();

            /*
            var users = db.ReceiveListChats(1);
            foreach (var u in users) Console.Write($"{u}  ");
            Console.WriteLine();
            var user = db.FindUser("Сергей", "123");
            Console.WriteLine(JsonSerializer.Serialize<DMUser>(user));*/
        }   

        static void Output(LogType type, string message)
        {
            switch (type)
            {
                case LogType.info:
                    Console.ForegroundColor = ConsoleColor.Green;
                    break;
                case LogType.warn:
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                    break;
                case LogType.error:
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
                case LogType.text:
                    Console.ForegroundColor = ConsoleColor.White;
                    break;
            }
            Console.WriteLine($"{type} {message}");
            Console.ResetColor();
        }
    }
}
