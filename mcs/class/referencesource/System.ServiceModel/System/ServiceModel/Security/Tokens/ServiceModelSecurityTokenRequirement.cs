//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Security.Tokens
{
    using System;
    using System.Text;
    using System.ServiceModel.Channels;
    using System.ServiceModel;
    using System.ServiceModel.Description;
    using System.Xml;
    using System.ServiceModel.Security;
    using System.Collections.Generic;
    using System.IdentityModel.Tokens;
    using System.IdentityModel.Selectors;
    using System.Globalization;

    public abstract class ServiceModelSecurityTokenRequirement : SecurityTokenRequirement
    {
        protected const string Namespace = "http://schemas.microsoft.com/ws/2006/05/servicemodel/securitytokenrequirement";
        const string securityAlgorithmSuiteProperty = Namespace + "/SecurityAlgorithmSuite";
        const string securityBindingElementProperty = Namespace + "/SecurityBindingElement";
        const string issuerAddressProperty = Namespace + "/IssuerAddress";
        const string issuerBindingProperty = Namespace + "/IssuerBinding";
        const string secureConversationSecurityBindingElementProperty = Namespace + "/SecureConversationSecurityBindingElement";
        const string supportSecurityContextCancellationProperty = Namespace + "/SupportSecurityContextCancellation";
        const string messageSecurityVersionProperty = Namespace + "/MessageSecurityVersion";
        const string defaultMessageSecurityVersionProperty = Namespace + "/DefaultMessageSecurityVersion";
        const string issuerBindingContextProperty = Namespace + "/IssuerBindingContext";
        const string transportSchemeProperty = Namespace + "/TransportScheme";
        const string isInitiatorProperty = Namespace + "/IsInitiator";
        const string targetAddressProperty = Namespace + "/TargetAddress";
        const string viaProperty = Namespace + "/Via";
        const string listenUriProperty = Namespace + "/ListenUri";
        const string auditLogLocationProperty = Namespace + "/AuditLogLocation";
        const string suppressAuditFailureProperty = Namespace + "/SuppressAuditFailure";
        const string messageAuthenticationAuditLevelProperty = Namespace + "/MessageAuthenticationAuditLevel";
        const string isOutOfBandTokenProperty = Namespace + "/IsOutOfBandToken";
        const string preferSslCertificateAuthenticatorProperty = Namespace + "/PreferSslCertificateAuthenticator";

        // the following properties dont have top level OM properties but are part of the property bag
        const string supportingTokenAttachmentModeProperty = Namespace + "/SupportingTokenAttachmentMode";
        const string messageDirectionProperty = Namespace + "/MessageDirection";
        const string httpAuthenticationSchemeProperty = Namespace + "/HttpAuthenticationScheme";
        const string issuedSecurityTokenParametersProperty = Namespace + "/IssuedSecurityTokenParameters";
        const string privacyNoticeUriProperty = Namespace + "/PrivacyNoticeUri";
        const string privacyNoticeVersionProperty = Namespace + "/PrivacyNoticeVersion";
        const string duplexClientLocalAddressProperty = Namespace + "/DuplexClientLocalAddress";
        const string endpointFilterTableProperty = Namespace + "/EndpointFilterTable";
        const string channelParametersCollectionProperty = Namespace + "/ChannelParametersCollection";
        const string extendedProtectionPolicy = Namespace + "/ExtendedProtectionPolicy";

        const bool defaultSupportSecurityContextCancellation = false;

        protected ServiceModelSecurityTokenRequirement()
            : base()
        {
            this.Properties[SupportSecurityContextCancellationProperty] = defaultSupportSecurityContextCancellation;
        }

        static public string SecurityAlgorithmSuiteProperty { get { return securityAlgorithmSuiteProperty; } }
        static public string SecurityBindingElementProperty { get { return securityBindingElementProperty; } }
        static public string IssuerAddressProperty { get { return issuerAddressProperty; } }
        static public string IssuerBindingProperty { get { return issuerBindingProperty; } }
        static public string SecureConversationSecurityBindingElementProperty { get { return secureConversationSecurityBindingElementProperty; } }
        static public string SupportSecurityContextCancellationProperty { get { return supportSecurityContextCancellationProperty; } }
        static public string MessageSecurityVersionProperty { get { return messageSecurityVersionProperty; } }
        static internal string DefaultMessageSecurityVersionProperty { get { return defaultMessageSecurityVersionProperty; } }
        static public string IssuerBindingContextProperty { get { return issuerBindingContextProperty; } }
        static public string TransportSchemeProperty { get { return transportSchemeProperty; } }
        static public string IsInitiatorProperty { get { return isInitiatorProperty; } }
        static public string TargetAddressProperty { get { return targetAddressProperty; } }
        static public string ViaProperty { get { return viaProperty; } }
        static public string ListenUriProperty { get { return listenUriProperty; } }
        static public string AuditLogLocationProperty { get { return auditLogLocationProperty; } }
        static public string SuppressAuditFailureProperty { get { return suppressAuditFailureProperty; } }
        static public string MessageAuthenticationAuditLevelProperty { get { return messageAuthenticationAuditLevelProperty; } }
        static public string IsOutOfBandTokenProperty { get { return isOutOfBandTokenProperty; } }
        static public string PreferSslCertificateAuthenticatorProperty { get { return preferSslCertificateAuthenticatorProperty; } }

        static public string SupportingTokenAttachmentModeProperty { get { return supportingTokenAttachmentModeProperty; } }
        static public string MessageDirectionProperty { get { return messageDirectionProperty; } }
        static public string HttpAuthenticationSchemeProperty { get { return httpAuthenticationSchemeProperty; } }
        static public string IssuedSecurityTokenParametersProperty { get { return issuedSecurityTokenParametersProperty; } }
        static public string PrivacyNoticeUriProperty { get { return privacyNoticeUriProperty; } }
        static public string PrivacyNoticeVersionProperty { get { return privacyNoticeVersionProperty; } }
        static public string DuplexClientLocalAddressProperty { get { return duplexClientLocalAddressProperty; } }
        static public string EndpointFilterTableProperty { get { return endpointFilterTableProperty; } }
        static public string ChannelParametersCollectionProperty { get { return channelParametersCollectionProperty; } }
        static public string ExtendedProtectionPolicy { get { return extendedProtectionPolicy; } }

        public bool IsInitiator
        {
            get
            {
                return GetPropertyOrDefault<bool>(IsInitiatorProperty, false);
            }
        }

        public SecurityAlgorithmSuite SecurityAlgorithmSuite
        {
            get
            {
                return GetPropertyOrDefault<SecurityAlgorithmSuite>(SecurityAlgorithmSuiteProperty, null);
            }
            set
            {
                this.Properties[SecurityAlgorithmSuiteProperty] = value;
            }
        }

        public SecurityBindingElement SecurityBindingElement
        {
            get
            {
                return GetPropertyOrDefault<SecurityBindingElement>(SecurityBindingElementProperty, null);
            }
            set
            {
                this.Properties[SecurityBindingElementProperty] = value;
            }
        }

        public EndpointAddress IssuerAddress
        {
            get
            {
                return GetPropertyOrDefault<EndpointAddress>(IssuerAddressProperty, null);
            }
            set
            {
                this.Properties[IssuerAddressProperty] = value;
            }
        }

        public Binding IssuerBinding
        {
            get
            {
                return GetPropertyOrDefault<Binding>(IssuerBindingProperty, null);
            }
            set
            {
                this.Properties[IssuerBindingProperty] = value;
            }
        }

        public SecurityBindingElement SecureConversationSecurityBindingElement
        {
            get
            {
                return GetPropertyOrDefault<SecurityBindingElement>(SecureConversationSecurityBindingElementProperty, null);
            }
            set
            {
                this.Properties[SecureConversationSecurityBindingElementProperty] = value;
            }
        }

        public SecurityTokenVersion MessageSecurityVersion
        {
            get
            {
                return GetPropertyOrDefault<SecurityTokenVersion>(MessageSecurityVersionProperty, null);
            }
            set
            {
                this.Properties[MessageSecurityVersionProperty] = value;
            }
        }

        internal MessageSecurityVersion DefaultMessageSecurityVersion
        {
            get
            {
                MessageSecurityVersion messageSecurityVersion;
                return (this.TryGetProperty<MessageSecurityVersion>(DefaultMessageSecurityVersionProperty, out messageSecurityVersion)) ? messageSecurityVersion : null;
            }
            set
            {
                this.Properties[DefaultMessageSecurityVersionProperty] = (object)value;
            }
        }

        public string TransportScheme
        {
            get
            {
                return GetPropertyOrDefault<string>(TransportSchemeProperty, null);
            }
            set
            {
                this.Properties[TransportSchemeProperty] = value;
            }
        }

        internal bool SupportSecurityContextCancellation
        {
            get
            {
                return GetPropertyOrDefault<bool>(SupportSecurityContextCancellationProperty, defaultSupportSecurityContextCancellation);
            }
            set
            {
                this.Properties[SupportSecurityContextCancellationProperty] = value;
            }
        }

        internal EndpointAddress DuplexClientLocalAddress
        {
            get
            {
                return GetPropertyOrDefault<EndpointAddress>(duplexClientLocalAddressProperty, null);
            }
            set
            {
                this.Properties[duplexClientLocalAddressProperty] = value;
            }
        }

        internal TValue GetPropertyOrDefault<TValue>(string propertyName, TValue defaultValue)
        {
            TValue result;
            if (!TryGetProperty<TValue>(propertyName, out result))
            {
                result = defaultValue;
            }
            return result;
        }

        internal string InternalToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine(String.Format(CultureInfo.InvariantCulture, "{0}:", this.GetType().ToString()));
            foreach (string propertyName in this.Properties.Keys)
            {
                object propertyValue = this.Properties[propertyName];
                sb.AppendLine(String.Format(CultureInfo.InvariantCulture, "PropertyName: {0}", propertyName));
                sb.AppendLine(String.Format(CultureInfo.InvariantCulture, "PropertyValue: {0}", propertyValue));
                sb.AppendLine(String.Format(CultureInfo.InvariantCulture, "---"));
            }
            return sb.ToString().Trim();
        }
    }
}
