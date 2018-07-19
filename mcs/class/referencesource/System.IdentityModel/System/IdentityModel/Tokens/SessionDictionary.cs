//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.IdentityModel.Claims
{
    using System;
    using System.Xml;

    /// <summary>
    /// Dictionary of naming elements relevant to Windows Identity Foundation.
    /// </summary>
    internal sealed class SessionDictionary : XmlDictionary
    {
        static readonly SessionDictionary instance = new SessionDictionary();

        XmlDictionaryString _claim;
        XmlDictionaryString _sct;
        XmlDictionaryString _issuer;
        XmlDictionaryString _originalIssuer;
        XmlDictionaryString _issuerRef;
        XmlDictionaryString _claimCollection;
        XmlDictionaryString _actor;
        XmlDictionaryString _claimProperty;
        XmlDictionaryString _claimProperties;
        XmlDictionaryString _value;
        XmlDictionaryString _valueType;
        XmlDictionaryString _label;
        XmlDictionaryString _claimPropertyName;
        XmlDictionaryString _claimPropertyValue;
        XmlDictionaryString _type;
        XmlDictionaryString _subjectId;
        XmlDictionaryString _contextId;
        XmlDictionaryString _authenticationType;
        XmlDictionaryString _nameClaimType;
        XmlDictionaryString _roleClaimType;
        XmlDictionaryString _version;
        XmlDictionaryString _scVersion;
        XmlDictionaryString _emptyString;
        XmlDictionaryString _nullValue;
        XmlDictionaryString _key;
        XmlDictionaryString _effectiveTime;
        XmlDictionaryString _expiryTime;
        XmlDictionaryString _keyGeneration;
        XmlDictionaryString _keyEffectiveTime;
        XmlDictionaryString _keyExpiryTime;
        XmlDictionaryString _sessionId;
        XmlDictionaryString _id;
        XmlDictionaryString _validFrom;
        XmlDictionaryString _validTo;
        XmlDictionaryString _sesionToken;
        XmlDictionaryString _sesionTokenCookie;
        XmlDictionaryString _bootStrapToken;
        XmlDictionaryString _context;
        XmlDictionaryString _claimsPrincipal;
        XmlDictionaryString _windowsPrincipal;
        XmlDictionaryString _windowsIdentity;
        XmlDictionaryString _identity;
        XmlDictionaryString _identities;
        XmlDictionaryString _windowsLogonName;
        XmlDictionaryString _persistentTrue;
        XmlDictionaryString _sctAuthorizationPolicy;
        XmlDictionaryString _right;
        XmlDictionaryString _endpointId;
        XmlDictionaryString _windowsSidClaim;
        XmlDictionaryString _denyOnlySidClaim;
        XmlDictionaryString _x500DistinguishedNameClaim;
        XmlDictionaryString _x509ThumbprintClaim;
        XmlDictionaryString _nameClaim;
        XmlDictionaryString _dnsClaim;
        XmlDictionaryString _rsaClaim;
        XmlDictionaryString _mailAddressClaim;
        XmlDictionaryString _systemClaim;
        XmlDictionaryString _hashClaim;
        XmlDictionaryString _spnClaim;
        XmlDictionaryString _upnClaim;
        XmlDictionaryString _urlClaim;
        XmlDictionaryString _sid;
        XmlDictionaryString _referenceModeTrue;

        private SessionDictionary()
        {
            _claim = Add("Claim");
            _sct = Add("SecurityContextToken");
            _version = Add("Version");
            _scVersion = Add("SecureConversationVersion");
            _issuer = Add("Issuer");
            _originalIssuer = Add("OriginalIssuer");
            _issuerRef = Add("IssuerRef");
            _claimCollection = Add("ClaimCollection");
            _actor = Add("Actor");
            _claimProperty = Add("ClaimProperty");
            _claimProperties = Add("ClaimProperties");
            _value = Add("Value");
            _valueType = Add("ValueType");
            _label = Add("Label");
            _type = Add("Type");
            _subjectId = Add("subjectID");
            _claimPropertyName = Add("ClaimPropertyName");
            _claimPropertyValue = Add("ClaimPropertyValue");
            _authenticationType = Add("AuthenticationType");
            _nameClaimType = Add("NameClaimType");
            _roleClaimType = Add("RoleClaimType");
            _nullValue = Add("Null");
            _emptyString = Add(String.Empty);
            _key = Add("Key");
            _effectiveTime = Add("EffectiveTime");
            _expiryTime = Add("ExpiryTime");
            _keyGeneration = Add("KeyGeneration");
            _keyEffectiveTime = Add("KeyEffectiveTime");
            _keyExpiryTime = Add("KeyExpiryTime");
            _sessionId = Add("SessionId");
            _id = Add("Id");
            _validFrom = Add("ValidFrom");
            _validTo = Add("ValidTo");
            _contextId = Add("ContextId");
            _sesionToken = Add("SessionToken");
            _sesionTokenCookie = Add("SessionTokenCookie");
            _bootStrapToken = Add("BootStrapToken");
            _context = Add("Context");
            _claimsPrincipal = Add("ClaimsPrincipal");
            _windowsPrincipal = Add("WindowsPrincipal");
            _windowsIdentity = Add("WindowIdentity");
            _identity = Add("Identity");
            _identities = Add("Identities");
            _windowsLogonName = Add("WindowsLogonName");
            _persistentTrue = Add("PersistentTrue");
            _sctAuthorizationPolicy = Add("SctAuthorizationPolicy");
            _right = Add("Right");
            _endpointId = Add("EndpointId");
            _windowsSidClaim = Add("WindowsSidClaim");
            _denyOnlySidClaim = Add("DenyOnlySidClaim");
            _x500DistinguishedNameClaim = Add("X500DistinguishedNameClaim");
            _x509ThumbprintClaim = Add("X509ThumbprintClaim");
            _nameClaim = Add("NameClaim");
            _dnsClaim = Add("DnsClaim");
            _rsaClaim = Add("RsaClaim");
            _mailAddressClaim = Add("MailAddressClaim");
            _systemClaim = Add("SystemClaim");
            _hashClaim = Add("HashClaim");
            _spnClaim = Add("SpnClaim");
            _upnClaim = Add("UpnClaim");
            _urlClaim = Add("UrlClaim");
            _sid = Add("Sid");
            _referenceModeTrue = Add("ReferenceModeTrue");
        }

#pragma warning disable 1591
        public static SessionDictionary Instance
        {
            get { return instance; }
        }

        public XmlDictionaryString PersistentTrue
        {
            get { return _persistentTrue; }
        }

        public XmlDictionaryString WindowsLogonName
        {
            get { return _windowsLogonName; }
        }

        public XmlDictionaryString ClaimsPrincipal
        {
            get { return _claimsPrincipal; }
        }

        public XmlDictionaryString WindowsPrincipal
        {
            get { return _windowsPrincipal; }
        }

        public XmlDictionaryString WindowsIdentity
        {
            get { return _windowsIdentity; }
        }

        public XmlDictionaryString Identity
        {
            get { return _identity; }
        }

        public XmlDictionaryString Identities
        {
            get { return _identities; }
        }

        public XmlDictionaryString SessionId
        {
            get { return _sessionId; }
        }

        public XmlDictionaryString ReferenceModeTrue
        {
            get { return _referenceModeTrue; }
        }

        public XmlDictionaryString ValidFrom
        {
            get { return _validFrom; }
        }

        public XmlDictionaryString ValidTo
        {
            get { return _validTo; }
        }

        public XmlDictionaryString EffectiveTime
        {
            get { return _effectiveTime; }
        }

        public XmlDictionaryString ExpiryTime
        {
            get { return _expiryTime; }
        }

        public XmlDictionaryString KeyEffectiveTime
        {
            get { return _keyEffectiveTime; }
        }

        public XmlDictionaryString KeyExpiryTime
        {
            get { return _keyExpiryTime; }
        }

        public XmlDictionaryString Claim
        {
            get { return _claim; }
        }

        public XmlDictionaryString Issuer
        {
            get { return _issuer; }
        }

        public XmlDictionaryString OriginalIssuer
        {
            get { return _originalIssuer; }
        }

        public XmlDictionaryString IssuerRef
        {
            get { return _issuerRef; }
        }

        public XmlDictionaryString ClaimCollection
        {
            get { return _claimCollection; }
        }

        public XmlDictionaryString Actor
        {
            get { return _actor; }
        }

        public XmlDictionaryString ClaimProperties
        {
            get { return _claimProperties; }
        }

        public XmlDictionaryString ClaimProperty
        {
            get { return _claimProperty; }
        }

        public XmlDictionaryString Value
        {
            get { return _value; }
        }

        public XmlDictionaryString ValueType
        {
            get { return _valueType; }
        }

        public XmlDictionaryString Label
        {
            get { return _label; }
        }

        public XmlDictionaryString Type
        {
            get { return _type; }
        }

        public XmlDictionaryString SubjectId
        {
            get { return _subjectId; }
        }

        public XmlDictionaryString ClaimPropertyName
        {
            get { return _claimPropertyName; }
        }

        public XmlDictionaryString ClaimPropertyValue
        {
            get { return _claimPropertyValue; }
        }

        public XmlDictionaryString AuthenticationType
        {
            get { return _authenticationType; }
        }

        public XmlDictionaryString NameClaimType
        {
            get { return _nameClaimType; }
        }

        public XmlDictionaryString RoleClaimType
        {
            get { return _roleClaimType; }
        }

        public XmlDictionaryString NullValue
        {
            get { return _nullValue; }
        }

        public XmlDictionaryString SecurityContextToken
        {
            get { return _sct; }
        }

        public XmlDictionaryString Version
        {
            get { return _version; }
        }

        public XmlDictionaryString SecureConversationVersion
        {
            get { return _scVersion; }
        }

        public XmlDictionaryString EmptyString
        {
            get { return _emptyString; }
        }

        public XmlDictionaryString Key
        {
            get { return _key; }
        }

        public XmlDictionaryString KeyGeneration
        {
            get { return _keyGeneration; }
        }

        public XmlDictionaryString Id
        {
            get { return _id; }
        }

        public XmlDictionaryString ContextId
        {
            get { return _contextId; }
        }

        public XmlDictionaryString SessionToken
        {
            get { return _sesionToken; }
        }

        public XmlDictionaryString SessionTokenCookie
        {
            get { return _sesionTokenCookie; }
        }

        public XmlDictionaryString BootstrapToken
        {
            get { return _bootStrapToken; }
        }

        public XmlDictionaryString Context
        {
            get { return _context; }
        }

        public XmlDictionaryString SctAuthorizationPolicy
        {
            get { return _sctAuthorizationPolicy; }
        }

        public XmlDictionaryString Right
        {
            get { return _right; }
        }

        public XmlDictionaryString EndpointId
        {
            get { return _endpointId; }
        }

        public XmlDictionaryString WindowsSidClaim
        {
            get { return _windowsSidClaim; }
        }

        public XmlDictionaryString DenyOnlySidClaim
        {
            get { return _denyOnlySidClaim; }
        }

        public XmlDictionaryString X500DistinguishedNameClaim
        {
            get { return _x500DistinguishedNameClaim; }
        }

        public XmlDictionaryString X509ThumbprintClaim
        {
            get { return _x509ThumbprintClaim; }
        }

        public XmlDictionaryString NameClaim
        {
            get { return _nameClaim; }
        }

        public XmlDictionaryString DnsClaim
        {
            get { return _dnsClaim; }
        }

        public XmlDictionaryString RsaClaim
        {
            get { return _rsaClaim; }
        }

        public XmlDictionaryString MailAddressClaim
        {
            get { return _mailAddressClaim; }
        }

        public XmlDictionaryString SystemClaim
        {
            get { return _systemClaim; }
        }

        public XmlDictionaryString HashClaim
        {
            get { return _hashClaim; }
        }

        public XmlDictionaryString SpnClaim
        {
            get { return _spnClaim; }
        }

        public XmlDictionaryString UpnClaim
        {
            get { return _upnClaim; }
        }

        public XmlDictionaryString UrlClaim
        {
            get { return _urlClaim; }
        }

        public XmlDictionaryString Sid
        {
            get { return _sid; }
        }

#pragma warning restore 1591

    }
}
