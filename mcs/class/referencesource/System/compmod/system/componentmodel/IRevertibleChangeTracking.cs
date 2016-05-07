//------------------------------------------------------------------------------
// <copyright file="IRevertibleChangeTracking.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.ComponentModel {

    /// <devdoc>
    /// </devdoc>
    public interface IRevertibleChangeTracking : IChangeTracking {

        /// <devdoc>
        /// </devdoc>
        void RejectChanges();
    }
}
