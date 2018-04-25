//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.Dispatcher
{
    using System.Workflow.Runtime;
    using System.Runtime.Diagnostics;
    using System.ServiceModel.Diagnostics;
    using System.Diagnostics;

    class WorkflowDurableInstance : DurableInstance
    {
        WorkflowOperationAsyncResult currentOperationInvocation;
        WorkflowInstanceContextProvider instanceContextProvider;
        bool shouldCreateNew = false;
        object thisLock = new object();
        WorkflowDefinitionContext workflowDefinition;
        WorkflowInstance workflowInstance = null;

        public WorkflowDurableInstance(WorkflowInstanceContextProvider instanceContextProvider, Guid instanceId, WorkflowDefinitionContext workflowDefinition, bool createNew)
            :
            base(instanceContextProvider, instanceId)
        {
            if (workflowDefinition == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("workflowDefinition");
            }

            this.workflowDefinition = workflowDefinition;
            this.shouldCreateNew = createNew;
            this.instanceContextProvider = instanceContextProvider;
        }

        public WorkflowOperationAsyncResult CurrentOperationInvocation
        {
            get
            {
                return this.currentOperationInvocation;
            }
            set
            {
                this.currentOperationInvocation = value;
            }
        }

        public WorkflowInstance GetWorkflowInstance(bool canCreateInstance)
        {
            if (this.workflowInstance == null)
            {
                lock (thisLock)
                {
                    if (shouldCreateNew)
                    {
                        if (canCreateInstance)
                        {
                            this.workflowInstance = this.workflowDefinition.CreateWorkflow(this.InstanceId);
                            shouldCreateNew = false;

                            if (DiagnosticUtility.ShouldTraceInformation)
                            {
                                string traceText = SR.GetString(SR.TraceCodeWorkflowDurableInstanceActivated, InstanceId);
                                TraceUtility.TraceEvent(TraceEventType.Information,
                                    TraceCode.WorkflowDurableInstanceActivated, traceText,
                                    new StringTraceRecord("DurableInstanceDetail", traceText),
                                    this, null);
                            }
                            using (new WorkflowDispatchContext(true, true))
                            {
                                this.workflowInstance.Start();
                            }
                        }
                        else
                        {
                            //Make sure we clean up this InstanceContext;
                            DurableErrorHandler.CleanUpInstanceContextAtOperationCompletion();
                            //Inform InstanceLifeTimeManager to clean up record for InstanceId;
                            if (this.instanceContextProvider.InstanceLifeTimeManager != null)
                            {
                                this.instanceContextProvider.InstanceLifeTimeManager.CleanUp(this.InstanceId);
                            }
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new FaultException(new DurableDispatcherAddressingFault()));
                        }
                    }
                    else
                    {
                        this.workflowInstance = this.workflowDefinition.WorkflowRuntime.GetWorkflow(InstanceId);

                        if (DiagnosticUtility.ShouldTraceInformation)
                        {
                            string traceText = SR.GetString(SR.TraceCodeWorkflowDurableInstanceLoaded, InstanceId);
                            TraceUtility.TraceEvent(TraceEventType.Information,
                                TraceCode.WorkflowDurableInstanceLoaded, traceText,
                                new StringTraceRecord("DurableInstanceDetail", traceText),
                                this, null);
                        }
                    }
                }
            }
            return workflowInstance;
        }
    }
}
