using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Security.Cryptography.Certificates;
using Windows.Web.Http.Filters;

namespace ModernHttpClient
{
    public class NativeMessageHandler : HttpClientHandler
    {
        readonly bool throwOnCaptiveNetwork;

        public bool DisableCaching { get; set; }
        public bool EnableUntrustedCertificates { get; set; }
        public TimeSpan? Timeout { get; set; }

        private readonly CertificatePinner CertificatePinner;

        public NativeMessageHandler() : this(false, new SSLConfig()) { }

        public NativeMessageHandler(bool throwOnCaptiveNetwork, SSLConfig sSLConfig, NativeCookieHandler cookieHandler = null, IWebProxy proxy = null)
        {
            this.throwOnCaptiveNetwork = throwOnCaptiveNetwork;

            // Enforce TLS1.2
            SslProtocols = SslProtocols.Tls12;

            // Add Certificate Pins
            if (sSLConfig.Pins != null &&
                sSLConfig.Pins.Count > 0 &&
                sSLConfig.Pins[0].PublicKeys.Count() > 0 &&
                sSLConfig.Pins[0].PublicKeys[0].StartsWith("sha256/", StringComparison.Ordinal))
            {
                this.CertificatePinner = new CertificatePinner();

                foreach (var pin in sSLConfig.Pins)
                {
                    this.CertificatePinner.AddPins(pin.Hostname, pin.PublicKeys);
                }
            }

            // Set client credentials
            SetClientCertificate(sSLConfig.ClientCertificate);

            if (cookieHandler != null)
            {
                this.CookieContainer = cookieHandler;
            }

            // Adding proxy support
            if (proxy != null)
            {
                Proxy = proxy;
                UseProxy = true;
            }

            this.ServerCertificateCustomValidationCallback = (request, root, chain, errors) =>
            {
                if (EnableUntrustedCertificates)
                {
                    goto sslErrorVerify;
                }

                // Build certificate chain and check for errors
                if (chain == null || chain.ChainElements.Count == 0)
                {//no cert at all
                    errors = SslPolicyErrors.RemoteCertificateNotAvailable;
                    goto sslErrorVerify;
                }

                if (chain.ChainElements.Count == 1)
                {//no root?
                    errors = SslPolicyErrors.RemoteCertificateChainErrors;
                    goto sslErrorVerify;
                }

                if (!chain.Build(root))
                {
                    errors = SslPolicyErrors.RemoteCertificateChainErrors;
                    goto sslErrorVerify;
                }

                if (this.CertificatePinner != null)
                {
                    var hostname = request.RequestUri.Host;

                    if (!this.CertificatePinner.HasPins(hostname))
                    {
                        errors = SslPolicyErrors.RemoteCertificateNameMismatch;
                        goto sslErrorVerify;
                    }

                    if (!this.CertificatePinner.Check(hostname, root.RawData))
                    {
                        errors = SslPolicyErrors.RemoteCertificateNameMismatch;
                    }
                }

            sslErrorVerify:
                return errors == SslPolicyErrors.None;
            };
        }

        private async void SetClientCertificate(ClientCertificate certificate)
        {
            if (certificate == null) return;

            this.ClientCertificateOptions = ClientCertificateOption.Automatic;

            await CertificateEnrollmentManager.ImportPfxDataAsync(certificate.RawData,
                certificate.Passphrase, // the password is blank, but you can specify one here
                ExportOption.NotExportable, // there is no reason to keep the certificate Exportable
                KeyProtectionLevel.NoConsent, // whether any consent is required
                InstallOptions.DeleteExpired, // no installation options
                Package.Current.DisplayName);
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // Disable caching
            if (this.DisableCaching)
            {
                var cache = new CacheControlHeaderValue();
                cache.NoCache = true;
                cache.NoStore = true;
                request.Headers.CacheControl = cache;
            }

            // Add Cookie Header if any cookie for the domain in the cookie store
            var stringBuilder = new StringBuilder();

            // check if CookieContainer is NativeCookieHandler
            var nativeCookieHandler = this.CookieContainer as NativeCookieHandler;

            if (nativeCookieHandler != null)
            {
                var cookies = nativeCookieHandler.Cookies;

                if (cookies != null)
                {
                    foreach (var cookie in cookies)
                    {
                        if (cookie != null)
                            stringBuilder.Append(cookie.Name + "=" + cookie.Value + ";");
                    }
                }

                var headers = request.Headers;

                foreach (var h in headers)
                {
                    if (h.Key == "Cookie")
                    {
                        foreach (var val in h.Value)
                            stringBuilder.Append(val + ";");
                    }
                }

                if (stringBuilder.Length > 0)
                    request.Headers.Set("Cookie", stringBuilder.ToString().TrimEnd(';'));
            }

            // Set Timeout
            if (this.Timeout != null)
            {
                var source = new CancellationTokenSource(Timeout.Value);
                cancellationToken = source.Token;
            }

            var response = await base.SendAsync(request, cancellationToken);

            // throwOnCaptiveNetwork
            if (this.throwOnCaptiveNetwork && request.RequestUri.Host != response.RequestMessage.RequestUri.Host)
            {
                throw new CaptiveNetworkException(request.RequestUri, response.RequestMessage.RequestUri);
            }

            return response;
        }
    }
}