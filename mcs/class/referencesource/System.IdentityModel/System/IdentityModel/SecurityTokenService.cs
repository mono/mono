//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.IdentityModel
{
    using System;
    using System.IdentityModel.Tokens;
    using System.Security.Claims;
    using RST = System.IdentityModel.Protocols.WSTrust.RequestSecurityToken;
    using RSTR = System.IdentityModel.Protocols.WSTrust.RequestSecurityTokenResponse;
    using System.IdentityModel.Protocols.WSTrust;
    using System.IdentityModel.Configuration;

    /// <summary>
    /// Abstract class for building WS-Security token services.
    /// </summary>
    public abstract class SecurityTokenService
    {
        /// <summary>
        /// This class is used to maintain request state across asynchronous calls
        /// within the security token service.
        /// </summary>
        protected class FederatedAsyncState
        {
            RST _request;
            ClaimsPrincipal _claimsPrincipal;
            SecurityTokenHandler _securityTokenHandler;
            IAsyncResult _result;

            /// <summary>
            /// Copy constructor.
            /// </summary>
            /// <param name="federatedAsyncState">The input FederatedAsyncState instance.</param>
            /// <exception cref="ArgumentNullException">The input 'FederatedAsyncState' is null.</exception>
            public FederatedAsyncState(FederatedAsyncState federatedAsyncState)
            {
                if (null == federatedAsyncState)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("FederatedAsyncState");
                }

                _request = federatedAsyncState.Request;
                _claimsPrincipal = federatedAsyncState.ClaimsPrincipal;
                _securityTokenHandler = federatedAsyncState.SecurityTokenHandler;
                _result = federatedAsyncState.Result;
            }

            /// <summary>
            /// Constructs a FederatedAsyncState instance with token request, principal, and the async result.
            /// </summary>
            /// <param name="request">The token request instance.</param>
            /// <param name="principal">The identity of the token requestor.</param>
            /// <param name="result">The async result.</param>
            /// <exception cref="ArgumentNullException">When the given request or async result is null.</exception>
            public FederatedAsyncState(RST request, ClaimsPrincipal principal, IAsyncResult result)
            {
                if (null == request)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("request");
                }

                if (null == result)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("result");
                }

                _request = request;
                _claimsPrincipal = principal;
                _result = result;
            }

            /// <summary>
            /// Gets the token request instance.
            /// </summary>
            public RST Request
            {
                get
                {
                    return _request;
                }
            }

            /// <summary>
            /// Gets the ClaimsPrincipal instance.
            /// </summary>
            public ClaimsPrincipal ClaimsPrincipal
            {
                get
                {
                    return _claimsPrincipal;
                }
            }

            /// <summary>
            /// Gets or sets the SecurityTokenHandler to be used during an async token-issuance call.
            /// </summary>
            public SecurityTokenHandler SecurityTokenHandler
            {
                get { return _securityTokenHandler; }
                set { _securityTokenHandler = value; }
            }

            /// <summary>
            /// Gets the async result.
            /// </summary>
            public IAsyncResult Result
            {
                get
                {
                    return _result;
                }
            }
        }

        //
        // STS settings
        //
        SecurityTokenServiceConfiguration _securityTokenServiceConfiguration;

        ClaimsPrincipal _principal;
        RequestSecurityToken _request;
        SecurityTokenDescriptor _tokenDescriptor;

        /// <summary>
        /// Use this constructor to initialize scope provider and token issuer certificate.
        /// </summary>
        /// <param name="securityTokenServiceConfiguration">The SecurityTokenServiceConfiguration that will have the related settings for the STS.</param>
        protected SecurityTokenService(SecurityTokenServiceConfiguration securityTokenServiceConfiguration)
        {
            if (securityTokenServiceConfiguration == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("securityTokenServiceConfiguration");
            }

            _securityTokenServiceConfiguration = securityTokenServiceConfiguration;
        }

        /// <summary>
        /// Async Cancel.
        /// </summary>
        /// <param name="principal">The identity of the token requestor.</param>
        /// <param name="request">The security token request which includes request message as well as other client 
        /// related information such as authorization context.</param>
        /// <param name="callback">The async call back.</param>
        /// <param name="state">The state object.</param>
        /// <returns>The async result.</returns>
        public virtual IAsyncResult BeginCancel(ClaimsPrincipal principal, RST request, AsyncCallback callback, object state)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidRequestException(SR.GetString(SR.ID3141, (request != null ? request.RequestType : "Cancel"))));
        }

        /// <summary>
        /// Begins async call for GetScope routine. Default implementation will throw a NotImplementedExcetion.
        /// Refer MSDN articles on Using an AsyncCallback Delegate to End an Asynchronous Operation.
        /// </summary>
        /// <param name="principal">The identity of the token requestor.</param>
        /// <param name="request">The request.</param>
        /// <param name="callback">The callback to be invoked when the user Asynchronous operation completed.</param>
        /// <param name="state">The state object.</param>
        /// <returns>IAsyncResult. Represents the status of an asynchronous operation. This will be passed into 
        /// EndGetScope.</returns>
        protected virtual IAsyncResult BeginGetScope(ClaimsPrincipal principal, RST request, AsyncCallback callback, object state)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotImplementedException(SR.GetString(SR.ID2081)));
        }

        /// <summary>
        /// Begins the async call of the Issue request.
        /// </summary>
        /// <param name="principal">The <see cref="ClaimsPrincipal"/> to issue a token for.</param>
        /// <param name="request">The security token request which includes request message as well as other client 
        /// related information such as authorization context.</param>
        /// <param name="callback">The async call back.</param>
        /// <param name="state">The state object.</param>
        /// <returns>The async result.</returns>
        public virtual IAsyncResult BeginIssue(ClaimsPrincipal principal, RST request, AsyncCallback callback, object state)
        {
            if (request == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("request");
            }
            _principal = principal;
            _request = request;

            //
            // Step 1: Validate the rst: check if this STS is capable of handling this
            // rst
            //
            ValidateRequest(request);

            //
            // 



            FederatedAsyncState asyncState = new FederatedAsyncState(request, principal, new TypedAsyncResult<RSTR>(callback, state));

            BeginGetScope(principal, request, OnGetScopeComplete, asyncState);

            return asyncState.Result;
        }

        /// <summary>
        /// Async Renew.
        /// </summary>
        /// <param name="principal">The <see cref="ClaimsPrincipal"/> to renew.</param>
        /// <param name="request">The security token request which includes request message as well as other client 
        /// related information such as authorization context.</param>
        /// <param name="callback">The async call back.</param>
        /// <param name="state">The state object.</param>
        /// <returns>The async result.</returns>
        public virtual IAsyncResult BeginRenew(ClaimsPrincipal principal, RST request, AsyncCallback callback, object state)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidRequestException(SR.GetString(SR.ID3141, (request != null && request.RequestType != null ? request.RequestType : "Renew"))));
        }

        /// <summary>
        /// Async Validate.
        /// </summary>
        /// <param name="principal">The <see cref="ClaimsPrincipal"/> to validate.</param>
        /// <param name="request">The security token request which includes request message as well as other client 
        /// related information such as authorization context.</param>
        /// <param name="callback">The async call back.</param>
        /// <param name="state">The state object.</param>
        /// <returns>The async result.</returns>
        public virtual IAsyncResult BeginValidate(ClaimsPrincipal principal, RST request, AsyncCallback callback, object state)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidRequestException(SR.GetString(SR.ID3141, (request != null && request.RequestType != null ? request.RequestType : "Validate"))));
        }

        /// <summary>
        /// Cancel.
        /// </summary>
        /// <param name="principal">The <see cref="ClaimsPrincipal"/> to cancel.</param>
        /// <param name="request">The request.</param>
        /// <returns>The response.</returns>
        public virtual RSTR Cancel(ClaimsPrincipal principal, RST request)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidRequestException(SR.GetString(SR.ID3141, (request != null && request.RequestType != null ? request.RequestType : "Cancel"))));
        }

        /// <summary>
        /// Creates an instance of a <see cref="SecurityTokenDescriptor"/>.
        /// </summary>
        /// <param name="request">The incoming token request.</param>
        /// <param name="scope">The <see cref="Scope"/> object returned from <see cref="SecurityTokenService.GetScope"/>.</param>
        /// <returns>The <see cref="SecurityTokenDescriptor"/>.</returns>
        /// <remarks>Invoked during token issuance after <see cref="SecurityTokenService.GetScope"/>.</remarks>
        protected virtual SecurityTokenDescriptor CreateSecurityTokenDescriptor(RST request, Scope scope)
        {
            if (request == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("request");
            }

            if (scope == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("scope");
            }

            SecurityTokenDescriptor d = new SecurityTokenDescriptor();
            d.AppliesToAddress = scope.AppliesToAddress;
            d.ReplyToAddress = scope.ReplyToAddress;
            d.SigningCredentials = scope.SigningCredentials;
            if (null == d.SigningCredentials)
            {
                d.SigningCredentials = this.SecurityTokenServiceConfiguration.SigningCredentials;
            }



            //
            // The encrypting credentials specified on the Scope object
            // are invariant relative to a specific RP. Allowing the STS to 
            // cache the Scope for each RP.
            // Our default implementation will generate the symmetric bulk 
            // encryption key on the fly.
            //
            if (scope.EncryptingCredentials != null &&
                 scope.EncryptingCredentials.SecurityKey is AsymmetricSecurityKey
                 )
            {
                if ((request.EncryptionAlgorithm == null || request.EncryptionAlgorithm == SecurityAlgorithms.Aes256Encryption) &&
                    (request.SecondaryParameters == null || request.SecondaryParameters.EncryptionAlgorithm == null || request.SecondaryParameters.EncryptionAlgorithm == SecurityAlgorithms.Aes256Encryption)
                    )
                {
                    d.EncryptingCredentials = new EncryptedKeyEncryptingCredentials(scope.EncryptingCredentials, 256, SecurityAlgorithms.Aes256Encryption);
                }
            }

            return d;
        }

        /// <summary>
        /// Gets the STS's name.
        /// </summary>
        /// <returns>Returns the issuer name.</returns>
        protected virtual string GetIssuerName()
        {
            return SecurityTokenServiceConfiguration.TokenIssuerName;
        }

        /// <summary>
        /// Checks the IssuerName for validity (non-null)
        /// </summary>
        private string GetValidIssuerName()
        {
            string issuerName = GetIssuerName();

            if (string.IsNullOrEmpty(issuerName))
            {
                throw DiagnosticUtility.ThrowHelperInvalidOperation(SR.GetString(SR.ID2083));
            }

            return issuerName;
        }

        /// <summary>
        /// Gets the proof token.
        /// </summary>
        /// <param name="request">The incoming token request.</param>
        /// <param name="scope">The scope instance encapsulating information about the relying party.</param>
        /// <returns>The newly created proof decriptor that could be either asymmetric proof descriptor or symmetric proof descriptor or null in the bearer token case.</returns>
        protected virtual ProofDescriptor GetProofToken(RST request, Scope scope)
        {
            if (request == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("request");
            }

            if (scope == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("scope");
            }

            EncryptingCredentials requestorWrappingCredentials = GetRequestorProofEncryptingCredentials(request);

            if (scope.EncryptingCredentials != null &&
                  !(scope.EncryptingCredentials.SecurityKey is AsymmetricSecurityKey))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                                    new SecurityTokenException(SR.GetString(SR.ID4179)));
            }

            EncryptingCredentials targetWrappingCredentials = scope.EncryptingCredentials;

            //
            // Generate the proof key
            //
            string keyType = (string.IsNullOrEmpty(request.KeyType)) ? KeyTypes.Symmetric : request.KeyType;
            ProofDescriptor result = null;

            if (StringComparer.Ordinal.Equals(keyType, KeyTypes.Asymmetric))
            {
                //
                // Asymmetric is only supported with UseKey
                //
                if (request.UseKey == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidRequestException(SR.GetString(SR.ID3091)));
                }

                result = new AsymmetricProofDescriptor(request.UseKey.SecurityKeyIdentifier);
            }
            else if (StringComparer.Ordinal.Equals(keyType, KeyTypes.Symmetric))
            {
                //
                // Only support PSHA1. Overwrite STS to support custom key algorithm
                //
                if (request.ComputedKeyAlgorithm != null && !StringComparer.Ordinal.Equals(request.ComputedKeyAlgorithm, ComputedKeyAlgorithms.Psha1))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new RequestFailedException(SR.GetString(SR.ID2011, request.ComputedKeyAlgorithm)));
                }
                //
                // We must wrap the symmetric key inside the security token
                //
                if (targetWrappingCredentials == null && scope.SymmetricKeyEncryptionRequired)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new RequestFailedException(SR.GetString(SR.ID4007)));
                }

                //
                // We are encrypting the proof token or the server entropy using client's encrypting credential if present,
                // which will be used to encrypt the key during serialization.
                // Otherwise, we can only send back the key in plain text. However, the current implementation of 
                // WSTrustServiceContract sets the rst.ProofEncryption = null by default. Therefore, the server entropy
                // or the proof token will be sent in plain text no matter the client's entropy is sent encrypted or unencrypted.
                //
                if (request.KeySizeInBits.HasValue)
                {
                    if (request.Entropy != null)
                    {
                        result = new SymmetricProofDescriptor(request.KeySizeInBits.Value, targetWrappingCredentials, requestorWrappingCredentials,
                                                               request.Entropy.GetKeyBytes(), request.EncryptWith);
                    }
                    else
                    {
                        result = new SymmetricProofDescriptor(request.KeySizeInBits.Value, targetWrappingCredentials,
                                                               requestorWrappingCredentials, request.EncryptWith);
                    }
                }
                else
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new RequestFailedException(SR.GetString(SR.ID2059)));
                }
            }
            else if (StringComparer.Ordinal.Equals(keyType, KeyTypes.Bearer))
            {
                //
                // Intentionally empty, no proofDescriptor
                //
            }

            return result;
        }

        /// <summary>
        /// Get the Requestor's Proof encrypting credentials.
        /// </summary>
        /// <param name="request">RequestSecurityToken</param>
        /// <returns>EncryptingCredentials</returns>
        /// <exception cref="ArgumentNullException">Input argument 'request' is null.</exception>
        protected virtual EncryptingCredentials GetRequestorProofEncryptingCredentials(RST request)
        {
            if (request == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("request");
            }

            if (request.ProofEncryption == null)
            {
                return null;
            }

            X509SecurityToken x509SecurityToken = request.ProofEncryption.GetSecurityToken() as X509SecurityToken;
            if (x509SecurityToken != null)
            {
                return new X509EncryptingCredentials(x509SecurityToken);
            }

            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new RequestFailedException(SR.GetString(SR.ID2084, request.ProofEncryption.GetSecurityToken())));
        }

        /// <summary>
        /// Gets the lifetime of the issued token.
        /// Normally called with the lifetime that arrived in the RST.  
        /// The algorithm for calculating the token lifetime is:
        /// requestLifeTime (in)            LifeTime (returned)
        /// Created     Expires             Created             Expires
        /// null        null                DateTime.UtcNow     DateTime.UtcNow + SecurityTokenServiceConfiguration.DefaultTokenLifetime
        /// C           null                C                   C + SecurityTokenServiceConfiguration.DefaultTokenLifetime
        /// null        E                   DateTime.UtcNow     E
        /// C           E                   C                   E
        /// </summary>
        /// <param name="requestLifetime">The requestor's desired life time.</param>
        protected virtual Lifetime GetTokenLifetime(Lifetime requestLifetime)
        {
            DateTime created;
            DateTime expires;

            if (requestLifetime == null)
            {
                created = DateTime.UtcNow;
                expires = DateTimeUtil.Add(created, _securityTokenServiceConfiguration.DefaultTokenLifetime);
            }
            else
            {
                if (requestLifetime.Created.HasValue)
                {
                    created = requestLifetime.Created.Value;
                }
                else
                {
                    created = DateTime.UtcNow;
                }

                if (requestLifetime.Expires.HasValue)
                {
                    expires = requestLifetime.Expires.Value;
                }
                else
                {
                    expires = DateTimeUtil.Add(created, _securityTokenServiceConfiguration.DefaultTokenLifetime);
                }
            }

            VerifyComputedLifetime(created, expires);

            return new Lifetime(created, expires);
        }

        private void VerifyComputedLifetime(DateTime created, DateTime expires)
        {

            DateTime utcNow = DateTime.UtcNow;

            // if expires in past, throw
            if (DateTimeUtil.Add(DateTimeUtil.ToUniversalTime(expires), _securityTokenServiceConfiguration.MaxClockSkew) < utcNow)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidRequestException(SR.GetString(SR.ID2075, created, expires, utcNow)));
            }

            // if creation time specified is greater than one day in future, throw
            if (DateTimeUtil.ToUniversalTime(created) > DateTimeUtil.Add(utcNow + TimeSpan.FromDays(1), _securityTokenServiceConfiguration.MaxClockSkew))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidRequestException(SR.GetString(SR.ID2076, created, expires, utcNow)));
            }

            // if expiration time is equal to or before creation time, throw.  This would be hard to make happen as the Lifetime class checks this condition in the constructor
            if (expires <= created)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidRequestException(SR.GetString(SR.ID2077, created, expires)));
            }

            // if timespan is greater than allowed, throw
            if ((expires - created) > _securityTokenServiceConfiguration.MaximumTokenLifetime)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidRequestException(SR.GetString(SR.ID2078, created, expires, _securityTokenServiceConfiguration.MaximumTokenLifetime)));
            }

            return;
        }

        /// <summary>
        /// Creates the RSTR and finally read the information from TokenDescriptor and apply 
        /// those to the RSTR.
        /// </summary>
        /// <param name="request">The RST from the request.</param>
        /// <param name="tokenDescriptor">The token descriptor which contains the information for the issued token.</param>
        /// <returns>The RSTR for the response, null if the token descriptor is null.</returns>
        protected virtual RSTR GetResponse(RST request, SecurityTokenDescriptor tokenDescriptor)
        {
            if (tokenDescriptor != null)
            {
                RSTR rstr = new RSTR(request);
                tokenDescriptor.ApplyTo(rstr);

                // Set the replyTo address of the relying party (if any) in the outgoing RSTR from the generated 
                // token descriptor (STD)  based on the table below:
                //
                // RST.ReplyTo       STD.ReplyToAddress            RSTR.ReplyTo
                // ===========       ====================          ============
                // Set               Not Set                       Not Set
                // Set               Set                           Set to STD.ReplyToAddress
                // Not Set           Not Set                       Not Set
                // Not Set           Set                           Not Set
                //
                if (request.ReplyTo != null)
                {
                    rstr.ReplyTo = tokenDescriptor.ReplyToAddress;
                }

                //
                // Set the appliesTo address (if any) in the outgoing RSTR from the generated token descriptor. 
                //
                if (!string.IsNullOrEmpty(tokenDescriptor.AppliesToAddress))
                {
                    rstr.AppliesTo = new EndpointReference(tokenDescriptor.AppliesToAddress);
                }

                return rstr;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Async Cancel.
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        public virtual RSTR EndCancel(IAsyncResult result)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidRequestException(SR.GetString(SR.ID3141, "Cancel")));
        }

        /// <summary>
        /// Ends the Async call to BeginGetScope. Default implementation will throw a NotImplementedException.
        /// Refer MSDN articles on Using an AsyncCallback Delegate to End an Asynchronous Operation.
        /// </summary>
        /// <param name="result">Typed Async result which contains the result. This is the same instance of 
        /// IAsyncResult that was returned by the BeginGetScope method.</param>
        /// <returns>The scope.</returns>
        protected virtual Scope EndGetScope(IAsyncResult result)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotImplementedException(SR.GetString(SR.ID2081)));
        }

        /// <summary>
        /// Ends the async call of Issue request. This would finally return the RequestSecurityTokenResponse.
        /// </summary>
        /// <param name="result">The async result returned from the BeginIssue method.</param>
        /// <returns>The security token response.</returns>
        public virtual RSTR EndIssue(IAsyncResult result)
        {
            if (result == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("result");
            }

            if (!(result is TypedAsyncResult<RSTR>))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.ID2012, typeof(TypedAsyncResult<RSTR>), result.GetType())));
            }

            return TypedAsyncResult<RSTR>.End(result);
        }

        /// <summary>
        /// Async Renew.
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        public virtual RSTR EndRenew(IAsyncResult result)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidRequestException(SR.GetString(SR.ID3141, "Renew")));
        }

        /// <summary>
        /// Async Validate.
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        public virtual RSTR EndValidate(IAsyncResult result)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidRequestException(SR.GetString(SR.ID3141, "Validate")));
        }

        /// <summary>
        /// Retrieves the relying party information. Override this method to provide a custom scope with
        /// relying party related information.
        /// </summary>
        /// <param name="principal">The identity of the token requestor.</param>
        /// <param name="request">The request message.</param>
        /// <returns>A scope object based on the relying party related information.</returns>
        protected abstract Scope GetScope(ClaimsPrincipal principal, RST request);

        /// <summary>
        /// When overridden in a derived class, this method should return a collection of output subjects to be included in the issued token.
        /// </summary>
        /// <param name="principal">The ClaimsPrincipal that represents the identity of the requestor.</param>
        /// <param name="request">The token request parameters that arrived in the call.</param>
        /// <param name="scope">The scope information about the Relying Party.</param>
        /// <returns>The ClaimsIdentity representing the collection of claims that will be placed in the issued security token.</returns>
        protected abstract ClaimsIdentity GetOutputClaimsIdentity(ClaimsPrincipal principal, RequestSecurityToken request, Scope scope);

        /// <summary>
        /// Begins async call for GetOutputSubjects routine. Default implementation will throw a NotImplementedExcetion.
        /// Refer MSDN articles on Using an AsyncCallback Delegate to End an Asynchronous Operation.
        /// </summary>
        /// <param name="principal">The authorization context that represents the identity of the requestor.</param>
        /// <param name="request">The token request parameters that arrived in the call.</param>
        /// <param name="scope">The scope information about the Relying Party.</param>
        /// <param name="callback">The callback to be invoked when the user Asynchronous operation completed.</param>
        /// <param name="state">The state object.</param>
        /// <returns>IAsyncResult. Represents the status of an asynchronous operation. This will be passed into 
        /// EndGetOutputClaimsIdentity.</returns>
        protected virtual IAsyncResult BeginGetOutputClaimsIdentity(ClaimsPrincipal principal, RequestSecurityToken request, Scope scope, AsyncCallback callback, object state)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotImplementedException(SR.GetString(SR.ID2081)));
        }

        /// <summary>
        /// Ends the Async call to BeginGetOutputSubjects. Default implementation will throw a NotImplementedExcetion.
        /// Refer MSDN articles on Using an AsyncCallback Delegate to End an Asynchronous Operation.
        /// </summary>
        /// <param name="result">Typed Async result which contains the result. This was the same IAsyncResult that
        /// was returned by the BeginGetOutputClaimsIdentity.</param>
        /// <returns>The claimsets collection that will be placed inside the issued token.</returns>
        protected virtual ClaimsIdentity EndGetOutputClaimsIdentity(IAsyncResult result)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotImplementedException(SR.GetString(SR.ID2081)));
        }

        /// <summary>
        /// Issues a Security Token.
        /// </summary>
        /// <param name="principal">The identity of the token requestor.</param>
        /// <param name="request">The request.</param>
        /// <returns>The response.</returns>
        public virtual RSTR Issue(ClaimsPrincipal principal, RST request)
        {
            if (request == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("request");
            }

            _principal = principal;
            _request = request;

            // 1. Do request validation
            ValidateRequest(request);

            // 2. Get the scope and populate into tokenDescriptor
            Scope scope = GetScope(principal, request);
            if (scope == null)
            {
                throw DiagnosticUtility.ThrowHelperInvalidOperation(SR.GetString(SR.ID2013));
            }
            this.Scope = scope;


            // Create the security token descriptor now that we have a scope.
            this.SecurityTokenDescriptor = CreateSecurityTokenDescriptor(request, scope);
            if (this.SecurityTokenDescriptor == null)
            {
                throw DiagnosticUtility.ThrowHelperInvalidOperation(SR.GetString(SR.ID2003));
            }

            if (this.SecurityTokenDescriptor.SigningCredentials == null)
            {
                throw DiagnosticUtility.ThrowHelperInvalidOperation(SR.GetString(SR.ID2079));
            }

            //
            // If TokenEncryptionRequired is set to true, then we must encrypt the token.
            //
            if (this.Scope.TokenEncryptionRequired && this.SecurityTokenDescriptor.EncryptingCredentials == null)
            {
                throw DiagnosticUtility.ThrowHelperInvalidOperation(SR.GetString(SR.ID4184));
            }

            // 3. Get the token-handler
            SecurityTokenHandler securityTokenHandler = GetSecurityTokenHandler(request.TokenType);
            if (securityTokenHandler == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(SR.GetString(SR.ID4010, request.TokenType)));
            }


            // 4. Get the issuer name and populate into tokenDescriptor
            _tokenDescriptor.TokenIssuerName = GetValidIssuerName();

            // 5. Get the token lifetime and populate into tokenDescriptor
            _tokenDescriptor.Lifetime = GetTokenLifetime(request.Lifetime);

            // 6. Get the proof token and populate into tokenDescriptor
            _tokenDescriptor.Proof = GetProofToken(request, scope);

            // 7. Get the subjects and populate into tokenDescriptor
            _tokenDescriptor.Subject = GetOutputClaimsIdentity(principal, request, scope);

            // use the securityTokenHandler from Step 3 to create and setup the issued token information on the tokenDescriptor 
            // (actual issued token, AttachedReference and UnattachedReference)
            // TokenType is preserved from the request if possible
            if (!string.IsNullOrEmpty(request.TokenType))
            {
                _tokenDescriptor.TokenType = request.TokenType;
            }
            else
            {
                string[] identifiers = securityTokenHandler.GetTokenTypeIdentifiers();
                if (identifiers == null || identifiers.Length == 0)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.ID4264, request.TokenType)));
                }
                _tokenDescriptor.TokenType = identifiers[0];
            }
            _tokenDescriptor.Token = securityTokenHandler.CreateToken(_tokenDescriptor);
            _tokenDescriptor.AttachedReference = securityTokenHandler.CreateSecurityTokenReference(_tokenDescriptor.Token, true);
            _tokenDescriptor.UnattachedReference = securityTokenHandler.CreateSecurityTokenReference(_tokenDescriptor.Token, false);

            // 9. Create the RSTR
            RSTR rstr = GetResponse(request, _tokenDescriptor);

            return rstr;
        }

        /// <summary>
        /// Gets an appropriate SecurityTokenHandler for issuing a security token.
        /// </summary>
        /// <param name="requestedTokenType">The requested TokenType.</param>
        /// <returns>The SecurityTokenHandler to be used for creating the issued security token.</returns>
        protected virtual SecurityTokenHandler GetSecurityTokenHandler(string requestedTokenType)
        {
            string tokenType = string.IsNullOrEmpty(requestedTokenType) ? _securityTokenServiceConfiguration.DefaultTokenType : requestedTokenType;

            SecurityTokenHandler securityTokenHandler = _securityTokenServiceConfiguration.SecurityTokenHandlers[tokenType];
            return securityTokenHandler;
        }

        /// <summary>
        /// This routine processes the stored data about the target resource
        /// for who the Issued token is for.
        /// </summary>
        /// <param name="result">The async result.</param>
        /// <exception cref="ArgumentNullException">When the given async result is null.</exception>
        void OnGetScopeComplete(IAsyncResult result)
        {
            if (null == result)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("result");
            }

            FederatedAsyncState state = result.AsyncState as FederatedAsyncState;
            if (state == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.ID2001)));
            }

            Exception unhandledException = null;
            TypedAsyncResult<RSTR> typedResult = state.Result as TypedAsyncResult<RSTR>;
            if (typedResult == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.ID2004, typeof(TypedAsyncResult<RSTR>), state.Result.GetType())));
            }

            RST request = state.Request;

            try
            {
                //
                // 2. Retrieve the scope information
                //
                Scope scope = EndGetScope(result);
                if (scope == null)
                {
                    throw DiagnosticUtility.ThrowHelperInvalidOperation(SR.GetString(SR.ID2013));
                }
                this.Scope = scope;

                //
                // Create a security token descriptor
                //
                this.SecurityTokenDescriptor = CreateSecurityTokenDescriptor(request, this.Scope);
                if (this.SecurityTokenDescriptor == null)
                {
                    throw DiagnosticUtility.ThrowHelperInvalidOperation(SR.GetString(SR.ID2003));
                }

                if (this.SecurityTokenDescriptor.SigningCredentials == null)
                {
                    throw DiagnosticUtility.ThrowHelperInvalidOperation(SR.GetString(SR.ID2079));
                }

                //
                // If TokenEncryptionRequired is set to true, then we must encrypt the token.
                //
                if (this.Scope.TokenEncryptionRequired && this.SecurityTokenDescriptor.EncryptingCredentials == null)
                {
                    throw DiagnosticUtility.ThrowHelperInvalidOperation(SR.GetString(SR.ID4184));
                }

                //
                // Step 3: Retrieve the token handler to use for creating token and store it in the state
                //
                SecurityTokenHandler securityTokenHandler = GetSecurityTokenHandler(request == null ? null : request.TokenType);
                if (securityTokenHandler == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(SR.GetString(SR.ID4010, request == null ? String.Empty : request.TokenType)));
                }
                state.SecurityTokenHandler = securityTokenHandler;

                //
                // Step 4: Logical issuer name
                //
                _tokenDescriptor.TokenIssuerName = GetValidIssuerName();

                //
                // Step 5: Establish token lifetime
                //
                _tokenDescriptor.Lifetime = GetTokenLifetime(request == null ? null : request.Lifetime);

                //
                // Step 6: Compute the proof key
                //
                _tokenDescriptor.Proof = GetProofToken(request, this.Scope);

                //
                // Start the async call for generating the output subjects. 
                //
                BeginGetOutputClaimsIdentity(state.ClaimsPrincipal, state.Request, scope, OnGetOutputClaimsIdentityComplete, state);
            }
#pragma warning suppress 56500
            catch (Exception e)
            {
                if (System.Runtime.Fx.IsFatal(e))
                    throw;

                unhandledException = e;
            }

            if (unhandledException != null)
            {
                //
                // Complete the request in failure
                //
                typedResult.Complete(null, result.CompletedSynchronously, unhandledException);
            }
        }

        /// <summary>
        /// The callback that is invoked on the completion of the BeginGetOutputSubjects call.
        /// </summary>
        /// <param name="result">The async result.</param>
        /// <exception cref="ArgumentNullException">When the given async result is null.</exception>
        void OnGetOutputClaimsIdentityComplete(IAsyncResult result)
        {
            if (null == result)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("result");
            }

            FederatedAsyncState state = result.AsyncState as FederatedAsyncState;
            if (state == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.ID2001)));
            }

            SecurityTokenHandler securityTokenHandler = state.SecurityTokenHandler;
            if (securityTokenHandler == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.ID2016)));
            }

            Exception unhandledException = null;
            RST request = state.Request;
            RSTR response = null;

            TypedAsyncResult<RSTR> typedResult = state.Result as TypedAsyncResult<RSTR>;
            if (typedResult == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.ID2004, typeof(TypedAsyncResult<RSTR>), state.Result.GetType())));
            }

            try
            {
                //
                // get token descriptor
                //
                if (_tokenDescriptor == null)
                {
                    throw DiagnosticUtility.ThrowHelperInvalidOperation(SR.GetString(SR.ID2003));
                }
                // Step 7: Retrieve the output claims to be included in the issued-token
                _tokenDescriptor.Subject = EndGetOutputClaimsIdentity(result);

                //
                // Use the retrieved securityTokenHandler to create and setup the issued token information on the tokenDescriptor 
                // (actual issued token, AttachedReference and UnattachedReference) 
                // TokenType is preserved from the request if possible
                if (!string.IsNullOrEmpty(request.TokenType))
                {
                    _tokenDescriptor.TokenType = request.TokenType;
                }
                else
                {
                    string[] identifiers = securityTokenHandler.GetTokenTypeIdentifiers();
                    if (identifiers == null || identifiers.Length == 0)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.ID4264, request.TokenType)));
                    }
                    _tokenDescriptor.TokenType = identifiers[0];
                }
                _tokenDescriptor.Token = securityTokenHandler.CreateToken(_tokenDescriptor);
                _tokenDescriptor.AttachedReference = securityTokenHandler.CreateSecurityTokenReference(_tokenDescriptor.Token, true);
                _tokenDescriptor.UnattachedReference = securityTokenHandler.CreateSecurityTokenReference(_tokenDescriptor.Token, false);

                // 9. Create the RSTR
                response = GetResponse(request, _tokenDescriptor);
            }
#pragma warning suppress 56500
            catch (Exception e)
            {
                if (System.Runtime.Fx.IsFatal(e))
                    throw;

                unhandledException = e;
            }

            typedResult.Complete(response, typedResult.CompletedSynchronously, unhandledException);
        }

        /// <summary>
        /// Gets the Owner configuration instance. 
        /// </summary>
        public SecurityTokenServiceConfiguration SecurityTokenServiceConfiguration
        {
            get
            {
                return _securityTokenServiceConfiguration;
            }
        }

        /// <summary>
        /// Gets or sets the ClaimsPrincipal associated with the current instance. 
        /// </summary>
        public ClaimsPrincipal Principal
        {
            get { return _principal; }
            set { _principal = value; }
        }

        /// <summary>
        /// Gets or sets the RequestSecurityToken associated with the current instance. 
        /// </summary>
        public RequestSecurityToken Request
        {
            get { return _request; }
            set { _request = value; }
        }

        /// <summary>
        /// Gets or sets the Scope associated with the current instance.
        /// </summary>
        public Scope Scope
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the SecurityTokenDescriptor associated with the current instance. 
        /// </summary>
        protected SecurityTokenDescriptor SecurityTokenDescriptor
        {
            get { return _tokenDescriptor; }
            set
            {
                if (value == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");
                }
                _tokenDescriptor = value;
            }
        }

        /// <summary>
        /// Renew.
        /// </summary>
        /// <param name="principal">The <see cref="ClaimsPrincipal"/> to renew.</param>
        /// <param name="request">The request.</param>
        /// <returns>The response.</returns>
        public virtual RSTR Renew(ClaimsPrincipal principal, RST request)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidRequestException(SR.GetString(SR.ID3141, (request != null && request.RequestType != null ? request.RequestType : "Renew"))));
        }

        /// <summary>
        /// Validate.
        /// </summary>
        /// <param name="principal">The <see cref="ClaimsPrincipal"/> to validate.</param>
        /// <param name="request">The request.</param>
        /// <returns>The response.</returns>
        public virtual RSTR Validate(ClaimsPrincipal principal, RST request)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidRequestException(SR.GetString(SR.ID3141, (request != null && request.RequestType != null ? request.RequestType : "Validate"))));
        }

        /// <summary>
        /// Validates the RequestSecurityToken encapsulated by this SecurityTokenService instance.
        /// </summary>
        protected virtual void ValidateRequest(RST request)
        {
            // currently we only support RST/RSTR pattern
            if (request == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidRequestException(SR.GetString(SR.ID2051)));
            }

            // STS only support Issue for now
            if (request.RequestType != null && request.RequestType != RequestTypes.Issue)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidRequestException(SR.GetString(SR.ID2052)));
            }

            // key type must be one of the known types
            if (request.KeyType != null && !IsKnownType(request.KeyType))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidRequestException(SR.GetString(SR.ID2053)));
            }

            // if key type is bearer key, we should fault if the KeySize element is present and its value is not equal to zero.
            if (StringComparer.Ordinal.Equals(request.KeyType, KeyTypes.Bearer) && request.KeySizeInBits.HasValue && (request.KeySizeInBits.Value != 0))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidRequestException(SR.GetString(SR.ID2050)));
            }

            // token type must be supported for this STS
            if (GetSecurityTokenHandler(request.TokenType) == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new UnsupportedTokenTypeBadRequestException(request.TokenType));
            }

            request.KeyType = (string.IsNullOrEmpty(request.KeyType)) ? KeyTypes.Symmetric : request.KeyType;

            if (StringComparer.Ordinal.Equals(request.KeyType, KeyTypes.Symmetric))
            {
                //
                // Check if the key size is within certain limit to prevent Dos attack
                //
                if (request.KeySizeInBits.HasValue)
                {
                    if (request.KeySizeInBits.Value > _securityTokenServiceConfiguration.DefaultMaxSymmetricKeySizeInBits)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidRequestException(SR.GetString(SR.ID2056, request.KeySizeInBits.Value, _securityTokenServiceConfiguration.DefaultMaxSymmetricKeySizeInBits)));
                    }
                }
                else
                {
                    request.KeySizeInBits = _securityTokenServiceConfiguration.DefaultSymmetricKeySizeInBits;
                }
            }
        }

        static bool IsKnownType(string keyType)
        {
            return (StringComparer.Ordinal.Equals(keyType, KeyTypes.Symmetric)
                  || StringComparer.Ordinal.Equals(keyType, KeyTypes.Asymmetric)
                  || StringComparer.Ordinal.Equals(keyType, KeyTypes.Bearer));
        }
    }
}
