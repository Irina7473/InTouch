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
        private CancellationTokenSource cancelTokenSend;
        //private CancellationTokenSource cancelTokenRead;

        Client client;

        //readonly object chatsLock = new object();
        //private Mutex mutexChat;
        private ObservableCollection<DMChat> chats;
        /*
        public ObservableCollection<DMChat> Chats
        {
            get { return chats; }
            set
            {
                chats = value;
                BindingOperations.EnableCollectionSynchronization(chats, chatsLock);
            }
        }
            */

        public MainWindow()
        {
            InitializeComponent();
            RichTextBox_СhatСontent.IsEnabled = false;
            Closed += Exit;
            log = new();
            Notify += log.RecordToLog;
            Client.Notify += log.RecordToLog;
                        
            client = new();

            //chatsLock = new();
            //mutexChat = new ();

            chats = new();
            ChatsList.ItemsSource = chats;
        }
       
        private void Button_Entry_Click(object sender, RoutedEventArgs e)
        {
            EntryWindow entry = new ();
            entry.Owner = this;
            entry.client = this.client;
            
            if (entry.ShowDialog() == true)
            {
                client.user = client.ReceiveUser(); // получаю user 
                Button_Entry.Content = client.user.Login;  
                Button_Entry.IsEnabled = false;
                // получаю чаты user и добавляю в список чатов
                foreach (var chat in client.user.Chats) chats.Add(chat);
                var message = JsonSerializer.Serialize<MessageInfo>(new MessageInfo(MessageType.recd, string.Empty));
                client.Send(message);
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
                    var mes = client.Read();
                    var mesCreat = JsonSerializer.Deserialize<MessageCreation>(mes);
                    if (mesCreat.Type == MessageType.content)
                    {
                        try
                        {
                            var mesSend = JsonSerializer.Deserialize<MessageSendContent>(mes);
                            //mesSend.Message.Status = true;
                            Notify?.Invoke(LogType.info, mesSend.Message.ToString());
                            AddMessageToChat(mesSend.Message);
                            int id =-1;
                            lock (chats)
                                // здесь не получаю доступ к основному потоку
                            { id = ((DMChat)((ChatsList).SelectedItem)).Id; }
                            if (mesSend.Message.ChatId == id) RefreshChatsList(id);
                        }
                        catch (Exception e) { Notify?.Invoke(LogType.error, $"{DateTime.Now} {e}"); }
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
            lock (chats)
            {
                for (var i = 0; i < chats.Count; i++)
                    if (chats[i].Id == message.ChatId)
                    {
                        Notify?.Invoke(LogType.info, $"Для {client.user.Id} в chats {chats[i].Messages.Count} сообщений");
                        chats[i].Messages.Add(message);
                        Notify?.Invoke(LogType.info, $"Для {client.user.Id} доставлено в chats {chats[i].Messages.Count}-е сообщение");
                    }
            }
        }

        private void ChatsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var id = ((DMChat)((ListBox)sender).SelectedItem).Id;
            RefreshChatsList(id);
        }

        private void RefreshChatsList(int id)
        {
            RichTextBox_СhatСontent.Document.Blocks.Clear();
            List<DMMessage> messages = new();
            lock (chats)
            {
                foreach (var chat in chats)
                    if (chat.Id == id)
                        if (chat.Messages.Count != 0) messages = chat.Messages;
                if (messages != null && messages.Count != 0)
                {
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

        private void Button_Send_Click(object sender, RoutedEventArgs e)
        {
            if (client.client != null)
            {
                if (client.client.Connected)
                {
                    if (TextBox_Message.Text != null && TextBox_Message.Text != string.Empty)
                    {
                        var mes = new DMMessage();
                        mes.MessageType = "text";
                        mes.DateTime = DateTime.Now;
                        mes.SenderId = client.user.Id;
                        mes.ChatId = ((DMChat)((ChatsList).SelectedItem)).Id;
                        mes.Content = TextBox_Message.Text;
                        /*
                        if (cancelTokenSend != null) return;
                        try
                        {
                            using (cancelTokenSend = new CancellationTokenSource())
                            { await AppendFormattedTextAsync("client", mes.Content, cancelTokenSend.Token); }
                        }
                        catch (Exception exc) { Notify?.Invoke(LogType.error, exc.Message); }
                        finally { cancelTokenSend = null; }
                        */
                        var message = JsonSerializer.Serialize<MessageSendContent>(new MessageSendContent(MessageType.content, mes));
                        client.Send(message);
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
            // выбор чат или диалог с 1
            //если чат - имя, для 1 взять логин собеседника
            //если чат - выбор аватар, для 1 взять аватар собеседника, по умолчанию аватар из ресурсов
            //запрос в БД списка user, выбор нужных
            // создание Chat(string name, byte[] avatar, List<User> users) 
            AddChatWindow addchat = new();
            addchat.Owner = this;
            addchat.client = this.client;
        }
        private void TextBox_SearchContact_GotFocus(object sender, RoutedEventArgs e)
        {
            if (TextBox_SearchContact.Text == "Поиск")
            {
                TextBox_SearchContact.Text = "";
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

        private void MenuItem_Click_Look(object sender, RoutedEventArgs e)
        {

        }
        private void MenuItem_Click_Delete(object sender, RoutedEventArgs e)
        {

        }

        private void Exit(object sender, System.EventArgs e)
        {
            client.Close();
        }

        private void Button_AccountSettings_Click(object sender, RoutedEventArgs e)
        {

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
                rangeOfWord.Text = "\t\t\t\t" + text + "\r";
                rangeOfWord.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.DarkGreen);
            }
                
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
                    rangeOfWord.Text = "\t\t\t\t" + text + "\r";
                    rangeOfWord.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Green);
                }
                await Task.Delay(10, token);
            }, DispatcherPriority.Normal, token);
        }
        /*
        private async Task UpdateChatsListAsync(MessageSendContent mesSend, CancellationToken token)
        {
            await ChatsList.Dispatcher.Invoke(async () =>
            {
                mesSend.Message.Status = true;
                for (var i=0; i < CHATS.Count; i++)
                    if (CHATS[i].Id == mesSend.Message.ChatId)
                        CHATS[i].Messages.Add(mesSend.Message);
                
                ChatsList.ItemsSource = CHATS;
                ChatsList.Items.Refresh();
                await Task.Delay(10, token);
            }, DispatcherPriority.Normal, token);

        }
        */
    }
}