//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------
namespace System.ServiceModel.Security
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IdentityModel.Policy;
    using System.IdentityModel.Selectors;
    using System.IdentityModel.Tokens;
    using System.IO;
    using System.Runtime;
    using System.Runtime.Serialization;
    using System.Security.Authentication.ExtendedProtection;
    using System.Security.Cryptography;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Diagnostics;
    using System.ServiceModel.Dispatcher;
    using System.ServiceModel.Security.Tokens;
    using System.Xml;

    using CanonicalizationDriver = System.IdentityModel.CanonicalizationDriver;
    using Psha1DerivedKeyGenerator = System.IdentityModel.Psha1DerivedKeyGenerator;

    abstract class SspiNegotiationTokenAuthenticator : NegotiationTokenAuthenticator<SspiNegotiationTokenAuthenticatorState>
    {
        ExtendedProtectionPolicy extendedProtectionPolicy;
        string defaultServiceBinding;
        Object thisLock = new Object();

        protected SspiNegotiationTokenAuthenticator()
            : base()
        {
        }

        public ExtendedProtectionPolicy ExtendedProtectionPolicy
        {
            get { return this.extendedProtectionPolicy; }
            set { this.extendedProtectionPolicy = value; }
        }

        protected Object ThisLock
        {
            get { return this.thisLock; }
        }

        public string DefaultServiceBinding
        {
            get 
            {
                if (this.defaultServiceBinding == null)
                {
                    lock (ThisLock)
                    {
                        if (this.defaultServiceBinding == null)
                        {
                            this.defaultServiceBinding = SecurityUtils.GetSpnFromIdentity(
                                                            SecurityUtils.CreateWindowsIdentity(),
                                                            new EndpointAddress(ListenUri));
                        }
                    }
                }

                return this.defaultServiceBinding;
            }
            set { this.defaultServiceBinding = value; }
        }

        // abstract methods
        public abstract XmlDictionaryString NegotiationValueType { get; }
        protected abstract ReadOnlyCollection<IAuthorizationPolicy> ValidateSspiNegotiation(ISspiNegotiation sspiNegotiation);
        protected abstract SspiNegotiationTokenAuthenticatorState CreateSspiState(byte[] incomingBlob, string incomingValueTypeUri);

        // helpers
        protected virtual void IssueServiceToken(SspiNegotiationTokenAuthenticatorState sspiState, ReadOnlyCollection<IAuthorizationPolicy> authorizationPolicies, out SecurityContextSecurityToken serviceToken, out WrappedKeySecurityToken proofToken,
            out int issuedKeySize)
        {
            UniqueId contextId = SecurityUtils.GenerateUniqueId();
            string id = SecurityUtils.GenerateId();
            if (sspiState.RequestedKeySize == 0)
            {
                issuedKeySize = this.SecurityAlgorithmSuite.DefaultSymmetricKeyLength;
            }
            else
            {
                issuedKeySize = sspiState.RequestedKeySize;
            }
            byte[] key = new byte[issuedKeySize / 8];
            CryptoHelper.FillRandomBytes(key);
            DateTime effectiveTime = DateTime.UtcNow;
            DateTime expirationTime = TimeoutHelper.Add(effectiveTime, this.ServiceTokenLifetime);
            serviceToken = IssueSecurityContextToken(contextId, id, key, effectiveTime, expirationTime, authorizationPolicies, this.EncryptStateInServiceToken);
            proofToken = new WrappedKeySecurityToken(string.Empty, key, sspiState.SspiNegotiation);
        }

        protected virtual void ValidateIncomingBinaryNegotiation(BinaryNegotiation incomingNego)
        {
            if (incomingNego == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityNegotiationException(SR.GetString(SR.NoBinaryNegoToReceive)));
            }
            incomingNego.Validate(this.NegotiationValueType);
        }

        protected virtual BinaryNegotiation GetOutgoingBinaryNegotiation(ISspiNegotiation sspiNegotiation, byte[] outgoingBlob)
        {
            return new BinaryNegotiation(this.NegotiationValueType, outgoingBlob);
        }

        static void AddToDigest(HashAlgorithm negotiationDigest, Stream stream)
        {
            stream.Flush();
            stream.Seek(0, SeekOrigin.Begin);
            CanonicalizationDriver canonicalizer = new CanonicalizationDriver();
            canonicalizer.SetInput(stream);
            byte[] canonicalizedData = canonicalizer.GetBytes();
            lock (negotiationDigest)
            {
                negotiationDigest.TransformBlock(canonicalizedData, 0, canonicalizedData.Length, canonicalizedData, 0);
            }
        }

        static void AddToDigest(SspiNegotiationTokenAuthenticatorState sspiState, RequestSecurityToken rst)
        {
            MemoryStream stream = new MemoryStream();
            XmlDictionaryWriter writer = XmlDictionaryWriter.CreateTextWriter(stream);
            rst.RequestSecurityTokenXml.WriteTo(writer);
            writer.Flush();
            AddToDigest(sspiState.NegotiationDigest, stream);
        }

        static void AddToDigest(SspiNegotiationTokenAuthenticatorState sspiState, RequestSecurityTokenResponse rstr, bool wasReceived)
        {
            MemoryStream stream = new MemoryStream();
            XmlDictionaryWriter writer = XmlDictionaryWriter.CreateTextWriter(stream);
            if (wasReceived)
            {
                rstr.RequestSecurityTokenResponseXml.WriteTo(writer);
            }
            else
            {
                rstr.WriteTo(writer);
            }
            writer.Flush();
            AddToDigest(sspiState.NegotiationDigest, stream);
        }

        static byte[] ComputeAuthenticator(SspiNegotiationTokenAuthenticatorState sspiState, byte[] key)
        {
            byte[] negotiationHash;
            lock (sspiState.NegotiationDigest)
            {
                sspiState.NegotiationDigest.TransformFinalBlock(CryptoHelper.EmptyBuffer, 0, 0);
                negotiationHash = sspiState.NegotiationDigest.Hash;
            }
            Psha1DerivedKeyGenerator generator = new Psha1DerivedKeyGenerator(key);
            return generator.GenerateDerivedKey(SecurityUtils.CombinedHashLabel, negotiationHash, SecurityNegotiationConstants.NegotiationAuthenticatorSize, 0);
        }

        // overrides
        protected override bool IsMultiLegNegotiation
        {
            get 
            {
                return true;
            }
        }

        protected override Binding GetNegotiationBinding(Binding binding)
        {
            return binding;
        }

        protected override MessageFilter GetListenerFilter()
        {
            return new SspiNegotiationFilter(this);
        }

        protected override BodyWriter ProcessRequestSecurityToken(Message request, RequestSecurityToken requestSecurityToken, out SspiNegotiationTokenAuthenticatorState negotiationState)
        {
            if (request == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("request");
            }
            if (requestSecurityToken == null)
            {
                throw TraceUtility.ThrowHelperArgumentNull("requestSecurityToken", request);
            }
            if (requestSecurityToken.RequestType != null && requestSecurityToken.RequestType != this.StandardsManager.TrustDriver.RequestTypeIssue)
            {
                throw TraceUtility.ThrowHelperWarning(new SecurityNegotiationException(SR.GetString(SR.InvalidRstRequestType, requestSecurityToken.RequestType)), request);
            }
            BinaryNegotiation incomingNego = requestSecurityToken.GetBinaryNegotiation();
            ValidateIncomingBinaryNegotiation(incomingNego);
            negotiationState = CreateSspiState(incomingNego.GetNegotiationData(), incomingNego.ValueTypeUri);
            AddToDigest(negotiationState, requestSecurityToken);
            negotiationState.Context = requestSecurityToken.Context;
            if (requestSecurityToken.KeySize != 0)
            {
                WSTrust.Driver.ValidateRequestedKeySize(requestSecurityToken.KeySize, this.SecurityAlgorithmSuite);
            }
            negotiationState.RequestedKeySize = requestSecurityToken.KeySize;
            string appliesToName;
            string appliesToNamespace;
            requestSecurityToken.GetAppliesToQName(out appliesToName, out appliesToNamespace);
            if (appliesToName == AddressingStrings.EndpointReference && appliesToNamespace == request.Version.Addressing.Namespace)
            {
                DataContractSerializer serializer;
                if (request.Version.Addressing == AddressingVersion.WSAddressing10)
                {
                    serializer = DataContractSerializerDefaults.CreateSerializer(typeof(EndpointAddress10), DataContractSerializerDefaults.MaxItemsInObjectGraph);
                    negotiationState.AppliesTo = requestSecurityToken.GetAppliesTo<EndpointAddress10>(serializer).ToEndpointAddress();
                }
                else if (request.Version.Addressing == AddressingVersion.WSAddressingAugust2004)
                {
                    serializer = DataContractSerializerDefaults.CreateSerializer(typeof(EndpointAddressAugust2004), DataContractSerializerDefaults.MaxItemsInObjectGraph);
                    negotiationState.AppliesTo = requestSecurityToken.GetAppliesTo<EndpointAddressAugust2004>(serializer).ToEndpointAddress();
                }
                else
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                        new ProtocolException(SR.GetString(SR.AddressingVersionNotSupported, request.Version.Addressing)));
                }

                negotiationState.AppliesToSerializer = serializer;
            }
            return ProcessNegotiation(negotiationState, request, incomingNego);
        }

        protected override BodyWriter ProcessRequestSecurityTokenResponse(SspiNegotiationTokenAuthenticatorState negotiationState, Message request, RequestSecurityTokenResponse requestSecurityTokenResponse)
        {
            if (request == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("request");
            }
            if (requestSecurityTokenResponse == null)
            {
                throw TraceUtility.ThrowHelperArgumentNull("requestSecurityTokenResponse", request);
            }
            if (requestSecurityTokenResponse.Context != negotiationState.Context)
            {
                throw TraceUtility.ThrowHelperError(new SecurityNegotiationException(SR.GetString(SR.BadSecurityNegotiationContext)), request);
            }
            AddToDigest(negotiationState, requestSecurityTokenResponse, true);
            BinaryNegotiation incomingNego = requestSecurityTokenResponse.GetBinaryNegotiation();
            ValidateIncomingBinaryNegotiation(incomingNego);
            return ProcessNegotiation(negotiationState, request, incomingNego);
        }

        BodyWriter ProcessNegotiation(SspiNegotiationTokenAuthenticatorState negotiationState, Message incomingMessage, BinaryNegotiation incomingNego)
        {
            ISspiNegotiation sspiNegotiation = negotiationState.SspiNegotiation;
            
            byte[] outgoingBlob = sspiNegotiation.GetOutgoingBlob(incomingNego.GetNegotiationData(), 
                                                            SecurityUtils.GetChannelBindingFromMessage(incomingMessage), 
                                                            this.extendedProtectionPolicy);

            if (sspiNegotiation.IsValidContext == false)
            {
                throw TraceUtility.ThrowHelperError(new SecurityNegotiationException(SR.GetString(SR.InvalidSspiNegotiation)), incomingMessage);
            }
            // if there is no blob to send back the nego must be complete from the server side
            if (outgoingBlob == null && sspiNegotiation.IsCompleted == false)
            {
                throw TraceUtility.ThrowHelperError(new SecurityNegotiationException(SR.GetString(SR.NoBinaryNegoToSend)), incomingMessage);
            }
            BinaryNegotiation outgoingBinaryNegotiation;
            if (outgoingBlob != null)
            {
                outgoingBinaryNegotiation = GetOutgoingBinaryNegotiation(sspiNegotiation, outgoingBlob); 
            }
            else
            {
                outgoingBinaryNegotiation = null;
            }
            BodyWriter replyBody;
            if (sspiNegotiation.IsCompleted)
            {
                ReadOnlyCollection<IAuthorizationPolicy> authorizationPolicies = ValidateSspiNegotiation(sspiNegotiation);
                SecurityContextSecurityToken serviceToken;
                WrappedKeySecurityToken proofToken;
                int issuedKeySize;
                IssueServiceToken(negotiationState, authorizationPolicies, out serviceToken, out proofToken, out issuedKeySize);
                negotiationState.SetServiceToken(serviceToken);
                
                SecurityKeyIdentifierClause externalTokenReference = this.IssuedSecurityTokenParameters.CreateKeyIdentifierClause(serviceToken, SecurityTokenReferenceStyle.External);
                SecurityKeyIdentifierClause internalTokenReference = this.IssuedSecurityTokenParameters.CreateKeyIdentifierClause(serviceToken, SecurityTokenReferenceStyle.Internal);

                RequestSecurityTokenResponse dummyRstr = new RequestSecurityTokenResponse(this.StandardsManager);
                dummyRstr.Context = negotiationState.Context;
                dummyRstr.KeySize = issuedKeySize;
                dummyRstr.TokenType = this.SecurityContextTokenUri;
                if (outgoingBinaryNegotiation != null)
                {
                    dummyRstr.SetBinaryNegotiation(outgoingBinaryNegotiation);
                }
                dummyRstr.RequestedUnattachedReference = externalTokenReference;
                dummyRstr.RequestedAttachedReference = internalTokenReference;
                dummyRstr.SetLifetime(serviceToken.ValidFrom, serviceToken.ValidTo);
                if (negotiationState.AppliesTo != null)
                {
                    if (incomingMessage.Version.Addressing == AddressingVersion.WSAddressing10)
                    {
                        dummyRstr.SetAppliesTo<EndpointAddress10>(EndpointAddress10.FromEndpointAddress(
                            negotiationState.AppliesTo), 
                            negotiationState.AppliesToSerializer);
                    }
                    else if (incomingMessage.Version.Addressing == AddressingVersion.WSAddressingAugust2004)
                    {
                        dummyRstr.SetAppliesTo<EndpointAddressAugust2004>(EndpointAddressAugust2004.FromEndpointAddress(
                            negotiationState.AppliesTo), 
                            negotiationState.AppliesToSerializer);
                    }
                    else
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                            new ProtocolException(SR.GetString(SR.AddressingVersionNotSupported, incomingMessage.Version.Addressing)));
                    }
                }
                dummyRstr.MakeReadOnly();
                AddToDigest(negotiationState, dummyRstr, false);
                RequestSecurityTokenResponse negotiationRstr = new RequestSecurityTokenResponse(this.StandardsManager);
                negotiationRstr.RequestedSecurityToken = serviceToken;

                negotiationRstr.RequestedProofToken = proofToken;
                negotiationRstr.Context = negotiationState.Context;
                negotiationRstr.KeySize = issuedKeySize;
                negotiationRstr.TokenType = this.SecurityContextTokenUri;
                if (outgoingBinaryNegotiation != null)
                {
                    negotiationRstr.SetBinaryNegotiation(outgoingBinaryNegotiation);
                }
                negotiationRstr.RequestedAttachedReference = internalTokenReference;
                negotiationRstr.RequestedUnattachedReference = externalTokenReference;
                if (negotiationState.AppliesTo != null)
                {
                    if (incomingMessage.Version.Addressing == AddressingVersion.WSAddressing10)
                    {
                        negotiationRstr.SetAppliesTo<EndpointAddress10>(
                            EndpointAddress10.FromEndpointAddress(negotiationState.AppliesTo), 
                            negotiationState.AppliesToSerializer);
                    }
                    else if (incomingMessage.Version.Addressing == AddressingVersion.WSAddressingAugust2004)
                    {
                        negotiationRstr.SetAppliesTo<EndpointAddressAugust2004>(
                            EndpointAddressAugust2004.FromEndpointAddress(negotiationState.AppliesTo), 
                            negotiationState.AppliesToSerializer);
                    }
                    else
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                            new ProtocolException(SR.GetString(SR.AddressingVersionNotSupported, incomingMessage.Version.Addressing)));
                    }
                }
                negotiationRstr.MakeReadOnly();

                byte[] authenticator = ComputeAuthenticator(negotiationState, serviceToken.GetKeyBytes());
                RequestSecurityTokenResponse authenticatorRstr = new RequestSecurityTokenResponse(this.StandardsManager);
                authenticatorRstr.Context = negotiationState.Context;
                authenticatorRstr.SetAuthenticator(authenticator);
                authenticatorRstr.MakeReadOnly();

                List<RequestSecurityTokenResponse> rstrList = new List<RequestSecurityTokenResponse>(2);
                rstrList.Add(negotiationRstr);
                rstrList.Add(authenticatorRstr);
                replyBody = new RequestSecurityTokenResponseCollection(rstrList, this.StandardsManager);
               
            }
            else
            {
                RequestSecurityTokenResponse rstr = new RequestSecurityTokenResponse(this.StandardsManager);
                rstr.Context = negotiationState.Context;
                rstr.SetBinaryNegotiation(outgoingBinaryNegotiation);
                rstr.MakeReadOnly();
                AddToDigest(negotiationState, rstr, false);
                replyBody = rstr;
            }

            return replyBody;
        }

        class SspiNegotiationFilter : HeaderFilter
        {
            SspiNegotiationTokenAuthenticator authenticator;
            
            public SspiNegotiationFilter(SspiNegotiationTokenAuthenticator authenticator)
            {
                this.authenticator = authenticator;
            }

            public override bool Match(Message message)
            {
                if (message.Headers.Action == authenticator.RequestSecurityTokenAction.Value
                    || message.Headers.Action == authenticator.RequestSecurityTokenResponseAction.Value)
                {
                    return !SecurityVersion.Default.DoesMessageContainSecurityHeader(message);
                }
                else
                {
                    return false;
                }
            }
        }
    }
}
