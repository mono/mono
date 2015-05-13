/******************************************************************************
* The MIT License
* Copyright (c) 2007 Novell Inc.,  www.novell.com
*
* Permission is hereby granted, free of charge, to any person obtaining  a copy
* of this software and associated documentation files (the Software), to deal
* in the Software without restriction, including  without limitation the rights
* to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
* copies of the Software, and to  permit persons to whom the Software is
* furnished to do so, subject to the following conditions:
*
* The above copyright notice and this permission notice shall be included in
* all copies or substantial portions of the Software.
*
* THE SOFTWARE IS PROVIDED AS IS, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
* IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
* AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
* OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
* SOFTWARE.
*******************************************************************************/

// Authors:
// 		Thomas Wiest (twiest@novell.com)
//		Rusty Howell  (rhowell@novell.com)
//
// (C)  Novell Inc.
//
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace System.Management.Internal.BaseDataTypes
{
    /// <summary>
    /// Base collection type for all Cim Collections
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class BaseCollection<T> : IEnumerable<T>
    {
        protected List<T> items;

        #region Constructors
        /// <summary>
        /// Creates an empty collection
        /// </summary>
        public BaseCollection()
        {
            items = new List<T>();
        }

        /// <summary>
        /// Creates an empty collection with a set capacity
        /// </summary>
        /// <param name="capacity"></param>
        public BaseCollection(int capacity)
        {            
            items = new List<T>(capacity);
        }
        #endregion

        #region Properties and Indexers
        /// <summary>
        /// Gets or set the capacity of the collection
        /// </summary>
        public int Capacity
        {
            get { return items.Capacity; }
            set { items.Capacity = value; }
        }

        /// <summary>
        /// Returns the number of items in the collection
        /// </summary>
        public int Count
        {
            get { return items.Count; }
        }

        /// <summary>
        /// Gets or sets the indexed item in the collection
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public T this[int index]
        {
            get { return items[index]; }
            set { items[index] = value; }
        }

        /// <summary>
        /// Returns true if count is greater than zero
        /// </summary>
        public bool IsSet
        {
            get { return (Count > 0); }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Adds an item to the collection
        /// </summary>
        /// <param name="item"></param>
        public void Add(T item)
        {
            items.Add(item);
        }

        /// <summary>
        /// Adds the contents of a BaseCollection&lt;T&gt; to the collection
        /// </summary>
        /// <param name="baseCollection"></param>
        public void AddRange(BaseCollection<T> baseCollection)
        {
            items.AddRange(baseCollection.items);
        }

        /// <summary>
        /// Inserts an item into the collection at the specified index
        /// </summary>
        /// <param name="index"></param>
        /// <param name="item"></param>
        public void Insert(int index, T item)
        {
            items.Insert(index, item);
        }

        /// <summary>
        /// Inserts a collection into another collection at the specified index
        /// </summary>
        /// <param name="index"></param>
        /// <param name="baseCollection"></param>
        public void InsertRange(int index, BaseCollection<T> baseCollection)
        {
            items.InsertRange(index, baseCollection.items);            
        }

        /// <summary>
        /// Removes the specified item from the collection
        /// </summary>
        /// <param name="item"></param>
        public void Remove(T item)
        {
            items.Remove(item);
        }

        /// <summary>
        /// Removes an element at the specified index
        /// </summary>
        /// <param name="index"></param>
        public void RemoveAt(int index)
        {
            items.RemoveAt(index);
        }

        /// <summary>
        /// Removes a range of elements from the collection
        /// </summary>
        /// <param name="index"></param>
        /// <param name="count"></param>
        public void RemoveRange(int index, int count)
        {
            items.RemoveRange(index, count);
        }
        #endregion

        #region Interface Stuff

        #region IEnumerable<T> Members
        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return items.GetEnumerator();
        }
        #endregion

        #region IEnumerable Members
        IEnumerator IEnumerable.GetEnumerator()
        {
            return items.GetEnumerator();
        }
        #endregion

        #endregion
    }
}
