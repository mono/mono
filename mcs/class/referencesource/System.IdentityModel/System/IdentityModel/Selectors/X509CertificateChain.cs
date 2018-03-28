//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.IdentityModel.Selectors
{
    using System.Collections.Generic;
    using System.IdentityModel.Tokens;
    using System.Net;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Cryptography;
    using System.Security.Cryptography.X509Certificates;
    using System.Security.Permissions;
    using System.Text;
    using FILETIME = System.Runtime.InteropServices.ComTypes.FILETIME;

    // Most of codes are copied from \ndp\fx\src\security\system\security\cryptography\x509\X509Chain.cs
    class X509CertificateChain
    {
        public const uint DefaultChainPolicyOID = CAPI.CERT_CHAIN_POLICY_BASE;
        bool useMachineContext;
        X509ChainPolicy chainPolicy;
        uint chainPolicyOID = X509CertificateChain.DefaultChainPolicyOID;

        public X509CertificateChain()
            : this(false)
        {
        }

        public X509CertificateChain(bool useMachineContext)
        {
            this.useMachineContext = useMachineContext;
        }

        public X509CertificateChain(bool useMachineContext, uint chainPolicyOID)
        {
            this.useMachineContext = useMachineContext;
            // One of the condition to pass NT_AUTH is the issuer of the cert must be trusted by NT auth.
            // Simply add to HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\EnterpriseCertificates\NTAuth\Certificates
            this.chainPolicyOID = chainPolicyOID;
        }

        public X509ChainPolicy ChainPolicy
        {
            get
            {
                if (this.chainPolicy == null)
                {
                    this.chainPolicy = new X509ChainPolicy();
                }
                return this.chainPolicy;
            }
            set
            {
                this.chainPolicy = value;
            }
        }

        public X509ChainStatus[] ChainStatus
        {
#pragma warning suppress 56503 
            get { throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException()); }
        }

        // There are 2 steps in chain validation.
        // 1) BuildChain by calling CAPI.CertGetCertificateChain.  The result is
        // the chain context containing the chain and status.  
        // 2) VerifyChain by calling CAPI.CertVerifyCertificateChainPolicy.
        // Refer to MB50916, Since Vista out-of-the-box will trust the chain with PeerTrust, 
        // we include the flag to ignore PeerTrust for CAPI.CertVerifyCertificateChainPolicy.
        [Fx.Tag.SecurityNote(Critical = "Builds chain trust through interop calls.",
            Safe = "Proteced by StorePermission and WebPermission demands.")]
        [SecuritySafeCritical]
        [StorePermission(SecurityAction.Demand, CreateStore = true, OpenStore = true, EnumerateCertificates = true)]
        public bool Build(X509Certificate2 certificate)
        {
            if (certificate == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("certificate");
            if (certificate.Handle == IntPtr.Zero)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("certificate", SR.GetString(SR.ArgumentInvalidCertificate));

            SafeCertChainHandle safeCertChainHandle = SafeCertChainHandle.InvalidHandle;
            X509ChainPolicy chainPolicy = this.ChainPolicy;
            chainPolicy.VerificationTime = DateTime.Now;
            if (chainPolicy.RevocationMode == X509RevocationMode.Online)
            {
                if (certificate.Extensions[CAPI.szOID_CRL_DIST_POINTS] != null ||
                    certificate.Extensions[CAPI.szOID_AUTHORITY_INFO_ACCESS] != null)
                {
                    // If there is a CDP or AIA extension, we demand unrestricted network access and store add permission
                    // since CAPI can download certificates into the CA store from the network.
                    PermissionSet ps = new PermissionSet(PermissionState.None);
                    ps.AddPermission(new WebPermission(PermissionState.Unrestricted));
                    ps.AddPermission(new StorePermission(StorePermissionFlags.AddToStore));
                    ps.Demand();
                }
            }

            BuildChain(this.useMachineContext ? new IntPtr(CAPI.HCCE_LOCAL_MACHINE) : new IntPtr(CAPI.HCCE_CURRENT_USER),
                    certificate.Handle,
                    chainPolicy.ExtraStore,
                    chainPolicy.ApplicationPolicy,
                    chainPolicy.CertificatePolicy,
                    chainPolicy.RevocationMode,
                    chainPolicy.RevocationFlag,
                    chainPolicy.VerificationTime,
                    chainPolicy.UrlRetrievalTimeout,
                    out safeCertChainHandle);

            // Verify the chain using the specified policy.
            CAPI.CERT_CHAIN_POLICY_PARA PolicyPara = new CAPI.CERT_CHAIN_POLICY_PARA(Marshal.SizeOf(typeof(CAPI.CERT_CHAIN_POLICY_PARA)));
            CAPI.CERT_CHAIN_POLICY_STATUS PolicyStatus = new CAPI.CERT_CHAIN_POLICY_STATUS(Marshal.SizeOf(typeof(CAPI.CERT_CHAIN_POLICY_STATUS)));

            // Ignore peertrust.  Peer trust caused the chain to succeed out-of-the-box in Vista.
            // This new flag is only available in Vista.
            PolicyPara.dwFlags = (uint)chainPolicy.VerificationFlags | CAPI.CERT_CHAIN_POLICY_IGNORE_PEER_TRUST_FLAG;

            if (!CAPI.CertVerifyCertificateChainPolicy(new IntPtr(this.chainPolicyOID),
                                                       safeCertChainHandle,
                                                       ref PolicyPara,
                                                       ref PolicyStatus))
            {
                int error = Marshal.GetLastWin32Error();
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new CryptographicException(error));
            }

            if (PolicyStatus.dwError != CAPI.S_OK)
            {
                int error = (int)PolicyStatus.dwError;
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenValidationException(SR.GetString(SR.X509ChainBuildFail,
                    SecurityUtils.GetCertificateId(certificate), new CryptographicException(error).Message)));
            }

            return true;
        }

        [SecurityCritical]
        static unsafe void BuildChain(IntPtr hChainEngine,
                                     IntPtr pCertContext,
                                     X509Certificate2Collection extraStore,
                                     OidCollection applicationPolicy,
                                     OidCollection certificatePolicy,
                                     X509RevocationMode revocationMode,
                                     X509RevocationFlag revocationFlag,
                                     DateTime verificationTime,
                                     TimeSpan timeout,
                                     out SafeCertChainHandle ppChainContext)
        {
            SafeCertStoreHandle hCertStore = ExportToMemoryStore(extraStore, pCertContext);

            CAPI.CERT_CHAIN_PARA ChainPara = new CAPI.CERT_CHAIN_PARA();
            ChainPara.cbSize = (uint)Marshal.SizeOf(typeof(CAPI.CERT_CHAIN_PARA));

            // Application policy
            SafeHGlobalHandle applicationPolicyHandle = SafeHGlobalHandle.InvalidHandle;
            SafeHGlobalHandle certificatePolicyHandle = SafeHGlobalHandle.InvalidHandle;
            try
            {
                if (applicationPolicy != null && applicationPolicy.Count > 0)
                {
                    ChainPara.RequestedUsage.dwType = CAPI.USAGE_MATCH_TYPE_AND;
                    ChainPara.RequestedUsage.Usage.cUsageIdentifier = (uint)applicationPolicy.Count;
                    applicationPolicyHandle = CopyOidsToUnmanagedMemory(applicationPolicy);
                    ChainPara.RequestedUsage.Usage.rgpszUsageIdentifier = applicationPolicyHandle.DangerousGetHandle();
                }

                // Certificate policy
                if (certificatePolicy != null && certificatePolicy.Count > 0)
                {
                    ChainPara.RequestedIssuancePolicy.dwType = CAPI.USAGE_MATCH_TYPE_AND;
                    ChainPara.RequestedIssuancePolicy.Usage.cUsageIdentifier = (uint)certificatePolicy.Count;
                    certificatePolicyHandle = CopyOidsToUnmanagedMemory(certificatePolicy);
                    ChainPara.RequestedIssuancePolicy.Usage.rgpszUsageIdentifier = certificatePolicyHandle.DangerousGetHandle();
                }

                ChainPara.dwUrlRetrievalTimeout = (uint)timeout.Milliseconds;

                FILETIME ft = new FILETIME();
                *((long*)&ft) = verificationTime.ToFileTime();

                uint flags = MapRevocationFlags(revocationMode, revocationFlag);

                // Build the chain.
                if (!CAPI.CertGetCertificateChain(hChainEngine,
                                                  pCertContext,
                                                  ref ft,
                                                  hCertStore,
                                                  ref ChainPara,
                                                  flags,
                                                  IntPtr.Zero,
                                                  out ppChainContext))
                {
                    int error = Marshal.GetLastWin32Error();
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new CryptographicException(error));
                }
            }
            finally
            {
                if (applicationPolicyHandle != null)
                    applicationPolicyHandle.Dispose();
                if (certificatePolicyHandle != null)
                    certificatePolicyHandle.Dispose();
                hCertStore.Close();
            }
        }
        
        [Fx.Tag.SecurityNote(Critical = "Uses unmanaged code to create an in memory store which links to the original cert store."
            + "User must protect the store handle.")]
        [SecurityCritical]
        static SafeCertStoreHandle ExportToMemoryStore(X509Certificate2Collection collection, IntPtr pCertContext)
        {
            CAPI.CERT_CONTEXT certContext = (CAPI.CERT_CONTEXT)Marshal.PtrToStructure(pCertContext, typeof(CAPI.CERT_CONTEXT));

            // No extra store nor intermediate certificates
            if ((collection == null || collection.Count <= 0) && certContext.hCertStore == IntPtr.Zero)
            {
                return SafeCertStoreHandle.InvalidHandle;
            }

            // we always want to use CERT_STORE_ENUM_ARCHIVED_FLAG since we want to preserve the collection in this operation.
            // By default, Archived certificates will not be included.
            SafeCertStoreHandle certStoreHandle = CAPI.CertOpenStore(
                                                     new IntPtr(CAPI.CERT_STORE_PROV_MEMORY),
                                                     CAPI.X509_ASN_ENCODING | CAPI.PKCS_7_ASN_ENCODING,
                                                     IntPtr.Zero,
                                                     CAPI.CERT_STORE_ENUM_ARCHIVED_FLAG | CAPI.CERT_STORE_CREATE_NEW_FLAG,
                                                     null);

            if (certStoreHandle == null || certStoreHandle.IsInvalid)
            {
                int error = Marshal.GetLastWin32Error();
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new CryptographicException(error));
            }

            //
            // We use CertAddCertificateLinkToStore to keep a link to the original store, so any property changes get
            // applied to the original store. This has a limit of 99 links per cert context however.
            //

            // Add extra store
            if (collection != null && collection.Count > 0)
            {
                foreach (X509Certificate2 x509 in collection)
                {
                    if (!CAPI.CertAddCertificateLinkToStore(certStoreHandle,
                                                            x509.Handle,
                                                            CAPI.CERT_STORE_ADD_ALWAYS,
                                                            SafeCertContextHandle.InvalidHandle))
                    {
                        int error = Marshal.GetLastWin32Error();
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new CryptographicException(error));
                    }
                }
            }

            // Add intermediates
            // The hCertStore needs to be acquired from an X509Certificate2 object
            // constructed using a fresh cert context handle. If we simply refer to the hCertStore
            // property of the certContext local variable directly, there is a risk that we are accessing
            // a closed store. This is because if the X509Certificate2(rawdata) constructor closes the store handle (hCertStore).  
            // There is no way to know which constructor was used at this point.
            //
            using ( SafeCertContextHandle safeCertContext 
                = CAPI.CertCreateCertificateContext( certContext.dwCertEncodingType, 
                                                     certContext.pbCertEncoded, 
                                                     certContext.cbCertEncoded ) )
            {   
                //
                // Create an X509Certificate2 using the new cert context that dup's the provided certificate.
                //
                X509Certificate2 intermediatesCert = new X509Certificate2( safeCertContext.DangerousGetHandle() );

                //
                // Dereference the handle to this intermediate cert and use it to access the handle
                // of this certificate's cert store. Then, call CAPI.CertAddCertificateLinkToStore
                // on each cert in this store by wrapping this cert store handle with an X509Store
                // object.
                //
                CAPI.CERT_CONTEXT intermediatesCertContext = (CAPI.CERT_CONTEXT) Marshal.PtrToStructure( intermediatesCert.Handle, typeof( CAPI.CERT_CONTEXT ) );
                if (intermediatesCertContext.hCertStore != IntPtr.Zero)
                {
                    X509Certificate2Collection intermediates = null;
                    X509Store store = new X509Store(intermediatesCertContext.hCertStore);

                    try
                    {
                        intermediates = store.Certificates;
                        foreach (X509Certificate2 x509 in intermediates)
                        {
                            if (!CAPI.CertAddCertificateLinkToStore(certStoreHandle,
                                                                    x509.Handle,
                                                                    CAPI.CERT_STORE_ADD_ALWAYS,
                                                                    SafeCertContextHandle.InvalidHandle))
                            {
                                int error = Marshal.GetLastWin32Error();
                                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new CryptographicException(error));
                            }
                        }
                    }
                    finally
                    {
                        SecurityUtils.ResetAllCertificates(intermediates);
                        store.Close();
                    }
                }
            }

            return certStoreHandle;
        }

        [Fx.Tag.SecurityNote(Critical = "Copies the oid collection to unamanged memory."
            + "User must protect the handle.")]
        [SecurityCritical]
        static SafeHGlobalHandle CopyOidsToUnmanagedMemory(OidCollection oids)
        {
            SafeHGlobalHandle safeAllocHandle = SafeHGlobalHandle.InvalidHandle;
            if (oids == null || oids.Count == 0)
                return safeAllocHandle;

            // Copy the oid strings to a local list to prevent a security race condition where
            // the OidCollection or individual oids can be modified by another thread and
            // potentially cause a buffer overflow
            List<string> oidStrs = new List<string>();
            foreach (Oid oid in oids) {
                oidStrs.Add(oid.Value);
            }

            IntPtr pOid = IntPtr.Zero;
            IntPtr pNullTerminator = IntPtr.Zero;
            // Needs to be checked to avoid having large sets of oids overflow the sizes and allow
            // a potential buffer overflow
            checked {
                int ptrSize = oidStrs.Count * Marshal.SizeOf(typeof(IntPtr));
                int oidSize = 0;
                foreach (string oidStr in oidStrs) {
                    oidSize += (oidStr.Length + 1);
                }
                safeAllocHandle = SafeHGlobalHandle.AllocHGlobal(ptrSize + oidSize);
                pOid = new IntPtr((long)safeAllocHandle.DangerousGetHandle() + ptrSize);
            }

            for (int index = 0; index < oidStrs.Count; index++) {
                Marshal.WriteIntPtr(new IntPtr((long) safeAllocHandle.DangerousGetHandle() + index * Marshal.SizeOf(typeof(IntPtr))), pOid);
                byte[] ansiOid = Encoding.ASCII.GetBytes(oidStrs[index]);

                if (ansiOid.Length != oidStrs[index].Length) {
                    // We assumed single byte characters, fail if this is not the case.  The exception is not ideal.
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.CollectionWasModified)));
                }

                Marshal.Copy(ansiOid, 0, pOid, ansiOid.Length);
                pNullTerminator = new IntPtr((long) pOid + ansiOid.Length);
                Marshal.WriteByte(pNullTerminator, 0);

                pOid = new IntPtr((long)pOid + oidStrs[index].Length + 1);
            }
            return safeAllocHandle;
        }

        static uint MapRevocationFlags(X509RevocationMode revocationMode, X509RevocationFlag revocationFlag)
        {
            uint dwFlags = 0;
            if (revocationMode == X509RevocationMode.NoCheck)
                return dwFlags;

            if (revocationMode == X509RevocationMode.Offline)
                dwFlags |= CAPI.CERT_CHAIN_REVOCATION_CHECK_CACHE_ONLY;

            if (revocationFlag == X509RevocationFlag.EndCertificateOnly)
                dwFlags |= CAPI.CERT_CHAIN_REVOCATION_CHECK_END_CERT;
            else if (revocationFlag == X509RevocationFlag.EntireChain)
                dwFlags |= CAPI.CERT_CHAIN_REVOCATION_CHECK_CHAIN;
            else
                dwFlags |= CAPI.CERT_CHAIN_REVOCATION_CHECK_CHAIN_EXCLUDE_ROOT;

            return dwFlags;
        }
    }
}
