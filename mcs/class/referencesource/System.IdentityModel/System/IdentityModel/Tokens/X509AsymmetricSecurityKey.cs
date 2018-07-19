
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
        AsymmetricAlgorithm publicKey;
        bool publicKeyAvailabilityDetermined;

        object thisLock = new Object();

        public X509AsymmetricSecurityKey(X509Certificate2 certificate)
        {
            if (certificate == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("certificate");

            this.certificate = certificate;
        }

        public override int KeySize
        {
            get { return this.PublicKey.KeySize; }
        }

        AsymmetricAlgorithm PrivateKey
        {
            get
            {
                if (!this.privateKeyAvailabilityDetermined)
                {
                    lock (ThisLock)
                    {
                        if (LocalAppContextSwitches.DisableCngCertificates)
                        {
                            this.privateKey = this.certificate.PrivateKey;
                        }
                        else
                        {
                            this.privateKey = CngLightup.GetRSAPrivateKey(this.certificate);
                            if (this.privateKey != null)
                            {
                                RSACryptoServiceProvider rsaCsp = this.privateKey as RSACryptoServiceProvider;
                                // ProviderType == 1 is PROV_RSA_FULL provider type that only supports SHA1. Change it to PROV_RSA_AES=24 that supports SHA2 also.
                                if (rsaCsp != null && rsaCsp.CspKeyContainerInfo.ProviderType == 1)
                                {
                                    CspParameters csp = new CspParameters();
                                    csp.ProviderType = 24;
                                    csp.KeyContainerName = rsaCsp.CspKeyContainerInfo.KeyContainerName;
                                    csp.KeyNumber = (int)rsaCsp.CspKeyContainerInfo.KeyNumber;
                                    if (rsaCsp.CspKeyContainerInfo.MachineKeyStore)
                                        csp.Flags = CspProviderFlags.UseMachineKeyStore;

                                    csp.Flags |= CspProviderFlags.UseExistingKey;
                                    this.privateKey = new RSACryptoServiceProvider(csp);
                                }
                            }
                            else
                            {
                                this.privateKey = CngLightup.GetDSAPrivateKey(this.certificate);
                            }
                            if (certificate.HasPrivateKey && this.privateKey == null)
                                DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(SR.GetString(SR.PrivateKeyNotSupported)));
                        }
                        this.privateKeyAvailabilityDetermined = true;
                    }
                }
                return this.privateKey;
            }
        }

        AsymmetricAlgorithm PublicKey
        {
            get
            {
                if (!this.publicKeyAvailabilityDetermined)
                {
                    lock (ThisLock)
                    {
                        if (!this.publicKeyAvailabilityDetermined)
                        {
                            if (LocalAppContextSwitches.DisableCngCertificates)
                            {
                                this.publicKey = this.certificate.PublicKey.Key;
                            }
                            else
                            {
                                this.publicKey = CngLightup.GetRSAPublicKey(this.certificate);
                                if (this.publicKey == null)
                                    this.publicKey = CngLightup.GetDSAPublicKey(this.certificate);
                                if (this.publicKey == null)
                                    DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(SR.GetString(SR.PublicKeyNotSupported)));
                            }
                            this.publicKeyAvailabilityDetermined = true;
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
            RSA rsa = this.PublicKey as RSA;
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
                        if ((this.PublicKey as DSA) != null)
                        {
                            return (this.PublicKey as DSA);
                        }
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(SR.GetString(SR.AlgorithmAndPublicKeyMisMatch)));
                    case SignedXml.XmlDsigRSASHA1Url:
                    case SecurityAlgorithms.RsaSha256Signature:
                    case EncryptedXml.XmlEncRSA15Url:
                    case EncryptedXml.XmlEncRSAOAEPUrl:
                        if ((this.PublicKey as RSA) != null)
                        {
                            return (this.PublicKey as RSA);
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
                    return description.CreateDeformatter(this.PublicKey);

                try
                {
                    AsymmetricSignatureDeformatter asymmetricSignatureDeformatter = algorithmObject as AsymmetricSignatureDeformatter;
                    if (asymmetricSignatureDeformatter != null)
                    {
                        asymmetricSignatureDeformatter.SetKey(this.PublicKey);
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
                    DSA dsa = (this.PublicKey as DSA);
                    if (dsa == null)
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(SR.GetString(SR.PublicKeyNotDSA)));
                    return new DSASignatureDeformatter(dsa);

                case SignedXml.XmlDsigRSASHA1Url:
                case SecurityAlgorithms.RsaSha256Signature:
                    // Ensure that we have an RSA algorithm object.
                    RSA rsa = (this.PublicKey as RSA);
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

            // We support:
            //     XmlDsigDSAUrl = "http://www.w3.org/2000/09/xmldsig#dsa-sha1";
            //     XmlDsigRSASHA1Url = "http://www.w3.org/2000/09/xmldsig#rsa-sha1";
            //     RsaSha256Signature = "http://www.w3.org/2001/04/xmldsig-more#rsa-sha256";
            AsymmetricAlgorithm privateKey = LevelUpRsa(this.PrivateKey, algorithm);

            object algorithmObject = CryptoHelper.GetAlgorithmFromConfig(algorithm);
            if (algorithmObject != null)
            {
                SignatureDescription description = algorithmObject as SignatureDescription;
                if (description != null)
                    return description.CreateFormatter(privateKey);

                try
                {
                    AsymmetricSignatureFormatter asymmetricSignatureFormatter = algorithmObject as AsymmetricSignatureFormatter;
                    if (asymmetricSignatureFormatter != null)
                    {
                        asymmetricSignatureFormatter.SetKey(privateKey);
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
                    RSA rsaSha256 = (privateKey as RSA);
                    if (rsaSha256 == null)
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(SR.GetString(SR.PrivateKeyNotRSA)));
                    return new RSAPKCS1SignatureFormatter(rsaSha256);

                default:
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(SR.GetString(SR.UnsupportedCryptoAlgorithm, algorithm)));
            }

        }

        private static AsymmetricAlgorithm LevelUpRsa(AsymmetricAlgorithm asymmetricAlgorithm, string algorithm)
        {
            // If user turned off leveling up at app level, return
            if (LocalAppContextSwitches.DisableUpdatingRsaProviderType)
                return asymmetricAlgorithm;

            if (asymmetricAlgorithm == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("asymmetricAlgorithm"));

            if (string.IsNullOrEmpty(algorithm))
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(algorithm, SR.GetString(SR.EmptyOrNullArgumentString, "algorithm"));

            // only level up if alg is sha256
            if (!string.Equals(algorithm, SecurityAlgorithms.RsaSha256Signature))
                return asymmetricAlgorithm;

            RSACryptoServiceProvider rsaCsp = asymmetricAlgorithm as RSACryptoServiceProvider;
            if (rsaCsp == null)
                return asymmetricAlgorithm;

            // ProviderType == 1(PROV_RSA_FULL) and providerType == 12(PROV_RSA_SCHANNEL) are provider types that only support SHA1. Change them to PROV_RSA_AES=24 that supports SHA2 also.
            // Only levels up if the associated key is not a hardware key.
            // Another provider type related to rsa, PROV_RSA_SIG == 2 that only supports Sha1 is no longer supported
            if ((rsaCsp.CspKeyContainerInfo.ProviderType == 1 || rsaCsp.CspKeyContainerInfo.ProviderType == 12) && !rsaCsp.CspKeyContainerInfo.HardwareDevice)
            {
                CspParameters csp = new CspParameters();
                csp.ProviderType = 24;
                csp.KeyContainerName = rsaCsp.CspKeyContainerInfo.KeyContainerName;
                csp.KeyNumber = (int)rsaCsp.CspKeyContainerInfo.KeyNumber;
                if (rsaCsp.CspKeyContainerInfo.MachineKeyStore)
                    csp.Flags = CspProviderFlags.UseMachineKeyStore;

                csp.Flags |= CspProviderFlags.UseExistingKey;
                return new RSACryptoServiceProvider(csp);
            }

            return rsaCsp;
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
                    return (this.PublicKey is DSA);

                case SignedXml.XmlDsigRSASHA1Url:
                case SecurityAlgorithms.RsaSha256Signature:
                case EncryptedXml.XmlEncRSA15Url:
                case EncryptedXml.XmlEncRSAOAEPUrl:
                    return (this.PublicKey is RSA);
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
