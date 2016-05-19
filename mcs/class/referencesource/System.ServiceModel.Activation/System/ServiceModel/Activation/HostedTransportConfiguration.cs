//----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------------------

namespace System.ServiceModel.Activation
{
    using System.Runtime.CompilerServices;

    [TypeForwardedFrom("System.ServiceModel, Version=3.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089")]
    public abstract class HostedTransportConfiguration
    {
        public abstract Uri[] GetBaseAddresses(string virtualPath);
    }
}
