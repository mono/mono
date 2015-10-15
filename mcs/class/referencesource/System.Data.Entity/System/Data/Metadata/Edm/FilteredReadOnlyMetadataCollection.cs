//---------------------------------------------------------------------
// <copyright file="FilteredReadOnlyMetadataCollection.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       Microsoft
// @backupOwner Microsoft
//---------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Common;
using System.Diagnostics;
using System.Reflection;
using System.Text;

namespace System.Data.Metadata.Edm
{
    internal interface IBaseList<T> : IList
    {
        T this[string identity] { get;}

        new T this[int index] { get;}

        int IndexOf(T item);
    }

#pragma warning disable 1711 // compiler 
    /// <summary>
    /// Class to filter stuff out from a metadata collection
    /// </summary>
    /* 


*/
    internal class FilteredReadOnlyMetadataCollection<TDerived, TBase> : ReadOnlyMetadataCollection<TDerived>, IBaseList<TBase>
                where TDerived : TBase 
                where TBase : MetadataItem
    {
        #region Constructors
        /// <summary>
        /// The constructor for constructing a read-only metadata collection to wrap another MetadataCollection.
        /// </summary>
        /// <param name="collection">The metadata collection to wrap</param>
        /// <exception cref="System.ArgumentNullException">Thrown if collection argument is null</exception>
        /// <param name="predicate">Predicate method which determines membership</param>
        internal FilteredReadOnlyMetadataCollection(ReadOnlyMetadataCollection<TBase> collection, Predicate<TBase> predicate) : base(FilterCollection(collection, predicate))
        {
            Debug.Assert(collection != null);
            Debug.Assert(collection.IsReadOnly, "wrappers should only be created once loading is over, and this collection is still loading");
            _source = collection;
            _predicate = predicate;

        }
        #endregion

        #region Private Fields
        // The original metadata collection over which this filtered collection is the view
        private readonly ReadOnlyMetadataCollection<TBase> _source;
        private readonly Predicate<TBase> _predicate;
        #endregion

        #region Properties

        /// <summary>
        /// Gets an item from the collection with the given identity
        /// </summary>
        /// <param name="identity">The identity of the item to search for</param>
        /// <returns>An item from the collection</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if identity argument passed in is null</exception>
        /// <exception cref="System.NotSupportedException">Thrown if setter is called</exception>
        public override TDerived this[string identity]
        {
            get
            {
                TBase item = _source[identity];
                if (_predicate(item))
                {
                    return (TDerived)item; 
                }
                throw EntityUtil.ItemInvalidIdentity(identity, "identity");
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
        public override TDerived GetValue(string identity, bool ignoreCase)
        {
            TBase item = _source.GetValue(identity, ignoreCase);

            if (_predicate(item))
            {
                return (TDerived)item;
            }
            throw EntityUtil.ItemInvalidIdentity(identity, "identity");
        }

        /// <summary>
        /// Determines if this collection contains an item of the given identity
        /// </summary>
        /// <param name="identity">The identity of the item to check for</param>
        /// <returns>True if the collection contains the item with the given identity</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if identity argument passed in is null</exception>
        /// <exception cref="System.ArgumentException">Thrown if identity argument passed in is empty string</exception>
        public override bool Contains(string identity)
        {
            TBase item;
            if (_source.TryGetValue(identity, false/*ignoreCase*/, out item))
            {
                return (_predicate(item));
            }
            return false;
        }

        /// <summary>
        /// Gets an item from the collection with the given identity
        /// </summary>
        /// <param name="identity">The identity of the item to search for</param>
        /// <param name="ignoreCase">Whether case is ignore in the search</param>
        /// <param name="item">An item from the collection, null if the item is not found</param>
        /// <returns>True an item is retrieved</returns>
        /// <exception cref="System.ArgumentNullException">if identity argument is null</exception>
        public override bool TryGetValue(string identity, bool ignoreCase, out TDerived item)
        {
            item = null; 
            TBase baseTypeItem;
            if (_source.TryGetValue(identity, ignoreCase, out baseTypeItem))
            {
                if (_predicate(baseTypeItem))
                {
                    item = (TDerived)baseTypeItem;
                    return true;
                }
            }
            return false;
        }

        internal static List<TDerived> FilterCollection(ReadOnlyMetadataCollection<TBase> collection, Predicate<TBase> predicate)
        {
            List<TDerived> list = new List<TDerived>(collection.Count);
            foreach (TBase item in collection)
            {
                if (predicate(item))
                {
                    list.Add((TDerived)item);
                }
            }

            return list;
        }

        /// <summary>
        /// Get index of the element passed as the argument
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public override int IndexOf(TDerived value)
        {
            TBase item;
            if (_source.TryGetValue(value.Identity, false /*ignoreCase*/, out item))
            {
                if (_predicate(item))
                {
                    // Since we are gauranteed to have a unique identity per collection, this item must of T Type
                    return base.IndexOf((TDerived)item);
                }
            }
            return -1;
        }

        #endregion

        #region IBaseList<TBaseItem> Members

        TBase IBaseList<TBase>.this[string identity]
        {
            get { return this[identity]; }
        }

        TBase IBaseList<TBase>.this[int index]
        {
            get
            {
                return this[index];
            }
        }

        /// <summary>
        /// Get index of the element passed as the argument
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        int IBaseList<TBase>.IndexOf(TBase item)
        {
            if (_predicate(item))
            {
                return this.IndexOf((TDerived)item);
            }

            return -1;
        }

        #endregion
    }
#pragma warning restore 1711
}
