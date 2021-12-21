using System;

namespace InTouchServer
{
    public interface IServiceChat
    {        
        void Connect(string name);
        void Disconnect(string name);
        void SendMessage (Message message);
        void ReceivingMessages(Message message);
    }
}