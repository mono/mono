//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Activities
{
    using System.Activities;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Runtime;
    using System.ServiceModel.Activities.Description;
    using System.ServiceModel.Activities.Dispatcher;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.ServiceModel.Dispatcher;

    public abstract class WorkflowHostingEndpoint : ServiceEndpoint
    {
        Collection<CorrelationQuery> correlationQueries;

        protected WorkflowHostingEndpoint(Type contractType)
            : this(contractType, null, null)
        {
        }

        protected WorkflowHostingEndpoint(Type contractType, Binding binding, EndpointAddress address)
            : base(ContractDescription.GetContract(contractType), binding, address)
        {
            this.IsSystemEndpoint = true;
            this.Contract.Behaviors.Add(new ServiceMetadataContractBehavior(false));
            this.Contract.Behaviors.Add(new WorkflowHostingContractBehavior());
            Fx.Assert(!this.Behaviors.Contains(typeof(CorrelationQueryBehavior)), "Must not contain correlation query!");
            this.correlationQueries = new Collection<CorrelationQuery>();
            this.Behaviors.Add(new CorrelationQueryBehavior(this.correlationQueries));

            // If TransactionFlowOption.Allowed or TransactionFlowOption.Mandatory is defined on an operation, we will set 
            // TransactionScopeRequired = true for that operation.  The operation will become transacted (use transaction flow, 
            // or create one locally).  For usability reason, we assume this is the majority usage.  User could opt out by 
            // setting TransactionScopeRequired to false or remove the TransactionFlowAttribute from the operation.
            foreach (OperationDescription operationDescription in this.Contract.Operations)
            {
                TransactionFlowAttribute transactionFlow = operationDescription.Behaviors.Find<TransactionFlowAttribute>();
                if (transactionFlow != null && transactionFlow.Transactions != TransactionFlowOption.NotAllowed)
                {
                    OperationBehaviorAttribute operationAttribute = operationDescription.Behaviors.Find<OperationBehaviorAttribute>();
                    operationAttribute.TransactionScopeRequired = true;
                }
            }
        }

        public Collection<CorrelationQuery> CorrelationQueries
        {
            get { return this.correlationQueries; }
        }

        // There are two main scenario that user will override this api.
        // - For ResumeBookmark, User explicitly put or know how to extract InstanceId from Message.  This enables user to provide
        //   customized and lighter-weight (no InstanceKeys indirection) correlation.
        // - For Workflow Creation, User could provide a preferred Id for newly created Workflow Instance.
        protected internal virtual Guid OnGetInstanceId(object[] inputs, OperationContext operationContext)
        {
            return Guid.Empty;
        }

        protected internal virtual WorkflowCreationContext OnGetCreationContext(
            object[] inputs, OperationContext operationContext,
            Guid instanceId, WorkflowHostingResponseContext responseContext)
        {
            return null;
        }

        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.AvoidOutParameters,
            Justification = "By design, return both Bookmark and its payload.")]
        protected internal virtual Bookmark OnResolveBookmark(object[] inputs, OperationContext operationContext,
            WorkflowHostingResponseContext responseContext, out object value)
        {
            value = null;
            return null;
        }

        internal static FaultException CreateDispatchFaultException()
        {
            FaultCode code = new FaultCode(FaultCodeConstants.Codes.InternalServiceFault, FaultCodeConstants.Namespaces.NetDispatch);
            code = FaultCode.CreateReceiverFaultCode(code);
            MessageFault dispatchFault = MessageFault.CreateFault(code,
                    new FaultReason(new FaultReasonText(SR.InternalServerError, CultureInfo.CurrentCulture)));
            return new FaultException(dispatchFault, FaultCodeConstants.Actions.NetDispatcher);
        }

        class WorkflowHostingContractBehavior : IContractBehavior
        {
            public void AddBindingParameters(ContractDescription contractDescription, ServiceEndpoint endpoint, BindingParameterCollection bindingParameters)
            {
            }

            public void ApplyClientBehavior(ContractDescription contractDescription, ServiceEndpoint endpoint, ClientRuntime clientRuntime)
            {
            }

            public void ApplyDispatchBehavior(ContractDescription contractDescription, ServiceEndpoint endpoint, DispatchRuntime dispatchRuntime)
            {
                Fx.Assert(endpoint is WorkflowHostingEndpoint, "Must be hosting endpoint!");
                foreach (OperationDescription operation in contractDescription.Operations)
                {
                    if (operation.Behaviors.Find<WorkflowHostingOperationBehavior>() == null)
                    {
                        operation.Behaviors.Add(new WorkflowHostingOperationBehavior());
                    }
                }
            }

            public void Validate(ContractDescription contractDescription, ServiceEndpoint endpoint)
            {
                Fx.Assert(endpoint is WorkflowHostingEndpoint, "Must be hosting endpoint!");
            }

            class WorkflowHostingOperationBehavior : WorkflowOperationBehavior
            {
                public WorkflowHostingOperationBehavior()
                    : base(true)
                {
                }

                protected internal override Bookmark OnResolveBookmark(WorkflowOperationContext context, out BookmarkScope bookmarkScope, out object value)
                {
                    CorrelationMessageProperty correlationMessageProperty;
                    if (CorrelationMessageProperty.TryGet(context.OperationContext.IncomingMessageProperties, out correlationMessageProperty))
                    {
                        bookmarkScope = new BookmarkScope(correlationMessageProperty.CorrelationKey.Value);
                    }
                    else
                    {
                        bookmarkScope = null;
                    }

                    WorkflowHostingResponseContext responseContext = new WorkflowHostingResponseContext(context);
                    Fx.Assert(context.ServiceEndpoint is WorkflowHostingEndpoint, "serviceEnpoint must be of WorkflowHostingEndpoint type!");
                    Bookmark bookmark = ((WorkflowHostingEndpoint)context.ServiceEndpoint).OnResolveBookmark(context.Inputs, context.OperationContext, responseContext, out value);
                    if (bookmark == null)
                    {
                        throw FxTrace.Exception.AsError(CreateDispatchFaultException());
                    }
                    return bookmark;
                }
            }
        }
    }
}

