///----------- ----------- ----------- ----------- ----------- -----------
/// <copyright file="WindowsRuntimeBufferHelper.cs" company="Microsoft">
///     Copyright (c) Microsoft Corporation.  All rights reserved.
/// </copyright>                               
///
/// <owner>gpaperin</owner>
///----------- ----------- ----------- ----------- ----------- -----------


using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;
using System.Runtime.Versioning;
using System.Security;
using System.Threading;


namespace System.Runtime.InteropServices.WindowsRuntime {

/// <summary>
/// Exposes a helper method that allows <code>WindowsRuntimeBuffer : IBuffer, IBufferInternal</code> which is implemented in
/// <code>System.Runtime.WindowsRuntime.dll</code> to call into the VM.
/// </summary>
[FriendAccessAllowed]
internal static class WindowsRuntimeBufferHelper {


    [SecurityCritical]
    [ResourceExposure(ResourceScope.AppDomain)]    
    [DllImport(JitHelpers.QCall)]
    [SuppressUnmanagedCodeSecurity]
    [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
    private unsafe extern static void StoreOverlappedPtrInCCW(ObjectHandleOnStack windowsRuntimeBuffer, NativeOverlapped* overlapped);


    [FriendAccessAllowed]
    [SecurityCritical]
    [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
    internal unsafe static void StoreOverlappedInCCW(Object windowsRuntimeBuffer, NativeOverlapped* overlapped) {

        StoreOverlappedPtrInCCW(JitHelpers.GetObjectHandleOnStack(ref windowsRuntimeBuffer), overlapped);
    }

}  // class WindowsRuntimeBufferHelper

}  // namespace

// WindowsRuntimeBufferHelper.cs
