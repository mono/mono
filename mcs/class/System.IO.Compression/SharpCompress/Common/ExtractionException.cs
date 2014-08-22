using System;

namespace SharpCompress.Common
{
    internal class ExtractionException : Exception
    {
        public ExtractionException(string message)
            : base(message)
        {
        }

        public ExtractionException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}