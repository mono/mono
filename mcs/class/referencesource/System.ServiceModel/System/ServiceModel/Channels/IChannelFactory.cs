//----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------------------

namespace System.ServiceModel.Channels
{
    using System;
    using System.ServiceModel;

    public interface IChannelFactory : ICommunicationObject
    {
        T GetProperty<T>() where T : class;
    }

    public interface IChannelFactory<TChannel> : IChannelFactory
    {
        TChannel CreateChannel(EndpointAddress to);
        TChannel CreateChannel(EndpointAddress to, Uri via);
    }
}
