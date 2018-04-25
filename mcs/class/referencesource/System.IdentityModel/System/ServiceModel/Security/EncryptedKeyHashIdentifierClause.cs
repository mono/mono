//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------
namespace System.ServiceModel.Security
{
    using System.Globalization;
    using System.IdentityModel.Tokens;
    using System.Runtime.CompilerServices;

    [TypeForwardedFrom("System.ServiceModel, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089")]
    sealed class EncryptedKeyHashIdentifierClause : BinaryKeyIdentifierClause
    {
        public EncryptedKeyHashIdentifierClause(byte[] encryptedKeyHash)
            : this(encryptedKeyHash, true)
        {
        }

        internal EncryptedKeyHashIdentifierClause(byte[] encryptedKeyHash, bool cloneBuffer)
            : this(encryptedKeyHash, cloneBuffer, null, 0)
        {
        }

        internal EncryptedKeyHashIdentifierClause(byte[] encryptedKeyHash, bool cloneBuffer, byte[] derivationNonce, int derivationLength)
            : base(null, encryptedKeyHash, cloneBuffer, derivationNonce, derivationLength)
        {
        }

        public byte[] GetEncryptedKeyHash()
        {
            return GetBuffer();
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "EncryptedKeyHashIdentifierClause(Hash = {0})", Convert.ToBase64String(GetRawBuffer()));
        }
    }
}
