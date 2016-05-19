//------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------

namespace System.ServiceModel.Configuration
{
    using System.Configuration;
    using System.ServiceModel;

    [ObsoleteAttribute ("PeerChannel feature is obsolete and will be removed in the future.", false)]
    public partial class NetPeerTcpBindingCollectionElement : StandardBindingCollectionElement<NetPeerTcpBinding, NetPeerTcpBindingElement>
    {
        internal static NetPeerTcpBindingCollectionElement GetBindingCollectionElement()
        {
            return (NetPeerTcpBindingCollectionElement)ConfigurationHelpers.GetBindingCollectionElement(ConfigurationStrings.NetPeerTcpBindingCollectionElementName);
        }
    }
}
