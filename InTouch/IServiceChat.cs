using System;

namespace InTouchLibrary
{
    public interface IServiceChat
    {        
        void Connect(string name);
        void Disconnect(string name);
        void SendMessage (Message message);
    }
}