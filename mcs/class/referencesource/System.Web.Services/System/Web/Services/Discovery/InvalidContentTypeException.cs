//------------------------------------------------------------------------------
// <copyright file="DiscoveryDocumentReference.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.Services.Discovery {

    using System;

    internal class InvalidContentTypeException : Exception {

        private string contentType;

        internal InvalidContentTypeException(string message, string contentType) : base(message) {
            this.contentType = contentType;
        }

        internal string ContentType {
            get {
                return contentType;
            }
        }
    }
}
