//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.ServiceModel.Channels;
    using System.ServiceModel;

    public interface IClientMessageInspector
    {
        object BeforeSendRequest(ref Message request, IClientChannel channel);
        void AfterReceiveReply(ref Message reply, object correlationState);
    }
}
