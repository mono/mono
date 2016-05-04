//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.Dispatcher
{
    using System.Runtime;
    using System.Runtime.Diagnostics;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Diagnostics;
    using System.Threading;
    using System.Workflow.Runtime;
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics;

    class WorkflowInstanceContextProvider : DurableInstanceContextProvider
    {
        bool hasCheckedForExtension;
        WorkflowInstanceLifetimeManagerExtension instanceLifeTimeManager;
        ServiceHostBase serviceHostBase;
        WaitCallback workflowActivationCompleteCallback;
        WorkflowDefinitionContext workflowDefinitionContext;


        public WorkflowInstanceContextProvider(ServiceHostBase serviceHostBase, bool isPerCall, WorkflowDefinitionContext workflowDefinitionContext)
            : base(serviceHostBase, isPerCall)
        {
            if (workflowDefinitionContext == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("workflowDefinitionContext");
            }

            this.workflowDefinitionContext = workflowDefinitionContext;
            this.serviceHostBase = serviceHostBase;
            this.workflowActivationCompleteCallback = Fx.ThunkCallback(new WaitCallback(this.OnWorkflowActivationCompleted));
        }

        public WorkflowInstanceLifetimeManagerExtension InstanceLifeTimeManager
        {
            get
            {
                if (!hasCheckedForExtension)
                {
                    this.instanceLifeTimeManager = this.serviceHostBase.Extensions.Find<WorkflowInstanceLifetimeManagerExtension>();
                    hasCheckedForExtension = true;
                }
                return this.instanceLifeTimeManager;
            }
        }

        public override InstanceContext GetExistingInstanceContext(Message message, IContextChannel channel)
        {
            InstanceContext instanceContext = base.GetExistingInstanceContext(message, channel);

            if (instanceContext != null && this.InstanceLifeTimeManager != null)
            {
                WorkflowDurableInstance workflowDurableInstance = instanceContext.Extensions.Find<WorkflowDurableInstance>();

                if (workflowDurableInstance == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                        new InvalidOperationException(
                        SR2.GetString(
                        SR2.RequiredInstanceContextExtensionNotFound,
                        typeof(WorkflowDurableInstance).Name)));
                }

                this.InstanceLifeTimeManager.NotifyWorkflowActivationComplete(
                    workflowDurableInstance.InstanceId,
                    this.workflowActivationCompleteCallback,
                    new WorkflowActivationCompletedCallbackState
                    (
                    workflowDurableInstance.InstanceId,
                    instanceContext),
                    false);
            }

            return instanceContext;
        }


        public override void InitializeInstanceContext(InstanceContext instanceContext, Message message, IContextChannel channel)
        {
            base.InitializeInstanceContext(instanceContext, message, channel);

            WorkflowDurableInstance workflowDurableInstance = instanceContext.Extensions.Find<WorkflowDurableInstance>();

            if (workflowDurableInstance == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new InvalidOperationException(
                    SR2.GetString(
                    SR2.RequiredInstanceContextExtensionNotFound,
                    typeof(WorkflowDurableInstance).Name)));
            }

            if (this.InstanceLifeTimeManager != null)
            {
                this.InstanceLifeTimeManager.NotifyWorkflowActivationComplete(workflowDurableInstance.InstanceId,
                    this.workflowActivationCompleteCallback,
                    new WorkflowActivationCompletedCallbackState
                    (workflowDurableInstance.InstanceId, instanceContext),
                    false);
            }
        }

        public override bool IsIdle(InstanceContext instanceContext)
        {
            if (instanceContext == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("instanceContext");
            }

            WorkflowDurableInstance workflowDurableInstance = instanceContext.Extensions.Find<WorkflowDurableInstance>();

            if (workflowDurableInstance == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new InvalidOperationException(
                    SR2.GetString(
                    SR2.RequiredInstanceContextExtensionNotFound,
                    typeof(WorkflowDurableInstance).Name)));
            }

            if (this.InstanceLifeTimeManager != null)
            {
                return (!this.InstanceLifeTimeManager.IsInstanceInMemory(workflowDurableInstance.InstanceId)) &&
                    base.IsIdle(instanceContext);
            }
            return base.IsIdle(instanceContext);
        }

        public override void NotifyIdle(InstanceContextIdleCallback callback, InstanceContext instanceContext)
        {
            WorkflowDurableInstance workflowDurableInstance = instanceContext.Extensions.Find<WorkflowDurableInstance>();

            if (workflowDurableInstance == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new InvalidOperationException(
                    SR2.GetString(
                    SR2.RequiredInstanceContextExtensionNotFound,
                    typeof(WorkflowDurableInstance).Name)));
            }

            if (this.InstanceLifeTimeManager != null)
            {
                if (this.InstanceLifeTimeManager.IsInstanceInMemory(workflowDurableInstance.InstanceId))
                {
                    this.InstanceLifeTimeManager.NotifyWorkflowActivationComplete(workflowDurableInstance.InstanceId,
                        Fx.ThunkCallback(new WaitCallback(this.OnWorkflowActivationCompleted)),
                        new WorkflowActivationCompletedCallbackState
                        (
                        workflowDurableInstance.InstanceId,
                        instanceContext,
                        callback),
                        true);
                }
                else
                {
                    if (base.IsIdle(instanceContext))
                    {
                        callback(instanceContext);
                    }
                    else
                    {
                        base.NotifyIdle(callback, instanceContext);
                    }
                }
            }
            else
            {
                base.NotifyIdle(callback, instanceContext);
            }
        }

        protected override DurableInstance OnCreateNewInstance(Guid instanceId)
        {
            if (DiagnosticUtility.ShouldTraceInformation)
            {
                string traceText = SR2.GetString(SR2.InstanceContextProviderCreatedNewInstance, "Workflow", instanceId);
                TraceUtility.TraceEvent(TraceEventType.Information,
                    TraceCode.ActivatingMessageReceived, SR.GetString(SR.TraceCodeActivatingMessageReceived),
                    new StringTraceRecord("NewInstanceDetail", traceText),
                    this, null);
            }

            return new WorkflowDurableInstance(this, instanceId, this.workflowDefinitionContext, true);
        }

        protected override DurableInstance OnGetExistingInstance(Guid instanceId)
        {
            return new WorkflowDurableInstance(this, instanceId, this.workflowDefinitionContext, false);
        }

        void OnWorkflowActivationCompleted(object state)
        {
            WorkflowActivationCompletedCallbackState callbackState = (WorkflowActivationCompletedCallbackState) state;

            lock (callbackState.InstanceContext.ThisLock)
            {
                if (base.Cache.Contains(callbackState.InstanceId, callbackState.InstanceContext))
                {
                    WorkflowDurableInstance durableInstance = callbackState.InstanceContext.Extensions.Find<WorkflowDurableInstance>();
                    if (durableInstance != null
                        && durableInstance.CurrentOperationInvocation != null
                        && durableInstance.CurrentOperationInvocation.HasWorkflowRequestContextBeenSerialized
                        && !durableInstance.CurrentOperationInvocation.IsCompleted)
                    {
                        // If we are here, it means the workflow instance completed, terminated, or otherwise unloaded without
                        // completing the current operation invocation. In such case, we want to make the best effort to let
                        // service model to consider this operation invocation failed. 
                        try
                        {
                            durableInstance.CurrentOperationInvocation.SendFault(
                                WorkflowOperationErrorHandler.CreateUnhandledException(
                                new InvalidOperationException(SR2.GetString(SR2.WorkflowServiceUnloadedWithoutSendingResponse))),
                                null);
                        }
                        catch (Exception e)
                        {
                            if (Fx.IsFatal(e))
                            {
                                throw;
                            }
                        }
                    }

                    IChannel[] incomingChannels = new IChannel[callbackState.InstanceContext.IncomingChannels.Count];
                    callbackState.InstanceContext.IncomingChannels.CopyTo(incomingChannels, 0);

                    if (callbackState.InstanceContext.IncomingChannels.Count != 0)
                    {
                        foreach (IChannel channel in incomingChannels)
                        {
                            callbackState.InstanceContext.IncomingChannels.Remove(channel);
                        }
                    }
                    else
                    {
                        //Call notify only when IncomingChannels Collection is empty.
                        if (callbackState.InstanceContextIdleCallback != null)
                        {
                            callbackState.InstanceContextIdleCallback(callbackState.InstanceContext);
                        }
                    }
                }
            }
        }

        class WorkflowActivationCompletedCallbackState
        {
            InstanceContext instanceContext;
            InstanceContextIdleCallback instanceContextIdleCallback;
            Guid instanceId;

            public WorkflowActivationCompletedCallbackState(Guid instanceId, InstanceContext instanceContext)
                : this(instanceId, instanceContext, null)
            {

            }

            public WorkflowActivationCompletedCallbackState(Guid instanceId, InstanceContext instanceContext, InstanceContextIdleCallback callback)
            {
                this.instanceId = instanceId;
                this.instanceContext = instanceContext;
                this.instanceContextIdleCallback = callback;
            }


            public InstanceContext InstanceContext
            {
                get
                {
                    return this.instanceContext;
                }
            }

            public InstanceContextIdleCallback InstanceContextIdleCallback
            {
                get
                {
                    return this.instanceContextIdleCallback;
                }
            }

            public Guid InstanceId
            {
                get
                {
                    return this.instanceId;
                }
            }
        }
    }
}
