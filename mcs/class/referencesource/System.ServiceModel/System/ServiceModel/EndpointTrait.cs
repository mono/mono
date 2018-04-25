//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel
{
    abstract class EndpointTrait<TChannel>
        where TChannel : class
    {
        public abstract ChannelFactory<TChannel> CreateChannelFactory();
    }
}