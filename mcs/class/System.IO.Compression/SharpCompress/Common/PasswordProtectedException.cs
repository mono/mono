using System;

namespace SharpCompress.Common
{
    internal class PasswordProtectedException : ExtractionException
    {
        public PasswordProtectedException(string message)
            : base(message)
        {
        }

        public PasswordProtectedException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}