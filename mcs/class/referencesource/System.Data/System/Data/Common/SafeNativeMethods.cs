//------------------------------------------------------------------------------
// <copyright file="SafeNativeMethods.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">[....]</owner>
// <owner current="true" primary="false">[....]</owner>
// <owner current="true" primary="false">[....]</owner>
//------------------------------------------------------------------------------

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Permissions;
using System.Text;
using System.Threading;
using System.Runtime.ConstrainedExecution;
using System.Runtime.Versioning;

namespace System.Data.Common {

    [SuppressUnmanagedCodeSecurityAttribute()]
    internal static class SafeNativeMethods {
    
        [DllImport(ExternDll.Ole32, SetLastError=false)]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        [ResourceExposure(ResourceScope.None)]
        static internal extern IntPtr CoTaskMemAlloc(IntPtr cb);

        [DllImport(ExternDll.Ole32, SetLastError=false)]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        [ResourceExposure(ResourceScope.None)]
        static internal extern void CoTaskMemFree(IntPtr handle);

        [DllImport(ExternDll.Kernel32, CharSet=CharSet.Unicode, PreserveSig=true)]
        [ResourceExposure(ResourceScope.None)]
        static internal extern int GetUserDefaultLCID();

        [DllImport(ExternDll.Kernel32, PreserveSig=true)]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        [ResourceExposure(ResourceScope.None)]
        static internal extern void ZeroMemory(IntPtr dest, IntPtr length);

        // <WARNING>
        // Using the int versions of the Increment() and Decrement() methods is correct.
        // Please check \fx\src\Data\System\Data\Odbc\OdbcHandle.cs for the memory layout.
        // </WARNING>

        // <NDPWHIDBEY 18133>
        // The following casting operations require these three methods to be unsafe.  This is
        // a workaround for this issue to meet the M1 exit criteria.  We need to revisit this in M2.

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        static internal unsafe IntPtr InterlockedExchangePointer(
                IntPtr lpAddress,
                IntPtr lpValue)
        {
            IntPtr  previousPtr;
            IntPtr  actualPtr = *(IntPtr *)lpAddress.ToPointer();

            do {
                previousPtr = actualPtr;
                actualPtr   = Interlocked.CompareExchange(ref *(IntPtr *)lpAddress.ToPointer(), lpValue, previousPtr);
            }
            while (actualPtr != previousPtr);

            return actualPtr;
        }

        // http://msdn.microsoft.com/library/default.asp?url=/library/en-us/sysinfo/base/getcomputernameex.asp
        [DllImport(ExternDll.Kernel32, CharSet = CharSet.Unicode, EntryPoint="GetComputerNameExW", SetLastError=true)]
        [ResourceExposure(ResourceScope.None)]
        static internal extern int GetComputerNameEx(int nameType, StringBuilder nameBuffer, ref int bufferSize);

        [DllImport(ExternDll.Kernel32, CharSet=System.Runtime.InteropServices.CharSet.Auto)]
        [ResourceExposure(ResourceScope.Process)]
        static internal extern int GetCurrentProcessId();

        [DllImport(ExternDll.Kernel32, CharSet=CharSet.Auto, BestFitMapping=false, ThrowOnUnmappableChar=true)]
//        [DllImport(ExternDll.Kernel32, CharSet=CharSet.Auto)]
        [ResourceExposure(ResourceScope.Process)]
        static internal extern IntPtr GetModuleHandle([MarshalAs(UnmanagedType.LPTStr), In] string moduleName/*lpctstr*/);

        [DllImport(ExternDll.Kernel32, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true, SetLastError = true)]
//        [DllImport(ExternDll.Kernel32, CharSet=CharSet.Ansi)]
        [ResourceExposure(ResourceScope.None)]
        static internal extern IntPtr GetProcAddress(IntPtr HModule, [MarshalAs(UnmanagedType.LPStr), In] string funcName/*lpcstr*/);

        [DllImport(ExternDll.Kernel32, SetLastError=true)]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        [ResourceExposure(ResourceScope.None)]
        static internal extern IntPtr LocalAlloc(int flags, IntPtr countOfBytes);

        [DllImport(ExternDll.Kernel32, SetLastError=true)]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        [ResourceExposure(ResourceScope.None)]
        static internal extern IntPtr LocalFree(IntPtr handle);

        [DllImport(ExternDll.Oleaut32, CharSet=CharSet.Unicode)]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]            
        [ResourceExposure(ResourceScope.None)]
        internal static extern IntPtr SysAllocStringLen(String src, int len);  // BSTR

        [DllImport(ExternDll.Oleaut32)]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        [ResourceExposure(ResourceScope.None)]
        internal static extern void SysFreeString(IntPtr bstr);

        // only using this to clear existing error info with null
        [DllImport(ExternDll.Oleaut32, CharSet=CharSet.Unicode, PreserveSig=false)]
        // TLS values are preserved between threads, need to check that we use this API to clear the error state only.
        [ResourceExposure(ResourceScope.Process)]
        static private extern void SetErrorInfo(Int32 dwReserved, IntPtr pIErrorInfo);

        [DllImport(ExternDll.Kernel32, SetLastError=true)]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        [ResourceExposure(ResourceScope.Machine)]
        static internal extern int ReleaseSemaphore(IntPtr handle, int releaseCount, IntPtr previousCount);

        [DllImport(ExternDll.Kernel32, SetLastError=true)]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        [ResourceExposure(ResourceScope.None)]
        static internal extern int WaitForMultipleObjectsEx(uint nCount, IntPtr lpHandles, bool bWaitAll, uint dwMilliseconds, bool bAlertable);

        [DllImport(ExternDll.Kernel32/*, SetLastError=true*/)]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        [ResourceExposure(ResourceScope.None)]
        static internal extern int WaitForSingleObjectEx(IntPtr lpHandles, uint dwMilliseconds, bool bAlertable);

        [DllImport(ExternDll.Ole32, PreserveSig=false)]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        [ResourceExposure(ResourceScope.None)]
        static internal extern void PropVariantClear(IntPtr pObject);

        [DllImport(ExternDll.Oleaut32, PreserveSig=false)]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        [ResourceExposure(ResourceScope.None)]
        static internal extern void VariantClear(IntPtr pObject);

        sealed internal class Wrapper {

            private Wrapper() { }

            // SxS: clearing error information is considered safe
            [ResourceExposure(ResourceScope.None)]
            [ResourceConsumption(ResourceScope.Process, ResourceScope.Process)]
            static internal void ClearErrorInfo() { // MDAC 68199
                SafeNativeMethods.SetErrorInfo(0, ADP.PtrZero);
            }
        }


    }
}
