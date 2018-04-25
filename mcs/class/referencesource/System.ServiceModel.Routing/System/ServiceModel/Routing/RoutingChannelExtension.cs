//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Routing
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Dispatcher;
    using System.ServiceModel.Channels;
    using System.Collections.Generic;
    using System.Threading;

    abstract class RoutingChannelExtension : IExtension<IContextChannel>
    {
        static AsyncCallback closeChannelsCallback = Fx.ThunkCallback(CloseChannelsCallback);        
        static AsyncCallback shutdownCallback = Fx.ThunkCallback(ShutdownCallback);
        IContextChannel channel;
        bool hasSession;
        RoutingBehavior.RoutingEndpointBehavior endpointBehavior;
        RoutingService sessionService;
        volatile SessionChannels sessionChannels;
        [Fx.Tag.SynchronizationObject]
        object thisLock;

        public RoutingChannelExtension(RoutingBehavior.RoutingEndpointBehavior endpointBehavior)
        {
            this.ActivityID = Guid.NewGuid();
            this.endpointBehavior = endpointBehavior;
            this.thisLock = new object();
        }

        internal Guid ActivityID
        {
            get;
            private set;
        }

        public string EndpointName
        {
            get { return this.endpointBehavior.EndpointName; }
        }

        public bool HasSession
        {
            get { return this.hasSession; }
        }

        public bool ImpersonationRequired
        {
            get { return this.endpointBehavior.ImpersonationRequired; }
        }

        public TimeSpan OperationTimeout
        {
            get;
            private set;
        }

        public bool ReceiveContextEnabled
        {
            get { return this.endpointBehavior.ReceiveContextEnabled; }
        }

        [SuppressMessage(FxCop.Category.Performance, FxCop.Rule.AvoidUncalledPrivateCode, Justification = "get_SessionChannels is called by RoutingService..ctor")]
        public SessionChannels SessionChannels
        {
            get
            {
                if (this.sessionChannels == null)
                {
                    lock (this.thisLock)
                    {
                        if (this.sessionChannels == null)
                        {
                            Fx.AssertAndThrow(!(this.ImpersonationRequired && !this.HasSession), "Shouldn't allocate SessionChannels if session-less and impersonating");
                            this.sessionChannels = new SessionChannels(this.ActivityID);
                        }
                    }
                }
                return this.sessionChannels;
            }
        }

        public bool TransactedReceiveEnabled
        {
            get { return this.endpointBehavior.TransactedReceiveEnabled; }
        }

        [SuppressMessage(FxCop.Category.Performance, FxCop.Rule.AvoidUncalledPrivateCode, Justification = "AttachService is called by RoutingService..ctor")]
        public void AttachService(RoutingService service)
        {
            SessionChannels channelsToClose = null;
            lock (this.thisLock)
            {
                if (!this.hasSession)
                {
                    RoutingConfiguration oldConfig = null;
                    if (this.sessionService != null)
                    {
                        oldConfig = this.sessionService.RoutingConfig;
                    }

                    if (oldConfig != null && !object.ReferenceEquals(service.RoutingConfig, oldConfig))
                    {
                        //The RoutingConfiguration has changed.  We need to release any old channels that are cached.
                        channelsToClose = this.sessionChannels;
                        this.sessionChannels = null;
                    }
                }
                else
                {
                    Fx.Assert(this.sessionService == null, "There must only be one RoutingService created for a sessionful channel");
                }
                this.sessionService = service;
            }

            if (channelsToClose != null)
            {
                channelsToClose.BeginClose(this.channel.OperationTimeout, closeChannelsCallback, channelsToClose);
            }
        }

        public abstract IAsyncResult BeginShutdown(RoutingService service, TimeSpan timeout, AsyncCallback callback, object state);

        static void CloseChannelsCallback(IAsyncResult asyncResult)
        {
            SessionChannels channelsToClose = (SessionChannels)asyncResult.AsyncState;
            Exception exception = null;
            try
            {
                channelsToClose.EndClose(asyncResult);
            }
            catch (CommunicationException communicationException)
            {
                exception = communicationException;
            }
            catch (TimeoutException timeoutException)
            {
                exception = timeoutException;
            }

            if (exception != null && TD.RoutingServiceHandledExceptionIsEnabled())
            {
                TD.RoutingServiceHandledException(null, exception);
            }
        }

        static void ShutdownCallback(IAsyncResult result)
        {
            if (result.CompletedSynchronously)
            {
                return;
            }

            RoutingChannelExtension thisPtr = (RoutingChannelExtension)result.AsyncState;
            try
            {
                thisPtr.ShutdownComplete(result);
            }
            catch (Exception exception)
            {
                if (Fx.IsFatal(exception))
                {
                    throw;
                }

                thisPtr.Fault(exception);
            }
        }

        void ShutdownComplete(IAsyncResult result)
        {
            this.EndShutdown(result);
            this.channel.Close();
        }

        public static RoutingChannelExtension Create(RoutingBehavior.RoutingEndpointBehavior endpointBehavior)
        {
            Type contractType = endpointBehavior.Endpoint.Contract.ContractType;
            if (contractType == typeof(IDuplexSessionRouter))
            {
                return new RoutingChannelExtension<IDuplexSessionRouter>(endpointBehavior);
            }
            else if (contractType == typeof(ISimplexDatagramRouter))
            {
                return new RoutingChannelExtension<ISimplexDatagramRouter>(endpointBehavior);
            }
            else if (contractType == typeof(IRequestReplyRouter))
            {
                return new RoutingChannelExtension<IRequestReplyRouter>(endpointBehavior);
            }
            else
            {
                Fx.Assert(contractType == typeof(ISimplexSessionRouter), "Was a new contract added?");
                return new RoutingChannelExtension<ISimplexSessionRouter>(endpointBehavior);
            }
        }

        public void DoneReceiving(TimeSpan closeTimeout)
        {
            FxTrace.Trace.SetAndTraceTransfer(this.ActivityID, true);
            try
            {
                if (this.sessionService != null)
                {
                    IAsyncResult result = this.BeginShutdown(this.sessionService, closeTimeout, shutdownCallback, this);
                    if (result.CompletedSynchronously)
                    {
                        this.ShutdownComplete(result);
                    }
                }
                else
                {
                    this.channel.Close();
                }
            }
            catch (Exception exception)
            {
                if (Fx.IsFatal(exception))
                {
                    throw;
                }
                this.Fault(exception);
            }
        }

        public abstract void EndShutdown(IAsyncResult result);

        public void Fault(Exception exception)
        {
            FxTrace.Trace.SetAndTraceTransfer(this.ActivityID, true);

            //Notify the error handlers that a problem occurred
            foreach (IErrorHandler errorHandler in this.endpointBehavior.ChannelDispatcher.ErrorHandlers)
            {
                if (errorHandler.HandleError(exception))
                {
                    break;
                }
            }

            SessionChannels channelsToAbort;
            lock (this.thisLock)
            {
                channelsToAbort = this.sessionChannels;
                this.sessionChannels = null;
            }

            if (channelsToAbort != null)
            {
                channelsToAbort.AbortAll();
            }

            RoutingUtilities.Abort(this.channel, this.channel.LocalAddress);
        }

        void IExtension<IContextChannel>.Attach(IContextChannel owner)
        {
            this.channel = owner;
            this.hasSession = (owner.InputSession != null);
            this.OperationTimeout = owner.OperationTimeout;
        }

        void IExtension<IContextChannel>.Detach(IContextChannel owner)
        {
        }
    }

    sealed class RoutingChannelExtension<T> : RoutingChannelExtension
    {
        public RoutingChannelExtension(RoutingBehavior.RoutingEndpointBehavior endpointBehavior)
            : base(endpointBehavior)
        {
        }

        public override IAsyncResult BeginShutdown(RoutingService service, TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new ProcessMessagesAsyncResult<T>(null, service, timeout, callback, state);
        }

        public override void EndShutdown(IAsyncResult result)
        {
            ProcessMessagesAsyncResult<T>.End(result);
        }
    }
}
