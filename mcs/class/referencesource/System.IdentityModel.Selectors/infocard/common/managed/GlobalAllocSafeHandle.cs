//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------
namespace Microsoft.InfoCards
{
    using System;
    using System.Runtime.InteropServices;
    using System.Runtime.CompilerServices;
    using System.Runtime.ConstrainedExecution;
    using System.Security;
    using Microsoft.InfoCards.Diagnostics;
    using IDT = Microsoft.InfoCards.Diagnostics.InfoCardTrace;


    //
    // Summary:
    // Provides a wrapper over memory allocated by GlobalAlloc 
    // guaranteeing that it will be freed during rude thread / appdomain unloads.
    // Remarks:
    // There is a small ---- in the usage of this class, as it is used to wrap return parameters
    // immediatley following the function return.
    //
    internal class GlobalAllocSafeHandle : SafeHandle
    {
        [SuppressUnmanagedCodeSecurity]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        [DllImport("Kernel32.dll", EntryPoint = "RtlZeroMemory", SetLastError = false)]
        public static extern void ZeroMemory(IntPtr dest, Int32 size);

        [SuppressUnmanagedCodeSecurity]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        [DllImport("kernel32.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern IntPtr GlobalFree(IntPtr hMem);

        //
        // How many bytes we currently wrap. This can be zero, as our usage allows for a valid handle 
        // backed by 0 bytes of allocated memory - specificially TransformBlock and TransformFinalBlock 
        // can return this by design.
        //
        private int m_bytes;


        private GlobalAllocSafeHandle() : base(IntPtr.Zero, true) { m_bytes = 0; }

        public int Length
        {
            set { m_bytes = value; }
            get { return m_bytes; }
        }
        public override bool IsInvalid
        {
            get
            {
                return (IntPtr.Zero == base.handle);
            }
        }



        //
        // Summary:
        // Clear the data held and release the memory. 
        //
        protected override bool ReleaseHandle()
        {

            if (m_bytes > 0)
            {
                ZeroMemory(base.handle, m_bytes);
                GlobalFree(base.handle);
                m_bytes = 0;
            }
            return true;
        }

    }
}
