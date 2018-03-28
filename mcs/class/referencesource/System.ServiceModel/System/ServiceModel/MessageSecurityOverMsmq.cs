//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel
{
    using System.Runtime;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Security;
    using System.ServiceModel.Security.Tokens;

    public sealed class MessageSecurityOverMsmq
    {
        internal const MessageCredentialType DefaultClientCredentialType = MessageCredentialType.Windows;

        MessageCredentialType clientCredentialType;
        SecurityAlgorithmSuite algorithmSuite;
        bool wasAlgorithmSuiteSet;

        public MessageSecurityOverMsmq()
        {
            clientCredentialType = DefaultClientCredentialType;
            algorithmSuite = SecurityAlgorithmSuite.Default;
        }

        [DefaultValue(MsmqDefaults.DefaultClientCredentialType)]
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
        internal SecurityBindingElement CreateSecurityBindingElement()
        {
            SymmetricSecurityBindingElement result;
            bool isKerberosSelected = false;
            switch (this.clientCredentialType)
            {
                case MessageCredentialType.None:
                    result = SecurityBindingElement.CreateAnonymousForCertificateBindingElement();
                    break;
                case MessageCredentialType.UserName:
                    result = SecurityBindingElement.CreateUserNameForCertificateBindingElement();
                    break;
                case MessageCredentialType.Certificate:
                    result = (SymmetricSecurityBindingElement)SecurityBindingElement.CreateMutualCertificateBindingElement();
                    break;
                case MessageCredentialType.Windows:
                    result = SecurityBindingElement.CreateKerberosBindingElement();
                    isKerberosSelected = true;
                    break;
                case MessageCredentialType.IssuedToken:
                    result = SecurityBindingElement.CreateIssuedTokenForCertificateBindingElement(IssuedSecurityTokenParameters.CreateInfoCardParameters(new SecurityStandardsManager(), this.algorithmSuite));
                    break;
                default:
                    Fx.Assert("unknown ClientCredentialType");
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException());
            }

            result.MessageSecurityVersion = MessageSecurityVersion.WSSecurity11WSTrustFebruary2005WSSecureConversationFebruary2005WSSecurityPolicy11;

            // set the algorithm suite and issued token params if required
            if (wasAlgorithmSuiteSet || !isKerberosSelected)
            {
                result.DefaultAlgorithmSuite = this.AlgorithmSuite;
            }
            else if (isKerberosSelected)
            {
                result.DefaultAlgorithmSuite = SecurityAlgorithmSuite.KerberosDefault;
            }

            result.IncludeTimestamp = false;
            result.LocalServiceSettings.DetectReplays = false;
            result.LocalClientSettings.DetectReplays = false;

            return result;
        }

        internal static bool TryCreate(SecurityBindingElement sbe, out MessageSecurityOverMsmq messageSecurity)
        {
            messageSecurity = null;
            if (sbe == null)
                return false;

            SymmetricSecurityBindingElement ssbe = sbe as SymmetricSecurityBindingElement;
            if (ssbe == null)
                return false;

            if (sbe.MessageSecurityVersion != MessageSecurityVersion.WSSecurity11WSTrustFebruary2005WSSecureConversationFebruary2005WSSecurityPolicy11BasicSecurityProfile10
                && sbe.MessageSecurityVersion != MessageSecurityVersion.WSSecurity11WSTrustFebruary2005WSSecureConversationFebruary2005WSSecurityPolicy11)
            {
                return false;
            }

            // do not check local settings: sbe.LocalServiceSettings and sbe.LocalClientSettings

            if (ssbe.IncludeTimestamp)
                return false;

            bool isKerberosSelected = false;
            MessageCredentialType clientCredentialType;
            IssuedSecurityTokenParameters issuedParameters;
            if (SecurityBindingElement.IsAnonymousForCertificateBinding(sbe))
            {
                clientCredentialType = MessageCredentialType.None;
            }
            else if (SecurityBindingElement.IsUserNameForCertificateBinding(sbe))
            {
                clientCredentialType = MessageCredentialType.UserName;
            }
            else if (SecurityBindingElement.IsMutualCertificateBinding(sbe))
            {
                clientCredentialType = MessageCredentialType.Certificate;
            }
            else if (SecurityBindingElement.IsKerberosBinding(sbe))
            {
                clientCredentialType = MessageCredentialType.Windows;
                isKerberosSelected = true;
            }
            else if (SecurityBindingElement.IsIssuedTokenForCertificateBinding(sbe, out issuedParameters))
            {
                if (!IssuedSecurityTokenParameters.IsInfoCardParameters(
                        issuedParameters,
                        new SecurityStandardsManager(
                            sbe.MessageSecurityVersion,
                            new WSSecurityTokenSerializer(
                                sbe.MessageSecurityVersion.SecurityVersion,
                                sbe.MessageSecurityVersion.TrustVersion,
                                sbe.MessageSecurityVersion.SecureConversationVersion,
                                true,
                                null, null, null))))
                    return false;
                clientCredentialType = MessageCredentialType.IssuedToken;
            }
            else
            {
                return false;
            }

            messageSecurity = new MessageSecurityOverMsmq();
            messageSecurity.ClientCredentialType = clientCredentialType;
            // set the algorithm suite and issued token params if required
            if (clientCredentialType != MessageCredentialType.IssuedToken && !isKerberosSelected)
            {
                messageSecurity.AlgorithmSuite = ssbe.DefaultAlgorithmSuite;
            }
            return true;
        }
    }
}
