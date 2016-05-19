//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.IdentityModel.Selectors
{
    using System;
    using System.Security.Cryptography;
    using System.Security.Cryptography.Xml;
    using System.IdentityModel.Tokens;
    using IDT = Microsoft.InfoCards.Diagnostics.InfoCardTrace;

    //
    // For common & resources
    //
    using Microsoft.InfoCards;

    //
    // Summary:
    //  This class implements the IAsymmetricCrypto interface and is used as an adapter between the
    //  InfoCard system and Indigo.
    //
    internal class InfoCardAsymmetricCrypto : AsymmetricSecurityKey, IDisposable
    {
        InfoCardRSACryptoProvider m_rsa;

        //
        // Summary:
        //  Constructs a new InfoCardAsymmetricCrypto given an InfoCardRSACryptoProvider.
        //
        // Parameters:
        //  cryptoHandle  - the handle to the asymmetric key to base this crypto object on.  
        public InfoCardAsymmetricCrypto(AsymmetricCryptoHandle cryptoHandle)
        {
            m_rsa = new InfoCardRSACryptoProvider(cryptoHandle);
        }

        //
        // Summary:
        //  Returns the size of the asymmetric key
        //
        public override int KeySize
        {
            get { return m_rsa.KeySize; }
        }

        //
        // Summary:
        //  Indicates whether this IAsymmetricCrypto has access to the private key.
        //  In our case, that's the whole point, so it always returns true.
        //
        public override bool HasPrivateKey()
        {
            return true;
        }

        //
        // Summary:
        //  Returns a reference to the InfoCardRSACryptoProvider that give Indigo access to
        //  the private key associated with the infocard, recipient tuple.
        //
        // Parameters:
        //  algorithmUri  - The URI of the algorithm being requested.
        //  privateKey    - set to true if access to the private key is required.
        //
        public override AsymmetricAlgorithm GetAsymmetricAlgorithm(string algorithmUri, bool privateKey)
        {
            switch (algorithmUri)
            {
                case SignedXml.XmlDsigRSASHA1Url:
                case EncryptedXml.XmlEncRSA15Url:
                case EncryptedXml.XmlEncRSAOAEPUrl:
                    return m_rsa;

                default:
                    throw IDT.ThrowHelperError(new NotSupportedException(SR.GetString(SR.ClientUnsupportedCryptoAlgorithm, algorithmUri)));
            }
        }

        //
        // Sumamry:
        //  Returns a HashAlgorithm
        //
        // Parameters:
        //  algorithmUri  - the uri of the hash algorithm being requested.
        //
        public override HashAlgorithm GetHashAlgorithmForSignature(string algorithmUri)
        {
            switch (algorithmUri)
            {
                case SignedXml.XmlDsigRSASHA1Url:
                    return new SHA1Managed();
                default:
                    throw IDT.ThrowHelperError(new NotSupportedException(SR.GetString(SR.ClientUnsupportedCryptoAlgorithm, algorithmUri)));
            }
        }

        //
        // Summary:
        //  Returns a Signature deformatter.
        //
        // Parameters:
        //  algorithmUri  - the uri of signature deformatter being requeted.
        //
        public override AsymmetricSignatureDeformatter GetSignatureDeformatter(string algorithmUri)
        {
            switch (algorithmUri)
            {
                case SignedXml.XmlDsigRSASHA1Url:
                    return new InfoCardRSAPKCS1SignatureDeformatter(m_rsa);

                default:
                    throw IDT.ThrowHelperError(new NotSupportedException(SR.GetString(SR.ClientUnsupportedCryptoAlgorithm, algorithmUri)));
            }
        }

        //
        // Summary:
        //  Returns a Signature formatter.
        //
        // Parameters:
        //  algorithmUri  - the uri of signature formatter being requeted.
        //
        public override AsymmetricSignatureFormatter GetSignatureFormatter(string algorithmUri)
        {
            switch (algorithmUri)
            {
                case SignedXml.XmlDsigRSASHA1Url:
                    return new InfoCardRSAPKCS1SignatureFormatter(m_rsa);

                default:
                    throw IDT.ThrowHelperError(new NotSupportedException(SR.GetString(SR.ClientUnsupportedCryptoAlgorithm, algorithmUri)));
            }
        }

        //
        // Summary:
        //  Decrypts a symmetric key using the private key of a public/private key pair.
        //
        // Parameters:
        //  algorithmUri  - The algorithm to use to decrypt the key.
        //  keyData       - the key to decrypt.
        //
        public override byte[] DecryptKey(string algorithmUri, byte[] keyData)
        {
            AsymmetricKeyExchangeDeformatter deformatter;

            switch (algorithmUri)
            {
                case EncryptedXml.XmlEncRSA15Url:

                    deformatter = new InfoCardRSAPKCS1KeyExchangeDeformatter(m_rsa);
                    return deformatter.DecryptKeyExchange(keyData);

                case EncryptedXml.XmlEncRSAOAEPUrl:

                    deformatter = new InfoCardRSAOAEPKeyExchangeDeformatter(m_rsa);
                    return deformatter.DecryptKeyExchange(keyData);

                default:
                    throw IDT.ThrowHelperError(new NotSupportedException(SR.GetString(SR.ClientUnsupportedCryptoAlgorithm, algorithmUri)));
            }
        }

        //
        // Summary:
        //  Encrypts a symmetric key using the public key of a public/private key pair.
        //
        // Parameters:
        //  algorithmUri  - The algorithm to use to encrypt the key.
        //  keyData       - the key to encrypt.
        //
        public override byte[] EncryptKey(string algorithmUri, byte[] keyData)
        {
            AsymmetricKeyExchangeFormatter formatter;

            switch (algorithmUri)
            {
                case EncryptedXml.XmlEncRSA15Url:

                    formatter = new InfoCardRSAPKCS1KeyExchangeFormatter(m_rsa);
                    return formatter.CreateKeyExchange(keyData);

                case EncryptedXml.XmlEncRSAOAEPUrl:

                    formatter = new InfoCardRSAOAEPKeyExchangeFormatter(m_rsa);
                    return formatter.CreateKeyExchange(keyData);

                default:
                    throw IDT.ThrowHelperError(new NotSupportedException(SR.GetString(SR.ClientUnsupportedCryptoAlgorithm, algorithmUri)));
            }
        }

        public override bool IsSupportedAlgorithm(string algorithmUri)
        {
            switch (algorithmUri)
            {
                case SignedXml.XmlDsigRSASHA1Url:
                case EncryptedXml.XmlEncRSA15Url:
                case EncryptedXml.XmlEncRSAOAEPUrl:
                    return true;
                default:
                    return false;
            }
        }

        public override bool IsSymmetricAlgorithm(string algorithmUri)
        {
            return InfoCardCryptoHelper.IsSymmetricAlgorithm(algorithmUri);
        }

        public override bool IsAsymmetricAlgorithm(string algorithmUri)
        {
            return InfoCardCryptoHelper.IsAsymmetricAlgorithm(algorithmUri);
        }

        public void Dispose()
        {
            ((IDisposable)m_rsa).Dispose();
            m_rsa = null;
        }
    }
}
