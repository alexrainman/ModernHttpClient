using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;

namespace ModernHttpClient
{
    internal static class X509Certificate2Extension
    {
        internal static List<string> ParseSubjectAlternativeName(this X509Certificate2 cert)
        {
            var result = new List<string>();

            var subjectAlternativeName = cert.Extensions.Cast<X509Extension>()
                                                .Where(n => n.Oid.Value.Equals(/* SAN OID */"2.5.29.17"))
                                                .Select(n => new AsnEncodedData(n.Oid, n.RawData))
                                                .Select(n => n.Format(true))
                                                .FirstOrDefault();

            if (subjectAlternativeName != null)
            {
                var alternativeNames = subjectAlternativeName.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

                foreach (var alternativeName in alternativeNames)
                {
                    var groups = Regex.Match(alternativeName, @"^DNS Name=(.*)").Groups;

                    if (groups.Count > 0 && !String.IsNullOrEmpty(groups[1].Value))
                    {
                        result.Add(groups[1].Value);
                    }
                }
            }

            return result;
        }
    }
}
