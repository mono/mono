// ==++==
//
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
// =+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+
//
// LongMinMaxAggregationOperator.cs
//
// <OWNER>[....]</OWNER>
//
// =-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Threading;
#if SILVERLIGHT
using System.Core; // for System.Core.SR
#endif

namespace System.Linq.Parallel
{
    /// <summary>
    /// An inlined min/max aggregation and its enumerator, for longs. 
    /// </summary>
    internal sealed class LongMinMaxAggregationOperator : InlinedAggregationOperator<long, long, long>
    {
        private readonly int m_sign; // The sign (-1 for min, 1 for max).

        //---------------------------------------------------------------------------------------
        // Constructs a new instance of a min/max associative operator.
        //

        internal LongMinMaxAggregationOperator(IEnumerable<long> child, int sign) : base(child)
        {
            Contract.Assert(sign == -1 || sign == 1, "invalid sign");
            m_sign = sign;
        }

        //---------------------------------------------------------------------------------------
        // Executes the entire query tree, and aggregates the intermediate results into the
        // final result based on the binary operators and final reduction.
        //
        // Return Value:
        //     The single result of aggregation.
        //

        protected override long InternalAggregate(ref Exception singularExceptionToThrow)
        {
            // Because the final reduction is typically much cheaper than the intermediate 
            // reductions over the individual partitions, and because each parallel partition
            // will do a lot of work to produce a single output element, we prefer to turn off
            // pipelining, and process the final reductions serially.
            using (IEnumerator<long> enumerator = GetEnumerator(ParallelMergeOptions.FullyBuffered, true))
            {
                // Throw an error for empty results.
                if (!enumerator.MoveNext())
                {
                    singularExceptionToThrow = new InvalidOperationException(SR.GetString(SR.NoElements));
                    return default(long);
                }

                long best = enumerator.Current;

                // Based on the sign, do either a min or max reduction.
                if (m_sign == -1)
                {
                    while (enumerator.MoveNext())
                    {
                        long current = enumerator.Current;
                        if (current < best)
                        {
                            best = current;
                        }
                    }
                }
                else
                {
                    while (enumerator.MoveNext())
                    {
                        long current = enumerator.Current;
                        if (current > best)
                        {
                            best = current;
                        }
                    }
                }

                return best;
            }
        }

        //---------------------------------------------------------------------------------------
        // Creates an enumerator that is used internally for the final aggregation step.
        //

        protected override QueryOperatorEnumerator<long, int> CreateEnumerator<TKey>(
            int index, int count, QueryOperatorEnumerator<long, TKey> source, object sharedData,
            CancellationToken cancellationToken)
        {
            return new LongMinMaxAggregationOperatorEnumerator<TKey>(source, index, m_sign, cancellationToken);
        }

        //---------------------------------------------------------------------------------------
        // This enumerator type encapsulates the intermediary aggregation over the underlying
        // (possibly partitioned) data source.
        //

        private class LongMinMaxAggregationOperatorEnumerator<TKey> : InlinedAggregationOperatorEnumerator<long>
        {
            private QueryOperatorEnumerator<long, TKey> m_source; // The source data.
            private int m_sign; // The sign for comparisons (-1 means min, 1 means max).

            //---------------------------------------------------------------------------------------
            // Instantiates a new aggregation operator.
            //

            internal LongMinMaxAggregationOperatorEnumerator(QueryOperatorEnumerator<long, TKey> source, int partitionIndex, int sign,
                CancellationToken cancellationToken) :
                base(partitionIndex, cancellationToken)
            {
                Contract.Assert(source != null);
                m_source = source;
                m_sign = sign;
            }

            //---------------------------------------------------------------------------------------
            // Tallies up the min/max of the underlying data source, walking the entire thing the first
            // time MoveNext is called on this object.
            //

            protected override bool MoveNextCore(ref long currentElement)
            {
                // Based on the sign, do either a min or max reduction.
                QueryOperatorEnumerator<long, TKey> source = m_source;
                TKey keyUnused = default(TKey);

                if (source.MoveNext(ref currentElement, ref keyUnused))
                {
                    int i = 0;
                    // We just scroll through the enumerator and find the min or max.
                    if (m_sign == -1)
                    {
                        long elem = default(long);
                        while (source.MoveNext(ref elem, ref keyUnused))
                        {
                            if ((i++ & CancellationState.POLL_INTERVAL) == 0)
                                CancellationState.ThrowIfCanceled(m_cancellationToken);

                            if (elem < currentElement)
                            {
                                currentElement = elem;
                            }
                        }
                    }
                    else
                    {
                        long elem = default(long);
                        while (source.MoveNext(ref elem, ref keyUnused))
                        {
                            if ((i++ & CancellationState.POLL_INTERVAL) == 0)
                                CancellationState.ThrowIfCanceled(m_cancellationToken);

                            if (elem > currentElement)
                            {
                                currentElement = elem;
                            }
                        }
                    }

                    // The sum has been calculated. Now just return.
                    return true;
                }

                return false;
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
