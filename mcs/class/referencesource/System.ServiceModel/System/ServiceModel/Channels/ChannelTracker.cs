//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Channels
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Diagnostics.Application;

    // Track channels and (optionally) associated state
    class ChannelTracker<TChannel, TState> : CommunicationObject
        where TChannel : IChannel
        where TState : class
    {
        Dictionary<TChannel, TState> receivers;
        EventHandler onInnerChannelClosed;
        EventHandler onInnerChannelFaulted;

        public ChannelTracker()
        {
            this.receivers = new Dictionary<TChannel, TState>();
            this.onInnerChannelClosed = new EventHandler(OnInnerChannelClosed);
            this.onInnerChannelFaulted = new EventHandler(OnInnerChannelFaulted);
        }

        public void Add(TChannel channel, TState channelReceiver)
        {
            bool abortChannel = false;
            lock (this.receivers)
            {
                if (this.State != CommunicationState.Opened)
                {
                    abortChannel = true;
                }
                else
                {
                    this.receivers.Add(channel, channelReceiver);
                }
            }

            if (abortChannel)
            {
                channel.Abort();
            }
        }

        public void PrepareChannel(TChannel channel)
        {
            channel.Faulted += this.onInnerChannelFaulted;
            channel.Closed += this.onInnerChannelClosed;
        }

        void OnInnerChannelFaulted(object sender, EventArgs e)
        {
            ((TChannel)sender).Abort();
        }

        void OnInnerChannelClosed(object sender, EventArgs e)
        {
            // remove the channel from our tracking dictionary
            TChannel channel = (TChannel)sender;
            this.Remove(channel);
            channel.Faulted -= this.onInnerChannelFaulted;
            channel.Closed -= this.onInnerChannelClosed;
        }

        public bool Remove(TChannel channel)
        {
            lock (this.receivers)
            {
                return this.receivers.Remove(channel);
            }
        }

        TChannel[] GetChannels()
        {
            lock (this.receivers)
            {
                TChannel[] channels = new TChannel[this.receivers.Keys.Count];
                this.receivers.Keys.CopyTo(channels, 0);
                this.receivers.Clear();
                return channels;
            }
        }

        protected override void OnAbort()
        {
            TChannel[] channels = GetChannels();
            for (int i = 0; i < channels.Length; i++)
            {
                channels[i].Abort();
            }
        }

        protected override void OnClose(TimeSpan timeout)
        {
            TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
            TChannel[] channels = GetChannels();
            for (int i = 0; i < channels.Length; i++)
            {
                bool success = false;
                try
                {
                    channels[i].Close(timeoutHelper.RemainingTime());
                    success = true;
                }
                catch (CommunicationException e)
                {
                    DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
                }
                catch (TimeoutException e)
                {
                    if (TD.CloseTimeoutIsEnabled())
                    {
                        TD.CloseTimeout(e.Message);
                    }
                    DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
                }
                finally
                {
                    if (!success)
                    {
                        channels[i].Abort();
                    }
                }
            }
        }

        protected override IAsyncResult OnBeginClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            TChannel[] channels = GetChannels();
            List<ICommunicationObject> collection = new List<ICommunicationObject>();
            for (int i = 0; i < channels.Length; i++)
            {
                collection.Add(channels[i]);
            }

            return new CloseCollectionAsyncResult(timeout, callback, state, collection);
        }

        protected override void OnEndClose(IAsyncResult result)
        {
            CloseCollectionAsyncResult.End(result);
        }

        protected override TimeSpan DefaultCloseTimeout
        {
            get { return ServiceDefaults.CloseTimeout; }
        }

        protected override TimeSpan DefaultOpenTimeout
        {
            get { return ServiceDefaults.OpenTimeout; }
        }

        protected override void OnOpen(TimeSpan timeout)
        {
        }

        protected override IAsyncResult OnBeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new CompletedAsyncResult(callback, state);
        }

        protected override void OnEndOpen(IAsyncResult result)
        {
            CompletedAsyncResult.End(result);
        }
    }
}
