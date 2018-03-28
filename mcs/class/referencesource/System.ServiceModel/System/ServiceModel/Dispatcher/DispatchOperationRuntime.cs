//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.IdentityModel.Configuration;
    using System.IdentityModel.Tokens;
    using System.Reflection;
    using System.Runtime;
    using System.Security;
    using System.Security.Claims;
    using System.Security.Principal;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Diagnostics;
    using System.ServiceModel.Diagnostics.Application;
    using System.ServiceModel.Security;
    
    class DispatchOperationRuntime
    {
        static AsyncCallback invokeCallback = Fx.ThunkCallback(new AsyncCallback(DispatchOperationRuntime.InvokeCallback));
        readonly string action;
        readonly ICallContextInitializer[] callContextInitializers;
        readonly IDispatchFaultFormatter faultFormatter;
        readonly IDispatchMessageFormatter formatter;
        readonly ImpersonationOption impersonation;
        readonly IParameterInspector[] inspectors;
        readonly IOperationInvoker invoker;
        readonly bool isTerminating;
        readonly bool isSessionOpenNotificationEnabled;
        readonly bool isSynchronous;
        readonly string name;
        readonly ImmutableDispatchRuntime parent;
        readonly bool releaseInstanceAfterCall;
        readonly bool releaseInstanceBeforeCall;
        readonly string replyAction;
        readonly bool transactionAutoComplete;
        readonly bool transactionRequired;
        readonly bool deserializeRequest;
        readonly bool serializeReply;
        readonly bool isOneWay;
        readonly bool disposeParameters;
        readonly ReceiveContextAcknowledgementMode receiveContextAcknowledgementMode;
        readonly bool bufferedReceiveEnabled;
        readonly bool isInsideTransactedReceiveScope;

        internal DispatchOperationRuntime(DispatchOperation operation, ImmutableDispatchRuntime parent)
        {
            if (operation == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("operation");
            }
            if (parent == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("parent");
            }
            if (operation.Invoker == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.RuntimeRequiresInvoker0)));
            }

            this.disposeParameters = ((operation.AutoDisposeParameters) && (!operation.HasNoDisposableParameters));
            this.parent = parent;
            this.callContextInitializers = EmptyArray<ICallContextInitializer>.ToArray(operation.CallContextInitializers);
            this.inspectors = EmptyArray<IParameterInspector>.ToArray(operation.ParameterInspectors);
            this.faultFormatter = operation.FaultFormatter;
            this.impersonation = operation.Impersonation;
            this.deserializeRequest = operation.DeserializeRequest;
            this.serializeReply = operation.SerializeReply;
            this.formatter = operation.Formatter;
            this.invoker = operation.Invoker;

            try
            {
                this.isSynchronous = operation.Invoker.IsSynchronous;
            }
            catch (Exception e)
            {
                if (Fx.IsFatal(e))
                {
                    throw;
                }
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperCallback(e);
            }
            this.isTerminating = operation.IsTerminating;
            this.isSessionOpenNotificationEnabled = operation.IsSessionOpenNotificationEnabled;
            this.action = operation.Action;
            this.name = operation.Name;
            this.releaseInstanceAfterCall = operation.ReleaseInstanceAfterCall;
            this.releaseInstanceBeforeCall = operation.ReleaseInstanceBeforeCall;
            this.replyAction = operation.ReplyAction;
            this.isOneWay = operation.IsOneWay;
            this.transactionAutoComplete = operation.TransactionAutoComplete;
            this.transactionRequired = operation.TransactionRequired;
            this.receiveContextAcknowledgementMode = operation.ReceiveContextAcknowledgementMode;
            this.bufferedReceiveEnabled = operation.BufferedReceiveEnabled;
            this.isInsideTransactedReceiveScope = operation.IsInsideTransactedReceiveScope;

            if (this.formatter == null && (deserializeRequest || serializeReply))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.DispatchRuntimeRequiresFormatter0, this.name)));
            }

            if ((operation.Parent.InstanceProvider == null) && (operation.Parent.Type != null))
            {
                SyncMethodInvoker sync = this.invoker as SyncMethodInvoker;
                if (sync != null)
                {
                    this.ValidateInstanceType(operation.Parent.Type, sync.Method);
                }

                AsyncMethodInvoker async = this.invoker as AsyncMethodInvoker;
                if (async != null)
                {
                    this.ValidateInstanceType(operation.Parent.Type, async.BeginMethod);
                    this.ValidateInstanceType(operation.Parent.Type, async.EndMethod);
                }

                TaskMethodInvoker task = this.invoker as TaskMethodInvoker;
                if (task != null)
                {
                    this.ValidateInstanceType(operation.Parent.Type, task.TaskMethod);
                }
            }
        }

        internal string Action
        {
            get { return this.action; }
        }

        internal ICallContextInitializer[] CallContextInitializers
        {
            get { return this.callContextInitializers; }
        }

        internal bool DisposeParameters
        {
            get { return this.disposeParameters; }
        }

        internal bool HasDefaultUnhandledActionInvoker
        {
            get { return (this.invoker is DispatchRuntime.UnhandledActionInvoker); }
        }

        internal bool SerializeReply
        {
            get { return this.serializeReply; }
        }

        internal IDispatchFaultFormatter FaultFormatter
        {
            get { return this.faultFormatter; }
        }

        internal IDispatchMessageFormatter Formatter
        {
            get { return this.formatter; }
        }

        internal ImpersonationOption Impersonation
        {
            get { return this.impersonation; }
        }

        internal IOperationInvoker Invoker
        {
            get { return this.invoker; }
        }

        internal bool IsSynchronous
        {
            get { return this.isSynchronous; }
        }

        internal bool IsOneWay
        {
            get { return this.isOneWay; }
        }

        internal bool IsTerminating
        {
            get { return this.isTerminating; }
        }

        internal string Name
        {
            get { return this.name; }
        }

        internal IParameterInspector[] ParameterInspectors
        {
            get { return this.inspectors; }
        }

        internal ImmutableDispatchRuntime Parent
        {
            get { return this.parent; }
        }

        internal ReceiveContextAcknowledgementMode ReceiveContextAcknowledgementMode
        {
            get { return this.receiveContextAcknowledgementMode; }
        }

        internal bool ReleaseInstanceAfterCall
        {
            get { return this.releaseInstanceAfterCall; }
        }

        internal bool ReleaseInstanceBeforeCall
        {
            get { return this.releaseInstanceBeforeCall; }
        }

        internal string ReplyAction
        {
            get { return this.replyAction; }
        }

        internal bool TransactionAutoComplete
        {
            get { return this.transactionAutoComplete; }
        }

        internal bool TransactionRequired
        {
            get { return this.transactionRequired; }
        }

        internal bool IsInsideTransactedReceiveScope
        {
            get { return this.isInsideTransactedReceiveScope; }
        }

        void DeserializeInputs(ref MessageRpc rpc)
        {
            bool success = false;
            try
            {
                try
                {
                    rpc.InputParameters = this.Invoker.AllocateInputs();
                }
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                    {
                        throw;
                    }
                    if (ErrorBehavior.ShouldRethrowExceptionAsIs(e))
                    {
                        throw;
                    }
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperCallback(e);
                }
                try
                {
                    // If the field is true, then this operation is to be invoked at the time the service 
                    // channel is opened. The incoming message is created at ChannelHandler level with no 
                    // content, so we don't need to deserialize the message.
                    if (!this.isSessionOpenNotificationEnabled)
                    {
                        if (this.deserializeRequest)
                        {
                            if (TD.DispatchFormatterDeserializeRequestStartIsEnabled())
                            {
                                TD.DispatchFormatterDeserializeRequestStart(rpc.EventTraceActivity);
                            }

                            this.Formatter.DeserializeRequest(rpc.Request, rpc.InputParameters);

                            if (TD.DispatchFormatterDeserializeRequestStopIsEnabled())
                            {
                                TD.DispatchFormatterDeserializeRequestStop(rpc.EventTraceActivity);
                            }
                        }
                        else
                        {
                            rpc.InputParameters[0] = rpc.Request;
                        }
                    }

                    success = true;
                }
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                    {
                        throw;
                    }
                    if (ErrorBehavior.ShouldRethrowExceptionAsIs(e))
                    {
                        throw;
                    }
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperCallback(e);
                }
            }
            finally
            {
                rpc.DidDeserializeRequestBody = (rpc.Request.State != MessageState.Created);

                if (!success && MessageLogger.LoggingEnabled)
                {
                    MessageLogger.LogMessage(ref rpc.Request, MessageLoggingSource.Malformed);
                }
            }
        }

        void InitializeCallContext(ref MessageRpc rpc)
        {
            if (this.CallContextInitializers.Length > 0)
            {
                InitializeCallContextCore(ref rpc);
            }
        }

        void InitializeCallContextCore(ref MessageRpc rpc)
        {
            IClientChannel channel = rpc.Channel.Proxy as IClientChannel;
            int offset = this.Parent.CallContextCorrelationOffset;

            try
            {
                for (int i = 0; i < rpc.Operation.CallContextInitializers.Length; i++)
                {
                    ICallContextInitializer initializer = this.CallContextInitializers[i];
                    rpc.Correlation[offset + i] = initializer.BeforeInvoke(rpc.InstanceContext, channel, rpc.Request);
                }
            }
            catch (Exception e)
            {
                if (Fx.IsFatal(e))
                {
                    throw;
                }
                if (ErrorBehavior.ShouldRethrowExceptionAsIs(e))
                {
                    throw;
                }
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperCallback(e);
            }
        }

        void UninitializeCallContext(ref MessageRpc rpc)
        {
            if (this.CallContextInitializers.Length > 0)
            {
                UninitializeCallContextCore(ref rpc);
            }
        }

        void UninitializeCallContextCore(ref MessageRpc rpc)
        {
            IClientChannel channel = rpc.Channel.Proxy as IClientChannel;
            int offset = this.Parent.CallContextCorrelationOffset;

            try
            {
                for (int i = this.CallContextInitializers.Length - 1; i >= 0; i--)
                {
                    ICallContextInitializer initializer = this.CallContextInitializers[i];
                    initializer.AfterInvoke(rpc.Correlation[offset + i]);
                }
            }
            catch (Exception e)
            {
                // thread-local storage may be corrupt
                DiagnosticUtility.FailFast(string.Format(CultureInfo.InvariantCulture, "ICallContextInitializer.BeforeInvoke threw an exception of type {0}: {1}", e.GetType(), e.Message));
            }
        }

        void InspectInputs(ref MessageRpc rpc)
        {
            if (this.ParameterInspectors.Length > 0)
            {
                InspectInputsCore(ref rpc);
            }
        }

        void InspectInputsCore(ref MessageRpc rpc)
        {
            int offset = this.Parent.ParameterInspectorCorrelationOffset;

            for (int i = 0; i < this.ParameterInspectors.Length; i++)
            {
                IParameterInspector inspector = this.ParameterInspectors[i];
                rpc.Correlation[offset + i] = inspector.BeforeCall(this.Name, rpc.InputParameters);
                if (TD.ParameterInspectorBeforeCallInvokedIsEnabled())
                {
                    TD.ParameterInspectorBeforeCallInvoked(rpc.EventTraceActivity, this.ParameterInspectors[i].GetType().FullName);
                }
            }
        }

        void InspectOutputs(ref MessageRpc rpc)
        {
            if (this.ParameterInspectors.Length > 0)
            {
                InspectOutputsCore(ref rpc);
            }
        }

        void InspectOutputsCore(ref MessageRpc rpc)
        {
            int offset = this.Parent.ParameterInspectorCorrelationOffset;

            for (int i = this.ParameterInspectors.Length - 1; i >= 0; i--)
            {
                IParameterInspector inspector = this.ParameterInspectors[i];
                inspector.AfterCall(this.Name, rpc.OutputParameters, rpc.ReturnParameter, rpc.Correlation[offset + i]);
                if (TD.ParameterInspectorAfterCallInvokedIsEnabled())
                {
                    TD.ParameterInspectorAfterCallInvoked(rpc.EventTraceActivity, this.ParameterInspectors[i].GetType().FullName);
                }
            }
        }

        [Fx.Tag.SecurityNote(Critical = "Calls SecurityCritical method StartImpersonation.",
            Safe = "Manages the result of impersonation and properly Disposes it.")]
        [DebuggerStepperBoundary]
        [SecuritySafeCritical]
        internal void InvokeBegin(ref MessageRpc rpc)
        {
            if (rpc.Error == null)
            {
                try
                {
                    this.InitializeCallContext(ref rpc);
                    object target = rpc.Instance;
                    this.DeserializeInputs(ref rpc);
                    this.InspectInputs(ref rpc);

                    ValidateMustUnderstand(ref rpc);

                    IAsyncResult result = null;
                    IDisposable impersonationContext = null;
                    IPrincipal originalPrincipal = null;
                    bool isThreadPrincipalSet = false;
                    bool isConcurrent = this.Parent.IsConcurrent(ref rpc);

                    try
                    {
                        if (this.parent.RequireClaimsPrincipalOnOperationContext)
                        {
                            SetClaimsPrincipalToOperationContext(rpc);
                        }
                       
                        if (this.parent.SecurityImpersonation != null)
                        {
                            this.parent.SecurityImpersonation.StartImpersonation(ref rpc, out impersonationContext, out originalPrincipal, out isThreadPrincipalSet);
                        }
                        IManualConcurrencyOperationInvoker manualInvoker = this.Invoker as IManualConcurrencyOperationInvoker;

                        if (this.isSynchronous)
                        {
                            if (manualInvoker != null && isConcurrent)
                            {
                                if (this.bufferedReceiveEnabled)
                                {
                                    rpc.OperationContext.IncomingMessageProperties.Add(
                                        BufferedReceiveMessageProperty.Name, new BufferedReceiveMessageProperty(ref rpc));
                                }
                                rpc.ReturnParameter = manualInvoker.Invoke(target, rpc.InputParameters, rpc.InvokeNotification, out rpc.OutputParameters);
                            }
                            else
                            {
                                rpc.ReturnParameter = this.Invoker.Invoke(target, rpc.InputParameters, out rpc.OutputParameters);
                            }
                        }
                        else
                        {
                            bool isBeginSuccessful = false;

                            if (manualInvoker != null && isConcurrent && this.bufferedReceiveEnabled)
                            {
                                // This will modify the rpc, it has to be done before rpc.Pause
                                // since IResumeMessageRpc implementation keeps reference of rpc.
                                // This is to ensure consistent rpc whether or not InvokeBegin completed
                                // synchronously or asynchronously.
                                rpc.OperationContext.IncomingMessageProperties.Add(
                                    BufferedReceiveMessageProperty.Name, new BufferedReceiveMessageProperty(ref rpc));
                            }

                            IResumeMessageRpc resumeRpc = rpc.Pause();
                            try
                            {
                                if (manualInvoker != null && isConcurrent)
                                {
                                    result = manualInvoker.InvokeBegin(target, rpc.InputParameters, rpc.InvokeNotification, invokeCallback, resumeRpc);
                                }
                                else
                                {
                                    result = this.Invoker.InvokeBegin(target, rpc.InputParameters, invokeCallback, resumeRpc);
                                }

                                isBeginSuccessful = true;
                                // if the call above actually went async, then responsibility to call 
                                // ProcessMessage{6,7,Cleanup} has been transferred to InvokeCallback
                            }
                            finally
                            {
                                if (!isBeginSuccessful)
                                {
                                    rpc.UnPause();
                                }
                            }
                        }
                    }
                    finally
                    {
                        try
                        {
                            if (this.parent.SecurityImpersonation != null)
                            {
                                this.parent.SecurityImpersonation.StopImpersonation(ref rpc, impersonationContext, originalPrincipal, isThreadPrincipalSet);
                            }
                        }
#pragma warning suppress 56500 // covered by FxCOP
                        catch
                        {
                            string message = null;
                            try
                            {
                                message = SR.GetString(SR.SFxRevertImpersonationFailed0);
                            }
                            finally
                            {
                                DiagnosticUtility.FailFast(message);
                            }
                        }
                    }

                    if (this.isSynchronous)
                    {
                        this.InspectOutputs(ref rpc);

                        this.SerializeOutputs(ref rpc);
                    }
                    else
                    {
                        if (result == null)
                        {
                            throw TraceUtility.ThrowHelperError(new ArgumentNullException("IOperationInvoker.BeginDispatch"), rpc.Request);
                        }

                        if (result.CompletedSynchronously)
                        {
                            // if the async call completed synchronously, then the responsibility to call
                            // ProcessMessage{6,7,Cleanup} still remains on this thread
                            rpc.UnPause();
                            rpc.AsyncResult = result;
                        }
                    }
                }
#pragma warning suppress 56500 // covered by FxCOP
                catch { throw; } // Make sure user Exception filters are not impersonated.
                finally
                {
                    this.UninitializeCallContext(ref rpc);
                }
            }
        }

        void SetClaimsPrincipalToOperationContext(MessageRpc rpc)
        {
            ServiceSecurityContext securityContext = rpc.SecurityContext;
            if (!rpc.HasSecurityContext)
            {
                SecurityMessageProperty securityContextProperty = rpc.Request.Properties.Security;
                if (securityContextProperty != null)
                {
                    securityContext = securityContextProperty.ServiceSecurityContext;
                }
            }

            if (securityContext != null)
            {
                object principal;
                if (securityContext.AuthorizationContext.Properties.TryGetValue(AuthorizationPolicy.ClaimsPrincipalKey, out principal))
                {
                    ClaimsPrincipal claimsPrincipal = principal as ClaimsPrincipal;
                    if (claimsPrincipal != null)
                    {
                        //
                        // Always set ClaimsPrincipal to OperationContext.Current if identityModel pipeline is used.
                        //
                        OperationContext.Current.ClaimsPrincipal = claimsPrincipal;
                    }
                    else
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.NoPrincipalSpecifiedInAuthorizationContext)));
                    }
                }
            }
        }

        static void InvokeCallback(IAsyncResult result)
        {
            if (result.CompletedSynchronously)
            {
                return;
            }

            IResumeMessageRpc resume = result.AsyncState as IResumeMessageRpc;

            if (resume == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(SR.GetString(SR.SFxInvalidAsyncResultState0));
            }

            resume.SignalConditionalResume(result);
        }

        [Fx.Tag.SecurityNote(Critical = "Calls SecurityCritical method StartImpersonation.",
            Safe = "Manages the result of impersonation and properly Disposes it.")]
        [DebuggerStepperBoundary]
        [SecuritySafeCritical]
        internal void InvokeEnd(ref MessageRpc rpc)
        {
            if ((rpc.Error == null) && !this.isSynchronous)
            {
                try
                {
                    this.InitializeCallContext(ref rpc);

                    if (this.parent.RequireClaimsPrincipalOnOperationContext)
                    {
                        SetClaimsPrincipalToOperationContext(rpc);
                    }

                    IDisposable impersonationContext = null;
                    IPrincipal originalPrincipal = null;
                    bool isThreadPrincipalSet = false;

                    try
                    {
                        if (this.parent.SecurityImpersonation != null)
                        {
                            this.parent.SecurityImpersonation.StartImpersonation(ref rpc, out impersonationContext, out originalPrincipal, out isThreadPrincipalSet);
                        }

                        rpc.ReturnParameter = this.Invoker.InvokeEnd(rpc.Instance, out rpc.OutputParameters, rpc.AsyncResult);
                    }
                    finally
                    {
                        try
                        {
                            if (this.parent.SecurityImpersonation != null)
                            {
                                this.parent.SecurityImpersonation.StopImpersonation(ref rpc, impersonationContext, originalPrincipal, isThreadPrincipalSet);
                            }
                        }
#pragma warning suppress 56500 // covered by FxCOP
                        catch
                        {
                            string message = null;
                            try
                            {
                                message = SR.GetString(SR.SFxRevertImpersonationFailed0);
                            }
                            finally
                            {
                                DiagnosticUtility.FailFast(message);
                            }
                        }
                    }

                    this.InspectOutputs(ref rpc);

                    this.SerializeOutputs(ref rpc);
                }
#pragma warning suppress 56500 // covered by FxCOP
                catch { throw; } // Make sure user Exception filters are not impersonated.
                finally
                {
                    this.UninitializeCallContext(ref rpc);
                }
            }
        }

        void SerializeOutputs(ref MessageRpc rpc)
        {
            if (!this.IsOneWay && this.parent.EnableFaults)
            {
                Message reply;
                if (this.serializeReply)
                {
                    try
                    {
                        if (TD.DispatchFormatterSerializeReplyStartIsEnabled())
                        {
                            TD.DispatchFormatterSerializeReplyStart(rpc.EventTraceActivity);
                        }
                        
                        reply = this.Formatter.SerializeReply(rpc.RequestVersion, rpc.OutputParameters, rpc.ReturnParameter);
                        
                        if (TD.DispatchFormatterSerializeReplyStopIsEnabled())
                        {
                            TD.DispatchFormatterSerializeReplyStop(rpc.EventTraceActivity);
                        }
                    }
                    catch (Exception e)
                    {
                        if (Fx.IsFatal(e))
                        {
                            throw;
                        }
                        if (ErrorBehavior.ShouldRethrowExceptionAsIs(e))
                        {
                            throw;
                        }
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperCallback(e);
                    }

                    if (reply == null)
                    {
                        string message = SR.GetString(SR.SFxNullReplyFromFormatter2, this.Formatter.GetType().ToString(), (this.name ?? ""));
                        ErrorBehavior.ThrowAndCatch(new InvalidOperationException(message));
                    }
                }
                else
                {
                    if ((rpc.ReturnParameter == null) && (rpc.OperationContext.RequestContext != null))
                    {
                        string message = SR.GetString(SR.SFxDispatchRuntimeMessageCannotBeNull, this.name);
                        ErrorBehavior.ThrowAndCatch(new InvalidOperationException(message));
                    }

                    reply = (Message)rpc.ReturnParameter;

                    if ((reply != null) && (!ProxyOperationRuntime.IsValidAction(reply, this.ReplyAction)))
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SFxInvalidReplyAction, this.Name, reply.Headers.Action ?? "{NULL}", this.ReplyAction)));
                    }
                }

                if (DiagnosticUtility.ShouldUseActivity && rpc.Activity != null && reply != null)
                {
                    TraceUtility.SetActivity(reply, rpc.Activity);
                    if (TraceUtility.ShouldPropagateActivity)
                    {
                        TraceUtility.AddActivityHeader(reply);
                    }
                }
                else if (TraceUtility.ShouldPropagateActivity && reply != null && rpc.ResponseActivityId != Guid.Empty)
                {
                    ActivityIdHeader header = new ActivityIdHeader(rpc.ResponseActivityId);
                    header.AddTo(reply);
                }

                //rely on the property set during the message receive to correlate the trace
                if (TraceUtility.MessageFlowTracingOnly)
                {
                    //Guard against MEX scenarios where the message is closed by now
                    if (null != rpc.OperationContext.IncomingMessage && MessageState.Closed != rpc.OperationContext.IncomingMessage.State)
                    {
                        FxTrace.Trace.SetAndTraceTransfer(TraceUtility.GetReceivedActivityId(rpc.OperationContext), true);
                    }
                    else
                    {
                        if (rpc.ResponseActivityId != Guid.Empty)
                        {
                            FxTrace.Trace.SetAndTraceTransfer(rpc.ResponseActivityId, true);
                        }
                    }
                }

                // Add the ImpersonateOnSerializingReplyMessageProperty on the reply message iff
                // a. reply message is not null.
                // b. Impersonation is enabled on serializing Reply

                if (reply != null && this.parent.IsImpersonationEnabledOnSerializingReply)
                {
                    bool shouldImpersonate = this.parent.SecurityImpersonation != null && this.parent.SecurityImpersonation.IsImpersonationEnabledOnCurrentOperation(ref rpc);
                    if (shouldImpersonate)
                    {
                        reply.Properties.Add(ImpersonateOnSerializingReplyMessageProperty.Name, new ImpersonateOnSerializingReplyMessageProperty(ref rpc));
                        reply = new ImpersonatingMessage(reply);
                    }
                }

                if (MessageLogger.LoggingEnabled && null != reply)
                {
                    MessageLogger.LogMessage(ref reply, MessageLoggingSource.ServiceLevelSendReply | MessageLoggingSource.LastChance);
                }
                rpc.Reply = reply;
            }
        }

        void ValidateInstanceType(Type type, MethodInfo method)
        {
            if (!method.DeclaringType.IsAssignableFrom(type))
            {
                string message = SR.GetString(SR.SFxMethodNotSupportedByType2,
                                              type.FullName,
                                              method.DeclaringType.FullName);

                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(message));
            }
        }

        void ValidateMustUnderstand(ref MessageRpc rpc)
        {
            if (parent.ValidateMustUnderstand)
            {
                rpc.NotUnderstoodHeaders = rpc.Request.Headers.GetHeadersNotUnderstood();
                if (rpc.NotUnderstoodHeaders != null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                        new MustUnderstandSoapException(rpc.NotUnderstoodHeaders, rpc.Request.Version.Envelope));
                }
            }
        }
    }
}
