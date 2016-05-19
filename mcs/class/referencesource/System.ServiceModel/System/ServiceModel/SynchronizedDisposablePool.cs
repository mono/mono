//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel
{
    using System.Collections.Generic;
    using System.Threading;

    class SynchronizedDisposablePool<T> where T : class, IDisposable
    {
        List<T> items;
        int maxCount;
        bool disposed;

        public SynchronizedDisposablePool(int maxCount)
        {
            this.items = new List<T>();
            this.maxCount = maxCount;
        }

        object ThisLock
        {
            get { return this; }
        }

        public void Dispose()
        {
            T[] items;
            lock (ThisLock)
            {
                if (!disposed)
                {
                    disposed = true;
                    if (this.items.Count > 0)
                    {
                        items = new T[this.items.Count];
                        this.items.CopyTo(items, 0);
                        this.items.Clear();
                    }
                    else
                    {
                        items = null;
                    }
                }
                else
                {
                    items = null;
                }
            }
            if (items != null)
            {
                for (int i = 0; i < items.Length; i++)
                {
                    items[i].Dispose();
                }
            }
        }

        public bool Return(T value)
        {
            if (!disposed && this.items.Count < this.maxCount)
            {
                lock (ThisLock)
                {
                    if (!disposed && this.items.Count < this.maxCount)
                    {
                        this.items.Add(value);
                        return true;
                    }
                }
            }
            return false;
        }

        public T Take()
        {
            if (!disposed && this.items.Count > 0)
            {
                lock (ThisLock)
                {
                    if (!disposed && this.items.Count > 0)
                    {
                        int index = this.items.Count - 1;
                        T item = this.items[index];
                        this.items.RemoveAt(index);
                        return item;
                    }
                }
            }
            return null;
        }
    }
}
