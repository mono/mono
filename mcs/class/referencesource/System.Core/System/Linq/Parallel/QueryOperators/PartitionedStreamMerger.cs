// ==++==
//
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
// =+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+
//
// PartitionedStreamMerger.cs
//
// <OWNER>[....]</OWNER>
//
// =-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-

using System.Threading.Tasks;
using System.Diagnostics.Contracts;

namespace System.Linq.Parallel
{
    /// <summary>
    /// Partitioned stream recipient that will merge the results. 
    /// </summary>
    internal class PartitionedStreamMerger<TOutput> : IPartitionedStreamRecipient<TOutput>
    {
        private bool m_forEffectMerge;
        private ParallelMergeOptions m_mergeOptions;
        private bool m_isOrdered;
        private MergeExecutor<TOutput> m_mergeExecutor = null;
        private TaskScheduler m_taskScheduler;
        private int m_queryId; // ID of the current query execution

        private CancellationState m_cancellationState;

#if DEBUG
            private bool m_received = false;
#endif
        // Returns the merge executor which merges the received partitioned stream.
        internal MergeExecutor<TOutput> MergeExecutor
        {
            get
            {
#if DEBUG
                Contract.Assert(m_received, "Cannot return the merge executor because Receive() has not been called yet.");
#endif
                return m_mergeExecutor;
            }
        }

        internal PartitionedStreamMerger(bool forEffectMerge, ParallelMergeOptions mergeOptions, TaskScheduler taskScheduler, bool outputOrdered, 
            CancellationState cancellationState, int queryId)
        {
            m_forEffectMerge = forEffectMerge;
            m_mergeOptions = mergeOptions;
            m_isOrdered = outputOrdered;
            m_taskScheduler = taskScheduler;
            m_cancellationState = cancellationState;
            m_queryId = queryId;
        }

        public void Receive<TKey>(PartitionedStream<TOutput, TKey> partitionedStream)
        {
#if DEBUG
                m_received = true;
#endif
            m_mergeExecutor = MergeExecutor<TOutput>.Execute<TKey>(
                partitionedStream, m_forEffectMerge, m_mergeOptions, m_taskScheduler, m_isOrdered, m_cancellationState, m_queryId);

            TraceHelpers.TraceInfo("[timing]: {0}: finished opening - QueryOperator<>::GetEnumerator", DateTime.Now.Ticks);
        }
    }
}
