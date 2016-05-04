//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Discovery
{
    using System;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Globalization;
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.ServiceModel.Discovery.Configuration;
    using System.Threading;
    using System.Xml;
    using SR2 = System.ServiceModel.Discovery.SR;
    using System.Runtime.Diagnostics;
    using System.ServiceModel.Diagnostics;
    using System.Threading.Tasks;

    [Fx.Tag.XamlVisible(false)]
    public sealed class DiscoveryClient : ICommunicationObject, IDiscoveryInnerClientResponse, IDisposable
    {
        static TimeSpan defaultCloseDuration = TimeSpan.FromSeconds(60);

        SendOrPostCallback findCompletedDelegate;
        SendOrPostCallback findProgressChangedDelegate;
        SendOrPostCallback resolveCompletedDelegate;
        SendOrPostCallback proxyAvailableDelegate;
        Action<object> findOperationTimeoutCallbackDelegate;
        Action<object> resolveOperationTimeoutCallbackDelegate;
        AsyncCallback probeOperationCallbackDelegate;
        AsyncCallback resolveOperationCallbackDelegate;
        Action<object> cancelTaskCallbackDelegate;

        IDiscoveryInnerClient innerClient;

        [Fx.Tag.Queue(typeof(AsyncOperationContext))]
        AsyncOperationLifetimeManager asyncOperationsLifetimeManager;

        [Fx.Tag.SynchronizationObject(Blocking = false, Kind = Fx.Tag.SynchronizationKind.InterlockedNoSpin)]
        int closeCalled;

        public DiscoveryClient()
            : this("*")
        {
        }

        public DiscoveryClient(string endpointConfigurationName)
        {
            if (endpointConfigurationName == null)
            {
                throw FxTrace.Exception.ArgumentNull("endpointConfigurationName");
            }

            DiscoveryEndpoint discoveryEndpoint =
                ConfigurationUtility.LookupEndpointFromClientSection<DiscoveryEndpoint>(
                endpointConfigurationName);

            this.Initialize(discoveryEndpoint);
        }

        public DiscoveryClient(DiscoveryEndpoint discoveryEndpoint)
        {
            if (discoveryEndpoint == null)
            {
                throw FxTrace.Exception.ArgumentNull("serviceDiscoveryEndpoint");
            }

            this.Initialize(discoveryEndpoint);
        }

        public event EventHandler<FindCompletedEventArgs> FindCompleted;
        public event EventHandler<FindProgressChangedEventArgs> FindProgressChanged;
        public event EventHandler<AnnouncementEventArgs> ProxyAvailable;
        public event EventHandler<ResolveCompletedEventArgs> ResolveCompleted;

        event EventHandler ICommunicationObject.Opening
        {
            add
            {
                if (this.InternalOpening == null)
                {
                    this.InnerCommunicationObject.Opening += OnInnerCommunicationObjectOpening;
                }
                this.InternalOpening += value;
            }
            remove
            {
                this.InternalOpening -= value;
                if (this.InternalOpening == null)
                {
                    this.InnerCommunicationObject.Opening -= OnInnerCommunicationObjectOpening;
                }
            }
        }

        event EventHandler ICommunicationObject.Opened
        {
            add
            {
                if (this.InternalOpened == null)
                {
                    this.InnerCommunicationObject.Opened += OnInnerCommunicationObjectOpened;
                }
                this.InternalOpened += value;
            }

            remove
            {
                this.InternalOpened -= value;
                if (this.InternalOpened == null)
                {
                    this.InnerCommunicationObject.Opened -= OnInnerCommunicationObjectOpened;
                }
            }
        }

        event EventHandler ICommunicationObject.Closing
        {
            add
            {
                if (this.InternalClosing == null)
                {
                    this.InnerCommunicationObject.Closing += OnInnerCommunicationObjectClosing;
                }
                this.InternalClosing += value;
            }

            remove
            {
                this.InternalClosing -= value;
                if (this.InternalClosing == null)
                {
                    this.InnerCommunicationObject.Closing -= OnInnerCommunicationObjectClosing;
                }
            }
        }

        event EventHandler ICommunicationObject.Closed
        {
            add
            {
                if (this.InternalClosed == null)
                {
                    this.InnerCommunicationObject.Closed += OnInnerCommunicationObjectClosed;
                }
                this.InternalClosed += value;
            }

            remove
            {
                this.InternalClosed -= value;
                if (this.InternalClosed == null)
                {
                    this.InnerCommunicationObject.Closed -= OnInnerCommunicationObjectClosed;
                }
            }
        }

        event EventHandler ICommunicationObject.Faulted
        {
            add
            {
                if (this.InternalFaulted == null)
                {
                    this.InnerCommunicationObject.Faulted += OnInnerCommunicationObjectFaulted;
                }
                this.InternalFaulted += value;
            }

            remove
            {
                this.InternalFaulted -= value;
                if (this.InternalFaulted == null)
                {
                    this.InnerCommunicationObject.Faulted -= OnInnerCommunicationObjectFaulted;
                }
            }
        }

        event EventHandler InternalOpening;
        event EventHandler InternalOpened;
        event EventHandler InternalClosing;
        event EventHandler InternalClosed;
        event EventHandler InternalFaulted;

        public ChannelFactory ChannelFactory
        {
            get
            {
                return this.InnerClient.ChannelFactory;
            }
        }

        public ClientCredentials ClientCredentials
        {
            get
            {
                return this.InnerClient.ClientCredentials;
            }
        }

        public ServiceEndpoint Endpoint
        {
            get
            {
                return this.InnerClient.Endpoint;
            }
        }

        public IClientChannel InnerChannel
        {
            get
            {
                return this.InnerClient.InnerChannel;
            }
        }

        CommunicationState ICommunicationObject.State
        {
            get
            {
                return this.InnerCommunicationObject.State;
            }
        }

        IDiscoveryInnerClient InnerClient
        {
            get
            {
                return this.innerClient;
            }
        }

        ICommunicationObject InnerCommunicationObject
        {
            get
            {
                return this.InnerClient.InnerCommunicationObject;
            }
        }

        [Fx.Tag.InheritThrows(From = "Open", FromDeclaringType = typeof(ICommunicationObject))]
        [Fx.Tag.Blocking(CancelMethod = "Abort", CancelDeclaringType = typeof(ICommunicationObject))]
        void ICommunicationObject.Open()
        {
            this.InnerCommunicationObject.Open();
        }

        [Fx.Tag.InheritThrows(From = "Open", FromDeclaringType = typeof(ICommunicationObject))]
        [Fx.Tag.Blocking(CancelMethod = "Abort", CancelDeclaringType = typeof(ICommunicationObject))]
        void ICommunicationObject.Open(TimeSpan timeout)
        {
            this.InnerCommunicationObject.Open(timeout);
        }

        [Fx.Tag.InheritThrows(From = "BeginOpen", FromDeclaringType = typeof(ICommunicationObject))]
        [Fx.Tag.Blocking(CancelMethod = "Abort", CancelDeclaringType = typeof(ICommunicationObject))]
        IAsyncResult ICommunicationObject.BeginOpen(AsyncCallback callback, object state)
        {
            return this.InnerCommunicationObject.BeginOpen(callback, state);
        }

        [Fx.Tag.InheritThrows(From = "BeginOpen", FromDeclaringType = typeof(ICommunicationObject))]
        [Fx.Tag.Blocking(CancelMethod = "Abort", CancelDeclaringType = typeof(ICommunicationObject))]
        IAsyncResult ICommunicationObject.BeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return this.InnerCommunicationObject.BeginOpen(timeout, callback, state);
        }

        [Fx.Tag.InheritThrows(From = "EndOpen", FromDeclaringType = typeof(ICommunicationObject))]
        [Fx.Tag.Blocking(CancelMethod = "Abort", CancelDeclaringType = typeof(ICommunicationObject))]
        void ICommunicationObject.EndOpen(IAsyncResult result)
        {
            this.InnerCommunicationObject.EndOpen(result);
        }

        [Fx.Tag.InheritThrows(From = "Close", FromDeclaringType = typeof(ICommunicationObject))]
        [Fx.Tag.Blocking(CancelMethod = "Abort", CancelDeclaringType = typeof(ICommunicationObject))]
        void ICommunicationObject.Close()
        {
            ((ICommunicationObject)this).Close(defaultCloseDuration);
        }

        [Fx.Tag.InheritThrows(From = "Close", FromDeclaringType = typeof(ICommunicationObject))]
        [Fx.Tag.Blocking(CancelMethod = "Abort", CancelDeclaringType = typeof(ICommunicationObject))]
        void ICommunicationObject.Close(TimeSpan timeout)
        {
            if (this.IsCloseOrAbortCalled())
            {
                return;
            }

            TimeoutException timeoutException = null;
            TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
            try
            {
                this.asyncOperationsLifetimeManager.Close(timeoutHelper.RemainingTime());
            }
            catch (TimeoutException e)
            {
                timeoutException = e;
            }

            if (timeoutException != null)
            {
                ((ICommunicationObject)this).Abort();
                throw FxTrace.Exception.AsError(new TimeoutException(SR2.DiscoveryCloseTimedOut(timeout), timeoutException));
            }
            else
            {
                try
                {
                    InnerCommunicationObject.Close(timeoutHelper.RemainingTime());
                }
                catch (ProtocolException protocolException)
                {
                    // no-op, When the client has received the required Matches and tries to
                    // close the connection, there could be a ProtocolException if the service is 
                    // trying to send more Matches. We catch such an exception and suppress it.                    
                    if (TD.DiscoveryClientProtocolExceptionSuppressedIsEnabled())
                    {
                        TD.DiscoveryClientProtocolExceptionSuppressed(protocolException);
                    }
                }
            }

        }

        [Fx.Tag.InheritThrows(From = "BeginClose", FromDeclaringType = typeof(ICommunicationObject))]
        [Fx.Tag.Blocking(CancelMethod = "Abort", CancelDeclaringType = typeof(ICommunicationObject))]
        IAsyncResult ICommunicationObject.BeginClose(AsyncCallback callback, object state)
        {
            return ((ICommunicationObject)this).BeginClose(DiscoveryClient.defaultCloseDuration, callback, state);
        }

        [Fx.Tag.InheritThrows(From = "BeginClose", FromDeclaringType = typeof(ICommunicationObject))]
        [Fx.Tag.Blocking(CancelMethod = "Abort", CancelDeclaringType = typeof(ICommunicationObject))]
        IAsyncResult ICommunicationObject.BeginClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            if (this.IsCloseOrAbortCalled())
            {
                return new CloseAsyncResult(callback, state);
            }
            else
            {
                return new CloseAsyncResult(this, timeout, callback, state);
            }
        }

        [Fx.Tag.InheritThrows(From = "EndClose", FromDeclaringType = typeof(ICommunicationObject))]
        [Fx.Tag.Blocking(CancelMethod = "Abort", CancelDeclaringType = typeof(ICommunicationObject))]
        void ICommunicationObject.EndClose(IAsyncResult result)
        {
            CloseAsyncResult.End(result);
        }

        [Fx.Tag.InheritThrows(From = "Abort", FromDeclaringType = typeof(ICommunicationObject))]
        void ICommunicationObject.Abort()
        {
            this.InnerCommunicationObject.Abort();
            this.AbortActiveOperations();
        }

        void IDisposable.Dispose()
        {
            this.Close();
        }

        [Fx.Tag.InheritThrows(From = "Open", FromDeclaringType = typeof(ICommunicationObject))]
        [Fx.Tag.Blocking(CancelMethod = "Abort", CancelDeclaringType = typeof(ICommunicationObject))]
        public void Open()
        {
            ((ICommunicationObject)this).Open();
        }

        [Fx.Tag.Throws(typeof(CommunicationException), "A communication failure interrupted this operation.")]
        [Fx.Tag.Throws.TimeoutAttribute]
        [Fx.Tag.Blocking(CancelMethod = "Abort")]
        public FindResponse Find(FindCriteria criteria)
        {
            if (criteria == null)
            {
                throw FxTrace.Exception.ArgumentNull("criteria");
            }

            if ((criteria.MaxResults == int.MaxValue) && (criteria.Duration.Equals(TimeSpan.MaxValue)))
            {
                throw FxTrace.Exception.AsError(new ArgumentException(SR2.DiscoveryFindCanNeverComplete));
            }

            SyncOperationState syncOperationState = new SyncOperationState();
            this.FindAsync(criteria, syncOperationState);

            syncOperationState.WaitEvent.WaitOne();
            return ((FindCompletedEventArgs)syncOperationState.EventArgs).Result;
        }

        [Fx.Tag.NonThrowing]
        [Fx.Tag.Blocking(CancelMethod = "Abort")]
        public void FindAsync(FindCriteria criteria)
        {
            this.FindAsync(criteria, null);
        }

        [Fx.Tag.NonThrowing]
        [Fx.Tag.Blocking(CancelMethod = "CancelAsync")]
        public void FindAsync(FindCriteria criteria, object userState)
        {
            if (criteria == null)
            {
                throw FxTrace.Exception.ArgumentNull("criteria");
            }

            using (new DiscoveryOperationContextScope(InnerChannel))
            {
                this.FindAsyncOperation(criteria, userState);
            }
        }

        [Fx.Tag.Throws(typeof(AggregateException), "Inherits from the Task exception contract.")]
        public Task<FindResponse> FindTaskAsync(FindCriteria criteria)
        {
            return this.FindTaskAsync(criteria, CancellationToken.None);
        }

        [Fx.Tag.Throws(typeof(AggregateException), "Inherits from the Task exception contract.")]
        public Task<FindResponse> FindTaskAsync(FindCriteria criteria, CancellationToken cancellationToken)
        {
            if (criteria == null)
            {
                throw FxTrace.Exception.ArgumentNull("criteria");
            }

            TaskCompletionSource<FindResponse> taskCompletionSource = new TaskCompletionSource<FindResponse>();
            TaskAsyncOperationState<FindResponse> taskAsyncOperationState = new TaskAsyncOperationState<FindResponse>(this, taskCompletionSource, cancellationToken);
            Task<FindResponse> task = taskCompletionSource.Task;
            this.FindAsync(criteria, taskAsyncOperationState);
            return task;
        }

        [Fx.Tag.Throws(typeof(AggregateException), "Inherits from the Task exception contract.")]
        public Task<ResolveResponse> ResolveTaskAsync(ResolveCriteria criteria)
        {
            return this.ResolveTaskAsync(criteria, CancellationToken.None);
        }

        [Fx.Tag.Throws(typeof(AggregateException), "Inherits from the Task exception contract.")]
        public Task<ResolveResponse> ResolveTaskAsync(ResolveCriteria criteria, CancellationToken cancellationToken)
        {
            if (criteria == null)
            {
                throw FxTrace.Exception.ArgumentNull("criteria");
            }

            TaskCompletionSource<ResolveResponse> taskCompletionSource = new TaskCompletionSource<ResolveResponse>();
            TaskAsyncOperationState<ResolveResponse> taskAsyncOperationState = new TaskAsyncOperationState<ResolveResponse>(this, taskCompletionSource, cancellationToken);
            Task<ResolveResponse> task = taskCompletionSource.Task;
            this.ResolveAsync(criteria, taskAsyncOperationState);
            return task;
        }

        [Fx.Tag.Throws(typeof(CommunicationException), "A communication failure interrupted this operation.")]
        [Fx.Tag.Throws.TimeoutAttribute]
        [Fx.Tag.Blocking(CancelMethod = "Abort")]
        public ResolveResponse Resolve(ResolveCriteria criteria)
        {
            SyncOperationState syncOperationState = new SyncOperationState();
            this.ResolveAsync(criteria, syncOperationState);
            syncOperationState.WaitEvent.WaitOne();

            return ((ResolveCompletedEventArgs)syncOperationState.EventArgs).Result;
        }

        [Fx.Tag.NonThrowing]
        [Fx.Tag.Blocking(CancelMethod = "Abort")]
        public void ResolveAsync(ResolveCriteria criteria)
        {
            this.ResolveAsync(criteria, null);
        }

        [Fx.Tag.NonThrowing]
        [Fx.Tag.Blocking(CancelMethod = "CancelAsync")]
        public void ResolveAsync(ResolveCriteria criteria, object userState)
        {
            if (criteria == null)
            {
                throw FxTrace.Exception.ArgumentNull("criteria");
            }

            using (new DiscoveryOperationContextScope(InnerChannel))
            {
                this.ResolveAsyncOperation(criteria, userState);
            }
        }

        [Fx.Tag.Throws(typeof(InvalidOperationException), "If there are more than one operations pending that are associated with the specified userState.")]
        public void CancelAsync(object userState)
        {
            if (userState == null)
            {
                throw FxTrace.Exception.ArgumentNull("userState");
            }

            AsyncOperationContext context = null;
            if (this.asyncOperationsLifetimeManager.TryRemoveUnique(userState, out context))
            {
                if (context is FindAsyncOperationContext)
                {
                    this.PostFindCompleted((FindAsyncOperationContext)context, true, null);
                }
                else
                {
                    this.PostResolveCompleted((ResolveAsyncOperationContext)context, true, null);
                }
            }
            else
            {
                if (context != null)
                {
                    throw FxTrace.Exception.AsError(new InvalidOperationException(SR2.DiscoveryMultiplePendingOperationsPerUserState));
                }
            }
        }

        [Fx.Tag.InheritThrows(From = "Close", FromDeclaringType = typeof(ICommunicationObject))]
        [Fx.Tag.Blocking(CancelMethod = "Abort", CancelDeclaringType = typeof(ICommunicationObject))]
        public void Close()
        {
            ((ICommunicationObject)this).Close();
        }

        void IDiscoveryInnerClientResponse.PostFindCompletedAndRemove(UniqueId operationId, bool cancelled, Exception error)
        {
            FindAsyncOperationContext context = this.asyncOperationsLifetimeManager.Remove<FindAsyncOperationContext>(operationId);
            if (context != null)
            {
                this.PostFindCompleted(context, cancelled, error);
            }
        }

        void IDiscoveryInnerClientResponse.PostResolveCompletedAndRemove(UniqueId operationId, bool cancelled, Exception error)
        {
            ResolveAsyncOperationContext context = this.asyncOperationsLifetimeManager.Remove<ResolveAsyncOperationContext>(operationId);
            if (context != null)
            {
                this.PostResolveCompleted(context, cancelled, error);
            }
        }

        void IDiscoveryInnerClientResponse.ProbeMatchOperation(UniqueId relatesTo, DiscoveryMessageSequence discoveryMessageSequence, Collection<EndpointDiscoveryMetadata> endpointDiscoveryMetadataCollection, bool findCompleted)
        {
            EventTraceActivity eventTraceActivity = null;
            OperationContext operationContext = OperationContext.Current;

            if (Fx.Trace.IsEtwProviderEnabled && operationContext != null)
            {
                eventTraceActivity = EventTraceActivityHelper.TryExtractActivity(operationContext.IncomingMessage);
            }

            if (relatesTo == null)
            {
                if (TD.DiscoveryMessageWithNullRelatesToIsEnabled() && operationContext != null)
                {
                    TD.DiscoveryMessageWithNullRelatesTo(
                        eventTraceActivity,
                        ProtocolStrings.TracingStrings.ProbeMatches,
                        operationContext.IncomingMessageHeaders.MessageId.ToString());
                }

                return;
            }

            FindAsyncOperationContext context = null;
            if (!this.asyncOperationsLifetimeManager.TryLookup<FindAsyncOperationContext>(relatesTo, out context))
            {
                if (TD.DiscoveryMessageWithInvalidRelatesToOrOperationCompletedIsEnabled() && operationContext != null)
                {
                    TD.DiscoveryMessageWithInvalidRelatesToOrOperationCompleted(
                        eventTraceActivity,
                        ProtocolStrings.TracingStrings.ProbeMatches,
                        operationContext.IncomingMessageHeaders.MessageId.ToString(),
                        relatesTo.ToString(),
                        ProtocolStrings.TracingStrings.FindOperation);
                }

                return;
            }

            bool postCompleted = false;
            lock (context.SyncRoot)
            {
                if (!context.IsCompleted && (context.Result.Endpoints.Count < context.MaxResults))
                {
                    bool postProgress = (!context.IsSyncOperation && !context.IsTaskBasedOperation && this.FindProgressChanged != null);

                    foreach (EndpointDiscoveryMetadata endpointDiscoveryMetadata in endpointDiscoveryMetadataCollection)
                    {
                        context.Result.AddDiscoveredEndpoint(endpointDiscoveryMetadata, discoveryMessageSequence);
                        if (postProgress)
                        {
                            context.AsyncOperation.Post(
                                this.findProgressChangedDelegate,
                                new FindProgressChangedEventArgs(context.Progress, context.UserState, endpointDiscoveryMetadata, discoveryMessageSequence));
                        }

                        if (context.Result.Endpoints.Count == context.MaxResults)
                        {
                            postCompleted = true;
                            break;
                        }
                    }
                }
                else
                {
                    if (TD.DiscoveryMessageReceivedAfterOperationCompletedIsEnabled() && operationContext != null)
                    {
                        TD.DiscoveryMessageReceivedAfterOperationCompleted(
                            eventTraceActivity,
                            ProtocolStrings.TracingStrings.ProbeMatches,
                            operationContext.IncomingMessageHeaders.MessageId.ToString(),
                            ProtocolStrings.TracingStrings.FindOperation);
                    }
                }
            }

            if (postCompleted || findCompleted)
            {
                ((IDiscoveryInnerClientResponse)this).PostFindCompletedAndRemove(context.OperationId, false, null);
            }
        }

        void IDiscoveryInnerClientResponse.ResolveMatchOperation(UniqueId relatesTo, DiscoveryMessageSequence discoveryMessageSequence, EndpointDiscoveryMetadata endpointDiscoveryMetadata)
        {
            EventTraceActivity eventTraceActivity = null;
            OperationContext operationContext = OperationContext.Current;

            if (Fx.Trace.IsEtwProviderEnabled && operationContext != null)
            {
                eventTraceActivity = EventTraceActivityHelper.TryExtractActivity(operationContext.IncomingMessage);
            }

            if (relatesTo == null)
            {
                if (TD.DiscoveryMessageWithNullRelatesToIsEnabled() && operationContext != null)
                {
                    TD.DiscoveryMessageWithNullRelatesTo(
                        eventTraceActivity,
                        ProtocolStrings.TracingStrings.ResolveMatches,
                        operationContext.IncomingMessageHeaders.MessageId.ToString());
                }

                return;
            }

            ResolveAsyncOperationContext context = null;
            if (!this.asyncOperationsLifetimeManager.TryLookup<ResolveAsyncOperationContext>(relatesTo, out context))
            {
                if (TD.DiscoveryMessageWithInvalidRelatesToOrOperationCompletedIsEnabled() && operationContext != null)
                {
                    TD.DiscoveryMessageWithInvalidRelatesToOrOperationCompleted(
                        eventTraceActivity,
                        ProtocolStrings.TracingStrings.ResolveMatches,
                        operationContext.IncomingMessageHeaders.MessageId.ToString(),
                        relatesTo.ToString(),
                        ProtocolStrings.TracingStrings.ResolveOperation);
                }

                return;
            }

            bool postCompleted = false;
            lock (context.SyncRoot)
            {
                if (!context.IsCompleted && (context.Result.EndpointDiscoveryMetadata == null))
                {
                    context.Result.EndpointDiscoveryMetadata = endpointDiscoveryMetadata;
                    context.Result.MessageSequence = discoveryMessageSequence;
                    postCompleted = true;
                }
                else
                {
                    if (TD.DiscoveryMessageReceivedAfterOperationCompletedIsEnabled() && operationContext != null)
                    {
                        TD.DiscoveryMessageReceivedAfterOperationCompleted(
                            eventTraceActivity,
                            ProtocolStrings.TracingStrings.ResolveMatches,
                            operationContext.IncomingMessageHeaders.MessageId.ToString(),
                            ProtocolStrings.TracingStrings.ResolveOperation);
                    }
                }
            }

            if (postCompleted)
            {
                ((IDiscoveryInnerClientResponse)this).PostResolveCompletedAndRemove(context.OperationId, false, null);
            }
        }

        void IDiscoveryInnerClientResponse.HelloOperation(UniqueId relatesTo, DiscoveryMessageSequence proxyMessageSequence, EndpointDiscoveryMetadata proxyEndpointMetadata)
        {
            EventTraceActivity eventTraceActivity = null;
            OperationContext operationContext = OperationContext.Current;

            if (Fx.Trace.IsEtwProviderEnabled && operationContext != null)
            {
                eventTraceActivity = EventTraceActivityHelper.TryExtractActivity(operationContext.IncomingMessage);
            }

            if (relatesTo == null)
            {
                if (TD.DiscoveryMessageWithNullRelatesToIsEnabled() && operationContext != null)
                {
                    TD.DiscoveryMessageWithNullRelatesTo(
                        eventTraceActivity,
                        ProtocolStrings.TracingStrings.Hello,
                        operationContext.IncomingMessageHeaders.MessageId.ToString());
                }

                return;
            }

            AsyncOperationContext context = null;
            if (!this.asyncOperationsLifetimeManager.TryLookup(relatesTo, out context))
            {
                if (TD.DiscoveryMessageWithInvalidRelatesToOrOperationCompletedIsEnabled() && operationContext != null)
                {
                    TD.DiscoveryMessageWithInvalidRelatesToOrOperationCompleted(
                        eventTraceActivity,
                        ProtocolStrings.TracingStrings.Hello,
                        operationContext.IncomingMessageHeaders.MessageId.ToString(),
                        relatesTo.ToString(),
                        string.Format(
                            CultureInfo.InvariantCulture,
                            "{0}/{1}",
                            ProtocolStrings.TracingStrings.FindOperation,
                            ProtocolStrings.TracingStrings.ResolveOperation));
                }

                return;
            }

            this.PostProxyAvailable(context, proxyEndpointMetadata, proxyMessageSequence);
        }

        void Initialize(DiscoveryEndpoint discoveryEndpoint)
        {
            if (discoveryEndpoint.Binding != null && discoveryEndpoint.Binding.MessageVersion.Addressing == AddressingVersion.None)
            {
                throw FxTrace.Exception.Argument(
                    "discoveryEndpoint",
                    SR.EndpointWithInvalidMessageVersion(
                        discoveryEndpoint.GetType().Name,
                        AddressingVersion.None,
                        this.GetType().Name,
                        AddressingVersion.WSAddressing10,
                        AddressingVersion.WSAddressingAugust2004));
            }

            this.innerClient = discoveryEndpoint.DiscoveryVersion.Implementation.CreateDiscoveryInnerClient(discoveryEndpoint, this);

            this.asyncOperationsLifetimeManager = new AsyncOperationLifetimeManager();

            this.findCompletedDelegate = Fx.ThunkCallback(new SendOrPostCallback(RaiseFindCompleted));
            this.findProgressChangedDelegate = Fx.ThunkCallback(new SendOrPostCallback(RaiseFindProgressChanged));
            this.resolveCompletedDelegate = Fx.ThunkCallback(new SendOrPostCallback(RaiseResolveCompleted));
            this.proxyAvailableDelegate = Fx.ThunkCallback(new SendOrPostCallback(RaiseProxyAvailable));
            this.findOperationTimeoutCallbackDelegate = new Action<object>(FindOperationTimeoutCallback);
            this.resolveOperationTimeoutCallbackDelegate = new Action<object>(ResolveOperationTimeoutCallback);

            this.probeOperationCallbackDelegate = Fx.ThunkCallback(new AsyncCallback(ProbeOperationCompletedCallback));
            this.resolveOperationCallbackDelegate = Fx.ThunkCallback(new AsyncCallback(ResolveOperationCompletedCallback));

            this.cancelTaskCallbackDelegate = Fx.ThunkCallback(new Action<object>(this.CancelAsync));

            this.closeCalled = 0;
        }

        void OnInnerCommunicationObjectOpened(object sender, EventArgs e)
        {
            this.RaiseCommunicationObjectEvent(this.InternalOpened, e);
        }

        void OnInnerCommunicationObjectOpening(object sender, EventArgs e)
        {
            this.RaiseCommunicationObjectEvent(this.InternalOpening, e);
        }

        void OnInnerCommunicationObjectClosing(object sender, EventArgs e)
        {
            this.RaiseCommunicationObjectEvent(this.InternalClosing, e);
        }

        void OnInnerCommunicationObjectClosed(object sender, EventArgs e)
        {
            this.RaiseCommunicationObjectEvent(this.InternalClosed, e);
        }

        void OnInnerCommunicationObjectFaulted(object sender, EventArgs e)
        {
            this.RaiseCommunicationObjectEvent(this.InternalFaulted, e);
        }

        void RaiseCommunicationObjectEvent(EventHandler handler, EventArgs e)
        {
            if (handler != null)
            {
                handler(this, e);
            }
        }

        void FindAsyncOperation(FindCriteria criteria, object userState)
        {
            Fx.Assert(OperationContext.Current != null, "OperationContext.Current cannot be null.");
            Fx.Assert(OperationContext.Current.OutgoingMessageHeaders != null, "OperationContext.Current.OutgoingMessageHeaders cannot be null.");

            AsyncOperationContext context = new FindAsyncOperationContext(
                OperationContext.Current.OutgoingMessageHeaders.MessageId,
                criteria.MaxResults,
                criteria.Duration,
                userState);

            this.InitializeAsyncOperation(context);

            Exception error = null;
            try
            {
                if (!context.IsCompleted)
                {
                    if (context.IsSyncOperation)
                    {
                        this.InnerClient.ProbeOperation(criteria);
                        this.StartTimer(context, this.findOperationTimeoutCallbackDelegate);
                    }
                    else
                    {
                        IAsyncResult result = InnerClient.BeginProbeOperation(criteria, this.probeOperationCallbackDelegate, context);
                        if (result.CompletedSynchronously)
                        {
                            this.CompleteProbeOperation(result);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                if (Fx.IsFatal(e))
                {
                    throw;
                }
                error = e;
            }
            if (error != null)
            {
                ((IDiscoveryInnerClientResponse)this).PostFindCompletedAndRemove(context.OperationId, false, error);
            }
        }

        void ResolveAsyncOperation(ResolveCriteria criteria, object userState)
        {
            Fx.Assert(OperationContext.Current != null, "OperationContext.Current cannot be null.");
            Fx.Assert(OperationContext.Current.OutgoingMessageHeaders != null, "OperationContext.Current.OutgoingMessageHeaders cannot be null.");

            AsyncOperationContext context =
                new ResolveAsyncOperationContext(
                OperationContext.Current.OutgoingMessageHeaders.MessageId,
                criteria.Duration,
                userState);

            this.InitializeAsyncOperation(context);

            Exception error = null;
            try
            {
                if (context.IsSyncOperation)
                {
                    this.InnerClient.ResolveOperation(criteria);
                    this.StartTimer(context, this.resolveOperationTimeoutCallbackDelegate);
                }
                else
                {
                    IAsyncResult result = InnerClient.BeginResolveOperation(criteria, this.resolveOperationCallbackDelegate, context);
                    if (result.CompletedSynchronously)
                    {
                        this.CompleteResolveOperation(result);
                    }
                }
            }
            catch (Exception e)
            {
                if (Fx.IsFatal(e))
                {
                    throw;
                }
                error = e;
            }
            if (error != null)
            {
                ((IDiscoveryInnerClientResponse)this).PostResolveCompletedAndRemove(context.OperationId, false, error);
            }
        }

        void InitializeAsyncOperation(AsyncOperationContext context)
        {
            context.AsyncOperation = AsyncOperationManager.CreateOperation(context.UserState);
            if (!this.asyncOperationsLifetimeManager.TryAdd(context))
            {
                if (this.asyncOperationsLifetimeManager.IsClosed || this.asyncOperationsLifetimeManager.IsAborted)
                {
                    throw FxTrace.Exception.AsError(new ObjectDisposedException(this.GetType().Name));
                }
                else
                {
                    throw FxTrace.Exception.AsError(new InvalidOperationException(SR.DiscoveryDuplicateOperationId(context.OperationId)));
                }
            }
        }

        bool IsCloseOrAbortCalled()
        {
            return ((Interlocked.CompareExchange(ref this.closeCalled, 1, 0) == 1) || this.asyncOperationsLifetimeManager.IsAborted);
        }

        void ProbeOperationCompletedCallback(IAsyncResult result)
        {
            if (result.CompletedSynchronously)
            {
                return;
            }

            this.CompleteProbeOperation(result);
        }

        void FindOperationTimeoutCallback(object state)
        {
            AsyncOperationContext context = (AsyncOperationContext)state;
            ((IDiscoveryInnerClientResponse)this).PostFindCompletedAndRemove(context.OperationId, false, null);
        }

        void CompleteProbeOperation(IAsyncResult result)
        {
            AsyncOperationContext context = (AsyncOperationContext)result.AsyncState;

            Exception error = null;
            try
            {
                this.InnerClient.EndProbeOperation(result);
            }
            catch (Exception e)
            {
                if (Fx.IsFatal(e))
                {
                    throw;
                }
                error = e;
            }

            if (error != null)
            {
                ((IDiscoveryInnerClientResponse)this).PostFindCompletedAndRemove(context.OperationId, false, error);
            }
            else
            {
                this.StartTimer(context, this.findOperationTimeoutCallbackDelegate);
            }
        }

        void PostFindCompleted(FindAsyncOperationContext context, bool cancelled, Exception error)
        {
            bool completed = false;
            lock (context.SyncRoot)
            {
                if (!context.IsCompleted)
                {
                    context.Complete();
                    completed = true;
                }
            }

            if (completed)
            {
                FindCompletedEventArgs e = new FindCompletedEventArgs(error, cancelled, context.UserState, context.Result);

                if (this.DispatchToSyncOperation(e) || 
                    this.DispatchToTaskAyncOperation<FindResponse>(context.UserState, context.Result, error, cancelled) ||
                    this.FindCompleted == null)
                {
                    context.AsyncOperation.OperationCompleted();
                }
                else
                {
                    context.AsyncOperation.PostOperationCompleted(this.findCompletedDelegate, e);
                }
            }
        }

        void RaiseFindCompleted(object state)
        {
            EventHandler<FindCompletedEventArgs> handler = this.FindCompleted;
            if (handler != null)
            {
                handler(this, (FindCompletedEventArgs)state);
            }
        }

        void RaiseFindProgressChanged(object state)
        {
            EventHandler<FindProgressChangedEventArgs> handler = this.FindProgressChanged;
            if (handler != null)
            {
                handler(this, (FindProgressChangedEventArgs)state);
            }
        }

        void ResolveOperationCompletedCallback(IAsyncResult result)
        {
            if (result.CompletedSynchronously)
            {
                return;
            }

            this.CompleteResolveOperation(result);
        }

        void ResolveOperationTimeoutCallback(object state)
        {
            AsyncOperationContext context = (AsyncOperationContext)state;
            ((IDiscoveryInnerClientResponse)this).PostResolveCompletedAndRemove(context.OperationId, false, null);
        }

        void CompleteResolveOperation(IAsyncResult result)
        {
            AsyncOperationContext context = (AsyncOperationContext)result.AsyncState;

            Exception error = null;
            try
            {
                this.InnerClient.EndResolveOperation(result);
            }
            catch (Exception e)
            {
                if (Fx.IsFatal(e))
                {
                    throw;
                }
                error = e;
            }
            if (error != null)
            {
                ((IDiscoveryInnerClientResponse)this).PostResolveCompletedAndRemove(context.OperationId, false, error);
            }
            else
            {
                this.StartTimer(context, this.resolveOperationTimeoutCallbackDelegate);
            }
        }

        void PostResolveCompleted(ResolveAsyncOperationContext context, bool cancelled, Exception error)
        {
            bool completed = false;
            lock (context.SyncRoot)
            {
                if (!context.IsCompleted)
                {
                    context.Complete();
                    completed = true;
                }
            }

            if (completed)
            {
                ResolveCompletedEventArgs e = new ResolveCompletedEventArgs(error, cancelled, context.UserState, context.Result);

                if (this.DispatchToSyncOperation(e) || 
                    this.DispatchToTaskAyncOperation<ResolveResponse>(context.UserState, context.Result, error, cancelled) ||
                    this.ResolveCompleted == null)
                {
                    context.AsyncOperation.OperationCompleted();
                }
                else
                {
                    context.AsyncOperation.PostOperationCompleted(this.resolveCompletedDelegate, e);
                }
            }
        }

        void RaiseResolveCompleted(object state)
        {
            EventHandler<ResolveCompletedEventArgs> handler = this.ResolveCompleted;
            if (handler != null)
            {
                handler(this, (ResolveCompletedEventArgs)state);
            }
        }

        void PostProxyAvailable(
            AsyncOperationContext context,
            EndpointDiscoveryMetadata proxyEndpointMetadata,
            DiscoveryMessageSequence proxyMessageSequence)
        {
            if (TD.DiscoveryClientReceivedMulticastSuppressionIsEnabled())
            {
                TD.DiscoveryClientReceivedMulticastSuppression();
            }

            if (this.ProxyAvailable != null)
            {
                lock (context.SyncRoot)
                {
                    if (!context.IsCompleted)
                    {
                        AnnouncementEventArgs e = new AnnouncementEventArgs(proxyMessageSequence, proxyEndpointMetadata);
                        context.AsyncOperation.Post(this.proxyAvailableDelegate, e);
                    }
                }
            }
        }

        void RaiseProxyAvailable(object state)
        {
            EventHandler<AnnouncementEventArgs> handler = this.ProxyAvailable;
            if (handler != null)
            {
                handler(this, (AnnouncementEventArgs)state);
            }
        }

        void StartTimer(AsyncOperationContext context, Action<object> operationTimeoutCallbackDelegate)
        {
            if (!this.InnerClient.IsRequestResponse)
            {
                lock (context.SyncRoot)
                {
                    if (!context.IsCompleted)
                    {
                        context.StartTimer(operationTimeoutCallbackDelegate);
                    }
                }
            }
        }

        bool DispatchToSyncOperation(AsyncCompletedEventArgs e)
        {
            if (e.UserState is SyncOperationState)
            {
                SyncOperationState syncOperationState = (SyncOperationState)e.UserState;
                syncOperationState.EventArgs = e;
                syncOperationState.WaitEvent.Set();
                return true;
            }
            else
            {
                return false;
            }
        }

        bool DispatchToTaskAyncOperation<TResult>(object userState, TResult result, Exception error, bool cancelled)
        {
            TaskAsyncOperationState<TResult> operationState = userState as TaskAsyncOperationState<TResult>;
            if (operationState != null)
            {
                operationState.Complete(result, error, cancelled);
                return true;
            }
            else
            {
                return false;
            }
        }

        void AbortActiveOperations()
        {
            AsyncOperationContext[] activeOperations = this.asyncOperationsLifetimeManager.Abort();

            for (int i = 0; i < activeOperations.Length; i++)
            {
                if (activeOperations[i] is FindAsyncOperationContext)
                {
                    this.PostFindCompleted((FindAsyncOperationContext)activeOperations[i], true, null);
                }
                else
                {
                    this.PostResolveCompleted((ResolveAsyncOperationContext)activeOperations[i], true, null);
                }
            }
        }

        class CloseAsyncResult : AsyncResult
        {
            static AsyncCompletion onAsyncLifetimeManangerCloseCompleted = new AsyncCompletion(OnAsyncLifetimeManagerCloseCompleted);
            static AsyncCompletion onInnerCommunicationObjectCloseCompleted = new AsyncCompletion(OnInnerCommunicationObjectCloseCompleted);

            DiscoveryClient client;
            TimeoutHelper timeoutHelper;

            internal CloseAsyncResult(AsyncCallback callback, object state)
                : base(callback, state)
            {
                Complete(true);
            }

            internal CloseAsyncResult(DiscoveryClient client, TimeSpan timeout, AsyncCallback callback, object state)
                : base(callback, state)
            {
                this.client = client;
                this.timeoutHelper = new TimeoutHelper(timeout);

                IAsyncResult result = this.client.asyncOperationsLifetimeManager.BeginClose(
                    this.timeoutHelper.RemainingTime(),
                    this.PrepareAsyncCompletion(onAsyncLifetimeManangerCloseCompleted),
                    this);

                if (result.CompletedSynchronously && OnAsyncLifetimeManagerCloseCompleted(result))
                {
                    Complete(true);
                }
            }

            internal static void End(IAsyncResult result)
            {
                AsyncResult.End<CloseAsyncResult>(result);
            }

            static bool OnAsyncLifetimeManagerCloseCompleted(IAsyncResult result)
            {
                CloseAsyncResult thisPtr = (CloseAsyncResult)result.AsyncState;
                Exception timeoutException = null;
                try
                {
                    thisPtr.client.asyncOperationsLifetimeManager.EndClose(result);
                }
                catch (TimeoutException e)
                {
                    timeoutException = e;
                }

                if (timeoutException != null)
                {
                    ((ICommunicationObject)thisPtr.client).Abort();
                    throw FxTrace.Exception.AsError(
                        new TimeoutException(
                        SR2.DiscoveryCloseTimedOut(thisPtr.timeoutHelper.OriginalTimeout),
                        timeoutException));
                }

                IAsyncResult closeAsyncResult = thisPtr.client.InnerCommunicationObject.BeginClose(
                    thisPtr.timeoutHelper.RemainingTime(),
                    thisPtr.PrepareAsyncCompletion(onInnerCommunicationObjectCloseCompleted),
                    thisPtr);

                if (closeAsyncResult.CompletedSynchronously)
                {
                    return OnInnerCommunicationObjectCloseCompleted(closeAsyncResult);
                }
                else
                {
                    return false;
                }
            }

            static bool OnInnerCommunicationObjectCloseCompleted(IAsyncResult result)
            {
                CloseAsyncResult thisPtr = (CloseAsyncResult)result.AsyncState;
                thisPtr.client.InnerCommunicationObject.EndClose(result);
                return true;
            }
        }

        sealed class DiscoveryOperationContextScope : IDisposable
        {
            OperationContextScope operationContextScope;
            UniqueId originalMessageId;
            EndpointAddress originalReplyTo;
            Uri originalTo;

            public DiscoveryOperationContextScope(IClientChannel clientChannel)
            {
                if (DiscoveryUtility.IsCompatible(OperationContext.Current, clientChannel))
                {
                    // reuse the same context
                    this.originalMessageId = OperationContext.Current.OutgoingMessageHeaders.MessageId;
                    this.originalReplyTo = OperationContext.Current.OutgoingMessageHeaders.ReplyTo;
                    this.originalTo = OperationContext.Current.OutgoingMessageHeaders.To;
                }
                else
                {
                    // create new context
                    this.operationContextScope = new OperationContextScope(clientChannel);
                }

                if (this.originalMessageId == null)
                {
                    // this is either a new context or an existing one with no message id.
                    OperationContext.Current.OutgoingMessageHeaders.MessageId = new UniqueId();
                }

                OperationContext.Current.OutgoingMessageHeaders.ReplyTo = clientChannel.LocalAddress;
                OperationContext.Current.OutgoingMessageHeaders.To = clientChannel.RemoteAddress.Uri;
            }

            public void Dispose()
            {
                if (this.operationContextScope != null)
                {
                    this.operationContextScope.Dispose();
                }
                else
                {
                    OperationContext.Current.OutgoingMessageHeaders.MessageId = this.originalMessageId;
                    OperationContext.Current.OutgoingMessageHeaders.ReplyTo = this.originalReplyTo;
                    OperationContext.Current.OutgoingMessageHeaders.To = this.originalTo;
                }
            }
        }

        class FindAsyncOperationContext : AsyncOperationContext
        {
            private static Type TaskAsyncOperationStateType = typeof(TaskAsyncOperationState<>);
            FindResponse result;

            internal FindAsyncOperationContext(UniqueId operationId, int maxResults, TimeSpan duration, object userState)
                : base(operationId, maxResults, duration, userState)
            {
                this.result = new FindResponse();

                // Task-based operations are detected by the type of the userState
                if (this.UserState != null)
                {
                    Type userStateType = this.UserState.GetType();
                    if (userStateType.IsGenericType && userStateType.GetGenericTypeDefinition() == TaskAsyncOperationStateType)
                    {
                        this.IsTaskBasedOperation = true;
                    }
                }
            }

            public FindResponse Result
            {
                get
                {
                    return this.result;
                }
            }

            public bool IsTaskBasedOperation { get; private set; }

            public int Progress
            {
                get
                {
                    int progress = 0;

                    if (MaxResults != int.MaxValue)
                    {
                        progress = (int)((float)Result.Endpoints.Count / (float)MaxResults * 100);
                    }
                    else if (StartedAt != null)
                    {
                        TimeSpan elaspedTime = DateTime.UtcNow.Subtract(StartedAt.Value);
                        progress = (int)(elaspedTime.TotalMilliseconds / Duration.TotalMilliseconds * 100);
                    }

                    return progress;
                }
            }
        }

        class ResolveAsyncOperationContext : AsyncOperationContext
        {
            ResolveResponse result;

            internal ResolveAsyncOperationContext(UniqueId operationId, TimeSpan duration, object userState)
                : base(operationId, 1, duration, userState)
            {
                this.result = new ResolveResponse();
            }

            public ResolveResponse Result
            {
                get
                {
                    return this.result;
                }
            }
        }

        // Class used to coordinate synchronization with Task-based asynchronous execution
        class TaskAsyncOperationState<TResult>
        {
            TaskCompletionSource<TResult> taskCompletionSource;
            CancellationToken cancellationToken;

            internal TaskAsyncOperationState(DiscoveryClient discoveryClient, TaskCompletionSource<TResult> taskCompletionSource, CancellationToken cancellationToken)
            {
                Fx.Assert(discoveryClient != null, "discoveryClient cannot be null");
                Fx.Assert(taskCompletionSource != null, "taskCompletionSource cannot be null");
                Fx.Assert(cancellationToken != null, "cancellationToken cannot be null");

                this.taskCompletionSource = taskCompletionSource;
                this.cancellationToken = cancellationToken;

                // Register an action that will be invoked when the user requests cancellation
                // through the CancellationToken.  We do not poll for cancellation requests but
                // rely solely on this callback.
                cancellationToken.Register(discoveryClient.cancelTaskCallbackDelegate, this);
            }

            internal void Complete(TResult result, Exception error, bool cancelled)
            {
                // Precedence is given to the 'cancelled' parameter over the cancellation token
                // to permit a normal completion to take precedence over cancellation if the
                // two occur concurrently.  This also addresses internally generated cancellations
                // such as aborts.  Subsequent calls to Complete have no effect.
                if (cancelled)
                {
                    this.taskCompletionSource.TrySetCanceled();
                }
                else if (error != null)
                {
                    this.taskCompletionSource.TrySetException(error);
                }
                else
                {
                    this.taskCompletionSource.TrySetResult(result);
                }
            }
        }
    }
}
