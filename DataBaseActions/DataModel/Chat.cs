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
        public bool Security { get; set; }   
        public List<User> Users { get; set; } //формируется из table_Contacts
        public List<Message> Messages { get; set; } //формируется из table_Messages

        public Chat(string name, byte[] avatar, List<User> users)
        {
            Name = name;
            Avatar = avatar;
            Users = users;
        }
        public Chat(string name, byte[] avatar)
        {
            Name = name;
            Avatar = avatar;
        }
        public Chat(string name)
        {
            Name = name;
        }
    }
}
