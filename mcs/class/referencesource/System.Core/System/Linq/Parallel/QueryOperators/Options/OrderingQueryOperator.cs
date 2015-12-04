// ==++==
//
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
// =+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+
//
// OrderingQueryOperator.cs
//
// <OWNER>[....]</OWNER>
//
// =-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-

using System.Collections.Generic;
using System.Threading;

namespace System.Linq.Parallel
{
    /// <summary>
    /// Represents operators AsOrdered and AsUnordered. In the current implementation, it
    /// simply turns on preservation globally in the query. 
    /// </summary>
    /// <typeparam name="TSource"></typeparam>
    internal sealed class OrderingQueryOperator<TSource> : QueryOperator<TSource>
    {
        // Turns on order (AsOrdered) or turns off order (AsUnordered)
        private bool m_orderOn;
        private QueryOperator<TSource> m_child;
        private OrdinalIndexState m_ordinalIndexState;

        public OrderingQueryOperator(QueryOperator<TSource> child, bool orderOn)
            : base(orderOn, child.SpecifiedQuerySettings)
        {
            m_child = child;
            m_ordinalIndexState = m_child.OrdinalIndexState;
            m_orderOn = orderOn;
        }

        internal override QueryResults<TSource> Open(QuerySettings settings, bool preferStriping)
        {
            return m_child.Open(settings, preferStriping);
        }

        internal override IEnumerator<TSource> GetEnumerator(ParallelMergeOptions? mergeOptions, bool suppressOrderPreservation)
        {
            ScanQueryOperator<TSource> childAsScan = m_child as ScanQueryOperator<TSource>;
            if (childAsScan != null)
            {
                return childAsScan.Data.GetEnumerator();
            }
            return base.GetEnumerator(mergeOptions, suppressOrderPreservation);
        }

        //---------------------------------------------------------------------------------------
        // Returns an enumerable that represents the query executing sequentially.
        //

        internal override IEnumerable<TSource> AsSequentialQuery(CancellationToken token)
        {
            return m_child.AsSequentialQuery(token);
        }


        //---------------------------------------------------------------------------------------
        // Whether this operator performs a premature merge that would not be performed in
        // a similar sequential operation (i.e., in LINQ to Objects).
        //
       
        internal override bool LimitsParallelism
        {
            get { return m_child.LimitsParallelism; }
        }

        internal override OrdinalIndexState OrdinalIndexState
        {
            get { return m_ordinalIndexState; }
        }
    }
}
