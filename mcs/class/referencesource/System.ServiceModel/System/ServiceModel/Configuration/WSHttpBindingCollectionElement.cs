//------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------

namespace System.ServiceModel.Configuration
{
    using System.Configuration;
    using System.ServiceModel;
    using System.Globalization;

    public partial class WSHttpBindingCollectionElement : StandardBindingCollectionElement<WSHttpBinding, WSHttpBindingElement>
    {
        internal static WSHttpBindingCollectionElement GetBindingCollectionElement()
        {
            return (WSHttpBindingCollectionElement)ConfigurationHelpers.GetBindingCollectionElement(ConfigurationStrings.WSHttpBindingCollectionElementName);
        }

    }
}
