using System;
using System.Collections.Generic;
using System.Net.Http;

namespace ModernHttpClient
{
    public class CertificatePinner : Java.Lang.Object
    {
        private readonly Square.OkHttp3.CertificatePinner.Builder Builder;

        private readonly Dictionary<string, string[]> Pins;

        public CertificatePinner()
        {
            Builder = new Square.OkHttp3.CertificatePinner.Builder();
            Pins = new Dictionary<string, string[]>();
        }

        public Square.OkHttp3.CertificatePinner Build()
        {
            return Builder.Build();
        }

        public bool HasPins(string hostname)
        {
            foreach (var pin in Pins)
            {
                if (Utility.MatchHostnameToPattern(hostname, pin.Key))
                {
                    return true;
                }
            }

            return false;
        }

        public void AddPins(string hostname, string[] pins)
        {
            Utility.VerifyPins(pins);
            Pins[hostname] = pins;
            Builder.Add(hostname, pins);
        }
    }
}
