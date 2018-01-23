using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using ModernHttpClient;
using UIKit;

namespace Demo.iOS
{
    public partial class ViewController : UIViewController
    {
        protected ViewController(IntPtr handle) : base(handle)
        {
            // Note: this .ctor should not contain any initialization logic.
        }

        public override async void ViewDidLoad()
        {
            base.ViewDidLoad();
            // Perform any additional setup after loading the view, typically from a nib.

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

        public override void DidReceiveMemoryWarning()
        {
            base.DidReceiveMemoryWarning();
            // Release any cached data, images, etc that aren't in use.
        }
    }
}
