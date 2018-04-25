//------------------------------------------------------------------------------
// <copyright file="HotSpotMode.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

using System;

namespace System.Web.UI.WebControls {


    /// <devdoc>
    /// Enumeration of possible ImageMap behaviors.
    /// </devdoc>
    public enum HotSpotMode { 
        

        /// <devdoc>
        /// <para>Inherit the properties of the ImageMap</para>
        /// </devdoc>
        NotSet = 0,
        

        /// <devdoc>
        /// <para>Navigate to a web page.</para>
        /// </devdoc>
        Navigate = 1,


        /// <devdoc>
        /// <para>Cause a postback.</para>
        /// </devdoc>
        PostBack = 2,


        /// <devdoc>
        /// <para>no href</para>
        /// </devdoc>
        Inactive = 3
    }
}
