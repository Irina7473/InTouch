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
                    var client = _listener.AcceptTcpClient();
                    Notify?.Invoke(MessageType.info, "Accept");

                    Task task = new(() =>
                    {
                        ConnectToClient(client);
                    });
                    task.Start();
                }
            }
        }

        private void ConnectToClient (TcpClient client)
        {
            // Проверка клиента или регистрация нового
            var netStream = client.GetStream();
            Task taskSend = new(() => { Send(netStream);});
            taskSend.Start();
            Task taskRead = new(() => { Read(netStream); });
            taskRead.Start();
        }

        public void Send(NetworkStream netStream)
        {
            try
            {
                if (netStream.CanWrite)
                {
                    // Добавить чтение из базы данных
                    Byte[] sendBytes = Encoding.Unicode.GetBytes("Получите новые сообщения");
                    netStream.Write(sendBytes, 0, sendBytes.Length);
                    Notify?.Invoke(MessageType.text, "Переданы сообщения");
                }
                else
                {
                    Notify?.Invoke(MessageType.error, "Вы не можете записывать данные в этот поток.");
                    netStream.Close();
                    return;
                }
            }
            catch (Exception e)
            {
                Notify?.Invoke(MessageType.error, e.ToString());
            }
        }

        public void Read(NetworkStream netStream)
        {
            try
            {
                if (netStream.CanRead)
                {
                    var buffer = new byte[1024];
                    var data = new List<byte>();
                    do
                    {
                        try
                        {
                            netStream.Read(buffer, 0, 1024);
                            data.AddRange(buffer);
                        }
                        catch (Exception exc)
                        {
                            Notify?.Invoke(MessageType.error, exc.Message);
                            break;
                        }
                    } while (netStream.DataAvailable);
                    var t = data.ToArray();
                    var message = Encoding.Unicode.GetString(t, 0, t.Length);
                    Notify?.Invoke(MessageType.text, "Получено сообщение {message}");
                    //Добавить запись в базу данных
                }
                else
                {
                    Notify?.Invoke(MessageType.error, "Вы не можете читать данные из этого потока.");
                    netStream.Close();
                }
            }
            catch (Exception e)
            {
                Notify?.Invoke(MessageType.error, e.ToString());
            }            
        }
    }
}
