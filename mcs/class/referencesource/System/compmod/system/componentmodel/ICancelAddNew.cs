//------------------------------------------------------------------------------
// <copyright file="ICancelAddNew.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.ComponentModel {

    using System;

    /// <devdoc>
    ///     Interface implemented by a list that allows the addition of a new item
    ///     to be either cancelled or committed.
    ///
    ///     Note: In some scenarios, specifically Windows Forms complex data binding,
    ///     the list may recieve CancelNew or EndNew calls for items other than the
    ///     new item. These calls should be ignored, ie. the new item should only be
    ///     cancelled or committed when that item's index is specified.
    /// </devdoc>
    public interface ICancelAddNew
    {
        /// <devdoc>
        ///     If a new item has been added to the list, and <paramref name="itemIndex"/> is the position of that item,
        ///     then this method should remove it from the list and cancel the add operation.
        /// </devdoc>
        void CancelNew(int itemIndex);

        /// <devdoc>
        ///     If a new item has been added to the list, and <paramref name="itemIndex"/> is the position of that item,
        ///     then this method should leave it in the list and complete the add operation.
        /// </devdoc>
        void EndNew(int itemIndex);
    }
}
