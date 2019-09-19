using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Java.Net;

namespace ModernHttpClient
{
    public class NativeCookieHandler : CookieManager, Square.OkHttp3.ICookieJar
    {
        //readonly CookieManager cookieManager = new CookieManager();

        public NativeCookieHandler()
        {
           CookieHandler.Default = this; //cookieManager; //set cookie manager if using NativeCookieHandler
        }

        public void SetCookies(IEnumerable<Cookie> cookies)
        {
            foreach (var nc in cookies.Select(ToNativeCookie)) {
                //cookieManager.CookieStore.Add(new URI(nc.Domain), nc);
                CookieStore.Add(new URI(nc.Domain), nc);
            }
        }

        public void DeleteCookies()
        {
            //cookieManager.CookieStore.RemoveAll();
            CookieStore.RemoveAll();
        }

        public void SetCookie(Cookie cookie)
        {
            var nc = ToNativeCookie(cookie);
            //cookieManager.CookieStore.Add(new URI(nc.Domain), nc);
            CookieStore.Add(new URI(nc.Domain), nc);
        }

        public void DeleteCookie(Cookie cookie)
        {
            var nc = ToNativeCookie(cookie);
            //cookieManager.CookieStore.Remove(new URI(nc.Domain), nc);
            CookieStore.Remove(new URI(nc.Domain), nc);
        }
            
        public List<Cookie> Cookies {
            get {
                //return cookieManager.CookieStore.Cookies
                return CookieStore.Cookies
                    .Select(ToNetCookie)
                    .ToList();
            }
        }

        static HttpCookie ToNativeCookie(Cookie cookie)
        {
            var nc = new HttpCookie(cookie.Name, cookie.Value);
            nc.Domain = cookie.Domain;
            nc.Path = cookie.Path;
            nc.Secure = cookie.Secure;

            return nc;
        }

        static Cookie ToNetCookie(HttpCookie cookie)
        {
            var nc = new Cookie(cookie.Name, cookie.Value, cookie.Path, cookie.Domain);
            nc.Secure = cookie.Secure;

            return nc;
        }

        public IList<Square.OkHttp3.Cookie> LoadForRequest(Square.OkHttp3.HttpUrl p0)
        {
            var cookies = new List<Square.OkHttp3.Cookie>();

            var nc = CookieStore.Get(p0.Uri());
            foreach(var cookie in nc)
            {
                if (Utility.PathMatches(p0.EncodedPath(), cookie.Path))
                {
	                var builder = new Square.OkHttp3.Cookie.Builder();
	                builder.Name(cookie.Name)
	                       .Value(cookie.Value)
	                       .Domain(cookie.Domain)
	                       .Path(cookie.Path);
	                if (cookie.Secure)
	                    builder.Secure();
                
	                cookies.Add(builder.Build());
                }
            }

            return cookies;
        }

        public void SaveFromResponse(Square.OkHttp3.HttpUrl p0, IList<Square.OkHttp3.Cookie> p1)
        {
            foreach(var cookie in p1)
            {
                var nc = new HttpCookie(cookie.Name(), cookie.Value());
                nc.Domain = cookie.Domain();
                nc.Path = cookie.Path();
                nc.Secure = cookie.Secure();

                CookieStore.Add(p0.Uri(), nc);
            }
        }
    }
}
