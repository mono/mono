//------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------

namespace System.ServiceModel.Configuration
{
    using System.ServiceModel;
    using System.ServiceModel.Activation;
    using System.ServiceModel.Channels;

    public partial class WebHttpBindingCollectionElement : StandardBindingCollectionElement<WebHttpBinding, WebHttpBindingElement>
    {

        protected internal override Binding GetDefault()
        {
            return new WebHttpBinding();
        }

        internal static WebHttpBindingCollectionElement GetBindingCollectionElement()
        {
            string sectionPath = "system.serviceModel/bindings";

            BindingsSection bindings = (BindingsSection)AspNetEnvironment.Current.GetConfigurationSection(sectionPath);

            return (WebHttpBindingCollectionElement)bindings[WebHttpBinding.WebHttpBindingConfigurationStrings.WebHttpBindingCollectionElementName];
        }
    }
}
