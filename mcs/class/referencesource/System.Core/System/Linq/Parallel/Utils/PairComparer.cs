// ==++==
//
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
// =+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+
//
// PairComparer.cs
//
// <OWNER>Microsoft</OWNER>
//
// =-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-

using System.Collections.Generic;

namespace System.Linq.Parallel
{
    /// <summary>
    /// PairComparer compares pairs by the first element, and breaks ties by the second
    /// element.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="U"></typeparam>
    internal class PairComparer<T, U> : IComparer<Pair<T, U>>
    {
        private IComparer<T> m_comparer1;
        private IComparer<U> m_comparer2;

        public PairComparer(IComparer<T> comparer1, IComparer<U> comparer2)
        {
            m_comparer1 = comparer1;
            m_comparer2 = comparer2;
        }

        public int Compare(Pair<T, U> x, Pair<T, U> y)
        {
            int result1 = m_comparer1.Compare(x.First, y.First);
            if (result1 != 0) 
            {
                return result1;
            }

            return m_comparer2.Compare(x.Second, y.Second);
        }
    }
}
