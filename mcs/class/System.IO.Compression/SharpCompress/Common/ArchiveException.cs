using System;

namespace SharpCompress.Common
{
    internal class ArchiveException : Exception
    {
        public ArchiveException(string message)
            : base(message)
        {
        }
    }
}