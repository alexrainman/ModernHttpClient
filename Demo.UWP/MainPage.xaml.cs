using ModernHttpClient.UWP;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
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

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Demo.UWP
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            var cookieHandler = new NativeCookieHandler();

            var client = new HttpClient(new NativeMessageHandler(false, false, cookieHandler) { EnableUntrustedCertificates = true, Timeout = new TimeSpan(0, 0, 9), UseCookies = false });

            var timer = new Stopwatch();
            timer.Start();

            // SetCookie before making the call
            // It will be loaded from the store and merged into the Cookie header in the native request
            var cookie = new Cookie("cookie1", "value1", "/", "self-signed.badssl.com");
            cookieHandler.SetCookie(cookie);

            client.DefaultRequestHeaders.Add("Cookie", "cookie2=value2");

            var response = await client.GetAsync(new Uri("https://self-signed.badssl.com"));

            timer.Stop();

            Console.WriteLine(response);
        }
    }
}
