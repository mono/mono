using System;

namespace SharpCompress.Common
{
    internal class MultiVolumeExtractionException : ExtractionException
    {
        public MultiVolumeExtractionException(string message)
            : base(message)
        {
        }

        public MultiVolumeExtractionException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}