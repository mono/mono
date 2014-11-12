// ==++==
//
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
// =+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+
//
// ConcatQueryOperator.cs
//
// <OWNER>[....]</OWNER>
//
// =-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Threading;

namespace System.Linq.Parallel
{
    /// <summary>
    /// Concatenates one data source with another.  Order preservation is used to ensure
    /// the output is actually a concatenation -- i.e. one after the other.  The only
    /// special synchronization required is to find the largest index N in the first data
    /// source so that the indices of elements in the second data source can be offset
    /// by adding N+1.  This makes it appear to the order preservation infrastructure as
    /// though all elements in the second came after all elements in the first, which is
    /// precisely what we want.
    /// </summary>
    /// <typeparam name="TSource"></typeparam>
    internal sealed class ConcatQueryOperator<TSource> : BinaryQueryOperator<TSource, TSource, TSource>
    {

        private readonly bool m_prematureMergeLeft = false; // Whether to prematurely merge the left data source
        private readonly bool m_prematureMergeRight = false; // Whether to prematurely merge the right data source

        //---------------------------------------------------------------------------------------
        // Initializes a new concatenation operator.
        //
        // Arguments:
        //     child                - the child whose data we will reverse
        //

        internal ConcatQueryOperator(ParallelQuery<TSource> firstChild, ParallelQuery<TSource> secondChild)
            : base(firstChild, secondChild)
        {
            Contract.Assert(firstChild != null, "first child data source cannot be null");
            Contract.Assert(secondChild != null, "second child data source cannot be null");
            m_outputOrdered = LeftChild.OutputOrdered || RightChild.OutputOrdered;

            m_prematureMergeLeft = LeftChild.OrdinalIndexState.IsWorseThan(OrdinalIndexState.Increasing);
            m_prematureMergeRight = RightChild.OrdinalIndexState.IsWorseThan(OrdinalIndexState.Increasing);

            if ((LeftChild.OrdinalIndexState == OrdinalIndexState.Indexible)
                && (RightChild.OrdinalIndexState == OrdinalIndexState.Indexible))
            {
                SetOrdinalIndex(OrdinalIndexState.Indexible);
            }
            else
            {
                SetOrdinalIndex(
                    ExchangeUtilities.Worse(OrdinalIndexState.Increasing, 
                        ExchangeUtilities.Worse(LeftChild.OrdinalIndexState, RightChild.OrdinalIndexState)));
            }
        }

        //---------------------------------------------------------------------------------------
        // Just opens the current operator, including opening the child and wrapping it with
        // partitions as needed.
        //

        internal override QueryResults<TSource> Open(QuerySettings settings, bool preferStriping)
        {
            // We just open the children operators.
            QueryResults<TSource> leftChildResults = LeftChild.Open(settings, preferStriping);
            QueryResults<TSource> rightChildResults = RightChild.Open(settings, preferStriping);

            return ConcatQueryOperatorResults.NewResults(leftChildResults, rightChildResults, this, settings, preferStriping);
        }

        public override void WrapPartitionedStream<TLeftKey, TRightKey>(
            PartitionedStream<TSource, TLeftKey> leftStream, PartitionedStream<TSource, TRightKey> rightStream,
            IPartitionedStreamRecipient<TSource> outputRecipient, bool preferStriping, QuerySettings settings)
        {
            // Prematurely merge the left results, if necessary
            if (m_prematureMergeLeft)
            {
                ListQueryResults<TSource> leftStreamResults = 
                    ExecuteAndCollectResults(leftStream, leftStream.PartitionCount, LeftChild.OutputOrdered, preferStriping, settings);
                PartitionedStream<TSource, int> leftStreamInc =  leftStreamResults.GetPartitionedStream();
                WrapHelper<int, TRightKey>(leftStreamInc, rightStream, outputRecipient, settings, preferStriping);
            }
            else
            {
                Contract.Assert(!ExchangeUtilities.IsWorseThan(leftStream.OrdinalIndexState, OrdinalIndexState.Increasing));
                WrapHelper<TLeftKey, TRightKey>(leftStream, rightStream, outputRecipient, settings, preferStriping);
            }
        }

        private void WrapHelper<TLeftKey,TRightKey>(
            PartitionedStream<TSource, TLeftKey> leftStreamInc, PartitionedStream<TSource, TRightKey> rightStream, 
            IPartitionedStreamRecipient<TSource> outputRecipient, QuerySettings settings, bool preferStriping)
        {
            // Prematurely merge the right results, if necessary
            if (m_prematureMergeRight)
            {
                ListQueryResults<TSource> rightStreamResults =
                    ExecuteAndCollectResults(rightStream, leftStreamInc.PartitionCount, LeftChild.OutputOrdered, preferStriping, settings);
                PartitionedStream<TSource, int> rightStreamInc = rightStreamResults.GetPartitionedStream();
                WrapHelper2<TLeftKey, int>(leftStreamInc, rightStreamInc, outputRecipient);
            }
            else
            {
                Contract.Assert(!ExchangeUtilities.IsWorseThan(rightStream.OrdinalIndexState, OrdinalIndexState.Increasing));
                WrapHelper2<TLeftKey, TRightKey>(leftStreamInc, rightStream, outputRecipient);
            }
        }

        private void WrapHelper2<TLeftKey, TRightKey>(
            PartitionedStream<TSource, TLeftKey> leftStreamInc, PartitionedStream<TSource, TRightKey> rightStreamInc,
            IPartitionedStreamRecipient<TSource> outputRecipient)
        {
            int partitionCount = leftStreamInc.PartitionCount;

            // Generate the shared data.
            IComparer<ConcatKey<TLeftKey, TRightKey>> comparer = ConcatKey<TLeftKey, TRightKey>.MakeComparer(
                leftStreamInc.KeyComparer, rightStreamInc.KeyComparer);
            var outputStream = new PartitionedStream<TSource, ConcatKey<TLeftKey, TRightKey>>(partitionCount, comparer, OrdinalIndexState);

            for (int i = 0; i < partitionCount; i++)
            {
                outputStream[i] = new ConcatQueryOperatorEnumerator<TLeftKey, TRightKey>(leftStreamInc[i], rightStreamInc[i]);
            }

            outputRecipient.Receive(outputStream);
        }


        //---------------------------------------------------------------------------------------
        // Returns an enumerable that represents the query executing sequentially.
        //

        internal override IEnumerable<TSource> AsSequentialQuery(CancellationToken token)
        {
            return LeftChild.AsSequentialQuery(token).Concat(RightChild.AsSequentialQuery(token));
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
        // The enumerator type responsible for concatenating two data sources.
        //

        class ConcatQueryOperatorEnumerator<TLeftKey, TRightKey> : QueryOperatorEnumerator<TSource, ConcatKey<TLeftKey, TRightKey>>
        {

            private QueryOperatorEnumerator<TSource, TLeftKey> m_firstSource; // The first data source to enumerate.
            private QueryOperatorEnumerator<TSource, TRightKey> m_secondSource; // The second data source to enumerate.
            private bool m_begunSecond; // Whether this partition has begun enumerating the second source yet.

            //---------------------------------------------------------------------------------------
            // Instantiates a new select enumerator.
            //

            internal ConcatQueryOperatorEnumerator(
                QueryOperatorEnumerator<TSource, TLeftKey> firstSource,
                QueryOperatorEnumerator<TSource, TRightKey> secondSource)
            {
                Contract.Assert(firstSource != null);
                Contract.Assert(secondSource != null);

                m_firstSource = firstSource;
                m_secondSource = secondSource;
            }

            //---------------------------------------------------------------------------------------
            // MoveNext advances to the next element in the output.  While the first data source has
            // elements, this consists of just advancing through it.  After this, all partitions must
            // synchronize at a barrier and publish the maximum index N.  Finally, all partitions can
            // move on to the second data source, adding N+1 to indices in order to get the correct
            // index offset.
            //

            internal override bool MoveNext(ref TSource currentElement, ref ConcatKey<TLeftKey, TRightKey> currentKey)
            {
                Contract.Assert(m_firstSource != null);
                Contract.Assert(m_secondSource != null);

                // If we are still enumerating the first source, fetch the next item.
                if (!m_begunSecond)
                {
                    // If elements remain, just return true and continue enumerating the left.
                    TLeftKey leftKey = default(TLeftKey);
                    if (m_firstSource.MoveNext(ref currentElement, ref leftKey))
                    {
                        currentKey = ConcatKey<TLeftKey, TRightKey>.MakeLeft(leftKey);
                        return true;
                    }
                    m_begunSecond = true;
                }

                // Now either move on to, or continue, enumerating the right data source.
                TRightKey rightKey = default(TRightKey);
                if (m_secondSource.MoveNext(ref currentElement, ref rightKey))
                {
                    currentKey = ConcatKey<TLeftKey, TRightKey>.MakeRight(rightKey);
                    return true;
                }

                return false;
            }

            protected override void Dispose(bool disposing)
            {
                m_firstSource.Dispose();
                m_secondSource.Dispose();
            }
        }


        //-----------------------------------------------------------------------------------
        // Query results for a Concat operator. The results are indexible if the child
        // results were indexible.
        //

        class ConcatQueryOperatorResults : BinaryQueryOperatorResults
        {
            ConcatQueryOperator<TSource> m_concatOp; // Operator that generated the results
            int m_leftChildCount; // The number of elements in the left child result set
            int m_rightChildCount; // The number of elements in the right child result set

            public static QueryResults<TSource> NewResults(
                QueryResults<TSource> leftChildQueryResults, QueryResults<TSource> rightChildQueryResults,
                ConcatQueryOperator<TSource> op, QuerySettings settings,
                bool preferStriping)
            {
                if (leftChildQueryResults.IsIndexible && rightChildQueryResults.IsIndexible)
                {
                    return new ConcatQueryOperatorResults(
                        leftChildQueryResults, rightChildQueryResults, op, settings, preferStriping);
                }
                else
                {
                    return new BinaryQueryOperatorResults(
                        leftChildQueryResults, rightChildQueryResults, op, settings, preferStriping);
                }
            }

            private ConcatQueryOperatorResults(
                QueryResults<TSource> leftChildQueryResults, QueryResults<TSource> rightChildQueryResults,
                ConcatQueryOperator<TSource> concatOp, QuerySettings settings,
                bool preferStriping)
                : base(leftChildQueryResults, rightChildQueryResults, concatOp, settings, preferStriping)
            {
                m_concatOp = concatOp;
                Contract.Assert(leftChildQueryResults.IsIndexible && rightChildQueryResults.IsIndexible);

                m_leftChildCount = leftChildQueryResults.ElementsCount;
                m_rightChildCount = rightChildQueryResults.ElementsCount;
            }

            internal override bool IsIndexible
            {
                get { return true; }
            }

            internal override int ElementsCount
            {
                get
                {
                    Contract.Assert(m_leftChildCount >= 0 && m_rightChildCount >= 0);
                    return m_leftChildCount + m_rightChildCount;
                }
            }

            internal override TSource GetElement(int index)
            {
                if (index < m_leftChildCount)
                {
                    return m_leftChildQueryResults.GetElement(index);
                }
                else
                {
                    return m_rightChildQueryResults.GetElement(index - m_leftChildCount);
                }
            }
        }

    }

    //---------------------------------------------------------------------------------------
    // ConcatKey represents an ordering key for the Concat operator. It knows whether the
    // element it is associated with is from the left source or the right source, and also
    // the elements ordering key.
    //

    internal struct ConcatKey<TLeftKey, TRightKey>
    {
        private readonly TLeftKey m_leftKey;
        private readonly TRightKey m_rightKey;
        private readonly bool m_isLeft;

        private ConcatKey(TLeftKey leftKey, TRightKey rightKey, bool isLeft)
        {
            m_leftKey = leftKey;
            m_rightKey = rightKey;
            m_isLeft = isLeft;
        }

        internal static ConcatKey<TLeftKey, TRightKey> MakeLeft(TLeftKey leftKey)
        {
            return new ConcatKey<TLeftKey, TRightKey>(leftKey, default(TRightKey), true);
        }

        internal static ConcatKey<TLeftKey, TRightKey> MakeRight(TRightKey rightKey)
        {
            return new ConcatKey<TLeftKey, TRightKey>(default(TLeftKey), rightKey, false);
        }

        internal static IComparer<ConcatKey<TLeftKey, TRightKey>> MakeComparer(
            IComparer<TLeftKey> leftComparer, IComparer<TRightKey> rightComparer)
        {
            return new ConcatKeyComparer(leftComparer, rightComparer);
        }

        //---------------------------------------------------------------------------------------
        // ConcatKeyComparer compares ConcatKeys, so that elements from the left source come
        // before elements from the right source, and elements within each source are ordered
        // according to the corresponding order key.
        //

        private class ConcatKeyComparer : IComparer<ConcatKey<TLeftKey, TRightKey>>
        {
            private IComparer<TLeftKey> m_leftComparer;
            private IComparer<TRightKey> m_rightComparer;

            internal ConcatKeyComparer(IComparer<TLeftKey> leftComparer, IComparer<TRightKey> rightComparer)
            {
                m_leftComparer = leftComparer;
                m_rightComparer = rightComparer;
            }

            public int Compare(ConcatKey<TLeftKey, TRightKey> x, ConcatKey<TLeftKey, TRightKey> y)
            {
                // If one element is from the left source and the other not, the element from the left source
                // comes earlier.
                if (x.m_isLeft != y.m_isLeft)
                {
                    return x.m_isLeft ? -1 : 1;
                }

                // Elements are from the same source (left or right). Compare the corresponding keys.
                if (x.m_isLeft)
                {
                    return m_leftComparer.Compare(x.m_leftKey, y.m_leftKey);
                }
                return m_rightComparer.Compare(x.m_rightKey, y.m_rightKey);
            }
        }
    }
}
