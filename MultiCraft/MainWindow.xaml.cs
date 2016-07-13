using System;
using System.Windows;
using MultiCraft.ModPackHelpers.AtLauncher;
using MultiCraft.Views;

namespace MultiCraft
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {
            Application.Current.Resources.Add(new ResourceDictionary { Source = new Uri("pack://application:,,,/MaterialDesignColors;component/Themes/Recommended/Accent/MaterialDesignColor.Blue.xaml") }, null);
            Application.Current.Resources.Add(new ResourceDictionary { Source = new Uri("pack://application:,,,/MaterialDesignThemes.Wpf;component/Themes/MaterialDesignTheme.Defaults.xaml") }, null);
            Application.Current.Resources.Add(new ResourceDictionary { Source = new Uri("pack://application:,,,/MaterialDesignThemes.Wpf;component/Themes/MaterialDesignTheme.Light.xaml") }, null);
            InitializeComponent();
            //This allows me to set up views without another class just to hold the settings page
            MainView.Content = new MainView(SettingsPage).Content;
        }
    }
}
