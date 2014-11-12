// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==

//
// X509Chain.cs
//

namespace System.Security.Cryptography.X509Certificates {
    using System.Collections;
    using System.Diagnostics;
    using System.Net;
    using System.Runtime.InteropServices;
    using System.Runtime.InteropServices.ComTypes;
    using System.Security.Cryptography;
    using System.Security.Permissions;
    using System.Text;

    using _FILETIME = System.Runtime.InteropServices.ComTypes.FILETIME;

    [Flags]
    public enum X509ChainStatusFlags {
        NoError                         = 0x00000000,
        NotTimeValid                    = 0x00000001,
        NotTimeNested                   = 0x00000002,
        Revoked                         = 0x00000004,
        NotSignatureValid               = 0x00000008,
        NotValidForUsage                = 0x00000010,
        UntrustedRoot                   = 0x00000020,
        RevocationStatusUnknown         = 0x00000040,
        Cyclic                          = 0x00000080,
        InvalidExtension                = 0x00000100,
        InvalidPolicyConstraints        = 0x00000200,
        InvalidBasicConstraints         = 0x00000400,
        InvalidNameConstraints          = 0x00000800,
        HasNotSupportedNameConstraint   = 0x00001000,
        HasNotDefinedNameConstraint     = 0x00002000,
        HasNotPermittedNameConstraint   = 0x00004000,
        HasExcludedNameConstraint       = 0x00008000,
        PartialChain                    = 0x00010000,
        CtlNotTimeValid                 = 0x00020000,
        CtlNotSignatureValid            = 0x00040000,
        CtlNotValidForUsage             = 0x00080000,
        OfflineRevocation               = 0x01000000,
        NoIssuanceChainPolicy           = 0x02000000
    }

    public struct X509ChainStatus {
        private X509ChainStatusFlags m_status;
        private string m_statusInformation;

        public X509ChainStatusFlags Status {
            get {
                return m_status;
            }
            set {
                m_status = value;
            }
        }

        public string StatusInformation {
            get {
                if (m_statusInformation == null)
                    return String.Empty;
                return m_statusInformation;
            }
            set {
                m_statusInformation = value;
            }
        }
    }

    public class X509Chain {
        private uint m_status;
        private X509ChainPolicy m_chainPolicy;
        private X509ChainStatus[] m_chainStatus;
        private X509ChainElementCollection m_chainElementCollection;
        private SafeCertChainHandle m_safeCertChainHandle;
        private bool m_useMachineContext;
        private readonly object m_syncRoot = new object();

        public static X509Chain Create() {
            return (X509Chain) CryptoConfig.CreateFromName("X509Chain");
        }

        public X509Chain () : this (false) {}

        public X509Chain (bool useMachineContext) {
            m_status = 0;
            m_chainPolicy = null;
            m_chainStatus = null;
            m_chainElementCollection = new X509ChainElementCollection();
            m_safeCertChainHandle = SafeCertChainHandle.InvalidHandle;
            m_useMachineContext = useMachineContext;
        }

        // Package protected constructor for creating a chain from a PCCERT_CHAIN_CONTEXT
        [SecurityPermissionAttribute(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.UnmanagedCode)]
        [SecurityPermissionAttribute(SecurityAction.InheritanceDemand, Flags=SecurityPermissionFlag.UnmanagedCode)]
        public X509Chain (IntPtr chainContext) {
            if (chainContext == IntPtr.Zero)
                throw new ArgumentNullException("chainContext");
            m_safeCertChainHandle = CAPI.CertDuplicateCertificateChain(chainContext);
            if (m_safeCertChainHandle == null || m_safeCertChainHandle == SafeCertChainHandle.InvalidHandle)
                throw new CryptographicException(SR.GetString(SR.Cryptography_InvalidContextHandle), "chainContext");

            Init();
        }

        public IntPtr ChainContext {
            [SecurityPermissionAttribute(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.UnmanagedCode)]
            [SecurityPermissionAttribute(SecurityAction.InheritanceDemand, Flags=SecurityPermissionFlag.UnmanagedCode)]
            get {
                return m_safeCertChainHandle.DangerousGetHandle();
            }
        }

        public X509ChainPolicy ChainPolicy {
            get {
                if (m_chainPolicy == null)
                    m_chainPolicy = new X509ChainPolicy();
                return m_chainPolicy;
            }
            set {
                if (value == null)
                    throw new ArgumentNullException("value");
                m_chainPolicy = value;
            }
        }

        public X509ChainStatus[] ChainStatus {
            get {
                // We give the user a reference to the array since we'll never access it.
                if (m_chainStatus == null) {
                    if (m_status == 0) {
                        m_chainStatus = new X509ChainStatus[0]; // empty array
                    } else {
                        m_chainStatus = GetChainStatusInformation(m_status);
                    }
                }
                return m_chainStatus;
            }
        }

        public X509ChainElementCollection ChainElements {
            get {
                return m_chainElementCollection;
            }
        }

        [PermissionSetAttribute(SecurityAction.LinkDemand, Unrestricted=true)]
        [PermissionSetAttribute(SecurityAction.InheritanceDemand, Unrestricted=true)]
        public bool Build (X509Certificate2 certificate) {
            lock (m_syncRoot) {
                if (certificate == null || certificate.CertContext.IsInvalid)
                    throw new ArgumentException(SR.GetString(SR.Cryptography_InvalidContextHandle), "certificate");

                // Chain building opens and enumerates the root store to see if the root of the chain is trusted.
                StorePermission sp = new StorePermission(StorePermissionFlags.OpenStore | StorePermissionFlags.EnumerateCertificates);
                sp.Demand();

                X509ChainPolicy chainPolicy = this.ChainPolicy;
                if (chainPolicy.RevocationMode == X509RevocationMode.Online) {
                    if (certificate.Extensions[CAPI.szOID_CRL_DIST_POINTS] != null ||
                        certificate.Extensions[CAPI.szOID_AUTHORITY_INFO_ACCESS] != null) {
                        // If there is a CDP or AIA extension, we demand unrestricted network access and store add permission
                        // since CAPI can download certificates into the CA store from the network.
                        PermissionSet ps = new PermissionSet(PermissionState.None);
                        ps.AddPermission(new WebPermission(PermissionState.Unrestricted));
                        ps.AddPermission(new StorePermission(StorePermissionFlags.AddToStore));
                        ps.Demand();
                    }
                }

                Reset();
                int hr = BuildChain(m_useMachineContext ? new IntPtr(CAPI.HCCE_LOCAL_MACHINE) : new IntPtr(CAPI.HCCE_CURRENT_USER),
                                    certificate.CertContext,
                                    chainPolicy.ExtraStore,
                                    chainPolicy.ApplicationPolicy,
                                    chainPolicy.CertificatePolicy,
                                    chainPolicy.RevocationMode,
                                    chainPolicy.RevocationFlag,
                                    chainPolicy.VerificationTime,
                                    chainPolicy.UrlRetrievalTimeout,
                                    ref m_safeCertChainHandle);

                if (hr != CAPI.S_OK)
                    return false;

                // Init.
                Init();

                // Verify the chain using the specified policy.
                CAPI.CERT_CHAIN_POLICY_PARA PolicyPara = new CAPI.CERT_CHAIN_POLICY_PARA(Marshal.SizeOf(typeof(CAPI.CERT_CHAIN_POLICY_PARA)));
                CAPI.CERT_CHAIN_POLICY_STATUS PolicyStatus = new CAPI.CERT_CHAIN_POLICY_STATUS(Marshal.SizeOf(typeof(CAPI.CERT_CHAIN_POLICY_STATUS)));

                PolicyPara.dwFlags = (uint) chainPolicy.VerificationFlags;

                if (!CAPI.CertVerifyCertificateChainPolicy(new IntPtr(CAPI.CERT_CHAIN_POLICY_BASE),
                                                           m_safeCertChainHandle,
                                                           ref PolicyPara,
                                                           ref PolicyStatus))
                    // The API failed.
                    throw new CryptographicException(Marshal.GetLastWin32Error());

                CAPI.SetLastError(PolicyStatus.dwError);
                return (PolicyStatus.dwError == 0);
            }
        }

        [PermissionSetAttribute(SecurityAction.LinkDemand, Unrestricted=true)]
        [PermissionSetAttribute(SecurityAction.InheritanceDemand, Unrestricted=true)]
        public void Reset () {
            m_status = 0;
            m_chainStatus = null;
            m_chainElementCollection = new X509ChainElementCollection();
            if (!m_safeCertChainHandle.IsInvalid) {
                m_safeCertChainHandle.Dispose();
                m_safeCertChainHandle = SafeCertChainHandle.InvalidHandle;
            }
        }

        private unsafe void Init () {
            using (SafeCertChainHandle safeCertChainHandle = CAPI.CertDuplicateCertificateChain(m_safeCertChainHandle)) {
                CAPI.CERT_CHAIN_CONTEXT pChain = new CAPI.CERT_CHAIN_CONTEXT(Marshal.SizeOf(typeof(CAPI.CERT_CHAIN_CONTEXT)));
                uint cbSize = (uint) Marshal.ReadInt32(safeCertChainHandle.DangerousGetHandle());
                if (cbSize > Marshal.SizeOf(pChain))
                    cbSize = (uint) Marshal.SizeOf(pChain);

                X509Utils.memcpy(m_safeCertChainHandle.DangerousGetHandle(), new IntPtr(&pChain), cbSize);

                m_status = pChain.dwErrorStatus;
                Debug.Assert(pChain.cChain > 0);
                m_chainElementCollection = new X509ChainElementCollection(Marshal.ReadIntPtr(pChain.rgpChain));
            }
        }

        internal static X509ChainStatus[] GetChainStatusInformation (uint dwStatus) {
            if (dwStatus == 0)
                return new X509ChainStatus[0];

            int count = 0;
            for (uint bits = dwStatus; bits != 0; bits = bits >> 1) {
                if ((bits & 0x1) != 0)
                    count++;
            }

            X509ChainStatus[] chainStatus = new X509ChainStatus[count];

            int index = 0;
            if ((dwStatus & CAPI.CERT_TRUST_IS_NOT_SIGNATURE_VALID) != 0) {
                chainStatus[index].StatusInformation = X509Utils.GetSystemErrorString(CAPI.TRUST_E_CERT_SIGNATURE);
                chainStatus[index].Status = X509ChainStatusFlags.NotSignatureValid;
                index++;
                dwStatus &= ~CAPI.CERT_TRUST_IS_NOT_SIGNATURE_VALID;
            }

            if ((dwStatus & CAPI.CERT_TRUST_CTL_IS_NOT_SIGNATURE_VALID) != 0) {
                chainStatus[index].StatusInformation = X509Utils.GetSystemErrorString(CAPI.TRUST_E_CERT_SIGNATURE);
                chainStatus[index].Status = X509ChainStatusFlags.CtlNotSignatureValid;
                index++;
                dwStatus &= ~CAPI.CERT_TRUST_CTL_IS_NOT_SIGNATURE_VALID;
            }

            if ((dwStatus & CAPI.CERT_TRUST_IS_UNTRUSTED_ROOT) != 0) {
                chainStatus[index].StatusInformation = X509Utils.GetSystemErrorString(CAPI.CERT_E_UNTRUSTEDROOT);
                chainStatus[index].Status = X509ChainStatusFlags.UntrustedRoot;
                index++;
                dwStatus &= ~CAPI.CERT_TRUST_IS_UNTRUSTED_ROOT;
            }

            if ((dwStatus & CAPI.CERT_TRUST_IS_PARTIAL_CHAIN) != 0) {
                chainStatus[index].StatusInformation = X509Utils.GetSystemErrorString(CAPI.CERT_E_CHAINING);
                chainStatus[index].Status = X509ChainStatusFlags.PartialChain;
                index++;
                dwStatus &= ~CAPI.CERT_TRUST_IS_PARTIAL_CHAIN;
            }

            if ((dwStatus & CAPI.CERT_TRUST_IS_REVOKED) != 0) {
                chainStatus[index].StatusInformation = X509Utils.GetSystemErrorString(CAPI.CRYPT_E_REVOKED);
                chainStatus[index].Status = X509ChainStatusFlags.Revoked;
                index++;
                dwStatus &= ~CAPI.CERT_TRUST_IS_REVOKED;
            }

            if ((dwStatus & CAPI.CERT_TRUST_IS_NOT_VALID_FOR_USAGE) != 0) {
                chainStatus[index].StatusInformation = X509Utils.GetSystemErrorString(CAPI.CERT_E_WRONG_USAGE);
                chainStatus[index].Status = X509ChainStatusFlags.NotValidForUsage;
                index++;
                dwStatus &= ~CAPI.CERT_TRUST_IS_NOT_VALID_FOR_USAGE; 
            }

            if ((dwStatus & CAPI.CERT_TRUST_CTL_IS_NOT_VALID_FOR_USAGE) != 0) {
                chainStatus[index].StatusInformation = X509Utils.GetSystemErrorString(CAPI.CERT_E_WRONG_USAGE);
                chainStatus[index].Status = X509ChainStatusFlags.CtlNotValidForUsage;
                index++;
                dwStatus &= ~CAPI.CERT_TRUST_CTL_IS_NOT_VALID_FOR_USAGE;
            }

            if ((dwStatus & CAPI.CERT_TRUST_IS_NOT_TIME_VALID) != 0) {
                chainStatus[index].StatusInformation = X509Utils.GetSystemErrorString(CAPI.CERT_E_EXPIRED);
                chainStatus[index].Status = X509ChainStatusFlags.NotTimeValid;
                index++;
                dwStatus &= ~CAPI.CERT_TRUST_IS_NOT_TIME_VALID;
            }

            if ((dwStatus & CAPI.CERT_TRUST_CTL_IS_NOT_TIME_VALID) != 0) {
                chainStatus[index].StatusInformation = X509Utils.GetSystemErrorString(CAPI.CERT_E_EXPIRED);
                chainStatus[index].Status = X509ChainStatusFlags.CtlNotTimeValid;
                index++;
                dwStatus &= ~CAPI.CERT_TRUST_CTL_IS_NOT_TIME_VALID;
            }

            if ((dwStatus & CAPI.CERT_TRUST_INVALID_NAME_CONSTRAINTS) != 0) {
                chainStatus[index].StatusInformation = X509Utils.GetSystemErrorString(CAPI.CERT_E_INVALID_NAME);
                chainStatus[index].Status = X509ChainStatusFlags.InvalidNameConstraints;
                index++;
                dwStatus &= ~CAPI.CERT_TRUST_INVALID_NAME_CONSTRAINTS;
            }

            if ((dwStatus & CAPI.CERT_TRUST_HAS_NOT_SUPPORTED_NAME_CONSTRAINT) != 0) {
                chainStatus[index].StatusInformation = X509Utils.GetSystemErrorString(CAPI.CERT_E_INVALID_NAME);
                chainStatus[index].Status = X509ChainStatusFlags.HasNotSupportedNameConstraint;
                index++;
                dwStatus &= ~CAPI.CERT_TRUST_HAS_NOT_SUPPORTED_NAME_CONSTRAINT;
            }

            if ((dwStatus & CAPI.CERT_TRUST_HAS_NOT_DEFINED_NAME_CONSTRAINT) != 0) {
                chainStatus[index].StatusInformation = X509Utils.GetSystemErrorString(CAPI.CERT_E_INVALID_NAME);
                chainStatus[index].Status = X509ChainStatusFlags.HasNotDefinedNameConstraint;
                index++;
                dwStatus &= ~CAPI.CERT_TRUST_HAS_NOT_DEFINED_NAME_CONSTRAINT;
            }

            if ((dwStatus & CAPI.CERT_TRUST_HAS_NOT_PERMITTED_NAME_CONSTRAINT) != 0) {
                chainStatus[index].StatusInformation = X509Utils.GetSystemErrorString(CAPI.CERT_E_INVALID_NAME);
                chainStatus[index].Status = X509ChainStatusFlags.HasNotPermittedNameConstraint;
                index++;
                dwStatus &= ~CAPI.CERT_TRUST_HAS_NOT_PERMITTED_NAME_CONSTRAINT;
            }

            if ((dwStatus & CAPI.CERT_TRUST_HAS_EXCLUDED_NAME_CONSTRAINT) != 0) {
                chainStatus[index].StatusInformation = X509Utils.GetSystemErrorString(CAPI.CERT_E_INVALID_NAME);
                chainStatus[index].Status = X509ChainStatusFlags.HasExcludedNameConstraint;
                index++;
                dwStatus &= ~CAPI.CERT_TRUST_HAS_EXCLUDED_NAME_CONSTRAINT;
            }

            if ((dwStatus & CAPI.CERT_TRUST_INVALID_POLICY_CONSTRAINTS) != 0) {
                chainStatus[index].StatusInformation = X509Utils.GetSystemErrorString(CAPI.CERT_E_INVALID_POLICY);
                chainStatus[index].Status = X509ChainStatusFlags.InvalidPolicyConstraints;
                index++;
                dwStatus &= ~CAPI.CERT_TRUST_INVALID_POLICY_CONSTRAINTS;
            }

            if ((dwStatus & CAPI.CERT_TRUST_NO_ISSUANCE_CHAIN_POLICY) != 0) {
                chainStatus[index].StatusInformation = X509Utils.GetSystemErrorString(CAPI.CERT_E_INVALID_POLICY);
                chainStatus[index].Status = X509ChainStatusFlags.NoIssuanceChainPolicy;
                index++;
                dwStatus &= ~CAPI.CERT_TRUST_NO_ISSUANCE_CHAIN_POLICY;
            }

            if ((dwStatus & CAPI.CERT_TRUST_INVALID_BASIC_CONSTRAINTS) != 0) {
                chainStatus[index].StatusInformation = X509Utils.GetSystemErrorString(CAPI.TRUST_E_BASIC_CONSTRAINTS);
                chainStatus[index].Status = X509ChainStatusFlags.InvalidBasicConstraints;
                index++;
                dwStatus &= ~CAPI.CERT_TRUST_INVALID_BASIC_CONSTRAINTS;
            }

            if ((dwStatus & CAPI.CERT_TRUST_IS_NOT_TIME_NESTED) != 0) {
                chainStatus[index].StatusInformation = X509Utils.GetSystemErrorString(CAPI.CERT_E_VALIDITYPERIODNESTING);
                chainStatus[index].Status = X509ChainStatusFlags.NotTimeNested;
                index++;
                dwStatus &= ~CAPI.CERT_TRUST_IS_NOT_TIME_NESTED;
            }

            if ((dwStatus & CAPI.CERT_TRUST_REVOCATION_STATUS_UNKNOWN) != 0) {
                chainStatus[index].StatusInformation = X509Utils.GetSystemErrorString(CAPI.CRYPT_E_NO_REVOCATION_CHECK);
                chainStatus[index].Status = X509ChainStatusFlags.RevocationStatusUnknown;
                index++;
                dwStatus &= ~CAPI.CERT_TRUST_REVOCATION_STATUS_UNKNOWN;
            }

            if ((dwStatus & CAPI.CERT_TRUST_IS_OFFLINE_REVOCATION) != 0) {
                chainStatus[index].StatusInformation = X509Utils.GetSystemErrorString(CAPI.CRYPT_E_REVOCATION_OFFLINE);
                chainStatus[index].Status = X509ChainStatusFlags.OfflineRevocation;
                index++;
                dwStatus &= ~CAPI.CERT_TRUST_IS_OFFLINE_REVOCATION;
            }

            int shiftCount = 0;
            for (uint bits = dwStatus; bits != 0; bits = bits >> 1) {
                if ((bits & 0x1) != 0) {
                    chainStatus[index].Status = (X509ChainStatusFlags) (1 << shiftCount);
                    chainStatus[index].StatusInformation = SR.GetString(SR.Unknown_Error);
                    index++;
                }
                shiftCount++;
            }

            return chainStatus;
        }

        //
        // Builds a certificate chain.
        //

        internal static unsafe int BuildChain (IntPtr hChainEngine,
                                               SafeCertContextHandle pCertContext,
                                               X509Certificate2Collection extraStore, 
                                               OidCollection applicationPolicy,
                                               OidCollection certificatePolicy,
                                               X509RevocationMode revocationMode,
                                               X509RevocationFlag revocationFlag,
                                               DateTime verificationTime,
                                               TimeSpan timeout,
                                               ref SafeCertChainHandle ppChainContext) {
            if (pCertContext == null || pCertContext.IsInvalid)
                throw new ArgumentException(SR.GetString(SR.Cryptography_InvalidContextHandle), "pCertContext");

            SafeCertStoreHandle hCertStore = SafeCertStoreHandle.InvalidHandle;
            if (extraStore != null && extraStore.Count > 0)
                hCertStore = X509Utils.ExportToMemoryStore(extraStore);

            CAPI.CERT_CHAIN_PARA ChainPara = new CAPI.CERT_CHAIN_PARA();

            // Initialize the structure size.
            ChainPara.cbSize = (uint) Marshal.SizeOf(ChainPara);

            SafeLocalAllocHandle applicationPolicyHandle = SafeLocalAllocHandle.InvalidHandle;
            SafeLocalAllocHandle certificatePolicyHandle = SafeLocalAllocHandle.InvalidHandle;
            try {
                // Application policy
                if (applicationPolicy != null && applicationPolicy.Count > 0) {
                    ChainPara.RequestedUsage.dwType = CAPI.USAGE_MATCH_TYPE_AND;
                    ChainPara.RequestedUsage.Usage.cUsageIdentifier = (uint) applicationPolicy.Count;
                    applicationPolicyHandle = X509Utils.CopyOidsToUnmanagedMemory(applicationPolicy);
                    ChainPara.RequestedUsage.Usage.rgpszUsageIdentifier = applicationPolicyHandle.DangerousGetHandle();
                }

                // Certificate policy
                if (certificatePolicy != null && certificatePolicy.Count > 0) {
                    ChainPara.RequestedIssuancePolicy.dwType = CAPI.USAGE_MATCH_TYPE_AND;
                    ChainPara.RequestedIssuancePolicy.Usage.cUsageIdentifier = (uint) certificatePolicy.Count;
                    certificatePolicyHandle = X509Utils.CopyOidsToUnmanagedMemory(certificatePolicy);
                    ChainPara.RequestedIssuancePolicy.Usage.rgpszUsageIdentifier = certificatePolicyHandle.DangerousGetHandle();
                }

                ChainPara.dwUrlRetrievalTimeout = (uint) Math.Floor(timeout.TotalMilliseconds);

                _FILETIME ft = new _FILETIME();
                *((long*) &ft) = verificationTime.ToFileTime();

                uint flags = X509Utils.MapRevocationFlags(revocationMode, revocationFlag);

                // Build the chain.
                if (!CAPI.CertGetCertificateChain(hChainEngine,
                                                  pCertContext,
                                                  ref ft,
                                                  hCertStore,
                                                  ref ChainPara,
                                                  flags,
                                                  IntPtr.Zero,
                                                  ref ppChainContext))
                    return Marshal.GetHRForLastWin32Error();
            }
            finally {
                applicationPolicyHandle.Dispose();
                certificatePolicyHandle.Dispose();
            }

            return CAPI.S_OK;
        }
    }
}
