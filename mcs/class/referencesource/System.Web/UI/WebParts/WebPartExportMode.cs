//------------------------------------------------------------------------------
// <copyright file="VerticalAlign.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls.WebParts {

    using System;

    /// <devdoc>
    ///    <para>
    ///       Specifies what properties can be exported from a WebPart.
    ///    </para>
    /// </devdoc>
    public enum WebPartExportMode {

        /// <devdoc>
        ///    <para>
        ///       The Part is not exportable.
        ///    </para>
        /// </devdoc>
        None = 0,
        
        /// <devdoc>
        ///    <para>
        ///       All properties can be exported.
        ///    </para>
        /// </devdoc>
        All = 1,

        /// <devdoc>
        ///    <para>
        ///       Only non-sensitive data can be exported.
        ///    </para>
        /// </devdoc>
        NonSensitiveData = 2,
    }
}
