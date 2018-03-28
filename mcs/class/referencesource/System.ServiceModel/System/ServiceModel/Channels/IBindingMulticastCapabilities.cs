//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.Channels
{
    public interface IBindingMulticastCapabilities
    {
        // Indicates that messages sent out may come back.
        // One use of this is to avoid sending standard faults.
        bool IsMulticast { get; }
    }
}
