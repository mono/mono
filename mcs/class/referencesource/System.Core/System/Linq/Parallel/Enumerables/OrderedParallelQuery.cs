// ==++==
//
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
// =+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+
//
// OrderedParallelQuery.cs
//
// <OWNER>[....]</OWNER>
//
// =-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-

using System;
using System.Collections.Generic;
using System.Text;
using System.Linq.Parallel;
using System.Diagnostics.Contracts;

namespace System.Linq
{
    /// <summary>
    /// Represents a sorted, parallel sequence.
    /// </summary>
    public class OrderedParallelQuery<TSource> : ParallelQuery<TSource>
    {
        private QueryOperator<TSource> m_sortOp;

        internal OrderedParallelQuery(QueryOperator<TSource> sortOp)
            :base(sortOp.SpecifiedQuerySettings)
        {
            m_sortOp = sortOp;
            Contract.Assert(sortOp is IOrderedEnumerable<TSource>);
        }

        internal QueryOperator<TSource> SortOperator
        {
            get { return m_sortOp; }
        }

        internal IOrderedEnumerable<TSource> OrderedEnumerable
        {
            get { return (IOrderedEnumerable<TSource>)m_sortOp; }
        }

        /// <summary>
        /// Returns an enumerator that iterates through the sequence.
        /// </summary>
        /// <returns>An enumerator that iterates through the sequence.</returns>
        public override IEnumerator<TSource> GetEnumerator()
        {
         
            return m_sortOp.GetEnumerator();
        }
    }
}
