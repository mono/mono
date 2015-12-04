//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Security
{
    using System;
    using System.ServiceModel;
    using System.ServiceModel.Description;
    using System.ServiceModel.Dispatcher;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Text;
    using System.Threading;
    using System.Xml;
    using System.Runtime;
    using System.Security.Cryptography;
    using System.IdentityModel.Claims;
    using System.IdentityModel.Policy;
    using System.IdentityModel.Selectors;
    using System.IdentityModel.Tokens;
    using System.Security.Cryptography.X509Certificates;
    using System.ServiceModel.Security.Tokens;
    using HexBinary = System.Runtime.Remoting.Metadata.W3cXsd2001.SoapHexBinary;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Security;
    using System.Runtime.Serialization;

    using KeyIdentifierEntry = WSSecurityTokenSerializer.KeyIdentifierEntry;
    using KeyIdentifierClauseEntry = WSSecurityTokenSerializer.KeyIdentifierClauseEntry;
    using TokenEntry = WSSecurityTokenSerializer.TokenEntry;
    using StrEntry = WSSecurityTokenSerializer.StrEntry;
    using Psha1DerivedKeyGenerator = System.IdentityModel.Psha1DerivedKeyGenerator;

    abstract class WSTrust : WSSecurityTokenSerializer.SerializerEntries
    {
        WSSecurityTokenSerializer tokenSerializer;

        public WSTrust(WSSecurityTokenSerializer tokenSerializer)
        {
            this.tokenSerializer = tokenSerializer;
        }

        public WSSecurityTokenSerializer WSSecurityTokenSerializer
        {
            get { return this.tokenSerializer; }
        }

        public abstract TrustDictionary SerializerDictionary
        {
            get;
        }

        public override void PopulateTokenEntries(IList<TokenEntry> tokenEntryList)
        {
            tokenEntryList.Add(new BinarySecretTokenEntry(this));
        }

        class BinarySecretTokenEntry : TokenEntry
        {
            WSTrust parent;
            TrustDictionary otherDictionary;

            public BinarySecretTokenEntry(WSTrust parent)
            {
                this.parent = parent;
                this.otherDictionary = null;

                if (parent.SerializerDictionary is TrustDec2005Dictionary)
                {
                    this.otherDictionary = XD.TrustFeb2005Dictionary;
                }

                if (parent.SerializerDictionary is TrustFeb2005Dictionary)
                {
                    this.otherDictionary = DXD.TrustDec2005Dictionary;
                }

                // always set it, so we don't have to worry about null
                if (this.otherDictionary == null)
                    this.otherDictionary = this.parent.SerializerDictionary;
            }

            protected override XmlDictionaryString LocalName { get { return parent.SerializerDictionary.BinarySecret; } }
            protected override XmlDictionaryString NamespaceUri { get { return parent.SerializerDictionary.Namespace; } }
            protected override Type[] GetTokenTypesCore() { return new Type[] { typeof(BinarySecretSecurityToken) }; }
            public override string TokenTypeUri { get { return null; } }
            protected override string ValueTypeUri { get { return null; } }

            public override bool CanReadTokenCore(XmlElement element)
            {
                string valueTypeUri = null;

                if (element.HasAttribute(SecurityJan2004Strings.ValueType, null))
                {
                    valueTypeUri = element.GetAttribute(SecurityJan2004Strings.ValueType, null);
                }

                return element.LocalName == LocalName.Value && (element.NamespaceURI == NamespaceUri.Value || element.NamespaceURI == this.otherDictionary.Namespace.Value) && valueTypeUri == this.ValueTypeUri;
            }

            public override bool CanReadTokenCore(XmlDictionaryReader reader)
            {
                return (reader.IsStartElement(this.LocalName, this.NamespaceUri) || reader.IsStartElement(this.LocalName, this.otherDictionary.Namespace)) &&
                       reader.GetAttribute(XD.SecurityJan2004Dictionary.ValueType, null) == this.ValueTypeUri;
            }


            public override SecurityKeyIdentifierClause CreateKeyIdentifierClauseFromTokenXmlCore(XmlElement issuedTokenXml,
                SecurityTokenReferenceStyle tokenReferenceStyle)
            {
                TokenReferenceStyleHelper.Validate(tokenReferenceStyle);

                switch (tokenReferenceStyle)
                {
                    case SecurityTokenReferenceStyle.Internal:
                        return CreateDirectReference(issuedTokenXml, UtilityStrings.IdAttribute, UtilityStrings.Namespace, typeof(GenericXmlSecurityToken));
                    case SecurityTokenReferenceStyle.External:
                        // Binary Secret tokens aren't referred to externally
                        return null;
                    default:
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("tokenReferenceStyle"));
                }
            }

            public override SecurityToken ReadTokenCore(XmlDictionaryReader reader, SecurityTokenResolver tokenResolver)
            {
                string secretType = reader.GetAttribute(XD.SecurityJan2004Dictionary.TypeAttribute, null);
                string id = reader.GetAttribute(XD.UtilityDictionary.IdAttribute, XD.UtilityDictionary.Namespace);
                bool isNonce = false;

                if (secretType != null && secretType.Length > 0)
                {
                    if (secretType == parent.SerializerDictionary.NonceBinarySecret.Value || secretType == otherDictionary.NonceBinarySecret.Value)
                    {
                        isNonce = true;
                    }
                    else if (secretType != parent.SerializerDictionary.SymmetricKeyBinarySecret.Value && secretType != otherDictionary.SymmetricKeyBinarySecret.Value)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageSecurityException(SR.GetString(SR.UnexpectedBinarySecretType, parent.SerializerDictionary.SymmetricKeyBinarySecret.Value, secretType)));
                    }
                }

                byte[] secret = reader.ReadElementContentAsBase64();
                if (isNonce)
                {
                    return new NonceToken(id, secret);
                }
                else
                {
                    return new BinarySecretSecurityToken(id, secret);
                }
            }

            public override void WriteTokenCore(XmlDictionaryWriter writer, SecurityToken token)
            {
                BinarySecretSecurityToken simpleToken = token as BinarySecretSecurityToken;
                byte[] secret = simpleToken.GetKeyBytes();
                writer.WriteStartElement(parent.SerializerDictionary.Prefix.Value, parent.SerializerDictionary.BinarySecret, parent.SerializerDictionary.Namespace);
                if (simpleToken.Id != null)
                {
                    writer.WriteAttributeString(XD.UtilityDictionary.Prefix.Value, XD.UtilityDictionary.IdAttribute, XD.UtilityDictionary.Namespace, simpleToken.Id);
                }
                if (token is NonceToken)
                {
                    writer.WriteAttributeString(XD.SecurityJan2004Dictionary.TypeAttribute, null, parent.SerializerDictionary.NonceBinarySecret.Value);
                }
                writer.WriteBase64(secret, 0, secret.Length);
                writer.WriteEndElement();
            }

        }

        public abstract class Driver : TrustDriver
        {
            static readonly string base64Uri = SecurityJan2004Strings.EncodingTypeValueBase64Binary;
            static readonly string hexBinaryUri = SecurityJan2004Strings.EncodingTypeValueHexBinary;


            SecurityStandardsManager standardsManager;
            List<SecurityTokenAuthenticator> entropyAuthenticators;

            public Driver(SecurityStandardsManager standardsManager)
            {
                this.standardsManager = standardsManager;
                this.entropyAuthenticators = new List<SecurityTokenAuthenticator>(2);
            }

            public abstract TrustDictionary DriverDictionary
            {
                get;
            }

            public override XmlDictionaryString RequestSecurityTokenAction
            {
                get
                {
                    return DriverDictionary.RequestSecurityTokenIssuance;
                }
            }

            public override XmlDictionaryString RequestSecurityTokenResponseAction
            {
                get
                {
                    return DriverDictionary.RequestSecurityTokenIssuanceResponse;
                }
            }

            public override string RequestTypeIssue
            {
                get
                {
                    return DriverDictionary.RequestTypeIssue.Value;
                }
            }

            public override string ComputedKeyAlgorithm
            {
                get { return DriverDictionary.Psha1ComputedKeyUri.Value; }
            }

            public override SecurityStandardsManager StandardsManager
            {
                get
                {
                    return this.standardsManager;
                }
            }

            public override XmlDictionaryString Namespace
            {
                get { return DriverDictionary.Namespace; }
            }

            public override RequestSecurityToken CreateRequestSecurityToken(XmlReader xmlReader)
            {
                XmlDictionaryReader reader = XmlDictionaryReader.CreateDictionaryReader(xmlReader);
                reader.MoveToStartElement(DriverDictionary.RequestSecurityToken, DriverDictionary.Namespace);
                string context = null;
                string tokenTypeUri = null;
                string requestType = null;
                int keySize = 0;
                XmlDocument doc = new XmlDocument();
                XmlElement rstXml = (doc.ReadNode(reader) as XmlElement);
                SecurityKeyIdentifierClause renewTarget = null;
                SecurityKeyIdentifierClause closeTarget = null;
                for (int i = 0; i < rstXml.Attributes.Count; ++i)
                {
                    XmlAttribute attr = rstXml.Attributes[i];
                    if (attr.LocalName == DriverDictionary.Context.Value)
                    {
                        context = attr.Value;
                    }
                }
                for (int i = 0; i < rstXml.ChildNodes.Count; ++i)
                {
                    XmlElement child = (rstXml.ChildNodes[i] as XmlElement);
                    if (child != null)
                    {
                        if (child.LocalName == DriverDictionary.TokenType.Value && child.NamespaceURI == DriverDictionary.Namespace.Value)
                            tokenTypeUri = XmlHelper.ReadTextElementAsTrimmedString(child);
                        else if (child.LocalName == DriverDictionary.RequestType.Value && child.NamespaceURI == DriverDictionary.Namespace.Value)
                            requestType = XmlHelper.ReadTextElementAsTrimmedString(child);
                        else if (child.LocalName == DriverDictionary.KeySize.Value && child.NamespaceURI == DriverDictionary.Namespace.Value)
                            keySize = Int32.Parse(XmlHelper.ReadTextElementAsTrimmedString(child), NumberFormatInfo.InvariantInfo);
                    }
                }

                ReadTargets(rstXml, out renewTarget, out closeTarget);

                RequestSecurityToken rst = new RequestSecurityToken(standardsManager, rstXml, context, tokenTypeUri, requestType, keySize, renewTarget, closeTarget);
                return rst;
            }

            System.IdentityModel.XmlBuffer GetIssuedTokenBuffer(System.IdentityModel.XmlBuffer rstrBuffer)
            {
                System.IdentityModel.XmlBuffer issuedTokenBuffer = null;
                using (XmlDictionaryReader reader = rstrBuffer.GetReader(0))
                {
                    reader.ReadFullStartElement();
                    while (reader.IsStartElement())
                    {
                        if (reader.IsStartElement(this.DriverDictionary.RequestedSecurityToken, this.DriverDictionary.Namespace))
                        {
                            reader.ReadStartElement();
                            reader.MoveToContent();
                            issuedTokenBuffer = new System.IdentityModel.XmlBuffer(Int32.MaxValue);
                            using (XmlDictionaryWriter writer = issuedTokenBuffer.OpenSection(reader.Quotas))
                            {
                                writer.WriteNode(reader, false);
                                issuedTokenBuffer.CloseSection();
                                issuedTokenBuffer.Close();
                            }
                            reader.ReadEndElement();
                            break;
                        }
                        else
                        {
                            reader.Skip();
                        }
                    }
                }
                return issuedTokenBuffer;
            }

            public override RequestSecurityTokenResponse CreateRequestSecurityTokenResponse(XmlReader xmlReader)
            {
                if (xmlReader == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("xmlReader");
                }
                XmlDictionaryReader reader = XmlDictionaryReader.CreateDictionaryReader(xmlReader);
                if (reader.IsStartElement(DriverDictionary.RequestSecurityTokenResponse, DriverDictionary.Namespace) == false)
                {
                    XmlHelper.OnRequiredElementMissing(DriverDictionary.RequestSecurityTokenResponse.Value, DriverDictionary.Namespace.Value);
                }

                System.IdentityModel.XmlBuffer rstrBuffer = new System.IdentityModel.XmlBuffer(Int32.MaxValue);
                using (XmlDictionaryWriter writer = rstrBuffer.OpenSection(reader.Quotas))
                {
                    writer.WriteNode(reader, false);
                    rstrBuffer.CloseSection();
                    rstrBuffer.Close();
                }
                XmlDocument doc = new XmlDocument();
                XmlElement rstrXml;
                using (XmlReader reader2 = rstrBuffer.GetReader(0))
                {
                    rstrXml = (doc.ReadNode(reader2) as XmlElement);
                }

                System.IdentityModel.XmlBuffer issuedTokenBuffer = GetIssuedTokenBuffer(rstrBuffer);
                string context = null;
                string tokenTypeUri = null;
                int keySize = 0;
                SecurityKeyIdentifierClause requestedAttachedReference = null;
                SecurityKeyIdentifierClause requestedUnattachedReference = null;
                bool computeKey = false;
                DateTime created = DateTime.UtcNow;
                DateTime expires = SecurityUtils.MaxUtcDateTime;
                bool isRequestedTokenClosed = false;
                for (int i = 0; i < rstrXml.Attributes.Count; ++i)
                {
                    XmlAttribute attr = rstrXml.Attributes[i];
                    if (attr.LocalName == DriverDictionary.Context.Value)
                    {
                        context = attr.Value;
                    }
                }

                for (int i = 0; i < rstrXml.ChildNodes.Count; ++i)
                {
                    XmlElement child = (rstrXml.ChildNodes[i] as XmlElement);
                    if (child != null)
                    {
                        if (child.LocalName == DriverDictionary.TokenType.Value && child.NamespaceURI == DriverDictionary.Namespace.Value)
                            tokenTypeUri = XmlHelper.ReadTextElementAsTrimmedString(child);
                        else if (child.LocalName == DriverDictionary.KeySize.Value && child.NamespaceURI == DriverDictionary.Namespace.Value)
                            keySize = Int32.Parse(XmlHelper.ReadTextElementAsTrimmedString(child), NumberFormatInfo.InvariantInfo);
                        else if (child.LocalName == DriverDictionary.RequestedProofToken.Value && child.NamespaceURI == DriverDictionary.Namespace.Value)
                        {
                            XmlElement proofXml = XmlHelper.GetChildElement(child);
                            if (proofXml.LocalName == DriverDictionary.ComputedKey.Value && proofXml.NamespaceURI == DriverDictionary.Namespace.Value)
                            {
                                string computedKeyAlgorithm = XmlHelper.ReadTextElementAsTrimmedString(proofXml);
                                if (computedKeyAlgorithm != this.DriverDictionary.Psha1ComputedKeyUri.Value)
                                {
                                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new SecurityNegotiationException(SR.GetString(SR.UnknownComputedKeyAlgorithm, computedKeyAlgorithm)));
                                }
                                computeKey = true;
                            }
                        }
                        else if (child.LocalName == DriverDictionary.Lifetime.Value && child.NamespaceURI == DriverDictionary.Namespace.Value)
                        {
                            XmlElement createdXml = XmlHelper.GetChildElement(child, UtilityStrings.CreatedElement, UtilityStrings.Namespace);
                            if (createdXml != null)
                            {
                                created = DateTime.ParseExact(XmlHelper.ReadTextElementAsTrimmedString(createdXml),
                                    WSUtilitySpecificationVersion.AcceptedDateTimeFormats, DateTimeFormatInfo.InvariantInfo, DateTimeStyles.None).ToUniversalTime();
                            }
                            XmlElement expiresXml = XmlHelper.GetChildElement(child, UtilityStrings.ExpiresElement, UtilityStrings.Namespace);
                            if (expiresXml != null)
                            {
                                expires = DateTime.ParseExact(XmlHelper.ReadTextElementAsTrimmedString(expiresXml),
                                    WSUtilitySpecificationVersion.AcceptedDateTimeFormats, DateTimeFormatInfo.InvariantInfo, DateTimeStyles.None).ToUniversalTime();
                            }
                        }
                    }
                }

                isRequestedTokenClosed = ReadRequestedTokenClosed(rstrXml);
                ReadReferences(rstrXml, out requestedAttachedReference, out requestedUnattachedReference);

                return new RequestSecurityTokenResponse(standardsManager, rstrXml, context, tokenTypeUri, keySize, requestedAttachedReference, requestedUnattachedReference,
                                                        computeKey, created, expires, isRequestedTokenClosed, issuedTokenBuffer);
            }

            public override RequestSecurityTokenResponseCollection CreateRequestSecurityTokenResponseCollection(XmlReader xmlReader)
            {
                XmlDictionaryReader reader = XmlDictionaryReader.CreateDictionaryReader(xmlReader);
                List<RequestSecurityTokenResponse> rstrCollection = new List<RequestSecurityTokenResponse>(2);
                string rootName = reader.Name;
                reader.ReadStartElement(DriverDictionary.RequestSecurityTokenResponseCollection, DriverDictionary.Namespace);
                while (reader.IsStartElement(DriverDictionary.RequestSecurityTokenResponse.Value, DriverDictionary.Namespace.Value))
                {
                    RequestSecurityTokenResponse rstr = this.CreateRequestSecurityTokenResponse(reader);
                    rstrCollection.Add(rstr);
                }
                reader.ReadEndElement();
                if (rstrCollection.Count == 0)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(SR.GetString(SR.NoRequestSecurityTokenResponseElements)));
                return new RequestSecurityTokenResponseCollection(rstrCollection.AsReadOnly(), this.StandardsManager);
            }

            XmlElement GetAppliesToElement(XmlElement rootElement)
            {
                if (rootElement == null)
                {
                    return null;
                }
                for (int i = 0; i < rootElement.ChildNodes.Count; ++i)
                {
                    XmlElement elem = (rootElement.ChildNodes[i] as XmlElement);
                    if (elem != null)
                    {
                        if (elem.LocalName == DriverDictionary.AppliesTo.Value && elem.NamespaceURI == Namespaces.WSPolicy)
                        {
                            return elem;
                        }
                    }
                }
                return null;
            }

            T GetAppliesTo<T>(XmlElement rootXml, XmlObjectSerializer serializer)
            {
                XmlElement appliesToElement = GetAppliesToElement(rootXml);
                if (appliesToElement != null)
                {
                    using (XmlReader reader = new XmlNodeReader(appliesToElement))
                    {
                        reader.ReadStartElement();
                        lock (serializer)
                        {
                            return (T)serializer.ReadObject(reader);
                        }
                    }
                }
                else
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.NoAppliesToPresent)));
                }
            }

            public override T GetAppliesTo<T>(RequestSecurityToken rst, XmlObjectSerializer serializer)
            {
                if (rst == null)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("rst");

                return GetAppliesTo<T>(rst.RequestSecurityTokenXml, serializer);
            }

            public override T GetAppliesTo<T>(RequestSecurityTokenResponse rstr, XmlObjectSerializer serializer)
            {
                if (rstr == null)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("rstr");

                return GetAppliesTo<T>(rstr.RequestSecurityTokenResponseXml, serializer);
            }

            public override bool IsAppliesTo(string localName, string namespaceUri)
            {
                return (localName == DriverDictionary.AppliesTo.Value && namespaceUri == Namespaces.WSPolicy);
            }

            void GetAppliesToQName(XmlElement rootElement, out string localName, out string namespaceUri)
            {
                localName = namespaceUri = null;
                XmlElement appliesToElement = GetAppliesToElement(rootElement);
                if (appliesToElement != null)
                {
                    using (XmlReader reader = new XmlNodeReader(appliesToElement))
                    {
                        reader.ReadStartElement();
                        reader.MoveToContent();
                        localName = reader.LocalName;
                        namespaceUri = reader.NamespaceURI;
                    }
                }
            }

            public override void GetAppliesToQName(RequestSecurityToken rst, out string localName, out string namespaceUri)
            {
                if (rst == null)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("rst");

                GetAppliesToQName(rst.RequestSecurityTokenXml, out localName, out namespaceUri);
            }

            public override void GetAppliesToQName(RequestSecurityTokenResponse rstr, out string localName, out string namespaceUri)
            {
                if (rstr == null)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("rstr");

                GetAppliesToQName(rstr.RequestSecurityTokenResponseXml, out localName, out namespaceUri);
            }

            public override byte[] GetAuthenticator(RequestSecurityTokenResponse rstr)
            {
                if (rstr != null && rstr.RequestSecurityTokenResponseXml != null && rstr.RequestSecurityTokenResponseXml.ChildNodes != null)
                {
                    for (int i = 0; i < rstr.RequestSecurityTokenResponseXml.ChildNodes.Count; ++i)
                    {
                        XmlElement element = rstr.RequestSecurityTokenResponseXml.ChildNodes[i] as XmlElement;
                        if (element != null)
                        {
                            if (element.LocalName == DriverDictionary.Authenticator.Value && element.NamespaceURI == DriverDictionary.Namespace.Value)
                            {
                                XmlElement combinedHashElement = XmlHelper.GetChildElement(element);
                                if (combinedHashElement.LocalName == DriverDictionary.CombinedHash.Value && combinedHashElement.NamespaceURI == DriverDictionary.Namespace.Value)
                                {
                                    string authenticatorString = XmlHelper.ReadTextElementAsTrimmedString(combinedHashElement);
                                    return Convert.FromBase64String(authenticatorString);
                                }
                            }
                        }
                    }
                }
                return null;
            }

            public override BinaryNegotiation GetBinaryNegotiation(RequestSecurityTokenResponse rstr)
            {
                if (rstr == null)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("rstr");

                return GetBinaryNegotiation(rstr.RequestSecurityTokenResponseXml);
            }

            public override BinaryNegotiation GetBinaryNegotiation(RequestSecurityToken rst)
            {
                if (rst == null)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("rst");

                return GetBinaryNegotiation(rst.RequestSecurityTokenXml);
            }

            BinaryNegotiation GetBinaryNegotiation(XmlElement rootElement)
            {
                if (rootElement == null)
                {
                    return null;
                }
                for (int i = 0; i < rootElement.ChildNodes.Count; ++i)
                {
                    XmlElement elem = rootElement.ChildNodes[i] as XmlElement;
                    if (elem != null)
                    {
                        if (elem.LocalName == DriverDictionary.BinaryExchange.Value && elem.NamespaceURI == DriverDictionary.Namespace.Value)
                        {
                            return ReadBinaryNegotiation(elem);
                        }
                    }
                }
                return null;
            }

            public override SecurityToken GetEntropy(RequestSecurityToken rst, SecurityTokenResolver resolver)
            {
                if (rst == null)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("rst");

                return GetEntropy(rst.RequestSecurityTokenXml, resolver);
            }

            public override SecurityToken GetEntropy(RequestSecurityTokenResponse rstr, SecurityTokenResolver resolver)
            {
                if (rstr == null)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("rstr");

                return GetEntropy(rstr.RequestSecurityTokenResponseXml, resolver);
            }

            SecurityToken GetEntropy(XmlElement rootElement, SecurityTokenResolver resolver)
            {
                if (rootElement == null || rootElement.ChildNodes == null)
                {
                    return null;
                }
                for (int i = 0; i < rootElement.ChildNodes.Count; ++i)
                {
                    XmlElement element = rootElement.ChildNodes[i] as XmlElement;
                    if (element != null)
                    {
                        if (element.LocalName == DriverDictionary.Entropy.Value && element.NamespaceURI == DriverDictionary.Namespace.Value)
                        {
                            XmlElement tokenXml = XmlHelper.GetChildElement(element);
                            string valueTypeUri = element.GetAttribute(SecurityJan2004Strings.ValueType);
                            if (valueTypeUri.Length == 0)
                                valueTypeUri = null;
                            return standardsManager.SecurityTokenSerializer.ReadToken(new XmlNodeReader(tokenXml), resolver);
                        }
                    }
                }
                return null;
            }

            void GetIssuedAndProofXml(RequestSecurityTokenResponse rstr, out XmlElement issuedTokenXml, out XmlElement proofTokenXml)
            {
                issuedTokenXml = null;
                proofTokenXml = null;
                if ((rstr.RequestSecurityTokenResponseXml != null) && (rstr.RequestSecurityTokenResponseXml.ChildNodes != null))
                {
                    for (int i = 0; i < rstr.RequestSecurityTokenResponseXml.ChildNodes.Count; ++i)
                    {
                        XmlElement elem = rstr.RequestSecurityTokenResponseXml.ChildNodes[i] as XmlElement;
                        if (elem != null)
                        {
                            if (elem.LocalName == DriverDictionary.RequestedSecurityToken.Value && elem.NamespaceURI == DriverDictionary.Namespace.Value)
                            {
                                if (issuedTokenXml != null)
                                {
                                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.RstrHasMultipleIssuedTokens)));
                                }
                                issuedTokenXml = XmlHelper.GetChildElement(elem);
                            }
                            else if (elem.LocalName == DriverDictionary.RequestedProofToken.Value && elem.NamespaceURI == DriverDictionary.Namespace.Value)
                            {
                                if (proofTokenXml != null)
                                {
                                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.RstrHasMultipleProofTokens)));
                                }
                                proofTokenXml = XmlHelper.GetChildElement(elem);
                            }
                        }
                    }
                }
            }

            /// <summary>
            /// The algorithm for computing the key is:
            /// 1. If there is requestorEntropy:
            ///    a. If there is no <RequestedProofToken> use the requestorEntropy as the key
            ///    b. If there is a <RequestedProofToken> with a ComputedKeyUri, combine the client and server entropies
            ///    c. Anything else, throw
            /// 2. If there is no requestorEntropy:
            ///    a. THere has to be a <RequestedProofToken> that contains the proof key
            /// </summary>
            public override GenericXmlSecurityToken GetIssuedToken(RequestSecurityTokenResponse rstr, SecurityTokenResolver resolver, IList<SecurityTokenAuthenticator> allowedAuthenticators, SecurityKeyEntropyMode keyEntropyMode, byte[] requestorEntropy, string expectedTokenType,
                ReadOnlyCollection<IAuthorizationPolicy> authorizationPolicies, int defaultKeySize, bool isBearerKeyType)
            {

                SecurityKeyEntropyModeHelper.Validate(keyEntropyMode);

                if (defaultKeySize < 0)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("defaultKeySize", SR.GetString(SR.ValueMustBeNonNegative)));
                }

                if (rstr == null)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("rstr");

                string tokenType;
                if (rstr.TokenType != null)
                {
                    if (expectedTokenType != null && expectedTokenType != rstr.TokenType)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.BadIssuedTokenType, rstr.TokenType, expectedTokenType)));
                    }
                    tokenType = rstr.TokenType;
                }
                else
                {
                    tokenType = expectedTokenType;
                }

                // search the response elements for licenseXml, proofXml, and lifetime
                DateTime created = rstr.ValidFrom;
                DateTime expires = rstr.ValidTo;
                XmlElement proofXml;
                XmlElement issuedTokenXml;
                GetIssuedAndProofXml(rstr, out issuedTokenXml, out proofXml);

                if (issuedTokenXml == null)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.NoLicenseXml)));

                if (isBearerKeyType)
                {
                    if (proofXml != null)
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.BearerKeyTypeCannotHaveProofKey)));

                    return new GenericXmlSecurityToken(issuedTokenXml, null, created, expires, rstr.RequestedAttachedReference, rstr.RequestedUnattachedReference, authorizationPolicies);
                }

                SecurityToken proofToken;
                SecurityToken entropyToken = GetEntropy(rstr, resolver);
                if (keyEntropyMode == SecurityKeyEntropyMode.ClientEntropy)
                {
                    if (requestorEntropy == null)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.EntropyModeRequiresRequestorEntropy, keyEntropyMode)));
                    }
                    // enforce that there is no entropy or proof token in the RSTR
                    if (proofXml != null || entropyToken != null)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.EntropyModeCannotHaveProofTokenOrIssuerEntropy, keyEntropyMode)));
                    }
                    proofToken = new BinarySecretSecurityToken(requestorEntropy);
                }
                else if (keyEntropyMode == SecurityKeyEntropyMode.ServerEntropy)
                {
                    if (requestorEntropy != null)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.EntropyModeCannotHaveRequestorEntropy, keyEntropyMode)));
                    }
                    if (rstr.ComputeKey || entropyToken != null)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.EntropyModeCannotHaveComputedKey, keyEntropyMode)));
                    }
                    if (proofXml == null)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.EntropyModeRequiresProofToken, keyEntropyMode)));
                    }
                    string valueTypeUri = proofXml.GetAttribute(SecurityJan2004Strings.ValueType);
                    if (valueTypeUri.Length == 0)
                        valueTypeUri = null;
                    proofToken = standardsManager.SecurityTokenSerializer.ReadToken(new XmlNodeReader(proofXml), resolver);
                }
                else
                {
                    if (!rstr.ComputeKey)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.EntropyModeRequiresComputedKey, keyEntropyMode)));
                    }
                    if (entropyToken == null)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.EntropyModeRequiresIssuerEntropy, keyEntropyMode)));
                    }
                    if (requestorEntropy == null)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.EntropyModeRequiresRequestorEntropy, keyEntropyMode)));
                    }
                    if (rstr.KeySize == 0 && defaultKeySize == 0)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.RstrKeySizeNotProvided)));
                    }
                    int issuedKeySize = (rstr.KeySize != 0) ? rstr.KeySize : defaultKeySize;
                    byte[] issuerEntropy;
                    if (entropyToken is BinarySecretSecurityToken)
                        issuerEntropy = ((BinarySecretSecurityToken)entropyToken).GetKeyBytes();
                    else if (entropyToken is WrappedKeySecurityToken)
                        issuerEntropy = ((WrappedKeySecurityToken)entropyToken).GetWrappedKey();
                    else
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(SR.GetString(SR.UnsupportedIssuerEntropyType)));
                    // compute the PSHA1 derived key
                    byte[] issuedKey = RequestSecurityTokenResponse.ComputeCombinedKey(requestorEntropy, issuerEntropy, issuedKeySize);
                    proofToken = new BinarySecretSecurityToken(issuedKey);
                }

                SecurityKeyIdentifierClause internalReference = rstr.RequestedAttachedReference;
                SecurityKeyIdentifierClause externalReference = rstr.RequestedUnattachedReference;

                return new BufferedGenericXmlSecurityToken(issuedTokenXml, proofToken, created, expires, internalReference, externalReference, authorizationPolicies, rstr.IssuedTokenBuffer);
            }

            public override GenericXmlSecurityToken GetIssuedToken(RequestSecurityTokenResponse rstr, string expectedTokenType,
                ReadOnlyCollection<IAuthorizationPolicy> authorizationPolicies, RSA clientKey)
            {
                if (rstr == null)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("rstr"));

                string tokenType;
                if (rstr.TokenType != null)
                {
                    if (expectedTokenType != null && expectedTokenType != rstr.TokenType)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.BadIssuedTokenType, rstr.TokenType, expectedTokenType)));
                    }
                    tokenType = rstr.TokenType;
                }
                else
                {
                    tokenType = expectedTokenType;
                }

                // search the response elements for licenseXml, proofXml, and lifetime
                DateTime created = rstr.ValidFrom;
                DateTime expires = rstr.ValidTo;
                XmlElement proofXml;
                XmlElement issuedTokenXml;
                GetIssuedAndProofXml(rstr, out issuedTokenXml, out proofXml);

                if (issuedTokenXml == null)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.NoLicenseXml)));

                // enforce that there is no proof token in the RSTR
                if (proofXml != null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.ProofTokenXmlUnexpectedInRstr)));
                }
                SecurityKeyIdentifierClause internalReference = rstr.RequestedAttachedReference;
                SecurityKeyIdentifierClause externalReference = rstr.RequestedUnattachedReference;

                SecurityToken proofToken = new RsaSecurityToken(clientKey);
                return new BufferedGenericXmlSecurityToken(issuedTokenXml, proofToken, created, expires, internalReference, externalReference, authorizationPolicies, rstr.IssuedTokenBuffer);
            }

            public override bool IsAtRequestSecurityTokenResponse(XmlReader reader)
            {
                if (reader == null)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("reader");

                return reader.IsStartElement(DriverDictionary.RequestSecurityTokenResponse.Value, DriverDictionary.Namespace.Value);
            }

            public override bool IsAtRequestSecurityTokenResponseCollection(XmlReader reader)
            {
                if (reader == null)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("reader");

                return reader.IsStartElement(DriverDictionary.RequestSecurityTokenResponseCollection.Value, DriverDictionary.Namespace.Value);
            }

            public override bool IsRequestedSecurityTokenElement(string name, string nameSpace)
            {
                return (name == DriverDictionary.RequestedSecurityToken.Value && nameSpace == DriverDictionary.Namespace.Value);
            }

            public override bool IsRequestedProofTokenElement(string name, string nameSpace)
            {
                return (name == DriverDictionary.RequestedProofToken.Value && nameSpace == DriverDictionary.Namespace.Value);
            }

            public static BinaryNegotiation ReadBinaryNegotiation(XmlElement elem)
            {
                if (elem == null)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("elem");

                // get the encoding and valueType attributes
                string encodingUri = null;
                string valueTypeUri = null;
                byte[] negotiationData = null;
                if (elem.Attributes != null)
                {
                    for (int i = 0; i < elem.Attributes.Count; ++i)
                    {
                        XmlAttribute attr = elem.Attributes[i];
                        if (attr.LocalName == SecurityJan2004Strings.EncodingType && attr.NamespaceURI.Length == 0)
                        {
                            encodingUri = attr.Value;
                            if (encodingUri != base64Uri && encodingUri != hexBinaryUri)
                            {
                                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(SR.GetString(SR.UnsupportedBinaryEncoding, encodingUri)));
                            }
                        }
                        else if (attr.LocalName == SecurityJan2004Strings.ValueType && attr.NamespaceURI.Length == 0)
                        {
                            valueTypeUri = attr.Value;
                        }
                        // ignore all other attributes
                    }
                }
                if (encodingUri == null)
                {
                    XmlHelper.OnRequiredAttributeMissing("EncodingType", elem.Name);
                }
                if (valueTypeUri == null)
                {
                    XmlHelper.OnRequiredAttributeMissing("ValueType", elem.Name);
                }
                string encodedBlob = XmlHelper.ReadTextElementAsTrimmedString(elem);
                if (encodingUri == base64Uri)
                {
                    negotiationData = Convert.FromBase64String(encodedBlob);
                }
                else
                {
                    negotiationData = HexBinary.Parse(encodedBlob).Value;
                }
                return new BinaryNegotiation(valueTypeUri, negotiationData);
            }

            // Note in Apr2004, internal & external references aren't supported - 
            // our strategy is to see if there's a token reference (and use it for external ref) and backup is to scan the token xml to compute reference
            protected virtual void ReadReferences(XmlElement rstrXml, out SecurityKeyIdentifierClause requestedAttachedReference,
                    out SecurityKeyIdentifierClause requestedUnattachedReference)
            {
                XmlElement issuedTokenXml = null;
                requestedAttachedReference = null;
                requestedUnattachedReference = null;
                for (int i = 0; i < rstrXml.ChildNodes.Count; ++i)
                {
                    XmlElement child = rstrXml.ChildNodes[i] as XmlElement;
                    if (child != null)
                    {
                        if (child.LocalName == DriverDictionary.RequestedSecurityToken.Value && child.NamespaceURI == DriverDictionary.Namespace.Value)
                        {
                            issuedTokenXml = XmlHelper.GetChildElement(child);
                        }
                        else if (child.LocalName == DriverDictionary.RequestedTokenReference.Value && child.NamespaceURI == DriverDictionary.Namespace.Value)
                        {
                            requestedUnattachedReference = GetKeyIdentifierXmlReferenceClause(XmlHelper.GetChildElement(child));
                        }
                    }
                }

                if (issuedTokenXml != null)
                {
                    requestedAttachedReference = standardsManager.CreateKeyIdentifierClauseFromTokenXml(issuedTokenXml, SecurityTokenReferenceStyle.Internal);
                    if (requestedUnattachedReference == null)
                    {
                        try
                        {
                            requestedUnattachedReference = standardsManager.CreateKeyIdentifierClauseFromTokenXml(issuedTokenXml, SecurityTokenReferenceStyle.External);
                        }
                        catch (XmlException)
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(SR.GetString(SR.TrustDriverIsUnableToCreatedNecessaryAttachedOrUnattachedReferences, issuedTokenXml.ToString())));
                        }
                    }
                }
            }

            internal bool TryReadKeyIdentifierClause(XmlNodeReader reader, out SecurityKeyIdentifierClause keyIdentifierClause)
            {
                keyIdentifierClause = null;

                try
                {
                    keyIdentifierClause = standardsManager.SecurityTokenSerializer.ReadKeyIdentifierClause(reader);
                }
                catch (XmlException e)
                {
                    if (Fx.IsFatal(e))
                    {
                        throw;
                    }

                    keyIdentifierClause = null;
                    return false;
                }
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                    {
                        throw;
                    }

                    keyIdentifierClause = null;
                    return false;
                }

                return true;
            }

            internal SecurityKeyIdentifierClause CreateGenericXmlSecurityKeyIdentifierClause(XmlNodeReader reader, XmlElement keyIdentifierReferenceXmlElement)
            {
                SecurityKeyIdentifierClause keyIdentifierClause = null;
                XmlDictionaryReader localReader = XmlDictionaryReader.CreateDictionaryReader(reader);
                string strId = localReader.GetAttribute(XD.UtilityDictionary.IdAttribute, XD.UtilityDictionary.Namespace);
                keyIdentifierClause = new GenericXmlSecurityKeyIdentifierClause(keyIdentifierReferenceXmlElement);
                if (!String.IsNullOrEmpty(strId))
                {
                    keyIdentifierClause.Id = strId;
                }
                return keyIdentifierClause;
            }

            internal SecurityKeyIdentifierClause GetKeyIdentifierXmlReferenceClause(XmlElement keyIdentifierReferenceXmlElement)
            {
                SecurityKeyIdentifierClause keyIdentifierClause = null;
                XmlNodeReader reader = new XmlNodeReader(keyIdentifierReferenceXmlElement);
                if (!this.TryReadKeyIdentifierClause(reader, out keyIdentifierClause))
                {
                    keyIdentifierClause = CreateGenericXmlSecurityKeyIdentifierClause(new XmlNodeReader(keyIdentifierReferenceXmlElement), keyIdentifierReferenceXmlElement);
                }

                return keyIdentifierClause;
            }

            protected virtual bool ReadRequestedTokenClosed(XmlElement rstrXml)
            {
                return false;
            }

            protected virtual void ReadTargets(XmlElement rstXml, out SecurityKeyIdentifierClause renewTarget, out SecurityKeyIdentifierClause closeTarget)
            {
                renewTarget = null;
                closeTarget = null;
            }

            public override void OnRSTRorRSTRCMissingException()
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(SR.GetString(SR.ExpectedOneOfTwoElementsFromNamespace,
                    DriverDictionary.RequestSecurityTokenResponse, DriverDictionary.RequestSecurityTokenResponseCollection,
                    DriverDictionary.Namespace)));
            }

            void WriteAppliesTo(object appliesTo, Type appliesToType, XmlObjectSerializer serializer, XmlWriter xmlWriter)
            {
                XmlDictionaryWriter writer = XmlDictionaryWriter.CreateDictionaryWriter(xmlWriter);
                writer.WriteStartElement(Namespaces.WSPolicyPrefix, DriverDictionary.AppliesTo.Value, Namespaces.WSPolicy);
                lock (serializer)
                {
                    serializer.WriteObject(writer, appliesTo);
                }
                writer.WriteEndElement();
            }

            public void WriteBinaryNegotiation(BinaryNegotiation negotiation, XmlWriter xmlWriter)
            {
                if (negotiation == null)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("negotiation");

                XmlDictionaryWriter writer = XmlDictionaryWriter.CreateDictionaryWriter(xmlWriter);
                negotiation.WriteTo(writer, this.DriverDictionary.Prefix.Value,
                                            this.DriverDictionary.BinaryExchange, this.DriverDictionary.Namespace,
                                            XD.SecurityJan2004Dictionary.ValueType, null);
            }

            public override void WriteRequestSecurityToken(RequestSecurityToken rst, XmlWriter xmlWriter)
            {
                if (rst == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("rst");
                }
                if (xmlWriter == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("xmlWriter");
                }
                XmlDictionaryWriter writer = XmlDictionaryWriter.CreateDictionaryWriter(xmlWriter);
                if (rst.IsReceiver)
                {
                    rst.WriteTo(writer);
                    return;
                }
                writer.WriteStartElement(DriverDictionary.Prefix.Value, DriverDictionary.RequestSecurityToken, DriverDictionary.Namespace);
                XmlHelper.AddNamespaceDeclaration(writer, DriverDictionary.Prefix.Value, DriverDictionary.Namespace);
                if (rst.Context != null)
                    writer.WriteAttributeString(DriverDictionary.Context, null, rst.Context);

                rst.OnWriteCustomAttributes(writer);
                if (rst.TokenType != null)
                {
                    writer.WriteStartElement(DriverDictionary.Prefix.Value, DriverDictionary.TokenType, DriverDictionary.Namespace);
                    writer.WriteString(rst.TokenType);
                    writer.WriteEndElement();
                }
                if (rst.RequestType != null)
                {
                    writer.WriteStartElement(DriverDictionary.Prefix.Value, DriverDictionary.RequestType, DriverDictionary.Namespace);
                    writer.WriteString(rst.RequestType);
                    writer.WriteEndElement();
                }

                if (rst.AppliesTo != null)
                {
                    WriteAppliesTo(rst.AppliesTo, rst.AppliesToType, rst.AppliesToSerializer, writer);
                }

                SecurityToken entropyToken = rst.GetRequestorEntropy();
                if (entropyToken != null)
                {
                    writer.WriteStartElement(DriverDictionary.Prefix.Value, DriverDictionary.Entropy, DriverDictionary.Namespace);
                    standardsManager.SecurityTokenSerializer.WriteToken(writer, entropyToken);
                    writer.WriteEndElement();
                }

                if (rst.KeySize != 0)
                {
                    writer.WriteStartElement(DriverDictionary.Prefix.Value, DriverDictionary.KeySize, DriverDictionary.Namespace);
                    writer.WriteValue(rst.KeySize);
                    writer.WriteEndElement();
                }

                BinaryNegotiation negotiationData = rst.GetBinaryNegotiation();
                if (negotiationData != null)
                    WriteBinaryNegotiation(negotiationData, writer);

                WriteTargets(rst, writer);

                if (rst.RequestProperties != null)
                {
                    foreach (XmlElement property in rst.RequestProperties)
                    {
                        property.WriteTo(writer);
                    }
                }

                rst.OnWriteCustomElements(writer);
                writer.WriteEndElement();
            }

            protected virtual void WriteTargets(RequestSecurityToken rst, XmlDictionaryWriter writer)
            {
            }

            // Note in Apr2004, internal & external references aren't supported - our strategy is to generate the external ref as the TokenReference.
            protected virtual void WriteReferences(RequestSecurityTokenResponse rstr, XmlDictionaryWriter writer)
            {
                if (rstr.RequestedUnattachedReference != null)
                {
                    writer.WriteStartElement(DriverDictionary.Prefix.Value, DriverDictionary.RequestedTokenReference, DriverDictionary.Namespace);
                    standardsManager.SecurityTokenSerializer.WriteKeyIdentifierClause(writer, rstr.RequestedUnattachedReference);
                    writer.WriteEndElement();
                }
            }

            protected virtual void WriteRequestedTokenClosed(RequestSecurityTokenResponse rstr, XmlDictionaryWriter writer)
            {
            }

            public override void WriteRequestSecurityTokenResponse(RequestSecurityTokenResponse rstr, XmlWriter xmlWriter)
            {
                if (rstr == null)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("rstr");
                if (xmlWriter == null)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("xmlWriter");
                XmlDictionaryWriter writer = XmlDictionaryWriter.CreateDictionaryWriter(xmlWriter);
                if (rstr.IsReceiver)
                {
                    rstr.WriteTo(writer);
                    return;
                }
                writer.WriteStartElement(DriverDictionary.Prefix.Value, DriverDictionary.RequestSecurityTokenResponse, DriverDictionary.Namespace);
                if (rstr.Context != null)
                {
                    writer.WriteAttributeString(DriverDictionary.Context, null, rstr.Context);
                }
                // define WSUtility at the top level to avoid multiple definitions below
                XmlHelper.AddNamespaceDeclaration(writer, UtilityStrings.Prefix, XD.UtilityDictionary.Namespace);
                rstr.OnWriteCustomAttributes(writer);

                if (rstr.TokenType != null)
                    writer.WriteElementString(DriverDictionary.Prefix.Value, DriverDictionary.TokenType, DriverDictionary.Namespace, rstr.TokenType);

                if (rstr.RequestedSecurityToken != null)
                {
                    writer.WriteStartElement(DriverDictionary.Prefix.Value, DriverDictionary.RequestedSecurityToken, DriverDictionary.Namespace);
                    standardsManager.SecurityTokenSerializer.WriteToken(writer, rstr.RequestedSecurityToken);
                    writer.WriteEndElement();
                }

                if (rstr.AppliesTo != null)
                {
                    WriteAppliesTo(rstr.AppliesTo, rstr.AppliesToType, rstr.AppliesToSerializer, writer);
                }

                WriteReferences(rstr, writer);

                if (rstr.ComputeKey || rstr.RequestedProofToken != null)
                {
                    writer.WriteStartElement(DriverDictionary.Prefix.Value, DriverDictionary.RequestedProofToken, DriverDictionary.Namespace);
                    if (rstr.ComputeKey)
                    {
                        writer.WriteElementString(DriverDictionary.Prefix.Value, DriverDictionary.ComputedKey, DriverDictionary.Namespace, DriverDictionary.Psha1ComputedKeyUri.Value);
                    }
                    else
                    {
                        standardsManager.SecurityTokenSerializer.WriteToken(writer, rstr.RequestedProofToken);
                    }
                    writer.WriteEndElement();
                }

                SecurityToken entropyToken = rstr.GetIssuerEntropy();
                if (entropyToken != null)
                {
                    writer.WriteStartElement(DriverDictionary.Prefix.Value, DriverDictionary.Entropy, DriverDictionary.Namespace);
                    standardsManager.SecurityTokenSerializer.WriteToken(writer, entropyToken);
                    writer.WriteEndElement();
                }

                // To write out the lifetime, the following algorithm is used
                //   1. If the lifetime is explicitly set, write it out.
                //   2. Else, if a token/tokenbuilder has been set, use the lifetime in that.
                //   3. Else do not serialize lifetime
                if (rstr.IsLifetimeSet || rstr.RequestedSecurityToken != null)
                {
                    DateTime effectiveTime = SecurityUtils.MinUtcDateTime;
                    DateTime expirationTime = SecurityUtils.MaxUtcDateTime;

                    if (rstr.IsLifetimeSet)
                    {
                        effectiveTime = rstr.ValidFrom.ToUniversalTime();
                        expirationTime = rstr.ValidTo.ToUniversalTime();
                    }
                    else if (rstr.RequestedSecurityToken != null)
                    {
                        effectiveTime = rstr.RequestedSecurityToken.ValidFrom.ToUniversalTime();
                        expirationTime = rstr.RequestedSecurityToken.ValidTo.ToUniversalTime();
                    }

                    // write out the lifetime
                    writer.WriteStartElement(DriverDictionary.Prefix.Value, DriverDictionary.Lifetime, DriverDictionary.Namespace);
                    // write out Created
                    writer.WriteStartElement(XD.UtilityDictionary.Prefix.Value, XD.UtilityDictionary.CreatedElement, XD.UtilityDictionary.Namespace);
                    writer.WriteString(effectiveTime.ToString("yyyy-MM-ddTHH:mm:ss.fffZ", CultureInfo.InvariantCulture.DateTimeFormat));
                    writer.WriteEndElement(); // wsu:Created
                    // write out Expires
                    writer.WriteStartElement(XD.UtilityDictionary.Prefix.Value, XD.UtilityDictionary.ExpiresElement, XD.UtilityDictionary.Namespace);
                    writer.WriteString(expirationTime.ToString("yyyy-MM-ddTHH:mm:ss.fffZ", CultureInfo.InvariantCulture.DateTimeFormat));
                    writer.WriteEndElement(); // wsu:Expires
                    writer.WriteEndElement(); // wsse:Lifetime
                }

                byte[] authenticator = rstr.GetAuthenticator();
                if (authenticator != null)
                {
                    writer.WriteStartElement(DriverDictionary.Prefix.Value, DriverDictionary.Authenticator, DriverDictionary.Namespace);
                    writer.WriteStartElement(DriverDictionary.Prefix.Value, DriverDictionary.CombinedHash, DriverDictionary.Namespace);
                    writer.WriteBase64(authenticator, 0, authenticator.Length);
                    writer.WriteEndElement();
                    writer.WriteEndElement();
                }

                if (rstr.KeySize > 0)
                {
                    writer.WriteStartElement(DriverDictionary.Prefix.Value, DriverDictionary.KeySize, DriverDictionary.Namespace);
                    writer.WriteValue(rstr.KeySize);
                    writer.WriteEndElement();
                }

                WriteRequestedTokenClosed(rstr, writer);

                BinaryNegotiation negotiationData = rstr.GetBinaryNegotiation();
                if (negotiationData != null)
                    WriteBinaryNegotiation(negotiationData, writer);

                rstr.OnWriteCustomElements(writer);
                writer.WriteEndElement();
            }

            public override void WriteRequestSecurityTokenResponseCollection(RequestSecurityTokenResponseCollection rstrCollection, XmlWriter xmlWriter)
            {
                if (rstrCollection == null)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("rstrCollection");

                XmlDictionaryWriter writer = XmlDictionaryWriter.CreateDictionaryWriter(xmlWriter);
                writer.WriteStartElement(DriverDictionary.Prefix.Value, DriverDictionary.RequestSecurityTokenResponseCollection, DriverDictionary.Namespace);
                foreach (RequestSecurityTokenResponse rstr in rstrCollection.RstrCollection)
                {
                    rstr.WriteTo(writer);
                }
                writer.WriteEndElement();
            }

            protected void SetProtectionLevelForFederation(OperationDescriptionCollection operations)
            {
                foreach (OperationDescription operation in operations)
                {
                    foreach (MessageDescription message in operation.Messages)
                    {
                        if (message.Body.Parts.Count > 0)
                        {
                            foreach (MessagePartDescription part in message.Body.Parts)
                            {
                                part.ProtectionLevel = System.Net.Security.ProtectionLevel.EncryptAndSign;
                            }
                        }
                        if (OperationFormatter.IsValidReturnValue(message.Body.ReturnValue))
                        {
                            message.Body.ReturnValue.ProtectionLevel = System.Net.Security.ProtectionLevel.EncryptAndSign;
                        }
                    }
                }
            }

            public override bool TryParseKeySizeElement(XmlElement element, out int keySize)
            {
                if (element == null)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("element");

                if (element.LocalName == this.DriverDictionary.KeySize.Value
                    && element.NamespaceURI == this.DriverDictionary.Namespace.Value)
                {
                    keySize = Int32.Parse(XmlHelper.ReadTextElementAsTrimmedString(element), NumberFormatInfo.InvariantInfo);
                    return true;
                }

                keySize = 0;
                return false;
            }

            public override XmlElement CreateKeySizeElement(int keySize)
            {
                if (keySize < 0)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("keySize", SR.GetString(SR.ValueMustBeNonNegative)));
                }
                XmlDocument doc = new XmlDocument();
                XmlElement result = doc.CreateElement(this.DriverDictionary.Prefix.Value, this.DriverDictionary.KeySize.Value,
                    this.DriverDictionary.Namespace.Value);
                result.AppendChild(doc.CreateTextNode(keySize.ToString(System.Globalization.CultureInfo.InvariantCulture.NumberFormat)));
                return result;
            }

            public override XmlElement CreateKeyTypeElement(SecurityKeyType keyType)
            {
                if (keyType == SecurityKeyType.SymmetricKey)
                    return CreateSymmetricKeyTypeElement();
                else if (keyType == SecurityKeyType.AsymmetricKey)
                    return CreatePublicKeyTypeElement();
                else
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.UnableToCreateKeyTypeElementForUnknownKeyType, keyType.ToString())));
            }

            public override bool TryParseKeyTypeElement(XmlElement element, out SecurityKeyType keyType)
            {
                if (element == null)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("element");

                if (TryParseSymmetricKeyElement(element))
                {
                    keyType = SecurityKeyType.SymmetricKey;
                    return true;
                }
                else if (TryParsePublicKeyElement(element))
                {
                    keyType = SecurityKeyType.AsymmetricKey;
                    return true;
                }

                keyType = SecurityKeyType.SymmetricKey;
                return false;

            }

            public bool TryParseSymmetricKeyElement(XmlElement element)
            {
                if (element == null)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("element");

                return element.LocalName == this.DriverDictionary.KeyType.Value
                    && element.NamespaceURI == this.DriverDictionary.Namespace.Value
                    && element.InnerText == this.DriverDictionary.SymmetricKeyType.Value;
            }

            XmlElement CreateSymmetricKeyTypeElement()
            {
                XmlDocument doc = new XmlDocument();
                XmlElement result = doc.CreateElement(this.DriverDictionary.Prefix.Value, this.DriverDictionary.KeyType.Value,
                    this.DriverDictionary.Namespace.Value);
                result.AppendChild(doc.CreateTextNode(this.DriverDictionary.SymmetricKeyType.Value));
                return result;
            }

            bool TryParsePublicKeyElement(XmlElement element)
            {
                if (element == null)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("element");

                return element.LocalName == this.DriverDictionary.KeyType.Value
                    && element.NamespaceURI == this.DriverDictionary.Namespace.Value
                    && element.InnerText == this.DriverDictionary.PublicKeyType.Value;
            }

            XmlElement CreatePublicKeyTypeElement()
            {
                XmlDocument doc = new XmlDocument();
                XmlElement result = doc.CreateElement(this.DriverDictionary.Prefix.Value, this.DriverDictionary.KeyType.Value,
                    this.DriverDictionary.Namespace.Value);
                result.AppendChild(doc.CreateTextNode(this.DriverDictionary.PublicKeyType.Value));
                return result;
            }

            public override bool TryParseTokenTypeElement(XmlElement element, out string tokenType)
            {
                if (element == null)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("element");

                if (element.LocalName == this.DriverDictionary.TokenType.Value
                    && element.NamespaceURI == this.DriverDictionary.Namespace.Value)
                {
                    tokenType = element.InnerText;
                    return true;
                }

                tokenType = null;
                return false;
            }

            public override XmlElement CreateTokenTypeElement(string tokenTypeUri)
            {
                if (tokenTypeUri == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("tokenTypeUri");
                }
                XmlDocument doc = new XmlDocument();
                XmlElement result = doc.CreateElement(this.DriverDictionary.Prefix.Value, this.DriverDictionary.TokenType.Value,
                    this.DriverDictionary.Namespace.Value);
                result.AppendChild(doc.CreateTextNode(tokenTypeUri));
                return result;
            }

            public override XmlElement CreateUseKeyElement(SecurityKeyIdentifier keyIdentifier, SecurityStandardsManager standardsManager)
            {
                if (keyIdentifier == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("keyIdentifier");
                }
                if (standardsManager == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("standardsManager");
                }
                XmlDocument doc = new XmlDocument();
                XmlElement result = doc.CreateElement(this.DriverDictionary.UseKey.Value, this.DriverDictionary.Namespace.Value);
                MemoryStream stream = new MemoryStream();
                using (XmlDictionaryWriter writer = XmlDictionaryWriter.CreateDictionaryWriter(new XmlTextWriter(stream, Encoding.UTF8)))
                {
#pragma warning suppress 56506 // standardsManager.SecurityTokenSerializer can never be null.
                    standardsManager.SecurityTokenSerializer.WriteKeyIdentifier(writer, keyIdentifier);
                    writer.Flush();
                    stream.Seek(0, SeekOrigin.Begin);
                    XmlNode skiNode;
                    using (XmlDictionaryReader reader = XmlDictionaryReader.CreateDictionaryReader(new XmlTextReader(stream)))
                    {
                        reader.MoveToContent();
                        skiNode = doc.ReadNode(reader);
                    }
                    result.AppendChild(skiNode);
                }
                return result;
            }

            public override XmlElement CreateSignWithElement(string signatureAlgorithm)
            {
                if (signatureAlgorithm == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("signatureAlgorithm");
                }
                XmlDocument doc = new XmlDocument();
                XmlElement result = doc.CreateElement(this.DriverDictionary.Prefix.Value, this.DriverDictionary.SignWith.Value,
                    this.DriverDictionary.Namespace.Value);
                result.AppendChild(doc.CreateTextNode(signatureAlgorithm));
                return result;
            }

            internal override bool IsSignWithElement(XmlElement element, out string signatureAlgorithm)
            {
                return CheckElement(element, this.DriverDictionary.SignWith.Value, this.DriverDictionary.Namespace.Value, out signatureAlgorithm);
            }

            public override XmlElement CreateEncryptWithElement(string encryptionAlgorithm)
            {
                if (encryptionAlgorithm == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("encryptionAlgorithm");
                }
                XmlDocument doc = new XmlDocument();
                XmlElement result = doc.CreateElement(this.DriverDictionary.Prefix.Value, this.DriverDictionary.EncryptWith.Value,
                    this.DriverDictionary.Namespace.Value);
                result.AppendChild(doc.CreateTextNode(encryptionAlgorithm));
                return result;
            }

            public override XmlElement CreateEncryptionAlgorithmElement(string encryptionAlgorithm)
            {
                if (encryptionAlgorithm == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("encryptionAlgorithm");
                }
                XmlDocument doc = new XmlDocument();
                XmlElement result = doc.CreateElement(this.DriverDictionary.Prefix.Value, this.DriverDictionary.EncryptionAlgorithm.Value,
                    this.DriverDictionary.Namespace.Value);
                result.AppendChild(doc.CreateTextNode(encryptionAlgorithm));
                return result;
            }

            internal override bool IsEncryptWithElement(XmlElement element, out string encryptWithAlgorithm)
            {
                return CheckElement(element, this.DriverDictionary.EncryptWith.Value, this.DriverDictionary.Namespace.Value, out encryptWithAlgorithm);
            }

            internal override bool IsEncryptionAlgorithmElement(XmlElement element, out string encryptionAlgorithm)
            {
                return CheckElement(element, this.DriverDictionary.EncryptionAlgorithm.Value, this.DriverDictionary.Namespace.Value, out encryptionAlgorithm);
            }

            public override XmlElement CreateComputedKeyAlgorithmElement(string algorithm)
            {
                if (algorithm == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("algorithm");
                }
                XmlDocument doc = new XmlDocument();
                XmlElement result = doc.CreateElement(this.DriverDictionary.Prefix.Value, this.DriverDictionary.ComputedKeyAlgorithm.Value,
                    this.DriverDictionary.Namespace.Value);
                result.AppendChild(doc.CreateTextNode(algorithm));
                return result;
            }

            public override XmlElement CreateCanonicalizationAlgorithmElement(string algorithm)
            {
                if (algorithm == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("algorithm");
                }
                XmlDocument doc = new XmlDocument();
                XmlElement result = doc.CreateElement(this.DriverDictionary.Prefix.Value, this.DriverDictionary.CanonicalizationAlgorithm.Value,
                    this.DriverDictionary.Namespace.Value);
                result.AppendChild(doc.CreateTextNode(algorithm));
                return result;
            }

            internal override bool IsCanonicalizationAlgorithmElement(XmlElement element, out string canonicalizationAlgorithm)
            {
                return CheckElement(element, this.DriverDictionary.CanonicalizationAlgorithm.Value, this.DriverDictionary.Namespace.Value, out canonicalizationAlgorithm);
            }

            public override bool TryParseRequiredClaimsElement(XmlElement element, out System.Collections.ObjectModel.Collection<XmlElement> requiredClaims)
            {
                if (element == null)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("element");

                if (element.LocalName == this.DriverDictionary.Claims.Value
                    && element.NamespaceURI == this.DriverDictionary.Namespace.Value)
                {
                    requiredClaims = new System.Collections.ObjectModel.Collection<XmlElement>();
                    foreach (XmlNode node in element.ChildNodes)
                        if (node is XmlElement)
                        {
                            // PreSharp Bug: Parameter 'requiredClaims' to this public method must be validated: A null-dereference can occur here.
#pragma warning suppress 56506
                            requiredClaims.Add((XmlElement)node);
                        }
                    return true;
                }

                requiredClaims = null;
                return false;
            }

            public override XmlElement CreateRequiredClaimsElement(IEnumerable<XmlElement> claimsList)
            {
                if (claimsList == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("claimsList");
                }
                XmlDocument doc = new XmlDocument();
                XmlElement result = doc.CreateElement(this.DriverDictionary.Prefix.Value, this.DriverDictionary.Claims.Value,
                    this.DriverDictionary.Namespace.Value);
                foreach (XmlElement claimElement in claimsList)
                {
                    XmlElement element = (XmlElement)doc.ImportNode(claimElement, true);
                    result.AppendChild(element);
                }
                return result;
            }

            internal static void ValidateRequestedKeySize(int keySize, SecurityAlgorithmSuite algorithmSuite)
            {
                if ((keySize % 8 == 0) && algorithmSuite.IsSymmetricKeyLengthSupported(keySize))
                {
                    return;
                }
                else
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new SecurityNegotiationException(SR.GetString(SR.InvalidKeyLengthRequested, keySize)));
                }
            }

            static void ValidateRequestorEntropy(SecurityToken entropy, SecurityKeyEntropyMode mode)
            {
                if ((mode == SecurityKeyEntropyMode.ClientEntropy || mode == SecurityKeyEntropyMode.CombinedEntropy)
                    && (entropy == null))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new InvalidOperationException(SR.GetString(SR.EntropyModeRequiresRequestorEntropy, mode)));
                }
                if (mode == SecurityKeyEntropyMode.ServerEntropy && entropy != null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new InvalidOperationException(SR.GetString(SR.EntropyModeCannotHaveRequestorEntropy, mode)));
                }
            }

            internal static void ProcessRstAndIssueKey(RequestSecurityToken requestSecurityToken, SecurityTokenResolver resolver, SecurityKeyEntropyMode keyEntropyMode, SecurityAlgorithmSuite algorithmSuite, out int issuedKeySize, out byte[] issuerEntropy, out byte[] proofKey,
                out SecurityToken proofToken)
            {
                SecurityToken requestorEntropyToken = requestSecurityToken.GetRequestorEntropy(resolver);
                ValidateRequestorEntropy(requestorEntropyToken, keyEntropyMode);
                byte[] requestorEntropy;
                if (requestorEntropyToken != null)
                {
                    if (requestorEntropyToken is BinarySecretSecurityToken)
                    {
                        BinarySecretSecurityToken skToken = (BinarySecretSecurityToken)requestorEntropyToken;
                        requestorEntropy = skToken.GetKeyBytes();
                    }
                    else if (requestorEntropyToken is WrappedKeySecurityToken)
                    {
                        requestorEntropy = ((WrappedKeySecurityToken)requestorEntropyToken).GetWrappedKey();
                    }
                    else
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new InvalidOperationException(SR.GetString(SR.TokenCannotCreateSymmetricCrypto, requestorEntropyToken)));
                    }
                }
                else
                {
                    requestorEntropy = null;
                }

                if (keyEntropyMode == SecurityKeyEntropyMode.ClientEntropy)
                {
                    if (requestorEntropy != null)
                    {
                        // validate that the entropy length matches the algorithm suite
                        ValidateRequestedKeySize(requestorEntropy.Length * 8, algorithmSuite);
                    }
                    proofKey = requestorEntropy;
                    issuerEntropy = null;
                    issuedKeySize = 0;
                    proofToken = null;
                }
                else
                {
                    if (requestSecurityToken.KeySize != 0)
                    {
                        ValidateRequestedKeySize(requestSecurityToken.KeySize, algorithmSuite);
                        issuedKeySize = requestSecurityToken.KeySize;
                    }
                    else
                    {
                        issuedKeySize = algorithmSuite.DefaultSymmetricKeyLength;
                    }
                    RNGCryptoServiceProvider random = new RNGCryptoServiceProvider();
                    if (keyEntropyMode == SecurityKeyEntropyMode.ServerEntropy)
                    {
                        proofKey = new byte[issuedKeySize / 8];
                        // proof key is completely issued by the server
                        random.GetNonZeroBytes(proofKey);
                        issuerEntropy = null;
                        proofToken = new BinarySecretSecurityToken(proofKey);
                    }
                    else
                    {
                        issuerEntropy = new byte[issuedKeySize / 8];
                        random.GetNonZeroBytes(issuerEntropy);
                        proofKey = RequestSecurityTokenResponse.ComputeCombinedKey(requestorEntropy, issuerEntropy, issuedKeySize);
                        proofToken = null;
                    }
                }
            }

        }

        protected static bool CheckElement(XmlElement element, string name, string ns, out string value)
        {
            value = null;
            if (element.LocalName != name || element.NamespaceURI != ns)
                return false;
            if (element.FirstChild is XmlText)
            {
                value = ((XmlText)element.FirstChild).Value;
                return true;
            }
            return false;
        }
    }
}
