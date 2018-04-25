//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------
namespace System.ServiceModel.Channels
{
    using System.ComponentModel;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Security.Cryptography.X509Certificates;
    using System.ServiceModel.Diagnostics;

    sealed partial class SelfSignedCertificate : IDisposable
    {
        CertificateHandle cert;
        KeyContainerHandle keyContainer;
        KeyHandle key;
        string keyContainerName;
        string password;
        byte[] exportedBytes;
        X509Certificate2 x509Cert;

        const int CERT_STORE_PROV_MEMORY = 2;
        const int DefaultLifeSpanInYears = 2;

        public static SelfSignedCertificate Create(string name, string password)
        {
            return Create(name,
                        password,
                        DateTime.UtcNow,
                        DateTime.UtcNow.AddYears(DefaultLifeSpanInYears),
                        Guid.NewGuid().ToString());
        }

        public static SelfSignedCertificate Create(
                                    string name,
                                    string password,
                                    DateTime start,
                                    DateTime expire,
                                    string containerName)
        {
            SelfSignedCertificate cert = new SelfSignedCertificate(password, containerName);
            cert.GenerateKeys();
            cert.CreateCertContext(name, start, expire);
            cert.GetX509Certificate();
            Fx.Assert(cert.cert != null, "CertContext could not be created");
            return cert;
        }

        void CreateCertContext(string name, DateTime start, DateTime expire)
        {
            CriticalAllocHandle provInfo;
            CriticalAllocHandle algorithmId;
            provInfo = GetProviderInfo();
            algorithmId = GetSha1AlgorithmId();

            // convert the times to SystemTime structures
            SystemTime beginTime = new SystemTime(start);
            SystemTime expireTime = new SystemTime(expire);

            // convert the name into a X500 name
            CertificateName certName = new CertificateName(name);

            using (CryptoApiBlob nameBlob = certName.GetCryptoApiBlob())
            {
                using (provInfo)
                {
                    using (algorithmId)
                    {
                        cert = CertCreateSelfSignCertificate(keyContainer,
                                                                    nameBlob.GetMemoryForPinning(),
                                                                    SelfSignFlags.None,
                                                                    provInfo,
                                                                    algorithmId,
                                                                    ref beginTime,
                                                                    ref expireTime,
                                                                    IntPtr.Zero);

                        if (cert.IsInvalid)
                            PeerExceptionHelper.ThrowInvalidOperation_PeerCertGenFailure(PeerExceptionHelper.GetLastException());

                        //                        if (!CertSetCertificateContextProperty(cert, CERT_KEY_PROV_INFO_PROP_ID, 0, provInfo))
                        //                          PeerExceptionHelper.ThrowInvalidOperation_PeerCertGenFailure(PeerExceptionHelper.GetLastException());
                        if (!CertSetCertificateContextProperty(cert, CERT_KEY_SPEC_PROP_ID, 0, key))
                            PeerExceptionHelper.ThrowInvalidOperation_PeerCertGenFailure(PeerExceptionHelper.GetLastException());
                    }
                }
            }
        }

        public X509Certificate2 GetX509Certificate()
        {
            if (this.x509Cert == null)
            {
                Export();
                this.x509Cert = new X509Certificate2(exportedBytes, password);
            }
            return this.x509Cert;
        }

        void Export()
        {
            Fx.Assert(this.exportedBytes == null, "calling Export twice!!");

            // create a temporary store to export
            using (CertificateStoreHandle store = CertOpenStore(new IntPtr(CERT_STORE_PROV_MEMORY),
                                                                0,
                                                                IntPtr.Zero,
                                                                0,
                                                                IntPtr.Zero))
            {
                // add the certificate to the store
                StoreCertificateHandle addedCert;
                if (!CertAddCertificateContextToStore(store,
                                                cert,
                                                AddDisposition.ReplaceExisting,
                                                out addedCert))
                {
                    int error = Marshal.GetLastWin32Error();
                    Utility.CloseInvalidOutSafeHandle(addedCert);
                    PeerExceptionHelper.ThrowInvalidOperation_PeerCertGenFailure(new Win32Exception(error));
                }

                using (addedCert)
                {
                    // Translate to a PFX
                    CryptoApiBlob pfxBlob = new CryptoApiBlob();
                    CryptoApiBlob.InteropHelper blob = pfxBlob.GetMemoryForPinning();
                    GCHandle pfxHandle = GCHandle.Alloc(blob, GCHandleType.Pinned);

                    try
                    {
                        // first figure out the storage space necessary
                        bool result = PFXExportCertStoreEx(store,
                                                            pfxHandle.AddrOfPinnedObject(),
                                                            password,
                                                            IntPtr.Zero,
                                                            PfxExportFlags.ExportPrivateKeys |
                                                            PfxExportFlags.ReportNoPrivateKey |
                                                            PfxExportFlags.ReportNotAbleToExportPrivateKey);

                        if (!result)
                            PeerExceptionHelper.ThrowInvalidOperation_PeerCertGenFailure(PeerExceptionHelper.GetLastException());

                        int storageSize = blob.size;
                        pfxHandle.Free();
                        pfxBlob.AllocateBlob(storageSize);
                        blob = pfxBlob.GetMemoryForPinning();
                        pfxHandle = GCHandle.Alloc(blob, GCHandleType.Pinned);

                        // now do the translation
                        if (!PFXExportCertStoreEx(store,
                                                    pfxHandle.AddrOfPinnedObject(),
                                                    password,
                                                    IntPtr.Zero,
                                                    PfxExportFlags.ExportPrivateKeys |
                                                    PfxExportFlags.ReportNoPrivateKey |
                                                    PfxExportFlags.ReportNotAbleToExportPrivateKey))
                            PeerExceptionHelper.ThrowInvalidOperation_PeerCertGenFailure(PeerExceptionHelper.GetLastException());
                        exportedBytes = pfxBlob.GetBytes();
                    }
                    finally
                    {
                        if (pfxHandle != null)
                            pfxHandle.Free();

                        if (pfxBlob != null)
                            pfxBlob.Dispose();
                    }
                }
            }
        }

        void GenerateKeys()
        {
            // generate the key container to put the key in
            if (!CryptAcquireContext(out keyContainer,
                                        keyContainerName,
                                        null,
                                        ProviderType.RsaSecureChannel,
                                        ContextFlags.NewKeySet | ContextFlags.Silent))
            {
                int error = Marshal.GetLastWin32Error();
                Utility.CloseInvalidOutSafeHandle(keyContainer);
                keyContainer = null;
                PeerExceptionHelper.ThrowInvalidOperation_PeerCertGenFailure(new Win32Exception(error));
            }

            // generate the key
            if (!CryptGenKey(keyContainer,
                                AlgorithmType.KeyExchange,
                                KeyFlags.Exportable2k,
                                out key))
            {
                int error = Marshal.GetLastWin32Error();
                Utility.CloseInvalidOutSafeHandle(key);
                key = null;
                PeerExceptionHelper.ThrowInvalidOperation_PeerCertGenFailure(new Win32Exception(error));
            }
        }

        void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (cert != null)
                    cert.Dispose();
                if (key != null)
                    key.Dispose();
                if (keyContainer != null)
                    keyContainer.Dispose();
                if (keyContainerName != null)
                {
                    CryptAcquireContext(out keyContainer,
                                        keyContainerName,
                                        null,
                                        ProviderType.RsaSecureChannel,
                                        ContextFlags.DeleteKeySet);
                    Utility.CloseInvalidOutSafeHandle(keyContainer);
                }
                GC.SuppressFinalize(this);
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        SelfSignedCertificate(string password, string containerName)
        {
            this.password = password;
            this.keyContainerName = containerName;
        }
    }
}

