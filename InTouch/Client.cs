using System;
using System.Text;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using Logger;

namespace InTouchServer
{
    public class Client
    {
        public static event Action<MessageType, string> Notify;
        public TcpClient client;
        private NetworkStream _netStream;        

        public bool Connected { get; set; }

        public void ConnectToServer(IPAddress ip, int port, string login, string password)
        {
            try
            {
                client = new();
                client.Connect(ip, port);
                _netStream = client.GetStream();
                Notify?.Invoke(MessageType.info, $"{DateTime.Now} Соединение с сервером установлено");                
                Send($"{login} {password}");
            }
            catch (Exception e)
            {
                Notify?.Invoke(MessageType.error, $"{DateTime.Now} {e}");
            }            
        }        
        
        public void Send(string message)
        {
            if (client.Connected)
            {
                try
            {
                if (_netStream.CanWrite)
                {
                    Byte[] sendBytes = Encoding.Unicode.GetBytes(message);
                    _netStream.Write(sendBytes, 0, sendBytes.Length);
                    Notify?.Invoke(MessageType.text, $"{DateTime.Now} Передано сообщение {message}");
                }
                else
                {
                    Notify?.Invoke(MessageType.error, $"{DateTime.Now} Вы не можете записывать данные в этот поток.");
                    Close();
                    _netStream.Close();
                    return;
                }
            }
            catch (Exception e)
            {
                Notify?.Invoke(MessageType.error, $"{DateTime.Now} {e}");
            }
        }
            else
            {
                Notify?.Invoke(MessageType.warn, $"{DateTime.Now} Соединение разорвано");
            }
}
        
        public string Read()
        {
            if (client.Connected)
            {
                try
            {
                if (_netStream.CanRead)
                {
                    var buffer = new byte [256];
                    var data = new StringBuilder();
                    do
                    {
                        try
                        {
                            int bytes=_netStream.Read(buffer, 0, buffer.Length);
                            data.Append(Encoding.Unicode.GetString( buffer, 0 , bytes));
                        }
                        catch (Exception e)
                        {
                            Notify?.Invoke(MessageType.error, $"{DateTime.Now} {e}");
                            break;
                        }
                    } while (_netStream.DataAvailable);
                                        
                    Notify?.Invoke(MessageType.text, $"{DateTime.Now} Получено сообщение {data.ToString()}");
                    return data.ToString();
                }
                else
                {
                    Notify?.Invoke(MessageType.error, "Вы не можете читать данные из этого потока.");
                    Close();
                    _netStream.Close();
                    return string.Empty;
                }               
            }
            catch (Exception e)
            {
                Notify?.Invoke(MessageType.error, $"{DateTime.Now} {e}");
                return string.Empty;
            }
            }
            else
            {
                Notify?.Invoke(MessageType.warn, $"{DateTime.Now} Соединение разорвано");
                return string.Empty;
            }
        }

        public void Close()
        {            
            if (client !=null) client.Close();
            if (_netStream !=null) _netStream.Close();
            Notify?.Invoke(MessageType.warn, $"{DateTime.Now} Соединение с сервером закрыто");
        }
    }
}