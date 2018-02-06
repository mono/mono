// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32.SafeHandles;

// stubs for external/corefx/src/Common/src/Interop/Unix/System.Net.Security.Native/Interop.NetSecurityNative.cs
// needed for https://github.com/mono/mono/issues/6766
// or can be implemented via pal_gssapi (see https://github.com/mono/mono/pull/6806)
static partial class Interop
{
    static partial class NetSecurityNative
    {
        internal static void ReleaseGssBuffer (
            IntPtr bufferPtr,
            UInt64 length) => throw new NotSupportedException ();

        internal static Status DisplayMinorStatus (
            out Status minorStatus,
            Status statusValue,
            ref GssBuffer buffer) => throw new NotSupportedException ();

        internal static Status DisplayMajorStatus (
            out Status minorStatus,
            Status statusValue,
            ref GssBuffer buffer) => throw new NotSupportedException ();

        internal static Status ImportUserName (
            out Status minorStatus,
            string inputName,
            int inputNameByteCount,
            out SafeGssNameHandle outputName) => throw new NotSupportedException ();

        internal static Status ImportPrincipalName (
            out Status minorStatus,
            string inputName,
            int inputNameByteCount,
            out SafeGssNameHandle outputName) => throw new NotSupportedException ();

        internal static Status ReleaseName (
            out Status minorStatus,
            ref IntPtr inputName) => throw new NotSupportedException ();

        internal static Status InitiateCredSpNego (
            out Status minorStatus,
            SafeGssNameHandle desiredName,
            out SafeGssCredHandle outputCredHandle) => throw new NotSupportedException ();

        internal static Status InitiateCredWithPassword (
            out Status minorStatus,
            bool isNtlm,
            SafeGssNameHandle desiredName,
            string password,
            int passwordLen,
            out SafeGssCredHandle outputCredHandle) => throw new NotSupportedException ();

        internal static Status ReleaseCred (
            out Status minorStatus,
            ref IntPtr credHandle) => throw new NotSupportedException ();

        internal static Status InitSecContext (
            out Status minorStatus,
            SafeGssCredHandle initiatorCredHandle,
            ref SafeGssContextHandle contextHandle,
            bool isNtlmOnly,
            SafeGssNameHandle targetName,
            uint reqFlags,
            byte[] inputBytes,
            int inputLength,
            ref GssBuffer token,
            out uint retFlags,
            out int isNtlmUsed) => throw new NotSupportedException ();

        internal static Status AcceptSecContext (
            out Status minorStatus,
            ref SafeGssContextHandle acceptContextHandle,
            byte[] inputBytes,
            int inputLength,
            ref GssBuffer token) => throw new NotSupportedException ();

        internal static Status DeleteSecContext (
            out Status minorStatus,
            ref IntPtr contextHandle) => throw new NotSupportedException ();

        static Status Wrap(
            out Status minorStatus,
            SafeGssContextHandle contextHandle,
            bool isEncrypt,
            byte[] inputBytes,
            int offset,
            int count,
            ref GssBuffer outBuffer) => throw new NotSupportedException ();

        static Status Unwrap (
            out Status minorStatus,
            SafeGssContextHandle contextHandle,
            byte[] inputBytes,
            int offset,
            int count,
            ref GssBuffer outBuffer) => throw new NotSupportedException ();

        internal static Status WrapBuffer (
            out Status minorStatus,
            SafeGssContextHandle contextHandle,
            bool isEncrypt,
            byte[] inputBytes,
            int offset,
            int count,
            ref GssBuffer outBuffer) => throw new NotSupportedException ();

        internal static Status UnwrapBuffer (
            out Status minorStatus,
            SafeGssContextHandle contextHandle,
            byte[] inputBytes,
            int offset,
            int count,
            ref GssBuffer outBuffer) => throw new NotSupportedException ();

        internal enum Status : uint
        {
            GSS_S_COMPLETE = 0,
            GSS_S_CONTINUE_NEEDED = 1
        }

        [Flags]
        internal enum GssFlags : uint
        {
            GSS_C_DELEG_FLAG = 0x1,
            GSS_C_MUTUAL_FLAG = 0x2,
            GSS_C_REPLAY_FLAG = 0x4,
            GSS_C_SEQUENCE_FLAG = 0x8,
            GSS_C_CONF_FLAG = 0x10,
            GSS_C_INTEG_FLAG = 0x20,
            GSS_C_ANON_FLAG = 0x40,
            GSS_C_PROT_READY_FLAG = 0x80,
            GSS_C_TRANS_FLAG = 0x100,
            GSS_C_DCE_STYLE = 0x1000,
            GSS_C_IDENTIFY_FLAG = 0x2000,
            GSS_C_EXTENDED_ERROR_FLAG = 0x4000,
            GSS_C_DELEG_POLICY_FLAG = 0x8000
        }
    }
}
