// ==++==
//
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
// =+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+
//
// IntersectQueryOperator.cs
//
// <OWNER>Microsoft</OWNER>
//
// =-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Threading;

namespace System.Linq.Parallel
{
    /// <summary>
    /// Operator that yields the intersection of two data sources. 
    /// </summary>
    /// <typeparam name="TInputOutput"></typeparam>
    internal sealed class IntersectQueryOperator<TInputOutput> :
        BinaryQueryOperator<TInputOutput, TInputOutput, TInputOutput>
    {

        private readonly IEqualityComparer<TInputOutput> m_comparer; // An equality comparer.

        //---------------------------------------------------------------------------------------
        // Constructs a new intersection operator.
        //

        internal IntersectQueryOperator(ParallelQuery<TInputOutput> left, ParallelQuery<TInputOutput> right, IEqualityComparer<TInputOutput> comparer)
            :base(left, right)
        {
            Contract.Assert(left != null && right != null, "child data sources cannot be null");

            m_comparer = comparer;
            m_outputOrdered = LeftChild.OutputOrdered;

            SetOrdinalIndex(OrdinalIndexState.Shuffled);
        }


        internal override QueryResults<TInputOutput> Open(
            QuerySettings settings, bool preferStriping)
        {
            // We just open our child operators, left and then right.  Do not propagate the preferStriping value, but 
            // instead explicitly set it to false. Regardless of whether the parent prefers striping or range
            // partitioning, the output will be hash-partititioned.
            QueryResults<TInputOutput> leftChildResults = LeftChild.Open(settings, false);
            QueryResults<TInputOutput> rightChildResults = RightChild.Open(settings, false);

            return new BinaryQueryOperatorResults(leftChildResults, rightChildResults, this, settings, false);
        }

        public override void WrapPartitionedStream<TLeftKey, TRightKey>(
            PartitionedStream<TInputOutput, TLeftKey> leftPartitionedStream, PartitionedStream<TInputOutput, TRightKey> rightPartitionedStream,
            IPartitionedStreamRecipient<TInputOutput> outputRecipient, bool preferStriping, QuerySettings settings)
        {
            Contract.Assert(leftPartitionedStream.PartitionCount == rightPartitionedStream.PartitionCount);

            if (OutputOrdered)
            {
                WrapPartitionedStreamHelper<TLeftKey, TRightKey>(
                    ExchangeUtilities.HashRepartitionOrdered<TInputOutput, NoKeyMemoizationRequired, TLeftKey>(
                        leftPartitionedStream, null, null, m_comparer, settings.CancellationState.MergedCancellationToken),
                    rightPartitionedStream, outputRecipient, settings.CancellationState.MergedCancellationToken);
            }
            else
            {
                WrapPartitionedStreamHelper<int, TRightKey>(
                    ExchangeUtilities.HashRepartition<TInputOutput, NoKeyMemoizationRequired, TLeftKey>(
                        leftPartitionedStream, null, null, m_comparer, settings.CancellationState.MergedCancellationToken),
                    rightPartitionedStream, outputRecipient, settings.CancellationState.MergedCancellationToken);
            }
        }

        //---------------------------------------------------------------------------------------
        // This is a helper method. WrapPartitionedStream decides what type TLeftKey is going
        // to be, and then call this method with that key as a generic parameter.
        //

        private void WrapPartitionedStreamHelper<TLeftKey, TRightKey>(
            PartitionedStream<Pair<TInputOutput, NoKeyMemoizationRequired>, TLeftKey> leftHashStream, PartitionedStream<TInputOutput, TRightKey> rightPartitionedStream, 
            IPartitionedStreamRecipient<TInputOutput> outputRecipient, CancellationToken cancellationToken)
        {
            int partitionCount = leftHashStream.PartitionCount;

            PartitionedStream<Pair<TInputOutput, NoKeyMemoizationRequired>, int> rightHashStream =
                ExchangeUtilities.HashRepartition<TInputOutput, NoKeyMemoizationRequired, TRightKey>(
                    rightPartitionedStream, null, null, m_comparer, cancellationToken);

            PartitionedStream<TInputOutput, TLeftKey> outputStream =
                new PartitionedStream<TInputOutput, TLeftKey>(partitionCount, leftHashStream.KeyComparer, OrdinalIndexState.Shuffled);
            for (int i = 0; i < partitionCount; i++)
            {
                if (OutputOrdered)
                {
                    outputStream[i] = new OrderedIntersectQueryOperatorEnumerator<TLeftKey>(
                        leftHashStream[i], rightHashStream[i], m_comparer, leftHashStream.KeyComparer, cancellationToken);
                }
                else
                {
                    outputStream[i] = (QueryOperatorEnumerator<TInputOutput, TLeftKey>)(object)
                            new IntersectQueryOperatorEnumerator<TLeftKey>(leftHashStream[i], rightHashStream[i], m_comparer, cancellationToken);
                }
            }

            outputRecipient.Receive(outputStream);
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
        // This enumerator performs the intersection operation incrementally. It does this by
        // maintaining a history -- in the form of a set -- of all data already seen. It then
        // only returns elements that are seen twice (returning each one only once).
        //

        class IntersectQueryOperatorEnumerator<TLeftKey> : QueryOperatorEnumerator<TInputOutput, int>
        {

            private QueryOperatorEnumerator<Pair<TInputOutput, NoKeyMemoizationRequired>, TLeftKey> m_leftSource; // Left data source.
            private QueryOperatorEnumerator<Pair<TInputOutput, NoKeyMemoizationRequired>, int> m_rightSource; // Right data source.
            private IEqualityComparer<TInputOutput> m_comparer; // Comparer to use for equality/hash-coding.
            private Set<TInputOutput> m_hashLookup; // The hash lookup, used to produce the intersection.
            private CancellationToken m_cancellationToken;
            private Shared<int> m_outputLoopCount;

            //---------------------------------------------------------------------------------------
            // Instantiates a new intersection operator.
            //

            internal IntersectQueryOperatorEnumerator(
                QueryOperatorEnumerator<Pair<TInputOutput, NoKeyMemoizationRequired>, TLeftKey> leftSource,
                QueryOperatorEnumerator<Pair<TInputOutput, NoKeyMemoizationRequired>, int> rightSource,
                IEqualityComparer<TInputOutput> comparer, CancellationToken cancellationToken)
            {
                Contract.Assert(leftSource != null);
                Contract.Assert(rightSource != null);

                m_leftSource = leftSource;
                m_rightSource = rightSource;
                m_comparer = comparer;
                m_cancellationToken = cancellationToken;
            }

            //---------------------------------------------------------------------------------------
            // Walks the two data sources, left and then right, to produce the intersection.
            //

            internal override bool MoveNext(ref TInputOutput currentElement, ref int currentKey)
            {
                Contract.Assert(m_leftSource != null);
                Contract.Assert(m_rightSource != null);

                // Build the set out of the right data source, if we haven't already.
                
                if (m_hashLookup == null)
                {
                    m_outputLoopCount = new Shared<int>(0);
                    m_hashLookup = new Set<TInputOutput>(m_comparer);

                    Pair<TInputOutput, NoKeyMemoizationRequired> rightElement = default(Pair<TInputOutput, NoKeyMemoizationRequired>);
                    int rightKeyUnused = default(int);

                    int i = 0;
                    while (m_rightSource.MoveNext(ref rightElement, ref rightKeyUnused))
                    {
                        if ((i++ & CancellationState.POLL_INTERVAL) == 0)
                            CancellationState.ThrowIfCanceled(m_cancellationToken);

                        m_hashLookup.Add(rightElement.First);
                    }
                }

                // Now iterate over the left data source, looking for matches.
                Pair<TInputOutput, NoKeyMemoizationRequired> leftElement = default(Pair<TInputOutput, NoKeyMemoizationRequired>);
                TLeftKey keyUnused = default(TLeftKey);

                while (m_leftSource.MoveNext(ref leftElement, ref keyUnused))
                {
                    if ((m_outputLoopCount.Value++ & CancellationState.POLL_INTERVAL) == 0)
                            CancellationState.ThrowIfCanceled(m_cancellationToken);

                    // If we found the element in our set, and if we haven't returned it yet,
                    // we can yield it to the caller. We also mark it so we know we've returned
                    // it once already and never will again.
                    if (m_hashLookup.Contains(leftElement.First))
                    {
                        m_hashLookup.Remove(leftElement.First);
                        currentElement = leftElement.First;
#if DEBUG
                        currentKey = unchecked((int)0xdeadbeef);
#endif
                        return true;
                    }
                }

                return false;
            }

            protected override void Dispose(bool disposing)
            {
                Contract.Assert(m_leftSource != null && m_rightSource != null);
                m_leftSource.Dispose();
                m_rightSource.Dispose();
            }
        }

        //---------------------------------------------------------------------------------------
        // Returns an enumerable that represents the query executing sequentially.
        //

        internal override IEnumerable<TInputOutput> AsSequentialQuery(CancellationToken token)
        {
            IEnumerable<TInputOutput> wrappedLeftChild = CancellableEnumerable.Wrap(LeftChild.AsSequentialQuery(token), token);
            IEnumerable<TInputOutput> wrappedRightChild = CancellableEnumerable.Wrap(RightChild.AsSequentialQuery(token), token);
            return wrappedLeftChild.Intersect(wrappedRightChild, m_comparer);
        }


        class OrderedIntersectQueryOperatorEnumerator<TLeftKey> : QueryOperatorEnumerator<TInputOutput, TLeftKey>
        {

            private QueryOperatorEnumerator<Pair<TInputOutput, NoKeyMemoizationRequired>, TLeftKey> m_leftSource; // Left data source.
            private QueryOperatorEnumerator<Pair<TInputOutput, NoKeyMemoizationRequired>, int> m_rightSource; // Right data source.
            private IEqualityComparer<Wrapper<TInputOutput>> m_comparer; // Comparer to use for equality/hash-coding.
            private IComparer<TLeftKey> m_leftKeyComparer; // Comparer to use to determine ordering of order keys.
            private Dictionary<Wrapper<TInputOutput>, Pair<TInputOutput,TLeftKey>> m_hashLookup; // The hash lookup, used to produce the intersection.
            private CancellationToken m_cancellationToken;

            //---------------------------------------------------------------------------------------
            // Instantiates a new intersection operator.
            //

            internal OrderedIntersectQueryOperatorEnumerator(
                QueryOperatorEnumerator<Pair<TInputOutput, NoKeyMemoizationRequired>, TLeftKey> leftSource,
                QueryOperatorEnumerator<Pair<TInputOutput, NoKeyMemoizationRequired>, int> rightSource,
                IEqualityComparer<TInputOutput> comparer, IComparer<TLeftKey> leftKeyComparer,
                CancellationToken cancellationToken)
            {
                Contract.Assert(leftSource != null);
                Contract.Assert(rightSource != null);

                m_leftSource = leftSource;
                m_rightSource = rightSource;
                m_comparer = new WrapperEqualityComparer<TInputOutput>(comparer);
                m_leftKeyComparer = leftKeyComparer;
                m_cancellationToken = cancellationToken;
            }

            //---------------------------------------------------------------------------------------
            // Walks the two data sources, left and then right, to produce the intersection.
            //

            internal override bool MoveNext(ref TInputOutput currentElement, ref TLeftKey currentKey)
            {
                Contract.Assert(m_leftSource != null);
                Contract.Assert(m_rightSource != null);

                // Build the set out of the left data source, if we haven't already.
                int i = 0;
                if (m_hashLookup == null)
                {
                    m_hashLookup = new Dictionary<Wrapper<TInputOutput>, Pair<TInputOutput, TLeftKey>>(m_comparer);

                    Pair<TInputOutput, NoKeyMemoizationRequired> leftElement = default(Pair<TInputOutput, NoKeyMemoizationRequired>);
                    TLeftKey leftKey = default(TLeftKey);
                    while (m_leftSource.MoveNext(ref leftElement, ref leftKey))
                    {
                        if ((i++ & CancellationState.POLL_INTERVAL) == 0)
                            CancellationState.ThrowIfCanceled(m_cancellationToken);

                        // For each element, we track the smallest order key for that element that we saw so far
                        Pair<TInputOutput, TLeftKey> oldEntry;
                        Wrapper<TInputOutput> wrappedLeftElem = new Wrapper<TInputOutput>(leftElement.First);

                        // If this is the first occurence of this element, or the order key is lower than all keys we saw previously,
                        // update the order key for this element.
                        if (!m_hashLookup.TryGetValue(wrappedLeftElem, out oldEntry) || m_leftKeyComparer.Compare(leftKey, oldEntry.Second) < 0)
                        {
                            // For each "elem" value, we store the smallest key, and the element value that had that key.
                            // Note that even though two element values are "equal" according to the EqualityComparer,
                            // we still cannot choose arbitrarily which of the two to yield.
                            m_hashLookup[wrappedLeftElem] = new Pair<TInputOutput, TLeftKey>(leftElement.First, leftKey);
                        }
                    }
                }

                // Now iterate over the right data source, looking for matches.
                Pair<TInputOutput, NoKeyMemoizationRequired> rightElement = default(Pair<TInputOutput, NoKeyMemoizationRequired>);
                int rightKeyUnused = default(int);
                while (m_rightSource.MoveNext(ref rightElement, ref rightKeyUnused))
                {
                    if ((i++ & CancellationState.POLL_INTERVAL) == 0)
                        CancellationState.ThrowIfCanceled(m_cancellationToken);

                    // If we found the element in our set, and if we haven't returned it yet,
                    // we can yield it to the caller. We also mark it so we know we've returned
                    // it once already and never will again.

                    Pair<TInputOutput, TLeftKey> entry;
                    Wrapper<TInputOutput> wrappedRightElem = new Wrapper<TInputOutput>(rightElement.First);

                    if (m_hashLookup.TryGetValue(wrappedRightElem, out entry))
                    {
                        currentElement = entry.First;
                        currentKey = entry.Second;

                        m_hashLookup.Remove(new Wrapper<TInputOutput>(entry.First));
                        return true;
                    }
                }

                return false;
            }

            protected override void Dispose(bool disposing)
            {
                Contract.Assert(m_leftSource != null && m_rightSource != null);
                m_leftSource.Dispose();
                m_rightSource.Dispose();
            }
        }
    }
}
