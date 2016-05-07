//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Security.Tokens
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.IdentityModel.Policy;
    using System.IdentityModel.Tokens;
    using System.Xml;

    public class SecurityContextSecurityToken : SecurityToken, TimeBoundedCache.IExpirableItem, IDisposable
    {
        byte[] cookieBlob;
        UniqueId contextId = null;
        UniqueId keyGeneration = null;
        DateTime keyEffectiveTime;
        DateTime keyExpirationTime;
        DateTime tokenEffectiveTime;
        DateTime tokenExpirationTime;
        bool isCookieMode = false;
        byte[] key;
        string keyString;
        ReadOnlyCollection<IAuthorizationPolicy> authorizationPolicies;
        ReadOnlyCollection<SecurityKey> securityKeys;
        string id;
        SecurityMessageProperty bootstrapMessageProperty;
        bool disposed = false;

        public SecurityContextSecurityToken(UniqueId contextId, byte[] key, DateTime validFrom, DateTime validTo)
            : this(contextId, SecurityUtils.GenerateId(), key, validFrom, validTo)
        { }

        public SecurityContextSecurityToken(UniqueId contextId, string id, byte[] key, DateTime validFrom, DateTime validTo)
            : this(contextId, id, key, validFrom, validTo, null)
        { }

        public SecurityContextSecurityToken(UniqueId contextId, string id, byte[] key, DateTime validFrom, DateTime validTo, ReadOnlyCollection<IAuthorizationPolicy> authorizationPolicies)
            : base()
        {
            this.id = id;
            this.Initialize(contextId, key, validFrom, validTo, authorizationPolicies, false, null, validFrom, validTo);
        }

        public SecurityContextSecurityToken(UniqueId contextId, string id, byte[] key, DateTime validFrom, DateTime validTo, UniqueId keyGeneration, DateTime keyEffectiveTime, DateTime keyExpirationTime, ReadOnlyCollection<IAuthorizationPolicy> authorizationPolicies)
            : base()
        {
            this.id = id;
            this.Initialize(contextId, key, validFrom, validTo, authorizationPolicies, false, keyGeneration, keyEffectiveTime, keyExpirationTime);
        }

        internal SecurityContextSecurityToken(SecurityContextSecurityToken sourceToken, string id)
            : this(sourceToken, id, sourceToken.key, sourceToken.keyGeneration, sourceToken.keyEffectiveTime, sourceToken.keyExpirationTime, sourceToken.AuthorizationPolicies)
        {
        }

        internal SecurityContextSecurityToken(SecurityContextSecurityToken sourceToken, string id, byte[] key, UniqueId keyGeneration, DateTime keyEffectiveTime, DateTime keyExpirationTime, ReadOnlyCollection<IAuthorizationPolicy> authorizationPolicies)
            : base()
        {
            this.id = id;
            this.Initialize(sourceToken.contextId, key, sourceToken.ValidFrom, sourceToken.ValidTo, authorizationPolicies, sourceToken.isCookieMode, keyGeneration, keyEffectiveTime, keyExpirationTime);
            this.cookieBlob = sourceToken.cookieBlob;
            this.bootstrapMessageProperty = (sourceToken.bootstrapMessageProperty == null) ? null : (SecurityMessageProperty)sourceToken.BootstrapMessageProperty.CreateCopy();
        }

        internal SecurityContextSecurityToken(UniqueId contextId, string id, byte[] key, DateTime validFrom, DateTime validTo, ReadOnlyCollection<IAuthorizationPolicy> authorizationPolicies, bool isCookieMode, byte[] cookieBlob)
            : this(contextId, id, key, validFrom, validTo, authorizationPolicies, isCookieMode, cookieBlob, null, validFrom, validTo)
        {
        }

        internal SecurityContextSecurityToken(UniqueId contextId, string id, byte[] key, DateTime validFrom, DateTime validTo, ReadOnlyCollection<IAuthorizationPolicy> authorizationPolicies, bool isCookieMode, byte[] cookieBlob,
            UniqueId keyGeneration, DateTime keyEffectiveTime, DateTime keyExpirationTime)
            : base()
        {
            this.id = id;
            this.Initialize(contextId, key, validFrom, validTo, authorizationPolicies, isCookieMode, keyGeneration, keyEffectiveTime, keyExpirationTime);
            this.cookieBlob = cookieBlob;
        }

        SecurityContextSecurityToken(SecurityContextSecurityToken from)
        {
            ReadOnlyCollection<IAuthorizationPolicy> authorizationPolicies = System.IdentityModel.SecurityUtils.CloneAuthorizationPoliciesIfNecessary(from.authorizationPolicies);
            this.id = from.id;
            this.Initialize(from.contextId, from.key, from.tokenEffectiveTime, from.tokenExpirationTime, authorizationPolicies, from.isCookieMode, from.keyGeneration, from.keyEffectiveTime, from.keyExpirationTime);
            this.cookieBlob = from.cookieBlob;
            this.bootstrapMessageProperty = (from.bootstrapMessageProperty == null) ? null : (SecurityMessageProperty)from.BootstrapMessageProperty.CreateCopy();
        }

        /// <summary>
        /// Gets or Sets the SecurityMessageProperty extracted from 
        /// the Bootstrap message. This will contain the original tokens
        /// that the client used to Authenticate with the service. By 
        /// default, this is turned off. To turn this feature on, add a custom 
        /// ServiceCredentialsSecurityTokenManager and override  
        /// CreateSecurityTokenManager. Create the SecurityContextToken Authenticator by calling 
        /// ServiceCredentialsSecurityTokenManager.CreateSecureConversationTokenAuthenticator
        /// with 'preserveBootstrapTokens' parameter to true. 
        /// If there are any UserNameSecurityToken in the bootstrap message, the password in
        /// these tokens will be removed. When 'Cookie' mode SCT is enabled the BootstrapMessageProperty
        /// is not preserved in the Cookie. To preserve the bootstrap tokens in the CookieMode case
        /// write a custom Serializer and serialize the property as part of the cookie.
        /// </summary>
        public SecurityMessageProperty BootstrapMessageProperty
        {
            get
            {
                return this.bootstrapMessageProperty;
            }
            set
            {
                this.bootstrapMessageProperty = value;
            }
        }

        public override string Id
        {
            get { return this.id; }
        }

        public UniqueId ContextId
        {
            get
            {
                return this.contextId;
            }
        }

        public UniqueId KeyGeneration
        {
            get
            {
                return this.keyGeneration;
            }
        }

        public DateTime KeyEffectiveTime
        {
            get 
            { 
                return this.keyEffectiveTime; 
            }
        }

        public DateTime KeyExpirationTime
        {
            get 
            { 
                return this.keyExpirationTime; 
            }
        }

        public ReadOnlyCollection<IAuthorizationPolicy> AuthorizationPolicies
        {
            get
            {
                ThrowIfDisposed();
                return this.authorizationPolicies;
            }
            
            internal set
            {
                this.authorizationPolicies = value;
            }
        }

        public override ReadOnlyCollection<SecurityKey> SecurityKeys
        {
            get
            {
                return this.securityKeys;
            }
        }

        public override DateTime ValidFrom
        {
            get { return this.tokenEffectiveTime; }
        }

        public override DateTime ValidTo
        {
            get { return this.tokenExpirationTime; }
        }

        internal byte[] CookieBlob
        {
            get
            {
                return this.cookieBlob;
            }
        }

        /// <summary>
        /// This is set by the issuer when creating the SCT to be sent in the RSTR
        /// The SecurityContextTokenManager examines this property to determine how to write
        /// out the SCT
        /// This field is set to true when the issuer reads in a cookie mode SCT
        /// </summary>
        public bool IsCookieMode
        {
            get
            {
                return this.isCookieMode;
            }
        }

        DateTime TimeBoundedCache.IExpirableItem.ExpirationTime
        {
            get
            {
                return this.ValidTo;
            }
        }

        internal string GetBase64KeyString()
        {
            if (this.keyString == null)
            {
                this.keyString = Convert.ToBase64String(this.key);
            }
            return this.keyString;
        }

        internal byte[] GetKeyBytes()
        {
            byte[] retval = new byte[this.key.Length];
            Buffer.BlockCopy(this.key, 0, retval, 0, this.key.Length);
            return retval;
        }

        public override string ToString()
        {
            return String.Format(CultureInfo.CurrentCulture, "SecurityContextSecurityToken(Identifier='{0}', KeyGeneration='{1}')", this.contextId, this.keyGeneration);
        }

        void Initialize(UniqueId contextId, byte[] key, DateTime validFrom, DateTime validTo, ReadOnlyCollection<IAuthorizationPolicy> authorizationPolicies, bool isCookieMode,
            UniqueId keyGeneration, DateTime keyEffectiveTime, DateTime keyExpirationTime)
        {
            if (contextId == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("contextId");
            }

            if (key == null || key.Length == 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("key");
            }

            DateTime tokenEffectiveTimeUtc = validFrom.ToUniversalTime();
            DateTime tokenExpirationTimeUtc = validTo.ToUniversalTime();
            if (tokenEffectiveTimeUtc > tokenExpirationTimeUtc)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("validFrom", SR.GetString(SR.EffectiveGreaterThanExpiration));
            }
            this.tokenEffectiveTime = tokenEffectiveTimeUtc;
            this.tokenExpirationTime = tokenExpirationTimeUtc;

            this.keyEffectiveTime = keyEffectiveTime.ToUniversalTime();
            this.keyExpirationTime = keyExpirationTime.ToUniversalTime();
            if (this.keyEffectiveTime > this.keyExpirationTime)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("keyEffectiveTime", SR.GetString(SR.EffectiveGreaterThanExpiration));
            }
            if ((this.keyEffectiveTime < tokenEffectiveTimeUtc) || (this.keyExpirationTime > tokenExpirationTimeUtc))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(SR.GetString(SR.KeyLifetimeNotWithinTokenLifetime));
            }

            this.key = new byte[key.Length];
            Buffer.BlockCopy(key, 0, this.key, 0, key.Length);
            this.contextId = contextId;
            this.keyGeneration = keyGeneration;
            this.authorizationPolicies = authorizationPolicies ?? EmptyReadOnlyCollection<IAuthorizationPolicy>.Instance;
            List<SecurityKey> temp = new List<SecurityKey>(1);
            temp.Add(new InMemorySymmetricSecurityKey(this.key, false));
            this.securityKeys = temp.AsReadOnly();
            this.isCookieMode = isCookieMode;
        }

        public override bool CanCreateKeyIdentifierClause<T>()
        {
            if (typeof(T) == typeof(SecurityContextKeyIdentifierClause))
                return true;

            return base.CanCreateKeyIdentifierClause<T>();
        }

        public override T CreateKeyIdentifierClause<T>()
        {
            if (typeof(T) == typeof(SecurityContextKeyIdentifierClause))
                return new SecurityContextKeyIdentifierClause(this.contextId, this.keyGeneration) as T;

            return base.CreateKeyIdentifierClause<T>();
        }

        public override bool MatchesKeyIdentifierClause(SecurityKeyIdentifierClause keyIdentifierClause)
        {
            SecurityContextKeyIdentifierClause sctKeyIdentifierClause = keyIdentifierClause as SecurityContextKeyIdentifierClause;
            if (sctKeyIdentifierClause != null)
                return sctKeyIdentifierClause.Matches(this.contextId, this.keyGeneration);

            return base.MatchesKeyIdentifierClause(keyIdentifierClause);
        }

        public static SecurityContextSecurityToken CreateCookieSecurityContextToken(UniqueId contextId, string id, byte[] key,
            DateTime validFrom, DateTime validTo, ReadOnlyCollection<IAuthorizationPolicy> authorizationPolicies, SecurityStateEncoder securityStateEncoder)
        {
            return CreateCookieSecurityContextToken(contextId, id, key, validFrom, validTo, null, validFrom, validTo, authorizationPolicies, securityStateEncoder);
        }


        public static SecurityContextSecurityToken CreateCookieSecurityContextToken(UniqueId contextId, string id, byte[] key,
            DateTime validFrom, DateTime validTo, UniqueId keyGeneration, DateTime keyEffectiveTime,
            DateTime keyExpirationTime, ReadOnlyCollection<IAuthorizationPolicy> authorizationPolicies, SecurityStateEncoder securityStateEncoder)
        {
            SecurityContextCookieSerializer cookieSerializer = new SecurityContextCookieSerializer(securityStateEncoder, null);
            byte[] cookieBlob = cookieSerializer.CreateCookieFromSecurityContext(contextId, id, key, validFrom, validTo, keyGeneration,
                                keyEffectiveTime, keyExpirationTime, authorizationPolicies);

            return new SecurityContextSecurityToken(contextId, id, key, validFrom, validTo,
                authorizationPolicies, true, cookieBlob, keyGeneration, keyEffectiveTime, keyExpirationTime);
        }

        internal SecurityContextSecurityToken Clone()
        {
            ThrowIfDisposed();
            return new SecurityContextSecurityToken(this);
        }

        public void Dispose()
        {
            if (!this.disposed)
            {
                this.disposed = true;
                System.IdentityModel.SecurityUtils.DisposeAuthorizationPoliciesIfNecessary(this.authorizationPolicies);
                if (this.bootstrapMessageProperty != null)
                {
                    this.bootstrapMessageProperty.Dispose();
                }
            }
        }

        void ThrowIfDisposed()
        {
            if (this.disposed)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ObjectDisposedException(this.GetType().FullName));
            }
        }
    }
}
