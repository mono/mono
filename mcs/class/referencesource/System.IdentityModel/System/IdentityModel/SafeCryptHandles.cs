//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.IdentityModel
{
    using System.ComponentModel;
    using System.Runtime.InteropServices;
    using System.Runtime.CompilerServices;
    using System.Runtime.ConstrainedExecution;
    using System.Security.Cryptography;
    using System.ServiceModel.Diagnostics;
    using Microsoft.Win32.SafeHandles;

    class SafeProvHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        SafeProvHandle() : base(true) { }

        // 0 is an Invalid Handle
        SafeProvHandle(IntPtr handle)
            : base(true)
        {
            DiagnosticUtility.DebugAssert(handle == IntPtr.Zero, "SafeProvHandle constructor can only be called with IntPtr.Zero.");
            SetHandle(handle);
        }

        internal static SafeProvHandle InvalidHandle
        {
            get { return new SafeProvHandle(IntPtr.Zero); }
        }

        protected override bool ReleaseHandle()
        {
            // PreSharp 
#pragma warning suppress 56523 // We are not interested in throwing an exception here if CloseHandle fails.
            return NativeMethods.CryptReleaseContext(handle, 0);
        }
    }

    class SafeKeyHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        SafeProvHandle provHandle = null;

        SafeKeyHandle() : base(true) { }

        // 0 is an Invalid Handle
        SafeKeyHandle(IntPtr handle)
            : base(true)
        {
            DiagnosticUtility.DebugAssert(handle == IntPtr.Zero, "SafeKeyHandle constructor can only be called with IntPtr.Zero.");
            SetHandle(handle);
        }

        internal static SafeKeyHandle InvalidHandle
        {
            get { return new SafeKeyHandle(IntPtr.Zero); }
        }

        protected override bool ReleaseHandle()
        {
            // PreSharp 
#pragma warning suppress 56523 // We are not interested in throwing an exception here if CloseHandle fails.
            bool ret = NativeMethods.CryptDestroyKey(handle);
            if (this.provHandle != null)
            {
                this.provHandle.DangerousRelease();
                this.provHandle = null;
            }
            return ret;
        }

        internal static unsafe SafeKeyHandle SafeCryptImportKey(SafeProvHandle provHandle, void* pbDataPtr, int cbData)
        {
            bool b = false;
            int err = 0;
            SafeKeyHandle keyHandle = null;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                provHandle.DangerousAddRef(ref b);
            }
            catch (Exception e)
            {
                if (System.Runtime.Fx.IsFatal(e))
                    throw;
                
                if (b)
                {
                    provHandle.DangerousRelease();
                    b = false;
                }
                if (!(e is ObjectDisposedException))
                    throw;
            }
            finally
            {
                if (b)
                {
                    b = NativeMethods.CryptImportKey(provHandle, pbDataPtr, (uint)cbData, IntPtr.Zero, 0, out keyHandle);
                    if (!b)
                    {
                        err = Marshal.GetLastWin32Error();
                        provHandle.DangerousRelease();
                    }
                    else
                    {
                        // Take ownership of AddRef.  Will Release at Close.
                        keyHandle.provHandle = provHandle;
                    }
                }
            }

            if (!b)
            {
                Utility.CloseInvalidOutSafeHandle(keyHandle);
                string reason = (err != 0) ? new Win32Exception(err).Message : String.Empty;
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new CryptographicException(SR.GetString(SR.AESCryptImportKeyFailed, reason)));
            }
            return keyHandle;
        }
    }
}
