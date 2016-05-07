//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.ServiceModel.Channels;

    public abstract class ChannelDispatcherBase : CommunicationObject
    {
        public abstract ServiceHostBase Host { get; }
        public abstract IChannelListener Listener { get; }

        internal void AttachInternal(ServiceHostBase host)
        {
            this.Attach(host);
        }

        protected virtual void Attach(ServiceHostBase host)
        {
        }

        internal void DetachInternal(ServiceHostBase host)
        {
            this.Detach(host);
        }

        protected virtual void Detach(ServiceHostBase host)
        {
        }

        public virtual void CloseInput()
        {
        }

        internal virtual void CloseInput(TimeSpan timeout)
        {
            CloseInput(); // back-compat
        }
    }
}
