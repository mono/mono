//------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------

namespace System.ServiceModel.Configuration
{
    using System.Configuration;
    using System.ServiceModel;
    using System.Globalization;

    public partial class WSDualHttpBindingCollectionElement : StandardBindingCollectionElement<WSDualHttpBinding, WSDualHttpBindingElement>
    {
        internal static WSDualHttpBindingCollectionElement GetBindingCollectionElement()
        {
            return (WSDualHttpBindingCollectionElement)ConfigurationHelpers.GetBindingCollectionElement(ConfigurationStrings.WSDualHttpBindingCollectionElementName);
        }

    }
}
