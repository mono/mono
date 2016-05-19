//---------------------------------------------------------------------
// <copyright file="ReadOnlyMetadataCollection.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       [....]
// @backupOwner [....]
//---------------------------------------------------------------------

using System.Collections;
using System.Collections.Generic;

namespace System.Data.Metadata.Edm
{
    /// <summary>
    /// Class representing a read-only wrapper around MetadataCollection
    /// </summary>
    /// <typeparam name="T">The type of items in this collection</typeparam>
    public class ReadOnlyMetadataCollection<T> : System.Collections.ObjectModel.ReadOnlyCollection<T> where T : MetadataItem
    {
        #region Constructors
        /// <summary>
        /// The constructor for constructing a read-only metadata collection to wrap another MetadataCollection.
        /// </summary>
        /// <param name="collection">The metadata collection to wrap</param>
        /// <exception cref="System.ArgumentNullException">Thrown if collection argument is null</exception>
        internal ReadOnlyMetadataCollection(IList<T> collection) : base(collection)
        {
        }
        #endregion

        #region InnerClasses
        // On the surface, this Enumerator doesn't do anything but delegating to the underlying enumerator

        /// <summary>
        /// The enumerator for MetadataCollection
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes")]
        public struct Enumerator : IEnumerator<T>
        {
            /// <summary>
            /// Constructor for the enumerator
            /// </summary>
            /// <param name="collection">The collection that this enumerator should enumerate on</param>
            internal Enumerator(IList<T> collection)
            {
                _parent = collection;
                _nextIndex = 0;
                _current = null;
            }

            private int _nextIndex;
            private IList<T> _parent;
            private T _current;

            /// <summary>
            /// Gets the member at the current position
            /// </summary>
            public T Current
            {
                get
                {
                    return _current;
                }
            }

            /// <summary>
            /// Gets the member at the current position
            /// </summary>
            object IEnumerator.Current
            {
                get
                {
                    return this.Current;
                }
            }

            /// <summary>
            /// Dispose this enumerator
            /// </summary>
            public void Dispose()
            {
            }

            /// <summary>
            /// Move to the next member in the collection
            /// </summary>
            /// <returns>True if the enumerator is moved</returns>
            public bool MoveNext()
            {
                if ((uint)_nextIndex < (uint)_parent.Count)
                {
                    _current = _parent[_nextIndex];
                    _nextIndex++;
                    return true;
                }

                _current = null;
                return false;
            }

            /// <summary>
            /// Sets the enumerator to the initial position before the first member
            /// </summary>
            public void Reset()
            {
                _current = null;
                _nextIndex = 0;
            }
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets whether the collection is a readonly collection
        /// </summary>
        public bool IsReadOnly
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Gets an item from the collection with the given identity
        /// </summary>
        /// <param name="identity">The identity of the item to search for</param>
        /// <returns>An item from the collection</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if identity argument passed in is null</exception>
        /// <exception cref="System.NotSupportedException">Thrown if setter is called</exception>
        public virtual T this[string identity]
        {
            get
            {
                return (((MetadataCollection<T>)this.Items)[identity]);
            }
        }

        /// <summary>
        /// Returns the metadata collection over which this collection is the view
        /// </summary>
        internal MetadataCollection<T> Source
        {
            get
            {
                return (MetadataCollection<T>)this.Items;
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Gets an item from the collection with the given identity
        /// </summary>
        /// <param name="identity">The identity of the item to search for</param>
        /// <param name="ignoreCase">Whether case is ignore in the search</param>
        /// <returns>An item from the collection</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if identity argument passed in is null</exception>
        /// <exception cref="System.ArgumentException">Thrown if the Collection does not have an item with the given identity</exception>
        public virtual T GetValue(string identity, bool ignoreCase)
        {
            return ((MetadataCollection<T>)this.Items).GetValue(identity, ignoreCase);
        }

        /// <summary>
        /// Determines if this collection contains an item of the given identity
        /// </summary>
        /// <param name="identity">The identity of the item to check for</param>
        /// <returns>True if the collection contains the item with the given identity</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if identity argument passed in is null</exception>
        /// <exception cref="System.ArgumentException">Thrown if identity argument passed in is empty string</exception>
        public virtual bool Contains(string identity)
        {
            return ((MetadataCollection<T>)this.Items).ContainsIdentity(identity);
        }

        /// <summary>
        /// Gets an item from the collection with the given identity
        /// </summary>
        /// <param name="identity">The identity of the item to search for</param>
        /// <param name="ignoreCase">Whether case is ignored in the search</param>
        /// <param name="item">An item from the collection, null if the item is not found</param>
        /// <returns>True an item is retrieved</returns>
        /// <exception cref="System.ArgumentNullException">if identity argument is null</exception>
        public virtual bool TryGetValue(string identity, bool ignoreCase, out T item)
        {
            return ((MetadataCollection<T>)this.Items).TryGetValue(identity, ignoreCase, out item);
        }


        /// <summary>
        /// Gets the enumerator over this collection
        /// </summary>
        /// <returns></returns>
        public new Enumerator GetEnumerator()
        {
            return new Enumerator(this.Items);
        }

        /// <summary>
        /// Workaround for bug 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public new virtual int IndexOf(T value)
        {
            return base.IndexOf(value);
        }

        #endregion
    }
}
