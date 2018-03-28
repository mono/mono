// ==++==
//
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
// =+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+
//
// QuerySettings.cs
//
// <OWNER>Microsoft</OWNER>
//
// =-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-

using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics.Contracts;
#if SILVERLIGHT
using System.Core; // for System.Core.SR
#endif

namespace System.Linq.Parallel
{
    /// <summary>
    /// This type contains query execution options specified by the user.
    /// QuerySettings are used as follows:
    /// - in the query construction phase, some settings may be uninitialized.
    /// - at the start of the query opening phase, the WithDefaults method
    ///   is used to initialize all uninitialized settings.
    /// - in the rest of the query opening phase, we assume that all settings
    ///   have been initialized.
    /// </summary>
    internal struct QuerySettings
    {
        private TaskScheduler m_taskScheduler;
        private int? m_degreeOfParallelism;
        private CancellationState m_cancellationState;
        private ParallelExecutionMode? m_executionMode;
        private ParallelMergeOptions? m_mergeOptions;
        private int m_queryId;

        internal CancellationState CancellationState
        {
            get { return m_cancellationState; }
            set
            {
                m_cancellationState = value;
                Contract.Assert(m_cancellationState != null);
            }
        }

        // The task manager on which to execute the query.
        internal TaskScheduler TaskScheduler
        {
            get { return m_taskScheduler; }
            set { m_taskScheduler = value; }
        }

        // The number of parallel tasks to utilize.
        internal int? DegreeOfParallelism
        {
            get { return m_degreeOfParallelism; }
            set { m_degreeOfParallelism = value; }
        }

        // The mode in which to execute this query.
        internal ParallelExecutionMode? ExecutionMode
        {
            get { return m_executionMode; }
            set { m_executionMode = value; }
        }

        internal ParallelMergeOptions? MergeOptions
        {
            get { return m_mergeOptions; }
            set { m_mergeOptions = value; }
        }

        internal int QueryId
        {
            get
            {
                return m_queryId;
            }
        }

        //-----------------------------------------------------------------------------------
        // Constructs a new settings structure.
        //
        internal QuerySettings(TaskScheduler taskScheduler, int? degreeOfParallelism, 
            CancellationToken externalCancellationToken, ParallelExecutionMode? executionMode,
            ParallelMergeOptions? mergeOptions)
        {
            m_taskScheduler = taskScheduler;
            m_degreeOfParallelism = degreeOfParallelism;
            m_cancellationState = new CancellationState(externalCancellationToken);
            m_executionMode = executionMode;
            m_mergeOptions = mergeOptions;
            m_queryId = -1;

            Contract.Assert(m_cancellationState != null);
        }

        //-----------------------------------------------------------------------------------
        // Combines two sets of options.
        //
        internal QuerySettings Merge(QuerySettings settings2)
        {
            if (this.TaskScheduler != null && settings2.TaskScheduler != null)
            {
                throw new InvalidOperationException(SR.GetString(SR.ParallelQuery_DuplicateTaskScheduler));
            }

            if (this.DegreeOfParallelism != null && settings2.DegreeOfParallelism != null)
            {
                throw new InvalidOperationException(SR.GetString(SR.ParallelQuery_DuplicateDOP));
            }

            if (this.CancellationState.ExternalCancellationToken.CanBeCanceled && settings2.CancellationState.ExternalCancellationToken.CanBeCanceled)
            {
                throw new InvalidOperationException(SR.GetString(SR.ParallelQuery_DuplicateWithCancellation));
            }
            
            if (this.ExecutionMode != null && settings2.ExecutionMode != null)
            {
                throw new InvalidOperationException(SR.GetString(SR.ParallelQuery_DuplicateExecutionMode));
            }
            
            if (this.MergeOptions != null && settings2.MergeOptions != null)
            {
                throw new InvalidOperationException(SR.GetString(SR.ParallelQuery_DuplicateMergeOptions));
            }
            
            TaskScheduler tm = (this.TaskScheduler == null) ? settings2.TaskScheduler : this.TaskScheduler;
            int? dop = this.DegreeOfParallelism.HasValue ? this.DegreeOfParallelism : settings2.DegreeOfParallelism;
            CancellationToken externalCancellationToken = (this.CancellationState.ExternalCancellationToken.CanBeCanceled) ? this.CancellationState.ExternalCancellationToken : settings2.CancellationState.ExternalCancellationToken;
            ParallelExecutionMode? executionMode = this.ExecutionMode.HasValue ? this.ExecutionMode : settings2.ExecutionMode;
            ParallelMergeOptions? mergeOptions = this.MergeOptions.HasValue ? this.MergeOptions : settings2.MergeOptions;
            
            return new QuerySettings(tm, dop, externalCancellationToken, executionMode, mergeOptions);
        }

        internal QuerySettings WithPerExecutionSettings()
        {
            return WithPerExecutionSettings(new CancellationTokenSource(), new Shared<bool>(false));
        }

        internal QuerySettings WithPerExecutionSettings(CancellationTokenSource topLevelCancellationTokenSource, Shared<bool> topLevelDisposedFlag)
        {
            //Initialize a new QuerySettings structure and copy in the current settings.
            //Note: this has the very important effect of newing a fresh CancellationSettings, 
            //      and _not_ copying in the current internalCancellationSource or topLevelDisposedFlag which should not be 
            //      propogated to internal query executions. (This affects SelectMany execution and specifically fixes bug:535510)
            //      The fresh toplevel parameters are used instead.
            QuerySettings settings = new QuerySettings(TaskScheduler, DegreeOfParallelism, CancellationState.ExternalCancellationToken, ExecutionMode, MergeOptions);

            Contract.Assert(topLevelCancellationTokenSource != null, "There should always be a top-level cancellation signal specified.");
            settings.CancellationState.InternalCancellationTokenSource = topLevelCancellationTokenSource;

            //Merge internal and external tokens to form the combined token
             settings.CancellationState.MergedCancellationTokenSource =
                    CancellationTokenSource.CreateLinkedTokenSource(settings.CancellationState.InternalCancellationTokenSource.Token, settings.CancellationState.ExternalCancellationToken);
 
            // and copy in the topLevelDisposedFlag 
            settings.CancellationState.TopLevelDisposedFlag = topLevelDisposedFlag;

            Contract.Assert(settings.CancellationState.InternalCancellationTokenSource != null);
            Contract.Assert(settings.CancellationState.MergedCancellationToken.CanBeCanceled);
            Contract.Assert(settings.CancellationState.TopLevelDisposedFlag != null);

            // Finally, assign a query Id to the settings
#if !PFX_LEGACY_3_5 && !SILVERLIGHT
            settings.m_queryId = PlinqEtwProvider.NextQueryId();
#endif

            return settings;
        }

        //-----------------------------------------------------------------------------------
        // Copies the settings, replacing unspecified settings with defaults.
        //
        internal QuerySettings WithDefaults()
        {
            QuerySettings settings = this;
            if (settings.TaskScheduler == null)
            {
                settings.TaskScheduler = TaskScheduler.Default;
            }

            if (settings.DegreeOfParallelism == null)
            {
                settings.DegreeOfParallelism = Scheduling.GetDefaultDegreeOfParallelism();
            }

            if (settings.ExecutionMode == null)
            {
                settings.ExecutionMode = ParallelExecutionMode.Default;
            }

            if (settings.MergeOptions == null)
            {
                settings.MergeOptions = ParallelMergeOptions.Default;
            }

            if (settings.MergeOptions == ParallelMergeOptions.Default)
            {
                settings.MergeOptions = ParallelMergeOptions.AutoBuffered;
            }

            Contract.Assert(settings.TaskScheduler != null);
            Contract.Assert(settings.DegreeOfParallelism.HasValue);
            Contract.Assert(settings.DegreeOfParallelism.Value >= 1 && settings.DegreeOfParallelism <= Scheduling.MAX_SUPPORTED_DOP);
            Contract.Assert(settings.ExecutionMode != null);
            Contract.Assert(settings.MergeOptions != null);

            Contract.Assert(settings.MergeOptions != ParallelMergeOptions.Default);

            return settings;
        }

        // Returns the default settings
        internal static QuerySettings Empty {
            get { return new QuerySettings(null, null, new CancellationToken(), null, null); }
        }

        // Cleanup internal state once the entire query is complete.
        // (this should not be performed after a 'premature-query' completes as the state should live
        // uninterrupted for the duration of the full query.)
        public void CleanStateAtQueryEnd()
        {
            m_cancellationState.MergedCancellationTokenSource.Dispose();
        }
    }
}
