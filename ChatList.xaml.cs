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
    /// Логика взаимодействия для ChatList.xaml
    /// </summary>
    public partial class ChatList : Window
    {
        FirebaseAuth auth;
        public string sender_login;
        public ChatList()
        {
            InitializeComponent();
            auth = new FirebaseAuth();
        }
        public async void LoadUsers(string line)
        {
            List<UserData> users = new List<UserData>();
            FirebaseResponse response = await auth.client.GetAsync("users/");
            Dictionary<string, UserData> result = response.ResultAs<Dictionary<string, UserData>>();
            if (result != null)
                users = result.Values.ToList();
            List<User2> userRecords = new List<User2>();
            foreach (UserData user in users)
            {
                if (user.login != sender_login) {
                    if (user.login.ToLower().Contains(line.ToLower())) {
                        User2 userRecord = new User2();
                        userRecord.recordloginTextBlock.Text = user.login;
                        userRecord.sender_login = sender_login;
                        try
                        {
                            if (await FileExists(user.image))
                                userRecord.recordImage.ImageSource = new BitmapImage(new Uri(user.image));
                        }
                        catch { }
                        userRecords.Add(userRecord);
                    }
                } 
            }
            usersChatListView.ItemsSource = userRecords;
        }
        private async Task<bool> FileExists(string url) //асинхронный
        {
            try
            {
                HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
                request.Method = "HEAD";
                using (HttpWebResponse response = await request.GetResponseAsync() as HttpWebResponse)
                {
                    return response?.StatusCode == HttpStatusCode.OK;
                }
            }
            catch
            {
                return false;
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

        private void usersChatListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (usersChatListView.SelectedItem != null)
            {
                User2 selectedItem = (User2)usersChatListView.SelectedItem; 
                 
                selectedItem.messageButton_Click(sender, e);
            }
        }

        private void searchTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            { 
                if (!string.IsNullOrEmpty(searchTextBox.Text.Trim()))
                {
                    LoadUsers(searchTextBox.Text.Trim());
                }
                else LoadUsers("");
            }
        }
    }
}
