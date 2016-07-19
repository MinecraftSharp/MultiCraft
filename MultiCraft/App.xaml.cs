using System;
using System.Diagnostics;

namespace MultiCraft
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        public App()
        {
            InitializeComponent();
            var textWriterTraceListener = new TextWriterTraceListener(Console.Out);
            Debug.Listeners.Add(textWriterTraceListener);
        }
    }
}
