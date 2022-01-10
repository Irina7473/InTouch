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
        //public List<string> Users { get; set; } //формируется из table_Contacts
        //public List<string> Messages { get; set; } //формируется из table_Messages

        public DMChat(){ }

        public List<DMMessage> ChatMessages()
        {
            var db = new DBConnection();
            return db.FindMessageToChat(Id);
        }
    }
}
