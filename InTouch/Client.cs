using System;
using System.Text;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using Logger;
using DataBaseActions;
using System.Text.Json;

namespace InTouchLibrary
{
    /// <summary>
    /// Класс клиента для взаимодействия с TCP-сервером
    /// </summary>
    public class Client
    {
        /// <summary>
        /// Событие делегата Action, принимающее тип события из перечисления LogType и его содержание типа string
        /// </summary>
        public static event Action<LogType, string> Notify;
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
        /// Соединение с сервером и передача логина и пароля для авторизации
        /// </summary>
        /// <param name="ip">IPAddress</param>
        /// <param name="port">Номер порта для соединения</param>
        /// <param name="login">Логин пользователя</param>
        /// <param name="password">Пароль пользователя</param>
        public void ConnectToServer(IPAddress ip, int port, string login, string password)
        {
            try
            {
                client = new();
                client.Connect(ip, port);
                _netStream = client.GetStream();
                Notify?.Invoke(LogType.info, $"{DateTime.Now} Соединение с сервером установлено");
                //передаю серверу логин-пароль для авторизации
                string message = JsonSerializer.Serialize<MessageIdent>(new MessageIdent(MessageType.ident, login, password));
                Send(message);
            }
            catch (Exception e) { Notify?.Invoke(LogType.error, $"{DateTime.Now} {e}"); }            
        }

        /// <summary>
        /// Получение от сервера пользователя и инициализация поля user
        /// </summary>
        /// <returns>оъбект класса DMUser</returns>
        public DMUser ReceiveUser()
        {
            // получаю user с сервера
            var message = Read();
            var mesCreat = JsonSerializer.Deserialize<MessageSendUser>(message); 
            if (mesCreat.Type == MessageType.user) user = mesCreat.User;
            return user;
        }

        /// <summary>
        /// Отправка сообщения
        /// </summary>
        /// <param name="message">Текстовое сообщение</param>
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
                        Notify?.Invoke(LogType.text, $"{DateTime.Now} Передано сообщение {message}");
                    }
                    else
                    {
                        Notify?.Invoke(LogType.error, $"{DateTime.Now} Вы не можете записывать данные в этот поток.");
                        Close();
                        _netStream.Close();
                        return;
                    }
                }
                catch (Exception e) { Notify?.Invoke(LogType.error, $"{DateTime.Now} {e}"); }
            }
            else { Notify?.Invoke(LogType.warn, $"{DateTime.Now} Соединение разорвано"); }
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

                        Notify?.Invoke(LogType.text, $"{DateTime.Now} Получено сообщение {data.ToString()}");
                        return data.ToString();
                    }
                    else
                    {
                        Notify?.Invoke(LogType.error, "Вы не можете читать данные из этого потока.");
                        Close();
                        _netStream.Close();
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
                Notify?.Invoke(LogType.warn, $"{DateTime.Now} Соединение разорвано");
                return string.Empty;
            }
        }

        /// <summary>
        /// Закрытие клиентом соединения и программы
        /// </summary>
        public void Close()
        {
            //сообщаю серверу о закрытии соединения
            var message = JsonSerializer.Serialize<MessageInfo>(new MessageInfo (MessageType.leave, "Закрываю соединение"));
            Send(message);
            //закрываю соединение
            if (client !=null) client.Close();
            if (_netStream !=null) _netStream.Close();
            Notify?.Invoke(LogType.warn, $"{DateTime.Now} Соединение с сервером закрыто");
        }
    }
}