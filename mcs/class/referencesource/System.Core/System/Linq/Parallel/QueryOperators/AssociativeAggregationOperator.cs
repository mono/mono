// ==++==
//
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
// =+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+
//
// AssociativeAggregationOperator.cs
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
    /// The aggregation operator is a little unique, in that the enumerators it returns
    /// yield intermediate results instead of the final results. That's because there is
    /// one last Aggregate operation that must occur in order to perform the final reduction
    /// over the intermediate streams. In other words, the intermediate enumerators produced
    /// by this operator are never seen by other query operators or consumers directly.
    ///
    /// An aggregation performs parallel prefixing internally. Given a binary operator O,
    /// it will generate intermediate results by folding O across partitions; then it
    /// performs a final reduction by folding O accross the intermediate results. The
    /// analysis engine knows about associativity and commutativity, and will ensure the
    /// style of partitioning inserted into the tree is compatable with the operator.
    ///
    /// For instance, say O is + (meaning it is AC), our input is {1,2,...,8}, and we
    /// use 4 partitions to calculate the aggregation. Sequentially this would look
    /// like this O(O(O(1,2),...),8), in other words ((1+2)+...)+8. The parallel prefix
    /// of this (w/ 4 partitions) instead calculates the intermediate aggregations, i.e.:
    /// t1 = O(1,2), t2 = O(3,4), ... t4 = O(7,8), aka t1 = 1+2, t2 = 3+4, t4 = 7+8.
    /// The final step is to aggregate O over these intermediaries, i.e.
    /// O(O(O(t1,t2),t3),t4), or ((t1+t2)+t3)+t4. This generalizes to any binary operator.
    ///
    /// Beause some aggregations use a different input, intermediate, and output types,
    /// we support an even more generalized aggregation type. In this model, we have
    /// three operators, an intermediate (used for the incremental aggregations), a
    /// final (used for the final summary of intermediate results), and a result selector
    /// (used to perform whatever transformation is needed on the final summary).
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <typeparam name="TIntermediate"></typeparam>
    /// <typeparam name="TOutput"></typeparam>
    internal sealed class AssociativeAggregationOperator<TInput, TIntermediate, TOutput> : UnaryQueryOperator<TInput, TIntermediate>
    {

        private readonly TIntermediate m_seed; // A seed used during aggregation.
        private readonly bool m_seedIsSpecified; // Whether a seed was specified. If not, the first element will be used.
        private readonly bool m_throwIfEmpty; // Whether to throw an exception if the data source is empty.

        // An intermediate reduction function.
        private Func<TIntermediate, TInput, TIntermediate> m_intermediateReduce;

        // A final reduction function.
        private Func<TIntermediate, TIntermediate, TIntermediate> m_finalReduce;

        // The result selector function.
        private Func<TIntermediate, TOutput> m_resultSelector;

        // A function that constructs seed instances
        private Func<TIntermediate> m_seedFactory;

        //---------------------------------------------------------------------------------------
        // Constructs a new instance of an associative operator.
        //
        // Assumptions:
        //     This operator must be associative.
        //

        internal AssociativeAggregationOperator(IEnumerable<TInput> child, TIntermediate seed, Func<TIntermediate> seedFactory, bool seedIsSpecified,
                                                Func<TIntermediate, TInput, TIntermediate> intermediateReduce,
                                                Func<TIntermediate, TIntermediate, TIntermediate> finalReduce,
                                                Func<TIntermediate, TOutput> resultSelector, bool throwIfEmpty, QueryAggregationOptions options)
            :base(child)
        {
            Contract.Assert(child != null, "child data source cannot be null");
            Contract.Assert(intermediateReduce != null, "need an intermediate reduce function");
            Contract.Assert(finalReduce != null, "need a final reduce function");
            Contract.Assert(resultSelector != null, "need a result selector function");
            Contract.Assert(Enum.IsDefined(typeof(QueryAggregationOptions), options), "enum out of valid range");
            Contract.Assert((options & QueryAggregationOptions.Associative) == QueryAggregationOptions.Associative, "expected an associative operator");
            Contract.Assert(typeof(TIntermediate) == typeof(TInput) || seedIsSpecified, "seed must be specified if TIntermediate differs from TInput");

            m_seed = seed;
            m_seedFactory = seedFactory;
            m_seedIsSpecified = seedIsSpecified;
            m_intermediateReduce = intermediateReduce;
            m_finalReduce = finalReduce;
            m_resultSelector = resultSelector;
            m_throwIfEmpty = throwIfEmpty;
        }

        //---------------------------------------------------------------------------------------
        // Executes the entire query tree, and aggregates the intermediate results into the
        // final result based on the binary operators and final reduction.
        //
        // Return Value:
        //     The single result of aggregation.
        //

        internal TOutput Aggregate()
        {
            Contract.Assert(m_finalReduce != null);
            Contract.Assert(m_resultSelector != null);

            TIntermediate accumulator = default(TIntermediate);
            bool hadElements = false;

            // Because the final reduction is typically much cheaper than the intermediate 
            // reductions over the individual partitions, and because each parallel partition
            // will do a lot of work to produce a single output element, we prefer to turn off
            // pipelining, and process the final reductions serially.
            using (IEnumerator<TIntermediate> enumerator = GetEnumerator(ParallelMergeOptions.FullyBuffered, true))
            {
                // We just reduce the elements in each output partition. If the operation is associative,
                // this will yield the correct answer. If not, we should never be calling this routine.
                while (enumerator.MoveNext())
                {
                    if (hadElements)
                    {
                        // Accumulate results by passing the current accumulation and current element to
                        // the reduction operation.
                        try
                        {
                            accumulator = m_finalReduce(accumulator, enumerator.Current);
                        }
                        catch (ThreadAbortException)
                        {
                            // Do not wrap ThreadAbortExceptions
                            throw;
                        }
                        catch (Exception ex)
                        {
                            // We need to wrap all exceptions into an aggregate.
                            throw new AggregateException(ex);
                        }
                    }
                    else
                    {
                        // This is the first element. Just set the accumulator to the first element.
                        accumulator = enumerator.Current;
                        hadElements = true;
                    }
                }

                // If there were no elements, we must throw an exception.
                if (!hadElements)
                {
                    if (m_throwIfEmpty)
                    {
                        throw new InvalidOperationException(SR.GetString(SR.NoElements));
                    }
                    else
                    {
                        accumulator = m_seedFactory == null ? m_seed : m_seedFactory();
                    }
                }
            }

            // Finally, run the selection routine to yield the final element.
            try
            {
                return m_resultSelector(accumulator);
            }
            catch (ThreadAbortException)
            {
                // Do not wrap ThreadAbortExceptions
                throw;
            }
            catch (Exception ex)
            {
                // We need to wrap all exceptions into an aggregate.
                throw new AggregateException(ex);
            }
        }

        //---------------------------------------------------------------------------------------
        // Just opens the current operator, including opening the child and wrapping it with
        // partitions as needed.
        //

        internal override QueryResults<TIntermediate> Open(QuerySettings settings, bool preferStriping)
        {
            // We just open the child operator.
            QueryResults<TInput> childQueryResults = Child.Open(settings, preferStriping);
            return new UnaryQueryOperatorResults(childQueryResults, this, settings, preferStriping);
        }

        internal override void  WrapPartitionedStream<TKey>(
            PartitionedStream<TInput, TKey> inputStream, IPartitionedStreamRecipient<TIntermediate> recipient,
            bool preferStriping, QuerySettings settings)
        {
            int partitionCount = inputStream.PartitionCount;
            PartitionedStream<TIntermediate, int> outputStream = new PartitionedStream<TIntermediate, int>(
                partitionCount, Util.GetDefaultComparer<int>(), OrdinalIndexState.Correct);

            for (int i = 0; i < partitionCount; i++)
            {
                outputStream[i] = new AssociativeAggregationOperatorEnumerator<TKey>(inputStream[i], this, i, settings.CancellationState.MergedCancellationToken);
            }
            
            recipient.Receive(outputStream);
        }

        //---------------------------------------------------------------------------------------
        // Returns an enumerable that represents the query executing sequentially.
        //

        internal override IEnumerable<TIntermediate> AsSequentialQuery(CancellationToken token)
        {
            Contract.Assert(false, "This method should never be called. Associative aggregation can always be parallelized.");
            throw new NotSupportedException();
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
        // This enumerator type encapsulates the intermediary aggregation over the underlying
        // (possibly partitioned) data source.
        //

        private class AssociativeAggregationOperatorEnumerator<TKey> : QueryOperatorEnumerator<TIntermediate, int>
        {
            private readonly QueryOperatorEnumerator<TInput, TKey> m_source; // The source data.
            private readonly AssociativeAggregationOperator<TInput, TIntermediate, TOutput> m_reduceOperator; // The operator.
            private readonly int m_partitionIndex; // The index of this partition.
            private readonly CancellationToken m_cancellationToken;
            private bool m_accumulated; // Whether we've accumulated already. (false-sharing risk, but only written once)
            

            //---------------------------------------------------------------------------------------
            // Instantiates a new aggregation operator.
            //

            internal AssociativeAggregationOperatorEnumerator(QueryOperatorEnumerator<TInput, TKey> source,
                                                              AssociativeAggregationOperator<TInput, TIntermediate, TOutput> reduceOperator, int partitionIndex,
                                                              CancellationToken cancellationToken)
            {
                Contract.Assert(source != null);
                Contract.Assert(reduceOperator != null);

                m_source = source;
                m_reduceOperator = reduceOperator;
                m_partitionIndex = partitionIndex;
                m_cancellationToken = cancellationToken;
            }

            //---------------------------------------------------------------------------------------
            // This API, upon the first time calling it, walks the entire source query tree. It begins
            // with an accumulator value set to the aggregation operator's seed, and always passes
            // the accumulator along with the current element from the data source to the binary
            // intermediary aggregation operator. The return value is kept in the accumulator. At
            // the end, we will have our intermediate result, ready for final aggregation.
            //

            internal override bool MoveNext(ref TIntermediate currentElement, ref int currentKey)
            {
                Contract.Assert(m_reduceOperator != null);
                Contract.Assert(m_reduceOperator.m_intermediateReduce != null, "expected a compiled operator");

                // Only produce a single element.  Return false if MoveNext() was already called before.
                if (m_accumulated)
                {
                    return false;
                }
                m_accumulated = true;

                bool hadNext = false;
                TIntermediate accumulator = default(TIntermediate);

                // Initialize the accumulator.
                if (m_reduceOperator.m_seedIsSpecified)
                {
                    // If the seed is specified, initialize accumulator to the seed value.
                    accumulator = m_reduceOperator.m_seedFactory == null
                                      ? m_reduceOperator.m_seed
                                      : m_reduceOperator.m_seedFactory();
                }
                else
                {
                    // If the seed is not specified, then we take the first element as the seed.
                    // Seed may be unspecified only if TInput is the same as TIntermediate.
                    Contract.Assert(typeof(TInput) == typeof(TIntermediate));

                    TInput acc = default(TInput);
                    TKey accKeyUnused = default(TKey);
                    if (!m_source.MoveNext(ref acc, ref accKeyUnused)) return false;
                    hadNext = true;
                    accumulator = (TIntermediate)((object)acc);
                }

                // Scan through the source and accumulate the result.
                TInput input = default(TInput);
                TKey keyUnused = default(TKey);
                int i = 0;
                while (m_source.MoveNext(ref input, ref keyUnused))
                {
                    if ((i++ & CancellationState.POLL_INTERVAL) == 0)
                        CancellationState.ThrowIfCanceled(m_cancellationToken);
                    hadNext = true;
                    accumulator = m_reduceOperator.m_intermediateReduce(accumulator, input);
                }

                if (hadNext)
                {
                    currentElement = accumulator;
                    currentKey = m_partitionIndex; // A reduction's "index" is just its partition number.
                    return true;
                }

                return false;
            }

            protected override void Dispose(bool disposing)
            {
                Contract.Assert(m_source != null);
                m_source.Dispose();
            }
        }
    }
}
