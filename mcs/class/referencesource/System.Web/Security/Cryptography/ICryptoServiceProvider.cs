//------------------------------------------------------------------------------
// <copyright file="ICryptoServiceProvider.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.Security.Cryptography {
    using System;

    // Represents an object that can provide ICryptoService instances.
    // Get an instance of this type via the AspNetCryptoServiceProvider.Instance singleton property.

    internal interface ICryptoServiceProvider {

        ICryptoService GetCryptoService(Purpose purpose, CryptoServiceOptions options = CryptoServiceOptions.None);

    }
}
