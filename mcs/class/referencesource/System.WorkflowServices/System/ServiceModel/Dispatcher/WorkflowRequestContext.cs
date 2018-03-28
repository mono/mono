//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Dispatcher
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics.CodeAnalysis;
    using System.IdentityModel.Policy;
    using System.Runtime;
    using System.Runtime.Diagnostics;
    using System.Runtime.Serialization;
    using System.ServiceModel.Diagnostics;
    using System.ServiceModel.Security;
    using System.Diagnostics;

    [Serializable]
    class WorkflowRequestContext
    {
        [NonSerialized]
        WorkflowOperationAsyncResult asyncResult;

        [NonSerialized]
        AuthorizationContext authorizationContext;

        IDictionary<string, string> contextProperties;
        ReadOnlyCollection<object> inputs;

        [NonSerialized]
        OperationContext operationContext;

        SerializableAuthorizationContext serializedAuthorizationContext;

        public WorkflowRequestContext(WorkflowOperationAsyncResult asyncResult, object[] inputs, IDictionary<string, string> contextProperties)
        {
            if (asyncResult == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("asyncResult");
            }

            if (inputs == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("inputs");
            }

            this.asyncResult = asyncResult;
            this.inputs = new ReadOnlyCollection<object>(inputs);
            this.contextProperties = contextProperties ?? SerializableReadOnlyDictionary<string, string>.Empty;
            this.operationContext = OperationContext.Current;
        }

        public AuthorizationContext AuthorizationContext
        {
            get
            {
                if (this.authorizationContext == null)
                {
                    if (this.serializedAuthorizationContext != null)
                    {
                        this.authorizationContext = serializedAuthorizationContext.Retrieve();
                    }
                }
                return authorizationContext;
            }
        }

        public IDictionary<string, string> ContextProperties
        {
            get
            {
                return this.contextProperties;
            }
        }

        public ReadOnlyCollection<object> Inputs
        {
            get
            {
                return this.inputs;
            }
        }

        public void PopulateAuthorizationState()
        {
            if (OperationContext.Current == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR2.GetString(SR2.NoOperationContext)));
            }

            if (OperationContext.Current.ServiceSecurityContext != null)
            {
                this.authorizationContext = OperationContext.Current.ServiceSecurityContext.AuthorizationContext;
            }
        }

        public void SendFault(Exception exception, IDictionary<string, string> outgoingContextProperties)
        {
            if (exception == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("exception");
            }

            if (!(exception is FaultException)) //Wrap the exception if it is not FaultException.
            {
                exception = WorkflowOperationErrorHandler.CreateUnhandledException(exception);
            }

            WorkflowOperationAsyncResult asyncResult = this.GetAsyncResult();
            asyncResult.SendFault(exception, outgoingContextProperties);
            this.SetOperationCompleted();

            if (DiagnosticUtility.ShouldTraceInformation)
            {
                string traceText = SR.GetString(SR.TraceCodeWorkflowRequestContextFaultSent, asyncResult.InstanceId);
                TraceUtility.TraceEvent(TraceEventType.Information,
                    TraceCode.WorkflowRequestContextFaultSent, traceText, 
                    new StringTraceRecord("Details", traceText),
                    this,
                    exception);
            }
        }

        public void SendReply(object returnValue, object[] outputs, IDictionary<string, string> outgoingContextProperties)
        {
            if (outputs == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("outputs");
            }

            WorkflowOperationAsyncResult asyncResult = this.GetAsyncResult();
            asyncResult.SendResponse(returnValue, outputs, outgoingContextProperties);
            this.SetOperationCompleted();

            if (DiagnosticUtility.ShouldTraceVerbose)
            {
                string traceText = SR.GetString(SR.TraceCodeWorkflowRequestContextReplySent, asyncResult.InstanceId);
                TraceUtility.TraceEvent(TraceEventType.Verbose,
                    TraceCode.WorkflowRequestContextReplySent, traceText,
                    new StringTraceRecord("Details", traceText),
                    this, null);
            }
        }

        public void SetOperationCompleted()
        {
            OperationContext.Current = this.operationContext;

            try
            {
                this.GetAsyncResult().MarkOneWayOperationCompleted();
            }
            catch (Exception e)
            {
                if (Fx.IsFatal(e))
                {
                    throw;
                }
                //For Two-ways it is no-op; For One-ways make it fire & forget.
            }
        }

        internal WorkflowOperationAsyncResult GetAsyncResult()
        {
            if (this.asyncResult == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR2.GetString(SR2.UnloadedBeforeResponse)));
            }
            return asyncResult;
        }

        //To be called by dispatcher before EnqueueItemOnIdle.
        internal void SetOperationBegin()
        {
            OperationContext current = OperationContext.Current;
            OperationContext.Current = this.operationContext;
            this.operationContext = current;
        }

        [OnSerializing]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters")]
        void OnSerializing(StreamingContext context)
        {
            if (this.serializedAuthorizationContext == null)
            {
                if (this.authorizationContext != null)
                {
                    this.serializedAuthorizationContext = SerializableAuthorizationContext.From(this.authorizationContext);
                }
            }
            if (this.asyncResult != null)
            {
                // Serialization time is the only reasonable hook point to determine that the workflow 
                // is not going to be able to send back a response (because asyncResult does not serialize). 
                // Setting this flag on the async result enables the logic in WorkflowInstanceContextProvider.OnWorkflowActivationCompleted
                // to complete the operation from service model's perspective. 
                this.asyncResult.HasWorkflowRequestContextBeenSerialized = true;
            }
        }
    }
}
