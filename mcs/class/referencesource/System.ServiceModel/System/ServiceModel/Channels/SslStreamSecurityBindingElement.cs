//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------
namespace System.ServiceModel.Channels
{
    using System.Security.Authentication;
    using System.ComponentModel;
    using System.Collections.Generic;
    using System.Net.Security;
    using System.ServiceModel.Description;
    using System.ServiceModel;
    using System.ServiceModel.Security;
    using System.ServiceModel.Security.Tokens;
    using System.Xml;
    
    public class SslStreamSecurityBindingElement : StreamUpgradeBindingElement, ITransportTokenAssertionProvider, IPolicyExportExtension
    {
        IdentityVerifier identityVerifier;
        bool requireClientCertificate;
        SslProtocols sslProtocols;

        public SslStreamSecurityBindingElement()
        {
            this.requireClientCertificate = TransportDefaults.RequireClientCertificate;
            this.sslProtocols = TransportDefaults.SslProtocols;
        }

        protected SslStreamSecurityBindingElement(SslStreamSecurityBindingElement elementToBeCloned)
            : base(elementToBeCloned)
        {
            this.identityVerifier = elementToBeCloned.identityVerifier;
            this.requireClientCertificate = elementToBeCloned.requireClientCertificate;
            this.sslProtocols = elementToBeCloned.sslProtocols;
        }

        public IdentityVerifier IdentityVerifier
        {
            get
            {
                if (this.identityVerifier == null)
                {
                    this.identityVerifier = IdentityVerifier.CreateDefault();
                }

                return this.identityVerifier;
            }
            set
            {
                if (value == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");
                }

                this.identityVerifier = value;
            }
        }

        [DefaultValue(TransportDefaults.RequireClientCertificate)]
        public bool RequireClientCertificate
        {
            get
            {
                return this.requireClientCertificate;
            }
            set
            {
                this.requireClientCertificate = value;
            }
        }

        [DefaultValue(TransportDefaults.SslProtocols)]
        public SslProtocols SslProtocols
        {
            get
            {
                return this.sslProtocols;
            }
            set
            {
                SslProtocolsHelper.Validate(value);
                this.sslProtocols = value;
            }
        }

        public override IChannelFactory<TChannel> BuildChannelFactory<TChannel>(BindingContext context)
        {
            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");
            }

#pragma warning suppress 56506 // Microsoft, BindingContext.BindingParameters cannot be null
            context.BindingParameters.Add(this);
            return context.BuildInnerChannelFactory<TChannel>();
        }

        public override bool CanBuildChannelFactory<TChannel>(BindingContext context)
        {
            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");
            }

#pragma warning suppress 56506 // Microsoft, BindingContext.BindingParameters cannot be null
            context.BindingParameters.Add(this);
            return context.CanBuildInnerChannelFactory<TChannel>();
        }

        public override IChannelListener<TChannel> BuildChannelListener<TChannel>(BindingContext context)
        {
            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");
            }

#pragma warning suppress 56506 // Microsoft, BindingContext.BindingParameters cannot be null
            context.BindingParameters.Add(this);
            return context.BuildInnerChannelListener<TChannel>();
        }

        public override bool CanBuildChannelListener<TChannel>(BindingContext context)
        {
            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");
            }

#pragma warning suppress 56506 // Microsoft, BindingContext.BindingParameters cannot be null
            context.BindingParameters.Add(this);
            return context.CanBuildInnerChannelListener<TChannel>();
        }

        public override BindingElement Clone()
        {
            return new SslStreamSecurityBindingElement(this);
        }

        public override T GetProperty<T>(BindingContext context)
        {
            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");
            }
            if (typeof(T) == typeof(ISecurityCapabilities))
            {
                return (T)(object)new SecurityCapabilities(this.RequireClientCertificate, true, this.RequireClientCertificate,
                    ProtectionLevel.EncryptAndSign, ProtectionLevel.EncryptAndSign);
            }
            else if (typeof(T) == typeof(IdentityVerifier))
            {
                return (T)(object)this.IdentityVerifier;
            }
            else
            {
                return context.GetInnerProperty<T>();
            }
        }

        public override StreamUpgradeProvider BuildClientStreamUpgradeProvider(BindingContext context)
        {
            return SslStreamSecurityUpgradeProvider.CreateClientProvider(this, context);
        }

        public override StreamUpgradeProvider BuildServerStreamUpgradeProvider(BindingContext context)
        {
            return SslStreamSecurityUpgradeProvider.CreateServerProvider(this, context);
        }


        internal static void ImportPolicy(MetadataImporter importer, PolicyConversionContext policyContext)
        {
            XmlElement assertion = PolicyConversionContext.FindAssertion(policyContext.GetBindingAssertions(),
                TransportPolicyConstants.SslTransportSecurityName, TransportPolicyConstants.DotNetFramingNamespace, true);

            if (assertion != null)
            {
                SslStreamSecurityBindingElement sslBindingElement = new SslStreamSecurityBindingElement();

                XmlReader reader = new XmlNodeReader(assertion);
                reader.ReadStartElement();
                sslBindingElement.RequireClientCertificate = reader.IsStartElement(
                    TransportPolicyConstants.RequireClientCertificateName,
                    TransportPolicyConstants.DotNetFramingNamespace);
                if (sslBindingElement.RequireClientCertificate)
                {
                    reader.ReadElementString();
                }

                policyContext.BindingElements.Add(sslBindingElement);
            }
        }

        #region ITransportTokenAssertionProvider Members

        public XmlElement GetTransportTokenAssertion()
        {
            XmlDocument document = new XmlDocument();
            XmlElement assertion =
                document.CreateElement(TransportPolicyConstants.DotNetFramingPrefix,
                TransportPolicyConstants.SslTransportSecurityName,
                TransportPolicyConstants.DotNetFramingNamespace);
            if (this.requireClientCertificate)
            {
                assertion.AppendChild(document.CreateElement(TransportPolicyConstants.DotNetFramingPrefix,
                    TransportPolicyConstants.RequireClientCertificateName,
                    TransportPolicyConstants.DotNetFramingNamespace));
            }
            return assertion;
        }

        #endregion

        void IPolicyExportExtension.ExportPolicy(MetadataExporter exporter, PolicyConversionContext context)
        {
            if (exporter == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("exporter");
            }
            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");
            }

            SecurityBindingElement.ExportPolicyForTransportTokenAssertionProviders(exporter, context);
        }

        internal override bool IsMatch(BindingElement b)
        {
            if (b == null)
            {
                return false;
            }
            SslStreamSecurityBindingElement ssl = b as SslStreamSecurityBindingElement;
            if (ssl == null)
            {
                return false;
            }

            return this.requireClientCertificate == ssl.requireClientCertificate && this.sslProtocols == ssl.sslProtocols;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool ShouldSerializeIdentityVerifier()
        {
            // IdentifyVerifier.CreateDefault() grabs the static instance of nested DefaultIdentityVerifier. 
            // DefaultIdentityVerifier can't be serialized directly because it's nested. 
            return (!object.ReferenceEquals(this.IdentityVerifier, IdentityVerifier.CreateDefault()));  
        }
    }
}
