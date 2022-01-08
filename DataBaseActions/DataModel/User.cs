using System;
using System.Collections.Generic;

namespace DataBaseActions
{
    public class User
    {
        public int Id { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
        public byte[] Avatar { get; set; }  // Добавит сам user
        public List <string> Chats { get; set; } //формируется из table_Contacts

        public User() { }

        public User(int id)
        {
            Id = id;
            var db = new DBConnection();
            Chats = db.ReceiveListChats(id);
        }

        public User(string login, string password)
        {
            Login = login;
            Password = password;
        }
    }
}