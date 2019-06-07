using System;
namespace ModernHttpClient
{
    public static class FailureMessages
    {
        public const string NoCertAtAll = "Certificate pinning failure: no cert at all.";
        public const string NoRoot = "Certificate pinning failure: no root?";
        public const string ChainError = "Certificate pinning failure: chain error.";
        public const string SubjectNameMismatch = "Certificate pinning failure: subject name mismatch.";
        public const string PinMismatch = "Certificate pinning failure: pins mismatch.";
        public const string NoPinsProvided = "Certificate pinning failure: no pins provided for host";
    }
}
