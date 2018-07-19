// ==++==
//
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
// =+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+
//
// TakeOrSkipWhileQueryOperator.cs
//
// <OWNER>Microsoft</OWNER>
//
// =-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-

using System.Collections.Generic;
using System.Threading;
using System.Diagnostics.Contracts;

namespace System.Linq.Parallel
{
    /// <summary>
    /// Take- and SkipWhile work similarly. Execution is broken into two phases: Search
    /// and Yield.
    ///
    /// During the Search phase, many partitions at once search for the first occurrence
    /// of a false element.  As they search, any time a partition finds a false element
    /// whose index is lesser than the current lowest-known false element, the new index
    /// will be published, so other partitions can stop the search.  The search stops
    /// as soon as (1) a partition exhausts its input, (2) the predicate yields false for
    /// one of the partition's elements, or (3) its input index passes the current lowest-
    /// known index (sufficient since a given partition's indices are always strictly
    /// incrementing -- asserted below).  Elements are buffered during this process.
    ///
    /// Partitions use a barrier after Search and before moving on to Yield.  Once all
    /// have passed the barrier, Yielding begins.  At this point, the lowest-known false
    /// index will be accurate for the entire set, since all partitions have finished
    /// scanning.  This is where TakeWhile and SkipWhile differ.  TakeWhile will start at
    /// the beginning of its buffer and yield all elements whose indices are less than
    /// the lowest-known false index.  SkipWhile, on the other hand, will skipp any such
    /// elements in the buffer, yielding those whose index is greater than or equal to
    /// the lowest-known false index, and then finish yielding any remaining elements in
    /// its data source (since it may have stopped prematurely due to (3) above).
    /// </summary>
    /// <typeparam name="TResult"></typeparam>
    internal sealed class TakeOrSkipWhileQueryOperator<TResult> : UnaryQueryOperator<TResult, TResult>
    {

        // Predicate function used to decide when to stop yielding elements. One pair is used for
        // index-based evaluation (i.e. it is passed the index as well as the element's value).
        private Func<TResult, bool> m_predicate;
        private Func<TResult, int, bool> m_indexedPredicate;

        private readonly bool m_take; // Whether to take (true) or skip (false).
        private bool m_prematureMerge = false; // Whether to prematurely merge the input of this operator.
        private bool m_limitsParallelism = false; // The precomputed value of LimitsParallelism

        //---------------------------------------------------------------------------------------
        // Initializes a new take-while operator.
        //
        // Arguments:
        //     child                - the child data source to enumerate
        //     predicate            - the predicate function (if expression tree isn't provided)
        //     indexedPredicate     - the index-based predicate function (if expression tree isn't provided)
        //     take                 - whether this is a TakeWhile (true) or SkipWhile (false)
        //
        // Notes:
        //     Only one kind of predicate can be specified, an index-based one or not.  If an
        //     expression tree is provided, the delegate cannot also be provided.
        //

        internal TakeOrSkipWhileQueryOperator(IEnumerable<TResult> child,
                                              Func<TResult, bool> predicate,
                                              Func<TResult, int, bool> indexedPredicate, bool take)
            :base(child)
        {
            Contract.Assert(child != null, "child data source cannot be null");
            Contract.Assert(predicate != null || indexedPredicate != null, "need a predicate function");

            m_predicate = predicate;
            m_indexedPredicate = indexedPredicate;
            m_take = take;

            InitOrderIndexState();
        }

        /// <summary>
        /// Determines the order index state for the output operator
        /// </summary>
        private void InitOrderIndexState()
        {
            // SkipWhile/TakeWhile needs an increasing index. However, if the predicate expression depends on the index,
            // the index needs to be correct, not just increasing.

            OrdinalIndexState requiredIndexState = OrdinalIndexState.Increasing;
            OrdinalIndexState childIndexState = Child.OrdinalIndexState;
            if (m_indexedPredicate != null)
            {
                requiredIndexState = OrdinalIndexState.Correct;
                m_limitsParallelism = childIndexState == OrdinalIndexState.Increasing;
            }

            OrdinalIndexState indexState = ExchangeUtilities.Worse(childIndexState, OrdinalIndexState.Correct);
            if (indexState.IsWorseThan(requiredIndexState))
            {
                m_prematureMerge = true;
            }

            if (!m_take)
            {
                // If the index was correct, now it is only increasing.
                indexState = indexState.Worse(OrdinalIndexState.Increasing);
            }

            SetOrdinalIndexState(indexState);
        }

        internal override void WrapPartitionedStream<TKey>(
            PartitionedStream<TResult, TKey> inputStream, IPartitionedStreamRecipient<TResult> recipient, bool preferStriping, QuerySettings settings)
        {
            
            if (m_prematureMerge)
            {
                ListQueryResults<TResult> results = ExecuteAndCollectResults(inputStream, inputStream.PartitionCount, Child.OutputOrdered, preferStriping, settings);
                PartitionedStream<TResult, int> listInputStream = results.GetPartitionedStream();
                WrapHelper<int>(listInputStream, recipient, settings);
            }
            else
            {
                WrapHelper<TKey>(inputStream, recipient, settings);
            }

            
        }

        private void WrapHelper<TKey>(PartitionedStream<TResult, TKey> inputStream, IPartitionedStreamRecipient<TResult> recipient, QuerySettings settings)
        {
            int partitionCount = inputStream.PartitionCount;

            // Create shared data.
            OperatorState<TKey> operatorState = new OperatorState<TKey>();
            CountdownEvent sharedBarrier = new CountdownEvent(partitionCount);

            Contract.Assert(m_indexedPredicate == null || typeof(TKey) == typeof(int));
            Func<TResult, TKey, bool> convertedIndexedPredicate = (Func<TResult, TKey, bool>)(object)m_indexedPredicate;

            PartitionedStream<TResult, TKey> partitionedStream =
                new PartitionedStream<TResult, TKey>(partitionCount, inputStream.KeyComparer, OrdinalIndexState);
            for (int i = 0; i < partitionCount; i++)
            {
                partitionedStream[i] = new TakeOrSkipWhileQueryOperatorEnumerator<TKey>(
                    inputStream[i], m_predicate, convertedIndexedPredicate, m_take, operatorState, sharedBarrier,
                    settings.CancellationState.MergedCancellationToken, inputStream.KeyComparer);
            }

            recipient.Receive(partitionedStream);
        }

        //---------------------------------------------------------------------------------------
        // Just opens the current operator, including opening the child and wrapping it with
        // partitions as needed.
        //

        internal override QueryResults<TResult> Open(QuerySettings settings, bool preferStriping)
        {
            QueryResults<TResult> childQueryResults = Child.Open(settings, true);
            return new UnaryQueryOperatorResults(childQueryResults, this, settings, preferStriping);
        }

        //---------------------------------------------------------------------------------------
        // Returns an enumerable that represents the query executing sequentially.
        //

        internal override IEnumerable<TResult> AsSequentialQuery(CancellationToken token)
        {
            if (m_take)
            {
                if (m_indexedPredicate != null)
                {
                    return Child.AsSequentialQuery(token).TakeWhile(m_indexedPredicate);
                }

                return Child.AsSequentialQuery(token).TakeWhile(m_predicate);
            }

            if (m_indexedPredicate != null)
            {
                IEnumerable<TResult> wrappedIndexedChild = CancellableEnumerable.Wrap(Child.AsSequentialQuery(token), token);
                return wrappedIndexedChild.SkipWhile(m_indexedPredicate);
            }

            IEnumerable<TResult> wrappedChild = CancellableEnumerable.Wrap(Child.AsSequentialQuery(token), token);
            return wrappedChild.SkipWhile(m_predicate);
        }

        //---------------------------------------------------------------------------------------
        // Whether this operator performs a premature merge that would not be performed in
        // a similar sequential operation (i.e., in LINQ to Objects).
        //

        internal override bool LimitsParallelism
        {
            get { return m_limitsParallelism; }
        }

        //---------------------------------------------------------------------------------------
        // The enumerator type responsible for executing the take- or skip-while.
        //

        class TakeOrSkipWhileQueryOperatorEnumerator<TKey> : QueryOperatorEnumerator<TResult, TKey>
        {

            private readonly QueryOperatorEnumerator<TResult, TKey> m_source; // The data source to enumerate.
            private readonly Func<TResult, bool> m_predicate;  // The actual predicate function.
            private readonly Func<TResult, TKey, bool> m_indexedPredicate;  // The actual index-based predicate function.
            private readonly bool m_take; // Whether to execute a take- (true) or skip-while (false).
            private readonly IComparer<TKey> m_keyComparer; // Comparer for the order keys.

            // These fields are all shared among partitions.
            private readonly OperatorState<TKey> m_operatorState; // The lowest false found by any partition.
            private readonly CountdownEvent m_sharedBarrier; // To separate the search/yield phases.
            private readonly CancellationToken m_cancellationToken; // Token used to cancel this operator.

            private List<Pair<TResult, TKey>> m_buffer; // Our buffer.
            private Shared<int> m_bufferIndex; // Our current index within the buffer.  [allocate in moveNext to avoid false-sharing]
            private int m_updatesSeen; // How many updates has this enumerator observed? (Each other enumerator will contribute one update.)
            private TKey m_currentLowKey; // The lowest key rejected by one of the other enumerators.
            

            //---------------------------------------------------------------------------------------
            // Instantiates a new select enumerator.
            //

            internal TakeOrSkipWhileQueryOperatorEnumerator(
                QueryOperatorEnumerator<TResult, TKey> source, Func<TResult, bool> predicate, Func<TResult, TKey, bool> indexedPredicate, bool take,
                OperatorState<TKey> operatorState, CountdownEvent sharedBarrier, CancellationToken cancelToken, IComparer<TKey> keyComparer)
            {
                Contract.Assert(source != null);
                Contract.Assert(predicate != null || indexedPredicate != null);
                Contract.Assert(operatorState != null);
                Contract.Assert(sharedBarrier != null);
                Contract.Assert(keyComparer != null);

                m_source = source;
                m_predicate = predicate;
                m_indexedPredicate = indexedPredicate;
                m_take = take;
                m_operatorState = operatorState;
                m_sharedBarrier = sharedBarrier;
                m_cancellationToken = cancelToken;
                m_keyComparer = keyComparer;
            }

            //---------------------------------------------------------------------------------------
            // Straightforward IEnumerator<T> methods.
            //

            internal override bool MoveNext(ref TResult currentElement, ref TKey currentKey)
            {
                // If the buffer has not been created, we will generate it lazily on demand.
                if (m_buffer == null)
                {
                    // Create a buffer, but don't publish it yet (in case of exception).
                    List<Pair<TResult, TKey>> buffer = new List<Pair<TResult, TKey>>();

                    // Enter the search phase.  In this phase, we scan the input until one of three
                    // things happens:  (1) all input has been exhausted, (2) the predicate yields
                    // false for one of our elements, or (3) we move past the current lowest index
                    // found by other partitions for a false element.  As we go, we have to remember
                    // the elements by placing them into the buffer.

                    try
                    {
                        TResult current = default(TResult);
                        TKey key = default(TKey);
                        int i = 0; //counter to help with cancellation
                        while (m_source.MoveNext(ref current, ref key))
                        {
                            if ((i++ & CancellationState.POLL_INTERVAL) == 0)
                                CancellationState.ThrowIfCanceled(m_cancellationToken);
                            
                            // Add the current element to our buffer.
                            buffer.Add(new Pair<TResult, TKey>(current, key));

                            // See if another partition has found a false value before this element. If so,
                            // we should stop scanning the input now and reach the barrier ASAP.
                            if (m_updatesSeen != m_operatorState.m_updatesDone)
                            {
                                lock (m_operatorState)
                                {
                                    m_currentLowKey = m_operatorState.m_currentLowKey;
                                    m_updatesSeen = m_operatorState.m_updatesDone;
                                }
                            }

                            if (m_updatesSeen > 0 && m_keyComparer.Compare(key, m_currentLowKey) > 0)
                            {
                                break;
                            }

                            // Evaluate the predicate, either indexed or not based on info passed to the ctor.
                            bool predicateResult;
                            if (m_predicate != null)
                            {
                                predicateResult = m_predicate(current);
                            }
                            else
                            {
                                Contract.Assert(m_indexedPredicate != null);
                                predicateResult = m_indexedPredicate(current, key);
                            }

                            if (!predicateResult)
                            {
                                // Signal that we've found a false element, racing with other partitions to
                                // set the shared index value.
                                lock (m_operatorState)
                                {
                                    if (m_operatorState.m_updatesDone == 0 || m_keyComparer.Compare(m_operatorState.m_currentLowKey, key) > 0)
                                    {
                                        m_currentLowKey = m_operatorState.m_currentLowKey = key;
                                        m_updatesSeen = ++m_operatorState.m_updatesDone;
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

                    // Before exiting the search phase, we will synchronize with others. This is a barrier.
                    m_sharedBarrier.Wait(m_cancellationToken);

                    // Publish the buffer and set the index to just before the 1st element.
                    m_buffer = buffer;
                    m_bufferIndex =  new Shared<int>(-1);
                }

                // Now either enter (or continue) the yielding phase. As soon as we reach this, we know the
                // current shared "low false" value is the absolute lowest with a false.                
                if (m_take)
                {
                    // In the case of a take-while, we will yield each element from our buffer for which
                    // the element is lesser than the lowest false index found.
                    if (m_bufferIndex.Value >= m_buffer.Count - 1)
                    {
                        return false;
                    }

                    // Increment the index, and remember the values.
                    ++m_bufferIndex.Value;
                    currentElement = m_buffer[m_bufferIndex.Value].First;
                    currentKey = m_buffer[m_bufferIndex.Value].Second;

                    return m_operatorState.m_updatesDone == 0 || m_keyComparer.Compare(m_operatorState.m_currentLowKey, currentKey) > 0;
                }
                else
                {
                    // If no false was found, the output is empty.
                    if (m_operatorState.m_updatesDone == 0)
                    {
                        return false;
                    }

                    // In the case of a skip-while, we must skip over elements whose index is lesser than the
                    // lowest index found. Once we've exhausted the buffer, we must go back and continue
                    // enumerating the data source until it is empty.
                    if (m_bufferIndex.Value < m_buffer.Count - 1)
                    {
                        for (m_bufferIndex.Value++; m_bufferIndex.Value < m_buffer.Count; m_bufferIndex.Value++)
                        {
                            // If the current buffered element's index is greater than or equal to the smallest
                            // false index found, we will yield it as a result.
                            if (m_keyComparer.Compare(m_buffer[m_bufferIndex.Value].Second, m_operatorState.m_currentLowKey) >= 0)
                            {
                                currentElement = m_buffer[m_bufferIndex.Value].First;
                                currentKey = m_buffer[m_bufferIndex.Value].Second;
                                return true;
                            }
                        }
                    }

                    // Lastly, so long as our input still has elements, they will be yieldable.
                    if (m_source.MoveNext(ref currentElement, ref currentKey))
                    {
                        Contract.Assert(m_keyComparer.Compare(currentKey, m_operatorState.m_currentLowKey) > 0,
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

        class OperatorState<TKey>
        {
            volatile internal int m_updatesDone = 0;
            internal TKey m_currentLowKey;
        }
    }
}
