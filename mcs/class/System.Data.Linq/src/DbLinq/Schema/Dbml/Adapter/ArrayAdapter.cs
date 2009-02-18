#region MIT license
// 
// MIT license
//
// Copyright (c) 2007-2008 Jiri Moudry, Pascal Craponne
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// 
#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using DbLinq.Util;

namespace DbLinq.Schema.Dbml.Adapter
{
    /// <summary>
    /// ArrayAdapter wraps an IEnumerable as an IList, where items can be dynamically changed.
    /// This is very slow, and should be used with caution.
    /// Maybe we will remove it (and change DbMetal)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [DebuggerDisplay("{reflectedMember}")]
#if MONO_STRICT
    internal
#else
    public
#endif
    class ArrayAdapter<T> : ISimpleList<T>
    {
        protected readonly object Owner;
        protected readonly MemberInfo MemberInfo;
        // just to be debugger friendly
        private object reflectedMember { get { return MemberInfo.GetMemberValue(Owner); } }

        /// <summary>
        /// Returns field value as enumerable
        /// </summary>
        /// <returns></returns>
        protected virtual IEnumerable GetValue()
        {
            return (IEnumerable)MemberInfo.GetMemberValue(Owner);
        }

        /// <summary>
        /// Returns field type
        /// </summary>
        /// <returns></returns>
        protected virtual System.Type GetValueType()
        {
            return MemberInfo.GetMemberType();
        }

        /// <summary>
        /// Sets field as IEnumerable
        /// </summary>
        /// <param name="value"></param>
        protected virtual void SetValue(IEnumerable value)
        {
            MemberInfo.SetMemberValue(Owner, value);
        }

        /// <summary>
        /// Gets target field as a dynamic array
        /// </summary>
        /// <returns></returns>
        protected virtual List<T> GetDynamic()
        {
            var list = new List<T>();
            var fieldValue = GetValue();
            if (fieldValue != null)
            {
                foreach (var o in fieldValue)
                {
                    if (o is T)
                        list.Add((T)o);
                }
            }
            return list;
        }

        /// <summary>
        /// Writes back target field given a list
        /// </summary>
        /// <param name="list"></param>
        protected virtual void SetStatic(IList<T> list)
        {
            var others = new ArrayList();
            var fieldValue = GetValue();
            if (fieldValue != null)
            {
                foreach (var o in fieldValue)
                {
                    if (!(o is T))
                        others.Add(o);
                }
            }
            var array = Array.CreateInstance(GetValueType().GetElementType(), others.Count + list.Count);
            others.CopyTo(array);
            for (int listIndex = 0; listIndex < list.Count; listIndex++)
            {
                array.SetValue(list[listIndex], others.Count + listIndex);
            }
            SetValue(array);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ArrayAdapter&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="o">The o.</param>
        /// <param name="fieldName">Name of the field.</param>
        public ArrayAdapter(object o, string fieldName)
        {
            Owner = o;
            MemberInfo = o.GetType().GetMember(fieldName)[0];
        }

        #region IList<T> Members

        /// <summary>
        /// Returns the index of given item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns></returns>
        public int IndexOf(T item)
        {
            return GetDynamic().IndexOf(item);
        }

        /// <summary>
        /// Inserts the specified item at given index.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="item">The item.</param>
        public void Insert(int index, T item)
        {
            IList<T> dynamicArray = GetDynamic();
            dynamicArray.Insert(index, item);
            SetStatic(dynamicArray);
        }

        /// <summary>
        /// Removes at given index.
        /// </summary>
        /// <param name="index">The index.</param>
        public void RemoveAt(int index)
        {
            IList<T> dynamicArray = GetDynamic();
            dynamicArray.RemoveAt(index);
            SetStatic(dynamicArray);
        }

        /// <summary>
        /// Gets or sets the <see cref="T"/> at the specified index.
        /// </summary>
        /// <value></value>
        public T this[int index]
        {
            get
            {
                return GetDynamic()[index];
            }
            set
            {
                IList<T> dynamicArray = GetDynamic();
                dynamicArray[index] = value;
                SetStatic(dynamicArray);
            }
        }

        #endregion

        #region ICollection<T> Members

        /// <summary>
        /// Append a parameter
        /// </summary>
        /// <param name="item"></param>
        public void Add(T item)
        {
            IList<T> dynamicArray = GetDynamic();
            dynamicArray.Add(item);
            SetStatic(dynamicArray);
        }

        /// <summary>
        /// Clears this instance.
        /// </summary>
        public void Clear()
        {
            SetStatic(new T[0]);
        }

        /// <summary>
        /// Determines whether [contains] [the specified item].
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns>
        /// 	<c>true</c> if [contains] [the specified item]; otherwise, <c>false</c>.
        /// </returns>
        public bool Contains(T item)
        {
            return GetDynamic().Contains(item);
        }

        /// <summary>
        /// Copies to array.
        /// </summary>
        /// <param name="array">The array.</param>
        /// <param name="arrayIndex">Index of the array.</param>
        public void CopyTo(T[] array, int arrayIndex)
        {
            GetDynamic().CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// Items count
        /// </summary>
        /// <value></value>
        public int Count
        {
            get { return GetDynamic().Count; }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is read only.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is read only; otherwise, <c>false</c>.
        /// </value>
        public bool IsReadOnly
        {
            get { return false; }
        }

        /// <summary>
        /// Removes the given item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns></returns>
        public bool Remove(T item)
        {
            IList<T> dynamicArray = GetDynamic();
            bool removed = dynamicArray.Remove(item);
            SetStatic(dynamicArray);
            return removed;
        }

        #endregion

        #region IEnumerable<T> Members

        /// <summary>
        /// Returns an enumerator to enumerate items.
        /// </summary>
        /// <returns>
        /// </returns>
        public IEnumerator<T> GetEnumerator()
        {
            return GetDynamic().GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        /// <summary>
        /// Returns an enumerator to enumerate items.
        /// </summary>
        /// <returns>
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetDynamic().GetEnumerator();
        }

        #endregion

        /// <summary>
        /// Sorts using the specified comparer.
        /// </summary>
        /// <param name="sorter">The sorter.</param>
        public void Sort(IComparer<T> sorter)
        {
            var list = GetDynamic();
            list.Sort(sorter);
            SetStatic(list);
        }

        /// <summary>
        /// Finds all items matching the given predicate.
        /// </summary>
        /// <param name="match">The match.</param>
        /// <returns></returns>
        public List<T> FindAll(Predicate<T> match)
        {
            return GetDynamic().FindAll(match);
        }
    }
}