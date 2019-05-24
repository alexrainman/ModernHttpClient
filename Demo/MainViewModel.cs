using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using ModernHttpClient;

namespace Demo
{
    public class MainViewModel
    {
readonly static HttpClient client = new HttpClient(new NativeMessageHandler(false, new CustomSSLVerification()
{
    Pins = new List<Pin>()
    {
        new Pin()
        {
            Hostname = "reqres.in",
            PublicKeys = new []
            {
                "sha256/CZEvkurQ3diX6pndH4Z5/dUNzK1Gm6+n8Hdx/DQgjO0=",
                "sha256/x9SZw6TwIqfmvrLZ/kz1o0Ossjmn728BnBKpUFqGNVM=",
                "sha256/58qRu/uxh4gFezqAcERupSkRYBlBAvfcw7mEjGPLnNU="
            }
        }
    }/*,
    ClientCertificate = new ClientCertificate()
    {
        RawData = "PFX_DATA",
        Passphrase = "PFX_PASSPHRASE"
    }*/
})
{
    DisableCaching = true,
    Timeout = new TimeSpan(0, 0, 9)
});

        public async Task Get()
        {
            var response = await client.GetAsync(new Uri("https://reqres.in/api/users"));

            Debug.WriteLine(response.Content);
        }
    }
}
