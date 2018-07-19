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
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Data;
    using System.Data.Common;
    using System.Data.Common.Utils;
    using System.Data.Metadata.Edm;
    using System.Data.Mapping;
    using System.Data.Objects.DataClasses;
    using System.Diagnostics;
    using System.Linq.Expressions;
    using System.Data.Common.Internal.Materialization;
        
    /// <summary>
    /// This class implements IEnumerable of T and IDisposable. Instance of this class
    /// is returned from ObjectQuery&lt;T&gt;.Execute method.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
    public sealed class ObjectResult<T> : ObjectResult, IEnumerable<T>
    {
        private Shaper<T> _shaper;
        private DbDataReader _reader;
        private readonly EntitySet _singleEntitySet;
        private readonly TypeUsage _resultItemType;
        private readonly bool _readerOwned;
        private IBindingList _cachedBindingList;
        private NextResultGenerator _nextResultGenerator;
        private Action<object, EventArgs> _onReaderDispose;

        internal ObjectResult(Shaper<T> shaper, EntitySet singleEntitySet, TypeUsage resultItemType)
            : this(shaper, singleEntitySet, resultItemType, true)
        {
        }

        internal ObjectResult(Shaper<T> shaper, EntitySet singleEntitySet, TypeUsage resultItemType, bool readerOwned)
            : this(shaper, singleEntitySet, resultItemType, readerOwned, null, null)
        {
        }

        internal ObjectResult(Shaper<T> shaper, EntitySet singleEntitySet, TypeUsage resultItemType, bool readerOwned, NextResultGenerator nextResultGenerator, Action<object, EventArgs> onReaderDispose)
        {
            _shaper = shaper;
            _reader = _shaper.Reader;
            _singleEntitySet = singleEntitySet;
            _resultItemType = resultItemType;
            _readerOwned = readerOwned;
            _nextResultGenerator = nextResultGenerator;
            _onReaderDispose = onReaderDispose;
        }

        private void EnsureCanEnumerateResults()
        {
            if (null == _shaper)
            {
                // Enumerating more than once is not allowed.
                throw EntityUtil.CannotReEnumerateQueryResults();
            }
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection. 
        /// </summary>
        public IEnumerator<T> GetEnumerator()
        {
            EnsureCanEnumerateResults();

            Shaper<T> shaper = _shaper;
            _shaper = null;
            IEnumerator<T> result = shaper.GetEnumerator();
            return result;
        }

        /// <summary>
        /// Performs tasks associated with freeing, releasing, or resetting resources. 
        /// </summary>
        public override void Dispose()
        {
            DbDataReader reader = _reader;
            _reader = null;
            _nextResultGenerator = null;

            if (null != reader && _readerOwned)
            {
                reader.Dispose();
                if (_onReaderDispose != null)
                {
                    _onReaderDispose(this, new EventArgs());
                    _onReaderDispose = null;
                }
            }
            if (_shaper != null)
            {
                // This case includes when the ObjectResult is disposed before it 
                // created an ObjectQueryEnumeration; at this time, the connection can be released
                if (_shaper.Context != null && _readerOwned)
                {
                    _shaper.Context.ReleaseConnection();
                }
                _shaper = null;
            }
        }

        internal override IEnumerator GetEnumeratorInternal()
        {
            return ((IEnumerable<T>)this).GetEnumerator();
        }

        internal override IList GetIListSourceListInternal()
        {
            // You can only enumerate the query results once, and the creation of an ObjectView consumes this enumeration.
            // However, there are situations where setting the DataSource of a control can result in multiple calls to this method.
            // In order to enable this scenario and allow direct binding to the ObjectResult instance, 
            // the ObjectView is cached and returned on subsequent calls to this method.

            if (_cachedBindingList == null)
            {
                EnsureCanEnumerateResults();

                bool forceReadOnly = this._shaper.MergeOption == MergeOption.NoTracking;
                _cachedBindingList = ObjectViewFactory.CreateViewForQuery<T>(this._resultItemType, this, this._shaper.Context, forceReadOnly, this._singleEntitySet);
            }

            return _cachedBindingList;
        }

        internal override ObjectResult<TElement> GetNextResultInternal<TElement>()
        {
            return null != _nextResultGenerator ? _nextResultGenerator.GetNextResult<TElement>(_reader) : null;
        }

        public override Type ElementType
        {
            get { return typeof(T); }
        }
    }
}
