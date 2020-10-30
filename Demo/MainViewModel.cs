using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using ModernHttpClient;

namespace Demo
{
    public class MainViewModel
    {
        readonly HttpClient client = new HttpClient(new NativeMessageHandler(false, new TLSConfig()
        {
            Pins = new List<Pin>()
            {
                /*new Pin()
                {
                    Hostname = "*.co.in",
                    PublicKeys = new string []
                    {
                        "sha256/2h5EszuV4ZbJj5RN705JWhhz3IwFCmRuj8RW1mpu218=",
                        "sha256/YLh1dUR9y6Kja30RrAn7JKnbQG/uEtLMkBgFF2Fuihg=",
                        "sha256/Vjs8r4z+80wjNcr1YKepWQboSIRi63WsWXhIMN+eWys="
                    }
                },
                new Pin()
                {
                    Hostname = "restcountries.eu",
                    PublicKeys = new string []
                    {
                        "sha256/wxgZ6Jx5WaNt5zAgUSDnLsK8E5uy+DUumAogHk4P7R8="
                    }
                }*/
            },
            DangerousAcceptAnyServerCertificateValidator = false
        })
        {
            DisableCaching = true,
            Timeout = new TimeSpan(0, 0, 9)
        });

        public async Task Get()
        {
            var response = await client.GetAsync(new Uri("https://restcountries.eu/data/ala.svg"));

            Debug.WriteLine(response.Content);
        }
    }
}