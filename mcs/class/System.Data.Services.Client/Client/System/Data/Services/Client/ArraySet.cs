//Copyright 2010 Microsoft Corporation
//
//Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
//You may obtain a copy of the License at 
//
//http://www.apache.org/licenses/LICENSE-2.0 
//
//Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
//"AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
//See the License for the specific language governing permissions and limitations under the License.



namespace System.Data.Services.Client
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;

    [DebuggerDisplay("Count = {count}")]
    internal struct ArraySet<T> : IEnumerable<T> where T : class
    {
        private T[] items;

        private int count;

        private int version;

        public ArraySet(int capacity)
        {
            this.items = new T[capacity];
            this.count = 0;
            this.version = 0;
        }

        public int Count
        {
            get { return this.count; }
        }

        public T this[int index]
        {
            get
            {
                Debug.Assert(index < this.count);
                return this.items[index];
            }
        }

        public bool Add(T item, Func<T, T, bool> equalityComparer)
        {
            if ((null != equalityComparer) && this.Contains(item, equalityComparer))
            {
                return false;
            }

            int index = this.count++;
            if ((null == this.items) || (index == this.items.Length))
            {                Array.Resize<T>(ref this.items, Math.Min(Math.Max(index, 16), Int32.MaxValue / 2) * 2);
            }

            this.items[index] = item;
            unchecked
            {
                this.version++;
            }

            return true;
        }

        public bool Contains(T item, Func<T, T, bool> equalityComparer)
        {
            return (0 <= this.IndexOf(item, equalityComparer));
        }

        public IEnumerator<T> GetEnumerator()
        {
            for (int i = 0; i < this.count; ++i)
            {
                yield return this.items[i];
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
        
        public int IndexOf(T item, Func<T, T, bool> comparer)
        {
            return this.IndexOf(item, IdentitySelect, comparer);
        }

        public int IndexOf<K>(K item, Func<T, K> select, Func<K, K, bool> comparer)
        {
            T[] array = this.items;
            if (null != array)
            {
                int length = this.count;
                for (int i = 0; i < length; ++i)
                {
                    if (comparer(item, select(array[i])))
                    {
                        return i;
                    }
                }
            }

            return -1;
        }

        public T Remove(T item, Func<T, T, bool> equalityComparer)
        {
            int index = this.IndexOf(item, equalityComparer);
            if (0 <= index)
            {
                item = this.items[index];
                this.RemoveAt(index);
                return item;
            }

            return default(T);
        }

        public void RemoveAt(int index)
        {
            Debug.Assert(unchecked((uint)index < (uint)this.count), "index out of range");
            T[] array = this.items;
            int lastIndex = --this.count;
            array[index] = array[lastIndex];
            array[lastIndex] = default(T);

            if ((0 == lastIndex) && (256 <= array.Length))
            {
                this.items = null;
            }
            else if ((256 < array.Length) && (lastIndex < array.Length / 4))
            {                Array.Resize(ref this.items, array.Length / 2);
            }

            unchecked
            {
                this.version++;
            }
        }

        public void Sort<K>(Func<T, K> selector, Func<K, K, int> comparer)
        {
            if (null != this.items)
            {
                SelectorComparer<K> scomp;
                scomp.Selector = selector;
                scomp.Comparer = comparer;
                Array.Sort<T>(this.items, 0, this.count, scomp);
            }
        }

        public void TrimToSize()
        {
            Array.Resize(ref this.items, this.count);
        }

        private static T IdentitySelect(T arg)
        {
            return arg;
        }

        private struct SelectorComparer<K> : IComparer<T>
        {
            internal Func<T, K> Selector;

            internal Func<K, K, int> Comparer;

            int IComparer<T>.Compare(T x, T y)
            {
                return this.Comparer(this.Selector(x), this.Selector(y));
            }
        }
    }
}
