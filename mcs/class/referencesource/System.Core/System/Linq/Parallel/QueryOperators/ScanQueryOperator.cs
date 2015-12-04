// ==++==
//
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
// =+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+
//
// ScanQueryOperator.cs
//
// <OWNER>[....]</OWNER>
//
// =-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Threading;

namespace System.Linq.Parallel
{
    /// <summary>
    /// A scan is just a simple operator that is positioned directly on top of some
    /// real data source. It's really just a place holder used during execution and
    /// analysis -- it should never actually get opened.
    /// </summary>
    /// <typeparam name="TElement"></typeparam>
    internal sealed class ScanQueryOperator<TElement> : QueryOperator<TElement>
    {

        private readonly IEnumerable<TElement> m_data; // The actual data source to scan.

        //-----------------------------------------------------------------------------------
        // Constructs a new scan on top of the target data source.
        //

        internal ScanQueryOperator(IEnumerable<TElement> data)
            : base(Scheduling.DefaultPreserveOrder, QuerySettings.Empty)
        {
            Contract.Assert(data != null);

            ParallelEnumerableWrapper<TElement> wrapper = data as ParallelEnumerableWrapper<TElement>;
            if (wrapper != null)
            {
                data = wrapper.WrappedEnumerable;
            }

            m_data = data;
        }

        //-----------------------------------------------------------------------------------
        // Accesses the underlying data source.
        //

        public IEnumerable<TElement> Data
        {
            get { return m_data; }
        }

        //-----------------------------------------------------------------------------------
        // Override of the query operator base class's Open method. It creates a partitioned
        // stream that reads scans the data source.
        //

        internal override QueryResults<TElement> Open(QuerySettings settings, bool preferStriping)
        {
            Contract.Assert(settings.DegreeOfParallelism.HasValue);

            IList<TElement> dataAsList = m_data as IList<TElement>;
            if (dataAsList != null)
            {
                return new ListQueryResults<TElement>(dataAsList, settings.DegreeOfParallelism.GetValueOrDefault(), preferStriping);
            }
            else
            {
                return new ScanEnumerableQueryOperatorResults(m_data, settings);
            }
        }


        //-----------------------------------------------------------------------------------
        // IEnumerable<T> data source represented as QueryResults<T>. Typically, we would not
        // use ScanEnumerableQueryOperatorResults if the data source implements IList<T>.
        //

        internal override IEnumerator<TElement> GetEnumerator(ParallelMergeOptions? mergeOptions, bool suppressOrderPreservation)
        {
            return m_data.GetEnumerator();
        }


        //---------------------------------------------------------------------------------------
        // Returns an enumerable that represents the query executing sequentially.
        //

        internal override IEnumerable<TElement> AsSequentialQuery(CancellationToken token)
        {
            return m_data;
        }

        //---------------------------------------------------------------------------------------
        // The state of the order index of the results returned by this operator.
        //

        internal override OrdinalIndexState OrdinalIndexState
        {
            get
            {
                return m_data is IList<TElement> 
                    ? OrdinalIndexState.Indexible
                    : OrdinalIndexState.Correct;
            }
        }


        //---------------------------------------------------------------------------------------
        // Whether this operator performs a premature merge that would not be performed in
        // a similar sequential operation (i.e., in LINQ to Objects).
        //

        internal override bool LimitsParallelism
        {
            get { return false; }
        }

        private class ScanEnumerableQueryOperatorResults : QueryResults<TElement>
        {
            private IEnumerable<TElement> m_data; // The data source for the query

            private QuerySettings m_settings; // Settings collected from the query

            internal ScanEnumerableQueryOperatorResults(IEnumerable<TElement> data, QuerySettings settings)
            {
                m_data = data;
                m_settings = settings;
            }

            internal override void GivePartitionedStream(IPartitionedStreamRecipient<TElement> recipient)
            {
                // Since we are not using m_data as an IList, we can pass useStriping = false.
                PartitionedStream<TElement, int> partitionedStream = ExchangeUtilities.PartitionDataSource(
                    m_data, m_settings.DegreeOfParallelism.Value, false);
                recipient.Receive<int>(partitionedStream);
            }
        }
    }
}
