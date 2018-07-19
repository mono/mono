//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel
{
    using System;
    using System.ComponentModel;
    using System.Runtime;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Configuration;
    using System.ServiceModel.Description;
    using System.Xml;

    public sealed class PeerTransportSecuritySettings
    {
        internal const PeerTransportCredentialType DefaultCredentialType = PeerTransportCredentialType.Password;

        PeerTransportCredentialType credentialType;

        public PeerTransportSecuritySettings()
        {
            this.credentialType = DefaultCredentialType;
        }

        internal PeerTransportSecuritySettings(PeerTransportSecuritySettings other)
        {
            this.credentialType = other.credentialType;
        }

        internal PeerTransportSecuritySettings(PeerTransportSecurityElement element)
        {
            credentialType = element.CredentialType;
        }

        public PeerTransportCredentialType CredentialType
        {
            get { return this.credentialType; }
            set
            {
                if (!PeerTransportCredentialTypeHelper.IsDefined(value))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidEnumArgumentException("value", (int)value, typeof(PeerTransportCredentialType)));
                }
                this.credentialType = value;
            }
        }

        internal void OnImportPolicy(MetadataImporter importer, PolicyConversionContext context)
        {
            XmlElement element = PolicyConversionContext.FindAssertion(context.GetBindingAssertions(),
                                    PeerTransportPolicyConstants.PeerTransportCredentialType,
                                    TransportPolicyConstants.PeerTransportUri, true);
            PeerTransportCredentialType credentialType = PeerTransportCredentialType.Password;
            if (element != null)
            {
                switch (element.InnerText)
                {
                    case PeerTransportPolicyConstants.PeerTransportCredentialTypePassword:
                        credentialType = PeerTransportCredentialType.Password;
                        break;
                    case PeerTransportPolicyConstants.PeerTransportCredentialTypeCertificate:
                        credentialType = PeerTransportCredentialType.Certificate;
                        break;
                    default:
                        break;
                }
            }
            this.CredentialType = credentialType;
        }

        internal void OnExportPolicy(MetadataExporter exporter, PolicyConversionContext context)
        {
            string assertion = "";
            switch (this.CredentialType)
            {
                case PeerTransportCredentialType.Password:
                    assertion = PeerTransportPolicyConstants.PeerTransportCredentialTypePassword;
                    break;
                case PeerTransportCredentialType.Certificate:
                    assertion = PeerTransportPolicyConstants.PeerTransportCredentialTypeCertificate;
                    break;
                default:
                    Fx.Assert("Unsupported value for PeerTransportSecuritySettings.CredentialType");
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException());
            }
            XmlDocument doc = new XmlDocument();
            XmlElement element = doc.CreateElement(PeerTransportPolicyConstants.PeerTransportPrefix,
                                                   PeerTransportPolicyConstants.PeerTransportCredentialType,
                                                   TransportPolicyConstants.PeerTransportUri);
            element.InnerText = assertion;
            context.GetBindingAssertions().Add(element);
        }
    }
}
