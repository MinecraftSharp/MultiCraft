using System;
using System.Windows;
using MultiCraft.ModPackHelpers.AtLauncher;
using MultiCraft.Views;
using System.Threading.Tasks;

namespace MultiCraft
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {
            //Idk why this doesn't work but it is here for ui
            Application.Current.Resources.Add(new ResourceDictionary { Source = new Uri("pack://application:,,,/MaterialDesignColors;component/Themes/Recommended/Accent/MaterialDesignColor.Blue.xaml") }, null);
            Application.Current.Resources.Add(new ResourceDictionary { Source = new Uri("pack://application:,,,/MaterialDesignThemes.Wpf;component/Themes/MaterialDesignTheme.Defaults.xaml") }, null);
            Application.Current.Resources.Add(new ResourceDictionary { Source = new Uri("pack://application:,,,/MaterialDesignThemes.Wpf;component/Themes/MaterialDesignTheme.Light.xaml") }, null);
            //Prepare for modpacks
            var x = ATLauncherServers.DownloadAllFiles();
            Task.WaitAll(x);
            InitializeComponent();
            //This allows me to set up views without another class just to hold the settings page
            MainView.Content = new MainView(SettingsPage).Content;
        }

    }
}
