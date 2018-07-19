//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.IdentityModel.Tokens
{
    using System.Security.Cryptography;

    public abstract class SymmetricSecurityKey : SecurityKey
    {
        public abstract byte[] GenerateDerivedKey(string algorithm, byte[] label, byte[] nonce, int derivedKeyLength, int offset);
        public abstract ICryptoTransform GetDecryptionTransform(string algorithm, byte[] iv);
        public abstract ICryptoTransform GetEncryptionTransform(string algorithm, byte[] iv);
        public abstract int GetIVSize(string algorithm);
        public abstract KeyedHashAlgorithm GetKeyedHashAlgorithm(string algorithm);
        public abstract SymmetricAlgorithm GetSymmetricAlgorithm(string algorithm);
        public abstract byte[] GetSymmetricKey();
    }
}
