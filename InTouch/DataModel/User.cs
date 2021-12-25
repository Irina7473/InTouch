using System;
using System.Collections.Generic;


namespace InTouchServer
{
    public class User
    {
        public int Id { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
        public string FullName { get; set; }
        public byte Avatar { get; set; }
        public List <Chat> Chats { get; set; }
    }
}