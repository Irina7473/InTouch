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
    /// <summary>
    /// Класс взаимодействия TCP-сервера с клиентом
    /// </summary>
    public class ClientConnection
    {
        /// <summary>
        /// Событие делегата Action, принимающее тип события из перечисления LogType и его содержание типа string
        /// </summary>
        public static event Action<LogType, string> Notify;
        /// <summary>
        /// Номер соединения сервера с клиентом
        /// </summary>
        public int numberTouch;
        /// <summary>
        /// Объект класса TcpClient
        /// </summary>
        public TcpClient client;
        private NetworkStream _netStream;
        /// <summary>
        /// Объект класса DMUser
        /// </summary>
        public DMUser user;

        /// <summary>
        /// Обработка результата авторизации клиента и сообщение ем результата
        /// </summary>
        /// <param name="connection">Объект класса TcpClient</param>
        /// <param name="number">Номер соединения сервера с клиентом</param>
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
                    //сообщаю клиенту об авторизации
                    var message = JsonSerializer.Serialize<MessageInfo>(new MessageInfo(MessageType.recd, $"{user.Login} авторизован"));
                    Send(message);
                    Notify?.Invoke(LogType.info, $"{DateTime.Now} Для соединения {numberTouch} успешный вход пользователя {user.Login}");
                    Communication();
                }
                else
                {
                    //сообщаю клиенту об ошибке авторизации
                    var message = JsonSerializer.Serialize<MessageInfo>(new MessageInfo(MessageType.error, "Неверный логин или пароль"));
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

        /// <summary>
        /// Авторизация клиента
        /// </summary>
        /// <param name="numberTouch">Номер соединения сервера с клиентом</param>
        /// <returns>Логическая переменная типа bool</returns>
        public bool Identification(int numberTouch)
        {
            var message = Read();
            var mesCreat = JsonSerializer.Deserialize<MessageIdent>(message);
            if ( mesCreat.Type != MessageType.ident) return false;
            else
            {
                var db = new DBConnection();
                user = db.FindUser(mesCreat.Login, mesCreat.Password); //создаю на сервере user со списком его чатов
                if (user != null) return true;
                else return false;
            }
        }

        /// <summary>
        /// Первая передача клиенту данных из базы данных и запуск передачи  прослушивания сообщений
        /// </summary>
        public void Communication() 
        {
            // Формирую для user список собщений для каждого его чата 
            for (var i = 0; i < user.Chats.Count; i++)
            {
                var db = new DBConnection();
                user.Chats[i].Messages = db.FindMessageToChat(user.Chats[i].Id);
            }
            // Передаю клиенту user со списком сообщений для каждого его чата
            var message = JsonSerializer.Serialize<MessageSendUser>(new MessageSendUser(MessageType.user, user));
            Notify?.Invoke(LogType.info, message);
            Send(message);
            // Получаю от клиента подтверждение получения чатов и сообщений
            message = Read();
            var mesCreat = JsonSerializer.Deserialize<MessageCreation>(message);
            if (mesCreat.Type == MessageType.recd)
            {
                //  Отправка новых сообщений 
                Task taskSend = new(() => { Sender(); });
                taskSend.Start();
                // Запуск чтения 
                Task taskRead = new(() => { Reader(); });
                taskRead.Start();
            }
        }  
        
        /// <summary>
        /// Передача сообщений клиенту по мере их появения в базе данных
        /// </summary>
        void Sender ()
        {
            while (client.Connected)
            {
                int count = 0; //число отправленных сообщений
                // Формирую для user список собщений для каждого его чата 
                for (var i = 0; i < user.Chats.Count; i++)
                {
                    var db = new DBConnection();
                    var messages = db.FindMessageToChat(user.Chats[i].Id);
                    if (messages != null && messages.Count != 0)
                    {
                        foreach (var mes in messages)
                        {
                            bool available = true; //отсутствие такого сообщения у user
                            if (user.Chats[i].Messages != null && user.Chats[i].Messages.Count != 0)
                            {
                                foreach (var meschat in user.Chats[i].Messages)
                                    if (mes.Id == meschat.Id)
                                    {
                                        available = false;
                                        break;
                                    }
                            }
                            if (available)
                            {
                                //передаю клиенту и добавляю user новые сообщения
                                var mesSend = JsonSerializer.Serialize(new MessageSendContent(MessageType.content, mes));
                                Send(mesSend);
                                user.Chats[i].Messages.Add(mes);
                                count++;
                            }
                        }
                    }
                }
                if (count > 0) Notify?.Invoke(LogType.text, $"{DateTime.Now} Для {numberTouch} передано {count} сообщений");
            }
        }
        
        /// <summary>
        /// Чтение сообщений от клиента по мере их поступления
        /// </summary>
        void Reader()
        {
            while (client.Connected)
            {
                //получаю сообщение клиента и распознаю его тип
                var message = Read();
                var mesCreat = JsonSerializer.Deserialize<MessageCreation>(message);
                if (mesCreat.Type == MessageType.leave) Close();
                else
                {
                    if (mesCreat.Type == MessageType.user) //Добавляю user в БД
                    {
                        try
                        {
                            var mesSend = JsonSerializer.Deserialize<MessageSendUser>(message);
                            var db = new DBConnection();
                            db.RecordToUser(mesSend.User);
                        }
                        catch (Exception e) { Notify?.Invoke(LogType.error, $"{DateTime.Now} {e}"); }
                    }
                    if (mesCreat.Type == MessageType.chat) //Добавляю чат в БД
                    {
                        try
                        {
                            var mesSend = JsonSerializer.Deserialize<MessageSendChat>(message);
                            var db = new DBConnection();
                            db.RecordToChat(mesSend.Chat);
                        }
                        catch (Exception e) { Notify?.Invoke(LogType.error, $"{DateTime.Now} {e}"); }
                    }
                    if (mesCreat.Type == MessageType.content) //Добавляю сообщение в БД
                    {
                        try
                        {
                            var mesSend = JsonSerializer.Deserialize<MessageSendContent>(message);
                            var db = new DBConnection();
                            db.RecordToMessage(mesSend.Message);
                        }
                        catch (Exception e) { Notify?.Invoke(LogType.error, $"{DateTime.Now} {e}"); }
                    }
                }
            }
        }

        /// <summary>
        /// Отправка сообщения
        /// </summary>
        /// <param name="message">Текстовое сообщение</param
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
                catch (Exception e) { Notify?.Invoke(LogType.error, $"{DateTime.Now} {e}"); }
            }
            else  { Notify?.Invoke(LogType.warn, $"{DateTime.Now} Клиент {numberTouch} разорвал соединение"); }
        }

        /// <summary>
        /// Чтение сообщения
        /// </summary>
        /// <returns>Текстовое сообщение</returns>
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

        /// <summary>
        /// Закрытие соединения с клиентом
        /// </summary>
        private void Close ()
        {
            if (client != null) client.Close();
            if (_netStream != null) _netStream.Close();
            Notify?.Invoke(LogType.warn, $"{DateTime.Now} Соединение с клиентом закрыто");
        }
    }
}
