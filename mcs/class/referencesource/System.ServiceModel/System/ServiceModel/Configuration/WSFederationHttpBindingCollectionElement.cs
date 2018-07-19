//------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------

namespace System.ServiceModel.Configuration
{
    using System.Configuration;
    using System.Globalization;

    public partial class WSFederationHttpBindingCollectionElement : StandardBindingCollectionElement<WSFederationHttpBinding, WSFederationHttpBindingElement>
    {
        internal static WSFederationHttpBindingCollectionElement GetBindingCollectionElement()
        {
            return (WSFederationHttpBindingCollectionElement)ConfigurationHelpers.GetBindingCollectionElement(ConfigurationStrings.WSFederationHttpBindingCollectionElementName);
        }

    }
}
