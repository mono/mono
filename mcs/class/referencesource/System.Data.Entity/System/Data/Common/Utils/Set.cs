//---------------------------------------------------------------------
// <copyright file="Set.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner [....]
// @backupOwner [....]
//---------------------------------------------------------------------


using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Linq;

namespace System.Data.Common.Utils {

    // An interface for a set abstraction
    internal class Set<TElement> : InternalBase, IEnumerable<TElement>
    {
        #region Fields
        /// <summary>
        /// Instance of set value comparer.
        /// </summary>
        internal static readonly IEqualityComparer<Set<TElement>> ValueComparer = 
            new SetValueComparer();

        /// <summary>
        /// Instance of empty set with default comparer.
        /// </summary>
        internal static readonly Set<TElement> Empty = new Set<TElement>().MakeReadOnly();

        private readonly HashSet<TElement> _values;
        private bool _isReadOnly;
        #endregion

        #region Constructors
        /// <summary>
        /// Initialize set with the same values and comparer as other set.
        /// </summary>
        internal Set(Set<TElement> other)
            : this(other._values, other.Comparer)
        {
        }

        /// <summary>
        /// Initialize empty set with default comparer.
        /// </summary>
        internal Set()
            : this(null, null)
        {
        }

        /// <summary>
        /// Initialize a set with the given elements and using default comparer.
        /// </summary>
        internal Set(IEnumerable<TElement> elements)
            : this(elements, null)
        {
        }

        /// <summary>
        /// Initializes an empty set with the given comparer.
        /// </summary>
        internal Set(IEqualityComparer<TElement> comparer)
            : this(null, comparer)
        {
        }

        /// <summary>
        /// Initialize a set with the given elements and comparer.
        /// </summary>
        internal Set(IEnumerable<TElement> elements, IEqualityComparer<TElement> comparer)
        {
            _values = new HashSet<TElement>(
                elements ?? Enumerable.Empty<TElement>(),
                comparer ?? EqualityComparer<TElement>.Default);
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the number of elements in this set.
        /// </summary>
        internal int Count 
        { 
            get 
            { 
                return _values.Count; 
            } 
        }

        /// <summary>
        /// Gets the comparer used to determine equality and hash codes for elements of the set.
        /// </summary>
        internal IEqualityComparer<TElement> Comparer
        {
            get
            {
                return _values.Comparer;
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Determines whether the given element exists in the set.
        /// </summary>
        internal bool Contains(TElement element) 
        { 
            return _values.Contains(element); 
        }

        /// <summary>
        /// Requires: !IsReadOnly
        /// Adds given element to the set. If the set already contains
        /// the element, does nothing.
        /// </summary>
        internal void Add(TElement element) 
        {
            AssertReadWrite();
            _values.Add(element); 
        }

        /// <summary>
        /// Requires: !IsReadOnly
        /// Adds given elements to the set. If the set already contains
        /// one of the elements, does nothing.
        /// </summary>
        internal void AddRange(IEnumerable<TElement> elements)
        {
            AssertReadWrite();
            foreach (TElement element in elements)
            {
                Add(element);
            }
        }

        /// <summary>
        /// Requires: !IsReadOnly
        /// Removes given element from the set. If the set does not contain
        /// the element, does nothing.
        /// </summary>
        internal void Remove(TElement element)
        {
            AssertReadWrite();
            _values.Remove(element);
        }

        /// <summary>
        /// Requires: !IsReadOnly
        /// Removes all elements from the set.
        /// </summary>
        internal void Clear()
        {
            AssertReadWrite();
            _values.Clear();
        }

        /// <summary>
        /// Returns an array containing all elements of the set. Order is arbitrary.
        /// </summary>
        internal TElement[] ToArray()
        {
            return _values.ToArray();
        }

        /// <summary>
        /// Requires: other set must not be null and must have the same comparer.
        /// Returns true if this set contains the same elements as the other set.
        /// </summary>
        internal bool SetEquals(Set<TElement> other)
        {
            AssertSetCompatible(other);
            return _values.Count == other._values.Count
                && _values.IsSubsetOf(other._values);
        }

        /// <summary>
        /// Requires: other set must not be null and must have the same comparer.
        /// Returns true if all elements in this set are contained in the other set.
        /// </summary>
        internal bool IsSubsetOf(Set<TElement> other)
        {
            AssertSetCompatible(other);
            return _values.IsSubsetOf(other._values);
        }

        /// <summary>
        /// Requires: other set must not be null and must have the same comparer.
        /// Returns true if this set and other set have some elements in common.
        /// </summary>
        internal bool Overlaps(Set<TElement> other)
        {
            AssertSetCompatible(other);
            return _values.Overlaps(other._values);
        }

        /// <summary>
        /// Requires: !IsReadOnly
        /// Requires: other collection must not be null.
        /// Subtracts other set from this set, leaving the result in this.
        /// </summary>
        internal void Subtract(IEnumerable<TElement> other)
        {
            AssertReadWrite();
            _values.ExceptWith(other);
        }

        /// <summary>
        /// Requires: other collection must not be null.
        /// Subtracts other set from this set, returning result.
        /// </summary>
        internal Set<TElement> Difference(IEnumerable<TElement> other)
        {
            Set<TElement> copy = new Set<TElement>(this);
            copy.Subtract(other);
            return copy;
        }

        /// <summary>
        /// Requires: !IsReadOnly
        /// Requires: other collection must not be null.
        /// Unions other set with this set, leaving the result in this set.
        /// </summary>
        internal void Unite(IEnumerable<TElement> other)
        {
            AssertReadWrite();
            _values.UnionWith(other);
        }

        /// <summary>
        /// Requires: other collection must not be null.
        /// Unions other set with this set, returning the result.
        /// </summary>
        internal Set<TElement> Union(IEnumerable<TElement> other)
        {
            Set<TElement> copy = new Set<TElement>(this);
            copy.Unite(other);
            return copy;
        }

        /// <summary>
        /// Requires: !IsReadOnly
        /// Requires: other set must not be null and must have the same comparer.
        /// Intersects this set and other set, leaving the result in this set.
        /// </summary>
        internal void Intersect(Set<TElement> other)
        {
            AssertReadWrite();
            AssertSetCompatible(other);
            _values.IntersectWith(other._values);
        }

        /// <summary>
        /// Returns a readonly version of this set.
        /// </summary>
        internal Set<TElement> AsReadOnly()
        {
            if (_isReadOnly)
            {
                // once it's readonly, it's always readonly
                return this;
            }
            Set<TElement> copy = new Set<TElement>(this);
            copy._isReadOnly = true;
            return copy;
        }

        /// <summary>
        /// Makes this set readonly and returns this set.
        /// </summary>
        internal Set<TElement> MakeReadOnly()
        {
            _isReadOnly = true;
            return this;
        }

        /// <summary>
        /// Returns aggregate hash code of all elements in this set.
        /// </summary>
        internal int GetElementsHashCode()
        {
            int hashCode = 0;
            foreach (TElement element in this)
            {
                hashCode ^= Comparer.GetHashCode(element);
            }
            return hashCode;
        }

        /// <summary>
        /// Returns typed enumerator over elements of the set. 
        /// Uses HashSet&lt;TElement&gt;.Enumerator to avoid boxing struct.
        /// </summary>
        public HashSet<TElement>.Enumerator GetEnumerator() 
        { 
            return _values.GetEnumerator(); 
        }

        [Conditional("DEBUG")]
        private void AssertReadWrite()
        {
            Debug.Assert(!_isReadOnly, "attempting to modify readonly collection");
        }

        [Conditional("DEBUG")]
        private void AssertSetCompatible(Set<TElement> other)
        {
            Debug.Assert(other != null, "other set null");
            Debug.Assert(other.Comparer.GetType().Equals(this.Comparer.GetType()));
        }
        #endregion

        #region IEnumerable<TElement> Members

        public class Enumerator : IEnumerator<TElement>
        {
            private Dictionary<TElement, bool>.KeyCollection.Enumerator keys;

            internal Enumerator(Dictionary<TElement, bool>.KeyCollection.Enumerator keys)
            {
                this.keys = keys;
            }

            public TElement Current { get { return keys.Current; } }

            public void Dispose() { keys.Dispose(); }

            object IEnumerator.Current { get { return ((IEnumerator)keys).Current; } }

            public bool MoveNext() { return keys.MoveNext(); }

            void System.Collections.IEnumerator.Reset() { ((System.Collections.IEnumerator)keys).Reset(); }
        }

        IEnumerator<TElement> IEnumerable<TElement>.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members
        /// <summary>
        /// Returns an untyped enumeration of elements in the set.
        /// </summary>
        /// <returns>Enumeration of set members.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        #endregion

        #region InternalBase
        internal override void ToCompactString(StringBuilder builder)
        {
            StringUtil.ToCommaSeparatedStringSorted(builder, this);
        }
        #endregion

        #region Nested types
        private class SetValueComparer : IEqualityComparer<Set<TElement>>
        {
            bool IEqualityComparer<Set<TElement>>.Equals(Set<TElement> x, Set<TElement> y)
            {
                Debug.Assert(null != x && null != y, "comparer must be used only in context of Dictionary/HashSet");
                return x.SetEquals(y);
            }

            int IEqualityComparer<Set<TElement>>.GetHashCode(Set<TElement> obj)
            {
                Debug.Assert(null != obj, "comparer must be used only in context of Dictionary/HashSet");
                return obj.GetElementsHashCode();
            }
        }
        #endregion
    }
}
