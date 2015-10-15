// ==++==
//
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
// =+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+
//
// ZipQueryOperator.cs
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
    /// A Zip operator combines two input data sources into a single output stream,
    /// using a pairwise element matching algorithm. For example, the result of zipping
    /// two vectors a = {0, 1, 2, 3} and b = {9, 8, 7, 6} is the vector of pairs,
    /// c = {(0,9), (1,8), (2,7), (3,6)}. Because the expectation is that each element
    /// is matched with the element in the other data source at the same ordinal
    /// position, the zip operator requires order preservation. 
    /// </summary>
    /// <typeparam name="TLeftInput"></typeparam>
    /// <typeparam name="TRightInput"></typeparam>
    /// <typeparam name="TOutput"></typeparam>
    internal sealed class ZipQueryOperator<TLeftInput, TRightInput, TOutput>
        : QueryOperator<TOutput>
    {

        private readonly Func<TLeftInput, TRightInput, TOutput> m_resultSelector; // To select result elements.
        private readonly QueryOperator<TLeftInput> m_leftChild;
        private readonly QueryOperator<TRightInput> m_rightChild;
        private readonly bool m_prematureMergeLeft = false; // Whether to prematurely merge the left data source
        private readonly bool m_prematureMergeRight = false; // Whether to prematurely merge the right data source
        private readonly bool m_limitsParallelism = false; // Whether this operator limits parallelism

        //---------------------------------------------------------------------------------------
        // Initializes a new zip operator.
        //
        // Arguments:
        //    leftChild     - the left data source from which to pull data.
        //    rightChild    - the right data source from which to pull data.
        //

        internal ZipQueryOperator(
            ParallelQuery<TLeftInput> leftChildSource, IEnumerable<TRightInput> rightChildSource,
            Func<TLeftInput, TRightInput, TOutput> resultSelector)
            :this(
                QueryOperator<TLeftInput>.AsQueryOperator(leftChildSource),
                QueryOperator<TRightInput>.AsQueryOperator(rightChildSource),
                resultSelector)
        {
        }

        private ZipQueryOperator(
            QueryOperator<TLeftInput> left, QueryOperator<TRightInput> right,
            Func<TLeftInput, TRightInput, TOutput> resultSelector)
            : base(left.SpecifiedQuerySettings.Merge(right.SpecifiedQuerySettings))
        {
            Contract.Assert(resultSelector != null, "operator cannot be null");

            m_leftChild = left;
            m_rightChild = right;
            m_resultSelector = resultSelector;
            m_outputOrdered = m_leftChild.OutputOrdered || m_rightChild.OutputOrdered;

            OrdinalIndexState leftIndexState = m_leftChild.OrdinalIndexState;
            OrdinalIndexState rightIndexState = m_rightChild.OrdinalIndexState;

            m_prematureMergeLeft = leftIndexState != OrdinalIndexState.Indexible;
            m_prematureMergeRight = rightIndexState != OrdinalIndexState.Indexible;
            m_limitsParallelism =
                (m_prematureMergeLeft && leftIndexState != OrdinalIndexState.Shuffled)
                || (m_prematureMergeRight && rightIndexState != OrdinalIndexState.Shuffled);
        }

        //---------------------------------------------------------------------------------------
        // Just opens the current operator, including opening the children and wrapping them with
        // partitions as needed.
        //

        internal override QueryResults<TOutput> Open(QuerySettings settings, bool preferStriping)
        {
            // We just open our child operators, left and then right.
            QueryResults<TLeftInput> leftChildResults = m_leftChild.Open(settings, preferStriping);
            QueryResults<TRightInput> rightChildResults = m_rightChild.Open(settings, preferStriping);

            int partitionCount = settings.DegreeOfParallelism.Value;
            if (m_prematureMergeLeft)
            {
                PartitionedStreamMerger<TLeftInput> merger = new PartitionedStreamMerger<TLeftInput>(
                    false, ParallelMergeOptions.FullyBuffered, settings.TaskScheduler, m_leftChild.OutputOrdered,
                    settings.CancellationState, settings.QueryId);
                leftChildResults.GivePartitionedStream(merger);
                leftChildResults = new ListQueryResults<TLeftInput>(
                    merger.MergeExecutor.GetResultsAsArray(), partitionCount, preferStriping);
            }

            if (m_prematureMergeRight)
            {
                PartitionedStreamMerger<TRightInput> merger = new PartitionedStreamMerger<TRightInput>(
                    false, ParallelMergeOptions.FullyBuffered, settings.TaskScheduler, m_rightChild.OutputOrdered,
                    settings.CancellationState, settings.QueryId);
                rightChildResults.GivePartitionedStream(merger);
                rightChildResults = new ListQueryResults<TRightInput>(
                    merger.MergeExecutor.GetResultsAsArray(), partitionCount, preferStriping);
            }

            return new ZipQueryOperatorResults(leftChildResults, rightChildResults, m_resultSelector, partitionCount, preferStriping);
        }

        //---------------------------------------------------------------------------------------
        // Returns an enumerable that represents the query executing sequentially.
        //

        internal override IEnumerable<TOutput> AsSequentialQuery(CancellationToken token)
        {
            using(IEnumerator<TLeftInput> leftEnumerator = m_leftChild.AsSequentialQuery(token).GetEnumerator())
            using(IEnumerator<TRightInput> rightEnumerator = m_rightChild.AsSequentialQuery(token).GetEnumerator())
            {
                while(leftEnumerator.MoveNext() && rightEnumerator.MoveNext())
                {
                    yield return m_resultSelector(leftEnumerator.Current, rightEnumerator.Current);
                }
            }
        }

        //---------------------------------------------------------------------------------------
        // The state of the order index of the results returned by this operator.
        //

        internal override OrdinalIndexState OrdinalIndexState
        {
            get
            {
                return OrdinalIndexState.Indexible;
            }
        }

        //---------------------------------------------------------------------------------------
        // Whether this operator performs a premature merge that would not be performed in
        // a similar sequential operation (i.e., in LINQ to Objects).
        //
       
        internal override bool LimitsParallelism
        {
            get
            {
                return m_limitsParallelism;
            }
        }

        //---------------------------------------------------------------------------------------
        // A special QueryResults class for the Zip operator. It requires that both of the child
        // QueryResults are indexible.
        //

        internal class ZipQueryOperatorResults : QueryResults<TOutput>
        {
            private readonly QueryResults<TLeftInput> m_leftChildResults;
            private readonly QueryResults<TRightInput> m_rightChildResults;
            private readonly Func<TLeftInput, TRightInput, TOutput> m_resultSelector; // To select result elements.
            private readonly int m_count;
            private readonly int m_partitionCount;
            private readonly bool m_preferStriping;

            internal ZipQueryOperatorResults(
                QueryResults<TLeftInput> leftChildResults, QueryResults<TRightInput> rightChildResults,
                Func<TLeftInput, TRightInput, TOutput> resultSelector, int partitionCount, bool preferStriping)
            {
                m_leftChildResults = leftChildResults;
                m_rightChildResults = rightChildResults;
                m_resultSelector = resultSelector;
                m_partitionCount = partitionCount;
                m_preferStriping = preferStriping;

                Contract.Assert(m_leftChildResults.IsIndexible);
                Contract.Assert(m_rightChildResults.IsIndexible);

                m_count = Math.Min(m_leftChildResults.Count, m_rightChildResults.Count);
            }

            internal override int ElementsCount
            {
                get { return m_count; }
            }

            internal override bool IsIndexible
            {
                get { return true; }
            }

            internal override TOutput GetElement(int index)
            {
                return m_resultSelector(m_leftChildResults.GetElement(index), m_rightChildResults.GetElement(index));
            }

            internal override void GivePartitionedStream(IPartitionedStreamRecipient<TOutput> recipient)
            {
                PartitionedStream<TOutput, int> partitionedStream = ExchangeUtilities.PartitionDataSource(this, m_partitionCount, m_preferStriping);
                recipient.Receive(partitionedStream);
            }
        }
    }
}
