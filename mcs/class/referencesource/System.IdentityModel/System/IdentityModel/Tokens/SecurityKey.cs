//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.IdentityModel.Tokens
{
    public abstract class SecurityKey
    {
        public abstract int KeySize { get; }
        public abstract byte[] DecryptKey(string algorithm, byte[] keyData);
        public abstract byte[] EncryptKey(string algorithm, byte[] keyData);
        public abstract bool IsAsymmetricAlgorithm(string algorithm);
        public abstract bool IsSupportedAlgorithm(string algorithm);
        public abstract bool IsSymmetricAlgorithm(string algorithm);
    }
}
