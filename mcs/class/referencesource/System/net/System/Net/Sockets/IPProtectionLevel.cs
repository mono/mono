//------------------------------------------------------------------------------
// <copyright file="IPProtectionLevel.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Net.Sockets
{
    public enum IPProtectionLevel
    {
        Unspecified = -1,
        Unrestricted = 10,
        EdgeRestricted = 20,
        Restricted = 30,
    }
}

