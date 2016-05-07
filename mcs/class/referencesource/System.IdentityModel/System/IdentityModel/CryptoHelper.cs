//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.IdentityModel
{
    using System.Collections.Generic;
    using System.IdentityModel.Tokens;
    using System.Reflection;
    using System.Security.Cryptography;
    using System.Security.Cryptography.X509Certificates;
    using System.Security.Cryptography.Xml;
    using System.ServiceModel.Security;

    static class CryptoHelper
    {
        static byte[] emptyBuffer;
        static RandomNumberGenerator random;
        static Rijndael rijndael;
        static TripleDES tripleDES;

        static Dictionary<string, Func<object>> algorithmDelegateDictionary = new Dictionary<string, Func<object>>();
        static object AlgorithmDictionaryLock = new object();
        public const int WindowsVistaMajorNumber = 6;
        const string SHAString = "SHA";
        const string SHA1String = "SHA1";
        const string SHA256String = "SHA256";
        const string SystemSecurityCryptographySha1String = "System.Security.Cryptography.SHA1";


        /// <summary>
        /// The helper class which helps user to compute the combined entropy as well as the session
        /// key
        /// </summary>
        public static class KeyGenerator
        {
            static RandomNumberGenerator _random = CryptoHelper.RandomNumberGenerator;

            //
            // 1/(2^32) keys will be weak.  20 random keys will never happen by chance without the RNG being messed up.
            //
            const int _maxKeyIterations = 20;
            /// <summary>
            /// Computes the session key based on PSHA1 algorithm.
            /// </summary>
            /// <param name="requestorEntropy">The entropy from the requestor side.</param>
            /// <param name="issuerEntropy">The entropy from the token issuer side.</param>
            /// <param name="keySizeInBits">The desired key size in bits.</param>
            /// <returns>The computed session key.</returns>
            public static byte[] ComputeCombinedKey( byte[] requestorEntropy, byte[] issuerEntropy, int keySizeInBits )
            {
                if ( null == requestorEntropy )
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull( "requestorEntropy" );
                }

                if ( null == issuerEntropy )
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull( "issuerEntropy" );
                }

                int keySizeInBytes = ValidateKeySizeInBytes( keySizeInBits );

                byte[] key = new byte[keySizeInBytes]; // Final key

                // The symmetric key generation chosen is 
                // http://schemas.xmlsoap.org/ws/2005/02/trust/CK/PSHA1
                // which per the WS-Trust specification is defined as follows:
                //
                //   The key is computed using P_SHA1
                //   from the TLS specification to generate
                //   a bit stream using entropy from both
                //   sides. The exact form is:
                //
                //   key = P_SHA1 (EntREQ, EntRES)
                //
                // where P_SHA1 is defined per http://www.ietf.org/rfc/rfc2246.txt 
                // and EntREQ is the entropy supplied by the requestor and EntRES 
                // is the entrophy supplied by the issuer.
                //
                // From http://www.faqs.org/rfcs/rfc2246.html:
                // 
                // 8<------------------------------------------------------------>8
                // First, we define a data expansion function, P_hash(secret, data)
                // which uses a single hash function to expand a secret and seed 
                // into an arbitrary quantity of output:
                // 
                // P_hash(secret, seed) = HMAC_hash(secret, A(1) + seed) +
                //                        HMAC_hash(secret, A(2) + seed) +
                //                        HMAC_hash(secret, A(3) + seed) + ...
                //
                // Where + indicates concatenation.
                //
                // A() is defined as:
                //   A(0) = seed
                //   A(i) = HMAC_hash(secret, A(i-1))
                //
                // P_hash can be iterated as many times as is necessary to produce
                // the required quantity of data. For example, if P_SHA-1 was 
                // being used to create 64 bytes of data, it would have to be 
                // iterated 4 times (through A(4)), creating 80 bytes of output 
                // data; the last 16 bytes of the final iteration would then be 
                // discarded, leaving 64 bytes of output data.
                // 8<------------------------------------------------------------>8

                // Note that requestorEntrophy is considered the 'secret'.
                using ( KeyedHashAlgorithm kha = CryptoHelper.NewHmacSha1KeyedHashAlgorithm() )
                {
                    kha.Key = requestorEntropy;

                    byte[] a = issuerEntropy; // A(0), the 'seed'.
                    byte[] b = new byte[kha.HashSize / 8 + a.Length]; // Buffer for A(i) + seed
                    byte[] result = null;

                    try
                    {

                        for ( int i = 0; i < keySizeInBytes; )
                        {
                            // Calculate A(i+1).                
                            kha.Initialize();
                            a = kha.ComputeHash( a );

                            // Calculate A(i) + seed
                            a.CopyTo( b, 0 );
                            issuerEntropy.CopyTo( b, a.Length );
                            kha.Initialize();
                            result = kha.ComputeHash( b );

                            for ( int j = 0; j < result.Length; j++ )
                            {
                                if ( i < keySizeInBytes )
                                {
                                    key[i++] = result[j];
                                }
                                else
                                {
                                    break;
                                }
                            }
                        }
                    }
                    catch
                    {
                        Array.Clear( key, 0, key.Length );
                        throw;
                    }
                    finally
                    {
                        if ( result != null )
                        {
                            Array.Clear( result, 0, result.Length );
                        }

                        Array.Clear( b, 0, b.Length );

                        kha.Clear();
                    }
                }

                return key;
            }

            /// <summary>
            /// Generates a symmetric key with a given size.
            /// </summary>
            /// <remarks>This function should not be used to generate DES keys because it does not perform an IsWeakKey check.
            /// Use GenerateDESKey() instead.</remarks>
            /// <param name="keySizeInBits">The key size in bits.</param>
            /// <returns>The symmetric key.</returns>
            /// <exception cref="ArgumentException">When keySizeInBits is not a whole number of bytes.</exception>
            public static byte[] GenerateSymmetricKey( int keySizeInBits )
            {
                int keySizeInBytes = ValidateKeySizeInBytes( keySizeInBits );

                byte[] key = new byte[keySizeInBytes];

                CryptoHelper.GenerateRandomBytes( key );

                return key;
            }

            /// <summary>
            /// Generates a combined-entropy key.
            /// </summary>
            /// <remarks>This function should not be used to generate DES keys because it does not perform an IsWeakKey check.
            /// Use GenerateDESKey() instead.</remarks>
            /// <param name="keySizeInBits">The key size in bits.</param>
            /// <param name="senderEntropy">Requestor's entropy.</param>
            /// <param name="receiverEntropy">The issuer's entropy.</param>
            /// <returns>The computed symmetric key based on PSHA1 algorithm.</returns>
            /// <exception cref="ArgumentException">When keySizeInBits is not a whole number of bytes.</exception>
            public static byte[] GenerateSymmetricKey( int keySizeInBits, byte[] senderEntropy, out byte[] receiverEntropy )
            {
                if ( senderEntropy == null )
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull( "senderEntropy" );
                }

                int keySizeInBytes = ValidateKeySizeInBytes( keySizeInBits );

                //
                // Generate proof key using sender entropy and receiver entropy
                //
                receiverEntropy = new byte[keySizeInBytes];

                _random.GetNonZeroBytes( receiverEntropy );

                return ComputeCombinedKey( senderEntropy, receiverEntropy, keySizeInBits );
            }

            /// <summary>
            /// Generates a symmetric key for use with the DES or Triple-DES algorithms.  This function will always return a key that is
            /// not considered weak by TripleDES.IsWeakKey().
            /// </summary>
            /// <param name="keySizeInBits">The key size in bits.</param>
            /// <returns>The symmetric key.</returns>
            /// <exception cref="ArgumentException">When keySizeInBits is not a proper DES key size.</exception>
            public static byte[] GenerateDESKey( int keySizeInBits )
            {
                int keySizeInBytes = ValidateKeySizeInBytes( keySizeInBits );

                byte[] key = new byte[keySizeInBytes];
                int tries = 0;

                do
                {
                    if ( tries > _maxKeyIterations )
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError( new CryptographicException( SR.GetString( SR.ID6048, _maxKeyIterations ) ) );
                    }

                    CryptoHelper.GenerateRandomBytes( key );
                    ++tries;

                } while ( TripleDES.IsWeakKey( key ) );

                return key;
            }

            /// <summary>
            /// Generates a combined-entropy key for use with the DES or Triple-DES algorithms.  This function will always return a key that is
            /// not considered weak by TripleDES.IsWeakKey().
            /// </summary>
            /// <param name="keySizeInBits">The key size in bits.</param>
            /// <param name="senderEntropy">Requestor's entropy.</param>
            /// <param name="receiverEntropy">The issuer's entropy.</param>
            /// <returns>The computed symmetric key based on PSHA1 algorithm.</returns>
            /// <exception cref="ArgumentException">When keySizeInBits is not a proper DES key size.</exception>
            public static byte[] GenerateDESKey( int keySizeInBits, byte[] senderEntropy, out byte[] receiverEntropy )
            {
                int keySizeInBytes = ValidateKeySizeInBytes( keySizeInBits );

                byte[] key = new byte[keySizeInBytes];
                int tries = 0;

                do
                {
                    if ( tries > _maxKeyIterations )
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError( new CryptographicException( SR.GetString( SR.ID6048, _maxKeyIterations ) ) );
                    }

                    receiverEntropy = new byte[keySizeInBytes];
                    _random.GetNonZeroBytes( receiverEntropy );
                    key = ComputeCombinedKey( senderEntropy, receiverEntropy, keySizeInBits );
                    ++tries;

                } while ( TripleDES.IsWeakKey( key ) );

                return key;
            }

            static int ValidateKeySizeInBytes( int keySizeInBits )
            {
                int keySizeInBytes = keySizeInBits / 8;

                if ( keySizeInBits <= 0 )
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError( new ArgumentOutOfRangeException( "keySizeInBits", SR.GetString( SR.ID6033, keySizeInBits ) ) );
                }
                else if ( keySizeInBytes * 8 != keySizeInBits )
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError( new ArgumentException( SR.GetString( SR.ID6002, keySizeInBits ), "keySizeInBits" ) );
                }

                return keySizeInBytes;
            }

            /// <summary>
            /// Gets a security key identifier which contains the BinarySecretKeyIdentifierClause or 
            /// EncryptedKeyIdentifierClause if the wrapping credentials is available.
            /// </summary>
            public static SecurityKeyIdentifier GetSecurityKeyIdentifier(byte[] secret, EncryptingCredentials wrappingCredentials)
            {
                if (secret == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("secret");
                }

                if (secret.Length == 0)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("secret", SR.GetString(SR.ID6031));
                }

                if (wrappingCredentials == null || wrappingCredentials.SecurityKey == null)
                {
                    //
                    // BinarySecret case
                    //
                    return new SecurityKeyIdentifier(new BinarySecretKeyIdentifierClause(secret));
                }
                else
                {
                    //
                    // EncryptedKey case
                    //
                    byte[] wrappedKey = wrappingCredentials.SecurityKey.EncryptKey(wrappingCredentials.Algorithm, secret);

                    return new SecurityKeyIdentifier(new EncryptedKeyIdentifierClause(wrappedKey, wrappingCredentials.Algorithm, wrappingCredentials.SecurityKeyIdentifier));
                }
            }

        }

        /// <summary>
        /// Provides an integer-domain mathematical operation for 
        /// Ceiling( dividend / divisor ). 
        /// </summary>
        /// <param name="dividend"></param>
        /// <param name="divisor"></param>
        /// <returns></returns>
        public static int CeilingDivide( int dividend, int divisor )
        {
            int remainder, quotient;

            remainder = dividend % divisor;
            quotient = dividend / divisor;

            if ( remainder > 0 )
            {
                quotient++;
            }

            return quotient;
        }

        internal static byte[] EmptyBuffer
        {
            get
            {
                if (emptyBuffer == null)
                {
                    byte[] tmp = new byte[0];
                    emptyBuffer = tmp;
                }
                return emptyBuffer;
            }
        }

        internal static Rijndael Rijndael
        {
            get
            {
                if (rijndael == null)
                {
                    Rijndael tmp = SecurityUtils.RequiresFipsCompliance ? (Rijndael)new RijndaelCryptoServiceProvider() : new RijndaelManaged();
                    tmp.Padding = PaddingMode.ISO10126;
                    rijndael = tmp;
                }
                return rijndael;
            }
        }

        internal static TripleDES TripleDES
        {
            get
            {
                if (tripleDES == null)
                {
                    TripleDESCryptoServiceProvider tmp = new TripleDESCryptoServiceProvider();
                    tmp.Padding = PaddingMode.ISO10126;
                    tripleDES = tmp;
                }
                return tripleDES;
            }
        }

        internal static RandomNumberGenerator RandomNumberGenerator
        {
            get
            {
                if (random == null)
                {
                    random = new RNGCryptoServiceProvider();
                }
                return random;
            }
        }

        /// <summary>
        /// Creates the default encryption algorithm.
        /// </summary>
        /// <returns>A SymmetricAlgorithm instance that must be disposed by the caller after use.</returns>
        internal static SymmetricAlgorithm NewDefaultEncryption()
        {
            return GetSymmetricAlgorithm(null, SecurityAlgorithms.DefaultEncryptionAlgorithm );
        }

        internal static HashAlgorithm NewSha1HashAlgorithm()
        {
            return CryptoHelper.CreateHashAlgorithm(SecurityAlgorithms.Sha1Digest);
        }

        internal static HashAlgorithm NewSha256HashAlgorithm()
        {
            return CryptoHelper.CreateHashAlgorithm(SecurityAlgorithms.Sha256Digest);
        }

        internal static KeyedHashAlgorithm NewHmacSha1KeyedHashAlgorithm()
        {
            KeyedHashAlgorithm algorithm = GetAlgorithmFromConfig( SecurityAlgorithms.HmacSha1Signature ) as KeyedHashAlgorithm;
            if ( algorithm == null )
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument( "algorithm", SR.GetString( SR.ID6037, SecurityAlgorithms.HmacSha1Signature ) );
            }
            return algorithm;
        }

        internal static KeyedHashAlgorithm NewHmacSha1KeyedHashAlgorithm(byte[] key)
        {
            return CryptoHelper.CreateKeyedHashAlgorithm(key, SecurityAlgorithms.HmacSha1Signature);
        }

        internal static KeyedHashAlgorithm NewHmacSha256KeyedHashAlgorithm(byte[] key)
        {
            return CryptoHelper.CreateKeyedHashAlgorithm(key, SecurityAlgorithms.HmacSha256Signature);
        }

        internal static Rijndael NewRijndaelSymmetricAlgorithm()
        {
            Rijndael rijndael = (GetSymmetricAlgorithm(null, SecurityAlgorithms.Aes128Encryption) as Rijndael);
            if (rijndael != null)
                return rijndael;
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new InvalidOperationException(SR.GetString(SR.CustomCryptoAlgorithmIsNotValidSymmetricAlgorithm, SecurityAlgorithms.Aes128Encryption)));
        }

        internal static ICryptoTransform CreateDecryptor(byte[] key, byte[] iv, string algorithm)
        {
            object algorithmObject = GetAlgorithmFromConfig(algorithm);

            if (algorithmObject != null)
            {
                SymmetricAlgorithm symmetricAlgorithm = algorithmObject as SymmetricAlgorithm;

                if (symmetricAlgorithm != null)
                {
                    return symmetricAlgorithm.CreateDecryptor(key, iv);
                }
                //NOTE: KeyedHashAlgorithms are symmetric in nature but we still throw if it is passed as an argument. 

                throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new InvalidOperationException(SR.GetString(SR.CustomCryptoAlgorithmIsNotValidSymmetricAlgorithm, algorithm)));
            }

            switch (algorithm)
            {
                case SecurityAlgorithms.TripleDesEncryption:
                    return TripleDES.CreateDecryptor(key, iv);
                case SecurityAlgorithms.Aes128Encryption:
                case SecurityAlgorithms.Aes192Encryption:
                case SecurityAlgorithms.Aes256Encryption:
                    return Rijndael.CreateDecryptor(key, iv);
                default:
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new InvalidOperationException(SR.GetString(SR.UnsupportedEncryptionAlgorithm, algorithm)));
            }
        }

        internal static ICryptoTransform CreateEncryptor(byte[] key, byte[] iv, string algorithm)
        {

            object algorithmObject = GetAlgorithmFromConfig(algorithm);

            if (algorithmObject != null)
            {
                SymmetricAlgorithm symmetricAlgorithm = algorithmObject as SymmetricAlgorithm;
                if (symmetricAlgorithm != null)
                {
                    return symmetricAlgorithm.CreateEncryptor(key, iv);
                }
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new InvalidOperationException(SR.GetString(SR.CustomCryptoAlgorithmIsNotValidSymmetricAlgorithm, algorithm)));
            }

            switch (algorithm)
            {
                case SecurityAlgorithms.TripleDesEncryption:
                    return TripleDES.CreateEncryptor(key, iv);
                case SecurityAlgorithms.Aes128Encryption:
                case SecurityAlgorithms.Aes192Encryption:
                case SecurityAlgorithms.Aes256Encryption:
                    return Rijndael.CreateEncryptor(key, iv);
                default:
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new InvalidOperationException(SR.GetString(SR.UnsupportedEncryptionAlgorithm, algorithm)));
            }
        }

        internal static HashAlgorithm CreateHashAlgorithm(string algorithm)
        {
            object algorithmObject = GetAlgorithmFromConfig(algorithm);

            if (algorithmObject != null)
            {
                HashAlgorithm hashAlgorithm = algorithmObject as HashAlgorithm;
                if (hashAlgorithm != null)
                {
                    return hashAlgorithm;
                }
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new InvalidOperationException(SR.GetString(SR.CustomCryptoAlgorithmIsNotValidHashAlgorithm, algorithm)));
            }

            switch (algorithm)
            {
                case SHAString:
                case SHA1String:
                case SystemSecurityCryptographySha1String:
                case SecurityAlgorithms.Sha1Digest:
                    if (SecurityUtils.RequiresFipsCompliance)
                        return new SHA1CryptoServiceProvider();
                    else
                        return new SHA1Managed();
                case SHA256String:
                case SecurityAlgorithms.Sha256Digest:
                    if (SecurityUtils.RequiresFipsCompliance)
                        return new SHA256CryptoServiceProvider();
                    else
                        return new SHA256Managed();
                default:
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new InvalidOperationException(SR.GetString(SR.UnsupportedCryptoAlgorithm, algorithm)));
            }
        }

        internal static KeyedHashAlgorithm CreateKeyedHashAlgorithm(byte[] key, string algorithm)
        {
            object algorithmObject = GetAlgorithmFromConfig(algorithm);

            if (algorithmObject != null)
            {
                KeyedHashAlgorithm keyedHashAlgorithm = algorithmObject as KeyedHashAlgorithm;
                if (keyedHashAlgorithm != null)
                {
                    keyedHashAlgorithm.Key = key;
                    return keyedHashAlgorithm;
                }

                throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new InvalidOperationException(SR.GetString(SR.CustomCryptoAlgorithmIsNotValidKeyedHashAlgorithm, algorithm)));
            }

            switch (algorithm)
            {
                case SecurityAlgorithms.HmacSha1Signature:
                    return new HMACSHA1(key, !SecurityUtils.RequiresFipsCompliance);
                case SecurityAlgorithms.HmacSha256Signature:
                    if (!SecurityUtils.RequiresFipsCompliance)
                        return new HMACSHA256(key);
                    else
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new InvalidOperationException(SR.GetString(SR.CryptoAlgorithmIsNotFipsCompliant, algorithm)));
                default:
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new InvalidOperationException(SR.GetString(SR.UnsupportedCryptoAlgorithm, algorithm)));
            }
        }

        internal static byte[] ComputeHash(byte[] buffer)
        {
            using (HashAlgorithm hasher = CryptoHelper.NewSha1HashAlgorithm())
            {
                return hasher.ComputeHash(buffer);
            }
        }

        internal static byte[] GenerateDerivedKey(byte[] key, string algorithm, byte[] label, byte[] nonce, int derivedKeySize, int position)
        {
            if ((algorithm != SecurityAlgorithms.Psha1KeyDerivation) && (algorithm != SecurityAlgorithms.Psha1KeyDerivationDec2005))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new InvalidOperationException(SR.GetString(SR.UnsupportedKeyDerivationAlgorithm, algorithm)));
            }
            return new Psha1DerivedKeyGenerator(key).GenerateDerivedKey(label, nonce, derivedKeySize, position);
        }

        internal static int GetIVSize(string algorithm)
        {
            object algorithmObject = GetAlgorithmFromConfig(algorithm);

            if (algorithmObject != null)
            {
                SymmetricAlgorithm symmetricAlgorithm = algorithmObject as SymmetricAlgorithm;
                if (symmetricAlgorithm != null)
                {
                    return symmetricAlgorithm.BlockSize;
                }
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new InvalidOperationException(SR.GetString(SR.CustomCryptoAlgorithmIsNotValidSymmetricAlgorithm, algorithm)));
            }

            switch (algorithm)
            {
                case SecurityAlgorithms.TripleDesEncryption:
                    return TripleDES.BlockSize;
                case SecurityAlgorithms.Aes128Encryption:
                case SecurityAlgorithms.Aes192Encryption:
                case SecurityAlgorithms.Aes256Encryption:
                    return Rijndael.BlockSize;
                default:
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new InvalidOperationException(SR.GetString(SR.UnsupportedEncryptionAlgorithm, algorithm)));
            }
        }

        internal static void FillRandomBytes( byte[] buffer )
        {
            RandomNumberGenerator.GetBytes( buffer );
        }

        /// <summary>
        /// This generates the entropy using random number. This is usually used on the sending 
        /// side to generate the requestor's entropy.
        /// </summary>
        /// <param name="data">The array to fill with cryptographically strong random nonzero bytes.</param>
        public static void GenerateRandomBytes( byte[] data )
        {
            RandomNumberGenerator.GetNonZeroBytes( data );
        }

        /// <summary>
        /// This method generates a random byte array used as entropy with the given size. 
        /// </summary>
        /// <param name="sizeInBits"></param>
        /// <returns></returns>
        public static byte[] GenerateRandomBytes( int sizeInBits )
        {
            int sizeInBytes = sizeInBits / 8;
            if ( sizeInBits <= 0 )
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError( new ArgumentOutOfRangeException( "sizeInBits", SR.GetString( SR.ID6033, sizeInBits ) ) );
            }
            else if ( sizeInBytes * 8 != sizeInBits )
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError( new ArgumentException( SR.GetString( SR.ID6002, sizeInBits ), "sizeInBits" ) );
            }

            byte[] data = new byte[sizeInBytes];
            GenerateRandomBytes( data );

            return data;
        }
            
        internal static SymmetricAlgorithm GetSymmetricAlgorithm(byte[] key, string algorithm)
        {
            SymmetricAlgorithm symmetricAlgorithm;

            object algorithmObject = GetAlgorithmFromConfig(algorithm);

            if (algorithmObject != null)
            {
                symmetricAlgorithm = algorithmObject as SymmetricAlgorithm;
                if (symmetricAlgorithm != null)
                {
                    if (key != null)
                    {
                        symmetricAlgorithm.Key = key;
                    }
                    return symmetricAlgorithm;
                }
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new InvalidOperationException(SR.GetString(SR.CustomCryptoAlgorithmIsNotValidSymmetricAlgorithm, algorithm)));
            }

            // NOTE: HMACSHA1 and HMACSHA256 ( KeyedHashAlgorithms ) are symmetric algorithms but they do not extend Symmetric class. 
            // Hence the function throws when they are passed as arguments.

            switch (algorithm)
            {
                case SecurityAlgorithms.TripleDesEncryption:
                case SecurityAlgorithms.TripleDesKeyWrap:
                    symmetricAlgorithm = new TripleDESCryptoServiceProvider();
                    break;
                case SecurityAlgorithms.Aes128Encryption:
                case SecurityAlgorithms.Aes192Encryption:
                case SecurityAlgorithms.Aes256Encryption:
                case SecurityAlgorithms.Aes128KeyWrap:
                case SecurityAlgorithms.Aes192KeyWrap:
                case SecurityAlgorithms.Aes256KeyWrap:
                    symmetricAlgorithm = SecurityUtils.RequiresFipsCompliance ? (Rijndael)new RijndaelCryptoServiceProvider() : new RijndaelManaged();
                    break;
                default:
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new InvalidOperationException(SR.GetString(SR.UnsupportedEncryptionAlgorithm, algorithm)));

            }

            if (key != null)
            {
                symmetricAlgorithm.Key = key;
            }
            return symmetricAlgorithm;
        }

        /// <summary>
        /// Wrapper that creates a signature for SHA256 taking into consideration the special logic required for FIPS compliance
        /// </summary>
        /// <param name="formatter">the signature formatter</param>
        /// <param name="hash">the hash algorithm</param>
        /// <returns>byte array representing the signature</returns>
        internal static byte[] CreateSignatureForSha256( AsymmetricSignatureFormatter formatter, HashAlgorithm hash )
        {
            if ( SecurityUtils.RequiresFipsCompliance )
            {
                //
                // When FIPS is turned ON. We need to set the hash algorithm specifically 
                // as we need to pass the pre-computed buffer to CreateSignature, else
                // for SHA256 and FIPS turned ON, the underlying formatter does not understand the 
                // OID for the hashing algorithm.
                //
                formatter.SetHashAlgorithm( "SHA256" );
                return formatter.CreateSignature( hash.Hash );
            }
            else
            {
                //
                // Calling the formatter with the object allows us to be Crypto-Agile
                //
                return formatter.CreateSignature( hash );
            }
        }

        /// <summary>
        /// Wrapper that verifies the signature for SHA256 taking into consideration the special logic for FIPS compliance
        /// </summary>
        /// <param name="deformatter">the signature deformatter</param>
        /// <param name="hash">the hash algorithm</param>
        /// <param name="signatureValue">the byte array for the signature value</param>
        /// <returns>true/false indicating if signature was verified or not</returns>
        internal static bool VerifySignatureForSha256( AsymmetricSignatureDeformatter deformatter, HashAlgorithm hash, byte[] signatureValue )
        {
            if ( SecurityUtils.RequiresFipsCompliance )
            {
                //
                // When FIPS is turned ON. We need to set the hash algorithm specifically 
                // else for SHA256 and FIPS turned ON, the underlying deformatter does not understand the 
                // OID for the hashing algorithm.
                //
                deformatter.SetHashAlgorithm( "SHA256" );
                return deformatter.VerifySignature( hash.Hash, signatureValue );
            }
            else
            {
                return deformatter.VerifySignature( hash, signatureValue );
            }
        }

        
        /// <summary>
        /// This method returns an AsymmetricSignatureFormatter capable of supporting Sha256 signatures. 
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        internal static AsymmetricSignatureFormatter GetSignatureFormatterForSha256( AsymmetricSecurityKey key )
        {
            AsymmetricAlgorithm algorithm = key.GetAsymmetricAlgorithm( SecurityAlgorithms.RsaSha256Signature, true );
            RSACryptoServiceProvider rsaProvider = algorithm as RSACryptoServiceProvider;
            if ( null != rsaProvider )
            {
                return GetSignatureFormatterForSha256( rsaProvider );
            }
            else
            {
                //
                // If not an RSaCryptoServiceProvider, we can only hope that
                //  the derived imlementation does the correct thing thing WRT Sha256.
                //
                return new RSAPKCS1SignatureFormatter( algorithm );
            }
        }

        /// <summary>
        /// This method returns an AsymmetricSignatureFormatter capable of supporting Sha256 signatures. 
        /// </summary>
        internal static AsymmetricSignatureFormatter GetSignatureFormatterForSha256( RSACryptoServiceProvider rsaProvider )
        {
            const int PROV_RSA_AES = 24;    // CryptoApi provider type for an RSA provider supporting sha-256 digital signatures
            AsymmetricSignatureFormatter formatter = null;
            CspParameters csp = new CspParameters();
            csp.ProviderType = PROV_RSA_AES;
            if ( PROV_RSA_AES == rsaProvider.CspKeyContainerInfo.ProviderType )
            {
                csp.ProviderName = rsaProvider.CspKeyContainerInfo.ProviderName;
            }
            csp.KeyContainerName = rsaProvider.CspKeyContainerInfo.KeyContainerName;
            csp.KeyNumber = (int)rsaProvider.CspKeyContainerInfo.KeyNumber;
            if ( rsaProvider.CspKeyContainerInfo.MachineKeyStore )
            {
                csp.Flags = CspProviderFlags.UseMachineKeyStore;
            }

            csp.Flags |= CspProviderFlags.UseExistingKey;
            rsaProvider = new RSACryptoServiceProvider( csp );
            formatter = new RSAPKCS1SignatureFormatter( rsaProvider );
            return formatter;
        }

        /// <summary>
        /// This method returns an AsymmetricSignatureDeFormatter capable of supporting Sha256 signatures. 
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        internal static AsymmetricSignatureDeformatter GetSignatureDeFormatterForSha256( AsymmetricSecurityKey key )
        {
            RSAPKCS1SignatureDeformatter deformatter;
            AsymmetricAlgorithm algorithm = key.GetAsymmetricAlgorithm( SecurityAlgorithms.RsaSha256Signature, false );
            RSACryptoServiceProvider rsaProvider = algorithm as RSACryptoServiceProvider;
            if ( null != rsaProvider )
            {
                return GetSignatureDeFormatterForSha256( rsaProvider );
            }
            else
            {
                //
                // If not an RSaCryptoServiceProvider, we can only hope that
                //  the derived imlementation does the correct thing WRT Sha256.
                //
                deformatter = new RSAPKCS1SignatureDeformatter( algorithm );
            }

            return deformatter;
        }

        /// <summary>
        /// This method returns an AsymmetricSignatureDeFormatter capable of supporting Sha256 signatures. 
        /// </summary>
        internal static AsymmetricSignatureDeformatter GetSignatureDeFormatterForSha256( RSACryptoServiceProvider rsaProvider )
        {
            const int PROV_RSA_AES = 24;    // CryptoApi provider type for an RSA provider supporting sha-256 digital signatures
            AsymmetricSignatureDeformatter deformatter = null;
            CspParameters csp = new CspParameters();
            csp.ProviderType = PROV_RSA_AES;
            if ( PROV_RSA_AES == rsaProvider.CspKeyContainerInfo.ProviderType )
            {
                csp.ProviderName = rsaProvider.CspKeyContainerInfo.ProviderName;
            }
            csp.KeyNumber = (int)rsaProvider.CspKeyContainerInfo.KeyNumber;
            if ( rsaProvider.CspKeyContainerInfo.MachineKeyStore )
            {
                csp.Flags = CspProviderFlags.UseMachineKeyStore;
            }
            
            csp.Flags |= CspProviderFlags.UseExistingKey;
            RSACryptoServiceProvider rsaPublicProvider = new RSACryptoServiceProvider( csp );
            rsaPublicProvider.ImportCspBlob( rsaProvider.ExportCspBlob( false ) );
            deformatter = new RSAPKCS1SignatureDeformatter( rsaPublicProvider );
            return deformatter;
        }

        internal static bool IsAsymmetricAlgorithm(string algorithm)
        {
            object algorithmObject = null;

            try
            {
                algorithmObject = GetAlgorithmFromConfig(algorithm);
            }
            catch (InvalidOperationException)
            {
                algorithmObject = null;
                // We ---- the exception and continue.
            }

            if (algorithmObject != null)
            {
                AsymmetricAlgorithm asymmetricAlgorithm = algorithmObject as AsymmetricAlgorithm;
                SignatureDescription signatureDescription = algorithmObject as SignatureDescription;
                if (asymmetricAlgorithm != null || signatureDescription != null)
                    return true;
                return false;
            }

            switch (algorithm)
            {
                case SecurityAlgorithms.DsaSha1Signature:
                case SecurityAlgorithms.RsaSha1Signature:
                case SecurityAlgorithms.RsaSha256Signature:
                case SecurityAlgorithms.RsaOaepKeyWrap:
                case SecurityAlgorithms.RsaV15KeyWrap:
                    return true;
                default:
                    return false;
            }
        }

        internal static bool IsSymmetricAlgorithm(string algorithm)
        {
            object algorithmObject = null;

            try
            {
                algorithmObject = GetAlgorithmFromConfig(algorithm);
            }
            catch (InvalidOperationException)
            {
                algorithmObject = null;
                // We ---- the exception and continue.
            }
            if (algorithmObject != null)
            {
                SymmetricAlgorithm symmetricAlgorithm = algorithmObject as SymmetricAlgorithm;
                KeyedHashAlgorithm keyedHashAlgorithm = algorithmObject as KeyedHashAlgorithm;
                if (symmetricAlgorithm != null || keyedHashAlgorithm != null)
                    return true;
                return false;
            }

            // NOTE: A KeyedHashAlgorithm is symmetric in nature.

            switch (algorithm)
            {
                case SecurityAlgorithms.DsaSha1Signature:
                case SecurityAlgorithms.RsaSha1Signature:
                case SecurityAlgorithms.RsaSha256Signature:
                case SecurityAlgorithms.RsaOaepKeyWrap:
                case SecurityAlgorithms.RsaV15KeyWrap:
                    return false;
                case SecurityAlgorithms.HmacSha1Signature:
                case SecurityAlgorithms.HmacSha256Signature:
                case SecurityAlgorithms.Aes128Encryption:
                case SecurityAlgorithms.Aes192Encryption:
                case SecurityAlgorithms.DesEncryption:
                case SecurityAlgorithms.Aes256Encryption:
                case SecurityAlgorithms.TripleDesEncryption:
                case SecurityAlgorithms.Aes128KeyWrap:
                case SecurityAlgorithms.Aes192KeyWrap:
                case SecurityAlgorithms.Aes256KeyWrap:
                case SecurityAlgorithms.TripleDesKeyWrap:
                case SecurityAlgorithms.Psha1KeyDerivation:
                case SecurityAlgorithms.Psha1KeyDerivationDec2005:
                    return true;
                default:
                    return false;
            }
        }

        internal static bool IsSymmetricSupportedAlgorithm(string algorithm, int keySize)
        {
            bool found = false;
            object algorithmObject = null;

            try
            {
                algorithmObject = GetAlgorithmFromConfig(algorithm);
            }
            catch (InvalidOperationException)
            {
                // We ---- the exception and continue.
            }
            if (algorithmObject != null)
            {
                SymmetricAlgorithm symmetricAlgorithm = algorithmObject as SymmetricAlgorithm;
                KeyedHashAlgorithm keyedHashAlgorithm = algorithmObject as KeyedHashAlgorithm;

                if (symmetricAlgorithm != null || keyedHashAlgorithm != null)
                    found = true;
                // The reason we do not return here even when the user has provided a custom algorithm in machine.config 
                // is because we need to check if the user has overwritten an existing standard URI.
            }

            switch (algorithm)
            {
                case SecurityAlgorithms.DsaSha1Signature:
                case SecurityAlgorithms.RsaSha1Signature:
                case SecurityAlgorithms.RsaSha256Signature:
                case SecurityAlgorithms.RsaOaepKeyWrap:
                case SecurityAlgorithms.RsaV15KeyWrap:
                    return false;
                case SecurityAlgorithms.HmacSha1Signature:
                case SecurityAlgorithms.HmacSha256Signature:
                case SecurityAlgorithms.Psha1KeyDerivation:
                case SecurityAlgorithms.Psha1KeyDerivationDec2005:
                    return true;
                case SecurityAlgorithms.Aes128Encryption:
                case SecurityAlgorithms.Aes128KeyWrap:
                    return keySize >= 128 && keySize <= 256;
                case SecurityAlgorithms.Aes192Encryption:
                case SecurityAlgorithms.Aes192KeyWrap:
                    return keySize >= 192 && keySize <= 256;
                case SecurityAlgorithms.Aes256Encryption:
                case SecurityAlgorithms.Aes256KeyWrap:
                    return keySize == 256;
                case SecurityAlgorithms.TripleDesEncryption:
                case SecurityAlgorithms.TripleDesKeyWrap:
                    return keySize == 128 || keySize == 192;
                default:
                    if (found)
                        return true;
                    return false;
                // We do not expect the user to map the uri of an existing standrad algorithm with say key size 128 bit 
                // to a custom algorithm with keySize 192 bits. If he does that, we anyways make sure that we return false.
            }
        }

        // We currently call the CLR APIs to do symmetric key wrap.
        // This ends up causing a triple cloning of the byte arrays.
        // However, the symmetric key wrap exists now primarily for
        // the feature completeness of cryptos and tokens.  That is,
        // it is never encountered in any Indigo AuthenticationMode.
        // The performance of this should be reviewed if this gets hit
        // in any mainline scenario.
        internal static byte[] UnwrapKey(byte[] wrappingKey, byte[] wrappedKey, string algorithm)
        {
            SymmetricAlgorithm symmetricAlgorithm;
            object algorithmObject = GetAlgorithmFromConfig(algorithm);
            if (algorithmObject != null)
            {
                symmetricAlgorithm = algorithmObject as SymmetricAlgorithm;
                if (symmetricAlgorithm == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new InvalidOperationException(SR.GetString(SR.InvalidCustomKeyWrapAlgorithm, algorithm)));
                }
                using (symmetricAlgorithm)
                {
                    symmetricAlgorithm.Key = wrappingKey;
                    return EncryptedXml.DecryptKey(wrappedKey, symmetricAlgorithm);
                }
            }
            switch (algorithm)
            {
                case SecurityAlgorithms.TripleDesKeyWrap:
                    symmetricAlgorithm = new TripleDESCryptoServiceProvider();
                    break;
                case SecurityAlgorithms.Aes128KeyWrap:
                case SecurityAlgorithms.Aes192KeyWrap:
                case SecurityAlgorithms.Aes256KeyWrap:
                    symmetricAlgorithm = SecurityUtils.RequiresFipsCompliance ? (Rijndael)new RijndaelCryptoServiceProvider() : new RijndaelManaged();
                    break;
                default:
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new InvalidOperationException(SR.GetString(SR.UnsupportedKeyWrapAlgorithm, algorithm)));
            }

            using (symmetricAlgorithm)
            {
                symmetricAlgorithm.Key = wrappingKey;
                return EncryptedXml.DecryptKey(wrappedKey, symmetricAlgorithm);
            }
        }

        internal static byte[] WrapKey(byte[] wrappingKey, byte[] keyToBeWrapped, string algorithm)
        {
            SymmetricAlgorithm symmetricAlgorithm;
            object algorithmObject = GetAlgorithmFromConfig(algorithm);
            if (algorithmObject != null)
            {
                symmetricAlgorithm = algorithmObject as SymmetricAlgorithm;
                if (symmetricAlgorithm == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new InvalidOperationException(SR.GetString(SR.InvalidCustomKeyWrapAlgorithm, algorithm)));
                }
                using (symmetricAlgorithm)
                {
                    symmetricAlgorithm.Key = wrappingKey;
                    return EncryptedXml.EncryptKey(keyToBeWrapped, symmetricAlgorithm);
                }
            }

            switch (algorithm)
            {
                case SecurityAlgorithms.TripleDesKeyWrap:
                    symmetricAlgorithm = new TripleDESCryptoServiceProvider();
                    break;
                case SecurityAlgorithms.Aes128KeyWrap:
                case SecurityAlgorithms.Aes192KeyWrap:
                case SecurityAlgorithms.Aes256KeyWrap:
                    symmetricAlgorithm = SecurityUtils.RequiresFipsCompliance ? (Rijndael)new RijndaelCryptoServiceProvider() : new RijndaelManaged();
                    break;
                default:
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new InvalidOperationException(SR.GetString(SR.UnsupportedKeyWrapAlgorithm, algorithm)));
            }

            using (symmetricAlgorithm)
            {
                symmetricAlgorithm.Key = wrappingKey;
                return EncryptedXml.EncryptKey(keyToBeWrapped, symmetricAlgorithm);
            }
        }

        internal static void ValidateBufferBounds(Array buffer, int offset, int count)
        {
            if (buffer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("buffer"));
            }
            if (count < 0 || count > buffer.Length)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("count", SR.GetString(SR.ValueMustBeInRange, 0, buffer.Length)));
            }
            if (offset < 0 || offset > buffer.Length - count)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("offset", SR.GetString(SR.ValueMustBeInRange, 0, buffer.Length - count)));
            }
        }

        internal static bool IsEqual(byte[] a, byte[] b)
        {
            if (ReferenceEquals(a, b))
            {
                return true;
            }

            if (a == null || b == null || a.Length != b.Length)
            {
                return false;
            }

            for (int i = 0; i < a.Length; i++)
            {
                if (a[i] != b[i])
                {
                    return false;
                }
            }
            return true;
        }

        private static object GetDefaultAlgorithm(string algorithm)
        {
            if (string.IsNullOrEmpty(algorithm))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("algorithm"));
            }

            switch (algorithm)
            {
                //case SecurityAlgorithms.RsaSha1Signature:
                //case SecurityAlgorithms.DsaSha1Signature:
                // For these algorithms above, crypto config returns internal objects.
                // As we cannot create those internal objects, we are returning null.
                // If no custom algorithm is plugged-in, at least these two algorithms
                // will be inside the delegate dictionary.
                case SecurityAlgorithms.Sha1Digest:
                    if (SecurityUtils.RequiresFipsCompliance)
                        return new SHA1CryptoServiceProvider();
                    else
                        return new SHA1Managed();
                case SecurityAlgorithms.ExclusiveC14n:
                    return new XmlDsigExcC14NTransform();
                case SHA256String:
                case SecurityAlgorithms.Sha256Digest:
                    if (SecurityUtils.RequiresFipsCompliance)
                        return new SHA256CryptoServiceProvider();
                    else
                        return new SHA256Managed();
                case SecurityAlgorithms.Sha512Digest:
                    if (SecurityUtils.RequiresFipsCompliance)
                        return new SHA512CryptoServiceProvider();
                    else
                        return new SHA512Managed();
                case SecurityAlgorithms.Aes128Encryption:
                case SecurityAlgorithms.Aes192Encryption:
                case SecurityAlgorithms.Aes256Encryption:
                case SecurityAlgorithms.Aes128KeyWrap:
                case SecurityAlgorithms.Aes192KeyWrap:
                case SecurityAlgorithms.Aes256KeyWrap:
                    if (SecurityUtils.RequiresFipsCompliance)
                        return new RijndaelCryptoServiceProvider();
                    else
                        return new RijndaelManaged();
                case SecurityAlgorithms.TripleDesEncryption:
                case SecurityAlgorithms.TripleDesKeyWrap:
                    return new TripleDESCryptoServiceProvider();
                case SecurityAlgorithms.HmacSha1Signature:
                    byte[] key = new byte[64];
                    new RNGCryptoServiceProvider().GetBytes(key);
                    return new HMACSHA1(key, !SecurityUtils.RequiresFipsCompliance);
                case SecurityAlgorithms.HmacSha256Signature:
                    if (!SecurityUtils.RequiresFipsCompliance)
                        return new HMACSHA256();
                    return null;
                case SecurityAlgorithms.ExclusiveC14nWithComments:
                    return new XmlDsigExcC14NWithCommentsTransform();
                case SecurityAlgorithms.Ripemd160Digest:
                    if (!SecurityUtils.RequiresFipsCompliance)
                        return new RIPEMD160Managed();
                    return null;
                case SecurityAlgorithms.DesEncryption:
                    return new DESCryptoServiceProvider();
                default:
                    return null;
            }
        }

        internal static object GetAlgorithmFromConfig(string algorithm)
        {
            if (string.IsNullOrEmpty(algorithm))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("algorithm"));
            }

            object algorithmObject = null;
            object defaultObject = null;
            Func<object> delegateFunction = null;

            if (!algorithmDelegateDictionary.TryGetValue(algorithm, out delegateFunction))
            {
                lock (AlgorithmDictionaryLock)
                {
                    if (!algorithmDelegateDictionary.ContainsKey(algorithm))
                    {
                        try
                        {
                            algorithmObject = CryptoConfig.CreateFromName(algorithm);
                        }
                        catch (TargetInvocationException)
                        {
                            algorithmDelegateDictionary[algorithm] = null;
                        }

                        if (algorithmObject == null)
                        {
                            algorithmDelegateDictionary[algorithm] = null;
                        }
                        else
                        {
                            defaultObject = GetDefaultAlgorithm(algorithm);
                            if ((!SecurityUtils.RequiresFipsCompliance && algorithmObject is SHA1CryptoServiceProvider)
                                || (defaultObject != null && defaultObject.GetType() == algorithmObject.GetType()))
                            {
                                algorithmDelegateDictionary[algorithm] = null;
                            }
                            else
                            {

                                // Create a factory delegate which returns new instances of the algorithm type for later calls.
                                Type algorithmType = algorithmObject.GetType();
                                System.Linq.Expressions.NewExpression algorithmCreationExpression = System.Linq.Expressions.Expression.New(algorithmType);
                                System.Linq.Expressions.LambdaExpression creationFunction = System.Linq.Expressions.Expression.Lambda<Func<object>>(algorithmCreationExpression);
                                delegateFunction = creationFunction.Compile() as Func<object>;

                                if (delegateFunction != null)
                                {
                                    algorithmDelegateDictionary[algorithm] = delegateFunction;
                                }
                                return algorithmObject;
                            }
                        }
                    }
                }
            }
            else
            {
                if (delegateFunction != null)
                {
                    return delegateFunction.Invoke();
                }
            }

            //
            // This is a fallback in case CryptoConfig fails to return a valid
            // algorithm object. CrytoConfig does not understand all the uri's and
            // can return a null in that case, in which case it is our responsibility
            // to fallback and create the right algorithm if it is a uri we understand
            //
            switch (algorithm)
            {
                case SHA256String:
                case SecurityAlgorithms.Sha256Digest:
                    if (SecurityUtils.RequiresFipsCompliance)
                    {
                        return new SHA256CryptoServiceProvider();
                    }
                    else
                    {
                        return new SHA256Managed();
                    }
                case SecurityAlgorithms.Sha1Digest:
                    if (SecurityUtils.RequiresFipsCompliance)
                    {
                        return new SHA1CryptoServiceProvider();
                    }
                    else
                    {
                        return new SHA1Managed();
                    }
                case SecurityAlgorithms.HmacSha1Signature:
                    return new HMACSHA1(GenerateRandomBytes(64),
                                                    !SecurityUtils.RequiresFipsCompliance /* indicates the managed version of the algortithm */ );
                default:
                    break;
            }

            return null;
        }

        public static void ResetAllCertificates(X509Certificate2Collection certificates)
        {
            if (certificates != null)
            {
                for (int i = 0; i < certificates.Count; ++i)
                {
                    certificates[i].Reset();
                }
            }
        }

    }
}
           
