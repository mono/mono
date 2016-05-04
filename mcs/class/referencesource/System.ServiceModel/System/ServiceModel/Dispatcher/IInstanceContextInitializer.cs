//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.ServiceModel.Channels;
    using System.ServiceModel;

    public interface IInstanceContextInitializer
    {
        // message=null for singleton
        void Initialize(InstanceContext instanceContext, Message message);
    }
}
