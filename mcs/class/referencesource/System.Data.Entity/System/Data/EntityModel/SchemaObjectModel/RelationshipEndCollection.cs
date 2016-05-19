//---------------------------------------------------------------------
// <copyright file="RelationshipEndCollection.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       [....]
// @backupOwner [....]
//---------------------------------------------------------------------

namespace System.Data.EntityModel.SchemaObjectModel
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.Metadata.Edm;
    using System.Diagnostics;

    /// <summary>
    /// A collection of RelationshipEnds
    /// </summary>
    internal sealed class RelationshipEndCollection : IList<IRelationshipEnd>
    {
        private Dictionary<string,IRelationshipEnd> _endLookup = null;
        private List<string> _keysInDefOrder = null;

        /// <summary>
        /// construct a RelationshipEndCollection
        /// </summary>
        public RelationshipEndCollection()
        {
        }

        /// <summary>
        /// How many RelationshipEnds are in the collection
        /// </summary>
        public int Count
        {
            get
            {
                return KeysInDefOrder.Count;
            }
        }

        /// <summary>
        /// Add a relationship end
        /// </summary>
        /// <param name="end">the end to add</param>
        public void Add(IRelationshipEnd end)
        {
            Debug.Assert(end != null, "end parameter is null");

            SchemaElement endElement = end as SchemaElement;
            Debug.Assert(endElement != null, "end is not a SchemaElement");

            // this should have been caught before this, just ignore it
            if ( !IsEndValid(end) )
                return;

            if ( !ValidateUniqueName(endElement, end.Name))
                return;

            EndLookup.Add(end.Name,end);
            KeysInDefOrder.Add(end.Name);
        }

        /// <summary>
        /// See if an end can be added to the collection
        /// </summary>
        /// <param name="end">the end to add</param>
        /// <returns>true if the end is valid, false otherwise</returns>
        private static bool IsEndValid(IRelationshipEnd end)
        {
            return !string.IsNullOrEmpty(end.Name);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="end"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        private bool ValidateUniqueName(SchemaElement end, string name)
        {
            if ( EndLookup.ContainsKey(name) )
            {
                end.AddError( ErrorCode.AlreadyDefined, EdmSchemaErrorSeverity.Error,
                    System.Data.Entity.Strings.EndNameAlreadyDefinedDuplicate(name));
                return false;
            }

            return true;
        }

        /// <summary>
        /// Remove a relationship end
        /// </summary>
        /// <param name="end">the end to remove</param>
        /// <returns>true if item was in list</returns>
        public bool Remove(IRelationshipEnd end)
        {
            Debug.Assert(end != null, "end parameter is null");

            if ( !IsEndValid(end) )
                return false;

            KeysInDefOrder.Remove(end.Name);
            bool wasInList = EndLookup.Remove(end.Name);

            return wasInList;
        }

        /// <summary>
        /// See if a relationship end is in the collection
        /// </summary>
        /// <param name="name">the name of the end</param>
        /// <returns>true if the end name is in the collection</returns>
        public bool Contains(string name)
        {
            return EndLookup.ContainsKey(name);
        }

        /// <summary>
        /// See if a relationship end is in the collection
        /// </summary>
        /// <param name="end">the name of the end</param>
        /// <returns>true if the end is in the collection</returns>
        public bool Contains(IRelationshipEnd end)
        {
            Debug.Assert(end != null, "end parameter is null");

            return Contains(end.Name);
        }

        public IRelationshipEnd this[int index]
        {
            get
            {
                return EndLookup[KeysInDefOrder[index]];
            }
            set
            {
                throw EntityUtil.NotSupported();
            }
        }

        /// <summary>
        /// get a typed enumerator for the collection
        /// </summary>
        /// <returns>the enumerator</returns>
        public IEnumerator<IRelationshipEnd> GetEnumerator()
        {
            return new Enumerator(EndLookup,KeysInDefOrder);
        }

        public bool TryGetEnd( string name, out IRelationshipEnd end )
        {
            return EndLookup.TryGetValue( name, out end );
        }

        /// <summary>
        /// get an un-typed enumerator for the collection
        /// </summary>
        /// <returns>the enumerator</returns>
        IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return new Enumerator(EndLookup,KeysInDefOrder);
        }

        /// <summary>
        /// The data for the collection
        /// </summary>
        private Dictionary<string,IRelationshipEnd> EndLookup
        {
            get
            {
                if ( _endLookup == null )
                    _endLookup = new Dictionary<string, IRelationshipEnd>(StringComparer.Ordinal);

                return _endLookup;
            }
        }

        /// <summary>
        /// the definition order collection
        /// </summary>
        private List<string> KeysInDefOrder
        {
            get
            {
                if ( _keysInDefOrder == null )
                    _keysInDefOrder = new List<string>();

                return _keysInDefOrder;
            }
        }

        /// <summary>
        /// remove all elements from the collection
        /// </summary>
        public void Clear()
        {
            EndLookup.Clear();
            KeysInDefOrder.Clear();
        }

        /// <summary>
        /// can the collection be modified
        /// </summary>
        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Not supported
        /// </summary>
        /// <param name="end">the end</param>
        /// <returns>nothing</returns>
        int IList<IRelationshipEnd>.IndexOf(IRelationshipEnd end)
        {
            throw EntityUtil.NotSupported();
        }

        /// <summary>
        /// Not supported
        /// </summary>
        /// <param name="index">the index</param>
        /// <param name="end">the end</param>
        void IList<IRelationshipEnd>.Insert(int index, IRelationshipEnd end)
        {
            throw EntityUtil.NotSupported();
        }

        /// <summary>
        /// Not supported
        /// </summary>
        /// <param name="index">the index</param>
        void IList<IRelationshipEnd>.RemoveAt(int index)
        {
            throw EntityUtil.NotSupported();
        }

        /// <summary>
        /// copy all elements to an array
        /// </summary>
        /// <param name="ends">array to copy to</param>
        /// <param name="index">The zero-based index in array at which copying begins.</param>
        public void CopyTo(IRelationshipEnd[] ends, int index )
        {
            Debug.Assert(ends.Length-index >= Count);
            foreach ( IRelationshipEnd end in this )
                ends[index++] = end;
        }

        /// <summary>
        /// enumerator for the RelationshipEnd collection
        /// the ends as traversed in the order in which they were added
        /// </summary>
        private sealed class Enumerator : IEnumerator<IRelationshipEnd>
        {
            private List<string>.Enumerator _Enumerator;
            private Dictionary<string,IRelationshipEnd> _Data = null;

            /// <summary>
            /// construct the enumerator
            /// </summary>
            /// <param name="data">the real data</param>
            /// <param name="keysInDefOrder">the keys to the real data in inserted order</param>
            public Enumerator(Dictionary<string, IRelationshipEnd> data, List<string> keysInDefOrder)
            {
                Debug.Assert(data != null);
                Debug.Assert(keysInDefOrder != null);
                _Enumerator = keysInDefOrder.GetEnumerator();
                _Data = data;
            }

            /// <summary>
            /// reset the enumerator
            /// </summary>
            public void Reset()
            {
                // reset is implemented explicitly
                ((IEnumerator)_Enumerator).Reset();
            }

            /// <summary>
            /// get current relationship end from the enumerator
            /// </summary>
            public IRelationshipEnd Current
            {
                get
                {
                    return _Data[_Enumerator.Current];
                }
            }

            /// <summary>
            /// get current relationship end from the enumerator
            /// </summary>
            object System.Collections.IEnumerator.Current
            {
                get
                {
                    return _Data[_Enumerator.Current];
                }
            }

            /// <summary>
            /// move to the next element in the collection
            /// </summary>
            /// <returns>true if there is a next, false if not</returns>
            public bool MoveNext()
            {
                return _Enumerator.MoveNext();
            }

            /// <summary>
            /// dispose of the enumerator
            /// </summary>
            public void Dispose()
            {
            }
        }
    }
}
