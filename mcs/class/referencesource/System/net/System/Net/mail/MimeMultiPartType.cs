using System;

namespace System.Net.Mime
{
    /// <summary>
    /// Summary description for MimeMultiPartType.
    /// </summary>
    internal enum MimeMultiPartType
    {
        Mixed = 0,
        Alternative = 1,
        Parallel = 2,
        Related = 3,

        Unknown = -1
    }
}
