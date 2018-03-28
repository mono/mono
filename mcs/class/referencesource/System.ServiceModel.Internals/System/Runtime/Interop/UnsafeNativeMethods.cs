//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.Runtime.Interop
{
    using System;
    using System.Text;
    using System.Security;
    using System.Collections.Generic;
    using System.Runtime.Versioning;
    using Microsoft.Win32.SafeHandles;
    using System.Runtime.InteropServices;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.Diagnostics;
    using FILETIME = System.Runtime.InteropServices.ComTypes.FILETIME;

    [SuppressUnmanagedCodeSecurity]    
    static class UnsafeNativeMethods
    {
        public const string KERNEL32 = "kernel32.dll";
        public const String ADVAPI32 = "advapi32.dll";

        public const int ERROR_INVALID_HANDLE = 6;
        public const int ERROR_MORE_DATA = 234;
        public const int ERROR_ARITHMETIC_OVERFLOW = 534;
        public const int ERROR_NOT_ENOUGH_MEMORY = 8;

        [StructLayout(LayoutKind.Explicit, Size = 16)]
        public struct EventData
        {
            [FieldOffset(0)]
            internal UInt64 DataPointer;
            [FieldOffset(8)]
            internal uint Size;
            [FieldOffset(12)]
            internal int Reserved;
        }

        [SuppressMessage(FxCop.Category.Security, FxCop.Rule.ReviewSuppressUnmanagedCodeSecurityUsage,
            Justification = "This PInvoke call has been reviewed")]
        [DllImport(KERNEL32, BestFitMapping = false, CharSet = CharSet.Auto)]
        [ResourceExposure(ResourceScope.Process)]
        [SecurityCritical]
        public static extern SafeWaitHandle CreateWaitableTimer(IntPtr mustBeZero, bool manualReset, string timerName);

        [SuppressMessage(FxCop.Category.Security, FxCop.Rule.ReviewSuppressUnmanagedCodeSecurityUsage,
            Justification = "This PInvoke call has been reviewed")]
        [DllImport(KERNEL32, ExactSpelling = true)]
        [ResourceExposure(ResourceScope.None)]
        [SecurityCritical]
        public static extern bool SetWaitableTimer(SafeWaitHandle handle, ref long dueTime, int period, IntPtr mustBeZero, IntPtr mustBeZeroAlso, bool resume);

        [SuppressMessage(FxCop.Category.Security, FxCop.Rule.ReviewSuppressUnmanagedCodeSecurityUsage,
            Justification = "This PInvoke call has been reviewed")]
        [DllImport(KERNEL32, SetLastError = true)]
        [ResourceExposure(ResourceScope.None)]
        [SecurityCritical]
        public static extern int QueryPerformanceCounter(out long time);

        [SuppressMessage(FxCop.Category.Security, FxCop.Rule.ReviewSuppressUnmanagedCodeSecurityUsage,
            Justification = "This PInvoke call has been reviewed")]
        [DllImport(KERNEL32, SetLastError = false)]
        [ResourceExposure(ResourceScope.None)]
        [SecurityCritical]
        public static extern uint GetSystemTimeAdjustment(
            [Out] out int adjustment,
            [Out] out uint increment,
            [Out] out uint adjustmentDisabled
            );

        [SuppressMessage(FxCop.Category.Security, FxCop.Rule.ReviewSuppressUnmanagedCodeSecurityUsage,
            Justification = "This PInvoke call has been reviewed")]
        [DllImport(KERNEL32, SetLastError = true)]
        [ResourceExposure(ResourceScope.None)]
        [SecurityCritical]
        private static extern void GetSystemTimeAsFileTime([Out] out FILETIME time);

        [SecurityCritical]
        public static void GetSystemTimeAsFileTime(out long time) {
            FILETIME fileTime;
            GetSystemTimeAsFileTime(out fileTime);
            time = 0;
            time |= (uint)fileTime.dwHighDateTime;
            time <<= sizeof(uint) * 8;
            time |= (uint)fileTime.dwLowDateTime;
        }

        [SuppressMessage(FxCop.Category.Security, FxCop.Rule.SpecifyMarshalingForPInvokeStringArguments, Justification = "")]
        [DllImport(KERNEL32, SetLastError = true, CharSet = CharSet.Auto)]
        [return: MarshalAs(UnmanagedType.Bool)]
        [ResourceExposure(ResourceScope.None)]
        [SecurityCritical]
        static extern bool GetComputerNameEx
            (
            [In] ComputerNameFormat nameType,
            [In, Out, MarshalAs(UnmanagedType.LPTStr)] StringBuilder lpBuffer,
            [In, Out] ref int size
            );

        [SecurityCritical]
        internal static string GetComputerName(ComputerNameFormat nameType)
        {
            int length = 0;
            if (!GetComputerNameEx(nameType, null, ref length))
            {
                int error = Marshal.GetLastWin32Error();

                if (error != ERROR_MORE_DATA)
                {
                    throw Fx.Exception.AsError(new System.ComponentModel.Win32Exception(error));
                }
            }

            if (length < 0)
            {
                Fx.AssertAndThrow("GetComputerName returned an invalid length: " + length);
            }

            StringBuilder stringBuilder = new StringBuilder(length);
            if (!GetComputerNameEx(nameType, stringBuilder, ref length))
            {
                int error = Marshal.GetLastWin32Error();
                throw Fx.Exception.AsError(new System.ComponentModel.Win32Exception(error));
            }

            return stringBuilder.ToString();
        }

        [SuppressMessage(FxCop.Category.Security, FxCop.Rule.ReviewSuppressUnmanagedCodeSecurityUsage,
            Justification = "This PInvoke call has been reviewed")]
        [DllImport(KERNEL32)]
        [ResourceExposure(ResourceScope.None)]
        [SecurityCritical]
        internal static extern bool IsDebuggerPresent();

        [SuppressMessage(FxCop.Category.Security, FxCop.Rule.ReviewSuppressUnmanagedCodeSecurityUsage,
            Justification = "This PInvoke call has been reviewed")]
        [DllImport(KERNEL32)]
        [ResourceExposure(ResourceScope.Process)]
        [SecurityCritical]
        internal static extern void DebugBreak();

        [SuppressMessage(FxCop.Category.Security, FxCop.Rule.ReviewSuppressUnmanagedCodeSecurityUsage,
            Justification = "This PInvoke call has been reviewed")]
        [DllImport(KERNEL32, CharSet = CharSet.Unicode)]
        [ResourceExposure(ResourceScope.Process)]
        [SecurityCritical]
        internal static extern void OutputDebugString(string lpOutputString);

        //
        // Callback
        //
        [SecurityCritical]
        internal unsafe delegate void EtwEnableCallback(
            [In] ref Guid sourceId,
            [In] int isEnabled,
            [In] byte level,
            [In] long matchAnyKeywords,
            [In] long matchAllKeywords,
            [In] void* filterData,
            [In] void* callbackContext
            );

        [SuppressMessage(FxCop.Category.Security, FxCop.Rule.ReviewSuppressUnmanagedCodeSecurityUsage,
            Justification = "This PInvoke call has been reviewed")]
        [DllImport(ADVAPI32, ExactSpelling = true, EntryPoint = "EventRegister", CharSet = System.Runtime.InteropServices.CharSet.Unicode)]
        [SecurityCritical]
        internal static extern unsafe uint EventRegister(
                    [In] ref Guid providerId,
                    [In]EtwEnableCallback enableCallback,
                    [In]void* callbackContext,
                    [In][Out]ref long registrationHandle
                    );

        [SuppressMessage(FxCop.Category.Security, FxCop.Rule.ReviewSuppressUnmanagedCodeSecurityUsage,
            Justification = "This PInvoke call has been reviewed")]
        [DllImport(ADVAPI32, ExactSpelling = true, EntryPoint = "EventUnregister", CharSet = System.Runtime.InteropServices.CharSet.Unicode)]
        [SecurityCritical]
        internal static extern uint EventUnregister([In] long registrationHandle);

        [SuppressMessage(FxCop.Category.Security, FxCop.Rule.ReviewSuppressUnmanagedCodeSecurityUsage,
            Justification = "This PInvoke call has been reviewed")]
        [DllImport(ADVAPI32, ExactSpelling = true, EntryPoint = "EventEnabled", CharSet = System.Runtime.InteropServices.CharSet.Unicode)]
        [SecurityCritical]
        internal static extern bool EventEnabled([In] long registrationHandle, [In] ref EventDescriptor eventDescriptor);

        [SuppressMessage(FxCop.Category.Security, FxCop.Rule.ReviewSuppressUnmanagedCodeSecurityUsage,
            Justification = "This PInvoke call has been reviewed")]
        [DllImport(ADVAPI32, ExactSpelling = true, EntryPoint = "EventWrite", CharSet = System.Runtime.InteropServices.CharSet.Unicode)]
        [SecurityCritical]
        internal static extern unsafe uint EventWrite(
                [In] long registrationHandle,
                [In] ref EventDescriptor eventDescriptor,
                [In] uint userDataCount,
                [In] EventData* userData
                );

        [SuppressMessage(FxCop.Category.Security, FxCop.Rule.ReviewSuppressUnmanagedCodeSecurityUsage,
            Justification = "This PInvoke call has been reviewed")]
        [DllImport(ADVAPI32, ExactSpelling = true, EntryPoint = "EventWriteTransfer", CharSet = System.Runtime.InteropServices.CharSet.Unicode)]
        [SecurityCritical]
        internal static extern unsafe uint EventWriteTransfer(
                [In] long registrationHandle,
                [In] ref EventDescriptor eventDescriptor,
                [In] ref Guid activityId,
                [In] ref Guid relatedActivityId,
                [In] uint userDataCount,
                [In] EventData* userData
                );

        [SuppressMessage(FxCop.Category.Security, FxCop.Rule.ReviewSuppressUnmanagedCodeSecurityUsage,
            Justification = "This PInvoke call has been reviewed")]
        [DllImport(ADVAPI32, ExactSpelling = true, EntryPoint = "EventWriteString", CharSet = System.Runtime.InteropServices.CharSet.Unicode)]
        [SecurityCritical]
        internal static extern unsafe uint EventWriteString(
                [In] long registrationHandle,
                [In] byte level,
                [In] long keywords,
                [In] char* message
                );

        [SuppressMessage(FxCop.Category.Security, FxCop.Rule.ReviewSuppressUnmanagedCodeSecurityUsage,
            Justification = "This PInvoke call has been reviewed")]
        [DllImport(ADVAPI32, ExactSpelling = true, EntryPoint = "EventActivityIdControl", CharSet = System.Runtime.InteropServices.CharSet.Unicode)]
        [SecurityCritical]
        internal static extern unsafe uint EventActivityIdControl([In] int ControlCode, [In][Out] ref Guid ActivityId);

        [SuppressMessage(FxCop.Category.Security, FxCop.Rule.ReviewSuppressUnmanagedCodeSecurityUsage,
            Justification = "This PInvoke call has been reviewed")]
        [DllImport(ADVAPI32, CharSet = System.Runtime.InteropServices.CharSet.Unicode, SetLastError = true)]
        [ResourceExposure(ResourceScope.None)]
        [SecurityCritical]
        [Fx.Tag.SecurityNote(Critical = "Accesses security critical type SafeHandle")]
        internal static extern bool ReportEvent(SafeHandle hEventLog, ushort type, ushort category,
                                                uint eventID, byte[] userSID, ushort numStrings, uint dataLen, HandleRef strings,
                                                byte[] rawData);

        [SuppressMessage(FxCop.Category.Security, FxCop.Rule.ReviewSuppressUnmanagedCodeSecurityUsage,
            Justification = "This PInvoke call has been reviewed")]
        [DllImport(ADVAPI32, CharSet = System.Runtime.InteropServices.CharSet.Unicode, SetLastError = true)]
        [ResourceExposure(ResourceScope.Machine)]
        [Fx.Tag.SecurityNote(Critical = "Returns security critical type SafeEventLogWriteHandle")]
        [SecurityCritical]
        internal static extern SafeEventLogWriteHandle RegisterEventSource(string uncServerName, string sourceName);
    }
}
