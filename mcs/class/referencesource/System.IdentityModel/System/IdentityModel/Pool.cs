//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.IdentityModel
{
    // see SynchronizedPool<T> for a threadsafe implementation
    class Pool<T> where T : class
    {
        T[] items;
        int count;

        public Pool(int maxCount)
        {
            items = new T[maxCount];
        }

        public int Count
        {
            get { return count; }
        }

        public T Take()
        {
            if (count > 0)
            {
                T item = items[--count];
                items[count] = null;
                return item;
            }
            else
            {
                return null;
            }
        }

        public bool Return(T item)
        {
            if (count < items.Length)
            {
                items[count++] = item;
                return true;
            }
            else
            {
                return false;
            }
        }

        public void Clear()
        {
            for (int i = 0; i < count; i++)
                items[i] = null;
            count = 0;
        }
    }
}
