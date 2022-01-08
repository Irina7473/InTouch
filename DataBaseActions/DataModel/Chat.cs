using System;
using System.Collections.Generic;
using System.Drawing;

namespace DataBaseActions
{
    public class Chat
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public byte[] Avatar { get; set; }
        public List<string> Users { get; set; } //формируется из table_Contacts
        //public List<Message> Messages { get; set; } //формируется из table_Messages

        public Chat(string name, byte[] avatar, List<string> users)
        {
            Name = name;
            Avatar = avatar;
            Users = users;
        }
        public Chat(int id)
        {
            Id = id;
            var db = new DBConnection();
            Name = db.FindChatName(id);
            Users = db.ReceiveListUsers(id);
        }
        public Chat(string name)
        {
            Name = name;
            var db = new DBConnection();
            Id = db.FindChatId(name);
            Users = db.ReceiveListUsers(Id);
        }
    }
}
