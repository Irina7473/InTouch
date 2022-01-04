using System;
using System.Collections.Generic;


namespace InTouchServer
{
    public class Chat
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool Security { get; set; }   
        public List<User> Users { get; set; } //формируется из table_Contacts

    }
}
