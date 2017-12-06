using System;
using System.IO;
using System.Net.Http;

namespace ModernHttpClient
{
    public class ProgressStreamContent : StreamContent
    {
        const string wrongVersion = "You're referencing the Portable version in your App - you need to reference the platform (iOS/Android) version";

        ProgressStreamContent(Stream stream) : base(stream)
        {
            throw new Exception(wrongVersion);
        }

        ProgressStreamContent(Stream stream, int bufferSize) : base(stream, bufferSize)
        {
            throw new Exception(wrongVersion);
        }

        public ProgressDelegate Progress
        {
            get { throw new Exception(wrongVersion); }
            set { throw new Exception(wrongVersion); }
        }
    }
}
