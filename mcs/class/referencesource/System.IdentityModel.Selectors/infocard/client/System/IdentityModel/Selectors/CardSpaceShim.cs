//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------
namespace System.IdentityModel.Selectors
{

    using System;
    using System.IO;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Runtime.InteropServices;
    using System.IdentityModel.Claims;
    using System.Text;
    using System.Xml;
    using System.IdentityModel.Tokens;
    using System.ServiceProcess;
    using System.Globalization;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.CompilerServices;
    using Microsoft.InfoCards.Diagnostics;
    using Microsoft.Win32;
    using System.Text.RegularExpressions;
    using IDT = Microsoft.InfoCards.Diagnostics.InfoCardTrace;


    //
    // For common & resources
    //
    using Microsoft.InfoCards;
    using System.Security;

    //
    // Summary:
    // If v2 is installed, this class will route calls to the native dll that was installed with v2.
    // This class essentially mimics the behavior in CSD Main 58552 which has been checked into the .Net branch for Win7 
    //
    class CardSpaceShim
    {
        private const string REDIRECT_DLL_REG_KEY = @"software\microsoft\cardspace\v1";
        private const string REDIRECT_DLL_IMPLEMENTATION_VALUE = "ImplementationDLL";
        private const string REDIRECT_DLL_IMPLEMENTATION_VALUE_DEFAULT = "infocardapi2";
        private const string REDIRECT_DLL_CARDSPACE_V1 = "infocardapi";

        private object m_syncRoot = new Object();

        private bool m_isInitialized = false;

        //
        // Delegates defined as public for convenience in invocation
        //
        public CsV2ManageCardSpace m_csShimManageCardSpace;
        public CsV2GetToken m_csShimGetToken;
        public CsV2ImportInformationCard m_csShimImportInformationCard;

        public CsV2Encrypt m_csShimEncrypt;
        public CsV2Decrypt m_csShimDecrypt;
        public CsV2SignHash m_csShimSignHash;
        public CsV2VerifyHash m_csShimVerifyHash;

        public CsV2GenerateDerivedKey m_csShimGenerateDerivedKey;
        public CsV2GetCryptoTransform m_csShimGetCryptoTransform;
        public CsV2TransformBlock m_csShimTransformBlock;
        public CsV2TransformFinalBlock m_csShimTransformFinalBlock;

        public CsV2GetKeyedHash m_csShimGetKeyedHash;
        public CsV2HashCore m_csShimHashCore;
        public CsV2HashFinal m_csShimHashFinal;

        public CsV2FreeToken m_csShimFreeToken;
        public CsV2CloseCryptoHandle m_csShimCloseCryptoHandle;

        SafeLibraryHandle m_implementationDll;

        //
        // GetBrowserToken not required because that is accomplished via Pheonix bit etc. (not exposed thru
        // managed interface).
        // 

        //
        // Summary:
        // Performs initialization of the CardSpaceShim if necessary.
        // The v1 service will only allow one request from the user,
        // however locking anyway in case we change our behavior in v2.
        //
        public void InitializeIfNecessary()
        {
            if (!m_isInitialized)
            {
                lock (m_syncRoot)
                {
                    if (!m_isInitialized)
                    {
                        string implDllPath = GetCardSpaceImplementationDll();

                        m_implementationDll = SafeLibraryHandle.LoadLibraryW(implDllPath);
                        if (m_implementationDll.IsInvalid)
                        {
                            throw NativeMethods.ThrowWin32ExceptionWithContext(new Win32Exception(), implDllPath);
                        }

                        try
                        {
                            //
                            // Functions are listed in alphabetical order
                            //

                            IntPtr procaddr1 = NativeMethods.GetProcAddressWrapper(m_implementationDll, "CloseCryptoHandle");
                            m_csShimCloseCryptoHandle =
                                (CsV2CloseCryptoHandle)Marshal.GetDelegateForFunctionPointer(
                                                            procaddr1, typeof(CsV2CloseCryptoHandle));

                            IntPtr procaddr2 = NativeMethods.GetProcAddressWrapper(
                                m_implementationDll, "Decrypt");
                            m_csShimDecrypt =
                                (CsV2Decrypt)Marshal.GetDelegateForFunctionPointer(
                                                            procaddr2, typeof(CsV2Decrypt));

                            IntPtr procaddr3 = NativeMethods.GetProcAddressWrapper(m_implementationDll, "Encrypt");
                            m_csShimEncrypt =
                                (CsV2Encrypt)Marshal.GetDelegateForFunctionPointer(
                                                            procaddr3, typeof(CsV2Encrypt));

                            IntPtr procaddr4 = NativeMethods.GetProcAddressWrapper(m_implementationDll, "FreeToken");
                            m_csShimFreeToken =
                                (CsV2FreeToken)Marshal.GetDelegateForFunctionPointer(
                                                            procaddr4, typeof(CsV2FreeToken));

                            IntPtr procaddr5 = NativeMethods.GetProcAddressWrapper(m_implementationDll, "GenerateDerivedKey");
                            m_csShimGenerateDerivedKey =
                                (CsV2GenerateDerivedKey)Marshal.GetDelegateForFunctionPointer(
                                                            procaddr5, typeof(CsV2GenerateDerivedKey));

                            IntPtr procaddr6 = NativeMethods.GetProcAddressWrapper(m_implementationDll, "GetCryptoTransform");
                            m_csShimGetCryptoTransform =
                                (CsV2GetCryptoTransform)Marshal.GetDelegateForFunctionPointer(
                                                            procaddr6, typeof(CsV2GetCryptoTransform));

                            IntPtr procaddr7 = NativeMethods.GetProcAddressWrapper(m_implementationDll, "GetKeyedHash");
                            m_csShimGetKeyedHash =
                                (CsV2GetKeyedHash)Marshal.GetDelegateForFunctionPointer(
                                                            procaddr7, typeof(CsV2GetKeyedHash));

                            IntPtr procaddr8 = NativeMethods.GetProcAddressWrapper(m_implementationDll, "GetToken");
                            m_csShimGetToken =
                                (CsV2GetToken)Marshal.GetDelegateForFunctionPointer(
                                                            procaddr8, typeof(CsV2GetToken));

                            IntPtr procaddr9 = NativeMethods.GetProcAddressWrapper(m_implementationDll, "HashCore");
                            m_csShimHashCore =
                                (CsV2HashCore)Marshal.GetDelegateForFunctionPointer(
                                                            procaddr9, typeof(CsV2HashCore));

                            IntPtr procaddr10 = NativeMethods.GetProcAddressWrapper(m_implementationDll, "HashFinal");
                            m_csShimHashFinal =
                                (CsV2HashFinal)Marshal.GetDelegateForFunctionPointer(
                                                            procaddr10, typeof(CsV2HashFinal));

                            IntPtr procaddr11 = NativeMethods.GetProcAddressWrapper(m_implementationDll, "ImportInformationCard");
                            m_csShimImportInformationCard =
                                (CsV2ImportInformationCard)Marshal.GetDelegateForFunctionPointer(
                                                            procaddr11, typeof(CsV2ImportInformationCard));

                            IntPtr procaddr12 = NativeMethods.GetProcAddressWrapper(m_implementationDll, "ManageCardSpace");
                            m_csShimManageCardSpace =
                                (CsV2ManageCardSpace)Marshal.GetDelegateForFunctionPointer(
                                                            procaddr12, typeof(CsV2ManageCardSpace));

                            IntPtr procaddr13 = NativeMethods.GetProcAddressWrapper(m_implementationDll, "SignHash");
                            m_csShimSignHash =
                                (CsV2SignHash)Marshal.GetDelegateForFunctionPointer(
                                                            procaddr13, typeof(CsV2SignHash));

                            IntPtr procaddr14 = NativeMethods.GetProcAddressWrapper(m_implementationDll, "TransformBlock");
                            m_csShimTransformBlock =
                                (CsV2TransformBlock)Marshal.GetDelegateForFunctionPointer(
                                                            procaddr14, typeof(CsV2TransformBlock));

                            IntPtr procaddr15 = NativeMethods.GetProcAddressWrapper(m_implementationDll, "TransformFinalBlock");
                            m_csShimTransformFinalBlock =
                                (CsV2TransformFinalBlock)Marshal.GetDelegateForFunctionPointer(
                                                            procaddr15, typeof(CsV2TransformFinalBlock));


                            IntPtr procaddr16 = NativeMethods.GetProcAddressWrapper(m_implementationDll, "VerifyHash");
                            m_csShimVerifyHash =
                                (CsV2VerifyHash)Marshal.GetDelegateForFunctionPointer(
                                                            procaddr16, typeof(CsV2VerifyHash));

                        }
                        catch (Win32Exception)
                        {
                            //
                            // NB: IDT.ThrowHelperError would have logged for the Win32Exception
                            //
                            IDT.Assert(!m_isInitialized, "If an exception occurred, we expect this to be false");
                            throw;
                        }

                        m_isInitialized = true;
                    }
                }
            }
        }

        //
        // Summary:
        // Returns true if fileName has only alphanumeric characters
        //
        bool IsSafeFile(string fileName)
        {
            //
            // If any match from outside the range of [A-Za-z0-9] then we will not use this file
            //
            return Regex.IsMatch(fileName, "^[A-Za-z0-9]+$");
        }



        //
        // Summary:
        // Return the path to the v2 (or a version above v2) implementation dll. 
        // We expect this to be infocardapi2.dll unless overriden by a registry key
        //
        // Remarks: It is left upto the caller to check if the v2+ implementation 
        // dll actually exists or not.
        //
        private string GetV2ImplementationDllPath()
        {
            string v2AndAboveImplementationDll = String.Empty;

            //
            // First look in the registry key to see if this is defined
            //
            using (RegistryKey implDllKey = Registry.LocalMachine.OpenSubKey(REDIRECT_DLL_REG_KEY))
            {
                if (null != implDllKey)
                {
                    v2AndAboveImplementationDll = (string)implDllKey.GetValue(REDIRECT_DLL_IMPLEMENTATION_VALUE);

                    if (!String.IsNullOrEmpty(v2AndAboveImplementationDll))
                    {
                        string v2RegPath = Path.Combine(
                                                Environment.GetFolderPath(Environment.SpecialFolder.System),
                                                v2AndAboveImplementationDll + ".dll");

                        //
                        // Is the filename safe (use alphanumeric like the CSD Main 58552). Does it exist?
                        // If not, discard the registry key we just read.
                        //
                        if (!IsSafeFile(v2AndAboveImplementationDll) || !File.Exists(v2RegPath))
                        {
                            v2AndAboveImplementationDll = String.Empty;
                        }
                    }
                }
            }


            // 
            // If reg key was not found or not safe, or value was not found, or found to be empty,
            // then use the default of infocardapi2.dll
            //
            if (String.IsNullOrEmpty(v2AndAboveImplementationDll))
            {
                v2AndAboveImplementationDll = REDIRECT_DLL_IMPLEMENTATION_VALUE_DEFAULT;
            }

            IDT.Assert(!String.IsNullOrEmpty(v2AndAboveImplementationDll), "v2AndAboveImplementationDll should not be empty");

            //
            // Get the full path to the v2Above dll
            //
            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.System),
                v2AndAboveImplementationDll + ".dll");

        }

        //
        // Summary:
        // Return handle to the CardSpace implementation dll.
        // We will first check to see if a v2 (or above) redirection dll has been installed.
        // If not we will check to see if the v1 infocardapi.dll is installed. 
        // If that's not found as well, an exception is thrown
        //
        private string GetCardSpaceImplementationDll()
        {
            string implDllFullPath = GetV2ImplementationDllPath();
            if (!File.Exists(implDllFullPath))
            {
                //
                // Choose infocardapi.dll, if v2+ dll does not exist
                //
                implDllFullPath = Path.Combine(
                                        Environment.GetFolderPath(Environment.SpecialFolder.System),
                                        REDIRECT_DLL_CARDSPACE_V1 + ".dll");

                if (!File.Exists(implDllFullPath))
                {
                    //
                    // If this does not exist either, then even CardSpace v1 is NOT installed
                    // on this machine. Note: Throwing an exception using IDT.ThrowHelperError 
                    // does not log to event log unless it derives from InfoCardBaseException.
                    // This seems fine given that we don't want to be logging as "CardSpace X.0.0.0",
                    // rather we'll let the client application log to event log if desired.
                    //
                    throw IDT.ThrowHelperError(
                        new CardSpaceException(SR.GetString(SR.ClientAPIServiceNotInstalledError)));
                }
            }

            return implDllFullPath;
        }

        //
        // Delegate definitions ported from NativeMethods.cs
        //

        internal delegate System.Int32 CsV2ManageCardSpace();

        //[ReliabilityContract( Consistency.WillNotCorruptState, Cer.MayFail )]
        internal delegate System.Int32 CsV2GetToken(
                                      int cPolicyChain,
                                      SafeHandle pPolicyChain,
                                      out SafeTokenHandle securityToken,
                                      out InternalRefCountedHandle pCryptoHandle);


        internal delegate System.Int32 CsV2ImportInformationCard(
             [MarshalAs(UnmanagedType.LPWStr)]
             string nativeFileName);

        //[ReliabilityContract( Consistency.WillNotCorruptState, Cer.MayFail )]
        internal delegate int CsV2Encrypt(
                                       InternalRefCountedHandle nativeCryptoHandle,
                                       bool fOAEP,
                                       [MarshalAs(UnmanagedType.U4)]
                                       int cbInData,
                                       SafeHandle pInData,
                                       [MarshalAs(UnmanagedType.U4)]
                                       out int pcbOutData,
                                       out GlobalAllocSafeHandle pOutData);

        //[ReliabilityContract( Consistency.WillNotCorruptState, Cer.MayFail )]
        internal delegate int CsV2Decrypt(
                                       InternalRefCountedHandle nativeCryptoHandle,
                                       bool fOAEP,
                                       [MarshalAs(UnmanagedType.U4)]
                                       int cbInData,
                                       SafeHandle pInData,
                                       [MarshalAs(UnmanagedType.U4)]
                                       out int pcbOutData,
                                       out GlobalAllocSafeHandle pOutData);

        //[ReliabilityContract( Consistency.WillNotCorruptState, Cer.MayFail )]
        internal delegate int CsV2SignHash(
                                       InternalRefCountedHandle nativeCryptoHandle,
                                       [MarshalAs(UnmanagedType.U4)]
                                       int cbHash,
                                       SafeHandle pInData,
                                       SafeHandle pHashAlgOid,
                                       [MarshalAs(UnmanagedType.U4)]
                                       out int pcbSig,
                                       out GlobalAllocSafeHandle pSig);

        //[ReliabilityContract( Consistency.WillNotCorruptState, Cer.MayFail )]
        internal delegate int CsV2VerifyHash(
                                       InternalRefCountedHandle nativeCryptoHandle,
                                       [MarshalAs(UnmanagedType.U4)]
                                       int cbHash,
                                       SafeHandle pInData,
                                       SafeHandle pHashAlgOid,
                                       [MarshalAs(UnmanagedType.U4)]
                                       int pcbSig,
                                       SafeHandle pSig,
                                       out bool verified);

        //[ReliabilityContract( Consistency.WillNotCorruptState, Cer.MayFail )]
        internal delegate int CsV2GenerateDerivedKey(InternalRefCountedHandle nativeCryptoHandle,
                                                     int cbLabel,
                                                     SafeHandle pLabel,
                                                     int cbNonce,
                                                     SafeHandle pNonce,
                                                     int derivedKeyLength,
                                                     int offset,
                                                     [MarshalAs(UnmanagedType.LPWStr)]
                                                     string derivationAlgUri,
                                                     out int cbDerivedKey,
                                                     out GlobalAllocSafeHandle pDerivedKey);

        //[ReliabilityContract( Consistency.WillNotCorruptState, Cer.MayFail )]
        internal delegate int CsV2GetCryptoTransform(
                                       InternalRefCountedHandle nativeCryptoHandle,
                                       int mode,
                                       int padding,
                                       int feedbackSize,
                                       int direction,
                                       int cbIV,
                                       SafeHandle pIV,
                                       out InternalRefCountedHandle nativeTransformHandle);

        //[ReliabilityContract( Consistency.WillNotCorruptState, Cer.MayFail )]
        internal delegate int CsV2TransformBlock(InternalRefCountedHandle nativeCryptoHandle,
                                                 int cbInData,
                                                 SafeHandle pInData,
                                                 out int cbOutData,
                                                 out GlobalAllocSafeHandle pOutData);

        //[ReliabilityContract( Consistency.WillNotCorruptState, Cer.MayFail )]
        internal delegate int CsV2TransformFinalBlock(InternalRefCountedHandle nativeCryptoHandle,
                                                 int cbInData,
                                                 SafeHandle pInData,
                                                 out int cbOutData,
                                                 out GlobalAllocSafeHandle pOutData);

        //[ReliabilityContract( Consistency.WillNotCorruptState, Cer.MayFail )]
        internal delegate int CsV2GetKeyedHash(
                                       InternalRefCountedHandle nativeCryptoHandle,
                                       out InternalRefCountedHandle nativeHashHandle);

        //[ReliabilityContract( Consistency.WillNotCorruptState, Cer.MayFail )]
        internal delegate int CsV2HashCore(InternalRefCountedHandle nativeCryptoHandle,
                                          int cbInData,
                                          SafeHandle pInData);

        //[ReliabilityContract( Consistency.WillNotCorruptState, Cer.MayFail )]
        internal delegate int CsV2HashFinal(InternalRefCountedHandle nativeCryptoHandle,
                                            int cbInData,
                                            SafeHandle pInData,
                                            out int cbOutData,
                                            out GlobalAllocSafeHandle pOutData);

        [SuppressUnmanagedCodeSecurity]
        //[ReliabilityContract( Consistency.WillNotCorruptState, Cer.Success )]
        internal delegate bool CsV2CloseCryptoHandle([In] IntPtr hKey);

        [SuppressUnmanagedCodeSecurity]
        //[ReliabilityContract( Consistency.WillNotCorruptState, Cer.Success )]
        internal delegate System.Int32 CsV2FreeToken([In] IntPtr token);

    }
}
