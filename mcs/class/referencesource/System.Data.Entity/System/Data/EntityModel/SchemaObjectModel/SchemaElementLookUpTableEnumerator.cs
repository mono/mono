//---------------------------------------------------------------------
// <copyright file="SchemaElementLookupTableEnumerator.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       Microsoft
// @backupOwner Microsoft
//---------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;

namespace System.Data.EntityModel.SchemaObjectModel
{
    /// <summary>
    /// Summary description for SchemaElementLookUpTableEnumerator.
    /// </summary>
    internal sealed class SchemaElementLookUpTableEnumerator<T,S>: IEnumerator<T>
    where T : S
    where S : SchemaElement
    {
        #region Instance Fields
        private Dictionary<string,S> _data = null;
        private List<string>.Enumerator _enumerator;
        #endregion

        #region Public Methods
        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <param name="keysInOrder"></param>
        public SchemaElementLookUpTableEnumerator(Dictionary<string,S> data,List<string> keysInOrder)
        {
            Debug.Assert(data != null, "data parameter is null");
            Debug.Assert(keysInOrder != null, "keysInOrder parameter is null");

            _data = data;
            _enumerator = keysInOrder.GetEnumerator();
        }
        #endregion

        #region IEnumerator Members
        /// <summary>
        /// 
        /// </summary>
        public void Reset()
        {
            // it is implemented explicitly
            ((IEnumerator)_enumerator).Reset();
        }

        /// <summary>
        /// 
        /// </summary>
        public T Current
        {
            get
            {
                string key = _enumerator.Current;
                return _data[key] as T;
            }
        }

        object System.Collections.IEnumerator.Current
        {
            get
            {
                string key = _enumerator.Current;
                return _data[key] as T;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool MoveNext()
        {
            while ( _enumerator.MoveNext() )
            {
                if ( Current != null )
                    return true;
            }
            return false;
        }
        #endregion

        #region IDisposable Members
        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
        }
        #endregion
    }
}
