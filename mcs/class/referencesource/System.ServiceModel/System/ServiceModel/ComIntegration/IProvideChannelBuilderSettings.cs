//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------
namespace System.ServiceModel.ComIntegration
{
    using System;
    using System.ServiceModel.Description;
    using System.Runtime.InteropServices;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.Collections.Generic;

    interface IProvideChannelBuilderSettings
    {
        ServiceChannelFactory ServiceChannelFactoryReadWrite
        {
            get;
        }
        ServiceChannelFactory ServiceChannelFactoryReadOnly
        {
            get;
        }
        KeyedByTypeCollection<IEndpointBehavior> Behaviors
        {
            get;
        }
        ServiceChannel ServiceChannel
        {
            get;
        }
    }
}
