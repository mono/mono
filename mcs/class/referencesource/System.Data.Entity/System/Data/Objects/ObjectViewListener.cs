//---------------------------------------------------------------------
// <copyright file="ObjectViewListener.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       Microsoft
// @backupOwner Microsoft
//---------------------------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Objects.DataClasses;
using System.Diagnostics;

// Dev notes -1
// why we need this class: in order to keep the view alive, we have to listen to evens from entities and
// also EntityCollection/ObjectStateManager they exists in. listening to event will prevent the view to be 
// disposed, hence GC'ed due to having a strong reference; and to avoid this situation we have to introduce 
// a new layer which will have a weakreference to view (1-so it can go out of scope, 2- this layer will listen to 
// the events and notify the view - by calling its APIS-  for any change that happens)

// Dev notes -2
// following statement is valid on current existing CLR: 
// lets say Customer is an Entity, Array[Customer] is not Array[Entity]; it is not supported
// to do the work around we have to use a non-Generic interface/class so we can pass the view 
// to ObjectViewListener safely (IObjectView)

namespace System.Data.Objects
{
    internal sealed class ObjectViewListener
    {
        private WeakReference _viewWeak;
        private object _dataSource;
        private IList _list;

        internal ObjectViewListener(IObjectView view, IList list, object dataSource)
        {
            _viewWeak = new WeakReference(view);
            _dataSource = dataSource;
            _list = list;

            RegisterCollectionEvents();
            RegisterEntityEvents();
        }

        private void CleanUpListener()
        {
            UnregisterCollectionEvents();
            UnregisterEntityEvents();
        }

        private void RegisterCollectionEvents()
        {
            ObjectStateManager cache = _dataSource as ObjectStateManager;
            if (cache != null)
            {
                cache.EntityDeleted += CollectionChanged;
            }
            else if (null != _dataSource)
            {
                ((RelatedEnd)_dataSource).AssociationChangedForObjectView += CollectionChanged;
            }
        }

        private void UnregisterCollectionEvents()
        {
            ObjectStateManager cache = _dataSource as ObjectStateManager;
            if (cache != null)
            {
                cache.EntityDeleted -= CollectionChanged;
            }
            else if (null != _dataSource)
            {
                ((RelatedEnd)_dataSource).AssociationChangedForObjectView -= CollectionChanged;
            }
        }

        internal void RegisterEntityEvents(object entity)
        {
            Debug.Assert(entity != null, "Entity should not be null");
            INotifyPropertyChanged propChanged = entity as INotifyPropertyChanged;
            if (propChanged != null)
            {
                propChanged.PropertyChanged += EntityPropertyChanged;
            } 
        }

        private void RegisterEntityEvents()
        {
            if (null != _list)
            {
                foreach (object entityObject in _list)
                {
                    INotifyPropertyChanged propChanged = entityObject as INotifyPropertyChanged;
                    if (propChanged != null)
                    {
                        propChanged.PropertyChanged += EntityPropertyChanged;
                    }
                }
            }
        }

        internal void UnregisterEntityEvents(object entity)
        {
            Debug.Assert(entity != null, "entity should not be null");
            INotifyPropertyChanged propChanged = entity as INotifyPropertyChanged;
            if (propChanged != null)
            {
                propChanged.PropertyChanged -= EntityPropertyChanged;
            } 
        }

        private void UnregisterEntityEvents()
        {
            if (null != _list)
            {
                foreach (object entityObject in _list)
                {
                    INotifyPropertyChanged propChanged = entityObject as INotifyPropertyChanged;
                    if (propChanged != null)
                    {
                        propChanged.PropertyChanged -= EntityPropertyChanged;
                    }
                }
            }
        }

        private void EntityPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            IObjectView view = (IObjectView)_viewWeak.Target;
            if (view != null)
            {
                view.EntityPropertyChanged(sender, e);
            }
            else
            {
                CleanUpListener();
            }
        }

        private void CollectionChanged(object sender, CollectionChangeEventArgs e)
        {
            IObjectView view = (IObjectView)_viewWeak.Target;
            if (view != null)
            {
                view.CollectionChanged(sender, e);
            }
            else
            {
                CleanUpListener();
            }
        }
    }
}
