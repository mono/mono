//------------------------------------------------------------------------------
// <copyright file="PartChromeType.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------
namespace System.Web.UI.WebControls.WebParts {

    using System;

    /// <devdoc>
    /// Specifies how to render the title bar and/or border of a part or zone.
    /// </devdoc>
    public enum PartChromeType {

        /// <devdoc>
        /// Inherit the chrome type from the zone.  Applies only to parts.
        /// </devdoc>
        Default = 0,

        /// <devdoc>
        /// Render the title bar and zone.
        /// </devdoc>
        TitleAndBorder = 1,

        /// <devdoc>
        /// Render neither the title bar nor the zone.
        /// </devdoc>
        None = 2,

        /// <devdoc>
        /// Render the title bar only.
        /// </devdoc>
        TitleOnly = 3,

        /// <devdoc>
        /// Render the border only.
        /// </devdoc>
        BorderOnly = 4
    }
}
