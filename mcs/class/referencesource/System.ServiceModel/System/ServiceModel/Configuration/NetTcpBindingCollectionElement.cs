//------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------

namespace System.ServiceModel.Configuration
{
    using System.Configuration;
    using System.ServiceModel;
    using System.Globalization;

    public partial class NetTcpBindingCollectionElement : StandardBindingCollectionElement<NetTcpBinding, NetTcpBindingElement>
    {
        internal static NetTcpBindingCollectionElement GetBindingCollectionElement()
        {
            return (NetTcpBindingCollectionElement)ConfigurationHelpers.GetBindingCollectionElement(ConfigurationStrings.NetTcpBindingCollectionElementName);
        }

    }
}
