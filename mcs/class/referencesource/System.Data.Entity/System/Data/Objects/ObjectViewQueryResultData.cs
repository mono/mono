//---------------------------------------------------------------------
// <copyright file="ObjectViewQueryResultData.cs" company="Microsoft">
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
using System.Data;
using System.Data.Common;
using System.Data.Metadata;
using System.Data.Metadata.Edm;
using System.Data.Objects.DataClasses;
using System.Diagnostics;

namespace System.Data.Objects
{
    /// <summary>
    /// Manages a binding list constructed from query results.
    /// </summary>
    /// <typeparam name="TElement">
    /// Type of the elements in the binding list.
    /// </typeparam>
    /// <remarks>
    /// The binding list is initialized from query results.
    /// If the binding list can be modified, 
    /// objects are added or removed from the ObjectStateManager (via the ObjectContext).
    /// </remarks>
    internal sealed class ObjectViewQueryResultData<TElement> : IObjectViewData<TElement>
    {
        private List<TElement> _bindingList;

        /// <summary>
        /// ObjectContext used to add or delete objects when the list can be modified.
        /// </summary>
        private ObjectContext _objectContext;
        
        /// <summary>
        /// If the TElement type is an Entity type of some kind,
        /// this field specifies the entity set to add entity objects.
        /// </summary>
        private EntitySet _entitySet; 

        private bool _canEditItems;
        private bool _canModifyList;

        /// <summary>
        /// Construct a new instance of the ObjectViewQueryResultData class using the supplied query results.
        /// </summary>
        /// <param name="queryResults">
        /// Result of object query execution used to populate the binding list.
        /// </param>
        /// <param name="objectContext">
        /// ObjectContext used to add or remove items.
        /// If the binding list can be modified, this parameter should not be null.
        /// </param>
        /// <param name="forceReadOnlyList">
        /// <b>True</b> if items should not be allowed to be added or removed from the binding list.
        /// Note that other conditions may prevent the binding list from being modified, so a value of <b>false</b>
        /// supplied for this parameter doesn't necessarily mean that the list will be writable.
        /// </param>
        /// <param name="entitySet">
        /// If the TElement type is an Entity type of some kind,
        /// this field specifies the entity set to add entity objects.
        /// </param>
        internal ObjectViewQueryResultData(IEnumerable queryResults, ObjectContext objectContext, bool forceReadOnlyList, EntitySet entitySet)
        {
            bool canTrackItemChanges = IsEditable(typeof(TElement));

            _objectContext = objectContext;
            _entitySet = entitySet;

            _canEditItems = canTrackItemChanges;
            _canModifyList = !forceReadOnlyList && canTrackItemChanges && _objectContext != null;

            _bindingList = new List<TElement>();
            foreach (TElement element in queryResults)
            {
                _bindingList.Add(element);
            }
        }

        /// <summary>
        /// Cannot be a DbDataRecord or a derivative of DbDataRecord
        /// </summary>
        /// <param name="elementType"></param>
        /// <returns></returns>
        private bool IsEditable(Type elementType)
        {
            return !((elementType == typeof(DbDataRecord)) ||
                     ((elementType != typeof(DbDataRecord)) && elementType.IsSubclassOf(typeof(DbDataRecord))));
        }

        /// <summary>
        /// Throw an exception is an entity set was not specified for this instance.
        /// </summary>
        private void EnsureEntitySet()
        {
            if (_entitySet == null)
            {
                throw EntityUtil.CannotResolveTheEntitySetforGivenEntity(typeof(TElement));
            }
        }

        #region IObjectViewData<T> Members

        public IList<TElement> List
        {
            get { return _bindingList; }
        }

        public bool AllowNew
        {
            get { return _canModifyList && _entitySet != null; }
        }

        public bool AllowEdit
        {
            get { return _canEditItems; }
        }

        public bool AllowRemove
        {
            get { return _canModifyList; }
        }

        public bool FiresEventOnAdd
        {
            get { return false; }
        }

        public bool FiresEventOnRemove
        {
            get { return true; }
        }

        public bool FiresEventOnClear
        {
            get { return false; }
        }

        public void EnsureCanAddNew()
        {
            EnsureEntitySet();
        }

        public int Add(TElement item, bool isAddNew)
        {
            EnsureEntitySet();

            Debug.Assert(_objectContext != null, "ObjectContext is null.");
            
            // If called for AddNew operation, add item to binding list, pending addition to ObjectContext.
            if (!isAddNew)
            {
                _objectContext.AddObject(TypeHelpers.GetFullName(_entitySet), item);
            }

            _bindingList.Add(item);

            return _bindingList.Count - 1;
        }

        public void CommitItemAt(int index)
        {
            EnsureEntitySet();

            Debug.Assert(_objectContext != null, "ObjectContext is null.");

            TElement item = _bindingList[index];
            _objectContext.AddObject(TypeHelpers.GetFullName(_entitySet), item);
        }

        public void Clear()
        {
            while (0 < _bindingList.Count)
            {
                TElement entity = _bindingList[_bindingList.Count - 1];

                Remove(entity, false);
            }
        }

        public bool Remove(TElement item, bool isCancelNew)
        {
            bool removed;

            Debug.Assert(_objectContext != null, "ObjectContext is null.");

            if (isCancelNew)
            {
                // Item was previously added to binding list, but not ObjectContext.
                removed = _bindingList.Remove(item);
            }
            else
            {
                EntityEntry stateEntry = _objectContext.ObjectStateManager.FindEntityEntry(item);

                if (stateEntry != null)
                {
                    stateEntry.Delete();
                    // OnCollectionChanged event will be fired, where the binding list will be updated.
                    removed = true;
                }
                else
                {
                    removed = false;
                }
            }

            return removed;
        }

        public ListChangedEventArgs OnCollectionChanged(object sender, CollectionChangeEventArgs e, ObjectViewListener listener)
        {
            ListChangedEventArgs changeArgs = null;

            // Since event is coming from cache and it might be shared amoung different queries
            // we have to check to see if correct event is being handled.
            if (e.Element.GetType().IsAssignableFrom(typeof(TElement)) &&
                _bindingList.Contains((TElement)(e.Element)))
            {
                TElement item = (TElement)e.Element;
                int itemIndex = _bindingList.IndexOf(item);

                if (itemIndex >= 0) // Ignore entities that we don't know about.
                {
                    // Only process "remove" events.
                    Debug.Assert(e.Action != CollectionChangeAction.Refresh, "Cache should never fire with refresh, it does not have clear");

                    if (e.Action == CollectionChangeAction.Remove)
                    {
                        _bindingList.Remove(item);

                        listener.UnregisterEntityEvents(item);

                        changeArgs = new ListChangedEventArgs(ListChangedType.ItemDeleted, itemIndex /* newIndex*/, -1 /* oldIndex*/);
                    }
                }
            }

            return changeArgs;
        }

        #endregion
    }
}
