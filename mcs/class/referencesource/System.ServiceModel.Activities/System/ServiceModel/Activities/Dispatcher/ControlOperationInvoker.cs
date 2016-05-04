//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Activities.Dispatcher
{
    using System.Activities;
    using System.Activities.DynamicUpdate;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Runtime;
    using System.Runtime.DurableInstancing;
    using System.ServiceModel.Activities;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.ServiceModel.Dispatcher;
    using System.Transactions;
    using System.Xml.Linq;

    //This will be the top most invoker for all endpoint operations for durable services.
    //It operates in 3 modes.
    //1) IWorkflowInstanceManagement endpoint operation(completely implemented by this type)
    //2) Application endpoint(Modelled Receive/DurableService) Custom invoker inherited from this type overriding OnBegin/End ServiceOperation.
    //3) Durable Standard Endpoint(LRCS/Interop/Delay) eventual dispatch call will be delegated to innerInvoker with DurableInstanceContext as service object.

    //This class is single point of interaction with DurableInstanceManagement and ensures appropriate DurableInstance for the request, based on the mode of operation
    //message will be dispatched to various part of DurableInstance.
    class ControlOperationInvoker : IManualConcurrencyOperationInvoker
    {
        protected static readonly object[] emptyObjectArray = new object[0];

        readonly DurableInstanceManager instanceManager;
        readonly string operationName;
        readonly bool isOneWay;
        protected readonly ServiceEndpoint endpoint;
        readonly int inputParameterCount;

        readonly IOperationInvoker innerInvoker;
        readonly bool isControlOperation;
        readonly CorrelationKeyCalculator keyCalculator;

        readonly WorkflowServiceHost host;
        readonly BufferedReceiveManager bufferedReceiveManager;
        readonly TimeSpan persistTimeout;

        public ControlOperationInvoker(OperationDescription description, ServiceEndpoint endpoint,
            CorrelationKeyCalculator correlationKeyCalculator, ServiceHostBase host)
            : this(description, endpoint, correlationKeyCalculator, null, host)
        {
        }

        public ControlOperationInvoker(OperationDescription description, ServiceEndpoint endpoint,
            CorrelationKeyCalculator correlationKeyCalculator, IOperationInvoker innerInvoker, ServiceHostBase host)
        {
            Fx.Assert(host is WorkflowServiceHost, "ControlOperationInvoker must be used with a WorkflowServiceHost");

            this.host = (WorkflowServiceHost)host;
            this.instanceManager = this.host.DurableInstanceManager;
            this.operationName = description.Name;
            this.isOneWay = description.IsOneWay;
            this.endpoint = endpoint;
            this.innerInvoker = innerInvoker;
            this.keyCalculator = correlationKeyCalculator;
            this.persistTimeout = this.host.PersistTimeout;

            if (description.DeclaringContract == WorkflowControlEndpoint.WorkflowControlServiceContract ||
                description.DeclaringContract == WorkflowControlEndpoint.WorkflowControlServiceBaseContract)
            {
                //Mode1: This invoker belongs to IWorkflowInstanceManagement operation.
                this.isControlOperation = true;
                switch (this.operationName)
                {
                    case XD2.WorkflowInstanceManagementService.Cancel:
                    case XD2.WorkflowInstanceManagementService.TransactedCancel:
                    case XD2.WorkflowInstanceManagementService.Run:
                    case XD2.WorkflowInstanceManagementService.TransactedRun:
                    case XD2.WorkflowInstanceManagementService.Unsuspend:
                    case XD2.WorkflowInstanceManagementService.TransactedUnsuspend:
                        this.inputParameterCount = 1;
                        break;
                    case XD2.WorkflowInstanceManagementService.Abandon:
                    case XD2.WorkflowInstanceManagementService.Suspend:
                    case XD2.WorkflowInstanceManagementService.TransactedSuspend:
                    case XD2.WorkflowInstanceManagementService.Terminate:
                    case XD2.WorkflowInstanceManagementService.TransactedTerminate:
                    case XD2.WorkflowInstanceManagementService.Update:
                    case XD2.WorkflowInstanceManagementService.TransactedUpdate:
                        this.inputParameterCount = 2;
                        break;
                    default:
                        throw Fx.AssertAndThrow("Unreachable code");
                }
            }
            else if (endpoint is WorkflowHostingEndpoint)
            {
                this.CanCreateInstance = true;
            }
            else
            {
                this.bufferedReceiveManager = this.host.Extensions.Find<BufferedReceiveManager>();
            }
        }

        public bool IsSynchronous { get { return false; } }

        protected bool CanCreateInstance { get; set; }
        protected string StaticBookmarkName { get; set; }

        protected string OperationName
        {
            get { return this.operationName; }
        }

        public BufferedReceiveManager BufferedReceiveManager
        {
            get { return this.bufferedReceiveManager; }
        }

        public DurableInstanceManager InstanceManager
        {
            get { return this.instanceManager; }
        }

        public virtual object[] AllocateInputs()
        {
            if (this.innerInvoker != null) //Mode 3: Delegate call to innerInvoker.
            {
                return this.innerInvoker.AllocateInputs();
            }
            else if (this.isControlOperation) //Mode 1
            {
                if (this.inputParameterCount == 0)
                {
                    return emptyObjectArray;
                }
                else
                {
                    return new object[this.inputParameterCount];
                }
            }
            //Mode 2: Derived invoker should ensure appropriate in parameter count based on its contract.
            throw Fx.AssertAndThrow("Derived invoker should have handled this case");
        }

        // We own the formatter only if the message is oneway and is used 
        // to detemine if the message needs to be disposed or not.
        bool IManualConcurrencyOperationInvoker.OwnsFormatter
        {
            get { return this.isOneWay; }
        }

        public object Invoke(object instance, object[] inputs, out object[] outputs)
        {
            return Invoke(instance, inputs, null, out outputs);
        }

        public object Invoke(object instance, object[] inputs, IInvokeReceivedNotification notification, out object[] outputs)
        {
            throw FxTrace.Exception.AsError(new NotImplementedException());
        }

        public IAsyncResult InvokeBegin(object instance, object[] inputs, AsyncCallback callback, object state)
        {
            return InvokeBegin(instance, inputs, null, callback, state);
        }

        public IAsyncResult InvokeBegin(object instance, object[] inputs, IInvokeReceivedNotification notification, AsyncCallback callback, object state)
        {
            if (inputs == null)
            {
                throw FxTrace.Exception.ArgumentNull("inputs");
            }

            //Fetch Instance and Dispatch.
            return new ControlOperationAsyncResult(this, inputs, notification, TimeSpan.MaxValue, callback, state);
        }

        public object InvokeEnd(object instance, out object[] outputs, IAsyncResult result)
        {
            return ControlOperationAsyncResult.End(out outputs, result);
        }

        //This is the dispatch call for Non-IWorkflowInstanceManagement Operations.
        protected virtual IAsyncResult OnBeginServiceOperation(WorkflowServiceInstance durableInstance,
            OperationContext operationContext, object[] inputs, Transaction currentTransaction, IInvokeReceivedNotification notification,
            TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new ServiceOperationAsyncResult(this.innerInvoker, durableInstance, inputs, operationContext, currentTransaction, notification,
                callback, state);
        }
        protected virtual object OnEndServiceOperation(WorkflowServiceInstance durableInstance, out object[] outputs, IAsyncResult result)
        {
            return ServiceOperationAsyncResult.End(out outputs, result);
        }


        // pass OperationContext.Current if you don't already have an OperationContext.
        protected void GetInstanceKeys(OperationContext operationContext, out InstanceKey instanceKey, out ICollection<InstanceKey> additionalKeys)
        {
            CorrelationMessageProperty correlationMessageProperty = null;
            InstanceKey localInstanceKey;
            ICollection<InstanceKey> localAdditionalKeys;

            instanceKey = InstanceKey.InvalidKey;
            additionalKeys = new ReadOnlyCollection<InstanceKey>(new InstanceKey[] { });

            if (!CorrelationMessageProperty.TryGet(operationContext.IncomingMessageProperties,
                out correlationMessageProperty))
            {
                if (this.keyCalculator != null)
                {
                    MessageBuffer requestMessageBuffer;
                    bool success;

                    if (operationContext.IncomingMessageProperties.TryGetValue(ChannelHandler.MessageBufferPropertyName, out requestMessageBuffer))
                    {
                        success = this.keyCalculator.CalculateKeys(requestMessageBuffer, operationContext.IncomingMessage, out localInstanceKey, out localAdditionalKeys);
                    }
                    else
                    {
                        // Message is not preserved.(DispatchRuntime.PreserveMessage is false.)
                        // this could be a case where we only have context queries, in this case we don't preserve the message
                        success = this.keyCalculator.CalculateKeys(operationContext.IncomingMessage, out localInstanceKey, out localAdditionalKeys);
                    }
                    if (success)
                    {
                        if (localInstanceKey != null)
                        {
                            instanceKey = localInstanceKey;
                        }
                        if (localAdditionalKeys != null)
                        {
                            additionalKeys = localAdditionalKeys;
                        }

                        correlationMessageProperty = new CorrelationMessageProperty(instanceKey, additionalKeys);

                        operationContext.IncomingMessageProperties.Add(CorrelationMessageProperty.Name, correlationMessageProperty);
                    }
                }
            }
            else
            {
                instanceKey = correlationMessageProperty.CorrelationKey;
                additionalKeys = correlationMessageProperty.AdditionalKeys;
            }

            //If InstanceKey is still not resolved do the activation operation validation.
            if (instanceKey == null || !instanceKey.IsValid)
            {
                if (!this.CanCreateInstance)
                {
                    this.host.RaiseUnknownMessageReceived(operationContext.IncomingMessage);

                    throw FxTrace.Exception.AsError(
                        new FaultException(
                        new DurableDispatcherAddressingFault()));
                }
            }
        }


        class ControlOperationAsyncResult : AsyncResult
        {
            static AsyncCompletion handleEndGetInstance = new AsyncCompletion(HandleEndGetInstance);
            static AsyncCompletion handleEndOperation = new AsyncCompletion(HandleEndOperation);
            static AsyncCompletion handleEndAbandonReceiveContext;
            static ReadOnlyCollection<InstanceKey> emptyKeyCollection = new ReadOnlyCollection<InstanceKey>(new InstanceKey[] { });
            static Action<AsyncResult, Exception> onCompleting = new Action<AsyncResult, Exception>(Finally);

            object[] inputs;
            TimeoutHelper timeoutHelper;

            Guid instanceId;
            WorkflowIdentityKey updatedIdentity;
            InstanceKey instanceKey = InstanceKey.InvalidKey;
            ICollection<InstanceKey> additionalKeys = emptyKeyCollection;

            WorkflowServiceInstance workflowServiceInstance;
            Transaction transaction;
            ReceiveContext receiveContext;
            Exception operationException;

            object returnValue;
            OperationContext operationContext;

            ControlOperationInvoker invoker;
            IInvokeReceivedNotification notification;

            object[] outputs = emptyObjectArray;

            WorkflowGetInstanceContext getInstanceContext;

            public ControlOperationAsyncResult(ControlOperationInvoker invoker, object[] inputs, IInvokeReceivedNotification notification, TimeSpan timeout,
                AsyncCallback callback, object state)
                : base(callback, state)
            {
                this.invoker = invoker;
                this.inputs = inputs;
                this.timeoutHelper = new TimeoutHelper(timeout);
                this.transaction = Transaction.Current;
                this.operationContext = OperationContext.Current;
                this.OnCompleting = onCompleting;

                bool completeSelf = false;
                bool success = false;
                try
                {
                    if (notification != null)
                    {
                        if (this.operationContext.SessionId == null)
                        {
                            // Datagram messages are completely concurrent to loadOrCreate and instance operations. Same as WCF's ConcurrencyMode.Single
                            notification.NotifyInvokeReceived();
                        }
                        else
                        {
                            // For session, we will notify after we enter into WorkflowServiceInstance pending queue.
                            // This achieves synchronization and ordered messages within a session and concurrent across distinct sessions.
                            this.notification = notification;
                        }
                    }

                    if (invoker.BufferedReceiveManager != null)
                    {
                        if (!ReceiveContext.TryGet(this.operationContext.IncomingMessageProperties, out this.receiveContext))
                        {
                            Fx.Assert("ReceiveContext expected when BufferedReceives are enabled");
                        }
                    }

                    completeSelf = this.Process();
                    success = true;
                }
                finally
                {
                    // in the success cases, OnCompleting has us covered
                    if (!success)
                    {
                        Finally(this, null);
                    }
                }

                if (completeSelf)
                {
                    this.Complete(true);
                }
            }

            static void Finally(AsyncResult result, Exception completionException)
            {
                ControlOperationAsyncResult thisPtr = (ControlOperationAsyncResult)result;
                if (thisPtr.workflowServiceInstance != null)
                {
                    thisPtr.workflowServiceInstance.ReleaseReference();
                    thisPtr.workflowServiceInstance = null;
                }
                if (completionException != null)
                {
                    thisPtr.invoker.host.FaultServiceHostIfNecessary(completionException);
                }
            }

            public static object End(out object[] outputs, IAsyncResult result)
            {
                ControlOperationAsyncResult thisPtr = AsyncResult.End<ControlOperationAsyncResult>(result);
                outputs = thisPtr.outputs;
                return thisPtr.returnValue;
            }

            bool Process()
            {
                EnsureInstanceIdAndIdentity();

                this.getInstanceContext = new WorkflowGetInstanceContext
                {
                    WorkflowHostingEndpoint = this.invoker.endpoint as WorkflowHostingEndpoint,
                    CanCreateInstance = this.invoker.CanCreateInstance,
                    Inputs = this.inputs,
                    OperationContext = this.operationContext,
                };

                IAsyncResult result;

                bool shouldAbandon = true;
                try
                {
                    try
                    {
                        if (!this.invoker.isControlOperation && this.instanceKey != null && this.instanceKey.IsValid)
                        {
                            result = this.invoker.instanceManager.BeginGetInstance(this.instanceKey, this.additionalKeys, this.getInstanceContext,
                                this.invoker.persistTimeout, this.PrepareAsyncCompletion(handleEndGetInstance), this);
                        }
                        else
                        {
                            result = this.invoker.instanceManager.BeginGetInstance(this.instanceId, this.getInstanceContext, this.updatedIdentity,
                                this.invoker.persistTimeout, this.PrepareAsyncCompletion(handleEndGetInstance), this);
                        }
                        shouldAbandon = false;
                    }
                    catch (InstanceLockedException exception)
                    {
                        RedirectionException redirectionException;
                        if (TryCreateRedirectionException(exception, out redirectionException))
                        {
                            throw FxTrace.Exception.AsError(redirectionException);
                        }
                        throw FxTrace.Exception.AsError(CreateFaultException(exception));
                    }
                    catch (OperationCanceledException exception)
                    {
                        BufferReceiveHelper(ref shouldAbandon, true);
                        throw FxTrace.Exception.AsError(new RetryException(null, exception));
                    }
                    catch (InstancePersistenceException exception)
                    {
                        BufferReceiveHelper(ref shouldAbandon, false);

                        if (exception is InstanceKeyNotReadyException)
                        {
                            this.invoker.host.RaiseUnknownMessageReceived(this.operationContext.IncomingMessage);
                        }

                        this.invoker.host.FaultServiceHostIfNecessary(exception);

                        throw FxTrace.Exception.AsError(CreateFaultException(exception));
                    }
                    catch (InstanceUpdateException)
                    {
                        throw FxTrace.Exception.AsError(new FaultException(OperationExecutionFault.CreateUpdateFailedFault(
                            SR.WorkflowInstanceUpdateFailed(this.instanceId, this.updatedIdentity.Identity))));
                    }
                }
                catch (Exception exception)
                {
                    if (Fx.IsFatal(exception))
                    {
                        throw;
                    }
                    if (!shouldAbandon || !ShouldAbandonReceiveContext())
                    {
                        throw;
                    }
                    return AbandonReceiveContext(exception);
                }

                if (result.CompletedSynchronously)
                {
                    return HandleEndGetInstance(result);
                }
                return false;
            }

            void BufferReceiveHelper(ref bool shouldAbandon, bool retry)
            {
                if (this.invoker.BufferedReceiveManager != null)
                {
                    Fx.Assert(this.receiveContext != null, "receiveContext must not be null!");
                    bool bufferSuccess = this.invoker.BufferedReceiveManager.BufferReceive(
                        this.operationContext, this.receiveContext, this.invoker.StaticBookmarkName, BufferedReceiveState.WaitingOnInstance, retry);
                    if (bufferSuccess)
                    {
                        if (TD.BufferOutOfOrderMessageNoInstanceIsEnabled())
                        {
                            TD.BufferOutOfOrderMessageNoInstance(this.invoker.StaticBookmarkName);
                        }

                        shouldAbandon = false;
                    }
                }
            }

            static bool HandleEndGetInstance(IAsyncResult result)
            {
                ControlOperationAsyncResult thisPtr = (ControlOperationAsyncResult)result.AsyncState;

                bool shouldAbandon = true;
                try
                {
                    try
                    {
                        thisPtr.workflowServiceInstance = thisPtr.invoker.instanceManager.EndGetInstance(result);
                        shouldAbandon = false;
                    }
                    catch (InstanceLockedException exception)
                    {
                        RedirectionException redirectionException;
                        if (thisPtr.TryCreateRedirectionException(exception, out redirectionException))
                        {
                            throw FxTrace.Exception.AsError(redirectionException);
                        }
                        throw FxTrace.Exception.AsError(CreateFaultException(exception));
                    }
                    catch (OperationCanceledException exception)
                    {
                        thisPtr.BufferReceiveHelper(ref shouldAbandon, true);
                        throw FxTrace.Exception.AsError(new RetryException(null, exception));
                    }
                    catch (InstancePersistenceException exception)
                    {
                        thisPtr.BufferReceiveHelper(ref shouldAbandon, false);

                        if (exception is InstanceKeyNotReadyException)
                        {
                            thisPtr.invoker.host.RaiseUnknownMessageReceived(thisPtr.operationContext.IncomingMessage);
                        }

                        thisPtr.invoker.host.FaultServiceHostIfNecessary(exception);

                        throw FxTrace.Exception.AsError(CreateFaultException(exception));
                    }
                    catch (InstanceUpdateException)
                    {
                        throw FxTrace.Exception.AsError(new FaultException(OperationExecutionFault.CreateUpdateFailedFault(
                            SR.WorkflowInstanceUpdateFailed(thisPtr.instanceId, thisPtr.updatedIdentity.Identity))));
                    }
                }
                catch (Exception exception)
                {
                    if (Fx.IsFatal(exception))
                    {
                        throw;
                    }
                    if (!shouldAbandon || !thisPtr.ShouldAbandonReceiveContext())
                    {
                        throw;
                    }
                    return thisPtr.AbandonReceiveContext(exception);
                }

                // When creating a new instance for a normal, keyless app message, create a key for use by context exchange.
                if (!thisPtr.instanceKey.IsValid && thisPtr.instanceId == Guid.Empty)
                {
                    ContextMessageProperty outgoingContextMessageProperty = null;

                    if (!ContextMessageProperty.TryGet(thisPtr.operationContext.OutgoingMessageProperties, out outgoingContextMessageProperty))
                    {
                        outgoingContextMessageProperty = new ContextMessageProperty();
                        outgoingContextMessageProperty.Context.Add(ContextMessageProperty.InstanceIdKey, Guid.NewGuid().ToString());
                        outgoingContextMessageProperty.AddOrReplaceInMessageProperties(thisPtr.operationContext.OutgoingMessageProperties);
                    }
                    else
                    {
                        outgoingContextMessageProperty.Context[ContextMessageProperty.InstanceIdKey] = Guid.NewGuid().ToString();
                    }
                }
                return thisPtr.PerformOperation();
            }

            bool PerformOperation()
            {
                IAsyncResult result = null;
                bool completed = false;

                if (this.invoker.isControlOperation)
                {
                    //Mode 1: Dispatch directly to WorkflowServiceInstance method.
                    switch (this.invoker.operationName)
                    {
                        case XD2.WorkflowInstanceManagementService.Suspend:
                        case XD2.WorkflowInstanceManagementService.TransactedSuspend:
                            result = this.workflowServiceInstance.BeginSuspend(false, (string)this.inputs[1] ?? SR.DefaultSuspendReason,
                                this.transaction, this.timeoutHelper.RemainingTime(),
                                this.PrepareAsyncCompletion(handleEndOperation), this);
                            break;
                        case XD2.WorkflowInstanceManagementService.Unsuspend:
                        case XD2.WorkflowInstanceManagementService.TransactedUnsuspend:
                            result = this.workflowServiceInstance.BeginUnsuspend(this.transaction, this.timeoutHelper.RemainingTime(),
                                this.PrepareAsyncCompletion(handleEndOperation), this);
                            break;
                        case XD2.WorkflowInstanceManagementService.Terminate:
                        case XD2.WorkflowInstanceManagementService.TransactedTerminate:
                            result = this.workflowServiceInstance.BeginTerminate((string)this.inputs[1] ?? SR.DefaultTerminationReason,
                                this.transaction, this.timeoutHelper.RemainingTime(),
                                this.PrepareAsyncCompletion(handleEndOperation), this);
                            break;
                        case XD2.WorkflowInstanceManagementService.Run:
                        case XD2.WorkflowInstanceManagementService.TransactedRun:
                            result = this.workflowServiceInstance.BeginRun(this.transaction, this.timeoutHelper.RemainingTime(),
                                this.PrepareAsyncCompletion(handleEndOperation), this);
                            break;
                        case XD2.WorkflowInstanceManagementService.Cancel:
                        case XD2.WorkflowInstanceManagementService.TransactedCancel:
                            result = this.workflowServiceInstance.BeginCancel(this.transaction,
                                this.timeoutHelper.RemainingTime(), this.PrepareAsyncCompletion(handleEndOperation), this);
                            break;
                        case XD2.WorkflowInstanceManagementService.Abandon:
                            string reason = (string)this.inputs[1];
                            result = this.workflowServiceInstance.BeginAbandon(
                                new WorkflowApplicationAbortedException(!String.IsNullOrEmpty(reason) ? reason : SR.DefaultAbortReason),
                                this.timeoutHelper.RemainingTime(), this.PrepareAsyncCompletion(handleEndOperation), this);
                            break;
                        case XD2.WorkflowInstanceManagementService.Update:
                        case XD2.WorkflowInstanceManagementService.TransactedUpdate:
                            WorkflowIdentity identity = (WorkflowIdentity)this.inputs[1];
                            if (!object.Equals(identity, this.workflowServiceInstance.DefinitionIdentity))
                            {
                                throw FxTrace.Exception.AsError(new FaultException(
                                    OperationExecutionFault.CreateUpdateFailedFault(SR.CannotUpdateLoadedInstance(this.workflowServiceInstance.Id))));
                            }
                            if (this.workflowServiceInstance.IsActive)
                            {
                                result = this.workflowServiceInstance.BeginRun(this.transaction, this.invoker.operationName, this.timeoutHelper.RemainingTime(),
                                    this.PrepareAsyncCompletion(handleEndOperation), this);
                            }
                            else
                            {
                                result = new CompletedAsyncResult(this.PrepareAsyncCompletion(handleEndOperation), this);
                            }
                            break;
                        default:
                            throw Fx.AssertAndThrow("Unreachable code");

                    }
                    if (this.notification != null)
                    {
                        this.notification.NotifyInvokeReceived();
                    }
                }
                else if (this.getInstanceContext.WorkflowCreationContext != null)
                {
                    result = BeginRunAndGetResponse(timeoutHelper, this.PrepareAsyncCompletion(handleEndOperation), this);
                    if (this.notification != null)
                    {
                        this.notification.NotifyInvokeReceived();
                    }
                }
                else
                {
                    try
                    {
                        //User Endpoint operation.
                        result = this.invoker.OnBeginServiceOperation(this.workflowServiceInstance, this.operationContext,
                            this.inputs, this.transaction, this.notification, this.timeoutHelper.RemainingTime(),
                            this.PrepareAsyncCompletion(handleEndOperation), this);
                    }
                    catch (FaultException)
                    {
                        throw; // ReceiveContext was handled appropriately by WorkflowOperationContext
                    }
                    catch (Exception exception)
                    {
                        if (Fx.IsFatal(exception))
                        {
                            throw;
                        }
                        if (!ShouldAbandonReceiveContext())
                        {
                            throw;
                        }
                        return AbandonReceiveContext(exception);
                    }
                }

                if (result != null && result.CompletedSynchronously)
                {
                    completed = HandleEndOperation(result);
                }

                return completed;
            }

            static bool HandleEndOperation(IAsyncResult result)
            {
                ControlOperationAsyncResult thisPtr = (ControlOperationAsyncResult)result.AsyncState;

                if (thisPtr.invoker.isControlOperation)
                {
                    switch (thisPtr.invoker.operationName)
                    {
                        case XD2.WorkflowInstanceManagementService.Suspend:
                        case XD2.WorkflowInstanceManagementService.TransactedSuspend:
                            thisPtr.workflowServiceInstance.EndSuspend(result);
                            break;
                        case XD2.WorkflowInstanceManagementService.Unsuspend:
                        case XD2.WorkflowInstanceManagementService.TransactedUnsuspend:
                            thisPtr.workflowServiceInstance.EndUnsuspend(result);
                            break;
                        case XD2.WorkflowInstanceManagementService.Terminate:
                        case XD2.WorkflowInstanceManagementService.TransactedTerminate:
                            thisPtr.workflowServiceInstance.EndTerminate(result);
                            break;
                        case XD2.WorkflowInstanceManagementService.Run:
                        case XD2.WorkflowInstanceManagementService.TransactedRun:
                            thisPtr.workflowServiceInstance.EndRun(result);
                            break;
                        case XD2.WorkflowInstanceManagementService.Cancel:
                        case XD2.WorkflowInstanceManagementService.TransactedCancel:
                            thisPtr.workflowServiceInstance.EndCancel(result);
                            break;
                        case XD2.WorkflowInstanceManagementService.Abandon:
                            thisPtr.workflowServiceInstance.EndAbandon(result);
                            break;
                        case XD2.WorkflowInstanceManagementService.Update:
                        case XD2.WorkflowInstanceManagementService.TransactedUpdate:
                            if (result is CompletedAsyncResult)
                            {
                                CompletedAsyncResult.End(result);
                            }
                            else
                            {
                                thisPtr.workflowServiceInstance.EndRun(result);
                            }
                            break;
                        default:
                            throw Fx.AssertAndThrow("Unreachable code");
                    }
                }
                else if (thisPtr.getInstanceContext.WorkflowCreationContext != null)
                {
                    thisPtr.returnValue = thisPtr.EndRunAndGetResponse(result, out thisPtr.outputs);
                }
                else
                {
                    //User operation
                    try
                    {
                        thisPtr.returnValue = thisPtr.invoker.OnEndServiceOperation(thisPtr.workflowServiceInstance, out thisPtr.outputs, result);
                    }
                    catch (FaultException)
                    {
                        throw; // ReceiveContext was handled appropriately by WorkflowOperationContext
                    }
                    catch (Exception exception)
                    {
                        if (Fx.IsFatal(exception))
                        {
                            throw;
                        }
                        if (!thisPtr.ShouldAbandonReceiveContext())
                        {
                            throw;
                        }
                        return thisPtr.AbandonReceiveContext(exception);
                    }
                }

                return true;
            }

            bool ShouldAbandonReceiveContext()
            {
                return this.receiveContext != null && this.receiveContext.State != ReceiveContextState.Faulted;
            }

            bool AbandonReceiveContext(Exception operationException)
            {
                Fx.Assert(ShouldAbandonReceiveContext(), "ShouldAbandonReceiveContext() is false!");
                if (handleEndAbandonReceiveContext == null)
                {
                    handleEndAbandonReceiveContext = new AsyncCompletion(HandleEndAbandonReceiveContext);
                }

                Fx.Assert(operationException != null, "operationException must not be null!");
                Fx.Assert(this.operationException == null, "AbandonReceiveContext must not be called twice!");
                this.operationException = operationException;
                IAsyncResult result = this.receiveContext.BeginAbandon(TimeSpan.MaxValue, this.PrepareAsyncCompletion(handleEndAbandonReceiveContext), this);
                return SyncContinue(result);
            }

            static bool HandleEndAbandonReceiveContext(IAsyncResult result)
            {
                ControlOperationAsyncResult thisPtr = (ControlOperationAsyncResult)result.AsyncState;
                thisPtr.receiveContext.EndAbandon(result);
                throw FxTrace.Exception.AsError(thisPtr.operationException);
            }

            void EnsureInstanceIdAndIdentity()
            {
                if (this.invoker.isControlOperation)
                {
                    switch (this.invoker.operationName)
                    {
                        case XD2.WorkflowInstanceManagementService.Abandon:
                        case XD2.WorkflowInstanceManagementService.Cancel:
                        case XD2.WorkflowInstanceManagementService.TransactedCancel:
                        case XD2.WorkflowInstanceManagementService.Run:
                        case XD2.WorkflowInstanceManagementService.TransactedRun:
                        case XD2.WorkflowInstanceManagementService.Suspend:
                        case XD2.WorkflowInstanceManagementService.TransactedSuspend:
                        case XD2.WorkflowInstanceManagementService.Terminate:
                        case XD2.WorkflowInstanceManagementService.TransactedTerminate:
                        case XD2.WorkflowInstanceManagementService.Unsuspend:
                        case XD2.WorkflowInstanceManagementService.TransactedUnsuspend:
                            this.instanceId = GetInstanceIdForControlOperation(this.inputs);
                            break;
                        case XD2.WorkflowInstanceManagementService.Update:
                        case XD2.WorkflowInstanceManagementService.TransactedUpdate:
                            this.instanceId = GetInstanceIdForControlOperation(this.inputs);
                            this.updatedIdentity = new WorkflowIdentityKey(GetIdentityForControlOperation(this.inputs));
                            break;
                        default:
                            throw Fx.AssertAndThrow("Unreachable code");
                    }
                }
                else if (this.invoker.endpoint is WorkflowHostingEndpoint)
                {
                    WorkflowHostingEndpoint hostingEndpoint = (WorkflowHostingEndpoint)this.invoker.endpoint;
                    this.instanceId = hostingEndpoint.OnGetInstanceId(inputs, this.operationContext);
                    if (this.instanceId == Guid.Empty)
                    {
                        this.invoker.GetInstanceKeys(this.operationContext, out this.instanceKey, out this.additionalKeys);
                    }
                }
                else
                {
                    //User endpoint operation.
                    this.invoker.GetInstanceKeys(this.operationContext, out this.instanceKey, out this.additionalKeys);
                }
            }

            Guid GetInstanceIdForControlOperation(object[] args)
            {
                Fx.Assert(args != null && args.Length > 0, "Cannot get argument");
                object arg = args[0];

                if (arg != null && arg is Guid)
                {
                    return (Guid)arg;
                }
                else
                {
                    throw FxTrace.Exception.AsError(new InvalidOperationException(SR.FailedToGetInstanceIdForControlOperation));
                }
            }

            WorkflowIdentity GetIdentityForControlOperation(object[] args)
            {
                Fx.Assert(args != null && args.Length > 1, "Cannot get argument");
                object arg = args[1];

                if (arg == null || arg is WorkflowIdentity)
                {
                    return (WorkflowIdentity)arg;
                }
                else
                {
                    throw FxTrace.Exception.AsError(new InvalidOperationException(SR.FailedToGetWorkflowIdentityForControlOperation));
                }
            }

            bool TryCreateRedirectionException(InstanceLockedException exception, out RedirectionException redirectionException)
            {
                Uri redirectVia = null;

                object redirectViaObject;
                string endpointName = this.invoker.endpoint != null ? this.invoker.endpoint.Name : null;
                XName endpointXName = endpointName == null ? null : WorkflowServiceNamespace.EndpointsPath.GetName(endpointName);
                if (endpointXName != null && exception.SerializableInstanceOwnerMetadata != null &&
                    exception.SerializableInstanceOwnerMetadata.TryGetValue(endpointXName, out redirectViaObject))
                {
                    redirectVia = redirectViaObject as Uri;
                }

                if (redirectVia == null)
                {
                    redirectionException = null;
                    return false;
                }

                redirectionException = new RedirectionException(RedirectionType.Resource, RedirectionDuration.Permanent,
                    RedirectionScope.Session, new RedirectionLocation(redirectVia));
                return true;
            }

            static FaultException CreateFaultException(InstancePersistenceException exception)
            {
                return new FaultException(OperationExecutionFault.CreateInstanceNotFoundFault(exception.Message));
            }

            IAsyncResult BeginRunAndGetResponse(TimeoutHelper timeoutHelper, AsyncCallback callback, object state)
            {
                return RunAndGetResponseAsyncResult.Create(this, timeoutHelper, callback, state);
            }

            object EndRunAndGetResponse(IAsyncResult result, out object[] outputs)
            {
                return RunAndGetResponseAsyncResult.End(result, out outputs);
            }

            class RunAndGetResponseAsyncResult : AsyncResult
            {
                static AsyncCompletion handleEndRun = new AsyncCompletion(HandleEndRun);
                static AsyncCompletion handleEndSuspend = new AsyncCompletion(HandleEndSuspend);
                static AsyncCompletion handleEndGetResponse = new AsyncCompletion(HandleEndGetResponse);

                ControlOperationAsyncResult control;
                TimeoutHelper timeoutHelper;
                object returnValue;
                object[] outputs;

                RunAndGetResponseAsyncResult(ControlOperationAsyncResult control, TimeoutHelper timeoutHelper, AsyncCallback callback, object state)
                    : base(callback, state)
                {
                    this.control = control;
                    this.timeoutHelper = timeoutHelper;

                    bool completeSelf = true;

                    if (control.getInstanceContext.WorkflowCreationContext.CreateOnly)
                    {
                        completeSelf = Suspend();
                    }
                    else
                    {
                        completeSelf = Run();
                    }

                    if (completeSelf)
                    {
                        Complete(true);
                    }
                }

                public static RunAndGetResponseAsyncResult Create(ControlOperationAsyncResult control, TimeoutHelper timeoutHelper, AsyncCallback callback, object state)
                {
                    return new RunAndGetResponseAsyncResult(control, timeoutHelper, callback, state);
                }

                public static object End(IAsyncResult result, out object[] outputs)
                {
                    RunAndGetResponseAsyncResult thisPtr = AsyncResult.End<RunAndGetResponseAsyncResult>(result);
                    outputs = thisPtr.outputs;
                    return thisPtr.returnValue;
                }

                bool Run()
                {
                    IAsyncResult result = this.control.workflowServiceInstance.BeginRun(this.control.transaction, this.timeoutHelper.RemainingTime(),
                        PrepareAsyncCompletion(handleEndRun), this);
                    return SyncContinue(result);
                }

                static bool HandleEndRun(IAsyncResult result)
                {
                    RunAndGetResponseAsyncResult thisPtr = (RunAndGetResponseAsyncResult)result.AsyncState;
                    thisPtr.control.workflowServiceInstance.EndRun(result);
                    return thisPtr.GetResponse();
                }

                bool Suspend()
                {
                    IAsyncResult result = this.control.workflowServiceInstance.BeginSuspend(false, SR.DefaultCreateOnlyReason,
                        this.control.transaction, this.timeoutHelper.RemainingTime(), PrepareAsyncCompletion(handleEndSuspend), this);
                    return SyncContinue(result);
                }

                static bool HandleEndSuspend(IAsyncResult result)
                {
                    RunAndGetResponseAsyncResult thisPtr = (RunAndGetResponseAsyncResult)result.AsyncState;
                    thisPtr.control.workflowServiceInstance.EndSuspend(result);
                    return thisPtr.GetResponse();
                }

                bool GetResponse()
                {
                    IAsyncResult result = this.control.getInstanceContext.WorkflowHostingResponseContext.BeginGetResponse(this.timeoutHelper.RemainingTime(),
                        PrepareAsyncCompletion(handleEndGetResponse), this);
                    return SyncContinue(result);
                }

                static bool HandleEndGetResponse(IAsyncResult result)
                {
                    RunAndGetResponseAsyncResult thisPtr = (RunAndGetResponseAsyncResult)result.AsyncState;
                    thisPtr.returnValue = thisPtr.control.getInstanceContext.WorkflowHostingResponseContext.EndGetResponse(result, out thisPtr.outputs);
                    return true;
                }
            }
        }

        //AsyncResult implementation for User endpoint operation dispatch.
        class ServiceOperationAsyncResult : TransactedAsyncResult
        {
            static AsyncCompletion handleEndInvoke = new AsyncCompletion(HandleEndInvoke);
            IOperationInvoker innerInvoker;
            WorkflowServiceInstance durableInstance;
            object[] inputs;
            OperationContext operationContext;
            object returnValue;
            object[] outputs;
            Transaction currentTransaction;
            IInvokeReceivedNotification notification;

            public ServiceOperationAsyncResult(IOperationInvoker innerInvoker, WorkflowServiceInstance durableInstance,
                object[] inputs, OperationContext operationContext, Transaction currentTransaction, IInvokeReceivedNotification notification,
                AsyncCallback callback, object state)
                : base(callback, state)
            {
                this.durableInstance = durableInstance;
                this.operationContext = operationContext;
                this.inputs = inputs;
                this.innerInvoker = innerInvoker;
                this.currentTransaction = currentTransaction;
                this.notification = notification;

                if (innerInvoker == null)
                {
                    //Mode2: Derived invoker should have handled this call.
                    throw Fx.AssertAndThrow("Cannot reach this path without innerInvoker");
                }

                if (this.innerInvoker.IsSynchronous)
                {
                    TransactionScope scope = TransactionHelper.CreateTransactionScope(this.currentTransaction);
                    try
                    {
                        using (new OperationContextScopeHelper(this.operationContext))
                        {
                            IManualConcurrencyOperationInvoker manualInvoker = this.innerInvoker as IManualConcurrencyOperationInvoker;
                            if (manualInvoker != null)
                            {
                                this.returnValue = manualInvoker.Invoke(this.durableInstance, this.inputs, this.notification, out this.outputs);
                            }
                            else
                            {
                                this.returnValue = this.innerInvoker.Invoke(this.durableInstance, this.inputs, out this.outputs);
                            }
                        }
                    }
                    finally
                    {
                        TransactionHelper.CompleteTransactionScope(ref scope);
                    }
                    this.Complete(true);
                }
                else
                {
                    IAsyncResult result;
                    using (PrepareTransactionalCall(this.currentTransaction))
                    {
                        using (new OperationContextScopeHelper(this.operationContext))
                        {
                            IManualConcurrencyOperationInvoker manualInvoker = this.innerInvoker as IManualConcurrencyOperationInvoker;
                            if (manualInvoker != null)
                            {
                                result = manualInvoker.InvokeBegin(this.durableInstance, this.inputs, this.notification, this.PrepareAsyncCompletion(handleEndInvoke), this);
                            }
                            else
                            {
                                result = this.innerInvoker.InvokeBegin(this.durableInstance, this.inputs, this.PrepareAsyncCompletion(handleEndInvoke), this);
                            }
                        }
                    }
                    if (SyncContinue(result))
                    {
                        this.Complete(true);
                    }
                }
            }

            public static object End(out object[] outputs, IAsyncResult result)
            {
                ServiceOperationAsyncResult thisPtr = AsyncResult.End<ServiceOperationAsyncResult>(result);
                outputs = thisPtr.outputs;
                return thisPtr.returnValue;
            }

            static bool HandleEndInvoke(IAsyncResult result)
            {
                ServiceOperationAsyncResult thisPtr = (ServiceOperationAsyncResult)result.AsyncState;

                TransactionScope scope = TransactionHelper.CreateTransactionScope(thisPtr.currentTransaction);
                try
                {
                    using (new OperationContextScopeHelper(thisPtr.operationContext))
                    {
                        thisPtr.returnValue = thisPtr.innerInvoker.InvokeEnd(thisPtr.durableInstance, out thisPtr.outputs, result);
                        return true;
                    }
                }
                finally
                {
                    TransactionHelper.CompleteTransactionScope(ref scope);
                }
            }

            class OperationContextScopeHelper : IDisposable
            {
                OperationContext current;

                public OperationContextScopeHelper(OperationContext newContext)
                {
                    this.current = OperationContext.Current;
                    OperationContext.Current = newContext;
                }

                void IDisposable.Dispose()
                {
                    OperationContext.Current = this.current;
                }
            }
        }
    }
}
