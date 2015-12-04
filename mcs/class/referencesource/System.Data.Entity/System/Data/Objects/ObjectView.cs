//---------------------------------------------------------------------
// <copyright file="ObjectView.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       [....]
// @backupOwner [....]
//---------------------------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Common;
using System.Data.Metadata;
using System.Data.Metadata.Edm;
using System.Data.Objects.DataClasses;
using System.Diagnostics;
using System.Reflection;

namespace System.Data.Objects
{
    /// <summary>
    /// Manages a list suitable for data binding.
    /// </summary>
    /// <typeparam name="TElement">
    /// The type of elements in the binding list.
    /// </typeparam>
    /// <remarks>
    /// <para>
    /// This class provides an implementation of IBindingList that exposes a list of elements to be bound,
    /// provides a mechanism to change the membership of the list,
    /// and events to notify interested objects when the membership of the list is modified 
    /// or an element in the list is modified.
    /// </para>
    /// <para>
    /// ObjectView relies on an object that implements IObjectViewData to manage the binding list.
    /// See the documentation for IObjectViewData for details.
    /// </para>
    /// </remarks>
    internal class ObjectView<TElement> : IBindingList, ICancelAddNew, IObjectView
    {
        /// <summary>
        /// Specifies whether events handled from an underlying collection or individual bound item
        /// should result in list change events being fired from this IBindingList.
        /// <b>True</b> to prevent events from being fired from this IBindingList; 
        /// otherwise <b>false</b> to allow events to propogate.
        /// </summary>
        private bool _suspendEvent; 
       
        // Delegate for IBindingList.ListChanged event.
        private ListChangedEventHandler onListChanged;
       
        /// <summary>
        /// Object that listens for underlying collection or individual bound item changes,
        /// and notifies this object when they occur.
        /// </summary>
        private ObjectViewListener _listener;

        /// <summary>
        /// Index of last item added via a call to IBindingList.AddNew.
        /// </summary>
        private int _addNewIndex = -1;

        /// <summary>
        /// Object that maintains the underlying bound list, 
        /// and specifies the operations allowed on that list.
        /// </summary>
        private IObjectViewData<TElement> _viewData;

        /// <summary>
        /// Construct a new instance of ObjectView using the supplied IObjectViewData and event data source.
        /// </summary>
        /// <param name="viewData">
        /// Object that maintains the underlying bound list, 
        /// and specifies the operations allowed on that list.
        /// </param>
        /// <param name="eventDataSource">
        /// Event source to "attach" to in order to listen to collection and item changes.
        /// </param>
        internal ObjectView(IObjectViewData<TElement> viewData, object eventDataSource)
        {
            _viewData = viewData;
            _listener = new ObjectViewListener(this, (IList)_viewData.List, eventDataSource);
        }

        private void EnsureWritableList()
        {
            if (((IList)this).IsReadOnly)
            {
                throw EntityUtil.WriteOperationNotAllowedOnReadOnlyBindingList();
            }
        }

        private bool IsElementTypeAbstract
        {
            get { return typeof(TElement).IsAbstract; }
        }

        #region ICancelAddNew implementation

        /// <summary>
        /// If a new item has been added to the list, and <paramref name="itemIndex"/> is the position of that item,
        /// remove it from the list and cancel the add operation.
        /// </summary>
        /// <param name="itemIndex">Index of item to be removed as a result of the cancellation of a previous addition.</param>
        void ICancelAddNew.CancelNew(int itemIndex)
        {
            if (_addNewIndex >= 0 && itemIndex == _addNewIndex)
            {
                TElement item = _viewData.List[_addNewIndex];
                _listener.UnregisterEntityEvents(item);

                int oldIndex = _addNewIndex;

                // Reset the addNewIndex here so that the IObjectView.CollectionChanged method 
                // will not attempt to examine the item being removed.
                // See IObjectView.CollectionChanged method for details.
                _addNewIndex = -1;

                try
                {
                    _suspendEvent = true;

                    _viewData.Remove(item, true);
                }
                finally
                {
                    _suspendEvent = false;
                }

                OnListChanged(ListChangedType.ItemDeleted, oldIndex, -1);
            }
        }

        /// <summary>
        /// Commit a new item to the binding list.
        /// </summary>
        /// <param name="itemIndex">
        /// Index of item to be committed.  
        /// This index must match the index of the item created by the last call to IBindindList.AddNew;
        /// otherwise this method is a nop.
        /// </param>
        void ICancelAddNew.EndNew(int itemIndex)
        {
            if (_addNewIndex >= 0 && itemIndex == _addNewIndex)
            {
                _viewData.CommitItemAt(_addNewIndex);
                _addNewIndex = -1;
            }
        }
        #endregion


        #region IBindingList implementation

        bool IBindingList.AllowNew
        {
            get { return _viewData.AllowNew && !IsElementTypeAbstract; }
        }

        bool IBindingList.AllowEdit
        {
            get { return _viewData.AllowEdit; }
        }

        object IBindingList.AddNew()
        {
            EnsureWritableList();

            if (IsElementTypeAbstract)
            {
                throw EntityUtil.AddNewOperationNotAllowedOnAbstractBindingList();
            }

            _viewData.EnsureCanAddNew();

            ((ICancelAddNew)this).EndNew(_addNewIndex);

            TElement newItem = (TElement)Activator.CreateInstance(typeof(TElement));

            _addNewIndex = _viewData.Add(newItem, true);

            _listener.RegisterEntityEvents(newItem);
            OnListChanged(ListChangedType.ItemAdded, _addNewIndex /* newIndex*/, -1 /*oldIndex*/);
            
            return newItem;
        }

        bool IBindingList.AllowRemove
        {
            get { return _viewData.AllowRemove; }
        }

        bool IBindingList.SupportsChangeNotification
        {
            get { return true; }
        }

        bool IBindingList.SupportsSearching
        {
            get { return false; }
        }

        bool IBindingList.SupportsSorting
        {
            get { return false; }
        }

        bool IBindingList.IsSorted
        {
            get { return false; }
        }

        PropertyDescriptor IBindingList.SortProperty
        {
            get { throw EntityUtil.NotSupported(); }
        }

        ListSortDirection IBindingList.SortDirection
        {
            get { throw EntityUtil.NotSupported(); }
        }

        public event System.ComponentModel.ListChangedEventHandler ListChanged
        {
            add
            {
                onListChanged += value;
            }
            remove
            {
                onListChanged -= value;
            }
        }

        void IBindingList.AddIndex(PropertyDescriptor property)
        {
            throw EntityUtil.NotSupported();
        }

        void IBindingList.ApplySort(PropertyDescriptor property, ListSortDirection direction)
        {
            throw EntityUtil.NotSupported();
        }

        int IBindingList.Find(PropertyDescriptor property, object key)
        {
            throw EntityUtil.NotSupported();
        }

        void IBindingList.RemoveIndex(PropertyDescriptor property)
        {
            throw EntityUtil.NotSupported();
        }

        void IBindingList.RemoveSort()
        {
            throw EntityUtil.NotSupported();
        }

        #endregion

        /// <summary>
        /// Get item at the specified index.
        /// </summary>
        /// <param name="index">
        /// The zero-based index of the element to get or set.
        /// </param>
        /// <remarks>
        /// This strongly-typed indexer is used by the data binding in WebForms and ASP.NET
        /// to determine the Type of elements in the bound list. 
        /// The list of properties available for binding can then be determined from that element Type.
        /// </remarks>
        public TElement this[int index]
        {
            get
            {
                return _viewData.List[index];
            }
            set
            {
                // this represents a ROW basically whole entity, we should not allow any setting
                throw EntityUtil.CannotReplacetheEntityorRow();
            }
        }

        #region IList implementation

        object IList.this[int index]
        {
            get
            {
                return _viewData.List[index];
            }
            set
            {
                // this represents a ROW basically whole entity, we should not allow any setting
                throw EntityUtil.CannotReplacetheEntityorRow();
            }
        }

        bool IList.IsReadOnly
        {
            get 
            {
                return !(_viewData.AllowNew || _viewData.AllowRemove);
            }
        }

        bool IList.IsFixedSize
        {
            get { return (false); }
        }

        int IList.Add(object value)
        {
            EnsureWritableList();

            EntityUtil.CheckArgumentNull(value, "value");

            if (!(value is TElement))
            {
                throw EntityUtil.IncompatibleArgument();
            }

            ((ICancelAddNew)this).EndNew(_addNewIndex);

            int index = ((IList)this).IndexOf(value);

            // Add the item if it doesn't already exist in the binding list.
            if (index == -1)
            {
                index = _viewData.Add((TElement)value, false);

                // Only fire a change event if the IObjectView data doesn't implicitly fire an event itself.
                if (!_viewData.FiresEventOnAdd)
                {
                    _listener.RegisterEntityEvents(value);
                    OnListChanged(ListChangedType.ItemAdded, index /*newIndex*/, -1/* oldIndex*/);
                }
            }

            return index;
        }

        void IList.Clear()
        {
            EnsureWritableList();

            ((ICancelAddNew)this).EndNew(_addNewIndex);

            // Only fire a change event if the IObjectView data doesn't implicitly fire an event itself.
            if (_viewData.FiresEventOnClear)
            {
                _viewData.Clear();
            }
            else
            {
                try
                {
                    // Suspend list changed events during the clear, since the IObjectViewData declared that it wouldn't fire an event.
                    // It's possible the IObjectViewData could implement Clear by repeatedly calling Remove, 
                    // and we don't want these events to percolate during the Clear operation.
                    _suspendEvent = true; 
                    _viewData.Clear();
                }
                finally
                {
                    _suspendEvent = false;
                }

                OnListChanged(ListChangedType.Reset, -1 /*newIndex*/, -1/* oldIndex*/); // Indexes not used for reset event.
            }
        }

        bool IList.Contains(object value)
        {
            bool itemExists;

            if (value is TElement)
            {
                itemExists = _viewData.List.Contains((TElement)value);
            }
            else
            {
                itemExists = false;
            }

            return itemExists;
        }

        int IList.IndexOf(object value)
        {
            int index;

            if (value is TElement)
            {
                index = _viewData.List.IndexOf((TElement)value);
            }
            else
            {
                index = -1;
            }

            return index;
        }

        void IList.Insert(int index, object value)
        {
            throw EntityUtil.IndexBasedInsertIsNotSupported();
        }

        void IList.Remove(object value)
        {
            EnsureWritableList();

            EntityUtil.CheckArgumentNull(value, "value");

            if (!(value is TElement))
            {
                throw EntityUtil.IncompatibleArgument();
            }
            
            Debug.Assert(((IList)this).Contains(value), "Value does not exist in view.");

            ((ICancelAddNew)this).EndNew(_addNewIndex);

            TElement item = (TElement)value;

            int index = _viewData.List.IndexOf(item);
            bool removed = _viewData.Remove(item, false);

            // Only fire a change event if the IObjectView data doesn't implicitly fire an event itself.
            if (removed && !_viewData.FiresEventOnRemove)
            {
                _listener.UnregisterEntityEvents(item);
                OnListChanged(ListChangedType.ItemDeleted, index /* newIndex */, -1 /* oldIndex */);
            }
        }

        void IList.RemoveAt(int index)
        {
            ((IList)this).Remove(((IList)this)[index]);
        }

        #endregion

        #region  ICollection implementation

        public int Count
        {
            get { return _viewData.List.Count; }
        }

        public void CopyTo(Array array, int index)
        {
            ((IList)_viewData.List).CopyTo(array, index);
        }
   
        object ICollection.SyncRoot
        {
            get { return this; }
        }

        bool ICollection.IsSynchronized
        {
            get { return false; }
        }

        public IEnumerator GetEnumerator()
        {
            return _viewData.List.GetEnumerator();
        }

        #endregion

        private void OnListChanged(ListChangedType listchangedType, int newIndex, int oldIndex)
        {
            ListChangedEventArgs changeArgs = new ListChangedEventArgs(listchangedType, newIndex, oldIndex);
            OnListChanged(changeArgs);
        }

        private void OnListChanged(ListChangedEventArgs changeArgs)
        {
            // Only fire the event if someone listens to it and it is not suspended.
            if (onListChanged != null && !_suspendEvent)
            {
                onListChanged(this, changeArgs);
            }
        }
        
        void IObjectView.EntityPropertyChanged(object sender, PropertyChangedEventArgs e) 
        {
            Debug.Assert(sender is TElement, "Entity should be of type TElement");
            
            int index = ((IList)this).IndexOf((TElement)sender);
            OnListChanged(ListChangedType.ItemChanged, index /*newIndex*/, index/*oldIndex*/);
        }
        
        /// <summary>
        /// Handle a change in the underlying collection bound by this ObjectView.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">
        /// Event arguments that specify the type of modification and the associated item.
        /// </param>
        void IObjectView.CollectionChanged(object sender, CollectionChangeEventArgs e) 
        {
            // If there is a pending edit of a new item in the bound list (indicated by _addNewIndex >= 0)
            // and the collection membership changed due to an operation external to this ObjectView,
            // it is possible that the _addNewIndex position will need to be adjusted.
            //
            // If the modification was made through this ObjectView, the pending edit would have been implicitly committed,
            // and there would be no need to examine it here.
            TElement addNew = default(TElement);
            
            if (_addNewIndex >= 0)
            {
                addNew = this[_addNewIndex];
            }

            ListChangedEventArgs changeArgs = _viewData.OnCollectionChanged(sender, e, _listener);

            if (_addNewIndex >= 0)
            {
                if (_addNewIndex >= this.Count)
                {
                    _addNewIndex = ((IList)this).IndexOf(addNew);
                }
                else if (!this[_addNewIndex].Equals(addNew))
                {
                    _addNewIndex = ((IList)this).IndexOf(addNew);
                }
            }

            if (changeArgs != null)
            {
                OnListChanged(changeArgs);
            }
        }
    }
}
