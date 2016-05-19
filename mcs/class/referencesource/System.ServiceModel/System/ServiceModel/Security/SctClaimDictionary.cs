//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Security
{
    using System.Xml;
    using System.ServiceModel;
    using System.Collections.Generic;
    using System.IdentityModel.Claims;
    using System.IdentityModel.Policy;

    sealed class SctClaimDictionary : XmlDictionary
    {
        static readonly SctClaimDictionary instance = new SctClaimDictionary();

        // Claim entries
        XmlDictionaryString claim;
        XmlDictionaryString claimSets;
        XmlDictionaryString claimSet;
        XmlDictionaryString identities;
        XmlDictionaryString primaryIdentity;
        XmlDictionaryString primaryIssuer;

        XmlDictionaryString x509CertificateClaimSet;
        XmlDictionaryString systemClaimSet;
        XmlDictionaryString windowsClaimSet;
        XmlDictionaryString anonymousClaimSet;
       
        XmlDictionaryString binaryClaim;
        XmlDictionaryString dnsClaim;
        XmlDictionaryString hashClaim;
        XmlDictionaryString mailAddressClaim;
        XmlDictionaryString nameClaim;
        XmlDictionaryString rsaClaim;
        XmlDictionaryString spnClaim;
        XmlDictionaryString systemClaim;
        XmlDictionaryString upnClaim;
        XmlDictionaryString urlClaim;
        XmlDictionaryString windowsSidClaim;
        XmlDictionaryString denyOnlySidClaim;
        XmlDictionaryString x500DistinguishedNameClaim;
        XmlDictionaryString x509ThumbprintClaim;
        XmlDictionaryString right;

        XmlDictionaryString windowsSidIdentity;
        XmlDictionaryString genericIdentity;
        XmlDictionaryString authenticationType;

        XmlDictionaryString contextId;
        XmlDictionaryString effectiveTime;
        XmlDictionaryString expiryTime;
        XmlDictionaryString id;
        XmlDictionaryString isCookieMode;
        XmlDictionaryString key;
 
        XmlDictionaryString name;
        XmlDictionaryString sid;
        XmlDictionaryString nullValue;
        XmlDictionaryString securityContextToken;
        XmlDictionaryString serviceContractId;
        XmlDictionaryString value;
        XmlDictionaryString version;

        XmlDictionaryString genericXmlToken;
        XmlDictionaryString tokenType;
        XmlDictionaryString tokenXml;
        XmlDictionaryString internalTokenReference;
        XmlDictionaryString externalTokenReference;
        XmlDictionaryString keyGeneration;
        XmlDictionaryString keyEffectiveTime;
        XmlDictionaryString keyExpiryTime;

        XmlDictionaryString emptyString;
        
        private SctClaimDictionary()
        {
            this.securityContextToken = this.Add("SecurityContextSecurityToken");
            this.version = this.Add("Version");
            this.contextId = this.Add("ContextId");
            this.id = this.Add("Id");
            this.key = this.Add("Key");
            this.isCookieMode = this.Add("IsCookieMode");
            this.serviceContractId = this.Add("ServiceContractId");
            this.effectiveTime = this.Add("EffectiveTime");
            this.expiryTime = this.Add("ExpiryTime");
            this.keyGeneration = this.Add("KeyGeneration");
            this.keyEffectiveTime = this.Add("KeyEffectiveTime");
            this.keyExpiryTime = this.Add("KeyExpiryTime");
            this.claim = this.Add("Claim");
            this.claimSets = this.Add("ClaimSets");
            this.claimSet = this.Add("ClaimSet");
            this.identities = this.Add("Identities");
            this.primaryIdentity = this.Add("PrimaryIdentity");
            this.primaryIssuer = this.Add("PrimaryIssuer");

            this.x509CertificateClaimSet = this.Add("X509CertificateClaimSet");
            this.systemClaimSet = this.Add("SystemClaimSet");
            this.windowsClaimSet = this.Add("WindowsClaimSet");
            this.anonymousClaimSet = this.Add("AnonymousClaimSet");

            this.binaryClaim = this.Add("BinaryClaim");
            this.dnsClaim = this.Add("DnsClaim");
            this.genericIdentity = this.Add("GenericIdentity");
            this.authenticationType = this.Add("AuthenticationType");
            this.right = this.Add("Right");
            this.hashClaim = this.Add("HashClaim");
            this.mailAddressClaim = this.Add("MailAddressClaim");
            this.nameClaim = this.Add("NameClaim");
            this.rsaClaim = this.Add("RsaClaim");
            this.spnClaim = this.Add("SpnClaim");
            this.systemClaim = this.Add("SystemClaim");
            this.upnClaim = this.Add("UpnClaim");
            this.urlClaim = this.Add("UrlClaim");
            this.windowsSidClaim = this.Add("WindowsSidClaim");
            this.denyOnlySidClaim = this.Add("DenyOnlySidClaim");
            this.windowsSidIdentity = this.Add("WindowsSidIdentity");
            this.x500DistinguishedNameClaim = this.Add("X500DistinguishedClaim");
            this.x509ThumbprintClaim = this.Add("X509ThumbprintClaim");

            this.name = this.Add("Name");
            this.sid = this.Add("Sid");
            this.value = this.Add("Value");
            this.nullValue = this.Add("Null");
            this.genericXmlToken = this.Add("GenericXmlSecurityToken");
            this.tokenType = this.Add("TokenType");
            this.internalTokenReference = this.Add("InternalTokenReference");
            this.externalTokenReference = this.Add("ExternalTokenReference");
            this.tokenXml = this.Add("TokenXml");
            this.emptyString = this.Add(String.Empty);
        }

        public static SctClaimDictionary Instance
        {
            get { return instance; }
        }

        public XmlDictionaryString Claim
        {
            get { return this.claim; }
        }

        public XmlDictionaryString ClaimSets
        {
            get { return this.claimSets; }
        }

        public XmlDictionaryString ClaimSet
        {
            get { return this.claimSet; }
        }

        public XmlDictionaryString PrimaryIssuer
        {
            get { return this.primaryIssuer; }
        }

        public XmlDictionaryString Identities
        {
            get { return this.identities; }
        }

        public XmlDictionaryString PrimaryIdentity
        {
            get { return this.primaryIdentity; }
        }

        public XmlDictionaryString X509CertificateClaimSet
        {
            get { return this.x509CertificateClaimSet; }
        }

        public XmlDictionaryString SystemClaimSet
        {
            get { return this.systemClaimSet; }
        }

        public XmlDictionaryString WindowsClaimSet
        {
            get { return this.windowsClaimSet; }
        }

        public XmlDictionaryString AnonymousClaimSet
        {
            get { return this.anonymousClaimSet; }
        }

        public XmlDictionaryString ContextId
        {
            get { return this.contextId; }
        }

        public XmlDictionaryString BinaryClaim
        {
            get { return this.binaryClaim; }
        }

        public XmlDictionaryString DnsClaim
        {
            get { return this.dnsClaim; }
        }

        public XmlDictionaryString GenericIdentity
        {
            get { return this.genericIdentity; }
        }

        public XmlDictionaryString AuthenticationType
        {
            get { return this.authenticationType; }
        }

        public XmlDictionaryString Right
        {
            get { return this.right; }
        }

        public XmlDictionaryString HashClaim
        {
            get { return this.hashClaim; }
        }

        public XmlDictionaryString MailAddressClaim
        {
            get { return this.mailAddressClaim; }
        }

        public XmlDictionaryString NameClaim
        {
            get { return this.nameClaim; }
        }

        public XmlDictionaryString RsaClaim
        {
            get { return this.rsaClaim; }
        }

        public XmlDictionaryString SpnClaim
        {
            get { return this.spnClaim; }
        }

        public XmlDictionaryString SystemClaim
        {
            get { return this.systemClaim; }
        }

        public XmlDictionaryString UpnClaim
        {
            get { return this.upnClaim; }
        }

        public XmlDictionaryString UrlClaim
        {
            get { return this.urlClaim; }
        }

        public XmlDictionaryString WindowsSidClaim
        {
            get { return this.windowsSidClaim; }
        }

        public XmlDictionaryString DenyOnlySidClaim
        {
            get { return this.denyOnlySidClaim; }
        }

        public XmlDictionaryString WindowsSidIdentity
        {
            get { return this.windowsSidIdentity; }
        }

        public XmlDictionaryString X500DistinguishedNameClaim
        {
            get { return this.x500DistinguishedNameClaim; }
        }

        public XmlDictionaryString X509ThumbprintClaim
        {
            get { return this.x509ThumbprintClaim; }
        }

        public XmlDictionaryString EffectiveTime
        {
            get { return this.effectiveTime; }
        }

        public XmlDictionaryString ExpiryTime
        {
            get { return this.expiryTime; }
        }

        public XmlDictionaryString Id
        {
            get { return this.id; }
        }

        public XmlDictionaryString IsCookieMode
        {
            get { return this.isCookieMode; }
        }

        public XmlDictionaryString Key
        {
            get { return this.key; }
        }

        public XmlDictionaryString Sid
        {
            get { return this.sid; }
        }

        public XmlDictionaryString Name
        {
            get { return this.name; }
        }

        public XmlDictionaryString NullValue
        {
            get { return this.nullValue; }
        }

        public XmlDictionaryString SecurityContextSecurityToken
        {
            get { return this.securityContextToken; }
        }

        public XmlDictionaryString ServiceContractId
        {
            get { return this.serviceContractId; }
        }

        public XmlDictionaryString Value
        {
            get { return this.value; }
        }

        public XmlDictionaryString Version
        {
            get { return this.version; }
        }

        public XmlDictionaryString GenericXmlSecurityToken
        {
            get { return this.genericXmlToken; }
        }

        public XmlDictionaryString TokenType
        {
            get { return this.tokenType; }
        }

        public XmlDictionaryString TokenXml
        {
            get { return this.tokenXml; }
        }

        public XmlDictionaryString InternalTokenReference
        {
            get { return this.internalTokenReference; }
        }

        public XmlDictionaryString ExternalTokenReference
        {
            get { return this.externalTokenReference; }
        }

        public XmlDictionaryString EmptyString
        {
            get { return this.emptyString; }
        }

        public XmlDictionaryString KeyGeneration
        {
            get { return this.keyGeneration; }
        }

        public XmlDictionaryString KeyEffectiveTime
        {
            get { return this.keyEffectiveTime; }
        }

        public XmlDictionaryString KeyExpiryTime
        {
            get { return this.keyExpiryTime; }
        }
    }
}
