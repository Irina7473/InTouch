using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;

namespace InTouchServer
{
    public class DBConnection
    {
        public static event Action<MessageType, string> Notify;
        string connectionString = "Server=mysql60.hostland.ru;Database=host1323541_shambala1;Uid=host1323541_itstep;Pwd=269f43dc;";

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
                throw new Exception("Ошибка открытия БД");                
            }
            catch (MySqlException)
            {
                Notify?.Invoke(MessageType.error, "Подключаемся к уже открытой БД");
                throw new Exception("Подключаемся к уже открытой БД");
            }
            catch (Exception)
            {
                Notify?.Invoke(MessageType.error, "Путь к базе данных не найден");
                throw new Exception("Путь к базе данных не найден");
            }
        }
                
        public void Close()
        {
            _connection.Close();
        }

        private MySqlDataReader SelectQuery(string sql)
        {
            _query.CommandText = sql;
            var result = _query.ExecuteReader();
            return result;
        }

        public void RecordToUser(string message)
        {            
            var user = message.Split('|');
            var login= user[0];
            var password = user[1];
            var fullName = user[2];

            _connection.Open();
            _query.CommandText = $"INSERT INTO table_Users (login, password, fullName)" +
                    $"VALUES ('{login}', '{password}', '{fullName}')";
            _query.ExecuteNonQuery();
            _connection.Close();
        }

        public void RecordToChat(string message)
        {
            var chat = message.Split('|');
            var name = chat[0];
            var security = chat[1];  //как его сделать bool?
            _connection.Open();
            _query.CommandText = $"INSERT INTO table_Chats (name, security)" +
                    $"VALUES ('{name}', '{security}')";
            _query.ExecuteNonQuery();
            _connection.Close();
        }

        public void RecordToContact(string message)
        {
            var contact = message.Split('|');
            var chat = contact[0];
            var chatId = FindChatId(chat);
            var user = contact[1];
            var userId = FindUserId(user);

            _connection.Open();
            _query.CommandText = $"INSERT INTO table_Contacts (chatId, userId)" +
                    $"VALUES ('{chatId}', '{userId}')";
            _query.ExecuteNonQuery();
            _connection.Close();
        }

        public void RecordToMessage(string message)
        {
            var mess = message.Split('|');
            var dateTime = mess[0];
            var sender = mess[1];
            var senderId = FindUserId(sender);
            var chat = mess[2];
            var chatId = FindChatId(chat);
            var messageType = mess[3];
            var content = mess[4];
            
            _connection.Open();
            _query.CommandText = $"INSERT INTO table_Messages (dateTime, senderId, chatId, messageType, content" +
                    $"VALUES ('{dateTime}', '{senderId}', '{chatId}', '{messageType}', '{content}')";
            _query.ExecuteNonQuery();
            _connection.Close();
        }

        //Запросы к БД
        private int FindUserId (string nameUser)
        {            
            _connection.Open();
            _query.CommandText = "SELECT id, name FROM table_Users;";
            var result = _query.ExecuteReader();
            if (!result.HasRows)
            {
                Notify?.Invoke(MessageType.warn, "Нет данных в таблице пользователей");
                return -1;
            }
            else
            {
                int idUser = -1;
                int id = -1;
                string name = string.Empty;                
                do
                {
                    while (result.Read())
                    {
                        do
                        {
                            id = result.GetInt32(0);
                            name = result.GetString(1);
                        }
                        while (name != nameUser);
                        idUser = id;
                    }
                } while (result.NextResult());
                if (result != null) result.Close();
                _connection.Close();
                if (idUser == -1) Notify?.Invoke(MessageType.warn, $"Нет данных о пользователе {nameUser}");
                return idUser;
            }
        }

        private string FindUserName(int idUser)
        {          
            _connection.Open();
            _query.CommandText = "SELECT id, name FROM table_Users;";
            var result = _query.ExecuteReader();
            if (!result.HasRows)
            {
                Notify?.Invoke(MessageType.warn, "Нет данных в таблице пользователей");
                return null;
            }
            else
            {
                string nameUser = string.Empty;
                string name = string.Empty;
                int id = -1;
                do
                {
                    while (result.Read())
                    {
                        do
                        {
                            id = result.GetInt32(0);
                            name = result.GetString(1);
                        }
                        while (id != idUser) ;
                        nameUser = name;
                    }
                } while (result.NextResult());
                if (result != null) result.Close();
                _connection.Close();
                if (nameUser==string.Empty) Notify?.Invoke(MessageType.warn, $"Нет данных о пользователе {idUser}");
                return nameUser;
            }
        }

        private int FindChatId(string nameChat)
        {
            _connection.Open();
            _query.CommandText = "SELECT id, name FROM table_Chats;";
            var result = _query.ExecuteReader();
            if (!result.HasRows)
            {
                Notify?.Invoke(MessageType.warn, "Нет данных в таблице чатов");
                return -1;
            }
            else
            {
                int idChat = -1;
                int id = -1;
                string name = string.Empty;
                do
                {
                    while (result.Read())
                    {
                        do
                        {
                            id = result.GetInt32(0);
                            name = result.GetString(1);
                        }
                        while (name != nameChat);
                        idChat = id;
                    }
                } while (result.NextResult());
                if (result != null) result.Close();
                _connection.Close();
                if (idChat == -1) Notify?.Invoke(MessageType.warn, $"Нет данных о чате {nameChat}");
                return idChat;
            }
        }

        private string FindChatName(int idChat)
        {
            _connection.Open();
            _query.CommandText = "SELECT id, name FROM table_Users;";
            var result = _query.ExecuteReader();
            if (!result.HasRows)
            {
                Notify?.Invoke(MessageType.warn, "Нет данных в таблице пользователей");
                return null;
            }
            else
            {
                string nameChat = string.Empty;
                string name = string.Empty;
                int id = -1;
                do
                {
                    while (result.Read())
                    {
                        do
                        {
                            id = result.GetInt32(0);
                            name = result.GetString(1);
                        }
                        while (id != idChat);
                        nameChat = name;
                    }
                } while (result.NextResult());
                if (result != null) result.Close();
                _connection.Close();
                if (nameChat == string.Empty) Notify?.Invoke(MessageType.warn, $"Нет данных о чате {idChat}");
                return nameChat;
            }
        }

        public List<int> UpdateListUsers(int idChat)
        {
            _connection.Open();
            _query.CommandText = "SELECT chatId, userId FROM table_Contacts;";
            var result = _query.ExecuteReader();
            if (!result.HasRows)
            {
                Notify?.Invoke(MessageType.warn, "Нет данных в таблице контактов");
                return null;
            }
            else {
                var users = new List<int>();
                do
                {
                    while (result.Read())
                    {
                        var idCh = result.GetInt32(0);
                        if (idCh == idChat)
                        {
                            var idUs = result.GetInt32(1);
                            users.Add(idUs);
                        }
                    }
                } while (result.NextResult());
                if (result != null) result.Close();
                _connection.Close();
                if (users.Count == 0) Notify?.Invoke(MessageType.warn, $"Нет данных о чате {idChat}");
                return users;
            }
        }

        public List<int> UpdateListChats(int idUser)
        {
            _connection.Open();
            _query.CommandText = "SELECT chatId, userId FROM table_Contacts;";
            var result = _query.ExecuteReader();
            if (!result.HasRows)
            {
                Notify?.Invoke(MessageType.warn, "Нет данных в таблице контактов");
                return null;
            }
            else
            {
                var chats = new List<int>();
                do
                {
                    while (result.Read())
                    {
                        var idUs = result.GetInt32(1);
                        if (idUs == idUser)
                        {
                            var idCh = result.GetInt32(0);
                            chats.Add(idCh);
                        }
                    }
                } while (result.NextResult());
                if (result != null) result.Close();
                _connection.Close();
                if (chats.Count == 0) Notify?.Invoke(MessageType.warn, $"Нет данных о пользователе {idUser}");
                return chats;
            }
        }
    }
}
