//---------------------------------------------------------------------
// <copyright file="ObjectResult.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       Microsoft
// @backupowner Microsoft
//---------------------------------------------------------------------

namespace System.Data.Objects
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    
    /// <summary>
    /// This class implements IEnumerable and IDisposable. Instance of this class
    /// is returned from ObjectQuery.Execute method.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
    public abstract class ObjectResult : IEnumerable, IDisposable, IListSource
    {
        internal ObjectResult()
        {
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumeratorInternal();
        }

        // ----------------------
        // IListSource  Properties
        // ----------------------
        /// <summary>
        ///   IListSource.ContainsListCollection implementation. Always returns false.
        /// </summary>
        bool IListSource.ContainsListCollection
        {
            get
            {
                return false; // this means that the IList we return is the one which contains our actual data, it is not a collection
            }
        }

        // ----------------------
        // IListSource  method
        // ----------------------
        /// <summary>
        ///   IListSource.GetList implementation
        /// </summary>
        /// <returns>
        ///   IList interface over the data to bind
        /// </returns>
        IList IListSource.GetList()
        {
            return this.GetIListSourceListInternal();
        }

        public abstract Type ElementType
        {
            get;
        }

        public abstract void Dispose();

        /// <summary>
        ///   Get the next result set of a stored procedure.
        /// </summary>
        /// <returns>
        ///   An ObjectResult that enumerates the values of the next result set.   null, if there are no more, or if the 
        ///   the ObjectResult is not the result of a stored procedure call.
        /// </returns>
        public ObjectResult<TElement> GetNextResult<TElement>()
        {
            return this.GetNextResultInternal<TElement>();
        }

        internal abstract IEnumerator GetEnumeratorInternal();
        internal abstract IList GetIListSourceListInternal();
        internal abstract ObjectResult<TElement> GetNextResultInternal<TElement>();
    }
}
