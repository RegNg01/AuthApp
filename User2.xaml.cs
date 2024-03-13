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
using System.Windows.Navigation;
using System.Windows.Shapes;
using FireSharp;
using FireSharp.Config;
using FireSharp.Interfaces;
using FireSharp.Response;
using System.IO;
using Firebase.Storage;
using System.Net;
using System.Globalization;
namespace WpfApp1
{
    /// <summary>
    /// Логика взаимодействия для User2.xaml
    /// </summary>
    public partial class User2 : UserControl
    {
        public User2()
        {
            InitializeComponent();
            auth = new FirebaseAuth();
        }
        FirebaseAuth auth;
        public string sender_login;
        public void messageButton_Click(object sender, RoutedEventArgs e)
        {
            ChatWindow chatWindow = new ChatWindow();
            chatWindow.reciever_login = recordloginTextBlock.Text;
            chatWindow.sender_login = sender_login;
            chatWindow.LoadMessages();
            chatWindow.userLabel.Content= recordloginTextBlock.Text;
            chatWindow.Show();
            Window parentWindow = Window.GetWindow(this);
            parentWindow.Close();
        }
    }
}
