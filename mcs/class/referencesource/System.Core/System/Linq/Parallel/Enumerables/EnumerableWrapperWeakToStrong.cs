// ==++==
//
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
// =+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+
//
// EnumerableWrapperWeakToStrong.cs
//
// <OWNER>[....]</OWNER>
//
// =-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-

using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace System.Linq.Parallel
{
    /// <summary>
    /// A simple implementation of the IEnumerable{object} interface which wraps
    /// a weakly typed IEnumerable object, allowing it to be accessed as a strongly typed
    /// IEnumerable{object}.
    /// </summary>
    internal class EnumerableWrapperWeakToStrong : IEnumerable<object>
    {

        private readonly IEnumerable m_wrappedEnumerable; // The wrapped enumerable object.

        //-----------------------------------------------------------------------------------
        // Instantiates a new wrapper object.
        //

        internal EnumerableWrapperWeakToStrong(IEnumerable wrappedEnumerable)
        {
            Contract.Assert(wrappedEnumerable != null);
            m_wrappedEnumerable = wrappedEnumerable;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<object>)this).GetEnumerator();
        }

        public IEnumerator<object> GetEnumerator()
        {
            return new WrapperEnumeratorWeakToStrong(m_wrappedEnumerable.GetEnumerator());
        }

        //-----------------------------------------------------------------------------------
        // A wrapper over IEnumerator that provides IEnumerator<object> interface
        //

        class WrapperEnumeratorWeakToStrong : IEnumerator<object>
        {

            private IEnumerator m_wrappedEnumerator; // The weakly typed enumerator we've wrapped.

            //-----------------------------------------------------------------------------------
            // Wrap the specified enumerator in a new weak-to-strong converter.
            //

            internal WrapperEnumeratorWeakToStrong(IEnumerator wrappedEnumerator)
            {
                Contract.Assert(wrappedEnumerator != null);
                m_wrappedEnumerator = wrappedEnumerator;
            }

            //-----------------------------------------------------------------------------------
            // These are all really simple IEnumerator<object> implementations that simply
            // forward to the corresponding weakly typed IEnumerator methods.
            //

            object IEnumerator.Current
            {
                get { return m_wrappedEnumerator.Current; }
            }

            object IEnumerator<object>.Current
            {
                get { return m_wrappedEnumerator.Current; }
            }

            void IDisposable.Dispose()
            {
                IDisposable disposable = m_wrappedEnumerator as IDisposable;
                if (disposable != null)
                {
                    disposable.Dispose();
                }
            }

            bool IEnumerator.MoveNext()
            {
                return m_wrappedEnumerator.MoveNext();
            }

            void IEnumerator.Reset()
            {
                m_wrappedEnumerator.Reset();
            }

        }

    }
}
