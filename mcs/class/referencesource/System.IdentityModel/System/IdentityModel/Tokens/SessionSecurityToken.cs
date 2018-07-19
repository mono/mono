//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.IdentityModel.Tokens
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.IdentityModel.Claims;
    using System.IdentityModel.Policy;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.Security.Claims;
    using System.Security.Cryptography;
    using System.Security.Cryptography.X509Certificates;
    using System.Security.Principal;
    using System.Text;
    using System.Xml;
    using Claim = System.Security.Claims.Claim;
    using ClaimTypes = System.Security.Claims.ClaimTypes;
    using SysClaim = System.IdentityModel.Claims.Claim;
    using SysClaimTypes = System.IdentityModel.Claims.ClaimTypes;
    using SysUniqueId = System.Xml.UniqueId;
        
    /// <summary>
    /// Defines a SessionSecurityToken that contains data associated with a session.
    /// </summary>
    [Serializable]
    public class SessionSecurityToken : SecurityToken, ISerializable
    {
        const string SupportedVersion = "1";
        const string tokenKey = "SessionToken";
        const string WindowsSecurityTokenStubElementName = "WindowsSecurityTokenStub";

        static Dictionary<string, string> DomainNameMap = new Dictionary<string, string>(MaxDomainNameMapSize, StringComparer.OrdinalIgnoreCase);
        const int MaxDomainNameMapSize = 50;
        static Random rnd = new Random();

        string _context;
        bool _isPersistent;

        ClaimsPrincipal _claimsPrincipal;
        SctAuthorizationPolicy _sctAuthorizationPolicy;

        string _endpointId;
        bool _isReferenceMode;
        bool _isSecurityContextSecurityTokenWrapper;

        string _id;

        SysUniqueId _contextId;
        SysUniqueId _keyGeneration;

        DateTime _keyEffectiveTime;
        DateTime _keyExpirationTime;

        Uri _secureConversationVersion;

        DateTime _validFrom;
        DateTime _validTo;

        ReadOnlyCollection<SecurityKey> _securityKeys;

        /// <summary>
        /// Create session security token from a principal.
        /// </summary>
        /// <param name="claimsPrincipal">The <see cref="ClaimsPrincipal"/>.</param>
        /// <returns></returns>
        public SessionSecurityToken(ClaimsPrincipal claimsPrincipal)
            : this(claimsPrincipal, null)
        { }

        /// <summary>
        /// Create session security token from a principal.
        /// </summary>
        /// <param name="claimsPrincipal">The <see cref="ClaimsPrincipal"/>.</param>
        /// <param name="lifetime">The Timespan the token is valid.</param>
        public SessionSecurityToken(ClaimsPrincipal claimsPrincipal, TimeSpan lifetime)
            : this(claimsPrincipal, null, DateTime.UtcNow, DateTimeUtil.AddNonNegative(DateTime.UtcNow, lifetime))
        {
        }

        /// <summary>
        /// Create session security token from principal and bootstrap token.
        /// </summary>
        /// <param name="claimsPrincipal">The <see cref="ClaimsPrincipal"/> that generated from the bootstrap token.</param>
        /// <param name="context">Session specific context string</param>
        public SessionSecurityToken(ClaimsPrincipal claimsPrincipal, string context)
            : this(claimsPrincipal, context, DateTime.UtcNow, DateTimeUtil.AddNonNegative(DateTime.UtcNow, SessionSecurityTokenHandler.DefaultTokenLifetime))
        { }

        /// <summary>
        /// Create session security token from principal and bootstrap token.
        /// </summary>
        /// <param name="claimsPrincipal">The <see cref="ClaimsPrincipal"/> that generated from the bootstrap token.</param>
        /// <param name="context">Session specific context string</param>
        /// <param name="validFrom">DateTime specifying the time the token becomes valid.</param>
        /// <param name="validTo">DateTime specifying the time the token becomes invalid.</param>
        public SessionSecurityToken(ClaimsPrincipal claimsPrincipal, string context, DateTime? validFrom, DateTime? validTo)
            : this(claimsPrincipal, new SysUniqueId(), context, String.Empty, validFrom, validTo, null)
        { }

        /// <summary>
        /// Create session security token from principal and bootstrap token.
        /// </summary>
        /// <param name="claimsPrincipal">The <see cref="ClaimsPrincipal"/> that generated from the bootstrap token.</param>
        /// <param name="context">Session specific context string</param>
        /// <param name="endpointId">The endpoint to which this token is bound. String.Empty would create a unscoped token.</param>
        /// <param name="validFrom">DateTime specifying the time the token becomes valid.</param>
        /// <param name="validTo">DateTime specifying the time the token becomes invalid.</param>
        public SessionSecurityToken(ClaimsPrincipal claimsPrincipal, string context, string endpointId, DateTime? validFrom, DateTime? validTo)
            : this(claimsPrincipal, new SysUniqueId(), context, endpointId, validFrom, validTo, null)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="SessionSecurityToken"/> class.
        /// </summary>
        /// <param name="claimsPrincipal"><see cref="ClaimsPrincipal"/> associated with this session.</param>
        /// <param name="contextId">Optional context identifier associated with this token.  If null a new identifier will be generated.</param>
        /// <param name="context">Optional context information associated with the session.</param>
        /// <param name="endpointId">The endpoint to which this token is bound. String.Empty would create a unscoped token.</param>
        /// <param name="lifetime">The lifetime of the session token.  ValidFrom will be set to DateTime.UtcNow, ValidTo will be set to ValidFrom + lifetime.</param>
        /// <param name="key">Optional symmetric session key.</param>
        /// <exception cref="InvalidOperationException">The value of lifetime &lt;= TimeSpan.Zero."</exception>
        public SessionSecurityToken(ClaimsPrincipal claimsPrincipal,
                                     SysUniqueId contextId,
                                     string context,
                                     string endpointId,
                                     TimeSpan lifetime,
                                     SymmetricSecurityKey key)
            : this(claimsPrincipal, contextId, context, endpointId, DateTime.UtcNow, lifetime, key)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SessionSecurityToken"/> class.
        /// </summary>
        /// <param name="claimsPrincipal"><see cref="ClaimsPrincipal"/> associated with this session.</param>
        /// <param name="contextId">Optional context identifier associated with this token.  If null a new identifier will be generated.</param>
        /// <param name="context">Optional context information associated with the session.</param>
        /// <param name="endpointId">The endpoint to which this token is bound. String.Empty would create a unscoped token.</param>
        /// <param name="validFrom">DateTime specifying the time the token becomes valid.</param>
        /// <param name="lifetime">The lifetime of the session token.  ValidTo will be set to ValidFrom + lifetime.</param>
        /// <param name="key">Optional symmetric session key.</param>
        /// <exception cref="InvalidOperationException">The value of lifetime &lt;= TimeSpan.Zero."</exception>
        public SessionSecurityToken(ClaimsPrincipal claimsPrincipal,
                                     SysUniqueId contextId,
                                     string context,
                                     string endpointId,
                                     DateTime validFrom,
                                     TimeSpan lifetime,
                                     SymmetricSecurityKey key)
            : this(claimsPrincipal, contextId, context, endpointId, validFrom, DateTimeUtil.AddNonNegative(validFrom, lifetime), key)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SessionSecurityToken"/> class.
        /// </summary>
        /// <param name="claimsPrincipal"><see cref="ClaimsPrincipal"/> associated with this session.</param>
        /// <param name="contextId">Context Identifier that identifies the session</param>
        /// <param name="context">Optional context information associated with the session.</param>
        /// <param name="endpointId">The endpoint to which this token is bound. String.Empty would create a unscoped token.</param>
        /// <param name="validFrom">DateTime specifying the time the token becomes valid.</param>
        /// <param name="validTo">DateTime specifying the time the token becomes invalid.</param>
        /// <param name="key">Optional symmetric session key.</param>
        /// <exception cref="ArgumentNullException">The input parameter 'claimsPrincipal' is null.</exception>
        /// <exception cref="ArgumentNullException">The input parameter 'contextId' is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">validFrom is greater than or equal to validTo.</exception>
        /// <exception cref="ArgumentOutOfRangeException">validTo is less than current time.</exception>
        /// <remarks>
        /// If no key is supplied, a 128bit key is generated. KeyEffectiveTime is set to validFrom, KeyExpirationTime is set to validTo.
        /// A key generation identifier is created.
        /// </remarks>
        public SessionSecurityToken(ClaimsPrincipal claimsPrincipal,
                                     SysUniqueId contextId,
                                     string context,
                                     string endpointId,
                                     DateTime? validFrom,
                                     DateTime? validTo,
                                     SymmetricSecurityKey key)
            : this(claimsPrincipal, contextId, System.IdentityModel.UniqueId.CreateUniqueId(), context, key == null ? null : key.GetSymmetricKey(), endpointId, validFrom, validTo, null, validFrom, validTo, null, null)
        {
        }

        /// <summary>
        /// Core ctor with all parameters in their most primitive form.  This constructor is used in deserialization and
        /// when generating a wrapper SessionSecurityToken from a SecurityContextSecurityToken.
        /// </summary>
        /// <param name="contextId">Context identifier</param>
        /// <param name="id">Token identifier</param>
        /// <param name="context">Session context data</param>
        /// <param name="endpointId">The endpoint to which this token is bound. String.Empty would create a unscoped token.</param>
        /// <param name="key">Key material</param>
        /// <param name="validFrom">Start time</param>
        /// <param name="validTo">End time</param>
        /// <param name="keyGeneration">Key Generation identifier</param>
        /// <param name="keyEffectiveTime">Key start time</param>
        /// <param name="keyExpirationTime">Key end time</param>
        /// <param name="authorizationPolicies">Authorization policies</param>
        /// <param name="securityContextSecurityTokenWrapperSecureConversationVersion">The version of
        /// WS-SecureConversation used to generate this SCT.  This should be null if the token is not an SCT wrapper.</param>
        internal SessionSecurityToken(ClaimsPrincipal claimsPrincipal,
                                       SysUniqueId contextId,
                                       string id,
                                       string context,
                                       byte[] key,
                                       string endpointId,
                                       DateTime? validFrom,
                                       DateTime? validTo,
                                       SysUniqueId keyGeneration,
                                       DateTime? keyEffectiveTime,
                                       DateTime? keyExpirationTime,
                                       SctAuthorizationPolicy sctAuthorizationPolicy,
                                       Uri securityContextSecurityTokenWrapperSecureConversationVersion)
            : base()
        {
            if (claimsPrincipal == null || claimsPrincipal.Identities == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("claimsPrincipal");
            }

            if (contextId == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("contextId");
            }

            //
            // validFrom and validTo may not have values.
            // need to set them to reasonable defaults before moving forward.
            // SecurityContextSecurityToken will check the values, but I choose to check
            // here to keep the exception in our stack.
            //

            DateTime validFromEffective;
            DateTime validToEffective;

            if (validFrom.HasValue)
            {
                validFromEffective = DateTimeUtil.ToUniversalTime(validFrom.Value);
            }
            else
            {
                validFromEffective = DateTime.UtcNow;
            }

            if (validTo.HasValue)
            {
                validToEffective = DateTimeUtil.ToUniversalTime(validTo.Value);
            }
            else
            {
                validToEffective = DateTimeUtil.Add(validFromEffective, SessionSecurityTokenHandler.DefaultTokenLifetime);
            }

            if (validFromEffective >= validToEffective)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("validFrom"));
            }

            if (validToEffective < DateTime.UtcNow)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("validTo"));
            }

            if (endpointId == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("endpointId");
            }

            if (!keyEffectiveTime.HasValue)
            {
                keyEffectiveTime = (DateTime?)validFromEffective;
            }

            if (!keyExpirationTime.HasValue)
            {
                keyExpirationTime = (DateTime?)validToEffective;
            }

            if (keyEffectiveTime.Value > keyExpirationTime.Value || keyEffectiveTime.Value < validFromEffective)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("keyEffectiveTime"));
            }

            if (keyExpirationTime.Value > validToEffective)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("keyExpirationTime"));
            }

            if (securityContextSecurityTokenWrapperSecureConversationVersion == null)
            {
                // Not an SCT wrapper: use the default namespace.
                _secureConversationVersion = WSSecureConversation13Constants.NamespaceUri;
            }
            else
            {
                // SCT wrapper: use the provided namespace.
                _isSecurityContextSecurityTokenWrapper = true;
                _secureConversationVersion = securityContextSecurityTokenWrapperSecureConversationVersion;
            }

            if (key == null)
            {
                // We have to create a dummy key here. We are not in WCF and we will
                // never use this key. But this is created only to satisfy WCF's
                // SecurityContextSecurityToken constructor.
                key = CryptoHelper.KeyGenerator.GenerateSymmetricKey(128);
            }

            if (endpointId == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("endpointId");
            }

            _claimsPrincipal = claimsPrincipal;
            _contextId = contextId;
            _id = id;
            _context = context;
            _securityKeys = new ReadOnlyCollection<SecurityKey>(new SecurityKey[] { new InMemorySymmetricSecurityKey(key) });
            _endpointId = endpointId;
            _validFrom = validFrom.Value;
            _validTo = validTo.Value;
            _keyGeneration = keyGeneration;
            _keyEffectiveTime = keyEffectiveTime.Value;
            _keyExpirationTime = keyExpirationTime.Value;
            _sctAuthorizationPolicy = sctAuthorizationPolicy;
        }

        protected SessionSecurityToken(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
                return;

            byte[] cookie = (byte[])info.GetValue(tokenKey, typeof(byte[]));

            if (null == cookie || 0 == cookie.Length)
            {
                throw DiagnosticUtility.ThrowHelperInvalidOperation(SR.GetString(SR.ID4272));
            }

            SessionDictionary dictionary = SessionDictionary.Instance;
            //
            // We are creating a reader over the decrypted form of the cookie that is in memory. 
            // Passing Max for the XmlDictionaryReaderQuotas is safe.
            //
            using (XmlDictionaryReader reader = XmlDictionaryReader.CreateBinaryReader(cookie, 0, cookie.Length, dictionary, XmlDictionaryReaderQuotas.Max, null, null))
            {
                //
                // Layout is strict, must be in following order:
                //
                // Version
                // SecureConversationVersion
                // ID
                // ContextID
                // Key
                // Generation {optional}
                // EffectiveTime
                // ExpiryTime
                // KeyEffectiveTime
                // KeyExpiryTime
                //
                // SessionSecurityToken data may follow, in the order:
                //
                bool isSecurityContextSecurityTokenWrapper = false;
                bool isPersistent = true;
                bool isReferenceMode = false;
                string cookieContext = String.Empty;

                if (reader.IsStartElement(dictionary.SecurityContextToken, dictionary.EmptyString))
                {
                    isSecurityContextSecurityTokenWrapper = true;
                }
                else if (reader.IsStartElement(dictionary.SessionToken, dictionary.EmptyString))
                {
                    //@PersistentTrue
                    if (reader.GetAttribute(dictionary.PersistentTrue, dictionary.EmptyString) == null)
                    {
                        isPersistent = false;
                    }

                    if (reader.GetAttribute(dictionary.ReferenceModeTrue, dictionary.EmptyString) != null)
                    {
                        isReferenceMode = true;
                    }

                    reader.ReadFullStartElement();
                    reader.MoveToContent();

                    // <Context>
                    if (reader.IsStartElement(dictionary.Context, dictionary.EmptyString))
                    {
                        cookieContext = reader.ReadElementContentAsString();
                    }
                }
                else
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(SR.GetString(SR.ID4230, dictionary.SecurityContextToken.Value, reader.Name)));
                }

                string version = reader.ReadElementString();
                if (version != SupportedVersion)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(SR.GetString(SR.ID4232, version, SupportedVersion)));
                }

                //
                // SecureConversation Version
                //
                string scNamespace = reader.ReadElementString();
                Uri scVersion;

                if (scNamespace == WSSecureConversationFeb2005Constants.Namespace)
                {
                    scVersion = WSSecureConversationFeb2005Constants.NamespaceUri;
                }
                else if (scNamespace == WSSecureConversation13Constants.Namespace)
                {
                    scVersion = WSSecureConversation13Constants.NamespaceUri;
                }
                else
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(SR.GetString(SR.ID4232, version, SupportedVersion)));
                }

                string instanceIdentifier = null;
                if (reader.IsStartElement(dictionary.Id, dictionary.EmptyString))
                {
                    instanceIdentifier = reader.ReadElementString();
                }

                if (string.IsNullOrEmpty(instanceIdentifier))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.ID4239, dictionary.Id.Value)));
                }

                if (!reader.IsStartElement(dictionary.ContextId, dictionary.EmptyString))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(SR.GetString(SR.ID4230, dictionary.ContextId.Value, reader.Name)));
                }

                SysUniqueId contextIdentifier = reader.ReadElementContentAsUniqueId();

                if (!reader.IsStartElement(dictionary.Key, dictionary.EmptyString))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(SR.GetString(SR.ID4230, dictionary.Key.Value, reader.Name)));
                }
                byte[] key = reader.ReadElementContentAsBase64();

                // optional
                SysUniqueId generation = null;
                if (reader.IsStartElement(dictionary.KeyGeneration, dictionary.EmptyString))
                {
                    generation = reader.ReadElementContentAsUniqueId();
                }

                if (!reader.IsStartElement(dictionary.EffectiveTime, dictionary.EmptyString))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(SR.GetString(SR.ID4230, dictionary.EffectiveTime.Value, reader.Name)));
                }
                DateTime effectiveTime = new DateTime(XmlUtil.ReadElementContentAsInt64(reader), DateTimeKind.Utc);

                if (!reader.IsStartElement(dictionary.ExpiryTime, dictionary.EmptyString))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(SR.GetString(SR.ID4230, dictionary.ExpiryTime.Value, reader.Name)));
                }
                DateTime expiryTime = new DateTime(XmlUtil.ReadElementContentAsInt64(reader), DateTimeKind.Utc);

                if (!reader.IsStartElement(dictionary.KeyEffectiveTime, dictionary.EmptyString))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(SR.GetString(SR.ID4230, dictionary.KeyEffectiveTime.Value, reader.Name)));
                }
                DateTime keyEffectiveTime = new DateTime(XmlUtil.ReadElementContentAsInt64(reader), DateTimeKind.Utc);

                if (!reader.IsStartElement(dictionary.KeyExpiryTime, dictionary.EmptyString))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(SR.GetString(SR.ID4230, dictionary.KeyExpiryTime.Value, reader.Name)));
                }
                DateTime keyExpiryTime = new DateTime(XmlUtil.ReadElementContentAsInt64(reader), DateTimeKind.Utc);

                ClaimsPrincipal principal = null;

                if (reader.IsStartElement(dictionary.ClaimsPrincipal, dictionary.EmptyString))
                {
                    principal = ReadPrincipal(reader, dictionary);
                }

                SctAuthorizationPolicy sctAuthorizationPolicy = null;                
                if (reader.IsStartElement(dictionary.SctAuthorizationPolicy, dictionary.EmptyString))
                {
                    reader.ReadStartElement(dictionary.SctAuthorizationPolicy, dictionary.EmptyString);
                    SysClaim sysClaim = DeserializeSysClaim(reader);
                    reader.ReadEndElement();
                    sctAuthorizationPolicy = new SctAuthorizationPolicy(sysClaim);
                }

                string endpointId = null;
                if (reader.IsStartElement(dictionary.EndpointId, dictionary.EmptyString))
                {
                    endpointId = reader.ReadElementContentAsString();
                }

                reader.ReadEndElement();

                _claimsPrincipal = principal;
                _contextId = contextIdentifier;
                _id = instanceIdentifier;
                _context = cookieContext;
                _securityKeys = new ReadOnlyCollection<SecurityKey>(new SecurityKey[] { new InMemorySymmetricSecurityKey(key) });
                _endpointId = endpointId;
                _validFrom = effectiveTime;
                _validTo = expiryTime;
                _keyGeneration = generation;
                _keyEffectiveTime = keyEffectiveTime;
                _keyExpirationTime = keyExpiryTime;
                _isSecurityContextSecurityTokenWrapper = isSecurityContextSecurityTokenWrapper;
                _secureConversationVersion = scVersion;
                _sctAuthorizationPolicy = sctAuthorizationPolicy;
                _isPersistent = isPersistent;
                _isReferenceMode = isReferenceMode;

            }
        }

        /// <summary>
        /// The <see cref="ClaimsPrincipal"/> associated with the session.
        /// </summary>
        public ClaimsPrincipal ClaimsPrincipal
        {
            get
            {
                return _claimsPrincipal;
            }
        }

        /// <summary>
        /// Gets the user specified value.
        /// </summary>
        public string Context
        {
            get { return _context; }
        }

        /// <summary>
        /// The Session Context Identifier
        /// </summary>
        public SysUniqueId ContextId
        {
            get { return _contextId; }
        }

        /// <summary>
        /// Gets the Id of the endpoint to which this token is scoped.
        /// </summary>
        public string EndpointId
        {
            get { return _endpointId; }
        }

        /// <summary>
        /// Gets a value indicating whether this token is wrapping an SCT.
        /// </summary>
        internal bool IsSecurityContextSecurityTokenWrapper
        {
            get { return _isSecurityContextSecurityTokenWrapper; }
        }

        /// <summary>
        /// The effective date/time of the key in this token
        /// </summary>
        public DateTime KeyEffectiveTime
        {
            get { return _keyEffectiveTime; }
        }

        /// <summary>
        /// The expiration date/time of the key in this token
        /// </summary>
        public DateTime KeyExpirationTime
        {
            get { return _keyExpirationTime; }
        }

        /// <summary>
        /// The identifier for the key generation in this token
        /// </summary>
        public SysUniqueId KeyGeneration
        {
            get { return _keyGeneration; }
        }

        /// <summary>
        /// Gets the id of this token.
        /// </summary>
        public override string Id
        {
            get { return _id; }
        }

        /// <summary>
        /// If true, cookie is written as a persistent cookie.
        /// </summary>
        public bool IsPersistent
        {
            get { return _isPersistent; }
            set { _isPersistent = value; }
        }

        /// <summary>
        /// If true, the SessionSecurityToken is operating in reference mode.
        /// </summary>
        /// <remarks>
        /// In reference mode, a simple artifact is produced during serialization
        /// and the real token is stored in the token cache associated with the
        /// token handler. For Web Farm scenarios, the token cache must operate
        /// across all nodes in teh farm.
        /// </remarks>
        public bool IsReferenceMode
        {
            get
            {
                return _isReferenceMode;
            }
            set
            {
                _isReferenceMode = value;
            }
        }

        /// <summary>
        /// Gets the authorization policies associated with the session. Only has meaning if this is wrapping an SCT
        /// token.
        /// </summary>
        internal SctAuthorizationPolicy SctAuthorizationPolicy
        {
            get { return _sctAuthorizationPolicy; }
        }

        /// <summary>
        /// Gets the SecureConversationVersion used for this token.
        /// </summary>
        public Uri SecureConversationVersion
        {
            get { return _secureConversationVersion; }
        }

        /// <summary>
        /// Gets the keys associated with this session, usually a single key
        /// </summary>
        public override ReadOnlyCollection<SecurityKey> SecurityKeys
        {
            get { return _securityKeys; }
        }

        /// <summary>
        /// Gets the begining DateTime before which token is invalid.
        /// </summary>
        public override DateTime ValidFrom
        {
            get { return _validFrom; }
        }

        /// <summary>
        /// Gets the ending DateTime after which the token is invalid.
        /// </summary>
        public override DateTime ValidTo
        {
            get { return _validTo; }
        }

        #region ISerializable Members

        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            MemoryStream stream = new MemoryStream();
            SessionDictionary dictionary = SessionDictionary.Instance;

            //
            // XmlDictionaryWriter.CreateBinaryWriter() defaults to ownsStream=true. 
            // So, the XmlWriter returned by the below code owns the memory stream, and will dispose it.
            //
            using (XmlDictionaryWriter dicWriter = XmlDictionaryWriter.CreateBinaryWriter(stream, dictionary))
            {
                //<SecurityContextSecurityToken> or <SessionSecurityToken>
                if (this.IsSecurityContextSecurityTokenWrapper)
                {
                    dicWriter.WriteStartElement(dictionary.SecurityContextToken, dictionary.EmptyString);
                }
                else
                {
                    dicWriter.WriteStartElement(dictionary.SessionToken, dictionary.EmptyString);

                    // @PersistentTrue
                    if (this.IsPersistent)
                    {
                        dicWriter.WriteAttributeString(dictionary.PersistentTrue, dictionary.EmptyString, "");
                    }

                    // @ReferenceModeTrue
                    if (this.IsReferenceMode)
                    {
                        dicWriter.WriteAttributeString(dictionary.ReferenceModeTrue, dictionary.EmptyString, "");
                    }

                    // <Context>
                    if (!string.IsNullOrEmpty(this.Context))
                    {
                        dicWriter.WriteElementString(dictionary.Context, dictionary.EmptyString, this.Context);
                    }
                }
                
                // Serialization Format Version
                // <Version>1</Version>
                dicWriter.WriteStartElement(dictionary.Version, dictionary.EmptyString);
                dicWriter.WriteValue(SupportedVersion);
                dicWriter.WriteEndElement();

                //
                // SecureConversation Version
                //
                dicWriter.WriteElementString(dictionary.SecureConversationVersion, dictionary.EmptyString, this.SecureConversationVersion.AbsoluteUri);

                //
                // ID and ContextId
                //
                dicWriter.WriteElementString(dictionary.Id, dictionary.EmptyString, this.Id);
                XmlUtil.WriteElementStringAsUniqueId(dicWriter, dictionary.ContextId, dictionary.EmptyString, this.ContextId.ToString());

                //
                // Key material
                //
                byte[] key = ((SymmetricSecurityKey)this.SecurityKeys[0]).GetSymmetricKey();

                dicWriter.WriteStartElement(dictionary.Key, dictionary.EmptyString);
                dicWriter.WriteBase64(key, 0, key.Length);
                dicWriter.WriteEndElement();

                //
                // Key Generation
                //
                if (this.KeyGeneration != null)
                {
                    XmlUtil.WriteElementStringAsUniqueId(dicWriter, dictionary.KeyGeneration, dictionary.EmptyString, this.KeyGeneration.ToString());
                }

                //
                // Effective and Expiry dates
                //
                XmlUtil.WriteElementContentAsInt64(dicWriter, dictionary.EffectiveTime, dictionary.EmptyString, this.ValidFrom.ToUniversalTime().Ticks);
                XmlUtil.WriteElementContentAsInt64(dicWriter, dictionary.ExpiryTime, dictionary.EmptyString, this.ValidTo.ToUniversalTime().Ticks);
                XmlUtil.WriteElementContentAsInt64(dicWriter, dictionary.KeyEffectiveTime, dictionary.EmptyString, this.KeyEffectiveTime.ToUniversalTime().Ticks);
                XmlUtil.WriteElementContentAsInt64(dicWriter, dictionary.KeyExpiryTime, dictionary.EmptyString, this.KeyExpirationTime.ToUniversalTime().Ticks);

                //
                // Claims Principal
                //
                WritePrincipal(dicWriter, dictionary, this.ClaimsPrincipal);

                // The WCF SCT will have a SctAuthorizationPolicy that wraps the Primary Identity
                // of the bootstrap token. This is required for SCT renewal scenarios. Write the 
                // SctAuthorizationPolicy if one is available.
                if (this.SctAuthorizationPolicy != null)
                {
                    dicWriter.WriteStartElement(dictionary.SctAuthorizationPolicy, dictionary.EmptyString);
                    SysClaim identityClaim = ((System.IdentityModel.Claims.DefaultClaimSet)((IAuthorizationPolicy)this.SctAuthorizationPolicy).Issuer)[0];
                    SerializeSysClaim(identityClaim, dicWriter);
                    dicWriter.WriteEndElement();
                }

                dicWriter.WriteElementString(dictionary.EndpointId, dictionary.EmptyString, this.EndpointId);
                dicWriter.WriteEndElement();
                dicWriter.Flush();

                info.AddValue(tokenKey, stream.ToArray());
            }
        }

        #endregion

        /// <summary>
        /// Reads a ClaimsPrincipal from a XmlDictionaryReader.
        /// </summary>
        /// <param name="dictionaryReader">XmlDictionaryReader positioned at dictionary.ClaimsPrincipal.</param>
        /// <param name="dictionary">SessionDictionary to provide dictionary strings.</param>
        /// <exception cref="ArgumentNullException">The input argument 'dictionaryReader' or 'dictionary' is null.</exception>
        /// <returns>ClaimsPrincipal</returns>
        ClaimsPrincipal ReadPrincipal(XmlDictionaryReader dictionaryReader, SessionDictionary dictionary)
        {
            if (dictionaryReader == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("dictionaryReader");
            }

            if (dictionary == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("dictionary");
            }

            ClaimsPrincipal principal = null;

            Collection<ClaimsIdentity> identities = new Collection<ClaimsIdentity>();

            dictionaryReader.MoveToContent();

            if (dictionaryReader.IsStartElement(dictionary.ClaimsPrincipal, dictionary.EmptyString))
            {
                dictionaryReader.ReadFullStartElement();

                ReadIdentities(dictionaryReader, dictionary, identities);

                dictionaryReader.ReadEndElement();
            }

            // If we find a WindowsIdentity in the identities we just read, we should be creating a WindowsPrincipal using it
            WindowsIdentity wi = null;
            foreach (ClaimsIdentity identity in identities)
            {
                wi = identity as WindowsIdentity;
                if (wi != null)
                {
                    principal = new WindowsPrincipal(wi);
                    break;
                }
            }

            // If we did create a WindowsPrincipal we can remove the associated WindowsIdentity from the identities collection
            // so that we dont add it twice in the subsequent step
            if (principal != null)
            {
                identities.Remove(wi);
            }
            else if (identities.Count > 0)
            {
                // If we did not create a WindowsPrincipal, default to a ClaimsPrincipal
                principal = new ClaimsPrincipal();
            }

            if (principal != null)
            {
                // Add the identities we just read to the principal
                principal.AddIdentities(identities);
            }

            return principal;
        }

        /// <summary>
        /// Reads the ClaimsIdentites from a XmlDictionaryReader and adds them to a ClaimIdentityColleciton.
        /// </summary>
        /// <param name="dictionaryReader">XmlDictionaryReader positioned at dictionary.Identities</param>
        /// <param name="dictionary">SessionDictionary to provide dictionary strings.</param>
        /// <param name="identities">A collection of <see cref="ClaimsIdentity"/> to populate.</param>
        /// <exception cref="ArgumentNullException">The input argument 'dictionaryReader', 'dictionary' or 'identities' is null.</exception>
        /// <remarks>Reads 'n' identies and adds them to identies.</remarks>
        void ReadIdentities(XmlDictionaryReader dictionaryReader, SessionDictionary dictionary, Collection<ClaimsIdentity> identities)
        {
            if (dictionaryReader == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("dictionaryReader");
            }

            if (dictionary == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("dictionary");
            }

            if (identities == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("identities");
            }

            dictionaryReader.MoveToContent();

            if (dictionaryReader.IsStartElement(dictionary.Identities, dictionary.EmptyString))
            {
                dictionaryReader.ReadFullStartElement();

                while (dictionaryReader.IsStartElement(dictionary.Identity, dictionary.EmptyString))
                {
                    identities.Add(ReadIdentity(dictionaryReader, dictionary));
                }

                dictionaryReader.ReadEndElement();
            }
        }

        /// <summary>
        /// Reads a single ClaimsIdentity from a XmlDictionaryReader.
        /// </summary>
        /// <param name="dictionaryReader">XmlDictionaryReader positioned at dictionary.Identity.</param>
        /// <param name="dictionary">SessionDictionary to provide dictionary strings.</param>
        /// <exception cref="ArgumentNullException">The input argument 'dictionaryReader' or 'dictionary' is null.</exception>
        /// <exception cref="SecurityTokenException">The dictionaryReader is not positioned a SessionDictionary.Identity.</exception>
        /// <returns>ClaimsIdentity</returns>
        ClaimsIdentity ReadIdentity(XmlDictionaryReader dictionaryReader, SessionDictionary dictionary)
        {
            if (dictionaryReader == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("dictionaryReader");
            }

            if (dictionary == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("dictionary");
            }

            dictionaryReader.MoveToContent();

            ClaimsIdentity identity = null;

            if (!dictionaryReader.IsStartElement(dictionary.Identity, dictionary.EmptyString))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(SR.GetString(SR.ID3007, dictionaryReader.LocalName, dictionaryReader.NamespaceURI)));
            }

            // @NameClaimType
            string nameClaimType = dictionaryReader.GetAttribute(dictionary.NameClaimType, dictionary.EmptyString);

            // @RoleClaimType
            string roleClaimType = dictionaryReader.GetAttribute(dictionary.RoleClaimType, dictionary.EmptyString);

            // @WindowsLogonName (optional) => windows claims identity
            string logonName = dictionaryReader.GetAttribute(dictionary.WindowsLogonName, dictionary.EmptyString);
            string authenticationType = dictionaryReader.GetAttribute(dictionary.AuthenticationType, dictionary.EmptyString);

            if (string.IsNullOrEmpty(logonName))
            {
                identity = new ClaimsIdentity(authenticationType, nameClaimType, roleClaimType);
            }
            else
            {
                // The WindowsIdentity(string, string) c'tor does not set the Auth type. Hence we use that c'tor to get a intPtr and 
                // call the other c'tor that actually sets the authType passed in. 
                // DevDiv 279196 tracks the issue and in WindowsIdentity c'tor. Its too late to fix it in 4.5 cycle as we are in Beta and would not be
                // able to complete the analysis of the change for the current release. This should be investigated in 5.0
                WindowsIdentity winId = new WindowsIdentity(GetUpn(logonName));
                identity = new WindowsIdentity(winId.Token, authenticationType);
            }

            // @Label
            identity.Label = dictionaryReader.GetAttribute(dictionary.Label, dictionary.EmptyString);


            dictionaryReader.ReadFullStartElement();

            // <ClaimCollection>
            if (dictionaryReader.IsStartElement(dictionary.ClaimCollection, dictionary.EmptyString))
            {
                dictionaryReader.ReadStartElement();

                Collection<Claim> claims = new Collection<Claim>();
                ReadClaims(dictionaryReader, dictionary, claims);
                identity.AddClaims(claims);

                dictionaryReader.ReadEndElement();
            }

            // <Actor>
            if (dictionaryReader.IsStartElement(dictionary.Actor, dictionary.EmptyString))
            {
                dictionaryReader.ReadStartElement();

                identity.Actor = ReadIdentity(dictionaryReader, dictionary);

                dictionaryReader.ReadEndElement();
            }

            if (dictionaryReader.IsStartElement(dictionary.BootstrapToken, dictionary.EmptyString))
            {
                dictionaryReader.ReadStartElement();

                byte[] bytes = dictionaryReader.ReadContentAsBase64();
                using (MemoryStream ms = new MemoryStream(bytes))
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    identity.BootstrapContext = (BootstrapContext)formatter.Deserialize(ms);
                }

                dictionaryReader.ReadEndElement();
            }

            dictionaryReader.ReadEndElement(); // Identity

            return identity;
        }

        /// <summary>
        /// Returns a User Principal Name from a windows logon name.
        /// </summary>
        /// <param name="windowsLogonName">Name to translate into the UPN</param>
        /// <exception cref="ArgumentNullException">The input argument 'windowsLogonName' is null or empty.</exception>
        /// <exception cref="InvalidOperationException">If 'windowsLogonName' is not of the form domain\\user or user@domain.</exception>
        /// <returns>A User Principal Name of the form 'user@domain'</returns>
        string GetUpn(string windowsLogonName)
        {
            if (string.IsNullOrEmpty(windowsLogonName))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("windowsLogonName");
            }
            int delimiterPos = windowsLogonName.IndexOf('\\');

            if ((delimiterPos < 0) || (delimiterPos == 0) || (delimiterPos == windowsLogonName.Length - 1))
            {
                if (IsPossibleUpn(windowsLogonName))
                {
                    return windowsLogonName;
                }

                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.ID4248, windowsLogonName)));
            }

            string shortDomainName = windowsLogonName.Substring(0, delimiterPos + 1);
            string userName = windowsLogonName.Substring(delimiterPos + 1);
            string fullDomainName;
            bool found;

            // 1) Read from cache
            lock (DomainNameMap)
            {
                found = DomainNameMap.TryGetValue(shortDomainName, out fullDomainName);
            }

            // 2) Not found, do expensive look up
            if (!found)
            {
                uint capacity = 50;
                StringBuilder fullyQualifiedDomainName = new StringBuilder((int)capacity);
                if (!NativeMethods.TranslateName(shortDomainName, EXTENDED_NAME_FORMAT.NameSamCompatible, EXTENDED_NAME_FORMAT.NameCanonical,
                    fullyQualifiedDomainName, out capacity))
                {
                    int errorCode = Marshal.GetLastWin32Error();
                    if (errorCode == (int)Win32Error.ERROR_INSUFFICIENT_BUFFER)
                    {
                        fullyQualifiedDomainName = new StringBuilder((int)capacity);
                        if (!NativeMethods.TranslateName(shortDomainName, EXTENDED_NAME_FORMAT.NameSamCompatible, EXTENDED_NAME_FORMAT.NameCanonical,
                            fullyQualifiedDomainName, out capacity))
                        {
                            errorCode = Marshal.GetLastWin32Error();
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.ID4248, windowsLogonName), new Win32Exception(errorCode)));
                        }
                    }
                    else
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.ID4248, windowsLogonName), new Win32Exception(errorCode)));
                    }
                }
                // trim the trailing / from fqdn
                fullyQualifiedDomainName = fullyQualifiedDomainName.Remove(fullyQualifiedDomainName.Length - 1, 1);
                fullDomainName = fullyQualifiedDomainName.ToString();

                // 3) Save in cache (remove a random item if cache is full)
                lock (DomainNameMap)
                {
                    if (DomainNameMap.Count >= MaxDomainNameMapSize)
                    {
                        if (rnd == null)
                        {
                            rnd = new Random(unchecked((int)DateTime.Now.Ticks));
                        }
                        int victim = rnd.Next() % DomainNameMap.Count;
                        foreach (string key in DomainNameMap.Keys)
                        {
                            if (victim <= 0)
                            {
                                DomainNameMap.Remove(key);
                                break;
                            }
                            --victim;
                        }
                    }
                    DomainNameMap[shortDomainName] = fullDomainName;
                }
            }

            return userName + "@" + fullDomainName;
        }
        
        /// <summary>
        /// Reads Claims from a XmlDictionaryReader and adds them to a ClaimCollection.
        /// </summary>
        /// <param name="dictionaryReader">XmlDictionaryReader positioned at dictionary.Claim.</param>
        /// <param name="dictionary">SessionDictionary to provide dictionary strings.</param>
        /// <param name="claims">ClaimCollection to add the claims to.</param>
        /// <exception cref="ArgumentNullException">The input argument 'dictionaryReader',  'dictionary' or 'claims' is null.</exception>
        /// <remarks>Reads 'n' claims and adds them to claims.</remarks>
        void ReadClaims(XmlDictionaryReader dictionaryReader, SessionDictionary dictionary, Collection<Claim> claims)
        {
            if (dictionaryReader == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("dictionaryReader");
            }

            if (dictionary == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("dictionary");
            }

            if (claims == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("claims");
            }

            while (dictionaryReader.IsStartElement(dictionary.Claim, dictionary.EmptyString))
            {
                // @Issuer (optional), @OriginalIssuer (optional), @Type, @Value, @ValueType

                Claim claim = new Claim(dictionaryReader.GetAttribute(dictionary.Type, dictionary.EmptyString),
                                         dictionaryReader.GetAttribute(dictionary.Value, dictionary.EmptyString),
                                         dictionaryReader.GetAttribute(dictionary.ValueType, dictionary.EmptyString),
                                         dictionaryReader.GetAttribute(dictionary.Issuer, dictionary.EmptyString),
                                         dictionaryReader.GetAttribute(dictionary.OriginalIssuer, dictionary.EmptyString));

                dictionaryReader.ReadFullStartElement();

                // <Properties> (optional)
                if (dictionaryReader.IsStartElement(dictionary.ClaimProperties, dictionary.EmptyString))
                {
                    ReadClaimProperties(dictionaryReader, dictionary, claim.Properties);
                }

                dictionaryReader.ReadEndElement();

                claims.Add(claim);
            }
        }

        /// <summary>
        /// Reads ClaimProperties from a XmlDictionaryReader and adds them to a Dictionary.
        /// </summary>
        /// <param name="dictionaryReader">XmlDictionaryReader positioned at the element dictionary.ClaimProperties </param>
        /// <param name="dictionary">SessionDictionary to provide dictionary strings.</param>
        /// <param name="properties">Dictionary to add properties to.</param>
        /// <exception cref="ArgumentNullException">The input argument 'dictionaryReader',  'dictionary' or 'properties' is null.</exception>
        /// <exception cref="SecurityTokenException">Is thrown if the 'name' of a property is null or an empty string.</exception>
        /// <exception cref="SecurityTokenException">Is thrown if the 'value' of a property is null.</exception>
        /// <remarks>Reads 'n' properties.</remarks>
        void ReadClaimProperties(XmlDictionaryReader dictionaryReader, SessionDictionary dictionary, IDictionary<string, string> properties)
        {
            if (dictionaryReader == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("dictionaryReader");
            }

            if (dictionary == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("dictionary");
            }

            if (properties == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("properties");
            }

            dictionaryReader.ReadStartElement();

            // <Property>
            while (dictionaryReader.IsStartElement(dictionary.ClaimProperty, dictionary.EmptyString))
            {
                // @Name, @Value

                string name = dictionaryReader.GetAttribute(dictionary.ClaimPropertyName, dictionary.EmptyString);
                string value = dictionaryReader.GetAttribute(dictionary.ClaimPropertyValue, dictionary.EmptyString);

                if (string.IsNullOrEmpty(name))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(SR.GetString(SR.ID4249)));
                }

                if (string.IsNullOrEmpty(value))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(SR.GetString(SR.ID4250)));
                }

                properties.Add(new KeyValuePair<string, string>(name, value));

                dictionaryReader.ReadFullStartElement();
                dictionaryReader.ReadEndElement();
            }

            dictionaryReader.ReadEndElement();
        }

        /// <summary>
        /// Writes out a ClaimsPrincipal using a XmlDictionaryWriter.
        /// </summary>
        /// <param name="dictionaryWriter">XmlDictionaryWriter to write to.</param>
        /// <param name="dictionary">SessionDictionary to provide dictionary strings.</param>
        /// <param name="principal">ClaimsPrincipal to write.</param>
        /// <exception cref="ArgumentNullException">The input argument 'dictionaryWriter', 'dictionary' or 'principal' is null.</exception>
        void WritePrincipal(XmlDictionaryWriter dictionaryWriter, SessionDictionary dictionary, ClaimsPrincipal principal)
        {
            if (dictionaryWriter == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("dictionaryWriter");
            }

            if (dictionary == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("dictionary");
            }

            if (principal == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("principal");
            }

            // <ClaimsPrincipal>
            dictionaryWriter.WriteStartElement(dictionary.ClaimsPrincipal, dictionary.EmptyString);

            if (principal.Identities != null)
            {
                WriteIdentities(dictionaryWriter, dictionary, principal.Identities);
            }

            dictionaryWriter.WriteEndElement();
        }

        /// <summary>
        /// Writes a collection of ClaimsIdentity using a XmlDictionaryWriter.
        /// </summary>
        /// <param name="dictionaryWriter">XmlDictionaryWriter to write to.</param>
        /// <param name="dictionary">SessionDictionary to provide dictionary strings.</param>
        /// <param name="identities">The collection of ClaimsIdentity to write.</param>
        /// <exception cref="ArgumentNullException">The input argument 'dictionaryWriter', 'dictionary' or 'identities' is null.</exception>
        void WriteIdentities(XmlDictionaryWriter dictionaryWriter, SessionDictionary dictionary, IEnumerable<ClaimsIdentity> identities)
        {
            if (dictionaryWriter == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("dictionaryWriter");
            }

            if (dictionary == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("dictionary");
            }

            if (identities == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("identities");
            }

            // <Identities>
            dictionaryWriter.WriteStartElement(dictionary.Identities, dictionary.EmptyString);

            foreach (ClaimsIdentity ci in identities)
            {
                WriteIdentity(dictionaryWriter, dictionary, ci);
            }

            dictionaryWriter.WriteEndElement();
        }

        /// <summary>
        /// Writes a single ClaimsIdentity using a XmlDictionaryWriter.
        /// </summary>
        /// <param name="dictionaryWriter">XmlDictionaryWriter to write to.</param>
        /// <param name="dictionary">SessionDictionary to provide dictionary strings.</param>
        /// <param name="identity">ClaimsIdentiy to write.</param>
        /// <exception cref="ArgumentNullException">The input argument 'dictionaryWriter', 'dictionary' or 'identity' is null.</exception>
        void WriteIdentity(XmlDictionaryWriter dictionaryWriter, SessionDictionary dictionary, ClaimsIdentity identity)
        {
            if (dictionaryWriter == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("dictionaryWriter");
            }

            if (dictionary == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("dictionary");
            }

            if (identity == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("identity");
            }

            //
            // WindowsIdentity needs special handling 
            //

            // <Identity>
            dictionaryWriter.WriteStartElement(dictionary.Identity, dictionary.EmptyString);

            WindowsIdentity wci = identity as WindowsIdentity;
            if (wci != null)
            {
                // @WindowsLogonName (optional)
                dictionaryWriter.WriteAttributeString(dictionary.WindowsLogonName, dictionary.EmptyString, wci.Name);
            }

            // @AuthenticationType (optional)
            if (!String.IsNullOrEmpty(identity.AuthenticationType))
            {
                dictionaryWriter.WriteAttributeString(dictionary.AuthenticationType, dictionary.EmptyString, identity.AuthenticationType);
            }

            // @LabelWrite (optional)
            if (!String.IsNullOrEmpty(identity.Label))
            {
                dictionaryWriter.WriteAttributeString(dictionary.Label, dictionary.EmptyString, identity.Label);
            }

            // @NameClaimType (optional)
            if (identity.NameClaimType != null)
            {
                dictionaryWriter.WriteAttributeString(dictionary.NameClaimType, dictionary.EmptyString, identity.NameClaimType);
            }

            // @RoleClaimType (optional)
            if (identity.RoleClaimType != null)
            {
                dictionaryWriter.WriteAttributeString(dictionary.RoleClaimType, dictionary.EmptyString, identity.RoleClaimType);
            }

            // <ClaimCollection> (optional)
            if (identity.Claims != null)
            {
                dictionaryWriter.WriteStartElement(dictionary.ClaimCollection, dictionary.EmptyString);

                WriteClaims(dictionaryWriter, dictionary, identity.Claims, (wci == null) ?
                    (OutboundClaimsFilter)null
                    :
                    // do not serialize SID claims for WindowsIdentities as they will be created when the 
                    // windows identity is recreated. 
                    delegate(Claim c)
                    {
                        if (c.Type == ClaimTypes.GroupSid
                          || c.Type == ClaimTypes.PrimaryGroupSid
                          || c.Type == ClaimTypes.PrimarySid
                          || c.Type == ClaimTypes.DenyOnlyPrimaryGroupSid
                          || c.Type == ClaimTypes.DenyOnlyPrimarySid
                          || c.Type == ClaimTypes.Name && c.Issuer == ClaimsIdentity.DefaultIssuer && c.ValueType == ClaimValueTypes.String)
                        {
                            return true;
                        }

                        return false;
                    }
                );

                dictionaryWriter.WriteEndElement();
            }

            // <Actor> (optional)
            if (identity.Actor != null)
            {
                dictionaryWriter.WriteStartElement(dictionary.Actor, dictionary.EmptyString);

                WriteIdentity(dictionaryWriter, dictionary, identity.Actor);

                dictionaryWriter.WriteEndElement();
            }

            if (identity.BootstrapContext != null)
            {
                dictionaryWriter.WriteStartElement(dictionary.BootstrapToken, dictionary.EmptyString);
                
                using (MemoryStream ms = new MemoryStream())
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    formatter.Serialize(ms, identity.BootstrapContext);
                    byte[] bootstrapArray = ms.ToArray();
                    dictionaryWriter.WriteBase64(bootstrapArray, 0, bootstrapArray.Length);
                }

                dictionaryWriter.WriteEndElement(); // </BootstrapToken>
            }

            dictionaryWriter.WriteEndElement();

        }

        /// <summary>
        /// Actor that returns true if a claim should be filtered.
        /// </summary>
        /// <param name="claim">Claim to check.</param>
        /// <returns></returns>
        delegate bool OutboundClaimsFilter(Claim claim);

        /// <summary>
        /// Writes a collection of claims using a XmlDictionaryWriter.
        /// </summary>
        /// <param name="dictionaryWriter">XmlDictionaryWriter to write to.</param>
        /// <param name="dictionary">SessionDictionary to provide dictionary strings.</param>
        /// <param name="claims">ClaimCollection to write.</param>
        /// <param name="outboundClaimsFilter">Filter to apply when writing claims. If parameter is not null and filter returns true, claim will not be written.</param>
        /// <exception cref="ArgumentNullException">The input argument 'dictionaryWriter', 'dictionary' or 'claims' is null.</exception>
        void WriteClaims(XmlDictionaryWriter dictionaryWriter, SessionDictionary dictionary, IEnumerable<Claim> claims, OutboundClaimsFilter outboundClaimsFilter)
        {
            if (dictionaryWriter == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("dictionaryWriter");
            }

            if (dictionary == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("dictionary");
            }

            if (claims == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("claims");
            }

            foreach (Claim claim in claims)
            {
                if (claim == null)
                {
                    continue;
                }

                if (outboundClaimsFilter != null && outboundClaimsFilter(claim))
                {
                    continue;
                }

                // <Claim>
                dictionaryWriter.WriteStartElement(dictionary.Claim, dictionary.EmptyString);

                // @Issuer
                if (!String.IsNullOrEmpty(claim.Issuer))
                {
                    dictionaryWriter.WriteAttributeString(dictionary.Issuer, dictionary.EmptyString, claim.Issuer);
                }

                // @OriginalIssuer
                if (!String.IsNullOrEmpty(claim.OriginalIssuer))
                {
                    dictionaryWriter.WriteAttributeString(dictionary.OriginalIssuer, dictionary.EmptyString, claim.OriginalIssuer);
                }

                // @Type
                dictionaryWriter.WriteAttributeString(dictionary.Type, dictionary.EmptyString, claim.Type);

                // @Value
                dictionaryWriter.WriteAttributeString(dictionary.Value, dictionary.EmptyString, claim.Value);

                // @ValueType
                dictionaryWriter.WriteAttributeString(dictionary.ValueType, dictionary.EmptyString, claim.ValueType);

                // <Properties>
                if (claim.Properties != null && claim.Properties.Count > 0)
                {
                    WriteClaimProperties(dictionaryWriter, dictionary, claim.Properties);
                }

                dictionaryWriter.WriteEndElement();
            }
        }

        /// <summary>
        /// Writes a collection of ClaimProperties using a XmlDictionaryWriter. 
        /// </summary>
        /// <param name="dictionaryWriter">XmlDictionaryWriter to write to.</param>
        /// <param name="dictionary">SessionDictionary to provide dictionary strings.</param>
        /// <param name="properties">ClaimProperties to write.</param>
        /// <exception cref="ArgumentNullException">The input argument 'dictionaryWriter', 'dictionary' or 'properties' is null.</exception>
        void WriteClaimProperties(XmlDictionaryWriter dictionaryWriter, SessionDictionary dictionary, IDictionary<string, string> properties)
        {
            if (dictionaryWriter == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("dictionaryWriter");
            }

            if (dictionary == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("dictionary");
            }

            if (properties == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("properties");
            }

            if (properties.Count > 0)
            {
                dictionaryWriter.WriteStartElement(dictionary.ClaimProperties, dictionary.EmptyString);

                foreach (KeyValuePair<string, string> property in properties)
                {
                    // <ClaimProperty>
                    if (!String.IsNullOrEmpty(property.Key) && !String.IsNullOrEmpty(property.Value))
                    {
                        dictionaryWriter.WriteStartElement(dictionary.ClaimProperty, dictionary.EmptyString);
                        // @Name

                        dictionaryWriter.WriteAttributeString(dictionary.ClaimPropertyName, dictionary.EmptyString, property.Key);

                        // @Value
                        dictionaryWriter.WriteAttributeString(dictionary.ClaimPropertyValue, dictionary.EmptyString, property.Value);

                        dictionaryWriter.WriteEndElement();
                    }
                }

                dictionaryWriter.WriteEndElement();
            }
        }

        /// <summary>
        /// Serializes the given <see cref="System.IdentityModel.Claims.Claim"/> to the given XmlDictionaryWriter.
        /// </summary>
        /// <param name="claim">The claim to be serialized.</param>
        /// <param name="writer">The XmlDictionaryWriter to which to serialize the claim.</param>
        private void SerializeSysClaim(SysClaim claim, XmlDictionaryWriter writer)
        {
            SessionDictionary dictionary = SessionDictionary.Instance;

            // the order in which known claim types are checked is optimized for use patterns
            if (claim == null)
            {
                writer.WriteElementString(dictionary.NullValue, dictionary.EmptyString, string.Empty);
                return;
            }
            else if (SysClaimTypes.Sid.Equals(claim.ClaimType))
            {
                writer.WriteStartElement(dictionary.WindowsSidClaim, dictionary.EmptyString);
                WriteRightAttribute(claim, dictionary, writer);
                SerializeSid((SecurityIdentifier)claim.Resource, dictionary, writer);
                writer.WriteEndElement();
                return;
            }
            else if (SysClaimTypes.DenyOnlySid.Equals(claim.ClaimType))
            {
                writer.WriteStartElement(dictionary.DenyOnlySidClaim, dictionary.EmptyString);
                WriteRightAttribute(claim, dictionary, writer);
                SerializeSid((SecurityIdentifier)claim.Resource, dictionary, writer);
                writer.WriteEndElement();
                return;
            }
            else if (SysClaimTypes.X500DistinguishedName.Equals(claim.ClaimType))
            {
                writer.WriteStartElement(dictionary.X500DistinguishedNameClaim, dictionary.EmptyString);
                WriteRightAttribute(claim, dictionary, writer);
                byte[] rawData = ((X500DistinguishedName)claim.Resource).RawData;
                writer.WriteBase64(rawData, 0, rawData.Length);
                writer.WriteEndElement();
                return;
            }
            else if (SysClaimTypes.Thumbprint.Equals(claim.ClaimType))
            {
                writer.WriteStartElement(dictionary.X509ThumbprintClaim, dictionary.EmptyString);
                WriteRightAttribute(claim, dictionary, writer);
                byte[] thumbprint = (byte[])claim.Resource;
                writer.WriteBase64(thumbprint, 0, thumbprint.Length);
                writer.WriteEndElement();
                return;
            }
            else if (SysClaimTypes.Name.Equals(claim.ClaimType))
            {
                writer.WriteStartElement(dictionary.NameClaim, dictionary.EmptyString);
                WriteRightAttribute(claim, dictionary, writer);
                writer.WriteString((string)claim.Resource);
                writer.WriteEndElement();
                return;
            }
            else if (SysClaimTypes.Dns.Equals(claim.ClaimType))
            {
                writer.WriteStartElement(dictionary.DnsClaim, dictionary.EmptyString);
                WriteRightAttribute(claim, dictionary, writer);
                writer.WriteString((string)claim.Resource);
                writer.WriteEndElement();
                return;
            }
            else if (SysClaimTypes.Rsa.Equals(claim.ClaimType))
            {
                writer.WriteStartElement(dictionary.RsaClaim, dictionary.EmptyString);
                WriteRightAttribute(claim, dictionary, writer);
                writer.WriteString(((RSA)claim.Resource).ToXmlString(false));
                writer.WriteEndElement();
                return;
            }
            else if (SysClaimTypes.Email.Equals(claim.ClaimType))
            {
                writer.WriteStartElement(dictionary.MailAddressClaim, dictionary.EmptyString);
                WriteRightAttribute(claim, dictionary, writer);
                writer.WriteString(((System.Net.Mail.MailAddress)claim.Resource).Address);
                writer.WriteEndElement();
                return;
            }
            else if (claim == SysClaim.System)
            {
                writer.WriteElementString(dictionary.SystemClaim, dictionary.EmptyString, string.Empty);
                return;
            }
            else if (SysClaimTypes.Hash.Equals(claim.ClaimType))
            {
                writer.WriteStartElement(dictionary.HashClaim, dictionary.EmptyString);
                WriteRightAttribute(claim, dictionary, writer);
                byte[] hash = (byte[])claim.Resource;
                writer.WriteBase64(hash, 0, hash.Length);
                writer.WriteEndElement();
                return;
            }
            else if (SysClaimTypes.Spn.Equals(claim.ClaimType))
            {
                writer.WriteStartElement(dictionary.SpnClaim, dictionary.EmptyString);
                WriteRightAttribute(claim, dictionary, writer);
                writer.WriteString((string)claim.Resource);
                writer.WriteEndElement();
                return;
            }
            else if (SysClaimTypes.Upn.Equals(claim.ClaimType))
            {
                writer.WriteStartElement(dictionary.UpnClaim, dictionary.EmptyString);
                WriteRightAttribute(claim, dictionary, writer);
                writer.WriteString((string)claim.Resource);
                writer.WriteEndElement();
                return;
            }
            else if (SysClaimTypes.Uri.Equals(claim.ClaimType))
            {
                writer.WriteStartElement(dictionary.UrlClaim, dictionary.EmptyString);
                WriteRightAttribute(claim, dictionary, writer);
                writer.WriteString(((Uri)claim.Resource).AbsoluteUri);
                writer.WriteEndElement();
                return;
            }
            else
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(SR.GetString(SR.ID4290, claim)));
            }
        }

        /// <summary>
        /// Deserializes a WCF claim.
        /// </summary>
        /// <param name="reader">XmlReader to the WCF Claim.</param>
        /// <returns>Instance of <see cref="System.IdentityModel.Claims.Claim"/></returns>
        private SysClaim DeserializeSysClaim(XmlDictionaryReader reader)
        {
            SessionDictionary dictionary = SessionDictionary.Instance;

            if (reader.IsStartElement(dictionary.NullValue, dictionary.EmptyString))
            {
                reader.ReadElementString();
                return null;
            }
            else if (reader.IsStartElement(dictionary.WindowsSidClaim, dictionary.EmptyString))
            {
                string right = ReadRightAttribute(reader, dictionary);
                reader.ReadStartElement();
                byte[] sidBytes = reader.ReadContentAsBase64();
                reader.ReadEndElement();
                return new SysClaim(SysClaimTypes.Sid, new SecurityIdentifier(sidBytes, 0), right);
            }
            else if (reader.IsStartElement(dictionary.DenyOnlySidClaim, dictionary.EmptyString))
            {
                string right = ReadRightAttribute(reader, dictionary);
                reader.ReadStartElement();
                byte[] sidBytes = reader.ReadContentAsBase64();
                reader.ReadEndElement();
                return new SysClaim(SysClaimTypes.DenyOnlySid, new SecurityIdentifier(sidBytes, 0), right);
            }
            else if (reader.IsStartElement(dictionary.X500DistinguishedNameClaim, dictionary.EmptyString))
            {
                string right = ReadRightAttribute(reader, dictionary);
                reader.ReadStartElement();
                byte[] rawData = reader.ReadContentAsBase64();
                reader.ReadEndElement();
                return new SysClaim(SysClaimTypes.X500DistinguishedName, new X500DistinguishedName(rawData), right);
            }
            else if (reader.IsStartElement(dictionary.X509ThumbprintClaim, dictionary.EmptyString))
            {
                string right = ReadRightAttribute(reader, dictionary);
                reader.ReadStartElement();
                byte[] thumbprint = reader.ReadContentAsBase64();
                reader.ReadEndElement();
                return new SysClaim(SysClaimTypes.Thumbprint, thumbprint, right);
            }
            else if (reader.IsStartElement(dictionary.NameClaim, dictionary.EmptyString))
            {
                string right = ReadRightAttribute(reader, dictionary);
                reader.ReadStartElement();
                string name = reader.ReadString();
                reader.ReadEndElement();
                return new SysClaim(SysClaimTypes.Name, name, right);
            }
            else if (reader.IsStartElement(dictionary.DnsClaim, dictionary.EmptyString))
            {
                string right = ReadRightAttribute(reader, dictionary);
                reader.ReadStartElement();
                string dns = reader.ReadString();
                reader.ReadEndElement();
                return new SysClaim(SysClaimTypes.Dns, dns, right);
            }
            else if (reader.IsStartElement(dictionary.RsaClaim, dictionary.EmptyString))
            {
                string right = ReadRightAttribute(reader, dictionary);
                reader.ReadStartElement();
                string rsaXml = reader.ReadString();
                reader.ReadEndElement();

                System.Security.Cryptography.RSACryptoServiceProvider rsa = new System.Security.Cryptography.RSACryptoServiceProvider();
                rsa.FromXmlString(rsaXml);
                return new SysClaim(SysClaimTypes.Rsa, rsa, right);
            }
            else if (reader.IsStartElement(dictionary.MailAddressClaim, dictionary.EmptyString))
            {
                string right = ReadRightAttribute(reader, dictionary);
                reader.ReadStartElement();
                string address = reader.ReadString();
                reader.ReadEndElement();
                return new SysClaim(SysClaimTypes.Email, new System.Net.Mail.MailAddress(address), right);
            }
            else if (reader.IsStartElement(dictionary.SystemClaim, dictionary.EmptyString))
            {
                reader.ReadElementString();
                return SysClaim.System;
            }
            else if (reader.IsStartElement(dictionary.HashClaim, dictionary.EmptyString))
            {
                string right = ReadRightAttribute(reader, dictionary);
                reader.ReadStartElement();
                byte[] hash = reader.ReadContentAsBase64();
                reader.ReadEndElement();
                return new SysClaim(SysClaimTypes.Hash, hash, right);
            }
            else if (reader.IsStartElement(dictionary.SpnClaim, dictionary.EmptyString))
            {
                string right = ReadRightAttribute(reader, dictionary);
                reader.ReadStartElement();
                string spn = reader.ReadString();
                reader.ReadEndElement();
                return new SysClaim(SysClaimTypes.Spn, spn, right);
            }
            else if (reader.IsStartElement(dictionary.UpnClaim, dictionary.EmptyString))
            {
                string right = ReadRightAttribute(reader, dictionary);
                reader.ReadStartElement();
                string upn = reader.ReadString();
                reader.ReadEndElement();
                return new SysClaim(SysClaimTypes.Upn, upn, right);
            }
            else if (reader.IsStartElement(dictionary.UrlClaim, dictionary.EmptyString))
            {
                string right = ReadRightAttribute(reader, dictionary);
                reader.ReadStartElement();
                string url = reader.ReadString();
                reader.ReadEndElement();
                return new SysClaim(SysClaimTypes.Uri, new Uri(url), right);
            }
            else
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(SR.GetString(SR.ID4289, reader.LocalName, reader.NamespaceURI)));
            }
        }

        static void SerializeSid(SecurityIdentifier sid, SessionDictionary dictionary, XmlDictionaryWriter writer)
        {
            byte[] sidBytes = new byte[sid.BinaryLength];
            sid.GetBinaryForm(sidBytes, 0);
            writer.WriteBase64(sidBytes, 0, sidBytes.Length);
        }

        static string ReadRightAttribute(XmlDictionaryReader reader, SessionDictionary dictionary)
        {
            string right = reader.GetAttribute(dictionary.Right, dictionary.EmptyString);
            return string.IsNullOrEmpty(right) ? System.IdentityModel.Claims.Rights.PossessProperty : right;
        }
        
        static void WriteRightAttribute(SysClaim claim, SessionDictionary dictionary, XmlDictionaryWriter writer)
        {
            if (System.IdentityModel.Claims.Rights.PossessProperty.Equals(claim.Right))
                return;
            writer.WriteAttributeString(dictionary.Right, dictionary.EmptyString, claim.Right);
        }

        // As name says, certainly not a complete test, but it will allow us to move forward on 
        // strings that are possible UPN's
        // a@b will succeed
        // @a, a@, @ will fail
        static bool IsPossibleUpn(string name)
        {
            int delimiterPos = name.IndexOf('@');

            // if it is the first of last character
            if ((name.Length < 3) || (delimiterPos < 0) || (delimiterPos == 0) || (delimiterPos == name.Length - 1))
            {
                return false;
            }

            return true;
        }
    }
}
