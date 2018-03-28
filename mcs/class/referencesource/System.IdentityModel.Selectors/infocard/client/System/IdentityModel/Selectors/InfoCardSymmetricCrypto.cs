//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------
namespace System.IdentityModel.Selectors
{
    using System;
    using System.ComponentModel;
    using System.Runtime.InteropServices;
    using System.Security.Cryptography;
    using System.Security.Cryptography.Xml;
    using System.IdentityModel.Tokens;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.CompilerServices;
    using IDT = Microsoft.InfoCards.Diagnostics.InfoCardTrace;

    //
    // For common & resources
    //
    using Microsoft.InfoCards;

    //
    // Summary:
    //  This class implements the ISymmetricCrypto interface and is used as an adapter between the
    //  InfoCard system and Indigo.
    //
    internal class InfoCardSymmetricCrypto : SymmetricSecurityKey, IDisposable
    {
        SymmetricCryptoHandle m_cryptoHandle;
        RpcSymmetricCryptoParameters m_params;

        //
        // Summary:
        //  Creates a new InfoCardSymmetricCrypto based on a SymmetricCryptoHandle
        //
        // Parameters:
        //  cryptoHandle  - A handle to the symmetric key on which to base this object.
        //
        public InfoCardSymmetricCrypto(SymmetricCryptoHandle cryptoHandle)
            : base()
        {
            m_cryptoHandle = (SymmetricCryptoHandle)cryptoHandle.Duplicate();

            try
            {
                m_params = (RpcSymmetricCryptoParameters)m_cryptoHandle.Parameters;
            }
            catch
            {
                if (null != m_cryptoHandle)
                {
                    m_cryptoHandle.Dispose();
                    m_cryptoHandle = null;
                }
                throw;
            }
        }

        // ICrypto

        public override int KeySize
        {
            get { return m_params.keySize; }
        }

        public override byte[] DecryptKey(string algorithmUri, byte[] keyData)
        {
            throw IDT.ThrowHelperError(new NotImplementedException());
        }

        public override byte[] EncryptKey(string algorithmUri, byte[] keyData)
        {
            throw IDT.ThrowHelperError(new NotImplementedException());
        }

        public override bool IsAsymmetricAlgorithm(string algorithmUri)
        {
            return InfoCardCryptoHelper.IsAsymmetricAlgorithm(algorithmUri);
        }

        public override bool IsSupportedAlgorithm(string algorithmUri)
        {
            switch (algorithmUri)
            {
                case SecurityAlgorithms.Aes128Encryption:
                case SecurityAlgorithms.HmacSha1Signature:
                case SecurityAlgorithms.Psha1KeyDerivation:
                case SecurityAlgorithms.Psha1KeyDerivationDec2005:
                    return true;
                default:
                    return false;
            }
        }

        // why does this map to supported?
        public override bool IsSymmetricAlgorithm(string algorithmUri)
        {
            return IsSupportedAlgorithm(algorithmUri);
        }

        // ISymmetricCrypto

        public override byte[] GenerateDerivedKey(string algorithmUri,
                                          byte[] label,
                                          byte[] nonce,
                                          int derivedKeyLength,
                                          int offset)
        {
            IDT.DebugAssert(!String.IsNullOrEmpty(algorithmUri), "null alg uri");
            IDT.DebugAssert(null != label && 0 != label.Length, "null label");
            IDT.DebugAssert(null != nonce && 0 != nonce.Length, "null nonce");

            if (!IsSupportedAlgorithm(algorithmUri))
            {
                throw IDT.ThrowHelperError(new NotSupportedException(SR.GetString(SR.ClientUnsupportedCryptoAlgorithm, algorithmUri)));
            }
            byte[] derivedKey = null;
            using (HGlobalSafeHandle pLabel = HGlobalSafeHandle.Construct(label.Length))
            {
                using (HGlobalSafeHandle pNonce = HGlobalSafeHandle.Construct(nonce.Length))
                {

                    GlobalAllocSafeHandle pDerivedKey = null;
                    int cbDerivedKey = 0;

                    Marshal.Copy(label, 0, pLabel.DangerousGetHandle(), label.Length);
                    Marshal.Copy(nonce, 0, pNonce.DangerousGetHandle(), nonce.Length);

                    int status = CardSpaceSelector.GetShim().m_csShimGenerateDerivedKey(m_cryptoHandle.InternalHandle,
                                                                   label.Length,
                                                                   pLabel,
                                                                   nonce.Length,
                                                                   pNonce,
                                                                   derivedKeyLength,
                                                                   offset,
                                                                   algorithmUri,
                                                                   out cbDerivedKey,
                                                                   out pDerivedKey);

                    if (0 != status)
                    {
                        throw IDT.ThrowHelperError(new Win32Exception(status));
                    }
                    pDerivedKey.Length = cbDerivedKey;
                    derivedKey = new byte[pDerivedKey.Length];
                    using (pDerivedKey)
                    {
                        Marshal.Copy(pDerivedKey.DangerousGetHandle(), derivedKey, 0, pDerivedKey.Length);
                    }

                }
            }
            return derivedKey;
        }

        //
        // Summary:
        //  Returns an ICryptoTransform based on the algorithm and initialization vector passed in and the underlying
        //  CryptoHandle.
        //
        // Parameters:
        //  algorithmUri  - The algorithm that the transform you implement.
        //  iv            - The initialization vector to use in the transform
        //
        public override ICryptoTransform GetDecryptionTransform(string algorithmUri, byte[] iv)
        {
            ICryptoTransform transform;

            switch (algorithmUri)
            {
                case SecurityAlgorithms.Aes128Encryption:
                    using (InfoCardSymmetricAlgorithm symAlgo = new InfoCardSymmetricAlgorithm(m_cryptoHandle))
                    {
                        symAlgo.IV = iv;
                        transform = symAlgo.CreateDecryptor();
                    }
                    break;
                default:
                    throw IDT.ThrowHelperError(new NotSupportedException(SR.GetString(SR.ClientUnsupportedCryptoAlgorithm, algorithmUri)));
            }

            return transform;
        }

        //
        // Summary:
        //  Returns an ICryptoTransform based on the algorithm and initialization vector passed in and the underlying
        //  CryptoHandle.
        //
        // Parameters:
        //  algorithmUri  - The algorithm that the transform you implement.
        //  iv            - The initialization vector to use in the transform
        //
        public override ICryptoTransform GetEncryptionTransform(string algorithmUri, byte[] iv)
        {
            ICryptoTransform transform;

            switch (algorithmUri)
            {
                case SecurityAlgorithms.Aes128Encryption:
                    using (InfoCardSymmetricAlgorithm symAlgo = new InfoCardSymmetricAlgorithm(m_cryptoHandle))
                    {
                        symAlgo.IV = iv;
                        transform = symAlgo.CreateEncryptor();
                    }
                    break;
                default:
                    throw IDT.ThrowHelperError(new NotSupportedException(SR.GetString(SR.ClientUnsupportedCryptoAlgorithm, algorithmUri)));
            }

            return transform;
        }

        //
        // Summary:
        //  Returns the size of the initialization vector for the underlying crypto algorithm.
        //
        public override int GetIVSize(string algorithmUri)
        {
            int size;

            switch (algorithmUri)
            {
                case SecurityAlgorithms.Aes128Encryption:
                    RpcSymmetricCryptoParameters param = (RpcSymmetricCryptoParameters)m_cryptoHandle.Parameters;
                    size = param.blockSize;
                    break;
                default:
                    throw IDT.ThrowHelperError(new NotSupportedException(SR.GetString(SR.ClientUnsupportedCryptoAlgorithm, algorithmUri)));
            }

            return size;
        }

        //
        // Summary:
        //  Returns a KeyedHashAlgorithm based on the underlying crypto handle and the algorithm passed in.
        //
        public override KeyedHashAlgorithm GetKeyedHashAlgorithm(string algorithmUri)
        {
            switch (algorithmUri)
            {
                case SecurityAlgorithms.HmacSha1Signature:
                    return new InfoCardKeyedHashAlgorithm(m_cryptoHandle);
                default:
                    throw IDT.ThrowHelperError(new NotSupportedException(SR.GetString(SR.ClientUnsupportedCryptoAlgorithm, algorithmUri)));
            }
        }

        //
        // Summary:
        //  Returns a SymmetricAlgorithm based on the underlying crypto handle and the algorithm passed in.
        //
        public override SymmetricAlgorithm GetSymmetricAlgorithm(string algorithmUri)
        {
            SymmetricAlgorithm algorithm;
            switch (algorithmUri)
            {
                case SecurityAlgorithms.Aes128Encryption:
                    algorithm = new InfoCardSymmetricAlgorithm(m_cryptoHandle);
                    break;
                default:
                    throw IDT.ThrowHelperError(new NotSupportedException(SR.GetString(SR.ClientUnsupportedCryptoAlgorithm, algorithmUri)));
            }

            return algorithm;
        }

        public override byte[] GetSymmetricKey()
        {
            throw IDT.ThrowHelperError(new NotImplementedException());
        }

        //
        // IDisposable
        //
        public void Dispose()
        {
            if (null != m_cryptoHandle)
            {
                m_cryptoHandle.Dispose();
                m_cryptoHandle = null;
            }
        }
    }
}
