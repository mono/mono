//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Channels
{
    using System.Runtime;
    using System.Threading;

    abstract class SingletonChannelAcceptor<ChannelInterfaceType, TChannel, QueueItemType>
        : InputQueueChannelAcceptor<ChannelInterfaceType>
        where ChannelInterfaceType : class, IChannel
        where TChannel : /*ChannelInterfaceType,*/ InputQueueChannel<QueueItemType>
        where QueueItemType : class, IDisposable
    {
        TChannel currentChannel;
        object currentChannelLock = new object();
        static Action<object> onInvokeDequeuedCallback;

        public SingletonChannelAcceptor(ChannelManagerBase channelManager)
            : base(channelManager)
        {
        }

        public override ChannelInterfaceType AcceptChannel(TimeSpan timeout)
        {
            EnsureChannelAvailable();
            return base.AcceptChannel(timeout);
        }

        public override IAsyncResult BeginAcceptChannel(TimeSpan timeout, AsyncCallback callback, object state)
        {
            EnsureChannelAvailable();
            return base.BeginAcceptChannel(timeout, callback, state);
        }

        protected TChannel GetCurrentChannel()
        {
            return this.currentChannel;
        }

        TChannel EnsureChannelAvailable()
        {
            bool channelCreated = false;
            TChannel newChannel;

            if ((newChannel = currentChannel) == null)
            {
                lock (currentChannelLock)
                {
                    if (IsDisposed)
                    {
                        return null;
                    }

                    if ((newChannel = currentChannel) == null)
                    {
                        newChannel = OnCreateChannel();
                        newChannel.Closed += OnChannelClosed;
                        currentChannel = newChannel;
                        channelCreated = true;
                    }
                }
            }

            if (channelCreated)
            {
                EnqueueAndDispatch((ChannelInterfaceType)(object)newChannel);
            }

            return newChannel;
        }

        protected abstract TChannel OnCreateChannel();
        protected abstract void OnTraceMessageReceived(QueueItemType item);

        public void DispatchItems()
        {
            TChannel channel = EnsureChannelAvailable();
            if (channel != null)
            {
                channel.Dispatch();
            }
        }

        public void Enqueue(QueueItemType item)
        {
            Enqueue(item, null);
        }

        public void Enqueue(QueueItemType item, Action dequeuedCallback)
        {
            Enqueue(item, dequeuedCallback, true);
        }

        public void Enqueue(QueueItemType item, Action dequeuedCallback, bool canDispatchOnThisThread)
        {
            TChannel channel = EnsureChannelAvailable();

            if (DiagnosticUtility.ShouldTraceInformation)
            {
                OnTraceMessageReceived(item);
            }

            if (channel != null)
            {
                channel.EnqueueAndDispatch(item, dequeuedCallback, canDispatchOnThisThread);
            }
            else
            {
                InvokeDequeuedCallback(dequeuedCallback, canDispatchOnThisThread);
                item.Dispose();
            }
        }

        public void Enqueue(Exception exception, Action dequeuedCallback)
        {
            Enqueue(exception, dequeuedCallback, true);
        }

        public void Enqueue(Exception exception, Action dequeuedCallback, bool canDispatchOnThisThread)
        {
            TChannel channel = EnsureChannelAvailable();

            if (channel != null)
            {
                channel.EnqueueAndDispatch(exception, dequeuedCallback, canDispatchOnThisThread);
            }
            else
            {
                InvokeDequeuedCallback(dequeuedCallback, canDispatchOnThisThread);
            }
        }

        public bool EnqueueWithoutDispatch(QueueItemType item, Action dequeuedCallback)
        {
            TChannel channel = EnsureChannelAvailable();

            if (DiagnosticUtility.ShouldTraceInformation)
            {
                OnTraceMessageReceived(item);
            }

            if (channel != null)
            {
                return channel.EnqueueWithoutDispatch(item, dequeuedCallback);
            }
            else
            {
                InvokeDequeuedCallback(dequeuedCallback, false);
                item.Dispose();
                return false;
            }
        }

        public override bool EnqueueWithoutDispatch(Exception exception, Action dequeuedCallback)
        {
            TChannel channel = EnsureChannelAvailable();

            if (channel != null)
            {
                return channel.EnqueueWithoutDispatch(exception, dequeuedCallback);
            }
            else
            {
                InvokeDequeuedCallback(dequeuedCallback, false);
                return false;
            }
        }

        public void EnqueueAndDispatch(QueueItemType item, Action dequeuedCallback, bool canDispatchOnThisThread)
        {
            TChannel channel = EnsureChannelAvailable();

            if (DiagnosticUtility.ShouldTraceInformation)
            {
                OnTraceMessageReceived(item);
            }

            if (channel != null)
            {
                channel.EnqueueAndDispatch(item, dequeuedCallback, canDispatchOnThisThread);
            }
            else
            {
                InvokeDequeuedCallback(dequeuedCallback, canDispatchOnThisThread);
                item.Dispose();
            }
        }

        public override void EnqueueAndDispatch(Exception exception, Action dequeuedCallback, bool canDispatchOnThisThread)
        {
            TChannel channel = EnsureChannelAvailable();

            if (channel != null)
            {
                channel.EnqueueAndDispatch(exception, dequeuedCallback, canDispatchOnThisThread);
            }
            else
            {
                InvokeDequeuedCallback(dequeuedCallback, canDispatchOnThisThread);
            }
        }

        protected void OnChannelClosed(object sender, EventArgs args)
        {
            IChannel channel = (IChannel)sender;
            lock (currentChannelLock)
            {
                if (channel == currentChannel)
                {
                    currentChannel = null;
                }
            }
        }

        static void InvokeDequeuedCallback(Action dequeuedCallback, bool canDispatchOnThisThread)
        {
            if (dequeuedCallback != null)
            {
                if (canDispatchOnThisThread)
                {
                    dequeuedCallback();
                    return;
                }

                if (onInvokeDequeuedCallback == null)
                {
                    onInvokeDequeuedCallback = new Action<object>(OnInvokeDequeuedCallback);
                }

                ActionItem.Schedule(onInvokeDequeuedCallback, dequeuedCallback);
            }
        }

        static void OnInvokeDequeuedCallback(object state)
        {
            Fx.Assert(state != null, "SingletonChannelAcceptor.OnInvokeDequeuedCallback: (state != null)");

            Action dequeuedCallback = (Action)state;
            dequeuedCallback();
        }
    }
}
