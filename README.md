Updating Paul Betts ModernHttpClient
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

Just reference the portable version of ModernHttpClient in your .Net Standard or Portable Library, and it will use the correct version on all platforms.

## What's new?

This release is focused on security so, the original code has been refactored, removing deprecated APIs to meet the new standards.

Support for untrusted certificates has been removed. Using untrusted certificates is considered a security flaw.

https://www.globalsign.com/en/ssl-information-center/dangers-self-signed-certificates/

You can get a valid SSL certificate for free from one of these certificate authorities:

https://www.sslforfree.com

https://letsencrypt.org/getting-started/

https://ssl.comodo.com/free-ssl-certificate.php

TLS 1.2 has been enforced.

Read why here:

https://www.brillianceweb.com/resources/answers-to-7-common-questions-about-upgrading-to-tls-1.2/

Really "modernizing" the way ModernHttpClient pin server certificates and adding support for client certificates (Mutual TLS Authentication).

As a result, minimumSSLProtocol static property has been removed from iOS, verifyHostnameCallback and customTrustManager static properties has been removed from Android.

## Usage

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
        Passphrase = "PFX_PASSPHRASE",
        RawData = "PFX_DATA"
    }
})
{
    DisableCaching = true,
    Timeout = new TimeSpan(0, 0, 9)
});
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
        Passphrase = "PFX_PASSPHRASE",
        RawData = "PFX_DATA"
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
