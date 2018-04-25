//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.ServiceModel.Channels;

    interface IInvokeReceivedNotification
    {
        void NotifyInvokeReceived();
        void NotifyInvokeReceived(RequestContext request);
    }
}
