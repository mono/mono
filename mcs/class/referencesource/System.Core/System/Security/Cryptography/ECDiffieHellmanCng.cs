// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Permissions;
using System.Diagnostics.Contracts;
using Microsoft.Win32.SafeHandles;

namespace System.Security.Cryptography {
    /// <summary>
    ///     Key derivation functions used to transform the raw secret agreement into key material
    /// </summary>
    public enum ECDiffieHellmanKeyDerivationFunction {
        Hash,
        Hmac,
        Tls
    }

    /// <summary>
    ///     Wrapper for CNG's implementation of elliptic curve Diffie-Hellman key exchange
    /// </summary>
    [System.Security.Permissions.HostProtection(MayLeakOnAbort = true)]
    public sealed class ECDiffieHellmanCng : ECDiffieHellman {
        private static KeySizes[] s_legalKeySizes = new KeySizes[] { new KeySizes(256, 384, 128), new KeySizes(521, 521, 0) };

        private CngAlgorithm m_hashAlgorithm = CngAlgorithm.Sha256;
        private byte[] m_hmacKey;
        private CngKey m_key;
        private ECDiffieHellmanKeyDerivationFunction m_kdf = ECDiffieHellmanKeyDerivationFunction.Hash;
        private byte[] m_label;
        private byte[] m_secretAppend;
        private byte[] m_secretPrepend;
        private byte[] m_seed;

        //
        // Constructors
        //

        public ECDiffieHellmanCng() : this(521) {
            Contract.Ensures(LegalKeySizesValue != null);
        }

        public ECDiffieHellmanCng(int keySize) {
            Contract.Ensures(LegalKeySizesValue != null);

            if (!NCryptNative.NCryptSupported) {
                throw new PlatformNotSupportedException(SR.GetString(SR.Cryptography_PlatformNotSupported));
            }

            LegalKeySizesValue = s_legalKeySizes;
            KeySize = keySize;
        }

        [SecuritySafeCritical]
        public ECDiffieHellmanCng(CngKey key) {
            Contract.Ensures(LegalKeySizesValue != null);
            Contract.Ensures(m_key != null && m_key.AlgorithmGroup == CngAlgorithmGroup.ECDiffieHellman);

            if (key == null) {
                throw new ArgumentNullException("key");
            }
            if (key.AlgorithmGroup != CngAlgorithmGroup.ECDiffieHellman) {
                throw new ArgumentException(SR.GetString(SR.Cryptography_ArgECDHRequiresECDHKey), "key");
            }

            if (!NCryptNative.NCryptSupported) {
                throw new PlatformNotSupportedException(SR.GetString(SR.Cryptography_PlatformNotSupported));
            }

            LegalKeySizesValue = s_legalKeySizes;

            // Make a copy of the key so that we continue to work if it gets disposed before this algorithm
            //
            // This requires an assert for UnmanagedCode since we'll need to access the raw handles of the key
            // and the handle constructor of CngKey.  The assert is safe since ECDiffieHellmanCng will never
            // expose the key handles to calling code (without first demanding UnmanagedCode via the Handle
            // property of CngKey).
            //
            // The bizzare looking disposal of the key.Handle property is intentional - Handle returns a
            // duplicate - without disposing it, we keep the key alive until the GC runs.
            new SecurityPermission(SecurityPermissionFlag.UnmanagedCode).Assert();
            using (SafeNCryptKeyHandle importHandle = key.Handle) {
                Key = CngKey.Open(importHandle, key.IsEphemeral ? CngKeyHandleOpenOptions.EphemeralKey : CngKeyHandleOpenOptions.None);
            }
            CodeAccessPermission.RevertAssert();

            // Our LegalKeySizes value stores the values that we encoded as being the correct
            // legal key size limitations for this algorithm, as documented on MSDN.
            //
            // But on a new OS version we might not question if our limit is accurate, or MSDN
            // could have been innacurate to start with.
            //
            // Since the key is already loaded, we know that Windows thought it to be valid;
            // therefore we should set KeySizeValue directly to bypass the LegalKeySizes conformance
            // check.
            //
            // For RSA there are known cases where this change matters. RSACryptoServiceProvider can
            // create a 384-bit RSA key, which we consider too small to be legal. It can also create
            // a 1032-bit RSA key, which we consider illegal because it doesn't match our 64-bit
            // alignment requirement. (In both cases Windows loads it just fine)
            KeySizeValue = m_key.KeySize;
        }

        /// <summary>
        ///     Hash algorithm used with the Hash and HMAC KDFs
        /// </summary>
        public CngAlgorithm HashAlgorithm {
            get {
                Contract.Ensures(Contract.Result<CngAlgorithm>() != null);
                return m_hashAlgorithm;
            }

            set {
                Contract.Ensures(m_hashAlgorithm != null);

                if (m_hashAlgorithm == null) {
                    throw new ArgumentNullException("value");
                }

                m_hashAlgorithm = value;
            }
        }

        /// <summary>
        ///     Key used with the HMAC KDF
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "Reviewed API design exception since these are really setters for explicit byte arrays rather than properties that will be iterated by users")]
        public byte[] HmacKey {
            get { return m_hmacKey; }
            set { m_hmacKey = value; }
        }

        /// <summary>
        ///     KDF used to transform the secret agreement into key material
        /// </summary>
        public ECDiffieHellmanKeyDerivationFunction KeyDerivationFunction {
            get {
                Contract.Ensures(Contract.Result<ECDiffieHellmanKeyDerivationFunction>() >= ECDiffieHellmanKeyDerivationFunction.Hash &&
                                 Contract.Result<ECDiffieHellmanKeyDerivationFunction>() <= ECDiffieHellmanKeyDerivationFunction.Tls);

                return m_kdf;
            }

            set {
                Contract.Ensures(m_kdf >= ECDiffieHellmanKeyDerivationFunction.Hash &&
                                        m_kdf <= ECDiffieHellmanKeyDerivationFunction.Tls);

                if (value < ECDiffieHellmanKeyDerivationFunction.Hash || value > ECDiffieHellmanKeyDerivationFunction.Tls) {
                    throw new ArgumentOutOfRangeException("value");
                }

                m_kdf = value;
            }
        }

        /// <summary>
        ///     Label bytes used for the TLS KDF
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "Reviewed API design exception since these are really setters for explicit byte arrays rather than properties that will be iterated by users")]
        public byte[] Label {
            get { return m_label; }
            set { m_label = value; }
        }

        /// <summary>
        ///     Bytes to append to the raw secret agreement before processing by the KDF
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "Reviewed API design exception since these are really setters for explicit byte arrays rather than properties that will be iterated by users")]
        public byte[] SecretAppend {
            get { return m_secretAppend; }
            set { m_secretAppend = value; }
        }

        /// <summary>
        ///     Bytes to prepend to the raw secret agreement before processing by the KDF
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "Reviewed API design exception since these are really setters for explicit byte arrays rather than properties that will be iterated by users")]
        public byte[] SecretPrepend {
            get { return m_secretPrepend; }
            set { m_secretPrepend = value; }
        }

        /// <summary>
        ///     Seed bytes used for the TLS KDF
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "Reviewed API design exception since these are really setters for explicit byte arrays rather than properties that will be iterated by users")]
        public byte[] Seed {
            get { return m_seed; }
            set { m_seed = value; }
        }

        /// <summary>
        ///     Full key pair being used for key generation
        /// </summary>
        public CngKey Key {
            get {
                Contract.Ensures(Contract.Result<CngKey>() != null);
                Contract.Ensures(Contract.Result<CngKey>().AlgorithmGroup == CngAlgorithmGroup.ECDiffieHellman);
                Contract.Ensures(m_key != null && m_key.AlgorithmGroup == CngAlgorithmGroup.ECDiffieHellman);

                // If the size of the key no longer matches our stored value, then we need to replace it with
                // a new key of the correct size.
                if (m_key != null && m_key.KeySize != KeySize) {
                    m_key.Dispose();
                    m_key = null;
                }

                if (m_key == null) {
                    // Map the current key size to a CNG algorithm name
                    CngAlgorithm algorithm = null;
                    switch (KeySize) {
                        case 256:
                            algorithm = CngAlgorithm.ECDiffieHellmanP256;
                            break;

                        case 384:
                            algorithm = CngAlgorithm.ECDiffieHellmanP384;
                            break;

                        case 521:
                            algorithm = CngAlgorithm.ECDiffieHellmanP521;
                            break;

                        default:
                            Debug.Assert(false, "Illegal key size set");
                            break;
                    }

                    m_key = CngKey.Create(algorithm);
                }

                return m_key;
            }

            private set {
                Contract.Requires(value != null);
                Contract.Ensures(m_key != null && m_key.AlgorithmGroup == CngAlgorithmGroup.ECDiffieHellman);

                if (value.AlgorithmGroup != CngAlgorithmGroup.ECDiffieHellman) {
                    throw new ArgumentException(SR.GetString(SR.Cryptography_ArgECDHRequiresECDHKey));
                }

                if (m_key != null) {
                    m_key.Dispose();
                }

                //
                // We do not duplicate the handle because the only time the user has access to the key itself
                // to dispose underneath us is when they construct via the CngKey constructor, which does a
                // duplication. Otherwise all key lifetimes are controlled directly by the ECDiffieHellmanCng
                // class.
                //

                m_key = value;

                // Our LegalKeySizes value stores the values that we encoded as being the correct
                // legal key size limitations for this algorithm, as documented on MSDN.
                //
                // But on a new OS version we might not question if our limit is accurate, or MSDN
                // could have been innacurate to start with.
                //
                // Since the key is already loaded, we know that Windows thought it to be valid;
                // therefore we should set KeySizeValue directly to bypass the LegalKeySizes conformance
                // check.
                //
                // For RSA there are known cases where this change matters. RSACryptoServiceProvider can
                // create a 384-bit RSA key, which we consider too small to be legal. It can also create
                // a 1032-bit RSA key, which we consider illegal because it doesn't match our 64-bit
                // alignment requirement. (In both cases Windows loads it just fine)
                KeySizeValue = m_key.KeySize;
            }
        }

        /// <summary>
        ///     Public key used to generate key material with the second party
        /// </summary>
        public override ECDiffieHellmanPublicKey PublicKey {
            get {
                Contract.Ensures(Contract.Result<ECDiffieHellmanPublicKey>() != null);
                return new ECDiffieHellmanCngPublicKey(Key);
            }
        }

        /// <summary>
        ///     Use the secret agreement as the HMAC key rather than supplying a seperate one
        /// </summary>
        public bool UseSecretAgreementAsHmacKey {
            get { return HmacKey == null; }
        }

        /// <summary>
        ///     Given a second party's public key, derive shared key material
        /// </summary>
        public override byte[] DeriveKeyMaterial(ECDiffieHellmanPublicKey otherPartyPublicKey) {
            Contract.Ensures(Contract.Result<byte[]>() != null);
            Contract.Assert(m_kdf >= ECDiffieHellmanKeyDerivationFunction.Hash &&
                            m_kdf <= ECDiffieHellmanKeyDerivationFunction.Tls);

            if (otherPartyPublicKey == null) {
                throw new ArgumentNullException("otherPartyPublicKey");
            }

            // We can only work with ECDiffieHellmanCngPublicKeys
            ECDiffieHellmanCngPublicKey otherKey = otherPartyPublicKey as ECDiffieHellmanCngPublicKey;
            if (otherPartyPublicKey == null) {
                throw new ArgumentException(SR.GetString(SR.Cryptography_ArgExpectedECDiffieHellmanCngPublicKey));
            }

            using (CngKey import = otherKey.Import()) {
                return DeriveKeyMaterial(import);
            }
        }

        /// <summary>
        ///     Given a second party's public key, derive shared key material
        /// </summary>
        [SecuritySafeCritical]
        public byte[] DeriveKeyMaterial(CngKey otherPartyPublicKey) {
            Contract.Ensures(Contract.Result<byte[]>() != null);
            Contract.Assert(m_kdf >= ECDiffieHellmanKeyDerivationFunction.Hash &&
                            m_kdf <= ECDiffieHellmanKeyDerivationFunction.Tls);

            if (otherPartyPublicKey == null) {
                throw new ArgumentNullException("otherPartyPublicKey");
            }
            if (otherPartyPublicKey.AlgorithmGroup != CngAlgorithmGroup.ECDiffieHellman) {
                throw new ArgumentException(SR.GetString(SR.Cryptography_ArgECDHRequiresECDHKey), "otherPartyPublicKey");
            }
            if (otherPartyPublicKey.KeySize != KeySize) {
                throw new ArgumentException(SR.GetString(SR.Cryptography_ArgECDHKeySizeMismatch), "otherPartyPublicKey");
            }

            NCryptNative.SecretAgreementFlags flags =
                UseSecretAgreementAsHmacKey ? NCryptNative.SecretAgreementFlags.UseSecretAsHmacKey : NCryptNative.SecretAgreementFlags.None;

            // We require access to the handles for generating key material. This is safe since we will never
            // expose these handles to user code
            new SecurityPermission(SecurityPermissionFlag.UnmanagedCode).Assert();

            // This looks horribly wrong - but accessing the handle property actually returns a duplicate handle, which
            // we need to dispose of - otherwise, we're stuck keepign the resource alive until the GC runs.  This explicitly
            // is not disposing of the handle underlying the key dispite what the syntax looks like.
            using (SafeNCryptKeyHandle localKey = Key.Handle)
            using (SafeNCryptKeyHandle otherKey = otherPartyPublicKey.Handle) {
                CodeAccessPermission.RevertAssert();

                //
                // Generating key material is a two phase process.
                //   1. Generate the secret agreement
                //   2. Pass the secret agreement through a KDF to get key material
                //

                using (SafeNCryptSecretHandle secretAgreement = NCryptNative.DeriveSecretAgreement(localKey, otherKey)) {
                    if (KeyDerivationFunction == ECDiffieHellmanKeyDerivationFunction.Hash) {
                        byte[] secretAppend = SecretAppend == null ? null : SecretAppend.Clone() as byte[];
                        byte[] secretPrepend = SecretPrepend == null ? null : SecretPrepend.Clone() as byte[];

                        return NCryptNative.DeriveKeyMaterialHash(secretAgreement,
                                                                  HashAlgorithm.Algorithm,
                                                                  secretPrepend,
                                                                  secretAppend,
                                                                  flags);
                    }
                    else if (KeyDerivationFunction == ECDiffieHellmanKeyDerivationFunction.Hmac) {
                        byte[] hmacKey = HmacKey == null ? null : HmacKey.Clone() as byte[];
                        byte[] secretAppend = SecretAppend == null ? null : SecretAppend.Clone() as byte[];
                        byte[] secretPrepend = SecretPrepend == null ? null : SecretPrepend.Clone() as byte[];

                        return NCryptNative.DeriveKeyMaterialHmac(secretAgreement,
                                                                  HashAlgorithm.Algorithm,
                                                                  hmacKey,
                                                                  secretPrepend,
                                                                  secretAppend,
                                                                  flags);
                    }
                    else {
                        Debug.Assert(KeyDerivationFunction == ECDiffieHellmanKeyDerivationFunction.Tls, "Unknown KDF");

                        byte[] label = Label == null ? null : Label.Clone() as byte[];
                        byte[] seed = Seed == null ? null : Seed.Clone() as byte[];

                        if (label == null || seed == null) {
                            throw new InvalidOperationException(SR.GetString(SR.Cryptography_TlsRequiresLabelAndSeed));
                        }

                        return NCryptNative.DeriveKeyMaterialTls(secretAgreement, label, seed, flags);
                    }
                }
            }
        }

        [SecuritySafeCritical]
        public override byte[] DeriveKeyFromHash(
            ECDiffieHellmanPublicKey otherPartyPublicKey,
            HashAlgorithmName hashAlgorithm,
            byte[] secretPrepend,
            byte[] secretAppend)
        {
            Contract.Ensures(Contract.Result<byte[]>() != null);

            if (otherPartyPublicKey == null)
                throw new ArgumentNullException("otherPartyPublicKey");
            if (string.IsNullOrEmpty(hashAlgorithm.Name))
                throw new ArgumentException(SR.GetString(SR.Cryptography_HashAlgorithmNameNullOrEmpty), "hashAlgorithm");

            using (SafeNCryptSecretHandle secretAgreement = DeriveSecretAgreementHandle(otherPartyPublicKey))
            {
                return NCryptNative.DeriveKeyMaterialHash(
                    secretAgreement,
                    hashAlgorithm.Name, 
                    secretPrepend,
                    secretAppend,
                    NCryptNative.SecretAgreementFlags.None);
            }
        }

        [SecuritySafeCritical]
        public override byte[] DeriveKeyFromHmac(
            ECDiffieHellmanPublicKey otherPartyPublicKey,
            HashAlgorithmName hashAlgorithm,
            byte[] hmacKey,
            byte[] secretPrepend,
            byte[] secretAppend)
        {
            Contract.Ensures(Contract.Result<byte[]>() != null);

            if (otherPartyPublicKey == null)
                throw new ArgumentNullException("otherPartyPublicKey");
            if (string.IsNullOrEmpty(hashAlgorithm.Name))
                throw new ArgumentException(SR.GetString(SR.Cryptography_HashAlgorithmNameNullOrEmpty), "hashAlgorithm");

            using (SafeNCryptSecretHandle secretAgreement = DeriveSecretAgreementHandle(otherPartyPublicKey))
            {
                NCryptNative.SecretAgreementFlags flags = hmacKey == null ?
                    NCryptNative.SecretAgreementFlags.UseSecretAsHmacKey :
                    NCryptNative.SecretAgreementFlags.None;

                return NCryptNative.DeriveKeyMaterialHmac(
                    secretAgreement,
                    hashAlgorithm.Name,
                    hmacKey,
                    secretPrepend,
                    secretAppend,
                    flags);
            }
        }

        [SecuritySafeCritical]
        public override byte[] DeriveKeyTls(ECDiffieHellmanPublicKey otherPartyPublicKey, byte[] prfLabel, byte[] prfSeed)
        {
            Contract.Ensures(Contract.Result<byte[]>() != null);

            if (otherPartyPublicKey == null)
                throw new ArgumentNullException("otherPartyPublicKey");
            if (prfLabel == null)
                throw new ArgumentNullException("prfLabel");
            if (prfSeed == null)
                throw new ArgumentNullException("prfSeed");

            using (SafeNCryptSecretHandle secretAgreement = DeriveSecretAgreementHandle(otherPartyPublicKey))
            {
                return NCryptNative.DeriveKeyMaterialTls(
                    secretAgreement,
                    prfLabel,
                    prfSeed,
                    NCryptNative.SecretAgreementFlags.None);
            }
        }

        /// <summary>
        ///     Get a handle to the secret agreement generated between two parties
        /// </summary>
        public SafeNCryptSecretHandle DeriveSecretAgreementHandle(ECDiffieHellmanPublicKey otherPartyPublicKey) {
            if (otherPartyPublicKey == null) {
                throw new ArgumentNullException("otherPartyPublicKey");
            }
            
            // We can only work with ECDiffieHellmanCngPublicKeys
            ECDiffieHellmanCngPublicKey otherKey = otherPartyPublicKey as ECDiffieHellmanCngPublicKey;
            if (otherPartyPublicKey == null) {
                throw new ArgumentException(SR.GetString(SR.Cryptography_ArgExpectedECDiffieHellmanCngPublicKey));
            }

            using (CngKey importedKey = otherKey.Import()) {
                return DeriveSecretAgreementHandle(importedKey);
            }
        }

        /// <summary>
        ///     Get a handle to the secret agreement between two parties
        /// </summary>
        [System.Security.SecurityCritical]
        [SecurityPermission(SecurityAction.Demand, UnmanagedCode = true)]
        public SafeNCryptSecretHandle DeriveSecretAgreementHandle(CngKey otherPartyPublicKey) {
            if (otherPartyPublicKey == null) {
                throw new ArgumentNullException("otherPartyPublicKey");
            }
            if (otherPartyPublicKey.AlgorithmGroup != CngAlgorithmGroup.ECDiffieHellman) {
                throw new ArgumentException(SR.GetString(SR.Cryptography_ArgECDHRequiresECDHKey), "otherPartyPublicKey");
            }
            if (otherPartyPublicKey.KeySize != KeySize) {
                throw new ArgumentException(SR.GetString(SR.Cryptography_ArgECDHKeySizeMismatch), "otherPartyPublicKey");
            }

            // This looks strange, but the Handle property returns a duplicate so we need to dispose of it when we're done
            using (SafeNCryptKeyHandle localHandle = Key.Handle)
            using (SafeNCryptKeyHandle otherPartyHandle = otherPartyPublicKey.Handle) {
                return NCryptNative.DeriveSecretAgreement(localHandle, otherPartyHandle);
            }
        }

        /// <summary>
        ///     Clean up the algorithm
        /// </summary>
        protected override void Dispose(bool disposing) {
            try {
                if (disposing) {
                    if (m_key != null) {
                        m_key.Dispose();
                    }
                }
            }
            finally {
                base.Dispose(disposing);
            }
        }

        //
        // XML Import
        //
        // See code:System.Security.Cryptography.ECDsaCng#ECCXMLFormat and
        // code:System.Security.Cryptography.Rfc4050KeyFormatter#RFC4050ECKeyFormat for information about
        // elliptic curve XML formats.
        //

        public override void FromXmlString(string xmlString) {
            throw new NotImplementedException(SR.GetString(SR.Cryptography_ECXmlSerializationFormatRequired));
        }

        public void FromXmlString(string xml, ECKeyXmlFormat format) {
            if (xml == null) {
                throw new ArgumentNullException("xml");
            }
            if (format != ECKeyXmlFormat.Rfc4050) {
                throw new ArgumentOutOfRangeException("format");
            }

            Key = Rfc4050KeyFormatter.FromXml(xml);
        }

        //
        // XML Export
        //
        // See code:System.Security.Cryptography.ECDsaCng#ECCXMLFormat and
        // code:System.Security.Cryptography.Rfc4050KeyFormatter#RFC4050ECKeyFormat for information about
        // elliptic curve XML formats.
        //

        public override string ToXmlString(bool includePrivateParameters) {
            throw new NotImplementedException(SR.GetString(SR.Cryptography_ECXmlSerializationFormatRequired));
        }

        public string ToXmlString(ECKeyXmlFormat format) {
            Contract.Ensures(Contract.Result<string>() != null);

            if (format != ECKeyXmlFormat.Rfc4050) {
                throw new ArgumentOutOfRangeException("format");
            }

            return Rfc4050KeyFormatter.ToXml(Key);
        }
    }
}
