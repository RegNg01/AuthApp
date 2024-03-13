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
using Firebase.Storage;
using System.IO;
using System.Net;
using Microsoft.Win32;

namespace WpfApp1
{
    /// <summary>
    /// Логика взаимодействия для AdminWindow.xaml
    /// </summary>
    public partial class AdminWindow : Window
    {
        FirebaseAuth auth;
        public AdminWindow()
        {
            InitializeComponent();
            auth = new FirebaseAuth();
            LoadUsers("");
        }
        
        public async void LoadUsers(string line)  
        {
           
            List<UserData> users = new List<UserData>();
            FirebaseResponse response =  await auth.client.GetAsync("users/");
            Dictionary<string,UserData> result = response.ResultAs<Dictionary<string, UserData>>(); 
            if (result!=null)
            users = result.Values.ToList();
            List<User> userRecords = new List<User>();

            foreach (UserData user in users)
            {

                if (user.role != "admin")
                {
                    
                        if (user.login.ToLower().Contains(line.ToLower()) ||
                       user.birthday.ToLower().Contains(line.ToLower()) ||
                       user.birthplace.ToLower().Contains(line.ToLower()) ||
                       user.study.ToLower().Contains(line.ToLower()))
                        {
                            User userRecord = new User();
                            userRecord.recordloginTextBlock.Text = user.login;
                            userRecord.recordbirthdayTextBlock.Text = user.birthday;
                            userRecord.recordbirthplaceTextBlock.Text = user.birthplace;
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
            UsersListView.ItemsSource = userRecords;
        }

        private void adminExitButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
            this.Close();
        }

        //private void UsersListView_MouseDoubleClick(object sender, MouseButtonEventArgs e) https://copyprogramming.com/howto/c-listview-how-to-handle-the-mouse-click-event-on-a-listviewitem
        //{
        //    ListViewItem item = UsersListView.SelectedItem as ListViewItem;
        //    MessageBox.Show(item.Name);
        //}
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

        private async void historyButton_Click(object sender, RoutedEventArgs e)
        { 
                List<History> history = new List<History>();
                FirebaseResponse response = await auth.client.GetAsync("login_history/");
                Dictionary<string, History> result = response.ResultAs<Dictionary<string, History>>();
                if (result != null)
                history = result.Values.ToList();
               
                HistoryWindow historyWindow = new HistoryWindow();
                historyWindow.historyListView.ItemsSource = history;
                historyWindow.Show();
             
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

        private async void UsersListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //if (UsersListView.SelectedItem != null)
            //{
            //    UserWindow userWindow = new UserWindow(); 
            //    User selectedItem = (User)UsersListView.SelectedItem;
            //    FirebaseResponse response = await auth.client.GetAsync("users/" + selectedItem.recordloginTextBlock.Text);
            //    UserData userData = response.ResultAs<UserData>();

            //    userWindow.userloginTextBlock.Text = userData.login;
            //    userWindow.userbirthdayTextBlock.Text = userData.birthday;
            //    userWindow.userbirthplaceTextBlock.Text = userData.birthplace;
            //    userWindow.userbioTextBox.Text = userData.bio;
            //    userWindow.userstudyTextBlock.Text = userData.study;
            //    try
            //    {
            //        if (FileExists(userData.image))
            //            userWindow.userImage.ImageSource = new BitmapImage(new Uri(userData.image));
            //    }
            //    catch
            //    {
            //        MessageBox.Show("ошибка");
            //    }
            //    userWindow.birthday = userData.birthday.ToString();
            //    userWindow.login = userData.login;
            //    userWindow.userageTextBlock.Text = userWindow.GetAge(Convert.ToInt32(userData.birthday.ToString().Substring(6)));
            //    userWindow.chatButton.Visibility = Visibility.Hidden;
            //    userWindow.userExitButton.Visibility = Visibility.Hidden;   
            //    userWindow.imageEditButton.Visibility = Visibility.Hidden;
            //    userWindow.Show();
                
            //}
        }

        private async void imageEditButton_Click(object sender, RoutedEventArgs e)
        {

            FirebaseResponse response = await auth.client.GetAsync("users/" + adminloginTextBlock.Text);  
            UserData userData = response.ResultAs<UserData>();
            string oldImageUrl = userData.image;
            string imageURL = oldImageUrl;
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = "Выбрать изображение";
            openFileDialog.Filter = "Файлы изображений (*.png, *.jpg)|*.png;*.jpg|Все файлы (*.*)|*.*\"";
            if (openFileDialog.ShowDialog().Value)
            {
                string ImagePath = openFileDialog.FileName;
                adminImage.ImageSource = new BitmapImage(new Uri(ImagePath));
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

                        string imageName = $@"{adminloginTextBlock.Text + DateTime.Now.Ticks}.jpg";
                        MemoryStream ms = new MemoryStream(File.ReadAllBytes(ImagePath));
                        imageURL = await storage.Child(imageName).PutAsync(ms);
                    }
                    catch { }
                }

                await auth.client.SetAsync("users/" + userData.login + "/image", imageURL);
            }
        }

        private void loginEditButton_Click(object sender, RoutedEventArgs e)
        {
            loginEditButton.Visibility = Visibility.Hidden;
            loginTextBox.Visibility = Visibility.Visible;
            passwordTextBox.Visibility = Visibility.Visible;
            loginSaveButton.Visibility = Visibility.Visible; 
        }

        private async void loginSaveButton_Click(object sender, RoutedEventArgs e)
        {
            loginSaveButton.IsEnabled = false;
            if (loginTextBox.Text.Trim() == ""
                || passwordTextBox.Text.Trim() == "")
            {
                MessageBox.Show("Заполните все поля");
                loginSaveButton.IsEnabled = true;
                return;
            }
            if (!isValid(loginTextBox.Text.Trim()))
            {
                MessageBox.Show("Логин не должен иметь символы: «@», «*», «+», «-», «/», «#» и подобные");
                loginSaveButton.IsEnabled = true;
                return;
            }
            FirebaseResponse user = await auth.client.GetAsync("users/" +loginTextBox.Text.Trim().ToLower());
            UserData userData1 = user.ResultAs<UserData>();
            if (userData1 != null && loginTextBox.Text.Trim() != adminloginTextBlock.Text)
            {
                MessageBox.Show("Пользователь с таким логином уже существует.");
                loginSaveButton.IsEnabled=true;
                return;
            }
            if (loginTextBox.Text.Trim() != adminloginTextBlock.Text) 
                {
                FirebaseResponse response = await auth.client.GetAsync("users/" + adminloginTextBlock.Text.Trim());
                UserData userData = response.ResultAs<UserData>();
                userData.login = loginTextBox.Text.Trim();
                userData.password = passwordTextBox.Text.Trim();
                await auth.client.DeleteAsync("users/" + adminloginTextBlock.Text);
                await auth.client.SetAsync("users/" + loginTextBox.Text,userData);
                }
            else
            {
                await auth.client.SetAsync("users/"+loginTextBox.Text+"/login", loginTextBox.Text.Trim());
                await auth.client.SetAsync("users/"+loginTextBox.Text+"/password", passwordTextBox.Text.Trim());
            }
            loginTextBox.Visibility = Visibility.Hidden;
            passwordTextBox.Visibility = Visibility.Hidden;
            loginEditButton.Visibility = Visibility.Visible;
            loginSaveButton.Visibility = Visibility.Hidden;
            adminloginTextBlock.Text = loginTextBox.Text;
            loginSaveButton.IsEnabled = true;
        }
        private bool isValid(string str)
        {
            str = str.ToLower();
            foreach (char ch in str)
            {
                int l = Convert.ToInt32(ch);
                if (!((l >= 1072 && l <= 1103) ||
                    (l >= 97 && l <= 122) ||
                    (l >= 48 && l <= 57)))
                {
                    return false;
                }
            }
            return true;
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
