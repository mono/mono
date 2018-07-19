//------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------

namespace System.ServiceModel.Configuration
{
    using System;
    using System.ServiceModel.Description;
    using System.Collections;
    using System.Configuration;
    using System.Globalization;
    using System.ServiceModel;
    using System.ServiceModel.Configuration;
    using System.ServiceModel.Channels;

    [ConfigurationCollection(typeof(WsdlImporterElement), AddItemName = ConfigurationStrings.Extension)]
    public sealed class WsdlImporterElementCollection : ServiceModelEnhancedConfigurationElementCollection<WsdlImporterElement>
    {
        public WsdlImporterElementCollection() : base(ConfigurationStrings.Extension)
        {
        }

        protected override Object GetElementKey(ConfigurationElement element) 
        {
            if (element == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("element");
            }
            WsdlImporterElement configElementKey = (WsdlImporterElement)element;
            return configElementKey.Type;
        }

        internal void SetDefaults()
        {
            this.Add(new WsdlImporterElement(typeof(DataContractSerializerMessageContractImporter)));
            this.Add(new WsdlImporterElement(typeof(XmlSerializerMessageContractImporter)));
            this.Add(new WsdlImporterElement(typeof(MessageEncodingBindingElementImporter)));
            this.Add(new WsdlImporterElement(typeof(TransportBindingElementImporter)));
            this.Add(new WsdlImporterElement(typeof(StandardBindingImporter)));
            this.Add(new WsdlImporterElement(ConfigurationStrings.UdpTransportImporterType));
        }
    }
}


