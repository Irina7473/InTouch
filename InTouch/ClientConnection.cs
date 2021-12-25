using System;
using System.Text;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using System.Threading.Tasks;


namespace InTouchServer
{
    public class ClientConnection
    {
        public static event Action<MessageType, string> Notify;
        public int numberTouch;
        public TcpClient client;
        public User user;
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
            //Notify?.Invoke(MessageType.info, $"Для соединения {numberTouch} создан client");
            try
            {
                var netStream = client.GetStream();
                //Notify?.Invoke(MessageType.info, $"Для соединения {numberTouch} создан netStream");
                // Проверка клиента или регистрация нового
                // user= user из БД
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
                    Close();
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
