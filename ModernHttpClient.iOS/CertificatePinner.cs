using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace ModernHttpClient
{
    public class CertificatePinner
    {
        private readonly Dictionary<string, string[]> Pins;

        public CertificatePinner()
        {
            Pins = new Dictionary<string, string[]>();
        }

        public bool HasPins(string hostname)
        {
            return Pins.ContainsKey(hostname);
        }

        public void AddPins(string hostname, string[] pins)
        {
            Pins[hostname] = pins;
        }

        public bool Check(string hostname, byte[] certificate)
        {
            if (!Pins.ContainsKey(hostname))
            {
                Debug.WriteLine($"No certificate pin found for {hostname}");
                return false;
            }

            // Get pins
            string[] pins = Pins[hostname];

            // Compute spki fingerprint
            var spkiFingerprint = SpkiFingerprint.Compute(certificate);

            // Check pin
            var match = Array.IndexOf(pins, spkiFingerprint) > -1;

            if (match)
            {
                Debug.WriteLine($"Certificate pin is ok for {hostname}");
            }
            else
            {
                Debug.WriteLine($"Certificate pinning failure! Peer certificate chain: {spkiFingerprint}, Pinned certificates for {hostname}: {string.Join("|", pins)}");
            }

            return match;
        }
    }
}
