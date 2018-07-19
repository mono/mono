//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel
{
    using System.Runtime;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Security;
    using System.ComponentModel;

    public sealed class BasicHttpMessageSecurity
    {
        internal const BasicHttpMessageCredentialType DefaultClientCredentialType = BasicHttpMessageCredentialType.UserName;

        BasicHttpMessageCredentialType clientCredentialType;
        SecurityAlgorithmSuite algorithmSuite;

        public BasicHttpMessageSecurity()
        {
            clientCredentialType = DefaultClientCredentialType;
            algorithmSuite = SecurityAlgorithmSuite.Default;
        }

        public BasicHttpMessageCredentialType ClientCredentialType
        {
            get { return this.clientCredentialType; }
            set
            {
                if (!BasicHttpMessageCredentialTypeHelper.IsDefined(value))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value"));
                }
                this.clientCredentialType = value;
            }
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
            }
        }

        // if any changes are made to this method, please reflect them in the corresponding TryCrete() method
        internal SecurityBindingElement CreateMessageSecurity(bool isSecureTransportMode)
        {
            SecurityBindingElement result;

            if (isSecureTransportMode)
            {
                MessageSecurityVersion version = MessageSecurityVersion.WSSecurity10WSTrustFebruary2005WSSecureConversationFebruary2005WSSecurityPolicy11BasicSecurityProfile10;
                switch (this.clientCredentialType)
                {
                    case BasicHttpMessageCredentialType.Certificate:
                        result = SecurityBindingElement.CreateCertificateOverTransportBindingElement(version);
                        break;
                    case BasicHttpMessageCredentialType.UserName:
                        result = SecurityBindingElement.CreateUserNameOverTransportBindingElement();
                        result.MessageSecurityVersion = version;
                        break;
                    default:
                        Fx.Assert("Unsupported basic http message credential type");
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException());
                }
            }
            else
            {
                if (this.clientCredentialType != BasicHttpMessageCredentialType.Certificate)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.BasicHttpMessageSecurityRequiresCertificate)));
                }
                result = SecurityBindingElement.CreateMutualCertificateBindingElement(MessageSecurityVersion.WSSecurity10WSTrustFebruary2005WSSecureConversationFebruary2005WSSecurityPolicy11BasicSecurityProfile10, true);
            }

            result.DefaultAlgorithmSuite = this.AlgorithmSuite;
            result.SecurityHeaderLayout = SecurityHeaderLayout.Lax;
            result.SetKeyDerivation(false);
            result.DoNotEmitTrust = true;

            return result;
        }

        // This method reverses the CreateMessageSecurity(bool) method
        internal static bool TryCreate(SecurityBindingElement sbe, out BasicHttpMessageSecurity security, out bool isSecureTransportMode)
        {
            Fx.Assert(null != sbe, string.Empty);

            security = null;
            isSecureTransportMode = false;

            if (sbe.DoNotEmitTrust == false)
                return false;
            if (!sbe.IsSetKeyDerivation(false))
                return false;
            if (sbe.SecurityHeaderLayout != SecurityHeaderLayout.Lax)
                return false;
            if (sbe.MessageSecurityVersion != MessageSecurityVersion.WSSecurity10WSTrustFebruary2005WSSecureConversationFebruary2005WSSecurityPolicy11BasicSecurityProfile10)
                return false;

            BasicHttpMessageCredentialType credentialType;
            if (!SecurityBindingElement.IsMutualCertificateBinding(sbe, true))
            {
                isSecureTransportMode = true;
                if (SecurityBindingElement.IsCertificateOverTransportBinding(sbe))
                {
                    credentialType = BasicHttpMessageCredentialType.Certificate;
                }
                else if (SecurityBindingElement.IsUserNameOverTransportBinding(sbe))
                {
                    credentialType = BasicHttpMessageCredentialType.UserName;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                credentialType = BasicHttpMessageCredentialType.Certificate;
            }
            security = new BasicHttpMessageSecurity();
            security.ClientCredentialType = credentialType;
            security.AlgorithmSuite = sbe.DefaultAlgorithmSuite;
            return true;
        }

        internal bool InternalShouldSerialize()
        {
            return this.ShouldSerializeAlgorithmSuite()
                || this.ShouldSerializeClientCredentialType();
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool ShouldSerializeAlgorithmSuite()
        {
            return this.algorithmSuite.GetType() != SecurityAlgorithmSuite.Default.GetType();
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool ShouldSerializeClientCredentialType()
        {
            return this.clientCredentialType != DefaultClientCredentialType;
        }
    }
}
