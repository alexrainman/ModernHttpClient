using System;
using System.Net;
using System.Net.Http;

namespace ModernHttpClient
{
    //public delegate void ProgressDelegate(long bytes, long totalBytes, long totalBytesExpected);

    public class NativeMessageHandler : HttpClientHandler
    {
        const string wrongVersion = "You're referencing the Portable version in your App - you need to reference the platform (iOS/Android/Windows) version";

        /// <summary>
        /// Initializes a new instance of the <see
        /// cref="ModernHttpClient.NativeMessageHandler"/> class.
        /// </summary>
        public NativeMessageHandler() : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see
        /// cref="ModernHttpClient.NativeMessageHandler"/> class.
        /// </summary>
        /// <param name="throwOnCaptiveNetwork">If set to <c>true</c> throw on
        /// captive network (ie: a captive network is usually a wifi network
        /// where an authentication html form is shown instead of the real
        /// content).</param>
        /// <param name="sSLConfig">To enable TLS Mutual Authentication</param>
        /// <param name="cookieHandler">Enable native cookie handling.
        /// </param>
        public NativeMessageHandler(bool throwOnCaptiveNetwork, TLSConfig tLSConfig, NativeCookieHandler cookieHandler = null, IWebProxy proxy = null) : base()
        {
        }

        public bool DisableCaching { get; set; }

        public TimeSpan? Timeout
        {
            get { throw new Exception(wrongVersion); }
            set { throw new Exception(wrongVersion); }
        }

        public void RegisterForProgress(HttpRequestMessage request, ProgressDelegate callback)
        {
            throw new Exception(wrongVersion);
        }
    }
}
