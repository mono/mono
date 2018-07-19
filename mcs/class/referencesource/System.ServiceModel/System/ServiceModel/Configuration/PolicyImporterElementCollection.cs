//------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------

namespace System.ServiceModel.Configuration
{
    using System;
    using System.Collections;
    using System.Configuration;
    using System.Globalization;
    using System.ServiceModel;
    using System.ServiceModel.Configuration;
    using System.ServiceModel.Channels;

    [ConfigurationCollection(typeof(PolicyImporterElement), AddItemName = ConfigurationStrings.Extension)]
    public sealed class PolicyImporterElementCollection : ServiceModelEnhancedConfigurationElementCollection<PolicyImporterElement>
    {
        public PolicyImporterElementCollection() : base(ConfigurationStrings.Extension)
        {
        }

        protected override Object GetElementKey(ConfigurationElement element) 
        {
            if (element == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("element");
            }
            PolicyImporterElement configElementKey = (PolicyImporterElement)element;
            return configElementKey.Type;
        }

        internal void SetDefaults()
        {
            this.Add(new PolicyImporterElement(typeof(PrivacyNoticeBindingElementImporter)));
            this.Add(new PolicyImporterElement(typeof(UseManagedPresentationBindingElementImporter)));
            this.Add(new PolicyImporterElement(typeof(TransactionFlowBindingElementImporter)));
            this.Add(new PolicyImporterElement(typeof(ReliableSessionBindingElementImporter)));
            this.Add(new PolicyImporterElement(typeof(SecurityBindingElementImporter)));
            this.Add(new PolicyImporterElement(typeof(CompositeDuplexBindingElementImporter)));
            this.Add(new PolicyImporterElement(typeof(OneWayBindingElementImporter)));
            this.Add(new PolicyImporterElement(typeof(MessageEncodingBindingElementImporter)));
            this.Add(new PolicyImporterElement(typeof(TransportBindingElementImporter)));
            this.Add(new PolicyImporterElement(ConfigurationStrings.UdpTransportImporterType));
        }
    }
}


