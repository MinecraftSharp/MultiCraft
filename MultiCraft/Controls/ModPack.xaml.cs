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

namespace MultiCraft.Controls
{
    /// <summary>
    /// Interaction logic for ModPack.xaml
    /// </summary>
    public partial class ModPack
    {
        public ModPack(string name, string discription, Uri image, string url, string serverURL, string website, string support)
        {
            InitializeComponent();
            //Load all content
            ModpackName.Content = $"{name} ";
            ModpackDescription.Text = discription;
            if (image == null)
                image = new Uri("pack://application:,,,/SamplePic/StopCraftSamplePic.png");
            ModpackImage.Source = new BitmapImage(image);
            //Save download URL's
            DownloadURL = url;
            ServerDownloadURL = serverURL;
            //Save website URL's
            Website = website;
            Support = support;
        }

        public string DownloadURL { get; }

        public string ServerDownloadURL { get; }

        public string Website { get; }

        public string Support { get; }
    }
}
