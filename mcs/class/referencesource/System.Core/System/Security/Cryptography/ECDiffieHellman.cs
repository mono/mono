// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==

using System;
using System.Runtime.Serialization;

namespace System.Security.Cryptography {
    /// <summary>
    ///     Abstract base class for implementations of elliptic curve Diffie-Hellman to derive from
    /// </summary>
    [System.Security.Permissions.HostProtection(MayLeakOnAbort = true)]
    public abstract class ECDiffieHellman : AsymmetricAlgorithm {
        public override string KeyExchangeAlgorithm {
            get { return "ECDiffieHellman"; }
        }

        public override string SignatureAlgorithm {
            get { return null; }
        }

        //
        // Creation factory methods
        //

        public static new ECDiffieHellman Create() {
#if MONO
            throw new NotImplementedException ();
#else
            return Create(typeof(ECDiffieHellmanCng).FullName);
#endif
        }

        public static new ECDiffieHellman Create(string algorithm) {
            if (algorithm == null) {
                throw new ArgumentNullException("algorithm");
            }

            return CryptoConfig.CreateFromName(algorithm) as ECDiffieHellman;
        }

        //
        // Key derivation
        //

        public abstract ECDiffieHellmanPublicKey PublicKey { get; }

        // This method must be implemented by derived classes. In order to conform to the contract, it cannot be abstract.
        public virtual byte[] DeriveKeyMaterial(ECDiffieHellmanPublicKey otherPartyPublicKey)
        {
            throw DerivedClassMustOverride();
        }

        /// <summary>
        /// Derive key material using the formula HASH(x) where x is the computed result of the EC Diffie-Hellman algorithm.
        /// </summary>
        /// <param name="otherPartyPublicKey">The public key of the party with which to derive a mutual secret.</param>
        /// <param name="hashAlgorithm">The identifier for the hash algorithm to use.</param>
        /// <returns>A hashed output suitable for key material</returns>
        /// <exception cref="ArgumentException"><paramref name="otherPartyPublicKey"/> is over a different curve than this key</exception>
        public byte[] DeriveKeyFromHash(ECDiffieHellmanPublicKey otherPartyPublicKey, HashAlgorithmName hashAlgorithm)
        {
            return DeriveKeyFromHash(otherPartyPublicKey, hashAlgorithm, null, null);
        }

        /// <summary>
        /// Derive key material using the formula HASH(secretPrepend || x || secretAppend) where x is the computed
        /// result of the EC Diffie-Hellman algorithm.
        /// </summary>
        /// <param name="otherPartyPublicKey">The public key of the party with which to derive a mutual secret.</param>
        /// <param name="hashAlgorithm">The identifier for the hash algorithm to use.</param>
        /// <param name="secretPrepend">A value to prepend to the derived secret before hashing. A <c>null</c> value is treated as an empty array.</param>
        /// <param name="secretAppend">A value to append to the derived secret before hashing. A <c>null</c> value is treated as an empty array.</param>
        /// <returns>A hashed output suitable for key material</returns>
        /// <exception cref="ArgumentException"><paramref name="otherPartyPublicKey"/> is over a different curve than this key</exception>
        public virtual byte[] DeriveKeyFromHash(
            ECDiffieHellmanPublicKey otherPartyPublicKey,
            HashAlgorithmName hashAlgorithm,
            byte[] secretPrepend,
            byte[] secretAppend)
        {
            throw DerivedClassMustOverride();
        }

        /// <summary>
        /// Derive key material using the formula HMAC(hmacKey, x) where x is the computed
        /// result of the EC Diffie-Hellman algorithm.
        /// </summary>
        /// <param name="otherPartyPublicKey">The public key of the party with which to derive a mutual secret.</param>
        /// <param name="hashAlgorithm">The identifier for the hash algorithm to use.</param>
        /// <param name="hmacKey">The key to use in the HMAC. A <c>null</c> value indicates that the result of the EC Diffie-Hellman algorithm should be used as the HMAC key.</param>
        /// <returns>A hashed output suitable for key material</returns>
        /// <exception cref="ArgumentException"><paramref name="otherPartyPublicKey"/> is over a different curve than this key</exception>
        public byte[] DeriveKeyFromHmac(
            ECDiffieHellmanPublicKey otherPartyPublicKey,
            HashAlgorithmName hashAlgorithm,
            byte[] hmacKey)
        {
            return DeriveKeyFromHmac(otherPartyPublicKey, hashAlgorithm, hmacKey, null, null);
        }

        /// <summary>
        /// Derive key material using the formula HMAC(hmacKey, secretPrepend || x || secretAppend) where x is the computed
        /// result of the EC Diffie-Hellman algorithm.
        /// </summary>
        /// <param name="otherPartyPublicKey">The public key of the party with which to derive a mutual secret.</param>
        /// <param name="hashAlgorithm">The identifier for the hash algorithm to use.</param>
        /// <param name="hmacKey">The key to use in the HMAC. A <c>null</c> value indicates that the result of the EC Diffie-Hellman algorithm should be used as the HMAC key.</param>
        /// <param name="secretPrepend">A value to prepend to the derived secret before hashing. A <c>null</c> value is treated as an empty array.</param>
        /// <param name="secretAppend">A value to append to the derived secret before hashing. A <c>null</c> value is treated as an empty array.</param>
        /// <returns>A hashed output suitable for key material</returns>
        /// <exception cref="ArgumentException"><paramref name="otherPartyPublicKey"/> is over a different curve than this key</exception>
        public virtual byte[] DeriveKeyFromHmac(
            ECDiffieHellmanPublicKey otherPartyPublicKey,
            HashAlgorithmName hashAlgorithm,
            byte[] hmacKey,
            byte[] secretPrepend,
            byte[] secretAppend)
        {
            throw DerivedClassMustOverride();
        }

        /// <summary>
        /// Derive key material using the TLS pseudo-random function (PRF) derivation algorithm.
        /// </summary>
        /// <param name="otherPartyPublicKey">The public key of the party with which to derive a mutual secret.</param>
        /// <param name="prfLabel">The ASCII encoded PRF label.</param>
        /// <param name="prfSeed">The 64-byte PRF seed.</param>
        /// <returns>A 48-byte output of the TLS pseudo-random function.</returns>
        /// <exception cref="ArgumentException"><paramref name="otherPartyPublicKey"/> is over a different curve than this key</exception>
        /// <exception cref="ArgumentNullException"><paramref name="prfLabel"/> is null</exception>
        /// <exception cref="ArgumentNullException"><paramref name="prfSeed"/> is null</exception>
        /// <exception cref="CryptographicException"><paramref name="prfSeed"/> is not exactly 64 bytes in length</exception>
        public virtual byte[] DeriveKeyTls(ECDiffieHellmanPublicKey otherPartyPublicKey, byte[] prfLabel, byte[] prfSeed)
        {
            throw DerivedClassMustOverride();
        }

        private static Exception DerivedClassMustOverride()
        {
            return new NotImplementedException(SR.GetString(SR.NotSupported_SubclassOverride));
        }
    }
}
