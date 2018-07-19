//-----------------------------------------------------------------------------
// <copyright file="ContentTransferEncoding.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------------

using System;

namespace System.Net.Mime
{
    /// <summary>
    /// Summary description for ContentTransferEncoding.
    /// </summary>
#if MAKE_MAILCLIENT_PUBLIC
    internal 
#else
    internal
#endif
        enum ContentTransferEncoding
    {
        SevenBit,
        EightBit,
        Binary,
        Base64,
        QuotedPrintable,
        QEncoded,
        Other,
        Unspecified
    }
}
