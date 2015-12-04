//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Diagnostics
{
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Runtime.ConstrainedExecution;
    using System.Diagnostics;
    using System.Threading;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Permissions;
    using System.Diagnostics.CodeAnalysis;

    class Utility
    {
        ExceptionUtility exceptionUtility;

        [Obsolete("For SMDiagnostics.dll use only. Call DiagnosticUtility.Utility instead")]
        internal Utility(ExceptionUtility exceptionUtility)
        {
            this.exceptionUtility = exceptionUtility;
        }

        // Call this when a p/invoke with an 'out SafeHandle' parameter returns an error.  This will safely clean up the handle.
        [SuppressMessage(FxCop.Category.Security, FxCop.Rule.TransparentMethodsMustNotReferenceCriticalCode)] // we got APTCA approval with no requirement to fix this transparency warning
        internal static void CloseInvalidOutSafeHandle(SafeHandle handle)
        {
            // Workaround for 64-bit CLR bug VSWhidbey 546830 - sometimes invalid SafeHandles come back null.
            if (handle != null)
            {
#pragma warning disable 618
                Fx.Assert(handle.IsInvalid, "CloseInvalidOutSafeHandle called with a valid handle!");
#pragma warning restore 618

                // Calls SuppressFinalize.
                handle.SetHandleAsInvalid();
            }
        }

        // Copy of the above for CriticalHandles.
        [SuppressMessage(FxCop.Category.Security, FxCop.Rule.TransparentMethodsMustNotReferenceCriticalCode)] // we got APTCA approval with no requirement to fix this transparency warning. plus, the callers of this method are not supported in partial trust.
        internal static void CloseInvalidOutCriticalHandle(CriticalHandle handle)
        {
            if (handle != null)
            {
#pragma warning disable 618
                Fx.Assert(handle.IsInvalid, "CloseInvalidOutCriticalHandle called with a valid handle!");
#pragma warning restore 618

                handle.SetHandleAsInvalid();
            }
        }

        internal Guid CreateGuid(string guidString)
        {
            return Fx.CreateGuid(guidString);
        }

        internal bool TryCreateGuid(string guidString, out Guid result)
        {
            return Fx.TryCreateGuid(guidString, out result);
        }

        internal byte[] AllocateByteArray(int size)
        {
            return Fx.AllocateByteArray(size);
        }

        internal char[] AllocateCharArray(int size)
        {
            return Fx.AllocateCharArray(size);
        }
    }
}
