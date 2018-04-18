//---------------------------------------------------------------------
// <copyright file="ObjectViewEntityCollectionData.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       Microsoft
// @backupOwner Microsoft
//---------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Objects.DataClasses;
using System.Data.Objects.Internal;
using System.Diagnostics;
using System.Data.Common;

namespace System.Data.Objects
{
    /// <summary>
    /// Manages a binding list constructed from an EntityCollection.
    /// </summary>
    /// <typeparam name="TViewElement">
    /// Type of the elements in the binding list.
    /// </typeparam>
    /// <typeparam name="TItemElement">
    /// Type of element in the underlying EntityCollection.
    /// </typeparam>
    /// <remarks>
    /// The binding list is initialized from the EntityCollection,
    /// and is synchronized with changes made to the EntityCollection membership.
    /// This class always allows additions and removals from the binding list.
    /// </remarks>
    internal sealed class ObjectViewEntityCollectionData<TViewElement, TItemElement> : IObjectViewData<TViewElement>
        where TItemElement : class
        where TViewElement : TItemElement
    {
        private List<TViewElement> _bindingList;

        private EntityCollection<TItemElement> _entityCollection;

        private readonly bool _canEditItems;

        /// <summary>
        /// <b>True</b> if item that was added to binding list but not underlying entity collection
        /// is now being committed to the collection.
        /// Otherwise <b>false</b>.
        /// Used by CommitItemAt and OnCollectionChanged methods to coordinate addition
        /// of new item to underlying entity collection.
        /// </summary>
        private bool _itemCommitPending;

        /// <summary>
        /// Construct a new instance of the ObjectViewEntityCollectionData class using the supplied entityCollection.
        /// </summary>
        /// <param name="entityCollection">
        /// EntityCollection used to populate the binding list.
        /// </param>
        internal ObjectViewEntityCollectionData(EntityCollection<TItemElement> entityCollection)
        {
            _entityCollection = entityCollection;

            _canEditItems = true;

            // Allow deferred loading to occur when initially populating the collection
            _bindingList = new List<TViewElement>(entityCollection.Count);
            foreach (TViewElement entity in entityCollection)
            {
                _bindingList.Add(entity);
            }
        }

        #region IObjectViewData<TViewElement> Members

        public IList<TViewElement> List
        {
            get { return _bindingList; }
        }

        public bool AllowNew
        {
            get { return !_entityCollection.IsReadOnly; }
        }

        public bool AllowEdit
        {
            get { return _canEditItems; }
        }

        public bool AllowRemove
        {
            get { return !_entityCollection.IsReadOnly; }
        }

        public bool FiresEventOnAdd
        {
            get { return true; }
        }

        public bool FiresEventOnRemove
        {
            get { return true; }
        }

        public bool FiresEventOnClear
        {
            get { return true; }
        }

        public void EnsureCanAddNew()
        {
            // nop
        }

        public int Add(TViewElement item, bool isAddNew)
        {
            if (isAddNew)
            {
                // Item is added to bindingList, but pending addition to entity collection.
                _bindingList.Add(item);
            }
            else
            {
                _entityCollection.Add(item);
                // OnCollectionChanged will be fired, where the binding list will be updated.
            }

            return _bindingList.Count - 1;
        }

        public void CommitItemAt(int index)
        {
            TViewElement item = _bindingList[index];

            try
            {
                _itemCommitPending = true;

                _entityCollection.Add(item);
                // OnCollectionChanged will be fired, where the binding list will be updated.
            }
            finally
            {
                _itemCommitPending = false;
            }

        }

        public void Clear()
        {
            if (0 < _bindingList.Count)
            {
                List<object> _deletionList = new List<object>();

                foreach (object item in _bindingList)
                {
                    _deletionList.Add(item);
                }

                _entityCollection.BulkDeleteAll(_deletionList);
                // EntityCollection will fire change event which this instance will use to clean up the binding list.
            }
        }

        public bool Remove(TViewElement item, bool isCancelNew)
        {
            bool removed;

            if (isCancelNew)
            {
                // Item was previously added to binding list, but not entity collection.
                removed = _bindingList.Remove(item);
            }
            else
            {
                removed = _entityCollection.RemoveInternal(item);
                // OnCollectionChanged will be fired, where the binding list will be updated.
            }

            return removed;
        }

        public ListChangedEventArgs OnCollectionChanged(object sender, CollectionChangeEventArgs e, ObjectViewListener listener)
        {
            ListChangedEventArgs changeArgs = null;

            switch (e.Action)
            {
                case CollectionChangeAction.Remove:
                    // An Entity is being removed from entity collection, remove it from list.
                    if (e.Element is TViewElement)
                    {
                        TViewElement removedItem = (TViewElement)e.Element;

                        int oldIndex = _bindingList.IndexOf(removedItem);
                        if (oldIndex != -1)
                        {
                            _bindingList.Remove(removedItem);

                            // Unhook from events of removed entity.
                            listener.UnregisterEntityEvents(removedItem);

                            changeArgs = new ListChangedEventArgs(ListChangedType.ItemDeleted, oldIndex /* newIndex*/, -1 /* oldIndex*/);
                        }
                    }
                    break;

                case CollectionChangeAction.Add:
                    // Add the entity to our list.
                    if (e.Element is TViewElement)
                    {
                        // Do not process Add events that fire as a result of committing an item to the entity collection.
                        if (!_itemCommitPending)
                        {
                            TViewElement addedItem = (TViewElement)e.Element;

                            _bindingList.Add(addedItem);

                            // Register to its events.
                            listener.RegisterEntityEvents(addedItem);

                            changeArgs = new ListChangedEventArgs(ListChangedType.ItemAdded, _bindingList.Count - 1 /* newIndex*/, -1 /* oldIndex*/);
                        }
                    }
                    break;

                case CollectionChangeAction.Refresh:
                    foreach (TViewElement entity in _bindingList)
                    {
                        listener.UnregisterEntityEvents(entity);
                    }

                    _bindingList.Clear();

                    foreach(TViewElement entity in _entityCollection.GetInternalEnumerable())
                    {
                        _bindingList.Add(entity);

                        listener.RegisterEntityEvents(entity);
                    }

                    changeArgs = new ListChangedEventArgs(ListChangedType.Reset, -1 /*newIndex*/, -1/*oldIndex*/);
                    break;
            }

            return changeArgs;
        }

        #endregion
    }
}
