//---------------------------------------------------------------------
// <copyright file="FilteredSchemaElementLookUpTable.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       [....]
// @backupOwner [....]
//---------------------------------------------------------------------

namespace System.Data.EntityModel.SchemaObjectModel
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Data;

    /// <summary>
    /// Summary description for FilteredSchemaTypes.
    /// </summary>
    internal sealed class FilteredSchemaElementLookUpTable<T,S> : IEnumerable<T>, ISchemaElementLookUpTable<T>
    where T : S
    where S : SchemaElement
    {
        #region Instance Fields
        private SchemaElementLookUpTable<S> _lookUpTable = null;
        #endregion

        #region Public Methods
        /// <summary>
        /// 
        /// </summary>
        /// <param name="lookUpTable"></param>
        public FilteredSchemaElementLookUpTable(SchemaElementLookUpTable<S> lookUpTable)
        {
            _lookUpTable = lookUpTable;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IEnumerator<T> GetEnumerator()
        {
            return _lookUpTable.GetFilteredEnumerator<T>();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return  _lookUpTable.GetFilteredEnumerator<T>();
        }

        /// <summary>
        /// 
        /// </summary>
        public int Count
        {
            get
            {
                int count = 0;
                foreach ( SchemaElement element  in _lookUpTable )
                {
                    if ( element is T )
                    {
                        ++count;
                    }
                }
                return count;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool ContainsKey(string key)
        {
            if ( !_lookUpTable.ContainsKey(key) )
                return false;
            return _lookUpTable[key] as T != null;
        }
        /// <summary>
        /// 
        /// </summary>
        public T this[string key]
        {
            get
            {
                S element = _lookUpTable[key];
                if ( element == null )
                {
                    return null;
                }
                T elementAsT = element as T;
                if ( elementAsT != null )
                {
                    return elementAsT;
                }
                throw EntityUtil.InvalidOperation(System.Data.Entity.Strings.UnexpectedTypeInCollection(element.GetType(),key));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public T LookUpEquivalentKey(string key)
        {
            return _lookUpTable.LookUpEquivalentKey(key) as T;
        }

     #endregion
    }
}
