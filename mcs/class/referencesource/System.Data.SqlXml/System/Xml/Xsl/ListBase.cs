//------------------------------------------------------------------------------
// <copyright file="ListBase.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">Microsoft</owner>
//------------------------------------------------------------------------------
using System;
using System.Xml;
using System.Xml.Schema;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace System.Xml.Xsl {

    /// <summary>
    /// Implementation of read-only IList and IList<T> interfaces.  Derived classes can inherit from
    /// this class and implement only two methods, Count and Item, rather than the entire IList interface.
    /// </summary>
    internal abstract class ListBase<T> : IList<T>, System.Collections.IList {
        //-----------------------------------------------
        // Abstract IList methods that must be
        // implemented by derived classes
        //-----------------------------------------------
        public abstract int Count { get; }
        public abstract T this[int index] { get; set; }


        //-----------------------------------------------
        // Implemented by base class -- accessible on
        // ListBase
        //-----------------------------------------------
        public virtual bool Contains(T value) {
            return IndexOf(value) != -1;
        }

        public virtual int IndexOf(T value) {
            for (int i = 0; i < Count; i++)
                if (value.Equals(this[i]))
                    return i;

            return -1;
        }

        public virtual void CopyTo(T[] array, int index) {
            for (int i = 0; i < Count; i++)
                array[index + i] = this[i];
        }

        public virtual IListEnumerator<T> GetEnumerator() {
            return new IListEnumerator<T>(this);
        }

        public virtual bool IsFixedSize {
            get { return true; }
        }

        public virtual bool IsReadOnly {
            get { return true; }
        }

        public virtual void Add(T value) {
            Insert(Count, value);
        }

        public virtual void Insert(int index, T value) {
            throw new NotSupportedException();
        }

        public virtual bool Remove(T value) {
            int index = IndexOf(value);
            if (index >= 0) {
                RemoveAt(index);
                return true;
            }
            return false;
        }

        public virtual void RemoveAt(int index) {
            throw new NotSupportedException();
        }

        public virtual void Clear() {
            for (int index = Count - 1; index >= 0; index--)
                RemoveAt(index);
        }


        //-----------------------------------------------
        // Implemented by base class -- only accessible
        // after casting to IList
        //-----------------------------------------------
        IEnumerator<T> IEnumerable<T>.GetEnumerator() {
            return new IListEnumerator<T>(this);
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
            return new IListEnumerator<T>(this);
        }

        bool System.Collections.ICollection.IsSynchronized {
            get { return IsReadOnly; }
        }

        object System.Collections.ICollection.SyncRoot {
            get { return this; }
        }

        void System.Collections.ICollection.CopyTo(Array array, int index) {
            for (int i = 0; i < Count; i++)
                array.SetValue(this[i], index);
        }

        object System.Collections.IList.this[int index] {
            get { return this[index]; }
            set {
                if (!IsCompatibleType(value.GetType()))
                    throw new ArgumentException(Res.GetString(Res.Arg_IncompatibleParamType), "value");

                this[index] = (T) value;
            }
        }

        int System.Collections.IList.Add(object value) {
            if (!IsCompatibleType(value.GetType()))
                throw new ArgumentException(Res.GetString(Res.Arg_IncompatibleParamType), "value");

            Add((T) value);
            return Count - 1;
        }

        void System.Collections.IList.Clear() {
            Clear();
        }
    
        bool System.Collections.IList.Contains(object value) {
            if (!IsCompatibleType(value.GetType()))
                return false;

            return Contains((T) value);
        }

        int System.Collections.IList.IndexOf(object value) {
            if (!IsCompatibleType(value.GetType()))
                return -1;

            return IndexOf((T) value);
        }

        void System.Collections.IList.Insert(int index, object value) {
            if (!IsCompatibleType(value.GetType()))
                throw new ArgumentException(Res.GetString(Res.Arg_IncompatibleParamType), "value");
           
            Insert(index, (T) value);
        }

        void System.Collections.IList.Remove(object value) {
            if (IsCompatibleType(value.GetType())) {
                Remove((T)value);
            }
        }


        //-----------------------------------------------
        // Helper methods and classes
        //-----------------------------------------------

        private static bool IsCompatibleType(object value) {
            if((value == null && !typeof(T).IsValueType) || (value is T))
                return true;

            return false;
        }
    }

    /// <summary>
    /// Implementation of IEnumerator<T> and IEnumerator over an IList<T>.
    /// </summary>
    internal struct IListEnumerator<T> : IEnumerator<T>, System.Collections.IEnumerator {
        private IList<T> sequence;
        private int index;
        private T current;

        /// <summary>
        /// Constructor.
        /// </summary>
        public IListEnumerator(IList<T> sequence) {
            this.sequence = sequence;
            this.index = 0;
            this.current = default(T);
        }

        /// <summary>
        /// No-op.
        /// </summary>
        public void Dispose() {
        }

        /// <summary>
        /// Return current item.  Return default value if before first item or after last item in the list.
        /// </summary>
        public T Current {
            get { return this.current; }
        }

        /// <summary>
        /// Return current item.  Throw exception if before first item or after last item in the list.
        /// </summary>
        object System.Collections.IEnumerator.Current {
            get {
                if (this.index == 0)
                    throw new InvalidOperationException(Res.GetString(Res.Sch_EnumNotStarted, string.Empty));

                if (this.index > this.sequence.Count)
                    throw new InvalidOperationException(Res.GetString(Res.Sch_EnumFinished, string.Empty));

                return this.current;
            }
        }

        /// <summary>
        /// Advance enumerator to next item in list.  Return false if there are no more items.
        /// </summary>
        public bool MoveNext() {
            if (this.index < this.sequence.Count) {
                this.current = this.sequence[this.index];
                this.index++;
                return true;
            }

            this.current = default(T);
            return false;
        }

        /// <summary>
        /// Set the enumerator to its initial position, which is before the first item in the list.
        /// </summary>
        void System.Collections.IEnumerator.Reset() {
            this.index = 0;
            this.current = default(T);
        }
    }
}
