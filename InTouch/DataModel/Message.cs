using System;
using System.Collections.Generic;

namespace InTouchServer
{
    public class Message
    {
        public int Id { get; set; }
        public string DateTime { get; set; } //или datetime
        public int SenderId { get; set; }
        public int ChatId { get; set; }       
        public string MessageType { get; set; }
        public string Content { get; set; } //byte[] 
        public string Status { get; set; }

    }

    public enum MessageType
    {
        info,
        warn,
        error,        
        text,
        img
    }
}