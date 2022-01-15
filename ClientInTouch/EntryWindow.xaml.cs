using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using InTouchLibrary;

namespace ClientInTouch
{
    /// <summary>
    /// Логика взаимодействия для EntryWindow.xaml
    /// </summary>
    public partial class EntryWindow : Window
    {
        public Client client;
        public EntryWindow()
        {
            InitializeComponent();
            client = new();
        }

        private void Button_Entry_Click(object sender, RoutedEventArgs e)
        {
            //проверка IPAddress
            if (!Regex.IsMatch(TextBox_IP.Text, @"([0-9]{1,3}[\.]){3}[0-9]{1,3}"))
            {
                MessageBox.Show("Некорректный ввод ipaddress");
                return;
            }
            else
            {
                IPAddress ip = IPAddress.Parse( TextBox_IP.Text);
                //проверка порта
                var correct = Int32.TryParse(TextBox_Port.Text.ToString(), out int port);
                if (!correct || port<=0)
                {
                    MessageBox.Show("Некорректный ввод port");
                    return;
                }
                else
                {
                    var login = TextBox_Login.Text.ToString();
                    var password = TextBox_Password.Text.ToString();
                    //подключение к серверу и отправка логина-пароля для авторизации
                    client.ConnectToServer(ip, port, login, password);
                    var message = client.Read();
                    var mesCreat = JsonSerializer.Deserialize<MessageInfo>(message);
                    if (mesCreat.Type == MessageType.error) MessageBox.Show(mesCreat.Mes);
                    else if (mesCreat.Type == MessageType.recd) //успешное подключение и авторизация
                    {
                        MessageBox.Show($"{mesCreat.Mes}");
                        this.DialogResult = true;
                    }
                    else MessageBox.Show("Ошибка авторизации");
                }
            }
        }

        private void Button_NowRegister_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Обратитесь к администратору");
        }

        private void TextBox_IP_GotFocus(object sender, RoutedEventArgs e)
        {
            if (TextBox_Password.Text == "127.0.0.1")
            {
                TextBox_Password.Text = string.Empty;
                TextBox_Password.Foreground = Brushes.Black;
            }
        }
        private void TextBox_IP_LostFocus(object sender, RoutedEventArgs e)
        {
            if (TextBox_Password.Text == string.Empty)
            {
                TextBox_Password.Text = "127.0.0.1";
                TextBox_Password.Foreground = Brushes.Gray;
            }
        }
        private void TextBox_Port_GotFocus(object sender, RoutedEventArgs e)
        {
            if (TextBox_Password.Text == "8005")
            {
                TextBox_Password.Text = string.Empty;
                TextBox_Password.Foreground = Brushes.Black;
            }
        }
        private void TextBox_Port_LostFocus(object sender, RoutedEventArgs e)
        {
            if (TextBox_Password.Text == string.Empty)
            {
                TextBox_Password.Text = "8005";
                TextBox_Password.Foreground = Brushes.Gray;
            }
        }
        private void TextBox_Login_GotFocus(object sender, RoutedEventArgs e)
        {
            if (TextBox_Login.Text == "Логин")
            {
                TextBox_Login.Text = string.Empty;
                TextBox_Login.Foreground = Brushes.Black;
            }
        }
        private void TextBox_Login_LostFocus(object sender, RoutedEventArgs e)
        {
            if (TextBox_Login.Text == string.Empty)
            {
                TextBox_Login.Text = "Логин";
                TextBox_Login.Foreground = Brushes.Gray;
            }
        }
        private void TextBox_Password_GotFocus(object sender, RoutedEventArgs e)
        {
            if (TextBox_Password.Text == "Пароль")
            {
                TextBox_Password.Text = string.Empty;
                TextBox_Password.Foreground = Brushes.Black;
            }
        }
        private void TextBox_Password_LostFocus(object sender, RoutedEventArgs e)
        {
            if (TextBox_Password.Text == string.Empty)
            {
                TextBox_Password.Text = "Пароль";
                TextBox_Password.Foreground = Brushes.Gray;
            }
        }
    }
}
