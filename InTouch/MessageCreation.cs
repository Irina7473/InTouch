using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using DataBaseActions;

namespace InTouchLibrary
{
    /// <summary>
    /// Перечисление типов сообщений между сервером и клиентом
    /// </summary>
    public enum MessageType
    {
        /// <summary>
        /// Информация
        /// </summary>
        info,
        /// <summary>
        /// Информация, требующая внимания
        /// </summary>
        warn,
        /// <summary>
        /// Информация об ошибке
        /// </summary>
        error,
        /// <summary>
        /// Сообщение о принятии информации
        /// </summary>
        recd,
        /// <summary>
        /// Информация для идентификации пользователя
        /// </summary>
        ident,
        /// <summary>
        /// Информация о пользователе
        /// </summary>
        user,
        /// <summary>
        /// Информация о чате
        /// </summary>
        chat,
        /// <summary>
        /// Информация о содержании сообщения
        /// </summary>
        content,
        /// <summary>
        /// Информация о закрытии соединения
        /// </summary>
        leave
    }

    // Попробовать позже отказаться от конструкторов совсем

    /// <summary>
    /// Базовый класс для создания сообщений
    /// </summary>
    public class MessageCreation
    {
        /// <summary>
        /// Объект перечисления MessageType
        /// </summary>
        public MessageType Type { get; set; }
        /// <summary>
        /// Конструктор, принимающий Объект перечисления MessageType
        /// </summary>
        /// <param name="type">Объект перечисления MessageType</param>
        public MessageCreation(MessageType type)
        { Type = type;}
    }

    /// <summary>
    /// Класс для создания информационных сообщений, наследник класса MessageCreation
    /// </summary>
    public class MessageInfo : MessageCreation
    {
        /// <summary>
        /// Текст сообщения
        /// </summary>
        public string Mes { get; set; }
        /// <summary>
        /// Конструктор, принимающий Объект перечисления MessageType и  текстовое сообщение
        /// </summary>
        /// <param name="type">Объект перечисления MessageType</param>
        /// <param name="mes">Текстовое сообщение</param>
        public MessageInfo (MessageType type, string mes) : base(type)
        { Mes = mes; }
    }

    /// <summary>
    /// Класс для создания сообщений об авторизации, наследник класса MessageCreation
    /// </summary>
    public class MessageIdent : MessageCreation
    {
        /// <summary>
        /// Логин пользователя
        /// </summary>
        public string Login { get; set; }
        /// <summary>
        /// Пароль пользователя
        /// </summary>
        public string Password { get; set; }
        /// <summary>
        /// Конструктор, принимающий Объект перечисления MessageType, логин и пароль пользователя типа string
        /// </summary>
        /// <param name="type">Объект перечисления MessageType</param>
        /// <param name="login">Логин типа strin</param>
        /// <param name="password">Пароль типа strin</param>
        public MessageIdent(MessageType type, string login, string password) : base(type)  
        {
            Login = login;
            Password = password;
        }     
    }

    /// <summary>
    /// Класс для создания сообщений о пользователе, наследник класса MessageCreation
    /// </summary>
    public class MessageSendUser : MessageCreation
    {
        /// <summary>
        /// Пользователь, объект класса DMUser
        /// </summary>
        public DMUser User { get; set; }
        /// <summary>
        /// Конструктор, принимающий Объект перечисления MessageType и объект класса DMUser
        /// </summary>
        /// <param name="type">Объект перечисления MessageType</param>
        /// <param name="user">Пользователь - объект класса DMUser</param>
        public MessageSendUser(MessageType type, DMUser user) : base(type)
        { User = user;}
    }

    /// <summary>
    /// Класс для создания сообщений о чате, наследник класса MessageCreation
    /// </summary>
    public class MessageSendChat : MessageCreation
    {
        /// <summary>
        /// Чат, объект класса DMChat
        /// </summary>
        public DMChat Chat { get; set; }
        /// <summary>
        /// Конструктор, принимающий Объект перечисления MessageType и объект класса DMChat
        /// </summary>
        /// <param name="type">Объект перечисления MessageType</param>
        /// <param name="chat">Чат - объект класса DMChat</param>
        public MessageSendChat(MessageType type, DMChat chat) : base(type)
        { Chat = chat; }
    }

    /// <summary>
    /// Класс для создания сообщений о сообщениях пользователей в чате, наследник класса MessageCreation
    /// </summary>
    public class MessageSendContent : MessageCreation
    {
        /// <summary>
        /// Сообщение, объект класса DMMessage
        /// </summary>
        public DMMessage Message { get; set; }
        /// <summary>
        /// Конструктор, принимающий Объект перечисления MessageType и объект класса DMMessage
        /// </summary>
        /// <param name="type">Объект перечисления MessageType</param>
        /// <param name="message">Сообщение - объект класса DMMessage</param>
        public MessageSendContent(MessageType type, DMMessage message) : base(type)
        { Message = message; }
    }
}
