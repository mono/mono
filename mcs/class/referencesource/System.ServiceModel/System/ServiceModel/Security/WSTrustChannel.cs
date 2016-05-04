//------------------------------------------------------------------------------
//     Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------

namespace System.ServiceModel.Security
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IdentityModel;
    using System.IdentityModel.Policy;
    using System.IdentityModel.Protocols.WSTrust;
    using System.Runtime;
    using System.IdentityModel.Tokens;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Security.Tokens;
    using IM = System.IdentityModel;
    using SR = System.ServiceModel.SR;

    /// <summary>
    /// A channel that is used to send WS-Trust messages to an STS.
    /// </summary>
    public class WSTrustChannel : IWSTrustChannelContract, IChannel
    {
        // Consistent with the IDFX STS configuration default.
        const int DefaultKeySizeInBits = 256;
        const int FaultMaxBufferSize = 20 * 1024;

        internal class WSTrustChannelAsyncResult : System.IdentityModel.AsyncResult
        {
            public enum Operations { Cancel, Issue, Renew, Validate };

            IWSTrustContract _client;
            System.IdentityModel.Protocols.WSTrust.RequestSecurityToken _rst;
            WSTrustSerializationContext _serializationContext;
            Message _response;
            Operations _operation;

            public WSTrustChannelAsyncResult(IWSTrustContract client,
                                              Operations operation,
                                              System.IdentityModel.Protocols.WSTrust.RequestSecurityToken rst,
                                              WSTrustSerializationContext serializationContext,
                                              Message request,
                                              AsyncCallback callback,
                                              object state)
                : base(callback, state)
            {
                _client = client;
                _rst = rst;
                _serializationContext = serializationContext;
                _operation = operation;

                switch (_operation)
                {
                    case Operations.Issue:
                        client.BeginIssue(request, OnOperationCompleted, null);
                        break;
                    case Operations.Cancel:
                        client.BeginCancel(request, OnOperationCompleted, null);
                        break;
                    case Operations.Renew:
                        client.BeginRenew(request, OnOperationCompleted, null);
                        break;
                    case Operations.Validate:
                        client.BeginValidate(request, OnOperationCompleted, null);
                        break;
                    default:
                        throw IM.DiagnosticUtility.ThrowHelperInvalidOperation(SR.GetString(SR.ID3285, Enum.GetName(typeof(Operations), _operation)));
                }
            }

            public IWSTrustContract Client
            {
                get { return _client; }
                set { _client = value; }
            }

            public System.IdentityModel.Protocols.WSTrust.RequestSecurityToken RequestSecurityToken
            {
                get { return _rst; }
                set { _rst = value; }
            }

            public Message Response
            {
                get { return _response; }
                set { _response = value; }
            }

            public WSTrustSerializationContext SerializationContext
            {
                get { return _serializationContext; }
                set { _serializationContext = value; }
            }

            public new static Message End(IAsyncResult iar)
            {
                System.IdentityModel.AsyncResult.End(iar);

                WSTrustChannelAsyncResult tcar = iar as WSTrustChannelAsyncResult;
                if (tcar == null)
                {
                    throw IM.DiagnosticUtility.ThrowHelperInvalidOperation(SR.GetString(SR.ID2004, typeof(WSTrustChannelAsyncResult), iar.GetType()));
                }

                return tcar.Response;
            }

            void OnOperationCompleted(IAsyncResult iar)
            {
                try
                {
                    this.Response = EndOperation(iar);
                    this.Complete(iar.CompletedSynchronously);
                }
                catch (Exception ex)
                {
                    if (Fx.IsFatal(ex))
                    {
                        throw;
                    }
                    this.Complete(false, ex);
                }
            }

            Message EndOperation(IAsyncResult iar)
            {
                switch (_operation)
                {
                    case Operations.Cancel:
                        return this.Client.EndCancel(iar);
                    case Operations.Issue:
                        return this.Client.EndIssue(iar);
                    case Operations.Renew:
                        return this.Client.EndRenew(iar);
                    case Operations.Validate:
                        return this.Client.EndValidate(iar);
                    default:
                        throw IM.DiagnosticUtility.ThrowHelperInvalidOperation(SR.GetString(SR.ID3285, _operation));
                }
            }
        }

        //
        // The channel factory that created this channel.
        //
        WSTrustChannelFactory _factory;

        //
        // All IChannel calls delegate to this.
        //
        IChannel _innerChannel;

        //
        // All Message-in/Message-out calls are sent through this.
        //
        IWSTrustChannelContract _innerContract;

        //
        // The serializers and the serialization context are used to write and read
        // WS-Trust messages.
        //
        MessageVersion _messageVersion;
        TrustVersion _trustVersion;
        WSTrustSerializationContext _context;
        WSTrustRequestSerializer _wsTrustRequestSerializer;
        WSTrustResponseSerializer _wsTrustResponseSerializer;

        /// <summary>
        /// The <see cref="IChannel" /> this class uses for sending and receiving <see cref="Message" /> objects.
        /// </summary>
        public IChannel Channel
        {
            get
            {
                return _innerChannel;
            }
            protected set
            {
                _innerChannel = value;
            }
        }

        /// <summary>
        /// The <see cref="WSTrustChannelFactory" /> that created this object.
        /// </summary>
        public WSTrustChannelFactory ChannelFactory
        {
            get
            {
                return _factory;
            }
            protected set
            {
                _factory = value;
            }
        }

        /// <summary>
        /// The <see cref="IWSTrustChannelContract" /> this class uses for sending and receiving 
        /// <see cref="Message" /> objects.
        /// </summary>
        public IWSTrustChannelContract Contract
        {
            get
            {
                return _innerContract;
            }
            protected set
            {
                _innerContract = value;
            }
        }

        /// <summary>
        /// The version of WS-Trust this channel will use for serializing <see cref="Message" /> objects.
        /// </summary>
        public TrustVersion TrustVersion
        {
            get
            {
                return _trustVersion;
            }
            protected set
            {
                if (!((value == null) || (value == TrustVersion.WSTrust13) || (value == TrustVersion.WSTrustFeb2005)))
                {
                }
                _trustVersion = value;
            }
        }

        /// <summary>
        /// The <see cref="WSTrustSerializationContext" /> this channel will use for serializing WS-Trust messages.
        /// </summary>
        public WSTrustSerializationContext WSTrustSerializationContext
        {
            get
            {
                return _context;
            }
            protected set
            {
                _context = value;
            }
        }

        /// <summary>
        /// The <see cref="WSTrustRequestSerializer" /> this channel will use for serializing WS-Trust request messages.
        /// </summary>
        public WSTrustRequestSerializer WSTrustRequestSerializer
        {
            get
            {
                return _wsTrustRequestSerializer;
            }
            protected set
            {
                _wsTrustRequestSerializer = value;
            }
        }

        /// <summary>
        /// The <see cref="WSTrustResponseSerializer" /> this channel will use for serializing WS-Trust response
        /// messages.
        /// </summary>
        public WSTrustResponseSerializer WSTrustResponseSerializer
        {
            get
            {
                return _wsTrustResponseSerializer;
            }
            protected set
            {
                _wsTrustResponseSerializer = value;
            }
        }

        /// <summary>
        /// Constructs a <see cref="WSTrustChannel" />.
        /// </summary>
        /// <param name="factory">The <see cref="WSTrustChannelFactory" /> that is creating this object.</param>
        /// <param name="inner">
        /// The <see cref="IWSTrustChannelContract" /> this object will use to send and receive 
        /// <see cref="Message" /> objects.
        /// </param>
        /// <param name="trustVersion">
        /// The version of WS-Trust this channel will use for serializing <see cref="Message" /> objects.
        /// </param>
        /// <param name="context">
        /// The <see cref="WSTrustSerializationContext" /> this channel will use for serializing WS-Trust messages.
        /// </param>
        /// <param name="requestSerializer">
        /// The <see cref="WSTrustRequestSerializer" /> this channel will use for serializing WS-Trust request messages.
        /// </param>
        /// <param name="responseSerializer">
        /// The <see cref="WSTrustResponseSerializer" /> this channel will use for serializing WS-Trust response
        /// messages.
        /// </param>
        public WSTrustChannel(WSTrustChannelFactory factory,
                               IWSTrustChannelContract inner,
                               TrustVersion trustVersion,
                               WSTrustSerializationContext context,
                               WSTrustRequestSerializer requestSerializer,
                               WSTrustResponseSerializer responseSerializer)
        {
            if (factory == null)
            {
                throw IM.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("inner");
            }

            if (inner == null)
            {
                throw IM.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("inner");
            }

            if (context == null)
            {
                throw IM.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");
            }

            if (requestSerializer == null)
            {
                throw IM.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("requestSerializer");
            }

            if (responseSerializer == null)
            {
                throw IM.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("responseSerializer");
            }

            if (trustVersion == null)
            {
                throw IM.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("trustVersion");
            }

            _innerChannel = inner as IChannel;
            if (_innerChannel == null)
            {
                throw IM.DiagnosticUtility.ThrowHelperInvalidOperation(SR.GetString(SR.ID3286));
            }

            _innerContract = inner;
            _factory = factory;
            _context = context;
            _wsTrustRequestSerializer = requestSerializer;
            _wsTrustResponseSerializer = responseSerializer;
            _trustVersion = trustVersion;

            //
            // Use the Binding's MessageVersion for creating our requests.
            //
            _messageVersion = MessageVersion.Default;
            if (_factory.Endpoint != null
                && _factory.Endpoint.Binding != null
                && _factory.Endpoint.Binding.MessageVersion != null)
            {
                _messageVersion = _factory.Endpoint.Binding.MessageVersion;
            }
        }

        /// <summary>
        /// Creates a <see cref="Message"/> object that represents a WS-Trust RST message.
        /// </summary>
        /// <param name="request">The <see cref="System.IdentityModel.Protocols.WSTrust.RequestSecurityToken"/> to serialize into the message.</param>
        /// <param name="requestType">The type of WS-Trust request to serialize. This parameter must be one of the
        /// string constants in <see cref="RequestTypes" />.</param>                
        /// <returns>The <see cref="Message" /> object that represents the WS-Trust message.</returns>
        protected virtual Message CreateRequest(System.IdentityModel.Protocols.WSTrust.RequestSecurityToken request, string requestType)
        {
            return Message.CreateMessage(_messageVersion,
                                          GetRequestAction(requestType, TrustVersion),
                                          new WSTrustRequestBodyWriter(request,
                                                                        WSTrustRequestSerializer,
                                                                        WSTrustSerializationContext));
        }

        /// <summary>
        /// Deserializes a <see cref="System.IdentityModel.Protocols.WSTrust.RequestSecurityTokenResponse" /> from a <see cref="Message" />
        /// received from the WS-Trust endpoint.
        /// </summary>
        /// <param name="response">The <see cref="Message" /> received from the WS-Trust endpoint.</param>
        /// <returns>
        /// The <see cref="System.IdentityModel.Protocols.WSTrust.RequestSecurityTokenResponse" /> deserialized from <paramref name="response"/>.
        /// </returns>
        protected virtual System.IdentityModel.Protocols.WSTrust.RequestSecurityTokenResponse ReadResponse(Message response)
        {
            if (response == null)
            {
                throw IM.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("response");
            }

            if (response.IsFault)
            {
                MessageFault fault = MessageFault.CreateFault(response, WSTrustChannel.FaultMaxBufferSize);
                string action = null;
                if (response.Headers != null)
                {
                    action = response.Headers.Action;
                }
                FaultException faultException = FaultException.CreateFault(fault, action);

                 throw FxTrace.Exception.AsError(faultException);
            }

            return WSTrustResponseSerializer.ReadXml(response.GetReaderAtBodyContents(), WSTrustSerializationContext);
        }

        /// <summary>
        /// Gets the WS-Addressing SOAP action that corresponds to the provided request type and
        /// WS-Trust version.
        /// </summary>
        /// <param name="requestType">The type of WS-Trust request. This parameter must be one of the
        /// string constants in <see cref="RequestTypes" />.</param>
        /// <param name="trustVersion">The <see cref="TrustVersion" /> of the request.</param>
        /// <returns>The WS-Addressing action to use.</returns>
        protected static string GetRequestAction(string requestType, TrustVersion trustVersion)
        {
            if (trustVersion != TrustVersion.WSTrust13 && trustVersion != TrustVersion.WSTrustFeb2005)
            {
                throw IM.DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new NotSupportedException(SR.GetString(SR.ID3137, trustVersion.ToString())));
            }

            switch (requestType)
            {
                case RequestTypes.Cancel:
                    return trustVersion == TrustVersion.WSTrustFeb2005 ?
                        WSTrustFeb2005Constants.Actions.Cancel : WSTrust13Constants.Actions.Cancel;

                case RequestTypes.Issue:
                    return trustVersion == TrustVersion.WSTrustFeb2005 ?
                        WSTrustFeb2005Constants.Actions.Issue : WSTrust13Constants.Actions.Issue;

                case RequestTypes.Renew:
                    return trustVersion == TrustVersion.WSTrustFeb2005 ?
                        WSTrustFeb2005Constants.Actions.Renew : WSTrust13Constants.Actions.Renew;

                case RequestTypes.Validate:
                    return trustVersion == TrustVersion.WSTrustFeb2005 ?
                        WSTrustFeb2005Constants.Actions.Validate : WSTrust13Constants.Actions.Validate;

                default:
                    throw IM.DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                        new NotSupportedException(SR.GetString(SR.ID3141, requestType.ToString())));
            }
        }

        /// <summary>
        ///     Get the security token from the RSTR
        /// </summary>
        /// <param name="request">The request used to ask for the security token.</param>
        /// <param name="response">The response containing the security token</param>
        /// <returns>parsed security token.</returns>
        /// <exception cref="ArgumentNullException">If response is null</exception>
        public virtual SecurityToken GetTokenFromResponse(System.IdentityModel.Protocols.WSTrust.RequestSecurityToken request, System.IdentityModel.Protocols.WSTrust.RequestSecurityTokenResponse response)
        {
            if (null == response)
            {
                throw IM.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("response");
            }

            if (!response.IsFinal)
            {
                throw IM.DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new NotImplementedException(SR.GetString(SR.ID3270)));
            }

            if (response.RequestedSecurityToken == null)
            {
                return null;
            }

            SecurityToken issuedToken = response.RequestedSecurityToken.SecurityToken;

            // if we couldn't get the security token via the simple access above, try the token xml
            if (issuedToken == null)
            {
                if (response.RequestedSecurityToken.SecurityTokenXml == null)
                {
                    throw IM.DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                        new InvalidOperationException(SR.GetString(SR.ID3138)));
                }

                SecurityToken proofToken = GetProofKey(request, response);

                //
                // If we don't see a lifetime in the response we set the expires time to 
                // 10 hours from created time.
                //
                DateTime? created = null;
                DateTime? expires = null;

                if (response.Lifetime != null)
                {
                    created = response.Lifetime.Created;
                    expires = response.Lifetime.Expires;

                    if (created == null)
                    {
                        created = DateTime.UtcNow;
                    }
                    if (expires == null)
                    {
                        expires = DateTime.UtcNow.AddHours(10);
                    }
                }
                else
                {
                    created = DateTime.UtcNow;
                    expires = DateTime.UtcNow.AddHours(10);
                }

                return new GenericXmlSecurityToken(response.RequestedSecurityToken.SecurityTokenXml,
                                                    proofToken,
                                                    created.Value,
                                                    expires.Value,
                                                    response.RequestedAttachedReference,
                                                    response.RequestedUnattachedReference,
                                                    new ReadOnlyCollection<IAuthorizationPolicy>(new List<IAuthorizationPolicy>()));
            }
            else
            {
                return issuedToken;
            }
        }

        internal static SecurityToken GetUseKeySecurityToken(UseKey useKey, string requestKeyType)
        {
            if (useKey != null && useKey.Token != null)
            {
                return useKey.Token;
            }
            else
            {
                throw IM.DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new NotSupportedException(SR.GetString(SR.ID3190, requestKeyType)));
            }
        }

        /// <summary>
        /// The types of proof keys that can be issued in WS-Trust
        /// </summary>
        internal enum ProofKeyType { Unknown, Bearer, Symmetric, Asymmetric };

        /// <summary>
        /// Determines the ProofKeyType corresponding to the Uri contents
        /// enclosed in WS-Trust KeyType elements.
        /// </summary>
        internal static ProofKeyType GetKeyType(string keyType)
        {
            if (keyType == WSTrust13Constants.KeyTypes.Symmetric
                || keyType == WSTrustFeb2005Constants.KeyTypes.Symmetric
                || keyType == KeyTypes.Symmetric
                || String.IsNullOrEmpty(keyType))
            {
                return ProofKeyType.Symmetric;
            }
            else if (keyType == WSTrust13Constants.KeyTypes.Asymmetric
                     || keyType == WSTrustFeb2005Constants.KeyTypes.Asymmetric
                     || keyType == KeyTypes.Asymmetric)
            {
                return ProofKeyType.Asymmetric;
            }
            else if (keyType == WSTrust13Constants.KeyTypes.Bearer
                     || keyType == WSTrustFeb2005Constants.KeyTypes.Bearer
                     || keyType == KeyTypes.Bearer)
            {
                return ProofKeyType.Bearer;
            }
            else
            {
                return ProofKeyType.Unknown;
            }
        }

        internal static bool IsPsha1(string algorithm)
        {
            return (algorithm == WSTrust13Constants.ComputedKeyAlgorithms.PSHA1
                  || algorithm == WSTrustFeb2005Constants.ComputedKeyAlgorithms.PSHA1
                  || algorithm == ComputedKeyAlgorithms.Psha1);
        }

        /// <summary>
        /// Computes a SecurityToken representing the computed proof key which combines
        /// requestor and issuer entropies using the PSHA1 algorithm.
        /// </summary>
        internal static SecurityToken ComputeProofKey(System.IdentityModel.Protocols.WSTrust.RequestSecurityToken request,
                                                       System.IdentityModel.Protocols.WSTrust.RequestSecurityTokenResponse response)
        {
            if (response.Entropy == null)
            {
                throw IM.DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new NotSupportedException(SR.GetString(SR.ID3193)));
            }

            if (request.Entropy == null)
            {
                throw IM.DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new NotSupportedException(SR.GetString(SR.ID3194)));
            }

            //
            // We need a key size. Use the requestor's keysize unless the issuer overrides it
            //
            int keySize = request.KeySizeInBits ?? WSTrustChannel.DefaultKeySizeInBits;
            if (response.KeySizeInBits.HasValue)
            {
                keySize = response.KeySizeInBits.Value;
            }

            byte[] keyMaterial = System.IdentityModel.CryptoHelper.KeyGenerator.ComputeCombinedKey(request.Entropy.GetKeyBytes(),
                                                                  response.Entropy.GetKeyBytes(),
                                                                  keySize);

            return new BinarySecretSecurityToken(keyMaterial);
        }

        //
        //  Response               | Request                | Proof Key
        //  =======================#========================#============================================
        //   Contains a proof key  | Ignored                | Use the response's issued proof key
        //  -----------------------+------------------------+-------------------------------------------- 
        //   Contains Entropy      | Contains Entropy       | Compute a proof key using the specified
        //   and MUST specify      |                        | computation algorithm
        //   computation algorithm |                        |
        //  -----------------------+------------------------+--------------------------------------------
        //   No proof key          | Contains Entropy       | Use request's entropy as proof key
        //  -----------------------+------------------------+--------------------------------------------
        //   No proof key          | No entropy             | No proof key is used
        //  -----------------------+------------------------+--------------------------------------------
        //   No proof key          | Contains UseKey        | Use UseKey as proof key
        //  -----------------------+------------------------+--------------------------------------------
        //
        internal static SecurityToken GetProofKey(System.IdentityModel.Protocols.WSTrust.RequestSecurityToken request, System.IdentityModel.Protocols.WSTrust.RequestSecurityTokenResponse response)
        {
            //
            // The following attempts to get an issued proof key or compute a proof key in accordance
            // with WS-Trust 1.3 section 4.4.3
            //
            if (response.RequestedProofToken != null)
            {
                //
                // specific key
                // -------------
                //   When the issuer provides a key it must be used as the proof key. This key is contained
                //   in the RequestedProofToken element of the RSTR.
                //
                if (response.RequestedProofToken.ProtectedKey != null)
                {
                    return new BinarySecretSecurityToken(response.RequestedProofToken.ProtectedKey.GetKeyBytes());
                }
                //
                // partial
                // ------------
                //   When the issuer does not provide a key but specifies a key computation algorithm in the
                //   RequestedProofToken element, then the requestor needs to compute the proof key using
                //   both entropies.
                //
                else if (IsPsha1(response.RequestedProofToken.ComputedKeyAlgorithm))
                {
                    return ComputeProofKey(request, response);
                }
                else
                {
                    //
                    // If there is a RequestedProofToken there must either be a
                    // ProtectedKey or a ComputedKeyAlgorithm!
                    //
                    throw IM.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(SR.GetString(SR.ID3192, response.RequestedProofToken.ComputedKeyAlgorithm)));
                }
            }
            else
            {
                //
                // ommitted
                //
                // " In the case of omitted, an existing key is used or the resulting token 
                //   is not directly associated with a key. "
                //                
                ProofKeyType requestKeyType = GetKeyType(request.KeyType);
                switch (requestKeyType)
                {
                    case ProofKeyType.Asymmetric:
                        return GetUseKeySecurityToken(request.UseKey, request.KeyType);

                    case ProofKeyType.Symmetric:
                        if (response.Entropy != null)
                        {
                            //
                            // If there is response.Entropy then there must
                            // also be an RSTR.RequestedProofToken containing a
                            // ComputedKey element.
                            //
                            throw IM.DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                                new NotSupportedException(SR.GetString(SR.ID3191)));
                        }

                        if (request.Entropy != null)
                        {
                            return new BinarySecretSecurityToken(request.Entropy.GetKeyBytes());
                        }
                        else
                        {
                            return null;
                        }

                    case ProofKeyType.Bearer:
                        return null;

                    default:
                        throw IM.DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                            new NotSupportedException(SR.GetString(SR.ID3139, request.KeyType)));
                }
            }
        }

        #region IChannel Members

        /// <summary>
        /// Returns a typed object requested, if present, from the appropriate layer in the channel stack.
        /// </summary>        
        /// <typeparam name="T">The typed object for which the method is querying.</typeparam>
        /// <returns>The typed object <typeparamref name="T"/> requested if it is present or nullNothingnullptra null reference (Nothing in Visual Basic) if it is not.</returns>
        public T GetProperty<T>() where T : class
        {
            return Channel.GetProperty<T>();
        }

        #endregion

        #region ICommunicationObject Members

        /// <summary>
        /// Causes a communication object to transition immediately from its current state into the closed state. 
        /// </summary>
        public void Abort()
        {
            Channel.Abort();
        }

        /// <summary>
        /// Begins an asynchronous operation to close a communication object with a specified timeout.
        /// </summary>
        /// <param name="timeout">
        /// The <see cref="TimeSpan" /> that specifies how long the close operation has to complete before timing out.
        /// </param>
        /// <param name="callback">
        /// The <see cref="AsyncCallback" /> delegate that receives notification of the completion of the asynchronous 
        /// close operation.
        /// </param>
        /// <param name="state">
        /// An object, specified by the application, that contains state information associated with the asynchronous 
        /// close operation.
        /// </param>
        /// <returns>The <see cref="IAsyncResult" /> that references the asynchronous close operation.</returns>
        public IAsyncResult BeginClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return Channel.BeginClose(timeout, callback, state);
        }

        /// <summary>
        /// Begins an asynchronous operation to close a communication object.
        /// </summary>
        /// <param name="callback">
        /// The <see cref="AsyncCallback" /> delegate that receives notification of the completion of the asynchronous 
        /// close operation.
        /// </param>
        /// <param name="state">
        /// An object, specified by the application, that contains state information associated with the asynchronous 
        /// close operation.
        /// </param>
        /// <returns>The <see cref="IAsyncResult" /> that references the asynchronous close operation.</returns>
        public IAsyncResult BeginClose(AsyncCallback callback, object state)
        {
            return Channel.BeginClose(callback, state);
        }

        /// <summary>
        /// Begins an asynchronous operation to open a communication object within a specified interval of time.
        /// </summary>
        /// <param name="timeout">
        /// The <see cref="TimeSpan" /> that specifies how long the open operation has to complete before timing out.
        /// </param>
        /// <param name="callback">
        /// The <see cref="AsyncCallback" /> delegate that receives notification of the completion of the asynchronous 
        /// close operation.
        /// </param>
        /// <param name="state">
        /// An object, specified by the application, that contains state information associated with the asynchronous 
        /// close operation.
        /// </param>
        /// <returns>The <see cref="IAsyncResult" /> that references the asynchronous open operation.</returns>
        public IAsyncResult BeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return Channel.BeginOpen(timeout, callback, state);
        }

        /// <summary>
        /// Begins an asynchronous operation to open a communication object.
        /// </summary>
        /// <param name="callback">
        /// The <see cref="AsyncCallback" /> delegate that receives notification of the completion of the asynchronous 
        /// close operation.
        /// </param>
        /// <param name="state">
        /// An object, specified by the application, that contains state information associated with the asynchronous 
        /// close operation.
        /// </param>
        /// <returns>The <see cref="IAsyncResult" /> that references the asynchronous open operation.</returns>
        public IAsyncResult BeginOpen(AsyncCallback callback, object state)
        {
            return Channel.BeginOpen(callback, state);
        }

        /// <summary>
        /// Causes a communication object to transition from its current state into the closed state.
        /// </summary>
        /// <param name="timeout">
        /// The <see cref="TimeSpan" /> that specifies how long the open operation has to complete before timing out.
        /// </param>
        public void Close(TimeSpan timeout)
        {
            Channel.Close(timeout);
        }

        /// <summary>
        /// Causes a communication object to transition from its current state into the closed state.
        /// </summary>
        public void Close()
        {
            Channel.Close();
        }

        /// <summary>
        /// Occurs when the communication object completes its transition from the closing state into the closed state.
        /// </summary>
        public event EventHandler Closed
        {
            add
            {
                Channel.Closed += value;
            }
            remove
            {
                Channel.Closed -= value;
            }
        }

        /// <summary>
        /// Occurs when the communication object first enters the closing state.
        /// </summary>
        public event EventHandler Closing
        {
            add
            {
                Channel.Closing += value;
            }
            remove
            {
                Channel.Closing -= value;
            }
        }

        /// <summary>
        /// Completes an asynchronous operation to close a communication object.
        /// </summary>
        /// <param name="result">The <see cref="IAsyncResult" /> that is returned by a call to the BeginClose() method.</param>
        public void EndClose(IAsyncResult result)
        {
            Channel.EndClose(result);
        }

        /// <summary>
        /// Completes an asynchronous operation to open a communication object.
        /// </summary>
        /// <param name="result">The <see cref="IAsyncResult" /> that is returned by a call to the BeginClose() method.</param>
        public void EndOpen(IAsyncResult result)
        {
            Channel.EndOpen(result);
        }

        /// <summary>
        /// Occurs when the communication object first enters the faulted state.
        /// </summary>
        public event EventHandler Faulted
        {
            add
            {
                Channel.Faulted += value;
            }
            remove
            {
                Channel.Faulted -= value;
            }
        }

        /// <summary>
        /// Causes a communication object to transition from the created state into the opened state within a specified interval of time.
        /// </summary>
        /// <param name="timeout">
        /// The <see cref="TimeSpan" /> that specifies how long the open operation has to complete before timing out.
        /// </param>
        public void Open(TimeSpan timeout)
        {
            Channel.Open(timeout);
        }

        /// <summary>
        /// Causes a communication object to transition from the created state into the opened state. 
        /// </summary>
        public void Open()
        {
            Channel.Open();
        }

        /// <summary>
        /// Occurs when the communication object completes its transition from the opening state into the opened state.
        /// </summary>
        public event EventHandler Opened
        {
            add
            {
                Channel.Opened += value;
            }
            remove
            {
                Channel.Opened -= value;
            }
        }

        /// <summary>
        /// Occurs when the communication object first enters the opening state.
        /// </summary>
        public event EventHandler Opening
        {
            add
            {
                Channel.Opening += value;
            }
            remove
            {
                Channel.Opening -= value;
            }
        }

        /// <summary>
        /// Gets the current state of the communication-oriented object.
        /// </summary>
        public System.ServiceModel.CommunicationState State
        {
            get { return Channel.State; }
        }

        #endregion

        #region IWSTrustChannelContract Members

        /// <summary>
        /// Sends a WS-Trust Cancel message to an endpoint.
        /// </summary>
        /// <param name="rst">The <see cref="System.IdentityModel.Protocols.WSTrust.RequestSecurityToken" /> that represents the request to the STS.</param>        
        /// <returns>The <see cref="System.IdentityModel.Protocols.WSTrust.RequestSecurityTokenResponse" /> representing the STS response.</returns>
        public virtual System.IdentityModel.Protocols.WSTrust.RequestSecurityTokenResponse Cancel(System.IdentityModel.Protocols.WSTrust.RequestSecurityToken rst)
        {
            return ReadResponse(this.Contract.Cancel(CreateRequest(rst, RequestTypes.Cancel)));
        }

        /// <summary>
        /// Sends a WS-Trust Issue message to an endpoint STS
        /// </summary>
        /// <param name="rst">The <see cref="System.IdentityModel.Protocols.WSTrust.RequestSecurityToken" /> that represents the request to the STS.</param>
        /// <returns>A <see cref="SecurityToken" /> that represents the token issued by the STS.</returns>
        public virtual SecurityToken Issue(System.IdentityModel.Protocols.WSTrust.RequestSecurityToken rst)
        {
            System.IdentityModel.Protocols.WSTrust.RequestSecurityTokenResponse rstr = null;
            return this.Issue(rst, out rstr);
        }

        /// <summary>
        /// Sends a WS-Trust Issue message to an endpoint STS
        /// </summary>
        /// <param name="rst">The <see cref="System.IdentityModel.Protocols.WSTrust.RequestSecurityToken" /> that represents the request to the STS.</param>
        /// <param name="rstr">The <see cref="System.IdentityModel.Protocols.WSTrust.RequestSecurityTokenResponse" /> that represents the response from 
        /// the STS.</param>
        /// <returns>A <see cref="SecurityToken" /> that represents the token issued by the STS.</returns>
        public virtual SecurityToken Issue(System.IdentityModel.Protocols.WSTrust.RequestSecurityToken rst, out System.IdentityModel.Protocols.WSTrust.RequestSecurityTokenResponse rstr)
        {
            Message request = CreateRequest(rst, RequestTypes.Issue);

            Message response = Contract.Issue(request);
            rstr = ReadResponse(response);

            return GetTokenFromResponse(rst, rstr);
        }

        /// <summary>
        /// Sends a WS-Trust Renew message to an endpoint.
        /// </summary>
        /// <param name="rst">The <see cref="System.IdentityModel.Protocols.WSTrust.RequestSecurityToken" /> that represents the request to the STS.</param>        
        /// <returns>The <see cref="System.IdentityModel.Protocols.WSTrust.RequestSecurityTokenResponse" /> representing the STS response.</returns>
        public virtual System.IdentityModel.Protocols.WSTrust.RequestSecurityTokenResponse Renew(System.IdentityModel.Protocols.WSTrust.RequestSecurityToken rst)
        {
            return ReadResponse(this.Contract.Renew(CreateRequest(rst, RequestTypes.Renew)));
        }

        /// <summary>
        /// Sends a WS-Trust Validate message to an endpoint.
        /// </summary>
        /// <param name="rst">The <see cref="System.IdentityModel.Protocols.WSTrust.RequestSecurityToken" /> that represents the request to the STS.</param>        
        /// <returns>The <see cref="System.IdentityModel.Protocols.WSTrust.RequestSecurityTokenResponse" /> representing the STS response.</returns>
        public virtual System.IdentityModel.Protocols.WSTrust.RequestSecurityTokenResponse Validate(System.IdentityModel.Protocols.WSTrust.RequestSecurityToken rst)
        {
            return ReadResponse(this.Contract.Validate(CreateRequest(rst, RequestTypes.Validate)));
        }

        #endregion

        IAsyncResult BeginOperation(WSTrustChannel.WSTrustChannelAsyncResult.Operations operation,
                                     string requestType,
                                     System.IdentityModel.Protocols.WSTrust.RequestSecurityToken rst,
                                     AsyncCallback callback,
                                     object state)
        {
            if (rst == null)
            {
                throw IM.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("rst");
            }

            Message request = this.CreateRequest(rst, requestType);

            WSTrustSerializationContext context = this.WSTrustSerializationContext;
            return new WSTrustChannelAsyncResult(this, operation, rst, context, request, callback, state);
        }

        System.IdentityModel.Protocols.WSTrust.RequestSecurityTokenResponse EndOperation(IAsyncResult result, out WSTrustChannelAsyncResult tcar)
        {
            if (result == null)
            {
                throw IM.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("result");
            }

            tcar = result as WSTrustChannelAsyncResult;
            if (tcar == null)
            {
                throw IM.DiagnosticUtility.ThrowHelperInvalidOperation(
                    SR.GetString(SR.ID2004, typeof(WSTrustChannelAsyncResult), result.GetType()));
            }

            Message response = WSTrustChannelAsyncResult.End(result);
            return ReadResponse(response);
        }

        #region IWSTrustChannelContract Async Members

        /// <summary>
        /// Asynchronously sends a WS-Trust Cancel message to an endpoint.
        /// </summary>
        /// <param name="rst">The <see cref="System.IdentityModel.Protocols.WSTrust.RequestSecurityToken" /> that represents the request to the STS.</param>        
        /// <param name="callback">An optional asynchronous callback, to be called when the send is complete.</param>
        /// <param name="state">A user-provided object that distinguishes this particular asynchronous send 
        /// request from other requests.</param>
        /// <returns>An <see cref="IAsyncResult" /> object that represents the asynchronous send, which could still 
        /// be pending. </returns>
        public IAsyncResult BeginCancel(System.IdentityModel.Protocols.WSTrust.RequestSecurityToken rst, AsyncCallback callback, object state)
        {
            return BeginOperation(WSTrustChannelAsyncResult.Operations.Cancel, RequestTypes.Cancel, rst, callback, state);
        }

        /// <summary>
        /// Completes the asynchronous send operation initiated by
        /// <see cref="BeginCancel(System.IdentityModel.Protocols.WSTrust.RequestSecurityToken,AsyncCallback,object)" />.
        /// </summary>
        /// <param name="result">A reference to the outstanding asynchronous send request.</param>
        /// <param name="rstr">The <see cref="System.IdentityModel.Protocols.WSTrust.RequestSecurityTokenResponse" /> representing the STS response.</param>
        public void EndCancel(IAsyncResult result, out System.IdentityModel.Protocols.WSTrust.RequestSecurityTokenResponse rstr)
        {
            WSTrustChannelAsyncResult tcar;
            rstr = EndOperation(result, out tcar);
        }

        /// <summary>
        /// Asynchronously sends a WS-Trust Renew message to an endpoint.
        /// </summary>
        /// <param name="rst">The <see cref="System.IdentityModel.Protocols.WSTrust.RequestSecurityToken" /> that represents the request to the STS.</param>        
        /// <param name="callback">An optional asynchronous callback, to be called when the send is complete.</param>
        /// <param name="asyncState">A user-provided object that distinguishes this particular asynchronous send 
        /// request from other requests.</param>
        /// <returns>An <see cref="IAsyncResult" /> object that represents the asynchronous send, which could still 
        /// be pending. </returns>
        public IAsyncResult BeginIssue(System.IdentityModel.Protocols.WSTrust.RequestSecurityToken rst, AsyncCallback callback, object asyncState)
        {
            return BeginOperation(WSTrustChannelAsyncResult.Operations.Issue, RequestTypes.Issue, rst, callback, asyncState);
        }

        /// <summary>
        /// Completes the asynchronous send operation initiated by
        /// <see cref="BeginIssue(System.IdentityModel.Protocols.WSTrust.RequestSecurityToken,AsyncCallback,object)" />.
        /// </summary>
        /// <param name="result">A reference to the outstanding asynchronous send request.</param>
        /// <param name="rstr">The <see cref="System.IdentityModel.Protocols.WSTrust.RequestSecurityTokenResponse" /> representing the STS response.</param>
        /// <returns>A <see cref="SecurityToken" /> that represents the token issued by the STS.</returns>
        public SecurityToken EndIssue(IAsyncResult result, out System.IdentityModel.Protocols.WSTrust.RequestSecurityTokenResponse rstr)
        {
            WSTrustChannelAsyncResult tcar;
            rstr = EndOperation(result, out tcar);

            return GetTokenFromResponse(tcar.RequestSecurityToken, rstr);
        }

        /// <summary>
        /// Asynchronously sends a WS-Trust Renew message to an endpoint.
        /// </summary>
        /// <param name="rst">The <see cref="System.IdentityModel.Protocols.WSTrust.RequestSecurityToken" /> that represents the request to the STS.</param>        
        /// <param name="callback">An optional asynchronous callback, to be called when the send is complete.</param>
        /// <param name="state">A user-provided object that distinguishes this particular asynchronous send 
        /// request from other requests.</param>
        /// <returns>An <see cref="IAsyncResult" /> object that represents the asynchronous send, which could still 
        /// be pending. </returns>
        public IAsyncResult BeginRenew(System.IdentityModel.Protocols.WSTrust.RequestSecurityToken rst, AsyncCallback callback, object state)
        {
            return BeginOperation(WSTrustChannelAsyncResult.Operations.Renew, RequestTypes.Renew, rst, callback, state);
        }

        /// <summary>
        /// Completes the asynchronous send operation initiated by
        /// <see cref="BeginRenew(System.IdentityModel.Protocols.WSTrust.RequestSecurityToken,AsyncCallback,object)" />.
        /// </summary>
        /// <param name="result">A reference to the outstanding asynchronous send request.</param>
        /// <param name="rstr">The <see cref="System.IdentityModel.Protocols.WSTrust.RequestSecurityTokenResponse" /> representing the STS response.</param>
        public void EndRenew(IAsyncResult result, out System.IdentityModel.Protocols.WSTrust.RequestSecurityTokenResponse rstr)
        {
            WSTrustChannelAsyncResult tcar;
            rstr = EndOperation(result, out tcar);
        }

        /// <summary>
        /// Asynchronously sends a WS-Trust Validate message to an endpoint.
        /// </summary>
        /// <param name="rst">The <see cref="System.IdentityModel.Protocols.WSTrust.RequestSecurityToken" /> that represents the request to the STS.</param>        
        /// <param name="callback">An optional asynchronous callback, to be called when the send is complete.</param>
        /// <param name="state">A user-provided object that distinguishes this particular asynchronous send 
        /// request from other requests.</param>
        /// <returns>An <see cref="IAsyncResult" /> object that represents the asynchronous send, which could still 
        /// be pending. </returns>
        public IAsyncResult BeginValidate(System.IdentityModel.Protocols.WSTrust.RequestSecurityToken rst, AsyncCallback callback, object state)
        {
            return BeginOperation(WSTrustChannelAsyncResult.Operations.Validate, RequestTypes.Validate, rst, callback, state);
        }

        /// <summary>
        /// Completes the asynchronous send operation initiated by
        /// <see cref="BeginValidate(System.IdentityModel.Protocols.WSTrust.RequestSecurityToken,AsyncCallback,object)" />.
        /// </summary>
        /// <param name="result">A reference to the outstanding asynchronous send request.</param>
        /// <param name="rstr">The <see cref="System.IdentityModel.Protocols.WSTrust.RequestSecurityTokenResponse" /> representing the STS response.</param>
        public void EndValidate(IAsyncResult result, out System.IdentityModel.Protocols.WSTrust.RequestSecurityTokenResponse rstr)
        {
            WSTrustChannelAsyncResult tcar;
            rstr = EndOperation(result, out tcar);
        }

        #endregion

        #region IWSTrustContract Members

        /// <summary>
        /// Sends a WS-Trust Cancel message to an endpoint.
        /// </summary>
        /// <param name="message">The <see cref="Message" /> that contains the instructions for the request to the STS.</param>
        /// <returns>The <see cref="Message" /> returned from the STS.</returns>
        public Message Cancel(Message message)
        {
            return Contract.Cancel(message);
        }

        /// <summary>
        /// Begins an asynchronous operation to send a WS-Trust Cancel message to an endpoint.
        /// </summary>
        /// <param name="message">The <see cref="Message" /> that contains the instructions for the request to the STS.</param>
        /// <param name="callback">
        /// The <see cref="AsyncCallback" /> delegate that receives notification of the completion of the asynchronous 
        /// close operation.
        /// </param>
        /// <param name="asyncState">
        /// An object, specified by the application, that contains state information associated with the asynchronous 
        /// close operation.
        /// </param>
        /// <returns>The <see cref="IAsyncResult" /> that references the asynchronous close operation.</returns>
        public IAsyncResult BeginCancel(Message message, AsyncCallback callback, object asyncState)
        {
            return Contract.BeginCancel(message, callback, asyncState);
        }

        /// <summary>
        /// Completes an asynchronous operation to send a WS-Trust Cancel message to an endpoint.
        /// </summary>
        /// <param name="asyncResult">The <see cref="IAsyncResult" /> that is returned by a call to the BeginClose() method.</param>
        /// <returns>The <see cref="Message" /> returned from the STS.</returns>
        public Message EndCancel(IAsyncResult asyncResult)
        {
            return Contract.EndCancel(asyncResult);
        }

        /// <summary>
        /// Sends a WS-Trust Issue message to an endpoint.
        /// </summary>
        /// <param name="message">The <see cref="Message" /> that contains the instructions for the request to the STS</param>
        /// <returns>The <see cref="Message" /> returned from the STS</returns>
        public Message Issue(Message message)
        {
            return Contract.Issue(message);
        }

        /// <summary>
        /// Begins an asynchronous operation to send a WS-Trust Issue message to an endpoint.
        /// </summary>
        /// <param name="message">The <see cref="Message" /> that contains the instructions for the request to the STS.</param>
        /// <param name="callback">
        /// The <see cref="AsyncCallback" /> delegate that receives notification of the completion of the asynchronous 
        /// issue operation.
        /// </param>
        /// <param name="asyncState">
        /// An object, specified by the application, that contains state information associated with the asynchronous 
        /// issue operation.
        /// </param>
        /// <returns>The <see cref="IAsyncResult" /> that references the asynchronous issue operation.</returns>
        public IAsyncResult BeginIssue(Message message, AsyncCallback callback, object asyncState)
        {
            return Contract.BeginIssue(message, callback, asyncState);
        }

        /// <summary>
        /// Completes an asynchronous operation to send a WS-Trust Issue message to an endpoint.
        /// </summary>
        /// <param name="asyncResult">The <see cref="IAsyncResult" /> that is returned by a call to the BeginIssue() method.</param>
        /// <returns>The <see cref="Message" /> returned from the STS.</returns>
        public Message EndIssue(IAsyncResult asyncResult)
        {
            return Contract.EndIssue(asyncResult);
        }

        /// <summary>
        /// Sends a WS-Trust Renew message to an endpoint.
        /// </summary>
        /// <param name="message">The <see cref="Message" /> that contains the instructions for the request to the STS</param>
        /// <returns>The <see cref="Message" /> returned from the STS</returns>
        public Message Renew(Message message)
        {
            return Contract.Renew(message);
        }

        /// <summary>
        /// Begins an asynchronous operation to send a WS-Trust Renew message to an endpoint.
        /// </summary>
        /// <param name="message">The <see cref="Message" /> that contains the instructions for the request to the STS.</param>
        /// <param name="callback">
        /// The <see cref="AsyncCallback" /> delegate that receives notification of the completion of the asynchronous 
        /// renew operation.
        /// </param>
        /// <param name="asyncState">
        /// An object, specified by the application, that contains state information associated with the asynchronous 
        /// renew operation.
        /// </param>
        /// <returns>The <see cref="IAsyncResult" /> that references the asynchronous renew operation.</returns>
        public IAsyncResult BeginRenew(Message message, AsyncCallback callback, object asyncState)
        {
            return Contract.BeginRenew(message, callback, asyncState);
        }

        /// <summary>
        /// Completes an asynchronous operation to send a WS-Trust Renew message to an endpoint.
        /// </summary>
        /// <param name="asyncResult">The <see cref="IAsyncResult" /> that is returned by a call to the BeginRenew() method.</param>
        /// <returns>The <see cref="Message" /> returned from the STS.</returns>
        public Message EndRenew(IAsyncResult asyncResult)
        {
            return Contract.EndRenew(asyncResult);
        }

        /// <summary>
        /// Sends a WS-Trust Validate message to an endpoint.
        /// </summary>
        /// <param name="message">The <see cref="Message" /> that contains the instructions for the request to the STS</param>
        /// <returns>The <see cref="Message" /> returned from the STS</returns>
        public Message Validate(Message message)
        {
            return Contract.Validate(message);
        }

        /// <summary>
        /// Begins an asynchronous operation to send a WS-Trust Validate message to an endpoint.
        /// </summary>
        /// <param name="message">The <see cref="Message" /> that contains the instructions for the request to the STS.</param>
        /// <param name="callback">
        /// The <see cref="AsyncCallback" /> delegate that receives notification of the completion of the asynchronous 
        /// validate operation.
        /// </param>
        /// <param name="asyncState">
        /// An object, specified by the application, that contains state information associated with the asynchronous 
        /// validate operation.
        /// </param>
        /// <returns>The <see cref="IAsyncResult" /> that references the asynchronous validate operation.</returns>
        public IAsyncResult BeginValidate(Message message, AsyncCallback callback, object asyncState)
        {
            return Contract.BeginValidate(message, callback, asyncState);
        }

        /// <summary>
        /// Completes an asynchronous operation to send a WS-Trust Validate message to an endpoint.
        /// </summary>
        /// <param name="asyncResult">The <see cref="IAsyncResult" /> that is returned by a call to the BeginValidate() method.</param>
        /// <returns>The <see cref="Message" /> returned from the STS.</returns>
        public Message EndValidate(IAsyncResult asyncResult)
        {
            return Contract.EndValidate(asyncResult);
        }

        #endregion
    }
}
