//------------------------------------------------------------------------------
// <copyright file="WebPartVerbRenderMode.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------
namespace System.Web.UI.WebControls.WebParts {

    using System;

    /// <devdoc>
    /// Specifies how to render the WebPartVerbs.
    /// </devdoc>
    public enum WebPartVerbRenderMode {

        /// <devdoc>
        /// Render the WebPartVerbs in a popup menu in the WebPart TitleBar.
        /// </devdoc>
        Menu = 0,

        /// <devdoc>
        /// Render the WebPartVerbs as links or buttons directly in the WebPart TitleBar.
        /// This mode is keyboard accessible.
        /// </devdoc>
        TitleBar = 1,
    }
}
