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
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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
            //Hide the settings view
            var mainWindow = Application.Current.MainWindow as MainWindow;
            if (mainWindow != null)
                mainWindow.SettingsPage.Visibility = Visibility.Hidden;
            //Remove the blur effect
            _view.MainGrid.Effect = null;
        }
    }
}
