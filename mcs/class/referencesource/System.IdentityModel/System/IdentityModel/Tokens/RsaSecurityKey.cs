//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.IdentityModel.Tokens
{
    using System.IdentityModel.Selectors;
    using System.Security.Cryptography;
    using System.Security.Cryptography.Xml;

    sealed public class RsaSecurityKey : AsymmetricSecurityKey
    {
        PrivateKeyStatus privateKeyStatus = PrivateKeyStatus.AvailabilityNotDetermined;
        readonly RSA rsa;

        public RsaSecurityKey(RSA rsa)
        {
            if (rsa == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("rsa");

            this.rsa = rsa;
        }

        public override int KeySize
        {
            get { return this.rsa.KeySize; }
        }

        public override byte[] DecryptKey(string algorithm, byte[] keyData)
        {
            switch (algorithm)
            {
                case SecurityAlgorithms.RsaV15KeyWrap:
                    return EncryptedXml.DecryptKey(keyData, rsa, false);
                case SecurityAlgorithms.RsaOaepKeyWrap:
                    return EncryptedXml.DecryptKey(keyData, rsa, true);
                default:
                    if (IsSupportedAlgorithm(algorithm))
                        return EncryptedXml.DecryptKey(keyData, rsa, false);

                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new CryptographicException(SR.GetString(SR.UnsupportedAlgorithmForCryptoOperation,
                 algorithm, "DecryptKey")));
            }
        }

        public override byte[] EncryptKey(string algorithm, byte[] keyData)
        {
            switch (algorithm)
            {
                case SecurityAlgorithms.RsaV15KeyWrap:
                    return EncryptedXml.EncryptKey(keyData, rsa, false);
                case SecurityAlgorithms.RsaOaepKeyWrap:
                    return EncryptedXml.EncryptKey(keyData, rsa, true);
                default:
                    if (IsSupportedAlgorithm(algorithm))
                        return EncryptedXml.EncryptKey(keyData, rsa, false);

                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new CryptographicException(SR.GetString(SR.UnsupportedAlgorithmForCryptoOperation,
                        algorithm, "EncryptKey")));
            }
        }

        public override AsymmetricAlgorithm GetAsymmetricAlgorithm(string algorithm, bool requiresPrivateKey)
        {
            if (requiresPrivateKey && !HasPrivateKey())
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new CryptographicException(SR.GetString(SR.NoPrivateKeyAvailable)));
            }

            return this.rsa;
        }

        public override HashAlgorithm GetHashAlgorithmForSignature(string algorithm)
        {
            if (string.IsNullOrEmpty(algorithm))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(algorithm, SR.GetString(SR.EmptyOrNullArgumentString, "algorithm"));
            }

            object algorithmObject = CryptoHelper.GetAlgorithmFromConfig(algorithm);

            if (algorithmObject != null)
            {
                SignatureDescription description = algorithmObject as SignatureDescription;
                if (description != null)
                    return description.CreateDigest();

                HashAlgorithm hashAlgorithm = algorithmObject as HashAlgorithm;
                if (hashAlgorithm != null)
                    return hashAlgorithm;

                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new CryptographicException(SR.GetString(SR.UnsupportedCryptoAlgorithm,
                        algorithm)));
            }

            switch (algorithm)
            {
                case SecurityAlgorithms.RsaSha1Signature:
                    return CryptoHelper.NewSha1HashAlgorithm();
                case SecurityAlgorithms.RsaSha256Signature:
                    return CryptoHelper.NewSha256HashAlgorithm();
                default:
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new CryptographicException(SR.GetString(SR.UnsupportedAlgorithmForCryptoOperation,
                        algorithm, "GetHashAlgorithmForSignature")));
            }
        }

        public override AsymmetricSignatureDeformatter GetSignatureDeformatter(string algorithm)
        {
            if (string.IsNullOrEmpty(algorithm))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(algorithm, SR.GetString(SR.EmptyOrNullArgumentString, "algorithm"));
            }

            object algorithmObject = CryptoHelper.GetAlgorithmFromConfig(algorithm);
            if (algorithmObject != null)
            {
                SignatureDescription description = algorithmObject as SignatureDescription;
                if (description != null)
                    return description.CreateDeformatter(this.rsa);

                try
                {
                    AsymmetricSignatureDeformatter asymmetricSignatureDeformatter = algorithmObject as AsymmetricSignatureDeformatter;
                    if (asymmetricSignatureDeformatter != null)
                    {
                        asymmetricSignatureDeformatter.SetKey(this.rsa);
                        return asymmetricSignatureDeformatter;
                    }
                }
                catch (InvalidCastException e)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(SR.GetString(SR.AlgorithmAndKeyMisMatch, algorithm), e));
                }

                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new CryptographicException(SR.GetString(SR.UnsupportedAlgorithmForCryptoOperation,
                       algorithm, "GetSignatureDeformatter")));
            }

            switch (algorithm)
            {
                case SecurityAlgorithms.RsaSha1Signature:
                case SecurityAlgorithms.RsaSha256Signature:
                    return new RSAPKCS1SignatureDeformatter(rsa);
                default:
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new CryptographicException(SR.GetString(SR.UnsupportedAlgorithmForCryptoOperation,
                        algorithm, "GetSignatureDeformatter")));
            }
        }

        public override AsymmetricSignatureFormatter GetSignatureFormatter(string algorithm)
        {
            if (string.IsNullOrEmpty(algorithm))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(algorithm, SR.GetString(SR.EmptyOrNullArgumentString, "algorithm"));
            }

            object algorithmObject = CryptoHelper.GetAlgorithmFromConfig(algorithm);
            if (algorithmObject != null)
            {
                SignatureDescription description = algorithmObject as SignatureDescription;
                if (description != null)
                    return description.CreateFormatter(rsa);

                try
                {
                    AsymmetricSignatureFormatter asymmetricSignatureFormatter = algorithmObject as AsymmetricSignatureFormatter;

                    if (asymmetricSignatureFormatter != null)
                    {
                        asymmetricSignatureFormatter.SetKey(rsa);
                        return asymmetricSignatureFormatter;
                    }
                }
                catch (InvalidCastException e)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(SR.GetString(SR.AlgorithmAndKeyMisMatch, algorithm), e));
                }

                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new CryptographicException(SR.GetString(SR.UnsupportedAlgorithmForCryptoOperation,
                       algorithm, "GetSignatureFormatter")));
            }

            switch (algorithm)
            {
                case SecurityAlgorithms.RsaSha1Signature:
                case SecurityAlgorithms.RsaSha256Signature:
                    // Ensure that we have an RSA algorithm object.
                    return new RSAPKCS1SignatureFormatter(this.rsa);
                default:
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new CryptographicException(SR.GetString(SR.UnsupportedAlgorithmForCryptoOperation,
                        algorithm, "GetSignatureFormatter")));
            }
        }

        public override bool HasPrivateKey()
        {
            if (this.privateKeyStatus == PrivateKeyStatus.AvailabilityNotDetermined)
            {
                RSACryptoServiceProvider rsaCryptoServiceProvider = this.rsa as RSACryptoServiceProvider;
                if (rsaCryptoServiceProvider != null)
                {
                    this.privateKeyStatus = rsaCryptoServiceProvider.PublicOnly ? PrivateKeyStatus.DoesNotHavePrivateKey : PrivateKeyStatus.HasPrivateKey;
                }
                else
                {
                    try
                    {
                        byte[] hash = new byte[20];
                        this.rsa.DecryptValue(hash); // imitate signing
                        this.privateKeyStatus = PrivateKeyStatus.HasPrivateKey;
                    }
                    catch (CryptographicException)
                    {
                        this.privateKeyStatus = PrivateKeyStatus.DoesNotHavePrivateKey;
                    }
                }
            }
            return this.privateKeyStatus == PrivateKeyStatus.HasPrivateKey;
        }

        public override bool IsAsymmetricAlgorithm(string algorithm)
        {
            if (string.IsNullOrEmpty(algorithm))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(algorithm, SR.GetString(SR.EmptyOrNullArgumentString, "algorithm"));
            }

            return CryptoHelper.IsAsymmetricAlgorithm(algorithm);
        }

        public override bool IsSupportedAlgorithm(string algorithm)
        {
            if (string.IsNullOrEmpty(algorithm))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(algorithm, SR.GetString(SR.EmptyOrNullArgumentString, "algorithm"));
            }

            object algorithmObject = null;

            try
            {
                algorithmObject = CryptoHelper.GetAlgorithmFromConfig(algorithm);
            }
            catch (InvalidOperationException)
            {
                algorithmObject = null;
            }

            if (algorithmObject != null)
            {
                SignatureDescription signatureDescription = algorithmObject as SignatureDescription;
                if (signatureDescription != null)
                    return true;
                AsymmetricAlgorithm asymmetricAlgorithm = algorithmObject as AsymmetricAlgorithm;
                if (asymmetricAlgorithm != null)
                    return true;
                return false;
            }

            switch (algorithm)
            {
                case SecurityAlgorithms.RsaV15KeyWrap:
                case SecurityAlgorithms.RsaOaepKeyWrap:
                case SecurityAlgorithms.RsaSha1Signature:
                case SecurityAlgorithms.RsaSha256Signature:
                    return true;
                default:
                    return false;
            }
        }

        public override bool IsSymmetricAlgorithm(string algorithm)
        {
            return CryptoHelper.IsSymmetricAlgorithm(algorithm);
        }

        enum PrivateKeyStatus
        {
            AvailabilityNotDetermined,
            HasPrivateKey,
            DoesNotHavePrivateKey
        }
    }
}
