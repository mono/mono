//---------------------------------------------------------------------
// <copyright file="ThreadSafeList.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner [....]
// @backupOwner [....]
//---------------------------------------------------------------------

using System.Collections.Generic;
using System.Threading;
namespace System.Data.Common.Utils
{

    internal sealed class ThreadSafeList<T> : IList<T>
    {
        private readonly ReaderWriterLockSlim _lock;
        private List<T> _list;

        internal ThreadSafeList()
        {
            _list = new List<T>();
            _lock = new ReaderWriterLockSlim();
        }

        public int Count
        {
            get
            {
                _lock.EnterReadLock();
                int count;
                try
                {
                     count = _list.Count;
                }
                finally
                {
                    _lock.ExitReadLock();
                }

                return count;
            }
        }

        public void Add(T item)
        {
            _lock.EnterWriteLock();
            try
            {
                _list.Add(item);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public T this[int index]
        {
            get
            {
                _lock.EnterReadLock();
                T result;
                try
                {
                    result = _list[index];
                }
                finally
                {
                    _lock.ExitReadLock();
                }
                return result;
            }
            set
            {
                _lock.EnterWriteLock();
                try
                {
                    _list[index] = value;
                }
                finally
                {
                    _lock.ExitWriteLock();
                }
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return false; 
            }
        }

        public int IndexOf(T item)
        {
            _lock.EnterReadLock();
            int result;
            try
            {
                result = _list.IndexOf(item);
            }
            finally
            {
                _lock.ExitReadLock();
            }
            return result;
        }

        public void Insert(int index, T item)
        {
            _lock.EnterWriteLock();
            try
            {
                _list.Insert(index, item);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public void RemoveAt(int index)
        {
            _lock.EnterWriteLock();
            try
            {
                _list.RemoveAt(index);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public void Clear()
        {
            _lock.EnterWriteLock();
            try
            {
                _list.Clear();
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public bool Contains(T item)
        {
            _lock.EnterReadLock();
            bool result;
            try
            {
                result = _list.Contains(item);
            }
            finally
            {
                _lock.ExitReadLock();
            }
            return result;
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            _lock.EnterWriteLock();
            try
            {
                _list.CopyTo(array, arrayIndex);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public bool Remove(T item)
        {
            _lock.EnterWriteLock();
            bool result;
            try
            {
                result = _list.Remove(item);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
            return result;
        }

        public IEnumerator<T> GetEnumerator()
        {
            _lock.EnterReadLock();
            try
            {
                foreach (T value in _list)
                {
                    yield return value;
                }
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }


  
}
