using System.Windows;

namespace MultiCraft.Views
{
    /// <summary>
    /// Interaction logic for SettingsPage.xaml
    /// </summary>
    public partial class SettingsPage
    {
        private readonly MainView _view;
        public SettingsPage(MainView view)
        {
            InitializeComponent();
            //Saved so we can remove blur effect later
            _view = view;
        }

        private void HideButton_OnClick(object sender, RoutedEventArgs e)
        {
            var mainWindow = Application.Current.MainWindow as MainWindow;
            if (mainWindow != null)
                mainWindow.SettingsPage.Visibility = Visibility.Hidden;
            //Remove the blur effect
            _view.MainGrid.Effect = null;
        }
    }
}
