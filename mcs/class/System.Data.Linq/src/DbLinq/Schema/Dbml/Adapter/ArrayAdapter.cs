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

        public ArrayAdapter(object o, string fieldName)
        {
            Owner = o;
            MemberInfo = o.GetType().GetMember(fieldName)[0];
        }

        #region IList<T> Members

        public int IndexOf(T item)
        {
            return GetDynamic().IndexOf(item);
        }

        public void Insert(int index, T item)
        {
            IList<T> dynamicArray = GetDynamic();
            dynamicArray.Insert(index, item);
            SetStatic(dynamicArray);
        }

        public void RemoveAt(int index)
        {
            IList<T> dynamicArray = GetDynamic();
            dynamicArray.RemoveAt(index);
            SetStatic(dynamicArray);
        }

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

        public void Add(T item)
        {
            IList<T> dynamicArray = GetDynamic();
            dynamicArray.Add(item);
            SetStatic(dynamicArray);
        }

        public void Clear()
        {
            SetStatic(new T[0]);
        }

        public bool Contains(T item)
        {
            return GetDynamic().Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            GetDynamic().CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get { return GetDynamic().Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(T item)
        {
            IList<T> dynamicArray = GetDynamic();
            bool removed = dynamicArray.Remove(item);
            SetStatic(dynamicArray);
            return removed;
        }

        #endregion

        #region IEnumerable<T> Members

        public IEnumerator<T> GetEnumerator()
        {
            return GetDynamic().GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetDynamic().GetEnumerator();
        }

        #endregion

        public void Sort(IComparer<T> sorter)
        {
            var list = GetDynamic();
            list.Sort(sorter);
            SetStatic(list);
        }

        public List<T> FindAll(Predicate<T> match)
        {
            return GetDynamic().FindAll(match);
        }
    }
}