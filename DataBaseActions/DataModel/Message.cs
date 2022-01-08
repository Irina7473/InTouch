using System;
using System.Collections.Generic;

namespace DataBaseActions
{
    public class Message
    {
        public int Id { get; set; }
        public string MessageType { get; set; }
        public DateTime DateTime { get; set; }
        public int SenderId { get; set; }
        public int ChatId { get; set; }       
        public string Content { get; set; } //byte[] 
        public bool Status { get; set; }

    }        
}