using Android.App;
using Android.OS;
using Android.Widget;

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

            var vm = new MainViewModel();

            Button button = FindViewById<Button>(Resource.Id.myButton);

            button.Click += async delegate {

                // The easiest way to pin a host is turn on pinning with a broken configuration and read the expected configuration when the connection fails.
                // Be sure to do this on a trusted network, and without man -in-the - middle tools like Charles or Fiddler.

                /*var hostname = "gorest.co.in";

                var certificatePinner = new Square.OkHttp3.CertificatePinner.Builder()
                    .Add(hostname, "sha256/AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA=")
                    .Build();

                var client = new OkHttpClient.Builder()
                    .CertificatePinner(certificatePinner)
                    .Build();

                var request = new Request.Builder()
                    .Url("https://" + hostname)
                    .Build();

                var call = client.NewCall(request);

                var response = await call.ExecuteAsync();*/

                // As expected, this fails with a certificate pinning exception:

                /*Certificate pinning failure!
                  Peer certificate chain:
                    sha256/CZEvkurQ3diX6pndH4Z5/dUNzK1Gm6+n8Hdx/DQgjO0=: CN=sni96286.cloudflaressl.com,OU=PositiveSSL Multi-Domain,OU=Domain Control Validated
                    sha256/x9SZw6TwIqfmvrLZ/kz1o0Ossjmn728BnBKpUFqGNVM=: CN=COMODO ECC Domain Validation Secure Server CA 2,O=COMODO CA Limited,L=Salford,ST=Greater Manchester,C=GB
                    sha256/58qRu/uxh4gFezqAcERupSkRYBlBAvfcw7mEjGPLnNU=: CN=COMODO ECC Certification Authority,O=COMODO CA Limited,L=Salford,ST=Greater Manchester,C=GB
                  Pinned certificates for reqres.in:
                    sha256/AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA=*/

                // Follow up by pasting the public key hashes from the exception into the NativeMessageHandler certificate pinner's configuration.

                await vm.Get();
            };
        }
    }
}

