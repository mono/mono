//------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------

namespace System.ServiceModel.Configuration
{
    using System.Configuration;
    using System.ServiceModel.Channels;

    public sealed partial class SslStreamSecurityElement : BindingElementExtensionElement
    {
        public SslStreamSecurityElement()
        {
        }

        [ConfigurationProperty(
            ConfigurationStrings.RequireClientCertificate, DefaultValue = TransportDefaults.RequireClientCertificate)]
        public bool RequireClientCertificate
        {
            get { return (bool)base[ConfigurationStrings.RequireClientCertificate]; }
            set { base[ConfigurationStrings.RequireClientCertificate] = value; }
        }

        public override void ApplyConfiguration(BindingElement bindingElement)
        {
            base.ApplyConfiguration(bindingElement);
            SslStreamSecurityBindingElement sslBindingElement = 
                (SslStreamSecurityBindingElement)bindingElement;
            sslBindingElement.RequireClientCertificate = this.RequireClientCertificate;
        }

        protected internal override BindingElement CreateBindingElement()
        {
            SslStreamSecurityBindingElement sslBindingElement 
                = new SslStreamSecurityBindingElement();

            this.ApplyConfiguration(sslBindingElement);
            return sslBindingElement;
        }

        public override Type BindingElementType
        {
            get { return typeof(SslStreamSecurityBindingElement); }
        }

        public override void CopyFrom(ServiceModelExtensionElement from)
        {
            base.CopyFrom(from);

            SslStreamSecurityElement source = (SslStreamSecurityElement)from;
#pragma warning suppress 56506 // [....], base.CopyFrom() validates the argument
            this.RequireClientCertificate = source.RequireClientCertificate;
        }

        protected internal override void InitializeFrom(BindingElement bindingElement)
        {
            base.InitializeFrom(bindingElement);
            SslStreamSecurityBindingElement sslBindingElement 
                = (SslStreamSecurityBindingElement)bindingElement;
            SetPropertyValueIfNotDefaultValue(ConfigurationStrings.RequireClientCertificate, sslBindingElement.RequireClientCertificate);
        }
    }
}



