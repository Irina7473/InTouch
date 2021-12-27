using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using InTouchServer;

namespace ClientInTouch
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    public partial class MainWindow : Window
    {
        Client client;
        IPAddress ip;
        int port;
        string message;
        public static event Action<MessageType, string> Notify;
        LogToFile log;        
        public Task taskRead;
        private CancellationTokenSource cancelTokenSend;
        private CancellationTokenSource cancelTokenRead;
        public MainWindow()
        {
            InitializeComponent();
            client = new();
            log = new();
            message = string.Empty;
            Notify += log.RecordToLog;
            Client.Notify += log.RecordToLog;
        }

        private void Button_Entry_Click(object sender, RoutedEventArgs e)
        {
            EntryWindow entry = new EntryWindow();
            entry.Owner = this;
            entry.client = this.client;
            
            if (entry.ShowDialog() == true)
            {
                //сравнение не работает!!
                if (entry.Message != "admit") MessageBox.Show("Неверный логин или пароль");
                else
                {
                    MessageBox.Show("Авторизация пройдена");
                    //распарсить entry.Message - взять user.Id
                    Button_Entry.Content = entry.TextBox_Login.Text; //сюда user.login
                    taskRead = new(() => { ReceivedAsync(); });
                    taskRead.Start();
                }
                MessageBox.Show("Авторизация пройдена");
                //распарсить entry.Message - взять user.Id
                Button_Entry.Content = entry.TextBox_Login.Text; //сюда user.login
                taskRead = new(() => { ReceivedAsync(); });
                taskRead.Start();
            }
            else MessageBox.Show("Авторизация не пройдена");
        }
                       
        private void Button_Settings_Click(object sender, RoutedEventArgs e)
        {
            ip= IPAddress.Parse("127.0.0.1");
            port = 8005;
            client.ConnectToServer(ip, port, "login", "password");
            
            taskRead = new(() => { ReceivedAsync(); });
            taskRead.Start();
        }

        private async void ReceivedAsync()
        {
            if (client.client.Connected)
            {
                while (client.client.Connected)
                {
                    message = client.Read();
                    if (cancelTokenRead != null) return;
                    try
                    {
                        using (cancelTokenRead = new CancellationTokenSource())
                        { await AppendFormattedTextAsync("server", message, cancelTokenRead.Token); }
                    }
                    catch (Exception exc) { Notify?.Invoke(MessageType.error, exc.Message); }
                    finally { cancelTokenRead = null; }
                }
            }
            else
            {
                Notify?.Invoke(MessageType.error, "Соединение с сервером разорвано");
                MessageBox.Show("Соединение с сервером разорвано");
            }
        }
        
        private async void Button_Send_Click(object sender, RoutedEventArgs e)
        {
            if (client.client != null)
            {
                if (client.client.Connected)
                {
                    message = TextBox_Message.Text;
                    //AppendFormattedText("client", message);
                    if (cancelTokenSend != null) return;
                    try
                    {
                        using (cancelTokenSend = new CancellationTokenSource())
                        { await AppendFormattedTextAsync("client", message, cancelTokenSend.Token); }
                    }
                    catch (Exception exc) { Notify?.Invoke(MessageType.error, exc.Message); }
                    finally { cancelTokenSend = null;}
                    TextBox_Message.Text = "";
                    client.Send(message);                    
                }
                else
                {
                    Notify?.Invoke(MessageType.error, "Соединение с сервером разорвано");
                    MessageBox.Show("Соединение с сервером разорвано");
                }
            }
            else
            {
                Notify?.Invoke(MessageType.error, "Соединение с сервером не установлено");
                MessageBox.Show("Соединение с сервером не установлено");
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
                    rangeOfWord.Text = "\t\t" + text + "\r";
                    rangeOfWord.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Green);
                }
                await Task.Delay(10, token);
            }, DispatcherPriority.Normal, token);
        }

        private void Button_AddChat_Click(object sender, RoutedEventArgs e)
        {

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
            if (TextBox_SearchContact.Text == "")
            {
                TextBox_SearchContact.Text = "Поиск";
                TextBox_SearchContact.Foreground = Brushes.Gray;
            }
        }

        private void TextBox_Message_GotFocus(object sender, RoutedEventArgs e)
        {
            if (TextBox_Message.Text == "Написать сообщение")
            {
                TextBox_Message.Text = "";
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
    }
}

/*
 *      
        private void Received()
        {
            while (client.client.Connected)
            {
                message = client.Read();
                AppendFormattedText("server", message);
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
                rangeOfWord.Text = "\t\t" + text + "\r";
                rangeOfWord.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Green);
            }
        }
*/