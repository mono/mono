//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.Dispatcher
{
    using System.Diagnostics;
    using System.Reflection;
    using System.Runtime;
    using System.Security;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.ServiceModel.Diagnostics;
    using System.Workflow.Runtime;

    class WorkflowOperationInvoker : IOperationInvoker
    {
        static object[] emptyObjectArray = new object[0];
        bool canCreateInstance;
        DispatchRuntime dispatchRuntime;
        int inParameterCount;
        bool isOneWay;
        OperationDescription operationDescription;
        ServiceAuthorizationManager serviceAuthorizationManager;
        string staticQueueName;
        MethodInfo syncMethod;
        WorkflowInstanceLifetimeManagerExtension workflowInstanceLifeTimeManager;
        WorkflowRuntime workflowRuntime;

        public WorkflowOperationInvoker(OperationDescription operationDescription, WorkflowOperationBehavior workflowOperationBehavior,
            WorkflowRuntime workflowRuntime, DispatchRuntime dispatchRuntime)
        {
            if (operationDescription == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("operationDescription");
            }

            if (workflowRuntime == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("workflowRuntime");
            }

            if (workflowOperationBehavior == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("workflowOperationBehavior");
            }

            this.isOneWay = operationDescription.IsOneWay;

            if (operationDescription.BeginMethod != null)
            {
                this.syncMethod = operationDescription.BeginMethod;
                inParameterCount = GetFlowedInParameterCount(operationDescription.BeginMethod.GetParameters()) - 2;
            }
            else
            {
                this.syncMethod = operationDescription.SyncMethod;
                inParameterCount = GetFlowedInParameterCount(operationDescription.SyncMethod.GetParameters());
            }

            this.operationDescription = operationDescription;
            this.workflowRuntime = workflowRuntime;
            this.canCreateInstance = workflowOperationBehavior.CanCreateInstance;
            this.serviceAuthorizationManager = workflowOperationBehavior.ServiceAuthorizationManager;
            this.dispatchRuntime = dispatchRuntime;
            staticQueueName = QueueNameHelper.Create(this.syncMethod.DeclaringType, operationDescription.Name);
        }

        public bool CanCreateInstance
        {
            get { return this.canCreateInstance; }
        }

        public DispatchRuntime DispatchRuntime
        {
            get { return this.dispatchRuntime; }
        }

        public WorkflowInstanceLifetimeManagerExtension InstanceLifetimeManager
        {
            get
            {
                if (this.workflowInstanceLifeTimeManager == null)
                {
                    this.workflowInstanceLifeTimeManager = this.dispatchRuntime.ChannelDispatcher.Host.Extensions.Find<WorkflowInstanceLifetimeManagerExtension>();
                }

                return this.workflowInstanceLifeTimeManager;
            }
        }

        public bool IsOneWay
        {
            get { return this.isOneWay; }
        }

        public bool IsSynchronous
        {
            get { return false; }
        }

        public string StaticQueueName
        {
            get { return this.staticQueueName; }
        }

        public object[] AllocateInputs()
        {
            if (inParameterCount == 0)
            {
                return emptyObjectArray;
            }
            return new object[inParameterCount];
        }

        public object Invoke(object instance, object[] inputs, out object[] outputs)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotImplementedException());
        }

        public IAsyncResult InvokeBegin(object instance, object[] inputs, AsyncCallback callback, object state)
        {
            long beginTime = 0;

            if (PerformanceCounters.PerformanceCountersEnabled)
            {
                PerformanceCounters.MethodCalled(this.operationDescription.Name);

                try
                {
                    if (UnsafeNativeMethods.QueryPerformanceCounter(out beginTime) == 0)
                    {
                        beginTime = -1;
                    }
                }
                catch (SecurityException exception)
                {
                    DiagnosticUtility.TraceHandledException(exception, TraceEventType.Warning);
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityException(SR.GetString("PartialTrustPerformanceCountersNotEnabled"), exception));
                }
            }

            Authorize();

            WorkflowDurableInstance durableInstance = (WorkflowDurableInstance) instance;

            Fx.Assert(durableInstance.CurrentOperationInvocation == null,
                "At the time WorkflowOperationInvoker.InvokeBegin is called, the WorkflowDurableInstance.CurrentOperationInvocation is expected to be null given the ConcurrencyMode.Single.");

            durableInstance.CurrentOperationInvocation = new WorkflowOperationAsyncResult(
                this,
                durableInstance,
                inputs,
                callback,
                state,
                beginTime);

            return durableInstance.CurrentOperationInvocation;
        }

        public object InvokeEnd(object instance, out object[] outputs, IAsyncResult result)
        {
            bool methodThrewException = false;

            if (result == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("result");
            }

            WorkflowDurableInstance durableInstance = (WorkflowDurableInstance) instance;
            WorkflowOperationAsyncResult asyncResult = (WorkflowOperationAsyncResult) result;

            Fx.Assert(durableInstance.CurrentOperationInvocation != null,
                "At the time WorkflowOperationInvoker.InvokeEnd is called, the WorkflowDurableInstance.CurrentOperationInvocation is expected to be present.");

            try
            {
                return WorkflowOperationAsyncResult.End(asyncResult, out outputs);
            }
            catch (FaultException)
            {
                methodThrewException = true;
                if (PerformanceCounters.PerformanceCountersEnabled)
                {
                    PerformanceCounters.MethodReturnedFault(this.operationDescription.Name);
                }
                throw;
            }
            catch (Exception e)
            {
                methodThrewException = true;

                if (Fx.IsFatal(e))
                {
                    throw;
                }

                if (PerformanceCounters.PerformanceCountersEnabled)
                {
                    PerformanceCounters.MethodReturnedError(this.operationDescription.Name);
                }
                throw;
            }
            finally
            {
                durableInstance.CurrentOperationInvocation = null;

                if (!methodThrewException)
                {
                    if (PerformanceCounters.PerformanceCountersEnabled)
                    {
                        long currentTime = 0;
                        long duration = 0;

                        if ((asyncResult.BeginTime >= 0) && (UnsafeNativeMethods.QueryPerformanceCounter(out currentTime) != 0))
                        {
                            duration = currentTime - asyncResult.BeginTime;
                        }
                        PerformanceCounters.MethodReturnedSuccess(this.operationDescription.Name, duration);
                    }
                }
            }
        }

        static int GetFlowedInParameterCount(System.Reflection.ParameterInfo[] parameterInfos)
        {
            int inputCount = 0;

            foreach (System.Reflection.ParameterInfo parameterInfo in parameterInfos)
            {
                if (parameterInfo.IsOut)
                {
                    if (parameterInfo.IsIn)
                    {
                        ++inputCount;
                    }
                }
                else
                {
                    ++inputCount;
                }
            }
            return inputCount;
        }

        void Authorize()
        {
            Fx.Assert(OperationContext.Current != null, "Not in service dispatch thread");

            if (this.serviceAuthorizationManager != null)
            {
                if (!this.serviceAuthorizationManager.CheckAccess(OperationContext.Current))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(AuthorizationBehavior.CreateAccessDeniedFaultException());
                }
            }
        }
    }
}
