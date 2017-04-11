// ==++==
//
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
// =+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+
//
// FloatMinMaxAggregationOperator.cs
//
// <OWNER>Microsoft</OWNER>
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
    // Notes:
    //     Note that normally float.NaN &lt; anything is false, as is anything &lt; NaN.  This would
    //     lead to some strangeness in Min and Max, e.g. Min({ NaN, 5.0 } == NaN, yet
    //     Min({ 5.0, NaN }) == 5.0!  We impose a total ordering so that NaN is smaller than
    //     everything, including -infinity, which is consistent with Comparer<T>. 
    /// <summary>
    /// An inlined min/max aggregation and its enumerator, for floats.
    /// </summary>
    internal sealed class FloatMinMaxAggregationOperator : InlinedAggregationOperator<float, float, float>
    {
        private readonly int m_sign; // The sign (-1 for min, 1 for max).

        //---------------------------------------------------------------------------------------
        // Constructs a new instance of a min/max associative operator.
        //

        internal FloatMinMaxAggregationOperator(IEnumerable<float> child, int sign) : base(child)
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

        protected override float InternalAggregate(ref Exception singularExceptionToThrow)
        {
            // Because the final reduction is typically much cheaper than the intermediate 
            // reductions over the individual partitions, and because each parallel partition
            // will do a lot of work to produce a single output element, we prefer to turn off
            // pipelining, and process the final reductions serially.
            using (IEnumerator<float> enumerator = GetEnumerator(ParallelMergeOptions.FullyBuffered, true))
            {
                // Throw an error for empty results.
                if (!enumerator.MoveNext())
                {
                    singularExceptionToThrow = new InvalidOperationException(SR.GetString(SR.NoElements));
                    return default(float);
                }

                float best = enumerator.Current;

                // Based on the sign, do either a min or max reduction.
                if (m_sign == -1)
                {
                    while (enumerator.MoveNext())
                    {
                        float current = enumerator.Current;
                        if (current < best || float.IsNaN(current))
                        {
                            best = current;
                        }
                    }
                }
                else
                {
                    while (enumerator.MoveNext())
                    {
                        float current = enumerator.Current;
                        if (current > best || float.IsNaN(best))
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

        protected override QueryOperatorEnumerator<float, int> CreateEnumerator<TKey>(
            int index, int count, QueryOperatorEnumerator<float, TKey> source, object sharedData, CancellationToken cancellationToken)
        {
            return new FloatMinMaxAggregationOperatorEnumerator<TKey>(source, index, m_sign, cancellationToken);
        }

        //---------------------------------------------------------------------------------------
        // This enumerator type encapsulates the intermediary aggregation over the underlying
        // (possibly partitioned) data source.
        //

        private class FloatMinMaxAggregationOperatorEnumerator<TKey> : InlinedAggregationOperatorEnumerator<float>
        {
            private QueryOperatorEnumerator<float, TKey> m_source; // The source data.
            private int m_sign; // The sign for comparisons (-1 means min, 1 means max).

            //---------------------------------------------------------------------------------------
            // Instantiates a new aggregation operator.
            //

            internal FloatMinMaxAggregationOperatorEnumerator(QueryOperatorEnumerator<float, TKey> source, int partitionIndex, int sign,
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

            protected override bool MoveNextCore(ref float currentElement)
            {
                // Based on the sign, do either a min or max reduction.
                QueryOperatorEnumerator<float, TKey> source = m_source;
                TKey keyUnused = default(TKey);

                if (source.MoveNext(ref currentElement, ref keyUnused))
                {
                    int i = 0;
                    // We just scroll through the enumerator and find the min or max.
                    if (m_sign == -1)
                    {
                        float elem = default(float);
                        while (source.MoveNext(ref elem, ref keyUnused))
                        {
                            if ((i++ & CancellationState.POLL_INTERVAL) == 0)
                                CancellationState.ThrowIfCanceled(m_cancellationToken);
                            if (elem < currentElement || float.IsNaN(elem))
                            {
                                currentElement = elem;
                            }
                        }
                    }
                    else
                    {
                        float elem = default(float);
                        while (source.MoveNext(ref elem, ref keyUnused))
                        {
                            if ((i++ & CancellationState.POLL_INTERVAL) == 0)
                                CancellationState.ThrowIfCanceled(m_cancellationToken);

                            if (elem > currentElement || float.IsNaN(currentElement))
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
