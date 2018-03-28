//-----------------------------------------------------------------------
// <copyright file="WSTrustSerializationHelper.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace System.IdentityModel.Protocols.WSTrust
{
    using System.Collections.Generic;
    using System.Globalization;
    using System.IdentityModel.Protocols.WSFederation;
    using System.IdentityModel.Tokens;
    using System.IO;
    using System.Runtime.Remoting.Metadata.W3cXsd2001;
    using System.ServiceModel.Security.Tokens;
    using System.Text;
    using System.Xml;

    internal class WSTrustSerializationHelper
    {
        public static RequestSecurityToken CreateRequest(XmlReader reader, WSTrustSerializationContext context, WSTrustRequestSerializer requestSerializer, WSTrustConstantsAdapter trustConstants)
        {
            if (reader == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("reader");
            }

            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");
            }

            if (requestSerializer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("requestSerializer");
            }

            if (trustConstants == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("trustConstants");
            }

            if (!reader.IsStartElement(trustConstants.Elements.RequestSecurityToken, trustConstants.NamespaceURI))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new WSTrustSerializationException(SR.GetString(SR.ID3032, reader.LocalName, reader.NamespaceURI, trustConstants.Elements.RequestSecurityToken, trustConstants.NamespaceURI)));
            }

            bool isEmptyElement = reader.IsEmptyElement;
            RequestSecurityToken rst = requestSerializer.CreateRequestSecurityToken();
            rst.Context = reader.GetAttribute(trustConstants.Attributes.Context);
            reader.Read();
            if (!isEmptyElement)
            {
                while (reader.IsStartElement())
                {
                    requestSerializer.ReadXmlElement(reader, rst, context);
                }

                reader.ReadEndElement();
            }

            requestSerializer.Validate(rst);

            return rst;
        }

        public static void ReadRSTXml(XmlReader reader, RequestSecurityToken rst, WSTrustSerializationContext context, WSTrustConstantsAdapter trustConstants)
        {
            if (reader == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("reader");
            }

            if (rst == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("rst");
            }

            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");
            }

            if (trustConstants == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("trustConstants");
            }

            bool isEmptyElement = false;

            if (reader.IsStartElement(trustConstants.Elements.TokenType, trustConstants.NamespaceURI))
            {
                rst.TokenType = reader.ReadElementContentAsString();
                if (!UriUtil.CanCreateValidUri(rst.TokenType, UriKind.Absolute))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new WSTrustSerializationException(SR.GetString(SR.ID3135, trustConstants.Elements.TokenType, trustConstants.NamespaceURI, rst.TokenType)));
                }

                return;
            }

            if (reader.IsStartElement(trustConstants.Elements.RequestType, trustConstants.NamespaceURI))
            {
                rst.RequestType = WSTrustSerializationHelper.ReadRequestType(reader, trustConstants);
                return;
            }

            if (reader.IsStartElement(WSPolicyConstants.ElementNames.AppliesTo, WSPolicyConstants.NamespaceURI))
            {
                rst.AppliesTo = WSTrustSerializationHelper.ReadAppliesTo(reader, trustConstants);
                return;
            }

            if (reader.IsStartElement(trustConstants.Elements.Issuer, trustConstants.NamespaceURI))
            {
                rst.Issuer = WSTrustSerializationHelper.ReadOnBehalfOfIssuer(reader, trustConstants);
                return;
            }

            if (reader.IsStartElement(trustConstants.Elements.ProofEncryption, trustConstants.NamespaceURI))
            {
                if (!reader.IsEmptyElement)
                {
                    rst.ProofEncryption = new SecurityTokenElement(WSTrustSerializationHelper.ReadInnerXml(reader), context.SecurityTokenHandlers);
                }

                if (rst.ProofEncryption == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new WSTrustSerializationException(SR.GetString(SR.ID3218)));
                }

                return;
            }

            if (reader.IsStartElement(trustConstants.Elements.Encryption, trustConstants.NamespaceURI))
            {
                if (!reader.IsEmptyElement)
                {
                    rst.Encryption = new SecurityTokenElement(WSTrustSerializationHelper.ReadInnerXml(reader), context.SecurityTokenHandlers);
                }

                if (rst.Encryption == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new WSTrustSerializationException(SR.GetString(SR.ID3268)));
                }

                return;
            }

            if (reader.IsStartElement(trustConstants.Elements.DelegateTo, trustConstants.NamespaceURI))
            {
                if (!reader.IsEmptyElement)
                {
                    rst.DelegateTo = new SecurityTokenElement(WSTrustSerializationHelper.ReadInnerXml(reader), context.SecurityTokenHandlers);
                }

                if (rst.DelegateTo == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new WSTrustSerializationException(SR.GetString(SR.ID3219)));
                }

                return;
            }

            if (reader.IsStartElement(trustConstants.Elements.Claims, trustConstants.NamespaceURI))
            {
                // According to trust specification, Trust13 requires Claims\@Dialect attribute but not TrustFeb2005.
                // Even for Trust13, the Dialect Uri is open.  After research, "http://schemas.xmlsoap.org/ws/2005/05/identity"
                // seems to be the most common and IDFx will use that if none defined.
                // Our implementation is, for reading/writing, we will be looking specifically for 
                // "http://docs.oasis-open.org/wsfed/authorization/200706/authclaims" (as defined in ws-federation)
                // and fallback to "http://schemas.xmlsoap.org/ws/2005/05/identity" for others.
                // This would also tolerate WCF Orcas which send us "http://schemas.xmlsoap.org/ws/2005/05/IdentityClaims" 
                // as dialect.
                rst.Claims.Dialect = reader.GetAttribute(trustConstants.Attributes.Dialect);
                if ((rst.Claims.Dialect != null) && !UriUtil.CanCreateValidUri(rst.Claims.Dialect, UriKind.Absolute))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new WSTrustSerializationException(SR.GetString(SR.ID3136, trustConstants.Attributes.Dialect, reader.LocalName, reader.NamespaceURI, rst.Claims.Dialect)));
                }

                string ns = WSTrustSerializationHelper.GetRequestClaimNamespace(rst.Claims.Dialect);

                isEmptyElement = reader.IsEmptyElement;
                reader.ReadStartElement(trustConstants.Elements.Claims, trustConstants.NamespaceURI);
                if (!isEmptyElement)
                {
                    while (reader.IsStartElement(WSIdentityConstants.Elements.ClaimType, ns))
                    {
                        isEmptyElement = reader.IsEmptyElement;
                        string claimType = reader.GetAttribute(WSIdentityConstants.Attributes.Uri);
                        if (string.IsNullOrEmpty(claimType))
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new WSTrustSerializationException(SR.GetString(SR.ID3009)));
                        }

                        bool isOptional = false;

                        string optionalAttributeVal = reader.GetAttribute(WSIdentityConstants.Attributes.Optional);
                        if (!string.IsNullOrEmpty(optionalAttributeVal))
                        {
                            isOptional = XmlConvert.ToBoolean(optionalAttributeVal);
                        }

                        reader.Read();
                        reader.MoveToContent();

                        string value = null;
                        if (!isEmptyElement)
                        {
                            if (reader.IsStartElement(WSAuthorizationConstants.Elements.Value, ns))
                            {
                                if (!StringComparer.Ordinal.Equals(rst.Claims.Dialect, WSAuthorizationConstants.Dialect))
                                {
                                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new WSTrustSerializationException(SR.GetString(SR.ID3258, rst.Claims.Dialect, WSAuthorizationConstants.Dialect)));
                                }
                                else
                                {
                                    // Value only supported for ws-federation authclaims
                                    value = reader.ReadElementContentAsString(WSAuthorizationConstants.Elements.Value, ns);
                                }
                            }

                            reader.ReadEndElement();
                        }

                        rst.Claims.Add(new RequestClaim(claimType, isOptional, value));
                    }

                    reader.ReadEndElement();
                }

                return;
            }

            if (reader.IsStartElement(trustConstants.Elements.Entropy, trustConstants.NamespaceURI))
            {
                isEmptyElement = reader.IsEmptyElement;

                reader.ReadStartElement(trustConstants.Elements.Entropy, trustConstants.NamespaceURI);
                if (!isEmptyElement)
                {
                    ProtectedKey protectedKey = ReadProtectedKey(reader, context, trustConstants);

                    if (protectedKey == null)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new WSTrustSerializationException(SR.GetString(SR.ID3026)));
                    }

                    rst.Entropy = new Entropy(protectedKey);

                    reader.ReadEndElement();
                }

                if (rst.Entropy == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new WSTrustSerializationException(SR.GetString(SR.ID3026)));
                }

                return;
            }

            if (reader.IsStartElement(trustConstants.Elements.BinaryExchange, trustConstants.NamespaceURI))
            {
                rst.BinaryExchange = ReadBinaryExchange(reader, trustConstants);
                return;
            }

            if (reader.IsStartElement(trustConstants.Elements.Lifetime, trustConstants.NamespaceURI))
            {
                rst.Lifetime = WSTrustSerializationHelper.ReadLifetime(reader, trustConstants);
                return;
            }

            if (reader.IsStartElement(trustConstants.Elements.RenewTarget, trustConstants.NamespaceURI))
            {
                isEmptyElement = reader.IsEmptyElement;

                if (!isEmptyElement)
                {
                    rst.RenewTarget = new SecurityTokenElement(WSTrustSerializationHelper.ReadInnerXml(reader), context.SecurityTokenHandlers);
                }

                if (rst.RenewTarget == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new WSTrustSerializationException(SR.GetString(SR.ID3151)));
                }

                return;
            }

            if (reader.IsStartElement(trustConstants.Elements.OnBehalfOf, trustConstants.NamespaceURI))
            {
                if (!reader.IsEmptyElement)
                {
                    // Check that we have the SecurityTokenHandlerCollection that we need for OnBehalfOf. If not, then fail now.
                    if (context.SecurityTokenHandlerCollectionManager.ContainsKey(SecurityTokenHandlerCollectionManager.Usage.OnBehalfOf))
                    {
                        rst.OnBehalfOf = new SecurityTokenElement(WSTrustSerializationHelper.ReadInnerXml(reader), context.SecurityTokenHandlerCollectionManager[SecurityTokenHandlerCollectionManager.Usage.OnBehalfOf]);
                    }
                    else
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new WSTrustSerializationException(SR.GetString(SR.ID3264)));
                    }
                }

                if (rst.OnBehalfOf == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new WSTrustSerializationException(SR.GetString(SR.ID3152)));
                }

                return;
            }

            if (reader.IsStartElement(WSTrust14Constants.ElementNames.ActAs, WSTrust14Constants.NamespaceURI))
            {
                if (!reader.IsEmptyElement)
                {
                    // Check that we have the SecurityTokenHandlerCollection that we need for ActAs. If not, then fail now.
                    if (context.SecurityTokenHandlerCollectionManager.ContainsKey(SecurityTokenHandlerCollectionManager.Usage.ActAs))
                    {
                        rst.ActAs = new SecurityTokenElement(WSTrustSerializationHelper.ReadInnerXml(reader), context.SecurityTokenHandlerCollectionManager[SecurityTokenHandlerCollectionManager.Usage.ActAs]);
                    }
                    else
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new WSTrustSerializationException(SR.GetString(SR.ID3265)));
                    }
                }

                if (rst.ActAs == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new WSTrustSerializationException(SR.GetString(SR.ID3153)));
                }

                return;
            }

            if (reader.IsStartElement(trustConstants.Elements.KeyType, trustConstants.NamespaceURI))
            {
                rst.KeyType = WSTrustSerializationHelper.ReadKeyType(reader, trustConstants);
                return;
            }

            if (reader.IsStartElement(trustConstants.Elements.KeySize, trustConstants.NamespaceURI))
            {
                if (!reader.IsEmptyElement)
                {
                    rst.KeySizeInBits = int.Parse(reader.ReadElementContentAsString(), CultureInfo.InvariantCulture);
                }

                if (rst.KeySizeInBits == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new WSTrustSerializationException(SR.GetString(SR.ID3154)));
                }

                return;
            }

            if (reader.IsStartElement(trustConstants.Elements.UseKey, trustConstants.NamespaceURI))
            {
                isEmptyElement = reader.IsEmptyElement;
                reader.ReadStartElement();

                if (!isEmptyElement)
                {
                    if (!context.SecurityTokenHandlers.CanReadToken(reader))
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new WSTrustSerializationException(SR.GetString(SR.ID3165)));
                    }

                    SecurityToken originalUseKeyToken = context.SecurityTokenHandlers.ReadToken(reader);
                    SecurityKeyIdentifier useKeySki = new SecurityKeyIdentifier();

                    if (originalUseKeyToken.CanCreateKeyIdentifierClause<RsaKeyIdentifierClause>())
                    {
                        useKeySki.Add(originalUseKeyToken.CreateKeyIdentifierClause<RsaKeyIdentifierClause>());
                    }
                    else if (originalUseKeyToken.CanCreateKeyIdentifierClause<X509RawDataKeyIdentifierClause>())
                    {
                        useKeySki.Add(originalUseKeyToken.CreateKeyIdentifierClause<X509RawDataKeyIdentifierClause>());
                    }
                    else
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new WSTrustSerializationException(SR.GetString(SR.ID3166)));
                    }

                    // Ensure that the provided UseKey SKI can be resolved by the UseKeyTokenResolver.
                    // This provides proof of possession because the keys in that resolver are ones that the client has used for signature.
                    SecurityToken resolvedUseKeyToken;

                    if (!context.UseKeyTokenResolver.TryResolveToken(useKeySki, out resolvedUseKeyToken))
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidRequestException(SR.GetString(SR.ID3092, useKeySki)));
                    }

                    rst.UseKey = new UseKey(useKeySki, resolvedUseKeyToken);

                    reader.ReadEndElement();
                }

                if (rst.UseKey == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new WSTrustSerializationException(SR.GetString(SR.ID3155)));
                }

                return;
            }

            if (reader.IsStartElement(trustConstants.Elements.SignWith, trustConstants.NamespaceURI))
            {
                rst.SignWith = reader.ReadElementContentAsString();
                if (!UriUtil.CanCreateValidUri(rst.SignWith, UriKind.Absolute))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new WSTrustSerializationException(SR.GetString(SR.ID3135, trustConstants.Elements.SignWith, trustConstants.NamespaceURI, rst.SignWith)));
                }

                return;
            }

            if (reader.IsStartElement(trustConstants.Elements.EncryptWith, trustConstants.NamespaceURI))
            {
                rst.EncryptWith = reader.ReadElementContentAsString();
                if (!UriUtil.CanCreateValidUri(rst.EncryptWith, UriKind.Absolute))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new WSTrustSerializationException(SR.GetString(SR.ID3135, trustConstants.Elements.EncryptWith, trustConstants.NamespaceURI, rst.EncryptWith)));
                }
                
                return;
            }

            if (reader.IsStartElement(trustConstants.Elements.ComputedKeyAlgorithm, trustConstants.NamespaceURI))
            {
                rst.ComputedKeyAlgorithm = ReadComputedKeyAlgorithm(reader, trustConstants);
                return;
            }

            if (reader.IsStartElement(trustConstants.Elements.AuthenticationType, trustConstants.NamespaceURI))
            {
                rst.AuthenticationType = reader.ReadElementContentAsString(trustConstants.Elements.AuthenticationType, trustConstants.NamespaceURI);
                if (!UriUtil.CanCreateValidUri(rst.AuthenticationType, UriKind.Absolute))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new WSTrustSerializationException(SR.GetString(SR.ID3135, trustConstants.Elements.AuthenticationType, trustConstants.NamespaceURI, rst.AuthenticationType)));
                }

                return;
            }

            if (reader.IsStartElement(trustConstants.Elements.EncryptionAlgorithm, trustConstants.NamespaceURI))
            {
                rst.EncryptionAlgorithm = reader.ReadElementContentAsString(trustConstants.Elements.EncryptionAlgorithm, trustConstants.NamespaceURI);
                if (!UriUtil.CanCreateValidUri(rst.EncryptionAlgorithm, UriKind.Absolute))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new WSTrustSerializationException(SR.GetString(SR.ID3135, trustConstants.Elements.EncryptionAlgorithm, trustConstants.NamespaceURI, rst.EncryptionAlgorithm)));
                }
                
                return;
            }

            if (reader.IsStartElement(trustConstants.Elements.CanonicalizationAlgorithm, trustConstants.NamespaceURI))
            {
                rst.CanonicalizationAlgorithm = reader.ReadElementContentAsString(trustConstants.Elements.CanonicalizationAlgorithm, trustConstants.NamespaceURI);
                if (!UriUtil.CanCreateValidUri(rst.CanonicalizationAlgorithm, UriKind.Absolute))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new WSTrustSerializationException(SR.GetString(SR.ID3135, trustConstants.Elements.CanonicalizationAlgorithm, trustConstants.NamespaceURI, rst.CanonicalizationAlgorithm)));
                }
                
                return;
            }

            if (reader.IsStartElement(trustConstants.Elements.SignatureAlgorithm, trustConstants.NamespaceURI))
            {
                rst.SignatureAlgorithm = reader.ReadElementContentAsString(trustConstants.Elements.SignatureAlgorithm, trustConstants.NamespaceURI);
                if (!UriUtil.CanCreateValidUri(rst.SignatureAlgorithm, UriKind.Absolute))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new WSTrustSerializationException(SR.GetString(SR.ID3135, trustConstants.Elements.SignatureAlgorithm, trustConstants.NamespaceURI, rst.SignatureAlgorithm)));
                }
                
                return;
            }

            if (reader.IsStartElement(trustConstants.Elements.Forwardable, trustConstants.NamespaceURI))
            {
                rst.Forwardable = reader.ReadElementContentAsBoolean();
                return;
            }

            if (reader.IsStartElement(trustConstants.Elements.Delegatable, trustConstants.NamespaceURI))
            {
                rst.Delegatable = reader.ReadElementContentAsBoolean();
                return;
            }

            if (reader.IsStartElement(trustConstants.Elements.AllowPostdating, trustConstants.NamespaceURI))
            {
                rst.AllowPostdating = true;
                isEmptyElement = reader.IsEmptyElement;
                reader.Read();
                reader.MoveToContent();
                if (!isEmptyElement)
                {
                    reader.ReadEndElement();
                }

                return;
            }

            if (reader.IsStartElement(trustConstants.Elements.Renewing, trustConstants.NamespaceURI))
            {
                isEmptyElement = reader.IsEmptyElement;
                string attrValue = reader.GetAttribute(trustConstants.Attributes.Allow);
                bool allowRenewal = true;
                bool renewalAfterExpiration = false;
                if (!string.IsNullOrEmpty(attrValue))
                {
                    allowRenewal = XmlConvert.ToBoolean(attrValue);
                }

                attrValue = reader.GetAttribute(trustConstants.Attributes.OK);
                if (!string.IsNullOrEmpty(attrValue))
                {
                    renewalAfterExpiration = XmlConvert.ToBoolean(attrValue);
                }

                rst.Renewing = new Renewing(allowRenewal, renewalAfterExpiration);

                reader.Read();
                reader.MoveToContent();
                if (!isEmptyElement)
                {
                    reader.ReadEndElement();
                }

                return;
            }

            if (reader.IsStartElement(trustConstants.Elements.CancelTarget, trustConstants.NamespaceURI))
            {
                if (!reader.IsEmptyElement)
                {
                    rst.CancelTarget = new SecurityTokenElement(WSTrustSerializationHelper.ReadInnerXml(reader), context.SecurityTokenHandlers);
                }

                if (rst.CancelTarget == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new WSTrustSerializationException(SR.GetString(SR.ID3220)));
                }

                return;
            }

            if (reader.IsStartElement(trustConstants.Elements.Participants, trustConstants.NamespaceURI))
            {
                EndpointReference primary = null;
                List<EndpointReference> participants = new List<EndpointReference>();

                isEmptyElement = reader.IsEmptyElement;

                reader.Read();
                reader.MoveToContent();

                if (!isEmptyElement)
                {
                    if (reader.IsStartElement(trustConstants.Elements.Primary, trustConstants.NamespaceURI))
                    {
                        reader.ReadStartElement(trustConstants.Elements.Primary, trustConstants.NamespaceURI);
                        primary = EndpointReference.ReadFrom(XmlDictionaryReader.CreateDictionaryReader(reader));
                        reader.ReadEndElement();
                    }

                    while (reader.IsStartElement(trustConstants.Elements.Participant, trustConstants.NamespaceURI))
                    {
                        reader.ReadStartElement(trustConstants.Elements.Participant, trustConstants.NamespaceURI);
                        participants.Add(EndpointReference.ReadFrom(XmlDictionaryReader.CreateDictionaryReader(reader)));
                        reader.ReadEndElement();
                    }

                    if (reader.IsStartElement())
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new WSTrustSerializationException(SR.GetString(SR.ID3223, trustConstants.Elements.Participants, trustConstants.NamespaceURI, reader.LocalName, reader.NamespaceURI)));
                    }

                    rst.Participants = new Participants();
                    rst.Participants.Primary = primary;
                    rst.Participants.Participant.AddRange(participants);

                    reader.ReadEndElement();
                }

                return;
            }

            if (reader.IsStartElement(WSAuthorizationConstants.Elements.AdditionalContext, WSAuthorizationConstants.Namespace))
            {
                rst.AdditionalContext = new AdditionalContext();

                isEmptyElement = reader.IsEmptyElement;
                reader.Read();
                reader.MoveToContent();

                if (!isEmptyElement)
                {
                    while (reader.IsStartElement(WSAuthorizationConstants.Elements.ContextItem, WSAuthorizationConstants.Namespace))
                    {
                        Uri name = null;
                        Uri scope = null;
                        string value = null;
                        string attrValue = reader.GetAttribute(WSAuthorizationConstants.Attributes.Name);
                        if (string.IsNullOrEmpty(attrValue) || !UriUtil.TryCreateValidUri(attrValue, UriKind.Absolute, out name))
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new WSTrustSerializationException(
                                SR.GetString(SR.ID3136, WSAuthorizationConstants.Attributes.Name, reader.LocalName, reader.NamespaceURI, attrValue)));
                        }

                        attrValue = reader.GetAttribute(WSAuthorizationConstants.Attributes.Scope);
                        if (!string.IsNullOrEmpty(attrValue) && !UriUtil.TryCreateValidUri(attrValue, UriKind.Absolute, out scope))
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new WSTrustSerializationException(
                                SR.GetString(SR.ID3136, WSAuthorizationConstants.Attributes.Scope, reader.LocalName, reader.NamespaceURI, attrValue)));
                        }

                        if (reader.IsEmptyElement)
                        {
                            reader.Read();
                        }
                        else
                        {
                            reader.Read();
                            if (reader.IsStartElement(WSAuthorizationConstants.Elements.Value, WSAuthorizationConstants.Namespace))
                            {
                                value = reader.ReadElementContentAsString(WSAuthorizationConstants.Elements.Value, WSAuthorizationConstants.Namespace);
                            }

                            reader.ReadEndElement();
                        }

                        rst.AdditionalContext.Items.Add(new ContextItem(name, value, scope));
                    }

                    if (reader.IsStartElement())
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new WSTrustSerializationException(SR.GetString(SR.ID3223, WSAuthorizationConstants.Elements.AdditionalContext, WSAuthorizationConstants.Namespace, reader.LocalName, reader.NamespaceURI)));
                    }

                    reader.ReadEndElement();
                }

                return;
            }

            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new WSTrustSerializationException(SR.GetString(SR.ID3007, reader.LocalName, reader.NamespaceURI)));
        }

        public static void WriteRequest(RequestSecurityToken rst, XmlWriter writer, WSTrustSerializationContext context, WSTrustRequestSerializer requestSerializer, WSTrustConstantsAdapter trustConstants)
        {
            if (rst == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("rst");
            }

            if (writer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("writer");
            }

            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");
            }

            if (requestSerializer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("requestSerializer");
            }

            if (trustConstants == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("trustConstants");
            }

            requestSerializer.Validate(rst);

            writer.WriteStartElement(trustConstants.Prefix, trustConstants.Elements.RequestSecurityToken, trustConstants.NamespaceURI);

            // Step 2: Write the first class attribute, i.e. Context
            // IDFX beta work item: use the handler to write out the context as well
            if (rst.Context != null)
            {
                writer.WriteAttributeString(trustConstants.Attributes.Context, (string)rst.Context);
            }

            // Step 3: Write the custom attributes here from the Attributes bag.
            // IDFX beta work item bug 878

            // Step 4: Write the first class Element here
            requestSerializer.WriteKnownRequestElement(rst, writer, context);

            // Step 5: Write the custom elements here from the Elements bag
            foreach (KeyValuePair<string, object> messageParam in rst.Properties)
            {
                requestSerializer.WriteXmlElement(writer, messageParam.Key, messageParam.Value, rst, context);
            }

            // Step 6: close the RST element
            writer.WriteEndElement();
        }

        public static void WriteKnownRequestElement(RequestSecurityToken rst, XmlWriter writer, WSTrustSerializationContext context, WSTrustRequestSerializer requestSerializer, WSTrustConstantsAdapter trustConstants)
        {
            if (rst == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("rst");
            }

            if (writer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("writer");
            }

            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");
            }

            if (requestSerializer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("requestSerializer");
            }

            if (trustConstants == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("trustConstants");
            }

            if (rst.AppliesTo != null)
            {
                requestSerializer.WriteXmlElement(writer, WSPolicyConstants.ElementNames.AppliesTo, rst.AppliesTo, rst, context);
            }

            if (rst.Claims.Count > 0)
            {
                requestSerializer.WriteXmlElement(writer, trustConstants.Elements.Claims, rst.Claims, rst, context);
            }

            if (!string.IsNullOrEmpty(rst.ComputedKeyAlgorithm))
            {
                requestSerializer.WriteXmlElement(writer, trustConstants.Elements.ComputedKeyAlgorithm, rst.ComputedKeyAlgorithm, rst, context);
            }

            if (!string.IsNullOrEmpty(rst.SignWith))
            {
                requestSerializer.WriteXmlElement(writer, trustConstants.Elements.SignWith, rst.SignWith, rst, context);
            }

            if (!string.IsNullOrEmpty(rst.EncryptWith))
            {
                requestSerializer.WriteXmlElement(writer, trustConstants.Elements.EncryptWith, rst.EncryptWith, rst, context);
            }

            if (rst.Entropy != null)
            {
                requestSerializer.WriteXmlElement(writer, trustConstants.Elements.Entropy, rst.Entropy, rst, context);
            }

            if (rst.KeySizeInBits.HasValue)
            {
                requestSerializer.WriteXmlElement(writer, trustConstants.Elements.KeySize, rst.KeySizeInBits, rst, context);
            }

            if (!string.IsNullOrEmpty(rst.KeyType))
            {
                requestSerializer.WriteXmlElement(writer, trustConstants.Elements.KeyType, rst.KeyType, rst, context);
            }

            if (rst.Lifetime != null)
            {
                requestSerializer.WriteXmlElement(writer, trustConstants.Elements.Lifetime, rst.Lifetime, rst, context);
            }

            if (rst.RenewTarget != null)
            {
                requestSerializer.WriteXmlElement(writer, trustConstants.Elements.RenewTarget, rst.RenewTarget, rst, context);
            }

            if (rst.OnBehalfOf != null)
            {
                requestSerializer.WriteXmlElement(writer, trustConstants.Elements.OnBehalfOf, rst.OnBehalfOf, rst, context);
            }

            if (rst.ActAs != null)
            {
                requestSerializer.WriteXmlElement(writer, WSTrust14Constants.ElementNames.ActAs, rst.ActAs, rst, context);
            }

            if (!string.IsNullOrEmpty(rst.RequestType))
            {
                requestSerializer.WriteXmlElement(writer, trustConstants.Elements.RequestType, rst.RequestType, rst, context);
            }

            if (!string.IsNullOrEmpty(rst.TokenType))
            {
                requestSerializer.WriteXmlElement(writer, trustConstants.Elements.TokenType, rst.TokenType, rst, context);
            }

            if (rst.UseKey != null)
            {
                requestSerializer.WriteXmlElement(writer, trustConstants.Elements.UseKey, rst.UseKey, rst, context);
            }

            if (!string.IsNullOrEmpty(rst.AuthenticationType))
            {
                requestSerializer.WriteXmlElement(writer, trustConstants.Elements.AuthenticationType, rst.AuthenticationType, rst, context);
            }

            if (!string.IsNullOrEmpty(rst.EncryptionAlgorithm))
            {
                requestSerializer.WriteXmlElement(writer, trustConstants.Elements.EncryptionAlgorithm, rst.EncryptionAlgorithm, rst, context);
            }

            if (!string.IsNullOrEmpty(rst.CanonicalizationAlgorithm))
            {
                requestSerializer.WriteXmlElement(writer, trustConstants.Elements.CanonicalizationAlgorithm, rst.CanonicalizationAlgorithm, rst, context);
            }

            if (!string.IsNullOrEmpty(rst.SignatureAlgorithm))
            {
                requestSerializer.WriteXmlElement(writer, trustConstants.Elements.SignatureAlgorithm, rst.SignatureAlgorithm, rst, context);
            }

            if (rst.BinaryExchange != null)
            {
                requestSerializer.WriteXmlElement(writer, trustConstants.Elements.BinaryExchange, rst.BinaryExchange, rst, context);
            }

            if (rst.Issuer != null)
            {
                requestSerializer.WriteXmlElement(writer, trustConstants.Elements.Issuer, rst.Issuer, rst, context);
            }

            if (rst.ProofEncryption != null)
            {
                requestSerializer.WriteXmlElement(writer, trustConstants.Elements.ProofEncryption, rst.ProofEncryption, rst, context);
            }

            if (rst.Encryption != null)
            {
                requestSerializer.WriteXmlElement(writer, trustConstants.Elements.Encryption, rst.Encryption, rst, context);
            }

            if (rst.DelegateTo != null)
            {
                requestSerializer.WriteXmlElement(writer, trustConstants.Elements.DelegateTo, rst.DelegateTo, rst, context);
            }

            if (rst.Forwardable != null)
            {
                requestSerializer.WriteXmlElement(writer, trustConstants.Elements.Forwardable, rst.Forwardable.Value, rst, context);
            }

            if (rst.Delegatable != null)
            {
                requestSerializer.WriteXmlElement(writer, trustConstants.Elements.Delegatable, rst.Delegatable.Value, rst, context);
            }

            if (rst.AllowPostdating)
            {
                requestSerializer.WriteXmlElement(writer, trustConstants.Elements.AllowPostdating, rst.AllowPostdating, rst, context);
            }

            if (rst.Renewing != null)
            {
                requestSerializer.WriteXmlElement(writer, trustConstants.Elements.Renewing, rst.Renewing, rst, context);
            }

            if (rst.CancelTarget != null)
            {
                requestSerializer.WriteXmlElement(writer, trustConstants.Elements.CancelTarget, rst.CancelTarget, rst, context);
            }

            if ((rst.Participants != null) && ((rst.Participants.Primary != null) || (rst.Participants.Participant.Count > 0)))
            {
                requestSerializer.WriteXmlElement(writer, trustConstants.Elements.Participants, rst.Participants, rst, context);
            }

            if (rst.AdditionalContext != null)
            {
                requestSerializer.WriteXmlElement(writer, WSAuthorizationConstants.Elements.AdditionalContext, rst.AdditionalContext, rst, context);
            }
        }

        public static void WriteRSTXml(XmlWriter writer, string elementName, object elementValue, WSTrustSerializationContext context, WSTrustConstantsAdapter trustConstants)
        {
            if (writer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("writer");
            }

            if (string.IsNullOrEmpty(elementName))
            {
                throw DiagnosticUtility.ThrowHelperArgumentNullOrEmptyString("elementName");
            }

            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");
            }

            if (trustConstants == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("trustConstants");
            }

            if (StringComparer.Ordinal.Equals(elementName, WSPolicyConstants.ElementNames.AppliesTo))
            {
                EndpointReference appliesTo = elementValue as EndpointReference;
                WSTrustSerializationHelper.WriteAppliesTo(writer, appliesTo, trustConstants);
                return;
            }

            if (StringComparer.Ordinal.Equals(elementName, trustConstants.Elements.Claims))
            {
                writer.WriteStartElement(trustConstants.Prefix, trustConstants.Elements.Claims, trustConstants.NamespaceURI);
                RequestClaimCollection claims = (RequestClaimCollection)elementValue;
                if ((claims.Dialect != null) && !UriUtil.CanCreateValidUri(claims.Dialect, UriKind.Absolute))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new WSTrustSerializationException(SR.GetString(SR.ID3136, trustConstants.Attributes.Dialect, trustConstants.Elements.Claims, trustConstants.NamespaceURI, claims.Dialect)));
                }

                string ns = WSTrustSerializationHelper.GetRequestClaimNamespace(claims.Dialect);
                string prefix = writer.LookupPrefix(ns);
                if (string.IsNullOrEmpty(prefix))
                {
                    prefix = WSTrustSerializationHelper.GetRequestClaimPrefix(claims.Dialect);
                    writer.WriteAttributeString("xmlns", prefix, null, ns);
                }
                
                writer.WriteAttributeString(trustConstants.Attributes.Dialect, !string.IsNullOrEmpty(claims.Dialect) ? claims.Dialect : WSIdentityConstants.Dialect);
                foreach (RequestClaim claim in claims)
                {
                    writer.WriteStartElement(prefix, WSIdentityConstants.Elements.ClaimType, ns);
                    writer.WriteAttributeString(WSIdentityConstants.Attributes.Uri, claim.ClaimType);
                    writer.WriteAttributeString(WSIdentityConstants.Attributes.Optional, claim.IsOptional ? "true" : "false");
                    if (claim.Value != null)
                    {
                        if (StringComparer.Ordinal.Equals(claims.Dialect, WSAuthorizationConstants.Dialect))
                        {
                            writer.WriteElementString(prefix, WSAuthorizationConstants.Elements.Value, ns, claim.Value);
                        }
                        else
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new WSTrustSerializationException(SR.GetString(SR.ID3257, claims.Dialect, WSAuthorizationConstants.Dialect)));
                        }
                    }

                    writer.WriteEndElement();
                }

                writer.WriteEndElement();
                return;
            }

            if (StringComparer.Ordinal.Equals(elementName, trustConstants.Elements.ComputedKeyAlgorithm))
            {
                WriteComputedKeyAlgorithm(writer, trustConstants.Elements.ComputedKeyAlgorithm, (string)elementValue, trustConstants);
                return;
            }

            if (StringComparer.Ordinal.Equals(elementName, trustConstants.Elements.BinaryExchange))
            {
                WriteBinaryExchange(writer, elementValue as BinaryExchange, trustConstants);
                return;
            }

            if (StringComparer.Ordinal.Equals(elementName, trustConstants.Elements.Issuer))
            {
                WriteOnBehalfOfIssuer(writer, elementValue as EndpointReference, trustConstants);
                return;
            }

            if (StringComparer.Ordinal.Equals(elementName, trustConstants.Elements.SignWith))
            {
                if (!UriUtil.CanCreateValidUri((string)elementValue, UriKind.Absolute))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new WSTrustSerializationException(SR.GetString(SR.ID3135, trustConstants.Elements.SignWith, trustConstants.NamespaceURI, (string)elementValue)));
                }

                writer.WriteElementString(trustConstants.Prefix, trustConstants.Elements.SignWith, trustConstants.NamespaceURI, (string)elementValue);
                return;
            }

            if (StringComparer.Ordinal.Equals(elementName, trustConstants.Elements.EncryptWith))
            {
                if (!UriUtil.CanCreateValidUri((string)elementValue, UriKind.Absolute))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new WSTrustSerializationException(SR.GetString(SR.ID3135, trustConstants.Elements.EncryptWith, trustConstants.NamespaceURI, (string)elementValue)));
                }

                writer.WriteElementString(trustConstants.Prefix, trustConstants.Elements.EncryptWith, trustConstants.NamespaceURI, (string)elementValue);
                return;
            }

            if (StringComparer.Ordinal.Equals(elementName, trustConstants.Elements.Entropy))
            {
                Entropy entropy = elementValue as Entropy;
                if (entropy != null)
                {
                    writer.WriteStartElement(trustConstants.Elements.Entropy, trustConstants.NamespaceURI);
                    WriteProtectedKey(writer, entropy, context, trustConstants);
                    writer.WriteEndElement();
                }

                return;
            }

            if (StringComparer.Ordinal.Equals(elementName, trustConstants.Elements.KeySize))
            {
                writer.WriteElementString(trustConstants.Prefix, trustConstants.Elements.KeySize, trustConstants.NamespaceURI, Convert.ToString(((int)elementValue), CultureInfo.InvariantCulture));
                return;
            }

            if (StringComparer.Ordinal.Equals(elementName, trustConstants.Elements.KeyType))
            {
                WSTrustSerializationHelper.WriteKeyType(writer, ((string)elementValue), trustConstants);
                return;
            }

            if (StringComparer.Ordinal.Equals(elementName, trustConstants.Elements.Lifetime))
            {
                Lifetime lifeTime = (Lifetime)elementValue;
                WSTrustSerializationHelper.WriteLifetime(writer, lifeTime, trustConstants);
                return;
            }

            if (StringComparer.Ordinal.Equals(elementName, trustConstants.Elements.RenewTarget))
            {
                SecurityTokenElement tokenElement = elementValue as SecurityTokenElement;
                if (tokenElement == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("elementValue", SR.GetString(SR.ID3222, trustConstants.Elements.RenewTarget, trustConstants.NamespaceURI, typeof(SecurityTokenElement), elementValue));
                }

                writer.WriteStartElement(trustConstants.Prefix, trustConstants.Elements.RenewTarget, trustConstants.NamespaceURI);

                if (tokenElement.SecurityTokenXml != null)
                {
                    tokenElement.SecurityTokenXml.WriteTo(writer);
                }
                else
                {
                    context.SecurityTokenHandlers.WriteToken(writer, tokenElement.GetSecurityToken());
                }

                writer.WriteEndElement();
                return;
            }

            if (StringComparer.Ordinal.Equals(elementName, trustConstants.Elements.OnBehalfOf))
            {
                writer.WriteStartElement(trustConstants.Prefix, trustConstants.Elements.OnBehalfOf, trustConstants.NamespaceURI);
                WriteTokenElement((SecurityTokenElement)elementValue, SecurityTokenHandlerCollectionManager.Usage.OnBehalfOf, context, writer);
                writer.WriteEndElement();

                return;
            }

            if (StringComparer.Ordinal.Equals(elementName, WSTrust14Constants.ElementNames.ActAs))
            {
                writer.WriteStartElement(WSTrust14Constants.Prefix, WSTrust14Constants.ElementNames.ActAs, WSTrust14Constants.NamespaceURI);
                WriteTokenElement((SecurityTokenElement)elementValue, SecurityTokenHandlerCollectionManager.Usage.ActAs, context, writer);
                writer.WriteEndElement();

                return;
            }

            if (StringComparer.Ordinal.Equals(elementName, trustConstants.Elements.RequestType))
            {
                if (!UriUtil.CanCreateValidUri((string)elementValue, UriKind.Absolute))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new WSTrustSerializationException(SR.GetString(SR.ID3135, trustConstants.Elements.RequestType, trustConstants.NamespaceURI, (string)elementValue)));
                }

                WSTrustSerializationHelper.WriteRequestType(writer, (string)elementValue, trustConstants);
                return;
            }

            if (StringComparer.Ordinal.Equals(elementName, trustConstants.Elements.TokenType))
            {
                if (!UriUtil.CanCreateValidUri((string)elementValue, UriKind.Absolute))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new WSTrustSerializationException(SR.GetString(SR.ID3135, trustConstants.Elements.TokenType, trustConstants.NamespaceURI, ((string)elementValue))));
                }

                writer.WriteElementString(trustConstants.Prefix, trustConstants.Elements.TokenType, trustConstants.NamespaceURI, ((string)elementValue));
                return;
            }

            if (StringComparer.Ordinal.Equals(elementName, trustConstants.Elements.UseKey))
            {
                UseKey useKey = (UseKey)elementValue;

                if (useKey.Token == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new WSTrustSerializationException(SR.GetString(SR.ID3012)));
                }

                if (!context.SecurityTokenHandlers.CanWriteToken(useKey.Token))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new WSTrustSerializationException(SR.GetString(SR.ID3017)));
                }

                writer.WriteStartElement(trustConstants.Prefix, trustConstants.Elements.UseKey, trustConstants.NamespaceURI);

                context.SecurityTokenHandlers.WriteToken(writer, useKey.Token);

                writer.WriteEndElement();
                return;
            }

            if (StringComparer.Ordinal.Equals(elementName, trustConstants.Elements.AuthenticationType))
            {
                if (!UriUtil.CanCreateValidUri((string)elementValue, UriKind.Absolute))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new WSTrustSerializationException(SR.GetString(SR.ID3135, trustConstants.Elements.AuthenticationType, trustConstants.NamespaceURI, ((string)elementValue))));
                }

                writer.WriteElementString(trustConstants.Prefix, trustConstants.Elements.AuthenticationType, trustConstants.NamespaceURI, (string)elementValue);
                return;
            }

            if (StringComparer.Ordinal.Equals(elementName, trustConstants.Elements.EncryptionAlgorithm))
            {
                if (!UriUtil.CanCreateValidUri((string)elementValue, UriKind.Absolute))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new WSTrustSerializationException(SR.GetString(SR.ID3135, trustConstants.Elements.EncryptionAlgorithm, trustConstants.NamespaceURI, ((string)elementValue))));
                }

                writer.WriteElementString(trustConstants.Prefix, trustConstants.Elements.EncryptionAlgorithm, trustConstants.NamespaceURI, (string)elementValue);
                return;
            }

            if (StringComparer.Ordinal.Equals(elementName, trustConstants.Elements.CanonicalizationAlgorithm))
            {
                if (!UriUtil.CanCreateValidUri((string)elementValue, UriKind.Absolute))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new WSTrustSerializationException(SR.GetString(SR.ID3135, trustConstants.Elements.CanonicalizationAlgorithm, trustConstants.NamespaceURI, ((string)elementValue))));
                }

                writer.WriteElementString(trustConstants.Prefix, trustConstants.Elements.CanonicalizationAlgorithm, trustConstants.NamespaceURI, (string)elementValue);
                return;
            }

            if (StringComparer.Ordinal.Equals(elementName, trustConstants.Elements.SignatureAlgorithm))
            {
                if (!UriUtil.CanCreateValidUri((string)elementValue, UriKind.Absolute))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new WSTrustSerializationException(SR.GetString(SR.ID3135, trustConstants.Elements.SignatureAlgorithm, trustConstants.NamespaceURI, ((string)elementValue))));
                }

                writer.WriteElementString(trustConstants.Prefix, trustConstants.Elements.SignatureAlgorithm, trustConstants.NamespaceURI, (string)elementValue);
                return;
            }

            if (StringComparer.Ordinal.Equals(elementName, trustConstants.Elements.Encryption))
            {
                SecurityTokenElement token = (SecurityTokenElement)elementValue;

                writer.WriteStartElement(trustConstants.Prefix, trustConstants.Elements.Encryption, trustConstants.NamespaceURI);

                if (token.SecurityTokenXml != null)
                {
                    token.SecurityTokenXml.WriteTo(writer);
                }
                else
                {
                    context.SecurityTokenHandlers.WriteToken(writer, token.GetSecurityToken());
                }

                writer.WriteEndElement();
                return;
            }

            if (StringComparer.Ordinal.Equals(elementName, trustConstants.Elements.ProofEncryption))
            {
                SecurityTokenElement token = (SecurityTokenElement)elementValue;

                writer.WriteStartElement(trustConstants.Prefix, trustConstants.Elements.ProofEncryption, trustConstants.NamespaceURI);

                if (token.SecurityTokenXml != null)
                {
                    token.SecurityTokenXml.WriteTo(writer);
                }
                else
                {
                    context.SecurityTokenHandlers.WriteToken(writer, token.GetSecurityToken());
                }

                writer.WriteEndElement();
                return;
            }

            if (StringComparer.Ordinal.Equals(elementName, trustConstants.Elements.DelegateTo))
            {
                SecurityTokenElement token = (SecurityTokenElement)elementValue;

                writer.WriteStartElement(trustConstants.Prefix, trustConstants.Elements.DelegateTo, trustConstants.NamespaceURI);

                if (token.SecurityTokenXml != null)
                {
                    token.SecurityTokenXml.WriteTo(writer);
                }
                else
                {
                    context.SecurityTokenHandlers.WriteToken(writer, token.GetSecurityToken());
                }

                writer.WriteEndElement();
                return;
            }

            if (StringComparer.Ordinal.Equals(elementName, trustConstants.Elements.Forwardable))
            {
                if (!(elementValue is bool))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("elementValue", SR.GetString(SR.ID3222, trustConstants.Elements.Forwardable, trustConstants.NamespaceURI, typeof(bool), elementValue));
                }

                writer.WriteStartElement(trustConstants.Elements.Forwardable, trustConstants.NamespaceURI);
                writer.WriteString(XmlConvert.ToString((bool)elementValue));
                writer.WriteEndElement();
                return;
            }

            if (StringComparer.Ordinal.Equals(elementName, trustConstants.Elements.Delegatable))
            {
                if (!(elementValue is bool))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("elementValue", SR.GetString(SR.ID3222, trustConstants.Elements.Delegatable, trustConstants.NamespaceURI, typeof(bool), elementValue));
                }

                writer.WriteStartElement(trustConstants.Elements.Delegatable, trustConstants.NamespaceURI);
                writer.WriteString(XmlConvert.ToString((bool)elementValue));
                writer.WriteEndElement();
                return;
            }

            if (StringComparer.Ordinal.Equals(elementName, trustConstants.Elements.AllowPostdating))
            {
                writer.WriteStartElement(trustConstants.Prefix, trustConstants.Elements.AllowPostdating, trustConstants.NamespaceURI);
                writer.WriteEndElement();
                return;
            }

            if (StringComparer.Ordinal.Equals(elementName, trustConstants.Elements.Renewing))
            {
                Renewing renewing = elementValue as Renewing;
                if (renewing == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("elementValue", SR.GetString(SR.ID3222, trustConstants.Elements.Renewing, trustConstants.NamespaceURI, typeof(Renewing), elementValue));
                }

                writer.WriteStartElement(trustConstants.Prefix, trustConstants.Elements.Renewing, trustConstants.NamespaceURI);
                writer.WriteAttributeString(trustConstants.Attributes.Allow, XmlConvert.ToString(renewing.AllowRenewal));
                writer.WriteAttributeString(trustConstants.Attributes.OK, XmlConvert.ToString(renewing.OkForRenewalAfterExpiration));
                writer.WriteEndElement();
                return;
            }

            if (StringComparer.Ordinal.Equals(elementName, trustConstants.Elements.CancelTarget))
            {
                SecurityTokenElement tokenElement = elementValue as SecurityTokenElement;

                if (tokenElement == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("elementValue", SR.GetString(SR.ID3222, trustConstants.Elements.CancelTarget, trustConstants.NamespaceURI, typeof(SecurityTokenElement), elementValue));
                }

                writer.WriteStartElement(trustConstants.Prefix, trustConstants.Elements.CancelTarget, trustConstants.NamespaceURI);

                if (tokenElement.SecurityTokenXml != null)
                {
                    tokenElement.SecurityTokenXml.WriteTo(writer);
                }
                else
                {
                    context.SecurityTokenHandlers.WriteToken(writer, tokenElement.GetSecurityToken());
                }

                writer.WriteEndElement();
                return;
            }

            if (StringComparer.Ordinal.Equals(elementName, trustConstants.Elements.Participants))
            {
                Participants participants = elementValue as Participants;

                if (participants == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("elementValue", SR.GetString(SR.ID3222, trustConstants.Elements.Participant, trustConstants.NamespaceURI, typeof(Participants), elementValue));
                }

                writer.WriteStartElement(trustConstants.Prefix, trustConstants.Elements.Participants, trustConstants.NamespaceURI);

                if (participants.Primary != null)
                {
                    writer.WriteStartElement(trustConstants.Prefix, trustConstants.Elements.Primary, trustConstants.NamespaceURI);
                    participants.Primary.WriteTo(writer);
                    writer.WriteEndElement();
                }

                foreach (EndpointReference participant in participants.Participant)
                {
                    writer.WriteStartElement(trustConstants.Prefix, trustConstants.Elements.Participant, trustConstants.NamespaceURI);
                    participant.WriteTo(writer);
                    writer.WriteEndElement();
                }

                writer.WriteEndElement();
                return;
            }

            if (StringComparer.Ordinal.Equals(elementName, WSAuthorizationConstants.Elements.AdditionalContext))
            {
                AdditionalContext additionalContext = elementValue as AdditionalContext;

                if (additionalContext == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("elementValue", SR.GetString(SR.ID3222, WSAuthorizationConstants.Elements.AdditionalContext, WSAuthorizationConstants.Namespace, typeof(AdditionalContext), elementValue));
                }

                writer.WriteStartElement(WSAuthorizationConstants.Prefix, WSAuthorizationConstants.Elements.AdditionalContext, WSAuthorizationConstants.Namespace);
                foreach (ContextItem item in additionalContext.Items)
                {
                    writer.WriteStartElement(WSAuthorizationConstants.Prefix, WSAuthorizationConstants.Elements.ContextItem, WSAuthorizationConstants.Namespace);
                    writer.WriteAttributeString(WSAuthorizationConstants.Attributes.Name, item.Name.AbsoluteUri);
                    if (item.Scope != null)
                    {
                        writer.WriteAttributeString(WSAuthorizationConstants.Attributes.Scope, item.Scope.AbsoluteUri);
                    }

                    if (item.Value != null)
                    {
                        writer.WriteElementString(WSAuthorizationConstants.Elements.Value, WSAuthorizationConstants.Namespace, item.Value);
                    }

                    writer.WriteEndElement();
                }

                writer.WriteEndElement();
                return;
            }

            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new WSTrustSerializationException(SR.GetString(SR.ID3013, elementName, elementValue.GetType())));
        }

        public static RequestSecurityTokenResponse CreateResponse(XmlReader reader, WSTrustSerializationContext context, WSTrustResponseSerializer responseSerializer, WSTrustConstantsAdapter trustConstants)
        {
            if (reader == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("reader");
            }

            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");
            }

            if (responseSerializer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("responseSerializer");
            }

            if (trustConstants == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("trustConstants");
            }

            if (!reader.IsStartElement(trustConstants.Elements.RequestSecurityTokenResponse, trustConstants.NamespaceURI))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new WSTrustSerializationException(SR.GetString(SR.ID3032, reader.LocalName, reader.NamespaceURI, trustConstants.Elements.RequestSecurityTokenResponse, trustConstants.NamespaceURI)));
            }

            RequestSecurityTokenResponse rstr = responseSerializer.CreateInstance();
            bool isEmptyElement = reader.IsEmptyElement;
            rstr.Context = reader.GetAttribute(trustConstants.Attributes.Context);

            reader.Read();
            if (!isEmptyElement)
            {
                while (reader.IsStartElement())
                {
                    responseSerializer.ReadXmlElement(reader, rstr, context);
                }

                reader.ReadEndElement();
            }

            responseSerializer.Validate(rstr);

            return rstr;
        }

        public static void ReadRSTRXml(XmlReader reader, RequestSecurityTokenResponse rstr, WSTrustSerializationContext context, WSTrustConstantsAdapter trustConstants)
        {
            if (reader == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("reader");
            }

            if (rstr == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("rstr");
            }

            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");
            }

            if (trustConstants == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("trustConstants");
            }

            if (reader.IsStartElement(trustConstants.Elements.Entropy, trustConstants.NamespaceURI))
            {
                if (!reader.IsEmptyElement)
                {
                    reader.ReadStartElement(trustConstants.Elements.Entropy, trustConstants.NamespaceURI);

                    ProtectedKey protectedKey = ReadProtectedKey(reader, context, trustConstants);
                    if (protectedKey == null)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new WSTrustSerializationException(SR.GetString(SR.ID3026)));
                    }

                    rstr.Entropy = new Entropy(protectedKey);

                    reader.ReadEndElement();
                }

                if (rstr.Entropy == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new WSTrustSerializationException(SR.GetString(SR.ID3026)));
                }

                return;
            }

            if (reader.IsStartElement(trustConstants.Elements.KeySize, trustConstants.NamespaceURI))
            {
                if (!reader.IsEmptyElement)
                {
                    rstr.KeySizeInBits = Convert.ToInt32(reader.ReadElementContentAsString(), CultureInfo.InvariantCulture);
                }

                if (rstr.KeySizeInBits == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new WSTrustSerializationException(SR.GetString(SR.ID3154)));
                }

                return;
            }

            if (reader.IsStartElement(trustConstants.Elements.RequestType, trustConstants.NamespaceURI))
            {
                rstr.RequestType = WSTrustSerializationHelper.ReadRequestType(reader, trustConstants);
                return;
            }

            if (reader.IsStartElement(trustConstants.Elements.Lifetime, trustConstants.NamespaceURI))
            {
                rstr.Lifetime = WSTrustSerializationHelper.ReadLifetime(reader, trustConstants);
                return;
            }

            if (reader.IsStartElement(trustConstants.Elements.RequestedSecurityToken, trustConstants.NamespaceURI))
            {
                if (!reader.IsEmptyElement)
                {
                    rstr.RequestedSecurityToken = new RequestedSecurityToken(WSTrustSerializationHelper.ReadInnerXml(reader));
                }

                if (rstr.RequestedSecurityToken == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new WSTrustSerializationException(SR.GetString(SR.ID3158)));
                }

                return;
            }

            if (reader.IsStartElement(WSPolicyConstants.ElementNames.AppliesTo, WSPolicyConstants.NamespaceURI))
            {
                rstr.AppliesTo = WSTrustSerializationHelper.ReadAppliesTo(reader, trustConstants);
                return;
            }

            if (reader.IsStartElement(trustConstants.Elements.RequestedProofToken, trustConstants.NamespaceURI))
            {
                if (!reader.IsEmptyElement)
                {
                    reader.ReadStartElement();

                    if ((reader.LocalName == trustConstants.Elements.ComputedKey) && (reader.NamespaceURI == trustConstants.NamespaceURI))
                    {
                        rstr.RequestedProofToken = new RequestedProofToken(ReadComputedKeyAlgorithm(reader, trustConstants));
                    }
                    else
                    {
                        ProtectedKey protectedKey = ReadProtectedKey(reader, context, trustConstants);

                        if (protectedKey == null)
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new WSTrustSerializationException(SR.GetString(SR.ID3025)));
                        }

                        rstr.RequestedProofToken = new RequestedProofToken(protectedKey);
                    }

                    reader.ReadEndElement();
                }

                if (rstr.RequestedProofToken == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new WSTrustSerializationException(SR.GetString(SR.ID3025)));
                }

                return;
            }

            if (reader.IsStartElement(trustConstants.Elements.RequestedAttachedReference, trustConstants.NamespaceURI))
            {
                if (!reader.IsEmptyElement)
                {
                    reader.ReadStartElement();
                    rstr.RequestedAttachedReference = context.SecurityTokenHandlers.ReadKeyIdentifierClause(reader);
                    reader.ReadEndElement();
                }

                if (rstr.RequestedAttachedReference == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new WSTrustSerializationException(SR.GetString(SR.ID3159)));
                }

                return;
            }

            if (reader.IsStartElement(trustConstants.Elements.RequestedUnattachedReference, trustConstants.NamespaceURI))
            {
                if (!reader.IsEmptyElement)
                {
                    reader.ReadStartElement();
                    rstr.RequestedUnattachedReference = context.SecurityTokenHandlers.ReadKeyIdentifierClause(reader);
                    reader.ReadEndElement();
                }

                if (rstr.RequestedUnattachedReference == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new WSTrustSerializationException(SR.GetString(SR.ID3160)));
                }

                return;
            }

            if (reader.IsStartElement(trustConstants.Elements.TokenType, trustConstants.NamespaceURI))
            {
                rstr.TokenType = reader.ReadElementContentAsString();
                if (!UriUtil.CanCreateValidUri(rstr.TokenType, UriKind.Absolute))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new WSTrustSerializationException(SR.GetString(SR.ID3135, trustConstants.Elements.TokenType, trustConstants.NamespaceURI, rstr.TokenType)));
                }

                return;
            }

            if (reader.IsStartElement(trustConstants.Elements.KeyType, trustConstants.NamespaceURI))
            {
                rstr.KeyType = WSTrustSerializationHelper.ReadKeyType(reader, trustConstants);
                return;
            }

            if (reader.IsStartElement(trustConstants.Elements.AuthenticationType, trustConstants.NamespaceURI))
            {
                rstr.AuthenticationType = reader.ReadElementContentAsString(trustConstants.Elements.AuthenticationType, trustConstants.NamespaceURI);
                if (!UriUtil.CanCreateValidUri(rstr.AuthenticationType, UriKind.Absolute))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new WSTrustSerializationException(SR.GetString(SR.ID3135, trustConstants.Elements.AuthenticationType, trustConstants.NamespaceURI, rstr.AuthenticationType)));
                }

                return;
            }

            if (reader.IsStartElement(trustConstants.Elements.EncryptionAlgorithm, trustConstants.NamespaceURI))
            {
                rstr.EncryptionAlgorithm = reader.ReadElementContentAsString(trustConstants.Elements.EncryptionAlgorithm, trustConstants.NamespaceURI);
                if (!UriUtil.CanCreateValidUri(rstr.EncryptionAlgorithm, UriKind.Absolute))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new WSTrustSerializationException(SR.GetString(SR.ID3135, trustConstants.Elements.EncryptionAlgorithm, trustConstants.NamespaceURI, rstr.EncryptionAlgorithm)));
                }

                return;
            }

            if (reader.IsStartElement(trustConstants.Elements.CanonicalizationAlgorithm, trustConstants.NamespaceURI))
            {
                rstr.CanonicalizationAlgorithm = reader.ReadElementContentAsString(trustConstants.Elements.CanonicalizationAlgorithm, trustConstants.NamespaceURI);
                if (!UriUtil.CanCreateValidUri(rstr.CanonicalizationAlgorithm, UriKind.Absolute))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new WSTrustSerializationException(SR.GetString(SR.ID3135, trustConstants.Elements.CanonicalizationAlgorithm, trustConstants.NamespaceURI, rstr.CanonicalizationAlgorithm)));
                }

                return;
            }

            if (reader.IsStartElement(trustConstants.Elements.SignatureAlgorithm, trustConstants.NamespaceURI))
            {
                rstr.SignatureAlgorithm = reader.ReadElementContentAsString(trustConstants.Elements.SignatureAlgorithm, trustConstants.NamespaceURI);
                if (!UriUtil.CanCreateValidUri(rstr.SignatureAlgorithm, UriKind.Absolute))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new WSTrustSerializationException(SR.GetString(SR.ID3135, trustConstants.Elements.SignatureAlgorithm, trustConstants.NamespaceURI, rstr.SignatureAlgorithm)));
                }

                return;
            }

            if (reader.IsStartElement(trustConstants.Elements.SignWith, trustConstants.NamespaceURI))
            {
                rstr.SignWith = reader.ReadElementContentAsString();
                if (!UriUtil.CanCreateValidUri(rstr.SignWith, UriKind.Absolute))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new WSTrustSerializationException(SR.GetString(SR.ID3135, trustConstants.Elements.SignWith, trustConstants.NamespaceURI, rstr.SignWith)));
                }

                return;
            }

            if (reader.IsStartElement(trustConstants.Elements.EncryptWith, trustConstants.NamespaceURI))
            {
                rstr.EncryptWith = reader.ReadElementContentAsString();
                if (!UriUtil.CanCreateValidUri(rstr.EncryptWith, UriKind.Absolute))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new WSTrustSerializationException(SR.GetString(SR.ID3135, trustConstants.Elements.EncryptWith, trustConstants.NamespaceURI, rstr.EncryptWith)));
                }

                return;
            }

            if (reader.IsStartElement(trustConstants.Elements.BinaryExchange, trustConstants.NamespaceURI))
            {
                rstr.BinaryExchange = WSTrustSerializationHelper.ReadBinaryExchange(reader, trustConstants);
                return;
            }

            if (reader.IsStartElement(trustConstants.Elements.Status, trustConstants.NamespaceURI))
            {
                rstr.Status = WSTrustSerializationHelper.ReadStatus(reader, trustConstants);
                return;
            }

            if (reader.IsStartElement(trustConstants.Elements.RequestedTokenCancelled, trustConstants.NamespaceURI))
            {
                rstr.RequestedTokenCancelled = true;
                reader.ReadStartElement();
                return;
            }

            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new WSTrustSerializationException(SR.GetString(SR.ID3007, reader.LocalName, reader.NamespaceURI)));
        }

        public static void WriteResponse(RequestSecurityTokenResponse response, XmlWriter writer, WSTrustSerializationContext context, WSTrustResponseSerializer responseSerializer, WSTrustConstantsAdapter trustConstants)
        {
            if (response == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("response");
            }

            if (writer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("writer");
            }

            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");
            }

            if (responseSerializer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("responseSerializer");
            }

            if (trustConstants == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("trustConstants");
            }

            responseSerializer.Validate(response);

            // Step 1: Write RSTR start element
            writer.WriteStartElement(trustConstants.Prefix, trustConstants.Elements.RequestSecurityTokenResponse, trustConstants.NamespaceURI);

            // Step 2: Write known RSTR attributes, i.e. Context
            if (!string.IsNullOrEmpty(response.Context))
            {
                writer.WriteAttributeString(trustConstants.Attributes.Context, response.Context);
            }

            // Step 3: Write known RSTR elements
            responseSerializer.WriteKnownResponseElement(response, writer, context);

            // Step 4: Write custom RSTR elements
            foreach (KeyValuePair<string, object> messageParam in response.Properties)
            {
                responseSerializer.WriteXmlElement(writer, messageParam.Key, messageParam.Value, response, context);
            }

            // Step 5: Write RSTR end element to close it
            writer.WriteEndElement();
        }

        public static void WriteKnownResponseElement(RequestSecurityTokenResponse rstr, XmlWriter writer, WSTrustSerializationContext context, WSTrustResponseSerializer responseSerializer, WSTrustConstantsAdapter trustConstants)
        {
            if (rstr == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("rstr");
            }

            if (writer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("writer");
            }

            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");
            }

            if (responseSerializer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("responseSerializer");
            }

            if (trustConstants == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("trustConstants");
            }

            if (rstr.Entropy != null)
            {
                responseSerializer.WriteXmlElement(writer, trustConstants.Elements.Entropy, rstr.Entropy, rstr, context);
            }

            if (rstr.KeySizeInBits.HasValue)
            {
                responseSerializer.WriteXmlElement(writer, trustConstants.Elements.KeySize, rstr.KeySizeInBits, rstr, context);
            }

            if (rstr.Lifetime != null)
            {
                responseSerializer.WriteXmlElement(writer, trustConstants.Elements.Lifetime, rstr.Lifetime, rstr, context);
            }

            if (rstr.AppliesTo != null)
            {
                responseSerializer.WriteXmlElement(writer, WSPolicyConstants.ElementNames.AppliesTo, rstr.AppliesTo, rstr, context);
            }

            if (rstr.RequestedSecurityToken != null)
            {
                responseSerializer.WriteXmlElement(writer, trustConstants.Elements.RequestedSecurityToken, rstr.RequestedSecurityToken, rstr, context);
            }

            if (rstr.RequestedProofToken != null)
            {
                responseSerializer.WriteXmlElement(writer, trustConstants.Elements.RequestedProofToken, rstr.RequestedProofToken, rstr, context);
            }

            if (rstr.RequestedAttachedReference != null)
            {
                responseSerializer.WriteXmlElement(writer, trustConstants.Elements.RequestedAttachedReference, rstr.RequestedAttachedReference, rstr, context);
            }

            if (rstr.RequestedUnattachedReference != null)
            {
                responseSerializer.WriteXmlElement(writer, trustConstants.Elements.RequestedUnattachedReference, rstr.RequestedUnattachedReference, rstr, context);
            }

            if (!string.IsNullOrEmpty(rstr.SignWith))
            {
                responseSerializer.WriteXmlElement(writer, trustConstants.Elements.SignWith, rstr.SignWith, rstr, context);
            }

            if (!string.IsNullOrEmpty(rstr.EncryptWith))
            {
                responseSerializer.WriteXmlElement(writer, trustConstants.Elements.EncryptWith, rstr.EncryptWith, rstr, context);
            }

            if (!string.IsNullOrEmpty(rstr.TokenType))
            {
                responseSerializer.WriteXmlElement(writer, trustConstants.Elements.TokenType, rstr.TokenType, rstr, context);
            }

            if (!string.IsNullOrEmpty(rstr.RequestType))
            {
                responseSerializer.WriteXmlElement(writer, trustConstants.Elements.RequestType, rstr.RequestType, rstr, context);
            }

            if (!string.IsNullOrEmpty(rstr.KeyType))
            {
                responseSerializer.WriteXmlElement(writer, trustConstants.Elements.KeyType, rstr.KeyType, rstr, context);
            }

            if (!string.IsNullOrEmpty(rstr.AuthenticationType))
            {
                responseSerializer.WriteXmlElement(writer, trustConstants.Elements.AuthenticationType, rstr.AuthenticationType, rstr, context);
            }

            if (!string.IsNullOrEmpty(rstr.EncryptionAlgorithm))
            {
                responseSerializer.WriteXmlElement(writer, trustConstants.Elements.EncryptionAlgorithm, rstr.EncryptionAlgorithm, rstr, context);
            }

            if (!string.IsNullOrEmpty(rstr.CanonicalizationAlgorithm))
            {
                responseSerializer.WriteXmlElement(writer, trustConstants.Elements.CanonicalizationAlgorithm, rstr.CanonicalizationAlgorithm, rstr, context);
            }

            if (!string.IsNullOrEmpty(rstr.SignatureAlgorithm))
            {
                responseSerializer.WriteXmlElement(writer, trustConstants.Elements.SignatureAlgorithm, rstr.SignatureAlgorithm, rstr, context);
            }

            if (rstr.BinaryExchange != null)
            {
                responseSerializer.WriteXmlElement(writer, trustConstants.Elements.BinaryExchange, rstr.BinaryExchange, rstr, context);
            }

            if (rstr.Status != null)
            {
                responseSerializer.WriteXmlElement(writer, trustConstants.Elements.Status, rstr.Status, rstr, context);
            }

            if (rstr.RequestedTokenCancelled)
            {
                responseSerializer.WriteXmlElement(writer, trustConstants.Elements.RequestedTokenCancelled, rstr.RequestedTokenCancelled, rstr, context);
            }
        }

        public static void WriteRSTRXml(XmlWriter writer, string elementName, object elementValue, WSTrustSerializationContext context, WSTrustConstantsAdapter trustConstants)
        {
            if (writer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("writer");
            }

            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");
            }

            if (string.IsNullOrEmpty(elementName))
            {
                throw DiagnosticUtility.ThrowHelperArgumentNullOrEmptyString("elementName");
            }

            if (trustConstants == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("trustConstants");
            }

            if (StringComparer.Ordinal.Equals(elementName, trustConstants.Elements.Entropy))
            {
                Entropy entropy = elementValue as Entropy;
                if (entropy != null)
                {
                    writer.WriteStartElement(trustConstants.Prefix, trustConstants.Elements.Entropy, trustConstants.NamespaceURI);
                    WriteProtectedKey(writer, entropy, context, trustConstants);
                    writer.WriteEndElement();
                }

                return;
            }

            if (StringComparer.Ordinal.Equals(elementName, trustConstants.Elements.KeySize))
            {
                writer.WriteElementString(trustConstants.Prefix, trustConstants.Elements.KeySize, trustConstants.NamespaceURI, Convert.ToString((int)elementValue, CultureInfo.InvariantCulture));
                return;
            }

            if (StringComparer.Ordinal.Equals(elementName, trustConstants.Elements.Lifetime))
            {
                Lifetime lifeTime = (Lifetime)elementValue;
                WSTrustSerializationHelper.WriteLifetime(writer, lifeTime, trustConstants);
                return;
            }

            if (StringComparer.Ordinal.Equals(elementName, WSPolicyConstants.ElementNames.AppliesTo))
            {
                EndpointReference appliesTo = elementValue as EndpointReference;
                WSTrustSerializationHelper.WriteAppliesTo(writer, appliesTo, trustConstants);
                return;
            }

            if (StringComparer.Ordinal.Equals(elementName, trustConstants.Elements.RequestedSecurityToken))
            {
                RequestedSecurityToken requestedToken = (RequestedSecurityToken)elementValue;

                writer.WriteStartElement(trustConstants.Prefix, trustConstants.Elements.RequestedSecurityToken, trustConstants.NamespaceURI);

                if (requestedToken.SecurityTokenXml != null)
                {
                    requestedToken.SecurityTokenXml.WriteTo(writer);
                }
                else
                {
                    context.SecurityTokenHandlers.WriteToken(writer, requestedToken.SecurityToken);
                }

                writer.WriteEndElement();
                return;
            }

            if (StringComparer.Ordinal.Equals(elementName, trustConstants.Elements.RequestedProofToken))
            {
                RequestedProofToken proofToken = (RequestedProofToken)elementValue;
                if (string.IsNullOrEmpty(proofToken.ComputedKeyAlgorithm) && proofToken.ProtectedKey == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.ID3021)));
                }

                writer.WriteStartElement(trustConstants.Prefix, trustConstants.Elements.RequestedProofToken, trustConstants.NamespaceURI);

                if (!string.IsNullOrEmpty(proofToken.ComputedKeyAlgorithm))
                {
                    WriteComputedKeyAlgorithm(writer, trustConstants.Elements.ComputedKey, proofToken.ComputedKeyAlgorithm, trustConstants);
                }
                else
                {
                    WriteProtectedKey(writer, proofToken.ProtectedKey, context, trustConstants);
                }

                writer.WriteEndElement();

                return;
            }

            if (StringComparer.Ordinal.Equals(elementName, trustConstants.Elements.RequestedAttachedReference))
            {
                writer.WriteStartElement(trustConstants.Prefix, trustConstants.Elements.RequestedAttachedReference, trustConstants.NamespaceURI);
                context.SecurityTokenHandlers.WriteKeyIdentifierClause(writer, (SecurityKeyIdentifierClause)elementValue);
                writer.WriteEndElement();
                return;
            }

            if (StringComparer.Ordinal.Equals(elementName, trustConstants.Elements.RequestedUnattachedReference))
            {
                writer.WriteStartElement(trustConstants.Prefix, trustConstants.Elements.RequestedUnattachedReference, trustConstants.NamespaceURI);
                context.SecurityTokenHandlers.WriteKeyIdentifierClause(writer, (SecurityKeyIdentifierClause)elementValue);
                writer.WriteEndElement();
                return;
            }

            if (StringComparer.Ordinal.Equals(elementName, trustConstants.Elements.TokenType))
            {
                if (!UriUtil.CanCreateValidUri((string)elementValue, UriKind.Absolute))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new WSTrustSerializationException(SR.GetString(SR.ID3135, trustConstants.Elements.TokenType, trustConstants.NamespaceURI, (string)elementValue)));
                }

                writer.WriteElementString(trustConstants.Prefix, trustConstants.Elements.TokenType, trustConstants.NamespaceURI, (string)elementValue);
                return;
            }

            if (StringComparer.Ordinal.Equals(elementName, trustConstants.Elements.RequestType))
            {
                if (!UriUtil.CanCreateValidUri((string)elementValue, UriKind.Absolute))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new WSTrustSerializationException(SR.GetString(SR.ID3135, trustConstants.Elements.RequestType, trustConstants.NamespaceURI, (string)elementValue)));
                }

                WSTrustSerializationHelper.WriteRequestType(writer, (string)elementValue, trustConstants);
                return;
            }

            if (StringComparer.Ordinal.Equals(elementName, trustConstants.Elements.KeyType))
            {
                WSTrustSerializationHelper.WriteKeyType(writer, (string)elementValue, trustConstants);
                return;
            }

            if (StringComparer.Ordinal.Equals(elementName, trustConstants.Elements.AuthenticationType))
            {
                if (!UriUtil.CanCreateValidUri((string)elementValue, UriKind.Absolute))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new WSTrustSerializationException(SR.GetString(SR.ID3135, trustConstants.Elements.AuthenticationType, trustConstants.NamespaceURI, (string)elementValue)));
                }

                writer.WriteElementString(trustConstants.Prefix, trustConstants.Elements.AuthenticationType, trustConstants.NamespaceURI, (string)elementValue);
                return;
            }

            if (StringComparer.Ordinal.Equals(elementName, trustConstants.Elements.EncryptionAlgorithm))
            {
                if (!UriUtil.CanCreateValidUri((string)elementValue, UriKind.Absolute))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new WSTrustSerializationException(SR.GetString(SR.ID3135, trustConstants.Elements.EncryptionAlgorithm, trustConstants.NamespaceURI, (string)elementValue)));
                }

                writer.WriteElementString(trustConstants.Prefix, trustConstants.Elements.EncryptionAlgorithm, trustConstants.NamespaceURI, (string)elementValue);
                return;
            }

            if (StringComparer.Ordinal.Equals(elementName, trustConstants.Elements.CanonicalizationAlgorithm))
            {
                if (!UriUtil.CanCreateValidUri((string)elementValue, UriKind.Absolute))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new WSTrustSerializationException(SR.GetString(SR.ID3135, trustConstants.Elements.CanonicalizationAlgorithm, trustConstants.NamespaceURI, (string)elementValue)));
                }

                writer.WriteElementString(trustConstants.Prefix, trustConstants.Elements.CanonicalizationAlgorithm, trustConstants.NamespaceURI, (string)elementValue);
                return;
            }

            if (StringComparer.Ordinal.Equals(elementName, trustConstants.Elements.SignatureAlgorithm))
            {
                if (!UriUtil.CanCreateValidUri((string)elementValue, UriKind.Absolute))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new WSTrustSerializationException(SR.GetString(SR.ID3135, trustConstants.Elements.SignatureAlgorithm, trustConstants.NamespaceURI, (string)elementValue)));
                }

                writer.WriteElementString(trustConstants.Prefix, trustConstants.Elements.SignatureAlgorithm, trustConstants.NamespaceURI, (string)elementValue);
                return;
            }

            if (StringComparer.Ordinal.Equals(elementName, trustConstants.Elements.SignWith))
            {
                if (!UriUtil.CanCreateValidUri((string)elementValue, UriKind.Absolute))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new WSTrustSerializationException(SR.GetString(SR.ID3135, trustConstants.Elements.SignWith, trustConstants.NamespaceURI, (string)elementValue)));
                }

                writer.WriteElementString(trustConstants.Prefix, trustConstants.Elements.SignWith, trustConstants.NamespaceURI, (string)elementValue);
                return;
            }

            if (StringComparer.Ordinal.Equals(elementName, trustConstants.Elements.EncryptWith))
            {
                if (!UriUtil.CanCreateValidUri((string)elementValue, UriKind.Absolute))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new WSTrustSerializationException(SR.GetString(SR.ID3135, trustConstants.Elements.EncryptWith, trustConstants.NamespaceURI, (string)elementValue)));
                }

                writer.WriteElementString(trustConstants.Prefix, trustConstants.Elements.EncryptWith, trustConstants.NamespaceURI, (string)elementValue);
                return;
            }

            if (StringComparer.Ordinal.Equals(elementName, trustConstants.Elements.BinaryExchange))
            {
                WriteBinaryExchange(writer, elementValue as BinaryExchange, trustConstants);
                return;
            }

            if (StringComparer.Ordinal.Equals(elementName, trustConstants.Elements.Status))
            {
                WriteStatus(writer, elementValue as Status, trustConstants);
                return;
            }

            if (StringComparer.Ordinal.Equals(elementName, trustConstants.Elements.RequestedTokenCancelled))
            {
                writer.WriteStartElement(trustConstants.Prefix, trustConstants.Elements.RequestedTokenCancelled, trustConstants.NamespaceURI);
                writer.WriteEndElement();
                return;
            }

            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new WSTrustSerializationException(SR.GetString(SR.ID3013, elementName, elementValue.GetType())));
        }

        public static string ReadComputedKeyAlgorithm(XmlReader reader, WSTrustConstantsAdapter trustConstants)
        {
            if (reader == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("reader");
            }

            if (trustConstants == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("trustConstants");
            }

            string computedKeyAlgorithm = reader.ReadElementContentAsString();

            if (string.IsNullOrEmpty(computedKeyAlgorithm))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new WSTrustSerializationException(SR.GetString(SR.ID3006)));
            }

            if (!UriUtil.CanCreateValidUri(computedKeyAlgorithm, UriKind.Absolute))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new WSTrustSerializationException(SR.GetString(SR.ID3135, trustConstants.Elements.ComputedKeyAlgorithm, trustConstants.NamespaceURI, computedKeyAlgorithm)));
            }

            if (StringComparer.Ordinal.Equals(computedKeyAlgorithm, trustConstants.ComputedKeyAlgorithm.Psha1))
            {
                computedKeyAlgorithm = ComputedKeyAlgorithms.Psha1;
            }

            return computedKeyAlgorithm;
        }

        public static void WriteComputedKeyAlgorithm(XmlWriter writer, string elementName, string computedKeyAlgorithm, WSTrustConstantsAdapter trustConstants)
        {
            if (writer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("writer");
            }

            if (string.IsNullOrEmpty(computedKeyAlgorithm))
            {
                throw DiagnosticUtility.ThrowHelperArgumentNullOrEmptyString("computedKeyAlgorithm");
            }

            if (trustConstants == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("trustConstants");
            }

            if (!UriUtil.CanCreateValidUri(computedKeyAlgorithm, UriKind.Absolute))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new WSTrustSerializationException(SR.GetString(SR.ID3135, elementName, trustConstants.NamespaceURI, computedKeyAlgorithm)));
            }

            string computedKeyAlgorithmAsStr = null;
            if (StringComparer.Ordinal.Equals(computedKeyAlgorithm, ComputedKeyAlgorithms.Psha1))
            {
                computedKeyAlgorithmAsStr = trustConstants.ComputedKeyAlgorithm.Psha1;
            }
            else
            {
                computedKeyAlgorithmAsStr = computedKeyAlgorithm;
            }

            if (!UriUtil.CanCreateValidUri(computedKeyAlgorithmAsStr, UriKind.Absolute))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new WSTrustSerializationException(SR.GetString(SR.ID3135, elementName, trustConstants.NamespaceURI, computedKeyAlgorithmAsStr)));
            }

            writer.WriteElementString(trustConstants.Prefix, elementName, trustConstants.NamespaceURI, computedKeyAlgorithmAsStr);
        }

        public static Status ReadStatus(XmlReader reader, WSTrustConstantsAdapter trustConstants)
        {
            if (reader == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("reader");
            }

            if (trustConstants == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("trustConstants");
            }

            if (!reader.IsStartElement(trustConstants.Elements.Status, trustConstants.NamespaceURI))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new WSTrustSerializationException(
                    SR.GetString(SR.ID3032, reader.LocalName, reader.NamespaceURI, trustConstants.Elements.Status, trustConstants.NamespaceURI)));
            }

            string code = null;
            string reason = null;
            reader.ReadStartElement();

            if (!reader.IsStartElement(trustConstants.Elements.Code, trustConstants.NamespaceURI))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new WSTrustSerializationException(
                    SR.GetString(SR.ID3032, reader.LocalName, reader.NamespaceURI, trustConstants.Elements.Code, trustConstants.NamespaceURI)));
            }

            code = reader.ReadElementContentAsString(trustConstants.Elements.Code, trustConstants.NamespaceURI);
            if (reader.IsStartElement(trustConstants.Elements.Reason, trustConstants.NamespaceURI))
            {
                reason = reader.ReadElementContentAsString(trustConstants.Elements.Reason, trustConstants.NamespaceURI);
            }

            reader.ReadEndElement();
            return new Status(code, reason);
        }

        public static BinaryExchange ReadBinaryExchange(XmlReader reader, WSTrustConstantsAdapter trustConstants)
        {
            if (reader == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("reader");
            }

            if (trustConstants == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("trustConstants");
            }

            if (!reader.IsStartElement(trustConstants.Elements.BinaryExchange, trustConstants.NamespaceURI))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new WSTrustSerializationException(
                    SR.GetString(SR.ID3032, reader.LocalName, reader.NamespaceURI, trustConstants.Elements.BinaryExchange, trustConstants.NamespaceURI)));
            }

            string attrValue = reader.GetAttribute(trustConstants.Attributes.ValueType);
            if (string.IsNullOrEmpty(attrValue))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new WSTrustSerializationException(
                    SR.GetString(SR.ID0001, trustConstants.Attributes.ValueType, reader.Name)));
            }

            Uri valueType;
            if (!UriUtil.TryCreateValidUri(attrValue, UriKind.Absolute, out valueType))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new WSTrustSerializationException(
                    SR.GetString(SR.ID3136, trustConstants.Attributes.ValueType, reader.LocalName, reader.NamespaceURI, attrValue)));
            }

            attrValue = reader.GetAttribute(trustConstants.Attributes.EncodingType);
            if (string.IsNullOrEmpty(attrValue))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new WSTrustSerializationException(
                    SR.GetString(SR.ID0001, trustConstants.Attributes.EncodingType, reader.Name)));
            }

            Uri encodingType;
            if (!UriUtil.TryCreateValidUri(attrValue, UriKind.Absolute, out encodingType))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new WSTrustSerializationException(
                    SR.GetString(SR.ID3136, trustConstants.Attributes.EncodingType, reader.LocalName, reader.NamespaceURI, attrValue)));
            }

            byte[] binaryData;
            switch (encodingType.AbsoluteUri)
            {
                case WSSecurity10Constants.EncodingTypes.Base64:
                    {
                        binaryData = Convert.FromBase64String(reader.ReadElementContentAsString());
                        break;
                    }

                case WSSecurity10Constants.EncodingTypes.HexBinary:
                    {
                        binaryData = SoapHexBinary.Parse(reader.ReadElementContentAsString()).Value;
                        break;
                    }

                default:
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new WSTrustSerializationException(SR.GetString(SR.ID3215, encodingType, reader.LocalName, reader.NamespaceURI, string.Format(CultureInfo.InvariantCulture, "({0}, {1})", WSSecurity10Constants.EncodingTypes.Base64, WSSecurity10Constants.EncodingTypes.HexBinary))));
                    }
            }

            return new BinaryExchange(binaryData, valueType, encodingType);
        }

        public static void WriteBinaryExchange(XmlWriter writer, BinaryExchange binaryExchange, WSTrustConstantsAdapter trustConstants)
        {
            if (writer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("writer");
            }

            if (binaryExchange == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("binaryExchange");
            }

            if (trustConstants == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("trustConstants");
            }

            string binaryData = null;
            switch (binaryExchange.EncodingType.AbsoluteUri)
            {
                case WSSecurity10Constants.EncodingTypes.Base64:
                    {
                        binaryData = Convert.ToBase64String(binaryExchange.BinaryData);
                        break;
                    }

                case WSSecurity10Constants.EncodingTypes.HexBinary:
                    {
                        SoapHexBinary hexBinary = new SoapHexBinary(binaryExchange.BinaryData);
                        binaryData = hexBinary.ToString();
                        break;
                    }

                default:
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new WSTrustSerializationException(SR.GetString(
                            SR.ID3217,
                            binaryExchange.EncodingType.AbsoluteUri,
                            string.Format(CultureInfo.InvariantCulture, "({0}, {1})", WSSecurity10Constants.EncodingTypes.Base64, WSSecurity10Constants.EncodingTypes.HexBinary))));
                    }
            }

            writer.WriteStartElement(trustConstants.Prefix, trustConstants.Elements.BinaryExchange, trustConstants.NamespaceURI);
            writer.WriteAttributeString(trustConstants.Attributes.ValueType, binaryExchange.ValueType.AbsoluteUri);
            writer.WriteAttributeString(trustConstants.Attributes.EncodingType, binaryExchange.EncodingType.AbsoluteUri);
            writer.WriteString(binaryData);
            writer.WriteEndElement();
        }

        public static void WriteStatus(XmlWriter writer, Status status, WSTrustConstantsAdapter trustConstants)
        {
            if (writer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("writer");
            }

            if (status == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("status");
            }

            if (trustConstants == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("trustConstants");
            }

            if (status.Code == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("status code");
            }

            writer.WriteStartElement(trustConstants.Prefix, trustConstants.Elements.Status, trustConstants.NamespaceURI);
            writer.WriteStartElement(trustConstants.Prefix, trustConstants.Elements.Code, trustConstants.NamespaceURI);
            writer.WriteString(status.Code);
            writer.WriteEndElement();
            if (status.Reason != null)
            {
                writer.WriteStartElement(trustConstants.Prefix, trustConstants.Elements.Reason, trustConstants.NamespaceURI);
                writer.WriteString(status.Reason);
                writer.WriteEndElement();
            }

            writer.WriteEndElement();
        }

        // This method reads the binary secret or encrypted key 
        public static ProtectedKey ReadProtectedKey(XmlReader reader, WSTrustSerializationContext context, WSTrustConstantsAdapter trustConstants)
        {
            if (reader == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("reader");
            }

            if (trustConstants == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("trustConstants");
            }

            ProtectedKey protectedKey = null;

            if (!reader.IsEmptyElement)
            {
                if (reader.IsStartElement(trustConstants.Elements.BinarySecret, trustConstants.NamespaceURI))
                {
                    // BinarySecret case
                    BinarySecretSecurityToken token = ReadBinarySecretSecurityToken(reader, trustConstants);
                    byte[] secret = token.GetKeyBytes();
                    protectedKey = new ProtectedKey(secret);
                }
                else if (context.SecurityTokenHandlers.CanReadKeyIdentifierClause(reader))
                {
                    // EncryptedKey case
                    EncryptedKeyIdentifierClause encryptedKeyClause = context.SecurityTokenHandlers.ReadKeyIdentifierClause(reader) as EncryptedKeyIdentifierClause;

                    if (encryptedKeyClause != null)
                    {
                        SecurityKey wrappingKey = null;
                        byte[] secret;

                        foreach (SecurityKeyIdentifierClause wrappingKeyClause in encryptedKeyClause.EncryptingKeyIdentifier)
                        {
                            if (context.TokenResolver.TryResolveSecurityKey(wrappingKeyClause, out wrappingKey))
                            {
                                break;
                            }
                        }

                        if (wrappingKey == null)
                        {
                            // We can't resolve the ski, throw
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new WSTrustSerializationException(SR.GetString(SR.ID3027, "the SecurityHeaderTokenResolver or OutOfBandTokenResolver")));
                        }

                        secret = wrappingKey.DecryptKey(encryptedKeyClause.EncryptionMethod, encryptedKeyClause.GetEncryptedKey());
                        EncryptingCredentials wrappingCredentials = new EncryptingCredentials(wrappingKey, encryptedKeyClause.EncryptingKeyIdentifier, encryptedKeyClause.EncryptionMethod);

                        protectedKey = new ProtectedKey(secret, wrappingCredentials);
                    }
                }
            }

            return protectedKey;
        }

        public static void WriteProtectedKey(XmlWriter writer, ProtectedKey protectedKey, WSTrustSerializationContext context, WSTrustConstantsAdapter trustConstants)
        {
            if (writer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("writer");
            }

            if (protectedKey == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("protectedKey");
            }

            if (trustConstants == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("trustConstants");
            }

            if (protectedKey.WrappingCredentials != null)
            {
                byte[] encryptedKey = protectedKey.WrappingCredentials.SecurityKey.EncryptKey(protectedKey.WrappingCredentials.Algorithm, protectedKey.GetKeyBytes());
                EncryptedKeyIdentifierClause clause = new EncryptedKeyIdentifierClause(encryptedKey, protectedKey.WrappingCredentials.Algorithm, protectedKey.WrappingCredentials.SecurityKeyIdentifier);
                context.SecurityTokenHandlers.WriteKeyIdentifierClause(writer, clause);
            }
            else
            {
                BinarySecretSecurityToken entropyToken = new BinarySecretSecurityToken(protectedKey.GetKeyBytes());
                WriteBinarySecretSecurityToken(writer, entropyToken, trustConstants);
            }
        }

        public static string ReadRequestType(XmlReader reader, WSTrustConstantsAdapter trustConstants)
        {
            if (reader == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("reader");
            }

            if (trustConstants == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("trustConstants");
            }

            string requestType = reader.ReadElementContentAsString();

            if (!UriUtil.CanCreateValidUri(requestType, UriKind.Absolute))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new WSTrustSerializationException(SR.GetString(SR.ID3135, trustConstants.Elements.RequestType, trustConstants.NamespaceURI, requestType)));
            }

            if (trustConstants.RequestTypes.Issue.Equals(requestType))
            {
                return RequestTypes.Issue;
            }
            else if (trustConstants.RequestTypes.Cancel.Equals(requestType))
            {
                return RequestTypes.Cancel;
            }
            else if (trustConstants.RequestTypes.Renew.Equals(requestType))
            {
                return RequestTypes.Renew;
            }
            else if (trustConstants.RequestTypes.Validate.Equals(requestType))
            {
                return RequestTypes.Validate;
            }
            else
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new WSTrustSerializationException(SR.GetString(SR.ID3011, requestType)));
            }
        }

        public static void WriteRequestType(XmlWriter writer, string requestType, WSTrustConstantsAdapter trustConstants)
        {
            if (writer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("writer");
            }

            if (requestType == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("requestType");
            }

            if (trustConstants == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("trustConstants");
            }

            string requestTypeAsStr = null;
            if (StringComparer.Ordinal.Equals(requestType, RequestTypes.Issue) || StringComparer.Ordinal.Equals(requestType, trustConstants.RequestTypes.Issue))
            {
                requestTypeAsStr = trustConstants.RequestTypes.Issue;
            }
            else if (StringComparer.Ordinal.Equals(requestType, RequestTypes.Renew) || StringComparer.Ordinal.Equals(requestType, trustConstants.RequestTypes.Renew))
            {
                requestTypeAsStr = trustConstants.RequestTypes.Renew;
            }
            else if (StringComparer.Ordinal.Equals(requestType, RequestTypes.Cancel) || StringComparer.Ordinal.Equals(requestType, trustConstants.RequestTypes.Cancel))
            {
                requestTypeAsStr = trustConstants.RequestTypes.Cancel;
            }
            else if (StringComparer.Ordinal.Equals(requestType, RequestTypes.Validate) || StringComparer.Ordinal.Equals(requestType, trustConstants.RequestTypes.Validate))
            {
                requestTypeAsStr = trustConstants.RequestTypes.Validate;
            }
            else
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new WSTrustSerializationException(SR.GetString(SR.ID3011, requestType)));
            }

            writer.WriteElementString(trustConstants.Prefix, trustConstants.Elements.RequestType, trustConstants.NamespaceURI, requestTypeAsStr);
        }

        public static Lifetime ReadLifetime(XmlReader reader, WSTrustConstantsAdapter trustConstants)
        {
            if (reader == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("reader");
            }

            if (trustConstants == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("trustConstants");
            }

            DateTime? created = null;
            DateTime? expires = null;
            Lifetime lifetime = null;
            bool isEmptyElement = reader.IsEmptyElement;
            reader.ReadStartElement();

            if (!isEmptyElement)
            {
                if (reader.IsStartElement(WSUtilityConstants.ElementNames.Created, WSUtilityConstants.NamespaceURI))
                {
                    reader.ReadStartElement(WSUtilityConstants.ElementNames.Created, WSUtilityConstants.NamespaceURI);
                    created = DateTime.ParseExact(reader.ReadString(), DateTimeFormats.Accepted, DateTimeFormatInfo.InvariantInfo, DateTimeStyles.None).ToUniversalTime();
                    reader.ReadEndElement();
                }

                if (reader.IsStartElement(WSUtilityConstants.ElementNames.Expires, WSUtilityConstants.NamespaceURI))
                {
                    reader.ReadStartElement(WSUtilityConstants.ElementNames.Expires, WSUtilityConstants.NamespaceURI);
                    expires = DateTime.ParseExact(reader.ReadString(), DateTimeFormats.Accepted, DateTimeFormatInfo.InvariantInfo, DateTimeStyles.None).ToUniversalTime();
                    reader.ReadEndElement();
                }

                reader.ReadEndElement();

                lifetime = new Lifetime(created, expires);
            }

            if (lifetime == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new WSTrustSerializationException(SR.GetString(SR.ID3161)));
            }

            return lifetime;
        }

        public static void WriteLifetime(XmlWriter writer, Lifetime lifetime, WSTrustConstantsAdapter trustConstants)
        {
            if (writer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("writer");
            }

            if (lifetime == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("lifetime");
            }

            if (trustConstants == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("trustConstants");
            }

            writer.WriteStartElement(trustConstants.Prefix, trustConstants.Elements.Lifetime, trustConstants.NamespaceURI);

            if (lifetime.Created != null)
            {
                writer.WriteElementString(WSUtilityConstants.Prefix, WSUtilityConstants.ElementNames.Created, WSUtilityConstants.NamespaceURI, lifetime.Created.Value.ToString(DateTimeFormats.Generated, CultureInfo.InvariantCulture));
            }

            if (lifetime.Expires != null)
            {
                writer.WriteElementString(WSUtilityConstants.Prefix, WSUtilityConstants.ElementNames.Expires, WSUtilityConstants.NamespaceURI, lifetime.Expires.Value.ToString(DateTimeFormats.Generated, CultureInfo.InvariantCulture));
            }

            writer.WriteEndElement();
        }

        public static EndpointReference ReadOnBehalfOfIssuer(XmlReader reader, WSTrustConstantsAdapter trustConstants)
        {
            if (reader == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("reader");
            }

            if (trustConstants == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("trustConstants");
            }

            if (!reader.IsStartElement(trustConstants.Elements.Issuer, trustConstants.NamespaceURI))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new WSTrustSerializationException(
                    SR.GetString(SR.ID3032, reader.LocalName, reader.NamespaceURI, trustConstants.Elements.Issuer, trustConstants.NamespaceURI)));
            }

            EndpointReference issuer = null;
            if (!reader.IsEmptyElement)
            {
                reader.ReadStartElement();
                issuer = EndpointReference.ReadFrom(XmlDictionaryReader.CreateDictionaryReader(reader));
                reader.ReadEndElement();
            }

            if (issuer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new WSTrustSerializationException(SR.GetString(SR.ID3216)));
            }

            return issuer;
        }

        public static void WriteOnBehalfOfIssuer(XmlWriter writer, EndpointReference issuer, WSTrustConstantsAdapter trustConstants)
        {
            if (writer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("writer");
            }

            if (issuer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("issuer");
            }

            if (trustConstants == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("trustConstants");
            }

            writer.WriteStartElement(trustConstants.Prefix, trustConstants.Elements.Issuer, trustConstants.NamespaceURI);
            issuer.WriteTo(writer);
            writer.WriteEndElement();
        }

        public static EndpointReference ReadAppliesTo(XmlReader reader, WSTrustConstantsAdapter trustConstants)
        {
            if (reader == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("reader");
            }

            if (trustConstants == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("trustConstants");
            }

            EndpointReference appliesTo = null;
            if (!reader.IsEmptyElement)
            {
                reader.ReadStartElement();
                appliesTo = EndpointReference.ReadFrom(XmlDictionaryReader.CreateDictionaryReader(reader));
                reader.ReadEndElement();
            }

            if (appliesTo == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new WSTrustSerializationException(SR.GetString(SR.ID3162)));
            }

            return appliesTo;
        }

        public static void WriteAppliesTo(XmlWriter writer, EndpointReference appliesTo, WSTrustConstantsAdapter trustConstants)
        {
            if (writer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("writer");
            }

            if (appliesTo == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("appliesTo");
            }

            if (trustConstants == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("trustConstants");
            }

            writer.WriteStartElement(WSPolicyConstants.Prefix, WSPolicyConstants.ElementNames.AppliesTo, WSPolicyConstants.NamespaceURI);
            appliesTo.WriteTo(writer);
            writer.WriteEndElement();
        }

        public static string ReadKeyType(XmlReader reader, WSTrustConstantsAdapter trustConstants)
        {
            if (reader == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("reader");
            }

            if (trustConstants == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("trustConstants");
            }

            string incomingKeyType = reader.ReadElementContentAsString();
            if (!UriUtil.CanCreateValidUri(incomingKeyType, UriKind.Absolute))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new WSTrustSerializationException(SR.GetString(SR.ID3135, trustConstants.Elements.KeyType, trustConstants.NamespaceURI, incomingKeyType)));
            }

            if (trustConstants.KeyTypes.Symmetric.Equals(incomingKeyType))
            {
                return KeyTypes.Symmetric;
            }
            else if (trustConstants.KeyTypes.Asymmetric.Equals(incomingKeyType))
            {
                return KeyTypes.Asymmetric;
            }
            else if (trustConstants.KeyTypes.Bearer.Equals(incomingKeyType))
            {
                return KeyTypes.Bearer;
            }
            else
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new WSTrustSerializationException(SR.GetString(SR.ID3020, incomingKeyType)));
            }
        }

        public static void WriteKeyType(XmlWriter writer, string keyType, WSTrustConstantsAdapter trustConstants)
        {
            if (writer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("writer");
            }

            if (string.IsNullOrEmpty(keyType))
            {
                throw DiagnosticUtility.ThrowHelperArgumentNullOrEmptyString("keyType");
            }

            if (trustConstants == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("trustConstants");
            }

            if (!UriUtil.CanCreateValidUri(keyType, UriKind.Absolute))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new WSTrustSerializationException(SR.GetString(SR.ID3135, trustConstants.Elements.KeyType, trustConstants.NamespaceURI, keyType)));
            }

            string keyTypeAsStr = null;

            if (StringComparer.Ordinal.Equals(keyType, KeyTypes.Asymmetric) || StringComparer.Ordinal.Equals(keyType, trustConstants.KeyTypes.Asymmetric))
            {
                keyTypeAsStr = trustConstants.KeyTypes.Asymmetric;
            }
            else if (StringComparer.Ordinal.Equals(keyType, KeyTypes.Symmetric) || StringComparer.Ordinal.Equals(keyType, trustConstants.KeyTypes.Symmetric))
            {
                keyTypeAsStr = trustConstants.KeyTypes.Symmetric;
            }
            else if (StringComparer.Ordinal.Equals(keyType, KeyTypes.Bearer) || StringComparer.Ordinal.Equals(keyType, trustConstants.KeyTypes.Bearer))
            {
                keyTypeAsStr = trustConstants.KeyTypes.Bearer;
            }
            else
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new WSTrustSerializationException(SR.GetString(SR.ID3010, keyType)));
            }

            writer.WriteElementString(trustConstants.Prefix, trustConstants.Elements.KeyType, trustConstants.NamespaceURI, keyTypeAsStr);
        }

        public static XmlElement ReadInnerXml(XmlReader reader)
        {
            return ReadInnerXml(reader, false);
        }

        public static XmlElement ReadInnerXml(XmlReader reader, bool onStartElement)
        {
            if (reader == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("reader");
            }

            string elementName = reader.LocalName;
            string elementNs = reader.NamespaceURI;

            if (reader.IsEmptyElement)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new WSTrustSerializationException(SR.GetString(SR.ID3061, elementName, elementNs)));
            }

            if (!onStartElement)
            {
                reader.ReadStartElement();
            }

            reader.MoveToContent();

            XmlElement securityTokenXml;

            using (MemoryStream ms = new MemoryStream())
            {
                using (XmlWriter writer = XmlDictionaryWriter.CreateTextWriter(ms, Encoding.UTF8, false))
                {
                    writer.WriteNode(reader, true);
                    writer.Flush();
                }

                ms.Seek(0, SeekOrigin.Begin);

                if (ms.Length == 0)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new WSTrustSerializationException(SR.GetString(SR.ID3061, elementName, elementNs)));
                }

                XmlDictionaryReader memoryReader = XmlDictionaryReader.CreateTextReader(ms, Encoding.UTF8, XmlDictionaryReaderQuotas.Max, null);
                XmlDocument dom = new XmlDocument();
                dom.PreserveWhitespace = true;
                dom.Load(memoryReader);
                securityTokenXml = dom.DocumentElement;
            }

            if (!onStartElement)
            {
                reader.ReadEndElement();
            }

            return securityTokenXml;
        }

        public static BinarySecretSecurityToken ReadBinarySecretSecurityToken(XmlReader reader, WSTrustConstantsAdapter trustConstants)
        {
            if (reader == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("reader");
            }

            if (trustConstants == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("trustConstants");
            }

            string base64KeyBytes = reader.ReadElementContentAsString(trustConstants.Elements.BinarySecret, trustConstants.NamespaceURI);

            if (string.IsNullOrEmpty(base64KeyBytes))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new WSTrustSerializationException(SR.GetString(SR.ID3164)));
            }

            return new BinarySecretSecurityToken(Convert.FromBase64String(base64KeyBytes));
        }

        public static void WriteBinarySecretSecurityToken(XmlWriter writer, BinarySecretSecurityToken token, WSTrustConstantsAdapter trustConstants)
        {
            if (writer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("writer");
            }

            if (token == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("token");
            }

            if (trustConstants == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("trustConstants");
            }

            byte[] keyBytes = token.GetKeyBytes();
            writer.WriteStartElement(trustConstants.Elements.BinarySecret, trustConstants.NamespaceURI);
            writer.WriteBase64(keyBytes, 0, keyBytes.Length);
            writer.WriteEndElement();
        }

        /// <summary>
        /// Gets the namespace for a given dialect.
        /// </summary>
        /// <param name="dialect">The dialect.</param>
        /// <returns>The claims namespace.</returns>
        private static string GetRequestClaimNamespace(string dialect)
        {
            if (StringComparer.Ordinal.Equals(dialect, WSAuthorizationConstants.Dialect))
            {
                return WSAuthorizationConstants.Namespace;
            }
            else
            {
                return WSIdentityConstants.Namespace;
            }
        }

        private static string GetRequestClaimPrefix(string dialect)
        {
            if (StringComparer.Ordinal.Equals(dialect, WSAuthorizationConstants.Dialect))
            {
                return WSAuthorizationConstants.Prefix;
            }
            else
            {
                return WSIdentityConstants.Prefix;
            }
        }
        
        /// <summary>
        /// This method is currently called when serializing ActAs and OBO elements.  Factored shared code.
        /// </summary>
        /// <param name="tokenElement">The <see cref="SecurityTokenElement"/> to write.</param>
        /// <param name="usage">A string defining the SecurityTokenCollection to use.</param>
        /// <param name="context">The <see cref="WSTrustSerializationContext"/> of the request.</param>
        /// <param name="writer">The <see cref="XmlWriter"/> to use.</param>
        private static void WriteTokenElement(SecurityTokenElement tokenElement, string usage, WSTrustSerializationContext context, XmlWriter writer)
        {
            if (tokenElement.SecurityTokenXml != null)
            {
                tokenElement.SecurityTokenXml.WriteTo(writer);
            }
            else
            {
                SecurityTokenHandlerCollection tokenHandlerCollection = null;
                if (context.SecurityTokenHandlerCollectionManager.ContainsKey(usage))
                {
                    tokenHandlerCollection = context.SecurityTokenHandlerCollectionManager[usage];
                }
                else
                {
                    // by default this is the default handler collection, review the WSTrustSerializationContext
                    tokenHandlerCollection = context.SecurityTokenHandlers;
                }

                SecurityToken token = tokenElement.GetSecurityToken();
                bool tokenWritten = false;

                if (tokenHandlerCollection != null && tokenHandlerCollection.CanWriteToken(token))
                {
                    tokenHandlerCollection.WriteToken(writer, token);
                    tokenWritten = true;
                }

                if (!tokenWritten)
                {
                    // by default this is the default handler collection, review the WSTrustSerializationContext
                    context.SecurityTokenHandlers.WriteToken(writer, token);
                }
            }
        }
    }
}
