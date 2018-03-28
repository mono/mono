//------------------------------------------------------------------------------
// <copyright file="IDataProtectorFactory.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.Security.Cryptography {
    using System;
    using System.Security.Cryptography;

    // Represents an object that can provide DataProtector instances

    internal interface IDataProtectorFactory {

        DataProtector GetDataProtector(Purpose purpose);

    }
}
