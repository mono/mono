//------------------------------------------------------------------------------
// <copyright file="IChangeTracking.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.ComponentModel {
    
    /// <devdoc>
    /// </devdoc>
    public interface IChangeTracking {

        /// <devdoc>
        /// </devdoc>
        bool IsChanged { get; }

        /// <devdoc>
        /// </devdoc>
        void AcceptChanges();
    }
}
