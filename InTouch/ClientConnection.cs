using System;
using System.Text;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using System.Threading;
using Logger;
using DataBaseActions;
using System.Text.Json;

namespace InTouchLibrary
{
    public class ClientConnection
    {
        public static event Action<LogType, string> Notify;
        public int numberTouch;
        public TcpClient client;
        private NetworkStream _netStream;
        public DMUser user;

        public void ConnectToClient(TcpClient connection, int number)
        {
            client = connection;
            numberTouch = number;
            try
            {
                _netStream = client.GetStream();
                var ident = Identification(numberTouch);
                if (ident)
                {
                    var message = JsonSerializer.Serialize<MessageCreation>(new MessageCreation(MessageType.ident, $"{user.Login} авторизован"));
                    Send(message);
                    Notify?.Invoke(LogType.info, $"{DateTime.Now} Для соединения {numberTouch} успешный вход");
                    Communication();
                }
                else
                {
                    var message = JsonSerializer.Serialize<MessageCreation>(new MessageCreation(MessageType.error, "Неверный логин или пароль"));
                    Send(message);
                    Close();
                    Notify?.Invoke(LogType.warn, $"{DateTime.Now} Для соединения {numberTouch} неверный логин или пароль");
                }
            }
            catch (Exception e)
            {
                Notify?.Invoke(LogType.error, $"{DateTime.Now} Для соединения {numberTouch} {e.ToString()}");
            }
        }

        public bool Identification(int numberTouch)
        // Проверка клиента по логину-паролю
        {
            var message = Read();
            var mesCreat = JsonSerializer.Deserialize<MessageIdent>(message);
            if ( mesCreat.Type != MessageType.ident) return false;
            else
            {
                var db = new DBConnection();
                user = db.FindUser(mesCreat.Login, mesCreat.Password);
                if (user!=null) return true;
                else return false;
            }
        }

        public void Communication()
        {
            // Передаю user и его чаты
            var message = JsonSerializer.Serialize<MessageSendUser>(new MessageSendUser(MessageType.user, user));
            Notify?.Invoke(LogType.info, message);
            Send(message);

            /*  Проверка новых сообщений НУЖНА
            Task taskSend = new(() => { 
                Send("Hello");                
                Notify?.Invoke(LogType.text, $"{DateTime.Now} Для {numberTouch} переданы сообщения");
            });
            taskSend.Start();*/

            // Запуск чтения 
            Task taskRead = new(() => {
                while (client.Connected)
                {
                    var message = Read();
                    //Добавить запись в базу данных
                    var mesCreat = JsonSerializer.Deserialize<MessageSendContent>(message);
                    if (mesCreat.Type == MessageType.content)
                    {
                        var db = new DBConnection();
                        db.RecordToMessage(mesCreat.Message);
                    }
                    

                } });
            taskRead.Start();
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
                        //Notify?.Invoke(MessageType.text, $"{DateTime.Now} Для {numberTouch} передано {message}");
                    }
                    else
                    {
                        Notify?.Invoke(LogType.error, $"{DateTime.Now} Вы не можете записывать данные в этот поток для {numberTouch}");
                        _netStream.Close();
                        Close();
                        return;
                    }
                }
                catch (Exception e)
                {
                    Notify?.Invoke(LogType.error, $"{DateTime.Now} {e}");
                }
            }
            else
            {
                Notify?.Invoke(LogType.warn, $"{DateTime.Now} Клиент {numberTouch} разорвал соединение");
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
                        var buffer = new byte[256];
                        var data = new StringBuilder();
                        do
                        {
                            try
                            {
                                int bytes = _netStream.Read(buffer, 0, buffer.Length);
                                data.Append(Encoding.Unicode.GetString(buffer, 0, bytes));
                            }
                            catch (Exception e)
                            {
                                Notify?.Invoke(LogType.error, $"{DateTime.Now} {e}");
                                break;
                            }
                        } while (_netStream.DataAvailable);
                        
                        Notify?.Invoke(LogType.text, $"{DateTime.Now} от {numberTouch} получено сообщение {data.ToString()} ");
                        return data.ToString();
                    }
                    else
                    {
                        Notify?.Invoke(LogType.error, $"{DateTime.Now} Вы не можете читать данные из этого потока для {numberTouch}");
                        _netStream.Close();
                        Close();
                        return string.Empty;
                    }
                }
                catch (Exception e)
                {                    
                    Notify?.Invoke(LogType.error, $"{DateTime.Now} {e}");
                    return string.Empty;
                }
            }
            else
            {
                Notify?.Invoke(LogType.warn, $"{DateTime.Now} Клиент {numberTouch} разорвал соединение");
                return string.Empty;
            }
        }

        private void Close ()
        {
            if (client != null) client.Close();
            if (_netStream != null) _netStream.Close();
            Notify?.Invoke(LogType.warn, $"{DateTime.Now} Соединение с клиентом закрыто");
        }
    }
}
