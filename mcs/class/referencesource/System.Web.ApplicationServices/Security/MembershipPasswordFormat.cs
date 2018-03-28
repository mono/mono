//------------------------------------------------------------------------------
// <copyright file="MembershipPasswordFormat.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.Security {
    using System;
    using System.Runtime.CompilerServices;
    
    [TypeForwardedFrom("System.Web, Version=2.0.0.0, Culture=Neutral, PublicKeyToken=b03f5f7f11d50a3a")]
    public enum MembershipPasswordFormat {

        // The password is stored in cleartext in the database.
        Clear = 0, 

        // The password is cryptographically hashed and stored in the database.
        Hashed = 1, 

        // The password is encrypted using reversible encryption (using <machineKey>) and stored in the database.
        Encrypted = 2,

    }
}
