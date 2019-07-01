using System;
using Square.OkHttp3;

namespace ModernHttpClient
{
    public class ProxyAuthenticator : Java.Lang.Object, IAuthenticator
    {
        string credentials;

        public ProxyAuthenticator(string username, string password)
        {
            credentials = Credentials.Basic(username, password);
        }

        public Request Authenticate(Route route, Response response)
        {          
            return response.Request().NewBuilder()
                .Header("Proxy-Authorization", credentials)
                .Build();
        }
    }
}
