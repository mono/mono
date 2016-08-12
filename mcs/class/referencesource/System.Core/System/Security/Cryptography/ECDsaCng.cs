// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==

using System;
using System.Diagnostics;
using System.IO;
using System.Security;
using System.Security.Permissions;
using System.Diagnostics.Contracts;
using Microsoft.Win32.SafeHandles;

namespace System.Security.Cryptography {
    /// <summary>
    ///     Wrapper for NCrypt's implementation of elliptic curve DSA
    /// </summary>
    [System.Security.Permissions.HostProtection(MayLeakOnAbort = true)]
    public sealed class ECDsaCng : ECDsa {
#if MONO
        public ECDsaCng() : this(521) {
        }

        public ECDsaCng(int keySize) {
            throw new NotImplementedException ();
        }

        [SecuritySafeCritical]
        public ECDsaCng(CngKey key) {
            throw new NotImplementedException ();
        }

#if NETSTANDARD
        public ECDsaCng(ECCurve curve) {
            throw new NotImplementedException ();
        }
#endif

        public CngKey Key {
            get {
                throw new NotImplementedException ();
            }

            private set {
                throw new NotImplementedException ();
            }
        }

        public override byte[] SignHash(byte[] hash) {
            throw new NotImplementedException();
        }

        public override bool VerifyHash(byte[] hash, byte[] signature) {
            throw new NotImplementedException();
        }
#else
        private static KeySizes[] s_legalKeySizes = new KeySizes[] { new KeySizes(256, 384, 128), new KeySizes(521, 521, 0) };

        private CngKey m_key;
        private CngAlgorithm m_hashAlgorithm = CngAlgorithm.Sha256;

        //
        // Constructors
        //

        public ECDsaCng() : this(521) {
            Contract.Ensures(LegalKeySizesValue != null);
        }

        public ECDsaCng(int keySize) {
            Contract.Ensures(LegalKeySizesValue != null);

            if (!NCryptNative.NCryptSupported) {
                throw new PlatformNotSupportedException(SR.GetString(SR.Cryptography_PlatformNotSupported));
            }

            LegalKeySizesValue = s_legalKeySizes;
            KeySize = keySize;
        }

        [SecuritySafeCritical]
        public ECDsaCng(CngKey key) {
            Contract.Ensures(LegalKeySizesValue != null);
            Contract.Ensures(m_key != null && IsEccAlgorithmGroup(m_key.AlgorithmGroup));

            if (key == null) {
                throw new ArgumentNullException("key");
            }
            if (!IsEccAlgorithmGroup(key.AlgorithmGroup)) {
                throw new ArgumentException(SR.GetString(SR.Cryptography_ArgECDsaRequiresECDsaKey), "key");
            }

            if (!NCryptNative.NCryptSupported) {
                throw new PlatformNotSupportedException(SR.GetString(SR.Cryptography_PlatformNotSupported));
            }

            LegalKeySizesValue = s_legalKeySizes;

            // Make a copy of the key so that we continue to work if it gets disposed before this algorithm
            //
            // This requires an assert for UnmanagedCode since we'll need to access the raw handles of the key
            // and the handle constructor of CngKey.  The assert is safe since ECDsaCng will never expose the
            // key handles to calling code (without first demanding UnmanagedCode via the Handle property of
            // CngKey).
            //
            // We also need to dispose of the key handle since CngKey.Handle returns a duplicate
            new SecurityPermission(SecurityPermissionFlag.UnmanagedCode).Assert();
            using (SafeNCryptKeyHandle keyHandle = key.Handle) {
                Key = CngKey.Open(keyHandle, key.IsEphemeral ? CngKeyHandleOpenOptions.EphemeralKey : CngKeyHandleOpenOptions.None);
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
        ///     Hash algorithm to use when generating a signature over arbitrary data
        /// </summary>
        public CngAlgorithm HashAlgorithm {
            get {
                Contract.Ensures(Contract.Result<CngAlgorithm>() != null);
                return m_hashAlgorithm;
            }

            set {
                Contract.Ensures(m_hashAlgorithm != null);

                if (value == null) {
                    throw new ArgumentNullException("value");
                }

                m_hashAlgorithm = value;
            }
        }

        /// <summary>
        ///     Key to use for signing
        /// </summary>
        public CngKey Key {
            get {
                Contract.Ensures(Contract.Result<CngKey>() != null);
                Contract.Ensures(IsEccAlgorithmGroup(Contract.Result<CngKey>().AlgorithmGroup));
                Contract.Ensures(m_key != null && IsEccAlgorithmGroup(m_key.AlgorithmGroup));

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
                            algorithm = CngAlgorithm.ECDsaP256;
                            break;

                        case 384:
                            algorithm = CngAlgorithm.ECDsaP384;
                            break;

                        case 521:
                            algorithm = CngAlgorithm.ECDsaP521;
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
                Contract.Ensures(m_key != null && IsEccAlgorithmGroup(m_key.AlgorithmGroup));

                if (!IsEccAlgorithmGroup(value.AlgorithmGroup)) {
                    throw new ArgumentException(SR.GetString(SR.Cryptography_ArgECDsaRequiresECDsaKey));
                }

                if (m_key != null) {
                    m_key.Dispose();
                }

                //
                // We do not duplicate the handle because the only time the user has access to the key itself
                // to dispose underneath us is when they construct via the CngKey constructor, which does a
                // copy. Otherwise all key lifetimes are controlled directly by the ECDsaCng class.
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
        ///     Clean up the algorithm
        /// </summary>
        protected override void Dispose(bool disposing) {
            try {
                if (m_key != null) {
                    m_key.Dispose();
                }
            }
            finally {
                base.Dispose(disposing);
            }
        }

        //
        // XML Import
        //
        // #ECCXMLFormat
        //
        // There is currently not a standard XML format for ECC keys, so we will not implement the default
        // To/FromXmlString so that we're not tied to one format when a standard one does exist. Instead we'll
        // use an overload which allows the user to specify the format they'd like to serialize into.
        //
        // See code:System.Security.Cryptography.Rfc4050KeyFormatter#RFC4050ECKeyFormat for information about
        // the currently supported format.
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
        // Signature generation
        //

        public byte[] SignData(byte[] data) {
            Contract.Ensures(Contract.Result<byte[]>() != null);

            if (data == null) {
                throw new ArgumentNullException("data");
            }

            return SignData(data, 0, data.Length);
        }

        [SecuritySafeCritical]
        public byte[] SignData(byte[] data, int offset, int count) {
            Contract.Ensures(Contract.Result<byte[]>() != null);

            if (data == null) {
                throw new ArgumentNullException("data");
            }
            if (offset < 0 || offset > data.Length) {
                throw new ArgumentOutOfRangeException("offset");
            }
            if (count < 0 || count > data.Length - offset) {
                throw new ArgumentOutOfRangeException("count");
            }

            using (BCryptHashAlgorithm hashAlgorithm = new BCryptHashAlgorithm(HashAlgorithm, BCryptNative.ProviderName.MicrosoftPrimitiveProvider)) {
                hashAlgorithm.HashCore(data, offset, count);
                byte[] hashValue = hashAlgorithm.HashFinal();

                return SignHash(hashValue);
            }
        }

        [SecuritySafeCritical]
        public byte[] SignData(Stream data) {
            Contract.Ensures(Contract.Result<byte[]>() != null);

            if (data == null) {
                throw new ArgumentNullException("data");
            }

            using (BCryptHashAlgorithm hashAlgorithm = new BCryptHashAlgorithm(HashAlgorithm, BCryptNative.ProviderName.MicrosoftPrimitiveProvider)) {
                hashAlgorithm.HashStream(data);
                byte[] hashValue = hashAlgorithm.HashFinal();

                return SignHash(hashValue);
            }
        }

        [SecuritySafeCritical]
        public override byte[] SignHash(byte[] hash) {
            if (hash == null) {
                throw new ArgumentNullException("hash");
            }

            // Make sure we're allowed to sign using this key
            KeyContainerPermission permission = Key.BuildKeyContainerPermission(KeyContainerPermissionFlags.Sign);
            if (permission != null) {
                permission.Demand();
            }

            // Now that know we have permission to use this key for signing, pull the key value out, which
            // will require unmanaged code permission
            new SecurityPermission(SecurityPermissionFlag.UnmanagedCode).Assert();

            // This looks odd, but the key handle is actually a duplicate so we need to dispose it
            using (SafeNCryptKeyHandle keyHandle = Key.Handle) {
                CodeAccessPermission.RevertAssert();

                return NCryptNative.SignHash(keyHandle, hash);
            }
        }

        //
        // XML Export
        //
        // See  code:System.Security.Cryptography.ECDsaCng#ECCXMLFormat and 
        // code:System.Security.Cryptography.Rfc4050KeyFormatter#RFC4050ECKeyFormat for information about
        // XML serialization of elliptic curve keys
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

        //
        // Signature verification
        //

        public bool VerifyData(byte[] data, byte[] signature) {
            if (data == null) {
                throw new ArgumentNullException("data");
            }

            return VerifyData(data, 0, data.Length, signature);
        }

        [SecuritySafeCritical]
        public bool VerifyData(byte[] data, int offset, int count, byte[] signature) {
            if (data == null) {
                throw new ArgumentNullException("data");
            }
            if (offset < 0 || offset > data.Length) {
                throw new ArgumentOutOfRangeException("offset");
            }
            if (count < 0 || count > data.Length - offset) {
                throw new ArgumentOutOfRangeException("count");
            }
            if (signature == null) {
                throw new ArgumentNullException("signature");
            }

            using (BCryptHashAlgorithm hashAlgorithm = new BCryptHashAlgorithm(HashAlgorithm, BCryptNative.ProviderName.MicrosoftPrimitiveProvider)) {
                hashAlgorithm.HashCore(data, offset, count);
                byte[] hashValue = hashAlgorithm.HashFinal();

                return VerifyHash(hashValue, signature);
            }
        }

        [SecuritySafeCritical]
        public bool VerifyData(Stream data, byte[] signature) {
            if (data == null) {
                throw new ArgumentNullException("data");
            }
            if (signature == null) {
                throw new ArgumentNullException("signature");
            }

            using (BCryptHashAlgorithm hashAlgorithm = new BCryptHashAlgorithm(HashAlgorithm, BCryptNative.ProviderName.MicrosoftPrimitiveProvider)) {
                hashAlgorithm.HashStream(data);
                byte[] hashValue = hashAlgorithm.HashFinal();

                return VerifyHash(hashValue, signature);
            }
        }

        [SecuritySafeCritical]
        public override bool VerifyHash(byte[] hash, byte[] signature) {
            if (hash == null) {
                throw new ArgumentNullException("hash");
            }
            if (signature == null) {
                throw new ArgumentNullException("signature");
            }

            // We need to get the raw key handle to verify the signature. Asserting here is safe since verifiation
            // is not a protected operation, and we do not expose the handle to the user code.
            new SecurityPermission(SecurityPermissionFlag.UnmanagedCode).Assert();

            // This looks odd, but Key.Handle is really a duplicate so we need to dispose it
            using (SafeNCryptKeyHandle keyHandle = Key.Handle) {
                CodeAccessPermission.RevertAssert();

                return NCryptNative.VerifySignature(keyHandle, hash, signature);
            }
        }

        /// <summary>
        ///     Helper property to get the NCrypt key handle
        /// </summary>
        private SafeNCryptKeyHandle KeyHandle {
            [SecuritySafeCritical]
            get { return Key.Handle; }
        }

        protected override byte[] HashData(byte[] data, int offset, int count, HashAlgorithmName hashAlgorithm) {
            // we're sealed and the base should have checked this before calling us
            Debug.Assert(data != null);
            Debug.Assert(offset >= 0 && offset <= data.Length);
            Debug.Assert(count >= 0 && count <= data.Length - offset);
            Debug.Assert(!String.IsNullOrEmpty(hashAlgorithm.Name));

            using (BCryptHashAlgorithm hasher = new BCryptHashAlgorithm(new CngAlgorithm(hashAlgorithm.Name), BCryptNative.ProviderName.MicrosoftPrimitiveProvider)) {
                hasher.HashCore(data, offset, count);
                return hasher.HashFinal();
            }
        }

        protected override byte[] HashData(Stream data, HashAlgorithmName hashAlgorithm) {
            // we're sealed and the base should have checked this before calling us
            Debug.Assert(data != null);
            Debug.Assert(!String.IsNullOrEmpty(hashAlgorithm.Name));

            using (BCryptHashAlgorithm hasher = new BCryptHashAlgorithm(new CngAlgorithm(hashAlgorithm.Name), BCryptNative.ProviderName.MicrosoftPrimitiveProvider)) {
                hasher.HashStream(data);
                return hasher.HashFinal();
            }
        }

        private static bool IsEccAlgorithmGroup(CngAlgorithmGroup algorithmGroup)
        {
            // Sometimes, when reading from certificates, ECDSA keys get identified as ECDH.
            // Windows allows the ECDH keys to perform both key exchange (ECDH) and signing (ECDSA),
            // so either value is acceptable for the ECDSA wrapper object.
            //
            // It is worth noting, however, that ECDSA-identified keys cannot be used for key exchange (ECDH) in CNG.
            return algorithmGroup == CngAlgorithmGroup.ECDsa || algorithmGroup == CngAlgorithmGroup.ECDiffieHellman;
        }
#endif
    }
}
