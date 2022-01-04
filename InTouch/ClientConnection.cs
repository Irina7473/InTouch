using System;
using System.Text;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using System.Threading;
using Logger;

namespace InTouchServer
{
    public class ClientConnection
    {
        public static event Action<MessageType, string> Notify;
        public int numberTouch;
        public TcpClient client;
        public User user;


        public void ConnectToClient(TcpClient connection, int number)
        {
            client = connection;
            numberTouch = number;
            try
            {
                var netStream = client.GetStream();
                var ident = Identification(netStream, numberTouch);
                if (ident)
                {
                    Communication(netStream);
                }
                else Close();
            }
            catch (Exception e)
            {
                Notify?.Invoke(MessageType.error, $"Для соединения {numberTouch} {e.ToString()}");
            }
        }

        public bool Identification(NetworkStream netStream, int numberTouch)
        {

            var message = Read(netStream);
            //распарсить 1 сообщение клиента
            // Проверка клиента или регистрация нового, по логину-паролю
            var found = true;
            if (found) //написать условие
            {
                // user= user из БД
                user = new();
                Send(netStream, "admit");
                return true;
            }
            else
            {
                // если нет такого
                Send(netStream, "prevent");
                return false;
            }
        }

        public void Communication(NetworkStream netStream)
        {
            // Передать user его чаты
            // Если есть сообщения, то передать
            // Запуск чтения и передачи
            
            Task taskSend = new(() => { 
                Send(netStream, "Hello");
                //for (int i = 0; i < 10; i++) { Send(netStream, "123456789"); Thread.Sleep(1000); }
                Notify?.Invoke(MessageType.text, $"{DateTime.Now} Для {numberTouch} переданы сообщения");
            });
            
            Task taskRead = new(() => {
                while (client.Connected)
                {
                    Read(netStream);
                    //Добавить запись в базу данных
                    // Добавить чтение из базы данных
                    Send(netStream, "Доставлено");
                } });
            taskRead.Start();
            //taskSend.Start();
        }        

        public void Send(NetworkStream netStream, string message)
        {
            if (client.Connected)
            {
                try
                {
                    if (netStream.CanWrite)
                    {
                        Byte[] sendBytes = Encoding.Unicode.GetBytes(message);
                        netStream.Write(sendBytes, 0, sendBytes.Length);
                        //Notify?.Invoke(MessageType.text, $"{DateTime.Now} Для {numberTouch} передано {message}");
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
            else
            {
                Notify?.Invoke(MessageType.info, $"{DateTime.Now} Клиент {numberTouch} разорвал соединение");
                
            }
        }

        public string Read(NetworkStream netStream)
        {
            if (client.Connected)
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
                                netStream.Read(buffer, 0, buffer.Length);
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
                        return message;
                    }
                    else
                    {
                        Notify?.Invoke(MessageType.error, $"{DateTime.Now} Вы не можете читать данные из этого потока для {numberTouch}");
                        netStream.Close();
                        Close();
                        return string.Empty;
                    }
                }
                catch (Exception e)
                {
                    Notify?.Invoke(MessageType.error, e.ToString());
                    return string.Empty;
                }
            }
            else
            {
                Notify?.Invoke(MessageType.info, $"{DateTime.Now} Клиент {numberTouch} разорвал соединение");
                return string.Empty;
            }
            }

        private void Close ()
        {
            client.Close();
            Notify?.Invoke(MessageType.warn, $"{DateTime.Now} Соединение с клиентом закрыто");
        }


    }
}
