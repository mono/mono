//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Channels
{
    using System.Runtime;
    using System.ServiceModel;

    abstract class LayeredChannelListener<TChannel>
        : ChannelListenerBase<TChannel>
        where TChannel : class, IChannel
    {
        IChannelListener innerChannelListener;
        bool sharedInnerListener;
        EventHandler onInnerListenerFaulted;

        protected LayeredChannelListener(IDefaultCommunicationTimeouts timeouts, IChannelListener innerChannelListener)
            : this(false, timeouts, innerChannelListener)
        {
        }

        protected LayeredChannelListener(bool sharedInnerListener)
            : this(sharedInnerListener, null, null)
        {
        }

        protected LayeredChannelListener(bool sharedInnerListener, IDefaultCommunicationTimeouts timeouts)
            : this(sharedInnerListener, timeouts, null)
        {
        }

        protected LayeredChannelListener(bool sharedInnerListener, IDefaultCommunicationTimeouts timeouts, IChannelListener innerChannelListener)
            : base(timeouts)
        {
            this.sharedInnerListener = sharedInnerListener;
            this.innerChannelListener = innerChannelListener;
            this.onInnerListenerFaulted = new EventHandler(OnInnerListenerFaulted);
            if (this.innerChannelListener != null)
            {
                this.innerChannelListener.Faulted += onInnerListenerFaulted;
            }
        }

        internal virtual IChannelListener InnerChannelListener
        {
            get
            {
                return innerChannelListener;
            }
            set
            {
                lock (ThisLock)
                {
                    ThrowIfDisposedOrImmutable();
                    if (this.innerChannelListener != null)
                    {
                        this.innerChannelListener.Faulted -= onInnerListenerFaulted;
                    }
                    this.innerChannelListener = value;
                    if (this.innerChannelListener != null)
                    {
                        this.innerChannelListener.Faulted += onInnerListenerFaulted;
                    }
                }
            }
        }

        internal bool SharedInnerListener
        {
            get { return sharedInnerListener; }
        }

        public override Uri Uri
        {
            get { return GetInnerListenerSnapshot().Uri; }
        }

        public override T GetProperty<T>()
        {
            T baseProperty = base.GetProperty<T>();
            if (baseProperty != null)
            {
                return baseProperty;
            }

            IChannelListener channelListener = this.InnerChannelListener;
            if (channelListener != null)
            {
                return channelListener.GetProperty<T>();
            }
            else
            {
                return default(T);
            }
        }

        protected override void OnAbort()
        {
            lock (ThisLock)
            {
                this.OnCloseOrAbort();
            }
            IChannelListener channelListener = this.InnerChannelListener;
            if (channelListener != null && !sharedInnerListener)
            {
                channelListener.Abort();
            }
        }

        protected override IAsyncResult OnBeginClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            this.OnCloseOrAbort();
            return new CloseAsyncResult(InnerChannelListener, sharedInnerListener, timeout, callback, state);
        }

        protected override void OnEndClose(IAsyncResult result)
        {
            CloseAsyncResult.End(result);
        }

        protected override void OnClose(TimeSpan timeout)
        {
            this.OnCloseOrAbort();
            if (InnerChannelListener != null && !sharedInnerListener)
            {
                InnerChannelListener.Close(timeout);
            }
        }

        void OnCloseOrAbort()
        {
            IChannelListener channelListener = this.InnerChannelListener;
            if (channelListener != null)
            {
                channelListener.Faulted -= onInnerListenerFaulted;
            }
        }

        protected override IAsyncResult OnBeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new OpenAsyncResult(InnerChannelListener, sharedInnerListener, timeout, callback, state);
        }

        protected override void OnEndOpen(IAsyncResult result)
        {
            OpenAsyncResult.End(result);
        }

        protected override void OnOpen(TimeSpan timeout)
        {
            if (InnerChannelListener != null && !sharedInnerListener)
                InnerChannelListener.Open(timeout);
        }

        protected override void OnOpening()
        {
            base.OnOpening();
            ThrowIfInnerListenerNotSet();
        }

        void OnInnerListenerFaulted(object sender, EventArgs e)
        {
            // if our inner listener faulted, we should fault as well
            this.Fault();
        }

        internal void ThrowIfInnerListenerNotSet()
        {
            if (this.InnerChannelListener == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.InnerListenerFactoryNotSet, this.GetType().ToString())));
            }
        }

        internal IChannelListener GetInnerListenerSnapshot()
        {
            IChannelListener innerChannelListener = this.InnerChannelListener;

            if (innerChannelListener == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.InnerListenerFactoryNotSet, this.GetType().ToString())));
            }

            return innerChannelListener;
        }

        class OpenAsyncResult : AsyncResult
        {
            ICommunicationObject communicationObject;
            static AsyncCallback onOpenComplete = Fx.ThunkCallback(new AsyncCallback(OnOpenComplete));

            public OpenAsyncResult(ICommunicationObject communicationObject, bool sharedInnerListener, TimeSpan timeout, AsyncCallback callback, object state)
                : base(callback, state)
            {
                this.communicationObject = communicationObject;

                if (this.communicationObject == null || sharedInnerListener)
                {
                    this.Complete(true);
                    return;
                }

                IAsyncResult result = this.communicationObject.BeginOpen(timeout, onOpenComplete, this);
                if (result.CompletedSynchronously)
                {
                    this.communicationObject.EndOpen(result);
                    this.Complete(true);
                }
            }

            static void OnOpenComplete(IAsyncResult result)
            {
                if (result.CompletedSynchronously)
                    return;

                OpenAsyncResult thisPtr = (OpenAsyncResult)result.AsyncState;
                Exception exception = null;

                try
                {
                    thisPtr.communicationObject.EndOpen(result);
                }
#pragma warning suppress 56500 // covered by FxCOP
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                        throw;

                    exception = e;
                }

                thisPtr.Complete(false, exception);
            }

            public static void End(IAsyncResult result)
            {
                AsyncResult.End<OpenAsyncResult>(result);
            }
        }

        class CloseAsyncResult : AsyncResult
        {
            ICommunicationObject communicationObject;
            static AsyncCallback onCloseComplete = Fx.ThunkCallback(new AsyncCallback(OnCloseComplete));

            public CloseAsyncResult(ICommunicationObject communicationObject, bool sharedInnerListener, TimeSpan timeout, AsyncCallback callback, object state)
                : base(callback, state)
            {
                this.communicationObject = communicationObject;

                if (this.communicationObject == null || sharedInnerListener)
                {
                    this.Complete(true);
                    return;
                }

                IAsyncResult result = this.communicationObject.BeginClose(timeout, onCloseComplete, this);

                if (result.CompletedSynchronously)
                {
                    this.communicationObject.EndClose(result);
                    this.Complete(true);
                }
            }

            static void OnCloseComplete(IAsyncResult result)
            {
                if (result.CompletedSynchronously)
                    return;

                CloseAsyncResult thisPtr = (CloseAsyncResult)result.AsyncState;
                Exception exception = null;

                try
                {
                    thisPtr.communicationObject.EndClose(result);
                }
#pragma warning suppress 56500 // covered by FxCOP
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                        throw;

                    exception = e;
                }

                thisPtr.Complete(false, exception);
            }

            public static void End(IAsyncResult result)
            {
                AsyncResult.End<CloseAsyncResult>(result);
            }
        }
    }

    abstract class LayeredChannelAcceptor<TChannel, TInnerChannel> : ChannelAcceptor<TChannel>
        where TChannel : class, IChannel
        where TInnerChannel : class, IChannel
    {
        IChannelListener<TInnerChannel> innerListener;

        protected LayeredChannelAcceptor(ChannelManagerBase channelManager, IChannelListener<TInnerChannel> innerListener)
            : base(channelManager)
        {
            this.innerListener = innerListener;
        }

        protected abstract TChannel OnAcceptChannel(TInnerChannel innerChannel);

        public override TChannel AcceptChannel(TimeSpan timeout)
        {
            TInnerChannel innerChannel = this.innerListener.AcceptChannel(timeout);
            if (innerChannel == null)
                return null;
            else
                return OnAcceptChannel(innerChannel);
        }

        public override IAsyncResult BeginAcceptChannel(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return this.innerListener.BeginAcceptChannel(timeout, callback, state);
        }

        public override TChannel EndAcceptChannel(IAsyncResult result)
        {
            TInnerChannel innerChannel = this.innerListener.EndAcceptChannel(result);
            if (innerChannel == null)
                return null;
            else
                return OnAcceptChannel(innerChannel);
        }

        public override bool WaitForChannel(TimeSpan timeout)
        {
            return this.innerListener.WaitForChannel(timeout);
        }

        public override IAsyncResult BeginWaitForChannel(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return this.innerListener.BeginWaitForChannel(timeout, callback, state);
        }

        public override bool EndWaitForChannel(IAsyncResult result)
        {
            return this.innerListener.EndWaitForChannel(result);
        }
    }
}
