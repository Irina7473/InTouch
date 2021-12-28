using System;
using System.Text;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using System.Threading.Tasks;

namespace InTouchServer
{
    public class Client
    {
        public static event Action<MessageType, string> Notify;
        public TcpClient client;
        //private string _macAddress;
        public NetworkStream _netStream;
        private User _user;

        public bool Connected { get; set; }

        /*
public Client ()
{
client = new();
_netStream = client.GetStream();
}*/

        public void ConnectToServer(IPAddress ip, int port, string login, string password)
        {
            try
            {
                client = new();
                client.Connect(ip, port);
                _netStream = client.GetStream();
                //_macAddress = GetMacAddress();
                Notify?.Invoke(MessageType.info, $"{DateTime.Now} Соединение с сервером установлено");                
                Send($"{login} {password}");
            }
            catch (Exception e)
            {
                Notify?.Invoke(MessageType.error, e.ToString());
            }            
        }
        /*
        public void Communication()
        {            
            Task taskRead = new(() => {
                while (client.Connected)
                {
                    Read();
                }
            });
            taskRead.Start();            
        }*/

        /*
        private string GetMacAddress()
        {
            string macAddress = "";
            foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (nic.OperationalStatus == OperationalStatus.Up)
                {
                    macAddress += nic.GetPhysicalAddress().ToString();
                    break;
                }
            }
            return macAddress;
        }*/

        public void Send(string message)
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
                    Notify?.Invoke(MessageType.text, $"{DateTime.Now} Получено сообщение {message}");
                    return message;
                }
                else
                {
                    Notify?.Invoke(MessageType.error, "Вы не можете читать данные из этого потока.");
                    Close();
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

        public void Close()
        {
            client.Close();
            Notify?.Invoke(MessageType.warn, $"{DateTime.Now} Соединение с сервером закрыто");
        }

    }
}