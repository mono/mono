//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.IdentityModel.Selectors
{
    using System;
    using System.ComponentModel;
    using System.Runtime.InteropServices;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.CompilerServices;
    using System.Security.Cryptography;
    using IDT = Microsoft.InfoCards.Diagnostics.InfoCardTrace;
    using DiagnosticUtility = Microsoft.InfoCards.Diagnostics.DiagnosticUtility;


    //
    // For common & resources
    //
    using Microsoft.InfoCards;


    //
    // Summary:
    //  This class provides indirect access to the private key associated with a released token via InfoCard
    //  native crypto functions.
    //
    internal class InfoCardRSACryptoProvider : RSA
    {
        AsymmetricCryptoHandle m_cryptoHandle;
        RpcAsymmetricCryptoParameters m_params;

        //
        // Summary:
        //  Given a pointer to a CryptoHandle create a new instance of this class.
        //
        public InfoCardRSACryptoProvider(AsymmetricCryptoHandle cryptoHandle)
            : base()
        {
            m_cryptoHandle = (AsymmetricCryptoHandle)cryptoHandle.Duplicate();

            try
            {
                m_params = (RpcAsymmetricCryptoParameters)m_cryptoHandle.Parameters;

                int keySize = m_params.keySize;

                LegalKeySizesValue = new KeySizes[1];
                KeySizeValue = keySize;
                LegalKeySizesValue[0] = new KeySizes(keySize, keySize, 0);
            }
            catch
            {
                m_cryptoHandle.Dispose();
                m_cryptoHandle = null;
                throw;
            }
        }

        public override String SignatureAlgorithm
        {
            get { return m_params.signatureAlgorithm; }
        }

        public override String KeyExchangeAlgorithm
        {
            get { return m_params.keyExchangeAlgorithm; }
        }

        public override byte[] EncryptValue(byte[] rgb)
        {
            throw IDT.ThrowHelperError(new NotSupportedException());
        }

        public override byte[] DecryptValue(byte[] rgb)
        {
            throw IDT.ThrowHelperError(new NotSupportedException());
        }

        public byte[] Decrypt(byte[] inData, bool fAOEP)
        {
            GlobalAllocSafeHandle pOutData = null;
            int cbOutData = 0;
            byte[] outData;
            IDT.ThrowInvalidArgumentConditional(null == inData, "indata");
            using (HGlobalSafeHandle pInData = HGlobalSafeHandle.Construct(inData.Length))
            {
                Marshal.Copy(inData, 0, pInData.DangerousGetHandle(), inData.Length);
                int status = CardSpaceSelector.GetShim().m_csShimDecrypt(m_cryptoHandle.InternalHandle,
                                                    fAOEP,
                                                    inData.Length,
                                                    pInData,
                                                    out cbOutData,
                                                    out pOutData);

                if (0 != status)
                {
                    ExceptionHelper.ThrowIfCardSpaceException(status);
                    throw IDT.ThrowHelperError(new Win32Exception(status));
                }
                pOutData.Length = cbOutData;
                outData = DiagnosticUtility.Utility.AllocateByteArray(pOutData.Length);
                using (pOutData)
                {
                    Marshal.Copy(pOutData.DangerousGetHandle(), outData, 0, pOutData.Length);
                }
            }

            return outData;
        }

        public byte[] Encrypt(byte[] inData, bool fAOEP)
        {
            GlobalAllocSafeHandle pOutData = null;
            int cbOutData = 0;
            byte[] outData;
            IDT.ThrowInvalidArgumentConditional(null == inData, "indata");
            using (HGlobalSafeHandle pInData = HGlobalSafeHandle.Construct(inData.Length))
            {
                Marshal.Copy(inData, 0, pInData.DangerousGetHandle(), inData.Length);
                int status = CardSpaceSelector.GetShim().m_csShimEncrypt(m_cryptoHandle.InternalHandle,
                                                    fAOEP,
                                                    inData.Length,
                                                    pInData,
                                                    out cbOutData,
                                                    out pOutData);

                if (0 != status)
                {
                    ExceptionHelper.ThrowIfCardSpaceException(status);
                    throw IDT.ThrowHelperError(new Win32Exception(status));
                }

                pOutData.Length = cbOutData;
                outData = DiagnosticUtility.Utility.AllocateByteArray(pOutData.Length);
                Marshal.Copy(pOutData.DangerousGetHandle(), outData, 0, pOutData.Length);
            }

            return outData;
        }

        public byte[] SignHash(byte[] hash, string hashAlgOid)
        {
            IDT.ThrowInvalidArgumentConditional(null == hash || 0 == hash.Length, "hash");
            IDT.ThrowInvalidArgumentConditional(String.IsNullOrEmpty(hashAlgOid), "hashAlgOid");
            int cbSig = 0;
            GlobalAllocSafeHandle pSig = null;
            byte[] sig;
            using (HGlobalSafeHandle pHash = HGlobalSafeHandle.Construct(hash.Length))
            {
                using (HGlobalSafeHandle pHashAlgOid = HGlobalSafeHandle.Construct(hashAlgOid))
                {

                    Marshal.Copy(hash, 0, pHash.DangerousGetHandle(), hash.Length);

                    RuntimeHelpers.PrepareConstrainedRegions();
                    int status = CardSpaceSelector.GetShim().m_csShimSignHash(m_cryptoHandle.InternalHandle,
                                                         hash.Length,
                                                         pHash,
                                                         pHashAlgOid,
                                                         out cbSig,
                                                         out pSig);

                    if (0 != status)
                    {
                        ExceptionHelper.ThrowIfCardSpaceException(status);
                        throw IDT.ThrowHelperError(new Win32Exception(status));
                    }
                    pSig.Length = cbSig;
                    sig = DiagnosticUtility.Utility.AllocateByteArray(pSig.Length);
                    using (pSig)
                    {
                        Marshal.Copy(pSig.DangerousGetHandle(), sig, 0, pSig.Length);
                    }

                }
            }

            return sig;
        }

        public bool VerifyHash(byte[] hash, string hashAlgOid, byte[] sig)
        {
            IDT.ThrowInvalidArgumentConditional(null == hash || 0 == hash.Length, "hash");
            IDT.ThrowInvalidArgumentConditional(String.IsNullOrEmpty(hashAlgOid), "hashAlgOid");
            IDT.ThrowInvalidArgumentConditional(null == sig || 0 == sig.Length, "sig");
            bool verified = false;
            using (HGlobalSafeHandle pHash = HGlobalSafeHandle.Construct(hash.Length))
            {
                using (HGlobalSafeHandle pHashAlgOid = HGlobalSafeHandle.Construct(hashAlgOid))
                {


                    Marshal.Copy(hash, 0, pHash.DangerousGetHandle(), hash.Length);
                    int status = 0;
                    using (HGlobalSafeHandle pSig = HGlobalSafeHandle.Construct(sig.Length))
                    {
                        Marshal.Copy(sig, 0, pSig.DangerousGetHandle(), sig.Length);

                        status = CardSpaceSelector.GetShim().m_csShimVerifyHash(m_cryptoHandle.InternalHandle,
                                                               hash.Length,
                                                               pHash,
                                                               pHashAlgOid,
                                                               sig.Length,
                                                               pSig,
                                                               out verified);
                    }
                    if (0 != status)
                    {
                        ExceptionHelper.ThrowIfCardSpaceException(status);
                        throw IDT.ThrowHelperError(new Win32Exception(status));
                    }

                }
            }

            return verified;
        }

        public override RSAParameters ExportParameters(bool includePrivateParameters)
        {
            throw IDT.ThrowHelperError(new NotSupportedException());
        }

        public override string ToXmlString(bool includePrivateParameters)
        {
            throw IDT.ThrowHelperError(new NotSupportedException());
        }

        public override void FromXmlString(string xmlString)
        {
            throw IDT.ThrowHelperError(new NotSupportedException());
        }

        public override void ImportParameters(System.Security.Cryptography.RSAParameters parameters)
        {
            throw IDT.ThrowHelperError(new NotSupportedException());
        }

        protected override void Dispose(bool disposing)
        {
            m_cryptoHandle.Dispose();
        }
    }
}
