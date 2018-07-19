// Copyright (c) Microsoft Corporation.  All rights reserved.

using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace System.Security.Cryptography.X509Certificates
{
    /// <summary>
    /// Provides extension methods for retrieving <see cref="RSA" /> implementations for the
    /// public and private keys of a <see cref="X509Certificate2" />.
    /// </summary>
    public static class RSACertificateExtensions
    {
        /// <summary>
        /// Gets the <see cref="RSA" /> public key from the certificate or null if the certificate does not have an RSA public key.
        /// </summary>
        [SecuritySafeCritical]
        public static RSA GetRSAPublicKey(this X509Certificate2 certificate)
        {
            if (certificate == null)
            {
                throw new ArgumentNullException("certificate");
            }

            if (!IsRSA(certificate))
            {
                return null;
            }

            PublicKey publicKey = certificate.PublicKey;
            AsnEncodedData asn = publicKey.EncodedKeyValue;
            IntPtr structType = new IntPtr(CapiNative.CNG_RSA_PUBLIC_KEY_BLOB);

            SafeLocalAllocHandle cngBlobHandle;
            uint cngBlobLength;
            bool result = CapiNative.DecodeObject(structType, asn.RawData, out cngBlobHandle, out cngBlobLength);

            if (!result)
            {
                throw new CryptographicException(Marshal.GetLastWin32Error());
            }

            byte[] cngBlob = new byte[cngBlobLength];
            using (cngBlobHandle)
            {
                Marshal.Copy(cngBlobHandle.DangerousGetHandle(), cngBlob, 0, cngBlob.Length);
            }

            CngKey key = CngKey.Import(cngBlob, CngKeyBlobFormat.GenericPublicBlob);
            return new RSACng(key);
        }

        /// <summary>
        /// Gets the <see cref="RSA" /> private key from the certificate or null if the certificate does not have an RSA private key.
        /// </summary>
        [SecuritySafeCritical]
        public static RSA GetRSAPrivateKey(this X509Certificate2 certificate)
        {
            if (certificate == null)
            {
                throw new ArgumentNullException("certificate");
            }

            if (!certificate.HasPrivateKey || !IsRSA(certificate))
            {
                return null;
            }

            using (SafeCertContextHandle certificateContext = X509Native.GetCertificateContext(certificate))
            using (SafeNCryptKeyHandle privateKeyHandle = X509Native.TryAcquireCngPrivateKey(certificateContext))
            {
                if (privateKeyHandle == null)
                {
                    if (LocalAppContextSwitches.DontReliablyClonePrivateKey)
                        return (RSA)certificate.PrivateKey;

                    // fall back to CAPI if we cannot acquire the key using CNG.
                    RSACryptoServiceProvider rsaCsp = (RSACryptoServiceProvider)certificate.PrivateKey;
                    CspParameters cspParameters = DSACertificateExtensions.CopyCspParameters(rsaCsp);
                    RSACryptoServiceProvider clone = new RSACryptoServiceProvider(cspParameters);
                    return clone;
                }

                CngKey key = CngKey.Open(privateKeyHandle, CngKeyHandleOpenOptions.None);
                return new RSACng(key);
            }
        }

        private static bool IsRSA(X509Certificate2 certificate)
        {
            uint algorithmId = OidToAlgorithmId(certificate.PublicKey.Oid);

            switch (algorithmId)
            {
                case CapiNative.CALG_RSA_SIGN:
                case CapiNative.CALG_RSA_KEYX:
                    return true;
                default:
                    return false;
            }
        }

        private static uint OidToAlgorithmId(Oid oid)
        {
            using (SafeLocalAllocHandle oidHandle = X509Utils.StringToAnsiPtr(oid.Value))
            {
                CapiNative.CRYPT_OID_INFO oidInfo = CapiNative.CryptFindOIDInfo(CapiNative.CRYPT_OID_INFO_OID_KEY, oidHandle, 0);
                return oidInfo.Algid;
            }
        }     
    }
}
