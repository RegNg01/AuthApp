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
using Microsoft.Win32;
using System.IO;
using Firebase.Storage;
using System.Net;

namespace WpfApp1
{
    /// <summary>
    /// Логика взаимодействия для UserEdit.xaml
    /// </summary>
    public partial class UserEdit : Window
    {
        public string oldImageUrl="";
        public string ImagePath;
        // https://github.com/ziyasal/FireSharp https://www.youtube.com/watch?v=_jB2iOgo_9Q&list=PLuv4McUMmyAbsNdfZ4rH_OyEM3P2MJS-F&index=2
        FirebaseAuth auth;
        public UserEdit()
        {
            InitializeComponent();
            auth = new FirebaseAuth();
        }

        private async void addButton_Click(object sender, RoutedEventArgs e)
        {

            string imageURL="";
            addButton.IsEnabled = false;
            try
            {
                if (regloginTextBox.Text.Trim() == ""
                || regpasswordTextBox.Text.Trim() == ""
                || regbirthplaceTextBox.Text.Trim() == ""
                || regbirthdayDatePicker.Text.Trim() == ""
                || regstudyTextBox.Text.Trim() == ""
                || regbioTextBox.Text.Trim() == "")
                {
                    MessageBox.Show("Заполните все поля");
                    addButton.IsEnabled = true;
                    return;
                }
                if (!isValid(regloginTextBox.Text.Trim()))
                {
                    MessageBox.Show("Логин не должен иметь символы: «@», «*», «+», «-», «/», «#» и подобные");
                    addButton.IsEnabled = true;
                    return;
                }

                FirebaseResponse user = await auth.client.GetAsync("users/" + regloginTextBox.Text.Trim().ToLower());
                UserData userData1 = user.ResultAs<UserData>();
                if (userData1 != null && regloginTextBox.Text.Trim() != loginLabel.Content.ToString())
                {
                    MessageBox.Show("Пользователь с таким логином уже существует.");
                    addButton.IsEnabled = true;
                    return;
                }

                string imageName = $@"{regloginTextBox.Text.Trim() + DateTime.Now.Ticks}.jpg"; //https://stackoverflow.com/questions/4657974/how-to-generate-unique-file-names-in-c-sharp
                if (ImagePath != "" && ImagePath != null) try
                    {
                        FirebaseStorage storage = new FirebaseStorage("authapp-60a49.appspot.com");
                        MemoryStream ms = new MemoryStream(File.ReadAllBytes(ImagePath));
                        imageURL = await storage.Child(imageName).PutAsync(ms);
                    }
                    catch { }
                UserData userData = new UserData()
                {
                    login = regloginTextBox.Text.Trim(),
                    password = regpasswordTextBox.Text.Trim(),
                    birthday = regbirthdayDatePicker.Text.Trim(),
                    birthplace = regbirthplaceTextBox.Text.Trim(),
                    bio = regbioTextBox.Text.Trim(),
                    study = regstudyTextBox.Text.Trim(),
                    role = "user",
                    image = oldImageUrl,
                };
                try
                {
                    userData.image = imageURL;
                }
                catch
                {
                }
                await auth.client.SetAsync("users/" + regloginTextBox.Text.Trim(), userData);
                MessageBox.Show("Добавлен пользователь " + regloginTextBox.Text.Trim());
                regloginTextBox.Text = "";
                  regpasswordTextBox = "";
                 regbirthplaceTextBox = "";
                regbirthdayDatePicker.Text = "";
                regstudyTextBox.Text = "";
                  regbioTextBox.Text = "";
            } 
            catch (Exception ex) { MessageBox.Show(ex.Message); }
            addButton.IsEnabled = true;
        }

        private async void saveButton_Click(object sender, RoutedEventArgs e) 
        {
            saveButton.IsEnabled = false;
            if (regloginTextBox.Text.Trim() == ""
                || regpasswordTextBox.Text.Trim() == ""
                || regbirthplaceTextBox.Text.Trim() == ""
                || regbirthdayDatePicker.Text.Trim() == ""
                || regstudyTextBox.Text.Trim() == ""
                || regbioTextBox.Text.Trim() == "")
            {
                MessageBox.Show("Заполните все поля");
                saveButton.IsEnabled = true;
                return;
            }
            if (!isValid(regloginTextBox.Text.Trim()))
            {
                MessageBox.Show("Логин не должен иметь символы: «@», «*», «+», «-», «/», «#» и подобные");
                saveButton.IsEnabled = true;
                return;
            }
            FirebaseResponse user = await auth.client.GetAsync("users/" + regloginTextBox.Text.Trim().ToLower());
            UserData userData1 = user.ResultAs<UserData>();
            if (userData1 != null && regloginTextBox.Text.Trim() != loginLabel.Content.ToString())
            {
                MessageBox.Show("Пользователь с таким логином уже существует.");
                saveButton.IsEnabled = true;
                return;
            }
            if (regloginTextBox.Text.Trim() != loginLabel.Content)
                await auth.client.DeleteAsync("users/" + loginLabel.Content);
            string imageURL = oldImageUrl;

            if (ImagePath!=""&&ImagePath!=null)
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

                    string imageName = $@"{regloginTextBox.Text.Trim() + DateTime.Now.Ticks}.jpg";
                    MemoryStream ms = new MemoryStream(File.ReadAllBytes(ImagePath));
                    imageURL = await storage.Child(imageName).PutAsync(ms);
                }
                catch { }
            }
            UserData userData = new UserData()
            {
            
                login = regloginTextBox.Text.Trim(),
                password = regpasswordTextBox.Text.Trim(),
                birthday = regbirthdayDatePicker.Text.Trim(),
                birthplace = regbirthplaceTextBox.Text.Trim(),
                bio = regbioTextBox.Text.Trim(),
                study = regstudyTextBox.Text.Trim(),
                role = "user",
                image = imageURL,

            };
             
            await auth.client.SetAsync("users/" + userData.login, userData);
            MessageBox.Show("Изменен пользователь " + loginLabel.Content);
            saveButton.IsEnabled = true;
            
            UserWindow userWindow = Application.Current.Windows.OfType<UserWindow>().FirstOrDefault();
            if (userWindow != null)
            {
                userWindow.login = userData.login;
                userWindow.LoadUser();
            }
            AdminWindow adminWindow = Application.Current.Windows.OfType<AdminWindow>().FirstOrDefault();
            if (adminWindow != null)
            { 
                adminWindow.LoadUsers("");
            } 
            this.Close();
        }

        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void regimage_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = "Выбрать изображение";
            openFileDialog.Filter = "Файлы изображений (*.png, *.jpg)|*.png;*.jpg|Все файлы (*.*)|*.*\"";
            if (openFileDialog.ShowDialog().Value)
            {
                ImagePath = openFileDialog.FileName;
                regimage.ImageSource = new BitmapImage(new Uri(ImagePath));
               
            }
        }
         
        private bool isValid(string str) {
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
        private bool FileExists(string url)
        {
            try
            {

                HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
                request.Method = "HEAD";
                HttpWebResponse response1 = request.GetResponse() as HttpWebResponse;
                if (response1.StatusCode == HttpStatusCode.OK)
                    return true;
            }
            catch
            {

                return false;
            }
            return false;
        }
    }
}
