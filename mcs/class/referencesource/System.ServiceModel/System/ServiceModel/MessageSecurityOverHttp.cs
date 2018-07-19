//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel
{
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Security;
    using System.ServiceModel.Security.Tokens;
    using System.ComponentModel;

    public class MessageSecurityOverHttp
    {
        internal const MessageCredentialType DefaultClientCredentialType = MessageCredentialType.Windows;
        internal const bool DefaultNegotiateServiceCredential = true;

        MessageCredentialType clientCredentialType;
        bool negotiateServiceCredential;
        SecurityAlgorithmSuite algorithmSuite;
        bool wasAlgorithmSuiteSet;

        public MessageSecurityOverHttp()
        {
            clientCredentialType = DefaultClientCredentialType;
            negotiateServiceCredential = DefaultNegotiateServiceCredential;
            algorithmSuite = SecurityAlgorithmSuite.Default;
        }

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

        public bool NegotiateServiceCredential
        {
            get { return this.negotiateServiceCredential; }
            set { this.negotiateServiceCredential = value; }
        }

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

        protected virtual bool IsSecureConversationEnabled()
        {
            return true;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal SecurityBindingElement CreateSecurityBindingElement(bool isSecureTransportMode, bool isReliableSession, MessageSecurityVersion version)
        {
            if (isReliableSession && !this.IsSecureConversationEnabled())
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SecureConversationRequiredByReliableSession)));
            }

            SecurityBindingElement result;
            SecurityBindingElement oneShotSecurity;

            bool isKerberosSelected = false;
            bool emitBspAttributes = true;
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
                        oneShotSecurity = SecurityBindingElement.CreateIssuedTokenOverTransportBindingElement(IssuedSecurityTokenParameters.CreateInfoCardParameters(new SecurityStandardsManager(new WSSecurityTokenSerializer(emitBspAttributes)), this.algorithmSuite));
                        break;
                    default:
                        Fx.Assert("unknown ClientCredentialType");
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException());
                }
                if (this.IsSecureConversationEnabled())
                {
                    result = SecurityBindingElement.CreateSecureConversationBindingElement(oneShotSecurity, true);
                }
                else
                {
                    result = oneShotSecurity;
                }
            }
            else
            {
                if (negotiateServiceCredential)
                {
                    switch (this.clientCredentialType)
                    {
                        case MessageCredentialType.None:
                            oneShotSecurity = SecurityBindingElement.CreateSslNegotiationBindingElement(false, true);
                            break;
                        case MessageCredentialType.UserName:
                            oneShotSecurity = SecurityBindingElement.CreateUserNameForSslBindingElement(true);
                            break;
                        case MessageCredentialType.Certificate:
                            oneShotSecurity = SecurityBindingElement.CreateSslNegotiationBindingElement(true, true);
                            break;
                        case MessageCredentialType.Windows:
                            oneShotSecurity = SecurityBindingElement.CreateSspiNegotiationBindingElement(true);
                            break;
                        case MessageCredentialType.IssuedToken:
                            oneShotSecurity = SecurityBindingElement.CreateIssuedTokenForSslBindingElement(IssuedSecurityTokenParameters.CreateInfoCardParameters(new SecurityStandardsManager(new WSSecurityTokenSerializer(emitBspAttributes)), this.algorithmSuite), true);
                            break;
                        default:
                            Fx.Assert("unknown ClientCredentialType");
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException());
                    }
                }
                else
                {
                    switch (this.clientCredentialType)
                    {
                        case MessageCredentialType.None:
                            oneShotSecurity = SecurityBindingElement.CreateAnonymousForCertificateBindingElement();
                            break;
                        case MessageCredentialType.UserName:
                            oneShotSecurity = SecurityBindingElement.CreateUserNameForCertificateBindingElement();
                            break;
                        case MessageCredentialType.Certificate:
                            oneShotSecurity = SecurityBindingElement.CreateMutualCertificateBindingElement();
                            break;
                        case MessageCredentialType.Windows:
                            oneShotSecurity = SecurityBindingElement.CreateKerberosBindingElement();
                            isKerberosSelected = true;
                            break;
                        case MessageCredentialType.IssuedToken:
                            oneShotSecurity = SecurityBindingElement.CreateIssuedTokenForCertificateBindingElement(IssuedSecurityTokenParameters.CreateInfoCardParameters(new SecurityStandardsManager(new WSSecurityTokenSerializer(emitBspAttributes)), this.algorithmSuite));
                            break;
                        default:
                            Fx.Assert("unknown ClientCredentialType");
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException());
                    }
                }
                if (this.IsSecureConversationEnabled())
                {
                    result = SecurityBindingElement.CreateSecureConversationBindingElement(oneShotSecurity, true);
                }
                else
                {
                    result = oneShotSecurity;
                }
            }

            // set the algorithm suite and issued token params if required
            if (wasAlgorithmSuiteSet || (!isKerberosSelected))
            {
                result.DefaultAlgorithmSuite = oneShotSecurity.DefaultAlgorithmSuite = this.AlgorithmSuite;
            }
            else if (isKerberosSelected)
            {
                result.DefaultAlgorithmSuite = oneShotSecurity.DefaultAlgorithmSuite = SecurityAlgorithmSuite.KerberosDefault;
            }

            result.IncludeTimestamp = true;
            oneShotSecurity.MessageSecurityVersion = version;
            result.MessageSecurityVersion = version;
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

            if (this.IsSecureConversationEnabled())
            {
                // issue the transition SCT for a short duration only
                oneShotSecurity.LocalServiceSettings.IssuedCookieLifetime = SpnegoTokenAuthenticator.defaultServerIssuedTransitionTokenLifetime;
            }

            return result;
        }

        internal static bool TryCreate<TSecurity>(SecurityBindingElement sbe, bool isSecureTransportMode, bool isReliableSession, out TSecurity messageSecurity)
            where TSecurity : MessageSecurityOverHttp
        {
            Fx.Assert(null != sbe, string.Empty);

            messageSecurity = null;

            // do not check local settings: sbe.LocalServiceSettings and sbe.LocalClientSettings

            if (!sbe.IncludeTimestamp)
            {
                return false;
            }

            // Do not check MessageSecurityVersion: it maybe changed by the wrapper element and gets checked later in the SecuritySection.AreBindingsMatching()

            if (sbe.SecurityHeaderLayout != SecurityProtocolFactory.defaultSecurityHeaderLayout)
            {
                return false;
            }

            bool negotiateServiceCredential = DefaultNegotiateServiceCredential;
            MessageCredentialType clientCredentialType;
            SecurityAlgorithmSuite algorithmSuite = SecurityAlgorithmSuite.Default;
            bool isSecureConversation;

            SecurityBindingElement bootstrapSecurity;
            if (!SecurityBindingElement.IsSecureConversationBinding(sbe, true, out bootstrapSecurity))
            {
                isSecureConversation = false;

                bootstrapSecurity = sbe;
            }
            else
            {
                isSecureConversation = true;
            }

            if (!isSecureConversation && typeof(TSecurity).Equals(typeof(MessageSecurityOverHttp)))
            {
                return false;
            }

            if (!isSecureConversation && isReliableSession)
            {
                return false;
            }

            if (isSecureTransportMode && !(bootstrapSecurity is TransportSecurityBindingElement))
            {
                return false;
            }

            IssuedSecurityTokenParameters infocardParameters;
            if (isSecureTransportMode)
            {
                if (SecurityBindingElement.IsUserNameOverTransportBinding(bootstrapSecurity))
                {
                    clientCredentialType = MessageCredentialType.UserName;
                }
                else if (SecurityBindingElement.IsCertificateOverTransportBinding(bootstrapSecurity))
                {
                    clientCredentialType = MessageCredentialType.Certificate;
                }
                else if (SecurityBindingElement.IsSspiNegotiationOverTransportBinding(bootstrapSecurity, true))
                {
                    clientCredentialType = MessageCredentialType.Windows;
                }
                else if (SecurityBindingElement.IsIssuedTokenOverTransportBinding(bootstrapSecurity, out infocardParameters))
                {
                    if (!IssuedSecurityTokenParameters.IsInfoCardParameters(
                            infocardParameters,
                            new SecurityStandardsManager(
                                sbe.MessageSecurityVersion,
                                new WSSecurityTokenSerializer(
                                    sbe.MessageSecurityVersion.SecurityVersion,
                                    sbe.MessageSecurityVersion.TrustVersion,
                                    sbe.MessageSecurityVersion.SecureConversationVersion,
                                    true,
                                    null, null, null))))
                    {
                        return false;
                    }
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
                if (SecurityBindingElement.IsSslNegotiationBinding(bootstrapSecurity, false, true))
                {
                    negotiateServiceCredential = true;
                    clientCredentialType = MessageCredentialType.None;
                }
                else if (SecurityBindingElement.IsUserNameForSslBinding(bootstrapSecurity, true))
                {
                    negotiateServiceCredential = true;
                    clientCredentialType = MessageCredentialType.UserName;
                }
                else if (SecurityBindingElement.IsSslNegotiationBinding(bootstrapSecurity, true, true))
                {
                    negotiateServiceCredential = true;
                    clientCredentialType = MessageCredentialType.Certificate;
                }
                else if (SecurityBindingElement.IsSspiNegotiationBinding(bootstrapSecurity, true))
                {
                    negotiateServiceCredential = true;
                    clientCredentialType = MessageCredentialType.Windows;
                }
                else if (SecurityBindingElement.IsIssuedTokenForSslBinding(bootstrapSecurity, true, out infocardParameters))
                {
                    if (!IssuedSecurityTokenParameters.IsInfoCardParameters(
                            infocardParameters,
                            new SecurityStandardsManager(
                                sbe.MessageSecurityVersion,
                                new WSSecurityTokenSerializer(
                                    sbe.MessageSecurityVersion.SecurityVersion,
                                    sbe.MessageSecurityVersion.TrustVersion,
                                    sbe.MessageSecurityVersion.SecureConversationVersion,
                                    true,
                                    null, null, null))))
                    {
                        return false;
                    }
                    negotiateServiceCredential = true;
                    clientCredentialType = MessageCredentialType.IssuedToken;
                }
                else if (SecurityBindingElement.IsUserNameForCertificateBinding(bootstrapSecurity))
                {
                    negotiateServiceCredential = false;
                    clientCredentialType = MessageCredentialType.UserName;
                }
                else if (SecurityBindingElement.IsMutualCertificateBinding(bootstrapSecurity))
                {
                    negotiateServiceCredential = false;
                    clientCredentialType = MessageCredentialType.Certificate;
                }
                else if (SecurityBindingElement.IsKerberosBinding(bootstrapSecurity))
                {
                    negotiateServiceCredential = false;
                    clientCredentialType = MessageCredentialType.Windows;
                }
                else if (SecurityBindingElement.IsIssuedTokenForCertificateBinding(bootstrapSecurity, out infocardParameters))
                {
                    if (!IssuedSecurityTokenParameters.IsInfoCardParameters(
                            infocardParameters,
                            new SecurityStandardsManager(
                                sbe.MessageSecurityVersion,
                                new WSSecurityTokenSerializer(
                                    sbe.MessageSecurityVersion.SecurityVersion,
                                    sbe.MessageSecurityVersion.TrustVersion,
                                    sbe.MessageSecurityVersion.SecureConversationVersion,
                                    true,
                                    null, null, null))))
                    {
                        return false;
                    }
                    negotiateServiceCredential = false;
                    clientCredentialType = MessageCredentialType.IssuedToken;
                }
                else if (SecurityBindingElement.IsAnonymousForCertificateBinding(bootstrapSecurity))
                {
                    negotiateServiceCredential = false;
                    clientCredentialType = MessageCredentialType.None;
                }
                else
                {
                    return false;
                }
            }

            // Do not check any Local* settings

            // Do not check DefaultAlgorithmSuite: is it often changed after the Security element is created, it will verified by SecuritySectionBase.AreBindingsMatching().

            if (typeof(NonDualMessageSecurityOverHttp).Equals(typeof(TSecurity)))
            {
                messageSecurity = (TSecurity)(object)new NonDualMessageSecurityOverHttp();
                ((NonDualMessageSecurityOverHttp)(object)messageSecurity).EstablishSecurityContext = isSecureConversation;
            }
            else
            {
                messageSecurity = (TSecurity)(object)new MessageSecurityOverHttp();
            }

            messageSecurity.ClientCredentialType = clientCredentialType;
            messageSecurity.NegotiateServiceCredential = negotiateServiceCredential;
            messageSecurity.AlgorithmSuite = sbe.DefaultAlgorithmSuite;
            return true;
        }

        internal bool InternalShouldSerialize()
        {
            return this.ShouldSerializeAlgorithmSuite()
                || this.ShouldSerializeClientCredentialType()
                || ShouldSerializeNegotiateServiceCredential();
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool ShouldSerializeAlgorithmSuite()
        {
            return this.AlgorithmSuite != SecurityAlgorithmSuite.Default;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool ShouldSerializeClientCredentialType()
        {
            return this.ClientCredentialType != DefaultClientCredentialType;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool ShouldSerializeNegotiateServiceCredential()
        {
            return this.NegotiateServiceCredential != DefaultNegotiateServiceCredential;
        }
    }
}
