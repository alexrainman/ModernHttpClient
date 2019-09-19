using System;
using System.Globalization;
using System.Net.Http;

namespace ModernHttpClient
{
    internal static class Utility
    {
        public static bool MatchHostnameToPattern(string hostname, string pattern)
        {
            // check if this is a pattern
            int index = pattern.IndexOf('*');
            if (index == -1)
            {
                // not a pattern, do a direct case-insensitive comparison
                return (string.Compare(hostname, pattern, StringComparison.OrdinalIgnoreCase) == 0);
            }

            // check pattern validity
            // A "*" wildcard character MAY be used as the left-most name component in the certificate.

            // unless this is the last char (valid)
            if (index != pattern.Length - 1)
            {
                // then the next char must be a dot .'.
                if (pattern[index + 1] != '.')
                {
                    return false;
                }
            }

            // only one (A) wildcard is supported
            int i2 = pattern.IndexOf('*', index + 1);
            if (i2 != -1) return false;

            // match the end of the pattern
            string end = pattern.Substring(index + 1);
            int length = hostname.Length - end.Length;
            // no point to check a pattern that is longer than the hostname
            if (length <= 0) return false;

            if (string.Compare(hostname, length, end, 0, end.Length, StringComparison.OrdinalIgnoreCase) != 0) {
                return false;
            }

            // special case, we start with the wildcard
            if (index == 0)
            {
                // ensure we hostname non-matched part (start) doesn't contain a dot
                int i3 = hostname.IndexOf('.');
                return ((i3 == -1) || (i3 >= (hostname.Length - end.Length)));
            }

            // match the start of the pattern
            string start = pattern.Substring(0, index);

            return (string.Compare(hostname, 0, start, 0, start.Length, StringComparison.OrdinalIgnoreCase) == 0);
        }

        public static bool PathMatches(string path, string cookiePath) //per update 6265 rules
        {
            if (path == cookiePath)
                return true;
            if (string.IsNullOrEmpty(path) || string.IsNullOrEmpty(cookiePath))
                return false;
            if (path.StartsWith(cookiePath, StringComparison.InvariantCultureIgnoreCase) && cookiePath.EndsWith("/", StringComparison.InvariantCultureIgnoreCase))
                return true;
            if (path.StartsWith(cookiePath, StringComparison.InvariantCultureIgnoreCase) && path.Substring(cookiePath.Length).StartsWith("/", StringComparison.InvariantCultureIgnoreCase))
                return true;

            return false;
        }

        public static void VerifyPins(string[] pins)
        {
            foreach (var pin in pins)
            {
                if (!pin.StartsWith("sha256/", StringComparison.Ordinal) && !pin.StartsWith("sha1/", StringComparison.Ordinal) && !pin.StartsWith("md5/", StringComparison.Ordinal))
                {
                    throw new HttpRequestException(FailureMessages.InvalidPublicKey);
                }

                try
                {
                    byte[] bytes = Convert.FromBase64String(pin.Remove(0, 7));
                }
                catch (Exception ex)
                {
                    throw new HttpRequestException(FailureMessages.InvalidPublicKey, ex);
                }
            }
        }
    } 
}
