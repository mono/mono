// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==

using System;

namespace System.Security.Cryptography {
    /// <summary>
    ///     Base class for implementations of elliptic curve DSA
    /// </summary>
    [System.Security.Permissions.HostProtection(MayLeakOnAbort = true)]
    public abstract class ECDsa : AsymmetricAlgorithm {
        public override string KeyExchangeAlgorithm {
            get { return null; }
        }

        public override string SignatureAlgorithm {
            get { return "ECDsa"; }
        }

        //
        // Creation factory methods
        //

        public static new ECDsa Create() {
            return Create(typeof(ECDsaCng).FullName);
        }

        public static new ECDsa Create(string algorithm) {
            if (algorithm == null) {
                throw new ArgumentNullException("algorithm");
            }

            return CryptoConfig.CreateFromName(algorithm) as ECDsa;
        }

        //
        // Signature operations
        //

        public abstract byte[] SignHash(byte[] hash);
        public abstract bool VerifyHash(byte[] hash, byte[] signature);
    }
}
