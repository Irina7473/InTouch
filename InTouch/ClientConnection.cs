using System;
using System.Text;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using System.Threading.Tasks;


namespace InTouchServer
{
    class ClientConnection
    {
        public static event Action<MessageType, string> Notify;
        public int numberTouch;
        public TcpClient client;
        //private NetworkStream _netStream;

        /*
        public ClientConnection(TcpClient client, int numberTouch)
        {
            this.client = client;
            this.numberTouch = numberTouch;
        }*/

        public void ConnectToClient(TcpClient connection, int number)
        {
            client = connection;
            numberTouch = number;
            try
            {
                var netStream = client.GetStream();
                // Проверка клиента или регистрация нового
                Task taskRead = new(() => { Read(netStream); });
                taskRead.Start();                
                Task taskSend = new(() => { Send(netStream); });
                taskSend.Start();
                
            }
            catch (Exception e)
            {
                Notify?.Invoke(MessageType.error, $"Для соединения {numberTouch} {e.ToString()}");
            }
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
                    Notify?.Invoke(MessageType.text, $"Переданы сообщения");
                }
                else
                {
                    Notify?.Invoke(MessageType.error, $"{DateTime.Now} Вы не можете записывать данные в этот поток.");
                    netStream.Close();
                    Close();
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
                    Notify?.Invoke(MessageType.text, $"Получено сообщение {message}");
                    //Добавить запись в базу данных
                }
                else
                {
                    Notify?.Invoke(MessageType.error, $"{DateTime.Now} Вы не можете читать данные из этого потока.");
                    netStream.Close();
                    Close();
                }
            }
            catch (Exception e)
            {
                Notify?.Invoke(MessageType.error, e.ToString());
            }
        }

        private void Close ()
        {
            client.Close();
            Notify?.Invoke(MessageType.warn, $"{DateTime.Now} Соединение с клиентом закрыто");
        }


    }
}
