// ==++==
//
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
// =+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+
//
// ListChunk.cs
//
// <OWNER>Microsoft</OWNER>
//
// =-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-

using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace System.Linq.Parallel
{
    /// <summary>
    /// A linked list of array chunks. Allows direct access to its arrays.
    /// </summary>
    /// <typeparam name="TInputOutput">The elements held within.</typeparam>
    internal class ListChunk<TInputOutput> : IEnumerable<TInputOutput>
    {
        internal TInputOutput[] m_chunk;
        private int m_chunkCount;
        private ListChunk<TInputOutput> m_nextChunk;
        private ListChunk<TInputOutput> m_tailChunk;

        /// <summary>
        /// Allocates a new root chunk of a particular size.
        /// </summary>
        internal ListChunk(int size)
        {
            Contract.Assert(size > 0);
            m_chunk = new TInputOutput[size];
            m_chunkCount = 0;
            m_tailChunk = this;
        }

        /// <summary>
        /// Adds an element to this chunk.  Only ever called on the root.
        /// </summary>
        /// <param name="e">The new element.</param>
        internal void Add(TInputOutput e)
        {
            ListChunk<TInputOutput> tail = m_tailChunk;
            if (tail.m_chunkCount == tail.m_chunk.Length)
            {
                m_tailChunk = new ListChunk<TInputOutput>(tail.m_chunkCount * 2);
                tail = (tail.m_nextChunk = m_tailChunk);
            }

            tail.m_chunk[tail.m_chunkCount++] = e;
        }

        /// <summary>
        /// The next chunk in the linked chain.
        /// </summary>
        internal ListChunk<TInputOutput> Next
        {
            get { return m_nextChunk; }
        }

        /// <summary>
        /// The number of elements contained within this particular chunk.
        /// </summary>
        internal int Count
        {
            get { return m_chunkCount; }
        }

        /// <summary>
        /// Fetches an enumerator to walk the elements in all chunks rooted from this one.
        /// </summary>
        public IEnumerator<TInputOutput> GetEnumerator()
        {
            ListChunk<TInputOutput> curr = this;
            while (curr != null)
            {
                for (int i = 0; i < curr.m_chunkCount; i++)
                {
                    yield return curr.m_chunk[i];
                }
                Contract.Assert(curr.m_chunkCount == curr.m_chunk.Length || curr.m_nextChunk == null);
                curr = curr.m_nextChunk;
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<TInputOutput>)this).GetEnumerator();
        }
    }
}
