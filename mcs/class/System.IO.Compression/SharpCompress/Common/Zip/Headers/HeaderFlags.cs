using System;

namespace SharpCompress.Common.Zip.Headers
{
    [Flags]
    internal enum HeaderFlags : ushort
    {
        Encrypted = 1, // http://www.pkware.com/documents/casestudies/APPNOTE.TXT
        Bit1 = 2,
        Bit2 = 4,
        UsePostDataDescriptor = 8,
        EnhancedDeflate = 16,
        UTF8 = 2048
    }
}