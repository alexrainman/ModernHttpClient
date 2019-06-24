using System.Collections.Generic;

namespace ModernHttpClient
{
    public class TLSConfig
    {
        public TLSConfig(){}

        public List<Pin> Pins { get; set; }
        public ClientCertificate ClientCertificate { get; set; }
        public bool DangerousAcceptAnyServerCertificateValidator { get; set; }
        public bool DangerousAllowInsecureHTTPLoads { get; set; } // to match iOS NSAppTransportSecurity in Android
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
