//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Discovery
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Runtime;
    using System.ServiceModel.Channels;
    using System.Threading;

    abstract partial class DiscoveryClientChannelBase<TChannel> : ChannelBase
        where TChannel : class, IChannel
    {
        TChannel innerChannel;

        IChannelFactory<TChannel> innerChannelFactory;
        FindCriteria findCriteria;
        DiscoveryEndpointProvider discoveryEndpointProvider;

        DiscoveryClient discoveryClient;

        InputQueue<EndpointDiscoveryMetadata> discoveredEndpoints;
        Exception exception;
        int totalExpectedEndpoints;
        int totalDiscoveredEndpoints;
        bool discoveryCompleted;

        [Fx.Tag.SynchronizationObject]
        object thisLock;

        public DiscoveryClientChannelBase(
            ChannelManagerBase channelManagerBase,
            IChannelFactory<TChannel> innerChannelFactory,
            FindCriteria findCriteria,
            DiscoveryEndpointProvider discoveryEndpointProvider)
            : base(channelManagerBase)
        {
            Fx.Assert(findCriteria != null, "The findCriteria must be non null.");
            Fx.Assert(discoveryEndpointProvider != null, "The discoveryEndpointProvider must be non null.");
            Fx.Assert(innerChannelFactory != null, "The innerChannelFactory must be non null.");

            this.innerChannelFactory = innerChannelFactory;
            this.findCriteria = findCriteria;
            this.discoveryEndpointProvider = discoveryEndpointProvider;

            this.discoveredEndpoints = new InputQueue<EndpointDiscoveryMetadata>();
            this.totalExpectedEndpoints = int.MaxValue;
            this.totalDiscoveredEndpoints = 0;
            this.discoveryCompleted = false;
            this.thisLock = new object();
        }

        protected TChannel InnerChannel
        {
            get
            {
                return this.innerChannel;
            }
        }

        public override T GetProperty<T>()
        {
            T baseProperty = base.GetProperty<T>();
            if (baseProperty != null)
            {
                return baseProperty;
            }

            if (this.innerChannel != null)
            {
                return this.InnerChannel.GetProperty<T>();
            }

            return null;
        }

        protected override void OnOpen(TimeSpan timeout)
        {
            this.innerChannel = this.BuildChannel(timeout);
            this.innerChannel.Faulted += new EventHandler(OnInnerChannelFaulted);
        }

        protected override IAsyncResult OnBeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new DiscoveryChannelBuilderAsyncResult(
                this,
                timeout,
                callback,
                state);
        }

        protected override void OnEndOpen(IAsyncResult result)
        {
            this.innerChannel = DiscoveryChannelBuilderAsyncResult.End(result);
            this.innerChannel.Faulted += new EventHandler(OnInnerChannelFaulted);
        }

        protected override void OnClosing()
        {
            if (this.innerChannel != null)
            {
                this.innerChannel.Faulted -= new EventHandler(OnInnerChannelFaulted);
            }
            base.OnClosing();
        }

        protected override void OnClose(TimeSpan timeout)
        {
            if (this.innerChannel != null)
            {
                this.innerChannel.Close(timeout);
            }
        }

        protected override IAsyncResult OnBeginClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new CloseAsyncResult(
                this.innerChannel,
                timeout,
                callback,
                state);
        }

        protected override void OnEndClose(IAsyncResult result)
        {
            CloseAsyncResult.End(result);
        }

        protected override void OnAbort()
        {
            if (this.innerChannel != null)
            {
                this.innerChannel.Abort();
            }
        }

        void OnInnerChannelFaulted(object sender, EventArgs e)
        {
            this.Fault();
        }

        public TChannel BuildChannel(TimeSpan timeout)
        {
            TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);

            this.InitializeAndFindAsync();

            TChannel innerChannel = null;
            bool completed = false;

            EndpointDiscoveryMetadata currentEndpointDiscoveryMetadata = null;

            try
            {
                do
                {
                    try
                    {
                        currentEndpointDiscoveryMetadata = this.discoveredEndpoints.Dequeue(timeoutHelper.RemainingTime());
                    }
                    catch (TimeoutException te)
                    {
                        throw FxTrace.Exception.AsError(new TimeoutException(SR.DiscoveryClientChannelOpenTimeout(timeoutHelper.OriginalTimeout), te));
                    }

                    if (currentEndpointDiscoveryMetadata == null)
                    {
                        if (this.totalDiscoveredEndpoints < 1)
                        {
                            throw FxTrace.Exception.AsError(new EndpointNotFoundException(SR.DiscoveryClientChannelEndpointNotFound, this.exception));
                        }
                        else
                        {
                            throw FxTrace.Exception.AsError(new EndpointNotFoundException(SR.DiscoveryClientChannelCreationFailed(this.totalDiscoveredEndpoints), this.exception));
                        }
                    }

                    if (timeoutHelper.RemainingTime() == TimeSpan.Zero)
                    {
                        throw FxTrace.Exception.AsError(new TimeoutException(SR.DiscoveryClientChannelOpenTimeout(timeoutHelper.OriginalTimeout)));
                    }

                    if (currentEndpointDiscoveryMetadata.ListenUris.Count == 0)
                    {
                        completed = this.CreateChannel(
                            ref innerChannel,
                            currentEndpointDiscoveryMetadata.Address,
                            currentEndpointDiscoveryMetadata.Address.Uri,
                            timeoutHelper);
                    }
                    else
                    {
                        foreach (Uri listenUri in currentEndpointDiscoveryMetadata.ListenUris)
                        {
                            completed = this.CreateChannel(
                                ref innerChannel,
                                currentEndpointDiscoveryMetadata.Address,
                                listenUri,
                                timeoutHelper);

                            if (completed)
                            {
                                break;
                            }
                        }
                    }
                }
                while (!completed);
            }
            finally
            {
                if (completed && TD.InnerChannelOpenSucceededIsEnabled())
                {
                    TD.InnerChannelOpenSucceeded(
                        currentEndpointDiscoveryMetadata.Address.ToString(),
                        GetVia(innerChannel).ToString());
                }

                this.Cleanup(timeoutHelper.RemainingTime());
            }

            return innerChannel;
        }

        bool CreateChannel(ref TChannel innerChannel, EndpointAddress to, Uri via, TimeoutHelper timeoutHelper)
        {
            bool completed = false;
            Exception exception = null;

            try
            {
                innerChannel = this.innerChannelFactory.CreateChannel(to, via);
                innerChannel.Open(timeoutHelper.RemainingTime());
                completed = true;
            }
            catch (TimeoutException timeoutException)
            {
                throw FxTrace.Exception.AsError(new TimeoutException(SR.DiscoveryClientChannelOpenTimeout(timeoutHelper.OriginalTimeout), timeoutException));
            }
            catch (CommunicationException communicationException)
            {
                exception = communicationException;
            }
            catch (ArgumentException argumentException)
            {
                exception = argumentException;
            }
            catch (InvalidOperationException invalidOperationException)
            {
                exception = invalidOperationException;
            }
            finally
            {
                if (exception != null)
                {
                    TraceInnerChannelFailure(innerChannel, to, via, exception);
                }

                if (!completed && innerChannel != null)
                {
                    innerChannel.Abort();
                    innerChannel = null;
                }
            }

            return completed;
        }

        void OnFindProgressChanged(object sender, FindProgressChangedEventArgs e)
        {
            lock (this.thisLock)
            {
                if (!this.discoveryCompleted)
                {
                    this.discoveredEndpoints.EnqueueAndDispatch(e.EndpointDiscoveryMetadata, null, false);
                    if (++this.totalDiscoveredEndpoints == this.totalExpectedEndpoints)
                    {
                        this.discoveryCompleted = true;
                        this.discoveredEndpoints.Shutdown();
                    }
                }
            }
        }

        void OnFindCompleted(object sender, FindCompletedEventArgs e)
        {
            lock (this.thisLock)
            {
                if (!this.discoveryCompleted)
                {
                    if (e.Error != null ||
                        e.Cancelled ||
                        this.totalDiscoveredEndpoints == e.Result.Endpoints.Count)
                    {
                        this.exception = e.Error;
                        this.discoveryCompleted = true;
                        this.discoveredEndpoints.Shutdown();
                    }
                    else
                    {
                        this.totalExpectedEndpoints = e.Result.Endpoints.Count;
                    }
                }
            }
        }

        void InitializeAndFindAsync()
        {
            DiscoveryEndpoint discoveryEndpoint = this.discoveryEndpointProvider.GetDiscoveryEndpoint();

            if (discoveryEndpoint == null)
            {
                throw FxTrace.Exception.AsError(
                    new InvalidOperationException(
                        SR.DiscoveryMethodImplementationReturnsNull("GetDiscoveryEndpoint", this.discoveryEndpointProvider.GetType())));
            }

            this.discoveryClient = new DiscoveryClient(discoveryEndpoint);
            this.discoveryClient.FindProgressChanged += new EventHandler<FindProgressChangedEventArgs>(OnFindProgressChanged);
            this.discoveryClient.FindCompleted += new EventHandler<FindCompletedEventArgs>(OnFindCompleted);

            SynchronizationContext originalSynchronizationContext = SynchronizationContext.Current;
            if (originalSynchronizationContext != null)
            {
                SynchronizationContext.SetSynchronizationContext(null);

                if (TD.SynchronizationContextSetToNullIsEnabled())
                {
                    TD.SynchronizationContextSetToNull();
                }
            }

            try
            {
                // AsyncOperation uses the SynchronizationContext set during its 
                // initialization to Post the FindProgressed and FindProgressCompleted
                // events. Hence even if the async operation does not complete
                // synchronously, the right SynchronizationContext will be used by
                // AsyncOperation.                
                this.discoveryClient.FindAsync(this.findCriteria, this);
            }
            finally
            {
                if (originalSynchronizationContext != null)
                {
                    SynchronizationContext.SetSynchronizationContext(originalSynchronizationContext);

                    if (TD.SynchronizationContextResetIsEnabled())
                    {
                        TD.SynchronizationContextReset(originalSynchronizationContext.GetType().ToString());
                    }
                }
            }

            if (TD.FindInitiatedInDiscoveryClientChannelIsEnabled())
            {
                TD.FindInitiatedInDiscoveryClientChannel();
            }
        }

        void Cleanup(TimeSpan timeout)
        {
            TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
            Exception exception = null;

            lock (this.thisLock)
            {
                this.discoveryCompleted = true;
            }

            try
            {
                this.discoveryClient.CancelAsync(this);
                ((ICommunicationObject)this.discoveryClient).Close(timeoutHelper.RemainingTime());
            }
            catch (TimeoutException timeoutException)
            {
                exception = timeoutException;
            }
            catch (CommunicationException communicationException)
            {
                exception = communicationException;
            }
            finally
            {
                if (exception != null && TD.DiscoveryClientInClientChannelFailedToCloseIsEnabled())
                {
                    TD.DiscoveryClientInClientChannelFailedToClose(exception);
                }
            }

            this.discoveredEndpoints.Dispose();

            this.discoveryClient = null;
            this.discoveredEndpoints = null;
            this.findCriteria = null;
            this.discoveryEndpointProvider = null;
            this.innerChannelFactory = null;
        }

        static void TraceInnerChannelFailure(TChannel innerChannel, EndpointAddress to, Uri via, Exception exception)
        {
            if (innerChannel == null && TD.InnerChannelCreationFailedIsEnabled())
            {
                TD.InnerChannelCreationFailed(to.ToString(), via.ToString(), exception);
            }
            else if (innerChannel != null && TD.InnerChannelOpenFailedIsEnabled())
            {
                TD.InnerChannelOpenFailed(to.ToString(), via.ToString(), exception);
            }
        }

        static Uri GetVia(TChannel innerChannel)
        {
            Fx.Assert(innerChannel != null, "TChannel must be non null");

            IOutputChannel outputChannel = innerChannel as IOutputChannel;
            if (outputChannel != null)
            {
                return outputChannel.Via;
            }

            IRequestChannel requestChannel = innerChannel as IRequestChannel;
            if (requestChannel != null)
            {
                return requestChannel.Via;
            }

            Fx.Assert("TChannel must be a type derived from IOutputChannel or IRequestChannel");
            return null;
        }

        sealed class DiscoveryChannelBuilderAsyncResult : IteratorAsyncResult<DiscoveryChannelBuilderAsyncResult>
        {
            static AsyncStep openStep;
            static AsyncStep dequeueStep;
            TChannel innerChannel;

            EndpointDiscoveryMetadata currentEndpointDiscoveryMetadata;
            DiscoveryClientChannelBase<TChannel> discoveryClientChannelBase;

            public DiscoveryChannelBuilderAsyncResult(
                DiscoveryClientChannelBase<TChannel> discoveryClientChannelBase,
                TimeSpan timeout,
                AsyncCallback callback,
                object state)
                : base(callback, state)
            {
                this.discoveryClientChannelBase = discoveryClientChannelBase;

                this.Start(this, timeout);
            }

            public static TChannel End(IAsyncResult result)
            {
                DiscoveryChannelBuilderAsyncResult thisPtr = AsyncResult.End<DiscoveryChannelBuilderAsyncResult>(result);
                thisPtr.discoveryClientChannelBase.Cleanup(thisPtr.RemainingTime());
                return thisPtr.innerChannel;
            }

            protected override IEnumerator<AsyncStep> GetAsyncSteps()
            {
                this.discoveryClientChannelBase.InitializeAndFindAsync();

                while (true)
                {
                    this.currentEndpointDiscoveryMetadata = null;
                    yield return DiscoveryChannelBuilderAsyncResult.GetDequeueStep();

                    Exception ex = this.CheckEndpointDiscoveryMetadataAndGetException();
                    if (ex != null)
                    {
                        this.CompleteOnce(ex);
                        yield break;
                    }

                    bool checkListenUris = (currentEndpointDiscoveryMetadata.ListenUris.Count > 0);
                    int index = 0;

                    do
                    {
                        if (checkListenUris)
                        {
                            this.CreateChannel(
                                this.currentEndpointDiscoveryMetadata.Address,
                                this.currentEndpointDiscoveryMetadata.ListenUris[index++]);
                        }
                        else
                        {
                            this.CreateChannel(
                                this.currentEndpointDiscoveryMetadata.Address,
                                this.currentEndpointDiscoveryMetadata.Address.Uri);
                        }

                        if (this.innerChannel != null)
                        {
                            yield return DiscoveryChannelBuilderAsyncResult.GetOpenStep();

                            if (this.innerChannel != null)
                            {
                                // The channel was successfully opened                                  
                                if (TD.InnerChannelOpenSucceededIsEnabled())
                                {
                                    TD.InnerChannelOpenSucceeded(
                                        this.currentEndpointDiscoveryMetadata.Address.ToString(),
                                        GetVia(this.innerChannel).ToString());
                                }

                                yield break;
                            }
                        }
                    }
                    while (index < this.currentEndpointDiscoveryMetadata.ListenUris.Count);
                }
            }

            static AsyncStep GetDequeueStep()
            {
                if (dequeueStep == null)
                {
                    dequeueStep = DiscoveryChannelBuilderAsyncResult.CallAsync(
                        (thisPtr, t, c, s) => thisPtr.discoveryClientChannelBase.discoveredEndpoints.BeginDequeue(thisPtr.RemainingTime(), c, s),
                        (thisPtr, r) => thisPtr.currentEndpointDiscoveryMetadata = thisPtr.discoveryClientChannelBase.discoveredEndpoints.EndDequeue(r),
                        new IAsyncCatch[]
                        {
                            new DiscoveryChannelBuilderAsyncResult.AsyncCatch<TimeoutException>(HandleTimeoutException)
                        });
                }

                return dequeueStep;
            }

            static AsyncStep GetOpenStep()
            {
                if (openStep == null)
                {
                    openStep = DiscoveryChannelBuilderAsyncResult.CallAsync(
                        (thisPtr, t, c, s) => thisPtr.innerChannel.BeginOpen(thisPtr.RemainingTime(), c, s),
                        (thisPtr, r) => thisPtr.innerChannel.EndOpen(r),
                        new IAsyncCatch[]
                        {
                            new DiscoveryChannelBuilderAsyncResult.AsyncCatch<TimeoutException>(HandleTimeoutException),
                            new DiscoveryChannelBuilderAsyncResult.AsyncCatch<CommunicationException>(HandleCommunicationException),
                            new DiscoveryChannelBuilderAsyncResult.AsyncCatch<Exception>(HandleException)
                        });
                }

                return openStep;
            }

            static Exception HandleTimeoutException(DiscoveryChannelBuilderAsyncResult thisPtr, TimeoutException e)
            {
                if (thisPtr.innerChannel != null)
                {
                    thisPtr.innerChannel.Abort();
                    thisPtr.innerChannel = null;
                }

                return new TimeoutException(SR.DiscoveryClientChannelOpenTimeout(thisPtr.OriginalTimeout), e);
            }

            static Exception HandleException(DiscoveryChannelBuilderAsyncResult thisPtr, Exception e)
            {
                if (thisPtr.innerChannel != null)
                {
                    thisPtr.innerChannel.Abort();
                    thisPtr.innerChannel = null;
                }

                return e;
            }

            static Exception HandleCommunicationException(DiscoveryChannelBuilderAsyncResult thisPtr, CommunicationException e)
            {
                if (thisPtr.innerChannel != null)
                {
                    thisPtr.innerChannel.Abort();

                    if (TD.InnerChannelOpenFailedIsEnabled())
                    {
                        TD.InnerChannelOpenFailed(
                            thisPtr.currentEndpointDiscoveryMetadata.Address.ToString(),
                            GetVia(thisPtr.innerChannel).ToString(),
                            e);
                    }

                    thisPtr.innerChannel = null;
                }

                return null;
            }

            void CreateChannel(EndpointAddress address, Uri listenUri)
            {
                Exception exception = null;

                try
                {
                    this.innerChannel = this.discoveryClientChannelBase.innerChannelFactory.CreateChannel(
                        address,
                        listenUri);
                }
                catch (ArgumentException argumentException)
                {
                    exception = argumentException;
                }
                catch (InvalidOperationException invalidOperationException)
                {
                    exception = invalidOperationException;
                }
                catch (CommunicationException communicationException)
                {
                    exception = communicationException;
                    this.CompleteOnce(communicationException);
                }
                finally
                {
                    if (exception != null && TD.InnerChannelCreationFailedIsEnabled())
                    {
                        TD.InnerChannelCreationFailed(address.ToString(), listenUri.ToString(), exception);
                    }
                }

            }

            Exception CheckEndpointDiscoveryMetadataAndGetException()
            {
                if (this.RemainingTime() == TimeSpan.Zero)
                {
                    return new TimeoutException(SR.DiscoveryClientChannelOpenTimeout(this.OriginalTimeout));
                }

                if (this.currentEndpointDiscoveryMetadata == null)
                {
                    string exceptionMessage = (this.discoveryClientChannelBase.totalDiscoveredEndpoints < 1) ?
                        SR.DiscoveryClientChannelEndpointNotFound :
                        SR.DiscoveryClientChannelCreationFailed(this.discoveryClientChannelBase.totalDiscoveredEndpoints);
                    return new EndpointNotFoundException(exceptionMessage, this.discoveryClientChannelBase.exception);
                }

                return null;
            }
        }

        class CloseAsyncResult : AsyncResult
        {
            TChannel innerChannel;

            public CloseAsyncResult(TChannel innerChannel, TimeSpan timeout, AsyncCallback callback, object state)
                : base(callback, state)
            {
                this.innerChannel = innerChannel;

                if (this.innerChannel != null)
                {
                    IAsyncResult closeResult = this.innerChannel.BeginClose(
                        timeout,
                        PrepareAsyncCompletion(new AsyncCompletion(OnCloseCompleted)),
                        this);

                    if (closeResult.CompletedSynchronously && OnCloseCompleted(closeResult))
                    {
                        this.Complete(true);
                    }
                }
                else
                {
                    this.Complete(true);
                }
            }

            public static void End(IAsyncResult result)
            {
                AsyncResult.End<CloseAsyncResult>(result);
            }

            bool OnCloseCompleted(IAsyncResult result)
            {
                this.innerChannel.EndClose(result);
                return true;
            }
        }
    }
}
