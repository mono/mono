// ==++==
//
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
// =+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+
//
// FixedMaxHeap.cs
//
// <OWNER>Microsoft</OWNER>
//
// =-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-

using System.Collections.Generic;
using System.Diagnostics.Contracts;
#if SILVERLIGHT
using System.Core; // for System.Core.SR
#endif

namespace System.Linq.Parallel
{
    /// <summary>
    /// Very simple heap data structure, of fixed size.
    /// </summary>
    /// <typeparam name="TElement"></typeparam>
    internal class FixedMaxHeap<TElement>
    {

        private TElement[] m_elements; // Element array.
        private int m_count; // Current count.
        private IComparer<TElement> m_comparer; // Element comparison routine.

        //-----------------------------------------------------------------------------------
        // Create a new heap with the specified maximum size.
        //

        internal FixedMaxHeap(int maximumSize)
            : this(maximumSize, Util.GetDefaultComparer<TElement>())
        {
        }

        internal FixedMaxHeap(int maximumSize, IComparer<TElement> comparer)
        {
            Contract.Assert(comparer != null);

            m_elements = new TElement[maximumSize];
            m_comparer = comparer;
        }

        //-----------------------------------------------------------------------------------
        // Retrieve the count (i.e. how many elements are in the heap).
        //

        internal int Count
        {
            get { return m_count; }
        }

        //-----------------------------------------------------------------------------------
        // Retrieve the size (i.e. the maximum size of the heap).
        //

        internal int Size
        {
            get { return m_elements.Length; }
        }

        //-----------------------------------------------------------------------------------
        // Get the current maximum value in the max-heap.
        //
        // Note: The heap stores the maximumSize smallest elements that were inserted.
        // So, if the heap is full, the value returned is the maximumSize-th smallest
        // element that was inserted into the heap.
        //

        internal TElement MaxValue
        {
            get
            {
                if (m_count == 0)
                {
                    throw new InvalidOperationException(SR.GetString(SR.NoElements));
                }

                // The maximum element is in the 0th position.
                return m_elements[0];
            }
        }


        //-----------------------------------------------------------------------------------
        // Removes all elements from the heap.
        //

        internal void Clear()
        {
            m_count = 0;
        }

        //-----------------------------------------------------------------------------------
        // Inserts the new element, maintaining the heap property.
        //
        // Return Value:
        //     If the element is greater than the current max element, this function returns
        //     false without modifying the heap. Otherwise, it returns true.
        //

        internal bool Insert(TElement e)
        {
            if (m_count < m_elements.Length)
            {
                // There is room. We can add it and then max-heapify.
                m_elements[m_count] = e;
                m_count++;
                HeapifyLastLeaf();
                return true;
            }
            else
            {
                // No more room. The element might not even fit in the heap. The check
                // is simple: if it's greater than the maximum element, then it can't be
                // inserted. Otherwise, we replace the head with it and reheapify.
                if (m_comparer.Compare(e, m_elements[0]) < 0)
                {
                    m_elements[0] = e;
                    HeapifyRoot();
                    return true;
                }

                return false;
            }
        }

        //-----------------------------------------------------------------------------------
        // Replaces the maximum value in the heap with the user-provided value, and restores
        // the heap property.
        //

        internal void ReplaceMax(TElement newValue)
        {
            Contract.Assert(m_count > 0);
            m_elements[0] = newValue;
            HeapifyRoot();
        }

        //-----------------------------------------------------------------------------------
        // Removes the maximum value from the heap, and restores the heap property.
        //

        internal void RemoveMax()
        {
            Contract.Assert(m_count > 0);
            m_count--;

            if (m_count > 0)
            {
                m_elements[0] = m_elements[m_count];
                HeapifyRoot();
            }
        }

        //-----------------------------------------------------------------------------------
        // Private helpers to swap elements, and to reheapify starting from the root or
        // from a leaf element, depending on what is needed.
        //

        private void Swap(int i, int j)
        {
            TElement tmpElement = m_elements[i];
            m_elements[i] = m_elements[j];
            m_elements[j] = tmpElement;
        }

        private void HeapifyRoot()
        {
            // We are heapifying from the head of the list.
            int i = 0;
            int n = m_count;

            while (i < n)
            {
                // Calculate the current child node indexes.
                int n0 = ((i + 1) * 2) - 1;
                int n1 = n0 + 1;

                if (n0 < n && m_comparer.Compare(m_elements[i], m_elements[n0]) < 0)
                {
                    // We have to select the bigger of the two subtrees, and float
                    // the current element down. This maintains the max-heap property.
                    if (n1 < n && m_comparer.Compare(m_elements[n0], m_elements[n1]) < 0)
                    {
                        Swap(i, n1);
                        i = n1;
                    }
                    else
                    {
                        Swap(i, n0);
                        i = n0;
                    }
                }
                else if (n1 < n && m_comparer.Compare(m_elements[i], m_elements[n1]) < 0)
                {
                    // Float down the "right" subtree. We needn't compare this subtree
                    // to the "left", because if the element was smaller than that, the
                    // first if statement's predicate would have evaluated to true.
                    Swap(i, n1);
                    i = n1;
                }
                else
                {
                    // Else, the current key is in its final position. Break out
                    // of the current loop and return.
                    break;
                }
            }
        }

        private void HeapifyLastLeaf()
        {
            int i = m_count - 1;
            while (i > 0)
            {
                int j = ((i + 1) / 2) - 1;

                if (m_comparer.Compare(m_elements[i], m_elements[j]) > 0)
                {
                    Swap(i, j);
                    i = j;
                }
                else
                {
                    break;
                }
            }
        }
    }
}
