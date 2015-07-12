using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace WebLEDController
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Splash : Page
    {

        HttpServer.HttpServer server = null;
        readonly Api.Leds LedProcessor = new Api.Leds();

        public string Url { get; set; }


        public Splash()
        {
            this.InitializeComponent();

            var ApiProcessor = new Api.Processor();

            foreach (var route in LedProcessor.Routes)
            {
                ApiProcessor.Routes.Add(route);
            }

            server = new HttpServer.HttpServer(8000, ApiProcessor.process, @"\site\");

            getUrls("8000");
            server.StartServer();
        }

        void getUrls(string port)
        {
            var ipAddresses = new System.Text.StringBuilder();
            ipAddresses.AppendLine("Connect to:");
            var hostnames = Windows.Networking.Connectivity.NetworkInformation.GetHostNames();
            foreach (var hn in hostnames)
            {
                //IanaInterfaceType == 71 => Wifi
                //IanaInterfaceType == 6 => Ethernet (Emulator)
                if (hn.IPInformation != null &&
                   (hn.IPInformation.NetworkAdapter.IanaInterfaceType == 71
                   || hn.IPInformation.NetworkAdapter.IanaInterfaceType == 6))
                {
                    string ipAddress = hn.DisplayName;
                    ipAddresses.AppendLine("http://" + ipAddress + ":" + port);
                }
            }
            textBlock.Text = ipAddresses.ToString();
        }

        void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            // Cleanup
            LedProcessor.DisposeLeds();

            server.Dispose();
        }
    }
}
