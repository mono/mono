// ==++==
//
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
// =+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+
//
// MergeEnumerator.cs
//
// <OWNER>Microsoft</OWNER>
//
// =-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-

using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace System.Linq.Parallel
{
    /// <summary>
    /// Convenience class used by enumerators that merge many partitions into one. 
    /// </summary>
    /// <typeparam name="TInputOutput"></typeparam>
    internal abstract class MergeEnumerator<TInputOutput> : IEnumerator<TInputOutput>
    {
        protected QueryTaskGroupState m_taskGroupState;

        //-----------------------------------------------------------------------------------
        // Initializes a new enumerator with the specified group state.
        //

        protected MergeEnumerator(QueryTaskGroupState taskGroupState)
        {
            Contract.Assert(taskGroupState != null);
            m_taskGroupState = taskGroupState;
        }

        //-----------------------------------------------------------------------------------
        // Abstract members of IEnumerator<T> that must be implemented by concrete subclasses.
        //

        public abstract TInputOutput Current { get; }
        
        public abstract bool MoveNext();

        //-----------------------------------------------------------------------------------
        // Straightforward IEnumerator<T> methods. So subclasses needn't bother.
        //

        object IEnumerator.Current
        {
            get { return ((IEnumerator<TInputOutput>)this).Current; }
        }

        public virtual void Reset()
        {
            // (intentionally left blank)
        }

        //-----------------------------------------------------------------------------------
        // If the enumerator is disposed of before the query finishes, we need to ensure
        // we properly tear down the query such that exceptions are not lost.
        //

        public virtual void Dispose()
        {
            // If the enumerator is being disposed of before the query has finished,
            // we will wait for the query to finish.  Cancellation should have already
            // been initiated, so we just need to ensure exceptions are propagated.
            if (!m_taskGroupState.IsAlreadyEnded)
            {
                Contract.Assert(m_taskGroupState.CancellationState.TopLevelDisposedFlag.Value);
                m_taskGroupState.QueryEnd(true);
            }
        }
    }
}
