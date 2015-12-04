// ==++==
//
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
// =+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+
//
// TakeOrSkipQueryOperator.cs
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
    /// Take and Skip either take or skip a specified number of elements, captured in the
    /// count argument.  These will work a little bit like TakeWhile and SkipWhile: there
    /// are two phases, (1) Search and (2) Yield.  In the search phase, our goal is to
    /// find the 'count'th index from the input.  We do this in parallel by sharing a count-
    /// sized array.  Each thread ----s to populate the array with indices in ascending
    /// order.  This requires synchronization for inserts.  We use a simple heap, for decent
    /// worst case performance.  After a thread has scanned ‘count’ elements, or its current
    /// index is greater than or equal to the maximum index in the array (and the array is
    /// fully populated), the thread can stop searching.  All threads issue a barrier before
    /// moving to the Yield phase.  When the Yield phase is entered, the count-1th element
    /// of the array contains: in the case of Take, the maximum index (exclusive) to be
    /// returned; or in the case of Skip, the minimum index (inclusive) to be returned.  The
    /// Yield phase simply consists of yielding these elements as output.
    /// </summary>
    /// <typeparam name="TResult"></typeparam>
    internal sealed class TakeOrSkipQueryOperator<TResult> : UnaryQueryOperator<TResult, TResult>
    {

        private readonly int m_count; // The number of elements to take or skip.
        private readonly bool m_take; // Whether to take (true) or skip (false).
        private bool m_prematureMerge = false; // Whether to prematurely merge the input of this operator.

        //---------------------------------------------------------------------------------------
        // Initializes a new take-while operator.
        //
        // Arguments:
        //     child  - the child data source to enumerate
        //     count  - the number of elements to take or skip
        //     take   - whether this is a Take (true) or Skip (false)
        //

        internal TakeOrSkipQueryOperator(IEnumerable<TResult> child, int count, bool take)
            :base(child)
        {
            Contract.Assert(child != null, "child data source cannot be null");

            m_count = count;
            m_take = take;

            SetOrdinalIndexState(OutputOrdinalIndexState());
        }

        /// <summary>
        /// Determines the order index state for the output operator
        /// </summary>
        private OrdinalIndexState OutputOrdinalIndexState()
        {
            OrdinalIndexState indexState = Child.OrdinalIndexState;

            if (indexState == OrdinalIndexState.Indexible)
            {
                return OrdinalIndexState.Indexible;
            }

            if (indexState.IsWorseThan(OrdinalIndexState.Increasing))
            {
                m_prematureMerge = true;
                indexState = OrdinalIndexState.Correct;
            }

            // If the operator is skip and the index was correct, now it is only increasing.
            if (!m_take && indexState == OrdinalIndexState.Correct)
            {
                indexState = OrdinalIndexState.Increasing;
            }

            return indexState;
        }

        internal override void WrapPartitionedStream<TKey>(
            PartitionedStream<TResult, TKey> inputStream, IPartitionedStreamRecipient<TResult> recipient, bool preferStriping, QuerySettings settings)
        {
            Contract.Assert(Child.OrdinalIndexState != OrdinalIndexState.Indexible, "Don't take this code path if the child is indexible.");

            // If the index is not at least increasing, we need to reindex.
            if (m_prematureMerge)
            {
                ListQueryResults<TResult> results = ExecuteAndCollectResults(
                    inputStream, inputStream.PartitionCount, Child.OutputOrdered, preferStriping, settings);
                PartitionedStream<TResult, int> inputIntStream = results.GetPartitionedStream();
                WrapHelper<int>(inputIntStream, recipient, settings);
            }
            else
            {
                WrapHelper<TKey>(inputStream, recipient, settings);
            }
        }

        private void WrapHelper<TKey>(PartitionedStream<TResult, TKey> inputStream, IPartitionedStreamRecipient<TResult> recipient, QuerySettings settings)
        {
            int partitionCount = inputStream.PartitionCount;
            FixedMaxHeap<TKey> sharedIndices = new FixedMaxHeap<TKey>(m_count, inputStream.KeyComparer); // an array used to track the sequence of indices leading up to the Nth index
            CountdownEvent sharredBarrier = new CountdownEvent(partitionCount); // a barrier to synchronize before yielding

            PartitionedStream<TResult, TKey> outputStream =
                new PartitionedStream<TResult, TKey>(partitionCount, inputStream.KeyComparer, OrdinalIndexState);
            for (int i = 0; i < partitionCount; i++)
            {
                outputStream[i] = new TakeOrSkipQueryOperatorEnumerator<TKey>(
                    inputStream[i], m_take, sharedIndices, sharredBarrier,
                    settings.CancellationState.MergedCancellationToken, inputStream.KeyComparer);
            }

            recipient.Receive(outputStream);
        }

        //---------------------------------------------------------------------------------------
        // Just opens the current operator, including opening the child and wrapping it with
        // partitions as needed.
        //

        internal override QueryResults<TResult> Open(QuerySettings settings, bool preferStriping)
        {
            QueryResults<TResult> childQueryResults = Child.Open(settings, true);
            return TakeOrSkipQueryOperatorResults.NewResults(childQueryResults, this, settings, preferStriping);
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
        // The enumerator type responsible for executing the Take or Skip.
        //

        class TakeOrSkipQueryOperatorEnumerator<TKey> : QueryOperatorEnumerator<TResult, TKey>
        {
            private readonly QueryOperatorEnumerator<TResult, TKey> m_source; // The data source to enumerate.
            private readonly int m_count; // The number of elements to take or skip.
            private readonly bool m_take; // Whether to execute a Take (true) or Skip (false).
            private readonly IComparer<TKey> m_keyComparer; // Comparer for the order keys.

            // These fields are all shared among partitions.
            private readonly FixedMaxHeap<TKey> m_sharedIndices; // The indices shared among partitions.
            private readonly CountdownEvent m_sharedBarrier; // To separate the search/yield phases.
            private readonly CancellationToken m_cancellationToken; // Indicates that cancellation has occurred.

            private List<Pair<TResult, TKey>> m_buffer; // Our buffer.
            private Shared<int> m_bufferIndex; // Our current index within the buffer. [allocate in moveNext to avoid false-sharing]

            //---------------------------------------------------------------------------------------
            // Instantiates a new select enumerator.
            //

            internal TakeOrSkipQueryOperatorEnumerator(
                QueryOperatorEnumerator<TResult, TKey> source, bool take,
                FixedMaxHeap<TKey> sharedIndices, CountdownEvent sharedBarrier, CancellationToken cancellationToken,
                IComparer<TKey> keyComparer)
            {
                Contract.Assert(source != null);
                Contract.Assert(sharedIndices != null);
                Contract.Assert(sharedBarrier != null);
                Contract.Assert(keyComparer != null);

                m_source = source;
                m_count = sharedIndices.Size;
                m_take = take;
                m_sharedIndices = sharedIndices;
                m_sharedBarrier = sharedBarrier;
                m_cancellationToken = cancellationToken;
                m_keyComparer = keyComparer;
            }

            //---------------------------------------------------------------------------------------
            // Straightforward IEnumerator<T> methods.
            //

            internal override bool MoveNext(ref TResult currentElement, ref TKey currentKey)
            {
                Contract.Assert(m_sharedIndices != null);

                // If the buffer has not been created, we will populate it lazily on demand.
                if (m_buffer == null && m_count > 0)
                {
                    

                    // Create a buffer, but don't publish it yet (in case of exception).
                    List<Pair<TResult, TKey>> buffer = new List<Pair<TResult, TKey>>();

                    // Enter the search phase. In this phase, all partitions ---- to populate
                    // the shared indices with their first 'count' contiguous elements.
                    TResult current = default(TResult);
                    TKey index = default(TKey);
                    int i = 0; //counter to help with cancellation
                    while (buffer.Count < m_count && m_source.MoveNext(ref current, ref index))
                    {
                        if ((i++ & CancellationState.POLL_INTERVAL) == 0)
                            CancellationState.ThrowIfCanceled(m_cancellationToken);
                        
                        // Add the current element to our buffer.
                        buffer.Add(new Pair<TResult, TKey>(current, index));

                        // Now we will try to insert our index into the shared indices list, quitting if
                        // our index is greater than all of the indices already inside it.
                        lock (m_sharedIndices)
                        {
                            if (!m_sharedIndices.Insert(index))
                            {
                                // We have read past the maximum index. We can move to the barrier now.
                                break;
                            }
                        }
                    }

                    // Before exiting the search phase, we will synchronize with others. This is a barrier.
                    m_sharedBarrier.Signal();
                    m_sharedBarrier.Wait(m_cancellationToken);

                    // Publish the buffer and set the index to just before the 1st element.
                    m_buffer = buffer;
                    m_bufferIndex = new Shared<int>(-1);
                }

                // Now either enter (or continue) the yielding phase. As soon as we reach this, we know the
                // index of the 'count'-th input element.
                if (m_take)
                {
                    // In the case of a Take, we will yield each element from our buffer for which
                    // the element is lesser than the 'count'-th index found.
                    if (m_count == 0 || m_bufferIndex.Value >= m_buffer.Count - 1)
                    {
                        return false;
                    }

                    // Increment the index, and remember the values.
                    ++m_bufferIndex.Value;
                    currentElement = m_buffer[m_bufferIndex.Value].First;
                    currentKey = m_buffer[m_bufferIndex.Value].Second;

                    // Only yield the element if its index is less than or equal to the max index.
                    return m_sharedIndices.Count == 0 
                        || m_keyComparer.Compare(m_buffer[m_bufferIndex.Value].Second, m_sharedIndices.MaxValue) <= 0;
                }
                else
                {
                    TKey minKey = default(TKey);

                    // If the count to skip was greater than 0, look at the buffer.
                    if (m_count > 0)
                    {
                        // If there wasn't enough input to skip, return right away.
                        if (m_sharedIndices.Count < m_count)
                        {
                            return false;
                        }

                        minKey = m_sharedIndices.MaxValue;

                        // In the case of a skip, we must skip over elements whose index is lesser than the
                        // 'count'-th index found. Once we've exhausted the buffer, we must go back and continue
                        // enumerating the data source until it is empty.
                        if (m_bufferIndex.Value < m_buffer.Count - 1)
                        {
                            for (m_bufferIndex.Value++; m_bufferIndex.Value < m_buffer.Count; m_bufferIndex.Value++)
                            {
                                // If the current buffered element's index is greater than the 'count'-th index,
                                // we will yield it as a result.
                                if (m_keyComparer.Compare(m_buffer[m_bufferIndex.Value].Second, minKey) > 0)
                                {
                                    currentElement = m_buffer[m_bufferIndex.Value].First;
                                    currentKey = m_buffer[m_bufferIndex.Value].Second;
                                    return true;
                                }
                            }
                        }                    
                    }

                    // Lastly, so long as our input still has elements, they will be yieldable.
                    if (m_source.MoveNext(ref currentElement, ref currentKey))
                    {
                        Contract.Assert(m_count <= 0 || m_keyComparer.Compare(currentKey, minKey) > 0,
                                        "expected remaining element indices to be greater than smallest");
                        return true;
                    }
                }

                return false;
            }

            protected override void Dispose(bool disposing)
            {
                m_source.Dispose();
            }
        }

        //---------------------------------------------------------------------------------------
        // Returns an enumerable that represents the query executing sequentially.
        //

        internal override IEnumerable<TResult> AsSequentialQuery(CancellationToken token)
        {
            if (m_take)
            {
                return Child.AsSequentialQuery(token).Take(m_count);
            }

            IEnumerable<TResult> wrappedChild = CancellableEnumerable.Wrap(Child.AsSequentialQuery(token), token);
            return wrappedChild.Skip(m_count);
        }

        //-----------------------------------------------------------------------------------
        // Query results for a Take or a Skip operator. The results are indexible if the child
        // results were indexible.
        //

        class TakeOrSkipQueryOperatorResults : UnaryQueryOperatorResults
        {
            TakeOrSkipQueryOperator<TResult> m_takeOrSkipOp; // The operator that generated the results
            int m_childCount; // The number of elements in child results

            public static QueryResults<TResult> NewResults(
                QueryResults<TResult> childQueryResults, TakeOrSkipQueryOperator<TResult> op,
                QuerySettings settings, bool preferStriping)
            {
                if (childQueryResults.IsIndexible)
                {
                    return new TakeOrSkipQueryOperatorResults(
                        childQueryResults, op, settings, preferStriping);
                }
                else
                {
                    return new UnaryQueryOperatorResults(
                        childQueryResults, op, settings, preferStriping);
                }
            }

            private TakeOrSkipQueryOperatorResults(
                QueryResults<TResult> childQueryResults, TakeOrSkipQueryOperator<TResult> takeOrSkipOp,
                QuerySettings settings, bool preferStriping)
                : base(childQueryResults, takeOrSkipOp, settings, preferStriping)
            {
                m_takeOrSkipOp = takeOrSkipOp;
                Contract.Assert(m_childQueryResults.IsIndexible);

                m_childCount = m_childQueryResults.ElementsCount;
            }

            internal override bool IsIndexible
            {
                get { return m_childCount >= 0; }
            }

            internal override int ElementsCount
            {
                get
                {
                    Contract.Assert(m_childCount >= 0);
                    if (m_takeOrSkipOp.m_take)
                    {
                        return Math.Min(m_childCount, m_takeOrSkipOp.m_count);
                    }
                    else
                    {
                        return Math.Max(m_childCount - m_takeOrSkipOp.m_count, 0);                        
                    }
                }
            }

            internal override TResult GetElement(int index)
            {
                Contract.Assert(m_childCount >= 0);
                Contract.Assert(index >= 0);
                Contract.Assert(index < ElementsCount);

                if (m_takeOrSkipOp.m_take)
                {
                    return m_childQueryResults.GetElement(index);
                }
                else
                {
                    return m_childQueryResults.GetElement(m_takeOrSkipOp.m_count + index);
                }
            }
        }
    }
}
