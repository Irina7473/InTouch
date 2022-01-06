using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
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

using InTouchServer;

namespace ClientInTouch
{
    /// <summary>
    /// Логика взаимодействия для EntryWindow.xaml
    /// </summary>
    public partial class EntryWindow : Window
    {
        public Client client;
        IPAddress ip;
        int port;
        string login;
        string password;
        string message; 
        
        public EntryWindow()
        {
            InitializeComponent();
            client = new();
            login = string.Empty;
            password = string.Empty;
            message = string.Empty;
        }

        private void Button_Entry_Click(object sender, RoutedEventArgs e)
        {
            if (!Regex.IsMatch(TextBox_IP.Text, @"([0-9]{1,3}[\.]){3}[0-9]{1,3}"))
            {
                MessageBox.Show("Некорректный ввод ipaddress");
                return;
            }
            else
            {
                ip = IPAddress.Parse( TextBox_IP.Text);
                var correct = Int32.TryParse(TextBox_Port.Text.ToString(), out port);
                if (!correct || port<=0)
                {
                    MessageBox.Show("Некорректный ввод port");
                    return;
                }
                else
                {
                    login = TextBox_Login.Text.ToString();
                    password = TextBox_Password.Text.ToString();
                    client.ConnectToServer(ip, port, login, password);
                    message = client.Read();
                    MessageBox.Show(message);
                    //сравнение не работает из-за буфера!!
                    if (message == "admit")
                    {
                        MessageBox.Show("Авторизация пройдена");
                        this.DialogResult = true;
                    }
                    else MessageBox.Show("Неверный логин или пароль");
                }
            }
        }

    private void Button_NowRegister_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Обратитесь к администратору");
        }

        private void CheckBox_Remember_Checked(object sender, RoutedEventArgs e)
        {

            login = TextBox_Login.Text.ToString();
            password = TextBox_Password.Text.ToString();
        }

        private void CheckBox_Remember_Unchecked(object sender, RoutedEventArgs e)
        {
            login = string.Empty;
            password = string.Empty;
        }

        private void TextBox_IP_GotFocus(object sender, RoutedEventArgs e)
        {
            if (TextBox_Password.Text == "127.0.0.1")
            {
                TextBox_Password.Text = "";
                TextBox_Password.Foreground = Brushes.Black;
            }
        }

        private void TextBox_IP_LostFocus(object sender, RoutedEventArgs e)
        {
            if (TextBox_Password.Text == "")
            {
                TextBox_Password.Text = "127.0.0.1";
                TextBox_Password.Foreground = Brushes.Gray;
            }
        }

        private void TextBox_Port_GotFocus(object sender, RoutedEventArgs e)
        {
            if (TextBox_Password.Text == "8005")
            {
                TextBox_Password.Text = "";
                TextBox_Password.Foreground = Brushes.Black;
            }
        }

        private void TextBox_Port_LostFocus(object sender, RoutedEventArgs e)
        {
            if (TextBox_Password.Text == "")
            {
                TextBox_Password.Text = "8005";
                TextBox_Password.Foreground = Brushes.Gray;
            }
        }

        private void TextBox_Login_GotFocus(object sender, RoutedEventArgs e)
        {
            if (TextBox_Login.Text == "Логин")
            {
                TextBox_Login.Text = "";
                TextBox_Login.Foreground = Brushes.Black;
            }
        }

        private void TextBox_Login_LostFocus(object sender, RoutedEventArgs e)
        {
            if (TextBox_Login.Text == "")
            {
                TextBox_Login.Text = "Логин";
                TextBox_Login.Foreground = Brushes.Gray;
            }
        }

        private void TextBox_Password_GotFocus(object sender, RoutedEventArgs e)
        {
            if (TextBox_Password.Text == "Пароль")
            {
                TextBox_Password.Text = "";
                TextBox_Password.Foreground = Brushes.Black;
            }
        }

        private void TextBox_Password_LostFocus(object sender, RoutedEventArgs e)
        {
            if (TextBox_Password.Text == "")
            {
                TextBox_Password.Text = "Пароль";
                TextBox_Password.Foreground = Brushes.Gray;
            }
        }

    }
}
