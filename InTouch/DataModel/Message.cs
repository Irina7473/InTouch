using System;
using System.Collections.Generic;

namespace InTouchServer
{
    public class Message
    {
        public int Id { get; set; }
        public string DateTime { get; set; }
        public int SenderId { get; set; }
        public int ChatId { get; set; }       
        public string MessageType { get; set; }
        public byte[] Content { get; set; }
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