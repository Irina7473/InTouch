using System;
using System.Collections.Generic;
using System.Linq;
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
using InTouchLibrary;
using DataBaseActions;
using Logger;
using System.Collections.ObjectModel;

namespace ClientInTouch
{
    /// <summary>
    /// Логика взаимодействия для AddChatWindow.xaml
    /// </summary>
    public partial class AddChatWindow : Window
    {
        public static event Action<LogType, string> Notify;
        LogToFile log;
        public Client client;
        private ObservableCollection<DMUser> users;

        public AddChatWindow()
        {
        InitializeComponent();
            log = new();
            Notify += log.RecordToLog;
            Client.Notify += log.RecordToLog;
            client = new ();
            users = new ();
            UsersList.ItemsSource = users;
        }

        private void TextBox_SearchContacts_SelectionChanged(object sender, RoutedEventArgs e)
        {

        }

        private void Button_CreateChat_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ContactsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void Button_Back_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }
    }
}
