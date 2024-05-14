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
using Firebase.Storage;
using System.IO;
using System.Net;
using System.Globalization;

namespace WpfApp1
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        FirebaseAuth auth;
        public MainWindow()
        {
            InitializeComponent();
            auth = new FirebaseAuth();
         
        }

        private void registerButton_Click(object sender, RoutedEventArgs e)
        {
            UserEdit userEdit = new UserEdit();
            userEdit.loginLabel.Visibility = Visibility.Hidden;
            userEdit.regloginTextBox.Visibility = Visibility.Visible;
            userEdit.ShowDialog();
        }

        private async void enterButton_Click(object sender, RoutedEventArgs e)
        {
            enterButton.IsEnabled = false;
            if (loginTextBox.Text.Trim() == ""
            || passwordBox.Password.Trim() == "")
            {
                MessageBox.Show("Заполните все поля");
                enterButton.IsEnabled = true;
                return;
            }
            try
            {
                FirebaseResponse response = await auth.client.GetAsync("users/" + loginTextBox.Text.Trim());
                UserData userData = response.ResultAs<UserData>();

                if (userData != null && userData.login == loginTextBox.Text.Trim() && userData.role == "admin" && userData.password == passwordBox.Password.Trim() && loginTextBox.Text.Trim() != "" && passwordBox.Password.Trim() != "")
                {
                    AdminWindow adminWindow = new AdminWindow();
                    adminWindow.adminloginTextBlock.Text = userData.login;
                    try
                    {

                        if (await FileExists(userData.image))
                            adminWindow.adminImage.ImageSource = new BitmapImage(new Uri(userData.image));
                    }
                    catch
                    {
                        MessageBox.Show("ошибка");
                    }
                    // MessageBox.Show("Успешный вход под логином " + userData.login);
                    adminWindow.Show();
                    this.Close();
                }
                else if (userData != null && userData.login == loginTextBox.Text.Trim() && userData.role == "user" && userData.password == passwordBox.Password.Trim() && loginTextBox.Text.Trim() != "" && passwordBox.Password.Trim() != "")
                {
                    UserWindow userWindow = new UserWindow();
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
                    await userWindow.AddHistory(userData.login);
                    userWindow.Show();
                    MessageBox.Show("Успешный вход под логином " + userData.login);
                    this.Close();
                }
                else MessageBox.Show("Неверный логин или пароль");
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
            enterButton.IsEnabled=true;
        }
        private async Task<bool> FileExists(string url) 
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
    }
}
