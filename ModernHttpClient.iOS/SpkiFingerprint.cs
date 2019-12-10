using System;
using System.Security.Cryptography;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.X509;

namespace ModernHttpClient
{
    public class SpkiFingerprint
    {
        public static string ComputeSHA256(byte[] certificate)
        {
            // Load ASN.1 encoded certificate structure
            var certAsn1 = Asn1Object.FromByteArray(certificate);
            var certStruct = X509CertificateStructure.GetInstance(certAsn1);

            // Extract SPKI and DER-encode it
            var spki = certStruct.SubjectPublicKeyInfo;
            var spkiDer = spki.GetDerEncoded();

            // Compute spki fingerprint (sha256)
            string spkiFingerprint;

            using (var digester = SHA256.Create())
            {
                var digest = digester.ComputeHash(spkiDer);
                spkiFingerprint = Convert.ToBase64String(digest);
            }

            return $"sha256/{spkiFingerprint}";
        }

        public static string ComputeSHA1(byte[] certificate)
        {
            // Load ASN.1 encoded certificate structure
            var certAsn1 = Asn1Object.FromByteArray(certificate);
            var certStruct = X509CertificateStructure.GetInstance(certAsn1);

            // Extract SPKI and DER-encode it
            var spki = certStruct.SubjectPublicKeyInfo;
            var spkiDer = spki.GetDerEncoded();

            // Compute spki fingerprint (sha1)
            string spkiFingerprint;

            using (var digester = SHA1.Create())
            {
                var digest = digester.ComputeHash(spkiDer);
                spkiFingerprint = Convert.ToBase64String(digest);
            }

            return $"sha1/{spkiFingerprint}";
        }
    }
}
