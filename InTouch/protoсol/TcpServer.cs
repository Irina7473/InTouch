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
        public int numberTouch;

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
                Notify?.Invoke(MessageType.error, e.ToString());
            }

            while (true)
            {
                if (_listener.Pending())
                {
                    var connection = _listener.AcceptTcpClient();
                    //var client=new ClientConnection();
                    numberTouch++;
                    Notify?.Invoke(MessageType.info, $"{DateTime.Now} Accept {numberTouch}");
                    Task task = new(() =>
                    {
                        ConnectToClient(connection, numberTouch);
                        //client.ConnectToClient(connection, numberTouch);
                    });
                    task.Start();
                    
                }
            }
        }

        
        private void ConnectToClient (TcpClient connection, int numberTouch)
        {
            try
            {
                // Проверка клиента или регистрация нового
                var netStream = connection.GetStream();
                
                Task taskRead = new(() =>
                {
                    do
                    {
                        Read(netStream, numberTouch);
                        // Если есть сообщения, то передать
                    }
                    while (connection.Connected);
                });
                taskRead.Start();
                Task taskSend = new(() => { Send(netStream, numberTouch); });
                taskSend.Start();                
            }
            catch (Exception e)
            {
                Notify?.Invoke(MessageType.error, $"Для соединения {numberTouch} {e.ToString()}");
            }
        }

        public void Send(NetworkStream netStream, int numberTouch)
        {
            try
            {
                if (netStream.CanWrite)
                {
                    // Добавить чтение из базы данных
                    Byte[] sendBytes = Encoding.Unicode.GetBytes("Получите новые сообщения");
                    netStream.Write(sendBytes, 0, sendBytes.Length);
                    Notify?.Invoke(MessageType.text, $"{DateTime.Now} Переданы сообщения для {numberTouch}");
                }
                else
                {
                    Notify?.Invoke(MessageType.error, $"{DateTime.Now} Вы не можете записывать данные в этот поток для {numberTouch}");
                    netStream.Close();
                    return;
                }
            }
            catch (Exception e)
            {
                Notify?.Invoke(MessageType.error, e.ToString());
            }            
        }

        public void Read(NetworkStream netStream, int numberTouch)
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
                    Notify?.Invoke(MessageType.text, $"{DateTime.Now} от {numberTouch} получено сообщение {message} ");
                    //Добавить запись в базу данных
                }
                else
                {
                    Notify?.Invoke(MessageType.error, $"{DateTime.Now} Вы не можете читать данные из этого потока для {numberTouch}");
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
