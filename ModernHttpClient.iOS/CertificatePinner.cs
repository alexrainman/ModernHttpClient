using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;

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
            foreach(var pin in Pins)
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
        }

        public bool Check(string hostname, List<X509Certificate2> peerCertificates)
        {
            if (!HasPins(hostname))
            {
                Debug.WriteLine($"No certificate pin found for {hostname}");
                return false;
            }

            hostname = Pins.FirstOrDefault(p => Utility.MatchHostnameToPattern(hostname, p.Key)).Key;

            // Get pins
            string[] pins = Pins[hostname];

            // Skip pinning with empty array
            if (pins == null || pins.Length == 0)
            {
                return true;
            }

            foreach(var certificate in peerCertificates)
            {
                // Compute sha256
                var sha256Fingerprint = SpkiFingerprint.ComputeSHA256(certificate.RawData);

                // Check pins for sha256
                if (Array.IndexOf(pins, sha256Fingerprint) > -1)
                {
                    Debug.WriteLine($"Certificate pin {sha256Fingerprint} is ok for {hostname}");
                    return true;
                }

                // Compute sha1
                var sha1Fingerprint = SpkiFingerprint.ComputeSHA1(certificate.RawData);

                // Check pins for sha1
                if (Array.IndexOf(pins, sha1Fingerprint) > -1)
                {
                    Debug.WriteLine($"Certificate pin {sha1Fingerprint} is ok for {hostname}");
                    return true;
                }
            }

            Debug.WriteLine($"Certificate pinning failure! Peer certificate chain for {hostname}: {string.Join("|", pins)}");
            return false;
        }
    }
}
