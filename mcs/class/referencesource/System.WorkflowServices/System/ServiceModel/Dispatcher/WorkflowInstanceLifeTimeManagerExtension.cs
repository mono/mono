//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Dispatcher
{
    using System.Collections.Generic;
    using System.Runtime;
    using System.Runtime.Diagnostics;
    using System.ServiceModel.Diagnostics;
    using System.Threading;
    using System.Workflow.Runtime;
    using System.Workflow.Runtime.Hosting;
    using System.Diagnostics;

    class WorkflowInstanceLifetimeManagerExtension : IExtension<ServiceHostBase>
    {

        readonly Action<object> cachedInstanceExpirationTimerCallback;
        TimeSpan cachedInstanceExpiration;
        bool hasPersistenceService;
        Dictionary<Guid, InstanceRecord> instanceRecordMap;

        //This class takes self contained lock, never calls out external method with lock taken.
        object lockObject;
        WorkflowRuntime workflowRuntime;

        public WorkflowInstanceLifetimeManagerExtension(WorkflowRuntime workflowRuntime, TimeSpan cachedInstanceExpiration, bool hasPersistenceService)
        {
            this.workflowRuntime = workflowRuntime;
            this.cachedInstanceExpiration = cachedInstanceExpiration;
            this.instanceRecordMap = new Dictionary<Guid, InstanceRecord>();
            this.lockObject = new object();
            this.cachedInstanceExpirationTimerCallback = new Action<object>(this.OnTimer);
            this.hasPersistenceService = hasPersistenceService;


            RegisterEvents();
        }

        //This is called when InstanceContext is taken down behind us;
        //1. UnhandledException causing InstanceContext to Abort;
        //2. Activating Request to Non-Activating operation;
        public void CleanUp(Guid instanceId)
        {
            //If no WorkflowInstance actively running for this InstanceId;
            //This will be last opportunity to cleanup their record; to avoid 
            //growth of this HashTable.
            InstanceRecord instanceRecord;

            lock (this.lockObject)
            {
                if (this.instanceRecordMap.TryGetValue(instanceId, out instanceRecord))
                {
                    if (!instanceRecord.InstanceLoadedOrStarted)
                    {
                        if (instanceRecord.UnloadTimer != null)
                        {
                            instanceRecord.UnloadTimer.Cancel();
                        }
                        this.instanceRecordMap.Remove(instanceId);
                    }
                }
            }
        }

        void IExtension<ServiceHostBase>.Attach(ServiceHostBase owner)
        {

        }

        void IExtension<ServiceHostBase>.Detach(ServiceHostBase owner)
        {

        }

        public bool IsInstanceInMemory(Guid instanceId)
        {
            InstanceRecord instanceRecord;

            lock (this.lockObject)
            {
                if (instanceRecordMap.TryGetValue(instanceId, out instanceRecord))
                {
                    return instanceRecord.InstanceLoadedOrStarted;
                }
            }
            return false;
        }

        //Assumption: Message arrived means; instance turned to Executing state.
        public void NotifyMessageArrived(Guid instanceId)
        {
            CancelTimer(instanceId, false);
        }

        public void NotifyWorkflowActivationComplete(Guid instanceId, WaitCallback callback, object state, bool fireImmediatelyIfDontExist)
        {
            bool instanceFound;
            InstanceRecord instanceRecord;

            lock (this.lockObject)
            {
                instanceFound = instanceRecordMap.TryGetValue(instanceId, out instanceRecord);
                if (instanceFound)
                {
                    instanceRecord.Callback = callback;
                    instanceRecord.CallbackState = state;
                }
                else if (!fireImmediatelyIfDontExist)
                {
                    instanceRecord = new InstanceRecord();
                    instanceRecord.Callback = callback;
                    instanceRecord.CallbackState = state;
                    instanceRecordMap.Add(instanceId, instanceRecord);
                }
            }

            if (!instanceFound && fireImmediatelyIfDontExist)
            {
                //Instance is not in-memory; Notify immediately.
                callback(state);
            }
        }

        public void ScheduleTimer(Guid instanceId)
        {
            InstanceRecord instanceRecord;
            lock (this.lockObject)
            {
                if (this.instanceRecordMap.TryGetValue(instanceId, out instanceRecord))
                {
                    if (instanceRecord.UnloadTimer != null)
                    {
                        instanceRecord.UnloadTimer.Cancel();
                    }
                    else
                    {
                        instanceRecord.UnloadTimer = new IOThreadTimer(this.cachedInstanceExpirationTimerCallback, instanceId, true);
                    }
                    instanceRecord.UnloadTimer.Set(this.cachedInstanceExpiration);
                }
            }
        }

        void CancelTimer(Guid instanceId, bool markActivationCompleted)
        {
            InstanceRecord instanceRecord;
            WaitCallback callback = null;
            object callbackState = null;

            lock (this.lockObject)
            {
                if (instanceRecordMap.TryGetValue(instanceId, out instanceRecord))
                {
                    if (instanceRecord.UnloadTimer != null)
                    {
                        instanceRecord.UnloadTimer.Cancel();
                        instanceRecord.UnloadTimer = null;
                    }

                    if (markActivationCompleted)
                    {
                        instanceRecordMap.Remove(instanceId);
                        callback = instanceRecord.Callback;
                        callbackState = instanceRecord.CallbackState;
                    }
                }
            }

            if (callback != null)
            {
                callback(callbackState);
            }
        }

        void OnTimer(object state)
        {
            Guid instanceId = (Guid) state;

            try
            {
                if (this.hasPersistenceService)
                {
                    this.workflowRuntime.GetWorkflow(instanceId).TryUnload();
                }
                else
                {
                    this.workflowRuntime.GetWorkflow(instanceId).Abort();

                    if (DiagnosticUtility.ShouldTraceInformation)
                    {
                        string traceText = SR2.GetString(SR2.AutoAbortingInactiveInstance, instanceId);
                        TraceUtility.TraceEvent(TraceEventType.Information,
                            TraceCode.WorkflowDurableInstanceAborted, SR.GetString(SR.TraceCodeWorkflowDurableInstanceAborted),
                            new StringTraceRecord("InstanceDetail", traceText),
                            this, null);
                    }
                }
            }
            catch (PersistenceException)
            {
                try
                {
                    this.workflowRuntime.GetWorkflow(instanceId).Abort();

                    if (DiagnosticUtility.ShouldTraceInformation)
                    {
                        string traceText = SR2.GetString(SR2.AutoAbortingInactiveInstance, instanceId);
                        TraceUtility.TraceEvent(TraceEventType.Information,
                            TraceCode.WorkflowDurableInstanceAborted, SR.GetString(SR.TraceCodeWorkflowDurableInstanceAborted),
                            new StringTraceRecord("InstanceDetail", traceText),
                            this, null);
                    }
                }
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                    {
                        throw;
                    }
                }
            }
            catch (Exception e)
            {
                if (Fx.IsFatal(e))
                {
                    throw;
                }
            }
        }

        void OnWorkflowAborted(object sender, WorkflowEventArgs args)
        {
            if (args == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("args");
            }

            CancelTimer(args.WorkflowInstance.InstanceId, true);
        }

        void OnWorkflowCompleted(object sender, WorkflowCompletedEventArgs args)
        {
            if (args == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("args");
            }

            CancelTimer(args.WorkflowInstance.InstanceId, true);
        }

        void OnWorkflowIdled(object sender, WorkflowEventArgs args)
        {
            if (args == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("args");
            }
            ScheduleTimer(args.WorkflowInstance.InstanceId);
        }

        void OnWorkflowLoaded(object sender, WorkflowEventArgs args)
        {
            if (args == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("args");
            }

            InstanceRecord instanceRecord;

            lock (this.lockObject)
            {
                if (!this.instanceRecordMap.TryGetValue(args.WorkflowInstance.InstanceId, out instanceRecord))
                {
                    instanceRecord = new InstanceRecord();
                    this.instanceRecordMap.Add(args.WorkflowInstance.InstanceId, instanceRecord);
                }
                instanceRecord.InstanceLoadedOrStarted = true;
            }
        }

        void OnWorkflowStarted(object sender, WorkflowEventArgs args)
        {
            if (args == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("args");
            }

            InstanceRecord instanceRecord;

            lock (this.lockObject)
            {
                if (!this.instanceRecordMap.TryGetValue(args.WorkflowInstance.InstanceId, out instanceRecord))
                {
                    instanceRecord = new InstanceRecord();
                    this.instanceRecordMap.Add(args.WorkflowInstance.InstanceId, instanceRecord);
                }
                instanceRecord.InstanceLoadedOrStarted = true;
            }
        }

        void OnWorkflowTerminated(object sender, WorkflowTerminatedEventArgs args)
        {
            if (args == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("args");
            }

            CancelTimer(args.WorkflowInstance.InstanceId, true);
        }

        void OnWorkflowUnloaded(object sender, WorkflowEventArgs args)
        {
            if (args == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("args");
            }

            CancelTimer(args.WorkflowInstance.InstanceId, true);
        }

        void RegisterEvents()
        {
            //Events marking the beggining of activation cycle.
            this.workflowRuntime.WorkflowLoaded += OnWorkflowLoaded;
            this.workflowRuntime.WorkflowCreated += OnWorkflowStarted;

            //Events marking the end of activation cycle.
            this.workflowRuntime.WorkflowAborted += OnWorkflowAborted;
            this.workflowRuntime.WorkflowCompleted += OnWorkflowCompleted;
            this.workflowRuntime.WorkflowTerminated += OnWorkflowTerminated;
            this.workflowRuntime.WorkflowUnloaded += OnWorkflowUnloaded;

            //Event which triggers the idle unload timer.
            this.workflowRuntime.WorkflowIdled += OnWorkflowIdled;
        }

        class InstanceRecord
        {
            object callbackState;
            WaitCallback instanceActivationCompletedCallBack;
            bool loadedOrStarted = false;
            IOThreadTimer unloadTimer;

            public WaitCallback Callback
            {
                get
                {
                    return this.instanceActivationCompletedCallBack;
                }
                set
                {
                    this.instanceActivationCompletedCallBack = value;
                }
            }

            public object CallbackState
            {
                get
                {
                    return this.callbackState;
                }
                set
                {
                    this.callbackState = value;
                }
            }

            public bool InstanceLoadedOrStarted
            {
                get
                {
                    return this.loadedOrStarted;
                }
                set
                {
                    this.loadedOrStarted = value;
                }
            }

            public IOThreadTimer UnloadTimer
            {
                get
                {
                    return this.unloadTimer;
                }
                set
                {
                    this.unloadTimer = value;
                }
            }
        }
    }
}
