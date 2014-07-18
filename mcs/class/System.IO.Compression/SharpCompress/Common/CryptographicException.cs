using System;

namespace SharpCompress.Common
{
    internal class CryptographicException : Exception
    {
        public CryptographicException(string message)
            : base(message)
        {
        }
    }
}