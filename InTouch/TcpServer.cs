using System;
using System.Text;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using Logger;

namespace InTouchLibrary
{
    /// <summary>
    /// Класс TCP-сервера для создания соединений с клиентами
    /// </summary>
    public class TcpServer
    {
        /// <summary>
        /// Событие делегата Action, принимающее тип события из перечисления LogType и его содержание типа string
        /// </summary>
        public static event Action<LogType, string> Notify;
        private int _port;
        private TcpListener _listener;
        /// <summary>
        /// Максимальное количество активных соединения
        /// </summary>
        private int _amtTouch;
        /// <summary>
        /// Номер соединения
        /// </summary>
        public int numberTouch;

        /// <summary>
        /// Конструктор TCP-сервера
        /// </summary>
        /// <param name="port">Номер порта для соединения</param>
        /// <param name="amt">Максимальное количество активных соединения</param>
        public TcpServer(int port, int amt)
        {
            _port = port;
            _amtTouch = amt;
            numberTouch = 0;
        }

        /// <summary>
        /// Подключение TCP-сервера с прослушиванием и направлением каждого соединения в отдельный поток
        /// </summary>
        public void StartTcpServer()
        {
            try
            {
                _listener = new TcpListener(IPAddress.Any, _port);
                _listener.Start(_amtTouch);
                Notify?.Invoke(LogType.info, $"{DateTime.Now} Listen");
            }
            catch (Exception e)
            {                
                Notify?.Invoke(LogType.error, $"{DateTime.Now} {e}");
            }

            while (true)
            {
                if (_listener.Pending())
                {
                    var connection = _listener.AcceptTcpClient();
                    var client=new ClientConnection();
                    numberTouch++;
                    Notify?.Invoke(LogType.info, $"{DateTime.Now} Accept {numberTouch}");
                    Task task = new(() =>
                    {                        
                        client.ConnectToClient(connection, numberTouch);
                    });
                    task.Start();                    
                }
            }
        }
    }
}
