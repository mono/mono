//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.IdentityModel.Tokens
{
    // All subclasses are required to be thread-safe and immutable

    // Self-resolving clauses such as RSA and X509 raw data should
    // override CanCreateKey and return true, and implement
    // CreateKey()

    public abstract class SecurityKeyIdentifierClause
    {
        readonly string clauseType;
        byte[] derivationNonce;
        int derivationLength;
        string id = null;

        protected SecurityKeyIdentifierClause(string clauseType)
            : this(clauseType, null, 0)
        {
        }

        protected SecurityKeyIdentifierClause(string clauseType, byte[] nonce, int length)
        {
            this.clauseType = clauseType;
            this.derivationNonce = nonce;
            this.derivationLength = length;
        }

        public virtual bool CanCreateKey
        {
            get { return false; }
        }

        public string ClauseType
        {
            get { return this.clauseType; }
        }

        public string Id
        {
            get { return this.id; }
            set { this.id = value; }
        }

        public virtual SecurityKey CreateKey()
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(SR.GetString(SR.KeyIdentifierClauseDoesNotSupportKeyCreation)));
        }

        public virtual bool Matches(SecurityKeyIdentifierClause keyIdentifierClause)
        {
            return ReferenceEquals(this, keyIdentifierClause);
        }

        public byte[] GetDerivationNonce()
        {
            return (this.derivationNonce != null) ? (byte[])this.derivationNonce.Clone() : null;
        }

        public int DerivationLength
        {
            get { return this.derivationLength; }
        }
    }
}
