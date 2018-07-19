//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.Security
{
    using System.Collections.Generic;
    using System.ServiceModel.Channels;
    using System.ServiceModel;
    using System.Collections.ObjectModel;
    using System.IdentityModel.Claims;
    using System.IdentityModel.Policy;
    using System.IdentityModel.Tokens;
    using System.IdentityModel.Selectors;
    using System.Security.Cryptography;
    using System.ServiceModel.Security.Tokens;
    
    using System.Text;
    using System.Runtime.Serialization;
    using System.Xml;
    using System.ComponentModel;

    [TypeConverter(typeof(System.ServiceModel.Configuration.SecurityAlgorithmSuiteConverter))]
    public abstract class SecurityAlgorithmSuite
    {
        static SecurityAlgorithmSuite basic256;
        static SecurityAlgorithmSuite basic192;
        static SecurityAlgorithmSuite basic128;
        static SecurityAlgorithmSuite tripleDes;
        static SecurityAlgorithmSuite basic256Rsa15;
        static SecurityAlgorithmSuite basic192Rsa15;
        static SecurityAlgorithmSuite basic128Rsa15;
        static SecurityAlgorithmSuite tripleDesRsa15;
        static SecurityAlgorithmSuite basic256Sha256;
        static SecurityAlgorithmSuite basic192Sha256;
        static SecurityAlgorithmSuite basic128Sha256;
        static SecurityAlgorithmSuite tripleDesSha256;
        static SecurityAlgorithmSuite basic256Sha256Rsa15;
        static SecurityAlgorithmSuite basic192Sha256Rsa15;
        static SecurityAlgorithmSuite basic128Sha256Rsa15;
        static SecurityAlgorithmSuite tripleDesSha256Rsa15;

        static internal SecurityAlgorithmSuite KerberosDefault
        {
            get
            {
                return Basic128;
            }
        }
        static public SecurityAlgorithmSuite Default
        {
            get
            {
                return Basic256;
            }
        }

        static public SecurityAlgorithmSuite Basic256
        {
            get
            {
                if (basic256 == null)
                    basic256 = new Basic256SecurityAlgorithmSuite();
                return basic256;
            }
        }
        static public SecurityAlgorithmSuite Basic192
        {
            get
            {
                if (basic192 == null)
                    basic192 = new Basic192SecurityAlgorithmSuite();
                return basic192;
            }
        }
        static public SecurityAlgorithmSuite Basic128
        {
            get
            {
                if (basic128 == null)
                    basic128 = new Basic128SecurityAlgorithmSuite();
                return basic128;
            }
        }
        static public SecurityAlgorithmSuite TripleDes
        {
            get
            {
                if (tripleDes == null)
                    tripleDes = new TripleDesSecurityAlgorithmSuite();
                return tripleDes;
            }
        }
        static public SecurityAlgorithmSuite Basic256Rsa15
        {
            get
            {
                if (basic256Rsa15 == null)
                    basic256Rsa15 = new Basic256Rsa15SecurityAlgorithmSuite();
                return basic256Rsa15;
            }
        }
        static public SecurityAlgorithmSuite Basic192Rsa15
        {
            get
            {
                if (basic192Rsa15 == null)
                    basic192Rsa15 = new Basic192Rsa15SecurityAlgorithmSuite();
                return basic192Rsa15;
            }
        }
        static public SecurityAlgorithmSuite Basic128Rsa15
        {
            get
            {
                if (basic128Rsa15 == null)
                    basic128Rsa15 = new Basic128Rsa15SecurityAlgorithmSuite();
                return basic128Rsa15;
            }
        }
        static public SecurityAlgorithmSuite TripleDesRsa15
        {
            get
            {
                if (tripleDesRsa15 == null)
                    tripleDesRsa15 = new TripleDesRsa15SecurityAlgorithmSuite();
                return tripleDesRsa15;
            }
        }

        static public SecurityAlgorithmSuite Basic256Sha256
        {
            get
            {
                if (basic256Sha256 == null)
                    basic256Sha256 = new Basic256Sha256SecurityAlgorithmSuite();
                return basic256Sha256;
            }
        }
        static public SecurityAlgorithmSuite Basic192Sha256
        {
            get
            {
                if (basic192Sha256 == null)
                    basic192Sha256 = new Basic192Sha256SecurityAlgorithmSuite();
                return basic192Sha256;
            }
        }
        static public SecurityAlgorithmSuite Basic128Sha256
        {
            get
            {
                if (basic128Sha256 == null)
                    basic128Sha256 = new Basic128Sha256SecurityAlgorithmSuite();
                return basic128Sha256;
            }
        }
        static public SecurityAlgorithmSuite TripleDesSha256
        {
            get
            {
                if (tripleDesSha256 == null)
                    tripleDesSha256 = new TripleDesSha256SecurityAlgorithmSuite();
                return tripleDesSha256;
            }
        }
        static public SecurityAlgorithmSuite Basic256Sha256Rsa15
        {
            get
            {
                if (basic256Sha256Rsa15 == null)
                    basic256Sha256Rsa15 = new Basic256Sha256Rsa15SecurityAlgorithmSuite();
                return basic256Sha256Rsa15;
            }
        }
        static public SecurityAlgorithmSuite Basic192Sha256Rsa15
        {
            get
            {
                if (basic192Sha256Rsa15 == null)
                    basic192Sha256Rsa15 = new Basic192Sha256Rsa15SecurityAlgorithmSuite();
                return basic192Sha256Rsa15;
            }
        }
        static public SecurityAlgorithmSuite Basic128Sha256Rsa15
        {
            get
            {
                if (basic128Sha256Rsa15 == null)
                    basic128Sha256Rsa15 = new Basic128Sha256Rsa15SecurityAlgorithmSuite();
                return basic128Sha256Rsa15;
            }
        }
        static public SecurityAlgorithmSuite TripleDesSha256Rsa15
        {
            get
            {
                if (tripleDesSha256Rsa15 == null)
                    tripleDesSha256Rsa15 = new TripleDesSha256Rsa15SecurityAlgorithmSuite();
                return tripleDesSha256Rsa15;
            }
        }
       
        public abstract string DefaultCanonicalizationAlgorithm { get; }
        public abstract string DefaultDigestAlgorithm { get; }
        public abstract string DefaultEncryptionAlgorithm { get; }
        public abstract int DefaultEncryptionKeyDerivationLength { get; }
        public abstract string DefaultSymmetricKeyWrapAlgorithm { get; }
        public abstract string DefaultAsymmetricKeyWrapAlgorithm { get; }
        public abstract string DefaultSymmetricSignatureAlgorithm { get; }
        public abstract string DefaultAsymmetricSignatureAlgorithm { get; }
        public abstract int DefaultSignatureKeyDerivationLength { get; }
        public abstract int DefaultSymmetricKeyLength { get; }

        internal virtual XmlDictionaryString DefaultCanonicalizationAlgorithmDictionaryString { get { return null; } }
        internal virtual XmlDictionaryString DefaultDigestAlgorithmDictionaryString { get { return null; } }
        internal virtual XmlDictionaryString DefaultEncryptionAlgorithmDictionaryString { get { return null; } }
        internal virtual XmlDictionaryString DefaultSymmetricKeyWrapAlgorithmDictionaryString { get { return null; } }
        internal virtual XmlDictionaryString DefaultAsymmetricKeyWrapAlgorithmDictionaryString { get { return null; } }
        internal virtual XmlDictionaryString DefaultSymmetricSignatureAlgorithmDictionaryString { get { return null; } }
        internal virtual XmlDictionaryString DefaultAsymmetricSignatureAlgorithmDictionaryString { get { return null; } }

        protected SecurityAlgorithmSuite() { }

        public virtual bool IsCanonicalizationAlgorithmSupported(string algorithm) { return algorithm == DefaultCanonicalizationAlgorithm; }
        public virtual bool IsDigestAlgorithmSupported(string algorithm) { return algorithm == DefaultDigestAlgorithm; }
        public virtual bool IsEncryptionAlgorithmSupported(string algorithm) { return algorithm == DefaultEncryptionAlgorithm; }
        public virtual bool IsEncryptionKeyDerivationAlgorithmSupported(string algorithm) { return (algorithm == SecurityAlgorithms.Psha1KeyDerivation) || (algorithm == SecurityAlgorithms.Psha1KeyDerivationDec2005); }
        public virtual bool IsSymmetricKeyWrapAlgorithmSupported(string algorithm) { return algorithm == DefaultSymmetricKeyWrapAlgorithm; }
        public virtual bool IsAsymmetricKeyWrapAlgorithmSupported(string algorithm) { return algorithm == DefaultAsymmetricKeyWrapAlgorithm; }
        public virtual bool IsSymmetricSignatureAlgorithmSupported(string algorithm) { return algorithm == DefaultSymmetricSignatureAlgorithm; }
        public virtual bool IsAsymmetricSignatureAlgorithmSupported(string algorithm) { return algorithm == DefaultAsymmetricSignatureAlgorithm; }
        public virtual bool IsSignatureKeyDerivationAlgorithmSupported(string algorithm) { return (algorithm == SecurityAlgorithms.Psha1KeyDerivation) || (algorithm == SecurityAlgorithms.Psha1KeyDerivationDec2005); }
        public abstract bool IsSymmetricKeyLengthSupported(int length);
        public abstract bool IsAsymmetricKeyLengthSupported(int length);

        internal static bool IsRsaSHA256(SecurityAlgorithmSuite suite)
        {
            if ( suite == null )
                return false;

            return (suite == Basic128Sha256 || suite == Basic128Sha256Rsa15 || suite == Basic192Sha256 || suite == Basic192Sha256Rsa15 ||
                suite == Basic256Sha256 || suite == Basic256Sha256Rsa15 || suite == TripleDesSha256 || suite == TripleDesSha256Rsa15);
         
        }

        internal string GetEncryptionKeyDerivationAlgorithm(SecurityToken token, SecureConversationVersion version)
        {
            if (token == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("token");

            string derivationAlgorithm = SecurityUtils.GetKeyDerivationAlgorithm(version);
            if (SecurityUtils.IsSupportedAlgorithm(derivationAlgorithm, token))
                return derivationAlgorithm;
            else
                return null;
        }

        internal int GetEncryptionKeyDerivationLength(SecurityToken token, SecureConversationVersion version)
        {
            if (token == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("token");

            string derivationAlgorithm = SecurityUtils.GetKeyDerivationAlgorithm(version);
            if (SecurityUtils.IsSupportedAlgorithm(derivationAlgorithm, token))
            {
                if (this.DefaultEncryptionKeyDerivationLength % 8 != 0)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(SR.GetString(SR.Psha1KeyLengthInvalid, this.DefaultEncryptionKeyDerivationLength)));

                return this.DefaultEncryptionKeyDerivationLength / 8;
            }
            else
                return 0;
        }

        internal void GetKeyWrapAlgorithm(SecurityToken token, out string keyWrapAlgorithm, out XmlDictionaryString keyWrapAlgorithmDictionaryString)
        {
            if (token == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("token");

            if (SecurityUtils.IsSupportedAlgorithm(this.DefaultSymmetricKeyWrapAlgorithm, token))
            {
                keyWrapAlgorithm = this.DefaultSymmetricKeyWrapAlgorithm;
                keyWrapAlgorithmDictionaryString = this.DefaultSymmetricKeyWrapAlgorithmDictionaryString;
            }
            else
            {
                keyWrapAlgorithm = this.DefaultAsymmetricKeyWrapAlgorithm;
                keyWrapAlgorithmDictionaryString = this.DefaultAsymmetricKeyWrapAlgorithmDictionaryString;
            }
        }

        internal void GetSignatureAlgorithmAndKey(SecurityToken token, out string signatureAlgorithm, out SecurityKey key, out XmlDictionaryString signatureAlgorithmDictionaryString)
        {
            ReadOnlyCollection<SecurityKey> keys = token.SecurityKeys;
            if (keys == null || keys.Count == 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SigningTokenHasNoKeys, token)));
            }

            for (int i = 0; i < keys.Count; i++)
            {
                if (keys[i].IsSupportedAlgorithm(this.DefaultSymmetricSignatureAlgorithm))
                {
                    signatureAlgorithm = this.DefaultSymmetricSignatureAlgorithm;
                    signatureAlgorithmDictionaryString = this.DefaultSymmetricSignatureAlgorithmDictionaryString;
                    key = keys[i];
                    return;
                }
                else if (keys[i].IsSupportedAlgorithm(this.DefaultAsymmetricSignatureAlgorithm))
                {
                    signatureAlgorithm = this.DefaultAsymmetricSignatureAlgorithm;
                    signatureAlgorithmDictionaryString = this.DefaultAsymmetricSignatureAlgorithmDictionaryString;
                    key = keys[i];
                    return;
                }
            }

            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SigningTokenHasNoKeysSupportingTheAlgorithmSuite, token, this)));
        }

        internal string GetSignatureKeyDerivationAlgorithm(SecurityToken token, SecureConversationVersion version)
        {
            if (token == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("token");

            string derivationAlgorithm = SecurityUtils.GetKeyDerivationAlgorithm(version);
            if (SecurityUtils.IsSupportedAlgorithm(derivationAlgorithm, token))
                return derivationAlgorithm;
            else
                return null;
        }

        internal int GetSignatureKeyDerivationLength(SecurityToken token, SecureConversationVersion version)
        {
            if (token == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("token");

            string derivationAlgorithm = SecurityUtils.GetKeyDerivationAlgorithm(version);
            if (SecurityUtils.IsSupportedAlgorithm(derivationAlgorithm, token))
            {
                if (this.DefaultSignatureKeyDerivationLength % 8 != 0)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(SR.GetString(SR.Psha1KeyLengthInvalid, this.DefaultSignatureKeyDerivationLength)));

                return this.DefaultSignatureKeyDerivationLength / 8;
            }
            else
                return 0;
        }

        internal void EnsureAcceptableSymmetricSignatureAlgorithm(string algorithm)
        {
            if (!IsSymmetricSignatureAlgorithmSupported(algorithm))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageSecurityException(SR.GetString(SR.SuiteDoesNotAcceptAlgorithm,
                    algorithm, "SymmetricSignature", this)));
            }
        }

        internal void EnsureAcceptableSignatureKeySize(SecurityKey securityKey, SecurityToken token)
        {
            AsymmetricSecurityKey asymmetricSecurityKey = securityKey as AsymmetricSecurityKey;
            if (asymmetricSecurityKey != null)
            {
                if (!IsAsymmetricKeyLengthSupported(asymmetricSecurityKey.KeySize))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageSecurityException(
                        SR.GetString(SR.TokenDoesNotMeetKeySizeRequirements, this, token, asymmetricSecurityKey.KeySize)));
                }
            }
            else
            {
                SymmetricSecurityKey symmetricSecurityKey = securityKey as SymmetricSecurityKey;
                if (symmetricSecurityKey == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.UnknownICryptoType, symmetricSecurityKey)));
                }
                EnsureAcceptableSignatureSymmetricKeySize(symmetricSecurityKey, token);
            }
        }

        // Ensure acceptable signing symmetric key.
        // 1) if derived key, validate derived key against DefaultSignatureKeyDerivationLength and validate
        //    source key against DefaultSymmetricKeyLength
        // 2) if not derived key, validate key against DefaultSymmetricKeyLength
        internal void EnsureAcceptableSignatureSymmetricKeySize(SymmetricSecurityKey securityKey, SecurityToken token)
        {
            int keySize;
            DerivedKeySecurityToken dkt = token as DerivedKeySecurityToken;
            if (dkt != null)
            {
                token = dkt.TokenToDerive;
                keySize = ((SymmetricSecurityKey)token.SecurityKeys[0]).KeySize;

                // doing special case for derived key token signing length since
                // the sending side doesn't honor the algorithm suite. It used the DefaultSignatureKeyDerivationLength instead
                if (dkt.SecurityKeys[0].KeySize < this.DefaultSignatureKeyDerivationLength)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageSecurityException(
                        SR.GetString(SR.TokenDoesNotMeetKeySizeRequirements, this, dkt, dkt.SecurityKeys[0].KeySize)));
                }
            }
            else
            {
                keySize = securityKey.KeySize;
            }

            if (!IsSymmetricKeyLengthSupported(keySize))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageSecurityException(
                    SR.GetString(SR.TokenDoesNotMeetKeySizeRequirements, this, token, keySize)));
            }
        }

        // Ensure acceptable decrypting symmetric key.
        // 1) if derived key, validate derived key against DefaultEncryptionKeyDerivationLength and validate
        //    source key against DefaultSymmetricKeyLength
        // 2) if not derived key, validate key against DefaultSymmetricKeyLength
        internal void EnsureAcceptableDecryptionSymmetricKeySize(SymmetricSecurityKey securityKey, SecurityToken token)
        {
            int keySize;
            DerivedKeySecurityToken dkt = token as DerivedKeySecurityToken;
            if (dkt != null)
            {
                token = dkt.TokenToDerive;
                keySize = ((SymmetricSecurityKey)token.SecurityKeys[0]).KeySize;

                // doing special case for derived key token signing length since
                // the sending side doesn't honor the algorithm suite. It used the DefaultSignatureKeyDerivationLength instead
                if (dkt.SecurityKeys[0].KeySize < this.DefaultEncryptionKeyDerivationLength)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageSecurityException(
                        SR.GetString(SR.TokenDoesNotMeetKeySizeRequirements, this, dkt, dkt.SecurityKeys[0].KeySize)));
                }
            }
            else
            {
                keySize = securityKey.KeySize;
            }

            if (!IsSymmetricKeyLengthSupported(keySize))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageSecurityException(
                    SR.GetString(SR.TokenDoesNotMeetKeySizeRequirements, this, token, keySize)));
            }
        }

        internal void EnsureAcceptableSignatureAlgorithm(SecurityKey verificationKey, string algorithm)
        {
            InMemorySymmetricSecurityKey symmeticKey = verificationKey as InMemorySymmetricSecurityKey;
            if (symmeticKey != null)
            {
                this.EnsureAcceptableSymmetricSignatureAlgorithm(algorithm);
            }
            else
            {
                AsymmetricSecurityKey asymmetricKey = verificationKey as AsymmetricSecurityKey;
                if (asymmetricKey == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.UnknownICryptoType, verificationKey)));
                }

                this.EnsureAcceptableAsymmetricSignatureAlgorithm(algorithm);
            }
        }

        internal void EnsureAcceptableAsymmetricSignatureAlgorithm(string algorithm)
        {
            if (!IsAsymmetricSignatureAlgorithmSupported(algorithm))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageSecurityException(SR.GetString(SR.SuiteDoesNotAcceptAlgorithm,
                    algorithm, "AsymmetricSignature", this)));
            }
        }

        internal void EnsureAcceptableKeyWrapAlgorithm(string algorithm, bool isAsymmetric)
        {
            if (isAsymmetric)
            {
                if (!IsAsymmetricKeyWrapAlgorithmSupported(algorithm))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageSecurityException(SR.GetString(SR.SuiteDoesNotAcceptAlgorithm,
                        algorithm, "AsymmetricKeyWrap", this)));
                }
            }
            else
            {
                if (!IsSymmetricKeyWrapAlgorithmSupported(algorithm))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageSecurityException(SR.GetString(SR.SuiteDoesNotAcceptAlgorithm,
                        algorithm, "SymmetricKeyWrap", this)));
                }
            }
        }

        internal void EnsureAcceptableEncryptionAlgorithm(string algorithm)
        {
            if (!IsEncryptionAlgorithmSupported(algorithm))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageSecurityException(SR.GetString(SR.SuiteDoesNotAcceptAlgorithm,
                    algorithm, "Encryption", this)));
            }
        }

        internal void EnsureAcceptableSignatureKeyDerivationAlgorithm(string algorithm)
        {
            if (!IsSignatureKeyDerivationAlgorithmSupported(algorithm))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageSecurityException(SR.GetString(SR.SuiteDoesNotAcceptAlgorithm,
                    algorithm, "SignatureKeyDerivation", this)));
            }
        }

        internal void EnsureAcceptableEncryptionKeyDerivationAlgorithm(string algorithm)
        {
            if (!IsEncryptionKeyDerivationAlgorithmSupported(algorithm))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageSecurityException(SR.GetString(SR.SuiteDoesNotAcceptAlgorithm,
                    algorithm, "EncryptionKeyDerivation", this)));
            }
        }

        internal void EnsureAcceptableDigestAlgorithm(string algorithm)
        {
            if (!IsDigestAlgorithmSupported(algorithm))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageSecurityException(SR.GetString(SR.SuiteDoesNotAcceptAlgorithm,
                    algorithm, "Digest", this)));
            }
        }

    }

    public class Basic256SecurityAlgorithmSuite : SecurityAlgorithmSuite
    {
        public Basic256SecurityAlgorithmSuite() : base() { }

        public override string DefaultCanonicalizationAlgorithm { get { return DefaultCanonicalizationAlgorithmDictionaryString.Value; } }
        public override string DefaultDigestAlgorithm { get { return DefaultDigestAlgorithmDictionaryString.Value; } }
        public override string DefaultEncryptionAlgorithm { get { return DefaultEncryptionAlgorithmDictionaryString.Value; } }
        public override int DefaultEncryptionKeyDerivationLength { get { return 256; } }
        public override string DefaultSymmetricKeyWrapAlgorithm { get { return DefaultSymmetricKeyWrapAlgorithmDictionaryString.Value; } }
        public override string DefaultAsymmetricKeyWrapAlgorithm { get { return DefaultAsymmetricKeyWrapAlgorithmDictionaryString.Value; } }
        public override string DefaultSymmetricSignatureAlgorithm { get { return DefaultSymmetricSignatureAlgorithmDictionaryString.Value; } }
        public override string DefaultAsymmetricSignatureAlgorithm { get { return DefaultAsymmetricSignatureAlgorithmDictionaryString.Value; } }
        public override int DefaultSignatureKeyDerivationLength { get { return 192; } }
        public override int DefaultSymmetricKeyLength { get { return 256; } }
        public override bool IsSymmetricKeyLengthSupported(int length) { return length == 256; }
        public override bool IsAsymmetricKeyLengthSupported(int length) { return length >= 1024 && length <= 4096; }

        internal override XmlDictionaryString DefaultCanonicalizationAlgorithmDictionaryString { get { return XD.SecurityAlgorithmDictionary.ExclusiveC14n; } }
        internal override XmlDictionaryString DefaultDigestAlgorithmDictionaryString { get { return XD.SecurityAlgorithmDictionary.Sha1Digest; } }
        internal override XmlDictionaryString DefaultEncryptionAlgorithmDictionaryString { get { return XD.SecurityAlgorithmDictionary.Aes256Encryption; } }
        internal override XmlDictionaryString DefaultSymmetricKeyWrapAlgorithmDictionaryString { get { return XD.SecurityAlgorithmDictionary.Aes256KeyWrap; } }
        internal override XmlDictionaryString DefaultAsymmetricKeyWrapAlgorithmDictionaryString { get { return XD.SecurityAlgorithmDictionary.RsaOaepKeyWrap; } }
        internal override XmlDictionaryString DefaultSymmetricSignatureAlgorithmDictionaryString { get { return XD.SecurityAlgorithmDictionary.HmacSha1Signature; } }
        internal override XmlDictionaryString DefaultAsymmetricSignatureAlgorithmDictionaryString { get { return XD.SecurityAlgorithmDictionary.RsaSha1Signature; } }

        public override string ToString()
        {
            return "Basic256";
        }
    }

    public class Basic192SecurityAlgorithmSuite : SecurityAlgorithmSuite
    {
        public Basic192SecurityAlgorithmSuite() : base() { }

        public override string DefaultCanonicalizationAlgorithm { get { return DefaultCanonicalizationAlgorithmDictionaryString.Value; } }
        public override string DefaultDigestAlgorithm { get { return DefaultDigestAlgorithmDictionaryString.Value; } }
        public override string DefaultEncryptionAlgorithm { get { return DefaultEncryptionAlgorithmDictionaryString.Value; } }
        public override int DefaultEncryptionKeyDerivationLength { get { return 192; } }
        public override string DefaultSymmetricKeyWrapAlgorithm { get { return DefaultSymmetricKeyWrapAlgorithmDictionaryString.Value; } }
        public override string DefaultAsymmetricKeyWrapAlgorithm { get { return DefaultAsymmetricKeyWrapAlgorithmDictionaryString.Value; } }
        public override string DefaultSymmetricSignatureAlgorithm { get { return DefaultSymmetricSignatureAlgorithmDictionaryString.Value; } }
        public override string DefaultAsymmetricSignatureAlgorithm { get { return DefaultAsymmetricSignatureAlgorithmDictionaryString.Value; } }
        public override int DefaultSignatureKeyDerivationLength { get { return 192; } }
        public override int DefaultSymmetricKeyLength { get { return 192; } }
        public override bool IsSymmetricKeyLengthSupported(int length) { return length >= 192 && length <= 256; }
        public override bool IsAsymmetricKeyLengthSupported(int length) { return length >= 1024 && length <= 4096; }

        internal override XmlDictionaryString DefaultCanonicalizationAlgorithmDictionaryString { get { return XD.SecurityAlgorithmDictionary.ExclusiveC14n; } }
        internal override XmlDictionaryString DefaultDigestAlgorithmDictionaryString { get { return XD.SecurityAlgorithmDictionary.Sha1Digest; } }
        internal override XmlDictionaryString DefaultEncryptionAlgorithmDictionaryString { get { return XD.SecurityAlgorithmDictionary.Aes192Encryption; } }
        internal override XmlDictionaryString DefaultSymmetricKeyWrapAlgorithmDictionaryString { get { return XD.SecurityAlgorithmDictionary.Aes192KeyWrap; } }
        internal override XmlDictionaryString DefaultAsymmetricKeyWrapAlgorithmDictionaryString { get { return XD.SecurityAlgorithmDictionary.RsaOaepKeyWrap; } }
        internal override XmlDictionaryString DefaultSymmetricSignatureAlgorithmDictionaryString { get { return XD.SecurityAlgorithmDictionary.HmacSha1Signature; } }
        internal override XmlDictionaryString DefaultAsymmetricSignatureAlgorithmDictionaryString { get { return XD.SecurityAlgorithmDictionary.RsaSha1Signature; } }

        public override string ToString()
        {
            return "Basic192";
        }
    }

    public class Basic128SecurityAlgorithmSuite : SecurityAlgorithmSuite
    {
        public Basic128SecurityAlgorithmSuite() : base() { }

        public override string DefaultCanonicalizationAlgorithm { get { return this.DefaultCanonicalizationAlgorithmDictionaryString.Value; } }
        public override string DefaultDigestAlgorithm { get { return this.DefaultDigestAlgorithmDictionaryString.Value; } }
        public override string DefaultEncryptionAlgorithm { get { return this.DefaultEncryptionAlgorithmDictionaryString.Value; } }
        public override int DefaultEncryptionKeyDerivationLength { get { return 128; } }
        public override string DefaultSymmetricKeyWrapAlgorithm { get { return this.DefaultSymmetricKeyWrapAlgorithmDictionaryString.Value; } }
        public override string DefaultAsymmetricKeyWrapAlgorithm { get { return this.DefaultAsymmetricKeyWrapAlgorithmDictionaryString.Value; } }
        public override string DefaultSymmetricSignatureAlgorithm { get { return this.DefaultSymmetricSignatureAlgorithmDictionaryString.Value; } }
        public override string DefaultAsymmetricSignatureAlgorithm { get { return this.DefaultAsymmetricSignatureAlgorithmDictionaryString.Value; } }
        public override int DefaultSignatureKeyDerivationLength { get { return 128; } }
        public override int DefaultSymmetricKeyLength { get { return 128; } }
        public override bool IsSymmetricKeyLengthSupported(int length) { return length >= 128 && length <= 256; }
        public override bool IsAsymmetricKeyLengthSupported(int length) { return length >= 1024 && length <= 4096; }

        internal override XmlDictionaryString DefaultCanonicalizationAlgorithmDictionaryString { get { return XD.SecurityAlgorithmDictionary.ExclusiveC14n; } }
        internal override XmlDictionaryString DefaultDigestAlgorithmDictionaryString { get { return XD.SecurityAlgorithmDictionary.Sha1Digest; } }
        internal override XmlDictionaryString DefaultEncryptionAlgorithmDictionaryString { get { return XD.SecurityAlgorithmDictionary.Aes128Encryption; } }
        internal override XmlDictionaryString DefaultSymmetricKeyWrapAlgorithmDictionaryString { get { return XD.SecurityAlgorithmDictionary.Aes128KeyWrap; } }
        internal override XmlDictionaryString DefaultAsymmetricKeyWrapAlgorithmDictionaryString { get { return XD.SecurityAlgorithmDictionary.RsaOaepKeyWrap; } }
        internal override XmlDictionaryString DefaultSymmetricSignatureAlgorithmDictionaryString { get { return XD.SecurityAlgorithmDictionary.HmacSha1Signature; } }
        internal override XmlDictionaryString DefaultAsymmetricSignatureAlgorithmDictionaryString { get { return XD.SecurityAlgorithmDictionary.RsaSha1Signature; } }

        public override string ToString() 
        {
            return "Basic128";
        }
    }

    public class TripleDesSecurityAlgorithmSuite : SecurityAlgorithmSuite
    {
        public TripleDesSecurityAlgorithmSuite() : base() { }

        public override string DefaultCanonicalizationAlgorithm { get { return DefaultCanonicalizationAlgorithmDictionaryString.Value; } }
        public override string DefaultDigestAlgorithm { get { return DefaultDigestAlgorithmDictionaryString.Value; } }
        public override string DefaultEncryptionAlgorithm { get { return DefaultEncryptionAlgorithmDictionaryString.Value; } }
        public override int DefaultEncryptionKeyDerivationLength { get { return 192; } }
        public override string DefaultSymmetricKeyWrapAlgorithm { get { return DefaultSymmetricKeyWrapAlgorithmDictionaryString.Value; } }
        public override string DefaultAsymmetricKeyWrapAlgorithm { get { return this.DefaultAsymmetricKeyWrapAlgorithmDictionaryString.Value; } }

        public override string DefaultSymmetricSignatureAlgorithm { get { return DefaultSymmetricSignatureAlgorithmDictionaryString.Value; } }
        public override string DefaultAsymmetricSignatureAlgorithm { get { return DefaultAsymmetricSignatureAlgorithmDictionaryString.Value; } }
        public override int DefaultSignatureKeyDerivationLength { get { return 192; } }
        public override int DefaultSymmetricKeyLength { get { return 192; } }
        public override bool IsSymmetricKeyLengthSupported(int length) { return length >= 192 && length <= 256; }
        public override bool IsAsymmetricKeyLengthSupported(int length) { return length >= 1024 && length <= 4096; }

        internal override XmlDictionaryString DefaultCanonicalizationAlgorithmDictionaryString { get { return XD.SecurityAlgorithmDictionary.ExclusiveC14n; } }
        internal override XmlDictionaryString DefaultDigestAlgorithmDictionaryString { get { return XD.SecurityAlgorithmDictionary.Sha1Digest; } }
        internal override XmlDictionaryString DefaultEncryptionAlgorithmDictionaryString { get { return XD.SecurityAlgorithmDictionary.TripleDesEncryption; } }
        internal override XmlDictionaryString DefaultSymmetricKeyWrapAlgorithmDictionaryString { get { return XD.SecurityAlgorithmDictionary.TripleDesKeyWrap; } }
        internal override XmlDictionaryString DefaultAsymmetricKeyWrapAlgorithmDictionaryString { get { return XD.SecurityAlgorithmDictionary.RsaOaepKeyWrap; } }
        internal override XmlDictionaryString DefaultSymmetricSignatureAlgorithmDictionaryString { get { return XD.SecurityAlgorithmDictionary.HmacSha1Signature; } }
        internal override XmlDictionaryString DefaultAsymmetricSignatureAlgorithmDictionaryString { get { return XD.SecurityAlgorithmDictionary.RsaSha1Signature; } }

        public override string ToString()
        {
            return "TripleDes";
        }
    }

    class Basic128Rsa15SecurityAlgorithmSuite : Basic128SecurityAlgorithmSuite
    {
        public Basic128Rsa15SecurityAlgorithmSuite() : base() { }

        internal override XmlDictionaryString DefaultAsymmetricKeyWrapAlgorithmDictionaryString { get { return XD.SecurityAlgorithmDictionary.RsaV15KeyWrap; } }

        public override string ToString()
        {
            return "Basic128Rsa15";
        }
    }

    class Basic192Rsa15SecurityAlgorithmSuite : Basic192SecurityAlgorithmSuite
    {
        public Basic192Rsa15SecurityAlgorithmSuite() : base() { }

        internal override XmlDictionaryString DefaultAsymmetricKeyWrapAlgorithmDictionaryString { get { return XD.SecurityAlgorithmDictionary.RsaV15KeyWrap; } }

        public override string ToString()
        {
            return "Basic192Rsa15";
        }
    }

    class Basic256Rsa15SecurityAlgorithmSuite : Basic256SecurityAlgorithmSuite
    {
        public Basic256Rsa15SecurityAlgorithmSuite() : base() { }

        internal override XmlDictionaryString DefaultAsymmetricKeyWrapAlgorithmDictionaryString { get { return XD.SecurityAlgorithmDictionary.RsaV15KeyWrap; } }

        public override string ToString()
        {
            return "Basic256Rsa15";
        }
    }

    class TripleDesRsa15SecurityAlgorithmSuite : TripleDesSecurityAlgorithmSuite
    {
        public TripleDesRsa15SecurityAlgorithmSuite() : base() { }

        internal override XmlDictionaryString DefaultAsymmetricKeyWrapAlgorithmDictionaryString { get { return XD.SecurityAlgorithmDictionary.RsaV15KeyWrap; } }

        public override string ToString()
        {
            return "TripleDesRsa15";
        }
    }

    class Basic256Sha256SecurityAlgorithmSuite : Basic256SecurityAlgorithmSuite
    {
        public Basic256Sha256SecurityAlgorithmSuite() : base() { }

        internal override XmlDictionaryString DefaultDigestAlgorithmDictionaryString { get { return XD.SecurityAlgorithmDictionary.Sha256Digest; } }
        internal override XmlDictionaryString DefaultSymmetricSignatureAlgorithmDictionaryString { get { return XD.SecurityAlgorithmDictionary.HmacSha256Signature; } }
        internal override XmlDictionaryString DefaultAsymmetricSignatureAlgorithmDictionaryString { get { return XD.SecurityAlgorithmDictionary.RsaSha256Signature; } }

        public override string ToString()
        {
            return "Basic256Sha256";
        }
    }

    class Basic192Sha256SecurityAlgorithmSuite : Basic192SecurityAlgorithmSuite
    {
        public Basic192Sha256SecurityAlgorithmSuite() : base() { }

        internal override XmlDictionaryString DefaultDigestAlgorithmDictionaryString { get { return XD.SecurityAlgorithmDictionary.Sha256Digest; } }
        internal override XmlDictionaryString DefaultSymmetricSignatureAlgorithmDictionaryString { get { return XD.SecurityAlgorithmDictionary.HmacSha256Signature; } }
        internal override XmlDictionaryString DefaultAsymmetricSignatureAlgorithmDictionaryString { get { return XD.SecurityAlgorithmDictionary.RsaSha256Signature; } }

        public override string ToString()
        {
            return "Basic192Sha256";
        }
    }

    class Basic128Sha256SecurityAlgorithmSuite : Basic128SecurityAlgorithmSuite
    {
        public Basic128Sha256SecurityAlgorithmSuite() : base() { }

        internal override XmlDictionaryString DefaultDigestAlgorithmDictionaryString { get { return XD.SecurityAlgorithmDictionary.Sha256Digest; } }
        internal override XmlDictionaryString DefaultSymmetricSignatureAlgorithmDictionaryString { get { return XD.SecurityAlgorithmDictionary.HmacSha256Signature; } }
        internal override XmlDictionaryString DefaultAsymmetricSignatureAlgorithmDictionaryString { get { return XD.SecurityAlgorithmDictionary.RsaSha256Signature; } }

        public override string ToString()
        {
            return "Basic128Sha256";
        }
    }

    class TripleDesSha256SecurityAlgorithmSuite : TripleDesSecurityAlgorithmSuite
    {
        public TripleDesSha256SecurityAlgorithmSuite() : base() { }

        internal override XmlDictionaryString DefaultDigestAlgorithmDictionaryString { get { return XD.SecurityAlgorithmDictionary.Sha256Digest; } }
        internal override XmlDictionaryString DefaultSymmetricSignatureAlgorithmDictionaryString { get { return XD.SecurityAlgorithmDictionary.HmacSha256Signature; } }
        internal override XmlDictionaryString DefaultAsymmetricSignatureAlgorithmDictionaryString { get { return XD.SecurityAlgorithmDictionary.RsaSha256Signature; } }

        public override string ToString()
        {
            return "TripleDesSha256";
        }
    }

    class Basic256Sha256Rsa15SecurityAlgorithmSuite : Basic256Rsa15SecurityAlgorithmSuite
    {
        public Basic256Sha256Rsa15SecurityAlgorithmSuite() : base() { }

        internal override XmlDictionaryString DefaultDigestAlgorithmDictionaryString { get { return XD.SecurityAlgorithmDictionary.Sha256Digest; } }
        internal override XmlDictionaryString DefaultSymmetricSignatureAlgorithmDictionaryString { get { return XD.SecurityAlgorithmDictionary.HmacSha256Signature; } }
        internal override XmlDictionaryString DefaultAsymmetricSignatureAlgorithmDictionaryString { get { return XD.SecurityAlgorithmDictionary.RsaSha256Signature; } }

        public override string ToString()
        {
            return "Basic256Sha256Rsa15";
        }
    }

    class Basic192Sha256Rsa15SecurityAlgorithmSuite : Basic192Rsa15SecurityAlgorithmSuite
    {
        public Basic192Sha256Rsa15SecurityAlgorithmSuite() : base() { }

        internal override XmlDictionaryString DefaultDigestAlgorithmDictionaryString { get { return XD.SecurityAlgorithmDictionary.Sha256Digest; } }
        internal override XmlDictionaryString DefaultSymmetricSignatureAlgorithmDictionaryString { get { return XD.SecurityAlgorithmDictionary.HmacSha256Signature; } }
        internal override XmlDictionaryString DefaultAsymmetricSignatureAlgorithmDictionaryString { get { return XD.SecurityAlgorithmDictionary.RsaSha256Signature; } }

        public override string ToString()
        {
            return "Basic192Sha256Rsa15";
        }
    }

    class Basic128Sha256Rsa15SecurityAlgorithmSuite : Basic128Rsa15SecurityAlgorithmSuite
    {
        public Basic128Sha256Rsa15SecurityAlgorithmSuite() : base() { }

        internal override XmlDictionaryString DefaultDigestAlgorithmDictionaryString { get { return XD.SecurityAlgorithmDictionary.Sha256Digest; } }
        internal override XmlDictionaryString DefaultSymmetricSignatureAlgorithmDictionaryString { get { return XD.SecurityAlgorithmDictionary.HmacSha256Signature; } }
        internal override XmlDictionaryString DefaultAsymmetricSignatureAlgorithmDictionaryString { get { return XD.SecurityAlgorithmDictionary.RsaSha256Signature; } }

        public override string ToString()
        {
            return "Basic128Sha256Rsa15";
        }
    }

    class TripleDesSha256Rsa15SecurityAlgorithmSuite : TripleDesRsa15SecurityAlgorithmSuite
    {
        public TripleDesSha256Rsa15SecurityAlgorithmSuite() : base() { }

        internal override XmlDictionaryString DefaultDigestAlgorithmDictionaryString { get { return XD.SecurityAlgorithmDictionary.Sha256Digest; } }
        internal override XmlDictionaryString DefaultSymmetricSignatureAlgorithmDictionaryString { get { return XD.SecurityAlgorithmDictionary.HmacSha256Signature; } }
        internal override XmlDictionaryString DefaultAsymmetricSignatureAlgorithmDictionaryString { get { return XD.SecurityAlgorithmDictionary.RsaSha256Signature; } }

        public override string ToString()
        {
            return "TripleDesSha256Rsa15";
        }
    }
}
