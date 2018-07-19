//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel
{
    using System.Collections.Generic;
    using System.Runtime;
    using System.ServiceModel.Channels;
    using System.Threading;

    delegate void InstanceContextEmptyCallback(InstanceContext instanceContext);

    class ServiceChannelManager : LifetimeManager
    {
        int activityCount;
        ICommunicationWaiter activityWaiter;
        int activityWaiterCount;
        InstanceContextEmptyCallback emptyCallback;
        IChannel firstIncomingChannel;
        ChannelCollection incomingChannels;
        ChannelCollection outgoingChannels;
        InstanceContext instanceContext;

        public ServiceChannelManager(InstanceContext instanceContext)
            : this(instanceContext, null)
        {
        }

        public ServiceChannelManager(InstanceContext instanceContext, InstanceContextEmptyCallback emptyCallback)
            : base(instanceContext.ThisLock)
        {
            this.instanceContext = instanceContext;
            this.emptyCallback = emptyCallback;
        }

        public int ActivityCount
        {
            get { return this.activityCount; }
        }

        public ICollection<IChannel> IncomingChannels
        {
            get
            {
                this.EnsureIncomingChannelCollection();
                return (ICollection<IChannel>)this.incomingChannels;
            }
        }

        public ICollection<IChannel> OutgoingChannels
        {
            get
            {
                if (this.outgoingChannels == null)
                {
                    lock (this.ThisLock)
                    {
                        if (this.outgoingChannels == null)
                            this.outgoingChannels = new ChannelCollection(this, this.ThisLock);
                    }
                }
                return this.outgoingChannels;
            }
        }

        public bool IsBusy
        {
            get
            {
                if (this.ActivityCount > 0)
                    return true;

                if (base.BusyCount > 0)
                    return true;

                ICollection<IChannel> outgoing = this.outgoingChannels;
                if ((outgoing != null) && (outgoing.Count > 0))
                    return true;

                return false;
            }
        }

        public void AddIncomingChannel(IChannel channel)
        {
            bool added = false;

            lock (this.ThisLock)
            {
                if (this.State == LifetimeState.Opened)
                {
                    if (this.firstIncomingChannel == null)
                    {
                        if (this.incomingChannels == null)
                        {
                            this.firstIncomingChannel = channel;
                            this.ChannelAdded(channel);
                        }
                        else
                        {
                            if (this.incomingChannels.Contains(channel))
                                return;
                            this.incomingChannels.Add(channel);
                        }
                    }
                    else
                    {
                        this.EnsureIncomingChannelCollection();
                        if (this.incomingChannels.Contains(channel))
                            return;
                        this.incomingChannels.Add(channel);
                    }
                    added = true;
                }
            }

            if (!added)
            {
                channel.Abort();
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ObjectDisposedException(this.GetType().ToString()));
            }
        }

        public IAsyncResult BeginCloseInput(TimeSpan timeout, AsyncCallback callback, object state)
        {
            CloseCommunicationAsyncResult closeResult = null;

            lock (this.ThisLock)
            {
                if (this.activityCount > 0)
                {
                    closeResult = new CloseCommunicationAsyncResult(timeout, callback, state, this.ThisLock);

                    if (!(this.activityWaiter == null))
                    {
                        Fx.Assert("ServiceChannelManager.BeginCloseInput: (this.activityWaiter == null)");
                    }
                    this.activityWaiter = closeResult;
                    Interlocked.Increment(ref this.activityWaiterCount);
                }
            }

            if (closeResult != null)
                return closeResult;
            else
                return new CompletedAsyncResult(callback, state);
        }

        void ChannelAdded(IChannel channel)
        {
            base.IncrementBusyCount();
            channel.Closed += this.OnChannelClosed;
        }

        void ChannelRemoved(IChannel channel)
        {
            channel.Closed -= this.OnChannelClosed;
            base.DecrementBusyCount();
        }


        public void CloseInput(TimeSpan timeout)
        {
            SyncCommunicationWaiter activityWaiter = null;

            lock (this.ThisLock)
            {
                if (this.activityCount > 0)
                {
                    activityWaiter = new SyncCommunicationWaiter(this.ThisLock);
                    if (!(this.activityWaiter == null))
                    {
                        Fx.Assert("ServiceChannelManager.CloseInput: (this.activityWaiter == null)");
                    }
                    this.activityWaiter = activityWaiter;
                    Interlocked.Increment(ref this.activityWaiterCount);
                }
            }

            if (activityWaiter != null)
            {
                CommunicationWaitResult result = activityWaiter.Wait(timeout, false);
                if (Interlocked.Decrement(ref this.activityWaiterCount) == 0)
                {
                    activityWaiter.Dispose();
                    this.activityWaiter = null;
                }

                switch (result)
                {
                    case CommunicationWaitResult.Expired:
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new TimeoutException(SR.GetString(SR.SfxCloseTimedOutWaitingForDispatchToComplete)));
                    case CommunicationWaitResult.Aborted:
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ObjectDisposedException(this.GetType().ToString()));
                }
            }
        }

        public void DecrementActivityCount()
        {
            ICommunicationWaiter activityWaiter = null;
            bool empty = false;

            lock (this.ThisLock)
            {
                if (!(this.activityCount > 0))
                {
                    Fx.Assert("ServiceChannelManager.DecrementActivityCount: (this.activityCount > 0)");
                }
                if (--this.activityCount == 0)
                {
                    if (this.activityWaiter != null)
                    {
                        activityWaiter = this.activityWaiter;
                        Interlocked.Increment(ref this.activityWaiterCount);
                    }
                    if (this.BusyCount == 0)
                        empty = true;
                }
            }

            if (activityWaiter != null)
            {
                activityWaiter.Signal();
                if (Interlocked.Decrement(ref this.activityWaiterCount) == 0)
                {
                    activityWaiter.Dispose();
                    this.activityWaiter = null;
                }
            }

            if (empty && this.State == LifetimeState.Opened)
                OnEmpty();
        }

        public void EndCloseInput(IAsyncResult result)
        {
            if (result is CloseCommunicationAsyncResult)
            {
                CloseCommunicationAsyncResult.End(result);
                if (Interlocked.Decrement(ref this.activityWaiterCount) == 0)
                {
                    this.activityWaiter.Dispose();
                    this.activityWaiter = null;
                }
            }
            else
                CompletedAsyncResult.End(result);
        }

        void EnsureIncomingChannelCollection()
        {
            lock (this.ThisLock)
            {
                if (this.incomingChannels == null)
                {
                    this.incomingChannels = new ChannelCollection(this, this.ThisLock);
                    if (this.firstIncomingChannel != null)
                    {
                        this.incomingChannels.Add(this.firstIncomingChannel);
                        this.ChannelRemoved(this.firstIncomingChannel); // Adding to collection called ChannelAdded, so call ChannelRemoved to balance
                        this.firstIncomingChannel = null;
                    }
                }
            }
        }

        public void IncrementActivityCount()
        {
            lock (this.ThisLock)
            {
                if (this.State == LifetimeState.Closed)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ObjectDisposedException(this.GetType().ToString()));
                this.activityCount++;
            }
        }

        protected override void IncrementBusyCount()
        {
            base.IncrementBusyCount();
        }

        protected override void OnAbort()
        {
            IChannel[] channels = this.SnapshotChannels();
            for (int index = 0; index < channels.Length; index++)
                channels[index].Abort();

            ICommunicationWaiter activityWaiter = null;

            lock (this.ThisLock)
            {
                if (this.activityWaiter != null)
                {
                    activityWaiter = this.activityWaiter;
                    Interlocked.Increment(ref this.activityWaiterCount);
                }
            }

            if (activityWaiter != null)
            {
                activityWaiter.Signal();
                if (Interlocked.Decrement(ref this.activityWaiterCount) == 0)
                {
                    activityWaiter.Dispose();
                    this.activityWaiter = null;
                }
            }

            base.OnAbort();
        }

        protected override IAsyncResult OnBeginClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new ChainedAsyncResult(timeout, callback, state, BeginCloseInput, EndCloseInput, OnBeginCloseContinue, OnEndCloseContinue);
        }

        IAsyncResult OnBeginCloseContinue(TimeSpan timeout, AsyncCallback callback, object state)
        {
            TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
            return base.OnBeginClose(timeoutHelper.RemainingTime(), callback, state);
        }

        protected override void OnClose(TimeSpan timeout)
        {
            TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);

            this.CloseInput(timeoutHelper.RemainingTime());

            base.OnClose(timeoutHelper.RemainingTime());
        }

        protected override void OnEndClose(IAsyncResult result)
        {
            ChainedAsyncResult.End(result);
        }

        void OnEndCloseContinue(IAsyncResult result)
        {
            base.OnEndClose(result);
        }

        protected override void OnEmpty()
        {
            if (this.emptyCallback != null)
                this.emptyCallback(this.instanceContext);
        }

        void OnChannelClosed(object sender, EventArgs args)
        {
            this.RemoveChannel((IChannel)sender);
        }

        public bool RemoveChannel(IChannel channel)
        {
            lock (this.ThisLock)
            {
                if (this.firstIncomingChannel == channel)
                {
                    this.firstIncomingChannel = null;
                    this.ChannelRemoved(channel);
                    return true;
                }
                else if (this.incomingChannels != null && this.incomingChannels.Contains(channel))
                {
                    this.incomingChannels.Remove(channel);
                    return true;
                }
                else if (this.outgoingChannels != null && this.outgoingChannels.Contains(channel))
                {
                    this.outgoingChannels.Remove(channel);
                    return true;
                }
            }

            return false;
        }

        public IChannel[] SnapshotChannels()
        {
            lock (this.ThisLock)
            {
                int outgoingCount = (this.outgoingChannels != null ? this.outgoingChannels.Count : 0);

                if (this.firstIncomingChannel != null)
                {
                    IChannel[] channels = new IChannel[1 + outgoingCount];
                    channels[0] = this.firstIncomingChannel;
                    if (outgoingCount > 0)
                        this.outgoingChannels.CopyTo(channels, 1);
                    return channels;
                }

                if (this.incomingChannels != null)
                {
                    IChannel[] channels = new IChannel[this.incomingChannels.Count + outgoingCount];
                    this.incomingChannels.CopyTo(channels, 0);
                    if (outgoingCount > 0)
                        this.outgoingChannels.CopyTo(channels, this.incomingChannels.Count);
                    return channels;
                }

                if (outgoingCount > 0)
                {
                    IChannel[] channels = new IChannel[outgoingCount];
                    this.outgoingChannels.CopyTo(channels, 0);
                    return channels;
                }
            }
            return EmptyArray<IChannel>.Allocate(0);
        }

        class ChannelCollection : ICollection<IChannel>
        {
            ServiceChannelManager channelManager;
            object syncRoot;
            HashSet<IChannel> hashSet = new HashSet<IChannel>();

            public bool IsReadOnly
            {
                get { return false; }
            }

            public int Count
            {
                get
                {
                    lock (this.syncRoot)
                    {
                        return this.hashSet.Count;
                    }
                }
            }

            public ChannelCollection(ServiceChannelManager channelManager, object syncRoot)
            {
                if (syncRoot == null)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("syncRoot"));

                this.channelManager = channelManager;
                this.syncRoot = syncRoot;
            }

            public void Add(IChannel channel)
            {
                lock (this.syncRoot)
                {
                    if (this.hashSet.Add(channel))
                    {
                        this.channelManager.ChannelAdded(channel);
                    }
                }
            }

            public void Clear()
            {
                lock (this.syncRoot)
                {
                    foreach (IChannel channel in this.hashSet)
                        this.channelManager.ChannelRemoved(channel);
                    this.hashSet.Clear();
                }
            }

            public bool Contains(IChannel channel)
            {
                lock (this.syncRoot)
                {
                    if (channel != null)
                    {
                        return this.hashSet.Contains(channel);
                    }
                    return false;
                }
            }

            public void CopyTo(IChannel[] array, int arrayIndex)
            {
                lock (this.syncRoot)
                {
                    this.hashSet.CopyTo(array, arrayIndex);
                }
            }

            public bool Remove(IChannel channel)
            {
                lock (this.syncRoot)
                {
                    bool ret = false;
                    if (channel != null)
                    {
                        ret = this.hashSet.Remove(channel);
                        if (ret)
                        {
                            this.channelManager.ChannelRemoved(channel);
                        }
                    }
                    return ret;
                }
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                lock (this.syncRoot)
                {
                    return this.hashSet.GetEnumerator();
                }
            }

            IEnumerator<IChannel> IEnumerable<IChannel>.GetEnumerator()
            {
                lock (this.syncRoot)
                {
                    return this.hashSet.GetEnumerator();
                }
            }
        }
    }
}
