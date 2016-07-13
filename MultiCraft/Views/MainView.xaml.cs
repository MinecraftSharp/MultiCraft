using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using Framework.UI.Input;
using MultiCraft.Account;
using MultiCraft.Minecraft;

namespace MultiCraft.Views
{
    /// <summary>
    /// Interaction logic for MainView.xaml
    /// </summary>
    public partial class MainView
    {
        /// <summary>
        /// Holds the view <seealso cref="SettingsPage"/>
        /// </summary>
        private readonly ContentControl _settingsPage;

        public MainView(ContentControl settingsPage)
        {
            InitializeComponent();
            //Makes it so that we can clear the username using one button
            Username.Command = Clear;

            //Set up the settings page
            _settingsPage = settingsPage;
            //We refer this so that way we can create and remove the blur effect
            _settingsPage.Content = new SettingsPage(this).Content;
            //Hide the settings page
            _settingsPage.Visibility = Visibility.Hidden;
            //Make sure there is no blur effect 
            MainGrid.Effect = null;
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            //Handles the watermark
            PasswordLabel.Visibility = Password.Password.Length > 0 ? Visibility.Hidden : Visibility.Visible;
            if (Password.Password.Length == 0)
                PasswordLabel.Visibility = Visibility.Hidden;
        }

        private void Password_Focus(object sender, RoutedEventArgs e)
        {
            //Handles the watermark
            if (Password.IsFocused)
                PasswordLabel.Visibility = Visibility.Hidden;
            else if (Password.Password.Length > 0 && Password.Password.Length != 0)
                PasswordLabel.Visibility = Visibility.Hidden;
            else PasswordLabel.Visibility = Visibility.Visible;
        }

        private void AddAccount_OnClick(object sender, RoutedEventArgs e)
        {
            //Simply switches these grids
            AccountGrid.Visibility = Visibility.Hidden;
            LoginGrid.Visibility = Visibility.Visible;
        }

        private void Login_OnClick(object sender, RoutedEventArgs e)
        {
            //login
            var login = new MinecraftLogin();
            var result = Auth.Login(Username.Text, Password.Password, ref login);
            if (result != MinecraftAuth.Success)
            {
                ErrorMessage.Document.Blocks.Clear();
                switch (result)
                {
                    case MinecraftAuth.MigratedAccount:
                        Info.Content = "Migrated Account";
                        ErrorMessage.AppendText("Your account is migrated. Please use your email.");
                        break;
                    case MinecraftAuth.NotPremium:
                        Info.Content = "Get Premium";
                        ErrorMessage.AppendText("You did not buy Minecraft. Please buy Minecraft at https://minecraft.net/en/store/minecraft/.");
                        break;
                    case MinecraftAuth.SSLError:
                        Info.Content = "Security Error";
                        ErrorMessage.AppendText("We were not able to connect to Mojang's authentication servers securely. Please try again later.");
                        break;
                    case MinecraftAuth.ServiceUnavailable:
                        Info.Content = "Service Unavailable";
                        ErrorMessage.AppendText("There was a problem connecting to the login service. Please check https://help.mojang.com/ to see if the login server is running.");
                        break;
                    case MinecraftAuth.Empty:
                        Info.Content = "Error";
                        ErrorMessage.AppendText("Something went wrong when you tried to login. Please try again.");
                        break;
                    case MinecraftAuth.WrongPass:
                        Info.Content = "Wrong Password";
                        ErrorMessage.AppendText("Your username or password is incorrect. Please check them again.");
                        break;
                    case MinecraftAuth.UnknownError:
                        Info.Content = "Unknown Error";
                        ErrorMessage.AppendText("Sorry, eddy5641 is a noob and has no idea what went wrong. Please try again and file a bug report.");
                        break;
                    case MinecraftAuth.StreamError:
                        Info.Content = "Stream Error";
                        ErrorMessage.AppendText("Something went wrong when trying to authenticate your account. Please try again.");
                        break;
                }
                DoShowInfoAnimation();
                //Remove the password
                Password.Password = string.Empty;
                return;
            }
            
            //This is a terrible method of doing things, the GUID isn't even stored -.-
            if (RememberUser.IsChecked == true)
            {
                if (RememberPassword.IsChecked == null)
                    RememberPassword.IsChecked = false;
                AccountStore.AddAccount(Username.Text, Password.Password, Guid.NewGuid().ToString(),
                    (bool) RememberPassword.IsChecked);
            }

            //Remove the username and password
            Password.Password = string.Empty;
            Username.Text = string.Empty;

            //Swap grid visibility
            AccountGrid.Visibility = Visibility.Visible;
            LoginGrid.Visibility = Visibility.Hidden;
        }

        public async Task DoShowInfoAnimation()
        {
            //100, -55, 100, 0
            var moveAnimation = new ThicknessAnimation(new Thickness(100, 10, 100, 0), TimeSpan.FromMilliseconds(500));
            MoveGrid.BeginAnimation(MarginProperty, moveAnimation);
            await Task.Delay(10000);
            moveAnimation = new ThicknessAnimation(new Thickness(100, -55, 100, 0), TimeSpan.FromMilliseconds(500));
            MoveGrid.BeginAnimation(MarginProperty, moveAnimation);
        }

        //Handles clearing the text
        public DelegateCommand Clear => new DelegateCommand(ClearText);

        public void ClearText()
        {
            Username.Text = string.Empty;
        }

        private void SettingsButton_OnClick(object sender, RoutedEventArgs e)
        {
            //Creates a blur effect
            MainGrid.Effect = new BlurEffect();

            //Shows the settings page
            _settingsPage.Visibility = Visibility.Visible;
        }
    }
}
