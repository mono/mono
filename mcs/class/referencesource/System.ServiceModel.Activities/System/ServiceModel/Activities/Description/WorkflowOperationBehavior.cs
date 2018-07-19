//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Activities.Description
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Activities;
    using System.Runtime;
    using System.Runtime.Diagnostics;
    using System.Runtime.DurableInstancing;
    using System.ServiceModel.Activities.Dispatcher;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.ServiceModel.Diagnostics;
    using System.ServiceModel.Dispatcher;
    using System.Transactions;

    class WorkflowOperationBehavior : IOperationBehavior
    {
        Bookmark bookmark;


        public WorkflowOperationBehavior(Bookmark bookmark, bool canCreateInstance)
            : this(canCreateInstance)
        {
            Fx.Assert(bookmark != null, "bookmark must not be null!");
            this.bookmark = bookmark;
        }

        protected WorkflowOperationBehavior(bool canCreateInstance)
        {
            this.CanCreateInstance = canCreateInstance;
        }

        internal bool CanCreateInstance
        {
            get;
            set;
        }

        public void AddBindingParameters(OperationDescription operationDescription, BindingParameterCollection bindingParameters)
        {
        }

        public void ApplyClientBehavior(OperationDescription operationDescription, ClientOperation clientOperation)
        {
        }

        public void ApplyDispatchBehavior(OperationDescription operationDescription, DispatchOperation dispatchOperation)
        {
            if (operationDescription == null)
            {
                throw FxTrace.Exception.ArgumentNull("operationDescription");
            }
            if (dispatchOperation == null)
            {
                throw FxTrace.Exception.ArgumentNull("dispatchOperation");
            }
            if (dispatchOperation.Parent == null
                || dispatchOperation.Parent.ChannelDispatcher == null
                || dispatchOperation.Parent.ChannelDispatcher.Host == null
                || dispatchOperation.Parent.ChannelDispatcher.Host.Description == null
                || dispatchOperation.Parent.ChannelDispatcher.Host.Description.Behaviors == null)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(SR.DispatchOperationInInvalidState));
            }

            ServiceHostBase serviceHost = dispatchOperation.Parent.ChannelDispatcher.Host;
            if (!(serviceHost is WorkflowServiceHost))
            {
                throw FxTrace.Exception.AsError(
                   new InvalidOperationException(SR.WorkflowBehaviorWithNonWorkflowHost(typeof(WorkflowOperationBehavior).Name)));
            }

            CorrelationKeyCalculator correlationKeyCalculator = null;

            ServiceEndpoint endpoint = null;
            foreach (ServiceEndpoint endpointToMatch in serviceHost.Description.Endpoints)
            {
                if (endpointToMatch.Id == dispatchOperation.Parent.EndpointDispatcher.Id)
                {
                    endpoint = endpointToMatch;
                    break;
                }
            }

            if (endpoint != null)
            {
                CorrelationQueryBehavior queryBehavior = endpoint.Behaviors.Find<CorrelationQueryBehavior>();

                if (queryBehavior != null)
                {
                    correlationKeyCalculator = queryBehavior.GetKeyCalculator();
                }
            }

            dispatchOperation.Invoker = new WorkflowOperationInvoker(operationDescription,
                endpoint, correlationKeyCalculator, this, serviceHost, dispatchOperation.Invoker);
        }

        public void Validate(OperationDescription operationDescription)
        {
        }

        protected internal virtual Bookmark OnResolveBookmark(WorkflowOperationContext context, out BookmarkScope bookmarkScope, out object value)
        {
            Fx.Assert(this.bookmark != null, "bookmark must not be null!");

            CorrelationMessageProperty correlationMessageProperty;
            if (CorrelationMessageProperty.TryGet(context.OperationContext.IncomingMessageProperties, out correlationMessageProperty))
            {
                bookmarkScope = new BookmarkScope(correlationMessageProperty.CorrelationKey.Value);
            }
            else
            {
                bookmarkScope = BookmarkScope.Default;
            }
            value = context;
            return this.bookmark;
        }

        //Invoker for workflowbased application endpoint operation 
        class WorkflowOperationInvoker : ControlOperationInvoker, IInstanceTransaction
        {
            bool performanceCountersEnabled;
            bool propagateActivity;
            bool isHostingEndpoint;
            IOperationInvoker innerInvoker;
            WorkflowOperationBehavior behavior;
            bool isFirstReceiveOfTransactedReceiveScopeTree;
            
            public WorkflowOperationInvoker(OperationDescription operationDescription, ServiceEndpoint endpoint,
                CorrelationKeyCalculator keyCalculator, WorkflowOperationBehavior behavior, ServiceHostBase host, IOperationInvoker innerInvoker)
                : base(operationDescription, endpoint, keyCalculator, host)
            {
                Fx.Assert(operationDescription != null, "Null OperationDescription");
                Fx.Assert(behavior != null, "Null WorkflowOperationBehavior");
                this.StaticBookmarkName = behavior.bookmark == null ? null : behavior.bookmark.Name;
                this.behavior = behavior;
                this.CanCreateInstance = behavior.CanCreateInstance;
                this.performanceCountersEnabled = PerformanceCounters.PerformanceCountersEnabled;
                this.propagateActivity = TraceUtility.ShouldPropagateActivity;
                this.isHostingEndpoint = endpoint is WorkflowHostingEndpoint;
                this.innerInvoker = innerInvoker;
                this.isFirstReceiveOfTransactedReceiveScopeTree = operationDescription.IsFirstReceiveOfTransactedReceiveScopeTree;
            }

            public override object[] AllocateInputs()
            {
                if (this.isHostingEndpoint)
                {
                    return this.innerInvoker.AllocateInputs();
                }
                // InternalReceiveMessage & InternalSendMessage is always MessageIn - MessageOut.
                // Therefore we always need an array of size 1.
                // DispatchOperationRuntime saves the request into this array in this case ( i.e., when DeserializeRequest is false)
                return new object[1];
            }

            protected override IAsyncResult OnBeginServiceOperation(WorkflowServiceInstance workflowInstance, OperationContext operationContext,
                object[] inputs, Transaction currentTransaction, IInvokeReceivedNotification notification, TimeSpan timeout, AsyncCallback callback, object state)
            {
                Fx.Assert(workflowInstance != null, "caller must verify");
                Fx.Assert(inputs != null, "caller must verify");

                return WorkflowOperationContext.BeginProcessRequest(workflowInstance, operationContext, this.OperationName, inputs,
                    this.performanceCountersEnabled, this.propagateActivity, currentTransaction, notification, this.behavior, this.endpoint, timeout, callback, state);
            }

            protected override object OnEndServiceOperation(WorkflowServiceInstance durableInstance, out object[] outputs, IAsyncResult result)
            {
                // InternalSendMessage always redirects the replyMessage into the returnValue
                object returnValue = WorkflowOperationContext.EndProcessRequest(result, out outputs);

                //we will just assert that outputs is always an empty array
                Fx.Assert(this.isHostingEndpoint || outputs == null || outputs.Length == 0, "Workflow returned a non-empty out-arg");
                
                return returnValue;
            }

            public Transaction GetTransactionForInstance(OperationContext operationContext)
            {
                Transaction tx = null;

                // We are only going to go ask the PPD for the transaction if we are NOT the first
                // Receive in a TransactedReceiveScope;
                if (!this.isFirstReceiveOfTransactedReceiveScopeTree)
                {
                    // We need to get the InstanceKey.
                    InstanceKey instanceKey;
                    ICollection<InstanceKey> additionalKeys;
                    this.GetInstanceKeys(operationContext, out instanceKey, out additionalKeys);
                    Fx.Assert((instanceKey != null) && (instanceKey.IsValid), "InstanceKey is null or invalid in GetInstanceTransaction");

                    tx = this.InstanceManager.PersistenceProviderDirectory.GetTransactionForInstance(instanceKey);
                }

                return tx;
            }
        }
    }
}
