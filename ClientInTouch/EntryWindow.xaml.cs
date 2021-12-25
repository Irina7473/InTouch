using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
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
        public string Message { get; set; }
        
        public EntryWindow()
        {
            InitializeComponent();
            Message = string.Empty;
            
        }

        private void Button_Entry_Click(object sender, RoutedEventArgs e)
        {
            //Добавить ввод и проверку адреса и порта
            ip = IPAddress.Parse("127.0.0.1");
            port = 8005;
            client.ConnectToServer(ip, port, TextBox_Login.Text, TextBox_Password.Text);
            Message = client.Read();
            this.DialogResult = true;             
        }

        private void Button_NowRegister_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Обратитесь к администратору");
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
