//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Security
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Net;
    using System.Runtime;
    using System.ServiceModel.Description;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Security.Tokens;
    using System.Text;
    using System.Xml;

    class WSSecurityPolicy12 : WSSecurityPolicy
    {
        public const string WsspNamespace = @"http://docs.oasis-open.org/ws-sx/ws-securitypolicy/200702";

        public const string SignedEncryptedSupportingTokensName = "SignedEncryptedSupportingTokens";
        public const string RequireImpliedDerivedKeysName = "RequireImpliedDerivedKeys";
        public const string RequireExplicitDerivedKeysName = "RequireExplicitDerivedKeys";

        public override string WsspNamespaceUri
        {
            get { return WSSecurityPolicy12.WsspNamespace; }
        }

        public override bool IsSecurityVersionSupported(MessageSecurityVersion version)
        {
            return version == MessageSecurityVersion.WSSecurity10WSTrust13WSSecureConversation13WSSecurityPolicy12BasicSecurityProfile10 ||
                version == MessageSecurityVersion.WSSecurity11WSTrust13WSSecureConversation13WSSecurityPolicy12 ||
                version == MessageSecurityVersion.WSSecurity11WSTrust13WSSecureConversation13WSSecurityPolicy12BasicSecurityProfile10;
        }

        public override MessageSecurityVersion GetSupportedMessageSecurityVersion(SecurityVersion version)
        {
            return (version == SecurityVersion.WSSecurity10) ? MessageSecurityVersion.WSSecurity10WSTrust13WSSecureConversation13WSSecurityPolicy12BasicSecurityProfile10 : MessageSecurityVersion.WSSecurity11WSTrust13WSSecureConversation13WSSecurityPolicy12BasicSecurityProfile10;
        }

        public override TrustDriver TrustDriver
        {
            get
            {
                return new WSTrustDec2005.DriverDec2005(new SecurityStandardsManager(MessageSecurityVersion.WSSecurity11WSTrust13WSSecureConversation13WSSecurityPolicy12, WSSecurityTokenSerializer.DefaultInstance));
            }
        }

        public override XmlElement CreateWsspHttpsTokenAssertion(MetadataExporter exporter, HttpsTransportBindingElement httpsBinding)
        {
            Fx.Assert(httpsBinding != null, "httpsBinding must not be null.");
            Fx.Assert(httpsBinding.AuthenticationScheme.IsSingleton(), "authenticationScheme must be a singleton value for security-mode TransportWithMessageCredential.");

            XmlElement result = CreateWsspAssertion(WSSecurityPolicy.HttpsTokenName);
            if (httpsBinding.RequireClientCertificate ||
                httpsBinding.AuthenticationScheme == AuthenticationSchemes.Basic ||
                httpsBinding.AuthenticationScheme == AuthenticationSchemes.Digest)
            {
                XmlElement policy = CreateWspPolicyWrapper(exporter);
                if (httpsBinding.RequireClientCertificate)
                {
                    policy.AppendChild(CreateWsspAssertion(WSSecurityPolicy.RequireClientCertificateName));
                }
                if (httpsBinding.AuthenticationScheme == AuthenticationSchemes.Basic)
                {
                    policy.AppendChild(CreateWsspAssertion(WSSecurityPolicy.HttpBasicAuthenticationName));
                }
                else if (httpsBinding.AuthenticationScheme == AuthenticationSchemes.Digest)
                {
                    policy.AppendChild(CreateWsspAssertion(WSSecurityPolicy.HttpDigestAuthenticationName));
                }
                result.AppendChild(policy);
            }
            return result;
        }

        public override bool TryImportWsspHttpsTokenAssertion(MetadataImporter importer, ICollection<XmlElement> assertions, HttpsTransportBindingElement httpsBinding)
        {
            if (assertions == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("assertions");
            }

            bool result = true;
            XmlElement assertion;

            if (TryImportWsspAssertion(assertions, HttpsTokenName, out assertion))
            {
                XmlElement policyElement = null;
                foreach (XmlNode node in assertion.ChildNodes)
                {
                    if (node is XmlElement && node.LocalName == WSSecurityPolicy.PolicyName && (node.NamespaceURI == WSSecurityPolicy.WspNamespace || node.NamespaceURI == WSSecurityPolicy.Wsp15Namespace))
                    {
                        policyElement = (XmlElement)node;
                        break;
                    }
                }

                if (policyElement != null)
                {
                    foreach (XmlNode node in policyElement.ChildNodes)
                    {
                        if (node is XmlElement && node.NamespaceURI == this.WsspNamespaceUri)
                        {
                            if (node.LocalName == WSSecurityPolicy.RequireClientCertificateName)
                            {
                                httpsBinding.RequireClientCertificate = true;
                            }
                            else if (node.LocalName == WSSecurityPolicy.HttpBasicAuthenticationName)
                            {
                                httpsBinding.AuthenticationScheme = AuthenticationSchemes.Basic;
                            }
                            else if (node.LocalName == WSSecurityPolicy.HttpDigestAuthenticationName)
                            {
                                httpsBinding.AuthenticationScheme = AuthenticationSchemes.Digest;
                            }
                        }
                    }
                }
            }
            else
            {
                result = false;
            }

            return result;
        }

        public override Collection<XmlElement> CreateWsspSupportingTokensAssertion(MetadataExporter exporter, Collection<SecurityTokenParameters> signed, Collection<SecurityTokenParameters> signedEncrypted, Collection<SecurityTokenParameters> endorsing, Collection<SecurityTokenParameters> signedEndorsing, Collection<SecurityTokenParameters> optionalSigned, Collection<SecurityTokenParameters> optionalSignedEncrypted, Collection<SecurityTokenParameters> optionalEndorsing, Collection<SecurityTokenParameters> optionalSignedEndorsing, AddressingVersion addressingVersion)
        {
            Collection<XmlElement> supportingTokenAssertions = new Collection<XmlElement>();

            // Signed Supporting Tokens
            XmlElement supportingTokenAssertion = CreateWsspSignedSupportingTokensAssertion(exporter, signed, optionalSigned);
            if (supportingTokenAssertion != null)
                supportingTokenAssertions.Add(supportingTokenAssertion);

            // Signed Encrypted Supporting Tokens
            supportingTokenAssertion = CreateWsspSignedEncryptedSupportingTokensAssertion(exporter, signedEncrypted, optionalSignedEncrypted);
            if (supportingTokenAssertion != null)
                supportingTokenAssertions.Add(supportingTokenAssertion);

            // Endorsing Supporting Tokens.
            supportingTokenAssertion = CreateWsspEndorsingSupportingTokensAssertion(exporter, endorsing, optionalEndorsing, addressingVersion);
            if (supportingTokenAssertion != null)
                supportingTokenAssertions.Add(supportingTokenAssertion);

            // Signed Endorsing Supporting Tokens.
            supportingTokenAssertion = CreateWsspSignedEndorsingSupportingTokensAssertion(exporter, signedEndorsing, optionalSignedEndorsing, addressingVersion);
            if (supportingTokenAssertion != null)
                supportingTokenAssertions.Add(supportingTokenAssertion);

            return supportingTokenAssertions;
        }

        public override XmlElement CreateWsspSpnegoContextTokenAssertion(MetadataExporter exporter, SspiSecurityTokenParameters parameters)
        {
            XmlElement result = CreateWsspAssertion(SpnegoContextTokenName);
            SetIncludeTokenValue(result, parameters.InclusionMode);
            result.AppendChild(
                CreateWspPolicyWrapper(
                    exporter,
                    CreateWsspRequireDerivedKeysAssertion(parameters.RequireDerivedKeys),
                    // Always emit <sp:MustNotSendCancel/> for spnego and sslnego
                    CreateWsspMustNotSendCancelAssertion(false),
                    CreateWsspMustNotSendAmendAssertion(),
                    CreateWsspMustNotSendRenewAssertion()
            ));
            return result;
        }

        public override XmlElement CreateMsspSslContextTokenAssertion(MetadataExporter exporter, SslSecurityTokenParameters parameters)
        {
            XmlElement result = CreateMsspAssertion(SslContextTokenName);
            SetIncludeTokenValue(result, parameters.InclusionMode);
            result.AppendChild(
                CreateWspPolicyWrapper(
                    exporter,
                    CreateWsspRequireDerivedKeysAssertion(parameters.RequireDerivedKeys),
                    // Always emit <sp:MustNotSendCancel/> for spnego and sslnego
                    CreateWsspMustNotSendCancelAssertion(false),
                    CreateMsspRequireClientCertificateAssertion(parameters.RequireClientCertificate),
                    CreateWsspMustNotSendAmendAssertion(),
                    CreateWsspMustNotSendRenewAssertion()
            ));
            return result;
        }

        public override XmlElement CreateWsspSecureConversationTokenAssertion(MetadataExporter exporter, SecureConversationSecurityTokenParameters parameters)
        {
            XmlElement result = CreateWsspAssertion(SecureConversationTokenName);
            SetIncludeTokenValue(result, parameters.InclusionMode);
            result.AppendChild(
                CreateWspPolicyWrapper(
                    exporter,
                    CreateWsspRequireDerivedKeysAssertion(parameters.RequireDerivedKeys),
                    CreateWsspMustNotSendCancelAssertion(parameters.RequireCancellation),
                    CreateWsspBootstrapPolicyAssertion(exporter, parameters.BootstrapSecurityBindingElement),
                    CreateWsspMustNotSendAmendAssertion(),
                    (!parameters.RequireCancellation || !parameters.CanRenewSession) ? CreateWsspMustNotSendRenewAssertion() : null
            ));
            return result;
        }

        XmlElement CreateWsspMustNotSendAmendAssertion()
        {
            XmlElement result = CreateWsspAssertion(MustNotSendAmendName);
            return result;
        }

        XmlElement CreateWsspMustNotSendRenewAssertion()
        {
            XmlElement result = CreateWsspAssertion(MustNotSendRenewName);
            return result;
        }

        public override bool TryImportWsspSpnegoContextTokenAssertion(MetadataImporter importer, XmlElement assertion, out SecurityTokenParameters parameters)
        {
            parameters = null;

            SecurityTokenInclusionMode inclusionMode;
            Collection<Collection<XmlElement>> alternatives;

            if (IsWsspAssertion(assertion, SpnegoContextTokenName)
                && TryGetIncludeTokenValue(assertion, out inclusionMode))
            {
                if (TryGetNestedPolicyAlternatives(importer, assertion, out alternatives))
                {
                    foreach (Collection<XmlElement> alternative in alternatives)
                    {
                        SspiSecurityTokenParameters sspi = new SspiSecurityTokenParameters();
                        parameters = sspi;
                        bool requireCancellation;
                        bool canRenewSession;
                        if (TryImportWsspRequireDerivedKeysAssertion(alternative, sspi)
                            && TryImportWsspMustNotSendCancelAssertion(alternative, out requireCancellation)
                            && TryImportWsspMustNotSendAmendAssertion(alternative)
                            // We do not support Renew for spnego and sslnego. Read the 
                            // assertion if present and ignore it.
                            && TryImportWsspMustNotSendRenewAssertion(alternative, out canRenewSession)
                            && alternative.Count == 0)
                        {
                            // Client always set this to true to match the standardbinding.
                            // This setting on client has no effect for spnego and sslnego.
                            sspi.RequireCancellation = true;
                            sspi.InclusionMode = inclusionMode;
                            break;
                        }
                        else
                        {
                            parameters = null;
                        }
                    }
                }
                else
                {
                    parameters = new SspiSecurityTokenParameters();
                    parameters.RequireDerivedKeys = false;
                    parameters.InclusionMode = inclusionMode;
                }
            }

            return parameters != null;
        }

        public override bool TryImportMsspSslContextTokenAssertion(MetadataImporter importer, XmlElement assertion, out SecurityTokenParameters parameters)
        {
            parameters = null;

            SecurityTokenInclusionMode inclusionMode;
            Collection<Collection<XmlElement>> alternatives;

            if (IsMsspAssertion(assertion, SslContextTokenName)
                && TryGetIncludeTokenValue(assertion, out inclusionMode))
            {
                if (TryGetNestedPolicyAlternatives(importer, assertion, out alternatives))
                {
                    foreach (Collection<XmlElement> alternative in alternatives)
                    {
                        SslSecurityTokenParameters ssl = new SslSecurityTokenParameters();
                        parameters = ssl;
                        bool requireCancellation;
                        bool canRenewSession;
                        if (TryImportWsspRequireDerivedKeysAssertion(alternative, ssl)
                            && TryImportWsspMustNotSendCancelAssertion(alternative, out requireCancellation)
                            && TryImportWsspMustNotSendAmendAssertion(alternative)
                            // We do not support Renew for spnego and sslnego. Read the 
                            // assertion if present and ignore it.
                            && TryImportWsspMustNotSendRenewAssertion(alternative, out canRenewSession)
                            && TryImportMsspRequireClientCertificateAssertion(alternative, ssl)
                            && alternative.Count == 0)
                        {
                            // Client always set this to true to match the standardbinding.
                            // This setting on client has no effect for spnego and sslnego.
                            ssl.RequireCancellation = true;
                            ssl.InclusionMode = inclusionMode;
                            break;
                        }
                        else
                        {
                            parameters = null;
                        }
                    }
                }
                else
                {
                    parameters = new SslSecurityTokenParameters();
                    parameters.RequireDerivedKeys = false;
                    parameters.InclusionMode = inclusionMode;
                }
            }

            return parameters != null;
        }

        public override bool TryImportWsspSecureConversationTokenAssertion(MetadataImporter importer, XmlElement assertion, out SecurityTokenParameters parameters)
        {
            parameters = null;

            SecurityTokenInclusionMode inclusionMode;
            Collection<Collection<XmlElement>> alternatives;

            if (IsWsspAssertion(assertion, SecureConversationTokenName)
                && TryGetIncludeTokenValue(assertion, out inclusionMode))
            {
                if (TryGetNestedPolicyAlternatives(importer, assertion, out alternatives))
                {
                    foreach (Collection<XmlElement> alternative in alternatives)
                    {
                        SecureConversationSecurityTokenParameters sc = new SecureConversationSecurityTokenParameters();
                        parameters = sc;
                        bool requireCancellation;
                        bool canRenewSession;
                        if (TryImportWsspRequireDerivedKeysAssertion(alternative, sc)
                            && TryImportWsspMustNotSendCancelAssertion(alternative, out requireCancellation)
                            && TryImportWsspMustNotSendAmendAssertion(alternative)
                            && TryImportWsspMustNotSendRenewAssertion(alternative, out canRenewSession)
                            && TryImportWsspBootstrapPolicyAssertion(importer, alternative, sc)
                            && alternative.Count == 0)
                        {
                            sc.RequireCancellation = requireCancellation;
                            sc.CanRenewSession = canRenewSession;
                            sc.InclusionMode = inclusionMode;
                            break;
                        }
                        else
                        {
                            parameters = null;
                        }
                    }
                }
                else
                {
                    parameters = new SecureConversationSecurityTokenParameters();
                    parameters.InclusionMode = inclusionMode;
                    parameters.RequireDerivedKeys = false;
                }
            }

            return parameters != null;
        }

        public virtual bool TryImportWsspMustNotSendAmendAssertion(ICollection<XmlElement> assertions)
        {
            TryImportWsspAssertion(assertions, MustNotSendAmendName);
            return true;
        }

        public virtual bool TryImportWsspMustNotSendRenewAssertion(ICollection<XmlElement> assertions, out bool canRenewSession)
        {
            canRenewSession = !TryImportWsspAssertion(assertions, MustNotSendRenewName);
            return true;
        }

        XmlElement CreateWsspSignedSupportingTokensAssertion(MetadataExporter exporter, Collection<SecurityTokenParameters> signed, Collection<SecurityTokenParameters> optionalSigned)
        {
            XmlElement result;

            if ((signed == null || signed.Count == 0)
                && (optionalSigned == null || optionalSigned.Count == 0))
            {
                result = null;
            }
            else
            {
                XmlElement policy = CreateWspPolicyWrapper(exporter);

                if (signed != null)
                {
                    foreach (SecurityTokenParameters p in signed)
                    {
                        policy.AppendChild(CreateTokenAssertion(exporter, p));
                    }
                }
                if (optionalSigned != null)
                {
                    foreach (SecurityTokenParameters p in optionalSigned)
                    {
                        policy.AppendChild(CreateTokenAssertion(exporter, p, true));
                    }
                }

                result = CreateWsspAssertion(SignedSupportingTokensName);
                result.AppendChild(policy);
            }

            return result;
        }

        XmlElement CreateWsspSignedEncryptedSupportingTokensAssertion(MetadataExporter exporter, Collection<SecurityTokenParameters> signedEncrypted, Collection<SecurityTokenParameters> optionalSignedEncrypted)
        {
            XmlElement result;

            if ((signedEncrypted == null || signedEncrypted.Count == 0)
                && (optionalSignedEncrypted == null || optionalSignedEncrypted.Count == 0))
            {
                result = null;
            }
            else
            {
                XmlElement policy = CreateWspPolicyWrapper(exporter);

                if (signedEncrypted != null)
                {
                    foreach (SecurityTokenParameters p in signedEncrypted)
                    {
                        policy.AppendChild(CreateTokenAssertion(exporter, p));
                    }
                }
                if (optionalSignedEncrypted != null)
                {
                    foreach (SecurityTokenParameters p in optionalSignedEncrypted)
                    {
                        policy.AppendChild(CreateTokenAssertion(exporter, p, true));
                    }
                }

                result = CreateWsspAssertion(SignedEncryptedSupportingTokensName);
                result.AppendChild(policy);
            }

            return result;
        }

        public override bool TryImportWsspSupportingTokensAssertion(MetadataImporter importer, PolicyConversionContext policyContext, ICollection<XmlElement> assertions, Collection<SecurityTokenParameters> signed, Collection<SecurityTokenParameters> signedEncrypted, Collection<SecurityTokenParameters> endorsing, Collection<SecurityTokenParameters> signedEndorsing, Collection<SecurityTokenParameters> optionalSigned, Collection<SecurityTokenParameters> optionalSignedEncrypted, Collection<SecurityTokenParameters> optionalEndorsing, Collection<SecurityTokenParameters> optionalSignedEndorsing)
        {
            XmlElement assertion;

            if (!TryImportWsspSignedSupportingTokensAssertion(
                importer,
                policyContext,
                assertions,
                signed,
                optionalSigned,
                out assertion)
                && assertion != null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.UnsupportedSecurityPolicyAssertion, assertion.OuterXml)));
            }

            if (!TryImportWsspSignedEncryptedSupportingTokensAssertion(
                importer,
                policyContext,
                assertions,
                signedEncrypted,
                optionalSignedEncrypted,
                out assertion)
                && assertion != null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.UnsupportedSecurityPolicyAssertion, assertion.OuterXml)));
            }

            if (!TryImportWsspEndorsingSupportingTokensAssertion(
                importer,
                policyContext,
                assertions,
                endorsing,
                optionalEndorsing,
                out assertion)
                && assertion != null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.UnsupportedSecurityPolicyAssertion, assertion.OuterXml)));
            }

            if (!TryImportWsspSignedEndorsingSupportingTokensAssertion(
                importer,
                policyContext,
                assertions,
                signedEndorsing,
                optionalSignedEndorsing,
                out assertion)
                && assertion != null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.UnsupportedSecurityPolicyAssertion, assertion.OuterXml)));
            }

            return true;
        }

        bool TryImportWsspSignedSupportingTokensAssertion(MetadataImporter importer, PolicyConversionContext policyContext, ICollection<XmlElement> assertions, Collection<SecurityTokenParameters> signed, Collection<SecurityTokenParameters> optionalSigned, out XmlElement assertion)
        {
            if (signed == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("signed");
            }
            if (optionalSigned == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("optionalSigned");
            }

            bool result = true;

            Collection<Collection<XmlElement>> alternatives;

            if (TryImportWsspAssertion(assertions, SignedSupportingTokensName, out assertion)
                && TryGetNestedPolicyAlternatives(importer, assertion, out alternatives))
            {
                foreach (Collection<XmlElement> alternative in alternatives)
                {
                    SecurityTokenParameters parameters;
                    bool isOptional;
                    while (alternative.Count > 0 && TryImportTokenAssertion(importer, policyContext, alternative, out parameters, out isOptional))
                    {
                        if (isOptional)
                        {
                            optionalSigned.Add(parameters);
                        }
                        else
                        {
                            signed.Add(parameters);
                        }
                    }
                    if (alternative.Count == 0)
                    {
                        result = true;
                        break;
                    }
                    else
                    {
                        result = false;
                    }
                }
            }

            return result;
        }

        bool TryImportWsspSignedEncryptedSupportingTokensAssertion(MetadataImporter importer, PolicyConversionContext policyContext, ICollection<XmlElement> assertions, Collection<SecurityTokenParameters> signedEncrypted, Collection<SecurityTokenParameters> optionalSignedEncrypted, out XmlElement assertion)
        {
            if (signedEncrypted == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("signedEncrypted");
            }
            if (optionalSignedEncrypted == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("optionalSignedEncrypted");
            }

            bool result = true;

            Collection<Collection<XmlElement>> alternatives;

            if (TryImportWsspAssertion(assertions, SignedEncryptedSupportingTokensName, out assertion)
                && TryGetNestedPolicyAlternatives(importer, assertion, out alternatives))
            {
                foreach (Collection<XmlElement> alternative in alternatives)
                {
                    SecurityTokenParameters parameters;
                    bool isOptional;
                    while (alternative.Count > 0 && TryImportTokenAssertion(importer, policyContext, alternative, out parameters, out isOptional))
                    {
                        if (isOptional)
                        {
                            optionalSignedEncrypted.Add(parameters);
                        }
                        else
                        {
                            signedEncrypted.Add(parameters);
                        }
                    }
                    if (alternative.Count == 0)
                    {
                        result = true;
                        break;
                    }
                    else
                    {
                        result = false;
                    }
                }
            }

            return result;
        }

        public override bool TryImportWsspRequireDerivedKeysAssertion(ICollection<XmlElement> assertions, SecurityTokenParameters parameters)
        {
            parameters.RequireDerivedKeys = TryImportWsspAssertion(assertions, WSSecurityPolicy.RequireDerivedKeysName);

            if (!parameters.RequireDerivedKeys)
            {
                parameters.RequireDerivedKeys = TryImportWsspAssertion(assertions, WSSecurityPolicy12.RequireExplicitDerivedKeysName);
            }

            if (!parameters.RequireDerivedKeys)
            {
                XmlElement assertion = null;
                if (TryImportWsspAssertion(assertions, WSSecurityPolicy12.RequireImpliedDerivedKeysName, out assertion))
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.UnsupportedSecurityPolicyAssertion, assertion.OuterXml)));
            }

            return true;
        }

        public override XmlElement CreateWsspTrustAssertion(MetadataExporter exporter, SecurityKeyEntropyMode keyEntropyMode)
        {
            return CreateWsspTrustAssertion(Trust13Name, exporter, keyEntropyMode);
        }

        public override bool TryImportWsspTrustAssertion(MetadataImporter importer, ICollection<XmlElement> assertions, SecurityBindingElement binding, out XmlElement assertion)
        {
            return TryImportWsspTrustAssertion(Trust13Name, importer, assertions, binding, out assertion);
        }

        public override XmlElement CreateWsspRsaTokenAssertion(RsaSecurityTokenParameters parameters)
        {
            XmlElement result = CreateWsspAssertion(KeyValueTokenName);
            SetIncludeTokenValue(result, parameters.InclusionMode);
            return result;
        }

        public override bool TryImportWsspRsaTokenAssertion(MetadataImporter importer, XmlElement assertion, out SecurityTokenParameters parameters)
        {
            parameters = null;

            SecurityTokenInclusionMode inclusionMode;
            Collection<Collection<XmlElement>> alternatives;

            if (IsWsspAssertion(assertion, KeyValueTokenName)
                && TryGetIncludeTokenValue(assertion, out inclusionMode)
                && TryGetNestedPolicyAlternatives(importer, assertion, out alternatives) == false)
            {
                parameters = new RsaSecurityTokenParameters();
                parameters.InclusionMode = inclusionMode;
            }

            return parameters != null;
        }
    }
}
