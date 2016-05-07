//------------------------------------------------------------------------------
// <copyright file="DiscoveryClientProtocol.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.Services.Discovery {

    using System;

    internal class InvalidDocumentContentsException : Exception {
        internal InvalidDocumentContentsException(string message, Exception inner) : base(message, inner) {
        }
    }
}
