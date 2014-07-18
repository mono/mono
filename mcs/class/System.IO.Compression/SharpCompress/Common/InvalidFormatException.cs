using System;

namespace SharpCompress.Common
{
    internal class InvalidFormatException : ExtractionException
    {
        public InvalidFormatException(string message)
            : base(message)
        {
        }

        public InvalidFormatException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}