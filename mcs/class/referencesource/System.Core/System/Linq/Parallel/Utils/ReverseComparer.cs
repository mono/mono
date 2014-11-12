// ==++==
//
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
// =+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+
//
// ReverseComparer.cs
//
// <OWNER>[....]</OWNER>
//
// =-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-

using System.Collections.Generic;

namespace System.Linq.Parallel
{
    /// <summary>
    /// Comparer that wraps another comparer, and flips the result of each comparison to the
    /// opposite answer.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class ReverseComparer<T> : IComparer<T>
    {
        private IComparer<T> m_comparer;

        internal ReverseComparer(IComparer<T> comparer)
        {
            m_comparer = comparer;
        }

        public int Compare(T x, T y)
        {
            return -m_comparer.Compare(x, y);
        }
    }
}
