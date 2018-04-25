// ==++==
//
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
// =+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+
//
// GrowingArray.cs
//
// <OWNER>Microsoft</OWNER>
//
// =-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-

using System.Diagnostics.Contracts;

namespace System.Linq.Parallel
{
    /// <summary>
    /// A growing array. Unlike List{T}, it makes the internal array available to its user.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class GrowingArray<T>
    {
        T[] m_array;
        int m_count;
        const int DEFAULT_ARRAY_SIZE = 1024;

        internal GrowingArray()
        {
            m_array = new T[DEFAULT_ARRAY_SIZE];
            m_count = 0;
        }

        //---------------------------------------------------------------------------------------
        // Returns the internal array representing the list. Note that the array may be larger
        // than necessary to hold all elements in the list.
        //

        internal T[] InternalArray
        {
            get { return m_array; }
        }

        internal int Count
        {
            get { return m_count; }
        }

        internal void Add(T element)
        {
            if (m_count >= m_array.Length)
            {
                GrowArray(2 * m_array.Length);
            }
            m_array[m_count++] = element;
        }

        private void GrowArray(int newSize)
        {
            Contract.Assert(newSize > m_array.Length);

            T[] array2 = new T[newSize];
            m_array.CopyTo(array2, 0);
            m_array = array2;
        }

        internal void CopyFrom(T[] otherArray, int otherCount)
        {
            // Ensure there is just enough room for both.
            if (m_count + otherCount > m_array.Length)
            {
                GrowArray(m_count + otherCount);
            }

            // And now just blit the keys directly.
            Array.Copy(otherArray, 0, m_array, m_count, otherCount);
            m_count += otherCount;
        }
    }
}
