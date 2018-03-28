//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.IdentityModel.Policy;
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.ServiceModel.Diagnostics;
    using System.Threading;
    using System.Web.Security;
    using System.Runtime.Diagnostics;

    public sealed class DispatchRuntime
    {
        ServiceAuthenticationManager serviceAuthenticationManager;
        ServiceAuthorizationManager serviceAuthorizationManager;
        ReadOnlyCollection<IAuthorizationPolicy> externalAuthorizationPolicies;
        AuditLogLocation securityAuditLogLocation;
        ConcurrencyMode concurrencyMode;
        bool ensureOrderedDispatch;
        bool suppressAuditFailure;
        AuditLevel serviceAuthorizationAuditLevel;
        AuditLevel messageAuthenticationAuditLevel;
        bool automaticInputSessionShutdown;
        ChannelDispatcher channelDispatcher;
        SynchronizedCollection<IInputSessionShutdown> inputSessionShutdownHandlers;
        EndpointDispatcher endpointDispatcher;
        IInstanceProvider instanceProvider;
        IInstanceContextProvider instanceContextProvider;
        InstanceContext singleton;
        bool ignoreTransactionMessageProperty;
        SynchronizedCollection<IDispatchMessageInspector> messageInspectors;
        OperationCollection operations;
        IDispatchOperationSelector operationSelector;
        ClientRuntime proxyRuntime;
        ImmutableDispatchRuntime runtime;
        SynchronizedCollection<IInstanceContextInitializer> instanceContextInitializers;
        bool isExternalPoliciesSet;
        bool isAuthenticationManagerSet;
        bool isAuthorizationManagerSet;
        SynchronizationContext synchronizationContext;
        PrincipalPermissionMode principalPermissionMode;
        object roleProvider;
        Type type;
        DispatchOperation unhandled;
        bool transactionAutoCompleteOnSessionClose;
        bool impersonateCallerForAllOperations;
        bool impersonateOnSerializingReply;
        bool releaseServiceInstanceOnTransactionComplete;
        SharedRuntimeState shared;
        bool preserveMessage;
        bool requireClaimsPrincipalOnOperationContext;

        internal DispatchRuntime(EndpointDispatcher endpointDispatcher)
            : this(new SharedRuntimeState(true))
        {
            if (endpointDispatcher == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("endpointDispatcher");
            }

            this.endpointDispatcher = endpointDispatcher;

            Fx.Assert(shared.IsOnServer, "Server constructor called on client?");
        }

        internal DispatchRuntime(ClientRuntime proxyRuntime, SharedRuntimeState shared)
            : this(shared)
        {
            if (proxyRuntime == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("proxyRuntime");
            }

            this.proxyRuntime = proxyRuntime;
            this.instanceProvider = new CallbackInstanceProvider();
            this.channelDispatcher = new ChannelDispatcher(shared);
            this.instanceContextProvider = InstanceContextProviderBase.GetProviderForMode(InstanceContextMode.PerSession, this);

            Fx.Assert(!shared.IsOnServer, "Client constructor called on server?");
        }

        DispatchRuntime(SharedRuntimeState shared)
        {
            this.shared = shared;

            this.operations = new OperationCollection(this);

            this.inputSessionShutdownHandlers = this.NewBehaviorCollection<IInputSessionShutdown>();
            this.messageInspectors = this.NewBehaviorCollection<IDispatchMessageInspector>();
            this.instanceContextInitializers = this.NewBehaviorCollection<IInstanceContextInitializer>();
            this.synchronizationContext = ThreadBehavior.GetCurrentSynchronizationContext();

            this.automaticInputSessionShutdown = true;
            this.principalPermissionMode = ServiceAuthorizationBehavior.DefaultPrincipalPermissionMode;

            this.securityAuditLogLocation = ServiceSecurityAuditBehavior.defaultAuditLogLocation;
            this.suppressAuditFailure = ServiceSecurityAuditBehavior.defaultSuppressAuditFailure;
            this.serviceAuthorizationAuditLevel = ServiceSecurityAuditBehavior.defaultServiceAuthorizationAuditLevel;
            this.messageAuthenticationAuditLevel = ServiceSecurityAuditBehavior.defaultMessageAuthenticationAuditLevel;

            this.unhandled = new DispatchOperation(this, "*", MessageHeaders.WildcardAction, MessageHeaders.WildcardAction);
            this.unhandled.InternalFormatter = MessageOperationFormatter.Instance;
            this.unhandled.InternalInvoker = new UnhandledActionInvoker(this);
        }

        public IInstanceContextProvider InstanceContextProvider
        {
            get
            {
                return this.instanceContextProvider;
            }

            set
            {
                if (value == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("value"));
                }

                lock (this.ThisLock)
                {
                    this.InvalidateRuntime();
                    this.instanceContextProvider = value;
                }
            }
        }

        public InstanceContext SingletonInstanceContext
        {
            get { return this.singleton; }
            set
            {
                if (value == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("value"));
                }

                lock (this.ThisLock)
                {
                    this.InvalidateRuntime();
                    this.singleton = value;
                }
            }
        }

        public ConcurrencyMode ConcurrencyMode
        {
            get
            {
                return this.concurrencyMode;
            }
            set
            {
                lock (this.ThisLock)
                {
                    this.InvalidateRuntime();
                    this.concurrencyMode = value;
                }
            }
        }

        public bool EnsureOrderedDispatch
        {
            get
            {
                return this.ensureOrderedDispatch;
            }
            set
            {
                lock (this.ThisLock)
                {
                    this.InvalidateRuntime();
                    this.ensureOrderedDispatch = value;
                }
            }
        }

        public AuditLogLocation SecurityAuditLogLocation
        {
            get
            {
                return this.securityAuditLogLocation;
            }
            set
            {
                if (!AuditLogLocationHelper.IsDefined(value))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value"));
                }

                lock (this.ThisLock)
                {
                    this.InvalidateRuntime();
                    this.securityAuditLogLocation = value;
                }
            }
        }

        public bool SuppressAuditFailure
        {
            get
            {
                return this.suppressAuditFailure;
            }
            set
            {
                lock (this.ThisLock)
                {
                    this.InvalidateRuntime();
                    this.suppressAuditFailure = value;
                }
            }
        }

        public AuditLevel ServiceAuthorizationAuditLevel
        {
            get
            {
                return this.serviceAuthorizationAuditLevel;
            }
            set
            {
                if (!AuditLevelHelper.IsDefined(value))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value"));
                }

                lock (this.ThisLock)
                {
                    this.InvalidateRuntime();
                    this.serviceAuthorizationAuditLevel = value;
                }
            }
        }

        public AuditLevel MessageAuthenticationAuditLevel
        {
            get
            {
                return this.messageAuthenticationAuditLevel;
            }
            set
            {
                if (!AuditLevelHelper.IsDefined(value))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value"));
                }

                lock (this.ThisLock)
                {
                    this.InvalidateRuntime();
                    this.messageAuthenticationAuditLevel = value;
                }
            }
        }

        public ReadOnlyCollection<IAuthorizationPolicy> ExternalAuthorizationPolicies
        {
            get
            {
                return this.externalAuthorizationPolicies;
            }
            set
            {
                lock (this.ThisLock)
                {
                    this.InvalidateRuntime();
                    this.externalAuthorizationPolicies = value;
                    this.isExternalPoliciesSet = true;
                }
            }
        }

        public ServiceAuthenticationManager ServiceAuthenticationManager
        {
            get
            {
                return this.serviceAuthenticationManager;
            }
            set
            {
                lock (this.ThisLock)
                {
                    this.InvalidateRuntime();
                    this.serviceAuthenticationManager = value;
                    this.isAuthenticationManagerSet = true;
                }
            }
        }

        public ServiceAuthorizationManager ServiceAuthorizationManager
        {
            get
            {
                return this.serviceAuthorizationManager;
            }
            set
            {
                lock (this.ThisLock)
                {
                    this.InvalidateRuntime();
                    this.serviceAuthorizationManager = value;
                    this.isAuthorizationManagerSet = true;
                }
            }
        }

        public bool AutomaticInputSessionShutdown
        {
            get { return this.automaticInputSessionShutdown; }        
            set
            {
                lock (this.ThisLock)
                {
                    this.InvalidateRuntime();
                    this.automaticInputSessionShutdown = value;
                }
            }
        }

        public ChannelDispatcher ChannelDispatcher
        {
            get { return this.channelDispatcher ?? this.endpointDispatcher.ChannelDispatcher; }
        }

        public ClientRuntime CallbackClientRuntime
        {
            get
            {
                if (this.proxyRuntime == null)
                {
                    lock (this.ThisLock)
                    {
                        if (this.proxyRuntime == null)
                        {
                            this.proxyRuntime = new ClientRuntime(this, this.shared);
                        }
                    }
                }

                return this.proxyRuntime;
            }
        }

        public EndpointDispatcher EndpointDispatcher
        {
            get { return this.endpointDispatcher; }
        }

        public bool ImpersonateCallerForAllOperations
        {
            get
            {
                return this.impersonateCallerForAllOperations;
            }
            set
            {
                lock (this.ThisLock)
                {
                    this.InvalidateRuntime();
                    this.impersonateCallerForAllOperations = value;
                }
            }
        }

        public bool ImpersonateOnSerializingReply
        {
            get
            {
                return this.impersonateOnSerializingReply;
            }
            set
            {
                lock (this.ThisLock)
                {
                    this.InvalidateRuntime();
                    this.impersonateOnSerializingReply = value;
                }
            }
        }

        internal bool RequireClaimsPrincipalOnOperationContext
        {
            get
            {
                return this.requireClaimsPrincipalOnOperationContext;
            }
            set
            {
                lock (this.ThisLock)
                {
                    this.InvalidateRuntime();
                    this.requireClaimsPrincipalOnOperationContext = value;
                }
            }
        }

        public SynchronizedCollection<IInputSessionShutdown> InputSessionShutdownHandlers
        {
            get { return this.inputSessionShutdownHandlers; }
        }

        public bool IgnoreTransactionMessageProperty
        {
            get { return this.ignoreTransactionMessageProperty; }
            set
            {
                lock (this.ThisLock)
                {
                    this.InvalidateRuntime();
                    this.ignoreTransactionMessageProperty = value;
                }
            }
        }

        public IInstanceProvider InstanceProvider
        {
            get { return this.instanceProvider; }
            set
            {
                lock (this.ThisLock)
                {
                    this.InvalidateRuntime();
                    this.instanceProvider = value;
                }
            }
        }

        public SynchronizedCollection<IDispatchMessageInspector> MessageInspectors
        {
            get { return this.messageInspectors; }
        }

        public SynchronizedKeyedCollection<string, DispatchOperation> Operations
        {
            get { return this.operations; }
        }

        public IDispatchOperationSelector OperationSelector
        {
            get { return this.operationSelector; }
            set
            {
                lock (this.ThisLock)
                {
                    this.InvalidateRuntime();
                    this.operationSelector = value;
                }
            }
        }

        public bool ReleaseServiceInstanceOnTransactionComplete
        {
            get { return this.releaseServiceInstanceOnTransactionComplete; }
            set
            {
                lock (this.ThisLock)
                {
                    this.InvalidateRuntime();
                    this.releaseServiceInstanceOnTransactionComplete = value;
                }
            }
        }       

        public SynchronizedCollection<IInstanceContextInitializer> InstanceContextInitializers
        {
            get { return this.instanceContextInitializers; }
        }

        public SynchronizationContext SynchronizationContext
        {
            get { return this.synchronizationContext; }
            set
            {
                lock (this.ThisLock)
                {
                    this.InvalidateRuntime();
                    this.synchronizationContext = value;
                }
            }
        }

        public PrincipalPermissionMode PrincipalPermissionMode
        {
            get
            {
                return this.principalPermissionMode;
            }
            set
            {
                if (!PrincipalPermissionModeHelper.IsDefined(value))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value"));
                }

                lock (this.ThisLock)
                {
                    this.InvalidateRuntime();
                    this.principalPermissionMode = value;
                }
            }
        }

        public RoleProvider RoleProvider
        {
            get { return (RoleProvider)this.roleProvider; }
            set
            {
                lock (this.ThisLock)
                {
                    this.InvalidateRuntime();
                    this.roleProvider = value;
                }
            }
        }

        public bool TransactionAutoCompleteOnSessionClose
        {
            get { return this.transactionAutoCompleteOnSessionClose; }
            set
            {
                lock (this.ThisLock)
                {
                    this.InvalidateRuntime();
                    this.transactionAutoCompleteOnSessionClose = value;
                }
            }
        }

        public Type Type
        {
            get { return this.type; }
            set
            {
                lock (this.ThisLock)
                {
                    this.InvalidateRuntime();
                    this.type = value;
                }
            }
        }

        public DispatchOperation UnhandledDispatchOperation
        {
            get { return this.unhandled; }
            set
            {
                if (value == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");
                }

                lock (this.ThisLock)
                {
                    this.InvalidateRuntime();
                    this.unhandled = value;
                }
            }
        }

        public bool ValidateMustUnderstand
        {
            get { return this.shared.ValidateMustUnderstand; }
            set
            {
                lock (this.ThisLock)
                {
                    this.InvalidateRuntime();
                    this.shared.ValidateMustUnderstand = value;
                }
            }
        }

        public bool PreserveMessage
        {
            get { return this.preserveMessage; }
            set
            {
                lock (this.ThisLock)
                {
                    this.InvalidateRuntime();
                    this.preserveMessage = value;
                }
            }
        }

        internal bool RequiresAuthentication
        {
            get
            {
                return this.isAuthenticationManagerSet;
            }
        }

        internal bool RequiresAuthorization
        {
            get
            {
                return (this.isAuthorizationManagerSet || this.isExternalPoliciesSet ||
                    AuditLevel.Success == (this.serviceAuthorizationAuditLevel & AuditLevel.Success));
            }
        }

        internal bool HasMatchAllOperation
        {
            get
            {
                lock (this.ThisLock)
                {
                    return !(this.unhandled.Invoker is UnhandledActionInvoker);
                }
            }
        }

        internal bool EnableFaults
        {
            get
            {
                if (this.IsOnServer)
                {
                    ChannelDispatcher channelDispatcher = this.ChannelDispatcher;
                    return (channelDispatcher != null) && channelDispatcher.EnableFaults;
                }
                else
                {
                    return this.shared.EnableFaults;
                }
            }
        }

        internal bool IsOnServer
        {
            get { return this.shared.IsOnServer; }
        }

        internal bool ManualAddressing
        {
            get
            {
                if (this.IsOnServer)
                {
                    ChannelDispatcher channelDispatcher = this.ChannelDispatcher;
                    return (channelDispatcher != null) && channelDispatcher.ManualAddressing;
                }
                else
                {
                    return this.shared.ManualAddressing;
                }
            }
        }

        internal int MaxCallContextInitializers
        {
            get
            {
                lock (this.ThisLock)
                {
                    int max = 0;

                    for (int i = 0; i < this.operations.Count; i++)
                    {
                        max = System.Math.Max(max, this.operations[i].CallContextInitializers.Count);
                    }
                    max = System.Math.Max(max, this.unhandled.CallContextInitializers.Count);
                    return max;
                }
            }
        }

        internal int MaxParameterInspectors
        {
            get
            {
                lock (this.ThisLock)
                {
                    int max = 0;

                    for (int i = 0; i < this.operations.Count; i++)
                    {
                        max = System.Math.Max(max, this.operations[i].ParameterInspectors.Count);
                    }
                    max = System.Math.Max(max, this.unhandled.ParameterInspectors.Count);
                    return max;
                }
            }
        }

        // Internal access to CallbackClientRuntime, but this one doesn't create on demand
        internal ClientRuntime ClientRuntime
        {
            get { return this.proxyRuntime; }
        }

        internal object ThisLock
        {
            get { return this.shared; }
        }

        internal bool IsRoleProviderSet
        {
            get { return this.roleProvider != null; }
        }

        internal DispatchOperationRuntime GetOperation(ref Message message)
        {
            ImmutableDispatchRuntime runtime = this.GetRuntime();
            return runtime.GetOperation(ref message);
        }

        internal ImmutableDispatchRuntime GetRuntime()
        {
            ImmutableDispatchRuntime runtime = this.runtime;
            if (runtime != null)
            {
                return runtime;
            }
            else
            {
                return GetRuntimeCore();
            }
        }

        ImmutableDispatchRuntime GetRuntimeCore()
        {
            lock (this.ThisLock)
            {
                if (this.runtime == null)
                {
                    this.runtime = new ImmutableDispatchRuntime(this);
                }

                return this.runtime;
            }
        }

        internal void InvalidateRuntime()
        {
            lock (this.ThisLock)
            {
                this.shared.ThrowIfImmutable();
                this.runtime = null;
            }
        }

        internal void LockDownProperties()
        {
            this.shared.LockDownProperties();
            if (this.concurrencyMode != ConcurrencyMode.Single && this.ensureOrderedDispatch)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SfxDispatchRuntimeNonConcurrentOrEnsureOrderedDispatch)));
            }
        }

        internal SynchronizedCollection<T> NewBehaviorCollection<T>()
        {
            return new DispatchBehaviorCollection<T>(this);
        }

        internal void SetDebugFlagInDispatchOperations(bool includeExceptionDetailInFaults)
        {
            foreach (DispatchOperation dispatchOperation in this.operations)
            {
                dispatchOperation.IncludeExceptionDetailInFaults = includeExceptionDetailInFaults;
            }
        }

        internal class UnhandledActionInvoker : IOperationInvoker
        {
            DispatchRuntime dispatchRuntime;

            public UnhandledActionInvoker(DispatchRuntime dispatchRuntime)
            {
                this.dispatchRuntime = dispatchRuntime;
            }

            public bool IsSynchronous
            {
                get { return true; }
            }

            public object[] AllocateInputs()
            {
                return new object[1];
            }

            public object Invoke(object instance, object[] inputs, out object[] outputs)
            {
                outputs = EmptyArray<object>.Allocate(0);

                Message message = inputs[0] as Message;
                if (message == null)
                {
                    return null;
                }

                string action = message.Headers.Action;

                if (DiagnosticUtility.ShouldTraceInformation)
                {
                    TraceUtility.TraceEvent(TraceEventType.Information, TraceCode.UnhandledAction,
                        SR.GetString(SR.TraceCodeUnhandledAction),
                        new StringTraceRecord("Action", action),
                        this, null, message);
                }

                FaultCode code = FaultCode.CreateSenderFaultCode(AddressingStrings.ActionNotSupported,
                    message.Version.Addressing.Namespace);
                string reasonText = SR.GetString(SR.SFxNoEndpointMatchingContract, action);
                FaultReason reason = new FaultReason(reasonText);

                FaultException exception = new FaultException(reason, code);
                ErrorBehavior.ThrowAndCatch(exception);

                ServiceChannel serviceChannel = OperationContext.Current.InternalServiceChannel;
                OperationContext.Current.OperationCompleted += 
                    delegate(object sender, EventArgs e) 
                {
                    ChannelDispatcher channelDispatcher = this.dispatchRuntime.ChannelDispatcher;
                    if (!channelDispatcher.HandleError(exception) && serviceChannel.HasSession)
                    {
                        try
                        {
                            serviceChannel.Close(ChannelHandler.CloseAfterFaultTimeout); 
                        }
                        catch (Exception ex)
                        {
                            if (Fx.IsFatal(ex))
                            {
                                throw;
                            }
                            channelDispatcher.HandleError(ex);
                        }
                    }
                };

                if (this.dispatchRuntime.shared.EnableFaults)
                {
                    MessageFault fault = MessageFault.CreateFault(code, reason, action);
                    return Message.CreateMessage(message.Version, fault, message.Version.Addressing.DefaultFaultAction);
                }
                else
                {
                    OperationContext.Current.RequestContext.Close();
                    OperationContext.Current.RequestContext = null;
                    return null;
                }
            }

            public IAsyncResult InvokeBegin(object instance, object[] inputs, AsyncCallback callback, object state)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotImplementedException());
            }

            public object InvokeEnd(object instance, out object[] outputs, IAsyncResult result)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotImplementedException());
            }
        }

        class DispatchBehaviorCollection<T> : SynchronizedCollection<T>
        {
            DispatchRuntime outer;

            internal DispatchBehaviorCollection(DispatchRuntime outer)
                : base(outer.ThisLock)
            {
                this.outer = outer;
            }

            protected override void ClearItems()
            {
                this.outer.InvalidateRuntime();
                base.ClearItems();
            }

            protected override void InsertItem(int index, T item)
            {
                if (item == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("item");
                }

                this.outer.InvalidateRuntime();
                base.InsertItem(index, item);
            }

            protected override void RemoveItem(int index)
            {
                this.outer.InvalidateRuntime();
                base.RemoveItem(index);
            }

            protected override void SetItem(int index, T item)
            {
                if (item == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("item");
                }

                this.outer.InvalidateRuntime();
                base.SetItem(index, item);
            }
        }

        class OperationCollection : SynchronizedKeyedCollection<string, DispatchOperation>
        {
            DispatchRuntime outer;

            internal OperationCollection(DispatchRuntime outer)
                : base(outer.ThisLock)
            {
                this.outer = outer;
            }

            protected override void ClearItems()
            {
                this.outer.InvalidateRuntime();
                base.ClearItems();
            }

            protected override string GetKeyForItem(DispatchOperation item)
            {
                return item.Name;
            }

            protected override void InsertItem(int index, DispatchOperation item)
            {
                if (item == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("item");
                }
                if (item.Parent != this.outer)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(SR.GetString(SR.SFxMismatchedOperationParent));
                }

                this.outer.InvalidateRuntime();
                base.InsertItem(index, item);
            }

            protected override void RemoveItem(int index)
            {
                this.outer.InvalidateRuntime();
                base.RemoveItem(index);
            }

            protected override void SetItem(int index, DispatchOperation item)
            {
                if (item == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("item");
                }
                if (item.Parent != this.outer)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(SR.GetString(SR.SFxMismatchedOperationParent));
                }

                this.outer.InvalidateRuntime();
                base.SetItem(index, item);
            }
        }

        class CallbackInstanceProvider : IInstanceProvider
        {
            object IInstanceProvider.GetInstance(InstanceContext instanceContext)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SFxCannotActivateCallbackInstace)));
            }

            object IInstanceProvider.GetInstance(InstanceContext instanceContext, Message message)
            {
                throw TraceUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SFxCannotActivateCallbackInstace)), message);
            }

            void IInstanceProvider.ReleaseInstance(InstanceContext instanceContext, object instance)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SFxCannotActivateCallbackInstace)));
            }
        }
    }
}
