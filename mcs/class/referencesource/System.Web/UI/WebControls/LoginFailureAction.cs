//------------------------------------------------------------------------------
// <copyright file="LoginFailureAction.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls {


    /// <devdoc>
    /// Specifies the actions that may be taken when a login attempt fails.
    /// </devdoc>
    public enum LoginFailureAction {


        /// <devdoc>
        /// Refresh the current page.
        /// </devdoc>
        Refresh = 0,


        /// <devdoc>
        /// Redirect to the dedicated login page.
        /// </devdoc>
        RedirectToLoginPage = 1
    }
}
