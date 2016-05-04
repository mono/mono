//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Security.Tokens
{
    using System.Collections.ObjectModel;
    using System.ServiceModel;
    using System.IdentityModel.Claims;
    using System.IdentityModel.Policy;
    using System.IdentityModel.Tokens;

    sealed class DerivedKeySecurityTokenStub : SecurityToken
    {
        string id;
        string derivationAlgorithm;
        string label;
        int length;
        byte[] nonce;
        int offset;
        int generation;
        SecurityKeyIdentifierClause tokenToDeriveIdentifier;

        public DerivedKeySecurityTokenStub(int generation, int offset, int length,
            string label, byte[] nonce,
            SecurityKeyIdentifierClause tokenToDeriveIdentifier, string derivationAlgorithm, string id)
        {
            this.id = id;
            this.generation = generation;
            this.offset = offset;
            this.length = length;
            this.label = label;
            this.nonce = nonce;
            this.tokenToDeriveIdentifier = tokenToDeriveIdentifier;
            this.derivationAlgorithm = derivationAlgorithm;
        }

        public override string Id
        {
            get { return this.id; }
        }

        public override DateTime ValidFrom
        {
#pragma warning suppress 56503 // Property does not make sense for Derived Key tokens.
            get { throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotImplementedException()); }
        }

        public override DateTime ValidTo
        {
#pragma warning suppress 56503 // Property does not make sense for Derived Key tokens.
            get { throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotImplementedException()); }
        }

        public override ReadOnlyCollection<SecurityKey> SecurityKeys
        {
            get { return null; }
        }

        public SecurityKeyIdentifierClause TokenToDeriveIdentifier
        {
            get { return this.tokenToDeriveIdentifier; }
        }

        public DerivedKeySecurityToken CreateToken(SecurityToken tokenToDerive, int maxKeyLength)
        {
            DerivedKeySecurityToken result = new DerivedKeySecurityToken(this.generation, this.offset, this.length,
                this.label, this.nonce, tokenToDerive, this.tokenToDeriveIdentifier, this.derivationAlgorithm, this.Id);
            result.InitializeDerivedKey(maxKeyLength);
            return result;
        }
    }
}

