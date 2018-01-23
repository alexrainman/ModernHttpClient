using Android.App;
using Android.Widget;
using Android.OS;
using System.Net.Http;
using ModernHttpClient;
using System;
using System.Diagnostics;
using System.Net;

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

            var cookieHandler = new NativeCookieHandler();

            var client = new HttpClient(new NativeMessageHandler(false, false, cookieHandler) { EnableUntrustedCertificates = true, Timeout = new TimeSpan(0,0,9) });

            var timer = new Stopwatch();
            timer.Start();

            // SetCookie before making the call
            // It will be loaded by OkHttp3.CookieJar.LoadForRequest and merged into the Cookie header in the native request
            var cookie = new Cookie("cookie1", "value1", "/", "self-signed.badssl.com");
            cookieHandler.SetCookie(cookie);

            client.DefaultRequestHeaders.Add("Cookie", "cookie2=value2");

            var response = await client.GetAsync(new Uri("https://self-signed.badssl.com"));

            timer.Stop();

            cookieHandler.DeleteCookies();

            Console.WriteLine(timer.ElapsedMilliseconds);
        }
    }
}

