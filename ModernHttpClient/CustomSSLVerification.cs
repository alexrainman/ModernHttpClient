using System.Collections.Generic;

namespace ModernHttpClient
{
    public class CustomSSLVerification
    {
        public List<Pin> Pins { get; set; }
        public ClientCertificate ClientCertificate { get; set; }
    }

    public class Pin
    {
        public string Hostname { get; set; }
        public string[] PublicKeys { get; set; }
    }

    public class ClientCertificate
    {
        public string Passphrase { get; set; }
        public string RawData { get; set; }
    }
}
