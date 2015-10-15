//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Security
{
    using System.IdentityModel;
    using System.IdentityModel.Tokens;
    using System.Runtime.CompilerServices;

    [TypeForwardedFrom("System.ServiceModel, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089")]
    public class BinarySecretKeyIdentifierClause : BinaryKeyIdentifierClause
    {
        InMemorySymmetricSecurityKey symmetricKey;

        public BinarySecretKeyIdentifierClause(byte[] key)
            : this(key, true)
        {
        }

        public BinarySecretKeyIdentifierClause(byte[] key, bool cloneBuffer)
            : this(key, cloneBuffer, null, 0)
        {
        }

        public BinarySecretKeyIdentifierClause(byte[] key, bool cloneBuffer, byte[] derivationNonce, int derivationLength)
            : base(XD.TrustFeb2005Dictionary.BinarySecretClauseType.Value, key, cloneBuffer, derivationNonce, derivationLength)
        {
        }

        public byte[] GetKeyBytes()
        {
            return GetBuffer();
        }

        public override bool CanCreateKey
        {
            get { return true; }
        }

        public override SecurityKey CreateKey()
        {
            if (this.symmetricKey == null)
                this.symmetricKey = new InMemorySymmetricSecurityKey(GetBuffer(), false);

            return this.symmetricKey;
        }

        public override bool Matches(SecurityKeyIdentifierClause keyIdentifierClause)
        {
            BinarySecretKeyIdentifierClause that = keyIdentifierClause as BinarySecretKeyIdentifierClause;

            // PreSharp 
#pragma warning suppress 56506
            return ReferenceEquals(this, that) || (that != null && that.Matches(this.GetRawBuffer()));
        }
    }
}
