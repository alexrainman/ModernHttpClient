Updating Paul Betts ModernHttpClient
================

Available on NuGet: https://www.nuget.org/packages/modernhttpclient-updated/ [![NuGet](https://img.shields.io/nuget/v/modernhttpclient-updated.svg?label=NuGet)](https://www.nuget.org/packages/modernhttpclient-updated/)

This library brings the latest platform-specific networking libraries to
Xamarin applications via a custom HttpClient handler. Write your app using
System.Net.Http, but drop this library in and it will go drastically faster.
This is made possible by two native libraries:

* On iOS, [NSURLSession](https://developer.apple.com/library/ios/documentation/Foundation/Reference/NSURLSession_class/Introduction/Introduction.html)
* On Android, via [OkHttp3](http://square.github.io/okhttp/)
* On UWP, via a custom HttpClientHandler.

## How can I use this in a PCL?

Just reference the Portable version of ModernHttpClient in your Portable Library, and it will use the correct version on all platforms.

## Usage

The good news is, you don't have to know either of these two libraries above,
using ModernHttpClient is the most boring thing in the world.

Here's how it works:

```cs
private static HttpClient httpClient = new HttpClient(new NativeMessageHandler() { Timeout = new TimeSpan(0,0,9), EnableUntrustedCertificates = true, DisableCaching = true });
```

## NativeCookieHandler methods

```SetCookies(IEnumerable<Cookie> cookies)```: set native cookies

```DeleteCookies()```: delete all native cookies.

```SetCookie(Cookie cookie)```: set a native cookie.

```DeleteCookie(Cookie cookie)```: delete a native cookie.

### How to use NativeCookieHandler?

SetCookie before making the http call so, it will be loaded by OkHttp3.CookieJar.LoadForRequest

```cs
var cookieHandler = new NativeCookieHandler();
private static HttpClient httpClient = new HttpClient(new NativeMessageHandler(false, false, cookieHandler) { Timeout = new TimeSpan(0,0,9), EnableUntrustedCertificates = true });

var cookie = new Cookie("cookie1", "value1", "/", "self-signed.badssl.com");
cookieHandler.SetCookie(cookie);

var response = await client.GetAsync(new Uri("https://self-signed.badssl.com"));
```

## Self-signed certificates

A new property named EnableUntrustedCertificates has been added to support self-signed certificates.

To make it work in iOS, add this to your info.plist:

```xml
<key>NSAppTransportSecurity</key>
<dict>
    <key>NSExceptionDomains</key>
    <dict>
        <key>your-self-signed.badssl.com</key>
        <dict>
            <key>NSExceptionAllowsInsecureHTTPLoads</key>
            <true/>
        </dict>
    </dict>
</dict>
```

#### Release Notes

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
