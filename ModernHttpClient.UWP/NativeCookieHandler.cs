using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace ModernHttpClient.UWP
{
    public class NativeCookieHandler : CookieContainer
    {
        string CurrentDomain;

        Uri CurrentUri
        {
            get
            {
                return new Uri("http://" + this.CurrentDomain);
            }
        }

        public void SetCookies(IEnumerable<Cookie> cookies)
        {
            foreach (var nc in cookies)
            {
                this.CurrentDomain = nc.Domain;
                this.Add(this.CurrentUri, nc);
            }
        }

        public void DeleteCookies()
        {
            var cookies = this.GetCookies(this.CurrentUri);
            foreach (Cookie nc in cookies)
            {
                nc.Expired = true;
            }
        }

        public void SetCookie(Cookie cookie)
        {
            this.CurrentDomain = cookie.Domain;
            this.Add(this.CurrentUri, cookie);
        }

        public void DeleteCookie(Cookie cookie)
        {
            var cookies = this.GetCookies(this.CurrentUri);
            foreach(Cookie nc in cookies)
            {
                if (nc.Name == cookie.Name)
                    nc.Expired = true;
            }
        }

        public List<Cookie> Cookies
        {
            get
            {
                var collection = this.GetCookies(this.CurrentUri);
                var cookies = new Cookie[collection.Count];
                collection.CopyTo(cookies, 0);
                return cookies.ToList();
            }
        }
    }
}