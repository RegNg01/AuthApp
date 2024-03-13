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

namespace WpfApp1
{
    /// <summary>
    /// Логика взаимодействия для User.xaml
    /// </summary>
    public partial class User : UserControl
    {
        FirebaseAuth auth;
        public User()
        {
            InitializeComponent();
            auth = new FirebaseAuth();
        }

        public async void messageUserButton_Click(object sender, RoutedEventArgs e)
        {
            ChatWindow chatWindow = new ChatWindow();
            chatWindow.reciever_login = recordloginTextBlock.Text;
            AdminWindow adminWindow = Application.Current.Windows.OfType<AdminWindow>().FirstOrDefault();
            chatWindow.sender_login = adminWindow.adminloginTextBlock.Text;
            chatWindow.LoadMessages();
            chatWindow.userLabel.Content = recordloginTextBlock.Text;
            chatWindow.Show();
        }

        public async void editUserButton_Click(object sender, RoutedEventArgs e)
        {
            UserEdit userEdit = new UserEdit();
            FirebaseResponse response = await auth.client.GetAsync("users/" + recordloginTextBlock.Text);
            UserData userData = response.ResultAs<UserData>();
            userEdit.loginLabel.Content = userData.login;
            userEdit.regloginTextBox.Text = userData.login;
            userEdit.regpasswordTextBox.Text = userData.password;
            userEdit.regbirthdayDatePicker.Text = userData.birthday;
            userEdit.regbirthplaceTextBox.Text = userData.birthplace;
            userEdit.regstudyTextBox.Text = userData.study;
            userEdit.regbioTextBox.Text = userData.bio;
            try
            {
                if (await FileExists(userData.image))
                    userEdit.regimage.ImageSource = new BitmapImage(new Uri(userData.image));
                userEdit.oldImageUrl = userData.image;
            }
            catch
            {

            }
            userEdit.loginLabel.Visibility = Visibility.Hidden; 
            userEdit.addButton.Visibility = Visibility.Hidden;
            userEdit.saveButton.Visibility = Visibility.Visible;

            userEdit.ShowDialog();
        }

        public async void deleteUserButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Удален "+recordloginTextBlock.Text);
           
            FirebaseResponse response = await auth.client.GetAsync("users/" + recordloginTextBlock.Text); https://stackoverflow.com/questions/49476592/check-if-wpf-window-is-open
            UserData userData = response.ResultAs<UserData>();
            FirebaseStorage storage = new FirebaseStorage("authapp-60a49.appspot.com");
            try
            {
                string oldImageName = System.IO.Path.GetFileName(new Uri(userData.image).LocalPath);
                await storage.Child(oldImageName).DeleteAsync();
            }
            catch { }
            await auth.client.DeleteAsync("users/" + recordloginTextBlock.Text);
            AdminWindow adminWindow = Application.Current.Windows.OfType<AdminWindow>().FirstOrDefault();
            adminWindow.LoadUsers(""); // ошибка

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

        private async void openUserButton_Click(object sender, RoutedEventArgs e)
        {
            UserWindow userWindow = new UserWindow();
            FirebaseResponse response = await auth.client.GetAsync("users/" + recordloginTextBlock.Text);
            UserData userData = response.ResultAs<UserData>();
            userWindow.userloginTextBlock.Text = userData.login;
            userWindow.userbirthdayTextBlock.Text = userData.birthday;
            userWindow.userbirthplaceTextBlock.Text = userData.birthplace;
            userWindow.userbioTextBox.Text = userData.bio;
            userWindow.userstudyTextBlock.Text = userData.study;
            try
            {
                if (await FileExists(userData.image))
                    userWindow.userImage.ImageSource = new BitmapImage(new Uri(userData.image));
            }
            catch
            {
                MessageBox.Show("ошибка");
            }
            userWindow.birthday = userData.birthday.ToString();
            userWindow.login = userData.login;
            userWindow.userageTextBlock.Text = userWindow.GetAge(Convert.ToInt32(userData.birthday.ToString().Substring(6)));
            userWindow.chatButton.Visibility = Visibility.Hidden;
            userWindow.userExitButton.Visibility = Visibility.Hidden;
            userWindow.imageEditButton.Visibility = Visibility.Hidden;
            userWindow.bioExpander.IsExpanded = true;
            userWindow.Show();
        }
    }
}
