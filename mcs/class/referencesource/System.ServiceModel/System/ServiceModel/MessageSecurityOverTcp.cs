//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel
{
    using System;
    using System.ComponentModel;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Security;
    using System.ServiceModel.Security.Tokens;

    public sealed class MessageSecurityOverTcp
    {
        internal const MessageCredentialType DefaultClientCredentialType = MessageCredentialType.Windows;

        MessageCredentialType clientCredentialType;
        SecurityAlgorithmSuite algorithmSuite;
        bool wasAlgorithmSuiteSet;

        public MessageSecurityOverTcp()
        {
            clientCredentialType = DefaultClientCredentialType;
            algorithmSuite = SecurityAlgorithmSuite.Default;
        }

        [DefaultValue(MessageSecurityOverTcp.DefaultClientCredentialType)]
        public MessageCredentialType ClientCredentialType
        {
            get { return this.clientCredentialType; }
            set
            {
                if (!MessageCredentialTypeHelper.IsDefined(value))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value"));
                }
                this.clientCredentialType = value;
            }
        }
        [DefaultValue(typeof(SecurityAlgorithmSuite), System.ServiceModel.Configuration.ConfigurationStrings.Default)]
        public SecurityAlgorithmSuite AlgorithmSuite
        {
            get { return this.algorithmSuite; }
            set
            {
                if (value == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");
                }
                this.algorithmSuite = value;
                wasAlgorithmSuiteSet = true;
            }
        }

        internal bool WasAlgorithmSuiteSet
        {
            get { return this.wasAlgorithmSuiteSet; }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal SecurityBindingElement CreateSecurityBindingElement(bool isSecureTransportMode, bool isReliableSession, BindingElement transportBindingElement)
        {
            SecurityBindingElement result;
            SecurityBindingElement oneShotSecurity;
            if (isSecureTransportMode)
            {
                switch (this.clientCredentialType)
                {
                    case MessageCredentialType.None:
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.ClientCredentialTypeMustBeSpecifiedForMixedMode)));
                    case MessageCredentialType.UserName:
                        oneShotSecurity = SecurityBindingElement.CreateUserNameOverTransportBindingElement();
                        break;
                    case MessageCredentialType.Certificate:
                        oneShotSecurity = SecurityBindingElement.CreateCertificateOverTransportBindingElement();
                        break;
                    case MessageCredentialType.Windows:
                        oneShotSecurity = SecurityBindingElement.CreateSspiNegotiationOverTransportBindingElement(true);
                        break;
                    case MessageCredentialType.IssuedToken:
                        oneShotSecurity = SecurityBindingElement.CreateIssuedTokenOverTransportBindingElement(IssuedSecurityTokenParameters.CreateInfoCardParameters(new SecurityStandardsManager(), this.algorithmSuite));
                        break;
                    default:
                        Fx.Assert("unknown ClientCredentialType");
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException());
                }
                result = SecurityBindingElement.CreateSecureConversationBindingElement(oneShotSecurity);
            }
            else
            {
                switch (this.clientCredentialType)
                {
                    case MessageCredentialType.None:
                        oneShotSecurity = SecurityBindingElement.CreateSslNegotiationBindingElement(false, true);
                        break;
                    case MessageCredentialType.UserName:
                        // require cancellation so that impersonation is possible
                        oneShotSecurity = SecurityBindingElement.CreateUserNameForSslBindingElement(true);
                        break;
                    case MessageCredentialType.Certificate:
                        oneShotSecurity = SecurityBindingElement.CreateSslNegotiationBindingElement(true, true);
                        break;
                    case MessageCredentialType.Windows:
                        // require cancellation so that impersonation is possible
                        oneShotSecurity = SecurityBindingElement.CreateSspiNegotiationBindingElement(true);
                        break;
                    case MessageCredentialType.IssuedToken:
                        oneShotSecurity = SecurityBindingElement.CreateIssuedTokenForSslBindingElement(IssuedSecurityTokenParameters.CreateInfoCardParameters(new SecurityStandardsManager(), this.algorithmSuite), true);
                        break;
                    default:
                        Fx.Assert("unknown ClientCredentialType");
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException());
                }
                result = SecurityBindingElement.CreateSecureConversationBindingElement(oneShotSecurity, true);
            }

            // set the algorithm suite and issued token params if required
            result.DefaultAlgorithmSuite = oneShotSecurity.DefaultAlgorithmSuite = this.AlgorithmSuite;

            result.IncludeTimestamp = true;
            if (!isReliableSession)
            {
                result.LocalServiceSettings.ReconnectTransportOnFailure = false;
                result.LocalClientSettings.ReconnectTransportOnFailure = false;
            }
            else
            {
                result.LocalServiceSettings.ReconnectTransportOnFailure = true;
                result.LocalClientSettings.ReconnectTransportOnFailure = true;
            }

            // since a session is always bootstrapped, configure the transition sct to live for a short time only
            oneShotSecurity.LocalServiceSettings.IssuedCookieLifetime = SpnegoTokenAuthenticator.defaultServerIssuedTransitionTokenLifetime;
            result.MessageSecurityVersion = MessageSecurityVersion.WSSecurity11WSTrustFebruary2005WSSecureConversationFebruary2005WSSecurityPolicy11;
            oneShotSecurity.MessageSecurityVersion = MessageSecurityVersion.WSSecurity11WSTrustFebruary2005WSSecureConversationFebruary2005WSSecurityPolicy11;

            return result;
        }

        internal static bool TryCreate(SecurityBindingElement sbe, bool isReliableSession, BindingElement transportBindingElement, out MessageSecurityOverTcp messageSecurity)
        {
            messageSecurity = null;
            if (sbe == null)
                return false;

            // do not check local settings: sbe.LocalServiceSettings and sbe.LocalClientSettings

            if (!sbe.IncludeTimestamp)
                return false;

            if (sbe.MessageSecurityVersion != MessageSecurityVersion.WSSecurity11WSTrustFebruary2005WSSecureConversationFebruary2005WSSecurityPolicy11
                && sbe.MessageSecurityVersion != MessageSecurityVersion.WSSecurity11WSTrustFebruary2005WSSecureConversationFebruary2005WSSecurityPolicy11BasicSecurityProfile10)
            {
                return false;
            }

            if (sbe.SecurityHeaderLayout != SecurityProtocolFactory.defaultSecurityHeaderLayout)
                return false;

            MessageCredentialType clientCredentialType;

            SecurityBindingElement bootstrapSecurity;

            if (!SecurityBindingElement.IsSecureConversationBinding(sbe, true, out bootstrapSecurity))
                return false;

            bool isSecureTransportMode = bootstrapSecurity is TransportSecurityBindingElement;

            IssuedSecurityTokenParameters infocardParameters;
            if (isSecureTransportMode)
            {
                if (SecurityBindingElement.IsUserNameOverTransportBinding(bootstrapSecurity))
                    clientCredentialType = MessageCredentialType.UserName;
                else if (SecurityBindingElement.IsCertificateOverTransportBinding(bootstrapSecurity))
                    clientCredentialType = MessageCredentialType.Certificate;
                else if (SecurityBindingElement.IsSspiNegotiationOverTransportBinding(bootstrapSecurity, true))
                    clientCredentialType = MessageCredentialType.Windows;
                else if (SecurityBindingElement.IsIssuedTokenOverTransportBinding(bootstrapSecurity, out infocardParameters))
                {
                    if (!IssuedSecurityTokenParameters.IsInfoCardParameters(
                            infocardParameters,
                            new SecurityStandardsManager(
                                bootstrapSecurity.MessageSecurityVersion,
                                new WSSecurityTokenSerializer(
                                    bootstrapSecurity.MessageSecurityVersion.SecurityVersion,
                                    bootstrapSecurity.MessageSecurityVersion.TrustVersion,
                                    bootstrapSecurity.MessageSecurityVersion.SecureConversationVersion,
                                    true,
                                    null, null, null))))
                        return false;
                    clientCredentialType = MessageCredentialType.IssuedToken;
                }
                else
                {
                    // the standard binding does not support None client credential type in mixed mode
                    return false;
                }
            }
            else
            {
                if (SecurityBindingElement.IsUserNameForSslBinding(bootstrapSecurity, true))
                    clientCredentialType = MessageCredentialType.UserName;
                else if (SecurityBindingElement.IsSslNegotiationBinding(bootstrapSecurity, true, true))
                    clientCredentialType = MessageCredentialType.Certificate;
                else if (SecurityBindingElement.IsSspiNegotiationBinding(bootstrapSecurity, true))
                    clientCredentialType = MessageCredentialType.Windows;
                else if (SecurityBindingElement.IsIssuedTokenForSslBinding(bootstrapSecurity, true, out infocardParameters))
                {
                    if (!IssuedSecurityTokenParameters.IsInfoCardParameters(
                            infocardParameters,
                            new SecurityStandardsManager(
                                bootstrapSecurity.MessageSecurityVersion,
                                new WSSecurityTokenSerializer(
                                    bootstrapSecurity.MessageSecurityVersion.SecurityVersion,
                                    bootstrapSecurity.MessageSecurityVersion.TrustVersion,
                                    bootstrapSecurity.MessageSecurityVersion.SecureConversationVersion,
                                    true,
                                    null, null, null))))
                        return false;
                    clientCredentialType = MessageCredentialType.IssuedToken;
                }
                else if (SecurityBindingElement.IsSslNegotiationBinding(bootstrapSecurity, false, true))
                    clientCredentialType = MessageCredentialType.None;
                else
                    return false;
            }
            messageSecurity = new MessageSecurityOverTcp();
            messageSecurity.ClientCredentialType = clientCredentialType;
            // set the algorithm suite and issued token params if required
            if (clientCredentialType != MessageCredentialType.IssuedToken)
            {
                messageSecurity.AlgorithmSuite = bootstrapSecurity.DefaultAlgorithmSuite;
            }
            return true;
        }

        internal bool InternalShouldSerialize()
        {
            return this.ClientCredentialType != NetTcpDefaults.MessageSecurityClientCredentialType
                || this.AlgorithmSuite != NetTcpDefaults.MessageSecurityAlgorithmSuite;
        }
    }
}
