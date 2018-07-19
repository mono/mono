// ==++==
//
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
// =+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+
//
// WrapperEqualityComparer.cs
//
// <OWNER>Microsoft</OWNER>
//
// =-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-

using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace System.Linq.Parallel
{
    /// <summary>
    /// Compares two wrapped structs of the same underlying type for equality.  Simply
    /// wraps the actual comparer for the type being wrapped.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal struct WrapperEqualityComparer<T> : IEqualityComparer<Wrapper<T>>
    {
        private IEqualityComparer<T> m_comparer;

        internal WrapperEqualityComparer(IEqualityComparer<T> comparer)
        {
            if (comparer == null)
            {
                m_comparer = EqualityComparer<T>.Default;
            }
            else
            {
                m_comparer = comparer;
            }
        }

        public bool Equals(Wrapper<T> x, Wrapper<T> y)
        {
            Contract.Assert(m_comparer != null);
            return m_comparer.Equals(x.Value, y.Value);
        }

        public int GetHashCode(Wrapper<T> x)
        {
            Contract.Assert(m_comparer != null);
            return m_comparer.GetHashCode(x.Value);
        }
    }
}
