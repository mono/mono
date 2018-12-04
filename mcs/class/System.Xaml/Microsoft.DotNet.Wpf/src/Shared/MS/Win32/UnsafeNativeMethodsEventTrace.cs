//------------------------------------------------------------------------------
// <copyright file="EventTraceNativeMethods.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------
//
#if !SILVERLIGHTXAML

using System;
using System.Security;
using System.Runtime.InteropServices;
#if SYSTEM_XAML
using MS.Internal.Xaml;
#else
using MS.Utility;
#endif

namespace MS.Win32
{
    //
    // ETW Tracing native methods
    //

    ///<SecurityNote>
    /// Critical as this code performs an elevation.
    ///</SecurityNote>
    [SecurityCritical(SecurityCriticalScope.Everything)]
    [SuppressUnmanagedCodeSecurity]
    internal static unsafe class ManifestEtw
    {
        //
        // ETW Methods
        //
        //
        // Callback
        //
        ///<SecurityNote>
        /// Critical - Accepts untrusted pointer argument
        ///</SecurityNote>
        [SecurityCritical]
        internal unsafe delegate void EtwEnableCallback(
            [In] ref Guid sourceId,
            [In] int isEnabled,
            [In] byte level,
            [In] long matchAnyKeywords,
            [In] long matchAllKeywords,
            [In] EVENT_FILTER_DESCRIPTOR* filterData,
            [In] void* callbackContext
            );

        //
        // Registration APIs
        //
        ///<SecurityNote>
        /// Critical as this code performs an elevation.
        ///</SecurityNote>
        [SecurityCritical]
        [SuppressUnmanagedCodeSecurity]
        [DllImport("Advapi32.dll", ExactSpelling = true, EntryPoint = "EventRegister", CharSet = System.Runtime.InteropServices.CharSet.Unicode)]
        internal static extern unsafe uint EventRegister(
                    [In] ref Guid providerId,
                    [In]EtwEnableCallback enableCallback,
                    [In]void* callbackContext,
                    [In][Out]ref ulong registrationHandle
                    );

        ///<SecurityNote>
        /// Critical as this code performs an elevation.
        ///</SecurityNote>
        [SecurityCritical]
        [SuppressUnmanagedCodeSecurity]
        [DllImport("Advapi32.dll", ExactSpelling = true, EntryPoint = "EventUnregister", CharSet = System.Runtime.InteropServices.CharSet.Unicode)]
        internal static extern uint EventUnregister([In] ulong registrationHandle);

        //
        // Writing (Publishing/Logging) APIs
        //
        ///<SecurityNote>
        /// Critical as this code performs an elevation.
        ///</SecurityNote>
        [SecurityCritical]
        [SuppressUnmanagedCodeSecurity]
        [DllImport("Advapi32.dll", ExactSpelling = true, EntryPoint = "EventWrite", CharSet = System.Runtime.InteropServices.CharSet.Unicode)]
        internal static extern unsafe uint EventWrite(
                [In] ulong registrationHandle,
                [In] ref EventDescriptor eventDescriptor,
                [In] uint userDataCount,
                [In] EventData* userData
                );

        [StructLayout(LayoutKind.Sequential)]
        unsafe internal struct EVENT_FILTER_DESCRIPTOR
        {
            public long Ptr;
            public int Size;
            public int Type;
        };


        [StructLayout(LayoutKind.Explicit, Size = 16)]
        internal struct EventDescriptor
        {
            [FieldOffset(0)]
            internal ushort Id;
            [FieldOffset(2)]
            internal byte Version;
            [FieldOffset(3)]
            internal byte Channel;
            [FieldOffset(4)]
            internal byte Level;
            [FieldOffset(5)]
            internal byte Opcode;
            [FieldOffset(6)]
            internal ushort Task;
            [FieldOffset(8)]
            internal long Keywords;
        }
    }

    [SuppressUnmanagedCodeSecurity]
    internal static unsafe class ClassicEtw
    {
        #region RegisterTraceGuidsW()
        // Support structs for RegisterTraceGuidsW
        [StructLayout(LayoutKind.Sequential)]
        internal struct TRACE_GUID_REGISTRATION
        {
            ///<SecurityNote>
            /// Critical:  Pointer field
            ///</SecurityNote>
            [SecurityCritical]
            internal unsafe Guid* Guid;
            
            ///<SecurityNote>
            /// Critical:  Pointer field
            ///</SecurityNote>
            [SecurityCritical]
            internal unsafe void* RegHandle;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct WNODE_HEADER
        {
            public UInt32 BufferSize;
            public UInt32 ProviderId;
            public UInt64 HistoricalContext;
            public UInt64 TimeStamp;
            public Guid Guid;
            public UInt32 ClientContext;
            public UInt32 Flags;
        };


        internal enum WMIDPREQUESTCODE
        {
            GetAllData = 0,
            GetSingleInstance = 1,
            SetSingleInstance = 2,
            SetSingleItem = 3,
            EnableEvents = 4,
            DisableEvents = 5,
            EnableCollection = 6,
            DisableCollection = 7,
            RegInfo = 8,
            ExecuteMethod = 9,
        };

         ///<SecurityNote>
        /// Critical:  Delegate that takes an unsafe pointer argument
        ///</SecurityNote>
        [SecurityCritical]
        internal unsafe delegate uint ControlCallback(WMIDPREQUESTCODE requestCode, IntPtr requestContext, IntPtr reserved, WNODE_HEADER* data);

        ///<SecurityNote>
        /// Critical as this code performs an elevation.
        ///</SecurityNote>
        [SecurityCritical]
        [SuppressUnmanagedCodeSecurity]
        [DllImport("Advapi32.dll", CharSet = CharSet.Unicode)]
        internal static extern uint RegisterTraceGuidsW([In] ControlCallback cbFunc, [In] IntPtr context, [In] ref Guid providerGuid, [In] int taskGuidCount, [In, Out] ref TRACE_GUID_REGISTRATION taskGuids, [In] string mofImagePath, [In] string mofResourceName, out ulong regHandle);
        #endregion // RegisterTraceGuidsW

        ///<SecurityNote>
        /// Critical as this code performs an elevation.
        ///</SecurityNote>
        [SecurityCritical]
        [SuppressUnmanagedCodeSecurity]
        [DllImport("Advapi32.dll")]
        internal static extern uint UnregisterTraceGuids(ulong regHandle);

        ///<SecurityNote>
        /// Critical as this code performs an elevation.
        ///</SecurityNote>
        [SecurityCritical]
        [SuppressUnmanagedCodeSecurity]
        [DllImport("Advapi32.dll")]
        internal static extern int GetTraceEnableFlags(ulong traceHandle);

        ///<SecurityNote>
        /// Critical as this code performs an elevation.
        ///</SecurityNote>
        [SecurityCritical]
        [SuppressUnmanagedCodeSecurity]
        [DllImport("Advapi32.dll")]
        internal static extern byte GetTraceEnableLevel(ulong traceHandle);

        ///<SecurityNote>
        /// Critical as this code performs an elevation.
        ///</SecurityNote>
        [SecurityCritical]
        [SuppressUnmanagedCodeSecurity]
        [DllImport("Advapi32.dll")]
        internal static extern long GetTraceLoggerHandle(WNODE_HEADER* data);

        #region TraceEvent()
        // Structures for TraceEvent API.

        // Constants for flags field.
        internal const int WNODE_FLAG_TRACED_GUID = 0x00020000;
        internal const int WNODE_FLAG_USE_MOF_PTR = 0x00100000;

        // Size is 48 = 0x30 bytes;
        [StructLayout(LayoutKind.Sequential)]
        internal struct EVENT_TRACE_HEADER
        {
            public ushort Size;
            public ushort FieldTypeFlags;   // holds our MarkerFlags too
            public byte Type;               // This is now called opcode.
            public byte Level;
            public ushort Version;
            public int ThreadId;
            public int ProcessId;
            public long TimeStamp;          // Offset 0x10
            public Guid Guid;               // Offset 0x18
            public uint ClientContext;      // Offset 0x28
            public uint Flags;              // Offset 0x2C
        }

        internal const int MAX_MOF_FIELDS = 16;
        [StructLayout(LayoutKind.Explicit, Size = 48 + 16 * MAX_MOF_FIELDS)]
        internal struct EVENT_HEADER
        {
            [FieldOffset(0)]
            public EVENT_TRACE_HEADER Header;
            [FieldOffset(48)]
            public EventData Data;         // Actually variable sized;
        }

        ///<SecurityNote>
        /// Critical as this code performs an elevation.
        ///</SecurityNote>
        [SecurityCritical]
        [SuppressUnmanagedCodeSecurity]
        [DllImport("Advapi32.dll")]
        internal static extern unsafe uint TraceEvent(ulong traceHandle, EVENT_HEADER* header);
        #endregion // TraceEvent()
    }
}

#endif //!SILVERLIGHTXAML

