// ==++==
//
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
// =+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+
//
// FirstQueryOperator.cs
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
    /// First tries to discover the first element in the source, optionally matching a
    /// predicate.  All partitions search in parallel, publish the lowest index for a
    /// candidate match, and reach a barrier.  Only the partition that "wins" the ----,
    /// i.e. who found the candidate with the smallest index, will yield an element.
    /// </summary>
    /// <typeparam name="TSource"></typeparam>
    internal sealed class FirstQueryOperator<TSource> : UnaryQueryOperator<TSource, TSource>
    {

        private readonly Func<TSource, bool> m_predicate; // The optional predicate used during the search.
        private readonly bool m_prematureMergeNeeded; // Whether to prematurely merge the input of this operator.

        //---------------------------------------------------------------------------------------
        // Initializes a new first operator.
        //
        // Arguments:
        //     child                - the child whose data we will reverse
        //

        internal FirstQueryOperator(IEnumerable<TSource> child, Func<TSource, bool> predicate)
            :base(child)
        {
            Contract.Assert(child != null, "child data source cannot be null");
            m_predicate = predicate;
            m_prematureMergeNeeded = Child.OrdinalIndexState.IsWorseThan(OrdinalIndexState.Increasing);
        }

        //---------------------------------------------------------------------------------------
        // Just opens the current operator, including opening the child and wrapping it with
        // partitions as needed.
        //

        internal override QueryResults<TSource> Open(QuerySettings settings, bool preferStriping)
        {
            // We just open the child operator.
            QueryResults<TSource> childQueryResults = Child.Open(settings, false);
            return new UnaryQueryOperatorResults(childQueryResults, this, settings, preferStriping);
        }

        internal override void  WrapPartitionedStream<TKey>(
            PartitionedStream<TSource, TKey> inputStream, IPartitionedStreamRecipient<TSource> recipient, bool preferStriping, QuerySettings settings)
        {
            // If the index is not at least increasing, we need to reindex.
            if (m_prematureMergeNeeded)
            {
                ListQueryResults<TSource> listResults = ExecuteAndCollectResults(inputStream, inputStream.PartitionCount, Child.OutputOrdered, preferStriping, settings);
                WrapHelper<int>(listResults.GetPartitionedStream(), recipient, settings);
            }
            else
            {
                WrapHelper<TKey>(inputStream, recipient, settings);
            }
        }

        private void WrapHelper<TKey>(
            PartitionedStream<TSource, TKey> inputStream, IPartitionedStreamRecipient<TSource> recipient, QuerySettings settings)
        {
            int partitionCount = inputStream.PartitionCount;

            // Generate the shared data.
            FirstQueryOperatorState<TKey> operatorState = new FirstQueryOperatorState<TKey>();
            CountdownEvent sharedBarrier = new CountdownEvent(partitionCount);

            PartitionedStream<TSource, int> outputStream = new PartitionedStream<TSource, int>(
                partitionCount, Util.GetDefaultComparer<int>(), OrdinalIndexState.Shuffled);

            for (int i = 0; i < partitionCount; i++)
            {
                outputStream[i] = new FirstQueryOperatorEnumerator<TKey>(
                    inputStream[i], m_predicate, operatorState, sharedBarrier, 
                    settings.CancellationState.MergedCancellationToken, inputStream.KeyComparer, i);
            }

            recipient.Receive(outputStream);
        }


        //---------------------------------------------------------------------------------------
        // Returns an enumerable that represents the query executing sequentially.
        //

        internal override IEnumerable<TSource> AsSequentialQuery(CancellationToken token)
        {
            Contract.Assert(false, "This method should never be called as fallback to sequential is handled in ParallelEnumerable.First().");
            throw new NotSupportedException();
        }

        //---------------------------------------------------------------------------------------
        // Whether this operator performs a premature merge that would not be performed in
        // a similar sequential operation (i.e., in LINQ to Objects).
        //

        internal override bool LimitsParallelism
        {
            get { return false; }
        }

        //---------------------------------------------------------------------------------------
        // The enumerator type responsible for executing the first operation.
        //

        class FirstQueryOperatorEnumerator<TKey> : QueryOperatorEnumerator<TSource, int>
        {

            private QueryOperatorEnumerator<TSource, TKey> m_source; // The data source to enumerate.
            private Func<TSource, bool> m_predicate; // The optional predicate used during the search.
            private bool m_alreadySearched; // Set once the enumerator has performed the search.
            private int m_partitionId; // ID of this partition

            // Data shared among partitions.
            private FirstQueryOperatorState<TKey> m_operatorState; // The current first candidate and its partition index.
            private CountdownEvent m_sharedBarrier; // Shared barrier, signaled when partitions find their 1st element.
            private CancellationToken m_cancellationToken; // Token used to cancel this operator.
            private IComparer<TKey> m_keyComparer; // Comparer for the order keys

            //---------------------------------------------------------------------------------------
            // Instantiates a new enumerator.
            //

            internal FirstQueryOperatorEnumerator(
                QueryOperatorEnumerator<TSource, TKey> source, Func<TSource, bool> predicate,
                FirstQueryOperatorState<TKey> operatorState, CountdownEvent sharedBarrier, CancellationToken cancellationToken,
                IComparer<TKey> keyComparer, int partitionId)
            {
                Contract.Assert(source != null);
                Contract.Assert(operatorState != null);
                Contract.Assert(sharedBarrier != null);
                Contract.Assert(keyComparer != null);

                m_source = source;
                m_predicate = predicate;
                m_operatorState = operatorState;
                m_sharedBarrier = sharedBarrier;
                m_cancellationToken = cancellationToken;
                m_keyComparer = keyComparer;
                m_partitionId = partitionId;
            }

            //---------------------------------------------------------------------------------------
            // Straightforward IEnumerator<T> methods.
            //

            internal override bool MoveNext(ref TSource currentElement, ref int currentKey)
            {
                Contract.Assert(m_source != null);

                if (m_alreadySearched)
                {
                    return false;
                }

                // Look for the lowest element.
                TSource candidate = default(TSource);
                TKey candidateKey = default(TKey);
                try
                {
                    TSource value = default(TSource);
                    TKey key = default(TKey);
                    int i = 0;
                    while (m_source.MoveNext(ref value, ref key))
                    {
                        if ((i++ & CancellationState.POLL_INTERVAL) == 0)
                            CancellationState.ThrowIfCanceled(m_cancellationToken);

                        // If the predicate is null or the current element satisfies it, we have found the
                        // current partition's "candidate" for the first element.  Note it.
                        if (m_predicate == null || m_predicate(value))
                        {
                            candidate = value;
                            candidateKey = key;

                            lock (m_operatorState)
                            {
                                if (m_operatorState.m_partitionId == -1 || m_keyComparer.Compare(candidateKey, m_operatorState.m_key) < 0)
                                {
                                    m_operatorState.m_key = candidateKey;
                                    m_operatorState.m_partitionId = m_partitionId;
                                }
                            }

                            break;
                        }
                    }
                }
                finally
                {
                    // No matter whether we exit due to an exception or normal completion, we must ensure
                    // that we signal other partitions that we have completed.  Otherwise, we can cause deadlocks.
                    m_sharedBarrier.Signal();
                }

                m_alreadySearched = true;

                // Wait only if we may have the result
                if (m_partitionId == m_operatorState.m_partitionId)
                {
                    m_sharedBarrier.Wait(m_cancellationToken);

                    // Now re-read the shared index. If it's the same as ours, we won and return true.
                    if (m_partitionId == m_operatorState.m_partitionId)
                    {
                        currentElement = candidate;
                        currentKey = 0; // 1st (and only) element, so we hardcode the output index to 0.
                        return true;
                    }
                }

                // If we got here, we didn't win. Return false.
                return false;
            }

            protected override void Dispose(bool disposing)
            {
                m_source.Dispose();
            }
        }

        class FirstQueryOperatorState<TKey>
        {
            internal TKey m_key;
            internal int m_partitionId = -1;
        }
    }
}
