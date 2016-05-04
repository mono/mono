
//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.IdentityModel.Tokens
{
    using System.Security.Cryptography;
    using System.Security.Cryptography.X509Certificates;
    using System.Security.Cryptography.Xml;

    public class X509AsymmetricSecurityKey : AsymmetricSecurityKey
    {
        X509Certificate2 certificate;
        AsymmetricAlgorithm privateKey;
        bool privateKeyAvailabilityDetermined;
        PublicKey publicKey;

        object thisLock = new Object();

        public X509AsymmetricSecurityKey(X509Certificate2 certificate)
        {
            if (certificate == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("certificate");

            this.certificate = certificate;
        }

        public override int KeySize
        {
            get { return this.PublicKey.Key.KeySize; }
        }

        AsymmetricAlgorithm PrivateKey
        {
            get
            {
                if (!this.privateKeyAvailabilityDetermined)
                {
                    lock (ThisLock)
                    {
                        if (!this.privateKeyAvailabilityDetermined)
                        {
                            this.privateKey = this.certificate.PrivateKey;
                            this.privateKeyAvailabilityDetermined = true;
                        }
                    }
                }
                return this.privateKey;
            }
        }

        PublicKey PublicKey
        {
            get
            {
                if (this.publicKey == null)
                {
                    lock (ThisLock)
                    {
                        if (this.publicKey == null)
                        {
                            this.publicKey = this.certificate.PublicKey;
                        }
                    }
                }
                return this.publicKey;
            }
        }

        Object ThisLock
        {
            get
            {
                return thisLock;
            }
        }

        public override byte[] DecryptKey(string algorithm, byte[] keyData)
        {
            // We can decrypt key only if we have the private key in the certificate.
            if (this.PrivateKey == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(SR.GetString(SR.MissingPrivateKey)));
            }

            RSA rsa = this.PrivateKey as RSA;
            if (rsa == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(SR.GetString(SR.PrivateKeyNotRSA)));
            }

            // Support exchange keySpec, AT_EXCHANGE ?
            if (rsa.KeyExchangeAlgorithm == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(SR.GetString(SR.PrivateKeyExchangeNotSupported)));
            }

            switch (algorithm)
            {
                case EncryptedXml.XmlEncRSA15Url:
                    return EncryptedXml.DecryptKey(keyData, rsa, false);

                case EncryptedXml.XmlEncRSAOAEPUrl:
                    return EncryptedXml.DecryptKey(keyData, rsa, true);

                default:
                    if (IsSupportedAlgorithm(algorithm))
                        return EncryptedXml.DecryptKey(keyData, rsa, true);

                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(SR.GetString(SR.UnsupportedCryptoAlgorithm, algorithm)));
            }
        }

        public override byte[] EncryptKey(string algorithm, byte[] keyData)
        {
            // Ensure that we have an RSA algorithm object
            RSA rsa = this.PublicKey.Key as RSA;
            if (rsa == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(SR.GetString(SR.PublicKeyNotRSA)));
            }

            switch (algorithm)
            {
                case EncryptedXml.XmlEncRSA15Url:
                    return EncryptedXml.EncryptKey(keyData, rsa, false);

                case EncryptedXml.XmlEncRSAOAEPUrl:
                    return EncryptedXml.EncryptKey(keyData, rsa, true);

                default:
                    if (IsSupportedAlgorithm(algorithm))
                        return EncryptedXml.EncryptKey(keyData, rsa, true);

                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(SR.GetString(SR.UnsupportedCryptoAlgorithm, algorithm)));
            }
        }

        public override AsymmetricAlgorithm GetAsymmetricAlgorithm(string algorithm, bool privateKey)
        {
            if (privateKey)
            {
                if (this.PrivateKey == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(SR.GetString(SR.MissingPrivateKey)));
                }

                if (string.IsNullOrEmpty(algorithm))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(algorithm, SR.GetString(SR.EmptyOrNullArgumentString, "algorithm"));
                }

                switch (algorithm)
                {
                    case SignedXml.XmlDsigDSAUrl:
                        if ((this.PrivateKey as DSA) != null)
                        {
                            return (this.PrivateKey as DSA);
                        }
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(SR.GetString(SR.AlgorithmAndPrivateKeyMisMatch)));

                    case SignedXml.XmlDsigRSASHA1Url:
                    case SecurityAlgorithms.RsaSha256Signature:
                    case EncryptedXml.XmlEncRSA15Url:
                    case EncryptedXml.XmlEncRSAOAEPUrl:
                        if ((this.PrivateKey as RSA) != null)
                        {
                            return (this.PrivateKey as RSA);
                        }
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(SR.GetString(SR.AlgorithmAndPrivateKeyMisMatch)));
                    default:
                        if (IsSupportedAlgorithm(algorithm))
                            return this.PrivateKey;
                        else
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(SR.GetString(SR.UnsupportedCryptoAlgorithm, algorithm)));
                }
            }
            else
            {
                switch (algorithm)
                {
                    case SignedXml.XmlDsigDSAUrl:
                        if ((this.PublicKey.Key as DSA) != null)
                        {
                            return (this.PublicKey.Key as DSA);
                        }
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(SR.GetString(SR.AlgorithmAndPublicKeyMisMatch)));
                    case SignedXml.XmlDsigRSASHA1Url:
                    case SecurityAlgorithms.RsaSha256Signature:
                    case EncryptedXml.XmlEncRSA15Url:
                    case EncryptedXml.XmlEncRSAOAEPUrl:
                        if ((this.PublicKey.Key as RSA) != null)
                        {
                            return (this.PublicKey.Key as RSA);
                        }
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(SR.GetString(SR.AlgorithmAndPublicKeyMisMatch)));
                    default:

                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(SR.GetString(SR.UnsupportedCryptoAlgorithm, algorithm)));
                }
            }
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

                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new CryptographicException(SR.GetString(SR.UnsupportedAlgorithmForCryptoOperation,
                        algorithm, "CreateDigest")));
            }

            switch (algorithm)
            {
                case SignedXml.XmlDsigDSAUrl:
                case SignedXml.XmlDsigRSASHA1Url:
                    return CryptoHelper.NewSha1HashAlgorithm();
                case SecurityAlgorithms.RsaSha256Signature:
                    return CryptoHelper.NewSha256HashAlgorithm();
                default:
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(SR.GetString(SR.UnsupportedCryptoAlgorithm, algorithm)));
            }
        }

        public override AsymmetricSignatureDeformatter GetSignatureDeformatter(string algorithm)
        {

            // We support one of the two algoritms, but not both.
            //     XmlDsigDSAUrl = "http://www.w3.org/2000/09/xmldsig#dsa-sha1";
            //     XmlDsigRSASHA1Url = "http://www.w3.org/2000/09/xmldsig#rsa-sha1";

            if (string.IsNullOrEmpty(algorithm))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(algorithm, SR.GetString(SR.EmptyOrNullArgumentString, "algorithm"));
            }

            object algorithmObject = CryptoHelper.GetAlgorithmFromConfig(algorithm);
            if (algorithmObject != null)
            {
                SignatureDescription description = algorithmObject as SignatureDescription;
                if (description != null)
                    return description.CreateDeformatter(this.PublicKey.Key);

                try
                {
                    AsymmetricSignatureDeformatter asymmetricSignatureDeformatter = algorithmObject as AsymmetricSignatureDeformatter;
                    if (asymmetricSignatureDeformatter != null)
                    {
                        asymmetricSignatureDeformatter.SetKey(this.PublicKey.Key);
                        return asymmetricSignatureDeformatter;
                    }
                }
                catch (InvalidCastException e)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(SR.GetString(SR.AlgorithmAndPublicKeyMisMatch), e));
                }

                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new CryptographicException(SR.GetString(SR.UnsupportedAlgorithmForCryptoOperation,
                       algorithm, "GetSignatureDeformatter")));
            }

            switch (algorithm)
            {
                case SignedXml.XmlDsigDSAUrl:

                    // Ensure that we have a DSA algorithm object.
                    DSA dsa = (this.PublicKey.Key as DSA);
                    if (dsa == null)
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(SR.GetString(SR.PublicKeyNotDSA)));
                    return new DSASignatureDeformatter(dsa);

                case SignedXml.XmlDsigRSASHA1Url:
                case SecurityAlgorithms.RsaSha256Signature:
                    // Ensure that we have an RSA algorithm object.
                    RSA rsa = (this.PublicKey.Key as RSA);
                    if (rsa == null)
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(SR.GetString(SR.PublicKeyNotRSA)));
                    return new RSAPKCS1SignatureDeformatter(rsa);

                default:
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(SR.GetString(SR.UnsupportedCryptoAlgorithm, algorithm)));
            }
        }

        public override AsymmetricSignatureFormatter GetSignatureFormatter(string algorithm)
        {
            // One can sign only if the private key is present.
            if (this.PrivateKey == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(SR.GetString(SR.MissingPrivateKey)));
            }

            if (string.IsNullOrEmpty(algorithm))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(algorithm, SR.GetString(SR.EmptyOrNullArgumentString, "algorithm"));
            }
            // We support one of the two algoritms, but not both.
            //     XmlDsigDSAUrl = "http://www.w3.org/2000/09/xmldsig#dsa-sha1";
            //     XmlDsigRSASHA1Url = "http://www.w3.org/2000/09/xmldsig#rsa-sha1";
            object algorithmObject = CryptoHelper.GetAlgorithmFromConfig(algorithm);
            if (algorithmObject != null)
            {
                SignatureDescription description = algorithmObject as SignatureDescription;
                if (description != null)
                    return description.CreateFormatter(this.PrivateKey);

                try
                {
                    AsymmetricSignatureFormatter asymmetricSignatureFormatter = algorithmObject as AsymmetricSignatureFormatter;
                    if (asymmetricSignatureFormatter != null)
                    {
                        asymmetricSignatureFormatter.SetKey(this.PrivateKey);
                        return asymmetricSignatureFormatter;
                    }
                }
                catch (InvalidCastException e)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(SR.GetString(SR.AlgorithmAndPrivateKeyMisMatch), e));
                }

                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new CryptographicException(SR.GetString(SR.UnsupportedAlgorithmForCryptoOperation,
                       algorithm, "GetSignatureFormatter")));
            }

            switch (algorithm)
            {
                case SignedXml.XmlDsigDSAUrl:

                    // Ensure that we have a DSA algorithm object.
                    DSA dsa = (this.PrivateKey as DSA);
                    if (dsa == null)
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(SR.GetString(SR.PrivateKeyNotDSA)));
                    return new DSASignatureFormatter(dsa);

                case SignedXml.XmlDsigRSASHA1Url:
                    // Ensure that we have an RSA algorithm object.
                    RSA rsa = (this.PrivateKey as RSA);
                    if (rsa == null)
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(SR.GetString(SR.PrivateKeyNotRSA)));
                    return new RSAPKCS1SignatureFormatter(rsa);

                case SecurityAlgorithms.RsaSha256Signature:
                    // Ensure that we have an RSA algorithm object.
                    RSACryptoServiceProvider rsa_prov_full = (this.PrivateKey as RSACryptoServiceProvider);
                    if (rsa_prov_full == null)
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(SR.GetString(SR.PrivateKeyNotRSA)));
                    CspParameters csp = new CspParameters();
                    csp.ProviderType = 24;
                    csp.KeyContainerName = rsa_prov_full.CspKeyContainerInfo.KeyContainerName;
                    csp.KeyNumber = (int)rsa_prov_full.CspKeyContainerInfo.KeyNumber;
                    if (rsa_prov_full.CspKeyContainerInfo.MachineKeyStore)
                        csp.Flags = CspProviderFlags.UseMachineKeyStore;
                    
                    csp.Flags |= CspProviderFlags.UseExistingKey;

                    return new RSAPKCS1SignatureFormatter(new RSACryptoServiceProvider(csp));

                default:
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(SR.GetString(SR.UnsupportedCryptoAlgorithm, algorithm)));
            }

        }

        public override bool HasPrivateKey()
        {
            return (this.PrivateKey != null);
        }

        public override bool IsAsymmetricAlgorithm(string algorithm)
        {
            if (string.IsNullOrEmpty(algorithm))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(algorithm, SR.GetString(SR.EmptyOrNullArgumentString, "algorithm"));
            }

            return (CryptoHelper.IsAsymmetricAlgorithm(algorithm));
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
                algorithm = null;
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
                case SignedXml.XmlDsigDSAUrl:
                    return (this.PublicKey.Key is DSA);

                case SignedXml.XmlDsigRSASHA1Url:
                case SecurityAlgorithms.RsaSha256Signature:
                case EncryptedXml.XmlEncRSA15Url:
                case EncryptedXml.XmlEncRSAOAEPUrl:
                    return (this.PublicKey.Key is RSA);
                default:
                    return false;
            }
        }

        public override bool IsSymmetricAlgorithm(string algorithm)
        {
            return CryptoHelper.IsSymmetricAlgorithm(algorithm);
        }
    }
}
