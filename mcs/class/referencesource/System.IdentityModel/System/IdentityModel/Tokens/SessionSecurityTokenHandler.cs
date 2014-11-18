//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.IdentityModel.Tokens
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.IdentityModel.Configuration;
    using System.IdentityModel.Diagnostics;
    using System.IdentityModel.Selectors;
    using System.IO;
    using System.Runtime;
    using System.Security.Claims;
    using System.ServiceModel.Security;
    using System.Xml;
    using SessionDictionary = System.IdentityModel.Claims.SessionDictionary;
    using SysUniqueId = System.Xml.UniqueId;
    using System.Security.Cryptography.X509Certificates;
    using System.Runtime.Serialization.Formatters.Binary;

    /// <summary>
    /// A <see cref="SecurityTokenHandler"/> that processes <see cref="SessionSecurityToken"/>.
    /// </summary>
    public class SessionSecurityTokenHandler : SecurityTokenHandler
    {
        const string DefaultCookieElementName = "Cookie";
        const string DefaultCookieNamespace = "http://schemas.microsoft.com/ws/2006/05/security";
        private const string SecureConversationTokenIdentifier = "http://schemas.microsoft.com/ws/2006/05/servicemodel/tokens/SecureConversation";

#pragma warning disable 1591
        public static readonly TimeSpan DefaultLifetime = TimeSpan.FromHours(10);
        public static readonly ReadOnlyCollection<CookieTransform> DefaultCookieTransforms = (new List<CookieTransform>(new CookieTransform[] { new DeflateCookieTransform(), new ProtectedDataCookieTransform() }).AsReadOnly());
#pragma warning restore 1591

        TimeSpan _tokenLifetime = DefaultLifetime;
        ReadOnlyCollection<CookieTransform> _transforms;

        /// <summary>
        /// Initializes an instance of <see cref="SessionSecurityTokenHandler"/>
        /// </summary>
        /// <remarks>
        /// Properties are used for defaults:
        /// DefaultCookieTransforms
        /// DefaultLifetime
        /// </remarks>
        public SessionSecurityTokenHandler()
            : this(SessionSecurityTokenHandler.DefaultCookieTransforms)
        {
        }

        /// <summary>
        /// Initializes an instance of <see cref="SessionSecurityTokenHandler"/>
        /// </summary>
        /// <param name="transforms">The transforms to apply when encoding the cookie.</param>
        /// <remarks>
        /// Properties are used for the remaining defaults:
        /// DefaultLifetime
        /// </remarks>
        public SessionSecurityTokenHandler(ReadOnlyCollection<CookieTransform> transforms)
            : this(transforms, DefaultLifetime)
        { }

        /// <summary>
        /// Initializes an instance of <see cref="SessionSecurityTokenHandler"/>
        /// </summary>
        /// <param name="transforms">The transforms to apply when encoding the cookie.</param>
        /// <param name="tokenLifetime">The default for a token.</param>
        /// <exception cref="ArgumentNullException">Is thrown if 'transforms' is null.</exception>
        /// <exception cref="InvalidOperationException">Is thrown if 'tokenLifetime' is less than or equal to TimeSpan.Zero.</exception>
        public SessionSecurityTokenHandler(ReadOnlyCollection<CookieTransform> transforms, TimeSpan tokenLifetime)
        {
            if (transforms == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("transforms");
            }

            if (tokenLifetime <= TimeSpan.Zero)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.ID0016)));
            }

            _transforms = transforms;
            _tokenLifetime = tokenLifetime;
        }

        /// <summary>
        /// Load custom configuration from Xml
        /// </summary>
        /// <param name="customConfigElements">XmlElement to custom configuration.</param>
        /// <exception cref="ArgumentNullException">The param customConfigElements is null.</exception>
        /// <exception cref="InvalidOperationException">Custom configuration specified was invalid.</exception>
        public override void LoadCustomConfiguration(XmlNodeList customConfigElements)
        {
            if (customConfigElements == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("customConfigElements");
            }

            List<XmlElement> configNodes = XmlUtil.GetXmlElements(customConfigElements);

            bool foundValidConfig = false;

            foreach (XmlElement customConfigElement in configNodes)
            {
                if (!StringComparer.Ordinal.Equals(customConfigElement.LocalName, ConfigurationStrings.SessionTokenRequirement))
                {
                    continue;
                }

                if (foundValidConfig)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.ID7026, ConfigurationStrings.SessionTokenRequirement)));
                }

                _tokenLifetime = DefaultLifetime;

                foreach (XmlAttribute attribute in customConfigElement.Attributes)
                {
                    if (StringComparer.OrdinalIgnoreCase.Equals(attribute.LocalName, ConfigurationStrings.Lifetime))
                    {
                        TimeSpan outTokenLifetime = DefaultLifetime;
                        if (!TimeSpan.TryParse(attribute.Value, out outTokenLifetime))
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.ID7017, attribute.Value)));
                        }
                        if (outTokenLifetime < TimeSpan.Zero)
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.ID7018)));
                        }
                        _tokenLifetime = outTokenLifetime;
                    }
                    else
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.ID7004, attribute.LocalName, customConfigElement.LocalName)));
                    }
                }

                foundValidConfig = true;
            }
        }

        /// <summary>
        /// Gets the name for the cookie element.
        /// </summary>
        public virtual string CookieElementName
        {
            get { return DefaultCookieElementName; }
        }

        /// <summary>
        /// Gets the namspace for the cookie element.
        /// </summary>
        public virtual string CookieNamespace
        {
            get { return DefaultCookieNamespace; }
        }

        /// <summary>
        /// Applies Transforms to the cookie.
        /// </summary>
        /// <param name="cookie">The cookie that will be transformed.</param>
        /// <param name="outbound">Controls if the cookie should be encoded (true) or decoded (false)</param>
        /// <returns>Encoded cookie.</returns>
        protected virtual byte[] ApplyTransforms(byte[] cookie, bool outbound)
        {
            byte[] transformedCookie = cookie;

            if (Transforms == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.ID4296)));
            }

            if (outbound)
            {
                for (int i = 0; i < _transforms.Count; i++)
                {
                    transformedCookie = _transforms[i].Encode(transformedCookie);
                }
            }
            else
            {
                for (int i = _transforms.Count; i > 0; i--)
                {
                    transformedCookie = _transforms[i - 1].Decode(transformedCookie);
                }
            }

            return transformedCookie;
        }

        /// <summary>
        /// Checks the reader if this is a SecurityContextToken.
        /// </summary>
        /// <param name="reader">XmlReader over the incoming SecurityToken.</param>
        /// <returns>'True' if the reader points to a SecurityContextToken.</returns>
        /// <exception cref="ArgumentNullException">The input argument 'reader' is null.</exception>
        public override bool CanReadToken(XmlReader reader)
        {
            if (reader == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("reader");
            }

            return (reader.IsStartElement(WSSecureConversationFeb2005Constants.ElementNames.Name, WSSecureConversationFeb2005Constants.Namespace)
                  || reader.IsStartElement(WSSecureConversation13Constants.ElementNames.Name, WSSecureConversation13Constants.Namespace));
        }

        /// <summary>
        /// Indicates whether this handler supports validation of tokens.
        /// </summary>
        /// <returns>'True' if the class is capable of SecurityToken validation.</returns>
        public override bool CanValidateToken
        {
            get { return true; }
        }

        /// <summary>
        /// Gets information on whether this Token Handler can write tokens.
        /// </summary>
        public override bool CanWriteToken
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Creates a security token based on a token descriptor.
        /// </summary>
        /// <param name="tokenDescriptor">The token descriptor.</param>
        /// <returns>A security token.</returns>
        /// <exception cref="ArgumentNullException">Thrown if 'tokenDescriptor' is null.</exception>
        public override SecurityToken CreateToken(SecurityTokenDescriptor tokenDescriptor)
        {
            if (null == tokenDescriptor)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("tokenDescriptor");
            }

            if (this.Configuration == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.ID4272)));
            }

            ClaimsPrincipal principal = new ClaimsPrincipal(tokenDescriptor.Subject);

            if (this.Configuration.SaveBootstrapContext)
            {
                SecurityTokenHandlerCollection bootstrapTokenCollection = CreateBootstrapTokenHandlerCollection();
                if (!bootstrapTokenCollection.CanWriteToken(tokenDescriptor.Token))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.ID4010, tokenDescriptor.Token.GetType().ToString())));
                }

                (principal.Identities as ReadOnlyCollection<ClaimsIdentity>)[0].BootstrapContext = new BootstrapContext(tokenDescriptor.Token, bootstrapTokenCollection[tokenDescriptor.Token.GetType()]);
            }

            DateTime validFrom = (tokenDescriptor.Lifetime.Created.HasValue) ? (DateTime)tokenDescriptor.Lifetime.Created : DateTime.UtcNow;
            DateTime validTo = (tokenDescriptor.Lifetime.Expires.HasValue) ? (DateTime)tokenDescriptor.Lifetime.Expires : DateTime.UtcNow + SessionSecurityTokenHandler.DefaultTokenLifetime;

            return new SessionSecurityToken(principal, null, validFrom, validTo);
        }

        /// <summary>
        /// Creates a <see cref="SessionSecurityToken"/> based on an <see cref="ClaimsPrincipal"/> and a valid time range.
        /// </summary>
        /// <param name="principal"><see cref="ClaimsPrincipal"/></param>
        /// <param name="context">Caller defined context string</param>
        /// <param name="endpointId">Identifier of the endpoint to which the token is scoped.</param>
        /// <param name="validFrom">Earliest valid time.</param>
        /// <param name="validTo">Latest valid time.</param>
        public virtual SessionSecurityToken CreateSessionSecurityToken(
            ClaimsPrincipal principal,
            string context,
            string endpointId,
            DateTime validFrom,
            DateTime validTo)
        {
            if (null == principal)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("principal");
            }

            if (this.Configuration == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.ID4272)));
            }

            return new SessionSecurityToken(principal, context, endpointId, validFrom, validTo);
        }

        /// <summary>
        /// Gets the default token lifetime
        /// </summary>
        public static TimeSpan DefaultTokenLifetime
        {
            get { return DefaultLifetime; }
        }

        /// <summary>
        /// Reads the SessionSecurityToken from a stream of bytes.
        /// </summary>
        /// <param name="token">token.</param>
        /// <param name="tokenResolver">SecurityTokenResolver that can be used to resolve the SessionSecurityToken.</param>
        /// <returns>Instance of SessionSecurityToken.</returns>
        public virtual SecurityToken ReadToken(byte[] token, SecurityTokenResolver tokenResolver)
        {
            // Our implementation of ReadToken( byte[] ) will always return null. We make the above call not to 
            // break SharePoint. SharePoint has overridden ReadToken(byte[] token) and expect the SessionAuthenticationModule to 
            // call that. So SessionAuthenticationModule will calls this method which does the correct thing.
            using (XmlReader reader = XmlDictionaryReader.CreateTextReader(token, XmlDictionaryReaderQuotas.Max))
            {
                return this.ReadToken(reader, tokenResolver);
            }
        }

        /// <summary>
        /// Reads the SessionSecurityToken from the given reader.
        /// </summary>
        /// <param name="reader">XmlReader over the SessionSecurityToken.</param>
        /// <returns>An instance of <see cref="SessionSecurityToken"/>.</returns> 
        /// <exception cref="ArgumentNullException">The input argument 'reader' is null.</exception>
        /// <exception cref="SecurityTokenException">The 'reader' is not positioned at a SessionSecurityToken
        /// or the SessionSecurityToken cannot be read.</exception>
        public override SecurityToken ReadToken(XmlReader reader)
        {
            return this.ReadToken(reader, EmptySecurityTokenResolver.Instance);
        }

        /// <summary>
        /// Reads the SessionSecurityToken from the given reader.
        /// </summary>
        /// <param name="reader">XmlReader over the SessionSecurityToken.</param>
        /// <param name="tokenResolver">SecurityTokenResolver that can used to resolve SessionSecurityToken.</param>
        /// <returns>An instance of <see cref="SessionSecurityToken"/>.</returns> 
        /// <exception cref="ArgumentNullException">The input argument 'reader' is null.</exception>
        /// <exception cref="SecurityTokenException">The 'reader' is not positioned at a SessionSecurityToken
        /// or the SessionSecurityToken cannot be read.</exception>
        public override SecurityToken ReadToken(XmlReader reader, SecurityTokenResolver tokenResolver)
        {
            if (reader == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("reader");
            }

            if (tokenResolver == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("tokenResolver");
            }

            byte[] encodedCookie = null;
            SysUniqueId contextId = null;
            SysUniqueId keyGeneration = null;

            string ns = null;
            string identifier = null;
            string instance = null;

            SecurityToken securityContextToken = null;
            SessionDictionary dictionary = SessionDictionary.Instance;

            XmlDictionaryReader dicReader = XmlDictionaryReader.CreateDictionaryReader(reader);

            if (dicReader.IsStartElement(WSSecureConversationFeb2005Constants.ElementNames.Name, WSSecureConversationFeb2005Constants.Namespace))
            {
                ns = WSSecureConversationFeb2005Constants.Namespace;
                identifier = WSSecureConversationFeb2005Constants.ElementNames.Identifier;
                instance = WSSecureConversationFeb2005Constants.ElementNames.Instance;
            }
            else if (dicReader.IsStartElement(WSSecureConversation13Constants.ElementNames.Name, WSSecureConversation13Constants.Namespace))
            {
                ns = WSSecureConversation13Constants.Namespace;
                identifier = WSSecureConversation13Constants.ElementNames.Identifier;
                instance = WSSecureConversation13Constants.ElementNames.Instance;
            }
            else
            {
                //
                // Something is wrong
                //
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(
                    SR.GetString(SR.ID4230, WSSecureConversationFeb2005Constants.ElementNames.Name, dicReader.Name)));
            }

            string id = dicReader.GetAttribute(WSUtilityConstants.Attributes.IdAttribute, WSUtilityConstants.NamespaceURI);

            dicReader.ReadFullStartElement();
            if (!dicReader.IsStartElement(identifier, ns))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(
                    SR.GetString(SR.ID4230, WSSecureConversation13Constants.ElementNames.Identifier, dicReader.Name)));
            }

            contextId = dicReader.ReadElementContentAsUniqueId();
            if (contextId == null || string.IsNullOrEmpty(contextId.ToString()))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(SR.GetString(SR.ID4242)));
            }

            //
            // The token can be a renewed token, in which case we need to know the
            // instance id, which will be the secondary key to the context id for 
            // cache lookups
            //
            if (dicReader.IsStartElement(instance, ns))
            {
                keyGeneration = dicReader.ReadElementContentAsUniqueId();
            }

            if (dicReader.IsStartElement(CookieElementName, CookieNamespace))
            {
                // Get the token from the Cache, which is returned as an SCT
                SecurityToken cachedToken = null;

                SecurityContextKeyIdentifierClause sctClause = null;
                if (keyGeneration == null)
                {
                    sctClause = new SecurityContextKeyIdentifierClause(contextId);
                }
                else
                {
                    sctClause = new SecurityContextKeyIdentifierClause(contextId, keyGeneration);
                }

                tokenResolver.TryResolveToken(sctClause, out cachedToken);

                if (cachedToken != null)
                {
                    securityContextToken = cachedToken;

                    dicReader.Skip();
                }
                else
                {
                    //
                    // CookieMode
                    //
                    encodedCookie = dicReader.ReadElementContentAsBase64();

                    if (encodedCookie == null)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(SR.GetString(SR.ID4237)));
                    }
                    //
                    // appply transforms
                    //
                    byte[] decodedCookie = ApplyTransforms(encodedCookie, false);

                    using (MemoryStream ms = new MemoryStream(decodedCookie))
                    {
                        BinaryFormatter formatter = new BinaryFormatter();
                        securityContextToken = formatter.Deserialize(ms) as SecurityToken;
                    }

                    SessionSecurityToken sessionToken = securityContextToken as SessionSecurityToken;
                    if (sessionToken != null && sessionToken.ContextId != contextId)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(SR.GetString(SR.ID4229, sessionToken.ContextId, contextId)));
                    }

                    if (sessionToken != null && sessionToken.Id != id)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(SR.GetString(SR.ID4227, sessionToken.Id, id)));
                    }
                }
            }
            else
            {
                //
                // SessionMode
                //

                // Get the token from the Cache.
                SecurityToken cachedToken = null;

                SecurityContextKeyIdentifierClause sctClause = null;
                if (keyGeneration == null)
                {
                    sctClause = new SecurityContextKeyIdentifierClause(contextId);
                }
                else
                {
                    sctClause = new SecurityContextKeyIdentifierClause(contextId, keyGeneration);
                }

                tokenResolver.TryResolveToken(sctClause, out cachedToken);

                if (cachedToken != null)
                {
                    securityContextToken = cachedToken;
                }
            }

            dicReader.ReadEndElement();

            if (securityContextToken == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(SR.GetString(SR.ID4243)));
            }

            return securityContextToken;
        }

        /// <summary>
        /// Gets or sets the TokenLifetime.
        /// </summary>
        public virtual TimeSpan TokenLifetime
        {
            get { return _tokenLifetime; }
            set
            {
                if (value <= TimeSpan.Zero)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("value", SR.GetString(SR.ID0016));
                }

                _tokenLifetime = value;
            }
        }

        /// <summary>
        /// Gets the bootstrap token handler collection.
        /// </summary>
        SecurityTokenHandlerCollection CreateBootstrapTokenHandlerCollection()
        {
            SecurityTokenHandlerCollection tokenHandlerCollection = this.ContainingCollection ?? SecurityTokenHandlerCollection.CreateDefaultSecurityTokenHandlerCollection();
            return tokenHandlerCollection;
        }

        /// <summary>
        /// Gets the token type URIs
        /// </summary>
        public override string[] GetTokenTypeIdentifiers()
        {
            return new string[] { SecureConversationTokenIdentifier,
                                 WSSecureConversation13Constants.TokenTypeURI,
                                 WSSecureConversationFeb2005Constants.TokenTypeURI };
        }

        /// <summary>
        /// Gets the type of token this handler can work with.
        /// </summary>
        public override Type TokenType
        {
            get { return typeof(SessionSecurityToken); }
        }

        /// <summary>
        /// Gets the transforms that will be applied to the cookie.
        /// </summary>        
        public ReadOnlyCollection<CookieTransform> Transforms
        {
            get
            {
                return _transforms;
            }
        }

        /// <summary>
        /// Sets the transforms that will be applied to cookies.
        /// </summary>
        /// <param name="transforms">The <see cref="CookieTransform"/> objects to use.  </param>
        protected void SetTransforms(IEnumerable<CookieTransform> transforms)
        {
            _transforms = new List<CookieTransform>(transforms).AsReadOnly();
        }

        /// <summary>
        /// Validates a <see cref="SessionSecurityToken"/>.
        /// </summary>
        /// <param name="token">The <see cref="SessionSecurityToken"/> to validate.</param>
        /// <returns>A <see cref="ReadOnlyCollection{T}"/> of <see cref="ClaimsIdentity"/> representing the identities contained in the token.</returns>
        /// <exception cref="ArgumentNullException">The parameter 'token' is null.</exception>
        /// <exception cref="ArgumentException">The token is not assignable from <see cref="SessionSecurityToken"/>.</exception>
        public override ReadOnlyCollection<ClaimsIdentity> ValidateToken(SecurityToken token)
        {
            if (token == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("token");
            }

            SessionSecurityToken sessionToken = token as SessionSecurityToken;
            if (sessionToken == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.ID4292, token.GetType().ToString(), this.GetType().ToString())));
            }

            try
            {
                if (DiagnosticUtility.ShouldTrace(TraceEventType.Verbose))
                {
                    TraceUtility.TraceEvent(
                        TraceEventType.Verbose,
                        TraceCode.Diagnostics,
                        SR.GetString(SR.TraceValidateToken),
                        new SecurityTraceRecordHelper.TokenTraceRecord(token),
                        null,
                        null);
                }

                this.ValidateSession(sessionToken);

                this.TraceTokenValidationSuccess(token);

                List<ClaimsIdentity> identitites = new List<ClaimsIdentity>(1);
                identitites.AddRange(sessionToken.ClaimsPrincipal.Identities);
                return identitites.AsReadOnly();
            }
            catch (Exception e)
            {
                if (Fx.IsFatal(e))
                {
                    throw;
                }

                this.TraceTokenValidationFailure(token, e.Message);
                throw e;
            }
        }

        /// <summary>
        /// Validates a token and returns its claims.
        /// </summary>
        /// <param name="token">The <see cref="SessionSecurityToken"/> to validate.</param>
        /// <param name="endpointId">Identifier to the endpoint to which the token is scoped.</param>
        /// <returns>A <see cref="ReadOnlyCollection{T}"/> of <see cref="ClaimsIdentity"/> representing the identities contained in the token.</returns>
        /// <exception cref="ArgumentNullException">The parameter 'token' is null.</exception>
        /// <exception cref="ArgumentNullException">The parameter 'endpointId' is null.</exception>
        /// <exception cref="SecurityTokenException">token.EndpointId != endpointId.</exception>
        public virtual ReadOnlyCollection<ClaimsIdentity> ValidateToken(SessionSecurityToken token, string endpointId)
        {
            if (token == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("token");
            }

            if (endpointId == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("endpointId");
            }

            // We consider SessionTokens with String.Empty as the endpoint Id to be
            // globally scoped tokens. This in insecure, we are allowing this only 
            // for compatibility with customers who have overriden SessionSecurityTokenHandler.
            if (!string.IsNullOrEmpty(token.EndpointId))
            {
                if (token.EndpointId != endpointId)
                {
                    string errorMessage = SR.GetString(SR.ID4291, token);
                    this.TraceTokenValidationFailure(token, errorMessage);
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(errorMessage));
                }
            }

            return this.ValidateToken(token);
        }

        /// <summary>
        /// Checks the valid time of a SecurityToken. 
        /// </summary>
        /// <remarks>
        /// The token is invalid if the securityToken.ValidFrom &gt; DateTime.UtcNow OR securityToken.ValidTo &lt; DateTime.UtcNow 
        /// </remarks>
        /// <param name="token">The <see cref="SessionSecurityToken"/> to validate.</param>
        /// <exception cref="ArgumentNullException">Thrown if 'securityToken' is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown if 'Configuration' is null.</exception>
        /// <exception cref="SecurityTokenNotYetValidException">Thrown if securityToken.ValidFrom &gt; DateTime.UtcNow.</exception>
        /// <exception cref="SecurityTokenExpiredException">Thrown if securityToken.ValidTo &lt; DateTime.UtcNow.</exception>
        protected virtual void ValidateSession(SessionSecurityToken securityToken)
        {
            if (securityToken == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("securityToken");
            }

            if (this.Configuration == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.ID4274)));
            }

            Fx.Assert(this.Configuration != null, SR.GetString(SR.ID8027));

            DateTime utcNow = DateTime.UtcNow;

            // apply clock skew here.
            DateTime maxTime = DateTimeUtil.Add(utcNow, Configuration.MaxClockSkew);
            DateTime minTime = DateTimeUtil.Add(utcNow, -Configuration.MaxClockSkew);

            if (securityToken.ValidFrom > maxTime)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenNotYetValidException(SR.GetString(SR.ID4255, securityToken.ValidTo, securityToken.ValidFrom, utcNow)));
            }

            if (securityToken.ValidTo < minTime)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenExpiredException(SR.GetString(SR.ID4255, securityToken.ValidTo, securityToken.ValidFrom, utcNow)));
            }
        }

        /// <summary>
        /// Writes the token into a byte array.
        /// </summary>
        /// <param name="sessionToken">The SessionSecurityToken to write.</param>
        /// <exception cref="ArgumentNullException">Thrown if 'sessiontoken' is null.</exception>
        /// <returns>An encoded byte array.</returns>
        public virtual byte[] WriteToken(SessionSecurityToken sessionToken)
        {
            if (sessionToken == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("sessionToken");
            }

            using (MemoryStream ms = new MemoryStream())
            {
                using (XmlWriter writer = XmlWriter.Create(ms))
                {
                    WriteToken(writer, sessionToken);
                    writer.Flush();
                }

                return ms.ToArray();
            }
        }

        /// <summary>
        /// Serializes the given token to the XmlWriter.
        /// </summary>
        /// <param name="writer">XmlWriter to which the token needs to be serialized</param>
        /// <param name="token">The SecurityToken to be serialized.</param>
        /// <exception cref="ArgumentNullException">The input argument 'writer' is null.</exception>
        /// <exception cref="InvalidOperationException">The input argument 'token' is either null or not of type
        /// SessionSecurityToken.</exception>
        public override void WriteToken(XmlWriter writer, SecurityToken token)
        {
            if (writer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("writer");
            }

            if (token == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("token");
            }

            SessionSecurityToken sessionToken = token as SessionSecurityToken;
            if (sessionToken == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.ID4046, token, TokenType)));
            }

            string ns, elementName, contextIdElementName, instance;

            if (sessionToken.SecureConversationVersion == WSSecureConversationFeb2005Constants.NamespaceUri)
            {
                ns = WSSecureConversationFeb2005Constants.Namespace;
                elementName = WSSecureConversationFeb2005Constants.ElementNames.Name;
                contextIdElementName = WSSecureConversationFeb2005Constants.ElementNames.Identifier;
                instance = WSSecureConversationFeb2005Constants.ElementNames.Instance;
            }
            else if (sessionToken.SecureConversationVersion == WSSecureConversation13Constants.NamespaceUri)
            {
                ns = WSSecureConversation13Constants.Namespace;
                elementName = WSSecureConversation13Constants.ElementNames.Name;
                contextIdElementName = WSSecureConversation13Constants.ElementNames.Identifier;
                instance = WSSecureConversation13Constants.ElementNames.Instance;
            }
            else
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.ID4050)));
            }

            XmlDictionaryWriter dicWriter;
            SessionDictionary dictionary = SessionDictionary.Instance;

            if (writer is XmlDictionaryWriter)
            {
                dicWriter = (XmlDictionaryWriter)writer;
            }
            else
            {
                dicWriter = XmlDictionaryWriter.CreateDictionaryWriter(writer);
            }

            dicWriter.WriteStartElement(elementName, ns);
            if (sessionToken.Id != null)
            {
                dicWriter.WriteAttributeString(WSUtilityConstants.Attributes.IdAttribute, WSUtilityConstants.NamespaceURI, sessionToken.Id);
            }

            dicWriter.WriteElementString(contextIdElementName, ns, sessionToken.ContextId.ToString());

            if (sessionToken.KeyGeneration != null)
            {
                dicWriter.WriteStartElement(instance, ns);
                dicWriter.WriteValue(sessionToken.KeyGeneration);
                dicWriter.WriteEndElement();
            }

            if (!sessionToken.IsReferenceMode)
            {
                dicWriter.WriteStartElement(CookieElementName, CookieNamespace);
                byte[] cookie;

                using (MemoryStream ms = new MemoryStream())
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    formatter.Serialize(ms, token);
                    cookie = ms.ToArray();
                }

                cookie = ApplyTransforms(cookie, true);
                dicWriter.WriteBase64(cookie, 0, cookie.Length);
                dicWriter.WriteEndElement();
            }

            dicWriter.WriteEndElement();
            dicWriter.Flush();
        }

    }
}
