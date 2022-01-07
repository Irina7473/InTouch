using System;
using System.Collections.Generic;

namespace DataBaseActions
{
    public class User
    {
        public int Id { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
        public string FullName { get; set; }
        public byte Avatar { get; set; }  // Добавит сам user
        public List <Chat> Chats { get; set; } //формируется из table_Contacts

        public User() { }
        public User(int id)
        {
            Id = id;
            var db = new DBConnection();
            Chats = db.UpdateListChats(id);
        }
    }
}