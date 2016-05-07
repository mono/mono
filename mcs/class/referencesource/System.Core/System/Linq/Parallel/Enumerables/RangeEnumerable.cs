// ==++==
//
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
// =+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+
//
// RangeEnumerable.cs
//
// <OWNER>[....]</OWNER>
//
// =-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-

using System.Collections.Generic;

namespace System.Linq.Parallel
{
    /// <summary>
    /// A simple enumerable type that implements the range algorithm. It also supports
    /// partitioning of the indices by implementing an interface that PLINQ recognizes.
    /// </summary>
    internal class RangeEnumerable : ParallelQuery<int>, IParallelPartitionable<int>
    {

        private int m_from; // Lowest index to include.
        private int m_count; // Number of indices to include.

        //-----------------------------------------------------------------------------------
        // Constructs a new range enumerable object for the specified range.
        //

        internal RangeEnumerable(int from, int count)
            :base(QuerySettings.Empty)
        {
            // Transform the from and to indices into low and highs.
            m_from = from;
            m_count = count;
        }

        //-----------------------------------------------------------------------------------
        // Retrieves 'count' partitions, each of which uses a non-overlapping set of indices.
        //

        public QueryOperatorEnumerator<int, int>[] GetPartitions(int partitionCount)
        {
            // Calculate a stride size, avoiding overflow if m_count is large
            int stride = m_count / partitionCount;
            int biggerPartitionCount = m_count % partitionCount;

            // Create individual partitions, carefully avoiding overflow
            int doneCount = 0;
            QueryOperatorEnumerator<int, int>[] partitions = new QueryOperatorEnumerator<int, int>[partitionCount];
            for (int i = 0; i < partitionCount; i++)
            {
                int partitionSize = (i < biggerPartitionCount) ? stride + 1 : stride;
                partitions[i] = new RangeEnumerator(
                    m_from + doneCount, 
                    partitionSize, 
                    doneCount);
                doneCount += partitionSize;
            }

            return partitions;
        }

        //-----------------------------------------------------------------------------------
        // Basic IEnumerator<T> method implementations.
        //

        public override IEnumerator<int> GetEnumerator()
        {
            return new RangeEnumerator(m_from, m_count, 0).AsClassicEnumerator();
        }

        //-----------------------------------------------------------------------------------
        // The actual enumerator that walks over the specified range.
        //

        class RangeEnumerator : QueryOperatorEnumerator<int, int>
        {

            private readonly int m_from; // The initial value.
            private readonly int m_count; // How many values to yield.
            private readonly int m_initialIndex; // The ordinal index of the first value in the range.
            private Shared<int> m_currentCount; // The 0-based index of the current value. [allocate in moveNext to avoid false-sharing]
            
            //-----------------------------------------------------------------------------------
            // Creates a new enumerator.
            //

            internal RangeEnumerator(int from, int count, int initialIndex)
            {
                m_from = from;
                m_count = count;
                m_initialIndex = initialIndex;
            }

            //-----------------------------------------------------------------------------------
            // Basic enumeration method. This implements the logic to walk the desired
            // range, using the step specified at construction time.
            //

            internal override bool MoveNext(ref int currentElement, ref int currentKey)
            {
                if( m_currentCount == null)
                    m_currentCount = new Shared<int>(-1);

                // Calculate the next index and ensure it falls within our range.
                int nextCount = m_currentCount.Value + 1;
                if (nextCount < m_count)
                {
                    m_currentCount.Value = nextCount;
                    currentElement = nextCount + m_from;
                    currentKey = nextCount + m_initialIndex;
                    return true;
                }

                return false;
            }

            internal override void Reset()
            {
                // We set the current value such that the next addition of step
                // results in the 1st real value in the range.
                m_currentCount = null;
            }
        }
    }
}
