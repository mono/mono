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


using System;
using System.Collections.Generic;
using System.Text;

namespace System.Management.Internal.BaseDataTypes
{
    /// <summary>
    /// Base data type list for all Cim Collections
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class BaseDataTypeList<T> : BaseCollection<T>
    {
        #region Constructors
        /// <summary>
        /// Creates an empty BaseDataTypeList of type T
        /// </summary>
        public BaseDataTypeList()
        {
        }

        /// <summary>
        /// Creates an empty BaseDataTypeList of type T with a specified capacity
        /// </summary>
        /// <param name="capacity"></param>
        public BaseDataTypeList(int capacity):base(capacity)
        {            
        }

        /// <summary>
        /// Creates a BaseDataTypeList with the given items
        /// </summary>
        /// <param name="items"></param>
        public BaseDataTypeList(T[] items)
        {
            for (int i = 0; i < items.Length; ++i)
            {
                this.Add(items[i]);
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Sort the items of the list according to ISortable
        /// </summary>
        public void Sort()
        {
            items.Sort();
        }

        /// <summary>
        /// Sort the list according to the given Comparison object
        /// </summary>
        /// <param name="comparison"></param>
        public void Sort(Comparison<T> comparison)
        {
            items.Sort(comparison);
        }

        /// <summary>
        /// Sort the list according to the given IComparer object
        /// </summary>
        /// <param name="comparer"></param>
        public void Sort(IComparer<T> comparer)
        {
            items.Sort(comparer);
        }

        /// <summary>
        /// Sorts a specified range of elements according to the IComparer object
        /// </summary>
        /// <param name="index">Starting index of the range to sort</param>
        /// <param name="count">Number of items in the range</param>
        /// <param name="comparer">IComparer object to use to compare</param>
        public void Sort(int index, int count, IComparer<T> comparer)
        {
            items.Sort(index, count, comparer);
        }
        #endregion
    }
}
