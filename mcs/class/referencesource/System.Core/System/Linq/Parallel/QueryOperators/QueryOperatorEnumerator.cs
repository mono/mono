// ==++==
//
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
// =+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+
//
// QueryOperatorEnumerator.cs
//
// <OWNER>Microsoft</OWNER>
//
// =-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-

using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
#if SILVERLIGHT
using System.Core; // for System.Core.SR
#endif

namespace System.Linq.Parallel
{
    /// <summary>
    /// A common enumerator type that unifies all query operator enumerators. 
    /// </summary>
    /// <typeparam name="TElement"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    internal abstract class QueryOperatorEnumerator<TElement, TKey>
    {
        // Moves the position of the enumerator forward by one, and simultaneously returns
        // the (new) current element and key. If empty, false is returned.
        internal abstract bool MoveNext(ref TElement currentElement, ref TKey currentKey);

        // Standard implementation of the disposable pattern.
        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            // This is a no-op by default.  Subclasses can override.
        }

        internal virtual void Reset()
        {
            // This is a no-op by default.  Subclasses can override.
        }

        //-----------------------------------------------------------------------------------
        // A simple way to turn a query operator enumerator into a "classic" one.
        //

        internal IEnumerator<TElement> AsClassicEnumerator()
        {
            return new QueryOperatorClassicEnumerator(this);
        }

        class QueryOperatorClassicEnumerator : IEnumerator<TElement>
        {
            private QueryOperatorEnumerator<TElement, TKey> m_operatorEnumerator;
            private TElement m_current;

            internal QueryOperatorClassicEnumerator(QueryOperatorEnumerator<TElement, TKey> operatorEnumerator)
            {
                Contract.Assert(operatorEnumerator != null);
                m_operatorEnumerator = operatorEnumerator;
            }

            public bool MoveNext()
            {
                TKey keyUnused = default(TKey);
                return m_operatorEnumerator.MoveNext(ref m_current, ref keyUnused);
            }

            public TElement Current
            {
                get { return m_current; }
            }

            object IEnumerator.Current
            {
                get { return m_current; }
            }

            public void Dispose()
            {
                m_operatorEnumerator.Dispose();
                m_operatorEnumerator = null;
            }

            public void Reset()
            {
                m_operatorEnumerator.Reset();
            }
        }
    }
}
