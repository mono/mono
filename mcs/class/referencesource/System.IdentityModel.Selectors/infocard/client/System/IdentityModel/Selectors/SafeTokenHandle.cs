//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.IdentityModel.Selectors
{
    using System;
    using System.Runtime.InteropServices;
    using System.Runtime.ConstrainedExecution;
    using System.Security;




    //
    // Summary:
    // Provides a wrapper over the generic xml token returned from the native client
    //
    internal class SafeTokenHandle : SafeHandle
    {


        [DllImport("infocardapi.dll",
                    EntryPoint = "FreeToken",
                    CharSet = CharSet.Unicode,
                    CallingConvention = CallingConvention.StdCall,
                    ExactSpelling = true,
                    SetLastError = true)]
        [SuppressUnmanagedCodeSecurity]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        public static extern System.Int32 FreeToken([In] IntPtr token);




        private SafeTokenHandle()
            : base(IntPtr.Zero, true)
        {
        }


        public override bool IsInvalid
        {
            get
            {
                return (IntPtr.Zero == base.handle);
            }
        }


        protected override bool ReleaseHandle()
        {
#pragma warning suppress 56523
            return (0 == FreeToken(base.handle)) ? false : true;
        }

    }
}
