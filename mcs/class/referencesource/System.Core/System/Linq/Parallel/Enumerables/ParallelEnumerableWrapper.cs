// ==++==
//
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
// =+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+
//
// ParallelEnumerableWrapper.cs
//
// <OWNER>[....]</OWNER>
//
// =-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-

using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace System.Linq.Parallel
{
    /// <summary>
    /// A simple implementation of the ParallelQuery{object} interface which wraps an
    /// underlying IEnumerable, such that it can be used in parallel queries.
    /// </summary>
    internal class ParallelEnumerableWrapper : ParallelQuery<object>
    {

        private readonly IEnumerable m_source; // The wrapped enumerable object.

        //-----------------------------------------------------------------------------------
        // Instantiates a new wrapper object.
        //

        internal ParallelEnumerableWrapper(Collections.IEnumerable source)
            : base(QuerySettings.Empty)
        {
            Contract.Assert(source != null);
            m_source = source;
        }

        internal override IEnumerator GetEnumeratorUntyped()
        {
            return m_source.GetEnumerator();
        }

        public override IEnumerator<object> GetEnumerator()
        {
            return new EnumerableWrapperWeakToStrong(m_source).GetEnumerator();
        }
    }
    
    /// <summary>
    /// A simple implementation of the ParallelQuery{T} interface which wraps an
    /// underlying IEnumerable{T}, such that it can be used in parallel queries.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class ParallelEnumerableWrapper<T> : ParallelQuery<T>
    {

        private readonly IEnumerable<T> m_wrappedEnumerable; // The wrapped enumerable object.

        //-----------------------------------------------------------------------------------
        // Instantiates a new wrapper object.
        //
        // Arguments:
        //     wrappedEnumerable   - the underlying enumerable object being wrapped
        //
        // Notes:
        //     The analysisOptions and degreeOfParallelism settings are optional.  Passing null
        //     indicates that the system defaults should be used instead.
        //

        internal ParallelEnumerableWrapper(IEnumerable<T> wrappedEnumerable)
            : base(QuerySettings.Empty)
        {
            Contract.Assert(wrappedEnumerable != null, "wrappedEnumerable must not be null.");

            m_wrappedEnumerable = wrappedEnumerable;
        }

        //-----------------------------------------------------------------------------------
        // Retrieves the wrapped enumerable object.
        //

        internal IEnumerable<T> WrappedEnumerable
        {
            get { return m_wrappedEnumerable; }
        }

        //-----------------------------------------------------------------------------------
        // Implementations of GetEnumerator that just delegate to the wrapped enumerable.
        //

        public override IEnumerator<T> GetEnumerator()
        {
            Contract.Assert(m_wrappedEnumerable != null);
            return m_wrappedEnumerable.GetEnumerator();
        }
    }
}
