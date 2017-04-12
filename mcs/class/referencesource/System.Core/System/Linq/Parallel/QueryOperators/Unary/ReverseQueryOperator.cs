// ==++==
//
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
// =+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+
//
// ReverseQueryOperator.cs
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
    /// Reverse imposes ordinal order preservation. There are normally two phases to this
    /// operator's execution.  Each partition first builds a buffer containing all of its
    /// elements, and then proceeds to yielding the elements in reverse.  There is a
    /// 'barrier' (but not a blocking barrier) in between these two steps, at which point the largest index becomes
    /// known.  This is necessary so that when elements from the buffer are yielded, the
    /// CurrentIndex can be reported as the largest index minus the original index (thereby
    /// reversing the indices as well as the elements themselves).  If the largest index is
    /// known a priori, because we have an array for example, we can avoid the barrier in
    /// between the steps.
    /// </summary>
    /// <typeparam name="TSource"></typeparam>
    internal sealed class ReverseQueryOperator<TSource> : UnaryQueryOperator<TSource, TSource>
    {

        //---------------------------------------------------------------------------------------
        // Initializes a new reverse operator.
        //
        // Arguments:
        //     child                - the child whose data we will reverse
        //

        internal ReverseQueryOperator(IEnumerable<TSource> child)
            :base(child)
        {
            Contract.Assert(child != null, "child data source cannot be null");

            if (Child.OrdinalIndexState == OrdinalIndexState.Indexible)
            {
                SetOrdinalIndexState(OrdinalIndexState.Indexible);
            }
            else
            {
                SetOrdinalIndexState(OrdinalIndexState.Shuffled);
            }

        }

        internal override void WrapPartitionedStream<TKey>(
            PartitionedStream<TSource, TKey> inputStream, IPartitionedStreamRecipient<TSource> recipient, bool preferStriping, QuerySettings settings)
        {
            Contract.Assert(Child.OrdinalIndexState != OrdinalIndexState.Indexible, "Don't take this code path if the child is indexible.");

            int partitionCount = inputStream.PartitionCount;
            PartitionedStream<TSource, TKey> outputStream = new PartitionedStream<TSource, TKey>(
                partitionCount, new ReverseComparer<TKey>(inputStream.KeyComparer), OrdinalIndexState.Shuffled);

            for (int i = 0; i < partitionCount; i++)
            {
                outputStream[i] = new ReverseQueryOperatorEnumerator<TKey>(inputStream[i], settings.CancellationState.MergedCancellationToken);
            }
            recipient.Receive(outputStream);
        }

        //---------------------------------------------------------------------------------------
        // Just opens the current operator, including opening the child and wrapping it with
        // partitions as needed.
        //

        internal override QueryResults<TSource> Open(QuerySettings settings, bool preferStriping)
        {
            QueryResults<TSource> childQueryResults = Child.Open(settings, false);
            return ReverseQueryOperatorResults.NewResults(childQueryResults, this, settings, preferStriping);
        }

        //---------------------------------------------------------------------------------------
        // Returns an enumerable that represents the query executing sequentially.
        //

        internal override IEnumerable<TSource> AsSequentialQuery(CancellationToken token)
        {
            IEnumerable<TSource> wrappedChild = CancellableEnumerable.Wrap(Child.AsSequentialQuery(token), token);
            return wrappedChild.Reverse();
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
        // The enumerator type responsible for executing the reverse operation.
        //
        
        class ReverseQueryOperatorEnumerator<TKey> : QueryOperatorEnumerator<TSource, TKey>
        {

            private readonly QueryOperatorEnumerator<TSource, TKey> m_source; // The data source to reverse.
            private readonly CancellationToken m_cancellationToken;
            private List<Pair<TSource, TKey>> m_buffer; // Our buffer. [allocate in moveNext to avoid false-sharing]
            private Shared<int> m_bufferIndex; // Our current index within the buffer. [allocate in moveNext to avoid false-sharing]

            //---------------------------------------------------------------------------------------
            // Instantiates a new select enumerator.
            //

            internal ReverseQueryOperatorEnumerator(QueryOperatorEnumerator<TSource, TKey> source,
                CancellationToken cancellationToken)
            {
                Contract.Assert(source != null);
                m_source = source;
                m_cancellationToken = cancellationToken;
            }

            //---------------------------------------------------------------------------------------
            // Straightforward IEnumerator<T> methods.
            //

            internal override bool MoveNext(ref TSource currentElement, ref TKey currentKey)
            {
                // If the buffer has not been created, we will generate it lazily on demand.
                if (m_buffer == null)
                {
                    m_bufferIndex = new Shared<int>(0);
                    // Buffer all of our data.
                    m_buffer = new List<Pair<TSource, TKey>>();
                    TSource current = default(TSource);
                    TKey key = default(TKey);
                    int i = 0;
                    while (m_source.MoveNext(ref current, ref key))
                    {
                        if ((i++ & CancellationState.POLL_INTERVAL) == 0)
                            CancellationState.ThrowIfCanceled(m_cancellationToken);

                        m_buffer.Add(new Pair<TSource, TKey>(current, key));
                        m_bufferIndex.Value++;
                    }
                }

                // Continue yielding elements from our buffer.
                if (--m_bufferIndex.Value >= 0)
                {
                    currentElement = m_buffer[m_bufferIndex.Value].First;
                    currentKey = m_buffer[m_bufferIndex.Value].Second;
                    return true;
                }

                return false;
            }

            protected override void Dispose(bool disposing)
            {
                m_source.Dispose();
            }
        }

        //-----------------------------------------------------------------------------------
        // Query results for a Reverse operator. The results are indexible if the child
        // results were indexible.
        //

        class ReverseQueryOperatorResults : UnaryQueryOperatorResults
        {
            private int m_count; // The number of elements in child results

            public static QueryResults<TSource> NewResults(
                QueryResults<TSource> childQueryResults, ReverseQueryOperator<TSource> op,
                QuerySettings settings, bool preferStriping)
            {
                if (childQueryResults.IsIndexible)
                {
                    return new ReverseQueryOperatorResults(
                        childQueryResults, op, settings, preferStriping);
                }
                else
                {
                    return new UnaryQueryOperatorResults(
                        childQueryResults, op, settings, preferStriping);
                }
            }

            private ReverseQueryOperatorResults(
                QueryResults<TSource> childQueryResults, ReverseQueryOperator<TSource> op,
                QuerySettings settings, bool preferStriping)
                : base(childQueryResults, op, settings, preferStriping)
            {
                Contract.Assert(m_childQueryResults.IsIndexible);
                m_count = m_childQueryResults.ElementsCount;
            }

            internal override bool IsIndexible
            {
                get { return true; }
            }

            internal override int ElementsCount
            {
                get
                {
                    Contract.Assert(m_count >= 0);
                    return m_count;
                }
            }

            internal override TSource GetElement(int index)
            {
                Contract.Assert(m_count >= 0);
                Contract.Assert(index >= 0);
                Contract.Assert(index < m_count);

                return m_childQueryResults.GetElement(m_count - index - 1);
            }
        }

    }
}
