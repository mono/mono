// ==++==
//
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
// =+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+
//
// QueryTaskGroupState.cs
//
// <OWNER>Microsoft</OWNER>
//
// =-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics.Contracts;
#if SILVERLIGHT
using System.Core; // for System.Core.SR
#endif

namespace System.Linq.Parallel
{
    /// <summary>
    /// A collection of tasks used by a single query instance. This type also offers some
    /// convenient methods for tracing significant ETW events, waiting on tasks, propagating
    /// exceptions, and performing cancellation activities.
    /// </summary>
    internal class QueryTaskGroupState
    {
        private Task m_rootTask; // The task under which all query tasks root.
        private int m_alreadyEnded; // Whether the tasks have been waited on already.
        private CancellationState m_cancellationState; // The cancellation state.
        private int m_queryId; // Id of this query execution.


        //-----------------------------------------------------------------------------------
        // Creates a new shared bit of state among tasks.
        //

        internal QueryTaskGroupState(CancellationState cancellationState, int queryId)
        {
            m_cancellationState = cancellationState;
            m_queryId = queryId;
        }

        //-----------------------------------------------------------------------------------
        // Whether this query has ended or not.
        //

        internal bool IsAlreadyEnded
        {
            get { return m_alreadyEnded == 1; }
        }

        //-----------------------------------------------------------------------------------
        // Cancellation state, used to tear down tasks cooperatively when necessary.
        //

        internal CancellationState CancellationState
        {
            get { return m_cancellationState; }
        }

        //-----------------------------------------------------------------------------------
        // Id of this query execution.
        //

        internal int QueryId
        {
            get { return m_queryId; }
        }

        //-----------------------------------------------------------------------------------
        // Marks the beginning of a query's execution.
        //

        internal void QueryBegin(Task rootTask)
        {
            Contract.Assert(rootTask != null, "Expected a non-null task");
            Contract.Assert(m_rootTask == null, "Cannot begin a query more than once");
            m_rootTask = rootTask;
        }

        //-----------------------------------------------------------------------------------
        // Marks the end of a query's execution, waiting for all tasks to finish and
        // propagating any relevant exceptions.  Note that the full set of tasks must have
        // been initialized (with SetTask) before calling this.
        //

        internal void QueryEnd(bool userInitiatedDispose)
        {
            Contract.Assert(m_rootTask != null);
            //Contract.Assert(Task.Current == null || (Task.Current != m_rootTask && Task.Current.Parent != m_rootTask));

            if (Interlocked.Exchange(ref m_alreadyEnded, 1) == 0)
            {
                // There are four cases:
                // Case #1: Wait produced an exception that is not OCE(ct), or an AggregateException which is not full of OCE(ct) ==>  We rethrow.
                // Case #2: External cancellation has been requested ==> we'll manually throw OCE(externalToken).
                // Case #3a: We are servicing a call to Dispose() (and possibly also external cancellation has been requested).. simply return. See 



                // See also "InlinedAggregationOperator" which duplicates some of this logic for the aggregators.
                // See also "QueryOpeningEnumerator" which duplicates some of this logic.
                // See also "ExceptionAggregator" which duplicates some of this logic.

                try
                {
                    // Wait for all the tasks to complete
                    // If any of the tasks ended in the Faulted stated, an AggregateException will be thrown.
                    m_rootTask.Wait();
                }
                catch (AggregateException ae)
                {
                    AggregateException flattenedAE = ae.Flatten();
                    bool allOCEsOnTrackedExternalCancellationToken = true;
                    for (int i = 0; i < flattenedAE.InnerExceptions.Count; i++)
                    {
                        OperationCanceledException oce = flattenedAE.InnerExceptions[i] as OperationCanceledException;

                        // we only let it pass through iff:
                        // it is not null, not default, and matches the exact token we were given as being the external token
                        // and the external Token is actually canceled (ie not a spoof OCE(extCT) for a non-canceled extCT)
                        if (oce == null ||
                            !oce.CancellationToken.IsCancellationRequested ||
                            oce.CancellationToken != m_cancellationState.ExternalCancellationToken)
                        {
                            allOCEsOnTrackedExternalCancellationToken = false;
                            break;
                        }
                    }

                    // if all the exceptions were OCE(externalToken), then we will propogate only a single OCE(externalToken) below
                    // otherwise, we flatten the aggregate (because the WaitAll above already aggregated) and rethrow.
                    if (!allOCEsOnTrackedExternalCancellationToken)
                        throw flattenedAE;  // Case #1
                }
                finally
                {
                    m_rootTask.Dispose();
                }

                if (m_cancellationState.MergedCancellationToken.IsCancellationRequested)
                {
                    // cancellation has occured but no user-delegate exceptions were detected 

                    // NOTE: it is important that we see other state variables correctly here, and that
                    // read-reordering hasn't played havoc. 
                    // This is OK because 
                    //   1. all the state writes (eg in the Initiate* methods) are volatile writes (standard .NET MM)
                    //   2. tokenCancellationRequested is backed by a volatile field, hence the reads below
                    //   won't get reordered about the read of token.IsCancellationRequested.

                    // If the query has already been disposed, we don't want to throw an OCE (this is a fix for 
                    if (!m_cancellationState.TopLevelDisposedFlag.Value)
                    {
                        CancellationState.ThrowWithStandardMessageIfCanceled(m_cancellationState.ExternalCancellationToken); // Case #2
                    }

                    //otherwise, given that there were no user-delegate exceptions (they would have been rethrown above),
                    //the only remaining situation is user-initiated dispose.
                    Contract.Assert(m_cancellationState.TopLevelDisposedFlag.Value);

                    // If we aren't actively disposing, that means somebody else previously disposed
                    // of the enumerator. We must throw an ObjectDisposedException.
                    if (!userInitiatedDispose)
                    {
                        throw new ObjectDisposedException("enumerator", SR.GetString(SR.PLINQ_DisposeRequested)); // Case #3
                    }
                }

                // Case #4. nothing to do.
            }
        }
    }
}
