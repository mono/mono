// ==++==
//
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
// =+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+
//
// PartitionedStream.cs
//
// <OWNER>Microsoft</OWNER>
//
// =-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-

using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace System.Linq.Parallel
{
    /// <summary>
    /// A partitioned stream just partitions some data source using an extensible 
    /// partitioning algorithm and exposes a set of N enumerators that are consumed by
    /// their ordinal index [0..N). It is used to build up a set of streaming computations.
    /// At instantiation time, the actual data source to be partitioned is supplied; and
    /// then the caller will layer on top additional enumerators to represent phases in the
    /// computation. Eventually, a merge can then schedule enumeration of all of the
    /// individual partitions in parallel by obtaining references to the individual
    /// partition streams.
    ///
    /// This type has a set of subclasses which implement different partitioning algorithms,
    /// allowing us to easily plug in different partitioning techniques as needed. The type
    /// supports wrapping IEnumerables and IEnumerators alike, with some preference for the
    /// former as many partitioning algorithms are more intelligent for certain data types.
    /// </summary>
    /// <typeparam name="TElement"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    internal class PartitionedStream<TElement, TKey>
    {
        protected QueryOperatorEnumerator<TElement, TKey>[] m_partitions; // Partitions exposed by this object.
        private readonly IComparer<TKey> m_keyComparer; // Comparer for order keys.
        private readonly OrdinalIndexState m_indexState; // State of the order keys.

        internal PartitionedStream(int partitionCount, IComparer<TKey> keyComparer, OrdinalIndexState indexState)
        {
            Contract.Assert(partitionCount > 0);
            m_partitions = new QueryOperatorEnumerator<TElement, TKey>[partitionCount];
            m_keyComparer = keyComparer;
            m_indexState = indexState;
        }

        //---------------------------------------------------------------------------------------
        // Retrieves or sets a partition for the given index.
        //
        // Assumptions:
        //    The index falls within the legal range of the enumerator, i.e. 0 <= value < count.
        //

        internal QueryOperatorEnumerator<TElement, TKey> this[int index]
        {
            get
            {
                Contract.Assert(m_partitions != null);
                Contract.Assert(0 <= index && index < m_partitions.Length, "index out of bounds");
                return m_partitions[index];
            }
            set
            {
                Contract.Assert(m_partitions != null);
                Contract.Assert(value != null);
                Contract.Assert(0 <= index && index < m_partitions.Length, "index out of bounds");                
                m_partitions[index] = value;
            }
        }

        //---------------------------------------------------------------------------------------
        // Retrives the number of partitions.
        //

        public int PartitionCount
        {
            get
            {
                Contract.Assert(m_partitions != null);
                return m_partitions.Length;
            }
        }

        //---------------------------------------------------------------------------------------
        // The comparer for the order keys.
        //

        internal IComparer<TKey> KeyComparer
        {
            get { return m_keyComparer; }
        }

        //---------------------------------------------------------------------------------------
        // State of the order keys.
        //

        internal OrdinalIndexState OrdinalIndexState
        {
            get { return m_indexState; }
        }
    }
}
