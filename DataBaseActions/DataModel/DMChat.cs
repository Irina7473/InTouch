using System;
using System.Collections.Generic;
using System.Drawing;

namespace DataBaseActions
{
    public class DMChat
    {
        public int Id { get; set; }
        public string ChatName { get; set; }
        public byte[] Avatar { get; set; }
        //public List<string> Users { get; set; } //формируется из table_Contacts пока не использую
        public List<DMMessage> Messages { get; set; } //формируется из table_Messages
    }
}
