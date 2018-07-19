//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.Dispatcher
{
    using System.Collections.Generic;
    using System.Runtime;
    using System.Runtime.Diagnostics;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Diagnostics;
    using System.Threading;
    using System.Workflow.Runtime;
    using System.Workflow.Runtime.Hosting;
    using System.Diagnostics;

    sealed class WorkflowOperationAsyncResult : AsyncResult
    {
        static readonly object[] emptyObjectArray = new object[] { };

        static Action<object> waitCallback = new Action<object>(WorkflowOperationAsyncResult.DoWork);
        static SendOrPostCallback sendOrPostCallback = Fx.ThunkCallback(new SendOrPostCallback(waitCallback));

        Guid instanceIdGuid;
        string instanceIdString;
        bool isOneway;
        IDictionary<string, string> outgoingContextProperties = SerializableReadOnlyDictionary<string, string>.Empty;
        object[] outputs = emptyObjectArray;
        object returnValue;
        long time;

        public WorkflowOperationAsyncResult(WorkflowOperationInvoker workflowOperationInvoker,
            WorkflowDurableInstance workflowDurableInstance, object[] inputs,
            AsyncCallback callback, object state, long time)
            : base(callback, state)
        {
            if (inputs == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("inputs");
            }

            if (workflowDurableInstance == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("workflowDurableInstance");
            }

            if (workflowOperationInvoker == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("workflowOperationInvoker");
            }

            string queueName;

            WorkflowRequestContext workflowRequestContext = new WorkflowRequestContext(
                this,
                inputs,
                GetContextProperties());

            queueName = workflowOperationInvoker.StaticQueueName;

            if (workflowRequestContext.ContextProperties.Count > 1) //DurableDispatchContextProperty. 
            {
                queueName = QueueNameHelper.Create(workflowOperationInvoker.StaticQueueName, workflowRequestContext.ContextProperties);
            }

            WorkflowInstance workflowInstance = workflowDurableInstance.GetWorkflowInstance
                (workflowOperationInvoker.CanCreateInstance);

            AsyncCallbackState callbackState = new AsyncCallbackState(workflowRequestContext,
                workflowInstance, workflowOperationInvoker.DispatchRuntime.SynchronizationContext,
                workflowOperationInvoker.InstanceLifetimeManager, queueName);

            this.isOneway = workflowOperationInvoker.IsOneWay;
            this.instanceIdGuid = workflowInstance.InstanceId;
            this.time = time;

            ActionItem.Schedule(waitCallback, callbackState);

            if (DiagnosticUtility.ShouldTraceVerbose)
            {
                string traceText = SR2.GetString(SR2.WorkflowOperationInvokerItemQueued, this.InstanceId, queueName);
                TraceUtility.TraceEvent(TraceEventType.Verbose,
                    TraceCode.WorkflowOperationInvokerItemQueued, SR.GetString(SR.TraceCodeWorkflowOperationInvokerItemQueued),
                    new StringTraceRecord("ItemDetails", traceText),
                    this, null);
            }
        }

        public long BeginTime
        {
            get
            {
                return this.time;
            }
        }

        public bool HasWorkflowRequestContextBeenSerialized
        {
            get; 
            set;
        }

        internal string InstanceId
        {
            get
            {
                if (this.instanceIdString == null)
                {
                    Fx.Assert(!this.instanceIdGuid.Equals(Guid.Empty), "WorkflowOperationInvokerAsyncResut.instanceIdGuid != Guid.Empty");
                    this.instanceIdString = this.instanceIdGuid.ToString();
                }

                return this.instanceIdString;
            }
        }

        public static object End(WorkflowOperationAsyncResult result, out object[] outputs)
        {
            if (result == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("result");
            }

            try
            {
                AsyncResult.End<WorkflowOperationAsyncResult>(result);
            }
            finally
            {
                //Application Fault's should carry Context Properties
                result.PromoteContextProperties();
            }

            outputs = result.outputs;
            return result.returnValue;
        }

        public void SendFault(Exception exception, IDictionary<string, string> contextProperties)
        {
            if (exception == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("exception");
            }

            if (!IsCompleted)
            {
                this.outgoingContextProperties = (contextProperties != null) ? new ContextDictionary(contextProperties) : null;
            }
            base.Complete(false, exception);
        }

        public void SendResponse(object returnValue, object[] outputs, IDictionary<string, string> contextProperties)
        {
            if (outputs == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("outputs");
            }

            if (!IsCompleted)
            {
                this.returnValue = returnValue;
                this.outputs = outputs;
                this.outgoingContextProperties = (contextProperties != null) ? new ContextDictionary(contextProperties) : null;
            }
            base.Complete(false);
        }

        //No-op for two-ways.
        internal void MarkOneWayOperationCompleted()
        {
            if (this.isOneway && !this.IsCompleted)
            {
                base.Complete(false);
            }
        }

        static void DoWork(object state)
        {
            if (state == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("state");
            }

            AsyncCallbackState callbackState = (AsyncCallbackState) state;

            bool executingWork = false;
            try
            {
                //If SyncContext is enabled
                //We have to do another post to get to correct thread.
                if (callbackState.SynchronizationContext != null)
                {
                    SynchronizationContext synchronizationContext = callbackState.SynchronizationContext;
                    callbackState.SynchronizationContext = null;

                    SynchronizationContextWorkflowSchedulerService.SynchronizationContextPostHelper.Post(
                        synchronizationContext,
                        WorkflowOperationAsyncResult.sendOrPostCallback,
                        callbackState);
                }
                else //We are in correct thread to do the work.
                {
                    using (new WorkflowDispatchContext(true))
                    {
                        callbackState.WorkflowRequestContext.SetOperationBegin();
                        executingWork = true;

                        if (callbackState.WorkflowInstanceLifeTimeManager != null)
                        {
                            callbackState.WorkflowInstanceLifeTimeManager.NotifyMessageArrived(callbackState.WorkflowInstance.InstanceId);
                        }

                        callbackState.WorkflowInstance.EnqueueItemOnIdle(
                            callbackState.QueueName,
                            callbackState.WorkflowRequestContext,
                            null,
                            null);
                    }
                }
            }
            catch (QueueException e)
            {
                WorkflowOperationFault operationFault = new WorkflowOperationFault(e.ErrorCode);

                try
                {
                    if (callbackState.WorkflowInstanceLifeTimeManager != null)
                    {
                        callbackState.WorkflowInstanceLifeTimeManager.ScheduleTimer(callbackState.WorkflowInstance.InstanceId);
                    }
                    callbackState.WorkflowRequestContext.SendFault(new FaultException(operationFault), null);
                }
                catch (Exception unhandled)
                {
                    if (Fx.IsFatal(unhandled))
                    {
                        throw;
                    }
                    // ignore exception; we made best effort to propagate the exception back to the invoker thread
                }
            }
            catch (Exception e)
            {
                if (Fx.IsFatal(e))
                {
                    throw;
                }

                try
                {
                    if (callbackState.WorkflowInstanceLifeTimeManager != null)
                    {
                        callbackState.WorkflowInstanceLifeTimeManager.ScheduleTimer(callbackState.WorkflowInstance.InstanceId);
                    }
                    //We should field only user code exception; Everything else should go abort path.
                    callbackState.WorkflowRequestContext.GetAsyncResult().SendFault(e, null);
                }
                catch (Exception e1)
                {
                    if (Fx.IsFatal(e1))
                    {
                        throw;
                    }
                    // ignore exception; we made best effort to propagate the exception back to the invoker thread
                }
            }
            finally
            {
                try
                {
                    if (executingWork)
                    {
                        callbackState.WorkflowRequestContext.SetOperationCompleted();
                    }
                }
                catch (Exception e1)
                {
                    if (Fx.IsFatal(e1))
                    {
                        throw;
                    }

                    // ignore exception; we made best effort to propagate the exception back to the invoker thread                    }
                }
            }
        }

        IDictionary<string, string> GetContextProperties()
        {
            Fx.Assert(OperationContext.Current != null, "Called from non service thread");

            ContextMessageProperty incomingContextProperties = null;
            if (OperationContext.Current.IncomingMessageProperties != null
                && ContextMessageProperty.TryGet(OperationContext.Current.IncomingMessageProperties, out incomingContextProperties))
            {
                return incomingContextProperties.Context;
            }
            else
            {
                return SerializableReadOnlyDictionary<string, string>.Empty;
            }
        }

        void PromoteContextProperties()
        {
            Fx.Assert(OperationContext.Current != null, "Called from non service thread");

            if (outgoingContextProperties != null)
            {
                ContextMessageProperty context;
                if (!ContextMessageProperty.TryGet(OperationContext.Current.OutgoingMessageProperties, out context))
                {
                    new ContextMessageProperty(this.outgoingContextProperties).AddOrReplaceInMessageProperties(OperationContext.Current.OutgoingMessageProperties);
                }
                else
                {
                    foreach (KeyValuePair<string, string> contextElement in this.outgoingContextProperties)
                    {
                        context.Context[contextElement.Key] = contextElement.Value;
                    }
                }
            }
        }

        class AsyncCallbackState
        {
            WorkflowInstanceLifetimeManagerExtension instanceLifeTimeManager;
            IComparable queueName;
            SynchronizationContext synchronizationContext;
            WorkflowInstance workflowInstance;
            WorkflowRequestContext workflowRequestContext;

            public AsyncCallbackState(
                WorkflowRequestContext workflowRequestContext,
                WorkflowInstance workflowInstance,
                SynchronizationContext synchronizationContext,
                WorkflowInstanceLifetimeManagerExtension instanceLifeTimeManager,
                IComparable queueName)
            {
                this.workflowInstance = workflowInstance;
                this.workflowRequestContext = workflowRequestContext;
                this.synchronizationContext = synchronizationContext;
                this.queueName = queueName;
                this.instanceLifeTimeManager = instanceLifeTimeManager;
            }

            public IComparable QueueName
            {
                get
                {
                    return this.queueName;
                }
            }

            public SynchronizationContext SynchronizationContext
            {
                get
                {
                    return this.synchronizationContext;
                }
                set
                {
                    this.synchronizationContext = value;
                }
            }

            public WorkflowInstance WorkflowInstance
            {
                get
                {
                    return this.workflowInstance;
                }
            }

            public WorkflowInstanceLifetimeManagerExtension WorkflowInstanceLifeTimeManager
            {
                get
                {
                    return this.instanceLifeTimeManager;
                }
            }

            public WorkflowRequestContext WorkflowRequestContext
            {
                get
                {
                    return this.workflowRequestContext;
                }
            }
        }
    }
}
