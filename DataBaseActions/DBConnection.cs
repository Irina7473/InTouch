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

        // Запись в БД
        public void RecordToUser(DMUser User)
        {
            var loginParam = new MySqlParameter("@login", User.Login);
            _query.Parameters.Add(loginParam);
            var passwordParam = new MySqlParameter("@password", User.Password);
            _query.Parameters.Add(passwordParam);
            var avatarParam = new MySqlParameter("@avatar", User.Avatar);
            _query.Parameters.Add(avatarParam);

            Open();
            _query.CommandText = $"INSERT INTO table_users (login, password, avatar)" +
                    $"VALUES (@login, @password, @avatar)";
            try { _query.ExecuteNonQuery(); }
            catch { Notify?.Invoke(LogType.warn, "Пользователь с таким именем существует"); }
            Close();
        }
        public void RecordToChat(DMChat chat)
        {
            var nameParam = new MySqlParameter("@name", chat.ChatName);
            _query.Parameters.Add(nameParam);
            var avatarParam = new MySqlParameter("@avatar", chat.Avatar);
            _query.Parameters.Add(avatarParam);
            Open();
            _query.CommandText = $"INSERT INTO table_chats (chatName, avatar)" +
                    $"VALUES (@name, @avatar)";
            try { _query.ExecuteNonQuery(); }
            catch { Notify?.Invoke(LogType.warn, "Чат с таким именем существует"); }
            Close();
        }
        public void RecordToContact(DMContact contact)
        {
            if (!FindChat(contact.ChatId))
            {
                Notify?.Invoke(LogType.error, "Не найден чат");
                return;
            }
            else 
            {
                if (FindUser(contact.UserId))
                {
                    Notify?.Invoke(LogType.error, "Не найден пользователь");
                    return;
                }
                else
                {
                    var userIdParam = new MySqlParameter("@userId", contact.UserId);
                    _query.Parameters.Add(userIdParam);
                    var chatIdParam = new MySqlParameter("@chatId", contact.ChatId);
                    _query.Parameters.Add(chatIdParam);
                    Open();
                    _query.CommandText = $"SELECT chatId, userId FROM table_contacts WHERE chatId='{contact.ChatId}' AND userId='{contact.UserId}';";
                    var result = _query.ExecuteReader();
                    if (result.HasRows) Notify?.Invoke(LogType.warn, $"В чате {contact.ChatId} есть контакт {contact.UserId}");
                    else
                    {
                        result.Close();
                        _query.CommandText = "INSERT INTO table_contacts (chatId, userId) VALUES (@userId, @chatId)";
                        try { _query.ExecuteNonQuery(); }
                        catch { Notify?.Invoke(LogType.error, $"Не удалось добавить пользователя {contact.UserId} в чат {contact.ChatId}"); }
                    }
                    Close();
                }
            }
        }
        public void RecordToMessage(DMMessage message)
        {
            if (!FindChat(message.ChatId))
            {
                Notify?.Invoke(LogType.error, "Не найден чат");
                return;
            }
            else
            {
                if (!FindUser(message.SenderId))
                {
                    Notify?.Invoke(LogType.error, "Не найден пользователь");
                    return;
                }
                else
                {
                    var typeParam = new MySqlParameter("@type", message.MessageType);
                    _query.Parameters.Add(typeParam);
                    var timeParam = new MySqlParameter("@time", message.DateTime);
                    _query.Parameters.Add(timeParam);
                    var senderIdParam = new MySqlParameter("@senderId", message.SenderId);
                    _query.Parameters.Add(senderIdParam);
                    var chatIdParam = new MySqlParameter("@chatId", message.ChatId);
                    _query.Parameters.Add(chatIdParam);
                    var contentParam = new MySqlParameter("@content", message.Content);
                    _query.Parameters.Add(contentParam);

                    Open();            
                    _query.CommandText = "INSERT INTO table_messages (messageType, time, senderId, chatId, content)" +
                    "VALUES (@type, @time, @senderId, @chatId, @content)";
                    try { _query.ExecuteNonQuery(); }
                    catch (Exception e) { Notify?.Invoke(LogType.error, e.ToString()); }
                    Close();
                }
            }
        }
        public void UpdateMessageStatus (int idMes)
        {            
            Open();
            _query.CommandText = $"UPDATE table_messages SET status=1 WHERE id='{idMes}';";
            try { _query.ExecuteNonQuery(); }
            catch (Exception e) { Notify?.Invoke(LogType.error, e.ToString()); }
            Close();
        }

        //Запросы к БД
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
        public bool FindUser(int idUser)
        {
            Open();
            _query.CommandText = $"SELECT *FROM table_users where id='{idUser}';";
            var result = _query.ExecuteReader();
            if (!result.HasRows)
            {
                Notify?.Invoke(LogType.warn, $"Нет данных в таблице пользователей о {idUser}");
                Close();
                return false;
            }
            else
            {
                Close();
                return true;
            }
        }
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

        public bool FindChat(int idChat)
        {
            Open();
            _query.CommandText = $"SELECT *FROM table_chats where id='{idChat}';";
            var result = _query.ExecuteReader();
            if (!result.HasRows)
            {
                Notify?.Invoke(LogType.warn, $"Нет данных в таблице чатов о {idChat}");
                Close();
                return false;
            }
            else
            {
                Close();
                return true;
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
            _query.CommandText = $"SELECT * FROM table_messages WHERE chatId='{idChat}';";
            var result = _query.ExecuteReader();
            if (!result.HasRows)
            {
                Notify?.Invoke(LogType.warn, $"Нет сообщений в чате {idChat}");
                Close();
                return messages;
            }
            else
            {
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

        public List<DMMessage> FindMessagesStatus (int idChat)
        {
            var messages = new List<DMMessage>();
            Open();
            _query.CommandText = $"SELECT * FROM table_messages WHERE chatId='{idChat}' AND status='0';";
            var result = _query.ExecuteReader();
            if (!result.HasRows)
            {
                //Notify?.Invoke(LogType.warn, $"Нет недоставленных сообщений в чате {idChat}");
                Close();
                return null;
            }
            else
            {
                while (result.Read())
                {
                    var mes = new DMMessage();
                    mes.Id = result.GetInt32(0);
                    mes.MessageType = result.GetString(1);
                    mes.DateTime = result.GetDateTime(2);
                    mes.SenderId = result.GetInt32(3);
                    mes.ChatId = result.GetInt32(4);
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
