//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.ServiceModel.Channels;
    using System.ServiceModel;

    public interface ICallContextInitializer
    {
        object BeforeInvoke(InstanceContext instanceContext, IClientChannel channel, Message message);
        void AfterInvoke(object correlationState);
    }
}
