//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Runtime.Serialization;

    public class ChannelDispatcherCollection : SynchronizedCollection<ChannelDispatcherBase>
    {        
        ServiceHostBase service;

        internal ChannelDispatcherCollection(ServiceHostBase service, object syncRoot)
            : base(syncRoot)
        {
            if (service == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("service");

            this.service = service;
        }

        protected override void ClearItems()
        {
            ChannelDispatcherBase[] array = new ChannelDispatcherBase[this.Count];
            this.CopyTo(array, 0);
            base.ClearItems();

            if (this.service != null)
            {
                foreach (ChannelDispatcherBase channelDispatcher in array)
                    this.service.OnRemoveChannelDispatcher(channelDispatcher);
            }
        }

        protected override void InsertItem(int index, ChannelDispatcherBase item)
        {
            if (this.service != null)
            {
                if (this.service.State == CommunicationState.Closed)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ObjectDisposedException(this.service.GetType().ToString()));

                this.service.OnAddChannelDispatcher(item);
            }

            base.InsertItem(index, item);
        }

        protected override void RemoveItem(int index)
        {
            ChannelDispatcherBase channelDispatcher = this.Items[index];
            base.RemoveItem(index);
            if (this.service != null)
                this.service.OnRemoveChannelDispatcher(channelDispatcher);
        }

        protected override void SetItem(int index, ChannelDispatcherBase item)
        {
            if (this.service != null)
            {
                if (this.service.State == CommunicationState.Closed)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ObjectDisposedException(this.service.GetType().ToString()));
            }

            if (this.service != null)
                this.service.OnAddChannelDispatcher(item);

            ChannelDispatcherBase old;

            lock (this.SyncRoot)
            {
                old = this.Items[index];
                base.SetItem(index, item);
            }

            if (this.service != null)
                this.service.OnRemoveChannelDispatcher(old);
        }
    }
}
