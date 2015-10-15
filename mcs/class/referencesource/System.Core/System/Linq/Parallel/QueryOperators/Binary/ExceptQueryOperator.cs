// ==++==
//
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
// =+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+
//
// ExceptQueryOperator.cs
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
    /// Operator that yields the elements from the first data source that aren't in the second.
    /// This is known as the set relative complement, i.e. left - right. 
    /// </summary>
    /// <typeparam name="TInputOutput"></typeparam>
    internal sealed class ExceptQueryOperator<TInputOutput> :
        BinaryQueryOperator<TInputOutput, TInputOutput, TInputOutput>
    {

        private readonly IEqualityComparer<TInputOutput> m_comparer; // An equality comparer.

        //---------------------------------------------------------------------------------------
        // Constructs a new set except operator.
        //

        internal ExceptQueryOperator(ParallelQuery<TInputOutput> left, ParallelQuery<TInputOutput> right, IEqualityComparer<TInputOutput> comparer)
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
            PartitionedStream<TInputOutput, TLeftKey> leftStream, PartitionedStream<TInputOutput, TRightKey> rightStream,
            IPartitionedStreamRecipient<TInputOutput> outputRecipient, bool preferStriping, QuerySettings settings)
        {
            Contract.Assert(leftStream.PartitionCount == rightStream.PartitionCount);

            if (OutputOrdered)
            {
                WrapPartitionedStreamHelper<TLeftKey, TRightKey>(
                    ExchangeUtilities.HashRepartitionOrdered<TInputOutput, NoKeyMemoizationRequired, TLeftKey>(
                        leftStream, null, null, m_comparer, settings.CancellationState.MergedCancellationToken),
                    rightStream, outputRecipient, settings.CancellationState.MergedCancellationToken);
            }
            else
            {
                WrapPartitionedStreamHelper<int, TRightKey>(
                    ExchangeUtilities.HashRepartition<TInputOutput, NoKeyMemoizationRequired, TLeftKey>(
                        leftStream, null, null, m_comparer, settings.CancellationState.MergedCancellationToken),
                    rightStream, outputRecipient, settings.CancellationState.MergedCancellationToken);
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
                    outputStream[i] = new OrderedExceptQueryOperatorEnumerator<TLeftKey>(
                        leftHashStream[i], rightHashStream[i], m_comparer, leftHashStream.KeyComparer, cancellationToken);
                }
                else
                {
                    outputStream[i] = (QueryOperatorEnumerator<TInputOutput, TLeftKey>)(object)
                        new ExceptQueryOperatorEnumerator<TLeftKey>(leftHashStream[i], rightHashStream[i], m_comparer, cancellationToken);
                }
            }

            outputRecipient.Receive(outputStream);
        }

        
        //---------------------------------------------------------------------------------------
        // Returns an enumerable that represents the query executing sequentially.
        //

        internal override IEnumerable<TInputOutput> AsSequentialQuery(CancellationToken token)
        {
            IEnumerable<TInputOutput> wrappedLeftChild = CancellableEnumerable.Wrap(LeftChild.AsSequentialQuery(token), token);
            IEnumerable<TInputOutput> wrappedRightChild = CancellableEnumerable.Wrap(RightChild.AsSequentialQuery(token), token);
            return wrappedLeftChild.Except(wrappedRightChild, m_comparer);
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
        // This enumerator calculates the distinct set incrementally. It does this by maintaining
        // a history -- in the form of a set -- of all data already seen. It then only returns
        // elements that have not yet been seen.
        //

        class ExceptQueryOperatorEnumerator<TLeftKey> : QueryOperatorEnumerator<TInputOutput, int>
        {
            private QueryOperatorEnumerator<Pair<TInputOutput, NoKeyMemoizationRequired>, TLeftKey> m_leftSource; // Left data source.
            private QueryOperatorEnumerator<Pair<TInputOutput, NoKeyMemoizationRequired>, int> m_rightSource; // Right data source.
            private IEqualityComparer<TInputOutput> m_comparer; // A comparer used for equality checks/hash-coding.
            private Set<TInputOutput> m_hashLookup; // The hash lookup, used to produce the distinct set.
            private CancellationToken m_cancellationToken;
            private Shared<int> m_outputLoopCount;

            //---------------------------------------------------------------------------------------
            // Instantiates a new except query operator enumerator.
            //

            internal ExceptQueryOperatorEnumerator(
                QueryOperatorEnumerator<Pair<TInputOutput, NoKeyMemoizationRequired>, TLeftKey> leftSource,
                QueryOperatorEnumerator<Pair<TInputOutput, NoKeyMemoizationRequired>, int> rightSource,
                IEqualityComparer<TInputOutput> comparer,
                CancellationToken cancellationToken)
            {
                Contract.Assert(leftSource != null);
                Contract.Assert(rightSource != null);

                m_leftSource = leftSource;
                m_rightSource = rightSource;
                m_comparer = comparer;
                m_cancellationToken = cancellationToken;
            }

            //---------------------------------------------------------------------------------------
            // Walks the two data sources, left and then right, to produce the distinct set
            //

            internal override bool MoveNext(ref TInputOutput currentElement, ref int currentKey)
            {
                Contract.Assert(m_leftSource != null);
                Contract.Assert(m_rightSource != null);

                // Build the set out of the left data source, if we haven't already.
                
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

                // Now iterate over the right data source, looking for matches.
                Pair<TInputOutput, NoKeyMemoizationRequired> leftElement = default(Pair<TInputOutput, NoKeyMemoizationRequired>);
                TLeftKey leftKeyUnused = default(TLeftKey);

                while (m_leftSource.MoveNext(ref leftElement, ref leftKeyUnused))
                {
                    if ((m_outputLoopCount.Value++ & CancellationState.POLL_INTERVAL) == 0)
                        CancellationState.ThrowIfCanceled(m_cancellationToken);

                    if (m_hashLookup.Add(leftElement.First))
                    {
                        // This element has never been seen. Return it.
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

        class OrderedExceptQueryOperatorEnumerator<TLeftKey> : QueryOperatorEnumerator<TInputOutput, TLeftKey>
        {
            private QueryOperatorEnumerator<Pair<TInputOutput, NoKeyMemoizationRequired>, TLeftKey> m_leftSource; // Left data source.
            private QueryOperatorEnumerator<Pair<TInputOutput, NoKeyMemoizationRequired>, int> m_rightSource; // Right data source.
            private IEqualityComparer<TInputOutput> m_comparer; // A comparer used for equality checks/hash-coding.
            private IComparer<TLeftKey> m_leftKeyComparer; // A comparer for order keys.
            private IEnumerator<KeyValuePair<Wrapper<TInputOutput>, Pair<TInputOutput, TLeftKey>>> m_outputEnumerator; // The enumerator output elements + order keys.
            private CancellationToken m_cancellationToken;

            //---------------------------------------------------------------------------------------
            // Instantiates a new except query operator enumerator.
            //

            internal OrderedExceptQueryOperatorEnumerator(
                QueryOperatorEnumerator<Pair<TInputOutput, NoKeyMemoizationRequired>, TLeftKey> leftSource,
                QueryOperatorEnumerator<Pair<TInputOutput, NoKeyMemoizationRequired>, int> rightSource,
                IEqualityComparer<TInputOutput> comparer, IComparer<TLeftKey> leftKeyComparer,
                CancellationToken cancellationToken)
            {
                Contract.Assert(leftSource != null);
                Contract.Assert(rightSource != null);

                m_leftSource = leftSource;
                m_rightSource = rightSource;
                m_comparer = comparer;
                m_leftKeyComparer = leftKeyComparer;
                m_cancellationToken = cancellationToken;
            }

            //---------------------------------------------------------------------------------------
            // Walks the two data sources, left and then right, to produce the distinct set
            //

            internal override bool MoveNext(ref TInputOutput currentElement, ref TLeftKey currentKey)
            {
                Contract.Assert(m_leftSource != null);
                Contract.Assert(m_rightSource != null);

                // Build the set out of the left data source, if we haven't already.
                if (m_outputEnumerator == null)
                {
                    Set<TInputOutput> rightLookup = new Set<TInputOutput>(m_comparer);

                    Pair<TInputOutput, NoKeyMemoizationRequired> rightElement = default(Pair<TInputOutput, NoKeyMemoizationRequired>);
                    int rightKeyUnused = default(int);
                    int i=0;
                    while (m_rightSource.MoveNext(ref rightElement, ref rightKeyUnused))
                    {
                        if ((i++ & CancellationState.POLL_INTERVAL) == 0)
                            CancellationState.ThrowIfCanceled(m_cancellationToken);

                        rightLookup.Add(rightElement.First);
                    }

                    var leftLookup = 
                        new Dictionary<Wrapper<TInputOutput>, Pair<TInputOutput, TLeftKey>>(
                            new WrapperEqualityComparer<TInputOutput>(m_comparer));

                    Pair<TInputOutput, NoKeyMemoizationRequired> leftElement = default(Pair<TInputOutput, NoKeyMemoizationRequired>);
                    TLeftKey leftKey = default(TLeftKey);
                    while (m_leftSource.MoveNext(ref leftElement, ref leftKey))
                    {
                        if ((i++ & CancellationState.POLL_INTERVAL) == 0)
                            CancellationState.ThrowIfCanceled(m_cancellationToken);

                        if (rightLookup.Contains(leftElement.First))
                        {
                            continue;
                        }

                        Pair<TInputOutput, TLeftKey> oldEntry;
                        Wrapper<TInputOutput> wrappedLeftElement = new Wrapper<TInputOutput>(leftElement.First);
                        if (!leftLookup.TryGetValue(wrappedLeftElement, out oldEntry) || m_leftKeyComparer.Compare(leftKey, oldEntry.Second) < 0)
                        {
                            // For each "elem" value, we store the smallest key, and the element value that had that key.
                            // Note that even though two element values are "equal" according to the EqualityComparer,
                            // we still cannot choose arbitrarily which of the two to yield.
                            leftLookup[wrappedLeftElement] = new Pair<TInputOutput, TLeftKey>(leftElement.First, leftKey);
                        }
                    }

                    m_outputEnumerator = leftLookup.GetEnumerator();
                }

                if (m_outputEnumerator.MoveNext())
                {
                    Pair<TInputOutput, TLeftKey> currentPair = m_outputEnumerator.Current.Value;
                    currentElement = currentPair.First;
                    currentKey = currentPair.Second;
                    return true;
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
