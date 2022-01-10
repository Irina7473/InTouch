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
        public Client client;
        string message;
        public static event Action<LogType, string> Notify;
        LogToFile log;
        
        ObservableCollection<DMUser> users;
        public AddChatWindow()
        {
        InitializeComponent();
            client = new();
            log = new();
            message = string.Empty;
            Notify += log.RecordToLog;
            Client.Notify += log.RecordToLog;
            
            users = new ObservableCollection<DMUser>{};
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
