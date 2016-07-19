using System;
using System.Windows.Media.Imaging;

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
