using System;
using System.Windows;
using MultiCraft.ModPackHelpers.AtLauncher;
using MultiCraft.Views;
using System.Threading.Tasks;
using System.Threading;
using log4net.Config;

namespace MultiCraft
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(MainWindow));

        public MainWindow()
        {
            XmlConfigurator.Configure();
            //Idk why this doesn't work but it is here for ui
            Application.Current.Resources.Add(new ResourceDictionary { Source = new Uri("pack://application:,,,/MaterialDesignColors;component/Themes/Recommended/Accent/MaterialDesignColor.Blue.xaml") }, null);
            Application.Current.Resources.Add(new ResourceDictionary { Source = new Uri("pack://application:,,,/MaterialDesignThemes.Wpf;component/Themes/MaterialDesignTheme.Defaults.xaml") }, null);
            Application.Current.Resources.Add(new ResourceDictionary { Source = new Uri("pack://application:,,,/MaterialDesignThemes.Wpf;component/Themes/MaterialDesignTheme.Light.xaml") }, null);
            //Prepare for modpacks
            var x = ATLauncherServers.DownloadAllFiles();
            Task.WaitAll(x);
            log.Debug("All launcher files installed");
            InitializeComponent();
            //This allows me to set up views without another class just to hold the settings page
            MainView.Content = new MainView(SettingsPage).Content;
            log.Debug("Switching to MainView");

            //I am crazy
            ThreadPool.SetMinThreads(50, 50);
            ThreadPool.SetMaxThreads(1000, 1000);
            log.Debug("Set ThreadPool");
        }

        private void MenuItemLabelHide_Click(object sender, RoutedEventArgs e)
        {
            if (Visibility == Visibility.Hidden || Visibility == Visibility.Collapsed)
            {
                MenuItemLabelHide.Header = "Hide";
                Visibility = Visibility.Visible;
                Show();
                ShowInTaskbar = true;
                log.Debug("Window shown");
            }
            else
            {
                MenuItemLabelHide.Header = "Show";
                Visibility = Visibility.Hidden;
                Hide();
                ShowInTaskbar = false;
                log.Debug("Window hidden");
            }
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            Close();
            log.Debug("User closed window though menu");
            Environment.Exit(0);
        }
    }
}
