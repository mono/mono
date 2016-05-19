//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.IdentityModel.Selectors
{
    using Microsoft.Win32.SafeHandles;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Runtime.CompilerServices;
    using System.Runtime.ConstrainedExecution;
    using System.Security;
    using System.Security.Cryptography;
    using System.Security.Cryptography.X509Certificates;
    using System.Security.Permissions;

    class X509CertificateStore
    {
        SafeCertStoreHandle certStoreHandle = SafeCertStoreHandle.InvalidHandle;
        string storeName;
        StoreLocation storeLocation;

        [Fx.Tag.SecurityNote(Critical = "Uses critical type SafeCertStoreHandle.",
            Safe = "Performs a Demand for full trust.")]
        [SecuritySafeCritical]
        [SecurityPermission(SecurityAction.Demand, Unrestricted = true)]
        public X509CertificateStore(StoreName storeName, StoreLocation storeLocation)
        {
            switch (storeName)
            {
                case StoreName.AddressBook:
                    this.storeName = "AddressBook";
                    break;
                case StoreName.AuthRoot:
                    this.storeName = "AuthRoot";
                    break;
                case StoreName.CertificateAuthority:
                    this.storeName = "CA";
                    break;
                case StoreName.Disallowed:
                    this.storeName = "Disallowed";
                    break;
                case StoreName.My:
                    this.storeName = "My";
                    break;
                case StoreName.Root:
                    this.storeName = "Root";
                    break;
                case StoreName.TrustedPeople:
                    this.storeName = "TrustedPeople";
                    break;
                case StoreName.TrustedPublisher:
                    this.storeName = "TrustedPublisher";
                    break;
                default:
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidEnumArgumentException("storeName", (int)storeName,
                        typeof(StoreName)));

            }

            if (storeLocation != StoreLocation.CurrentUser && storeLocation != StoreLocation.LocalMachine)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("storeLocation", SR.GetString(SR.X509CertStoreLocationNotValid)));
            }
            this.storeLocation = storeLocation;
        }

        [Fx.Tag.SecurityNote(Critical = "Uses critical type SafeCertStoreHandle.",
            Safe = "Performs a Demand for full trust.")]
        [SecuritySafeCritical]
        [SecurityPermission(SecurityAction.Demand, Unrestricted = true)]
        public void Close()
        {
            // Accessing via IDisposable to avoid Security check (functionally the same)
            ((IDisposable)this.certStoreHandle).Dispose();
        }

        [Fx.Tag.SecurityNote(Critical = "Uses critical type SafeCertStoreHandle.",
            Safe = "Performs a Demand for full trust.")]
        [SecuritySafeCritical]
        [SecurityPermission(SecurityAction.Demand, Unrestricted = true)]
        public void Open(OpenFlags openFlags)
        {
            DiagnosticUtility.DebugAssert(this.certStoreHandle.IsInvalid, "");

            uint dwOpenFlags = MapX509StoreFlags(this.storeLocation, openFlags);
            SafeCertStoreHandle certStoreHandle = CAPI.CertOpenStore(new IntPtr(CAPI.CERT_STORE_PROV_SYSTEM),
                                                       CAPI.X509_ASN_ENCODING | CAPI.PKCS_7_ASN_ENCODING,
                                                       IntPtr.Zero,
                                                       dwOpenFlags,
                                                       this.storeName);

            if (certStoreHandle == null || certStoreHandle.IsInvalid)
            {
                int error = Marshal.GetLastWin32Error();
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new CryptographicException(error));
            }
            this.certStoreHandle = certStoreHandle;
        }

        [Fx.Tag.SecurityNote(Critical = "Uses critical types SafeCertContextHandle, SafeCertStoreHandle, SafeHGlobalHandle.",
            Safe = "Performs a Demand for full trust.")]
        [SecuritySafeCritical]
        [SecurityPermission(SecurityAction.Demand, Unrestricted = true)]
        public X509Certificate2Collection Find(X509FindType findType, object findValue, bool validOnly)
        {
            DiagnosticUtility.DebugAssert(!this.certStoreHandle.IsInvalid, "");

            uint dwFindType;
            SafeHGlobalHandle pvFindPara = SafeHGlobalHandle.InvalidHandle;
            SafeCertContextHandle pCertContext = SafeCertContextHandle.InvalidHandle;
            X509Certificate2Collection result = new X509Certificate2Collection();
            SafeHGlobalHandle pvTemp = SafeHGlobalHandle.InvalidHandle;
            string strFindValue;
            byte[] bytes;

            try
            {
                switch (findType)
                {
                    case X509FindType.FindBySubjectName:
                        strFindValue = findValue as string;
                        if (strFindValue == null)
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(SR.GetString(SR.X509FindValueMismatch, findType, typeof(string), findValue.GetType())));

                        dwFindType = CAPI.CERT_FIND_SUBJECT_STR;
                        pvFindPara = SafeHGlobalHandle.AllocHGlobal(strFindValue);
                        break;

                    case X509FindType.FindByThumbprint:
                        bytes = findValue as byte[];
                        if (bytes == null)
                        {
                            strFindValue = findValue as string;
                            if (strFindValue == null)
                                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(SR.GetString(SR.X509FindValueMismatchMulti, findType, typeof(string), typeof(byte[]), findValue.GetType())));

                            bytes = SecurityUtils.DecodeHexString(strFindValue);
                        }

                        CAPI.CRYPTOAPI_BLOB blob = new CAPI.CRYPTOAPI_BLOB();
                        pvTemp = SafeHGlobalHandle.AllocHGlobal(bytes);
                        blob.pbData = pvTemp.DangerousGetHandle();
                        blob.cbData = (uint)bytes.Length;
                        dwFindType = CAPI.CERT_FIND_HASH;
                        pvFindPara = SafeHGlobalHandle.AllocHGlobal(CAPI.CRYPTOAPI_BLOB.Size);
                        Marshal.StructureToPtr(blob, pvFindPara.DangerousGetHandle(), false);
                        break;

                    case X509FindType.FindBySubjectDistinguishedName:
                        if (!(findValue is string))
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(SR.GetString(SR.X509FindValueMismatch, findType, typeof(string), findValue.GetType())));

                        dwFindType = CAPI.CERT_FIND_ANY;
                        break;

                    case X509FindType.FindByIssuerName:
                        strFindValue = findValue as string;
                        if (strFindValue == null)
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(SR.GetString(SR.X509FindValueMismatch, findType, typeof(string), findValue.GetType())));

                        dwFindType = CAPI.CERT_FIND_ISSUER_STR;
                        pvFindPara = SafeHGlobalHandle.AllocHGlobal(strFindValue);
                        break;

                    case X509FindType.FindByIssuerDistinguishedName:
                        if (!(findValue is string))
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(SR.GetString(SR.X509FindValueMismatch, findType, typeof(string), findValue.GetType())));

                        dwFindType = CAPI.CERT_FIND_ANY;
                        break;

                    case X509FindType.FindBySerialNumber:
                        bytes = findValue as byte[];
                        if (bytes == null)
                        {
                            strFindValue = findValue as string;
                            if (strFindValue == null)
                                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(SR.GetString(SR.X509FindValueMismatchMulti, findType, typeof(string), typeof(byte[]), findValue.GetType())));

                            bytes = SecurityUtils.DecodeHexString(strFindValue);

                            // reverse bits
                            int len = bytes.Length;
                            for (int i = 0, j = len - 1; i < bytes.Length / 2; ++i, --j)
                            {
                                byte tmp = bytes[i];
                                bytes[i] = bytes[j];
                                bytes[j] = tmp;
                            }
                        }
                        findValue = bytes;
                        dwFindType = CAPI.CERT_FIND_ANY;
                        break;

                    case X509FindType.FindBySubjectKeyIdentifier:
                        bytes = findValue as byte[];
                        if (bytes == null)
                        {
                            strFindValue = findValue as string;
                            if (strFindValue == null)
                                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(SR.GetString(SR.X509FindValueMismatchMulti, findType, typeof(string), typeof(byte[]), findValue.GetType())));

                            bytes = SecurityUtils.DecodeHexString(strFindValue);

                        }
                        findValue = bytes;
                        dwFindType = CAPI.CERT_FIND_ANY;
                        break;

                    default:
                        // Fallback to CLR implementation
                        X509Store store = new X509Store(this.certStoreHandle.DangerousGetHandle());
                        try
                        {
                            return store.Certificates.Find(findType, findValue, validOnly);
                        }
                        finally
                        {
                            store.Close();
                        }
                }

#pragma warning suppress 56523 // We are not interested in CRYPT_E_NOT_FOUND error, it return null anyway.
                pCertContext = CAPI.CertFindCertificateInStore(this.certStoreHandle,
                                                               CAPI.X509_ASN_ENCODING | CAPI.PKCS_7_ASN_ENCODING,
                                                               0,
                                                               dwFindType,
                                                               pvFindPara,
                                                               pCertContext);

                while (pCertContext != null && !pCertContext.IsInvalid)
                {
                    X509Certificate2 cert;
                    if (TryGetMatchingX509Certificate(pCertContext.DangerousGetHandle(), findType,
                            dwFindType, findValue, validOnly, out cert))
                    {
                        result.Add(cert);
                    }

                    // CER
                    RuntimeHelpers.PrepareConstrainedRegions();
                    try { }
                    finally
                    {
                        // Suppress the finalizer
#pragma warning suppress 56508 // CertFindCertificateInStore will release the prev one.
                        GC.SuppressFinalize(pCertContext);
#pragma warning suppress 56523 // We are not interested in CRYPT_E_NOT_FOUND error, it return null anyway.
                        pCertContext = CAPI.CertFindCertificateInStore(this.certStoreHandle,
                                                                       CAPI.X509_ASN_ENCODING | CAPI.PKCS_7_ASN_ENCODING,
                                                                       0,
                                                                       dwFindType,
                                                                       pvFindPara,
                                                                       pCertContext);
                    }
                }
            }
            finally
            {
                if (pCertContext != null)
                {
                    pCertContext.Close();
                }
                pvFindPara.Close();
                pvTemp.Close();
            }
            return result;
        }

        bool TryGetMatchingX509Certificate(IntPtr certContext, X509FindType findType,
            uint dwFindType, object findValue, bool validOnly, out X509Certificate2 cert)
        {
            cert = new X509Certificate2(certContext);
            if (dwFindType == CAPI.CERT_FIND_ANY)
            {
                switch (findType)
                {
                    case X509FindType.FindBySubjectDistinguishedName:
                        if (0 != String.Compare((string)findValue, cert.SubjectName.Name, StringComparison.OrdinalIgnoreCase))
                        {
                            cert.Reset();
                            cert = null;
                            return false;
                        }
                        break;

                    case X509FindType.FindByIssuerDistinguishedName:
                        if (0 != String.Compare((string)findValue, cert.IssuerName.Name, StringComparison.OrdinalIgnoreCase))
                        {
                            cert.Reset();
                            cert = null;
                            return false;
                        }
                        break;

                    case X509FindType.FindBySerialNumber:
                        if (!BinaryMatches((byte[])findValue, cert.GetSerialNumber()))
                        {
                            cert.Reset();
                            cert = null;
                            return false;
                        }
                        break;

                    case X509FindType.FindBySubjectKeyIdentifier:
                        X509SubjectKeyIdentifierExtension skiExtension =
                            cert.Extensions[CAPI.SubjectKeyIdentifierOid] as X509SubjectKeyIdentifierExtension;
                        if (skiExtension == null || !BinaryMatches((byte[])findValue, skiExtension.RawData))
                        {
                            cert.Reset();
                            cert = null;
                            return false;
                        }
                        break;

                    default:
                        DiagnosticUtility.DebugAssert(findType + " is not supported!");
                        break;
                }
            }

            if (validOnly)
            {
                X509Chain chain = new X509Chain(false);
                chain.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck;
                chain.ChainPolicy.RevocationFlag = X509RevocationFlag.ExcludeRoot;
                if (!chain.Build(cert))
                {
                    cert.Reset();
                    cert = null;
                    return false;
                }
            }
            return cert != null;
        }

        bool BinaryMatches(byte[] src, byte[] dst)
        {
            if (src.Length != dst.Length)
                return false;

            for (int i = 0; i < src.Length; ++i)
            {
                if (src[i] != dst[i])
                    return false;
            }
            return true;
        }

        // this method maps X509Store OpenFlags to a combination of crypto API flags
        uint MapX509StoreFlags(StoreLocation storeLocation, OpenFlags flags)
        {
            uint dwFlags = 0;
            uint openMode = ((uint)flags) & 0x3;
            switch (openMode)
            {
                case (uint)OpenFlags.ReadOnly:
                    dwFlags |= CAPI.CERT_STORE_READONLY_FLAG;
                    break;
                case (uint)OpenFlags.MaxAllowed:
                    dwFlags |= CAPI.CERT_STORE_MAXIMUM_ALLOWED_FLAG;
                    break;
            }

            if ((flags & OpenFlags.OpenExistingOnly) == OpenFlags.OpenExistingOnly)
                dwFlags |= CAPI.CERT_STORE_OPEN_EXISTING_FLAG;
            if ((flags & OpenFlags.IncludeArchived) == OpenFlags.IncludeArchived)
                dwFlags |= CAPI.CERT_STORE_ENUM_ARCHIVED_FLAG;

            if (storeLocation == StoreLocation.LocalMachine)
                dwFlags |= CAPI.CERT_SYSTEM_STORE_LOCAL_MACHINE;
            else if (storeLocation == StoreLocation.CurrentUser)
                dwFlags |= CAPI.CERT_SYSTEM_STORE_CURRENT_USER;

            return dwFlags;
        }
    }
}
