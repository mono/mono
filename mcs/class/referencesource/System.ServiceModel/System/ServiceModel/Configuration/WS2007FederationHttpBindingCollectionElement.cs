//------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------

namespace System.ServiceModel.Configuration
{
    using System.Configuration;
    using System.ServiceModel;
    using System.Globalization;

    public partial class WS2007FederationHttpBindingCollectionElement : StandardBindingCollectionElement<WS2007FederationHttpBinding, WS2007FederationHttpBindingElement>
    {
        internal static WS2007FederationHttpBindingCollectionElement GetBindingCollectionElement()
        {
            return (WS2007FederationHttpBindingCollectionElement)ConfigurationHelpers.GetBindingCollectionElement(ConfigurationStrings.WS2007FederationHttpBindingCollectionElementName);
        }

    }
}

