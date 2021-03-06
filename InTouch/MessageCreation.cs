using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using DataBaseActions;

namespace InTouchLibrary
{
    public class MessageCreation
    {
        public MessageType Type { get; set; }
        public string Mes { get; set; }

        public MessageCreation() { }
        public MessageCreation(MessageType type, string message)
        {
            Type = type;
            Mes = message;
        }
        public MessageCreation(MessageType type)
        {
            Type = type;
        }
    }

    public class MessageIdent : MessageCreation
    {
        public string Login { get; set; }
        public string Password { get; set; }
        public MessageIdent(MessageType type, string login, string password) : base(type)  
        {
            Login = login;
            Password = password;
        }     
    }

    public class MessageSendUser : MessageCreation
    {
        public DMUser User { get; set; }
        public MessageSendUser(MessageType type, DMUser user) : base(type)
        {
            User = user;
        }
    }

    public class MessageSendContent : MessageCreation
    {
        public DMMessage Message { get; set; }
        public MessageSendContent(MessageType type, DMMessage message) : base(type)
        {
            Message = message;
        }
    }

    public enum MessageType
    {
        info,
        warn,
        error,
        ident,
        user,
        content
    }
}
