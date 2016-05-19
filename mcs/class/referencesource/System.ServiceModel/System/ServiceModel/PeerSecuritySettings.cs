//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------
namespace System.ServiceModel
{
    using System.Runtime;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Configuration;
    using System.ServiceModel.Description;
    using System.Xml;
    using System.ComponentModel;

    public sealed class PeerSecuritySettings
    {
        internal const SecurityMode DefaultMode = SecurityMode.Transport;

        SecurityMode mode;
        PeerTransportSecuritySettings transportSecurity;

        public PeerSecuritySettings()
        {
            mode = DefaultMode;
            transportSecurity = new PeerTransportSecuritySettings();
        }

        internal PeerSecuritySettings(PeerSecuritySettings other)
        {
            this.mode = other.mode;
            this.transportSecurity = new PeerTransportSecuritySettings(other.transportSecurity);
        }

        internal PeerSecuritySettings(PeerSecurityElement element)
        {
            mode = element.Mode;
            transportSecurity = new PeerTransportSecuritySettings(element.Transport);
        }

        public SecurityMode Mode
        {
            get { return this.mode; }
            set
            {
                if (!SecurityModeHelper.IsDefined(value))
                {
                    PeerExceptionHelper.ThrowArgumentOutOfRange_InvalidSecurityMode((int)value);
                }
                this.mode = value;
            }
        }

        public PeerTransportSecuritySettings Transport
        {
            get { return this.transportSecurity; }
            set { this.transportSecurity = value; }
        }

        internal bool SupportsAuthentication
        {
            get
            {
                return this.Mode == SecurityMode.Transport || this.Mode == SecurityMode.TransportWithMessageCredential;
            }
        }

        internal System.Net.Security.ProtectionLevel SupportedProtectionLevel
        {
            get
            {
                System.Net.Security.ProtectionLevel level = System.Net.Security.ProtectionLevel.None;
                if (this.Mode == SecurityMode.Message || this.Mode == SecurityMode.TransportWithMessageCredential)
                {
                    level = System.Net.Security.ProtectionLevel.Sign;
                }
                return level;
            }
        }


        internal void OnImportPolicy(MetadataImporter importer, PolicyConversionContext context)
        {
            XmlElement element = PolicyConversionContext.FindAssertion(context.GetBindingAssertions(),
                                    PeerTransportPolicyConstants.PeerTransportSecurityMode,
                                    TransportPolicyConstants.PeerTransportUri, true);

            this.Mode = SecurityMode.Transport;
            if (element != null)
            {
                switch (element.InnerText)
                {
                    case PeerTransportPolicyConstants.PeerTransportSecurityModeNone:
                        this.Mode = SecurityMode.None;
                        break;
                    case PeerTransportPolicyConstants.PeerTransportSecurityModeTransport:
                        this.Mode = SecurityMode.Transport;
                        break;
                    case PeerTransportPolicyConstants.PeerTransportSecurityModeMessage:
                        this.Mode = SecurityMode.Message;
                        break;
                    case PeerTransportPolicyConstants.PeerTransportSecurityModeTransportWithMessageCredential:
                        this.Mode = SecurityMode.TransportWithMessageCredential;
                        break;
                    default:
                        break;
                }
            }
            transportSecurity.OnImportPolicy(importer, context);
        }

        internal void OnExportPolicy(MetadataExporter exporter, PolicyConversionContext context)
        {
            string assertion = "";
            switch (this.Mode)
            {
                case SecurityMode.None:
                    assertion = PeerTransportPolicyConstants.PeerTransportSecurityModeNone;
                    break;
                case SecurityMode.Transport:
                    assertion = PeerTransportPolicyConstants.PeerTransportSecurityModeTransport;
                    break;
                case SecurityMode.Message:
                    assertion = PeerTransportPolicyConstants.PeerTransportSecurityModeMessage;
                    break;
                case SecurityMode.TransportWithMessageCredential:
                    assertion = PeerTransportPolicyConstants.PeerTransportSecurityModeTransportWithMessageCredential;
                    break;
                default:
                    Fx.Assert("Unsupported value for PeerSecuritySettings.Mode");
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException());
            }
            XmlDocument doc = new XmlDocument();
            XmlElement element = doc.CreateElement(PeerTransportPolicyConstants.PeerTransportPrefix,
                                                   PeerTransportPolicyConstants.PeerTransportSecurityMode,
                                                   TransportPolicyConstants.PeerTransportUri);
            element.InnerText = assertion;
            context.GetBindingAssertions().Add(element);
            transportSecurity.OnExportPolicy(exporter, context);
        }

        internal bool InternalShouldSerialize()
        {
            return this.ShouldSerializeMode()
                || this.ShouldSerializeTransport();
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool ShouldSerializeMode()
        {
            return this.Mode != DefaultMode;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool ShouldSerializeTransport()
        {
            return this.Transport.CredentialType != PeerTransportSecuritySettings.DefaultCredentialType;
        }
    }
}

