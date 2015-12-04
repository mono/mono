// ==++==
//
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
// =+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+
//
// RepeatEnumerable.cs
//
// <OWNER>[....]</OWNER>
//
// =-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-

using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace System.Linq.Parallel
{
    /// <summary>
    /// A simple enumerable type that implements the repeat algorithm. It also supports
    /// partitioning of the count space by implementing an interface that PLINQ recognizes.
    /// </summary>
    /// <typeparam name="TResult"></typeparam>
    internal class RepeatEnumerable<TResult> : ParallelQuery<TResult>, IParallelPartitionable<TResult>
    {

        private TResult m_element; // Element value to repeat.
        private int m_count; // Count of element values.

        //-----------------------------------------------------------------------------------
        // Constructs a new repeat enumerable object for the repeat operation.
        //

        internal RepeatEnumerable(TResult element, int count)
            : base(QuerySettings.Empty)
        {
            Contract.Assert(count >= 0, "count not within range (must be >= 0)");
            m_element = element;
            m_count = count;
        }

        //-----------------------------------------------------------------------------------
        // Retrieves 'count' partitions, dividing the total count by the partition count,
        // and having each partition produce a certain number of repeated elements.
        //

        public QueryOperatorEnumerator<TResult, int>[] GetPartitions(int partitionCount)
        {
            // Calculate a stride size.
            int stride = (m_count + partitionCount - 1) / partitionCount;

            // Now generate the actual enumerators. Each produces 'stride' elements, except
            // for the last partition which may produce fewer (if 'm_count' isn't evenly
            // divisible by 'partitionCount').
            QueryOperatorEnumerator<TResult, int>[] partitions = new QueryOperatorEnumerator<TResult, int>[partitionCount];
            for (int i = 0, offset = 0; i < partitionCount; i++, offset += stride)
            {
                if ((offset + stride) > m_count)
                {
                    partitions[i] = new RepeatEnumerator(m_element, offset < m_count ? m_count - offset : 0, offset);
                }
                else
                {
                    partitions[i] = new RepeatEnumerator(m_element, stride, offset);
                }
            }

            return partitions;
        }

        //-----------------------------------------------------------------------------------
        // Basic IEnumerator<T> method implementations.
        //

        public override IEnumerator<TResult> GetEnumerator()
        {
            return new RepeatEnumerator(m_element, m_count, 0).AsClassicEnumerator();
        }

        //-----------------------------------------------------------------------------------
        // The actual enumerator that produces a set of repeated elements.
        //

        class RepeatEnumerator : QueryOperatorEnumerator<TResult, int>
        {

            private readonly TResult m_element; // The element to repeat.
            private readonly int m_count; // The number of times to repeat it.
            private readonly int m_indexOffset; // Our index offset.
            private Shared<int> m_currentIndex; // The number of times we have already repeated it. [allocate in moveNext to avoid false-sharing]

            //-----------------------------------------------------------------------------------
            // Creates a new enumerator.
            //

            internal RepeatEnumerator(TResult element, int count, int indexOffset)
            {
                m_element = element;
                m_count = count;
                m_indexOffset = indexOffset;
            }

            //-----------------------------------------------------------------------------------
            // Basic IEnumerator<T> methods. These produce the repeating sequence..
            //

            internal override bool MoveNext(ref TResult currentElement, ref int currentKey)
            {
                if( m_currentIndex == null)
                    m_currentIndex = new Shared<int>(-1);
                
                if (m_currentIndex.Value < (m_count - 1))
                {
                    ++m_currentIndex.Value;
                    currentElement = m_element;
                    currentKey = m_currentIndex.Value + m_indexOffset;
                    return true;
                }

                return false;
            }

            internal override void Reset()
            {
                m_currentIndex = null;
            }
        }
    }
}
