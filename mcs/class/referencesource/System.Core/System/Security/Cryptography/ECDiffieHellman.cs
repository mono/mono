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
            return Create(typeof(ECDiffieHellmanCng).FullName);
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
        public abstract byte[] DeriveKeyMaterial(ECDiffieHellmanPublicKey otherPartyPublicKey);
    }
}
