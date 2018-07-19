//------------------------------------------------------------------------------
// <copyright file="CollectionChangeAction.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.ComponentModel {
    using System.ComponentModel;

    using System.Diagnostics;

    using System;

    /// <devdoc>
    ///    <para>Specifies how the collection is changed.</para>
    /// </devdoc>
    public enum CollectionChangeAction {
        /// <devdoc>
        ///    <para> Specifies that an element is added to the collection.</para>
        /// </devdoc>
        Add = 1,

        /// <devdoc>
        ///    <para>Specifies that an element is removed from the collection.</para>
        /// </devdoc>
        Remove = 2,

        /// <devdoc>
        ///    <para>Specifies that the entire collection has changed.</para>
        /// </devdoc>
        Refresh = 3
    }
}
