using Android.OS;
using Java.IO;
using Java.Security;
using Java.Util.Concurrent;
using Javax.Net.Ssl;
using Square.OkHttp3;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace ModernHttpClient
{
    public class NativeMessageHandler : HttpClientHandler
    {
        OkHttpClient client = new OkHttpClient();
        readonly CacheControl noCacheCacheControl = new CacheControl.Builder().NoCache().Build();
        readonly bool throwOnCaptiveNetwork;

        readonly Dictionary<HttpRequestMessage, WeakReference> registeredProgressCallbacks =
            new Dictionary<HttpRequestMessage, WeakReference>();
        readonly Dictionary<string, string> headerSeparators =
            new Dictionary<string, string>(){
                {"User-Agent", " "}
            };

        public bool DisableCaching { get; set; }
        public TimeSpan? Timeout { get; set; }

        private readonly CertificatePinner.Builder CertificatePinnerBuilder;

        private IKeyManager[] KeyManagers;

        public NativeMessageHandler() : this(false, new CustomSSLVerification()) { }

        public NativeMessageHandler(bool throwOnCaptiveNetwork, CustomSSLVerification customSSLVerification, NativeCookieHandler cookieHandler = null)
        {
            this.throwOnCaptiveNetwork = throwOnCaptiveNetwork;

            var clientBuilder = client.NewBuilder();

            var specsBuilder = new ConnectionSpec.Builder(ConnectionSpec.ModernTls).TlsVersions(TlsVersion.Tls12);
            var specs = specsBuilder.Build();

            clientBuilder.ConnectionSpecs(new List<ConnectionSpec>() { specs });
            clientBuilder.Protocols(new[] { Protocol.Http11 }); // Required to avoid stream was reset: PROTOCOL_ERROR 

            clientBuilder.HostnameVerifier(new HostnameVerifier(customSSLVerification.Pins));

            this.CertificatePinnerBuilder = new CertificatePinner.Builder();

            // Add Certificate Pins
            foreach (var pin in customSSLVerification.Pins)
            {
                this.CertificatePinnerBuilder.Add(pin.Hostname, pin.PublicKeys);
            }

            clientBuilder.CertificatePinner(CertificatePinnerBuilder.Build());

            // Set client credentials
            SetClientCertificate(customSSLVerification.ClientCertificate);

            // Set SslSocketFactory
            if (Build.VERSION.SdkInt < BuildVersionCodes.Lollipop)
            {
                // Support TLS1.2 on Android versions before Lollipop
                clientBuilder.SslSocketFactory(new TlsSslSocketFactory(KeyManagers, null), TlsSslSocketFactory.GetSystemDefaultTrustManager());
            }
            else
            {
                var sslContext = SSLContext.GetInstance("TLS");
                sslContext.Init(KeyManagers, null, null);
                clientBuilder.SslSocketFactory(sslContext.SocketFactory, TlsSslSocketFactory.GetSystemDefaultTrustManager());
            }

            if (cookieHandler != null) clientBuilder.CookieJar(cookieHandler);

            client = clientBuilder.Build();
        }

        private void SetClientCertificate(ClientCertificate certificate)
        {
            if (certificate == null) return;

            var bytes = Convert.FromBase64String(certificate.RawData);

            var stream = new System.IO.MemoryStream(bytes);
            var keyStore = KeyStore.GetInstance("PKCS12");
            keyStore.Load(stream, certificate.Passphrase.ToCharArray());

            var kmf = KeyManagerFactory.GetInstance("X509");
            kmf.Init(keyStore, certificate.Passphrase.ToCharArray());

            KeyManagers = kmf.GetKeyManagers();
        }

        public void RegisterForProgress(HttpRequestMessage request, ProgressDelegate callback)
        {
            if (callback == null && registeredProgressCallbacks.ContainsKey(request))
            {
                registeredProgressCallbacks.Remove(request);
                return;
            }

            registeredProgressCallbacks[request] = new WeakReference(callback);
        }

        ProgressDelegate getAndRemoveCallbackFromRegister(HttpRequestMessage request)
        {
            ProgressDelegate emptyDelegate = delegate { };

            lock (registeredProgressCallbacks)
            {
                if (!registeredProgressCallbacks.ContainsKey(request)) return emptyDelegate;

                var weakRef = registeredProgressCallbacks[request];
                if (weakRef == null) return emptyDelegate;

                var callback = weakRef.Target as ProgressDelegate;
                if (callback == null) return emptyDelegate;

                registeredProgressCallbacks.Remove(request);
                return callback;
            }
        }

        string getHeaderSeparator(string name)
        {
            if (headerSeparators.ContainsKey(name))
            {
                return headerSeparators[name];
            }

            return ",";
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var clientBuilder = client.NewBuilder();

            if (Timeout != null)
            {
                var timeout = (long)Timeout.Value.TotalMilliseconds;
                clientBuilder.ConnectTimeout(timeout, TimeUnit.Milliseconds);
                clientBuilder.WriteTimeout(timeout, TimeUnit.Milliseconds);
                clientBuilder.ReadTimeout(timeout, TimeUnit.Milliseconds);
            }

            client = clientBuilder.Build();

            var java_uri = request.RequestUri.GetComponents(UriComponents.AbsoluteUri, UriFormat.UriEscaped);
            var url = new Java.Net.URL(java_uri);

            var body = default(RequestBody);
            if (request.Content != null)
            {
                var bytes = await request.Content.ReadAsByteArrayAsync().ConfigureAwait(false);

                var contentType = "text/plain";
                if (request.Content.Headers.ContentType != null)
                {
                    contentType = String.Join(" ", request.Content.Headers.GetValues("Content-Type"));
                }
                body = RequestBody.Create(MediaType.Parse(contentType), bytes);
            }

            var requestBuilder = new Request.Builder()
                .Method(request.Method.Method.ToUpperInvariant(), body)
                .Url(url);

            if (DisableCaching)
            {
                requestBuilder.CacheControl(noCacheCacheControl);
            }

            var keyValuePairs = request.Headers
                .Union(request.Content != null ?
                    request.Content.Headers :
                    Enumerable.Empty<KeyValuePair<string, IEnumerable<string>>>());
            
            // Add Cookie Header if there's any cookie for the domain in the cookie jar
            var stringBuilder = new StringBuilder();

            if (client.CookieJar() != null)
            {
                var jar = client.CookieJar();
                var cookies = jar.LoadForRequest(HttpUrl.Get(url));
                foreach (var cookie in cookies)
                {
                    stringBuilder.Append(cookie.Name() + "=" + cookie.Value() + ";");
                }
            }
                
            foreach (var kvp in keyValuePairs)
            {
                if (kvp.Key == "Cookie")
                {
                    foreach (var val in kvp.Value)
                        stringBuilder.Append(val + ";");
                }
                else
                {
                    requestBuilder.AddHeader(kvp.Key, String.Join(getHeaderSeparator(kvp.Key), kvp.Value));
                }
            }

            if (stringBuilder.Length > 0)
            {
                requestBuilder.AddHeader("Cookie", stringBuilder.ToString().TrimEnd(';'));
            }

            cancellationToken.ThrowIfCancellationRequested();

            var rq = requestBuilder.Build();
            var call = client.NewCall(rq);

            // NB: Even closing a socket must be done off the UI thread. Cray!
            cancellationToken.Register(() => Task.Run(() => call.Cancel()));

            var resp = default(Response);
            try
            {
                resp = await call.EnqueueAsync().ConfigureAwait(false);
                var newReq = resp.Request();
                var newUri = newReq == null ? null : newReq.Url().Uri();
                request.RequestUri = new Uri(newUri.ToString());
                if (throwOnCaptiveNetwork && newUri != null)
                {
                    if (url.Host != newUri.Host)
                    {
                        throw new CaptiveNetworkException(new Uri(java_uri), new Uri(newUri.ToString()));
                    }
                }
            }
            catch (IOException ex)
            {
                if (ex.Message.ToLowerInvariant().Contains("canceled"))
                {
                    throw new System.OperationCanceledException();
                }

                // Calling HttpClient methods should throw .Net Exception when fail #5
                throw new HttpRequestException(ex.Message, ex);
            }

            var respBody = resp.Body();

            cancellationToken.ThrowIfCancellationRequested();

            var ret = new HttpResponseMessage((HttpStatusCode)resp.Code());
            ret.RequestMessage = request;
            ret.ReasonPhrase = resp.Message();

            // ReasonPhrase is empty under HTTPS #8
            if (string.IsNullOrEmpty(ret.ReasonPhrase))
            {
                try
                {
                    ret.ReasonPhrase = ((ReasonPhrases)resp.Code()).ToString().Replace('_', ' ');
                }
#pragma warning disable 0168
                catch (Exception ex)
                {
                    ret.ReasonPhrase = "Unassigned";
                }
#pragma warning restore 0168
            }

            if (respBody != null)
            {
                var content = new ProgressStreamContent(respBody.ByteStream(), CancellationToken.None);
                content.Progress = getAndRemoveCallbackFromRegister(request);
                ret.Content = content;
            }
            else
            {
                ret.Content = new ByteArrayContent(new byte[0]);
            }

            var respHeaders = resp.Headers();
            foreach (var k in respHeaders.Names())
            {
                ret.Headers.TryAddWithoutValidation(k, respHeaders.Get(k));
                ret.Content.Headers.TryAddWithoutValidation(k, respHeaders.Get(k));
            }

            return ret;
        }
    }

    public static class AwaitableOkHttp
    {
        public static Task<Response> EnqueueAsync(this ICall This)
        {
            var cb = new OkTaskCallback();
            This.Enqueue(cb);

            return cb.Task;
        }

        class OkTaskCallback : Java.Lang.Object, ICallback
        {
            readonly TaskCompletionSource<Response> tcs = new TaskCompletionSource<Response>();
            public Task<Response> Task { get { return tcs.Task; } }

            public void OnFailure(ICall p0, IOException p1)
            {
                // Kind of a hack, but the simplest way to find out that server cert. validation failed
                if (p1.Message.StartsWith("Hostname " + p0.Request().Url().Host() + " not verified", StringComparison.Ordinal))
                {
                    // SIGABRT after UnknownHostException #229
                    //tcs.TrySetException(new WebException(p1.Message));
                    //tcs.TrySetException(new WebException(p1.LocalizedMessage, WebExceptionStatus.TrustFailure));
                    var ex = new System.OperationCanceledException(HostnameVerifier.PinningFailureMessage, p1);
                    HostnameVerifier.PinningFailureMessage = null;
                    tcs.TrySetException(ex);
                }
                else if (p1.Message.StartsWith("Certificate pinning failure", StringComparison.Ordinal))
                {
                    System.Diagnostics.Debug.WriteLine(p1.Message);
                    tcs.TrySetException(new System.OperationCanceledException(FailureMessages.PinMismatch, p1));
                }
                else
                {
                    tcs.TrySetException(p1);
                }
            }

            public void OnResponse(ICall p0, Response p1)
            {
                tcs.TrySetResult(p1);
            }
        }
    }

    class HostnameVerifier : Java.Lang.Object, IHostnameVerifier
    {
        static readonly Regex cnRegex = new Regex(@"CN\s*=\s*([^,]*)", RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.Singleline);

        public static string PinningFailureMessage = null;

        private readonly List<Pin> Pins;

        public HostnameVerifier(List<Pin> pins)
        {
            this.Pins = pins;
        }

        /// <summary>
        /// Verifies the server certificate by calling into ServicePointManager.ServerCertificateValidationCallback or,
        /// if the is no delegate attached to it by using the default hostname verifier.
        /// </summary>
        /// <returns><c>true</c>, if server certificate was verifyed, <c>false</c> otherwise.</returns>
        /// <param name="hostname"></param>
        /// <param name="session"></param>
        public bool Verify(string hostname, ISSLSession session)
        {
            var errors = SslPolicyErrors.None;

            // Convert java certificates to .NET certificates and build cert chain from root certificate
            /*var serverCertChain = session.GetPeerCertificateChain();
            var chain = new X509Chain();
            X509Certificate2 root = null;
            var errors = SslPolicyErrors.None;

            // Build certificate chain and check for errors
            if (serverCertChain == null || serverCertChain.Length == 0)
            {//no cert at all
                errors = SslPolicyErrors.RemoteCertificateNotAvailable;
                PinningFailureMessage = FailureMessages.NoCertAtAll;
                goto sslErrorVerify;
            }

            if (serverCertChain.Length == 1)
            {//no root?
                errors = SslPolicyErrors.RemoteCertificateChainErrors;
                PinningFailureMessage = FailureMessages.NoRoot;
                goto sslErrorVerify;
            }

            var netCerts = serverCertChain.Select(x => new X509Certificate2(x.GetEncoded())).ToArray();

            for (int i = 1; i < netCerts.Length; i++)
            {
                chain.ChainPolicy.ExtraStore.Add(netCerts[i]);
            }

            root = netCerts[0];

            chain.ChainPolicy.RevocationFlag = X509RevocationFlag.EntireChain;
            chain.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck;
            chain.ChainPolicy.UrlRetrievalTimeout = new TimeSpan(0, 1, 0);
            chain.ChainPolicy.VerificationFlags = X509VerificationFlags.AllowUnknownCertificateAuthority;

            if (!chain.Build(root))
            {
                errors = SslPolicyErrors.RemoteCertificateChainErrors;
                PinningFailureMessage = FailureMessages.ChainError;
                goto sslErrorVerify;
            }

            var subject = root.Subject;
            var subjectCn = cnRegex.Match(subject).Groups[1].Value;

            if (string.IsNullOrWhiteSpace(subjectCn) || !Utility.MatchHostnameToPattern(hostname, subjectCn))
            {
                errors = SslPolicyErrors.RemoteCertificateNameMismatch;
                PinningFailureMessage = FailureMessages.SubjectNameMismatch;
                goto sslErrorVerify;
            }*/

            if (Pins.FirstOrDefault((pin) => pin.Hostname == hostname) == null)
            {
                errors = SslPolicyErrors.RemoteCertificateNameMismatch;
                PinningFailureMessage = FailureMessages.NoPinsProvided + " " + hostname;
            }

        //sslErrorVerify:
            return errors == SslPolicyErrors.None;
        }
    }
}