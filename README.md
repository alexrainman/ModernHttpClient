Updating Paul Betts ModernHttpClient
================

This library brings the latest platform-specific networking libraries to
Xamarin applications via a custom HttpClient handler. Write your app using
System.Net.Http, but drop this library in and it will go drastically faster.
This is made possible by two native libraries:

* On iOS, [NSURLSession](https://developer.apple.com/library/ios/documentation/Foundation/Reference/NSURLSession_class/Introduction/Introduction.html)
* On Android, via [OkHttp](http://square.github.io/okhttp/)

Available on NuGet: https://www.nuget.org/packages/modernhttpclient-updated/ [![NuGet](https://img.shields.io/nuget/v/modernhttpclient-updated.svg?label=NuGet)](https://www.nuget.org/packages/modernhttpclient-updated/)

## Usage

The good news is, you don't have to know either of these two libraries above,
using ModernHttpClient is the most boring thing in the world.

Here's how it works:

```cs
private static HttpClient httpClient = new HttpClient(new NativeMessageHandler() { Timeout = new TimeSpan(0,0,9), DisableCaching = true, UseCookies = false });
```

## How can I use this in a PCL?

Just reference the Portable version of ModernHttpClient in your Portable
Library, and it will use the correct version on all platforms.

#### Release Notes

2.4.3

[Update] Adding Timeout property

[Android] Updating to Square.OkHttp 2.7.5

[Android] Timeout value is not respected on Android #192
