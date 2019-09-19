using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Security.Cryptography.Certificates;

namespace ModernHttpClient
{
    public class NativeMessageHandler : HttpClientHandler
    {
        readonly bool throwOnCaptiveNetwork;

        public bool DisableCaching { get; set; }
       
        public TimeSpan? Timeout { get; set; }

        private readonly CertificatePinner CertificatePinner;

        public readonly TLSConfig TLSConfig;

		public readonly string PinningMode = "CertificateOnly";

		public NativeMessageHandler() : this(false, new TLSConfig()) { }

        static readonly Regex cnRegex = new Regex(@"CN\s*=\s*([^,]*)", RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.Singleline);

        public NativeMessageHandler(bool throwOnCaptiveNetwork, TLSConfig tLSConfig, NativeCookieHandler cookieHandler = null, IWebProxy proxy = null)
        {
            this.throwOnCaptiveNetwork = throwOnCaptiveNetwork;

            this.TLSConfig = tLSConfig;

            // Enforce TLS1.2
            SslProtocols = SslProtocols.Tls12;

            // Add Certificate Pins
            if (!TLSConfig.DangerousAcceptAnyServerCertificateValidator && 
                TLSConfig.Pins != null &&
                TLSConfig.Pins.Count > 0 &&
                TLSConfig.Pins.FirstOrDefault(p => p.PublicKeys.Count() > 0) != null)
            {
				this.PinningMode = "PublicKeysOnly";

				this.CertificatePinner = new CertificatePinner();

                foreach (var pin in TLSConfig.Pins)
                {
                    this.CertificatePinner.AddPins(pin.Hostname, pin.PublicKeys);
                }
            }

            // Set client credentials
            SetClientCertificate(TLSConfig.ClientCertificate);

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

            this.ServerCertificateCustomValidationCallback = (request, root, chain, e) =>
            {
                var errors = SslPolicyErrors.None;

                if (TLSConfig.DangerousAcceptAnyServerCertificateValidator)
                {
                    goto sslErrorVerify;
                }

                var hostname = request.RequestUri.Host;

                var netCerts = new List<X509Certificate2>();

                foreach(var element in chain.ChainElements)
                {
                    netCerts.Add(element.Certificate);
                }

                switch (this.PinningMode)
				{
					case "CertificateOnly":

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

						var subject = root.Subject;
						var subjectCn = cnRegex.Match(subject).Groups[1].Value;

						if (string.IsNullOrWhiteSpace(subjectCn) || !Utility.MatchHostnameToPattern(hostname, subjectCn))
						{
							var subjectAn = root.ParseSubjectAlternativeName();

							if (subjectAn.FirstOrDefault(s => Utility.MatchHostnameToPattern(hostname, s)) == null)
							{
								errors = SslPolicyErrors.RemoteCertificateNameMismatch;
								goto sslErrorVerify;
							}
						}
						break;
					case "PublicKeysOnly":

						if (this.CertificatePinner != null)
						{
							if (!this.CertificatePinner.HasPins(hostname))
							{
								errors = SslPolicyErrors.RemoteCertificateNameMismatch;
								goto sslErrorVerify;
							}

							if (!this.CertificatePinner.Check(hostname, netCerts))
							{
								errors = SslPolicyErrors.RemoteCertificateNameMismatch;
							}
						}
						break;
				}

            sslErrorVerify:
                return errors == SslPolicyErrors.None;
            };
        }

        private async void SetClientCertificate(ClientCertificate certificate)
        {
            if (certificate == null) return;

            try
            {
                var bytes = Convert.FromBase64String(certificate.RawData);
            }
            catch (Exception ex)
            {
                throw new HttpRequestException(FailureMessages.InvalidRawData, ex);
            }

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
				var cookies = nativeCookieHandler.Cookies
												 .Where(c => c.Domain == request.RequestUri.Host)
												 .Where(c => Utility.PathMatches(request.RequestUri.AbsolutePath, c.Path))
								                 .ToList(); ;

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