//------------------------------------------------------------------------------
// <copyright file="PathDirection.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls {

    using System;


    /// <devdoc>
    /// <para></para>
    /// </devdoc>
    public enum PathDirection {


        /// <devdoc>
        ///    Path will be rendered from root to current.
        /// </devdoc>
        RootToCurrent = 0,


        /// <devdoc>
        ///    Path will be rendered from current to root.
        /// </devdoc>
        CurrentToRoot = 1
    }
}
