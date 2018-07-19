//------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------

namespace System.ServiceModel.Configuration
{
    using System.Configuration;
    using System.ServiceModel;
    using System.Globalization;

    public partial class NetNamedPipeBindingCollectionElement : StandardBindingCollectionElement<NetNamedPipeBinding, NetNamedPipeBindingElement>
    {
        internal static NetNamedPipeBindingCollectionElement GetBindingCollectionElement()
        {
            return (NetNamedPipeBindingCollectionElement)ConfigurationHelpers.GetBindingCollectionElement(ConfigurationStrings.NetNamedPipeBindingCollectionElementName);
        }

    }
}
