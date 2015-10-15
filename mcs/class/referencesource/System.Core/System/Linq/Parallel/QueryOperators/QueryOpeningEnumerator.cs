// ==++==
//
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
// =+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+
//
// QueryOpeningEnumerator.cs
//
// <OWNER>Microsoft</OWNER>
//
// =-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-

using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Diagnostics.Contracts;
#if SILVERLIGHT
using System.Core; // for System.Core.SR
#endif

namespace System.Linq.Parallel
{
    /// <summary>
    /// A wrapper enumerator that just opens the query operator when MoveNext() is called for the
    /// first time. We use QueryOpeningEnumerator to call QueryOperator.GetOpenedEnumerator()
    /// lazily because once GetOpenedEnumerator() is called, PLINQ starts precomputing the
    /// results of the query.
    /// </summary>
    internal class QueryOpeningEnumerator<TOutput> : IEnumerator<TOutput>
    {
        private readonly QueryOperator<TOutput> m_queryOperator;
        private IEnumerator<TOutput> m_openedQueryEnumerator;
        private QuerySettings m_querySettings;
        private readonly ParallelMergeOptions? m_mergeOptions;
        private readonly bool m_suppressOrderPreservation;
        private int m_moveNextIteration = 0;
        private bool m_hasQueryOpeningFailed;
        
        // -- Cancellation and Dispose fields--
        // Disposal of the queryOpeningEnumerator can trigger internal cancellation and so it is important
        // that the internal cancellation signal is available both at this level, and deep in query execution
        // Also, it is useful to track the cause of cancellation so that appropriate exceptions etc can be
        // throw from the execution managers.
        // => Both the topLevelDisposeFlag and the topLevelCancellationSignal are defined here, and will be shared
        //    down to QuerySettings and to the QueryTaskGroupStates that are associated with actual task-execution.
        // => whilst these are the definitions, it is best to consider QuerySettings as the true owner of these.
        private readonly Shared<bool> m_topLevelDisposedFlag = new Shared<bool>(false);  //a shared<bool> so that it can be referenced by others.

        // a top-level cancellation signal is required so that QueryOpeningEnumerator.Dispose() can tear things down.
        // This cancellationSignal will be used as the actual internal signal in QueryTaskGroupState.
        private readonly CancellationTokenSource m_topLevelCancellationTokenSource = new CancellationTokenSource();
        

        internal QueryOpeningEnumerator(QueryOperator<TOutput> queryOperator, ParallelMergeOptions? mergeOptions, bool suppressOrderPreservation)
        {
            Contract.Assert(queryOperator != null);

            m_queryOperator = queryOperator;
            m_mergeOptions = mergeOptions;
            m_suppressOrderPreservation = suppressOrderPreservation;
        }

        public TOutput Current
        {
            get
            {
                if (m_openedQueryEnumerator == null)
                {
                    throw new InvalidOperationException(SR.GetString(SR.PLINQ_CommonEnumerator_Current_NotStarted));
                }

                return m_openedQueryEnumerator.Current;
            }
        }

        public void Dispose()
        {
            m_topLevelDisposedFlag.Value = true;
            m_topLevelCancellationTokenSource.Cancel(); // initiate internal cancellation.
            if (m_openedQueryEnumerator != null)
            {
                m_openedQueryEnumerator.Dispose();
                m_querySettings.CleanStateAtQueryEnd();
            }

            QueryLifecycle.LogicalQueryExecutionEnd(m_querySettings.QueryId);
        }

        object IEnumerator.Current
        {
            get { return ((IEnumerator<TOutput>)this).Current; }
        }

        public bool MoveNext()
        {
            if (m_topLevelDisposedFlag.Value)
            {
                throw new ObjectDisposedException("enumerator", SR.GetString(SR.PLINQ_DisposeRequested));
            }

            
            //Note: if Dispose has been called on a different thread to the thread that is enumerating,
            //then there is a ---- where m_openedQueryEnumerator is instantiated but not disposed.
            //Best practice is that Dispose() should only be called by the owning thread, hence this cannot occur in correct usage scenarios.
            
            // Open the query operator if called for the first time.
            
            if (m_openedQueryEnumerator == null)
            {
                // To keep the MoveNext method body small, the code that executes first time only is in a separate method.
                // It appears that if the method becomes too large, we observe a performance regression. This may have
                // to do with method inlining. See 
                OpenQuery();
            }

            bool innerMoveNextResult = m_openedQueryEnumerator.MoveNext();

            // This provides cancellation-testing for the consumer-side of the buffers that appears in each scenario:
            //   Non-order-preserving (defaultMergeHelper)
            //       - asynchronous channel (pipelining) 
            //       - synchronous channel  (stop-and-go)
            //   Order-preserving (orderPreservingMergeHelper)
            //       - internal results buffer.
            // This moveNext is consuming data out of buffers, hence the inner moveNext is expected to be very fast.
            // => thus we only test for cancellation per-N-iterations.
            // NOTE: the cancellation check occurs after performing moveNext in case the cancellation caused no data 
            //       to be produced.. We need to ensure that users sees an OCE rather than simply getting no data. (see Bug702254)
            if ((m_moveNextIteration & CancellationState.POLL_INTERVAL) == 0)
            {
                CancellationState.ThrowWithStandardMessageIfCanceled(
                    m_querySettings.CancellationState.ExternalCancellationToken);
            }

            m_moveNextIteration++;
            return innerMoveNextResult;
        }

        /// <summary>
        /// Opens the query and initializes m_openedQueryEnumerator and m_querySettings.
        /// Called from the first MoveNext call.
        /// </summary>
        private void OpenQuery()
        {
            // Avoid opening (and failing) twice.. not only would it be bad to re-enumerate some elements, but
            // the cancellation/disposed flags are most likely stale.
            if (m_hasQueryOpeningFailed)
                throw new InvalidOperationException(SR.GetString(SR.PLINQ_EnumerationPreviouslyFailed));

            try
            {
                // stuff in appropriate defaults for unspecified options.
                m_querySettings = m_queryOperator.SpecifiedQuerySettings
                    .WithPerExecutionSettings(m_topLevelCancellationTokenSource, m_topLevelDisposedFlag)
                    .WithDefaults();

                QueryLifecycle.LogicalQueryExecutionBegin(m_querySettings.QueryId);

                m_openedQueryEnumerator = m_queryOperator.GetOpenedEnumerator(
                    m_mergeOptions, m_suppressOrderPreservation, false, m_querySettings);


                // Now that we have opened the query, and got our hands on a supplied cancellation token
                // we can perform an early cancellation check so that we will not do any major work if the token is already canceled.
                CancellationState.ThrowWithStandardMessageIfCanceled(m_querySettings.CancellationState.ExternalCancellationToken);
            }
            catch
            {
                m_hasQueryOpeningFailed = true;
                throw;
            }
        }

        public void Reset()
        {
            throw new NotSupportedException();
        }
    }
}
