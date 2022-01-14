using System;
using System.Collections.Generic;

namespace DataBaseActions
{
    public class DMUser
    {
        public int Id { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
        public byte[] Avatar { get; set; }
        public List <DMChat> Chats { get; set; } //формируется из table_Contacts
    }
}