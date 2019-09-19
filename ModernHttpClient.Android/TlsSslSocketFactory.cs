using Android.Runtime;
using Java.Lang;
using Java.Security;
using Javax.Net.Ssl;
using Square.OkHttp3;

namespace ModernHttpClient
{
    internal class TlsSslSocketFactory : SSLSocketFactory
    {
        private readonly SSLSocketFactory _factory = (SSLSocketFactory)Default;

        public TlsSslSocketFactory(IKeyManager[] keyManagers = null, ITrustManager[] trustManagers = null)
        {
            if (keyManagers != null || trustManagers != null)
            {
                var context = SSLContext.GetInstance("TLS");
                context.Init(keyManagers, trustManagers, null);
                _factory = context.SocketFactory;
            }
        }

        public override string[] GetDefaultCipherSuites()
        {
            return _factory.GetDefaultCipherSuites();
        }

        public override string[] GetSupportedCipherSuites()
        {
            return _factory.GetSupportedCipherSuites();
        }

        public override Java.Net.Socket CreateSocket(Java.Net.InetAddress address, int port, Java.Net.InetAddress localAddress, int localPort)
        {
            var socket = (SSLSocket)_factory.CreateSocket(address, port, localAddress, localPort);
            socket.SetEnabledProtocols(socket.GetSupportedProtocols());
            socket.SetEnabledCipherSuites(socket.GetSupportedCipherSuites());

            return socket;
        }

        public override Java.Net.Socket CreateSocket(Java.Net.InetAddress host, int port)
        {
            var socket = (SSLSocket)_factory.CreateSocket(host, port);
            socket.SetEnabledProtocols(socket.GetSupportedProtocols());
            socket.SetEnabledCipherSuites(socket.GetSupportedCipherSuites());

            return socket;
        }

        public override Java.Net.Socket CreateSocket(string host, int port, Java.Net.InetAddress localHost, int localPort)
        {
            var socket = (SSLSocket)_factory.CreateSocket(host, port, localHost, localPort);
            socket.SetEnabledProtocols(socket.GetSupportedProtocols());
            socket.SetEnabledCipherSuites(socket.GetSupportedCipherSuites());

            return socket;
        }

        public override Java.Net.Socket CreateSocket(string host, int port)
        {
            var socket = (SSLSocket)_factory.CreateSocket(host, port);
            socket.SetEnabledProtocols(socket.GetSupportedProtocols());
            socket.SetEnabledCipherSuites(socket.GetSupportedCipherSuites());

            return socket;
        }

        public override Java.Net.Socket CreateSocket(Java.Net.Socket s, string host, int port, bool autoClose)
        {
            var socket = (SSLSocket)_factory.CreateSocket(s, host, port, autoClose);
            socket.SetEnabledProtocols(socket.GetSupportedProtocols());
            socket.SetEnabledCipherSuites(socket.GetSupportedCipherSuites());

            return socket;
        }

        protected override void Dispose(bool disposing)
        {
            _factory.Dispose();
            base.Dispose(disposing);
        }

        public override Java.Net.Socket CreateSocket()
        {
            var socket = (SSLSocket)_factory.CreateSocket();
            socket.SetEnabledProtocols(socket.GetSupportedProtocols());
            socket.SetEnabledCipherSuites(socket.GetSupportedCipherSuites());

            return socket;
        }

        public static IX509TrustManager GetSystemDefaultTrustManager()
        {
            IX509TrustManager x509TrustManager = null;
            try
            {
                var trustManagerFactory = TrustManagerFactory.GetInstance(TrustManagerFactory.DefaultAlgorithm);
                trustManagerFactory.Init((KeyStore)null);
                foreach (var trustManager in trustManagerFactory.GetTrustManagers())
                {
                    var manager = trustManager.JavaCast<IX509TrustManager>();
                    if (manager != null)
                    {
                        x509TrustManager = manager;
                        break;
                    }
                }
            }
            catch (Exception ex) when (ex is NoSuchAlgorithmException || ex is KeyStoreException)
            {
                // move along...
            }
            return x509TrustManager;
        }
    }
}
