// ==++==
//
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
// =+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+
//
// DistinctQueryOperator.cs
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
    /// This operator yields all of the distinct elements in a single data set. It works quite
    /// like the above set operations, with the obvious difference being that it only accepts
    /// a single data source as input. 
    /// </summary>
    /// <typeparam name="TInputOutput"></typeparam>
    internal sealed class DistinctQueryOperator<TInputOutput> : UnaryQueryOperator<TInputOutput, TInputOutput>
    {

        private readonly IEqualityComparer<TInputOutput> m_comparer; // An (optional) equality comparer.

        //---------------------------------------------------------------------------------------
        // Constructs a new distinction operator.
        //

        internal DistinctQueryOperator(IEnumerable<TInputOutput> source, IEqualityComparer<TInputOutput> comparer)
            :base(source)
        {
            Contract.Assert(source != null, "child data source cannot be null");
            m_comparer = comparer;
            SetOrdinalIndexState(OrdinalIndexState.Shuffled);
        }

        //---------------------------------------------------------------------------------------
        // Just opens the current operator, including opening the child and wrapping it with
        // partitions as needed.
        //

        internal override QueryResults<TInputOutput> Open(QuerySettings settings, bool preferStriping)
        {
            // We just open our child operator.  Do not propagate the preferStriping value, but 
            // instead explicitly set it to false. Regardless of whether the parent prefers striping or range
            // partitioning, the output will be hash-partititioned.
            QueryResults<TInputOutput> childResults = Child.Open(settings, false);
            return new UnaryQueryOperatorResults(childResults, this, settings, false);
        }

        internal override void WrapPartitionedStream<TKey>(
            PartitionedStream<TInputOutput, TKey> inputStream, IPartitionedStreamRecipient<TInputOutput> recipient, bool preferStriping, QuerySettings settings)
        {
            // Hash-repartion the source stream
            if (OutputOrdered)
            {
                WrapPartitionedStreamHelper<TKey>(
                    ExchangeUtilities.HashRepartitionOrdered<TInputOutput, NoKeyMemoizationRequired, TKey>(
                        inputStream, null, null, m_comparer, settings.CancellationState.MergedCancellationToken),
                    recipient, settings.CancellationState.MergedCancellationToken);
            }
            else
            {
                WrapPartitionedStreamHelper<int>(
                    ExchangeUtilities.HashRepartition<TInputOutput, NoKeyMemoizationRequired, TKey>(
                        inputStream, null, null, m_comparer, settings.CancellationState.MergedCancellationToken),
                    recipient, settings.CancellationState.MergedCancellationToken);
            }
        }

        //---------------------------------------------------------------------------------------
        // This is a helper method. WrapPartitionedStream decides what type TKey is going
        // to be, and then call this method with that key as a generic parameter.
        //

        private void WrapPartitionedStreamHelper<TKey>(
            PartitionedStream<Pair<TInputOutput, NoKeyMemoizationRequired>, TKey> hashStream,
            IPartitionedStreamRecipient<TInputOutput> recipient, CancellationToken cancellationToken)
        {
            int partitionCount = hashStream.PartitionCount;
            PartitionedStream<TInputOutput, TKey> outputStream =
                new PartitionedStream<TInputOutput, TKey>(partitionCount, hashStream.KeyComparer, OrdinalIndexState.Shuffled);

            for (int i = 0; i < partitionCount; i++)
            {
                if (OutputOrdered)
                {
                    outputStream[i] =
                        new OrderedDistinctQueryOperatorEnumerator<TKey>(hashStream[i], m_comparer, hashStream.KeyComparer, cancellationToken);
                }
                else
                {
                    outputStream[i] = (QueryOperatorEnumerator<TInputOutput, TKey>)(object)
                                                                                   new DistinctQueryOperatorEnumerator<TKey>(hashStream[i], m_comparer, cancellationToken);
                }
            }

            recipient.Receive(outputStream);
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
        // This enumerator performs the distinct operation incrementally. It does this by
        // maintaining a history -- in the form of a set -- of all data already seen. It simply
        // then doesn't return elements it has already seen before.
        //

        class DistinctQueryOperatorEnumerator<TKey> : QueryOperatorEnumerator<TInputOutput, int>
        {

            private QueryOperatorEnumerator<Pair<TInputOutput, NoKeyMemoizationRequired>, TKey> m_source; // The data source.
            private Set<TInputOutput> m_hashLookup; // The hash lookup, used to produce the distinct set.
            private CancellationToken m_cancellationToken;
            private Shared<int> m_outputLoopCount; // Allocated in MoveNext to avoid false sharing.

            //---------------------------------------------------------------------------------------
            // Instantiates a new distinction operator.
            //

            internal DistinctQueryOperatorEnumerator(
                QueryOperatorEnumerator<Pair<TInputOutput, NoKeyMemoizationRequired>, TKey> source, IEqualityComparer<TInputOutput> comparer,
                CancellationToken cancellationToken)
            {
                Contract.Assert(source != null);
                m_source = source;
                m_hashLookup = new Set<TInputOutput>(comparer);
                m_cancellationToken = cancellationToken;
            }

            //---------------------------------------------------------------------------------------
            // Walks the single data source, skipping elements it has already seen.
            //

            internal override bool MoveNext(ref TInputOutput currentElement, ref int currentKey)
            {
                Contract.Assert(m_source != null);
                Contract.Assert(m_hashLookup != null);

                // Iterate over this set's elements until we find a unique element.
                TKey keyUnused = default(TKey);
                Pair<TInputOutput, NoKeyMemoizationRequired> current = default(Pair<TInputOutput, NoKeyMemoizationRequired>);

                if (m_outputLoopCount == null)
                    m_outputLoopCount = new Shared<int>(0);

                while (m_source.MoveNext(ref current, ref keyUnused))
                {
                    if ((m_outputLoopCount.Value++ & CancellationState.POLL_INTERVAL) == 0)
                        CancellationState.ThrowIfCanceled(m_cancellationToken);

                    // We ensure we never return duplicates by tracking them in our set.
                    if (m_hashLookup.Add(current.First))
                    {
#if DEBUG
                        currentKey = unchecked((int)0xdeadbeef);
#endif
                        currentElement = current.First;
                        return true;
                    }
                }

                return false;
            }

            protected override void Dispose(bool disposing)
            {
                Contract.Assert(m_source != null);
                m_source.Dispose();
            }
        }

        //---------------------------------------------------------------------------------------
        // Returns an enumerable that represents the query executing sequentially.
        //

        internal override IEnumerable<TInputOutput> AsSequentialQuery(CancellationToken token)
        {
            IEnumerable<TInputOutput> wrappedChild = CancellableEnumerable.Wrap(Child.AsSequentialQuery(token), token);
            return wrappedChild.Distinct(m_comparer);
        }

        class OrderedDistinctQueryOperatorEnumerator<TKey> : QueryOperatorEnumerator<TInputOutput, TKey>
        {

            private QueryOperatorEnumerator<Pair<TInputOutput, NoKeyMemoizationRequired>, TKey> m_source; // The data source.
            private Dictionary<Wrapper<TInputOutput>, TKey> m_hashLookup; // The hash lookup, used to produce the distinct set.
            private IComparer<TKey> m_keyComparer; // Comparer to decide the key order.
            private IEnumerator<KeyValuePair<Wrapper<TInputOutput>, TKey>> m_hashLookupEnumerator; // Enumerates over m_hashLookup.
            private CancellationToken m_cancellationToken;

            //---------------------------------------------------------------------------------------
            // Instantiates a new distinction operator.
            //

            internal OrderedDistinctQueryOperatorEnumerator(
                QueryOperatorEnumerator<Pair<TInputOutput, NoKeyMemoizationRequired>, TKey> source,
                IEqualityComparer<TInputOutput> comparer, IComparer<TKey> keyComparer,
                CancellationToken cancellationToken)
            {
                Contract.Assert(source != null);
                m_source = source;
                m_keyComparer = keyComparer;

                m_hashLookup = new Dictionary<Wrapper<TInputOutput>, TKey>(
                    new WrapperEqualityComparer<TInputOutput>(comparer));

                m_cancellationToken = cancellationToken;
            }

            //---------------------------------------------------------------------------------------
            // Walks the single data source, skipping elements it has already seen.
            //

            internal override bool MoveNext(ref TInputOutput currentElement, ref TKey currentKey)
            {
                Contract.Assert(m_source != null);
                Contract.Assert(m_hashLookup != null);

                if (m_hashLookupEnumerator == null)
                {
                    Pair<TInputOutput, NoKeyMemoizationRequired> elem = default(Pair<TInputOutput, NoKeyMemoizationRequired>);
                    TKey orderKey = default(TKey);

                    int i = 0;
                    while (m_source.MoveNext(ref elem, ref orderKey))
                    {
                        if ((i++ & CancellationState.POLL_INTERVAL) == 0)
                            CancellationState.ThrowIfCanceled(m_cancellationToken);

                        // For each element, we track the smallest order key for that element that we saw so far
                        TKey oldEntry;

                        Wrapper<TInputOutput> wrappedElem = new Wrapper<TInputOutput>(elem.First);

                        // If this is the first occurence of this element, or the order key is lower than all keys we saw previously,
                        // update the order key for this element.
                        if (!m_hashLookup.TryGetValue(wrappedElem, out oldEntry) || m_keyComparer.Compare(orderKey, oldEntry) < 0)
                        {
                            // For each "elem" value, we store the smallest key, and the element value that had that key.
                            // Note that even though two element values are "equal" according to the EqualityComparer,
                            // we still cannot choose arbitrarily which of the two to yield.
                            m_hashLookup[wrappedElem] = orderKey;
                        }
                    }

                    m_hashLookupEnumerator = m_hashLookup.GetEnumerator();
                }

                if (m_hashLookupEnumerator.MoveNext())
                {
                    KeyValuePair<Wrapper<TInputOutput>, TKey> currentPair = m_hashLookupEnumerator.Current;
                    currentElement = currentPair.Key.Value;
                    currentKey = currentPair.Value;
                    return true;
                }

                return false;
            }

            protected override void Dispose(bool disposing)
            {
                Contract.Assert(m_source != null);
                m_source.Dispose();

                if (m_hashLookupEnumerator != null)
                {
                    m_hashLookupEnumerator.Dispose();
                }
            }
        }

    }
}
