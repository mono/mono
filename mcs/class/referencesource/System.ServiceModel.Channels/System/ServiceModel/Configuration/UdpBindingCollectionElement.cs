//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Configuration
{
    public class UdpBindingCollectionElement : StandardBindingCollectionElement<UdpBinding, UdpBindingElement>
    {
        internal static UdpBindingCollectionElement GetBindingCollectionElement()
        {
            return (UdpBindingCollectionElement)ConfigurationHelpers.GetBindingCollectionElement(UdpTransportConfigurationStrings.UdpBindingElementName);
        }
    }
}
