using System;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ModernHttpClient.UWP
{
    public class NativeMessageHandler : HttpClientHandler
    {
        readonly bool throwOnCaptiveNetwork;
        readonly bool customSSLVerification;

        public bool DisableCaching { get; set; }
        public TimeSpan? Timeout { get; set; }
        public bool EnableUntrustedCertificates { get; set; }

        public NativeMessageHandler() : this(false, false) { }

        public NativeMessageHandler(bool throwOnCaptiveNetwork, bool customSSLVerification, NativeCookieHandler cookieHandler = null)
        {
            this.throwOnCaptiveNetwork = throwOnCaptiveNetwork;
            this.customSSLVerification = customSSLVerification;

            if (cookieHandler != null)
            {
                this.CookieContainer = cookieHandler;
            }
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // Enable Untrusted Certificates
            if (this.EnableUntrustedCertificates)
            {
                this.ServerCertificateCustomValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
            }

            // Set Timeout
            var source = new CancellationTokenSource(Timeout.Value);
            cancellationToken = source.Token;

            // Disable caching
            if (this.DisableCaching)
            {
                request.Headers.CacheControl.NoCache = true;
                request.Headers.CacheControl.NoStore = true;
            }

            // Add Cookie Header if any cookie for the domain in the cookie store
            var stringBuilder = new StringBuilder();

            var cookies = ((NativeCookieHandler)this.CookieContainer).Cookies;

            if (cookies != null)
            {
                foreach (var cookie in cookies)
                {
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

            var response = base.SendAsync(request, cancellationToken);

            // throwOnCaptiveNetwork
            if (this.throwOnCaptiveNetwork && request.RequestUri.Host != response.Result.RequestMessage.RequestUri.Host)
            {
                throw new CaptiveNetworkException(request.RequestUri, response.Result.RequestMessage.RequestUri);
            }

            return response;
        }
    }
}
