//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------
namespace Microsoft.InfoCards
{
    using System;
    using System.Runtime.InteropServices;
    using Microsoft.InfoCards.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Runtime.ConstrainedExecution;
    using System.Security;
    using IDT = Microsoft.InfoCards.Diagnostics.InfoCardTrace;



    //
    // Summary:
    // Provides a wrapper over HGlobal alloc'd memory guaranteeing that the 
    // contents will be released in the presence of rude app domain and thread aborts.
    //
    internal class HGlobalSafeHandle : SafeHandle
    {

        public static HGlobalSafeHandle Construct()
        {
            return new HGlobalSafeHandle();
        }
        public static HGlobalSafeHandle Construct(string managedString)
        {
            IDT.DebugAssert(!String.IsNullOrEmpty(managedString), "null string");

            int bytes = (managedString.Length + 1) * 2;
            return new HGlobalSafeHandle(Marshal.StringToHGlobalUni(managedString), bytes);

        }

        public static HGlobalSafeHandle Construct(int bytes)
        {
            IDT.DebugAssert(bytes > 0, "attempt to allocate a handle with <= 0 bytes");
            return new HGlobalSafeHandle(Marshal.AllocHGlobal(bytes), bytes);

        }

        [SuppressUnmanagedCodeSecurity]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        [DllImport("Kernel32.dll", EntryPoint = "RtlZeroMemory", SetLastError = false)]
        public static extern void ZeroMemory(IntPtr dest, Int32 size);

        //
        // The number of bytes allocated.
        //
        private int m_bytes;
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        private HGlobalSafeHandle(IntPtr toManage, int length)
            : base(IntPtr.Zero, true)
        {
            m_bytes = length;
            SetHandle(toManage);
        }

        private HGlobalSafeHandle() : base(IntPtr.Zero, true) { }

        public override bool IsInvalid
        {
            get
            {
                return (IntPtr.Zero == base.handle);
            }
        }


        //
        // Summary:
        // Zero the string contents and release the handle
        //
        protected override bool ReleaseHandle()
        {

            IDT.DebugAssert(!IsInvalid, "handle is invalid in release handle");
            IDT.DebugAssert(0 != m_bytes, "invalid size");
            ZeroMemory(base.handle, m_bytes);

            Marshal.FreeHGlobal(base.handle);
            return true;
        }


    }
}
