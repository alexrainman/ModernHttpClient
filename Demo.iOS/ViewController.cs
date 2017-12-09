using System;
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

            var client = new HttpClient(new NativeMessageHandler() { EnableUntrustedCertificates = true });

            var response = await client.GetAsync(new Uri("https://self-signed.badssl.com"));

            Console.WriteLine(response);
        }

        public override void DidReceiveMemoryWarning()
        {
            base.DidReceiveMemoryWarning();
            // Release any cached data, images, etc that aren't in use.
        }
    }
}
