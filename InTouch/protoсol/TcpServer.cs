using System;
using System.Text;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using Logger;

namespace InTouchServer
{
    public class TcpServer
    {
        public static event Action<MessageType, string> Notify;
        private int _port;
        private TcpListener _listener;
        private int _amtTouch;  //макс кол-во активных соединений
        public int numberTouch; //номер соединения

        public TcpServer(int port, int amt)
        {
            _port = port;
            _amtTouch = amt;
            numberTouch = 0;
        }

        public void StartTcpServer()
        {
            try
            {
                _listener = new TcpListener(IPAddress.Any, _port);
                _listener.Start(_amtTouch);
                Notify?.Invoke(MessageType.info, $"{DateTime.Now} Listen");
            }
            catch (Exception e)
            {                
                Notify?.Invoke(MessageType.error, $"{DateTime.Now} {e}");
            }

            while (true)
            {
                if (_listener.Pending())
                {
                    var connection = _listener.AcceptTcpClient();
                    var client=new ClientConnection();
                    numberTouch++;
                    Notify?.Invoke(MessageType.info, $"{DateTime.Now} Accept {numberTouch}");
                    Task task = new(() =>
                    {                        
                        client.ConnectToClient(connection, numberTouch);
                    });
                    task.Start();                    
                }
            }
        }
    }
}
