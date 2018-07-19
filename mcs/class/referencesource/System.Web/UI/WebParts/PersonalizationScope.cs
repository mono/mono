//------------------------------------------------------------------------------
// <copyright file="PersonalizationScope.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls.WebParts {

    using System;

    /// <devdoc>
    /// Represents the personalization scope of an object or some data.
    /// </devdoc>
    public enum PersonalizationScope {

        /// <devdoc>
        /// Indicates that the personalized data applies to a specific user.
        /// </devdoc>
        User = 0,

        /// <devdoc>
        /// Indicates that the personalized data applies to all users.
        /// </devdoc>
        Shared = 1
    }
}

