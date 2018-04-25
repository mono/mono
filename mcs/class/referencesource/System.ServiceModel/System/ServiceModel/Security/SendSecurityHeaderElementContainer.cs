//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Security
{
    using System.IdentityModel.Claims;
    using System.ServiceModel;
    using System.IdentityModel.Policy;
    using System.IdentityModel.Tokens;
    using System.ServiceModel.Security.Tokens;
    using System.Xml;
    using System.Collections.Generic;

    using ISecurityElement = System.IdentityModel.ISecurityElement;

    class SendSecurityHeaderElementContainer
    {
        List<SecurityToken> signedSupportingTokens = null;
        List<SendSecurityHeaderElement> basicSupportingTokens = null;
        List<SecurityToken> endorsingSupportingTokens = null;
        List<SecurityToken> endorsingDerivedSupportingTokens = null;
        List<SecurityToken> signedEndorsingSupportingTokens = null;
        List<SecurityToken> signedEndorsingDerivedSupportingTokens = null;
        List<SendSecurityHeaderElement> signatureConfirmations = null;
        List<SendSecurityHeaderElement> endorsingSignatures = null;
        Dictionary<SecurityToken, SecurityKeyIdentifierClause> securityTokenMappedToIdentifierClause = null;

        public SecurityTimestamp Timestamp;
        public SecurityToken PrerequisiteToken;
        public SecurityToken SourceSigningToken;
        public SecurityToken DerivedSigningToken;
        public SecurityToken SourceEncryptionToken;
        public SecurityToken WrappedEncryptionToken;
        public SecurityToken DerivedEncryptionToken;
        public ISecurityElement ReferenceList;
        public SendSecurityHeaderElement PrimarySignature;

        void Add<T>(ref List<T> list, T item)
        {
            if (list == null)
            {
                list = new List<T>();
            }
            list.Add(item);
        }

        public SecurityToken[] GetSignedSupportingTokens()
        {
            return (this.signedSupportingTokens != null) ? this.signedSupportingTokens.ToArray() : null;
        }

        public void AddSignedSupportingToken(SecurityToken token)
        {
            Add<SecurityToken>(ref this.signedSupportingTokens, token);
        }

        public List<SecurityToken> EndorsingSupportingTokens
        {
            get { return this.endorsingSupportingTokens; }
        }

        public SendSecurityHeaderElement[] GetBasicSupportingTokens()
        {
            return (this.basicSupportingTokens != null) ? this.basicSupportingTokens.ToArray() : null;
        }

        public void AddBasicSupportingToken(SendSecurityHeaderElement tokenElement)
        {
            Add<SendSecurityHeaderElement>(ref this.basicSupportingTokens, tokenElement);
        }

        public SecurityToken[] GetSignedEndorsingSupportingTokens()
        {
            return (this.signedEndorsingSupportingTokens != null) ? this.signedEndorsingSupportingTokens.ToArray() : null;
        }

        public void AddSignedEndorsingSupportingToken(SecurityToken token)
        {
            Add<SecurityToken>(ref this.signedEndorsingSupportingTokens, token);
        }

        public SecurityToken[] GetSignedEndorsingDerivedSupportingTokens()
        {
            return (this.signedEndorsingDerivedSupportingTokens != null) ? this.signedEndorsingDerivedSupportingTokens.ToArray() : null;
        }

        public void AddSignedEndorsingDerivedSupportingToken(SecurityToken token)
        {
            Add<SecurityToken>(ref this.signedEndorsingDerivedSupportingTokens, token);
        }

        public SecurityToken[] GetEndorsingSupportingTokens()
        {
            return (this.endorsingSupportingTokens != null) ? this.endorsingSupportingTokens.ToArray() : null;
        }

        public void AddEndorsingSupportingToken(SecurityToken token)
        {
            Add<SecurityToken>(ref this.endorsingSupportingTokens, token);
        }

        public SecurityToken[] GetEndorsingDerivedSupportingTokens()
        {
            return (this.endorsingDerivedSupportingTokens != null) ? this.endorsingDerivedSupportingTokens.ToArray() : null;
        }

        public void AddEndorsingDerivedSupportingToken(SecurityToken token)
        {
            Add<SecurityToken>(ref this.endorsingDerivedSupportingTokens, token);
        }

        public SendSecurityHeaderElement[] GetSignatureConfirmations()
        {
            return (this.signatureConfirmations != null) ? this.signatureConfirmations.ToArray() : null;
        }

        public void AddSignatureConfirmation(SendSecurityHeaderElement confirmation)
        {
            Add<SendSecurityHeaderElement>(ref this.signatureConfirmations, confirmation);
        }

        public SendSecurityHeaderElement[] GetEndorsingSignatures()
        {
            return (this.endorsingSignatures != null) ? this.endorsingSignatures.ToArray() : null;
        }

        public void AddEndorsingSignature(SendSecurityHeaderElement signature)
        {
            Add<SendSecurityHeaderElement>(ref this.endorsingSignatures, signature);
        }

        public void MapSecurityTokenToStrClause(SecurityToken securityToken, SecurityKeyIdentifierClause keyIdentifierClause)
        {
            if (this.securityTokenMappedToIdentifierClause == null)
            {
                this.securityTokenMappedToIdentifierClause = new Dictionary<SecurityToken, SecurityKeyIdentifierClause>();
            }

            if (!this.securityTokenMappedToIdentifierClause.ContainsKey(securityToken))
            {
                this.securityTokenMappedToIdentifierClause.Add(securityToken, keyIdentifierClause);
            }
        }

        public bool TryGetIdentifierClauseFromSecurityToken(SecurityToken securityToken, out SecurityKeyIdentifierClause keyIdentifierClause)
        {
            keyIdentifierClause = null;
            if (securityToken == null
                || this.securityTokenMappedToIdentifierClause == null
                || !this.securityTokenMappedToIdentifierClause.TryGetValue(securityToken, out keyIdentifierClause))
            {
                return false;
            }
            return true;
        }       
    }
}
