//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

using System;
using System.IdentityModel.Selectors;

namespace System.IdentityModel.Tokens
{

    /// <summary>
    /// SecurityKeyElement provides delayed resolution of security keys by resolving the SecurityKeyIdentifierClause or SecurityKeyIdentifier 
    /// only when cryptographic functions are needed.  This allows a key clause or identifier that is never used by an application
    /// to be serialized and deserialzied on and off the wire without issue.
    /// </summary>
    public class SecurityKeyElement : SecurityKey
    {
        SecurityKey _securityKey;
        object _keyLock;
        SecurityTokenResolver _securityTokenResolver;
        SecurityKeyIdentifier _securityKeyIdentifier;

        /// <summary>
        /// Constructor to use when working with SecurityKeyIdentifierClauses
        /// </summary>
        /// <param name="securityKeyIdentifierClause">SecurityKeyIdentifierClause that represents a SecuriytKey</param>
        /// <param name="securityTokenResolver">SecurityTokenResolver that can be resolved to a SecurityKey</param>
        /// <exception cref="ArgumentNullException">Thrown if the 'clause' is null</exception>
        public SecurityKeyElement(SecurityKeyIdentifierClause securityKeyIdentifierClause, SecurityTokenResolver securityTokenResolver)
        {
            if (securityKeyIdentifierClause == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("securityKeyIdentifierClause");
            }

            Initialize(new SecurityKeyIdentifier(securityKeyIdentifierClause), securityTokenResolver);
        }

        /// <summary>
        /// Constructor to use when working with SecurityKeyIdentifiers
        /// </summary>
        /// <param name="securityKeyIdentifier">SecurityKeyIdentifier that represents a SecuriytKey</param>
        /// <param name="securityTokenResolver">SecurityTokenResolver that can be resolved to a SecurityKey</param>
        /// <exception cref="ArgumentNullException">Thrown if the 'securityKeyIdentifier' is null</exception>
        public SecurityKeyElement(SecurityKeyIdentifier securityKeyIdentifier, SecurityTokenResolver securityTokenResolver)
        {
            if (securityKeyIdentifier == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("securityKeyIdentifier");
            }

            Initialize(securityKeyIdentifier, securityTokenResolver);
        }

        void Initialize(SecurityKeyIdentifier securityKeyIdentifier, SecurityTokenResolver securityTokenResolver)
        {
            _keyLock = new object();
            _securityKeyIdentifier = securityKeyIdentifier;
            _securityTokenResolver = securityTokenResolver;
        }

        /// <summary>
        /// Decrypts a key using the specified algorithm.
        /// </summary>
        /// <param name="algorithm">Algorithm to use when decrypting the key.</param>
        /// <param name="keyData">Bytes representing the encrypted key.</param>
        /// <returns>Decrypted bytes.</returns>
        public override byte[] DecryptKey(string algorithm, byte[] keyData)
        {
            if (_securityKey == null)
            {
                ResolveKey();
            }

            return _securityKey.DecryptKey(algorithm, keyData);
        }

        /// <summary>
        /// Encrypts a key using the specified algorithm.
        /// </summary>
        /// <param name="algorithm">Algorithm to use when encrypting the key.</param>
        /// <param name="keyData">Bytes representing the key.</param>
        /// <returns>Encrypted bytes.</returns>
        public override byte[] EncryptKey(string algorithm, byte[] keyData)
        {
            if (_securityKey == null)
            {
                ResolveKey();
            }

            return _securityKey.EncryptKey(algorithm, keyData);
        }

        /// <summary>
        /// Answers question: is the algorithm Asymmetric.
        /// </summary>
        /// <param name="algorithm">Algorithm to check.</param>
        /// <returns>True if algorithm will be processed by runtime as Asymmetric.</returns>
        public override bool IsAsymmetricAlgorithm(string algorithm)
        {
            // Copied from System.IdentityModel.CryptoHelper
            // no need to ResolveKey

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

        /// <summary>
        /// Answers question: is the algorithm is supported by this key.
        /// </summary>
        /// <param name="algorithm">Algorithm to check.</param>
        /// <returns>True if algorithm is supported by this key.</returns>
        public override bool IsSupportedAlgorithm(string algorithm)
        {
            if (_securityKey == null)
            {
                ResolveKey();
            }

            return _securityKey.IsSupportedAlgorithm(algorithm);
        }

        /// <summary>
        /// Answers question: is the algorithm Symmetric.
        /// </summary>
        /// <param name="algorithm">Algorithm to check.</param>
        /// <returns>True if algorithm will be processed by runtime as Symmetric.</returns>
        public override bool IsSymmetricAlgorithm(string algorithm)
        {
            // Copied from System.IdentityModel.CryptoHelper
            // no need to ResolveKey.

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

        /// <summary>
        /// Gets the key size in bits.
        /// </summary>
        /// <returns>Key size in bits.</returns>
        public override int KeySize
        {
            get
            {
                if (_securityKey == null)
                {
                    ResolveKey();
                }

                return _securityKey.KeySize;
            }
        }

        /// <summary>
        /// Attempts to resolve the _securityKeyIdentifier into a securityKey.  If successful, the private _securityKey is set.
        /// Uses the tokenresolver that was passed in, it may be the case a keyIdentifier can 
        /// generate a securityKey.  A RSA key can generate a key with just the public part.
        /// </summary>
        /// <returns>void</returns>
        void ResolveKey()
        {

            if (_securityKeyIdentifier == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("ski");
            }

            if (_securityKey == null)
            {
                lock (_keyLock)
                {
                    if (_securityKey == null)
                    {

                        if (_securityTokenResolver != null)
                        {
                            for (int i = 0; i < _securityKeyIdentifier.Count; ++i)
                            {
                                if (_securityTokenResolver.TryResolveSecurityKey(_securityKeyIdentifier[i], out _securityKey))
                                {
                                    return;
                                }
                            }
                        }

                        // most likely a public key, do this last
                        if (_securityKeyIdentifier.CanCreateKey)
                        {
                            _securityKey = _securityKeyIdentifier.CreateKey();
                            return;
                        }

                        throw DiagnosticUtility.ExceptionUtility.ThrowHelper(
                            new SecurityTokenException(SR.GetString(SR.ID2080,
                                        _securityTokenResolver == null ? "null" : _securityTokenResolver.ToString(),
                                        _securityKeyIdentifier == null ? "null" : _securityKeyIdentifier.ToString())), System.Diagnostics.TraceEventType.Error);
                    }
                }
            }
        }
    }
}
