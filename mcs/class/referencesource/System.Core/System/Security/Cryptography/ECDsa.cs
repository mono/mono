// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==

using System;
using System.IO;
#if MONO
using System.Buffers;
#endif

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

        /// <summary>
        /// Creates a new instance of the default implementation of the Elliptic Curve Digital Signature Algorithm
        /// (ECDSA) with a newly generated key over the specified curve.
        /// </summary>
        /// <param name="curve">The curve to use for key generation.</param>
        /// <returns>A new instance of the default implementation of this class.</returns>
        public static ECDsa Create(ECCurve curve) {
            ECDsa ecdsa = Create();

            if (ecdsa != null) {
                try {
                    ecdsa.GenerateKey(curve);
                }
                catch {
                    ecdsa.Dispose();
                    throw;
                }
            }

            return ecdsa;
        }

        /// <summary>
        /// Creates a new instance of the default implementation of the Elliptic Curve Digital Signature Algorithm
        /// (ECDSA) using the specified ECParameters as the key.
        /// </summary>
        /// <param name="parameters">The parameters representing the key to use.</param>
        /// <returns>A new instance of the default implementation of this class.</returns>
        public static ECDsa Create(ECParameters parameters) {
            ECDsa ecdsa = Create();

            if (ecdsa != null) {
                try {
                    ecdsa.ImportParameters(parameters);
                }
                catch {
                    ecdsa.Dispose();
                    throw;
                }
            }

            return ecdsa;
        }

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

        /// <summary>
        /// When overridden in a derived class, exports the named or explicit ECParameters for an ECCurve.
        /// If the curve has a name, the Curve property will contain named curve parameters, otherwise it
        /// will contain explicit parameters.
        /// </summary>
        /// <param name="includePrivateParameters">true to include private parameters, otherwise, false.</param>
        /// <returns>The ECParameters representing the point on the curve for this key.</returns>
        public virtual ECParameters ExportParameters(bool includePrivateParameters) {
            throw new NotSupportedException(SR.GetString(SR.NotSupported_SubclassOverride));
        }

        /// <summary>
        /// When overridden in a derived class, exports the explicit ECParameters for an ECCurve.
        /// </summary>
        /// <param name="includePrivateParameters">true to include private parameters, otherwise, false.</param>
        /// <returns>The ECParameters representing the point on the curve for this key, using the explicit curve format.</returns>
        public virtual ECParameters ExportExplicitParameters(bool includePrivateParameters) {
            throw new NotSupportedException(SR.GetString(SR.NotSupported_SubclassOverride));
        }

        /// <summary>
        /// When overridden in a derived class, imports the specified ECParameters.
        /// </summary>
        /// <param name="parameters">The curve parameters.</param>
        public virtual void ImportParameters(ECParameters parameters) {
            throw new NotSupportedException(SR.GetString(SR.NotSupported_SubclassOverride));
        }

        /// <summary>
        /// When overridden in a derived class, generates a new public/private keypair for the specified curve.
        /// </summary>
        /// <param name="curve">The curve to use.</param>
        public virtual void GenerateKey(ECCurve curve) {
            throw new NotSupportedException(SR.GetString(SR.NotSupported_SubclassOverride));
        }

        private static Exception DerivedClassMustOverride() {
            return new NotImplementedException(SR.GetString(SR.NotSupported_SubclassOverride));
        }

        internal static Exception HashAlgorithmNameNullOrEmpty() {
            return new ArgumentException(SR.GetString(SR.Cryptography_HashAlgorithmNameNullOrEmpty), "hashAlgorithm");
        }

#if MONO // these methods were copied from CoreFX for NS2.1 support
        protected virtual bool TryHashData(ReadOnlySpan<byte> data, Span<byte> destination, HashAlgorithmName hashAlgorithm, out int bytesWritten)
        {
            byte[] array = ArrayPool<byte>.Shared.Rent(data.Length);
            try
            {
                data.CopyTo(array);
                byte[] hash = HashData(array, 0, data.Length, hashAlgorithm);
                if (hash.Length <= destination.Length)
                {
                    new ReadOnlySpan<byte>(hash).CopyTo(destination);
                    bytesWritten = hash.Length;
                    return true;
                }
                else
                {
                    bytesWritten = 0;
                    return false;
                }
            }
            finally
            {
                Array.Clear(array, 0, data.Length);
                ArrayPool<byte>.Shared.Return(array);
            }
        }

        public virtual bool TrySignHash(ReadOnlySpan<byte> hash, Span<byte> destination, out int bytesWritten)
        {
            byte[] result = SignHash(hash.ToArray());
            if (result.Length <= destination.Length)
            {
                new ReadOnlySpan<byte>(result).CopyTo(destination);
                bytesWritten = result.Length;
                return true;
            }
            else
            {
                bytesWritten = 0;
                return false;
            }
        }

        public virtual bool VerifyHash(ReadOnlySpan<byte> hash, ReadOnlySpan<byte> signature) =>
            VerifyHash(hash.ToArray(), signature.ToArray());

        public virtual bool TrySignData(ReadOnlySpan<byte> data, Span<byte> destination, HashAlgorithmName hashAlgorithm, out int bytesWritten)
        {
            if (string.IsNullOrEmpty(hashAlgorithm.Name))
            {
                throw new ArgumentException(SR.Cryptography_HashAlgorithmNameNullOrEmpty, nameof(hashAlgorithm));
            }

            if (TryHashData(data, destination, hashAlgorithm, out int hashLength) &&
                TrySignHash(destination.Slice(0, hashLength), destination, out bytesWritten))
            {
                return true;
            }

            bytesWritten = 0;
            return false;
        }

        public virtual bool VerifyData(ReadOnlySpan<byte> data, ReadOnlySpan<byte> signature, HashAlgorithmName hashAlgorithm)
        {
            if (string.IsNullOrEmpty(hashAlgorithm.Name))
            {
                throw new ArgumentException(SR.Cryptography_HashAlgorithmNameNullOrEmpty, nameof(hashAlgorithm));
            }

            for (int i = 256; ; i = checked(i * 2))
            {
                int hashLength = 0;
                byte[] hash = ArrayPool<byte>.Shared.Rent(i);
                try
                {
                    if (TryHashData(data, hash, hashAlgorithm, out hashLength))
                    {
                        return VerifyHash(new ReadOnlySpan<byte>(hash, 0, hashLength), signature);
                    }
                }
                finally
                {
                    Array.Clear(hash, 0, hashLength);
                    ArrayPool<byte>.Shared.Return(hash);
                }
            }
        }

        public virtual byte[] ExportECPrivateKey () => throw new PlatformNotSupportedException ();
        
        public virtual bool TryExportECPrivateKey (System.Span<byte> destination, out int bytesWritten) => throw new PlatformNotSupportedException ();
        
        public virtual void ImportECPrivateKey (System.ReadOnlySpan<byte> source, out int bytesRead) => throw new PlatformNotSupportedException ();
#endif
    }
}
