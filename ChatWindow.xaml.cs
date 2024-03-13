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
    /// Логика взаимодействия для ChatWindow.xaml
    /// </summary>
    public partial class ChatWindow : Window
    {
        FirebaseAuth auth;
        public string sender_login;
        public string reciever_login;
        public ChatWindow()
        {
            InitializeComponent();
            auth = new FirebaseAuth();
            
        }
        public async void LoadMessages()
        {
            List<Message> messages = new List<Message>();
            FirebaseResponse response = await auth.client.GetAsync("messages/");
            Dictionary<string, Message> result = response.ResultAs<Dictionary<string, Message>>();
            if (result != null) 
            messages = result.Values.ToList(); 
            foreach (Message message in messages)
            {
                if (message.sender==sender_login && message.reciever==reciever_login)
                {    
                    TextBlock textBlock = new TextBlock();
                    textBlock.Text = message.text;
                    textBlock.TextWrapping = TextWrapping.Wrap;
                    textBlock.MaxWidth = 300;
                    textBlock.Padding = new Thickness(10,5,10,5);
                    TextBlock textBlock1 = new TextBlock();
                    textBlock1.Text = message.time;
                    textBlock1.Padding = new Thickness(10, 5, 10, 0);
                    textBlock1.FontSize = 12;
                    Border border = new Border();
                    border.Background = Brushes.LightBlue;
                    border.CornerRadius = new CornerRadius(10);
                    StackPanel stackPanel = new StackPanel();
                    stackPanel.Children.Add(textBlock1);
                    stackPanel.Children.Add(textBlock); 
                    border.Child = stackPanel;
                    border.HorizontalAlignment = HorizontalAlignment.Right;
                    Grid grid = new Grid();
                    grid.Width = 350;
                    grid.Children.Add(border);
                    messagesListView.Items.Add(grid);
                }
                else if (message.reciever == sender_login && message.sender == reciever_login)
                {
                    TextBlock textBlock = new TextBlock();
                    textBlock.Text = message.text;
                    textBlock.TextWrapping = TextWrapping.Wrap;
                    textBlock.MaxWidth = 300;
                    textBlock.Padding = new Thickness(10, 5, 10, 5);
                    TextBlock textBlock1 = new TextBlock();
                    textBlock1.Text = message.time;
                    textBlock1.Padding = new Thickness(10, 5, 10, 0);
                    textBlock1.FontSize = 12;
                    Border border = new Border();
                    border.Background = Brushes.LightGray;
                    border.CornerRadius = new CornerRadius(10);
                    StackPanel stackPanel = new StackPanel();
                    stackPanel.Children.Add(textBlock1);
                    stackPanel.Children.Add(textBlock);
                    border.Child = stackPanel;
                    border.HorizontalAlignment = HorizontalAlignment.Left;
                    Grid grid = new Grid();
                    grid.Width = 350;
                    grid.Children.Add(border);
                    messagesListView.Items.Add(grid); 
                
            }
            if (messagesListView.Items.Count > 0)
            messagesListView.ScrollIntoView(messagesListView.Items[messagesListView.Items.Count - 1]);
            }
        }
        private async void user1TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Message message = new Message()
                {
                    reciever = reciever_login,
                    sender = sender_login,
                    time = DateTime.UtcNow.ToString(),
                    text = user1TextBox.Text
                };
                await auth.client.PushAsync("messages/",message);
                TextBlock textBlock = new TextBlock();
                textBlock.Text = message.text;
                textBlock.TextWrapping = TextWrapping.Wrap;
                textBlock.MaxWidth = 300;
                textBlock.Padding = new Thickness(10, 5, 10, 5);
                TextBlock textBlock1 = new TextBlock();
                textBlock1.Text = message.time;
                textBlock1.Padding = new Thickness(10, 5, 10, 0);
                textBlock1.FontSize = 12;
                Border border = new Border();
                border.Background = Brushes.LightBlue;
                border.CornerRadius = new CornerRadius(10);
                StackPanel stackPanel = new StackPanel();
                stackPanel.Children.Add(textBlock1);
                stackPanel.Children.Add(textBlock);
                border.Child = stackPanel;
                border.HorizontalAlignment = HorizontalAlignment.Right;
                Grid grid = new Grid();
                grid.Width = 350;
                grid.Children.Add(border);
                messagesListView.Items.Add(grid);
                user1TextBox.Text = "";
                if (messagesListView.Items.Count > 0)
                    messagesListView.ScrollIntoView(messagesListView.Items[messagesListView.Items.Count - 1]);
            }
        }
        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }

        private void closeButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

         
        private void returnButton_Click(object sender, RoutedEventArgs e)
        {
            ChatList chatList = new ChatList();
            chatList.sender_login = sender_login;
            chatList.LoadUsers("");
            chatList.Show();
            this.Close();
        }
    }
}
