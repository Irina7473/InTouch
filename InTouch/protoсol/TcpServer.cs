using System;
using System.Text;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace InTouchServer
{
    public class TcpServer
    {
        public static event Action<MessageType, string> Notify;
        private int _port;
        private TcpListener _listener;
        private int _amtTouch;



        public TcpServer(int port, int amt) 
        { 
            _port = port;
            _amtTouch = amt;
        }

        public void StartTcpServer()
        {
            try
            {                
                _listener = new TcpListener(IPAddress.Any, _port);
                _listener.Start(_amtTouch);
                Notify?.Invoke(MessageType.info, "Listen");
            }
            catch (Exception e)
            {
                Notify?.Invoke(MessageType.error, e.ToString());
            }            

            while (true)
            {
                if (_listener.Pending())
                {
                    var tspClient = _listener.AcceptTcpClient();
                    Notify?.Invoke(MessageType.info, "Accept");

                    Task task = new(() =>
                    {

                    });
                    task.Start();
                }
            }

        }

       
    }
}

    /*
     *  public string PartyReceive(Socket connect)
        {
            if (connect.Connected)
            {
                var buffer = new byte[256];
                var data = new List<byte>();
                do
                {
                    try
                    {
                        connect.Receive(buffer);
                        data.AddRange(buffer);
                    }
                    catch (Exception exc)
                    {
                        Notify?.Invoke(exc.Message);
                        break;
                    }
                } while (connect.Available > 0);

                var t = data.ToArray();
                var message = Encoding.Unicode.GetString(t, 0, t.Length);
                if (message == "bye" || message == "Bye") PartyClose(connect);
                //Notify?.Invoke("Received");
                return message;
            }
            else
            {
                Notify?.Invoke("No connection");
                return "Отсутствует соединение";
            }
        }

        public void PartySend(Socket connect, string message)
        {
            if (connect.Connected)
            {
                connect.Send(Encoding.Unicode.GetBytes(message));
                //Notify?.Invoke("Send...");
                if (message == "bye" || message == "Bye") PartyClose(connect);
            }
            else Notify?.Invoke("No connection");
        }

        public void PartyClose(Socket connect)
        {
            Notify?.Invoke("Close");
            connect.Shutdown(SocketShutdown.Both);
            connect.Close();
        }
    */
