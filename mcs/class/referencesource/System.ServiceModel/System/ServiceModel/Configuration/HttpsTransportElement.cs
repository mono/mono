//------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------

namespace System.ServiceModel.Configuration
{
    using System.Configuration;
    using System.ServiceModel.Channels;

    public partial class HttpsTransportElement : HttpTransportElement
    {
        public HttpsTransportElement() 
        {
        }

        public override void ApplyConfiguration(BindingElement bindingElement)
        {
            base.ApplyConfiguration(bindingElement);
            HttpsTransportBindingElement binding = (HttpsTransportBindingElement)bindingElement;
            binding.RequireClientCertificate = this.RequireClientCertificate;
        }

        public override Type BindingElementType
        {
            get { return typeof(HttpsTransportBindingElement); }
        }

        public override void CopyFrom(ServiceModelExtensionElement from)
        {
            base.CopyFrom(from);

            HttpsTransportElement source = (HttpsTransportElement)from;
#pragma warning suppress 56506 // Microsoft, base.CopyFrom() validates the argument
            this.RequireClientCertificate = source.RequireClientCertificate;
        }

        protected override TransportBindingElement CreateDefaultBindingElement()
        {
            return new HttpsTransportBindingElement();
        }

        protected internal override void InitializeFrom(BindingElement bindingElement)
        {
            base.InitializeFrom(bindingElement);
            HttpsTransportBindingElement binding = (HttpsTransportBindingElement)bindingElement;
            SetPropertyValueIfNotDefaultValue(ConfigurationStrings.RequireClientCertificate, binding.RequireClientCertificate);
        }

        [ConfigurationProperty(ConfigurationStrings.RequireClientCertificate, DefaultValue = TransportDefaults.RequireClientCertificate)]
        public bool RequireClientCertificate
        {
            get { return (bool)base[ConfigurationStrings.RequireClientCertificate]; }
            set { base[ConfigurationStrings.RequireClientCertificate] = value; }
        }

    }
}



