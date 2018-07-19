#region Imports

using System;
using System.Xml;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Threading;
using System.Transactions;
using System.Reflection;
using System.Workflow.ComponentModel;
using System.Workflow.Runtime.Hosting;
using System.Diagnostics;

#endregion

namespace System.Workflow.Runtime
{
    #region Class AmbientEnvironment

    internal abstract class AmbientEnvironment : IDisposable
    {
        /// <summary>
        /// Indicates that the value of a static field is unique for each thread
        /// CLR Perf suggests using this attribute over the slot approach.
        /// </summary>
        [ThreadStatic()]
        static EnvWrapper threadData;

        readonly object _prevEnv;
        readonly int _prevRC;

        protected AmbientEnvironment(object env)
        {
            if (threadData == null)
            {
                //Setting TLS for the first time
                threadData = new EnvWrapper();
            }

            threadData.Push(env, out _prevEnv, out _prevRC);
        }

        void IDisposable.Dispose()
        {
            Debug.Assert(null != threadData);
            threadData.Pop(_prevEnv, _prevRC);
            if (_prevRC == 0)
            {
                threadData = null;
            }
        }

        internal static object Retrieve()
        {
            if (threadData != null)
                return threadData.Retrieve();
            else
                return null;
        }

        private class EnvWrapper
        {
            int _rc;

            object _currEnv;

            internal void Push(object env, out object prevEnv, out int prevRc)
            {
                Debug.Assert(_rc >= 0);
                prevEnv = _currEnv;
                prevRc = _rc;
                _rc++;
                _currEnv = env;
            }

            internal void Pop(object prevEnv, int prevRC)
            {
                Debug.Assert(_rc > 0);
                _rc--;
                _currEnv = prevEnv;
                if (_rc != prevRC)
                {
                    Debug.Assert(false);
                    //
                }
            }

            internal object Retrieve()
            {
                Debug.Assert(_rc > 0);
                return _currEnv;
            }
        }
    }

    #endregion

    #region Class ServiceEnvironment

    // This class presents the transactional view of a WF instance:
    // mainly the current batch in transaction, and NOT the runtime view
    // of currently executing activity.
    internal sealed class ServiceEnvironment : AmbientEnvironment
    {
        internal static readonly Guid debuggerThreadGuid = new Guid("54D747AE-5CC6-4171-95C8-0A8C40443915");

        internal ServiceEnvironment(Activity currentActivity)
            : base(currentActivity)
        {
            GC.SuppressFinalize(this);
        }

        internal static IWorkBatch WorkBatch
        {
            get
            {
                Activity currentActivity = ServiceEnvironment.CurrentActivity;
                if (currentActivity == null)
                    return null;
                else
                    return (IWorkBatch)currentActivity.GetValue(WorkflowExecutor.TransientBatchProperty);
            }
        }

        internal static Guid WorkflowInstanceId
        {
            get
            {
                Activity currentActivity = ServiceEnvironment.CurrentActivity;
                if (currentActivity == null)
                    return Guid.Empty;

                return ((Guid)ContextActivityUtils.RootContextActivity(currentActivity).GetValue(WorkflowExecutor.WorkflowInstanceIdProperty));
            }
        }

        internal static WorkflowQueuingService QueuingService
        {
            get
            {
                Activity currentActivity = ServiceEnvironment.CurrentActivity;

                // fetch workflow executor
                IWorkflowCoreRuntime workflowExecutor = null;
                if (currentActivity != null)
                    workflowExecutor = ContextActivityUtils.RetrieveWorkflowExecutor(currentActivity);

                while (currentActivity != null)
                {
                    if (currentActivity == workflowExecutor.CurrentAtomicActivity)
                    {
                        TransactionalProperties transactionalProperties = (TransactionalProperties)currentActivity.GetValue(WorkflowExecutor.TransactionalPropertiesProperty);
                        if (transactionalProperties != null)
                        {
                            if (transactionalProperties.LocalQueuingService != null)
                            {
                                WorkflowQueuingService queuingService = transactionalProperties.LocalQueuingService;
                                return queuingService; // return local queuing service
                            }
                        }
                    }
                    currentActivity = currentActivity.Parent;
                }
                return null;
            }
        }

        // DO NOT change this to internal/public
        // Technically we only want to store the Batch in the TLS, 
        // but because we also want the queueing service and instId, 
        // we are storing the object encapsulating all the info.
        // The service environment only represents the transactional view:
        // the current batch; and not the current executing view:
        // e.g. some caller/caller, send/receive scenarios where 
        // we want the current batch to be the caller's so this activity 
        // does not reflect the executing activity (the callee).
        private static Activity CurrentActivity
        {
            get
            {
                object o = AmbientEnvironment.Retrieve();
                return o as Activity;
            }
        }

        internal static bool IsInServiceThread(Guid instanceId)
        {
            System.Diagnostics.Debug.Assert(instanceId != Guid.Empty, "IsInServiceThread expects valid guid.");
            if (WorkflowInstanceId == instanceId)
                return true;

            return DebuggerThreadMarker.IsInDebuggerThread();
        }
    }

    #endregion

    #region Class DebuggerThreadMarker

    internal class DebuggerThreadMarker : AmbientEnvironment
    {
        public DebuggerThreadMarker()
            : base(new object())
        {
        }

        internal static bool IsInDebuggerThread()
        {
            return AmbientEnvironment.Retrieve() != null;
        }
    }

    #endregion

    #region Class RuntimeEnvironment

    internal class RuntimeEnvironment : IDisposable
    {
        [ThreadStatic()]
        static WorkflowRuntime workflowRuntime;

        public RuntimeEnvironment(WorkflowRuntime runtime)
        {
            workflowRuntime = runtime;
        }

        internal static WorkflowRuntime CurrentRuntime
        {
            get
            {
                return RuntimeEnvironment.workflowRuntime;
            }
        }
        void IDisposable.Dispose()
        {
            workflowRuntime = null;
        }
    }

    #endregion
}
