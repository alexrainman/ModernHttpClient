using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Threading;
using System.IO;
using System.Net;

namespace ModernHttpClient
{
    //public delegate void ProgressDelegate(long bytes, long totalBytes, long totalBytesExpected);

    public class NativeMessageHandler : HttpClientHandler
    {
        const string wrongVersion = "You're referencing the Portable version in your App - you need to reference the platform (iOS/Android/Windows) version";

        public bool DisableCaching { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see
        /// cref="ModernHttpClient.NativeMessageHandler"/> class.
        /// </summary>
        public NativeMessageHandler(): base()
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
        /// <param name="customSSLVerification">Enable custom SSL certificate 
        /// verification via ServicePointManager. Disabled by default for 
        /// performance reasons (i.e. the OS default certificate verification 
        /// will take place)</param>
        /// <param name="cookieHandler">Enable native cookie handling.
        /// </param>
        public NativeMessageHandler(bool throwOnCaptiveNetwork, bool customSSLVerification, NativeCookieHandler cookieHandler = null) : base()
        {
        }

        public TimeSpan? Timeout
        {
            get { throw new Exception (wrongVersion); }
            set { throw new Exception (wrongVersion); }
        }

        public bool EnableUntrustedCertificates { get; set; }

        public void RegisterForProgress(HttpRequestMessage request, ProgressDelegate callback)
        {
            throw new Exception(wrongVersion);
        }
    }
}
