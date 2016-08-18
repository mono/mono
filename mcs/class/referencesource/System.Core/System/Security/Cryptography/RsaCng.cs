using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using Microsoft.Win32.SafeHandles;

namespace System.Security.Cryptography
{
    public sealed class RSACng : RSA
    {
#if MONO
        public RSACng() : this(2048) { }

        public RSACng(int keySize)
        {
            throw new NotImplementedException ();
        }

#if NETSTANDARD
        public RSACng(CngKey key)
        {
            throw new NotImplementedException ();
        }
#endif

        public CngKey Key
        {
            [SecuritySafeCritical]
            get
            {
                throw new NotImplementedException ();
            }

            private set
            {
                throw new NotImplementedException ();
            }
        }

        public override RSAParameters ExportParameters(bool includePrivateParameters)
        {
            throw new NotImplementedException();
        }

        public override void ImportParameters(RSAParameters parameters)
        {
            throw new NotImplementedException();
        }
#else

        // See https://msdn.microsoft.com/en-us/library/windows/desktop/bb931354(v=vs.85).aspx
        private static KeySizes[] s_legalKeySizes = new KeySizes[] { new KeySizes(512, 16384, 64) };

        // CngKeyBlob formats for RSA key blobs
        private static CngKeyBlobFormat s_rsaFullPrivateBlob = new CngKeyBlobFormat(BCryptNative.KeyBlobType.RsaFullPrivateBlob);
        private static CngKeyBlobFormat s_rsaPrivateBlob = new CngKeyBlobFormat(BCryptNative.KeyBlobType.RsaPrivateBlob);
        private static CngKeyBlobFormat s_rsaPublicBlob = new CngKeyBlobFormat(BCryptNative.KeyBlobType.RsaPublicBlob);

        // Key handle
        private CngKey _key;

        /// <summary>
        ///     Create an RSACng algorithm with a random 2048 bit key pair.
        /// </summary>
        public RSACng() : this(2048) { }

        /// <summary>
        ///     Creates a new RSACng object that will use a randomly generated key of the specified size.
        ///     Valid key sizes range from 384 to 16384 bits, in increments of 8. It's suggested that a
        ///     minimum size of 2048 bits be used for all keys.
        /// </summary>
        /// <param name="keySize">Size of the key to generate, in bits.</param>
        /// <exception cref="CryptographicException">if <paramref name="keySize" /> is not valid</exception>
        public RSACng(int keySize)
        {
            LegalKeySizesValue = s_legalKeySizes;
            KeySize = keySize;
        }

        /// <summary>
        ///     Creates a new RSACng object that will use the specified key. The key's
        ///     <see cref="CngKey.AlgorithmGroup" /> must be Rsa.
        ///     CngKey.Open creates a copy of the key. Even if someone disposes the key passed
        ///     copy of this key object in RSA stays alive. 
        /// </summary>
        /// <param name="key">Key to use for RSA operations</param>
        /// <exception cref="ArgumentException">if <paramref name="key" /> is not an RSA key</exception>
        /// <exception cref="ArgumentNullException">if <paramref name="key" /> is null.</exception>
        [SecuritySafeCritical]
        public RSACng(CngKey key)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }
            if (key.AlgorithmGroup != CngAlgorithmGroup.Rsa)
            {
                throw new ArgumentException(SR.GetString(SR.Cryptography_ArgRSAaRequiresRSAKey), "key");
            }
            LegalKeySizesValue = s_legalKeySizes;
            Key = CngKey.Open(key.Handle, key.IsEphemeral ? CngKeyHandleOpenOptions.EphemeralKey : CngKeyHandleOpenOptions.None);
        }

        /// <summary>
        ///     Gets the key that will be used by the RSA object for any cryptographic operation that it uses.
        ///     This key object will be disposed if the key is reset, for instance by changing the KeySize
        ///     property, using ImportParamers to create a new key, or by Disposing of the parent RSA object.
        ///     Therefore, you should make sure that the key object is no longer used in these scenarios. This
        ///     object will not be the same object as the CngKey passed to the RSACng constructor if that
        ///     constructor was used, however it will point at the same CNG key.
        /// </summary>
        /// <permission cref="SecurityPermission">
        ///     SecurityPermission/UnmanagedCode is required to read this property.
        /// </permission>
        public CngKey Key
        {
            [SecuritySafeCritical]
            get
            {
                // If our key size was changed from the key we're using, we need to generate a new key
                if (_key != null && _key.KeySize != KeySize)
                {
                    _key.Dispose();
                    _key = null;
                }

                // If we don't have a key yet, we need to generate a random one now
                if (_key == null)
                {
                    CngKeyCreationParameters creationParameters = new CngKeyCreationParameters()
                    {
                        ExportPolicy = CngExportPolicies.AllowPlaintextExport,
                    };

                    CngProperty keySizeProperty = new CngProperty(NCryptNative.KeyPropertyName.Length,
                                                                  BitConverter.GetBytes(KeySize),
                                                                  CngPropertyOptions.None);
                    creationParameters.Parameters.Add(keySizeProperty);
                    _key = CngKey.Create(CngAlgorithm.Rsa, null, creationParameters);
                }

                return _key;
            }

            private set
            {
                Debug.Assert(value != null, "value != null");
                if (value.AlgorithmGroup != CngAlgorithmGroup.Rsa)
                {
                    throw new ArgumentException(SR.GetString(SR.Cryptography_ArgRSAaRequiresRSAKey), "value");
                }
                // If we already have a key, clear it out
                if (_key != null)
                {
                    _key.Dispose();
                }

                _key = value;

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
                KeySizeValue = _key.KeySize;
            }
        }

        /// <summary>
        ///     Helper property to get the NCrypt key handle
        /// </summary>
        private SafeNCryptKeyHandle KeyHandle
        {
            [SecuritySafeCritical]
            get { return Key.Handle; }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && _key != null)
            {
                _key.Dispose();
            }
        }

        protected override byte[] HashData(byte[] data, int offset, int count, HashAlgorithmName hashAlgorithm)
        {
            // we're sealed and the base should have checked this already
            Debug.Assert(data != null);
            Debug.Assert(offset >= 0 && offset <= data.Length);
            Debug.Assert(count >= 0 && count <= data.Length);
            Debug.Assert(!String.IsNullOrEmpty(hashAlgorithm.Name));

            using (BCryptHashAlgorithm hasher = new BCryptHashAlgorithm(new CngAlgorithm(hashAlgorithm.Name), BCryptNative.ProviderName.MicrosoftPrimitiveProvider))
            {
                hasher.HashCore(data, offset, count);
                return hasher.HashFinal();
            }
        }

        protected override byte[] HashData(Stream data, HashAlgorithmName hashAlgorithm)
        {
            // We're sealed and the base should have checked these alread.
            Debug.Assert(data != null);
            Debug.Assert(!String.IsNullOrEmpty(hashAlgorithm.Name));

            using (BCryptHashAlgorithm hasher = new BCryptHashAlgorithm(new CngAlgorithm(hashAlgorithm.Name), BCryptNative.ProviderName.MicrosoftPrimitiveProvider))
            {
                hasher.HashStream(data);
                return hasher.HashFinal();
            }
        }

       
        /// <summary>
        /// This function checks the magic value in the key blob header
        /// </summary>
        /// <param name="includePrivateParameters">Private blob if true else public key blob</param>
        private void CheckMagicValueOfKey(int magic, bool includePrivateParameters)
        {
            if (false == includePrivateParameters)
            {
                if (magic != (int)BCryptNative.KeyBlobMagicNumber.RsaPublic)
                {
                    //Check for Private key magic as public key can be derived from private key blob
                    if (magic != (int)BCryptNative.KeyBlobMagicNumber.RsaPrivate && magic != (int)BCryptNative.KeyBlobMagicNumber.RsaFullPrivateMagic)
                    {
                        throw new CryptographicException(SR.GetString(SR.Cryptography_NotValidPublicOrPrivateKey));
                    }
                }
            }
            //If includePrivateParameters is true then certainly check for the private key magic
            else
            {
                if (magic != (int)BCryptNative.KeyBlobMagicNumber.RsaPrivate && magic != (int)BCryptNative.KeyBlobMagicNumber.RsaFullPrivateMagic)
                {
                    throw new CryptographicException(SR.GetString(SR.Cryptography_NotValidPrivateKey));
                }
            }
        }

        //
        // Key import and export
        //

        /// <summary>
        ///     Exports the key used by the RSA object into an RSAParameters object.
        /// </summary>        
        [SecuritySafeCritical]
        public override RSAParameters ExportParameters(bool includePrivateParameters)
        {
            byte[] rsaBlob = Key.Export(includePrivateParameters ? s_rsaFullPrivateBlob : s_rsaPublicBlob);
            RSAParameters rsaParams = new RSAParameters();

            //
            // We now have a buffer laid out as follows:
            //     BCRYPT_RSAKEY_BLOB   header
            //     byte[cbPublicExp]    publicExponent      - Exponent
            //     byte[cbModulus]      modulus             - Modulus
            //     -- Private only --
            //     byte[cbPrime1]       prime1              - P
            //     byte[cbPrime2]       prime2              - Q
            //     byte[cbPrime1]       exponent1           - DP
            //     byte[cbPrime2]       exponent2           - DQ
            //     byte[cbPrime1]       coefficient         - InverseQ
            //     byte[cbModulus]      privateExponent     - D
            //
            byte[] tempMagic = new byte[4];
            tempMagic[0] = rsaBlob[0]; tempMagic[1] = rsaBlob[1]; tempMagic[2] = rsaBlob[2]; tempMagic[3] = rsaBlob[3];
            int magic = BitConverter.ToInt32(tempMagic, 0);
            //Check the magic value in key blob header. If blob does not have required magic 
            // then it trhows Cryptographic exception
            CheckMagicValueOfKey(magic, includePrivateParameters);

            unsafe
            {
                fixed (byte* pRsaBlob = rsaBlob)
                {
                    BCryptNative.BCRYPT_RSAKEY_BLOB* pBcryptBlob = (BCryptNative.BCRYPT_RSAKEY_BLOB*)pRsaBlob;

                    int offset = Marshal.SizeOf(typeof(BCryptNative.BCRYPT_RSAKEY_BLOB));

                    // Read out the exponent
                    rsaParams.Exponent = new byte[pBcryptBlob->cbPublicExp];
                    Buffer.BlockCopy(rsaBlob, offset, rsaParams.Exponent, 0, rsaParams.Exponent.Length);
                    offset += pBcryptBlob->cbPublicExp;

                    // Read out the modulus
                    rsaParams.Modulus = new byte[pBcryptBlob->cbModulus];
                    Buffer.BlockCopy(rsaBlob, offset, rsaParams.Modulus, 0, rsaParams.Modulus.Length);
                    offset += pBcryptBlob->cbModulus;

                    if (includePrivateParameters)
                    {
                        // Read out P
                        rsaParams.P = new byte[pBcryptBlob->cbPrime1];
                        Buffer.BlockCopy(rsaBlob, offset, rsaParams.P, 0, rsaParams.P.Length);
                        offset += pBcryptBlob->cbPrime1;

                        // Read out Q
                        rsaParams.Q = new byte[pBcryptBlob->cbPrime2];
                        Buffer.BlockCopy(rsaBlob, offset, rsaParams.Q, 0, rsaParams.Q.Length);
                        offset += pBcryptBlob->cbPrime2;

                        // Read out DP
                        rsaParams.DP = new byte[pBcryptBlob->cbPrime1];
                        Buffer.BlockCopy(rsaBlob, offset, rsaParams.DP, 0, rsaParams.DP.Length);
                        offset += pBcryptBlob->cbPrime1;

                        // Read out DQ
                        rsaParams.DQ = new byte[pBcryptBlob->cbPrime2];
                        Buffer.BlockCopy(rsaBlob, offset, rsaParams.DQ, 0, rsaParams.DQ.Length);
                        offset += pBcryptBlob->cbPrime2;

                        // Read out InverseQ
                        rsaParams.InverseQ = new byte[pBcryptBlob->cbPrime1];
                        Buffer.BlockCopy(rsaBlob, offset, rsaParams.InverseQ, 0, rsaParams.InverseQ.Length);
                        offset += pBcryptBlob->cbPrime1;

                        //  Read out D
                        rsaParams.D = new byte[pBcryptBlob->cbModulus];
                        Buffer.BlockCopy(rsaBlob, offset, rsaParams.D, 0, rsaParams.D.Length);
                        offset += pBcryptBlob->cbModulus;
                    }
                }
            }

            return rsaParams;
        }

        /// <summary>
        ///     <para>
        ///         ImportParameters will replace the existing key that RSACng is working with by creating a
        ///         new CngKey for the parameters structure. If the parameters structure contains only an
        ///         exponent and modulus, then only a public key will be imported. If the parameters also
        ///         contain P and Q values, then a full key pair will be imported.
        ///     </para>        
        /// </summary>
        /// <exception cref="ArgumentException">
        ///     if <paramref name="parameters" /> contains neither an exponent nor a modulus.
        /// </exception>
        /// <exception cref="CryptographicException">
        ///     if <paramref name="parameters" /> is not a valid RSA key or if <paramref name="parameters"
        ///     /> is a full key pair and the default KSP is used.
        /// </exception>        
        [SecuritySafeCritical]
        public override void ImportParameters(RSAParameters parameters)
        {
            if (parameters.Exponent == null || parameters.Modulus == null)
            {
                throw new ArgumentException(SR.GetString(SR.Cryptography_InvalidRsaParameters));
            }
            bool publicOnly = parameters.P == null || parameters.Q == null;

            //
            // We need to build a key blob structured as follows:
            //     BCRYPT_RSAKEY_BLOB   header
            //     byte[cbPublicExp]    publicExponent      - Exponent
            //     byte[cbModulus]      modulus             - Modulus
            //     -- Private only --
            //     byte[cbPrime1]       prime1              - P
            //     byte[cbPrime2]       prime2              - Q
            //

            int blobSize = Marshal.SizeOf(typeof(BCryptNative.BCRYPT_RSAKEY_BLOB)) +
                           parameters.Exponent.Length +
                           parameters.Modulus.Length;
            if (!publicOnly)
            {
                blobSize += parameters.P.Length +
                            parameters.Q.Length;
            }

            byte[] rsaBlob = new byte[blobSize];
            unsafe
            {
                fixed (byte* pRsaBlob = rsaBlob)
                {
                    // Build the header
                    BCryptNative.BCRYPT_RSAKEY_BLOB* pBcryptBlob = (BCryptNative.BCRYPT_RSAKEY_BLOB*)pRsaBlob;
                    pBcryptBlob->Magic = publicOnly ? BCryptNative.KeyBlobMagicNumber.RsaPublic :
                                                      BCryptNative.KeyBlobMagicNumber.RsaPrivate;

                    pBcryptBlob->BitLength = parameters.Modulus.Length * 8;

                    pBcryptBlob->cbPublicExp = parameters.Exponent.Length;
                    pBcryptBlob->cbModulus = parameters.Modulus.Length;

                    if (!publicOnly)
                    {
                        pBcryptBlob->cbPrime1 = parameters.P.Length;
                        pBcryptBlob->cbPrime2 = parameters.Q.Length;
                    }

                    int offset = Marshal.SizeOf(typeof(BCryptNative.BCRYPT_RSAKEY_BLOB));

                    // Copy the exponent
                    Buffer.BlockCopy(parameters.Exponent, 0, rsaBlob, offset, parameters.Exponent.Length);
                    offset += parameters.Exponent.Length;

                    // Copy the modulus
                    Buffer.BlockCopy(parameters.Modulus, 0, rsaBlob, offset, parameters.Modulus.Length);
                    offset += parameters.Modulus.Length;

                    if (!publicOnly)
                    {
                        // Copy P
                        Buffer.BlockCopy(parameters.P, 0, rsaBlob, offset, parameters.P.Length);
                        offset += parameters.P.Length;

                        // Copy Q
                        Buffer.BlockCopy(parameters.Q, 0, rsaBlob, offset, parameters.Q.Length);
                        offset += parameters.Q.Length;
                    }
                }
            }
            Key = CngKey.Import(rsaBlob, publicOnly ? s_rsaPublicBlob : s_rsaPrivateBlob);
        }

        //
        // Encryption and decryption
        //
        [SecuritySafeCritical]
        public override byte[] Decrypt(byte[] data, RSAEncryptionPadding padding)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }

            if (padding == null)
            {
                throw new ArgumentNullException("padding");
            }

            SafeNCryptKeyHandle keyHandle = Key.Handle;

            if (padding == RSAEncryptionPadding.Pkcs1)
            {
                return NCryptNative.DecryptDataPkcs1(keyHandle, data);
            }
            else if (padding.Mode == RSAEncryptionPaddingMode.Oaep)
            {
                return NCryptNative.DecryptDataOaep(keyHandle, data, padding.OaepHashAlgorithm.Name);
            }
            else
            {
                // no other padding possibilities at present, but we might version independently from more being added.
                throw new CryptographicException(SR.GetString(SR.Cryptography_UnsupportedPaddingMode));
            }
        }

        [SecuritySafeCritical]
        public override byte[] Encrypt(byte[] data, RSAEncryptionPadding padding)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }
            if (padding == null)
            {
                throw new ArgumentNullException("padding");
            }

            if (padding == RSAEncryptionPadding.Pkcs1)
            {
                return NCryptNative.EncryptDataPkcs1(KeyHandle, data);
            }
            else if (padding.Mode == RSAEncryptionPaddingMode.Oaep)
            {
                 return NCryptNative.EncryptDataOaep(KeyHandle, data, padding.OaepHashAlgorithm.Name);
            }
            else
            {
                 // no other padding possibilities at present, but we might version independently from more being added.
                 throw new CryptographicException(SR.GetString(SR.Cryptography_UnsupportedPaddingMode));
            };
        }


        //
        // Signature APIs
        //

        [SecuritySafeCritical]
        public override byte[] SignHash(byte[] hash, HashAlgorithmName hashAlgorithm, RSASignaturePadding padding)
        {
            if (hash == null)
            {
               throw new ArgumentNullException("hash");
            }
            if (String.IsNullOrEmpty(hashAlgorithm.Name))
            {
                throw new ArgumentException(SR.GetString(SR.Cryptography_HashAlgorithmNameNullOrEmpty), "hashAlgorithm");
            } 
            if (padding == null)
            {
                throw new ArgumentNullException("padding");
            }

            // Keep a local copy of the key.
            CngKey key = Key;
            SafeNCryptKeyHandle keyHandle = key.Handle;

            if (padding == RSASignaturePadding.Pkcs1)
            {
                return NCryptNative.SignHashPkcs1(keyHandle, hash, hashAlgorithm.Name);
            }
            else if (padding == RSASignaturePadding.Pss)
            {
                return NCryptNative.SignHashPss(keyHandle, hash, hashAlgorithm.Name, hash.Length);
            }
            else
            {
                 // no other padding possibilities at present, but we might version independently from more being added.
                 throw new CryptographicException(SR.GetString(SR.Cryptography_UnsupportedPaddingMode));

            }
        }

        [SecuritySafeCritical]
        public override bool VerifyHash(byte[] hash, byte[] signature, HashAlgorithmName hashAlgorithm, RSASignaturePadding padding)
        {
            if (hash == null)
            {
                throw new ArgumentNullException("hash");
            }
            if (signature == null)
            {
                throw new ArgumentNullException("signature");
            }
            if (String.IsNullOrEmpty(hashAlgorithm.Name))
            {
                throw new ArgumentException(SR.GetString(SR.Cryptography_HashAlgorithmNameNullOrEmpty), "hashAlgorithm");
            }
            if (padding == null)
            {
                throw new ArgumentNullException("padding");
            }

            if (padding == RSASignaturePadding.Pkcs1)
            {
                return NCryptNative.VerifySignaturePkcs1(KeyHandle, hash, hashAlgorithm.Name, signature);
            }
            else if (padding == RSASignaturePadding.Pss)
            {
                return NCryptNative.VerifySignaturePss(KeyHandle, hash, hashAlgorithm.Name, hash.Length, signature);
            }
            else
            {
                 // no other padding possibilities at present, but we might version independently from more being added.
                 throw new CryptographicException(SR.GetString(SR.Cryptography_UnsupportedPaddingMode));
            }
        }

        /*
         * The members
         *   DecryptValue
         *   EncryptValue
         *   get_KeyExchangeAlgorithm
         *   get_SignatureAlgorithm
         * are all implemented on RSA as of net46.
         *
         * But in servicing situations, System.Core.dll can get patched onto a machine which has mscorlib < net46, meaning
         * these abstract members have no implementation.
         *
         * To keep servicing simple, we'll redefine the overrides here. Since this type is sealed it only affects reflection,
         * as there are no derived types to mis-target base.-invocations.
         */
        public override byte[] DecryptValue(byte[] rgb) { throw new NotSupportedException(SR.NotSupported_Method); }
        public override byte[] EncryptValue(byte[] rgb) { throw new NotSupportedException(SR.NotSupported_Method); }
        public override string KeyExchangeAlgorithm { get { return "RSA"; } }
        public override string SignatureAlgorithm { get { return "RSA"; } }
#endif
    }
}
