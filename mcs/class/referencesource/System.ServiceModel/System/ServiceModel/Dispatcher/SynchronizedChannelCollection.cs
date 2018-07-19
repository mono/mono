//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.Collections.Generic;
    using System.ServiceModel.Channels;

    class SynchronizedChannelCollection<TChannel> : SynchronizedCollection<TChannel>
        where TChannel : IChannel
    {
        EventHandler onChannelClosed;
        EventHandler onChannelFaulted;

        internal SynchronizedChannelCollection(object syncRoot)
            : base(syncRoot)
        {
            this.onChannelClosed = new EventHandler(OnChannelClosed);
            this.onChannelFaulted = new EventHandler(OnChannelFaulted);
        }

        void AddingChannel(TChannel channel)
        {
            channel.Faulted += this.onChannelFaulted;
            channel.Closed += this.onChannelClosed;
        }

        void RemovingChannel(TChannel channel)
        {
            channel.Faulted -= this.onChannelFaulted;
            channel.Closed -= this.onChannelClosed;
        }

        void OnChannelClosed(object sender, EventArgs args)
        {
            TChannel channel = (TChannel)sender;
            this.Remove(channel);
        }

        void OnChannelFaulted(object sender, EventArgs args)
        {
            TChannel channel = (TChannel)sender;
            this.Remove(channel);
        }

        protected override void ClearItems()
        {
            List<TChannel> items = this.Items;

            for (int i = 0; i < items.Count; i++)
            {
                this.RemovingChannel(items[i]);
            }

            base.ClearItems();
        }

        protected override void InsertItem(int index, TChannel item)
        {
            this.AddingChannel(item);
            base.InsertItem(index, item);
        }

        protected override void RemoveItem(int index)
        {
            TChannel oldItem = this.Items[index];

            base.RemoveItem(index);
            this.RemovingChannel(oldItem);
        }

        protected override void SetItem(int index, TChannel item)
        {
            TChannel oldItem = this.Items[index];

            this.AddingChannel(item);
            base.SetItem(index, item);
            this.RemovingChannel(oldItem);
        }
    }
}
