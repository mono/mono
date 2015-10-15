// ==++==
//
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
// =+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+
//
// WhereQueryOperator.cs
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
    /// The operator type for Where statements. This operator filters out elements that
    /// don't match a filter function (supplied at instantiation time). 
    /// </summary>
    /// <typeparam name="TInputOutput"></typeparam>
    internal sealed class WhereQueryOperator<TInputOutput> : UnaryQueryOperator<TInputOutput, TInputOutput>
    {

        // Predicate function. Used to filter out non-matching elements during execution.
        private Func<TInputOutput, bool> m_predicate;

        //---------------------------------------------------------------------------------------
        // Initializes a new where operator.
        //
        // Arguments:
        //    child         - the child operator or data source from which to pull data
        //    predicate     - a delegate representing the predicate function
        //
        // Assumptions:
        //    predicate must be non null.
        //

        internal WhereQueryOperator(IEnumerable<TInputOutput> child, Func<TInputOutput, bool> predicate)
            : base(child)
        {
            Contract.Assert(child != null, "child data source cannot be null");
            Contract.Assert(predicate != null, "need a filter function");

            SetOrdinalIndexState(
                ExchangeUtilities.Worse(Child.OrdinalIndexState, OrdinalIndexState.Increasing));

            m_predicate = predicate;
        }

        internal override void WrapPartitionedStream<TKey>(
            PartitionedStream<TInputOutput, TKey> inputStream, IPartitionedStreamRecipient<TInputOutput> recipient, bool preferStriping, QuerySettings settings)
        {
            PartitionedStream<TInputOutput, TKey> outputStream = new PartitionedStream<TInputOutput, TKey>(
                inputStream.PartitionCount, inputStream.KeyComparer, OrdinalIndexState);
            for (int i = 0; i < inputStream.PartitionCount; i++)
            {
                outputStream[i] = new WhereQueryOperatorEnumerator<TKey>(inputStream[i], m_predicate, 
                    settings.CancellationState.MergedCancellationToken);
            }

            recipient.Receive(outputStream);
        }

        //---------------------------------------------------------------------------------------
        // Just opens the current operator, including opening the child and wrapping it with
        // partitions as needed.
        //

        internal override QueryResults<TInputOutput> Open(QuerySettings settings, bool preferStriping)
        {
            // We just open the child operator.
            QueryResults<TInputOutput> childQueryResults = Child.Open(settings, preferStriping);

            // And then return the query results
            return new UnaryQueryOperatorResults(childQueryResults, this, settings, preferStriping);
        }

        //---------------------------------------------------------------------------------------
        // Returns an enumerable that represents the query executing sequentially.
        //

        internal override IEnumerable<TInputOutput> AsSequentialQuery(CancellationToken token)
        {
            IEnumerable<TInputOutput> wrappedChild = CancellableEnumerable.Wrap(Child.AsSequentialQuery(token), token);
            return wrappedChild.Where(m_predicate);
        }

        //---------------------------------------------------------------------------------------
        // Whether this operator performs a premature merge that would not be performed in
        // a similar sequential operation (i.e., in LINQ to Objects).
        //

        internal override bool LimitsParallelism
        {
            get { return false; }
        }

        //-----------------------------------------------------------------------------------
        // An enumerator that implements the filtering logic.
        //

        private class WhereQueryOperatorEnumerator<TKey> : QueryOperatorEnumerator<TInputOutput, TKey>
        {

            private readonly QueryOperatorEnumerator<TInputOutput, TKey> m_source; // The data source to enumerate.
            private readonly Func<TInputOutput, bool> m_predicate; // The predicate used for filtering.
            private CancellationToken m_cancellationToken;
            private Shared<int> m_outputLoopCount; 

            //-----------------------------------------------------------------------------------
            // Instantiates a new enumerator.
            //

            internal WhereQueryOperatorEnumerator(QueryOperatorEnumerator<TInputOutput, TKey> source, Func<TInputOutput, bool> predicate,
                CancellationToken cancellationToken)
            {
                Contract.Assert(source != null);
                Contract.Assert(predicate != null);

                m_source = source;
                m_predicate = predicate;
                m_cancellationToken = cancellationToken;
            }

            //-----------------------------------------------------------------------------------
            // Moves to the next matching element in the underlying data stream.
            //

            internal override bool MoveNext(ref TInputOutput currentElement, ref TKey currentKey)
            {
                Contract.Assert(m_predicate != null, "expected a compiled operator");

                // Iterate through the input until we reach the end of the sequence or find
                // an element matching the predicate.

                if (m_outputLoopCount == null)
                    m_outputLoopCount = new Shared<int>(0);
                
                while (m_source.MoveNext(ref currentElement, ref currentKey))
                {
                    if ((m_outputLoopCount.Value++ & CancellationState.POLL_INTERVAL) == 0)
                        CancellationState.ThrowIfCanceled(m_cancellationToken);

                    if (m_predicate(currentElement))
                    {
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
    }
}
