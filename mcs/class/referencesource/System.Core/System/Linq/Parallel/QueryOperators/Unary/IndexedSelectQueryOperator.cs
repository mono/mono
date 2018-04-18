// ==++==
//
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
// =+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+
//
// IndexedSelectQueryOperator.cs
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
    /// A variant of the Select operator that supplies element index while performing the
    /// projection operation. This requires cooperation with partitioning and merging to
    /// guarantee ordering is preserved.
    ///
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <typeparam name="TOutput"></typeparam>
    internal sealed class IndexedSelectQueryOperator<TInput, TOutput> : UnaryQueryOperator<TInput, TOutput>
    {

        // Selector function. Used to project elements to a transformed view during execution.
        private readonly Func<TInput, int, TOutput> m_selector;
        private bool m_prematureMerge = false; // Whether to prematurely merge the input of this operator.
        private bool m_limitsParallelism = false; // Whether this operator limits parallelism

        //---------------------------------------------------------------------------------------
        // Initializes a new select operator.
        //
        // Arguments:
        //    child         - the child operator or data source from which to pull data
        //    selector      - a delegate representing the selector function
        //
        // Assumptions:
        //    selector must be non null.
        //

        internal IndexedSelectQueryOperator(IEnumerable<TInput> child,
                                            Func<TInput, int, TOutput> selector)
            :base(child)
        {
            Contract.Assert(child != null, "child data source cannot be null");
            Contract.Assert(selector != null, "need a selector function");

            m_selector = selector;

            // In an indexed Select, elements must be returned in the order in which
            // indices were assigned.
            m_outputOrdered = true;

            InitOrdinalIndexState();
        }

        private void InitOrdinalIndexState()
        {
            OrdinalIndexState childIndexState = Child.OrdinalIndexState;
            OrdinalIndexState indexState = childIndexState;

            if (ExchangeUtilities.IsWorseThan(childIndexState, OrdinalIndexState.Correct))
            {
                m_prematureMerge = true;
                m_limitsParallelism = childIndexState != OrdinalIndexState.Shuffled;
                indexState = OrdinalIndexState.Correct;
            }

            Contract.Assert(!ExchangeUtilities.IsWorseThan(indexState, OrdinalIndexState.Correct));

            SetOrdinalIndexState(indexState);
        }

        //---------------------------------------------------------------------------------------
        // Just opens the current operator, including opening the child and wrapping it with
        // partitions as needed.
        //

        internal override QueryResults<TOutput> Open(
            QuerySettings settings, bool preferStriping)
        {
            QueryResults<TInput> childQueryResults = Child.Open(settings, preferStriping);
            return IndexedSelectQueryOperatorResults.NewResults(childQueryResults, this, settings, preferStriping);
        }

        internal override void  WrapPartitionedStream<TKey>(
            PartitionedStream<TInput, TKey> inputStream, IPartitionedStreamRecipient<TOutput> recipient, bool preferStriping, QuerySettings settings)
        {
            int partitionCount = inputStream.PartitionCount;

            // If the index is not correct, we need to reindex.
            PartitionedStream<TInput, int> inputStreamInt;
            if (m_prematureMerge)
            {
                ListQueryResults<TInput> listResults = QueryOperator<TInput>.ExecuteAndCollectResults(
                    inputStream, partitionCount, Child.OutputOrdered, preferStriping, settings);
                inputStreamInt = listResults.GetPartitionedStream();
            }
            else
            {
                Contract.Assert(typeof(TKey) == typeof(int));
                inputStreamInt = (PartitionedStream<TInput, int>)(object)inputStream;
            }

            // Since the index is correct, the type of the index must be int
            PartitionedStream<TOutput, int> outputStream =
                new PartitionedStream<TOutput, int>(partitionCount, Util.GetDefaultComparer<int>(), OrdinalIndexState);

            for (int i = 0; i < partitionCount; i++)
            {
                outputStream[i] = new IndexedSelectQueryOperatorEnumerator(inputStreamInt[i], m_selector);
            }

            recipient.Receive(outputStream);
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
        // The enumerator type responsible for projecting elements as it is walked.
        //

        class IndexedSelectQueryOperatorEnumerator : QueryOperatorEnumerator<TOutput, int>
        {

            private readonly QueryOperatorEnumerator<TInput, int> m_source; // The data source to enumerate.
            private readonly Func<TInput, int, TOutput> m_selector;  // The actual select function.

            //---------------------------------------------------------------------------------------
            // Instantiates a new select enumerator.
            //

            internal IndexedSelectQueryOperatorEnumerator(QueryOperatorEnumerator<TInput, int> source, Func<TInput, int, TOutput> selector)
            {
                Contract.Assert(source != null);
                Contract.Assert(selector != null);
                m_source = source;
                m_selector = selector;
            }

            //---------------------------------------------------------------------------------------
            // Straightforward IEnumerator<T> methods.
            //

            internal override bool MoveNext(ref TOutput currentElement, ref int currentKey)
            {
                // So long as the source has a next element, we have an element.
                TInput element = default(TInput);
                if (m_source.MoveNext(ref element, ref currentKey))
                {
                    Contract.Assert(m_selector != null, "expected a compiled selection function");
                    currentElement = m_selector(element, currentKey);
                    return true;
                }

                return false;
            }

            protected override void Dispose(bool disposing)
            {
                m_source.Dispose();
            }

        }


        //---------------------------------------------------------------------------------------
        // Returns an enumerable that represents the query executing sequentially.
        //

        internal override IEnumerable<TOutput> AsSequentialQuery(CancellationToken token)
        {
            return Child.AsSequentialQuery(token).Select(m_selector);
        }

        //-----------------------------------------------------------------------------------
        // Query results for an indexed Select operator. The results are indexible if the child
        // results were indexible.
        //

        class IndexedSelectQueryOperatorResults : UnaryQueryOperatorResults
        {
            IndexedSelectQueryOperator<TInput, TOutput> m_selectOp;  // Operator that generated the results
            int m_childCount; // The number of elements in child results

            public static QueryResults<TOutput> NewResults(
                QueryResults<TInput> childQueryResults, IndexedSelectQueryOperator<TInput, TOutput> op,
                QuerySettings settings, bool preferStriping)
            {
                if (childQueryResults.IsIndexible)
                {
                    return new IndexedSelectQueryOperatorResults(
                        childQueryResults, op, settings, preferStriping);
                }
                else
                {
                    return new UnaryQueryOperatorResults(
                        childQueryResults, op, settings, preferStriping);
                }
            }

            private IndexedSelectQueryOperatorResults(
                QueryResults<TInput> childQueryResults, IndexedSelectQueryOperator<TInput, TOutput> op, 
                QuerySettings settings, bool preferStriping)
                : base(childQueryResults, op, settings, preferStriping)
            {
                m_selectOp = op;
                Contract.Assert(m_childQueryResults.IsIndexible);

                m_childCount = m_childQueryResults.ElementsCount;
            }

            internal override int ElementsCount
            {
                get 
                {
                    Contract.Assert(m_childCount >= 0);
                    return m_childQueryResults.ElementsCount;
                }
            }

            internal override bool IsIndexible
            {
                get { return true; }
            }

            internal override TOutput GetElement(int index)
            {
                return m_selectOp.m_selector(m_childQueryResults.GetElement(index), index);
            }
        }
    }
}
