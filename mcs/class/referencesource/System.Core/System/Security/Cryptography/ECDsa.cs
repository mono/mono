// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==

using System;
using System.IO;

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
#if MONO
            throw new NotImplementedException ();
#else
            return Create(typeof(ECDsaCng).FullName);
#endif
        }

        public static new ECDsa Create(string algorithm) {
            if (algorithm == null) {
                throw new ArgumentNullException("algorithm");
            }

            return CryptoConfig.CreateFromName(algorithm) as ECDsa;
        }

#if NETSTANDARD
        public static ECDsa Create (ECCurve curve)
        {
            throw new NotImplementedException ();
        }

        public static ECDsa Create (ECParameters parameters)
        {
            throw new NotImplementedException ();
        }

        public virtual ECParameters ExportExplicitParameters (bool includePrivateParameters)
        {
            throw new NotImplementedException ();
        }

        public virtual ECParameters ExportParameters (bool includePrivateParameters)
        {
            throw new NotImplementedException ();
        }

        public virtual void GenerateKey (ECCurve curve)
        {
            throw new NotImplementedException ();
        }

        public virtual void ImportParameters (ECParameters parameters)
        {
            throw new NotImplementedException ();
        }
#endif

        //
        // Signature operations
        //

        // ECDsa does not encode the algorithm identifier into the signature blob, therefore SignHash and VerifyHash
        // do not need the HashAlgorithmName value, only SignData and VerifyData do.
        public abstract byte[] SignHash(byte[] hash);
        public abstract bool VerifyHash(byte[] hash, byte[] signature);

        protected virtual byte[] HashData(byte[] data, int offset, int count, HashAlgorithmName hashAlgorithm) {
            throw DerivedClassMustOverride();
        }

        protected virtual byte[] HashData(Stream data, HashAlgorithmName hashAlgorithm) {
            throw DerivedClassMustOverride();
        }

        public virtual byte[] SignData(byte[] data, HashAlgorithmName hashAlgorithm) {
            if (data == null) {
                throw new ArgumentNullException("data");
            }
            return SignData(data, 0, data.Length, hashAlgorithm);
        }

        public virtual byte[] SignData(byte[] data, int offset, int count, HashAlgorithmName hashAlgorithm) {
            if (data == null) { throw new ArgumentNullException("data"); }
            if (offset < 0 || offset > data.Length) { throw new ArgumentOutOfRangeException("offset"); }
            if (count < 0 || count > data.Length - offset) { throw new ArgumentOutOfRangeException("count"); }
            if (String.IsNullOrEmpty(hashAlgorithm.Name)) { throw HashAlgorithmNameNullOrEmpty(); }

            byte[] hash = HashData(data, offset, count, hashAlgorithm);
            return SignHash(hash);
        }

        public virtual byte[] SignData(Stream data, HashAlgorithmName hashAlgorithm) {
            if (data == null) {
                throw new ArgumentNullException("data");
            }
            if (String.IsNullOrEmpty(hashAlgorithm.Name)) {
                throw HashAlgorithmNameNullOrEmpty();
            }

            byte[] hash = HashData(data, hashAlgorithm);
            return SignHash(hash);
        }

        public bool VerifyData(byte[] data, byte[] signature, HashAlgorithmName hashAlgorithm) {
            if (data == null) {
                throw new ArgumentNullException("data");
            }
            return VerifyData(data, 0, data.Length, signature, hashAlgorithm);
        }

        public virtual bool VerifyData(byte[] data, int offset, int count, byte[] signature, HashAlgorithmName hashAlgorithm) {
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
            if (String.IsNullOrEmpty(hashAlgorithm.Name)) {
                throw HashAlgorithmNameNullOrEmpty();
            }

            byte[] hash = HashData(data, offset, count, hashAlgorithm);
            return VerifyHash(hash, signature);
        }

        public bool VerifyData(Stream data, byte[] signature, HashAlgorithmName hashAlgorithm) {
            if (data == null) {
                throw new ArgumentNullException("data");
            }
            if (signature == null) {
                throw new ArgumentNullException("signature");
            }
            if (String.IsNullOrEmpty(hashAlgorithm.Name)) {
                throw HashAlgorithmNameNullOrEmpty();
            }

            byte[] hash = HashData(data, hashAlgorithm);
            return VerifyHash(hash, signature);
        }

        private static Exception DerivedClassMustOverride() {
            return new NotImplementedException(SR.GetString(SR.NotSupported_SubclassOverride));
        }

        internal static Exception HashAlgorithmNameNullOrEmpty() {
            return new ArgumentException(SR.GetString(SR.Cryptography_HashAlgorithmNameNullOrEmpty), "hashAlgorithm");
        }
    }
}
