//------------------------------------------------------------------------------
// <copyright file="Logging.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System.Security.Permissions;

namespace System.Net.PeerToPeer
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using Microsoft.Win32.SafeHandles;
    using System.Security;
    using System.Runtime.InteropServices;
    using System.Runtime.ConstrainedExecution;
    using System.Threading;
    using System.Net.Sockets;
    using Microsoft.Win32;
    using System.Diagnostics;
    using System.IO;

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    internal struct PEER_PNRP_CLOUD_INFO
    {
        internal IntPtr pwzCloudName;
        internal UInt32 dwScope;
        internal UInt32 dwScopeId;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    internal struct PEER_PNRP_REGISTRATION_INFO
    {
        internal string pwszCloudName;
        internal string pwszPublishingIdentity;
        internal UInt32 cAddresses;
        internal IntPtr ArrayOfSOCKADDRIN6Pointers;
        internal ushort wport;
        internal string pwszComment;
        internal PEER_DATA payLoad;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    internal struct PEER_PNRP_ENDPOINT_INFO
    {
        internal IntPtr pwszPeerName;
        internal UInt32 cAddresses;
        internal IntPtr ArrayOfSOCKADDRIN6Pointers;
        internal IntPtr pwszComment;
        internal PEER_DATA payLoad;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    internal struct PEER_DATA
    {
        internal UInt32 cbPayload;
        internal IntPtr pbPayload;
    }

    [System.Security.SuppressUnmanagedCodeSecurityAttribute()]
    internal static class UnsafeP2PNativeMethods
    {
        internal const string P2P = "p2p.dll";

        [DllImport(P2P, CharSet = CharSet.Unicode)]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        internal extern static void PeerFreeData(IntPtr dataToFree);

        [SecurityCritical]
        [DllImport(P2P, CharSet = CharSet.Unicode)]
        internal extern static Int32 PeerPnrpGetCloudInfo(out UInt32 pNumClouds, out SafePeerData pArrayOfClouds);

        [DllImport(P2P, CharSet = CharSet.Unicode)]
        internal extern static Int32 PeerPnrpStartup(ushort versionRequired);

        [SecurityCritical]
        [DllImport(P2P, CharSet = CharSet.Unicode)]
        internal extern static Int32
            PeerCreatePeerName(string identity, string classfier, out SafePeerData peerName);

        //[DllImport(P2P, CharSet = CharSet.Unicode)]
        //internal extern static Int32 PeerCreatePeerName(string identity, string classfier, out SafePeerData peerName);

        [SecurityCritical]
        [DllImport(P2P, CharSet = CharSet.Unicode)]
        internal extern static Int32 PeerIdentityGetDefault(out SafePeerData defaultIdentity);

        /*
        [DllImport(P2P, CharSet = CharSet.Unicode)]
        internal extern static Int32 PeerIdentityCreate(string classifier, string friendlyName, IntPtr hCryptoProv, out SafePeerData defaultIdentity);
        */
        [SecurityCritical]
        [DllImport(P2P, CharSet = CharSet.Unicode)]
        internal extern static Int32 PeerNameToPeerHostName(string peerName, out SafePeerData peerHostName);

        [SecurityCritical]
        [DllImport(P2P, CharSet = CharSet.Unicode)]
        internal extern static Int32 PeerHostNameToPeerName(string peerHostName, out SafePeerData peerName);

        [SecurityCritical]
        [DllImport(P2P, CharSet = CharSet.Unicode)]
        public extern static Int32 PeerPnrpRegister(string pcwzPeerName,
                                                    ref PEER_PNRP_REGISTRATION_INFO registrationInfo,
                                                    out SafePeerNameUnregister handle);

        [DllImport(P2P, CharSet = CharSet.Unicode)]
        public extern static Int32 PeerPnrpUnregister(IntPtr handle);

        [SecurityCritical]
        [DllImport(P2P, CharSet = CharSet.Unicode)]
        public extern static Int32 PeerPnrpUpdateRegistration(SafePeerNameUnregister hRegistration,
                                                    ref PEER_PNRP_REGISTRATION_INFO registrationInfo);


        [SecurityCritical]
        [DllImport(P2P, CharSet = CharSet.Unicode)]
        public extern static Int32 PeerPnrpResolve(string pcwzPeerNAme,
                                                   string pcwzCloudName,
                                                   ref UInt32 pcEndPoints,
                                                   out SafePeerData pEndPoints);

        [SecurityCritical]
        [DllImport(P2P, CharSet = CharSet.Unicode)]
        public extern static Int32 PeerPnrpStartResolve(string pcwzPeerNAme,
                                                   string pcwzCloudName,
                                                   UInt32 cEndPoints,
                                                   SafeWaitHandle hEvent,
                                                   out SafePeerNameEndResolve safePeerNameEndResolve);

        [SecurityCritical]
        [DllImport(P2P, CharSet = CharSet.Unicode)]
        public extern static Int32 PeerPnrpGetEndpoint(IntPtr Handle,
                                                       out SafePeerData pEndPoint);

        [DllImport(P2P, CharSet = CharSet.Unicode)]
        public extern static Int32 PeerPnrpEndResolve(IntPtr Handle);

        private static object s_InternalSyncObject;
        private static volatile bool s_Initialized;
        private const int PNRP_VERSION = 2;
        private static object InternalSyncObject {
            get {
                if (s_InternalSyncObject == null) {
                    object o = new object();
                    Interlocked.CompareExchange(ref s_InternalSyncObject, o, null);
                }
                return s_InternalSyncObject;
            }
        }
        // <SecurityKernel Critical="True" Ring="0">
        // <CallsSuppressUnmanagedCode Name="PeerPnrpStartup(UInt16):Int32" />
        // <SatisfiesLinkDemand Name="Marshal.GetExceptionForHR(System.Int32):System.Exception" />
        // </SecurityKernel>
        [System.Security.SecurityCritical]
        internal static void PnrpStartup()
        {
            if (!s_Initialized) {
                lock (InternalSyncObject) {
                    if (!s_Initialized) {
                        Int32 result = PeerPnrpStartup(PNRP_VERSION);
                        if (result != 0) {
                            throw new PeerToPeerException(SR.GetString(SR.Pnrp_StartupFailed), Marshal.GetExceptionForHR(result));
                        }
                        s_Initialized = true;
                    }
                }
            }
        } //end of method PnrpStartup

    }


    // <SecurityKernel Critical="True" Ring="0">
    // <SatisfiesLinkDemand Name="SafeHandleZeroOrMinusOneIsInvalid" />
    // </SecurityKernel>
#pragma warning disable 618    // Have not migrated to v4 transparency yet
    [System.Security.SecurityCritical(System.Security.SecurityCriticalScope.Everything)]
#pragma warning restore 618
    [System.Security.SuppressUnmanagedCodeSecurityAttribute()]
    internal sealed class SafePeerData : SafeHandleZeroOrMinusOneIsInvalid
    {
        private SafePeerData() : base(true) { }
        //private SafePeerData(bool ownsHandle) : base(ownsHandle) { }
        internal string UnicodeString
        {
            get
            {
                return Marshal.PtrToStringUni(handle);
            }
        }
        protected override bool ReleaseHandle()
        {
            UnsafeP2PNativeMethods.PeerFreeData(handle);
            SetHandleAsInvalid();   //Mark it closed - This does not change the value of the handle it self
            SetHandle(IntPtr.Zero); //Mark it invalid - Change the value to Zero
            return true;
        }
    }


    // <SecurityKernel Critical="True" Ring="0">
    // <SatisfiesLinkDemand Name="SafeHandleZeroOrMinusOneIsInvalid" />
    // </SecurityKernel>
#pragma warning disable 618    // Have not migrated to v4 transparency yet
    [System.Security.SecurityCritical(System.Security.SecurityCriticalScope.Everything)]
#pragma warning restore 618
    [System.Security.SuppressUnmanagedCodeSecurityAttribute()]
    internal sealed class SafePeerNameUnregister : SafeHandleZeroOrMinusOneIsInvalid
    {
        internal SafePeerNameUnregister() : base(true) { }
        //internal SafePeerNameUnregister(bool ownsHandle) : base(ownsHandle) { }
        protected override bool ReleaseHandle()
        {
            UnsafeP2PNativeMethods.PeerPnrpUnregister(handle);
            SetHandleAsInvalid(); //Mark it closed - This does not change the value of the handle it self
            SetHandle(IntPtr.Zero); //Mark it invalid - Change the value to Zero
            return true;
        }
    }

    // <SecurityKernel Critical="True" Ring="0">
    // <SatisfiesLinkDemand Name="SafeHandleZeroOrMinusOneIsInvalid" />
    // </SecurityKernel>
#pragma warning disable 618    // Have not migrated to v4 transparency yet
    [System.Security.SecurityCritical(System.Security.SecurityCriticalScope.Everything)]
#pragma warning restore 618
    [System.Security.SuppressUnmanagedCodeSecurityAttribute()]
    internal sealed class SafePeerNameEndResolve : SafeHandleZeroOrMinusOneIsInvalid
    {
        internal SafePeerNameEndResolve() : base(true) { }
        //internal SafePeerNameEndResolve(bool ownsHandle) : base(ownsHandle) { }
        protected override bool ReleaseHandle()
        {
            UnsafeP2PNativeMethods.PeerPnrpEndResolve(handle);
            SetHandleAsInvalid(); //Mark it closed - This does not change the value of the handle it self
            SetHandle(IntPtr.Zero); //Mark it invalid - Change the value to Zero
            return true;
        }
    }

    /// <remarks>
    /// Determines whether P2P is installed
    /// Note static constructors are guaranteed to be
    /// run in a thread safe manner. so no locks are necessary
    /// </remarks>
    internal static class PeerToPeerOSHelper
    {
        private const string OSInstallTypeRegKey = @"Software\Microsoft\Windows NT\CurrentVersion";
        private const string OSInstallTypeRegKeyPath = @"HKEY_LOCAL_MACHINE\" + OSInstallTypeRegKey;
        private const string OSInstallTypeRegName = "InstallationType";
        private const string InstallTypeStringServerCore = "Server Core";

        private static bool s_supportsP2P = false;
        private static SafeLoadLibrary s_P2PLibrary = null;
        // <SecurityKernel Critical="True" Ring="0">
        // <CallsSuppressUnmanagedCode Name="UnsafeSystemNativeMethods.GetProcAddress(System.Net.SafeLoadLibrary,System.String):System.IntPtr" />
        // <SatisfiesLinkDemand Name="SafeHandle.get_IsInvalid():System.Boolean" />
        // <ReferencesCritical Name="Field: s_P2PLibrary" Ring="1" />
        // <ReferencesCritical Name="Method: SafeLoadLibrary.LoadLibraryEx(System.String):System.Net.SafeLoadLibrary" Ring="1" />
        // </SecurityKernel>
        [System.Security.SecurityCritical]
        static PeerToPeerOSHelper() {

            if (IsSupportedOS()) {

                // if OS is supported, but p2p.dll is not available, P2P is not supported (original behavior)
                string dllFileName = Path.Combine(Environment.SystemDirectory, UnsafeP2PNativeMethods.P2P);
                s_P2PLibrary = SafeLoadLibrary.LoadLibraryEx(dllFileName);
                if (!s_P2PLibrary.IsInvalid) {
                    IntPtr Address = UnsafeSystemNativeMethods.GetProcAddress(s_P2PLibrary, "PeerCreatePeerName");
                    if (Address != IntPtr.Zero) {
                        s_supportsP2P = true;
                    }
                }
            }
            //else --> the SafeLoadLibrary would have already been marked
            //          closed by the LoadLibraryEx call above.
        }

        [SecurityCritical]
        private static bool IsSupportedOS()
        {
            // extend this method when adding further OS/install type restrictions

            // P2P is not supported on Server Core installation type
            if (IsServerCore()) {
                return false;
            }

            return true;
        }

        [SecurityCritical]
        [RegistryPermission(SecurityAction.Assert, Read = OSInstallTypeRegKeyPath)]
        private static bool IsServerCore()
        {
            // This code does the same as System.Net.ComNetOS.GetWindowsInstallType(). Since ComNetOS is internal and
            // we don't want to add InternalsVisibleToAttribute to System.dll, we have to duplicate the code.
            try {
                using (RegistryKey installTypeKey = Registry.LocalMachine.OpenSubKey(OSInstallTypeRegKey)) {
                    string installType = installTypeKey.GetValue(OSInstallTypeRegName) as string;

                    if (string.IsNullOrEmpty(installType)) {
                        Logging.P2PTraceSource.TraceEvent(TraceEventType.Warning, 0,
                            SR.GetString(SR.P2P_empty_osinstalltype, OSInstallTypeRegKey + "\\" + OSInstallTypeRegName));
                    }
                    else {
                        if (String.Compare(installType, InstallTypeStringServerCore, StringComparison.OrdinalIgnoreCase) == 0) {
                            return true;
                        }
                    }
                }
            }
            catch (UnauthorizedAccessException e) {
                Logging.P2PTraceSource.TraceEvent(TraceEventType.Warning, 0,
                    SR.GetString(SR.P2P_cant_determine_osinstalltype, OSInstallTypeRegKey, e.Message));
            }
            catch (SecurityException e) {
                Logging.P2PTraceSource.TraceEvent(TraceEventType.Warning, 0,
                    SR.GetString(SR.P2P_cant_determine_osinstalltype, OSInstallTypeRegKey, e.Message));
            }

            return false;
        }

        internal static bool SupportsP2P {
            get {
                return s_supportsP2P;
            }
        }
        internal static IntPtr P2PModuleHandle
        {
            // <SecurityKernel Critical="True" Ring="0">
            // <SatisfiesLinkDemand Name="SafeHandle.get_IsClosed():System.Boolean" />
            // <SatisfiesLinkDemand Name="SafeHandle.get_IsInvalid():System.Boolean" />
            // <SatisfiesLinkDemand Name="SafeHandle.DangerousGetHandle():System.IntPtr" />
            // <ReferencesCritical Name="Field: s_P2PLibrary" Ring="1" />
            // </SecurityKernel>
            [System.Security.SecurityCritical]
            [SecurityPermissionAttribute(SecurityAction.LinkDemand, UnmanagedCode=true)]
            get
            {
                if (!s_P2PLibrary.IsClosed && !s_P2PLibrary.IsInvalid)
                    return s_P2PLibrary.DangerousGetHandle();
                return IntPtr.Zero;
            }
        }
    }

}
