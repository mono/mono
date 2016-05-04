//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Channels
{
    using System.Collections.Generic;
    using System.ServiceModel;
    using System.ServiceModel.Description;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Net;
    using System.Runtime.Serialization;
    using System.Text;
    using System.Threading;

    public abstract class ChannelListenerBase : ChannelManagerBase, IChannelListener
    {
        TimeSpan closeTimeout = ServiceDefaults.CloseTimeout;
        TimeSpan openTimeout = ServiceDefaults.OpenTimeout;
        TimeSpan receiveTimeout = ServiceDefaults.ReceiveTimeout;
        TimeSpan sendTimeout = ServiceDefaults.SendTimeout;

        protected ChannelListenerBase()
        {
        }

        protected ChannelListenerBase(IDefaultCommunicationTimeouts timeouts)
        {
            if (timeouts != null)
            {
                this.closeTimeout = timeouts.CloseTimeout;
                this.openTimeout = timeouts.OpenTimeout;
                this.receiveTimeout = timeouts.ReceiveTimeout;
                this.sendTimeout = timeouts.SendTimeout;
            }
        }

        protected override TimeSpan DefaultCloseTimeout
        {
            get { return this.closeTimeout; }
        }

        protected override TimeSpan DefaultOpenTimeout
        {
            get { return this.openTimeout; }
        }

        protected override TimeSpan DefaultReceiveTimeout
        {
            get { return this.receiveTimeout; }
        }

        protected override TimeSpan DefaultSendTimeout
        {
            get { return this.sendTimeout; }
        }

        public abstract Uri Uri { get; }

        public virtual T GetProperty<T>()
            where T : class
        {
            if (typeof(T) == typeof(IChannelListener))
            {
                return (T)(object)this;
            }

            return default(T);
        }

        public bool WaitForChannel(TimeSpan timeout)
        {
            this.ThrowIfNotOpened();
            this.ThrowPending();
            return this.OnWaitForChannel(timeout);
        }

        public IAsyncResult BeginWaitForChannel(TimeSpan timeout, AsyncCallback callback, object state)
        {
            this.ThrowIfNotOpened();
            this.ThrowPending();
            return this.OnBeginWaitForChannel(timeout, callback, state);
        }

        public bool EndWaitForChannel(IAsyncResult result)
        {
            return this.OnEndWaitForChannel(result);
        }

        protected abstract bool OnWaitForChannel(TimeSpan timeout);
        protected abstract IAsyncResult OnBeginWaitForChannel(TimeSpan timeout, AsyncCallback callback, object state);
        protected abstract bool OnEndWaitForChannel(IAsyncResult result);

    }

    public abstract class ChannelListenerBase<TChannel> : ChannelListenerBase, IChannelListener<TChannel>
        where TChannel : class, IChannel
    {
        protected ChannelListenerBase()
        {
        }

        protected ChannelListenerBase(IDefaultCommunicationTimeouts timeouts)
            : base(timeouts)
        {
        }

        protected abstract TChannel OnAcceptChannel(TimeSpan timeout);
        protected abstract IAsyncResult OnBeginAcceptChannel(TimeSpan timeout, AsyncCallback callback, object state);
        protected abstract TChannel OnEndAcceptChannel(IAsyncResult result);

        public TChannel AcceptChannel()
        {
            return this.AcceptChannel(this.InternalReceiveTimeout);
        }

        public TChannel AcceptChannel(TimeSpan timeout)
        {
            this.ThrowIfNotOpened();
            this.ThrowPending();
            return this.OnAcceptChannel(timeout);
        }

        public IAsyncResult BeginAcceptChannel(AsyncCallback callback, object state)
        {
            return this.BeginAcceptChannel(this.InternalReceiveTimeout, callback, state);
        }

        public IAsyncResult BeginAcceptChannel(TimeSpan timeout, AsyncCallback callback, object state)
        {
            this.ThrowIfNotOpened();
            this.ThrowPending();
            return this.OnBeginAcceptChannel(timeout, callback, state);
        }

        public TChannel EndAcceptChannel(IAsyncResult result)
        {
            return this.OnEndAcceptChannel(result);
        }
    }
}
