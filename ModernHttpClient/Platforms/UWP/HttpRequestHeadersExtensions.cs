using System.Net.Http.Headers;

namespace ModernHttpClient
{
    public static class HttpRequestHeadersExtensions
    {
        public static void Set(this HttpRequestHeaders headers, string name, string value)
        {
            if (headers.Contains(name)) headers.Remove(name);
            headers.Add(name, value);
        }
    }
}
