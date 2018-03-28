//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Security
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.ServiceModel.Security.Tokens;
    using System.Xml;

    abstract class WSSecurityPolicy
    {
        public static ContractDescription NullContract = new ContractDescription("null");
        public static ServiceEndpoint NullServiceEndpoint = new ServiceEndpoint(NullContract);
        public static XmlDocument doc = new XmlDocument();
        public const string WsspPrefix = "sp";
        public const string WspNamespace = MetadataStrings.WSPolicy.NamespaceUri; //@"http://schemas.xmlsoap.org/ws/2004/09/policy";
        public const string Wsp15Namespace = MetadataStrings.WSPolicy.NamespaceUri15;
        public const string WspPrefix = MetadataStrings.WSPolicy.Prefix; //"wsp";
        public const string MsspNamespace = @"http://schemas.microsoft.com/ws/2005/07/securitypolicy";
        public const string MsspPrefix = "mssp";
        public const string PolicyName = MetadataStrings.WSPolicy.Elements.Policy; //"Policy";
        public const string OptionalName = "Optional";
        public const string TrueName = "true";
        public const string FalseName = "false";
        public const string SymmetricBindingName = "SymmetricBinding";
        public const string AsymmetricBindingName = "AsymmetricBinding";
        public const string TransportBindingName = "TransportBinding";
        public const string OnlySignEntireHeadersAndBodyName = "OnlySignEntireHeadersAndBody";
        public const string ProtectionTokenName = "ProtectionToken";
        public const string InitiatorTokenName = "InitiatorToken";
        public const string RecipientTokenName = "RecipientToken";
        public const string TransportTokenName = "TransportToken";
        public const string AlgorithmSuiteName = "AlgorithmSuite";
        public const string LaxName = "Lax";
        public const string LaxTsLastName = "LaxTsLast";
        public const string LaxTsFirstName = "LaxTsFirst";
        public const string StrictName = "Strict";
        public const string IncludeTimestampName = "IncludeTimestamp";
        public const string EncryptBeforeSigningName = "EncryptBeforeSigning";
        public const string ProtectTokens = "ProtectTokens";
        public const string EncryptSignatureName = "EncryptSignature";
        public const string SignedSupportingTokensName = "SignedSupportingTokens";
        public const string EndorsingSupportingTokensName = "EndorsingSupportingTokens";
        public const string SignedEndorsingSupportingTokensName = "SignedEndorsingSupportingTokens";
        public const string Wss10Name = "Wss10";
        public const string MustSupportRefKeyIdentifierName = "MustSupportRefKeyIdentifier";
        public const string MustSupportRefIssuerSerialName = "MustSupportRefIssuerSerial";
        public const string MustSupportRefThumbprintName = "MustSupportRefThumbprint";
        public const string MustSupportRefEncryptedKeyName = "MustSupportRefEncryptedKey";
        public const string RequireSignatureConfirmationName = "RequireSignatureConfirmation";
        public const string MustSupportIssuedTokensName = "MustSupportIssuedTokens";
        public const string RequireClientEntropyName = "RequireClientEntropy";
        public const string RequireServerEntropyName = "RequireServerEntropy";
        public const string Wss11Name = "Wss11";
        public const string Trust10Name = "Trust10";
        public const string Trust13Name = "Trust13";
        public const string RequireAppliesTo = "RequireAppliesTo";
        public const string SignedPartsName = "SignedParts";
        public const string EncryptedPartsName = "EncryptedParts";
        public const string BodyName = "Body";
        public const string HeaderName = "Header";
        public const string NameName = "Name";
        public const string NamespaceName = "Namespace";
        public const string Basic128Name = "Basic128";
        public const string Basic192Name = "Basic192";
        public const string Basic256Name = "Basic256";
        public const string TripleDesName = "TripleDes";
        public const string Basic128Rsa15Name = "Basic128Rsa15";
        public const string Basic192Rsa15Name = "Basic192Rsa15";
        public const string Basic256Rsa15Name = "Basic256Rsa15";
        public const string TripleDesRsa15Name = "TripleDesRsa15";
        public const string Basic128Sha256Name = "Basic128Sha256";
        public const string Basic192Sha256Name = "Basic192Sha256";
        public const string Basic256Sha256Name = "Basic256Sha256";
        public const string TripleDesSha256Name = "TripleDesSha256";
        public const string Basic128Sha256Rsa15Name = "Basic128Sha256Rsa15";
        public const string Basic192Sha256Rsa15Name = "Basic192Sha256Rsa15";
        public const string Basic256Sha256Rsa15Name = "Basic256Sha256Rsa15";
        public const string TripleDesSha256Rsa15Name = "TripleDesSha256Rsa15";
        public const string IncludeTokenName = "IncludeToken";
        public const string KerberosTokenName = "KerberosToken";
        public const string X509TokenName = "X509Token";
        public const string IssuedTokenName = "IssuedToken";
        public const string UsernameTokenName = "UsernameToken";
        public const string RsaTokenName = "RsaToken";
        public const string KeyValueTokenName = "KeyValueToken";
        public const string SpnegoContextTokenName = "SpnegoContextToken";
        public const string SslContextTokenName = "SslContextToken";
        public const string SecureConversationTokenName = "SecureConversationToken";
        public const string WssGssKerberosV5ApReqToken11Name = "WssGssKerberosV5ApReqToken11";
        public const string RequireDerivedKeysName = "RequireDerivedKeys";
        public const string RequireIssuerSerialReferenceName = "RequireIssuerSerialReference";
        public const string RequireKeyIdentifierReferenceName = "RequireKeyIdentifierReference";
        public const string RequireThumbprintReferenceName = "RequireThumbprintReference";
        public const string WssX509V3Token10Name = "WssX509V3Token10";
        public const string WssUsernameToken10Name = "WssUsernameToken10";
        public const string RequestSecurityTokenTemplateName = "RequestSecurityTokenTemplate";
        public const string RequireExternalReferenceName = "RequireExternalReference";
        public const string RequireInternalReferenceName = "RequireInternalReference";
        public const string IssuerName = "Issuer";
        public const string RequireClientCertificateName = "RequireClientCertificate";
        public const string MustNotSendCancelName = "MustNotSendCancel";
        public const string MustNotSendAmendName = "MustNotSendAmend";
        public const string MustNotSendRenewName = "MustNotSendRenew";
        public const string LayoutName = "Layout";
        public const string BootstrapPolicyName = "BootstrapPolicy";
        public const string HttpsTokenName = "HttpsToken";
        public const string HttpBasicAuthenticationName = "HttpBasicAuthentication";
        public const string HttpDigestAuthenticationName = "HttpDigestAuthentication";

        bool _mustSupportRefKeyIdentifierName = false;
        bool _mustSupportRefIssuerSerialName = false;
        bool _mustSupportRefThumbprintName = false;
        bool _protectionTokenHasAsymmetricKey = false;

        public virtual XmlElement CreateWsspAssertion(string name)
        {
            return doc.CreateElement(WsspPrefix, name, this.WsspNamespaceUri);
        }

        public virtual bool IsWsspAssertion(XmlElement assertion)
        {
            return assertion.NamespaceURI == this.WsspNamespaceUri;
        }

        public virtual bool IsWsspAssertion(XmlElement assertion, string name)
        {
            return assertion.NamespaceURI == this.WsspNamespaceUri && assertion.LocalName == name;
        }

        public virtual bool IsMsspAssertion(XmlElement assertion, string name)
        {
            return assertion.NamespaceURI == MsspNamespace && assertion.LocalName == name;
        }

        public virtual bool TryImportWsspAssertion(ICollection<XmlElement> assertions, string name, out XmlElement assertion)
        {
            assertion = null;

            foreach (XmlElement e in assertions)
            {
                if (e.LocalName == name && e.NamespaceURI == this.WsspNamespaceUri)
                {
                    assertion = e;
                    assertions.Remove(e);
                    return true;
                }
            }

            return false;
        }

        public virtual bool TryImportWsspAssertion(ICollection<XmlElement> assertions, string name)
        {
            return TryImportWsspAssertion(assertions, name, false);
        }

        public virtual bool TryImportWsspAssertion(ICollection<XmlElement> assertions, string name, bool isOptional)
        {
            foreach (XmlElement e in assertions)
            {
                if (e.LocalName == name && e.NamespaceURI == this.WsspNamespaceUri)
                {
                    assertions.Remove(e);
                    return true;
                }
            }

            return isOptional;
        }

        public virtual XmlElement CreateMsspAssertion(string name)
        {
            return doc.CreateElement(MsspPrefix, name, MsspNamespace);
        }

        public virtual bool CanImportAssertion(ICollection<XmlElement> assertions)
        {
            foreach (XmlElement e in assertions)
            {
                if (e.NamespaceURI == this.WsspNamespaceUri || e.NamespaceURI == WSSecurityPolicy.MsspNamespace)
                {
                    return true;
                }
            }

            return false;
        }

        public abstract bool IsSecurityVersionSupported(MessageSecurityVersion version);

        public abstract MessageSecurityVersion GetSupportedMessageSecurityVersion(SecurityVersion version);

        public abstract string WsspNamespaceUri { get; }

        public abstract TrustDriver TrustDriver { get; }

        public virtual string AlwaysToRecipientUri
        {
            get { return this.WsspNamespaceUri + @"/IncludeToken/AlwaysToRecipient"; }
        }

        public virtual string NeverUri
        {
            get { return this.WsspNamespaceUri + @"/IncludeToken/Never"; }
        }

        public virtual string OnceUri
        {
            get { return this.WsspNamespaceUri + @"/IncludeToken/Once"; }
        }

        public virtual string AlwaysToInitiatorUri
        {
            get { return this.WsspNamespaceUri + @"/IncludeToken/AlwaysToInitiator"; }
        }

        public virtual bool TryImportMsspAssertion(ICollection<XmlElement> assertions, string name)
        {
            foreach (XmlElement e in assertions)
            {
                if (e.LocalName == name && e.NamespaceURI == MsspNamespace)
                {
                    assertions.Remove(e);
                    return true;
                }
            }

            return false;
        }

        public virtual XmlElement CreateWspPolicyWrapper(MetadataExporter exporter, params XmlElement[] nestedAssertions)
        {
            XmlElement result = doc.CreateElement(WspPrefix, PolicyName, exporter.PolicyVersion.Namespace);

            if (nestedAssertions != null)
            {
                foreach (XmlElement e in nestedAssertions)
                {
                    if (e != null)
                    {
                        result.AppendChild(e);
                    }
                }
            }

            return result;
        }

        public virtual XmlElement CreateWsspSignedPartsAssertion(MessagePartSpecification parts)
        {
            if (parts == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("parts");
            }

            XmlElement result;

            if (parts.IsEmpty())
            {
                result = null;
            }
            else
            {
                result = CreateWsspAssertion(SignedPartsName);
                if (parts.IsBodyIncluded)
                {
                    result.AppendChild(CreateWsspAssertion(BodyName));
                }
                foreach (XmlQualifiedName header in parts.HeaderTypes)
                {
                    result.AppendChild(CreateWsspHeaderAssertion(header));
                }
            }
            return result;
        }

        public virtual XmlElement CreateWsspEncryptedPartsAssertion(MessagePartSpecification parts)
        {
            if (parts == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("parts");
            }

            XmlElement result;

            if (parts.IsEmpty())
            {
                result = null;
            }
            else
            {
                result = CreateWsspAssertion(EncryptedPartsName);
                if (parts.IsBodyIncluded)
                {
                    result.AppendChild(CreateWsspAssertion(BodyName));
                }
                foreach (XmlQualifiedName header in parts.HeaderTypes)
                {
                    result.AppendChild(CreateWsspHeaderAssertion(header));
                }
            }
            return result;
        }

        public virtual MessagePartSpecification TryGetProtectedParts(XmlElement assertion)
        {
            MessagePartSpecification parts = new MessagePartSpecification();

            foreach (XmlNode node in assertion.ChildNodes)
            {
                if (node.NodeType == XmlNodeType.Whitespace || node.NodeType == XmlNodeType.Comment)
                {
                    continue;
                }
                else if (node is XmlElement)
                {
                    XmlElement element = (XmlElement)node;
                    if (IsWsspAssertion(element, BodyName))
                    {
                        parts.IsBodyIncluded = true;
                    }
                    else if (IsWsspAssertion(element, HeaderName))
                    {
                        string name = element.GetAttribute(NameName);
                        string ns = element.GetAttribute(NamespaceName);

                        if (ns == null)
                        {
                            parts = null;
                            break;
                        }

                        parts.HeaderTypes.Add(new XmlQualifiedName(name, ns));
                    }
                    else
                    {
                        parts = null;
                        break;
                    }
                }
                else
                {
                    parts = null;
                    break;
                }
            }

            return parts;
        }

        public virtual bool TryImportWsspEncryptedPartsAssertion(ICollection<XmlElement> assertions, out MessagePartSpecification parts, out XmlElement assertion)
        {
            if (TryImportWsspAssertion(assertions, EncryptedPartsName, out assertion))
            {
                parts = TryGetProtectedParts(assertion);
            }
            else
            {
                parts = null;
            }

            return parts != null;
        }

        public virtual bool TryImportWsspSignedPartsAssertion(ICollection<XmlElement> assertions, out MessagePartSpecification parts, out XmlElement assertion)
        {
            if (TryImportWsspAssertion(assertions, SignedPartsName, out assertion))
            {
                parts = TryGetProtectedParts(assertion);
            }
            else
            {
                parts = null;
            }

            return parts != null;
        }

        public virtual XmlElement CreateWsspHeaderAssertion(XmlQualifiedName header)
        {
            XmlElement result = CreateWsspAssertion(HeaderName);
            result.SetAttribute(NameName, header.Name);
            result.SetAttribute(NamespaceName, header.Namespace);

            return result;
        }

        public virtual XmlElement CreateWsspSymmetricBindingAssertion(MetadataExporter exporter, PolicyConversionContext policyContext, SymmetricSecurityBindingElement binding)
        {
            if (binding == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("binding");
            }

            XmlElement result = CreateWsspAssertion(SymmetricBindingName);
            result.AppendChild(
                CreateWspPolicyWrapper(
                    exporter,
                    CreateWsspProtectionTokenAssertion(exporter, binding.ProtectionTokenParameters),
                    CreateWsspAlgorithmSuiteAssertion(exporter, binding.DefaultAlgorithmSuite),
                    CreateWsspLayoutAssertion(exporter, binding.SecurityHeaderLayout),
                    CreateWsspIncludeTimestampAssertion(binding.IncludeTimestamp),
                    CreateWsspEncryptBeforeSigningAssertion(binding.MessageProtectionOrder),
                    CreateWsspEncryptSignatureAssertion(policyContext, binding),
                    CreateWsspProtectTokensAssertion(binding),
                    CreateWsspAssertion(OnlySignEntireHeadersAndBodyName)
            ));

            return result;
        }

        public virtual bool TryGetNestedPolicyAlternatives(MetadataImporter importer, XmlElement assertion, out Collection<Collection<XmlElement>> alternatives)
        {
            alternatives = null;

            XmlElement policyElement = null;
            foreach (XmlNode node in assertion.ChildNodes)
            {
                if (node is XmlElement && node.LocalName == PolicyName && (node.NamespaceURI == WspNamespace || node.NamespaceURI == Wsp15Namespace))
                {
                    policyElement = (XmlElement)node;
                    break;
                }
            }

            if (policyElement == null)
            {
                alternatives = null;
            }
            else
            {
                IEnumerable<IEnumerable<XmlElement>> enumerableAlternatives = importer.NormalizePolicy(new XmlElement[] { policyElement });

                alternatives = new Collection<Collection<XmlElement>>();
                foreach (IEnumerable<XmlElement> enumerableAlternative in enumerableAlternatives)
                {
                    Collection<XmlElement> alternative = new Collection<XmlElement>();
                    alternatives.Add(alternative);
                    foreach (XmlElement e in enumerableAlternative)
                    {
                        alternative.Add(e);
                    }
                }
            }

            return alternatives != null;
        }

        public virtual bool TryImportWsspSymmetricBindingAssertion(MetadataImporter importer, PolicyConversionContext policyContext, ICollection<XmlElement> assertions, out SymmetricSecurityBindingElement binding, out XmlElement assertion)
        {
            binding = null;

            Collection<Collection<XmlElement>> alternatives;

            if (TryImportWsspAssertion(assertions, SymmetricBindingName, out assertion)
                && TryGetNestedPolicyAlternatives(importer, assertion, out alternatives))
            {
                foreach (Collection<XmlElement> alternative in alternatives)
                {
                    MessageProtectionOrder order;
                    bool protectTokens;
                    binding = new SymmetricSecurityBindingElement();
                    if (TryImportWsspProtectionTokenAssertion(importer, policyContext, alternative, binding)
                        && TryImportWsspAlgorithmSuiteAssertion(importer, alternative, binding)
                        && TryImportWsspLayoutAssertion(importer, alternative, binding)
                        && TryImportWsspIncludeTimestampAssertion(alternative, binding)
                        && TryImportMessageProtectionOrderAssertions(alternative, out order)
                        && TryImportWsspProtectTokensAssertion(alternative, out protectTokens)
                        && TryImportWsspAssertion(alternative, OnlySignEntireHeadersAndBodyName, true)
                        && alternative.Count == 0)
                    {
                        binding.MessageProtectionOrder = order;
                        binding.ProtectTokens = protectTokens;
                        break;
                    }
                    else
                    {
                        binding = null;
                    }
                }
            }

            return binding != null;
        }

        public virtual XmlElement CreateWsspAsymmetricBindingAssertion(MetadataExporter exporter, PolicyConversionContext policyContext, AsymmetricSecurityBindingElement binding)
        {
            if (binding == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("binding");
            }

            XmlElement result = CreateWsspAssertion(AsymmetricBindingName);
            result.AppendChild(
                CreateWspPolicyWrapper(
                    exporter,
                    CreateWsspInitiatorTokenAssertion(exporter, binding.InitiatorTokenParameters),
                    CreateWsspRecipientTokenAssertion(exporter, binding.RecipientTokenParameters),
                    CreateWsspAlgorithmSuiteAssertion(exporter, binding.DefaultAlgorithmSuite),
                    CreateWsspLayoutAssertion(exporter, binding.SecurityHeaderLayout),
                    CreateWsspIncludeTimestampAssertion(binding.IncludeTimestamp),
                    CreateWsspEncryptBeforeSigningAssertion(binding.MessageProtectionOrder),
                    CreateWsspEncryptSignatureAssertion(policyContext, binding),
                    CreateWsspProtectTokensAssertion(binding),
                    CreateWsspAssertion(OnlySignEntireHeadersAndBodyName)
            ));

            return result;
        }

        public virtual bool TryImportWsspAsymmetricBindingAssertion(MetadataImporter importer, PolicyConversionContext policyContext, ICollection<XmlElement> assertions, out AsymmetricSecurityBindingElement binding, out XmlElement assertion)
        {
            binding = null;

            Collection<Collection<XmlElement>> alternatives;

            if (TryImportWsspAssertion(assertions, AsymmetricBindingName, out assertion)
                && TryGetNestedPolicyAlternatives(importer, assertion, out alternatives))
            {
                foreach (Collection<XmlElement> alternative in alternatives)
                {
                    MessageProtectionOrder order;
                    bool protectTokens;
                    binding = new AsymmetricSecurityBindingElement();
                    if (TryImportWsspInitiatorTokenAssertion(importer, policyContext, alternative, binding)
                        && TryImportWsspRecipientTokenAssertion(importer, policyContext, alternative, binding)
                        && TryImportWsspAlgorithmSuiteAssertion(importer, alternative, binding)
                        && TryImportWsspLayoutAssertion(importer, alternative, binding)
                        && TryImportWsspIncludeTimestampAssertion(alternative, binding)
                        && TryImportMessageProtectionOrderAssertions(alternative, out order)
                        && TryImportWsspProtectTokensAssertion(alternative, out protectTokens)
                        && TryImportWsspAssertion(alternative, OnlySignEntireHeadersAndBodyName, true)
                        && alternative.Count == 0)
                    {
                        binding.MessageProtectionOrder = order;
                        binding.ProtectTokens = protectTokens;
                        break;
                    }
                    else
                    {
                        binding = null;
                    }
                }
            }

            return binding != null;
        }

        public virtual XmlElement CreateWsspTransportBindingAssertion(MetadataExporter exporter, TransportSecurityBindingElement binding, XmlElement transportTokenAssertion)
        {
            XmlElement result = CreateWsspAssertion(TransportBindingName);
            result.AppendChild(
                CreateWspPolicyWrapper(
                    exporter,
                    CreateWsspTransportTokenAssertion(exporter, transportTokenAssertion),
                    CreateWsspAlgorithmSuiteAssertion(exporter, binding.DefaultAlgorithmSuite),
                    CreateWsspLayoutAssertion(exporter, binding.SecurityHeaderLayout),
                    CreateWsspIncludeTimestampAssertion(binding.IncludeTimestamp)
            ));

            return result;
        }

        public virtual bool TryImportWsspTransportBindingAssertion(MetadataImporter importer, ICollection<XmlElement> assertions, out TransportSecurityBindingElement binding, out XmlElement assertion)
        {
            binding = null;

            Collection<Collection<XmlElement>> alternatives;

            if (TryImportWsspAssertion(assertions, TransportBindingName, out assertion)
                && TryGetNestedPolicyAlternatives(importer, assertion, out alternatives))
            {
                foreach (Collection<XmlElement> alternative in alternatives)
                {
                    XmlElement transportTokenAssertion;
                    binding = new TransportSecurityBindingElement();
                    if (TryImportWsspTransportTokenAssertion(importer, alternative, out transportTokenAssertion)
                        && TryImportWsspAlgorithmSuiteAssertion(importer, alternative, binding)
                        && TryImportWsspLayoutAssertion(importer, alternative, binding)
                        && TryImportWsspIncludeTimestampAssertion(alternative, binding)
                        && alternative.Count == 0)
                    {
                        if (false == importer.State.ContainsKey(SecurityBindingElementImporter.InSecureConversationBootstrapBindingImportMode))
                        {
                            // The transportTokenAssertion should be consumed by the transport binding importer
                            // for all primary bindings. However, for secure conversation bootstrap bindings
                            // the bootstrap policy does not contain any transport assertions, so adding the
                            // transport token assertion to the collection of unimported assertions would
                            // increase the likelihood of policy import failure due to unrecognized assertions. 
                            assertions.Add(transportTokenAssertion);
                        }
                        break;
                    }
                    else
                    {
                        binding = null;
                    }
                }
            }

            return binding != null;
        }

        public virtual XmlElement CreateWsspWssAssertion(MetadataExporter exporter, SecurityBindingElement binding)
        {
            if (binding == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("binding");
            }

            if (binding.MessageSecurityVersion.SecurityVersion == SecurityVersion.WSSecurity10)
            {
                return CreateWsspWss10Assertion(exporter);
            }
            else if (binding.MessageSecurityVersion.SecurityVersion == SecurityVersion.WSSecurity11)
            {
                if (binding is SymmetricSecurityBindingElement)
                {
                    return CreateWsspWss11Assertion(exporter, ((SymmetricSecurityBindingElement)binding).RequireSignatureConfirmation);
                }
                else if (binding is AsymmetricSecurityBindingElement)
                {
                    return CreateWsspWss11Assertion(exporter, ((AsymmetricSecurityBindingElement)binding).RequireSignatureConfirmation);
                }
                else
                {
                    return CreateWsspWss11Assertion(exporter, false);
                }
            }
            else
            {
                return null;
            }
        }

        public virtual bool TryImportWsspWssAssertion(MetadataImporter importer, ICollection<XmlElement> assertions, SecurityBindingElement binding, out XmlElement assertion)
        {
            if (binding == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("binding");
            }
            if (assertions == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("assertions");
            }

            bool result = true;
            Collection<Collection<XmlElement>> alternatives;

            if (TryImportWsspAssertion(assertions, Wss10Name, out assertion))
            {
                if (TryGetNestedPolicyAlternatives(importer, assertion, out alternatives))
                {
                    foreach (Collection<XmlElement> alternative in alternatives)
                    {
                        TryImportWsspAssertion(alternative, MustSupportRefKeyIdentifierName);
                        TryImportWsspAssertion(alternative, MustSupportRefIssuerSerialName);
                        if (alternative.Count == 0)
                        {
                            binding.MessageSecurityVersion = this.GetSupportedMessageSecurityVersion(SecurityVersion.WSSecurity10);
                            result = true;
                            break;
                        }
                        else
                        {
                            result = false;
                        }
                    }
                }
            }
            else if (TryImportWsspAssertion(assertions, Wss11Name, out assertion))
            {
                if (TryGetNestedPolicyAlternatives(importer, assertion, out alternatives))
                {
                    foreach (Collection<XmlElement> alternative in alternatives)
                    {
                        TryImportWsspAssertion(alternative, MustSupportRefKeyIdentifierName);
                        TryImportWsspAssertion(alternative, MustSupportRefIssuerSerialName);
                        TryImportWsspAssertion(alternative, MustSupportRefThumbprintName);
                        TryImportWsspAssertion(alternative, MustSupportRefEncryptedKeyName);
                        bool requireSignatureConfirmation = TryImportWsspAssertion(alternative, RequireSignatureConfirmationName);
                        if (alternative.Count == 0)
                        {
                            binding.MessageSecurityVersion = this.GetSupportedMessageSecurityVersion(SecurityVersion.WSSecurity11);
                            if (binding is SymmetricSecurityBindingElement)
                            {
                                ((SymmetricSecurityBindingElement)binding).RequireSignatureConfirmation = requireSignatureConfirmation;
                            }
                            else if (binding is AsymmetricSecurityBindingElement)
                            {
                                ((AsymmetricSecurityBindingElement)binding).RequireSignatureConfirmation = requireSignatureConfirmation;
                            }
                            result = true;
                            break;
                        }
                        else
                        {
                            result = false;
                        }
                    }
                }
            }

            return result;
        }

        public virtual XmlElement CreateWsspWss10Assertion(MetadataExporter exporter)
        {
            XmlElement result = CreateWsspAssertion(Wss10Name);
            result.AppendChild(
                CreateWspPolicyWrapper(
                    exporter,
                    CreateWsspAssertionMustSupportRefKeyIdentifierName(),
                    CreateWsspAssertionMustSupportRefIssuerSerialName()
            ));

            return result;
        }

        public virtual XmlElement CreateWsspWss11Assertion(MetadataExporter exporter, bool requireSignatureConfirmation)
        {
            XmlElement result = CreateWsspAssertion(Wss11Name);
            result.AppendChild(
                CreateWspPolicyWrapper(
                    exporter,
                    CreateWsspAssertionMustSupportRefKeyIdentifierName(),
                    CreateWsspAssertionMustSupportRefIssuerSerialName(),
                    CreateWsspAssertionMustSupportRefThumbprintName(),
                    CreateWsspAssertionMustSupportRefEncryptedKeyName(),
                    CreateWsspRequireSignatureConformationAssertion(requireSignatureConfirmation)
            ));

            return result;
        }
        public virtual XmlElement CreateWsspAssertionMustSupportRefKeyIdentifierName()
        {
            if (_mustSupportRefKeyIdentifierName)
            {
                return CreateWsspAssertion(MustSupportRefKeyIdentifierName);
            }
            else
            {
                return null;
            }
        }

        public virtual XmlElement CreateWsspAssertionMustSupportRefIssuerSerialName()
        {
            if (_mustSupportRefIssuerSerialName)
            {
                return CreateWsspAssertion(MustSupportRefIssuerSerialName);
            }
            else
            {
                return null;
            }
        }

        public virtual XmlElement CreateWsspAssertionMustSupportRefThumbprintName()
        {
            if (_mustSupportRefThumbprintName)
            {
                return CreateWsspAssertion(MustSupportRefThumbprintName);
            }
            else
            {
                return null;
            }
        }

        public virtual XmlElement CreateWsspAssertionMustSupportRefEncryptedKeyName()
        {
            // protectionTokenHasAsymmetricKey is only set to true for a SymmetricBindingElement having an asymmetric key
            if (_protectionTokenHasAsymmetricKey)
            {
                return CreateWsspAssertion(MustSupportRefEncryptedKeyName);
            }
            else
            {
                return null;
            }
        }

        public virtual XmlElement CreateWsspRequireSignatureConformationAssertion(bool requireSignatureConfirmation)
        {
            if (requireSignatureConfirmation)
            {
                return CreateWsspAssertion(RequireSignatureConfirmationName);
            }
            else
            {
                return null;
            }
        }

        public abstract XmlElement CreateWsspTrustAssertion(MetadataExporter exporter, SecurityKeyEntropyMode keyEntropyMode);

        public abstract bool TryImportWsspTrustAssertion(MetadataImporter importer, ICollection<XmlElement> assertions, SecurityBindingElement binding, out XmlElement assertion);

        protected XmlElement CreateWsspTrustAssertion(string trustName, MetadataExporter exporter, SecurityKeyEntropyMode keyEntropyMode)
        {
            XmlElement result = CreateWsspAssertion(trustName);
            result.AppendChild(
                CreateWspPolicyWrapper(
                    exporter,
                    CreateWsspAssertion(MustSupportIssuedTokensName),
                    CreateWsspRequireClientEntropyAssertion(keyEntropyMode),
                    CreateWsspRequireServerEntropyAssertion(keyEntropyMode)
            ));

            return result;
        }

        protected bool TryImportWsspTrustAssertion(string trustName, MetadataImporter importer, ICollection<XmlElement> assertions, SecurityBindingElement binding, out XmlElement assertion)
        {
            if (binding == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("binding");
            }
            if (assertions == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("assertions");
            }

            bool result = true;
            Collection<Collection<XmlElement>> alternatives;

            if (TryImportWsspAssertion(assertions, trustName, out assertion)
                && TryGetNestedPolicyAlternatives(importer, assertion, out alternatives))
            {
                foreach (Collection<XmlElement> alternative in alternatives)
                {
                    TryImportWsspAssertion(alternative, MustSupportIssuedTokensName);
                    bool requireClientEntropy = TryImportWsspAssertion(alternative, RequireClientEntropyName);
                    bool requireServerEntropy = TryImportWsspAssertion(alternative, RequireServerEntropyName);
                    if (trustName == Trust13Name)
                    {
                        // We are just reading this optional element.
                        TryImportWsspAssertion(alternative, RequireAppliesTo);
                    }
                    if (alternative.Count == 0)
                    {
                        if (requireClientEntropy)
                        {
                            if (requireServerEntropy)
                            {
                                binding.KeyEntropyMode = SecurityKeyEntropyMode.CombinedEntropy;
                            }
                            else
                            {
                                binding.KeyEntropyMode = SecurityKeyEntropyMode.ClientEntropy;
                            }
                        }
                        else if (requireServerEntropy)
                        {
                            binding.KeyEntropyMode = SecurityKeyEntropyMode.ServerEntropy;
                        }

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

        public virtual XmlElement CreateWsspRequireClientEntropyAssertion(SecurityKeyEntropyMode keyEntropyMode)
        {
            if (keyEntropyMode == SecurityKeyEntropyMode.ClientEntropy || keyEntropyMode == SecurityKeyEntropyMode.CombinedEntropy)
            {
                return CreateWsspAssertion(RequireClientEntropyName);
            }
            else
            {
                return null;
            }
        }

        public virtual XmlElement CreateWsspRequireServerEntropyAssertion(SecurityKeyEntropyMode keyEntropyMode)
        {
            if (keyEntropyMode == SecurityKeyEntropyMode.ServerEntropy || keyEntropyMode == SecurityKeyEntropyMode.CombinedEntropy)
            {
                return CreateWsspAssertion(RequireServerEntropyName);
            }
            else
            {
                return null;
            }
        }

        public virtual Collection<XmlElement> CreateWsspSupportingTokensAssertion(MetadataExporter exporter, Collection<SecurityTokenParameters> signed, Collection<SecurityTokenParameters> signedEncrypted, Collection<SecurityTokenParameters> endorsing, Collection<SecurityTokenParameters> signedEndorsing, Collection<SecurityTokenParameters> optionalSigned, Collection<SecurityTokenParameters> optionalSignedEncrypted, Collection<SecurityTokenParameters> optionalEndorsing, Collection<SecurityTokenParameters> optionalSignedEndorsing)
        {
            return CreateWsspSupportingTokensAssertion(exporter, signed, signedEncrypted, endorsing, signedEndorsing, optionalSigned, optionalSignedEncrypted, optionalEndorsing, optionalSignedEndorsing, null);
        }

        public virtual Collection<XmlElement> CreateWsspSupportingTokensAssertion(MetadataExporter exporter, Collection<SecurityTokenParameters> signed, Collection<SecurityTokenParameters> signedEncrypted, Collection<SecurityTokenParameters> endorsing, Collection<SecurityTokenParameters> signedEndorsing, Collection<SecurityTokenParameters> optionalSigned, Collection<SecurityTokenParameters> optionalSignedEncrypted, Collection<SecurityTokenParameters> optionalEndorsing, Collection<SecurityTokenParameters> optionalSignedEndorsing, AddressingVersion addressingVersion)
        {
            Collection<XmlElement> supportingTokenAssertions = new Collection<XmlElement>();

            // Signed Supporting Tokens
            XmlElement supportingTokenAssertion = CreateWsspSignedSupportingTokensAssertion(exporter, signed, signedEncrypted, optionalSigned, optionalSignedEncrypted);
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

        protected XmlElement CreateWsspSignedSupportingTokensAssertion(MetadataExporter exporter, Collection<SecurityTokenParameters> signed, Collection<SecurityTokenParameters> signedEncrypted, Collection<SecurityTokenParameters> optionalSigned, Collection<SecurityTokenParameters> optionalSignedEncrypted)
        {
            XmlElement result;

            if ((signed == null || signed.Count == 0)
                && (signedEncrypted == null || signedEncrypted.Count == 0)
                && (optionalSigned == null || optionalSigned.Count == 0)
                && (optionalSignedEncrypted == null || optionalSignedEncrypted.Count == 0))
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
                if (signedEncrypted != null)
                {
                    foreach (SecurityTokenParameters p in signedEncrypted)
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
                if (optionalSignedEncrypted != null)
                {
                    foreach (SecurityTokenParameters p in optionalSignedEncrypted)
                    {
                        policy.AppendChild(CreateTokenAssertion(exporter, p, true));
                    }
                }

                result = CreateWsspAssertion(SignedSupportingTokensName);
                result.AppendChild(policy);
            }

            return result;
        }

        protected XmlElement CreateWsspEndorsingSupportingTokensAssertion(MetadataExporter exporter, Collection<SecurityTokenParameters> endorsing, Collection<SecurityTokenParameters> optionalEndorsing, AddressingVersion addressingVersion)
        {
            return CreateWsspiSupportingTokensAssertion(exporter, endorsing, optionalEndorsing, addressingVersion, EndorsingSupportingTokensName);
        }

        protected XmlElement CreateWsspSignedEndorsingSupportingTokensAssertion(MetadataExporter exporter, Collection<SecurityTokenParameters> signedEndorsing, Collection<SecurityTokenParameters> optionalSignedEndorsing, AddressingVersion addressingVersion)
        {
            return CreateWsspiSupportingTokensAssertion(exporter, signedEndorsing, optionalSignedEndorsing, addressingVersion, SignedEndorsingSupportingTokensName);
        }

        protected XmlElement CreateWsspiSupportingTokensAssertion(MetadataExporter exporter, Collection<SecurityTokenParameters> endorsing, Collection<SecurityTokenParameters> optionalEndorsing, AddressingVersion addressingVersion, string assertionName)
        {
            XmlElement result;
            bool hasAssymetricKey = false;

            if ((endorsing == null || endorsing.Count == 0)
                && (optionalEndorsing == null || optionalEndorsing.Count == 0))
            {
                result = null;
            }
            else
            {
                XmlElement policy = CreateWspPolicyWrapper(exporter);

                if (endorsing != null)
                {
                    foreach (SecurityTokenParameters p in endorsing)
                    {
                        if (p.HasAsymmetricKey)
                            hasAssymetricKey = true;

                        policy.AppendChild(CreateTokenAssertion(exporter, p));
                    }
                }
                if (optionalEndorsing != null)
                {
                    foreach (SecurityTokenParameters p in optionalEndorsing)
                    {
                        if (p.HasAsymmetricKey)
                            hasAssymetricKey = true;

                        policy.AppendChild(CreateTokenAssertion(exporter, p, true));
                    }
                }
                if (addressingVersion != null && AddressingVersion.None != addressingVersion)
                {
                    // only add assertion to sign the 'To' only if an assymetric key is found
                    if (hasAssymetricKey)
                    {
                        policy.AppendChild(
                            CreateWsspSignedPartsAssertion(
                                new MessagePartSpecification(new XmlQualifiedName(AddressingStrings.To, addressingVersion.Namespace))));
                    }
                }

                result = CreateWsspAssertion(assertionName);
                result.AppendChild(policy);
            }

            return result;
        }

        public virtual bool TryImportWsspSupportingTokensAssertion(MetadataImporter importer, PolicyConversionContext policyContext, ICollection<XmlElement> assertions, Collection<SecurityTokenParameters> signed, Collection<SecurityTokenParameters> signedEncrypted, Collection<SecurityTokenParameters> endorsing, Collection<SecurityTokenParameters> signedEndorsing, Collection<SecurityTokenParameters> optionalSigned, Collection<SecurityTokenParameters> optionalSignedEncrypted, Collection<SecurityTokenParameters> optionalEndorsing, Collection<SecurityTokenParameters> optionalSignedEndorsing)
        {
            XmlElement assertion;

            if (!TryImportWsspSignedSupportingTokensAssertion(
                importer,
                policyContext,
                assertions,
                signed,
                signedEncrypted,
                optionalSigned,
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

        protected bool TryImportWsspSignedSupportingTokensAssertion(MetadataImporter importer, PolicyConversionContext policyContext, ICollection<XmlElement> assertions, Collection<SecurityTokenParameters> signed, Collection<SecurityTokenParameters> signedEncrypted, Collection<SecurityTokenParameters> optionalSigned, Collection<SecurityTokenParameters> optionalSignedEncrypted, out XmlElement assertion)
        {
            if (signed == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("signed");
            }
            if (signedEncrypted == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("signedEncrypted");
            }
            if (optionalSigned == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("optionalSigned");
            }
            if (optionalSignedEncrypted == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("optionalSignedEncrypted");
            }

            bool result = true;

            Collection<Collection<XmlElement>> alternatives;

            if (TryImportWsspAssertion(assertions, SignedSupportingTokensName, out assertion)
                && TryGetNestedPolicyAlternatives(importer, assertion, out alternatives))
            {
                foreach (Collection<XmlElement> alternative in alternatives)
                {
                    Collection<SecurityTokenParameters> signedSupportingTokens = new Collection<SecurityTokenParameters>();
                    Collection<SecurityTokenParameters> optionalSignedSupportingTokens = new Collection<SecurityTokenParameters>();
                    SecurityTokenParameters parameters;
                    bool isOptional;
                    while (alternative.Count > 0 && TryImportTokenAssertion(importer, policyContext, alternative, out parameters, out isOptional))
                    {
                        if (isOptional)
                        {
                            optionalSignedSupportingTokens.Add(parameters);
                        }
                        else
                        {
                            signedSupportingTokens.Add(parameters);
                        }
                    }
                    if (alternative.Count == 0)
                    {
                        foreach (SecurityTokenParameters p in signedSupportingTokens)
                        {
                            if (p is UserNameSecurityTokenParameters)
                            {
                                signedEncrypted.Add(p);
                            }
                            else
                            {
                                signed.Add(p);
                            }
                        }
                        foreach (SecurityTokenParameters p in optionalSignedSupportingTokens)
                        {
                            if (p is UserNameSecurityTokenParameters)
                            {
                                optionalSignedEncrypted.Add(p);
                            }
                            else
                            {
                                optionalSigned.Add(p);
                            }
                        }
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

        protected bool TryImportWsspEndorsingSupportingTokensAssertion(MetadataImporter importer, PolicyConversionContext policyContext, ICollection<XmlElement> assertions, Collection<SecurityTokenParameters> endorsing, Collection<SecurityTokenParameters> optionalEndorsing, out XmlElement assertion)
        {
            if (endorsing == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("endorsing");
            }
            if (optionalEndorsing == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("optionalEndorsing");
            }

            bool result = true;

            Collection<Collection<XmlElement>> alternatives;

            if (TryImportWsspAssertion(assertions, EndorsingSupportingTokensName, out assertion)
                && TryGetNestedPolicyAlternatives(importer, assertion, out alternatives))
            {
                foreach (Collection<XmlElement> alternative in alternatives)
                {
                    MessagePartSpecification signedParts;
                    if (!TryImportWsspSignedPartsAssertion(alternative, out signedParts, out assertion) && assertion != null)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.UnsupportedSecurityPolicyAssertion, assertion.OuterXml)));
                    }

                    Collection<SecurityTokenParameters> supportingTokens = new Collection<SecurityTokenParameters>();
                    Collection<SecurityTokenParameters> optionalSupportingTokens = new Collection<SecurityTokenParameters>();
                    SecurityTokenParameters parameters;
                    bool isOptional;
                    while (alternative.Count > 0 && TryImportTokenAssertion(importer, policyContext, alternative, out parameters, out isOptional))
                    {
                        if (isOptional)
                        {
                            optionalSupportingTokens.Add(parameters);
                        }
                        else
                        {
                            supportingTokens.Add(parameters);
                        }
                    }
                    if (alternative.Count == 0)
                    {
                        foreach (SecurityTokenParameters p in supportingTokens)
                        {
                            endorsing.Add(p);
                        }
                        foreach (SecurityTokenParameters p in optionalSupportingTokens)
                        {
                            optionalEndorsing.Add(p);
                        }
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

        protected bool TryImportWsspSignedEndorsingSupportingTokensAssertion(MetadataImporter importer, PolicyConversionContext policyContext, ICollection<XmlElement> assertions, Collection<SecurityTokenParameters> signedEndorsing, Collection<SecurityTokenParameters> optionalSignedEndorsing, out XmlElement assertion)
        {
            if (signedEndorsing == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("signedEndorsing");
            }
            if (optionalSignedEndorsing == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("optionalSignedEndorsing");
            }

            bool result = true;

            Collection<Collection<XmlElement>> alternatives;

            if (TryImportWsspAssertion(assertions, SignedEndorsingSupportingTokensName, out assertion)
                && TryGetNestedPolicyAlternatives(importer, assertion, out alternatives))
            {
                foreach (Collection<XmlElement> alternative in alternatives)
                {
                    MessagePartSpecification signedParts;
                    if (!TryImportWsspSignedPartsAssertion(alternative, out signedParts, out assertion) && assertion != null)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.UnsupportedSecurityPolicyAssertion, assertion.OuterXml)));
                    }

                    Collection<SecurityTokenParameters> supportingTokens = new Collection<SecurityTokenParameters>();
                    Collection<SecurityTokenParameters> optionalSupportingTokens = new Collection<SecurityTokenParameters>();
                    SecurityTokenParameters parameters;
                    bool isOptional;
                    while (alternative.Count > 0 && TryImportTokenAssertion(importer, policyContext, alternative, out parameters, out isOptional))
                    {
                        if (isOptional)
                        {
                            optionalSupportingTokens.Add(parameters);
                        }
                        else
                        {
                            supportingTokens.Add(parameters);
                        }
                    }
                    if (alternative.Count == 0)
                    {
                        foreach (SecurityTokenParameters p in supportingTokens)
                        {
                            signedEndorsing.Add(p);
                        }
                        foreach (SecurityTokenParameters p in optionalSupportingTokens)
                        {
                            optionalSignedEndorsing.Add(p);
                        }
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

        public virtual XmlElement CreateWsspEncryptSignatureAssertion(PolicyConversionContext policyContext, SecurityBindingElement binding)
        {
            MessageProtectionOrder protectionOrder;
            if (binding is SymmetricSecurityBindingElement)
            {
                protectionOrder = ((SymmetricSecurityBindingElement)binding).MessageProtectionOrder;
            }
            else
            {
                protectionOrder = ((AsymmetricSecurityBindingElement)binding).MessageProtectionOrder;
            }

            if (protectionOrder == MessageProtectionOrder.SignBeforeEncryptAndEncryptSignature 
                && ContainsEncryptionParts(policyContext, binding))
            {
                return CreateWsspAssertion(EncryptSignatureName);
            }
            else
            {
                return null;
            }
        }

        // This api checks whether or not the message will or may contain Encrypted parts
        // to decide whether or not to emit sp:EncryptSignature on Binding assertion.
        // 1) (Optional)EndpointSupporting.
        // 2) (Optional)OperationSupporting.
        // 3) In/Out/Fault Message ProtectionLevel for each Operation.
        bool ContainsEncryptionParts(PolicyConversionContext policyContext, SecurityBindingElement security)
        {
            // special case for RST/RSTR since we hard coded the security for them
            if (policyContext.Contract == NullContract)
                return true;

            if (security.EndpointSupportingTokenParameters.SignedEncrypted.Count > 0 ||
                security.OptionalEndpointSupportingTokenParameters.SignedEncrypted.Count > 0)
            {
                return true;
            }
            foreach (SupportingTokenParameters r in security.OperationSupportingTokenParameters.Values)
            {
                if (r.SignedEncrypted.Count > 0)
                {
                    return true;
                }
            }
            foreach (SupportingTokenParameters r in security.OptionalOperationSupportingTokenParameters.Values)
            {
                if (r.SignedEncrypted.Count > 0)
                {
                    return true;
                }
            }

            BindingParameterCollection bindingParameters = new BindingParameterCollection();
            bindingParameters.Add(ChannelProtectionRequirements.CreateFromContract(policyContext.Contract, policyContext.BindingElements.Find<SecurityBindingElement>().GetIndividualProperty<ISecurityCapabilities>(), false));
            ChannelProtectionRequirements protectionRequirements = SecurityBindingElement.ComputeProtectionRequirements(security, bindingParameters, policyContext.BindingElements, true);
            protectionRequirements.MakeReadOnly();

            WSSecurityPolicy sp = WSSecurityPolicy.GetSecurityPolicyDriver(security.MessageSecurityVersion);

            foreach (OperationDescription operation in policyContext.Contract.Operations)
            {
                // export policy for application messages
                foreach (MessageDescription message in operation.Messages)
                {
                    MessagePartSpecification parts;
                    ScopedMessagePartSpecification scopedParts;

                    // confidentiality
                    if (message.Direction == MessageDirection.Input)
                    {
                        scopedParts = protectionRequirements.IncomingEncryptionParts;
                    }
                    else
                    {
                        scopedParts = protectionRequirements.OutgoingEncryptionParts;
                    }

                    if (scopedParts.TryGetParts(message.Action, out parts))
                    {
                        if (!parts.IsEmpty())
                        {
                            return true;
                        }
                    }
                }

                // export policy for faults
                foreach (FaultDescription fault in operation.Faults)
                {
                    MessagePartSpecification parts;

                    // confidentiality
                    if (protectionRequirements.OutgoingEncryptionParts.TryGetParts(fault.Action, out parts))
                    {
                        if (!parts.IsEmpty())
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        public virtual XmlElement CreateWsspEncryptBeforeSigningAssertion(MessageProtectionOrder protectionOrder)
        {
            if (protectionOrder == MessageProtectionOrder.EncryptBeforeSign)
            {
                return CreateWsspAssertion(EncryptBeforeSigningName);
            }
            else
            {
                return null;
            }
        }

        public virtual XmlElement CreateWsspProtectTokensAssertion(SecurityBindingElement sbe)
        {
            if (sbe.ProtectTokens)
            {
                return CreateWsspAssertion(ProtectTokens);
            }
            else
            {
                return null;
            }
        }


        public virtual bool TryImportMessageProtectionOrderAssertions(ICollection<XmlElement> assertions, out MessageProtectionOrder order)
        {
            if (TryImportWsspAssertion(assertions, EncryptBeforeSigningName))
            {
                order = MessageProtectionOrder.EncryptBeforeSign;
            }
            else if (TryImportWsspAssertion(assertions, EncryptSignatureName))
            {
                order = MessageProtectionOrder.SignBeforeEncryptAndEncryptSignature;
            }
            else
            {
                order = MessageProtectionOrder.SignBeforeEncrypt;
            }

            return true;
        }

        public virtual XmlElement CreateWsspIncludeTimestampAssertion(bool includeTimestamp)
        {
            if (includeTimestamp)
            {
                return CreateWsspAssertion(IncludeTimestampName);
            }
            else
            {
                return null;
            }
        }

        public virtual bool TryImportWsspIncludeTimestampAssertion(ICollection<XmlElement> assertions, SecurityBindingElement binding)
        {
            binding.IncludeTimestamp = TryImportWsspAssertion(assertions, IncludeTimestampName);
            return true;
        }

        public virtual bool TryImportWsspProtectTokensAssertion(ICollection<XmlElement> assertions, out bool protectTokens)
        {
            if (TryImportWsspAssertion(assertions, ProtectTokens))
            {
                protectTokens = true;
            }
            else
            {
                protectTokens = false;
            }

            return true;
        }

        public virtual XmlElement CreateWsspLayoutAssertion(MetadataExporter exporter, SecurityHeaderLayout layout)
        {
            XmlElement result = CreateWsspAssertion(LayoutName);
            result.AppendChild(
                CreateWspPolicyWrapper(
                    exporter,
                    CreateLayoutAssertion(layout)
            ));

            return result;
        }

        public virtual bool TryImportWsspLayoutAssertion(MetadataImporter importer, ICollection<XmlElement> assertions, SecurityBindingElement binding)
        {
            bool result = false;
            XmlElement assertion;

            if (TryImportWsspAssertion(assertions, LayoutName, out assertion))
            {
                SecurityHeaderLayout layout;
                Collection<Collection<XmlElement>> alternatives;

                if (TryGetNestedPolicyAlternatives(importer, assertion, out alternatives))
                {
                    foreach (Collection<XmlElement> alternative in alternatives)
                    {
                        if (TryImportLayoutAssertion(alternative, out layout)
                            && alternative.Count == 0)
                        {
                            binding.SecurityHeaderLayout = layout;
                            result = true;
                            break;
                        }
                    }
                }
            }
            else
            {
                binding.SecurityHeaderLayout = SecurityHeaderLayout.Lax;
                result = true;
            }

            return result;
        }

        public virtual XmlElement CreateLayoutAssertion(SecurityHeaderLayout layout)
        {
            switch (layout)
            {
                default:
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("layout"));
                case SecurityHeaderLayout.Lax:
                    return CreateWsspAssertion(LaxName);
                case SecurityHeaderLayout.LaxTimestampFirst:
                    return CreateWsspAssertion(LaxTsFirstName);
                case SecurityHeaderLayout.LaxTimestampLast:
                    return CreateWsspAssertion(LaxTsLastName);
                case SecurityHeaderLayout.Strict:
                    return CreateWsspAssertion(StrictName);
            }
        }

        public virtual bool TryImportLayoutAssertion(ICollection<XmlElement> assertions, out SecurityHeaderLayout layout)
        {
            bool result = true;
            layout = SecurityHeaderLayout.Lax;

            if (TryImportWsspAssertion(assertions, LaxName))
            {
                layout = SecurityHeaderLayout.Lax;
            }
            else if (TryImportWsspAssertion(assertions, LaxTsFirstName))
            {
                layout = SecurityHeaderLayout.LaxTimestampFirst;
            }
            else if (TryImportWsspAssertion(assertions, LaxTsLastName))
            {
                layout = SecurityHeaderLayout.LaxTimestampLast;
            }
            else if (TryImportWsspAssertion(assertions, StrictName))
            {
                layout = SecurityHeaderLayout.Strict;
            }
            else
            {
                result = false;
            }

            return result;
        }

        public virtual XmlElement CreateWsspAlgorithmSuiteAssertion(MetadataExporter exporter, SecurityAlgorithmSuite suite)
        {
            XmlElement result = CreateWsspAssertion(AlgorithmSuiteName);
            result.AppendChild(
                CreateWspPolicyWrapper(
                    exporter,
                    CreateAlgorithmSuiteAssertion(suite)
            ));

            return result;
        }

        public virtual bool TryImportWsspAlgorithmSuiteAssertion(MetadataImporter importer, ICollection<XmlElement> assertions, SecurityBindingElement binding)
        {
            SecurityAlgorithmSuite suite = null;
            XmlElement assertion;
            Collection<Collection<XmlElement>> alternatives;

            if (TryImportWsspAssertion(assertions, AlgorithmSuiteName, out assertion)
                && TryGetNestedPolicyAlternatives(importer, assertion, out alternatives))
            {
                foreach (Collection<XmlElement> alternative in alternatives)
                {
                    if (TryImportAlgorithmSuiteAssertion(alternative, out suite)
                        && alternative.Count == 0)
                    {
                        binding.DefaultAlgorithmSuite = suite;
                        break;
                    }
                    else
                    {
                        suite = null;
                    }
                }
            }

            return suite != null;
        }

        public virtual XmlElement CreateAlgorithmSuiteAssertion(SecurityAlgorithmSuite suite)
        {
            if (suite == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("suite");
            }

            XmlElement result;

            if (suite == SecurityAlgorithmSuite.Basic256)
                result = CreateWsspAssertion(Basic256Name);
            else if (suite == SecurityAlgorithmSuite.Basic192)
                result = CreateWsspAssertion(Basic192Name);
            else if (suite == SecurityAlgorithmSuite.Basic128)
                result = CreateWsspAssertion(Basic128Name);
            else if (suite == SecurityAlgorithmSuite.TripleDes)
                result = CreateWsspAssertion(TripleDesName);
            else if (suite == SecurityAlgorithmSuite.Basic256Rsa15)
                result = CreateWsspAssertion(Basic256Rsa15Name);
            else if (suite == SecurityAlgorithmSuite.Basic192Rsa15)
                result = CreateWsspAssertion(Basic192Rsa15Name);
            else if (suite == SecurityAlgorithmSuite.Basic128Rsa15)
                result = CreateWsspAssertion(Basic128Rsa15Name);
            else if (suite == SecurityAlgorithmSuite.TripleDesRsa15)
                result = CreateWsspAssertion(TripleDesRsa15Name);
            else if (suite == SecurityAlgorithmSuite.Basic256Sha256)
                result = CreateWsspAssertion(Basic256Sha256Name);
            else if (suite == SecurityAlgorithmSuite.Basic192Sha256)
                result = CreateWsspAssertion(Basic192Sha256Name);
            else if (suite == SecurityAlgorithmSuite.Basic128Sha256)
                result = CreateWsspAssertion(Basic128Sha256Name);
            else if (suite == SecurityAlgorithmSuite.TripleDesSha256)
                result = CreateWsspAssertion(TripleDesSha256Name);
            else if (suite == SecurityAlgorithmSuite.Basic256Sha256Rsa15)
                result = CreateWsspAssertion(Basic256Sha256Rsa15Name);
            else if (suite == SecurityAlgorithmSuite.Basic192Sha256Rsa15)
                result = CreateWsspAssertion(Basic192Sha256Rsa15Name);
            else if (suite == SecurityAlgorithmSuite.Basic128Sha256Rsa15)
                result = CreateWsspAssertion(Basic128Sha256Rsa15Name);
            else if (suite == SecurityAlgorithmSuite.TripleDesSha256Rsa15)
                result = CreateWsspAssertion(TripleDesSha256Rsa15Name);
            else
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("suite"));
            }

            return result;
        }

        public virtual bool TryImportAlgorithmSuiteAssertion(ICollection<XmlElement> assertions, out SecurityAlgorithmSuite suite)
        {
            if (TryImportWsspAssertion(assertions, Basic256Name))
                suite = SecurityAlgorithmSuite.Basic256;
            else if (TryImportWsspAssertion(assertions, Basic192Name))
                suite = SecurityAlgorithmSuite.Basic192;
            else if (TryImportWsspAssertion(assertions, Basic128Name))
                suite = SecurityAlgorithmSuite.Basic128;
            else if (TryImportWsspAssertion(assertions, TripleDesName))
                suite = SecurityAlgorithmSuite.TripleDes;
            else if (TryImportWsspAssertion(assertions, Basic256Rsa15Name))
                suite = SecurityAlgorithmSuite.Basic256Rsa15;
            else if (TryImportWsspAssertion(assertions, Basic192Rsa15Name))
                suite = SecurityAlgorithmSuite.Basic192Rsa15;
            else if (TryImportWsspAssertion(assertions, Basic128Rsa15Name))
                suite = SecurityAlgorithmSuite.Basic128Rsa15;
            else if (TryImportWsspAssertion(assertions, TripleDesRsa15Name))
                suite = SecurityAlgorithmSuite.TripleDesRsa15;
            else if (TryImportWsspAssertion(assertions, Basic256Sha256Name))
                suite = SecurityAlgorithmSuite.Basic256Sha256;
            else if (TryImportWsspAssertion(assertions, Basic192Sha256Name))
                suite = SecurityAlgorithmSuite.Basic192Sha256;
            else if (TryImportWsspAssertion(assertions, Basic128Sha256Name))
                suite = SecurityAlgorithmSuite.Basic128Sha256;
            else if (TryImportWsspAssertion(assertions, TripleDesSha256Name))
                suite = SecurityAlgorithmSuite.TripleDesSha256;
            else if (TryImportWsspAssertion(assertions, Basic256Sha256Rsa15Name))
                suite = SecurityAlgorithmSuite.Basic256Sha256Rsa15;
            else if (TryImportWsspAssertion(assertions, Basic192Sha256Rsa15Name))
                suite = SecurityAlgorithmSuite.Basic192Sha256Rsa15;
            else if (TryImportWsspAssertion(assertions, Basic128Sha256Rsa15Name))
                suite = SecurityAlgorithmSuite.Basic128Sha256Rsa15;
            else if (TryImportWsspAssertion(assertions, TripleDesSha256Rsa15Name))
                suite = SecurityAlgorithmSuite.TripleDesSha256Rsa15;
            else
                suite = null;

            return suite != null;
        }

        public virtual XmlElement CreateWsspProtectionTokenAssertion(MetadataExporter exporter, SecurityTokenParameters parameters)
        {
            XmlElement result = CreateWsspAssertion(ProtectionTokenName);

            result.AppendChild(
                CreateWspPolicyWrapper(
                    exporter,
                    CreateTokenAssertion(exporter, parameters)
            ));
            _protectionTokenHasAsymmetricKey = parameters.HasAsymmetricKey;

            return result;
        }

        public virtual bool TryImportWsspProtectionTokenAssertion(MetadataImporter importer, PolicyConversionContext policyContext, ICollection<XmlElement> assertions, SymmetricSecurityBindingElement binding)
        {
            bool result = false;

            XmlElement assertion;
            Collection<Collection<XmlElement>> alternatives;
            if (TryImportWsspAssertion(assertions, ProtectionTokenName, out assertion)
                && TryGetNestedPolicyAlternatives(importer, assertion, out alternatives))
            {
                foreach (Collection<XmlElement> alternative in alternatives)
                {
                    SecurityTokenParameters tokenParameters;
                    bool isOptional;
                    if (TryImportTokenAssertion(importer, policyContext, alternative, out tokenParameters, out isOptional)
                        && alternative.Count == 0)
                    {
                        result = true;
                        binding.ProtectionTokenParameters = tokenParameters;
                        break;
                    }
                }
            }

            return result;
        }

        public virtual bool TryImportWsspInitiatorTokenAssertion(MetadataImporter importer, PolicyConversionContext policyContext, ICollection<XmlElement> assertions, AsymmetricSecurityBindingElement binding)
        {
            bool result = false;

            XmlElement assertion;
            Collection<Collection<XmlElement>> alternatives;
            if (TryImportWsspAssertion(assertions, InitiatorTokenName, out assertion)
                && TryGetNestedPolicyAlternatives(importer, assertion, out alternatives))
            {
                foreach (Collection<XmlElement> alternative in alternatives)
                {
                    SecurityTokenParameters tokenParameters;
                    bool isOptional;
                    if (TryImportTokenAssertion(importer, policyContext, alternative, out tokenParameters, out isOptional)
                        && alternative.Count == 0)
                    {
                        result = true;
                        binding.InitiatorTokenParameters = tokenParameters;
                        break;
                    }
                }
            }

            return result;
        }

        public virtual bool TryImportWsspRecipientTokenAssertion(MetadataImporter importer, PolicyConversionContext policyContext, ICollection<XmlElement> assertions, AsymmetricSecurityBindingElement binding)
        {
            bool result = false;

            XmlElement assertion;
            Collection<Collection<XmlElement>> alternatives;
            if (TryImportWsspAssertion(assertions, RecipientTokenName, out assertion)
                && TryGetNestedPolicyAlternatives(importer, assertion, out alternatives))
            {
                foreach (Collection<XmlElement> alternative in alternatives)
                {
                    SecurityTokenParameters tokenParameters;
                    bool isOptional;
                    if (TryImportTokenAssertion(importer, policyContext, alternative, out tokenParameters, out isOptional)
                        && alternative.Count == 0)
                    {
                        result = true;
                        binding.RecipientTokenParameters = tokenParameters;
                        break;
                    }
                }
            }

            return result;
        }

        public virtual XmlElement CreateWsspInitiatorTokenAssertion(MetadataExporter exporter, SecurityTokenParameters parameters)
        {
            XmlElement result = CreateWsspAssertion(InitiatorTokenName);

            result.AppendChild(
                CreateWspPolicyWrapper(
                    exporter,
                    CreateTokenAssertion(exporter, parameters)
            ));

            return result;
        }

        public virtual XmlElement CreateWsspRecipientTokenAssertion(MetadataExporter exporter, SecurityTokenParameters parameters)
        {
            XmlElement result = CreateWsspAssertion(RecipientTokenName);

            result.AppendChild(
                CreateWspPolicyWrapper(
                    exporter,
                    CreateTokenAssertion(exporter, parameters)
            ));

            return result;
        }

        public virtual XmlElement CreateWsspTransportTokenAssertion(MetadataExporter exporter, XmlElement transportTokenAssertion)
        {
            if (transportTokenAssertion == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("transportTokenAssertion");
            }

            XmlElement result = CreateWsspAssertion(TransportTokenName);

            result.AppendChild(
                CreateWspPolicyWrapper(
                    exporter,
                    (XmlElement)(doc.ImportNode(transportTokenAssertion, true))
            ));

            return result;
        }

        public virtual bool TryImportWsspTransportTokenAssertion(MetadataImporter importer, ICollection<XmlElement> assertions, out XmlElement transportBindingAssertion)
        {
            transportBindingAssertion = null;

            XmlElement assertion;
            Collection<Collection<XmlElement>> alternatives;
            if (TryImportWsspAssertion(assertions, TransportTokenName, out assertion)
                && TryGetNestedPolicyAlternatives(importer, assertion, out alternatives)
                && alternatives.Count == 1
                && alternatives[0].Count == 1)
            {
                // we cannot process choices of transport tokens due to the current contract between 
                // security and transport binding element converters
                transportBindingAssertion = alternatives[0][0];
            }

            return transportBindingAssertion != null;
        }

        public virtual XmlElement CreateTokenAssertion(MetadataExporter exporter, SecurityTokenParameters parameters)
        {
            return CreateTokenAssertion(exporter, parameters, false);
        }

        public virtual XmlElement CreateTokenAssertion(MetadataExporter exporter, SecurityTokenParameters parameters, bool isOptional)
        {
            if (parameters == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("parameters");
            }

            XmlElement result;

            if (parameters is KerberosSecurityTokenParameters)
            {
                result = CreateWsspKerberosTokenAssertion(exporter, (KerberosSecurityTokenParameters)parameters);
            }
            else if (parameters is X509SecurityTokenParameters)
            {
                result = CreateWsspX509TokenAssertion(exporter, (X509SecurityTokenParameters)parameters);
            }
            else if (parameters is UserNameSecurityTokenParameters)
            {
                result = CreateWsspUsernameTokenAssertion(exporter, (UserNameSecurityTokenParameters)parameters);
            }
            else if (parameters is IssuedSecurityTokenParameters)
            {
                result = CreateWsspIssuedTokenAssertion(exporter, (IssuedSecurityTokenParameters)parameters);
            }
            else if (parameters is SspiSecurityTokenParameters)
            {
                result = CreateWsspSpnegoContextTokenAssertion(exporter, (SspiSecurityTokenParameters)parameters);
            }
            else if (parameters is SslSecurityTokenParameters)
            {
                result = CreateMsspSslContextTokenAssertion(exporter, (SslSecurityTokenParameters)parameters);
            }
            else if (parameters is SecureConversationSecurityTokenParameters)
            {
                result = CreateWsspSecureConversationTokenAssertion(exporter, (SecureConversationSecurityTokenParameters)parameters);
            }
            else if (parameters is RsaSecurityTokenParameters)
            {
                result = CreateWsspRsaTokenAssertion((RsaSecurityTokenParameters)parameters);
            }
            else
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("parameters"));
            }

            if (result != null && isOptional)
            {
                result.SetAttribute(OptionalName, exporter.PolicyVersion.Namespace, TrueName);
            }

            return result;
        }

        public virtual bool TryImportTokenAssertion(MetadataImporter importer, PolicyConversionContext policyContext, Collection<XmlElement> assertions, out SecurityTokenParameters parameters, out bool isOptional)
        {
            parameters = null;
            isOptional = false;

            if (assertions.Count >= 1)
            {
                XmlElement tokenAssertion = assertions[0];
                if (TryImportWsspKerberosTokenAssertion(importer, tokenAssertion, out parameters)
                    || TryImportWsspX509TokenAssertion(importer, tokenAssertion, out parameters)
                    || TryImportWsspUsernameTokenAssertion(importer, tokenAssertion, out parameters)
                    || TryImportWsspIssuedTokenAssertion(importer, policyContext, tokenAssertion, out parameters)
                    || TryImportWsspSpnegoContextTokenAssertion(importer, tokenAssertion, out parameters)
                    || TryImportMsspSslContextTokenAssertion(importer, tokenAssertion, out parameters)
                    || TryImportWsspSecureConversationTokenAssertion(importer, tokenAssertion, out parameters)
                    || TryImportWsspRsaTokenAssertion(importer, tokenAssertion, out parameters))
                {
                    string optionalAttribute = tokenAssertion.GetAttribute(OptionalName, WspNamespace);

                    if (String.IsNullOrEmpty(optionalAttribute))
                    {
                        optionalAttribute = tokenAssertion.GetAttribute(OptionalName, Wsp15Namespace);
                    }

                    try
                    {
                        isOptional = XmlUtil.IsTrue(optionalAttribute);
                    }
                    catch ( Exception e )
                    {
                        if (Fx.IsFatal(e))
                            throw;
                        if (e is NullReferenceException)
                            throw;

                        importer.Errors.Add(new MetadataConversionError(SR.GetString(SR.UnsupportedBooleanAttribute, OptionalName, e.Message), false));
                        return false;
                    }

                    assertions.RemoveAt(0);
                }
            }

            return (parameters != null);
        }
        
        public virtual void SetIncludeTokenValue(XmlElement tokenAssertion, SecurityTokenInclusionMode inclusionMode)
        {
            switch (inclusionMode)
            {
                default:
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("inclusionMode"));
                case SecurityTokenInclusionMode.AlwaysToInitiator:
                    tokenAssertion.SetAttribute(IncludeTokenName, this.WsspNamespaceUri, AlwaysToInitiatorUri);
                    break;
                case SecurityTokenInclusionMode.AlwaysToRecipient:
                    tokenAssertion.SetAttribute(IncludeTokenName, this.WsspNamespaceUri, AlwaysToRecipientUri);
                    break;
                case SecurityTokenInclusionMode.Never:
                    tokenAssertion.SetAttribute(IncludeTokenName, this.WsspNamespaceUri, NeverUri);
                    break;
                case SecurityTokenInclusionMode.Once:
                    tokenAssertion.SetAttribute(IncludeTokenName, this.WsspNamespaceUri, OnceUri);
                    break;
            }
        }

        public virtual bool TryGetIncludeTokenValue(XmlElement assertion, out SecurityTokenInclusionMode mode)
        {
            string includeTokenUri = assertion.GetAttribute(IncludeTokenName, this.WsspNamespaceUri);

            if (includeTokenUri == AlwaysToInitiatorUri)
            {
                mode = SecurityTokenInclusionMode.AlwaysToInitiator;
                return true;
            }
            else if (includeTokenUri == AlwaysToRecipientUri)
            {
                mode = SecurityTokenInclusionMode.AlwaysToRecipient;
                return true;
            }
            else if (includeTokenUri == NeverUri)
            {
                mode = SecurityTokenInclusionMode.Never;
                return true;
            }
            else if (includeTokenUri == OnceUri)
            {
                mode = SecurityTokenInclusionMode.Once;
                return true;
            }
            else
            {
                mode = SecurityTokenInclusionMode.Never;
                return false;
            }
        }

        public virtual XmlElement CreateWsspRequireDerivedKeysAssertion(bool requireDerivedKeys)
        {
            if (requireDerivedKeys)
            {
                return CreateWsspAssertion(RequireDerivedKeysName);
            }
            else
            {
                return null;
            }
        }

        public virtual bool TryImportWsspRequireDerivedKeysAssertion(ICollection<XmlElement> assertions, SecurityTokenParameters parameters)
        {
            parameters.RequireDerivedKeys = TryImportWsspAssertion(assertions, RequireDerivedKeysName);
            return true;
        }

        public virtual XmlElement CreateWsspKerberosTokenAssertion(MetadataExporter exporter, KerberosSecurityTokenParameters parameters)
        {
            XmlElement result = CreateWsspAssertion(KerberosTokenName);
            SetIncludeTokenValue(result, parameters.InclusionMode);
            result.AppendChild(
                CreateWspPolicyWrapper(
                    exporter,
                    CreateWsspRequireDerivedKeysAssertion(parameters.RequireDerivedKeys),
                    CreateWsspAssertion(WssGssKerberosV5ApReqToken11Name)
            ));
            return result;
        }

        public virtual bool TryImportWsspKerberosTokenAssertion(MetadataImporter importer, XmlElement assertion, out SecurityTokenParameters parameters)
        {
            parameters = null;

            SecurityTokenInclusionMode inclusionMode;
            Collection<Collection<XmlElement>> alternatives;

            if (IsWsspAssertion(assertion, KerberosTokenName)
                && TryGetIncludeTokenValue(assertion, out inclusionMode))
            {
                if (TryGetNestedPolicyAlternatives(importer, assertion, out alternatives))
                {
                    foreach (Collection<XmlElement> alternative in alternatives)
                    {
                        parameters = new KerberosSecurityTokenParameters();
                        if (TryImportWsspRequireDerivedKeysAssertion(alternative, parameters)
                            && TryImportWsspAssertion(alternative, WssGssKerberosV5ApReqToken11Name, true)
                            && alternative.Count == 0)
                        {
                            parameters.InclusionMode = inclusionMode;
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
                    parameters = new KerberosSecurityTokenParameters();
                    parameters.RequireDerivedKeys = false;
                    parameters.InclusionMode = inclusionMode;
                }
            }

            return parameters != null;
        }

        public virtual XmlElement CreateX509ReferenceStyleAssertion(X509KeyIdentifierClauseType referenceStyle)
        {
            switch (referenceStyle)
            {
                default:
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("referenceStyle"));
                case X509KeyIdentifierClauseType.IssuerSerial:
                    _mustSupportRefIssuerSerialName = true;
                    return CreateWsspAssertion(RequireIssuerSerialReferenceName);
                case X509KeyIdentifierClauseType.SubjectKeyIdentifier:
                    _mustSupportRefKeyIdentifierName = true;
                    return CreateWsspAssertion(RequireKeyIdentifierReferenceName);
                case X509KeyIdentifierClauseType.Thumbprint:
                    _mustSupportRefThumbprintName = true;
                    return CreateWsspAssertion(RequireThumbprintReferenceName);
                case X509KeyIdentifierClauseType.Any:
                    _mustSupportRefIssuerSerialName = true;
                    _mustSupportRefKeyIdentifierName = true;
                    _mustSupportRefThumbprintName = true;
                    return null;
            }
        }

        public virtual bool TryImportX509ReferenceStyleAssertion(ICollection<XmlElement> assertions, X509SecurityTokenParameters parameters)
        {
            if (TryImportWsspAssertion(assertions, RequireIssuerSerialReferenceName))
            {
                parameters.X509ReferenceStyle = X509KeyIdentifierClauseType.IssuerSerial;
            }
            else if (TryImportWsspAssertion(assertions, RequireKeyIdentifierReferenceName))
            {
                parameters.X509ReferenceStyle = X509KeyIdentifierClauseType.SubjectKeyIdentifier;
            }
            else if (TryImportWsspAssertion(assertions, RequireThumbprintReferenceName))
            {
                parameters.X509ReferenceStyle = X509KeyIdentifierClauseType.Thumbprint;
            }

            return true;
        }

        public virtual XmlElement CreateWsspX509TokenAssertion(MetadataExporter exporter, X509SecurityTokenParameters parameters)
        {
            XmlElement result = CreateWsspAssertion(X509TokenName);
            SetIncludeTokenValue(result, parameters.InclusionMode);
            result.AppendChild(
                CreateWspPolicyWrapper(
                    exporter,
                    CreateWsspRequireDerivedKeysAssertion(parameters.RequireDerivedKeys),
                    CreateX509ReferenceStyleAssertion(parameters.X509ReferenceStyle),
                    CreateWsspAssertion(WssX509V3Token10Name)
            ));
            return result;
        }

        public virtual bool TryImportWsspX509TokenAssertion(MetadataImporter importer, XmlElement assertion, out SecurityTokenParameters parameters)
        {
            parameters = null;

            SecurityTokenInclusionMode inclusionMode;
            Collection<Collection<XmlElement>> alternatives;

            if (IsWsspAssertion(assertion, X509TokenName)
                && TryGetIncludeTokenValue(assertion, out inclusionMode))
            {
                if (TryGetNestedPolicyAlternatives(importer, assertion, out alternatives))
                {
                    foreach (Collection<XmlElement> alternative in alternatives)
                    {
                        X509SecurityTokenParameters x509 = new X509SecurityTokenParameters();
                        parameters = x509;
                        if (TryImportWsspRequireDerivedKeysAssertion(alternative, x509)
                            && TryImportX509ReferenceStyleAssertion(alternative, x509)
                            && TryImportWsspAssertion(alternative, WssX509V3Token10Name, true)
                            && alternative.Count == 0)
                        {
                            parameters.InclusionMode = inclusionMode;
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
                    parameters = new X509SecurityTokenParameters();
                    parameters.RequireDerivedKeys = false;
                    parameters.InclusionMode = inclusionMode;
                }
            }

            return parameters != null;
        }

        public virtual XmlElement CreateWsspUsernameTokenAssertion(MetadataExporter exporter, UserNameSecurityTokenParameters parameters)
        {
            XmlElement result = CreateWsspAssertion(UsernameTokenName);
            SetIncludeTokenValue(result, parameters.InclusionMode);
            result.AppendChild(
                CreateWspPolicyWrapper(
                    exporter,
                    CreateWsspAssertion(WssUsernameToken10Name)
            ));
            return result;
        }

        public virtual bool TryImportWsspUsernameTokenAssertion(MetadataImporter importer, XmlElement assertion, out SecurityTokenParameters parameters)
        {
            parameters = null;

            SecurityTokenInclusionMode inclusionMode;
            Collection<Collection<XmlElement>> alternatives;

            if (IsWsspAssertion(assertion, UsernameTokenName)
                && TryGetIncludeTokenValue(assertion, out inclusionMode))
            {
                if (TryGetNestedPolicyAlternatives(importer, assertion, out alternatives))
                {
                    foreach (Collection<XmlElement> alternative in alternatives)
                    {
                        if (TryImportWsspAssertion(alternative, WssUsernameToken10Name)
                            && alternative.Count == 0)
                        {
                            parameters = new UserNameSecurityTokenParameters();
                            parameters.InclusionMode = inclusionMode;
                            break;
                        }
                    }
                }
                else
                {
                    parameters = new UserNameSecurityTokenParameters();
                    parameters.InclusionMode = inclusionMode;
                }
            }

            return parameters != null;
        }

        public virtual XmlElement CreateWsspRsaTokenAssertion(RsaSecurityTokenParameters parameters)
        {
            XmlElement result = CreateMsspAssertion(RsaTokenName);
            SetIncludeTokenValue(result, parameters.InclusionMode);
            return result;
        }

        public virtual bool TryImportWsspRsaTokenAssertion(MetadataImporter importer, XmlElement assertion, out SecurityTokenParameters parameters)
        {
            parameters = null;

            SecurityTokenInclusionMode inclusionMode;
            Collection<Collection<XmlElement>> alternatives;

            if (IsMsspAssertion(assertion, RsaTokenName)
                && TryGetIncludeTokenValue(assertion, out inclusionMode)
                && TryGetNestedPolicyAlternatives(importer, assertion, out alternatives) == false)
            {
                parameters = new RsaSecurityTokenParameters();
                parameters.InclusionMode = inclusionMode;
            }

            return parameters != null;
        }

        public virtual XmlElement CreateReferenceStyleAssertion(SecurityTokenReferenceStyle referenceStyle)
        {
            switch (referenceStyle)
            {
                default:
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("referenceStyle"));
                case SecurityTokenReferenceStyle.External:
                    return CreateWsspAssertion(RequireExternalReferenceName);
                case SecurityTokenReferenceStyle.Internal:
                    return CreateWsspAssertion(RequireInternalReferenceName);
            }
        }

        public virtual bool TryImportReferenceStyleAssertion(ICollection<XmlElement> assertions, IssuedSecurityTokenParameters parameters)
        {
            if (TryImportWsspAssertion(assertions, RequireExternalReferenceName))
            {
                parameters.ReferenceStyle = SecurityTokenReferenceStyle.External;
            }
            else if (TryImportWsspAssertion(assertions, RequireInternalReferenceName))
            {
                parameters.ReferenceStyle = SecurityTokenReferenceStyle.Internal;
            }

            return true;
        }

        public virtual XmlElement CreateWsspIssuerElement(EndpointAddress issuerAddress, EndpointAddress issuerMetadataAddress)
        {
            XmlElement result;
            if (issuerAddress == null && issuerMetadataAddress == null)
            {
                result = null;
            }
            else
            {
                EndpointAddress addressToSerialize;
                addressToSerialize = issuerAddress == null ? EndpointAddress.AnonymousAddress : issuerAddress;

                MemoryStream stream;
                XmlWriter writer;

                if (issuerMetadataAddress != null)
                {
                    MetadataSet metadataSet = new MetadataSet();
                    metadataSet.MetadataSections.Add(new MetadataSection(null, null, new MetadataReference(issuerMetadataAddress, AddressingVersion.WSAddressing10)));

                    stream = new MemoryStream();
                    writer = new XmlTextWriter(stream, System.Text.Encoding.UTF8);
                    metadataSet.WriteTo(XmlDictionaryWriter.CreateDictionaryWriter(writer));
                    writer.Flush();
                    stream.Seek(0, SeekOrigin.Begin);

                    addressToSerialize = new EndpointAddress(
                        addressToSerialize.Uri,
                        addressToSerialize.Identity,
                        addressToSerialize.Headers,
                        XmlDictionaryReader.CreateDictionaryReader(XmlReader.Create(stream)),
                        addressToSerialize.GetReaderAtExtensions());
                }

                stream = new MemoryStream();
                writer = new XmlTextWriter(stream, System.Text.Encoding.UTF8);
                writer.WriteStartElement(IssuerName, this.WsspNamespaceUri);
                addressToSerialize.WriteContentsTo(AddressingVersion.WSAddressing10, writer);
                writer.WriteEndElement();
                writer.Flush();
                stream.Seek(0, SeekOrigin.Begin);
                result = (XmlElement)doc.ReadNode(new XmlTextReader(stream) { DtdProcessing = DtdProcessing.Prohibit });
            }
            return result;
        }

        public virtual bool TryGetIssuer(XmlElement assertion, out EndpointAddress issuer, out EndpointAddress issuerMetadata)
        {
            bool result = true;
            issuer = null;
            issuerMetadata = null;

            foreach (XmlNode node in assertion.ChildNodes)
            {
                if (node is XmlElement && IsWsspAssertion((XmlElement)node, IssuerName))
                {
                    try
                    {
                        issuer = EndpointAddress.ReadFrom(XmlDictionaryReader.CreateDictionaryReader(new XmlNodeReader(node)));
                        XmlDictionaryReader metadataReader = issuer.GetReaderAtMetadata();
                        if (metadataReader != null)
                        {
                            while (metadataReader.MoveToContent() == XmlNodeType.Element)
                            {
                                if (metadataReader.LocalName == MetadataStrings.MetadataExchangeStrings.Metadata
                                    && metadataReader.NamespaceURI == MetadataStrings.MetadataExchangeStrings.Namespace)
                                {
                                    MetadataSet metadataSet = MetadataSet.ReadFrom(metadataReader);
                                    foreach (MetadataSection section in metadataSet.MetadataSections)
                                    {
                                        if (section.Metadata is MetadataReference)
                                        {
                                            issuerMetadata = ((MetadataReference)section.Metadata).Address;
                                        }
                                    }

                                    break;
                                }
                                else
                                {
                                    metadataReader.Skip();
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        if (Fx.IsFatal(e))
                            throw;
                        if (e is NullReferenceException)
                            throw;
                        result = false;
                    }
                    break;
                }
            }

            return result;
        }

        public virtual XmlElement CreateWsspIssuedTokenAssertion(MetadataExporter exporter, IssuedSecurityTokenParameters parameters)
        {
            XmlElement result = CreateWsspAssertion(IssuedTokenName);
            SetIncludeTokenValue(result, parameters.InclusionMode);
            XmlElement issuerAssertion = CreateWsspIssuerElement(parameters.IssuerAddress, parameters.IssuerMetadataAddress);
            if (issuerAssertion != null)
            {
                result.AppendChild(issuerAssertion);
            }
            XmlElement tokenTemplate = CreateWsspAssertion(RequestSecurityTokenTemplateName);
            TrustDriver driver = this.TrustDriver;
            foreach (XmlElement p in parameters.CreateRequestParameters(driver))
            {
                tokenTemplate.AppendChild(doc.ImportNode(p, true));
            }
            result.AppendChild(tokenTemplate);
            result.AppendChild(
                CreateWspPolicyWrapper(
                    exporter,
                    CreateWsspRequireDerivedKeysAssertion(parameters.RequireDerivedKeys),
                    CreateReferenceStyleAssertion(parameters.ReferenceStyle)
            ));
            return result;
        }

        public virtual bool TryGetRequestSecurityTokenTemplate(XmlElement assertion, out Collection<XmlElement> requestParameters)
        {
            requestParameters = null;

            foreach (XmlNode node in assertion.ChildNodes)
            {
                if (node is XmlElement && IsWsspAssertion((XmlElement)node, RequestSecurityTokenTemplateName))
                {
                    requestParameters = new Collection<XmlElement>();
                    foreach (XmlNode p in node.ChildNodes)
                    {
                        if (p is XmlElement)
                        {
                            requestParameters.Add((XmlElement)p);
                        }
                    }
                }
            }

            return requestParameters != null;
        }

        public virtual bool TryImportWsspIssuedTokenAssertion(MetadataImporter importer, PolicyConversionContext policyContext, XmlElement assertion, out SecurityTokenParameters parameters)
        {
            parameters = null;

            SecurityTokenInclusionMode inclusionMode;
            Collection<Collection<XmlElement>> alternatives;
            EndpointAddress issuer;
            EndpointAddress issuerMetadata;
            Collection<XmlElement> requestSecurityTokenTemplate;

            if (IsWsspAssertion(assertion, IssuedTokenName)
                && TryGetIncludeTokenValue(assertion, out inclusionMode)
                && TryGetIssuer(assertion, out issuer, out issuerMetadata)
                && TryGetRequestSecurityTokenTemplate(assertion, out requestSecurityTokenTemplate))
            {
                if (TryGetNestedPolicyAlternatives(importer, assertion, out alternatives))
                {
                    foreach (Collection<XmlElement> alternative in alternatives)
                    {
                        IssuedSecurityTokenParameters issued = new IssuedSecurityTokenParameters();
                        parameters = issued;
                        if (TryImportWsspRequireDerivedKeysAssertion(alternative, issued)
                            && TryImportReferenceStyleAssertion(alternative, issued)
                            && alternative.Count == 0)
                        {
                            issued.InclusionMode = inclusionMode;
                            issued.IssuerAddress = issuer;
                            issued.IssuerMetadataAddress = issuerMetadata;
                            issued.SetRequestParameters(requestSecurityTokenTemplate, this.TrustDriver);

                            TokenIssuerPolicyResolver policyResolver = new TokenIssuerPolicyResolver(this.TrustDriver);
                            policyResolver.ResolveTokenIssuerPolicy(importer, policyContext, issued);

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
                    IssuedSecurityTokenParameters issued = new IssuedSecurityTokenParameters();
                    parameters = issued;
                    issued.InclusionMode = inclusionMode;
                    issued.IssuerAddress = issuer;
                    issued.IssuerMetadataAddress = issuerMetadata;
                    issued.SetRequestParameters(requestSecurityTokenTemplate, this.TrustDriver);
                    issued.RequireDerivedKeys = false;
                }
            }

            return parameters != null;
        }

        public virtual XmlElement CreateWsspMustNotSendCancelAssertion(bool requireCancel)
        {
            if (!requireCancel)
            {
                XmlElement result = CreateWsspAssertion(MustNotSendCancelName);
                return result;
            }
            else
            {
                return null;
            }
        }

        public virtual bool TryImportWsspMustNotSendCancelAssertion(ICollection<XmlElement> assertions, out bool requireCancellation)
        {
            requireCancellation = !TryImportWsspAssertion(assertions, MustNotSendCancelName);
            return true;
        }

        public virtual XmlElement CreateWsspSpnegoContextTokenAssertion(MetadataExporter exporter, SspiSecurityTokenParameters parameters)
        {
            XmlElement result = CreateWsspAssertion(SpnegoContextTokenName);
            SetIncludeTokenValue(result, parameters.InclusionMode);
            result.AppendChild(
                CreateWspPolicyWrapper(
                    exporter,
                    CreateWsspRequireDerivedKeysAssertion(parameters.RequireDerivedKeys),
                    CreateWsspMustNotSendCancelAssertion(parameters.RequireCancellation)
            ));
            return result;
        }

        public virtual bool TryImportWsspSpnegoContextTokenAssertion(MetadataImporter importer, XmlElement assertion, out SecurityTokenParameters parameters)
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
                        if (TryImportWsspRequireDerivedKeysAssertion(alternative, sspi)
                            && TryImportWsspMustNotSendCancelAssertion(alternative, out requireCancellation)
                            && alternative.Count == 0)
                        {
                            sspi.RequireCancellation = requireCancellation;
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

        public abstract XmlElement CreateWsspHttpsTokenAssertion(MetadataExporter exporter, HttpsTransportBindingElement httpsBinding);

        public abstract bool TryImportWsspHttpsTokenAssertion(MetadataImporter importer, ICollection<XmlElement> assertions, HttpsTransportBindingElement httpsBinding);

        public virtual bool ContainsWsspHttpsTokenAssertion(ICollection<XmlElement> assertions)
        {
            return (PolicyConversionContext.FindAssertion(assertions, HttpsTokenName, this.WsspNamespaceUri, false) != null);
        }

        public virtual XmlElement CreateMsspRequireClientCertificateAssertion(bool requireClientCertificate)
        {
            if (requireClientCertificate)
            {
                return CreateMsspAssertion(RequireClientCertificateName);
            }
            else
            {
                return null;
            }
        }

        public virtual bool TryImportMsspRequireClientCertificateAssertion(ICollection<XmlElement> assertions, SslSecurityTokenParameters parameters)
        {
            parameters.RequireClientCertificate = TryImportMsspAssertion(assertions, RequireClientCertificateName);
            return true;
        }

        public virtual XmlElement CreateMsspSslContextTokenAssertion(MetadataExporter exporter, SslSecurityTokenParameters parameters)
        {
            XmlElement result = CreateMsspAssertion(SslContextTokenName);
            SetIncludeTokenValue(result, parameters.InclusionMode);
            result.AppendChild(
                CreateWspPolicyWrapper(
                    exporter,
                    CreateWsspRequireDerivedKeysAssertion(parameters.RequireDerivedKeys),
                    CreateWsspMustNotSendCancelAssertion(parameters.RequireCancellation),
                    CreateMsspRequireClientCertificateAssertion(parameters.RequireClientCertificate)
            ));
            return result;
        }

        public virtual bool TryImportMsspSslContextTokenAssertion(MetadataImporter importer, XmlElement assertion, out SecurityTokenParameters parameters)
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
                        if (TryImportWsspRequireDerivedKeysAssertion(alternative, ssl)
                            && TryImportWsspMustNotSendCancelAssertion(alternative, out requireCancellation)
                            && TryImportMsspRequireClientCertificateAssertion(alternative, ssl)
                            && alternative.Count == 0)
                        {
                            ssl.RequireCancellation = requireCancellation;
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

        public virtual XmlElement CreateWsspBootstrapPolicyAssertion(MetadataExporter exporter, SecurityBindingElement bootstrapSecurity)
        {
            if (bootstrapSecurity == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("bootstrapBinding");

            WSSecurityPolicy sp = WSSecurityPolicy.GetSecurityPolicyDriver(bootstrapSecurity.MessageSecurityVersion);

            // create complete bootstrap binding

            CustomBinding bootstrapBinding = new CustomBinding(bootstrapSecurity);
            if (exporter.State.ContainsKey(SecurityPolicyStrings.SecureConversationBootstrapBindingElementsBelowSecurityKey))
            {
                BindingElementCollection bindingElementsBelowSecurity = exporter.State[SecurityPolicyStrings.SecureConversationBootstrapBindingElementsBelowSecurityKey] as BindingElementCollection;
                if (bindingElementsBelowSecurity != null)
                {
                    foreach (BindingElement be in bindingElementsBelowSecurity)
                    {
                        bootstrapBinding.Elements.Add(be);
                    }
                }
            }

            // generate policy for the "how" of security 

            ServiceEndpoint bootstrapEndpoint = new ServiceEndpoint(NullContract);
            bootstrapEndpoint.Binding = bootstrapBinding;
            PolicyConversionContext policyContext = exporter.ExportPolicy(bootstrapEndpoint);

            // generate policy for the "what" of security (protection assertions)

            // hard-coded requirements in V1: sign and encrypt RST and RSTR body
            ChannelProtectionRequirements bootstrapProtection = new ChannelProtectionRequirements();
            bootstrapProtection.IncomingEncryptionParts.AddParts(new MessagePartSpecification(true));
            bootstrapProtection.OutgoingEncryptionParts.AddParts(new MessagePartSpecification(true));
            bootstrapProtection.IncomingSignatureParts.AddParts(new MessagePartSpecification(true));
            bootstrapProtection.OutgoingSignatureParts.AddParts(new MessagePartSpecification(true));

            // add boostrap binding protection requirements (e.g. addressing headers)
            ChannelProtectionRequirements cpr = bootstrapBinding.GetProperty<ChannelProtectionRequirements>(new BindingParameterCollection());
            if (cpr != null)
            {
                bootstrapProtection.Add(cpr);
            }

            // extract channel-scope protection requirements and union them across request and response
            MessagePartSpecification encryption = new MessagePartSpecification();
            encryption.Union(bootstrapProtection.IncomingEncryptionParts.ChannelParts);
            encryption.Union(bootstrapProtection.OutgoingEncryptionParts.ChannelParts);
            encryption.MakeReadOnly();
            MessagePartSpecification signature = new MessagePartSpecification();
            signature.Union(bootstrapProtection.IncomingSignatureParts.ChannelParts);
            signature.Union(bootstrapProtection.OutgoingSignatureParts.ChannelParts);
            signature.MakeReadOnly();

            // create final boostrap policy assertion

            XmlElement nestedPolicy = CreateWspPolicyWrapper(
                    exporter,
                    sp.CreateWsspSignedPartsAssertion(signature),
                    sp.CreateWsspEncryptedPartsAssertion(encryption));
            foreach (XmlElement e in sp.FilterWsspPolicyAssertions(policyContext.GetBindingAssertions()))
            {
                nestedPolicy.AppendChild(e);
            }
            XmlElement result = CreateWsspAssertion(BootstrapPolicyName);
            result.AppendChild(nestedPolicy);

            return result;
        }

        public virtual ICollection<XmlElement> FilterWsspPolicyAssertions(ICollection<XmlElement> policyAssertions)
        {
            Collection<XmlElement> result = new Collection<XmlElement>();

            foreach (XmlElement assertion in policyAssertions)
                if (IsWsspAssertion(assertion))
                    result.Add(assertion);

            return result;
        }

        public virtual bool TryImportWsspBootstrapPolicyAssertion(MetadataImporter importer, ICollection<XmlElement> assertions, SecureConversationSecurityTokenParameters parameters)
        {
            bool result = false;

            XmlElement assertion;
            Collection<Collection<XmlElement>> alternatives;

            if (TryImportWsspAssertion(assertions, BootstrapPolicyName, out assertion)
                && TryGetNestedPolicyAlternatives(importer, assertion, out alternatives))
            {
                BindingElementCollection bindingElements;
                importer.State[SecurityBindingElementImporter.InSecureConversationBootstrapBindingImportMode] = SecurityBindingElementImporter.InSecureConversationBootstrapBindingImportMode;
                try
                {
                    bindingElements = importer.ImportPolicy(NullServiceEndpoint, alternatives);
                    if (importer.State.ContainsKey(SecurityBindingElementImporter.SecureConversationBootstrapEncryptionRequirements))
                    {
                        MessagePartSpecification encryption = (MessagePartSpecification)importer.State[SecurityBindingElementImporter.SecureConversationBootstrapEncryptionRequirements];
                        if (encryption.IsBodyIncluded != true)
                        {
                            importer.Errors.Add(new MetadataConversionError(SR.GetString(SR.UnsupportedSecureConversationBootstrapProtectionRequirements), false));
                            bindingElements = null;
                        }
                    }
                    if (importer.State.ContainsKey(SecurityBindingElementImporter.SecureConversationBootstrapSignatureRequirements))
                    {
                        MessagePartSpecification signature = (MessagePartSpecification)importer.State[SecurityBindingElementImporter.SecureConversationBootstrapSignatureRequirements];
                        if (signature.IsBodyIncluded != true)
                        {
                            importer.Errors.Add(new MetadataConversionError(SR.GetString(SR.UnsupportedSecureConversationBootstrapProtectionRequirements), false));
                            bindingElements = null;
                        }
                    }
                }
                finally
                {
                    importer.State.Remove(SecurityBindingElementImporter.InSecureConversationBootstrapBindingImportMode);
                    if (importer.State.ContainsKey(SecurityBindingElementImporter.SecureConversationBootstrapEncryptionRequirements))
                        importer.State.Remove(SecurityBindingElementImporter.SecureConversationBootstrapEncryptionRequirements);
                    if (importer.State.ContainsKey(SecurityBindingElementImporter.SecureConversationBootstrapSignatureRequirements))
                        importer.State.Remove(SecurityBindingElementImporter.SecureConversationBootstrapSignatureRequirements);
                }
                if (bindingElements != null)
                {
                    parameters.BootstrapSecurityBindingElement = bindingElements.Find<SecurityBindingElement>();
                    return true;
                }
                else
                {
                    parameters.BootstrapSecurityBindingElement = null;
                    return true; // Consider returning false here.
                }
            }

            return result;
        }

        public virtual XmlElement CreateWsspSecureConversationTokenAssertion(MetadataExporter exporter, SecureConversationSecurityTokenParameters parameters)
        {
            XmlElement result = CreateWsspAssertion(SecureConversationTokenName);
            SetIncludeTokenValue(result, parameters.InclusionMode);
            result.AppendChild(
                CreateWspPolicyWrapper(
                    exporter,
                    CreateWsspRequireDerivedKeysAssertion(parameters.RequireDerivedKeys),
                    CreateWsspMustNotSendCancelAssertion(parameters.RequireCancellation),
                    CreateWsspBootstrapPolicyAssertion(exporter, parameters.BootstrapSecurityBindingElement)
            ));
            return result;
        }

        public virtual bool TryImportWsspSecureConversationTokenAssertion(MetadataImporter importer, XmlElement assertion, out SecurityTokenParameters parameters)
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
                        if (TryImportWsspRequireDerivedKeysAssertion(alternative, sc)
                            && TryImportWsspMustNotSendCancelAssertion(alternative, out requireCancellation)
                            && TryImportWsspBootstrapPolicyAssertion(importer, alternative, sc)
                            && alternative.Count == 0)
                        {
                            sc.RequireCancellation = requireCancellation;
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

        class TokenIssuerPolicyResolver
        {
            const string WSIdentityNamespace = @"http://schemas.xmlsoap.org/ws/2005/05/identity";
            static readonly Uri SelfIssuerUri = new Uri(WSIdentityNamespace + "/issuer/self");

            TrustDriver trustDriver;

            public TokenIssuerPolicyResolver(TrustDriver driver)
            {
                this.trustDriver = driver;
            }

            public void ResolveTokenIssuerPolicy(MetadataImporter importer, PolicyConversionContext policyContext, IssuedSecurityTokenParameters parameters)
            {
                if (policyContext == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("policyContext");
                }
                if (parameters == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("parameters");
                }

                EndpointAddress mexAddress = (parameters.IssuerMetadataAddress != null) ? parameters.IssuerMetadataAddress : parameters.IssuerAddress;
                if (mexAddress == null || mexAddress.IsAnonymous || mexAddress.Uri.Equals(SelfIssuerUri))
                {
                    return;
                }
                int maximumRedirections = (int)importer.State[SecurityBindingElementImporter.MaxPolicyRedirectionsKey];

                if (maximumRedirections <= 0)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.MaximumPolicyRedirectionsExceeded)));
                }
                --maximumRedirections;

                //
                // Try to retrieve the proxy from the importer.State bag so that we can have secure mex
                // and it fails, then we can create a default one
                //
                MetadataExchangeClient policyFetcher = null;
                if ((importer.State != null) && (importer.State.ContainsKey(MetadataExchangeClient.MetadataExchangeClientKey)))
                {
                    policyFetcher = importer.State[MetadataExchangeClient.MetadataExchangeClientKey] as MetadataExchangeClient;
                }

                if (policyFetcher == null)
                    policyFetcher = new MetadataExchangeClient(mexAddress);

                ServiceEndpointCollection federationEndpoints = null;
                MetadataSet metadataSet = null;
                Exception mexException = null;
                try
                {
                    metadataSet = policyFetcher.GetMetadata(mexAddress);
                }
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                        throw;
                    if (e is NullReferenceException)
                        throw;

                    mexException = e;
                }

                //
                // DCR 6729: Try the http get option here if mex failed.
                //
                if (metadataSet == null )
                {
                    try
                    {
                        metadataSet = policyFetcher.GetMetadata(mexAddress.Uri, MetadataExchangeClientMode.HttpGet);
                    }
                    catch (Exception e)
                    {
                        if (Fx.IsFatal(e))
                            throw;
                        if (e is NullReferenceException)
                            throw;

                        if (mexException == null)
                            mexException = e;
                    }
                }

                if (metadataSet == null)
                {
                    //
                    // we could not retrieve the metadata from the issuer for some reason
                    //
                    if (mexException != null)
                        importer.Errors.Add(new MetadataConversionError(SR.GetString(SR.UnableToObtainIssuerMetadata, mexAddress, mexException), false));
   
                    return;
                }
                WsdlImporter wsdlImporter;
                // NOTE: Microsoft, Policy import/export is seperate from WSDL however, this policy importer
                //      invokes the WsdlImporter. In the event that the current MetadataImporter is a WsdlImporter,
                //      we should use it's collection of extensions for the import process. Other wise
                WsdlImporter currentWsdlImporter = importer as WsdlImporter;
                if (currentWsdlImporter != null)
                {
                    wsdlImporter = new WsdlImporter(metadataSet, importer.PolicyImportExtensions, currentWsdlImporter.WsdlImportExtensions);
                }
                else
                {
                    wsdlImporter = new WsdlImporter(metadataSet, importer.PolicyImportExtensions, null);
                }

                //
                // Copy the State from the first importer to the second one so that the state can be passed to the second round wsdl retrieval
                //
                if ((importer.State != null) && (importer.State.ContainsKey(MetadataExchangeClient.MetadataExchangeClientKey)))
                {
                    wsdlImporter.State.Add(MetadataExchangeClient.MetadataExchangeClientKey, importer.State[MetadataExchangeClient.MetadataExchangeClientKey]);
                }

                wsdlImporter.State.Add(SecurityBindingElementImporter.MaxPolicyRedirectionsKey, maximumRedirections);

                federationEndpoints = wsdlImporter.ImportAllEndpoints();

                // copy all the import errors into the current metadata importer
                for (int i = 0; i < wsdlImporter.Errors.Count; ++i)
                {
                    MetadataConversionError error = wsdlImporter.Errors[i];
                    importer.Errors.Add(new MetadataConversionError(SR.GetString(SR.ErrorImportingIssuerMetadata, mexAddress, InsertEllipsisIfTooLong(error.Message)), error.IsWarning));
                }

                if (federationEndpoints != null)
                {
                    AddCompatibleFederationEndpoints(federationEndpoints, parameters);
                    if (parameters.AlternativeIssuerEndpoints != null && parameters.AlternativeIssuerEndpoints.Count > 0)
                    {
                        importer.Errors.Add(new MetadataConversionError(SR.GetString(SR.MultipleIssuerEndpointsFound, mexAddress)));
                    }
                }
            }

            static string InsertEllipsisIfTooLong(string message)
            {
                const int MaxLength = 1024;
                const string Ellipsis = "....";

                if (message != null && message.Length > MaxLength)
                {
                    return String.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}{1}{2}",
                        message.Substring(0, (MaxLength - Ellipsis.Length) / 2),
                        Ellipsis,
                        message.Substring(message.Length - (MaxLength - Ellipsis.Length) / 2));
                }
                return message;
            }

            void AddCompatibleFederationEndpoints(ServiceEndpointCollection serviceEndpoints, IssuedSecurityTokenParameters parameters)
            {
                // check if an explicit issuer address has been specified. If so,add the endpoint corresponding to that address only. If not add all acceptable endpoints.

                bool isIssuerSpecified = (parameters.IssuerAddress != null && !parameters.IssuerAddress.IsAnonymous);
                foreach (ServiceEndpoint endpoint in serviceEndpoints)
                {
                    TrustDriver trustDriver;
                    if (!TryGetTrustDriver(endpoint, out trustDriver))
                    {
                        // if endpoint does not have trustDriver, assume
                        // parent trustDriver.
                        trustDriver = this.trustDriver;
                    }
                    bool isFederationContract = false;
                    ContractDescription contract = endpoint.Contract;
                    for (int j = 0; j < contract.Operations.Count; ++j)
                    {
                        OperationDescription operation = contract.Operations[j];
                        bool hasIncomingRst = false;
                        bool hasOutgoingRstr = false;
                        for (int k = 0; k < operation.Messages.Count; ++k)
                        {
                            MessageDescription message = operation.Messages[k];
                            if (message.Action == trustDriver.RequestSecurityTokenAction.Value && message.Direction == MessageDirection.Input)
                            {
                                hasIncomingRst = true;
                            }
                            else if ((((trustDriver.StandardsManager.TrustVersion == TrustVersion.WSTrustFeb2005) && (message.Action == trustDriver.RequestSecurityTokenResponseAction.Value)) || 
                                ((trustDriver.StandardsManager.TrustVersion == TrustVersion.WSTrust13) && (message.Action == trustDriver.RequestSecurityTokenResponseFinalAction.Value))) && 
                                message.Direction == MessageDirection.Output)
                            {
                                hasOutgoingRstr = true;
                            }
                        }
                        if (hasIncomingRst && hasOutgoingRstr)
                        {
                            isFederationContract = true;
                            break;
                        }
                    }
                    if (isFederationContract)
                    {
                        // skip if it is not an acceptable endpoint
                        if (isIssuerSpecified && !parameters.IssuerAddress.Uri.Equals(endpoint.Address.Uri))
                        {
                            continue;
                        }

                        if (parameters.IssuerBinding == null)
                        {
                            parameters.IssuerAddress = endpoint.Address;
                            parameters.IssuerBinding = endpoint.Binding;
                        }
                        else
                        {
                            IssuedSecurityTokenParameters.AlternativeIssuerEndpoint endpointInfo = new IssuedSecurityTokenParameters.AlternativeIssuerEndpoint();
                            endpointInfo.IssuerAddress = endpoint.Address;
                            endpointInfo.IssuerBinding = endpoint.Binding;
                            parameters.AlternativeIssuerEndpoints.Add(endpointInfo);
                        }
                    }
                }
            }

            bool TryGetTrustDriver(ServiceEndpoint endpoint, out TrustDriver trustDriver)
            {
                SecurityBindingElement sbe = endpoint.Binding.CreateBindingElements().Find<SecurityBindingElement>();
                trustDriver = null;
                if (sbe != null)
                {
                    MessageSecurityVersion messageSecurityVersion = sbe.MessageSecurityVersion;
                    if (messageSecurityVersion.TrustVersion == TrustVersion.WSTrustFeb2005)
                    {
                        trustDriver = new WSTrustFeb2005.DriverFeb2005(new SecurityStandardsManager(messageSecurityVersion, WSSecurityTokenSerializer.DefaultInstance));
                    }
                    else if (messageSecurityVersion.TrustVersion == TrustVersion.WSTrust13)
                    {
                        trustDriver = new WSTrustDec2005.DriverDec2005(new SecurityStandardsManager(messageSecurityVersion, WSSecurityTokenSerializer.DefaultInstance));
                    }
                }
                return trustDriver != null;
            }
        }

        public static bool TryGetSecurityPolicyDriver(ICollection<XmlElement> assertions, out WSSecurityPolicy securityPolicy)
        {
            SecurityPolicyManager policyManager = new SecurityPolicyManager();
            return policyManager.TryGetSecurityPolicyDriver(assertions, out securityPolicy); 
        }

        public static WSSecurityPolicy GetSecurityPolicyDriver(MessageSecurityVersion version)
        {
            SecurityPolicyManager policyManager = new SecurityPolicyManager();
            return policyManager.GetSecurityPolicyDriver(version);
        }

        class SecurityPolicyManager
        {
            List<WSSecurityPolicy> drivers;

            public SecurityPolicyManager()
            {
                this.drivers = new List<WSSecurityPolicy>();
                Initialize();
            }

            public void Initialize()
            {
                this.drivers.Add(new WSSecurityPolicy11());
                this.drivers.Add(new WSSecurityPolicy12());
            }

            public bool TryGetSecurityPolicyDriver(ICollection<XmlElement> assertions, out WSSecurityPolicy securityPolicy)
            {
                securityPolicy = null;

                for (int i = 0; i < this.drivers.Count; ++i)
                {
                    if (this.drivers[i].CanImportAssertion(assertions))
                    {
                        securityPolicy = this.drivers[i];
                        return true;
                    }
                }

                return false;
            }

            public WSSecurityPolicy GetSecurityPolicyDriver(MessageSecurityVersion version)
            {
                for (int i = 0; i < this.drivers.Count; ++i)
                {
                    if (this.drivers[i].IsSecurityVersionSupported(version))
                    {
                        return this.drivers[i];
                    }
                }

                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException());
            }

        }
    }

    static class SecurityPolicyStrings
    {
        public const string SecureConversationBootstrapBindingElementsBelowSecurityKey = "SecureConversationBootstrapBindingElementsBelowSecurityKey";
    }

}
