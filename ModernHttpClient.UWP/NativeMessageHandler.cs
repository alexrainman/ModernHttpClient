using System;
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
        public TimeSpan? Timeout { get; set; }

        private readonly CertificatePinner CertificatePinner;
		
		public NativeMessageHandler() : this(false, new CustomSSLVerification()) { }

        public NativeMessageHandler(bool throwOnCaptiveNetwork, CustomSSLVerification customSSLVerification, NativeCookieHandler cookieHandler = null)
        {
            this.throwOnCaptiveNetwork = throwOnCaptiveNetwork;

            // Enforce TLS1.2
            SslProtocols = SslProtocols.Tls12;

            this.ServerCertificateCustomValidationCallback = (sender, cert, chain, errors) =>
            {
                var hostname = sender.RequestUri.Host;

                if (!this.CertificatePinner.HasPins(hostname) || !this.CertificatePinner.Check(hostname, cert.RawData))
                {
                    errors = SslPolicyErrors.RemoteCertificateNameMismatch;
                }

                return errors == SslPolicyErrors.None;
            };

            this.CertificatePinner = new CertificatePinner();

            // Add Certificate Pins
            foreach (var pin in customSSLVerification.Pins)
            {
                this.CertificatePinner.AddPins(pin.Hostname, pin.PublicKeys);
            }

            // Set client credentials
            SetClientCertificate(customSSLVerification.ClientCertificate);

            if (cookieHandler != null)
            {
                this.CookieContainer = cookieHandler;
            }
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
            // Set Timeout
            if (this.Timeout != null)
            {
                var source = new CancellationTokenSource(Timeout.Value);
                cancellationToken = source.Token;
            }

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