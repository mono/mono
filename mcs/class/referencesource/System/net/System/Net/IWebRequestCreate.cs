//------------------------------------------------------------------------------
// <copyright file="IWebRequestCreate.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Net {
    using System;

    //
    // IWebRequestCreate - Interface for creating WebRequests.
    //
    /// <devdoc>
    ///    <para>
    ///       The <see cref='System.Net.IWebRequestCreate'/> interface is used by the <see cref='System.Net.WebRequest'/>
    ///       class to create <see cref='System.Net.WebRequest'/>
    ///       instances for a registered scheme.
    ///    </para>
    /// </devdoc>
    public interface IWebRequestCreate {
        /// <devdoc>
        ///    <para>
        ///       Creates a <see cref='System.Net.WebRequest'/>
        ///       instance.
        ///    </para>
        /// </devdoc>
        WebRequest Create(Uri uri);

    } // interface IWebRequestCreate

} // namespace System.Net
