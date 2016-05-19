//-----------------------------------------------------------------------------
// <copyright file="RecipientLocationType.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------------

namespace System.Net.Mail
{
    using System;

#if MAKE_MAILCLIENT_PUBLIC
    internal 
#else
    internal
#endif
        enum RecipientLocationType
    {
        Local,
        Unknown,
        NotLocal,
        WillForward,
        Ambiguous
    }
}
