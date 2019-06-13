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
        readonly HttpClient client = new HttpClient(new NativeMessageHandler(false, new SSLConfig()
        /*{
            Pins = new List<Pin>()
            {
                new Pin()
                {
                    Hostname = "gorest.co.in",
                    PublicKeys = new []
                    {
                        "sha256/MCBrX+0kgfNc/qacknAJ5nojbFIx7kBSJSmXKjJviIg=",
                        "sha256/YLh1dUR9y6Kja30RrAn7JKnbQG/uEtLMkBgFF2Fuihg=",
                        "sha256/Vjs8r4z+80wjNcr1YKepWQboSIRi63WsWXhIMN+eWys="
                    }
                }
            }
        }*/)
        {
            DisableCaching = true,
            EnableUntrustedCertificates = false,
            Timeout = new TimeSpan(0, 0, 9)
        });

        public async Task Get()
        {
            var response = await client.GetAsync(new Uri("https://self-signed.badssl.com")); //https://gorest.co.in/public-api/users?_format=json&access-token=ZsjrVYhueqIMDxIUtMVxFJpecrfqiL3kLY37

            Debug.WriteLine(response.Content);
        }
    }
}
