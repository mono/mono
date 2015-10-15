// ==++==
//
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
// =+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+
//
// SortQueryOperator.cs
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
    /// The query operator for OrderBy and ThenBy.
    /// </summary>
    /// <typeparam name="TInputOutput"></typeparam>
    /// <typeparam name="TSortKey"></typeparam>
    internal sealed class SortQueryOperator<TInputOutput, TSortKey> :
        UnaryQueryOperator<TInputOutput, TInputOutput>, IOrderedEnumerable<TInputOutput>
    {
        private readonly Func<TInputOutput, TSortKey> m_keySelector; // Key selector used when sorting.
        private readonly IComparer<TSortKey> m_comparer; // Key comparison logic to use during sorting.

        //---------------------------------------------------------------------------------------
        // Instantiates a new sort operator.
        //

        internal SortQueryOperator(IEnumerable<TInputOutput> source, Func<TInputOutput, TSortKey> keySelector,
                                   IComparer<TSortKey> comparer, bool descending)
            : base(source, true)
        {
            Contract.Assert(keySelector != null, "key selector must not be null");

            m_keySelector = keySelector;

            // If a comparer wasn't supplied, we use the default one for the key type.
            if (comparer == null)
            {
                m_comparer = Util.GetDefaultComparer<TSortKey>();
            }
            else
            {
                m_comparer = comparer;
            }

            if (descending)
            {
                m_comparer = new ReverseComparer<TSortKey>(m_comparer);
            }

            SetOrdinalIndexState(OrdinalIndexState.Shuffled);
        }

        //---------------------------------------------------------------------------------------
        // IOrderedEnumerable method for nesting an order by operator inside another.
        //

        IOrderedEnumerable<TInputOutput> IOrderedEnumerable<TInputOutput>.CreateOrderedEnumerable<TKey2>(
            Func<TInputOutput, TKey2> key2Selector, IComparer<TKey2> key2Comparer, bool descending)
        {
            key2Comparer = key2Comparer ?? Util.GetDefaultComparer<TKey2>();

            if (descending)
            {
                key2Comparer = new ReverseComparer<TKey2>(key2Comparer);
            }

            IComparer<Pair<TSortKey, TKey2>> pairComparer = new PairComparer<TSortKey, TKey2>(m_comparer, key2Comparer);
            Func<TInputOutput, Pair<TSortKey, TKey2>> pairKeySelector =
                (TInputOutput elem) => new Pair<TSortKey, TKey2>(m_keySelector(elem), key2Selector(elem));

            return new SortQueryOperator<TInputOutput, Pair<TSortKey, TKey2>>(Child, pairKeySelector, pairComparer, false);
        }

        //---------------------------------------------------------------------------------------
        // Accessor the the key selector.
        //

        internal Func<TInputOutput, TSortKey> KeySelector
        {
            get { return m_keySelector; }
        }

        //---------------------------------------------------------------------------------------
        // Accessor the the key comparer.
        //

        internal IComparer<TSortKey> KeyComparer
        {
            get { return m_comparer; }
        }

        //---------------------------------------------------------------------------------------
        // Opens the current operator. This involves opening the child operator tree, enumerating
        // the results, sorting them, and then returning an enumerator that walks the result.
        //

        internal override QueryResults<TInputOutput> Open(QuerySettings settings, bool preferStriping)
        {
            QueryResults<TInputOutput> childQueryResults = Child.Open(settings, false);
            return new SortQueryOperatorResults<TInputOutput, TSortKey>(childQueryResults, this, settings, preferStriping);
        }


        internal override void WrapPartitionedStream<TKey>(
            PartitionedStream<TInputOutput, TKey> inputStream, IPartitionedStreamRecipient<TInputOutput> recipient, bool preferStriping, QuerySettings settings)
        {
            PartitionedStream<TInputOutput, TSortKey> outputStream =
                new PartitionedStream<TInputOutput, TSortKey>(inputStream.PartitionCount, this.m_comparer, OrdinalIndexState);

            for (int i = 0; i < outputStream.PartitionCount; i++)
            {
                outputStream[i] = new SortQueryOperatorEnumerator<TInputOutput, TKey, TSortKey>(
                    inputStream[i], m_keySelector, m_comparer);
            }

            recipient.Receive<TSortKey>(outputStream);
        }

        //---------------------------------------------------------------------------------------
        // Returns an enumerable that represents the query executing sequentially.
        //

        internal override IEnumerable<TInputOutput> AsSequentialQuery(CancellationToken token)
        {
            IEnumerable<TInputOutput> wrappedChild = CancellableEnumerable.Wrap(Child.AsSequentialQuery(token), token);
            return wrappedChild.OrderBy(m_keySelector, m_comparer);
        }

        //---------------------------------------------------------------------------------------
        // Whether this operator performs a premature merge that would not be performed in
        // a similar sequential operation (i.e., in LINQ to Objects).
        //

        internal override bool LimitsParallelism
        {
            get { return false; }
        }
    }

    internal class SortQueryOperatorResults<TInputOutput, TSortKey> : QueryResults<TInputOutput>
    {
        protected QueryResults<TInputOutput> m_childQueryResults; // Results of the child query
        private SortQueryOperator<TInputOutput, TSortKey> m_op; // Operator that generated these results
        private QuerySettings m_settings; // Settings collected from the query
        private bool m_preferStriping; // If the results are indexible, should we use striping when partitioning them

        internal SortQueryOperatorResults(
            QueryResults<TInputOutput> childQueryResults, SortQueryOperator<TInputOutput, TSortKey> op,
            QuerySettings settings, bool preferStriping)
        {
            m_childQueryResults = childQueryResults;
            m_op = op;
            m_settings = settings;
            m_preferStriping = preferStriping;
        }

        internal override bool IsIndexible
        {
            get { return false; }
        }

        internal override void GivePartitionedStream(IPartitionedStreamRecipient<TInputOutput> recipient)
        {
            m_childQueryResults.GivePartitionedStream(new ChildResultsRecipient(recipient, m_op, m_settings));
        }

        private class ChildResultsRecipient : IPartitionedStreamRecipient<TInputOutput>
        {
            IPartitionedStreamRecipient<TInputOutput> m_outputRecipient;
            SortQueryOperator<TInputOutput, TSortKey> m_op;
            QuerySettings m_settings;

            internal ChildResultsRecipient(IPartitionedStreamRecipient<TInputOutput> outputRecipient, SortQueryOperator<TInputOutput, TSortKey> op, QuerySettings settings)
            {
                m_outputRecipient = outputRecipient;
                m_op = op;
                m_settings = settings;
            }

            public void Receive<TKey>(PartitionedStream<TInputOutput, TKey> childPartitionedStream)
            {
                m_op.WrapPartitionedStream(childPartitionedStream, m_outputRecipient, false, m_settings);
            }
        }
    }

    //---------------------------------------------------------------------------------------
    // This enumerator performs sorting based on a key selection and comparison routine.
    //

    internal class SortQueryOperatorEnumerator<TInputOutput, TKey, TSortKey> : QueryOperatorEnumerator<TInputOutput, TSortKey>
    {
        private readonly QueryOperatorEnumerator<TInputOutput, TKey> m_source; // Data source to sort.
        private readonly Func<TInputOutput, TSortKey> m_keySelector; // Key selector used when sorting.
        private readonly IComparer<TSortKey> m_keyComparer; // Key comparison logic to use during sorting.

        //---------------------------------------------------------------------------------------
        // Instantiates a new sort operator enumerator.
        //

        internal SortQueryOperatorEnumerator(QueryOperatorEnumerator<TInputOutput, TKey> source,
            Func<TInputOutput, TSortKey> keySelector, IComparer<TSortKey> keyComparer)
        {
            Contract.Assert(source != null);
            Contract.Assert(keySelector != null, "need a key comparer");
            Contract.Assert(keyComparer != null, "expected a compiled operator");

            m_source = source;
            m_keySelector = keySelector;
            m_keyComparer = keyComparer;
        }
        //---------------------------------------------------------------------------------------
        // Accessor for the key comparison routine.
        //

        public IComparer<TSortKey> KeyComparer
        {
            get { return m_keyComparer; }
        }

        //---------------------------------------------------------------------------------------
        // Moves to the next element in the sorted output. When called for the first time, the
        // descendents in the sort's child tree are executed entirely, the results accumulated
        // in memory, and the data sorted.
        //

        internal override bool MoveNext(ref TInputOutput currentElement, ref TSortKey currentKey)
        {
            Contract.Assert(m_source != null);

            TKey keyUnused = default(TKey);
            if (!m_source.MoveNext(ref currentElement, ref keyUnused))
            {
                return false;
            }

            currentKey = m_keySelector(currentElement);
            return true;
        }

        protected override void Dispose(bool disposing)
        {
            Contract.Assert(m_source != null);
            m_source.Dispose();
        }
    }
}
