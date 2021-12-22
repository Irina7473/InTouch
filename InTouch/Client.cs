using System;
using System.Text;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace InTouchServer
{
    public class Client
    {
        public static event Action<MessageType, string> Notify;
        public TcpClient client;
        private NetworkStream _netStream;

        public void ConnectToServer(IPAddress ip, int port)
        {
            try
            {
                client = new();
                client.Connect(ip, port);
                _netStream = client.GetStream();
                Notify?.Invoke(MessageType.info, "Соединение установлено");
            }
            catch (Exception e)
            {
                Notify?.Invoke(MessageType.error, e.ToString());
            }            
        }

        public void Send(string message)
        {
            try
            {
                if (_netStream.CanWrite)
                {
                    Byte[] sendBytes = Encoding.UTF8.GetBytes(message);
                    _netStream.Write(sendBytes, 0, sendBytes.Length);
                    Notify?.Invoke(MessageType.text, "Передано сообщение {message}");
                }
                else
                {
                    Notify?.Invoke(MessageType.error, "Вы не можете записывать данные в этот поток.");
                    client.Close();
                    _netStream.Close();
                    return;
                }
            }
            catch (Exception e)
            {
                Notify?.Invoke(MessageType.error, e.ToString());
            }
        }

        public string Read()
        {
            try
            {
                if (_netStream.CanRead)
                {
                    var buffer = new byte [client.ReceiveBufferSize];
                    var data = new List<byte>();
                    do
                    {
                        try
                        {
                            _netStream.Read(buffer, 0, (int)client.ReceiveBufferSize);
                            data.AddRange(buffer);
                        }
                        catch (Exception exc)
                        {
                            Notify?.Invoke(MessageType.error, exc.Message);
                            break;
                        }
                    } while (_netStream.DataAvailable);
                    var t = data.ToArray();
                    var message = Encoding.Unicode.GetString(t, 0, t.Length);                    
                    Notify?.Invoke(MessageType.text, "Получено сообщение {message}");
                    return message;
                }
                else
                {
                    Notify?.Invoke(MessageType.error, "Вы не можете читать данные из этого потока.");
                    client.Close();
                    _netStream.Close();
                    return "Вы не можете читать данные из этого потока.";
                }               
            }
            catch (Exception e)
            {
                Notify?.Invoke(MessageType.error, e.ToString());
                return e.ToString();
            }            
        }

    }
}