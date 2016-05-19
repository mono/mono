//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel
{
    using System;
    using System.Text;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Configuration;
    using System.Globalization;
    using System.Net;
    using System.Net.Security;
    using System.Runtime.Serialization;
    using System.Security.Principal;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Configuration;
    using System.ServiceModel.Security;

    using System.Xml;
    using System.ComponentModel;

    public class WSFederationHttpBinding : WSHttpBindingBase
    {
        static readonly MessageSecurityVersion WSMessageSecurityVersion = MessageSecurityVersion.WSSecurity11WSTrustFebruary2005WSSecureConversationFebruary2005WSSecurityPolicy11BasicSecurityProfile10;

        Uri privacyNoticeAt;
        int privacyNoticeVersion;
        WSFederationHttpSecurity security = new WSFederationHttpSecurity();

        public WSFederationHttpBinding(string configName)
            : this()
        {
            ApplyConfiguration(configName);
        }

        public WSFederationHttpBinding()
            : base()
        {
        }

        public WSFederationHttpBinding(WSFederationHttpSecurityMode securityMode)
            : this(securityMode, false)
        {
        }

        public WSFederationHttpBinding(WSFederationHttpSecurityMode securityMode, bool reliableSessionEnabled)
            : base(reliableSessionEnabled)
        {
            security.Mode = securityMode;
        }


        internal WSFederationHttpBinding(WSFederationHttpSecurity security, PrivacyNoticeBindingElement privacy, bool reliableSessionEnabled)
            : base(reliableSessionEnabled)
        {
            this.security = security;
            if (null != privacy)
            {
                this.privacyNoticeAt = privacy.Url;
                this.privacyNoticeVersion = privacy.Version;
            }
        }

        [DefaultValue(null)]
        public Uri PrivacyNoticeAt
        {
            get { return this.privacyNoticeAt; }
            set { this.privacyNoticeAt = value; }
        }

        [DefaultValue(0)]
        public int PrivacyNoticeVersion
        {
            get { return this.privacyNoticeVersion; }
            set { this.privacyNoticeVersion = value; }
        }

        public WSFederationHttpSecurity Security
        {
            get { return this.security; }
            set
            {
                if (value == null)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");
                this.security = value;
            }
        }

        void ApplyConfiguration(string configurationName)
        {
            WSFederationHttpBindingCollectionElement section = WSFederationHttpBindingCollectionElement.GetBindingCollectionElement();
            WSFederationHttpBindingElement element = section.Bindings[configurationName];
            if (element == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ConfigurationErrorsException(
                    SR.GetString(SR.ConfigInvalidBindingConfigurationName,
                                 configurationName,
                                 ConfigurationStrings.WSFederationHttpBindingCollectionElementName)));
            }
            else
            {
                element.ApplyConfiguration(this);
            }
        }

        PrivacyNoticeBindingElement CreatePrivacyPolicy()
        {
            PrivacyNoticeBindingElement privacy = null;

            if (this.PrivacyNoticeAt != null)
            {
                privacy = new PrivacyNoticeBindingElement();
                privacy.Url = this.PrivacyNoticeAt;
                privacy.Version = this.privacyNoticeVersion;
            }

            return privacy;
        }

        // if you make changes here, see also WS2007FederationHttpBinding.TryCreate()
        internal static bool TryCreate(SecurityBindingElement sbe, TransportBindingElement transport, PrivacyNoticeBindingElement privacy, ReliableSessionBindingElement rsbe, TransactionFlowBindingElement tfbe, out Binding binding)
        {
            bool isReliableSession = (rsbe != null);
            binding = null;

            // reverse GetTransport
            HttpTransportSecurity transportSecurity = new HttpTransportSecurity();
            WSFederationHttpSecurityMode mode;
            if (!GetSecurityModeFromTransport(transport, transportSecurity, out mode))
            {
                return false;
            }

            HttpsTransportBindingElement httpsBinding = transport as HttpsTransportBindingElement;
            if (httpsBinding != null && httpsBinding.MessageSecurityVersion != null)
            {
                if (httpsBinding.MessageSecurityVersion.SecurityPolicyVersion != WSMessageSecurityVersion.SecurityPolicyVersion)
                {
                    return false;
                }
            }

            WSFederationHttpSecurity security;
            if (TryCreateSecurity(sbe, mode, transportSecurity, isReliableSession, out security))
            {
                binding = new WSFederationHttpBinding(security, privacy, isReliableSession);
            }

            if (rsbe != null && rsbe.ReliableMessagingVersion != ReliableMessagingVersion.WSReliableMessagingFebruary2005)
            {
                return false;
            }

            if (tfbe != null && tfbe.TransactionProtocol != TransactionProtocol.WSAtomicTransactionOctober2004)
            {
                return false;
            }

            return binding != null;
        }

        protected override TransportBindingElement GetTransport()
        {
            if (security.Mode == WSFederationHttpSecurityMode.None || security.Mode == WSFederationHttpSecurityMode.Message)
            {
                return this.HttpTransport;
            }
            else
            {
                return this.HttpsTransport;
            }
        }

        internal static bool GetSecurityModeFromTransport(TransportBindingElement transport, HttpTransportSecurity transportSecurity, out WSFederationHttpSecurityMode mode)
        {
            mode = WSFederationHttpSecurityMode.None | WSFederationHttpSecurityMode.Message | WSFederationHttpSecurityMode.TransportWithMessageCredential;
            if (transport is HttpsTransportBindingElement)
            {
                mode = WSFederationHttpSecurityMode.TransportWithMessageCredential;
            }
            else if (transport is HttpTransportBindingElement)
            {
                mode = WSFederationHttpSecurityMode.None | WSFederationHttpSecurityMode.Message;
            }
            else
            {
                return false;
            }
            return true;
        }

        protected override SecurityBindingElement CreateMessageSecurity()
        {
            return security.CreateMessageSecurity(this.ReliableSession.Enabled, WSMessageSecurityVersion);
        }

        // if you make changes here, see also WS2007FederationHttpBinding.TryCreateSecurity()
        static bool TryCreateSecurity(SecurityBindingElement sbe, WSFederationHttpSecurityMode mode, HttpTransportSecurity transportSecurity, bool isReliableSession, out WSFederationHttpSecurity security)
        {
            if (!WSFederationHttpSecurity.TryCreate(sbe, mode, transportSecurity, isReliableSession, WSMessageSecurityVersion, out security))
                return false;
            // the last check: make sure that security binding element match the incoming security
            return SecurityElement.AreBindingsMatching(security.CreateMessageSecurity(isReliableSession, WSMessageSecurityVersion), sbe);
        }

        public override BindingElementCollection CreateBindingElements()
        {   // return collection of BindingElements

            BindingElementCollection bindingElements = base.CreateBindingElements();
            // order of BindingElements is important

            PrivacyNoticeBindingElement privacy = this.CreatePrivacyPolicy();
            if (privacy != null)
            {
                // This must go first.
                bindingElements.Insert(0, privacy);
            }

            return bindingElements;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool ShouldSerializeSecurity()
        {
            return this.Security.InternalShouldSerialize();
        }
    }
}
