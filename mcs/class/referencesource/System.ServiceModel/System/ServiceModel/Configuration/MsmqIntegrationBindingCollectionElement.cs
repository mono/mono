//------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------

namespace System.ServiceModel.Configuration
{
    using System.Configuration;
    using System.Globalization;

    public partial class MsmqIntegrationBindingCollectionElement : StandardBindingCollectionElement<System.ServiceModel.MsmqIntegration.MsmqIntegrationBinding, MsmqIntegrationBindingElement>
    {
        internal static MsmqIntegrationBindingCollectionElement GetBindingCollectionElement()
        {
            return (MsmqIntegrationBindingCollectionElement)ConfigurationHelpers.GetBindingCollectionElement(ConfigurationStrings.MsmqIntegrationBindingCollectionElementName);
        }

    }
}
