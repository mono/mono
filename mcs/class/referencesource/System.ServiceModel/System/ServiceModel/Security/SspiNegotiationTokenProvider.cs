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
    using System.Security.Authentication.ExtendedProtection;
    using System.Security.Cryptography;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Diagnostics;
    using System.ServiceModel.Security.Tokens;
    using System.Xml;

    using CanonicalizationDriver = System.IdentityModel.CanonicalizationDriver;
    using Psha1DerivedKeyGenerator = System.IdentityModel.Psha1DerivedKeyGenerator;
    using SafeFreeCredentials = System.IdentityModel.SafeFreeCredentials;

    abstract class SspiNegotiationTokenProvider : NegotiationTokenProvider<SspiNegotiationTokenProviderState>
    {
        bool negotiateTokenOnOpen;
        SecurityBindingElement securityBindingElement;

        protected SspiNegotiationTokenProvider()
            : this(null)
        {
        }
        
        protected SspiNegotiationTokenProvider(SecurityBindingElement securityBindingElement)
            : base()
        {
            this.securityBindingElement = securityBindingElement;
        }

        public bool NegotiateTokenOnOpen
        {
            get
            {
                return this.negotiateTokenOnOpen;
            }
            set
            {
                this.CommunicationObject.ThrowIfDisposedOrImmutable();
                this.negotiateTokenOnOpen = value;
            }
        }

        // SspiNegotiationTokenProvider abstract methods
        protected abstract ReadOnlyCollection<IAuthorizationPolicy> ValidateSspiNegotiation(ISspiNegotiation sspiNegotiation);
        public abstract XmlDictionaryString NegotiationValueType { get; }
        
        public override void OnOpen(TimeSpan timeout)
        {
            TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
            this.EnsureEndpointAddressDoesNotRequireEncryption(this.TargetAddress);
            base.OnOpen(timeoutHelper.RemainingTime());
            if (this.negotiateTokenOnOpen)
            {
                this.DoNegotiation(timeoutHelper.RemainingTime());
            }
        }

        protected override IChannelFactory<IRequestChannel> GetNegotiationChannelFactory(IChannelFactory<IRequestChannel> transportChannelFactory, ChannelBuilder channelBuilder)
        {
            return transportChannelFactory;
        }

        // helper methods
        void ValidateIncomingBinaryNegotiation(BinaryNegotiation incomingNego)
        {
            incomingNego.Validate(NegotiationValueType);
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

        static void AddToDigest(SspiNegotiationTokenProviderState sspiState, RequestSecurityToken rst)
        {
            MemoryStream stream = new MemoryStream();
            XmlDictionaryWriter writer = XmlDictionaryWriter.CreateTextWriter(stream);
            rst.WriteTo(writer);
            writer.Flush();
            AddToDigest(sspiState.NegotiationDigest, stream);
        }

        void AddToDigest(SspiNegotiationTokenProviderState sspiState, RequestSecurityTokenResponse rstr, bool wasReceived, bool isFinalRstr)
        {
            MemoryStream stream = new MemoryStream();
            XmlDictionaryWriter writer = XmlDictionaryWriter.CreateTextWriter(stream);
            if (!wasReceived)
            {
                rstr.WriteTo(writer);
            }
            else
            {
                if (!isFinalRstr)
                {
                    rstr.RequestSecurityTokenResponseXml.WriteTo(writer);
                }
                else
                {
                    XmlElement rstrClone = (XmlElement) rstr.RequestSecurityTokenResponseXml.CloneNode(true);
                    List<XmlNode> nodesToRemove = new List<XmlNode>(2);
                    for (int i = 0; i < rstrClone.ChildNodes.Count; ++i)
                    {
                        XmlNode child = (rstrClone.ChildNodes[i]);
                        if (this.StandardsManager.TrustDriver.IsRequestedSecurityTokenElement(child.LocalName, child.NamespaceURI))
                        {
                            nodesToRemove.Add(child);
                        }
                        else if (this.StandardsManager.TrustDriver.IsRequestedProofTokenElement(child.LocalName, child.NamespaceURI))
                        {
                            nodesToRemove.Add(child);
                        }
                    }
                    for (int i = 0; i < nodesToRemove.Count; ++i)
                    {
                        rstrClone.RemoveChild(nodesToRemove[i]);
                    }
                    rstrClone.WriteTo(writer);
                }
            }
            writer.Flush();
            AddToDigest(sspiState.NegotiationDigest, stream);
        }

        static bool IsCorrectAuthenticator(SspiNegotiationTokenProviderState sspiState, byte[] proofKey, byte[] serverAuthenticator)
        {
            byte[] negotiationHash;
            lock (sspiState.NegotiationDigest)
            {
                sspiState.NegotiationDigest.TransformFinalBlock(CryptoHelper.EmptyBuffer, 0, 0);
                negotiationHash = sspiState.NegotiationDigest.Hash;
            }
            Psha1DerivedKeyGenerator generator = new Psha1DerivedKeyGenerator(proofKey);
            byte[] clientAuthenticator = generator.GenerateDerivedKey(SecurityUtils.CombinedHashLabel, negotiationHash, SecurityNegotiationConstants.NegotiationAuthenticatorSize, 0);
            if (clientAuthenticator.Length != serverAuthenticator.Length)
            {
                return false;
            }
            for (int i = 0; i < clientAuthenticator.Length; ++i)
            {
                if (clientAuthenticator[i] != serverAuthenticator[i])
                {
                    return false;
                }
            }
            return true;
        }

        BodyWriter PrepareRstr( SspiNegotiationTokenProviderState sspiState, byte[] outgoingBlob )
        {
            RequestSecurityTokenResponse rstr = new RequestSecurityTokenResponse(this.StandardsManager);
            rstr.Context = sspiState.Context;
            rstr.SetBinaryNegotiation(new BinaryNegotiation(NegotiationValueType, outgoingBlob));
            rstr.MakeReadOnly();
            AddToDigest(sspiState, rstr, false, false);
            return rstr;
        }

        protected override BodyWriter GetFirstOutgoingMessageBody( SspiNegotiationTokenProviderState sspiState, out MessageProperties messageProperties )
        {
            messageProperties = null;

            // both message logging and Visual Studio trigger message serialization and hence can cause 
            // premature invocation of OnGetBinaryNegotiation(); flag this RST as streamed to block 
            // serialization of its body and hence premature calls to InitializeSecurityContext()

            RequestSecurityToken rst = new RequestSecurityToken(this.StandardsManager, false);
            rst.Context = sspiState.Context;
            rst.TokenType = this.StandardsManager.SecureConversationDriver.TokenTypeUri;
            rst.KeySize = this.SecurityAlgorithmSuite.DefaultSymmetricKeyLength;

            // delay GetOutgoingBlob()'s first call to InitializeSecurityContext() until a channel binding
            // is available

            rst.OnGetBinaryNegotiation = (new GetOutgoingBlobProxy(sspiState, this, rst)).GetOutgoingBlob;

            return rst;
        }

        protected override IRequestChannel CreateClientChannel( EndpointAddress target, Uri via )
        {
            IRequestChannel rstChannel = base.CreateClientChannel(target, via);

            if (!SecurityUtils.IsChannelBindingDisabled && (this.securityBindingElement is TransportSecurityBindingElement))
            {
                // enable channel binding on this side channel
                IChannelBindingProvider cbp = rstChannel.GetProperty<IChannelBindingProvider>();
                if (cbp != null)
                {
                    cbp.EnableChannelBindingSupport();
                }
            }

            return rstChannel;
        }

        /// <summary>
        /// Proxy helps in implementating the delay of obtaining the binary data till later in the stack until the 
        /// ChannelBinding is obtained from the message.
        /// </summary>
        class GetOutgoingBlobProxy
        {
            RequestSecurityToken _rst;
            SspiNegotiationTokenProvider _sspiProvider;
            SspiNegotiationTokenProviderState _sspiState;

            public GetOutgoingBlobProxy( SspiNegotiationTokenProviderState sspiState, SspiNegotiationTokenProvider sspiProvider, RequestSecurityToken rst )
            {
                _sspiState = sspiState;
                _sspiProvider = sspiProvider;
                _rst = rst;
            }

            public void GetOutgoingBlob( ChannelBinding channelBinding )
            {                
                byte[] outgoingBlob = _sspiState.SspiNegotiation.GetOutgoingBlob(null, channelBinding, null);

                if (outgoingBlob == null && _sspiState.SspiNegotiation.IsCompleted == false)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityNegotiationException(SR.GetString(SR.NoBinaryNegoToSend)));
                }

                _rst.SetBinaryNegotiation(new BinaryNegotiation(_sspiProvider.NegotiationValueType, outgoingBlob));
                SspiNegotiationTokenProvider.AddToDigest(_sspiState, _rst);
                _rst.MakeReadOnly();
            }
        }

        protected override BodyWriter GetNextOutgoingMessageBody(Message incomingMessage, SspiNegotiationTokenProviderState sspiState)
        {
            try
            {
                ThrowIfFault(incomingMessage, this.TargetAddress);
            }
            catch (FaultException fault)
            {
                if (fault.Code.IsSenderFault)
                {
                    if (fault.Code.SubCode.Name == TrustApr2004Strings.FailedAuthenticationFaultCode || fault.Code.SubCode.Name == TrustFeb2005Strings.FailedAuthenticationFaultCode)
                        throw TraceUtility.ThrowHelperError(new SecurityNegotiationException(SR.GetString(SR.AuthenticationOfClientFailed), fault), incomingMessage);

                    throw TraceUtility.ThrowHelperError(new SecurityNegotiationException(SR.GetString(SR.FailedSspiNegotiation), fault), incomingMessage);
                }
                else
                {
                    throw;
                }
            }
            RequestSecurityTokenResponse negotiationRstr = null;
            RequestSecurityTokenResponse authenticatorRstr = null;
            XmlDictionaryReader bodyReader = incomingMessage.GetReaderAtBodyContents();
            using (bodyReader)
            {
                if (this.StandardsManager.TrustDriver.IsAtRequestSecurityTokenResponseCollection(bodyReader))
                {
                    RequestSecurityTokenResponseCollection rstrCollection = this.StandardsManager.TrustDriver.CreateRequestSecurityTokenResponseCollection(bodyReader);
                    using (IEnumerator<RequestSecurityTokenResponse> enumerator = rstrCollection.RstrCollection.GetEnumerator())
                    {
                        enumerator.MoveNext();
                        negotiationRstr = enumerator.Current;
                        if (enumerator.MoveNext())
                        {
                            authenticatorRstr = enumerator.Current;
                        }
                    }
                    if (authenticatorRstr == null)
                    {
                        throw TraceUtility.ThrowHelperError(new SecurityNegotiationException(SR.GetString(SR.AuthenticatorNotPresentInRSTRCollection)), incomingMessage);
                    }
                    else if (authenticatorRstr.Context != negotiationRstr.Context)
                    {
                        throw TraceUtility.ThrowHelperError(new SecurityNegotiationException(SR.GetString(SR.RSTRAuthenticatorHasBadContext)), incomingMessage);
                    }
                    AddToDigest(sspiState, negotiationRstr, true, true);
                }
                else if (this.StandardsManager.TrustDriver.IsAtRequestSecurityTokenResponse(bodyReader))
                {
                    negotiationRstr = RequestSecurityTokenResponse.CreateFrom(this.StandardsManager, bodyReader);
                    AddToDigest(sspiState, negotiationRstr, true, false);
                }
                else
                {
                    this.StandardsManager.TrustDriver.OnRSTRorRSTRCMissingException();
                }
                incomingMessage.ReadFromBodyContentsToEnd(bodyReader);
            }
            if (negotiationRstr.Context != sspiState.Context)
            {
                throw TraceUtility.ThrowHelperError(new SecurityNegotiationException(SR.GetString(SR.BadSecurityNegotiationContext)), incomingMessage);
            }
            BinaryNegotiation incomingBinaryNego = negotiationRstr.GetBinaryNegotiation();
            byte[] incomingBlob;
            if (incomingBinaryNego != null)
            {
                ValidateIncomingBinaryNegotiation(incomingBinaryNego);
                incomingBlob = incomingBinaryNego.GetNegotiationData();
            }
            else
            {
                incomingBlob = null;
            }
            BodyWriter nextMessageBody;
            if (incomingBlob == null && sspiState.SspiNegotiation.IsCompleted == false)
            {
                throw TraceUtility.ThrowHelperError(new SecurityNegotiationException(SR.GetString(SR.NoBinaryNegoToReceive)), incomingMessage);
            }
            else if (incomingBlob == null && sspiState.SspiNegotiation.IsCompleted == true)
            {
                // the incoming RSTR must have the negotiated token
                OnNegotiationComplete(sspiState, negotiationRstr, authenticatorRstr);
                nextMessageBody = null;
            }
            else
            {
                // we got an incoming blob. Process it and see if there is an outgoing blob
                byte[] outgoingBlob = sspiState.SspiNegotiation.GetOutgoingBlob(incomingBlob, 
                                                            SecurityUtils.GetChannelBindingFromMessage(incomingMessage), 
                                                            null);

                if (outgoingBlob == null && sspiState.SspiNegotiation.IsCompleted == false)
                {
                    throw TraceUtility.ThrowHelperError(new SecurityNegotiationException(SR.GetString(SR.NoBinaryNegoToSend)), incomingMessage);
                }
                else if (outgoingBlob == null && sspiState.SspiNegotiation.IsCompleted == true)
                {
                    // the incoming RSTR had the last blob. It must have the token too
                    this.OnNegotiationComplete(sspiState, negotiationRstr, authenticatorRstr);
                    nextMessageBody = null;
                }
                else
                {
                    nextMessageBody = PrepareRstr(sspiState, outgoingBlob);
                }
            }
            return nextMessageBody;
        }

        void OnNegotiationComplete(SspiNegotiationTokenProviderState sspiState, RequestSecurityTokenResponse negotiationRstr, RequestSecurityTokenResponse authenticatorRstr)
        {
            ISspiNegotiation sspiNegotiation = sspiState.SspiNegotiation;
            ReadOnlyCollection<IAuthorizationPolicy> authorizationPolicies = ValidateSspiNegotiation(sspiNegotiation);
            // the negotiation has completed successfully - the service token needs to be extracted from the
            // negotiationRstr
            SecurityTokenResolver tokenResolver = new SspiSecurityTokenResolver(sspiNegotiation);
            GenericXmlSecurityToken serviceToken = negotiationRstr.GetIssuedToken(tokenResolver, EmptyReadOnlyCollection<SecurityTokenAuthenticator>.Instance, 
                SecurityKeyEntropyMode.ServerEntropy, null, this.SecurityContextTokenUri, authorizationPolicies, 0, false);
            if (serviceToken == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityNegotiationException(SR.GetString(SR.NoServiceTokenReceived)));
            }
            WrappedKeySecurityToken wrappedToken = (serviceToken.ProofToken as WrappedKeySecurityToken);
            if (wrappedToken == null || wrappedToken.WrappingAlgorithm != sspiNegotiation.KeyEncryptionAlgorithm)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityNegotiationException(SR.GetString(SR.ProofTokenWasNotWrappedCorrectly)));
            }
            byte[] proofKey = wrappedToken.GetWrappedKey();
            if (authenticatorRstr == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityNegotiationException(SR.GetString(SR.RSTRAuthenticatorNotPresent)));
            }
            byte[] serverAuthenticator = authenticatorRstr.GetAuthenticator();
            if (serverAuthenticator == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityNegotiationException(SR.GetString(SR.RSTRAuthenticatorNotPresent)));
            }
            if (!IsCorrectAuthenticator(sspiState, proofKey, serverAuthenticator))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityNegotiationException(SR.GetString(SR.RSTRAuthenticatorIncorrect)));
            }
            sspiState.SetServiceToken(serviceToken);
        }

        class SspiSecurityTokenResolver : SecurityTokenResolver, ISspiNegotiationInfo
        {
            ISspiNegotiation sspiNegotiation;

            public SspiSecurityTokenResolver(ISspiNegotiation sspiNegotiation)
            {
                this.sspiNegotiation = sspiNegotiation;
            }

            public ISspiNegotiation SspiNegotiation 
            {
                get { return this.sspiNegotiation; }
            }

            protected override bool TryResolveTokenCore(SecurityKeyIdentifier keyIdentifier, out SecurityToken token)
            {
                token = null;
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotImplementedException());
            }

            protected override bool TryResolveTokenCore(SecurityKeyIdentifierClause keyIdentifierClause, out SecurityToken token)
            {
                token = null;
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotImplementedException());
            }

            protected override bool TryResolveSecurityKeyCore(SecurityKeyIdentifierClause keyIdentifierClause, out SecurityKey key)
            {
                key = null;
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotImplementedException());
            }
        }
    }

    class SspiIssuanceChannelParameter
    {
        bool getTokenOnOpen;
        SafeFreeCredentials credentialsHandle;

        public SspiIssuanceChannelParameter(bool getTokenOnOpen, SafeFreeCredentials credentialsHandle)
        {
            this.getTokenOnOpen = getTokenOnOpen;
            this.credentialsHandle = credentialsHandle;
        }

        public bool GetTokenOnOpen
        {
            get { return this.getTokenOnOpen; }
        }

        public SafeFreeCredentials CredentialsHandle
        {
            get { return this.credentialsHandle; }
        }
    }

}
