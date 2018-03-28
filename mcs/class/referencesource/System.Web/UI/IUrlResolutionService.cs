//------------------------------------------------------------------------------
// <copyright file="IUrlResolver.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI {

    using System;

    /// <devdoc>
    /// Implemented by objects that have context information about thier own
    /// location (or URL) and can resolve relative URLs based on that.
    /// </devdoc>
    public interface IUrlResolutionService {


        /// <devdoc>
        /// Return a resolved URL that is suitable for use on the client.
        /// If the specified URL is absolute, it is returned unchanged.
        /// Otherwise, it is turned into a relative URL that is based
        /// on the current request path (which the browser then resolves
        /// to get a complete URL).
        /// </devdoc>
        string ResolveClientUrl(string relativeUrl);
    }
}
