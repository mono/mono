//------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------

namespace System.ServiceModel.Configuration
{
    using System.Configuration;
    using System.ServiceModel;
    using System.Globalization;

    public partial class WS2007HttpBindingCollectionElement : StandardBindingCollectionElement<WS2007HttpBinding, WS2007HttpBindingElement>
    {
        internal static WS2007HttpBindingCollectionElement GetBindingCollectionElement()
        {
            return (WS2007HttpBindingCollectionElement)ConfigurationHelpers.GetBindingCollectionElement(ConfigurationStrings.WS2007HttpBindingCollectionElementName);
        }

    }
}
