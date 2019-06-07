using System.Collections.Generic;

namespace ModernHttpClient
{
    public class SSLConfig
    {
        public SSLConfig(){}

        public List<Pin> Pins { get; set; }
        public ClientCertificate ClientCertificate { get; set; }
    }

    public class Pin
    {
        public Pin(){}

        public string Hostname { get; set; }
        public string[] PublicKeys { get; set; }
    }

    public class ClientCertificate
    {
        public ClientCertificate(){}

        public string Passphrase { get; set; }
        public string RawData { get; set; }
    }
}
