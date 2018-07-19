//------------------------------------------------------------------------------
// <copyright file="ProcessWindowStyle.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Diagnostics {

    using System.Diagnostics;
    /// <devdoc>
    ///     A set of values indicating how the window should appear when starting
    ///     a process.
    /// </devdoc>
    public enum ProcessWindowStyle {
        /// <devdoc>
        ///     Show the window in a default location.
        /// </devdoc>
        Normal,

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        Hidden,
        
        /// <devdoc>
        ///     Show the window minimized.
        /// </devdoc>
        Minimized,
        
        /// <devdoc>
        ///     Show the window maximized.
        /// </devdoc>
        Maximized
    }
}
