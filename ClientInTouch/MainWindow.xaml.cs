using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.IO;
using System.Text.Json;
using InTouchLibrary;
using DataBaseActions;
using Logger;

namespace ClientInTouch
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    public partial class MainWindow : Window
    {
        public static event Action<LogType, string> Notify;
        LogToFile log;
        Task taskRead;
        private CancellationTokenSource cancelTokenRead;

        Client client;
        private ObservableCollection<DMChat> chats;
        int selectedChat; //не помогло

        public MainWindow()
        {
            InitializeComponent();
            RichTextBox_СhatСontent.IsEnabled = false;
            Closed += Exit;
            log = new();
            Notify += log.RecordToLog;
            Client.Notify += log.RecordToLog;
                        
            client = new();
            chats = new();
            ChatsList.ItemsSource = chats;
            selectedChat = -1; //не помогло
        }
       
        private void Button_Entry_Click(object sender, RoutedEventArgs e)
        {
            //открываю дочернее окно для подключения к серверу и автризации на нем
            EntryWindow entry = new ();
            entry.Owner = this;
            entry.client = this.client;
            if (entry.ShowDialog() == true) //удачное подключение и авторизация
            {
                client.user = client.ReceiveUser(); // получаю user 
                Button_Entry.Content = client.user.Login;  
                Button_Entry.IsEnabled = false;
                // получаю чаты из user и добавляю в список чатов
                foreach (var chat in client.user.Chats) chats.Add(chat);
                //сообщаю серверу, что клиент готов к обмену сообщениями
                var message = JsonSerializer.Serialize<MessageInfo>(new MessageInfo(MessageType.recd, string.Empty));
                client.Send(message);
                //запускаю прослушивание сервера
                taskRead = new(() => { Received(); });
                taskRead.Start();
            }
            else MessageBox.Show("Авторизация не пройдена");
        } 
       
        private void Received()
        {
            try
            {
                while (client.client.Connected)
                {
                    var message = client.Read();
                    var mesCreat = JsonSerializer.Deserialize<MessageCreation>(message);
                    if (mesCreat.Type == MessageType.content)
                    {
                        try
                        {
                            var mesSend = JsonSerializer.Deserialize<MessageSendContent>(message);
                            mesSend.Message.Status = true;
                            Notify?.Invoke(LogType.info, mesSend.Message.ToString());
                            AddMessageToChat(mesSend.Message);  // добавляю сообщение в коллекцию chats
                            
                            Thread.Sleep(1000);
                            //обноление чата не работает из этого потока
                            if (mesSend.Message.ChatId == selectedChat)
                            {
                                List<DMMessage> messages = new();
                                lock (chats)  //блокирую коллекцию
                                {
                                    foreach (var chat in chats)
                                        if (chat.Id == mesSend.Message.ChatId)
                                            if (chat.Messages.Count != 0) messages = chat.Messages;
                                }
                                if (messages != null && messages.Count != 0)
                                    RefreshChatsListAsync(messages);
                            }
                            
                        }
                        catch (Exception e) { Notify?.Invoke(LogType.error, $"{DateTime.Now} {e}"); }
                    }
                    if (mesCreat.Type == MessageType.chat)
                    {
                        var mesSend = JsonSerializer.Deserialize<MessageSendChat>(message);
                        // доделать
                    }
                }
            }
            catch
            {
                Notify?.Invoke(LogType.error, "Соединение с сервером разорвано");
                MessageBox.Show("Соединение с сервером разорвано");
            }
        }

        private void AddMessageToChat(DMMessage message)
        {
            lock (chats) //блокирую коллекцию
            {
                for (var i = 0; i < chats.Count; i++)
                    if (chats[i].Id == message.ChatId)
                    {
                        chats[i].Messages.Add(message);
                        Notify?.Invoke(LogType.info, $"Для {client.user.Id} доставлено в chats {chats[i].Messages.Count}-е сообщение");
                    }
            }
        }

        private void ChatsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var id = ((DMChat)((ListBox)sender).SelectedItem).Id;
            selectedChat = id;  //не помогло
            RefreshChatsList(id); // обновляю сообщения в выбранном чате
        }

        private void Button_Send_Click(object sender, RoutedEventArgs e)
        {
            if (client.client != null)
            {
                if (client.client.Connected)
                {
                    if (TextBox_Message.Text != null && TextBox_Message.Text != string.Empty)
                    {
                        //формирую и отправляю сообщение серверу
                        var mes = new DMMessage();
                        mes.MessageType = "text";
                        mes.DateTime = DateTime.Now;
                        mes.SenderId = client.user.Id;
                        mes.ChatId = ((DMChat)((ChatsList).SelectedItem)).Id;
                        mes.Content = TextBox_Message.Text;
                        var message = JsonSerializer.Serialize<MessageSendContent>(new MessageSendContent(MessageType.content, mes));
                        client.Send(message);
                        // обновляю сообщения в выбранном чате
                        Thread.Sleep(1000);
                        RefreshChatsList(mes.ChatId);
                    }
                }
                else
                {
                    Notify?.Invoke(LogType.error, "Соединение с сервером разорвано");
                    MessageBox.Show("Соединение с сервером разорвано");
                }
            }
            else
            {
                Notify?.Invoke(LogType.error, "Соединение с сервером не установлено");
                MessageBox.Show("Соединение с сервером не установлено");
            }
            TextBox_Message.Text = string.Empty;
        }

        private void Button_AddChat_Click(object sender, RoutedEventArgs e)
        {
            // открываю  
            AddChatWindow addchat = new();
            addchat.Owner = this;
            addchat.client = this.client;
            //не открывается - доделать
        }

        private void TextBox_SearchContact_GotFocus(object sender, RoutedEventArgs e)
        {
            if (TextBox_SearchContact.Text == "Поиск")
            {
                TextBox_SearchContact.Text = string.Empty;
                TextBox_SearchContact.Foreground = Brushes.DarkGreen;
            }
        }
        private void TextBox_SearchContact_LostFocus(object sender, RoutedEventArgs e)
        {
            if (TextBox_SearchContact.Text == string.Empty)
            {
                TextBox_SearchContact.Text = "Поиск";
                TextBox_SearchContact.Foreground = Brushes.Gray;
            }
        }
        private void TextBox_Message_GotFocus(object sender, RoutedEventArgs e)
        {
            if (TextBox_Message.Text == "Написать сообщение")
            {
                TextBox_Message.Text = string.Empty;
                TextBox_Message.Foreground = Brushes.DarkGreen;
            }
        }
        private void TextBox_Message_LostFocus(object sender, RoutedEventArgs e)
        {
            if (TextBox_Message.Text == "")
            {
                TextBox_Message.Text = "Написать сообщение";
                TextBox_Message.Foreground = Brushes.Gray;
            }
        }

        private void Exit(object sender, System.EventArgs e)
        {
            client.Close();
        }

        private void RefreshChatsList(int id)
        {
            List<DMMessage> messages = new();
            lock (chats)  //блокирую коллекцию
            {
                foreach (var chat in chats)
                    if (chat.Id == id)
                        if (chat.Messages.Count != 0) messages = chat.Messages;
                if (messages != null && messages.Count != 0)
                {

                    RichTextBox_СhatСontent.Document.Blocks.Clear();
                    foreach (var mes in messages)
                    {
                        string type = string.Empty;
                        string message = string.Empty;
                        if (mes.SenderId == client.user.Id)
                        {
                            type = "client";
                            message = mes.Content;
                        }
                        else
                        {
                            type = "server";
                            message = $"{mes.SenderLogin()} : " + mes.Content;
                        }
                        AppendFormattedText(type, message);
                    }
                }
            }
        }
        private void AppendFormattedText(string type, string text)
        {
            TextRange rangeOfWord = new TextRange(RichTextBox_СhatСontent.Document.ContentEnd, RichTextBox_СhatСontent.Document.ContentEnd);
            rangeOfWord.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.Regular);
            if (type == "server")
            {
                rangeOfWord.Text = text + "\r";
                rangeOfWord.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Blue);
            }
            if (type == "client")
            {
                rangeOfWord.Text = "\t\t\t" + text + "\r";
                rangeOfWord.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.DarkGreen);
            }
        }

        //эти 2 метода пока не помогают обновить чат
        private async Task RefreshChatsListAsync(List<DMMessage> messages)
        {
            if (cancelTokenRead != null) return;
            try
            {
                using (cancelTokenRead = new CancellationTokenSource())
                {
                    RichTextBox_СhatСontent.Document.Blocks.Clear();
                    foreach (var mes in messages)
                    {
                        string type = string.Empty;
                        string message = string.Empty;
                        if (mes.SenderId == client.user.Id)
                        {
                            type = "client";
                            message = mes.Content;
                        }
                        else
                        {
                            type = "server";
                            message = $"{mes.SenderLogin()} : " + mes.Content;
                        }
                        await AppendFormattedTextAsync(type, message, cancelTokenRead.Token);
                    }
                }
            }
            catch (Exception exc) { Notify?.Invoke(LogType.error, exc.Message); }
            finally { cancelTokenRead = null; }
        }
        private async Task AppendFormattedTextAsync(string type, string text, CancellationToken token)
        {
            await RichTextBox_СhatСontent.Dispatcher.Invoke(async () =>
            {
                TextRange rangeOfWord = new TextRange(RichTextBox_СhatСontent.Document.ContentEnd, RichTextBox_СhatСontent.Document.ContentEnd);
                rangeOfWord.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.Regular);
                if (type == "server")
                {
                    rangeOfWord.Text = text + "\r";
                    rangeOfWord.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Blue);
                }
                if (type == "client")
                {
                    rangeOfWord.Text = "\t\t\t" + text + "\r";
                    rangeOfWord.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Green);
                }
                await Task.Delay(10, token);
            }, DispatcherPriority.Normal, token);
        }
       
    }
}