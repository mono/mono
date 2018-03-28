//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.Diagnostics;
    using System.Reflection;
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Diagnostics;
    using System.ServiceModel.Diagnostics.Application;

    class InstanceBehavior
    {
        const BindingFlags DefaultBindingFlags = BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public;

        bool useSession;
        ServiceHostBase host;
        IInstanceContextInitializer[] initializers;
        IInstanceContextProvider instanceContextProvider;
        IInstanceProvider provider;
        InstanceContext singleton;
        bool transactionAutoCompleteOnSessionClose;
        bool releaseServiceInstanceOnTransactionComplete = true;
        bool isSynchronized;
        ImmutableDispatchRuntime immutableRuntime;

        internal InstanceBehavior(DispatchRuntime dispatch, ImmutableDispatchRuntime immutableRuntime)
        {
            this.useSession = dispatch.ChannelDispatcher.Session;
            this.immutableRuntime = immutableRuntime;
            this.host = (dispatch.ChannelDispatcher == null) ? null : dispatch.ChannelDispatcher.Host;
            this.initializers = EmptyArray<IInstanceContextInitializer>.ToArray(dispatch.InstanceContextInitializers);
            this.provider = dispatch.InstanceProvider;
            this.singleton = dispatch.SingletonInstanceContext;
            this.transactionAutoCompleteOnSessionClose = dispatch.TransactionAutoCompleteOnSessionClose;
            this.releaseServiceInstanceOnTransactionComplete = dispatch.ReleaseServiceInstanceOnTransactionComplete;
            this.isSynchronized = (dispatch.ConcurrencyMode != ConcurrencyMode.Multiple);
            this.instanceContextProvider = dispatch.InstanceContextProvider;

            if (this.provider == null)
            {
                ConstructorInfo constructor = null;
                if (dispatch.Type != null)
                {
                    constructor = InstanceBehavior.GetConstructor(dispatch.Type);
                }

                if (this.singleton == null)
                {
                    if (dispatch.Type != null && (dispatch.Type.IsAbstract || dispatch.Type.IsInterface))
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SFxServiceTypeNotCreatable)));
                    }

                    if (constructor == null)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SFxNoDefaultConstructor)));
                    }
                }

                if (constructor != null)
                {
                    if (this.singleton == null || !this.singleton.IsWellKnown)
                    {
                        InvokerUtil util = new InvokerUtil();
                        CreateInstanceDelegate creator = util.GenerateCreateInstanceDelegate(dispatch.Type, constructor);
                        this.provider = new InstanceProvider(creator);
                    }
                }
            }

            if (this.singleton != null)
            {
                this.singleton.Behavior = this;
            }
        }

        internal bool TransactionAutoCompleteOnSessionClose
        {
            get
            {
                return this.transactionAutoCompleteOnSessionClose;
            }
        }

        internal bool ReleaseServiceInstanceOnTransactionComplete
        {
            get
            {
                return this.releaseServiceInstanceOnTransactionComplete;
            }
        }

        internal IInstanceContextProvider InstanceContextProvider
        {
            get
            {
                return this.instanceContextProvider;
            }
        }

        internal void AfterReply(ref MessageRpc rpc, ErrorBehavior error)
        {
            InstanceContext context = rpc.InstanceContext;

            if (context != null)
            {
                try
                {
                    if (rpc.Operation.ReleaseInstanceAfterCall)
                    {
                        if (context.State == CommunicationState.Opened)
                        {
                            context.ReleaseServiceInstance();
                        }
                    }
                    else if (releaseServiceInstanceOnTransactionComplete &&
                            this.isSynchronized &&
                            rpc.transaction != null &&
                            (rpc.transaction.IsCompleted || (rpc.Error != null)))
                    {
                        if (context.State == CommunicationState.Opened)
                        {
                            context.ReleaseServiceInstance();
                        }
                        if (DiagnosticUtility.ShouldTraceInformation)
                        {
                            TraceUtility.TraceEvent(TraceEventType.Information,
                                                                         TraceCode.TxReleaseServiceInstanceOnCompletion,
                                                                         SR.GetString(SR.TraceCodeTxReleaseServiceInstanceOnCompletion, "*"));
                        }
                    }
                }
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                    {
                        throw;
                    }
                    error.HandleError(e);
                }

                try
                {
                    context.UnbindRpc(ref rpc);
                }
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                    {
                        throw;
                    }
                    error.HandleError(e);
                }
            }
        }

        internal bool CanUnload(InstanceContext instanceContext)
        {
            if (InstanceContextProviderBase.IsProviderSingleton(this.instanceContextProvider))
                return false;

            if (InstanceContextProviderBase.IsProviderPerCall(this.instanceContextProvider) ||
                InstanceContextProviderBase.IsProviderSessionful(this.instanceContextProvider))
                return true;

            //User provided InstanceContextProvider. Call the provider to check for idle.
            if (!this.instanceContextProvider.IsIdle(instanceContext))
            {
                this.instanceContextProvider.NotifyIdle(InstanceContext.NotifyIdleCallback, instanceContext);
                return false;
            }
            return true;
        }

        internal void EnsureInstanceContext(ref MessageRpc rpc)
        {
            if (rpc.InstanceContext == null)
            {
                rpc.InstanceContext = new InstanceContext(rpc.Host, false);
                rpc.InstanceContext.ServiceThrottle = rpc.channelHandler.InstanceContextServiceThrottle;
                rpc.MessageRpcOwnsInstanceContextThrottle = false;
            }

            rpc.OperationContext.SetInstanceContext(rpc.InstanceContext);
            rpc.InstanceContext.Behavior = this;

            if (rpc.InstanceContext.State == CommunicationState.Created)
            {
                lock (rpc.InstanceContext.ThisLock)
                {
                    if (rpc.InstanceContext.State == CommunicationState.Created)
                    {
                        rpc.InstanceContext.Open(rpc.Channel.CloseTimeout);
                    }
                }
            }
            rpc.InstanceContext.BindRpc(ref rpc);
        }

        static ConstructorInfo GetConstructor(Type type)
        {
            return type.GetConstructor(DefaultBindingFlags, null, Type.EmptyTypes, null);
        }

        internal object GetInstance(InstanceContext instanceContext)
        {
            if (this.provider == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SFxNoDefaultConstructor)));
            }

            return this.provider.GetInstance(instanceContext);
        }

        internal object GetInstance(InstanceContext instanceContext, Message request)
        {
            if (this.provider == null)
            {
                throw TraceUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SFxNoDefaultConstructor)), request);
            }

            return this.provider.GetInstance(instanceContext, request);
        }

        internal void Initialize(InstanceContext instanceContext)
        {
            OperationContext current = OperationContext.Current;
            Message message = (current != null) ? current.IncomingMessage : null;

            if (current != null && current.InternalServiceChannel != null)
            {
                IContextChannel transparentProxy = (IContextChannel)current.InternalServiceChannel.Proxy;
                this.instanceContextProvider.InitializeInstanceContext(instanceContext, message, transparentProxy);
            }

            for (int i = 0; i < this.initializers.Length; i++)
                this.initializers[i].Initialize(instanceContext, message);
        }

        internal void EnsureServiceInstance(ref MessageRpc rpc)
        {
            if (rpc.Operation.ReleaseInstanceBeforeCall)
            {
                rpc.InstanceContext.ReleaseServiceInstance();
            }

            if (TD.GetServiceInstanceStartIsEnabled())
            {
                TD.GetServiceInstanceStart(rpc.EventTraceActivity);
            }

            rpc.Instance = rpc.InstanceContext.GetServiceInstance(rpc.Request);

            if (TD.GetServiceInstanceStopIsEnabled())
            {
                TD.GetServiceInstanceStop(rpc.EventTraceActivity);
            }
        }

        internal void ReleaseInstance(InstanceContext instanceContext, object instance)
        {
            if (this.provider != null)
            {
                try
                {
                    this.provider.ReleaseInstance(instanceContext, instance);
                }
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                    {
                        throw;
                    }
                    this.immutableRuntime.ErrorBehavior.HandleError(e);
                }
            }
        }
    }

    class InstanceProvider : IInstanceProvider
    {
        CreateInstanceDelegate creator;

        internal InstanceProvider(CreateInstanceDelegate creator)
        {
            if (creator == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("creator");

            this.creator = creator;
        }

        public object GetInstance(InstanceContext instanceContext)
        {
            return this.creator();
        }

        public object GetInstance(InstanceContext instanceContext, Message message)
        {
            return this.creator();
        }

        public void ReleaseInstance(InstanceContext instanceContext, object instance)
        {
            IDisposable dispose = instance as IDisposable;
            if (dispose != null)
                dispose.Dispose();
        }
    }
}
