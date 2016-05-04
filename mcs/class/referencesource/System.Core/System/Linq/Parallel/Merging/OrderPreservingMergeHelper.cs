// ==++==
//
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
// =+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+
//
// OrderPreservingMergeHelper.cs
//
// <OWNER>[....]</OWNER>
//
// =-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-

using System.Collections.Generic;
using System.Threading.Tasks;
using System.Diagnostics.Contracts;

namespace System.Linq.Parallel
{
    /// <summary>
    /// The order preserving merge helper guarantees the output stream is in a specific order. This is done
    /// by comparing keys from a set of already-sorted input partitions, and coalescing output data using
    /// incremental key comparisons.
    /// </summary>
    /// <typeparam name="TInputOutput"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    internal class OrderPreservingMergeHelper<TInputOutput, TKey> : IMergeHelper<TInputOutput>
    {
        private QueryTaskGroupState m_taskGroupState; // State shared among tasks.
        private PartitionedStream<TInputOutput, TKey> m_partitions; // Source partitions.
        private Shared<TInputOutput[]> m_results; // The array where results are stored.
        private TaskScheduler m_taskScheduler; // The task manager to execute the query.

        //-----------------------------------------------------------------------------------
        // Instantiates a new merge helper.
        //
        // Arguments:
        //     partitions   - the source partitions from which to consume data.
        //     ignoreOutput - whether we're enumerating "for effect" or for output.
        //

        internal OrderPreservingMergeHelper(PartitionedStream<TInputOutput, TKey> partitions, TaskScheduler taskScheduler, 
            CancellationState cancellationState, int queryId)
        {
            Contract.Assert(partitions != null);

            TraceHelpers.TraceInfo("KeyOrderPreservingMergeHelper::.ctor(..): creating an order preserving merge helper");
            
            m_taskGroupState = new QueryTaskGroupState(cancellationState, queryId);
            m_partitions = partitions;
            m_results = new Shared<TInputOutput[]>(null);
            m_taskScheduler = taskScheduler;
        }

        //-----------------------------------------------------------------------------------
        // Schedules execution of the merge itself.
        //
        // Arguments:
        //    ordinalIndexState - the state of the ordinal index of the merged partitions
        //

        void IMergeHelper<TInputOutput>.Execute()
        {
            OrderPreservingSpoolingTask<TInputOutput, TKey>.Spool(m_taskGroupState, m_partitions, m_results, m_taskScheduler);
        }

        //-----------------------------------------------------------------------------------
        // Gets the enumerator from which to enumerate output results.
        //

        IEnumerator<TInputOutput> IMergeHelper<TInputOutput>.GetEnumerator()
        {
            Contract.Assert(m_results.Value != null);
            return ((IEnumerable<TInputOutput>)m_results.Value).GetEnumerator();
        }


        //-----------------------------------------------------------------------------------
        // Returns the results as an array.
        //

        public TInputOutput[] GetResultsAsArray()
        {
            return m_results.Value;
        }
    }
}
