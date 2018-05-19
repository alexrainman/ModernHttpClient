using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ModernHttpClient
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

            // TODO: check if CookieContainer is NativeCookieHandler

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

            var response = base.SendAsync(request, cancellationToken);
            var result = response.GetAwaiter().GetResult();

            // throwOnCaptiveNetwork
            if (this.throwOnCaptiveNetwork && request.RequestUri.Host != result.RequestMessage.RequestUri.Host)
            {
                throw new CaptiveNetworkException(request.RequestUri, result.RequestMessage.RequestUri);
            }

            return response;
        }
    }
}
