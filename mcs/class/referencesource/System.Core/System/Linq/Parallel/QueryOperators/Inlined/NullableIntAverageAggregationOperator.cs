// ==++==
//
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
// =+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+
//
// NullableIntAverageAggregationOperator.cs
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
    /// An inlined average aggregation operator and its enumerator, for Nullable ints. 
    /// </summary>
    internal sealed class NullableIntAverageAggregationOperator : InlinedAggregationOperator<int?, Pair<long, long>, double?>
    {
        //---------------------------------------------------------------------------------------
        // Constructs a new instance of an average associative operator.
        //

        internal NullableIntAverageAggregationOperator(IEnumerable<int?> child) : base(child)
        {
        }

        //---------------------------------------------------------------------------------------
        // Executes the entire query tree, and aggregates the intermediate results into the
        // final result based on the binary operators and final reduction.
        //
        // Return Value:
        //     The single result of aggregation.
        //

        protected override double? InternalAggregate(ref Exception singularExceptionToThrow)
        {
            // Because the final reduction is typically much cheaper than the intermediate 
            // reductions over the individual partitions, and because each parallel partition
            // will do a lot of work to produce a single output element, we prefer to turn off
            // pipelining, and process the final reductions serially.
            using (IEnumerator<Pair<long, long>> enumerator = GetEnumerator(ParallelMergeOptions.FullyBuffered, true))
            {
                // If the sequence was empty, return null right away.
                if (!enumerator.MoveNext())
                {
                    return null;
                }

                Pair<long, long> result = enumerator.Current;

                // Simply add together the sums and totals.
                while (enumerator.MoveNext())
                {
                    checked
                    {
                        result.First += enumerator.Current.First;
                        result.Second += enumerator.Current.Second;
                    }
                }

                // And divide the sum by the total to obtain the final result.
                return (double)result.First / result.Second;
            }
        }

        //---------------------------------------------------------------------------------------
        // Creates an enumerator that is used internally for the final aggregation step.
        //

        protected override QueryOperatorEnumerator<Pair<long, long>,int> CreateEnumerator<TKey>(
            int index, int count, QueryOperatorEnumerator<int?, TKey> source, object sharedData, CancellationToken cancellationToken)
        {
            return new NullableIntAverageAggregationOperatorEnumerator<TKey>(source, index, cancellationToken);
        }

        //---------------------------------------------------------------------------------------
        // This enumerator type encapsulates the intermediary aggregation over the underlying
        // (possibly partitioned) data source.
        //

        private class NullableIntAverageAggregationOperatorEnumerator<TKey> : InlinedAggregationOperatorEnumerator<Pair<long, long>>
        {
            private QueryOperatorEnumerator<int?, TKey> m_source; // The source data.

            //---------------------------------------------------------------------------------------
            // Instantiates a new aggregation operator.
            //

            internal NullableIntAverageAggregationOperatorEnumerator(QueryOperatorEnumerator<int?, TKey> source, int partitionIndex,
                CancellationToken cancellationToken) :
                base(partitionIndex, cancellationToken)
            {
                Contract.Assert(source != null);
                m_source = source;
            }

            //---------------------------------------------------------------------------------------
            // Tallies up the average of the underlying data source, walking the entire thing the first
            // time MoveNext is called on this object.
            //

            protected override bool MoveNextCore(ref Pair<long, long> currentElement)
            {
                // The temporary result contains the running sum and count, respectively.
                long sum = 0;
                long count = 0;

                QueryOperatorEnumerator<int?, TKey> source = m_source;
                int? current = default(int?);
                TKey keyUnused = default(TKey);

                int i = 0;
                while (source.MoveNext(ref current, ref keyUnused))
                {
                    if ((i++ & CancellationState.POLL_INTERVAL) == 0)
                        CancellationState.ThrowIfCanceled(m_cancellationToken);

                    if (current.HasValue)
                    {
                        sum += current.GetValueOrDefault();
                        count++;
                    }
                }

                currentElement = new Pair<long, long>(sum, count);
                return count > 0;
            }

            //---------------------------------------------------------------------------------------
            // Dispose of resources associated with the underlying enumerator.
            //

            protected override void Dispose(bool disposing)
            {
                Contract.Assert(m_source != null);
                m_source.Dispose();
            }
        }
    }
}
