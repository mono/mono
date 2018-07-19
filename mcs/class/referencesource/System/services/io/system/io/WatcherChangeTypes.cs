//------------------------------------------------------------------------------
// <copyright file="WatcherChangeTypes.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.IO {

    using System.Diagnostics;
    using System;

    
    /// <devdoc>
    ///    <para>Changes that may occur to a file or directory.
    ///       </para>
    /// </devdoc>
    [Flags()]
    public enum WatcherChangeTypes {
        /// <devdoc>
        ///    <para>
        ///       The creation of a file or folder.
        ///    </para>
        /// </devdoc>
        Created = 1,
        /// <devdoc>
        ///    <para>
        ///       The deletion of a file or folder.
        ///    </para>
        /// </devdoc>
        Deleted = 2,
        /// <devdoc>
        ///    <para>
        ///       The change of a file or folder.
        ///    </para>
        /// </devdoc>
        Changed = 4,
        /// <devdoc>
        ///    <para>
        ///       The renaming of a file or folder.
        ///    </para>
        /// </devdoc>
        Renamed = 8,
        // all of the above 
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        All = Created | Deleted | Changed | Renamed
    }
}
