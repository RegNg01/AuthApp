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
using Microsoft.Win32;

namespace WpfApp1
{
    /// <summary>
    /// Логика взаимодействия для UserWindow.xaml
    /// </summary>
    public partial class UserWindow : Window
    {
        FirebaseAuth auth;
        public string birthday;
        public UserWindow()
        {
            InitializeComponent();
            auth = new FirebaseAuth();
            
        }
        public string login;

        public async Task AddHistory(string name)
        {
            
            History history = new History()
            {
                username = name,
                time = DateTime.UtcNow.ToString(),
            };
            await auth.client.PushAsync("login_history/", history);
        }
        public string GetAge(int birthyear) {
            DateTime current = DateTime.Today;
            int year = current.Year;
              
            int age = year - birthyear; 
            string line="";
            switch (age % 10)
            {
                case 1:
                    line = " год";
                    break;
                case 2:
                case 3:
                case 4:
                    line = " года";
                    break;
                default:
                    line = " лет";
                    break;
            } 
                return age.ToString()+line; 
            
            
        }
        public async void LoadUser()
        {
            FirebaseResponse response = await auth.client.GetAsync("users/" + login); //исправить
            UserData userData = response.ResultAs<UserData>();
            userloginTextBlock.Text = userData.login;
            userbirthdayTextBlock.Text = userData.birthday;
            userbirthplaceTextBlock.Text = userData.birthplace;
            userbioTextBox.Text = userData.bio;
            userstudyTextBlock.Text = userData.study;
            try
            {
                if ( await FileExists(userData.image))
                userImage.ImageSource = new BitmapImage(new Uri(userData.image));
            }
            catch 
            { 
            }
        }
        //public async void LoadUser(string log)
        //{
        //    FirebaseResponse response = await auth.client.GetAsync("users/" + log);  
        //    UserData userData = response.ResultAs<UserData>();
        //    userloginTextBlock.Text = userData.login;
        //    userbirthdayTextBlock.Text = userData.birthday;
        //    userbirthplaceTextBlock.Text = userData.birthplace;
        //    userbioTextBox.Text = userData.bio;
        //    userstudyTextBlock.Text = userData.study;
        //    try
        //    {
        //        if ( await FileExists(userData.image))
        //            userImage.ImageSource = new BitmapImage(new Uri(userData.image));
        //    }
        //    catch
        //    {
        //    }
        //}
        private async void userEditButton_Click(object sender, RoutedEventArgs e)
        { 
            UserEdit userEdit = new UserEdit();
            FirebaseResponse response = await auth.client.GetAsync("users/" + userloginTextBlock.Text);
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
            //userEdit.regloginTextBox.Visibility = Visibility.Hidden;
            userEdit.addButton.Visibility = Visibility.Hidden;
            userEdit.saveButton.Visibility = Visibility.Visible;
            userEdit.ShowDialog();

            
        }

        private void userExitButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
            this.Close();
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

        private async void Expander_Collapsed(object sender, RoutedEventArgs e)
        {
            await auth.client.SetAsync("users/" + userloginTextBlock.Text + "/bio", userbioTextBox.Text);
        }

        private async void userbioTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                await auth.client.SetAsync("users/" + userloginTextBlock.Text + "/bio", userbioTextBox.Text);
            }
        }
         

        private void chatButton_Click(object sender, RoutedEventArgs e)
        {
            ChatList chatList = new ChatList();
            chatList.sender_login = userloginTextBlock.Text;
            chatList.LoadUsers("");
            chatList.ShowDialog();
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


        private void minimizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private async void imageEditButton_Click(object sender, RoutedEventArgs e)
        {
            FirebaseResponse response = await auth.client.GetAsync("users/" + login); //исправить
            UserData userData = response.ResultAs<UserData>();
            string oldImageUrl = userData.image;
            string imageURL = oldImageUrl;
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = "Выбрать изображение";
            openFileDialog.Filter = "Файлы изображений (*.png, *.jpg)|*.png;*.jpg|Все файлы (*.*)|*.*\"";
            if (openFileDialog.ShowDialog().Value)
            {
                string ImagePath = openFileDialog.FileName;
                userImage.ImageSource = new BitmapImage(new Uri(ImagePath));
                if (ImagePath != "")
                {
                    FirebaseStorage storage = new FirebaseStorage("authapp-60a49.appspot.com");
                    try
                    {
                        string oldImageName = System.IO.Path.GetFileName(new Uri(oldImageUrl).LocalPath);
                        await storage.Child(oldImageName).DeleteAsync();
                    }
                    catch { }
                    try
                    {

                        string imageName = $@"{userloginTextBlock.Text + DateTime.Now.Ticks}.jpg";
                        MemoryStream ms = new MemoryStream(File.ReadAllBytes(ImagePath));
                         imageURL = await storage.Child(imageName).PutAsync(ms);
                    }
                    catch { }
                }

                await auth.client.SetAsync("users/" + userData.login+"/image",imageURL);
            }
        }
    }
}
