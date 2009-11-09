// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Internal;
using Microsoft.Internal.Collections;

namespace Microsoft.Internal.Collections
{
    internal class WeakReferenceCollection<T> where T : class
    {
        private readonly List<WeakReference> _items = new List<WeakReference>();

        public void Add(T item)
        {
            // Only cleanup right before we need to reallocate space.
            if (this._items.Capacity == this._items.Count)
            {
                this.CleanupDeadReferences();
            }

            this._items.Add(new WeakReference(item));
        }

        public void Remove(T item)
        {
            int index = IndexOf(item);

            if (index != -1)
            {
                this._items.RemoveAt(index);
            }
        }

        public bool Contains(T item)
        {
            return IndexOf(item) >= 0;
        }

        public void Clear()
        {
            this._items.Clear();
        }

        // Should be executed under at least a read lock.
        private int IndexOf(T item)
        {
            int count = this._items.Count;
            for (int i = 0; i < count; i++)
            {
                if (this._items[i].Target == item)
                {
                    return i;
                }
            }
            return -1;
        }

        // Should be executed under a write lock
        private void CleanupDeadReferences()
        {
            int count = this._items.Count;
            for (int i = count - 1; i >= 0; i--)
            {
                if (this._items[i].Target == null)
                {
                    this._items.RemoveAt(i);
                }
            }           
        }

        public List<T> AliveItemsToList()
        {
            List<T> aliveItems = new List<T>();

            foreach (var weakItem in this._items)
            {
                T item = weakItem.Target as T;

                if (item != null)
                {
                    aliveItems.Add(item);
                }
            }

            return aliveItems;
        }
    }
}
