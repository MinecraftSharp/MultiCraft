using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using Framework.UI.Input;
using MultiCraft.Account;
using MultiCraft.Controls;
using MultiCraft.Minecraft;
using MultiCraft.ModPackHelpers.AtLauncher;
using System.Windows.Threading;
using System.Threading;
using System.Collections.Generic;
using System.Windows.Media;
using System.Linq;

namespace MultiCraft.Views
{
    /// <summary>
    /// Interaction logic for MainView.xaml
    /// </summary>
    public partial class MainView
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(MainView));

        /// <summary>
        /// Holds the view <seealso cref="SettingsPage"/>
        /// </summary>
        private readonly ContentControl _settingsPage;

        public MainView(ContentControl settingsPage)
        {
            InitializeComponent();
            //Makes it so that we can clear the username using one button
            Username.Command = ClearUsername;
            SearchText.Command = SearchClear;

            //Set up the settings page
            _settingsPage = settingsPage;
            //We refer this so that way we can create and remove the blur effect
            _settingsPage.Content = new SettingsPage(this).Content;
            //Hide the settings page
            _settingsPage.Visibility = Visibility.Hidden;
            //Make sure there is no blur effect 
            MainGrid.Effect = null;
            ModPackWebsites.SelectedItem = MultiCraft;
            log.Debug("MainView now shown");
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
            log.Debug("User adding account");
        }

        private async void Login_OnClick(object sender, RoutedEventArgs e)
        {
            //login
            var login = new MinecraftLogin();
            var result = Auth.Login(Username.Text, Password.Password, ref login);
            log.Debug("User trying to login");
            if (result != MinecraftAuth.Success)
            {
                ErrorMessage.Document.Blocks.Clear();
                switch (result)
                {
                    //Get all results from login
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
                //Cool way to show that the password was wrong
                await DoShowInfoAnimation();
                //Remove the password
                Password.Password = string.Empty;
                log.Error("User unable to login: " + result.ToString());
                return;
            }

            log.Debug("User signed in");

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
            log.Debug("Showing info - SLIDE TOP");
            //100, -55, 100, 0
            //Moves a grid from the top of screen showing basic information
            var moveAnimation = new ThicknessAnimation(new Thickness(100, 10, 100, 0), TimeSpan.FromMilliseconds(500));
            MoveGrid.BeginAnimation(MarginProperty, moveAnimation);
            await Task.Delay(10000);
            log.Debug("Hiding info - SLIDE TOP");
            moveAnimation = new ThicknessAnimation(new Thickness(100, -55, 100, 0), TimeSpan.FromMilliseconds(500));
            MoveGrid.BeginAnimation(MarginProperty, moveAnimation);
        }

        //Handles clearing the text
        public DelegateCommand ClearUsername => new DelegateCommand(ClearUsernameText);

        public void ClearUsernameText()
        {
            Username.Text = string.Empty;
        }

        public DelegateCommand SearchClear => new DelegateCommand(SearchEmpty);

        public void SearchEmpty()
        {
            SearchText.Text = string.Empty;
        }

        private void SettingsButton_OnClick(object sender, RoutedEventArgs e)
        {
            //Creates a blur effect
            MainGrid.Effect = new BlurEffect();

            //Shows the settings page
            _settingsPage.Visibility = Visibility.Visible;
        }

        //Get all of the ATLauncher modpacks
        private List<AtLauncherPacks> sortPacks = ATLauncherServers.GetPacks();

        private void ModPackWebsites_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //Ensures there are no modpacks
            ModPackListing.Items.Clear();
            if (Equals(ModPackWebsites.SelectedItem, MultiCraft))
            {
                log.Debug("Modpack selected: MultiCraft");
                PlayCurrent.Content = "Play Modpack";
                BackgroundImage.Stretch = Stretch.UniformToFill;
                //Adds a non-existant test modpack
                Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
                {
                    ModPackListing.Items.Add(new ModPack("StopCraft", "This is a test",
                        new Uri("pack://application:,,,/SamplePic/StopCraftSamplePic.png"),
                        null, null, null, null));
                }));
            }
            else if (Equals(ModPackWebsites.SelectedItem, ATLauncher))
            {
                log.Debug("Modpack selected: ATLauncher");
                //Makes it so you can't change the pack, a small bug happens if you change the pack while still loading
                ModPackWebsites.IsEnabled = false;
                //Changes the play modpack
                PlayCurrent.Content = "Play Modpack";
                BackgroundImage.Stretch = Stretch.Uniform;
                //Sort them the way ATLauncher wants them to be displayed
                sortPacks.Sort((c1, c2) => c1.Position.CompareTo(c2.Position));

                //There a a lot of modpacks, loading them this way should be faster
                Parallel.ForEach(sortPacks, (pack) =>
                {
                    Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
                    {
                        //I don't care if it is public or private, all should be shown for users
                        ModPackListing.Items.Add(new ModPack(pack.Name, pack.Description,
                            ATLauncherServers.GetImageFromPackName(pack.Name), pack.WebsiteURL,
                            pack.WebsiteURL, pack.WebsiteURL, pack.SupportURL));
                    }));
                });
                //Allow you to switch packs again
                ModPackWebsites.IsEnabled = true;
            }
            else if (Equals(ModPackWebsites.SelectedItem, FTBLauncher))
            {
                log.Debug("Modpack selected: FTB Launcher");
                PlayCurrent.Content = "Play Modpack";
                BackgroundImage.Stretch = Stretch.Uniform;

            }
            else if (Equals(ModPackWebsites.SelectedItem, TekkitLauncher))
            {
                log.Debug("Modpack selected: Tekkit Launcher");
                PlayCurrent.Content = "Play Modpack";
                BackgroundImage.Stretch = Stretch.Uniform;

            }
            else if (Equals(ModPackWebsites.SelectedItem, Minecraft))
            {
                log.Debug("Minecraft Selected");
                BackgroundImage.Stretch = Stretch.UniformToFill;
                PlayCurrent.Content = "Play Minecraft";
                Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
                {
                    //Planned for the future
                    ModPackListing.Items.Add(new ModPack("Vanilla Minecraft", "Play Minecraft the way Mojang made it. No mods are added in this version.",
                        new Uri("pack://application:,,,/SamplePic/StopCraftSamplePic.png"),
                        null, null, null, null));

                    //Planned for the future
                    ModPackListing.Items.Add(new ModPack("FPScraft", "Play Minecraft the way Mojang made it. A few mods are added such as optifine to improve performance.",
                        new Uri("pack://application:,,,/SamplePic/StopCraftSamplePic.png"),
                        null, null, null, null));
                }));
            }
        }

        private void ModPackListing_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            log.Debug("Modpack/Minecraft pack selected");
            //Hide the background by default
            //TODO: use a fade
            BackgroundImage.Visibility = Visibility.Hidden;

            if (ModPackListing.SelectedItem?.GetType() == typeof(ModPack))
            {
                BackgroundImage.Visibility = Visibility.Visible;
                var modPack = (ModPack)ModPackListing.SelectedItem;
                BackgroundImage.Source = modPack.ModpackImage.Source;
            }
        }

        private void SearchText_TextChanged(object sender, TextChangedEventArgs e)
        {
            ModPackListing.Items.Filter = (o) =>
            {
                if (((ModPack)o).ModpackName.Content.ToString().ToLower().Contains(SearchText.Text.ToLower()))
                    return true;
                else if (string.IsNullOrWhiteSpace(SearchText.Text))
                    return true;
                return false;
            };
        }
    }
}
