// ==++==
//
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
// =+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+
//
// DefaultIfEmptyQueryOperator.cs
//
// <OWNER>[....]</OWNER>
//
// =-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-

using System.Collections.Generic;
using System.Threading;
using System.Diagnostics.Contracts;

namespace System.Linq.Parallel
{
    /// <summary>
    /// This operator just exposes elements directly from the underlying data source, if
    /// it's not empty, or yields a single default element if the data source is empty.
    /// There is a minimal amount of synchronization at the beginning, until all partitions
    /// have registered whether their stream is empty or not. Once the 0th partition knows
    /// that at least one other partition is non-empty, it may proceed. Otherwise, it is
    /// the 0th partition which yields the default value.
    /// </summary>
    /// <typeparam name="TSource"></typeparam>
    internal sealed class DefaultIfEmptyQueryOperator<TSource> : UnaryQueryOperator<TSource, TSource>
    {

        private readonly TSource m_defaultValue; // The default value to use (if empty).

        //---------------------------------------------------------------------------------------
        // Initializes a new reverse operator.
        //
        // Arguments:
        //     child                - the child whose data we will reverse
        //

        internal DefaultIfEmptyQueryOperator(IEnumerable<TSource> child, TSource defaultValue)
            :base(child)
        {
            Contract.Assert(child != null, "child data source cannot be null");
            m_defaultValue = defaultValue;
            SetOrdinalIndexState(ExchangeUtilities.Worse(Child.OrdinalIndexState, OrdinalIndexState.Correct));
        }

        //---------------------------------------------------------------------------------------
        // Just opens the current operator, including opening the child and wrapping it with
        // partitions as needed.
        //

        internal override QueryResults<TSource> Open(QuerySettings settings, bool preferStriping)
        {
            // We just open the child operator.
            QueryResults<TSource> childQueryResults = Child.Open(settings, preferStriping);
            return new UnaryQueryOperatorResults(childQueryResults, this, settings, preferStriping);
        }

        internal override void  WrapPartitionedStream<TKey>(
            PartitionedStream<TSource, TKey> inputStream, IPartitionedStreamRecipient<TSource> recipient, bool preferStriping, QuerySettings settings)
        {
            int partitionCount = inputStream.PartitionCount;

            // Generate the shared data.
            Shared<int> sharedEmptyCount = new Shared<int>(0);
            CountdownEvent sharedLatch = new CountdownEvent(partitionCount - 1);

            PartitionedStream<TSource, TKey> outputStream = 
                new PartitionedStream<TSource,TKey>(partitionCount, inputStream.KeyComparer, OrdinalIndexState);

            for (int i = 0; i < partitionCount; i++)
            {
                outputStream[i] = new DefaultIfEmptyQueryOperatorEnumerator<TKey>(
                    inputStream[i], m_defaultValue, i, partitionCount, sharedEmptyCount, sharedLatch, settings.CancellationState.MergedCancellationToken);
            }

            recipient.Receive(outputStream);
        }

        //---------------------------------------------------------------------------------------
        // Returns an enumerable that represents the query executing sequentially.
        //

        internal override IEnumerable<TSource> AsSequentialQuery(CancellationToken token)
        {
            return Child.AsSequentialQuery(token).DefaultIfEmpty(m_defaultValue);
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
        // The enumerator type responsible for executing the default-if-empty operation.
        //

        class DefaultIfEmptyQueryOperatorEnumerator<TKey> : QueryOperatorEnumerator<TSource, TKey>
        {
            private QueryOperatorEnumerator<TSource, TKey> m_source; // The data source to enumerate.
            private bool m_lookedForEmpty; // Whether this partition has looked for empty yet.
            private int m_partitionIndex; // This enumerator's partition index.
            private int m_partitionCount; // The number of partitions.
            private TSource m_defaultValue; // The default value if the 0th partition is empty.

            // Data shared among partitions.
            private Shared<int> m_sharedEmptyCount; // The number of empty partitions.
            private CountdownEvent m_sharedLatch; // Shared latch, signaled when partitions process the 1st item.
            private CancellationToken m_cancelToken; // Token used to cancel this operator.

            //---------------------------------------------------------------------------------------
            // Instantiates a new select enumerator.
            //

            internal DefaultIfEmptyQueryOperatorEnumerator(
                QueryOperatorEnumerator<TSource, TKey> source, TSource defaultValue, int partitionIndex, int partitionCount,
                Shared<int> sharedEmptyCount, CountdownEvent sharedLatch, CancellationToken cancelToken)
            {
                Contract.Assert(source != null);
                Contract.Assert(0 <= partitionIndex && partitionIndex < partitionCount);
                Contract.Assert(partitionCount > 0);
                Contract.Assert(sharedEmptyCount != null);
                Contract.Assert(sharedLatch != null);

                m_source = source;
                m_defaultValue = defaultValue;
                m_partitionIndex = partitionIndex;
                m_partitionCount = partitionCount;
                m_sharedEmptyCount = sharedEmptyCount;
                m_sharedLatch = sharedLatch;
                m_cancelToken = cancelToken;
            }

            //---------------------------------------------------------------------------------------
            // Straightforward IEnumerator<T> methods.
            //

            internal override bool MoveNext(ref TSource currentElement, ref TKey currentKey)
            {
                Contract.Assert(m_source != null);

                bool moveNextResult = m_source.MoveNext(ref currentElement, ref currentKey);

                // There is special logic the first time this function is called.
                if (!m_lookedForEmpty)
                {
                    // Ensure we don't enter this loop again.
                    m_lookedForEmpty = true;

                    if (!moveNextResult)
                    {
                        if (m_partitionIndex == 0)
                        {
                            // If this is the 0th partition, we must wait for all others.  Note: we could
                            // actually do a wait-any here: if at least one other partition finds an element,
                            // there is strictly no need to wait.  But this would require extra coordination
                            // which may or may not be worth the trouble.
                            m_sharedLatch.Wait(m_cancelToken);
                            m_sharedLatch.Dispose();

                            // Now see if there were any other partitions with data.
                            if (m_sharedEmptyCount.Value == m_partitionCount - 1)
                            {
                                // No data, we will yield the default value.
                                currentElement = m_defaultValue;
                                currentKey = default(TKey);
                                return true;
                            }
                            else
                            {
                                // Another partition has data, we are done.
                                return false;
                            }
                        }
                        else
                        {
                            // Not the 0th partition, we will increment the shared empty counter.
                            Interlocked.Increment(ref m_sharedEmptyCount.Value);
                        }
                    }

                    // Every partition (but the 0th) will signal the latch the first time.
                    if (m_partitionIndex != 0)
                    {
                        m_sharedLatch.Signal();
                    }
                }

                return moveNextResult;
            }

            protected override void Dispose(bool disposing)
            {
                m_source.Dispose();
            }
        }
    }
}
