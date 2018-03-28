// <copyright>
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>

namespace System.ServiceModel
{
    using System;
    using System.ServiceModel.Channels;

    /// <summary>
    /// An interface used by ChannelBase to override the ServiceChannel that would normally be returned by ClientBase. 
    /// </summary>
    internal interface IChannelBaseProxy
    {
        ServiceChannel GetServiceChannel();
    }
}
