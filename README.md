Modernizing Paul Betts ModernHttpClient
================

Available on NuGet: https://www.nuget.org/packages/modernhttpclient-updated/ [![NuGet](https://img.shields.io/nuget/v/modernhttpclient-updated.svg?label=NuGet)](https://www.nuget.org/packages/modernhttpclient-updated/)

This library brings the latest platform-specific networking libraries to
Xamarin applications via a custom HttpClient handler. Write your app using
System.Net.Http, but drop this library in and it will go securely faster.

This is made possible by:

* On iOS, [NSURLSession](https://developer.apple.com/library/ios/documentation/Foundation/Reference/NSURLSession_class/Introduction/Introduction.html)
* On Android, via [OkHttp3](http://square.github.io/okhttp/)
* On UWP, via a custom HttpClientHandler.

## How can I use this from shared code?

Just reference ModernHttpClient in your .Net Standard or Portable Library, and it will use the correct version on all platforms.

### What's new?

This release is focused on security so, the original code has been refactored to remove deprecated APIs and meet the new standards.

Support for untrusted certificates has been removed. Using untrusted certificates is considered a security flaw.

https://www.globalsign.com/en/ssl-information-center/dangers-self-signed-certificates/

You can get a valid SSL certificate for free from one of these certificate authorities:

https://www.sslforfree.com

https://letsencrypt.org/getting-started/

https://ssl.comodo.com/free-ssl-certificate.php

TLS 1.2 has been enforced.

Read why here:

https://www.brillianceweb.com/resources/answers-to-7-common-questions-about-upgrading-to-tls-1.2/

Really "modernizing" the way ModernHttpClient implements certificate pinning, using server certificate chain public keys and adding support for client certificates, for a 2-Way Certificate Pinning (or what is also called Mutual TLS Authentication).

As a result, minimumSSLProtocol static property has been removed from iOS, verifyHostnameCallback and customTrustManager static properties has been removed from Android.

### Usage

The good news is, you don't have to know either of these libraries above.

Here's how it works:

```cs
readonly static HttpClient client = new HttpClient(new NativeMessageHandler(false, new CustomSSLVerification()
{
    Pins = new List<Pin>()
    {
        new Pin()
        {
            Hostname = "reqres.in",
            PublicKeys = new []
            {
                "sha256/CZEvkurQ3diX6pndH4Z5/dUNzK1Gm6+n8Hdx/DQgjO0=",
                "sha256/x9SZw6TwIqfmvrLZ/kz1o0Ossjmn728BnBKpUFqGNVM=",
                "sha256/58qRu/uxh4gFezqAcERupSkRYBlBAvfcw7mEjGPLnNU="
            }
        }
    },
    ClientCertificate = new ClientCertificate()
    {
        RawData = "PFX_DATA",
        Passphrase = "PFX_PASSPHRASE"
    }
})
{
    DisableCaching = true,
    Timeout = new TimeSpan(0, 0, 9)
});
```

### How to obtain server certificate chain public keys?

1. In your Android project add Square.OkHttp3 Nuget Package.
2. In your Main Activity, add a button and on its click event run this code:

```cs
var hostname = "reqres.in";
var certificatePinner = new Square.OkHttp3.CertificatePinner.Builder()
    .Add(hostname, "sha256/AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA=")
    .Build();
var client = new OkHttpClient.Builder()
    .CertificatePinner(certificatePinner)
    .Build();
 
var request = new Request.Builder()
    .Url("https://" + hostname)
    .Build();
 
var call = client.NewCall(request);
var response = await call.ExecuteAsync();
```

As expected, this fails with a certificate pinning exception:

```cs
Certificate pinning failure!
  Peer certificate chain:
    sha256/CZEvkurQ3diX6pndH4Z5/dUNzK1Gm6+n8Hdx/DQgjO0=: CN=sni96286.cloudflaressl.com,OU=PositiveSSL Multi-Domain,OU=Domain Control Validated
    sha256/x9SZw6TwIqfmvrLZ/kz1o0Ossjmn728BnBKpUFqGNVM=: CN=COMODO ECC Domain Validation Secure Server CA 2,O=COMODO CA Limited,L=Salford,ST=Greater Manchester,C=GB
    sha256/58qRu/uxh4gFezqAcERupSkRYBlBAvfcw7mEjGPLnNU=: CN=COMODO ECC Certification Authority,O=COMODO CA Limited,L=Salford,ST=Greater Manchester,C=GB
  Pinned certificates for reqres.in:
    sha256/AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA=
```

3. Follow up by pasting the public key hashes from the exception into the NativeMessageHandler certificate pinner's configuration.

### How to merger your certificate and its private key into a pfx and convert it to Base64?

A certificate in pfx format can be created from client.crt, client.key and a passphrase using openssl:

```
openssl pkcs12 -export -out client.pfx -inkey client.key -in client.crt
```

A client.crt can be created following this tutorial: https://gist.github.com/mtigas/952344

The pfx certificate can be converted to Base64 using PowerShell:

```
$fileContentBytes = get-content 'path-to\client.pfx' -Encoding Byte
[System.Convert]::ToBase64String($fileContentBytes) | Out-File 'path-to\pfx-bytes.txt'
```

Or, from a terminal window on a Mac:

```
base64 -i path-to/client.pfx -o path-to/pfx-bytes.txt
```

### How to use NativeCookieHandler?

SetCookie before making the http call and they will be added to Cookie header in the native client:

```cs
NativeCookieHandler cookieHandler = new NativeCookieHandler();

readonly static HttpClient client = new HttpClient(new NativeMessageHandler(false, new CustomSSLVerification()
{
    Pins = new List<Pin>()
    {
        new Pin()
        {
            Hostname = "reqres.in",
            PublicKeys = new []
            {
                "sha256/CZEvkurQ3diX6pndH4Z5/dUNzK1Gm6+n8Hdx/DQgjO0=",
                "sha256/x9SZw6TwIqfmvrLZ/kz1o0Ossjmn728BnBKpUFqGNVM=",
                "sha256/58qRu/uxh4gFezqAcERupSkRYBlBAvfcw7mEjGPLnNU="
            }
        }
    },
    ClientCertificate = new ClientCertificate()
    {
        RawData = "PFX_DATA",
        Passphrase = "PFX_PASSPHRASE"
    }
}, cookieHandler)
{
    DisableCaching = true,
    Timeout = new TimeSpan(0, 0, 9)
});

var cookie = new Cookie("cookie1", "value1", "/", "reqres.in");
cookieHandler.SetCookie(cookie);

var response = await client.GetAsync(new Uri("https://reqres.in"));
```

## NativeCookieHandler methods

```SetCookies(IEnumerable<Cookie> cookies)```: set native cookies

```DeleteCookies()```: delete all native cookies.

```SetCookie(Cookie cookie)```: set a native cookie.

```DeleteCookie(Cookie cookie)```: delete a native cookie.

#### Release Notes

3.0.0

Code refactoring.

Focused on security.

Removing support for untrusted certificates.

Enforcing TLS1.2

Adding support for 2-way certificate pinning (Mutual TLS Authentication)

[iOS] Removing minimumSSLProtocol static property.

[Android] Removing verifyHostnameCallback static property.

[Android] Removing customTrustManager static property.

2.7.2

[Android] Handshake failed (adding customTrustManager static property) #11

2.7.1

[Android] MissingMethodException Method 'ModernHttpClient.NativeMessageHandler..ctor' not found. #9

[iOS] Removing minimumSSLProtocol from NativeMessageHandler ctor

[UWP] Exception on UWP with Xamarin Forms #3

2.7.0
      
[Update] Migrating to a multi-target project
      
[Android] Calling HttpClient methods should throw .Net Exception when fail #5
      
[Android] VerifyHostnameCallback parameter function on constructor (NativeMessageHandler - Android) when customSSLVerification is true #6
      
[Android] ReasonPhrase is empty under HTTPS #8

2.6.0

[Update] Adding support for UWP

[Update] Adding support for netstandard 2.0

2.5.3

[Update] Cookies set with the native handler will be merged into the Cookie header

2.5.1

[Android] NativeCookieHandler, if provided, is set as the default CookieJar for OkHttpClient

[Update] Adding DeleteCookies, SetCookie and DeleteCookie to NativeCookieHandler

2.5.0

[Android] Updating to Square.OkHttp3

2.4.9

[Android] Calling HttpClient methods should throw .Net Exception when fail #5

[Android] MissingMethodException Method 'ModernHttpClient.NativeMessageHandler..ctor' not found. #9

[Android] VerifyHostnameCallback parameter function on constructor when customSSLVerification is true #6

[Android] ReasonPhrase is empty under HTTPS #8

[Android] Handshake failed (adding customTrustManager static property) #11

[iOS] Removing minimumSSLProtocol from NativeMessageHandler ctor

2.4.7

[Update] Cookies set with the native handler will be merged into the Cookie header

2.4.5

[Android] NativeCookieHandler, if provided, is set as the default cookie handler for OkHttpClient

[Update] Adding DeleteCookies, SetCookie and DeleteCookie to NativeCookieHandler

2.4.4

[Android] SIGABRT after UnknownHostException #229

[iOS] Updating obsolete NSUrlSessionDelegate to INSUrlSessionDelegate

[Update] Adding EnableUntrustedCertificates to support self-signed certificates

2.4.3

[Update] Adding Timeout property

[Android] Updating to Square.OkHttp 2.7.5

[Android] Timeout value is not respected on Android #192
