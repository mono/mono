//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Dispatcher
{
    using System;

    class ImmutableCommunicationTimeouts : IDefaultCommunicationTimeouts
    {
        TimeSpan close;
        TimeSpan open;
        TimeSpan receive;
        TimeSpan send;

        internal ImmutableCommunicationTimeouts()
            : this(null)
        {
        }

        internal ImmutableCommunicationTimeouts(IDefaultCommunicationTimeouts timeouts)
        {
            if (timeouts == null)
            {
                this.close = ServiceDefaults.CloseTimeout;
                this.open = ServiceDefaults.OpenTimeout;
                this.receive = ServiceDefaults.ReceiveTimeout;
                this.send = ServiceDefaults.SendTimeout;
            }
            else
            {
                this.close = timeouts.CloseTimeout;
                this.open = timeouts.OpenTimeout;
                this.receive = timeouts.ReceiveTimeout;
                this.send = timeouts.SendTimeout;
            }
        }

        TimeSpan IDefaultCommunicationTimeouts.CloseTimeout
        {
            get { return this.close; }
        }

        TimeSpan IDefaultCommunicationTimeouts.OpenTimeout
        {
            get { return this.open; }
        }

        TimeSpan IDefaultCommunicationTimeouts.ReceiveTimeout
        {
            get { return this.receive; }
        }

        TimeSpan IDefaultCommunicationTimeouts.SendTimeout
        {
            get { return this.send; }
        }
    }
}
