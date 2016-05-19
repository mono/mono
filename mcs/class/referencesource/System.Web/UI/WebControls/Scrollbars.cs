//------------------------------------------------------------------------------
// <copyright file="Panel.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls {
        

    /// <devdoc>
    ///    Enum used for getting and setting the scrolling behavior of a Panel control.
    /// </devdoc>
    [Flags]
    public enum ScrollBars {

        /// <devdoc>
        ///    [To be supplied.]
        /// </devdoc>
        None = 0,

        /// <devdoc>
        ///    [To be supplied.]
        /// </devdoc>
        Horizontal = 1,

        /// <devdoc>
        ///    [To be supplied.]
        /// </devdoc>
        Vertical = 2,

        /// <devdoc>
        ///    [To be supplied.]
        /// </devdoc>
        Both = 3,

        /// <devdoc>
        ///    [To be supplied.]
        /// </devdoc>
        Auto = 4    
    }
}
