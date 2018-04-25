//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------
namespace System.IdentityModel.Selectors
{
    using System;
    using System.Runtime.InteropServices;
    using System.Runtime.ConstrainedExecution;
    using System.ComponentModel;

    using IDT = Microsoft.InfoCards.Diagnostics.InfoCardTrace;

    //
    // For common & resources
    //
    using Microsoft.InfoCards;

    internal static class NativeMethods
    {
        public static IntPtr GetProcAddressWrapper(SafeLibraryHandle implDll, string procName)
        {
            IntPtr procaddr = NativeMethods.GetProcAddress(implDll, procName);
            if (IntPtr.Zero == procaddr)
            {
                //
                // We'll get the last error's message using Win32Exception
                // Adding the procName gives more context.
                //
                ThrowWin32ExceptionWithContext(new Win32Exception(), procName);
            }
            return procaddr;
        }

        public static Win32Exception ThrowWin32ExceptionWithContext(Win32Exception wex, string context)
        {
            throw IDT.ThrowHelperError(new Win32Exception(
                wex.NativeErrorCode,
                SR.GetString(SR.ClientAPIDetailedExceptionMessage, wex.Message, context)));
        }


        [DllImport("kernel32.dll",
            CharSet = CharSet.Ansi,
            ExactSpelling = true,
            SetLastError = true,
            CallingConvention = CallingConvention.StdCall)]
        private static extern IntPtr GetProcAddress(
            SafeLibraryHandle hModule,
            [MarshalAs(UnmanagedType.LPStr)]
            string procname);

    }
}
