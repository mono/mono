//---------------------------------------------------------------------
// <copyright file="IObjectViewData.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       [....]
// @backupOwner [....]
//---------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace System.Data.Objects
{
    /// <summary>
    /// Defines the behavior required for objects that maintain a binding list exposed by ObjectView.
    /// </summary>
    /// <typeparam name="T">
    /// The type of elements in the binding list.
    /// </typeparam>
    internal interface IObjectViewData<T>
    {
        /// <summary>
        /// Get the binding list maintained by an instance of IObjectViewData.
        /// </summary>
        IList<T> List { get; }

        /// <summary>
        /// Get boolean that specifies whether newly-created items can be added to the binding list.
        /// </summary>
        /// <value>
        /// <b>True</b> if newly-created items can be added to the binding list; otherwise <b>false</b>.
        /// </value>
        bool AllowNew { get; }

        /// <summary>
        /// Get boolean that specifies whether properties of elements in the binding list can be modified.
        /// </summary>
        /// <value>
        /// <b>True</b> if elements can be edited; otherwise <b>false</b>.
        /// </value>
        bool AllowEdit { get; }

        /// <summary>
        /// Get boolean that specifies whether elements can be removed from the binding list.
        /// </summary>
        /// <value>
        /// <b>True</b> if elements can be removed from the binding list; otherwise <b>false</b>.
        /// </value>
        bool AllowRemove { get; }

        /// <summary>
        /// Get boolean that specifies whether the IObjectViewData instance implicitly fires list changed events
        /// when items are added to the binding list.
        /// </summary>
        /// <value>
        /// <b>True</b> if the IObjectViewData instance fires list changed events on add; otherwise <b>false</b>.
        /// </value>
        /// <remarks>
        /// List changed events are fired by the ObjectContext if the IObjectViewData.OnCollectionChanged
        /// method returns a non-null ListChangedEventArgs object.
        /// </remarks>
        bool FiresEventOnAdd { get; }

        /// <summary>
        /// Get boolean that specifies whether the IObjectViewData instance implicitly fires list changed events
        /// when items are removed from the binding list.
        /// </summary>
        /// <value>
        /// <b>True</b> if the IObjectViewData instance fires list changed events on remove; otherwise <b>false</b>.
        /// </value>
        /// <remarks>
        /// List changed events are fired by the ObjectContext if the IObjectViewData.OnCollectionChanged
        /// method returns a non-null ListChangedEventArgs object.
        /// </remarks>
        bool FiresEventOnRemove { get; }

        /// <summary>
        /// Get boolean that specifies whether the IObjectViewData instance implicitly fires list changed events
        /// when all items are cleared from the binding list.
        /// </summary>
        /// <value>
        /// <b>True</b> if the IObjectViewData instance fires list changed events on clear; otherwise <b>false</b>.
        /// </value>
        /// <remarks>
        /// List changed events are fired by the ObjectContext if the IObjectViewData.OnCollectionChanged
        /// method returns a non-null ListChangedEventArgs object.
        /// </remarks>
        bool FiresEventOnClear { get; }

        /// <summary>
        /// Throw an exception if the IObjectViewData instance does not allow newly-created items to be added to this list.
        /// </summary>
        void EnsureCanAddNew();

        /// <summary>
        /// Add an item to the binding list.
        /// </summary>
        /// <param name="item">
        /// Item to be added.
        /// The value of this parameter will never be null, 
        /// and the item is guaranteed to not already exist in the binding list.
        /// </param>
        /// <param name="isAddNew">
        /// <b>True</b> if this method is being called as part of a IBindingList.AddNew operation;
        /// otherwise <b>false</b>.
        /// </param>
        /// <returns>
        /// Index of added item in the binding list.
        /// </returns>
        /// <remarks>
        /// If <paramref name="isAddNew"/> is true, 
        /// the item should only be added to the list returned by the List property, and not any underlying collection.
        /// Otherwise, the item should be added to the binding list as well as any underlying collection.
        /// </remarks>
        int Add(T item, bool isAddNew);

        /// <summary>
        /// Add the item in the binding list at the specified index to any underlying collection.
        /// </summary>
        /// <param name="index">
        /// Index of the item in the binding list. 
        /// The index is guaranteed to be valid for the binding list.
        /// </param>
        void CommitItemAt(int index);

        /// <summary>
        /// Clear all of the items in the binding list, as well as in the underlying collection.
        /// </summary>
        void Clear();

        /// <summary>
        /// Remove an item from the binding list.
        /// </summary>
        /// <param name="item">
        /// Item to be removed.
        /// The value of this parameter will never be null.
        /// The item does not have to exist in the binding list.
        /// </param>
        /// <param name="isCancelNew">
        /// <b>True</b> if this method is being called as part of a ICancelAddNew.CancelNew operation;
        /// otherwise <b>false</b>.
        /// </param>
        /// <returns>
        /// <b>True</b> if item was removed from list; otherwise <b>false</b> if item was not present in the binding list.
        /// </returns>
        /// <remarks>
        /// If <paramref name="isCancelNew"/> is true, 
        /// the item should only be removed from the binding list, and not any underlying collection.
        /// Otherwise, the item should be removed from the binding list as well as any underlying collection.
        /// </remarks>
        bool Remove(T item, bool isCancelNew);

        /// <summary>
        /// Handle change to underlying collection.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">
        /// Event arguments that specify the type of modification and the associated item.
        /// </param>
        /// <param name="listener">
        /// Object used to register or unregister individual item notifications.
        /// </param>
        /// <returns>
        /// ListChangedEventArgs that provides details of how the binding list was changed,
        /// or null if no change to binding list occurred.
        /// The ObjectView will fire a list changed event if this method returns a non-null value.
        /// </returns>
        /// <remarks>
        /// The listener.RegisterEntityEvent method should be called for items added to the binding list,
        /// and the listener.UnregisterEntityEvents method should be called for items removed from the binding list.
        /// Other methods of the ObjectViewListener should not be used.
        /// </remarks>
        ListChangedEventArgs OnCollectionChanged(object sender, CollectionChangeEventArgs e, ObjectViewListener listener);
    }
}
