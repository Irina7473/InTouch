﻿using System;
using System.Text;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using System.Threading;
using Logger;
using DataBaseActions;

namespace InTouchLibrary
{
    public class ClientConnection
    {
        public static event Action<MessageType, string> Notify;
        public int numberTouch;
        public TcpClient client;
        private NetworkStream _netStream;
        public User user;

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
                    Communication();
                }
                else Close();
            }
            catch (Exception e)
            {
                Notify?.Invoke(MessageType.error, $"{DateTime.Now} Для соединения {numberTouch} {e.ToString()}");
            }
        }

        public bool Identification(int numberTouch)
        {

            var message = Read();
            //распарсить 1 сообщение клиента
            // Проверка клиента или регистрация нового, по логину-паролю
            var found = true;
            if (found) //написать условие
            {
                // user= user из БД
                user = new();
                user.Id = 10;
                Send($"ident|admit|{user.Id.ToString()}");
                return true;
            }
            else
            {
                // если нет такого
                Send("prevent|-1");
                return false;
            }
        }

        public void Communication()
        {
            // Передать user его чаты
            // Если есть сообщения, то передать
            // Запуск чтения и передачи
            
            Task taskSend = new(() => { 
                Send("Hello");                
                Notify?.Invoke(MessageType.text, $"{DateTime.Now} Для {numberTouch} переданы сообщения");
            });
            
            Task taskRead = new(() => {
                while (client.Connected)
                {
                    Read();
                    //Добавить запись в базу данных
                    // Добавить чтение из базы данных
                    Send("Доставлено");
                } });
            taskRead.Start();
            //taskSend.Start();
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
                        Notify?.Invoke(MessageType.error, $"{DateTime.Now} Вы не можете записывать данные в этот поток для {numberTouch}");
                        _netStream.Close();
                        Close();
                        return;
                    }
                }
                catch (Exception e)
                {
                    Notify?.Invoke(MessageType.error, $"{DateTime.Now} {e}");
                }
            }
            else
            {
                Notify?.Invoke(MessageType.warn, $"{DateTime.Now} Клиент {numberTouch} разорвал соединение");
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
                                Notify?.Invoke(MessageType.error, $"{DateTime.Now} {e}");
                                break;
                            }
                        } while (_netStream.DataAvailable);
                        
                        Notify?.Invoke(MessageType.text, $"{DateTime.Now} от {numberTouch} получено сообщение {data.ToString()} ");
                        return data.ToString();
                    }
                    else
                    {
                        Notify?.Invoke(MessageType.error, $"{DateTime.Now} Вы не можете читать данные из этого потока для {numberTouch}");
                        _netStream.Close();
                        Close();
                        return string.Empty;
                    }
                }
                catch (Exception e)
                {                    
                    Notify?.Invoke(MessageType.error, $"{DateTime.Now} {e}");
                    return string.Empty;
                }
            }
            else
            {
                Notify?.Invoke(MessageType.warn, $"{DateTime.Now} Клиент {numberTouch} разорвал соединение");
                return string.Empty;
            }
        }

        private void Close ()
        {
            if (client != null) client.Close();
            if (_netStream != null) _netStream.Close();
            Notify?.Invoke(MessageType.warn, $"{DateTime.Now} Соединение с клиентом закрыто");
        }
    }
}
