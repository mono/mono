// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
// <OWNER>Microsoft</OWNER>
// 

//
// RNGCryptoServiceProvider.cs
//

namespace System.Security.Cryptography {
    using Microsoft.Win32;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Runtime.Versioning;
    using System.Diagnostics.Contracts;

#if !FEATURE_CORECLR
[System.Runtime.InteropServices.ComVisible(true)]
#endif // !FEATURE_CORECLR
    public sealed class RNGCryptoServiceProvider : RandomNumberGenerator {
#if !FEATURE_CORECLR 
        [System.Security.SecurityCritical] // auto-generated
        SafeProvHandle m_safeProvHandle;
        bool m_ownsHandle;
#else // !FEATURE_CORECLR
        #if FEATURE_CORECLR
        [System.Security.SecurityCritical] // auto-generated
        #endif
        SafeCspHandle m_cspHandle;
#endif // !FEATURE_CORECLR

        //
        // public constructors
        //

#if !FEATURE_CORECLR

#if !FEATURE_PAL
        public RNGCryptoServiceProvider() : this((CspParameters) null) {}
#else // !FEATURE_PAL 
        public RNGCryptoServiceProvider() { }
#endif // !FEATURE_PAL

#if !FEATURE_PAL
        public RNGCryptoServiceProvider(string str) : this((CspParameters) null) {}

        public RNGCryptoServiceProvider(byte[] rgb) : this((CspParameters) null) {}

        [System.Security.SecuritySafeCritical]  // auto-generated
        public RNGCryptoServiceProvider(CspParameters cspParams) {
            if (cspParams != null) {
                m_safeProvHandle = Utils.AcquireProvHandle(cspParams);
                m_ownsHandle = true;
            }
            else {
                m_safeProvHandle = Utils.StaticProvHandle;
                m_ownsHandle = false;
            }
        }

        [System.Security.SecuritySafeCritical]  // auto-generated
        protected override void Dispose(bool disposing) {
            base.Dispose(disposing);

            if (disposing && m_ownsHandle) {
                m_safeProvHandle.Dispose();
            }
        }
#endif // !FEATURE_PAL

#else // !FEATURE_CORECLR

        #if FEATURE_CORECLR
        [System.Security.SecuritySafeCritical] // auto-generated
        #endif
        public RNGCryptoServiceProvider() {
            m_cspHandle = CapiNative.AcquireCsp(null,
                                                CapiNative.ProviderNames.MicrosoftEnhanced,
                                                CapiNative.ProviderType.RsaFull,
                                                CapiNative.CryptAcquireContextFlags.VerifyContext);
        }
#endif // !FEATURE_CORECLR

        //
        // public methods
        //
#if !FEATURE_CORECLR
        [System.Security.SecuritySafeCritical]  // auto-generated
        public override void GetBytes(byte[] data) {
            if (data == null) throw new ArgumentNullException("data");
            Contract.EndContractBlock();
#if !FEATURE_PAL
            GetBytes(m_safeProvHandle, data, data.Length);
#else
            if (!Win32Native.Random(true, data, data.Length))
                throw new CryptographicException(Marshal.GetLastWin32Error());
#endif // !FEATURE_PAL
        }

        [System.Security.SecuritySafeCritical]  // auto-generated
        public override void GetNonZeroBytes(byte[] data) {
            if (data == null)
                throw new ArgumentNullException("data");
            Contract.EndContractBlock();

#if !FEATURE_PAL
            GetNonZeroBytes(m_safeProvHandle, data, data.Length);
#else
            GetBytes(data);

            int indexOfFirst0Byte = data.Length;
            for (int i = 0; i < data.Length; i++) {
                if (data[i] == 0) {
                    indexOfFirst0Byte = i;
                    break;
                }
            }
            for (int i = indexOfFirst0Byte; i < data.Length; i++) {
                if (data[i] != 0) {
                    data[indexOfFirst0Byte++] = data[i];
                }
            }

            while (indexOfFirst0Byte < data.Length) {
                // this should be more than enough to fill the rest in one iteration
                byte[] tmp = new byte[2 * (data.Length - indexOfFirst0Byte)];
                GetBytes(tmp);

                for (int i = 0; i < tmp.Length; i++) {
                    if (tmp[i] != 0) {
                        data[indexOfFirst0Byte++] = tmp[i];
                        if (indexOfFirst0Byte >= data.Length) break;
                    }
                }
            }
#endif // !FEATURE_PAL
        }

#else // !FEATURE_CORECLR

        #if FEATURE_CORECLR
        [System.Security.SecuritySafeCritical] // auto-generated
        #endif
        protected override void Dispose(bool disposing) {
            try {
                if (disposing) {
                    if (m_cspHandle != null) {
                        m_cspHandle.Dispose();
                    }
                }
            }
            finally {
                base.Dispose(disposing);
            }
        }

        #if FEATURE_CORECLR
        [System.Security.SecuritySafeCritical] // auto-generated
        #endif
        public override void GetBytes(byte[] data) {
            if (data == null) {
                throw new ArgumentNullException("data");
            }
            Contract.EndContractBlock();

            if (data.Length > 0) {
                CapiNative.GenerateRandomBytes(m_cspHandle, data);
            }
        }

        #if FEATURE_CORECLR
        [System.Security.SecuritySafeCritical] // auto-generated
        #endif
        public override void GetBytes(byte[] data, int offset, int count) {
            if (data == null) throw new ArgumentNullException("data");
            if (offset < 0) throw new ArgumentOutOfRangeException("offset", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
            if (count < 0) throw new ArgumentOutOfRangeException("count", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
            if (offset + count > data.Length) throw new ArgumentException(Environment.GetResourceString("Argument_InvalidOffLen"));

            if (count > 0) {
                CapiNative.GenerateRandomBytes(m_cspHandle, data, offset, count);
            }
        }
#endif // !FEATURE_CORECLR

#if !FEATURE_PAL
        [System.Security.SecurityCritical]  // auto-generated
        [DllImport(JitHelpers.QCall, CharSet = CharSet.Unicode), SuppressUnmanagedCodeSecurity]
        [ResourceExposure(ResourceScope.None)]
        private static extern void GetBytes(SafeProvHandle hProv, byte[] randomBytes, int count);

        [System.Security.SecurityCritical]  // auto-generated
        [DllImport(JitHelpers.QCall, CharSet = CharSet.Unicode), SuppressUnmanagedCodeSecurity]
        [ResourceExposure(ResourceScope.None)]
        private static extern void GetNonZeroBytes(SafeProvHandle hProv, byte[] randomBytes, int count);
#endif
    }
}
