using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using Logger;
using System.Threading.Tasks;
using System.Text;
using System.Text.Json;
using System.Data;

namespace DataBaseActions
{
    public class DBConnection
    {
        public static event Action<LogType, string> Notify;
        string connectionString = "Server=mysql60.hostland.ru;Database=host1323541_itstep31;Uid=host1323541_itstep;Pwd=269f43dc;convert zero datetime=True;";

        private MySqlConnection _connection;
        private MySqlCommand _query;

        public DBConnection()
        {
            _connection = new MySqlConnection(connectionString);
            _query = new MySqlCommand
            {
                Connection = _connection
            };
        }

        public void Open()
        {
            try
            {
                _connection.Open();
            }
            catch (InvalidOperationException)
            {
                Notify?.Invoke(LogType.error, "Ошибка открытия БД");
            }
            catch (MySqlException)
            {
                Notify?.Invoke(LogType.error, "Подключаемся к уже открытой БД");
            }
            catch (Exception)
            {
                Notify?.Invoke(LogType.error, "Путь к базе данных не найден");
            }
        }
                
        public void Close()
        {
            _connection.Close();
        }

        public void RecordToUser(string message)
        {            
            var user = message.Split('|');
            var login= user[0];
            var password = user[1];
            var avatar = user[2];

            Open();
            _query.CommandText = $"INSERT INTO table_users (login, password, avatar)" +
                    $"VALUES ('{login}', '{password}', '{avatar}')";
            try { _query.ExecuteNonQuery(); }
            catch { Notify?.Invoke(LogType.warn, "Пользователь с таким именем существует"); }
            Close();
        }

        public void RecordToChat(string message)
        {
            var chat = message.Split('|');
            var chatName = chat[0];
            var avatar = chat[1];  //как его сделать bool?
           Open();
            _query.CommandText = $"INSERT INTO table_chats (chatName, avatar)" +
                    $"VALUES ('{chatName}', '{avatar}')";
            try { _query.ExecuteNonQuery(); }
            catch { Notify?.Invoke(LogType.warn, "Чат с таким именем существует"); }
            Close();
        }

        public void RecordToContact(string message)
        {
            var contact = message.Split('|');
            var chat = contact[0];
            var chatId = FindChatId(chat);
            var user = contact[1];
            var userId = FindUserId(user);
            if (chatId != -1)
            {
                if (userId != -1)
                {
                    Open();
                    _query.CommandText = $"SELECT chatId, userId FROM table_contacts WHERE chatId='{chatId}' AND userId='{userId}';";
                    var result = _query.ExecuteReader();
                    if (result.HasRows) Notify?.Invoke(LogType.warn, $"В чате {chatId} есть контакт {userId}");
                    else
                    {
                        result.Close();
                        _query.CommandText = $"INSERT INTO table_contacts (chatId, userId)" +
                            $"VALUES ({chatId}, {userId})";
                        try { _query.ExecuteNonQuery(); }
                        catch { Notify?.Invoke(LogType.error, $"Не удалось добавить пользователя {userId} в чат {chatId}"); }
                    }
                    Close();
                }
                else Notify?.Invoke(LogType.error, "Не найден пользователь");

            }
            else Notify?.Invoke(LogType.error, "Не найден чат");
        }

        public void RecordToMessage(string message)
        {
            var mess = message.Split('|');
            var messageType = mess[0];
            var time = (mess[1]);
            var sender = mess[2];
            var senderId = FindUserId(sender);
            var chat = mess[3];
            var chatId = FindChatId(chat);
            var content = mess[4];
            //var content = Encoding.Unicode.GetBytes(mess[4]);

            Open();
            _query.CommandText = $"INSERT INTO table_messages (messageType, time, senderId, chatId, content)" +
                    $"VALUES ('{messageType}', STR_TO_DATE('{time}', '%m/%d/%Y %h:%i:%s %p'), {senderId}, {chatId}, '{content}')";
            try { _query.ExecuteNonQuery(); }
            catch (Exception e) { Notify?.Invoke(LogType.error, e.ToString()); }
            Close();
        }


        //Запросы к БД
        public int FindUserId (string nameUser)
        {
            Open();
            _query.CommandText = $"SELECT id FROM table_users where login='{nameUser}';";
            var result = _query.ExecuteReader();
            if (!result.HasRows)
            {
                Notify?.Invoke(LogType.warn, "Нет данных в таблице пользователей");
                Close();
                return -1;
            }
            else
            {
                result.Read();
                int idUser = result.GetInt32(0);
                Close();
                return idUser;
            }
        }

        public DMUser FindUser(string login, string password)
        {
            Open();
            _query.CommandText = $"SELECT *FROM table_users where login='{login}' AND password='{password}';";
            var result = _query.ExecuteReader();
            if (!result.HasRows)
            {
                Notify?.Invoke(LogType.warn, "Нет данных в таблице пользователей");
                Close();
                return null;
            }
            else
            {
                var user = new DMUser();
                while (result.Read())
                {
                    user.Id = result.GetInt32(0);
                    user.Login = result.GetString(1);
                    user.Password = result.GetString(2);
                    if (!result.IsDBNull(3)) user.Avatar = (byte[])result.GetValue(3);
                }
                Close();
                user.Chats = ReceiveListChats(user.Id);
                return user;
            }
        }
        public string FindUserName(int idUser)
        {          
            Open();
            _query.CommandText = $"SELECT login FROM table_users where id='{idUser}';";
            var result = _query.ExecuteReader();
            if (!result.HasRows)
            {
                Notify?.Invoke(LogType.warn, "Нет данных в таблице пользователей");
                Close();
                return null;
            }
            else
            {
                result.Read();
                string nameUser = result.GetString(0);
               Close();
               if (nameUser==string.Empty) Notify?.Invoke(LogType.warn, $"Имя пользователя {idUser} не задано");
               return nameUser;
            }
        }

        public int FindChatId(string nameChat)
        {
            Open();
            _query.CommandText = $"SELECT id FROM table_chats where chatName='{nameChat}';";
            var result = _query.ExecuteReader();
            if (!result.HasRows)
            {
                Notify?.Invoke(LogType.warn, "Нет данных в таблице чатов");
                Close();
                return -1;
            }
            else
            {
                result.Read();
                int idChat = result.GetInt32(0);
                Close();
                return idChat;
            }
        }

        public string FindChatName(int idChat)
        {
            //if (_connection.State.ToString()!="Open") 
            Open();
            _query.CommandText = $"SELECT chatName FROM table_chats where id='{idChat}';";
            var result = _query.ExecuteReader();
            if (!result.HasRows)
            {
                Notify?.Invoke(LogType.warn, "Нет данных в таблице чатов");
                Close();
                return null;
            }
            else
            {
                result.Read();
                string nameChat = result.GetString(0);
                Close();
                if (nameChat == string.Empty) Notify?.Invoke(LogType.warn, $"Имя чата {idChat} не задано");
                return nameChat;
            }
        }

        
        public List<string> ReceiveListUsers(int idChat)
        {
            Open();
            _query.CommandText = $"SELECT login FROM table_users AS u INNER JOIN table_contacts AS c ON u.id=c.userId WHERE chatId='{idChat}';";
            var result = _query.ExecuteReader();
            if (!result.HasRows)
            {
                Notify?.Invoke(LogType.warn, $"Нет данных в таблице контактов о чате {idChat}");
                Close();
                return null;
            }
            else 
            {
                var users = new List<string>();
                while (result.Read())
                {
                    var user = result.GetString(0);
                    users.Add(user);
                }
                Close();
                return users;
            }
        }
        
        public List<DMChat> ReceiveListChats(int idUser)
        {
            Open();
            _query.CommandText = $"SELECT *FROM table_chats AS ch INNER JOIN table_contacts AS co ON ch.id = co.chatId WHERE userId='{idUser}';";
            var result = _query.ExecuteReader();
            if (!result.HasRows)
            {
                Notify?.Invoke(LogType.warn, $"Нет данных в таблице контактов о пользователе {idUser}");
                Close();
                return null;
            }
            else
            {
                var chats = new List<DMChat>();
                while (result.Read())
                {
                    var chat = new DMChat();
                    chat.Id = result.GetInt32(0);
                    chat.ChatName = result.GetString(1);
                    if (!result.IsDBNull(2)) chat.Avatar = ((byte[])result.GetValue(2));
                    chats.Add(chat);
                }
                Close();
                return chats;
            }
        }


        public List<DMMessage> FindMessageToChat(int idChat)
        {
            var messages = new List<DMMessage>();
            Open();
            _query.CommandText = $"SELECT * FROM table_messages WHERE chatId='{idChat}';";    // AS m INNER JOIN table_chats AS c ON m.chatId = c.id WHERE chatName='{nameChat}';";
            var result = _query.ExecuteReader();
            if (!result.HasRows)
            {
                Notify?.Invoke(LogType.warn, $"Нет сообщений в чате {idChat}");
                Close();
                return messages;
            }
            else
            {
                //var messages = new List<DMMessage>();
                while (result.Read())
                {
                    var mes= new DMMessage();
                    mes.Id= result.GetInt32(0);
                    mes.MessageType = result.GetString(1);
                    mes.DateTime = result.GetDateTime(2);
                    mes.SenderId = result.GetInt32(3); 
                    mes.ChatId= result.GetInt32(4);
                    mes.Content = result.GetString(5);
                    mes.Status = result.GetBoolean(6);
                    messages.Add(mes);
                }
                Close();
                return messages;
            }
        }
    }
}
