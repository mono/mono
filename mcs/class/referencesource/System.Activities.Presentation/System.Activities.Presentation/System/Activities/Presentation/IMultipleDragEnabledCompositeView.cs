//----------------------------------------------------------------
// <copyright company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//----------------------------------------------------------------

namespace System.Activities.Presentation
{
    using System.Activities.Presentation.Model;
    using System.Activities.Presentation.View;
    using System.Collections.Generic;
    using System.Windows;

    /// <summary>
    /// An extention interface for ICompositeView to better surpport
    /// multiple drag/drop.
    /// </summary>
    public interface IMultipleDragEnabledCompositeView : ICompositeView
    {
        /// <summary>
        /// This method will be used when item order is needed
        /// </summary>
        /// <param name="selectedItems">Selected items to sort.</param>
        /// <returns>Sorted items</returns>
        List<ModelItem> SortSelectedItems(List<ModelItem> selectedItems);

        /// <summary>
        /// After drag/drop, the source container will be notified which 
        /// items are moved out.
        /// After implement this interface, ICompositeView.OnItemMoved will
        /// not be called even in single element drag/drop.
        /// </summary>
        /// <param name="movedItems">moved items</param>
        void OnItemsMoved(List<ModelItem> movedItems);
    }
}
