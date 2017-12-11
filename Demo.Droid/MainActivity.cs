using Android.App;
using Android.Widget;
using Android.OS;
using System.Net.Http;
using ModernHttpClient;
using System;
using System.Diagnostics;

namespace Demo.Droid
{
    [Activity(Label = "Demo.Droid", MainLauncher = true)]
    public class MainActivity : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);
        }

        protected override async void OnResume()
        {
            base.OnResume();

            var client = new HttpClient(new NativeMessageHandler() { EnableUntrustedCertificates = true, Timeout = new TimeSpan(0,0,9) });

            var timer = new Stopwatch();
            timer.Start();

            var response = await client.GetAsync(new Uri("https://self-signed.badssl.com"));

            timer.Stop();

            Console.WriteLine(timer.ElapsedMilliseconds);
        }
    }
}

