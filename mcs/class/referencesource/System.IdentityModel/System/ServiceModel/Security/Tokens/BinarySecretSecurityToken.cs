//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Security.Tokens
{
    using System.Collections.ObjectModel;
    using System.IdentityModel;
    using System.IdentityModel.Tokens;
    using System.Runtime.CompilerServices;

    [TypeForwardedFrom("System.ServiceModel, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089")]
    public class BinarySecretSecurityToken : SecurityToken
    {
        string id;
        DateTime effectiveTime;
        byte[] key;
        ReadOnlyCollection<SecurityKey> securityKeys;

        public BinarySecretSecurityToken(int keySizeInBits)
            : this(SecurityUniqueId.Create().Value, keySizeInBits)
        {
        }

        public BinarySecretSecurityToken(string id, int keySizeInBits)
            : this(id, keySizeInBits, true)
        {
        }

        public BinarySecretSecurityToken(byte[] key)
            : this(SecurityUniqueId.Create().Value, key)
        {
        }

        public BinarySecretSecurityToken(string id, byte[] key)
            : this(id, key, true)
        {
        }

        protected BinarySecretSecurityToken(string id, int keySizeInBits, bool allowCrypto)
        {
            if (keySizeInBits <= 0 || keySizeInBits >= 512)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("keySizeInBits", SR.GetString(SR.ValueMustBeInRange, 0, 512)));
            }

            if ((keySizeInBits % 8) != 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("keySizeInBits", SR.GetString(SR.KeyLengthMustBeMultipleOfEight, keySizeInBits)));
            }

            this.id = id;
            this.effectiveTime = DateTime.UtcNow;
            this.key = new byte[keySizeInBits / 8];
            CryptoHelper.FillRandomBytes(this.key);

            if (allowCrypto)
            {
                this.securityKeys = SecurityUtils.CreateSymmetricSecurityKeys(this.key);
            }
            else
            {
                this.securityKeys = EmptyReadOnlyCollection<SecurityKey>.Instance;
            }
        }

        protected BinarySecretSecurityToken(string id, byte[] key, bool allowCrypto)
        {
            if (key == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("key");

            this.id = id;
            this.effectiveTime = DateTime.UtcNow;
            this.key = new byte[key.Length];
            Buffer.BlockCopy(key, 0, this.key, 0, key.Length);
            if (allowCrypto)
            {
                this.securityKeys = SecurityUtils.CreateSymmetricSecurityKeys(this.key);
            }
            else
            {
                this.securityKeys = EmptyReadOnlyCollection<SecurityKey>.Instance;
            }
        }

        public override string Id
        {
            get { return this.id; }
        }

        public override DateTime ValidFrom
        {
            get { return this.effectiveTime; }
        }

        public override DateTime ValidTo
        {
            // Never expire
            get { return DateTime.MaxValue; }
        }

        public int KeySize
        {
            get { return (this.key.Length * 8); }
        }

        public override ReadOnlyCollection<SecurityKey> SecurityKeys
        {
            get { return this.securityKeys; }
        }

        public byte[] GetKeyBytes()
        {
            return SecurityUtils.CloneBuffer(this.key);
        }
    }
}
