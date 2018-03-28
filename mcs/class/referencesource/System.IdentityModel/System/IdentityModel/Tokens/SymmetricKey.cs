//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.IdentityModel.Tokens
{
    using System.IdentityModel.Selectors;
    using System.Security.Cryptography;

    public class InMemorySymmetricSecurityKey : SymmetricSecurityKey
    {
        int keySize;
        byte[] symmetricKey;

        public InMemorySymmetricSecurityKey(byte[] symmetricKey)
            : this(symmetricKey, true)
        {
        }

        public InMemorySymmetricSecurityKey(byte[] symmetricKey, bool cloneBuffer)
        {
            if (symmetricKey == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("symmetricKey"));
            }

            if (symmetricKey.Length == 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(SR.GetString(SR.SymmetricKeyLengthTooShort, symmetricKey.Length)));
            }
            this.keySize = symmetricKey.Length * 8;

            if (cloneBuffer)
            {
                this.symmetricKey = new byte[symmetricKey.Length];
                Buffer.BlockCopy(symmetricKey, 0, this.symmetricKey, 0, symmetricKey.Length);
            }
            else
            {
                this.symmetricKey = symmetricKey;
            }
        }

        public override int KeySize
        {
            get { return this.keySize; }
        }

        public override byte[] DecryptKey(string algorithm, byte[] keyData)
        {
            return CryptoHelper.UnwrapKey(this.symmetricKey, keyData, algorithm);
        }

        public override byte[] EncryptKey(string algorithm, byte[] keyData)
        {
            return CryptoHelper.WrapKey(this.symmetricKey, keyData, algorithm);
        }

        public override byte[] GenerateDerivedKey(string algorithm, byte[] label, byte[] nonce, int derivedKeyLength, int offset)
        {
            return CryptoHelper.GenerateDerivedKey(this.symmetricKey, algorithm, label, nonce, derivedKeyLength, offset);
        }

        public override ICryptoTransform GetDecryptionTransform(string algorithm, byte[] iv)
        {
            return CryptoHelper.CreateDecryptor(this.symmetricKey, iv, algorithm);
        }

        public override ICryptoTransform GetEncryptionTransform(string algorithm, byte[] iv)
        {
            return CryptoHelper.CreateEncryptor(this.symmetricKey, iv, algorithm);
        }

        public override int GetIVSize(string algorithm)
        {
            return CryptoHelper.GetIVSize(algorithm);
        }

        public override KeyedHashAlgorithm GetKeyedHashAlgorithm(string algorithm)
        {
            return CryptoHelper.CreateKeyedHashAlgorithm(this.symmetricKey, algorithm);
        }

        public override SymmetricAlgorithm GetSymmetricAlgorithm(string algorithm)
        {
            return CryptoHelper.GetSymmetricAlgorithm(this.symmetricKey, algorithm);
        }

        public override byte[] GetSymmetricKey()
        {
            byte[] local = new byte[this.symmetricKey.Length];
            Buffer.BlockCopy(this.symmetricKey, 0, local, 0, this.symmetricKey.Length);

            return local;
        }

        public override bool IsAsymmetricAlgorithm(string algorithm)
        {
            return (CryptoHelper.IsAsymmetricAlgorithm(algorithm));
        }

        public override bool IsSupportedAlgorithm(string algorithm)
        {
            return (CryptoHelper.IsSymmetricSupportedAlgorithm(algorithm, this.KeySize));
        }

        public override bool IsSymmetricAlgorithm(string algorithm)
        {
            return CryptoHelper.IsSymmetricAlgorithm(algorithm);
        }
    }
}
