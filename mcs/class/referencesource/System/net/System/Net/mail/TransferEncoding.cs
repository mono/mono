using System;

namespace System.Net.Mime
{
    /// <summary>
    /// Summary description for TransferEncoding.
    /// </summary>
    public enum TransferEncoding
    {
        QuotedPrintable = 0,
        Base64 = 1,
        SevenBit = 2,
        EightBit = 3,
//        Binary = 4,
        Unknown = -1,
    }
}
