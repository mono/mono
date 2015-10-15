// ==++==
//
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
// =+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+
//
// UnionQueryOperator.cs
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
    /// Operator that yields the union of two data sources. 
    /// </summary>
    /// <typeparam name="TInputOutput"></typeparam>
    internal sealed class UnionQueryOperator<TInputOutput> :
        BinaryQueryOperator<TInputOutput, TInputOutput, TInputOutput>
    {

        private readonly IEqualityComparer<TInputOutput> m_comparer; // An equality comparer.

        //---------------------------------------------------------------------------------------
        // Constructs a new union operator.
        //

        internal UnionQueryOperator(ParallelQuery<TInputOutput> left, ParallelQuery<TInputOutput> right, IEqualityComparer<TInputOutput> comparer)
            :base(left, right)
        {
            Contract.Assert(left != null && right != null, "child data sources cannot be null");

            m_comparer = comparer;
            m_outputOrdered = LeftChild.OutputOrdered || RightChild.OutputOrdered;
        }

        //---------------------------------------------------------------------------------------
        // Just opens the current operator, including opening the child and wrapping it with
        // partitions as needed.
        //

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
            int partitionCount = leftStream.PartitionCount;

            // Wrap both child streams with hash repartition

            if (LeftChild.OutputOrdered)
            {
                PartitionedStream<Pair<TInputOutput, NoKeyMemoizationRequired>, TLeftKey> leftHashStream =
                    ExchangeUtilities.HashRepartitionOrdered<TInputOutput, NoKeyMemoizationRequired, TLeftKey>(
                        leftStream, null, null, m_comparer, settings.CancellationState.MergedCancellationToken);

                WrapPartitionedStreamFixedLeftType<TLeftKey, TRightKey>(
                    leftHashStream, rightStream, outputRecipient, partitionCount, settings.CancellationState.MergedCancellationToken);
            }
            else
            {
                PartitionedStream<Pair<TInputOutput, NoKeyMemoizationRequired>, int> leftHashStream =
                    ExchangeUtilities.HashRepartition<TInputOutput, NoKeyMemoizationRequired, TLeftKey>(
                        leftStream, null, null, m_comparer, settings.CancellationState.MergedCancellationToken);

                WrapPartitionedStreamFixedLeftType<int, TRightKey>(
                    leftHashStream, rightStream, outputRecipient, partitionCount, settings.CancellationState.MergedCancellationToken);
            }
        }

        //---------------------------------------------------------------------------------------
        // A helper method that allows WrapPartitionedStream to fix the TLeftKey type parameter.
        //

        private void WrapPartitionedStreamFixedLeftType<TLeftKey, TRightKey>(
            PartitionedStream<Pair<TInputOutput, NoKeyMemoizationRequired>, TLeftKey> leftHashStream, PartitionedStream<TInputOutput, TRightKey> rightStream,
            IPartitionedStreamRecipient<TInputOutput> outputRecipient, int partitionCount, CancellationToken cancellationToken)
        {
            if (RightChild.OutputOrdered)
            {
                PartitionedStream<Pair<TInputOutput, NoKeyMemoizationRequired>, TRightKey> rightHashStream =
                    ExchangeUtilities.HashRepartitionOrdered<TInputOutput, NoKeyMemoizationRequired, TRightKey>(
                        rightStream, null, null, m_comparer, cancellationToken);

                WrapPartitionedStreamFixedBothTypes<TLeftKey, TRightKey>(
                    leftHashStream, rightHashStream, outputRecipient, partitionCount, cancellationToken);
            }
            else
            {
                PartitionedStream<Pair<TInputOutput, NoKeyMemoizationRequired>, int> rightHashStream =
                    ExchangeUtilities.HashRepartition<TInputOutput, NoKeyMemoizationRequired, TRightKey>(
                        rightStream, null, null, m_comparer, cancellationToken);

                WrapPartitionedStreamFixedBothTypes<TLeftKey, int>(
                    leftHashStream, rightHashStream, outputRecipient, partitionCount, cancellationToken);
            }
        }

        //---------------------------------------------------------------------------------------
        // A helper method that allows WrapPartitionedStreamHelper to fix the TRightKey type parameter.
        //

        private void WrapPartitionedStreamFixedBothTypes<TLeftKey, TRightKey>(
            PartitionedStream<Pair<TInputOutput, NoKeyMemoizationRequired>, TLeftKey> leftHashStream,
            PartitionedStream<Pair<TInputOutput, NoKeyMemoizationRequired>, TRightKey> rightHashStream,
            IPartitionedStreamRecipient<TInputOutput> outputRecipient, int partitionCount,
            CancellationToken cancellationToken)
        {
            if (LeftChild.OutputOrdered || RightChild.OutputOrdered)
            {
                IComparer<ConcatKey<TLeftKey, TRightKey>> compoundKeyComparer =
                    ConcatKey<TLeftKey, TRightKey>.MakeComparer(leftHashStream.KeyComparer, rightHashStream.KeyComparer);

                PartitionedStream<TInputOutput, ConcatKey<TLeftKey, TRightKey>> outputStream =
                    new PartitionedStream<TInputOutput, ConcatKey<TLeftKey, TRightKey>>(partitionCount, compoundKeyComparer, OrdinalIndexState.Shuffled);

                for (int i = 0; i < partitionCount; i++)
                {
                    outputStream[i] = new OrderedUnionQueryOperatorEnumerator<TLeftKey, TRightKey>(
                        leftHashStream[i], rightHashStream[i], LeftChild.OutputOrdered, RightChild.OutputOrdered,
                        m_comparer, compoundKeyComparer, cancellationToken);
                }

                outputRecipient.Receive(outputStream);
            }
            else
            {
                PartitionedStream<TInputOutput, int> outputStream =
                    new PartitionedStream<TInputOutput, int>(partitionCount, Util.GetDefaultComparer<int>(), OrdinalIndexState.Shuffled);

                for (int i = 0; i < partitionCount; i++)
                {
                    outputStream[i] = new UnionQueryOperatorEnumerator<TLeftKey, TRightKey>(
                        leftHashStream[i], rightHashStream[i], i, m_comparer, cancellationToken);
                }

                outputRecipient.Receive(outputStream);
            }
        }


        //---------------------------------------------------------------------------------------
        // Returns an enumerable that represents the query executing sequentially.
        //

        internal override IEnumerable<TInputOutput> AsSequentialQuery(CancellationToken token)
        {
            IEnumerable<TInputOutput> wrappedLeftChild = CancellableEnumerable.Wrap(LeftChild.AsSequentialQuery(token), token);
            IEnumerable<TInputOutput> wrappedRightChild = CancellableEnumerable.Wrap(RightChild.AsSequentialQuery(token), token);
            return wrappedLeftChild.Union(wrappedRightChild, m_comparer);
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
        // This enumerator performs the union operation incrementally. It does this by maintaining
        // a history -- in the form of a set -- of all data already seen. It is careful not to
        // return any duplicates.
        //

        class UnionQueryOperatorEnumerator<TLeftKey, TRightKey> : QueryOperatorEnumerator<TInputOutput, int>
        {

            private QueryOperatorEnumerator<Pair<TInputOutput, NoKeyMemoizationRequired>, TLeftKey> m_leftSource; // Left data source.
            private QueryOperatorEnumerator<Pair<TInputOutput, NoKeyMemoizationRequired>, TRightKey> m_rightSource; // Right data source.
            private readonly int m_partitionIndex; // The current partition.
            private Set<TInputOutput> m_hashLookup; // The hash lookup, used to produce the union.
            private CancellationToken m_cancellationToken;
            private Shared<int> m_outputLoopCount;
            private readonly IEqualityComparer<TInputOutput> m_comparer;

            //---------------------------------------------------------------------------------------
            // Instantiates a new union operator.
            //

            internal UnionQueryOperatorEnumerator(
                QueryOperatorEnumerator<Pair<TInputOutput, NoKeyMemoizationRequired>, TLeftKey> leftSource,
                QueryOperatorEnumerator<Pair<TInputOutput, NoKeyMemoizationRequired>, TRightKey> rightSource,
                int partitionIndex, IEqualityComparer<TInputOutput> comparer,
                CancellationToken cancellationToken)
            {
                Contract.Assert(leftSource != null);
                Contract.Assert(rightSource != null);

                m_leftSource = leftSource;
                m_rightSource = rightSource;
                m_partitionIndex = partitionIndex;
                m_comparer = comparer;
                m_cancellationToken = cancellationToken;
            }

            //---------------------------------------------------------------------------------------
            // Walks the two data sources, left and then right, to produce the union.
            //

            internal override bool MoveNext(ref TInputOutput currentElement, ref int currentKey)
            {
                if (m_hashLookup == null)
                {
                    m_hashLookup = new Set<TInputOutput>(m_comparer);
                    m_outputLoopCount = new Shared<int>(0);
                }

                Contract.Assert(m_hashLookup != null);

                // Enumerate the left and then right data source. When each is done, we set the
                // field to null so we will skip it upon subsequent calls to MoveNext.
                if (m_leftSource != null)
                {
                    // Iterate over this set's elements until we find a unique element.
                    TLeftKey keyUnused = default(TLeftKey);
                    Pair<TInputOutput, NoKeyMemoizationRequired> currentLeftElement = default(Pair<TInputOutput, NoKeyMemoizationRequired>);

                    int i = 0;
                    while (m_leftSource.MoveNext(ref currentLeftElement, ref keyUnused))
                    {
                        if ((i++ & CancellationState.POLL_INTERVAL) == 0)
                            CancellationState.ThrowIfCanceled(m_cancellationToken);

                        // We ensure we never return duplicates by tracking them in our set.
                        if (m_hashLookup.Add(currentLeftElement.First))
                        {
#if DEBUG
                            currentKey = unchecked((int)0xdeadbeef);
#endif
                            currentElement = currentLeftElement.First;
                            return true;
                        }
                    }

                    m_leftSource.Dispose();
                    m_leftSource = null;
                }
                
                
                if (m_rightSource != null)
                {
                    // Iterate over this set's elements until we find a unique element.
                    TRightKey keyUnused = default(TRightKey);
                    Pair<TInputOutput, NoKeyMemoizationRequired> currentRightElement = default(Pair<TInputOutput, NoKeyMemoizationRequired>);

                    while (m_rightSource.MoveNext(ref currentRightElement, ref keyUnused))
                    {
                        if ((m_outputLoopCount.Value++ & CancellationState.POLL_INTERVAL) == 0)
                            CancellationState.ThrowIfCanceled(m_cancellationToken);

                        // We ensure we never return duplicates by tracking them in our set.
                        if (m_hashLookup.Add(currentRightElement.First))
                        {
#if DEBUG
                            currentKey = unchecked((int)0xdeadbeef);
#endif
                            currentElement = currentRightElement.First;
                            return true;
                        }
                    }

                    m_rightSource.Dispose();
                    m_rightSource = null;
                }

                return false;
            }

            protected override void Dispose(bool disposing)
            {
                if (m_leftSource != null)
                {
                    m_leftSource.Dispose();
                }
                if (m_rightSource != null)
                {
                    m_rightSource.Dispose();
                }
            }
        }

        class OrderedUnionQueryOperatorEnumerator<TLeftKey, TRightKey> : QueryOperatorEnumerator<TInputOutput, ConcatKey<TLeftKey, TRightKey>>
        {
            private QueryOperatorEnumerator<Pair<TInputOutput, NoKeyMemoizationRequired>, TLeftKey> m_leftSource; // Left data source.
            private QueryOperatorEnumerator<Pair<TInputOutput, NoKeyMemoizationRequired>, TRightKey> m_rightSource; // Right data source.
            private IComparer<ConcatKey<TLeftKey, TRightKey>> m_keyComparer; // Comparer for compound order keys.
            private IEnumerator<KeyValuePair<Wrapper<TInputOutput>, Pair<TInputOutput, ConcatKey<TLeftKey, TRightKey>>>> m_outputEnumerator; // Enumerator over the output of the union.
            private bool m_leftOrdered; // Whether the left data source is ordered.
            private bool m_rightOrdered; // Whether the right data source is ordered.
            private IEqualityComparer<TInputOutput> m_comparer; // Comparer for the elements.
            private CancellationToken m_cancellationToken;

            //---------------------------------------------------------------------------------------
            // Instantiates a new union operator.
            //

            internal OrderedUnionQueryOperatorEnumerator(
                QueryOperatorEnumerator<Pair<TInputOutput, NoKeyMemoizationRequired>, TLeftKey> leftSource,
                QueryOperatorEnumerator<Pair<TInputOutput, NoKeyMemoizationRequired>, TRightKey> rightSource,
                bool leftOrdered, bool rightOrdered, IEqualityComparer<TInputOutput> comparer, IComparer<ConcatKey<TLeftKey, TRightKey>> keyComparer,
                CancellationToken cancellationToken)
            {
                Contract.Assert(leftSource != null);
                Contract.Assert(rightSource != null);

                m_leftSource = leftSource;
                m_rightSource = rightSource;
                m_keyComparer = keyComparer;

                m_leftOrdered = leftOrdered;
                m_rightOrdered = rightOrdered;
                m_comparer = comparer;

                if (m_comparer == null)
                {
                    m_comparer = EqualityComparer<TInputOutput>.Default;
                }

                m_cancellationToken = cancellationToken;
            }

            //---------------------------------------------------------------------------------------
            // Walks the two data sources, left and then right, to produce the union.
            //

            internal override bool MoveNext(ref TInputOutput currentElement, ref ConcatKey<TLeftKey, TRightKey> currentKey)
            {
                Contract.Assert(m_leftSource != null);
                Contract.Assert(m_rightSource != null);

                if (m_outputEnumerator == null)
                {
                    IEqualityComparer<Wrapper<TInputOutput>> wrapperComparer = new WrapperEqualityComparer<TInputOutput>(m_comparer);
                    Dictionary<Wrapper<TInputOutput>, Pair<TInputOutput, ConcatKey<TLeftKey, TRightKey>>> union =
                        new Dictionary<Wrapper<TInputOutput>, Pair<TInputOutput, ConcatKey<TLeftKey, TRightKey>>>(wrapperComparer);

                    Pair<TInputOutput, NoKeyMemoizationRequired> elem = default(Pair<TInputOutput, NoKeyMemoizationRequired>);
                    TLeftKey leftKey = default(TLeftKey);

                    int i = 0;
                    while (m_leftSource.MoveNext(ref elem, ref leftKey))
                    {
                        if ((i++ & CancellationState.POLL_INTERVAL) == 0)
                            CancellationState.ThrowIfCanceled(m_cancellationToken);

                        ConcatKey<TLeftKey, TRightKey> key =
                            ConcatKey<TLeftKey, TRightKey>.MakeLeft(m_leftOrdered ? leftKey : default(TLeftKey));
                        Pair<TInputOutput, ConcatKey<TLeftKey, TRightKey>> oldEntry;
                        Wrapper<TInputOutput> wrappedElem = new Wrapper<TInputOutput>(elem.First);

                        if (!union.TryGetValue(wrappedElem, out oldEntry) || m_keyComparer.Compare(key, oldEntry.Second) < 0)
                        {
                            union[wrappedElem] = new Pair<TInputOutput, ConcatKey<TLeftKey, TRightKey>>(elem.First, key);
                        }
                    }

                    TRightKey rightKey = default(TRightKey);
                    while (m_rightSource.MoveNext(ref elem, ref rightKey))
                    {
                        if ((i++ & CancellationState.POLL_INTERVAL) == 0)
                            CancellationState.ThrowIfCanceled(m_cancellationToken);

                        ConcatKey<TLeftKey, TRightKey> key =
                            ConcatKey<TLeftKey, TRightKey>.MakeRight(m_rightOrdered ? rightKey : default(TRightKey));
                        Pair<TInputOutput, ConcatKey<TLeftKey, TRightKey>> oldEntry;
                        Wrapper<TInputOutput> wrappedElem = new Wrapper<TInputOutput>(elem.First);

                        if (!union.TryGetValue(wrappedElem, out oldEntry) || m_keyComparer.Compare(key, oldEntry.Second) < 0)
                        {
                            union[wrappedElem] = new Pair<TInputOutput, ConcatKey<TLeftKey, TRightKey>>(elem.First, key); ;
                        }
                    }

                    m_outputEnumerator = union.GetEnumerator();
                }

                if (m_outputEnumerator.MoveNext())
                {
                    Pair<TInputOutput, ConcatKey<TLeftKey, TRightKey>> current = m_outputEnumerator.Current.Value;
                    currentElement = current.First;
                    currentKey = current.Second;
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
