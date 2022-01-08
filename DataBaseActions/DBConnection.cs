using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using Logger;
using System.Threading.Tasks;
using System.Text;

namespace DataBaseActions
{
    public class DBConnection
    {
        public static event Action<MessageType, string> Notify;
        string connectionString = "Server=mysql60.hostland.ru;Database=host1323541_itstep31;Uid=host1323541_itstep;Pwd=269f43dc;";

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
                Notify?.Invoke(MessageType.error, "Ошибка открытия БД");
            }
            catch (MySqlException)
            {
                Notify?.Invoke(MessageType.error, "Подключаемся к уже открытой БД");
            }
            catch (Exception)
            {
                Notify?.Invoke(MessageType.error, "Путь к базе данных не найден");
            }
        }
                
        public void Close()
        {
            _connection.Close();
        }

        /*private MySqlDataReader SelectQuery(string sql)
        {
            _query.CommandText = sql;
            var result = _query.ExecuteReader();
            return result;
        }*/

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
            catch { Notify?.Invoke(MessageType.warn, "Пользователь с таким именем существует"); }
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
            catch { Notify?.Invoke(MessageType.warn, "Чат с таким именем существует"); }
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
                    if (result.HasRows) Notify?.Invoke(MessageType.warn, $"В чате {chatId} есть контакт {userId}");
                    else
                    {
                        result.Close();
                        _query.CommandText = $"INSERT INTO table_contacts (chatId, userId)" +
                            $"VALUES ({chatId}, {userId})";
                        try { _query.ExecuteNonQuery(); }
                        catch { Notify?.Invoke(MessageType.error, $"Не удалось добавить пользователя {userId} в чат {chatId}"); }
                    }
                    Close();
                }
                else Notify?.Invoke(MessageType.error, "Не найден пользователь");

            }
            else Notify?.Invoke(MessageType.error, "Не найден чат");
        }

        public void RecordToMessage(string message)
        {
            var mess = message.Split('|');
            var messageType = mess[0];
            var time = Convert.ToDateTime( mess[1]);
            var sender = mess[2];
            var senderId = FindUserId(sender);
            var chat = mess[3];
            var chatId = FindChatId(chat);
            var content = mess[4];
            //var content = Encoding.Unicode.GetBytes(mess[4]);

            Open();
            _query.CommandText = $"INSERT INTO table_messages (messageType, time, senderId, chatId, content)" +
                    $"VALUES ('{messageType}', '{time}', {senderId}, {chatId}, '{content}')";
            try { _query.ExecuteNonQuery(); }
            catch (Exception e) { Notify?.Invoke(MessageType.error, e.ToString()); }
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
                Notify?.Invoke(MessageType.warn, "Нет данных в таблице пользователей");
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

        public string FindUserName(int idUser)
        {          
            Open();
            _query.CommandText = $"SELECT login FROM table_users where id='{idUser}';";
            var result = _query.ExecuteReader();
            if (!result.HasRows)
            {
                Notify?.Invoke(MessageType.warn, "Нет данных в таблице пользователей");
                Close();
                return null;
            }
            else
            {
                result.Read();
                string nameUser = result.GetString(0);
               Close();
               if (nameUser==string.Empty) Notify?.Invoke(MessageType.warn, $"Имя пользователя {idUser} не задано");
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
                Notify?.Invoke(MessageType.warn, "Нет данных в таблице чатов");
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
                Notify?.Invoke(MessageType.warn, "Нет данных в таблице чатов");
                Close();
                return null;
            }
            else
            {
                result.Read();
                string nameChat = result.GetString(0);
                Close();
                if (nameChat == string.Empty) Notify?.Invoke(MessageType.warn, $"Имя чата {idChat} не задано");
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
                Notify?.Invoke(MessageType.warn, $"Нет данных в таблице контактов о чате {idChat}");
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
        
        public List<string> ReceiveListChats(int idUser)
        {
            Open();
            _query.CommandText = $"SELECT chatName FROM table_chats AS ch INNER JOIN table_contacts AS co ON ch.id = co.chatId WHERE userId='{idUser}';";
            var result = _query.ExecuteReader();
            if (!result.HasRows)
            {
                Notify?.Invoke(MessageType.warn, $"Нет данных в таблице контактов о пользователе {idUser}");
                Close();
                return null;
            }
            else
            {
                var chats = new List<string>();
                while (result.Read())
                {
                    var chat = result.GetString(0);
                    chats.Add(chat);
                }
                Close();
                return chats;
            }
        }

        public List<string> FindMessageToChat(string nameChat)
        {
            Open();
            _query.CommandText = $"SELECT * FROM table_messages AS m INNER JOIN table_chats AS c ON m.chatId = c.id WHERE chatName='{nameChat}';";
            var result = _query.ExecuteReader();
            if (!result.HasRows)
            {
                Notify?.Invoke(MessageType.warn, $"Нет сообщений в чате {nameChat}");
                Close();
                return null;
            }
            else
            {
                var messages = new List<string>();
                while (result.Read())
                {
                    var mes = result.GetString(1)+ result.GetDateTime(2) + result.GetString(3) + result.GetString(5);
                    messages.Add(mes);
                }
                Close();
                return messages;
            }
        }
    }
}
