//------------------------------------------------------------------------------
// <copyright file="UnsafeNativeMethods.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Net {
    using System.Runtime.InteropServices;
    using System.Runtime.CompilerServices;
    using System.Text;
    using System.Net.Sockets;
    using System.Net.Cache;
    using System.Threading;
    using System.ComponentModel;
    using System.Collections;
    using System.Runtime.ConstrainedExecution;
    using System.Security;
    using Microsoft.Win32.SafeHandles;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Security.Cryptography;
    using System.Security.Cryptography.X509Certificates;

    [System.Security.SuppressUnmanagedCodeSecurityAttribute()]
    internal static class UnsafeNclNativeMethods {

#if FEATURE_PAL
 #if !PLATFORM_UNIX
        internal const String DLLPREFIX = "";
        internal const String DLLSUFFIX = ".dll";
 #else // !PLATFORM_UNIX
  #if __APPLE__
        internal const String DLLPREFIX = "lib";
        internal const String DLLSUFFIX = ".dylib";
  #elif _AIX
        internal const String DLLPREFIX = "lib";
        internal const String DLLSUFFIX = ".a";
  #elif __hppa__ || IA64
        internal const String DLLPREFIX = "lib";
        internal const String DLLSUFFIX = ".sl";
  #else
        internal const String DLLPREFIX = "lib";
        internal const String DLLSUFFIX = ".so";
  #endif
 #endif // !PLATFORM_UNIX

        internal const String ROTOR_PAL   = DLLPREFIX + "rotor_pal" + DLLSUFFIX;
        internal const String ROTOR_PALRT = DLLPREFIX + "rotor_palrt" + DLLSUFFIX;
        private const String KERNEL32 = ROTOR_PAL;
#else
        private const string KERNEL32 = "kernel32.dll";
#endif // !FEATURE_PAL

#if !FEATURE_PAL
        private const string WS2_32 = "ws2_32.dll";
#else
        private const string WS2_32 = ExternDll.Kernel32; // Resolves to rotor_pal
#endif // !FEATURE_PAL

        private const string SECUR32 = "secur32.dll";
        private const string CRYPT32 = "crypt32.dll";
        private const string ADVAPI32 = "advapi32.dll";
        private const string HTTPAPI = "httpapi.dll";
        private const string SCHANNEL = "schannel.dll";
        private const string RASAPI32 = "rasapi32.dll";
        private const string WININET = "wininet.dll";
        private const string WINHTTP = "winhttp.dll";
        private const string BCRYPT = "bcrypt.dll";
        private const string USER32 = "user32.dll";

#if !FEATURE_PAL
        private const string OLE32 = "ole32.dll";        
#endif       
        [DllImport(KERNEL32)]
        internal static extern IntPtr CreateSemaphore([In] IntPtr lpSemaphoreAttributes, [In] int lInitialCount, [In] int lMaximumCount, [In] IntPtr lpName);

#if DEBUG
        [DllImport(KERNEL32)]
        internal static extern bool ReleaseSemaphore([In] IntPtr hSemaphore, [In] int lReleaseCount, [Out] out int lpPreviousCount);

#else
        [DllImport(KERNEL32)]
        internal static extern bool ReleaseSemaphore([In] IntPtr hSemaphore, [In] int lReleaseCount, [In] IntPtr lpPreviousCount);
#endif

        // 
        internal static class ErrorCodes
        {
            internal const uint ERROR_SUCCESS               = 0;
            internal const uint ERROR_HANDLE_EOF            = 38;
            internal const uint ERROR_NOT_SUPPORTED         = 50;
            internal const uint ERROR_INVALID_PARAMETER     = 87;
            internal const uint ERROR_ALREADY_EXISTS        = 183;
            internal const uint ERROR_MORE_DATA             = 234;
            internal const uint ERROR_OPERATION_ABORTED     = 995;
            internal const uint ERROR_IO_PENDING            = 997;
            internal const uint ERROR_NOT_FOUND             = 1168;
            internal const uint ERROR_CONNECTION_INVALID    = 1229;
        }

        internal static class NTStatus
        {
            internal const uint STATUS_SUCCESS               = 0x00000000;
            internal const uint STATUS_OBJECT_NAME_NOT_FOUND = 0xC0000034;
        }

        [DllImport(KERNEL32, ExactSpelling=true, CallingConvention=CallingConvention.StdCall, SetLastError=true)]
        internal static extern uint GetCurrentThreadId();


        [DllImport(KERNEL32, ExactSpelling = true, CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        internal static unsafe extern uint CancelIoEx(CriticalHandle handle, NativeOverlapped* overlapped);

        [SuppressMessage("Microsoft.Security", "CA2118:ReviewSuppressUnmanagedCodeSecurityUsage", Justification = "Implementation requires unmanaged code usage")]
        [DllImport(KERNEL32, ExactSpelling = true, CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        internal static unsafe extern bool SetFileCompletionNotificationModes(CriticalHandle handle, FileCompletionNotificationModes modes);

        [Flags]
        internal enum FileCompletionNotificationModes : byte
        {
            None = 0,
            SkipCompletionPortOnSuccess = 1,
            SkipSetEventOnHandle = 2
        }

#if STRESS || !DEBUG
        [DllImport(KERNEL32, ExactSpelling = true)]
        internal static extern void DebugBreak();
#endif

#if FEATURE_PAL
        [DllImport(ROTOR_PALRT, CharSet=CharSet.Unicode, SetLastError=true, EntryPoint="PAL_FetchConfigurationStringW")]
        internal static extern bool FetchConfigurationString(bool perMachine, String parameterName, StringBuilder parameterValue, int parameterValueLength);
#endif // FEATURE_PAL

#if !FEATURE_PAL
        [SuppressUnmanagedCodeSecurity]
        internal unsafe static class RegistryHelper
        {
            internal const uint REG_NOTIFY_CHANGE_LAST_SET = 4;
            internal const uint REG_BINARY = 3;
            internal const uint KEY_READ = 0x00020019;

            internal static readonly IntPtr HKEY_CURRENT_USER = (IntPtr) unchecked((int) 0x80000001L);
            internal static readonly IntPtr HKEY_LOCAL_MACHINE = (IntPtr) unchecked((int) 0x80000002L);

            // RELIABILITY:
            // this out parameter in this API, resultSubKey, is an allocated handle to a registry sub-key.
            // it must be a SafeHandle so we can guarantee that it is released correctly and never leaked.
            [DllImport(ADVAPI32, BestFitMapping=false, ThrowOnUnmappableChar=true, ExactSpelling=false, CharSet=CharSet.Auto, SetLastError=true)]
            internal static extern uint RegOpenKeyEx(IntPtr key, string subKey, uint ulOptions, uint samDesired, out SafeRegistryHandle resultSubKey);

            [DllImport(ADVAPI32, BestFitMapping=false, ThrowOnUnmappableChar=true, ExactSpelling=false, CharSet=CharSet.Auto, SetLastError=true)]
            internal static extern uint RegOpenKeyEx(SafeRegistryHandle key, string subKey, uint ulOptions, uint samDesired, out SafeRegistryHandle resultSubKey);

            [DllImport(ADVAPI32, ExactSpelling=true, SetLastError=true)]
            [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
            internal static extern uint RegCloseKey(IntPtr key);

            [DllImport(ADVAPI32, ExactSpelling=true, SetLastError=true)]
            internal static extern uint RegNotifyChangeKeyValue(SafeRegistryHandle key, bool watchSubTree, uint notifyFilter, SafeWaitHandle regEvent, bool async);

            [DllImport(ADVAPI32, ExactSpelling=true, SetLastError=true)]
            internal static extern uint RegOpenCurrentUser(uint samDesired, out SafeRegistryHandle resultKey);

            [DllImport(ADVAPI32, BestFitMapping=false, ThrowOnUnmappableChar=true, ExactSpelling=false, CharSet=CharSet.Auto, SetLastError=true)]
            internal static extern uint RegQueryValueEx(SafeRegistryHandle key, string valueName, IntPtr reserved, out uint type, [Out] byte[] data, [In][Out] ref uint size);
        }

        [System.Security.SuppressUnmanagedCodeSecurityAttribute()]
        internal unsafe class RasHelper
        {
            private static readonly bool s_RasSupported;

            private ManualResetEvent m_RasEvent;
            private bool m_Suppressed;

            /* Consider removing
            internal void Close()
            {
                ManualResetEvent rasEvent = m_RasEvent;
                m_RasEvent = null;
                m_Suppressed = false;
                if (rasEvent != null)
                {
                    rasEvent.Close();
                }
            }
            */

            static RasHelper()
            {
                if (ComNetOS.InstallationType == WindowsInstallationType.ServerCore)
                {
                    // if InstallationType == WindowsSku.Unknown we'll support RAS, since older OS (like XP) 
                    // don't have an "Installation Type" Registry value
                    s_RasSupported = false;
                }
                else
                {
                    s_RasSupported = true;
                }

                if (Logging.On)
                    Logging.PrintInfo(Logging.Web, SR.GetString(SR.net_log_proxy_ras_supported, s_RasSupported));
            }

            internal RasHelper()
            {
                if (!s_RasSupported) 
                {
                    throw new InvalidOperationException(SR.GetString(SR.net_log_proxy_ras_notsupported_exception));
                }

                m_RasEvent = new ManualResetEvent(false);

                // Use -1 as a handle, to receive all RAS notifications for the local machine.
                uint statusCode = RasConnectionNotification((IntPtr)(-1), m_RasEvent.SafeWaitHandle, RASCN_Connection | RASCN_Disconnection);

                GlobalLog.Print("RasHelper::RasHelper() RasConnectionNotification() statusCode:" + statusCode);
                if (statusCode != 0) 
                {
                    GlobalLog.Print("RasHelper::RasHelper() RasConnectionNotification() Marshal.GetLastWin32Error():" + Marshal.GetLastWin32Error());
                    m_Suppressed = true;
                    m_RasEvent.Close();
                    m_RasEvent = null;
                }
            }

            internal static bool RasSupported
            {
                get { return s_RasSupported; }
            }

            internal bool HasChanged
            {
                get
                {
                    if (m_Suppressed)
                    {
                        return false;
                    }

                    ManualResetEvent rasEvent = m_RasEvent;
                    if (rasEvent == null)
                    {
                        throw new ObjectDisposedException(GetType().FullName);
                    }
                    return rasEvent.WaitOne(0, false);
                }
            }

            internal void Reset()
            {
                if (!m_Suppressed)
                {
                    ManualResetEvent rasEvent = m_RasEvent;
                    if (rasEvent == null)
                    {
                        throw new ObjectDisposedException(GetType().FullName);
                    }
                    rasEvent.Reset();
                }
            }

            internal static string GetCurrentConnectoid()
            {
                uint dwSize = (uint) Marshal.SizeOf(typeof(RASCONN));
                GlobalLog.Print("RasHelper::GetCurrentConnectoid() using struct size dwSize:" + dwSize);

                if (!s_RasSupported) 
                {
                    // if RAS is not supported, behave as if no dial-up/VPN connection is in use
                    // (which is actually the case, since without RAS dial-up/VPN doesn't work)
                    return null;
                }

                uint count = 4;
                uint statusCode = 0;
                RASCONN[] connections = null;
                while (true)
                {
                    uint cb = checked(dwSize * count);
                    connections = new RASCONN[count];
                    connections[0].dwSize = dwSize;
                    statusCode = RasEnumConnections(connections, ref cb, ref count);
                    GlobalLog.Print("RasHelper::GetCurrentConnectoid() called RasEnumConnections() count:" + count + " statusCode: " + statusCode + " cb:" + cb);
                    if (statusCode != ERROR_BUFFER_TOO_SMALL)
                    {
                        break;
                    }
                    count = checked(cb + dwSize - 1) / dwSize;
                }

                if (count == 0 || statusCode != 0)
                {
                    return null;
                }

                for (uint i=0; i < count; i++)
                {
                    GlobalLog.Print("RasHelper::GetCurrentConnectoid() RASCONN[" + i + "]");
                    GlobalLog.Print("RasHelper::GetCurrentConnectoid() RASCONN[" + i + "].dwSize: " + connections[i].dwSize);
                    GlobalLog.Print("RasHelper::GetCurrentConnectoid() RASCONN[" + i + "].hrasconn: " + connections[i].hrasconn);
                    GlobalLog.Print("RasHelper::GetCurrentConnectoid() RASCONN[" + i + "].szEntryName: " + connections[i].szEntryName);
                    GlobalLog.Print("RasHelper::GetCurrentConnectoid() RASCONN[" + i + "].szDeviceType: " + connections[i].szDeviceType);
                    GlobalLog.Print("RasHelper::GetCurrentConnectoid() RASCONN[" + i + "].szDeviceName: " + connections[i].szDeviceName);

                    RASCONNSTATUS connectionStatus = new RASCONNSTATUS();
                    connectionStatus.dwSize = (uint)Marshal.SizeOf(connectionStatus);
                    // RELIABILITY:
                    // the 'hrasconn' field is an IntPtr because it's defined as a handle
                    // that said, its use is that of a opaque ID, so we're not
                    // allocating anything that needs to be released for reliability.
                    statusCode = RasGetConnectStatus(connections[i].hrasconn, ref connectionStatus);
                    GlobalLog.Print("RasHelper::GetCurrentConnectoid() called RasGetConnectStatus() statusCode: " + statusCode + " dwSize: " + connectionStatus.dwSize);
                    if (statusCode==0) {
                        GlobalLog.Print("RasHelper::GetCurrentConnectoid() RASCONN[" + i + "].RASCONNSTATUS.dwSize: " + connectionStatus.dwSize);
                        GlobalLog.Print("RasHelper::GetCurrentConnectoid() RASCONN[" + i + "].RASCONNSTATUS.rasconnstate: " + connectionStatus.rasconnstate);
                        GlobalLog.Print("RasHelper::GetCurrentConnectoid() RASCONN[" + i + "].RASCONNSTATUS.dwError: " + connectionStatus.dwError);
                        GlobalLog.Print("RasHelper::GetCurrentConnectoid() RASCONN[" + i + "].RASCONNSTATUS.szDeviceType: " + connectionStatus.szDeviceType);
                        GlobalLog.Print("RasHelper::GetCurrentConnectoid() RASCONN[" + i + "].RASCONNSTATUS.szDeviceName: " + connectionStatus.szDeviceName);
                        if (connectionStatus.rasconnstate==RASCONNSTATE.RASCS_Connected) {
                            return connections[i].szEntryName;
                        }
                    }
                }

                return null;
            }


            [DllImport(RASAPI32, ExactSpelling = false, CharSet = CharSet.Auto, BestFitMapping = false, ThrowOnUnmappableChar = true, SetLastError = false)]
            private static extern uint RasEnumConnections([In, Out] RASCONN[] lprasconn, ref uint lpcb, ref uint lpcConnections);

            [DllImport(RASAPI32, ExactSpelling=false, CharSet=CharSet.Auto, BestFitMapping=false, ThrowOnUnmappableChar=true, SetLastError=false)]
            private static extern uint RasGetConnectStatus([In] IntPtr hrasconn, [In, Out] ref RASCONNSTATUS lprasconnstatus);

            [DllImport(RASAPI32, ExactSpelling=false, CharSet=CharSet.Auto, BestFitMapping=false, ThrowOnUnmappableChar=true, SetLastError=false)]
            private static extern uint RasConnectionNotification([In] IntPtr hrasconn, [In] SafeWaitHandle hEvent, uint dwFlags);

            const int RAS_MaxEntryName = 256;
            const int RAS_MaxDeviceType = 16;
            const int RAS_MaxDeviceName = 128;
            const int RAS_MaxPhoneNumber = 128;
            const int RAS_MaxCallbackNumber = 128;

            const uint RASCN_Connection = 0x00000001;
            const uint RASCN_Disconnection = 0x00000002;

            const int UNLEN = 256;
            const int PWLEN = 256;
            const int DNLEN = 15;

            const int MAX_PATH = 260;

            const uint RASBASE = 600;
            const uint ERROR_DIAL_ALREADY_IN_PROGRESS = (RASBASE+156);
            const uint ERROR_BUFFER_TOO_SMALL = (RASBASE+3);

            [StructLayout(LayoutKind.Sequential, Pack=4, CharSet=CharSet.Auto)]
            struct RASCONN {
                internal uint dwSize;
                internal IntPtr hrasconn;
                [MarshalAs(UnmanagedType.ByValTStr, SizeConst=RAS_MaxEntryName + 1)]
                internal string szEntryName;
                [MarshalAs(UnmanagedType.ByValTStr, SizeConst=RAS_MaxDeviceType + 1)]
                internal string szDeviceType;
                [MarshalAs(UnmanagedType.ByValTStr, SizeConst=RAS_MaxDeviceName + 1)]
                internal string szDeviceName;

/* None of these are supported on Windows 98.
   MSDN lies twice: there is no dwSessionId at all, and szPhonebook and dwSubEntry are not on Win98.
                [MarshalAs(UnmanagedType.ByValTStr, SizeConst=MAX_PATH)]
                internal string szPhonebook;
                internal uint dwSubEntry;
                internal Guid guidEntry;
                internal uint dwFlags;
                internal ulong luid;
*/
            }

            [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto)]
            struct RASCONNSTATUS {
                internal uint dwSize;
                internal RASCONNSTATE rasconnstate;
                internal uint dwError;
                [MarshalAs(UnmanagedType.ByValTStr, SizeConst=RAS_MaxDeviceType + 1)]
                internal string szDeviceType;
                [MarshalAs(UnmanagedType.ByValTStr, SizeConst=RAS_MaxDeviceName + 1)]
                internal string szDeviceName;
/* Not supported on Windows 98.
                [MarshalAs(UnmanagedType.ByValTStr, SizeConst=RAS_MaxPhoneNumber + 1)]
                internal string szPhoneNumber;
*/
            }

            [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto)]
            struct RASDIALPARAMS {
                internal uint dwSize;
                [MarshalAs(UnmanagedType.ByValTStr, SizeConst=RAS_MaxEntryName + 1)]
                internal string szEntryName;
                [MarshalAs(UnmanagedType.ByValTStr, SizeConst=RAS_MaxPhoneNumber + 1)]
                internal string szPhoneNumber;
                [MarshalAs(UnmanagedType.ByValTStr, SizeConst=RAS_MaxCallbackNumber + 1)]
                internal string szCallbackNumber;
                [MarshalAs(UnmanagedType.ByValTStr, SizeConst=UNLEN + 1)]
                internal string szUserName;
                [MarshalAs(UnmanagedType.ByValTStr, SizeConst=PWLEN + 1)]
                internal string szPassword;
                [MarshalAs(UnmanagedType.ByValTStr, SizeConst=DNLEN + 1)]
                internal string szDomain;
/* Not supported on Windows 98.
                internal uint dwSubEntry;
                internal uint dwCallbackId;
*/
            }

            const int RASCS_PAUSED = 0x1000;
            const int RASCS_DONE = 0x2000;

            enum RASCONNSTATE {
                RASCS_OpenPort = 0,
                RASCS_PortOpened,
                RASCS_ConnectDevice,
                RASCS_DeviceConnected,
                RASCS_AllDevicesConnected,
                RASCS_Authenticate,
                RASCS_AuthNotify,
                RASCS_AuthRetry,
                RASCS_AuthCallback,
                RASCS_AuthChangePassword,
                RASCS_AuthProject,
                RASCS_AuthLinkSpeed,
                RASCS_AuthAck,
                RASCS_ReAuthenticate,
                RASCS_Authenticated,
                RASCS_PrepareForCallback,
                RASCS_WaitForModemReset,
                RASCS_WaitForCallback,
                RASCS_Projected,
                RASCS_StartAuthentication,    // Windows 95 only
                RASCS_CallbackComplete,       // Windows 95 only
                RASCS_LogonNetwork,           // Windows 95 only
                RASCS_SubEntryConnected,
                RASCS_SubEntryDisconnected,
                RASCS_Interactive = RASCS_PAUSED,
                RASCS_RetryAuthentication,
                RASCS_CallbackSetByCaller,
                RASCS_PasswordExpired,
                RASCS_InvokeEapUI,
                RASCS_Connected = RASCS_DONE,
                RASCS_Disconnected
            }
        }

        [System.Security.SuppressUnmanagedCodeSecurityAttribute()]
        internal static class SafeNetHandles_SECURITY {

            [DllImport(SECUR32, ExactSpelling=true, SetLastError=true)]
            [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
            internal static extern int FreeContextBuffer(
                [In] IntPtr contextBuffer);


            [DllImport(SECUR32, ExactSpelling=true, SetLastError=true)]
            [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
            internal static extern int FreeCredentialsHandle(
                  ref  SSPIHandle handlePtr
                  );

            [DllImport(SECUR32, ExactSpelling=true, SetLastError=true)]
            [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
            internal static extern int DeleteSecurityContext(
                  ref  SSPIHandle handlePtr
                  );

            [DllImport(SECUR32, ExactSpelling=true, SetLastError=true)]
            [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
            internal unsafe static extern int AcceptSecurityContext(
                      ref  SSPIHandle       credentialHandle,
                      [In] void*            inContextPtr,
                      [In] SecurityBufferDescriptor inputBuffer,
                      [In] ContextFlags     inFlags,
                      [In] Endianness       endianness,
                      ref  SSPIHandle       outContextPtr,
                      [In, Out] SecurityBufferDescriptor outputBuffer,
                      [In, Out] ref ContextFlags attributes,
                      out  long timeStamp
                      );

            [DllImport(SECUR32, ExactSpelling=true, SetLastError=true)]
            [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
            internal unsafe static extern int QueryContextAttributesW(
                ref SSPIHandle contextHandle,
                [In] ContextAttribute attribute,
                [In] void* buffer);

            [DllImport(SECUR32, ExactSpelling = true, SetLastError = true)]
            [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
            internal unsafe static extern int SetContextAttributesW(
                ref SSPIHandle contextHandle,
                [In] ContextAttribute attribute,
                [In] byte[] buffer,
                [In] int bufferSize);

            [DllImport(SECUR32, ExactSpelling=true, SetLastError=true)]
            internal static extern int EnumerateSecurityPackagesW(
                [Out] out int pkgnum,
                [Out] out SafeFreeContextBuffer_SECURITY handle);

            [DllImport(SECUR32, ExactSpelling=true, CharSet=CharSet.Unicode, SetLastError=true)]
            internal unsafe static extern int AcquireCredentialsHandleW(
                      [In] string principal,
                      [In] string moduleName,
                      [In] int usage,
                      [In] void* logonID,
                      [In] ref AuthIdentity authdata,
                      [In] void* keyCallback,
                      [In] void* keyArgument,
                      ref  SSPIHandle handlePtr,
                      [Out] out long timeStamp
                      );

            [DllImport(SECUR32, ExactSpelling=true, CharSet=CharSet.Unicode, SetLastError=true)]
            internal unsafe static extern int AcquireCredentialsHandleW(
                      [In] string principal,
                      [In] string moduleName,
                      [In] int usage,
                      [In] void* logonID,
                      [In] IntPtr zero,
                      [In] void* keyCallback,
                      [In] void* keyArgument,
                      ref  SSPIHandle handlePtr,
                      [Out] out long timeStamp
                      );

            //  Win7+
            [DllImport(SECUR32, ExactSpelling = true, CharSet = CharSet.Unicode, SetLastError = true)]
            internal unsafe static extern int AcquireCredentialsHandleW(
                      [In] string principal,
                      [In] string moduleName,
                      [In] int usage,
                      [In] void* logonID,
                      [In] SafeSspiAuthDataHandle authdata,
                      [In] void* keyCallback,
                      [In] void* keyArgument,
                      ref  SSPIHandle handlePtr,
                      [Out] out long timeStamp
                      );

            [DllImport(SECUR32, ExactSpelling=true, CharSet=CharSet.Unicode, SetLastError=true)]
            internal unsafe  static extern int AcquireCredentialsHandleW(
                      [In] string principal,
                      [In] string moduleName,
                      [In] int usage,
                      [In] void* logonID,
                      [In] ref SecureCredential authData,
                      [In] void* keyCallback,
                      [In] void* keyArgument,
                      ref  SSPIHandle handlePtr,
                      [Out] out long timeStamp
                      );

            [DllImport(SECUR32, ExactSpelling=true, SetLastError=true)]
            [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
            internal unsafe static extern int InitializeSecurityContextW(
                      ref  SSPIHandle       credentialHandle,
                      [In] void*            inContextPtr,
                      [In] byte*            targetName,
                      [In] ContextFlags     inFlags,
                      [In] int              reservedI,
                      [In] Endianness       endianness,
                      [In] SecurityBufferDescriptor inputBuffer,
                      [In] int              reservedII,
                      ref  SSPIHandle       outContextPtr,
                      [In, Out] SecurityBufferDescriptor outputBuffer,
                      [In, Out] ref ContextFlags attributes,
                      out  long timeStamp
                      );

            [DllImport(SECUR32, ExactSpelling=true, SetLastError=true)]
            [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
            internal unsafe static extern int CompleteAuthToken(
                      [In] void*            inContextPtr,
                      [In, Out] SecurityBufferDescriptor inputBuffers
                      );

        }
        
#endif // !FEATURE_PAL

        // Because the regular SafeNetHandles has a LocalAlloc with a different return type.
        [System.Security.SuppressUnmanagedCodeSecurityAttribute()]
        internal static class SafeNetHandlesSafeOverlappedFree {
            [DllImport(ExternDll.Kernel32, ExactSpelling=true, SetLastError=true)]
            internal static extern SafeOverlappedFree LocalAlloc(int uFlags, UIntPtr sizetdwBytes);
        }

#if !FEATURE_PAL
        // Because the regular SafeNetHandles tries to bind this MustRun method on type initialization, failing
        // on legacy platforms.
        [System.Security.SuppressUnmanagedCodeSecurityAttribute()]
        internal static class SafeNetHandlesXPOrLater {
            [DllImport(WS2_32, ExactSpelling = true, CharSet = CharSet.Unicode, BestFitMapping = false, ThrowOnUnmappableChar = true, SetLastError = true)]
            internal static extern int GetAddrInfoW(
                [In] string nodename,
                [In] string servicename,
                [In] ref AddressInfo hints,
                [Out] out SafeFreeAddrInfo handle
                );

            [DllImport(WS2_32, ExactSpelling = true, SetLastError = true)]
            [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
            internal static extern void freeaddrinfo([In] IntPtr info );
        }
#endif

        [System.Security.SuppressUnmanagedCodeSecurityAttribute()]
        internal static class SafeNetHandles {

    #if !FEATURE_PAL
            [DllImport(SECUR32, ExactSpelling=true, SetLastError=true)]
            internal static extern int QuerySecurityContextToken(ref SSPIHandle phContext, [Out] out SafeCloseHandle handle);

            [SuppressMessage("Microsoft.Security", "CA2118:ReviewSuppressUnmanagedCodeSecurityUsage", Justification = "Implementation requires unmanaged code usage")]
            [DllImport(HTTPAPI, ExactSpelling = true, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode, SetLastError = true)]
            internal static extern unsafe uint HttpCreateRequestQueue(HttpApi.HTTPAPI_VERSION version, string pName,
                Microsoft.Win32.NativeMethods.SECURITY_ATTRIBUTES pSecurityAttributes, uint flags, out HttpRequestQueueV2Handle pReqQueueHandle);

            [DllImport(HTTPAPI, ExactSpelling = true, CallingConvention = CallingConvention.StdCall, SetLastError = true)]
            internal static extern unsafe uint HttpCloseRequestQueue(IntPtr pReqQueueHandle);

    #endif

            [DllImport(ExternDll.Kernel32, ExactSpelling=true, SetLastError=true)]
            [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
            internal static extern bool CloseHandle(IntPtr handle);

            [DllImport(ExternDll.Kernel32, ExactSpelling=true, SetLastError=true)]
            internal static extern SafeLocalFree LocalAlloc(int uFlags, UIntPtr sizetdwBytes);

            [DllImport(ExternDll.Kernel32, EntryPoint = "LocalAlloc", SetLastError = true)]
            internal static extern SafeLocalFreeChannelBinding LocalAllocChannelBinding(int uFlags, UIntPtr sizetdwBytes);

            [DllImport(ExternDll.Kernel32, ExactSpelling=true, SetLastError=true)]
            [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
            internal static extern IntPtr LocalFree(IntPtr handle);

#if !FEATURE_PAL

            [DllImport(KERNEL32, ExactSpelling=true, CharSet=CharSet.Unicode, SetLastError=true)]
            internal static extern unsafe SafeLoadLibrary LoadLibraryExW([In] string lpwLibFileName, [In] void* hFile, [In] uint dwFlags);
#endif // !FEATURE_PAL


            [DllImport(KERNEL32, ExactSpelling=true, SetLastError=true)]
            [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
            internal static extern unsafe bool FreeLibrary([In] IntPtr hModule);

#if !FEATURE_PAL

            /*
            // Consider removing.
            [DllImport(CRYPT32, ExactSpelling=true, SetLastError=true)]
            internal static extern  bool CertGetCertificateChain(
                [In] IntPtr                 chainEngine,
                [In] SafeFreeCertContext    certContext,
                [In] IntPtr                 time,
                [In] SafeCloseStore         additionalStore,
                [In] ref ChainParameters    certCP,
                [In] int                    flags,
                [In] IntPtr                 reserved,
                [Out] out SafeFreeCertChain  chainContext);
            */

            [DllImport(CRYPT32, ExactSpelling=true, SetLastError=true)]
            [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
            internal static extern void CertFreeCertificateChain(
                [In] IntPtr pChainContext);

            [DllImport(CRYPT32, ExactSpelling = true, SetLastError = true)]
            [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
            internal static extern void CertFreeCertificateChainList(
                [In] IntPtr ppChainContext);

            [DllImport(CRYPT32, ExactSpelling=true, SetLastError=true)]
            [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
            internal static extern bool CertFreeCertificateContext(      // Suppressing returned status check, it's always==TRUE,
                [In] IntPtr certContext);

            /*
            // Consider removing.
            [DllImport(CRYPT32, ExactSpelling=true, SetLastError=true)]
            [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
            internal static extern bool CertCloseStore(
                [In] IntPtr hCertStore,
                [In] int dwFlags);
            */

#endif // !FEATURE_PAL

            [DllImport(ExternDll.Kernel32, ExactSpelling=true, SetLastError=true)]
            [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
            internal static extern IntPtr GlobalFree(IntPtr handle);

            // Blocking call - requires IntPtr instead of SafeCloseSocket.
            [DllImport(WS2_32, ExactSpelling=true, SetLastError=true)]
            internal static extern SafeCloseSocket.InnerSafeCloseSocket accept(
                                                  [In] IntPtr socketHandle,
                                                  [Out] byte[] socketAddress,
                                                  [In, Out] ref int socketAddressSize
                                                  );

            [DllImport(WS2_32, ExactSpelling=true, SetLastError=true)]
            [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
            internal static extern SocketError closesocket(
                                                  [In] IntPtr socketHandle
                                                  );

            [DllImport(WS2_32, ExactSpelling=true, SetLastError=true)]
            [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
            internal static extern SocketError ioctlsocket(
                                                [In] IntPtr handle,
                                                [In] int cmd,
                                                [In, Out] ref int argp
                                                );

            [DllImport(WS2_32, ExactSpelling=true, SetLastError=true)]
            [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
            internal static extern SocketError WSAEventSelect(
                                                     [In] IntPtr handle,
                                                     [In] IntPtr Event,
                                                     [In] AsyncEventBits NetworkEvents
                                                     );

            [DllImport(WS2_32, ExactSpelling=true, SetLastError=true)]
            [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
            internal static extern SocketError setsockopt(
                                               [In] IntPtr handle,
                                               [In] SocketOptionLevel optionLevel,
                                               [In] SocketOptionName optionName,
                                               [In] ref Linger linger,
                                               [In] int optionLength
                                               );

            /* Consider removing
            [DllImport(WS2_32, ExactSpelling=true, SetLastError=true)]
            [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
            internal static extern SocketError setsockopt(
                                               [In] IntPtr handle,
                                               [In] SocketOptionLevel optionLevel,
                                               [In] SocketOptionName optionName,
                                               [In] ref int optionValue,
                                               [In] int optionLength
                                               );
            */

#if !FEATURE_PAL
            [DllImport(WININET, ExactSpelling=true, SetLastError = true)]
            [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
            unsafe internal static extern bool RetrieveUrlCacheEntryFileW(
                                            [In]      char*     urlName,
                                            [In]      byte*     entryPtr,               //was [Out]
                                            [In, Out] ref int   entryBufSize,
                                            [In]      int       dwReserved              //must be 0
                                            );

            [DllImport(WININET, ExactSpelling=true, SetLastError = true)]
            [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
            unsafe internal static extern bool UnlockUrlCacheEntryFileW(
                                            [In]    char*       urlName,
                                            [In]    int         dwReserved                  //must be 0
                                            );
#endif // !FEATURE_PAL
        }

        //
        // UnsafeNclNativeMethods.OSSOCK class contains all Unsafe() calls and should all be protected
        // by the appropriate SocketPermission() to connect/accept to/from remote
        // peers over the network and to perform name resolution.
        // te following calls deal mainly with:
        // 1) socket calls
        // 2) DNS calls
        //

        //
        // here's a brief explanation of all possible decorations we use for PInvoke.
        // these are used in such a way that we hope to gain maximum performance from the
        // unmanaged/managed/unmanaged transition we need to undergo when calling into winsock:
        //
        // [In] (Note: this is similar to what msdn will show)
        // the managed data will be marshalled so that the unmanaged function can read it but even
        // if it is changed in unmanaged world, the changes won't be propagated to the managed data
        //
        // [Out] (Note: this is similar to what msdn will show)
        // the managed data will not be marshalled so that the unmanaged function will not see the
        // managed data, if the data changes in unmanaged world, these changes will be propagated by
        // the marshaller to the managed data
        //
        // objects are marshalled differently if they're:
        //
        // 1) structs
        // for structs, by default, the whole layout is pushed on the stack as it is.
        // in order to pass a pointer to the managed layout, we need to specify either the ref or out keyword.
        //
        //      a) for IN and OUT:
        //      [In, Out] ref Struct ([In, Out] is optional here)
        //
        //      b) for IN only (the managed data will be marshalled so that the unmanaged
        //      function can read it but even if it changes it the change won't be propagated
        //      to the managed struct)
        //      [In] ref Struct
        //
        //      c) for OUT only (the managed data will not be marshalled so that the
        //      unmanaged function cannot read, the changes done in unmanaged code will be
        //      propagated to the managed struct)
        //      [Out] out Struct ([Out] is optional here)
        //
        // 2) array or classes
        // for array or classes, by default, a pointer to the managed layout is passed.
        // we don't need to specify neither the ref nor the out keyword.
        //
        //      a) for IN and OUT:
        //      [In, Out] byte[]
        //
        //      b) for IN only (the managed data will be marshalled so that the unmanaged
        //      function can read it but even if it changes it the change won't be propagated
        //      to the managed struct)
        //      [In] byte[] ([In] is optional here)
        //
        //      c) for OUT only (the managed data will not be marshalled so that the
        //      unmanaged function cannot read, the changes done in unmanaged code will be
        //      propagated to the managed struct)
        //      [Out] byte[]
        //
        [System.Security.SuppressUnmanagedCodeSecurityAttribute()]
        internal static class OSSOCK {

#if FEATURE_PAL
            private const string WS2_32 = ROTOR_PAL;
#else
            private const string WS2_32 = "ws2_32.dll";
            private const string mswsock = "mswsock.dll";
#endif

            //
            // IPv6 Changes: These are initialized in InitializeSockets - don't set them here or
            //               there will be an ordering problem with the call above that will
            //               result in both being set to false !
            //

            [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto)]
            internal struct WSAPROTOCOLCHAIN {
                internal int ChainLen;                                 /* the length of the chain,     */
                [MarshalAs(UnmanagedType.ByValArray, SizeConst=7)]
                internal uint[] ChainEntries;       /* a list of dwCatalogEntryIds */
            }

            [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto)]
            internal struct WSAPROTOCOL_INFO {
                internal uint dwServiceFlags1;
                internal uint dwServiceFlags2;
                internal uint dwServiceFlags3;
                internal uint dwServiceFlags4;
                internal uint dwProviderFlags;
                Guid ProviderId;
                internal uint dwCatalogEntryId;
                WSAPROTOCOLCHAIN ProtocolChain;
                internal int iVersion;
                internal AddressFamily iAddressFamily;
                internal int iMaxSockAddr;
                internal int iMinSockAddr;
                internal int iSocketType;
                internal int iProtocol;
                internal int iProtocolMaxOffset;
                internal int iNetworkByteOrder;
                internal int iSecurityScheme;
                internal uint dwMessageSize;
                internal uint dwProviderReserved;
                [MarshalAs(UnmanagedType.ByValTStr, SizeConst=256)]
                internal string szProtocol;
            }

            [StructLayout(LayoutKind.Sequential)]
            internal struct ControlData {
                internal UIntPtr length;
                internal uint level;
                internal uint type;
                internal uint address;
                internal uint index;
            }

            [StructLayout(LayoutKind.Sequential)]
            internal struct ControlDataIPv6 {
                internal UIntPtr length;
                internal uint level;
                internal uint type;
                [MarshalAs(UnmanagedType.ByValArray,SizeConst=16)]
                internal byte[] address;
                internal uint index;
            }

            [StructLayout(LayoutKind.Sequential)]
            internal struct WSAMsg {
                internal IntPtr socketAddress;
                internal uint addressLength;
                internal IntPtr buffers;
                internal uint count;
                internal WSABuffer controlBuffer;
                internal SocketFlags flags;
            }
            
            //
            // Flags equivalent to winsock TRANSMIT_PACKETS_ELEMENT flags
            //    #define TP_ELEMENT_MEMORY   1
            //    #define TP_ELEMENT_FILE     2
            //    #define TP_ELEMENT_EOP      4
            //
            [Flags]
            internal enum TransmitPacketsElementFlags : uint {
                None = 0x00,
                Memory = 0x01,
                File = 0x02,
                EndOfPacket = 0x04
            }

            // Structure equivalent to TRANSMIT_PACKETS_ELEMENT
            //
            // typedef struct _TRANSMIT_PACKETS_ELEMENT {
            //     ULONG dwElFlags;  
            //     ULONG cLength;  
            //     union {    
            //         struct {      
            //             LARGE_INTEGER nFileOffset;      
            //             HANDLE hFile;
            //         };    
            //         PVOID pBuffer;  
            //     }
            //  };
            // } TRANSMIT_PACKETS_ELEMENT;
            //
            [StructLayout(LayoutKind.Explicit)]
            internal struct TransmitPacketsElement {
                [System.Runtime.InteropServices.FieldOffset(0)]
                internal TransmitPacketsElementFlags flags;
                [System.Runtime.InteropServices.FieldOffset(4)]
                internal uint length;
                [System.Runtime.InteropServices.FieldOffset(8)]
                internal Int64 fileOffset;
                [System.Runtime.InteropServices.FieldOffset(8)]
                internal IntPtr buffer;
                [System.Runtime.InteropServices.FieldOffset(16)]
                internal IntPtr fileHandle;
            }
             
            /*
               typedef struct _SOCKET_ADDRESS {  
                   PSOCKADDR lpSockaddr;  
                   INT iSockaddrLength;
               } SOCKET_ADDRESS, *PSOCKET_ADDRESS;			
            */
            [StructLayout(LayoutKind.Sequential)]
            internal struct SOCKET_ADDRESS {
                internal IntPtr lpSockAddr;
                internal int iSockaddrLength;
            }

            /*
               typedef struct _SOCKET_ADDRESS_LIST {
                   INT             iAddressCount;
                   SOCKET_ADDRESS  Address[1];
               } SOCKET_ADDRESS_LIST, *PSOCKET_ADDRESS_LIST, FAR *LPSOCKET_ADDRESS_LIST;
            */
            [StructLayout(LayoutKind.Sequential)]
            internal struct SOCKET_ADDRESS_LIST {
                internal int iAddressCount;            
                internal SOCKET_ADDRESS Addresses;
            }

            [StructLayout(LayoutKind.Sequential)]
            internal struct TransmitFileBuffersStruct {
                internal IntPtr preBuffer;// Pointer to Buffer
                internal int preBufferLength; // Length of Buffer
                internal IntPtr postBuffer;// Pointer to Buffer
                internal int postBufferLength; // Length of Buffer
            }

            // CharSet=Auto here since WSASocket has A and W versions. We can use Auto cause the method is not used under constrained execution region
            [DllImport(WS2_32, CharSet=CharSet.Auto, SetLastError=true)]
            internal static extern SafeCloseSocket.InnerSafeCloseSocket WSASocket(
                                                    [In] AddressFamily addressFamily,
                                                    [In] SocketType socketType,
                                                    [In] ProtocolType protocolType,
                                                    [In] IntPtr protocolInfo, // will be WSAProtcolInfo protocolInfo once we include QOS APIs
                                                    [In] uint group,
                                                    [In] SocketConstructorFlags flags
                                                    );

            [DllImport(WS2_32, CharSet=CharSet.Auto, SetLastError=true)]
            internal unsafe static extern SafeCloseSocket.InnerSafeCloseSocket WSASocket(
                                        [In] AddressFamily addressFamily,
                                        [In] SocketType socketType,
                                        [In] ProtocolType protocolType,
                                        [In] byte* pinnedBuffer, // will be WSAProtcolInfo protocolInfo once we include QOS APIs
                                        [In] uint group,
                                        [In] SocketConstructorFlags flags
                                        );


            [DllImport(WS2_32, CharSet=CharSet.Ansi, BestFitMapping=false, ThrowOnUnmappableChar=true, SetLastError=true)]
            internal static extern SocketError WSAStartup(
                                               [In] short wVersionRequested,
                                               [Out] out WSAData lpWSAData
                                               );

            [DllImport(WS2_32, SetLastError=true)]
            [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
            internal static extern SocketError ioctlsocket(
                                                [In] SafeCloseSocket socketHandle,
                                                [In] int cmd,
                                                [In, Out] ref int argp
                                                );

            [DllImport(WS2_32, CharSet=CharSet.Ansi, BestFitMapping=false, ThrowOnUnmappableChar=true, SetLastError=true)]
            internal static extern IntPtr gethostbyname(
                                                  [In] string host
                                                  );

            [DllImport(WS2_32, SetLastError=true)]
            internal static extern IntPtr gethostbyaddr(
                                                  [In] ref int addr,
                                                  [In] int len,
                                                  [In] ProtocolFamily type
                                                  );

            [DllImport(WS2_32, CharSet=CharSet.Ansi, BestFitMapping=false, ThrowOnUnmappableChar=true, SetLastError=true)]
            internal static extern SocketError gethostname(
                                                [Out] StringBuilder hostName,
                                                [In] int bufferLength
                                                );

            [DllImport(WS2_32, SetLastError=true)]
            internal static extern SocketError getpeername(
                                                [In] SafeCloseSocket socketHandle,
                                                [Out] byte[] socketAddress,
                                                [In, Out] ref int socketAddressSize
                                                );

            [DllImport(WS2_32, SetLastError=true)]
            internal static extern SocketError getsockopt(
                                               [In] SafeCloseSocket socketHandle,
                                               [In] SocketOptionLevel optionLevel,
                                               [In] SocketOptionName optionName,
                                               [Out] out int optionValue,
                                               [In, Out] ref int optionLength
                                               );

            [DllImport(WS2_32, SetLastError=true)]
            internal static extern SocketError getsockopt(
                                               [In] SafeCloseSocket socketHandle,
                                               [In] SocketOptionLevel optionLevel,
                                               [In] SocketOptionName optionName,
                                               [Out] byte[] optionValue,
                                               [In, Out] ref int optionLength
                                               );

            [DllImport(WS2_32, SetLastError=true)]
            internal static extern SocketError getsockopt(
                                               [In] SafeCloseSocket socketHandle,
                                               [In] SocketOptionLevel optionLevel,
                                               [In] SocketOptionName optionName,
                                               [Out] out Linger optionValue,
                                               [In, Out] ref int optionLength
                                               );

            [DllImport(WS2_32, SetLastError=true)]
            internal static extern SocketError getsockopt(
                                               [In] SafeCloseSocket socketHandle,
                                               [In] SocketOptionLevel optionLevel,
                                               [In] SocketOptionName optionName,
                                               [Out] out IPMulticastRequest optionValue,
                                               [In, Out] ref int optionLength
                                               );

            //
            // IPv6 Changes: need to receive and IPv6MulticastRequest from getsockopt
            //
            [DllImport(WS2_32, SetLastError=true)]
            internal static extern SocketError getsockopt(
                                               [In] SafeCloseSocket socketHandle,
                                               [In] SocketOptionLevel optionLevel,
                                               [In] SocketOptionName optionName,
                                               [Out] out IPv6MulticastRequest optionValue,
                                               [In, Out] ref int optionLength
                                               );

            [DllImport(WS2_32, SetLastError=true)]
            internal static extern SocketError setsockopt(
                                               [In] SafeCloseSocket socketHandle,
                                               [In] SocketOptionLevel optionLevel,
                                               [In] SocketOptionName optionName,
                                               [In] ref int optionValue,
                                               [In] int optionLength
                                               );

            [DllImport(WS2_32, SetLastError=true)]
            internal static extern SocketError setsockopt(
                                               [In] SafeCloseSocket socketHandle,
                                               [In] SocketOptionLevel optionLevel,
                                               [In] SocketOptionName optionName,
                                               [In] byte[] optionValue,
                                               [In] int optionLength
                                               );

            [DllImport(WS2_32, SetLastError=true)]
            internal static extern SocketError setsockopt(
                                               [In] SafeCloseSocket socketHandle,
                                               [In] SocketOptionLevel optionLevel,
                                               [In] SocketOptionName optionName,
                                               [In] ref IntPtr pointer,
                                               [In] int optionLength
                                               );

            [DllImport(WS2_32, SetLastError=true)]
            [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
            internal static extern SocketError setsockopt(
                                               [In] SafeCloseSocket socketHandle,
                                               [In] SocketOptionLevel optionLevel,
                                               [In] SocketOptionName optionName,
                                               [In] ref Linger linger,
                                               [In] int optionLength
                                               );

            [DllImport(WS2_32, SetLastError=true)]
            internal static extern SocketError setsockopt(
                                               [In] SafeCloseSocket socketHandle,
                                               [In] SocketOptionLevel optionLevel,
                                               [In] SocketOptionName optionName,
                                               [In] ref IPMulticastRequest mreq,
                                               [In] int optionLength
                                               );

            //
            // IPv6 Changes: need to pass an IPv6MulticastRequest to setsockopt
            //
            [DllImport(WS2_32, SetLastError=true)]
            internal static extern SocketError setsockopt(
                                               [In] SafeCloseSocket socketHandle,
                                               [In] SocketOptionLevel optionLevel,
                                               [In] SocketOptionName optionName,
                                               [In] ref IPv6MulticastRequest mreq,
                                               [In] int optionLength
                                               );

#if !FEATURE_PAL

            [DllImport(mswsock, SetLastError=true)]
            internal static extern bool TransmitFile(
                                      [In] SafeCloseSocket socket,
                                      [In] SafeHandle fileHandle,
                                      [In] int numberOfBytesToWrite,
                                      [In] int numberOfBytesPerSend,
                                      [In] SafeHandle overlapped,
                                      [In] TransmitFileBuffers buffers,
                                      [In] TransmitFileOptions flags
                                      );
        
            [DllImport(mswsock, SetLastError=true, EntryPoint = "TransmitFile")]
            internal static extern bool TransmitFile2(
                                      [In] SafeCloseSocket socket,
                                      [In] IntPtr fileHandle,
                                      [In] int numberOfBytesToWrite,
                                      [In] int numberOfBytesPerSend,
                                      [In] SafeHandle overlapped,
                                      [In] TransmitFileBuffers buffers,
                                      [In] TransmitFileOptions flags
                                      );


            [DllImport(mswsock, SetLastError = true, EntryPoint = "TransmitFile")]
            internal static extern bool TransmitFile_Blocking(
                                      [In] IntPtr socket,
                                      [In] SafeHandle fileHandle,
                                      [In] int numberOfBytesToWrite,
                                      [In] int numberOfBytesPerSend,
                                      [In] SafeHandle overlapped,
                                      [In] TransmitFileBuffers buffers,
                                      [In] TransmitFileOptions flags
                                      );

            [DllImport(mswsock, SetLastError = true, EntryPoint = "TransmitFile")]
            internal static extern bool TransmitFile_Blocking2(
                                      [In] IntPtr socket,
                                      [In] IntPtr fileHandle,
                                      [In] int numberOfBytesToWrite,
                                      [In] int numberOfBytesPerSend,
                                      [In] SafeHandle overlapped,
                                      [In] TransmitFileBuffers buffers,
                                      [In] TransmitFileOptions flags
                                      );

#endif // !FEATURE_PAL

            // This method is always blocking, so it uses an IntPtr.
            [DllImport(WS2_32, SetLastError = true)]
            internal unsafe static extern int send(
                                         [In] IntPtr      socketHandle,
                                         [In] byte*       pinnedBuffer,
                                         [In] int         len,
                                         [In] SocketFlags socketFlags
                                         );

            // This method is always blocking, so it uses an IntPtr.
            [DllImport(WS2_32, SetLastError = true)]
            internal unsafe static extern int recv(
                                         [In] IntPtr      socketHandle,
                                         [In] byte*       pinnedBuffer,
                                         [In] int         len,
                                         [In] SocketFlags socketFlags
                                         );

            [DllImport(WS2_32, SetLastError=true)]
            internal static extern SocketError listen(
                                           [In] SafeCloseSocket socketHandle,
                                           [In] int backlog
                                           );

            [DllImport(WS2_32, SetLastError=true)]
            internal static extern SocketError bind(
                                         [In] SafeCloseSocket socketHandle,
                                         [In] byte[] socketAddress,
                                         [In] int socketAddressSize
                                         );

            [DllImport(WS2_32, SetLastError=true)]
            internal static extern SocketError shutdown(
                                             [In] SafeCloseSocket socketHandle,
                                             [In] int how
                                             );

            // This method is always blocking, so it uses an IntPtr.
            [DllImport(WS2_32, SetLastError = true)]
            internal unsafe static extern int sendto(
                                           [In] IntPtr      socketHandle,
                                           [In] byte*       pinnedBuffer,
                                           [In] int         len,
                                           [In] SocketFlags socketFlags,
                                           [In] byte[]      socketAddress,
                                           [In] int         socketAddressSize
                                           );

            // This method is always blocking, so it uses an IntPtr.
            [DllImport(WS2_32, SetLastError = true)]
            internal unsafe static extern int recvfrom(
                                             [In] IntPtr        socketHandle,
                                             [In] byte*         pinnedBuffer,
                                             [In] int           len,
                                             [In] SocketFlags   socketFlags,
                                             [Out] byte[]       socketAddress,
                                             [In, Out] ref int  socketAddressSize
                                             );

            [DllImport(WS2_32, SetLastError=true)]
            internal static extern SocketError getsockname(
                                                [In] SafeCloseSocket socketHandle,
                                                [Out] byte[] socketAddress,
                                                [In, Out] ref int socketAddressSize
                                                );

            [DllImport(WS2_32, SetLastError=true)]
            internal static extern int select(
                                           [In] int ignoredParameter,
                                           [In, Out] IntPtr[] readfds,
                                           [In, Out] IntPtr[] writefds,
                                           [In, Out] IntPtr[] exceptfds,
                                           [In] ref TimeValue timeout
                                           );

            [DllImport(WS2_32, SetLastError=true)]
            internal static extern int select(
                                           [In] int ignoredParameter,
                                           [In, Out] IntPtr[] readfds,
                                           [In, Out] IntPtr[] writefds,
                                           [In, Out] IntPtr[] exceptfds,
                                           [In] IntPtr nullTimeout
                                           );

            // This function is always potentially blocking so it uses an IntPtr.
            [DllImport(WS2_32, SetLastError = true)]
            internal static extern SocketError WSAConnect(
                                              [In] IntPtr socketHandle,
                                              [In] byte[] socketAddress,
                                              [In] int    socketAddressSize,
                                              [In] IntPtr inBuffer,
                                              [In] IntPtr outBuffer,
                                              [In] IntPtr sQOS,
                                              [In] IntPtr gQOS
                                              );


            [DllImport(WS2_32, SetLastError=true)]
            internal static extern SocketError WSASend(
                                              [In] SafeCloseSocket socketHandle,
                                              [In] ref WSABuffer buffer,
                                              [In] int bufferCount,
                                              [Out] out int bytesTransferred,
                                              [In] SocketFlags socketFlags,
                                              [In] SafeHandle overlapped,
                                              [In] IntPtr completionRoutine
                                              );

            [DllImport(WS2_32, SetLastError=true)]
            internal static extern SocketError WSASend(
                                              [In] SafeCloseSocket socketHandle,
                                              [In] WSABuffer[] buffersArray,
                                              [In] int bufferCount,
                                              [Out] out int bytesTransferred,
                                              [In] SocketFlags socketFlags,
                                              [In] SafeHandle overlapped,
                                              [In] IntPtr completionRoutine
                                              );

            [DllImport(WS2_32, SetLastError = true, EntryPoint = "WSASend")]
            internal static extern SocketError WSASend_Blocking(
                                              [In] IntPtr socketHandle,
                                              [In] WSABuffer[] buffersArray,
                                              [In] int bufferCount,
                                              [Out] out int bytesTransferred,
                                              [In] SocketFlags socketFlags,
                                              [In] SafeHandle overlapped,
                                              [In] IntPtr completionRoutine
                                              );

            [DllImport(WS2_32, SetLastError = true)]
            internal static extern SocketError WSASendTo(
                                                [In] SafeCloseSocket socketHandle,
                                                [In] ref WSABuffer buffer,
                                                [In] int bufferCount,
                                                [Out] out int bytesTransferred,
                                                [In] SocketFlags socketFlags,
                                                [In] IntPtr socketAddress,
                                                [In] int socketAddressSize,
                                                [In] SafeHandle overlapped,
                                                [In] IntPtr completionRoutine
                                                );
            
            [DllImport(WS2_32, SetLastError = true)]
            internal static extern SocketError WSASendTo(
                                                [In] SafeCloseSocket socketHandle,
                                                [In] WSABuffer[] buffersArray,
                                                [In] int bufferCount,
                                                [Out] out int bytesTransferred,
                                                [In] SocketFlags socketFlags,
                                                [In] IntPtr socketAddress,
                                                [In] int socketAddressSize,
                                                [In] SafeNativeOverlapped overlapped,
                                                [In] IntPtr completionRoutine
                                                );

            [DllImport(WS2_32, SetLastError=true)]
            internal static extern SocketError WSARecv(
                                              [In] SafeCloseSocket socketHandle,
                                              [In] ref WSABuffer buffer,
                                              [In] int bufferCount,
                                              [Out] out int bytesTransferred,
                                              [In, Out] ref SocketFlags socketFlags,
                                              [In] SafeHandle overlapped,
                                              [In] IntPtr completionRoutine
                                              );

            [DllImport(WS2_32, SetLastError = true)]
            internal static extern SocketError WSARecv(
                                              [In] SafeCloseSocket socketHandle,
                                              [In, Out] WSABuffer[] buffers,
                                              [In] int bufferCount,
                                              [Out] out int bytesTransferred,
                                              [In, Out] ref SocketFlags socketFlags,
                                              [In] SafeHandle overlapped,
                                              [In] IntPtr completionRoutine
                                              );

            [DllImport(WS2_32, SetLastError = true, EntryPoint = "WSARecv")]
            internal static extern SocketError WSARecv_Blocking(
                                              [In] IntPtr socketHandle,
                                              [In, Out] WSABuffer[] buffers,
                                              [In] int bufferCount,
                                              [Out] out int bytesTransferred,
                                              [In, Out] ref SocketFlags socketFlags,
                                              [In] SafeHandle overlapped,
                                              [In] IntPtr completionRoutine
                                              );

            [DllImport(WS2_32, SetLastError=true)]
            internal static extern SocketError WSARecvFrom(
                                                  [In] SafeCloseSocket socketHandle,
                                                  [In] ref WSABuffer buffer,
                                                  [In] int bufferCount,
                                                  [Out] out int bytesTransferred,
                                                  [In, Out] ref SocketFlags socketFlags,
                                                  [In] IntPtr socketAddressPointer,
                                                  [In] IntPtr socketAddressSizePointer,
                                                  [In] SafeHandle overlapped,
                                                  [In] IntPtr completionRoutine
                                                  );

            [DllImport(WS2_32, SetLastError = true)]
            internal static extern SocketError WSARecvFrom(
                                                  [In] SafeCloseSocket socketHandle,
                                                  [In, Out] WSABuffer[] buffers,
                                                  [In] int bufferCount,
                                                  [Out] out int bytesTransferred,
                                                  [In, Out] ref SocketFlags socketFlags,
                                                  [In] IntPtr socketAddressPointer,
                                                  [In] IntPtr socketAddressSizePointer,
                                                  [In] SafeNativeOverlapped overlapped,
                                                  [In] IntPtr completionRoutine
                                                  );
            
            [DllImport(WS2_32, SetLastError=true)]
            internal static extern SocketError WSAEventSelect(
                                                     [In] SafeCloseSocket socketHandle,
                                                     [In] SafeHandle Event,
                                                     [In] AsyncEventBits NetworkEvents
                                                     );

            [DllImport(WS2_32, SetLastError=true)]
            internal static extern SocketError WSAEventSelect(
                                         [In] SafeCloseSocket socketHandle,
                                         [In] IntPtr Event,
                                         [In] AsyncEventBits NetworkEvents
                                         );


            // Used with SIOGETEXTENSIONFUNCTIONPOINTER - we're assuming that will never block.
            [DllImport(WS2_32, SetLastError=true)]
            internal static extern SocketError WSAIoctl(
                                                [In] SafeCloseSocket socketHandle,
                                                [In] int ioControlCode,
                                                [In,Out] ref Guid guid,
                                                [In] int guidSize,
                                                [Out] out IntPtr funcPtr,
                                                [In]  int funcPtrSize,
                                                [Out] out int bytesTransferred,
                                                [In] IntPtr shouldBeNull,
                                                [In] IntPtr shouldBeNull2
                                                );

            [DllImport(WS2_32, SetLastError = true, EntryPoint = "WSAIoctl")]
            internal static extern SocketError WSAIoctl_Blocking(
                                                [In] IntPtr socketHandle,
                                                [In] int ioControlCode,
                                                [In] byte[] inBuffer,
                                                [In] int inBufferSize,
                                                [Out] byte[] outBuffer,
                                                [In] int outBufferSize,
                                                [Out] out int bytesTransferred,
                                                [In] SafeHandle overlapped,
                                                [In] IntPtr completionRoutine
                                                );

            [DllImport(WS2_32, SetLastError = true, EntryPoint = "WSAIoctl")]
            internal static extern SocketError WSAIoctl_Blocking_Internal(
                                                [In]  IntPtr socketHandle,
                                                [In]  uint ioControlCode,
                                                [In]  IntPtr inBuffer,
                                                [In]  int inBufferSize,
                                                [Out] IntPtr outBuffer,
                                                [In]  int outBufferSize,
                                                [Out] out int bytesTransferred,
                                                [In]  SafeHandle overlapped,
                                                [In]  IntPtr completionRoutine
                                                );			

            [DllImport(WS2_32,SetLastError=true)]
            internal static extern SocketError WSAEnumNetworkEvents(
                                                     [In] SafeCloseSocket socketHandle,
                                                     [In] SafeWaitHandle Event,
                                                     [In, Out] ref NetworkEvents networkEvents
                                                     );

#if !FEATURE_PAL
            [DllImport(WS2_32, SetLastError=true)]
            internal unsafe static extern int WSADuplicateSocket(
                [In] SafeCloseSocket socketHandle,
                [In] uint targetProcessID,
                [In] byte* pinnedBuffer
            );
#endif // !FEATURE_PAL

            [DllImport(WS2_32, SetLastError=true)]
            internal static extern bool WSAGetOverlappedResult(
                                                     [In] SafeCloseSocket socketHandle,
                                                     [In] SafeHandle overlapped,
                                                     [Out] out uint bytesTransferred,
                                                     [In] bool wait,
                                                     [Out] out SocketFlags socketFlags
                                                     );
#if !FEATURE_PAL
            // Don't throw, it would crash IPAddress.TryParse
            [DllImport(WS2_32, CharSet=CharSet.Unicode, BestFitMapping=false, ThrowOnUnmappableChar=false, SetLastError=true)]
            internal static extern SocketError WSAStringToAddress(
                [In] string addressString,
                [In] AddressFamily addressFamily,
                [In] IntPtr lpProtocolInfo, // always passing in a 0
                [Out] byte[] socketAddress,
                [In, Out] ref int socketAddressSize );

            [DllImport(WS2_32, CharSet=CharSet.Ansi, BestFitMapping=false, ThrowOnUnmappableChar=true, SetLastError=true)]
            internal static extern SocketError WSAAddressToString(
                [In] byte[] socketAddress,
                [In] int socketAddressSize,
                [In] IntPtr lpProtocolInfo,// always passing in a 0
                [Out]StringBuilder addressString,
                [In, Out] ref int addressStringLength);

            [DllImport(WS2_32, CharSet=CharSet.Unicode, BestFitMapping=false, ThrowOnUnmappableChar=true, SetLastError=true)]
            internal static extern SocketError GetNameInfoW(
                [In]         byte[]        sa,
                [In]         int           salen,
                [In,Out]     StringBuilder host,
                [In]         int           hostlen,
                [In,Out]     StringBuilder serv,
                [In]         int           servlen,
                [In]         int           flags);

            //if we change this back to auto, we also have to change
            //WSAPROTOCOL_INFO and WSAPROTOCOLCHAIN
            [DllImport(WS2_32, SetLastError=true, CharSet=CharSet.Auto, ExactSpelling=false)]
            internal static extern int WSAEnumProtocols(
                                                        [MarshalAs(UnmanagedType.LPArray)]
                                                        [In] int[]     lpiProtocols,
                                                        [In] SafeLocalFree lpProtocolBuffer,
                                                        [In][Out] ref uint lpdwBufferLength
                                                       );
#if SOCKETTHREADPOOL
            [DllImport("kernel32.dll", SetLastError = true)]
            public static extern bool BindIoCompletionCallback(
                SafeCloseSocket socketHandle,
                IOCompletionCallback function,
                Int32 flags
            );
    
            [DllImport("kernel32.dll", SetLastError = true)]
            public static extern IntPtr CreateIoCompletionPort(
                SafeCloseSocket socketHandle,
                IntPtr ExistingCompletionPort,
                Int32 CompletionKey,
                Int32 NumberOfConcurrentThreads
            );
    
            [DllImport("kernel32.dll", SetLastError = true)]
            public static extern IntPtr CreateIoCompletionPort(
                SafeHandle Handle,
                IntPtr ExistingCompletionPort,
                Int32 CompletionKey,
                Int32 NumberOfConcurrentThreads
            );
            [DllImport("kernel32.dll", SetLastError = true)]
            public static extern IntPtr CreateIoCompletionPort(
                IntPtr Handle,
                IntPtr ExistingCompletionPort,
                Int32 CompletionKey,
                Int32 NumberOfConcurrentThreads
            );
    
            [DllImport("kernel32.dll", SetLastError = true)]
            public static extern unsafe bool GetQueuedCompletionStatus(
              IntPtr CompletionPort,
              out UInt32 lpNumberOfBytes,
              out Int32 lpCompletionKey,
              out NativeOverlapped* lpOverlapped,
              Int32 dwMilliseconds
            );
    
            [DllImport("kernel32.dll", SetLastError = true)]
            public static extern bool PostQueuedCompletionStatus(
                IntPtr CompletionPort,
                Int32 dwNumberOfBytesTransferred,
                IntPtr dwCompletionKey,
                IntPtr dwZero
            );
#endif // SOCKETTHREADPOOL
#endif // !FEATURE_PAL

        }; // class UnsafeNclNativeMethods.OSSOCK

#if !FEATURE_PAL
        //
        // UnsafeNclNativeMethods.NativePKI class contains methods
        // imported from crypt32.dll.
        // They deal mainly with certificates handling when doing https://
        //
        [System.Security.SuppressUnmanagedCodeSecurityAttribute()]
        internal static class NativePKI {

            [StructLayout(LayoutKind.Sequential)]
            internal struct CRYPT_OBJID_BLOB
            {
                public UInt32 cbData;
                public IntPtr pbData;
            }

            [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Ansi)]
            internal struct CERT_EXTENSION
            {
                public IntPtr pszObjId;
                public UInt32 fCritical;
                public CRYPT_OBJID_BLOB Value;
            }

            [StructLayout(LayoutKind.Sequential)]
            internal struct CERT_SELECT_CRITERIA
            {
                public UInt32 dwType;
                public UInt32 cPara;
                public IntPtr ppPara;
            }

            [DllImport(CRYPT32, ExactSpelling=true, SetLastError=true)]
            internal static extern  int CertVerifyCertificateChainPolicy(
                [In] IntPtr                     policy,
                [In] SafeFreeCertChain          chainContext,
                [In] ref ChainPolicyParameter   cpp,
                [In, Out] ref ChainPolicyStatus ps);

            // Win7+
            [DllImport(CRYPT32, ExactSpelling=true, SetLastError=true)]
            private static extern bool CertSelectCertificateChains(
                [In] IntPtr pSelectionContext, // LPCGUID
                [In] CertificateSelect flags, // DWORD
                [In] IntPtr pChainParameters, // PCCERT_SELECT_CHAIN_PARA 
                [In] int cCriteria, // DWORD
                [In] SafeCertSelectCritera rgpCriteria, // PCCERT_SELECT_CRITERIA
                [In] IntPtr hStore, // HCERTSTORE
                [Out] out int pcSelection, // PDWORD
                // **PCCERT_CHAIN_CONTEXT, Array of ptrs to contexts
                [Out] out SafeFreeCertChainList pprgpSelection);

            // See WinCrypt.h
            [Flags]
            private enum CertificateSelect : int
            {
                None = 0,
                AllowExpired = 0x00000001,
                TrustedRoot = 0x00000002,
                DisallowSelfsigned = 0x00000004,
                HasPrivateKey = 0x00000008,
                HasKeyForSignature = 0x00000010,
                HasKeyForKeyExchange = 0x00000020,
                HardwareOnly = 0x00000040,
                AllowDuplicates = 0x00000080,
            }

            // Discover available client certificates to send to the server.
            // SecureChannel will handle filtering by Issuer durring the request (AcquireClientCredentials).
            // See WinINet's implementation: //depot/winmain/inetcore/wininet/dll/CliauthCertselect.cxx
            [FriendAccessAllowed]
            [SuppressMessage("Microsoft.Reliability", "CA2001:AvoidCallingProblematicMethods",
                Justification = "Marshalling requires DangerousGetHandle")]
            internal static X509CertificateCollection FindClientCertificates()
            {
                if (!ComNetOS.IsWin7orLater)
                {
                    throw new PlatformNotSupportedException();
                }

                X509CertificateCollection certificates = new X509CertificateCollection();

                X509Store store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
                store.Open(OpenFlags.MaxAllowed);

                int chainCount = 0;
                SafeFreeCertChainList chainList = null;
                SafeCertSelectCritera criteria = new SafeCertSelectCritera();
                try
                {
                    bool success = CertSelectCertificateChains(
                        IntPtr.Zero,
                        CertificateSelect.HasPrivateKey, 
                        IntPtr.Zero,
                        criteria.Count,  // DWORD
                        criteria, // PCCERT_SELECT_CRITERIA
                        store.StoreHandle, 
                        out chainCount, 
                        out chainList);

                    if (!success)
                    {
                        throw new Win32Exception(); // Calls GetLastError.
                    }

                    Debug.Assert(chainCount == 0 || !chainList.IsInvalid);

                    for (int i = 0; i < chainCount; i++)
                    {
                        // Resolve IntPtr in array.
                        using (SafeFreeCertChain chainRef = new SafeFreeCertChain(
                            Marshal.ReadIntPtr(chainList.DangerousGetHandle() 
                            + i * Marshal.SizeOf(typeof(IntPtr))), true))
                        {
                            Debug.Assert(!chainRef.IsInvalid);

                            // X509Chain will duplicate the chain by increasing its ref-count.
                            X509Chain chain = new X509Chain(chainRef.DangerousGetHandle());
                            
                            // Copy base cert from chain.
                            if (chain.ChainElements.Count > 0)
                            {
                                X509Certificate2 cert = chain.ChainElements[0].Certificate;
                                certificates.Add(cert);
                            }

                            // Remove the X509Chain's reference prior to releasing the Chain List.
                            chain.Reset();
                        }
                    }
                }
                finally
                {
                    // Close store.
                    store.Close();
                    chainList.Dispose();
                    criteria.Dispose();
                }

                return certificates;
            }
        }; // class UnsafeNclNativeMethods.NativePKI

        [System.Security.SuppressUnmanagedCodeSecurityAttribute()]
        internal static class NativeNTSSPI {

            [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
            [DllImport(SECUR32, ExactSpelling=true, SetLastError=true)]
            internal static extern int EncryptMessage(
                  ref SSPIHandle contextHandle,
                  [In] uint qualityOfProtection,
                  [In, Out] SecurityBufferDescriptor inputOutput,
                  [In] uint sequenceNumber
                  );

            [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
            [DllImport(SECUR32, ExactSpelling = true, SetLastError = true)]
            internal static unsafe extern int DecryptMessage(
                  [In] ref SSPIHandle contextHandle,
                  [In, Out] SecurityBufferDescriptor inputOutput,
                  [In] uint sequenceNumber,
                       uint *qualityOfProtection
                  );

        }; // class UnsafeNclNativeMethods.NativeNTSSPI
        
        // The replacement for WinInet, WinHttp is preferred where it's available.  We require version 5.1.
        [SuppressUnmanagedCodeSecurity]
        internal static class WinHttp
        {
            [DllImport(WINHTTP, ExactSpelling=true, SetLastError=true)]
            internal static extern bool WinHttpDetectAutoProxyConfigUrl(AutoDetectType autoDetectFlags, 
                out SafeGlobalFree autoConfigUrl);

            [DllImport(WINHTTP, SetLastError = true)]
            internal static extern bool WinHttpGetIEProxyConfigForCurrentUser(
                ref WINHTTP_CURRENT_USER_IE_PROXY_CONFIG proxyConfig);

            [SuppressMessage("Microsoft.Security", "CA2118:ReviewSuppressUnmanagedCodeSecurityUsage", Justification = "Implementation requires unmanaged code usage")]
            [DllImport(WINHTTP, CharSet = CharSet.Unicode, SetLastError = true)]
            internal static extern SafeInternetHandle WinHttpOpen(string userAgent, AccessType accessType,
                string proxyName, string proxyBypass, int dwFlags);

            [SuppressMessage("Microsoft.Security", "CA2118:ReviewSuppressUnmanagedCodeSecurityUsage", Justification = "Implementation requires unmanaged code usage")]
            [DllImport(WINHTTP, CharSet = CharSet.Unicode, SetLastError = true)]
            internal static extern bool WinHttpSetTimeouts(SafeInternetHandle session, int resolveTimeout,
                int connectTimeout, int sendTimeout, int receiveTimeout);

            [DllImport(WINHTTP, CharSet = CharSet.Unicode, SetLastError = true)]
            internal static extern bool WinHttpGetProxyForUrl(SafeInternetHandle session, string url,
                [In] ref WINHTTP_AUTOPROXY_OPTIONS autoProxyOptions, out WINHTTP_PROXY_INFO proxyInfo);

            [DllImport(WINHTTP, CharSet = CharSet.Unicode, SetLastError = true)]
            [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
            internal static extern bool WinHttpCloseHandle(IntPtr httpSession);

            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
            internal struct WINHTTP_CURRENT_USER_IE_PROXY_CONFIG
            {
                public bool AutoDetect;
                public IntPtr AutoConfigUrl;
                public IntPtr Proxy;
                public IntPtr ProxyBypass;
            }

            [Flags]
            internal enum AutoProxyFlags
            {
                AutoDetect = 0x00000001, // WINHTTP_AUTOPROXY_AUTO_DETECT
                AutoProxyConfigUrl = 0x00000002, // WINHTTP_AUTOPROXY_CONFIG_URL
                RunInProcess = 0x00010000, // WINHTTP_AUTOPROXY_RUN_INPROCESS
                RunOutProcessOnly = 0x00020000 // WINHTTP_AUTOPROXY_RUN_OUTPROCESS_ONLY
            }

            internal enum AccessType
            { 
                DefaultProxy = 0,
                NoProxy = 1,
                NamedProxy = 3
            }

            [Flags]
            internal enum AutoDetectType
            {
                None = 0x0,
                Dhcp = 0x1, // WINHTTP_AUTO_DETECT_TYPE_DHCP
                DnsA = 0x2, // WINHTTP_AUTO_DETECT_TYPE_DNS_A
            }

            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
            internal struct WINHTTP_AUTOPROXY_OPTIONS
            {
                public AutoProxyFlags Flags;

                public AutoDetectType AutoDetectFlags;

                [MarshalAs(UnmanagedType.LPWStr)]
                public string AutoConfigUrl;

                private IntPtr lpvReserved;

                private int dwReserved;

                public bool AutoLogonIfChallenged;
            }

            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
            internal struct WINHTTP_PROXY_INFO
            {
                public AccessType AccessType;

                public IntPtr Proxy;

                public IntPtr ProxyBypass;
            }

            internal enum ErrorCodes
            {
                Success = 0,

                OutOfHandles = 12001,                       // ERROR_WINHTTP_OUT_OF_HANDLES
                Timeout = 12002,                            // ERROR_WINHTTP_TIMEOUT
                InternalError = 12004,                      // ERROR_WINHTTP_INTERNAL_ERROR
                InvalidUrl = 12005,                         // ERROR_WINHTTP_INVALID_URL
                UnrecognizedScheme = 12006,                 // ERROR_WINHTTP_UNRECOGNIZED_SCHEME
                NameNotResolved = 12007,                    // ERROR_WINHTTP_NAME_NOT_RESOLVED
                InvalidOption = 12009,                      // ERROR_WINHTTP_INVALID_OPTION
                OptionNotSettable = 12011,                  // ERROR_WINHTTP_OPTION_NOT_SETTABLE
                Shutdown = 12012,                           // ERROR_WINHTTP_SHUTDOWN

                LoginFailure = 12015,                       // ERROR_WINHTTP_LOGIN_FAILURE
                OperationCancelled = 12017,                 // ERROR_WINHTTP_OPERATION_CANCELLED
                IncorrectHandleType = 12018,                // ERROR_WINHTTP_INCORRECT_HANDLE_TYPE
                IncorrectHandleState = 12019,               // ERROR_WINHTTP_INCORRECT_HANDLE_STATE
                CannotConnect = 12029,                      // ERROR_WINHTTP_CANNOT_CONNECT
                ConnectionError = 12030,                    // ERROR_WINHTTP_CONNECTION_ERROR
                ResendRequest = 12032,                      // ERROR_WINHTTP_RESEND_REQUEST

                AuthCertNeeded = 12044,                     // ERROR_WINHTTP_CLIENT_AUTH_CERT_NEEDED

                CannotCallBeforeOpen = 12100,               // ERROR_WINHTTP_CANNOT_CALL_BEFORE_OPEN
                CannotCallBeforeSend = 12101,               // ERROR_WINHTTP_CANNOT_CALL_BEFORE_SEND
                CannotCallAfterSend = 12102,                // ERROR_WINHTTP_CANNOT_CALL_AFTER_SEND
                CannotCallAfterOpen = 12103,                // ERROR_WINHTTP_CANNOT_CALL_AFTER_OPEN

                HeaderNotFound = 12150,                     // ERROR_WINHTTP_HEADER_NOT_FOUND
                InvalidServerResponse = 12152,              // ERROR_WINHTTP_INVALID_SERVER_RESPONSE
                InvalidHeader = 12153,                      // ERROR_WINHTTP_INVALID_HEADER
                InvalidQueryRequest = 12154,                // ERROR_WINHTTP_INVALID_QUERY_REQUEST
                HeaderAlreadyExists = 12155,                // ERROR_WINHTTP_HEADER_ALREADY_EXISTS
                RedirectFailed = 12156,                     // ERROR_WINHTTP_REDIRECT_FAILED

                AutoProxyServiceError = 12178,              // ERROR_WINHTTP_AUTO_PROXY_SERVICE_ERROR
                BadAutoProxyScript = 12166,                 // ERROR_WINHTTP_BAD_AUTO_PROXY_SCRIPT
                UnableToDownloadScript = 12167,             // ERROR_WINHTTP_UNABLE_TO_DOWNLOAD_SCRIPT

                NotInitialized = 12172,                     // ERROR_WINHTTP_NOT_INITIALIZED
                SecureFailure = 12175,                      // ERROR_WINHTTP_SECURE_FAILURE

                SecureCertDateInvalid = 12037,              // ERROR_WINHTTP_SECURE_CERT_DATE_INVALID
                SecureCertCNInvalid = 12038,                // ERROR_WINHTTP_SECURE_CERT_CN_INVALID
                SecureInvalidCA = 12045,                    // ERROR_WINHTTP_SECURE_INVALID_CA
                SecureCertRevFailed = 12057,                // ERROR_WINHTTP_SECURE_CERT_REV_FAILED
                SecureChannelError = 12157,                 // ERROR_WINHTTP_SECURE_CHANNEL_ERROR
                SecureInvalidCert = 12169,                  // ERROR_WINHTTP_SECURE_INVALID_CERT
                SecureCertRevoked = 12170,                  // ERROR_WINHTTP_SECURE_CERT_REVOKED
                SecureCertWrongUsage = 12179,               // ERROR_WINHTTP_SECURE_CERT_WRONG_USAGE

                AudodetectionFailed = 12180,                // ERROR_WINHTTP_AUTODETECTION_FAILED
                HeaderCountExceeded = 12181,                // ERROR_WINHTTP_HEADER_COUNT_EXCEEDED
                HeaderSizeOverflow = 12182,                 // ERROR_WINHTTP_HEADER_SIZE_OVERFLOW
                ChunkedEncodingHeaderSizeOverflow = 12183,  // ERROR_WINHTTP_CHUNKED_ENCODING_HEADER_SIZE_OVERFLOW
                ResponseDrainOverflow = 12184,              // ERROR_WINHTTP_RESPONSE_DRAIN_OVERFLOW
                ClientCertNoPrivateKey = 12185,             // ERROR_WINHTTP_CLIENT_CERT_NO_PRIVATE_KEY
                ClientCertNoAccessPrivateKey = 12186,       // ERROR_WINHTTP_CLIENT_CERT_NO_ACCESS_PRIVATE_KEY
            }
        }


        //
        // Caching (must use WinInet to cache).
        //
        [System.Security.SuppressUnmanagedCodeSecurityAttribute()]
        internal static class UnsafeWinInetCache {
            public  const int    MAX_PATH = 260;

            [DllImport(WININET, CharSet = CharSet.Unicode, ExactSpelling=true, SetLastError = true)]
            internal static extern bool CreateUrlCacheEntryW(
                                            [In]  string        urlName,
                                            [In]  int           expectedFileSize,
                                            [In]  string        fileExtension,
                                            [Out] System.Text.StringBuilder fileName,
                                            [In]  int           dwReserved
        );

            [DllImport(WININET, CharSet = CharSet.Unicode, ExactSpelling=true, SetLastError = true)]
            unsafe internal static extern bool CommitUrlCacheEntryW(
                                            [In] string                 urlName,
                                            [In] string                 localFileName,
                                            [In] _WinInetCache.FILETIME  expireTime,
                                            [In] _WinInetCache.FILETIME  lastModifiedTime,
                                            [In] _WinInetCache.EntryType EntryType,
                                            [In] byte*                  headerInfo,
                                            [In] int                    headerSizeTChars,
                                            [In] string                 fileExtension,
                                            [In] string                 originalUrl
        );

            [DllImport(WININET, CharSet = CharSet.Unicode, ExactSpelling=true, SetLastError = true)]
            unsafe internal static extern bool GetUrlCacheEntryInfoW(
                                            [In]      string    urlName,
                                            [In]      byte*     entryPtr,                       //was [Out]
                                            [In, Out] ref int   bufferSz
                                            );

            [DllImport(WININET, CharSet = CharSet.Unicode, ExactSpelling=true, SetLastError = true)]
            unsafe internal static extern bool SetUrlCacheEntryInfoW(
                                            [In] string                 lpszUrlName,
                                            [In] byte*                  EntryPtr,
                                            [In] _WinInetCache.Entry_FC  fieldControl
                                            );

            [DllImport(WININET, CharSet = CharSet.Unicode, ExactSpelling=true, SetLastError = true)]
            internal static extern bool DeleteUrlCacheEntryW( [In] string urlName);

            [DllImport(WININET, CharSet = CharSet.Unicode, ExactSpelling=true, SetLastError = true)]
            internal static extern bool UnlockUrlCacheEntryFileW(
                                            [In] string     urlName,
                                            [In] int        dwReserved                  //must be 0
                                            );

    /*********
    NOT USED SO FAR
            unsafe private extern static SafeUnlockUrlCacheEntryStream RetrieveUrlCacheEntryStream(
                                            [In]      string    urlName,
                                            [In]      byte*     entryPtr,               //was [Out]
                                            [In, Out] ref int   entryBufSize,
                                            [In]      bool      randomRead,
                                            [In]      int       dwReserved
                                            );

            unsafe internal static extern bool ReadUrlCacheEntryStream(
                                            [In]      SafeUnlockUrlCacheEntryStream  urlCacheStream,
                                            [In]      int       offset,
                                            [In]      byte*     bufferPtr,
                                            [In, Out] ref int   bufferSz,
                                            [In]      int       dwReserved                      //must be 0
                                            );

            internal static extern bool UnlockUrlCacheEntryStream(
                                    [In] IntPtr         urlCacheStream,
                                    [In] int            dwReserved                      //mustbe 0
                                    );

            unsafe internal static extern bool GetUrlCacheEntryInfoEx(
                                    [In]      string    url,
                                    [In]      byte*     entryPtr,                       //was [Out]
                                    [In, Out] ref int   entryBufSize,
                                    [In]      IntPtr    lpszReserved,                   //was[Out] must pass null
                                    [In]      IntPtr    lpdwReserved,                   //was[In, Out] must pass null
                                    [In]      IntPtr    lpReserved,                     //must pass null
                                    [In]      int       dwFlags                         //reserved must be 0
                                    );

            internal static extern IntPtr  FindFirstUrlCacheGroup(
                                    [In]  _WinInetCache.GroupFlag     flags,
                                    [In]  _WinInetCache.GroupSrchType searchFilter,
                                    [In]  IntPtr                     searchConditionPtr, //must be null
                                    [In]  int                        searchConditionSz,  //must be 0
                                    [Out] out WinInet.GroupId        groupId,
                                    [In]  IntPtr                     lpReserved          //was [In,Out] must be IntPtr.Zero
                                    );

            internal static extern bool FindNextUrlCacheGroup(
                                    [In]  IntPtr                    hFind,
                                    [Out] out _WinInetCache.GroupId  groupId,
                                    [In]  IntPtr                    lpReserved          //was [In,Out] must be IntPtr.Zero
                                    );

            internal static extern bool GetUrlCacheGroupAttribute(
                                    [In]   _WinInetCache.GroupId     groupId,
                                    [In]   int                      flags,              //must 0
                                    [In]   _WinInetCache.GroupAttr   attr,
                                    [Out] out _WinInetCache.GroupInfo groupInfo,
                                    [In, Out] ref int               groupInfoSize,
                                    [In]  IntPtr                    lpReserved          //was [In,Out] must be IntPtr.Zero
                                    );

            internal static extern bool SetUrlCacheGroupAttribute(
                                    [In]  _WinInetCache.GroupId      groupId,
                                    [In]  int                       flags,              //must be 0
                                    [In]  _WinInetCache.GroupAttr    attr,
                                    [In]  _WinInetCache.GroupInfo    groupInfo,
                                    [In]  IntPtr                    lpReserved          //was [In,Out] must be IntPtr.Zero
                                    );

            internal static extern WinInet.GroupId CreateUrlCacheGroup(
                                    [In]  _WinInetCache.GroupFlag    flags,
                                    [In]  IntPtr                    lpReserved          //must be IntPtr.Zero
                                    );

            internal static extern bool DeleteUrlCacheGroup(
                                    [In]  _WinInetCache.GroupId      groupId,
                                    [In]  _WinInetCache.GroupFlag    flags,
                                    [In]  IntPtr                    lpReserved          //must be IntPtr.Zero
                                    );


            internal static extern bool SetUrlCacheEntryGroup(
                                    [In] string                     urlName,
                                    [In] _WinInetCache.GroupSetFlag  flags,
                                    [In] _WinInetCache.GroupId       groupId,
                                    [In] IntPtr                     groupAttributes,    // must pass NULL
                                    [In] int                        groupAttrCount,     // must pass 0
                                    [In] IntPtr                     lpReserved          // must pass NULL
                                    );

            unsafe internal static extern IntPtr FindFirstUrlCacheEntryEx(
                                    [In]      byte*             searchPattern,      //must be null
                                    [In]      int               dwFlags,            //must be 0
                                    [In]      CacheEntry.EntryType srchFilter,
                                    [In]      WinInet.GroupId   groupId,
                                    [In]      byte*             entryPtr,           //was [out]
                                    [In, Out] ref int           entryBufSize,
                                    [Out]     void*             lpReserved,         // must pass NULL
                                    [In]      void*             lpReserved2,        //was [In,Out] must be IntPtr.Zero
                                    [In]      void*             lpReserved3         // must pass NULL
                                    );

            unsafe internal static extern bool FindNextUrlCacheEntryEx(
                                    [In]     IntPtr             enumHandle,
                                    [In]     byte*              entryPtr,           //was [Out]
                                    [In, Out]ref int            entryBufSize,
                                    [In]     void*              lpReserved,         // [Out] must pass NULL
                                    [In]     void*              lpReserved2,        // [In] [Out] must pass NULL
                                    [In]     void*              lpReserved3         // must pass NULL
                                    );

            unsafe internal static extern IntPtr FindFirstUrlCacheEntry(
                                    [In]     string             searchPattern,
                                    [In]     byte*              entryPtr,           //was [Out]
                                    [In, Out]ref int            entryBufSize
                                    );


            unsafe internal static extern bool FindNextUrlCacheEntry(
                                    [In]     IntPtr             enumHandle,
                                    [In]     byte*              entryPtr,           //was [Out]
                                    [In, Out]ref int            entryBufSize
                                    );

            internal static extern bool FindCloseUrlCache( [In] IntPtr enumHandle);

    /**********/
        }

        [SuppressUnmanagedCodeSecurityAttribute]
        internal static class SspiHelper
        {
            [DllImport(SECUR32, ExactSpelling = true, SetLastError = true)]
            [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
            internal unsafe static extern SecurityStatus SspiFreeAuthIdentity(
                [In] IntPtr authData);

            [SuppressMessage("Microsoft.Security", "CA2118:ReviewSuppressUnmanagedCodeSecurityUsage", Justification = "Implementation requires unmanaged code usage")]
            [DllImport(SECUR32, ExactSpelling = true, SetLastError = true, CharSet = CharSet.Unicode)]
            internal unsafe static extern SecurityStatus SspiEncodeStringsAsAuthIdentity(
                [In] string userName,
                [In] string domainName,
                [In] string password,
                [Out] out SafeSspiAuthDataHandle authData);
        }

#endif // !FEATURE_PAL

#if !FEATURE_PAL
        [System.Security.SuppressUnmanagedCodeSecurityAttribute()]
        internal static unsafe class HttpApi {
            
            [DllImport(HTTPAPI, ExactSpelling=true, CallingConvention=CallingConvention.StdCall, SetLastError=true)]
            internal static extern uint HttpInitialize(HTTPAPI_VERSION version, uint flags, void* pReserved);

            [DllImport(HTTPAPI, ExactSpelling=true, CallingConvention=CallingConvention.StdCall, SetLastError=true)]
            internal static extern uint HttpReceiveRequestEntityBody(CriticalHandle requestQueueHandle, ulong requestId, uint flags, void* pEntityBuffer, uint entityBufferLength, out uint bytesReturned, NativeOverlapped* pOverlapped);
            [DllImport(HTTPAPI, EntryPoint = "HttpReceiveRequestEntityBody", ExactSpelling = true, CallingConvention = CallingConvention.StdCall, SetLastError = true)]
            internal static extern uint HttpReceiveRequestEntityBody2(CriticalHandle requestQueueHandle, ulong requestId, uint flags, void* pEntityBuffer, uint entityBufferLength, out uint bytesReturned, [In] SafeHandle pOverlapped);

            [SuppressMessage("Microsoft.Security", "CA2118:ReviewSuppressUnmanagedCodeSecurityUsage", Justification = "Implementation requires unmanaged code usage")]
            [DllImport(HTTPAPI, ExactSpelling=true, CallingConvention=CallingConvention.StdCall, SetLastError=true)]
            internal static extern uint HttpReceiveClientCertificate(CriticalHandle requestQueueHandle, ulong connectionId, uint flags, HTTP_SSL_CLIENT_CERT_INFO* pSslClientCertInfo, uint sslClientCertInfoSize, uint* pBytesReceived, NativeOverlapped* pOverlapped);

            [SuppressMessage("Microsoft.Security", "CA2118:ReviewSuppressUnmanagedCodeSecurityUsage", Justification = "Implementation requires unmanaged code usage")]
            [DllImport(HTTPAPI, ExactSpelling=true, CallingConvention=CallingConvention.StdCall, SetLastError=true)]
            internal static extern uint HttpReceiveClientCertificate(CriticalHandle requestQueueHandle, ulong connectionId, uint flags, byte* pSslClientCertInfo, uint sslClientCertInfoSize, uint* pBytesReceived, NativeOverlapped* pOverlapped);

            [SuppressMessage("Microsoft.Security", "CA2118:ReviewSuppressUnmanagedCodeSecurityUsage", Justification = "Implementation requires unmanaged code usage")]
            [DllImport(HTTPAPI, ExactSpelling=true, CallingConvention=CallingConvention.StdCall, SetLastError=true)]
            internal static extern uint HttpReceiveHttpRequest(CriticalHandle requestQueueHandle, ulong requestId, uint flags, HTTP_REQUEST* pRequestBuffer, uint requestBufferLength, uint* pBytesReturned, NativeOverlapped* pOverlapped);

            [SuppressMessage("Microsoft.Security", "CA2118:ReviewSuppressUnmanagedCodeSecurityUsage", Justification = "Implementation requires unmanaged code usage")]
            [DllImport(HTTPAPI, ExactSpelling=true, CallingConvention=CallingConvention.StdCall, SetLastError=true)]
            internal static extern uint HttpSendHttpResponse(CriticalHandle requestQueueHandle, ulong requestId, uint flags, HTTP_RESPONSE* pHttpResponse, void* pCachePolicy, uint* pBytesSent, SafeLocalFree pRequestBuffer, uint requestBufferLength, NativeOverlapped* pOverlapped, void* pLogData);

            [SuppressMessage("Microsoft.Security", "CA2118:ReviewSuppressUnmanagedCodeSecurityUsage", Justification = "Implementation requires unmanaged code usage")]
            [DllImport(HTTPAPI, ExactSpelling=true, CallingConvention=CallingConvention.StdCall, SetLastError=true)]
            internal static extern uint HttpSendResponseEntityBody(CriticalHandle requestQueueHandle, ulong requestId, uint flags, ushort entityChunkCount, HTTP_DATA_CHUNK* pEntityChunks, uint* pBytesSent, SafeLocalFree pRequestBuffer, uint requestBufferLength, NativeOverlapped* pOverlapped, void* pLogData);

            [SuppressMessage("Microsoft.Security", "CA2118:ReviewSuppressUnmanagedCodeSecurityUsage", Justification = "Implementation requires unmanaged code usage")]
            [DllImport(HTTPAPI, ExactSpelling = true, CallingConvention = CallingConvention.StdCall, SetLastError = true)]
            internal static extern uint HttpCancelHttpRequest(CriticalHandle requestQueueHandle, ulong requestId, IntPtr pOverlapped);

            [SuppressMessage("Microsoft.Security", "CA2118:ReviewSuppressUnmanagedCodeSecurityUsage", Justification = "Implementation requires unmanaged code usage")]
            [DllImport(HTTPAPI, EntryPoint = "HttpSendResponseEntityBody", ExactSpelling = true, CallingConvention = CallingConvention.StdCall, SetLastError = true)]
            internal static extern uint HttpSendResponseEntityBody2(CriticalHandle requestQueueHandle, ulong requestId, uint flags, ushort entityChunkCount, IntPtr pEntityChunks, out uint pBytesSent, SafeLocalFree pRequestBuffer, uint requestBufferLength, SafeHandle pOverlapped, IntPtr pLogData);

            [SuppressMessage("Microsoft.Security", "CA2118:ReviewSuppressUnmanagedCodeSecurityUsage", Justification = "Implementation requires unmanaged code usage")]
            [DllImport(HTTPAPI, ExactSpelling=true, CallingConvention=CallingConvention.StdCall, SetLastError=true)]
            internal static extern uint HttpWaitForDisconnect(CriticalHandle requestQueueHandle, ulong connectionId, NativeOverlapped* pOverlapped);

            [SuppressMessage("Microsoft.Security", "CA2118:ReviewSuppressUnmanagedCodeSecurityUsage", Justification = "Implementation requires unmanaged code usage")]
            [DllImport(HTTPAPI, ExactSpelling = true, CallingConvention = CallingConvention.StdCall, SetLastError = true)]
            internal static extern uint HttpCreateServerSession(HTTPAPI_VERSION version, ulong* serverSessionId, uint reserved);

            [SuppressMessage("Microsoft.Security", "CA2118:ReviewSuppressUnmanagedCodeSecurityUsage", Justification = "Implementation requires unmanaged code usage")]
            [DllImport(HTTPAPI, ExactSpelling = true, CallingConvention = CallingConvention.StdCall, SetLastError = true)]
            internal static extern uint HttpCreateUrlGroup(ulong serverSessionId, ulong* urlGroupId, uint reserved);

            [SuppressMessage("Microsoft.Security", "CA2118:ReviewSuppressUnmanagedCodeSecurityUsage", Justification = "Implementation requires unmanaged code usage")]
            [DllImport(HTTPAPI, ExactSpelling = true, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode, SetLastError = true)]
            internal static extern uint HttpAddUrlToUrlGroup(ulong urlGroupId, string pFullyQualifiedUrl, ulong context, uint pReserved);

            [SuppressMessage("Microsoft.Security", "CA2118:ReviewSuppressUnmanagedCodeSecurityUsage", Justification = "Implementation requires unmanaged code usage")]
            [DllImport(HTTPAPI, ExactSpelling = true, CallingConvention = CallingConvention.StdCall, SetLastError = true)]
            internal static extern uint HttpSetUrlGroupProperty(ulong urlGroupId, HTTP_SERVER_PROPERTY serverProperty, IntPtr pPropertyInfo, uint propertyInfoLength);

            [SuppressMessage("Microsoft.Security", "CA2118:ReviewSuppressUnmanagedCodeSecurityUsage", Justification = "Implementation requires unmanaged code usage")]
            [DllImport(HTTPAPI, ExactSpelling = true, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode, SetLastError = true)]
            internal static extern uint HttpRemoveUrlFromUrlGroup(ulong urlGroupId, string pFullyQualifiedUrl, uint flags);

            [DllImport(HTTPAPI, ExactSpelling = true, CallingConvention = CallingConvention.StdCall, SetLastError = true)]
            internal static extern uint HttpCloseServerSession(ulong serverSessionId);

            [SuppressMessage("Microsoft.Security", "CA2118:ReviewSuppressUnmanagedCodeSecurityUsage", Justification = "Implementation requires unmanaged code usage")]
            [DllImport(HTTPAPI, ExactSpelling = true, CallingConvention = CallingConvention.StdCall, SetLastError = true)]
            internal static extern uint HttpCloseUrlGroup(ulong urlGroupId);

            internal enum HTTP_API_VERSION {
                Invalid,
                Version10,
                Version20,
            }

            // see http.w for definitions
            internal enum HTTP_SERVER_PROPERTY {
                HttpServerAuthenticationProperty,
                HttpServerLoggingProperty,
                HttpServerQosProperty,
                HttpServerTimeoutsProperty,
                HttpServerQueueLengthProperty,
                HttpServerStateProperty,
                HttpServer503VerbosityProperty,
                HttpServerBindingProperty,
                HttpServerExtendedAuthenticationProperty,
                HttpServerListenEndpointProperty,
                HttpServerChannelBindProperty,
                HttpServerProtectionLevelProperty,
            }

            //
            // Currently only one request info type is supported but the enum is for future extensibility.
            //
            internal enum HTTP_REQUEST_INFO_TYPE {
                HttpRequestInfoTypeAuth,
            }

            internal enum HTTP_RESPONSE_INFO_TYPE {
                HttpResponseInfoTypeMultipleKnownHeaders,
                HttpResponseInfoTypeAuthenticationProperty,
                HttpResponseInfoTypeQosProperty ,
            }

            internal enum HTTP_TIMEOUT_TYPE {
                EntityBody,
                DrainEntityBody,
                RequestQueue,
                IdleConnection,
                HeaderWait,
                MinSendRate,
            }

            internal const int MaxTimeout = 6;

            [StructLayout(LayoutKind.Sequential)]
            internal struct HTTP_VERSION {
                internal ushort MajorVersion;
                internal ushort MinorVersion;
            }

            [StructLayout(LayoutKind.Sequential)]
            internal struct HTTP_KNOWN_HEADER {
                internal ushort RawValueLength;
                internal sbyte* pRawValue;
            }

            [StructLayout(LayoutKind.Sequential, Size=32)]
            internal struct HTTP_DATA_CHUNK {
                internal HTTP_DATA_CHUNK_TYPE DataChunkType;
                internal uint p0;
                internal byte* pBuffer;
                internal uint BufferLength;
            }

            [StructLayout(LayoutKind.Sequential)]
            internal struct HTTPAPI_VERSION {
                internal ushort HttpApiMajorVersion;
                internal ushort HttpApiMinorVersion;
            }

            [StructLayout(LayoutKind.Sequential)]
            internal struct HTTP_COOKED_URL {
                internal ushort FullUrlLength;
                internal ushort HostLength;
                internal ushort AbsPathLength;
                internal ushort QueryStringLength;
                internal ushort* pFullUrl;
                internal ushort* pHost;
                internal ushort* pAbsPath;
                internal ushort* pQueryString;
            }

            [StructLayout(LayoutKind.Sequential)]
            internal struct SOCKADDR {
                internal ushort sa_family;
                internal byte sa_data;
                internal byte sa_data_02;
                internal byte sa_data_03;
                internal byte sa_data_04;
                internal byte sa_data_05;
                internal byte sa_data_06;
                internal byte sa_data_07;
                internal byte sa_data_08;
                internal byte sa_data_09;
                internal byte sa_data_10;
                internal byte sa_data_11;
                internal byte sa_data_12;
                internal byte sa_data_13;
                internal byte sa_data_14;
            }

            [StructLayout(LayoutKind.Sequential)]
            internal struct HTTP_TRANSPORT_ADDRESS {
                internal SOCKADDR* pRemoteAddress;
                internal SOCKADDR* pLocalAddress;
            }

            [StructLayout(LayoutKind.Sequential)]
            internal struct HTTP_SSL_CLIENT_CERT_INFO {
                internal uint CertFlags;
                internal uint CertEncodedSize;
                internal byte* pCertEncoded;
                internal void* Token;
                internal byte CertDeniedByMapper;
            }

            internal enum HTTP_SERVICE_BINDING_TYPE : uint { 
                HttpServiceBindingTypeNone = 0,
                HttpServiceBindingTypeW,
                HttpServiceBindingTypeA
            }

            [StructLayout(LayoutKind.Sequential)]
            internal struct HTTP_SERVICE_BINDING_BASE
            {
                internal HTTP_SERVICE_BINDING_TYPE Type;
            }

            [StructLayout(LayoutKind.Sequential)]
            internal struct HTTP_REQUEST_CHANNEL_BIND_STATUS
            {
                internal IntPtr ServiceName;
                internal IntPtr ChannelToken;
                internal uint ChannelTokenSize;
                internal uint Flags;
            }

            [StructLayout(LayoutKind.Sequential)]
            internal struct HTTP_UNKNOWN_HEADER {
                internal ushort NameLength;
                internal ushort RawValueLength;
                internal sbyte* pName;
                internal sbyte* pRawValue;
            }

            [StructLayout(LayoutKind.Sequential)]
            internal struct HTTP_SSL_INFO {
                internal ushort ServerCertKeySize;
                internal ushort ConnectionKeySize;
                internal uint ServerCertIssuerSize;
                internal uint ServerCertSubjectSize;
                internal sbyte* pServerCertIssuer;
                internal sbyte* pServerCertSubject;
                internal HTTP_SSL_CLIENT_CERT_INFO* pClientCertInfo;
                internal uint SslClientCertNegotiated;
            }

            [StructLayout(LayoutKind.Sequential)]
            internal struct HTTP_RESPONSE_HEADERS {
                internal ushort UnknownHeaderCount;
                internal HTTP_UNKNOWN_HEADER* pUnknownHeaders;
                internal ushort TrailerCount;
                internal HTTP_UNKNOWN_HEADER* pTrailers;
                internal HTTP_KNOWN_HEADER KnownHeaders;
                internal HTTP_KNOWN_HEADER KnownHeaders_02;
                internal HTTP_KNOWN_HEADER KnownHeaders_03;
                internal HTTP_KNOWN_HEADER KnownHeaders_04;
                internal HTTP_KNOWN_HEADER KnownHeaders_05;
                internal HTTP_KNOWN_HEADER KnownHeaders_06;
                internal HTTP_KNOWN_HEADER KnownHeaders_07;
                internal HTTP_KNOWN_HEADER KnownHeaders_08;
                internal HTTP_KNOWN_HEADER KnownHeaders_09;
                internal HTTP_KNOWN_HEADER KnownHeaders_10;
                internal HTTP_KNOWN_HEADER KnownHeaders_11;
                internal HTTP_KNOWN_HEADER KnownHeaders_12;
                internal HTTP_KNOWN_HEADER KnownHeaders_13;
                internal HTTP_KNOWN_HEADER KnownHeaders_14;
                internal HTTP_KNOWN_HEADER KnownHeaders_15;
                internal HTTP_KNOWN_HEADER KnownHeaders_16;
                internal HTTP_KNOWN_HEADER KnownHeaders_17;
                internal HTTP_KNOWN_HEADER KnownHeaders_18;
                internal HTTP_KNOWN_HEADER KnownHeaders_19;
                internal HTTP_KNOWN_HEADER KnownHeaders_20;
                internal HTTP_KNOWN_HEADER KnownHeaders_21;
                internal HTTP_KNOWN_HEADER KnownHeaders_22;
                internal HTTP_KNOWN_HEADER KnownHeaders_23;
                internal HTTP_KNOWN_HEADER KnownHeaders_24;
                internal HTTP_KNOWN_HEADER KnownHeaders_25;
                internal HTTP_KNOWN_HEADER KnownHeaders_26;
                internal HTTP_KNOWN_HEADER KnownHeaders_27;
                internal HTTP_KNOWN_HEADER KnownHeaders_28;
                internal HTTP_KNOWN_HEADER KnownHeaders_29;
                internal HTTP_KNOWN_HEADER KnownHeaders_30;
            }

            [StructLayout(LayoutKind.Sequential)]
            internal struct HTTP_REQUEST_HEADERS {
                internal ushort UnknownHeaderCount;
                internal HTTP_UNKNOWN_HEADER* pUnknownHeaders;
                internal ushort TrailerCount;
                internal HTTP_UNKNOWN_HEADER* pTrailers;
                internal HTTP_KNOWN_HEADER KnownHeaders;
                internal HTTP_KNOWN_HEADER KnownHeaders_02;
                internal HTTP_KNOWN_HEADER KnownHeaders_03;
                internal HTTP_KNOWN_HEADER KnownHeaders_04;
                internal HTTP_KNOWN_HEADER KnownHeaders_05;
                internal HTTP_KNOWN_HEADER KnownHeaders_06;
                internal HTTP_KNOWN_HEADER KnownHeaders_07;
                internal HTTP_KNOWN_HEADER KnownHeaders_08;
                internal HTTP_KNOWN_HEADER KnownHeaders_09;
                internal HTTP_KNOWN_HEADER KnownHeaders_10;
                internal HTTP_KNOWN_HEADER KnownHeaders_11;
                internal HTTP_KNOWN_HEADER KnownHeaders_12;
                internal HTTP_KNOWN_HEADER KnownHeaders_13;
                internal HTTP_KNOWN_HEADER KnownHeaders_14;
                internal HTTP_KNOWN_HEADER KnownHeaders_15;
                internal HTTP_KNOWN_HEADER KnownHeaders_16;
                internal HTTP_KNOWN_HEADER KnownHeaders_17;
                internal HTTP_KNOWN_HEADER KnownHeaders_18;
                internal HTTP_KNOWN_HEADER KnownHeaders_19;
                internal HTTP_KNOWN_HEADER KnownHeaders_20;
                internal HTTP_KNOWN_HEADER KnownHeaders_21;
                internal HTTP_KNOWN_HEADER KnownHeaders_22;
                internal HTTP_KNOWN_HEADER KnownHeaders_23;
                internal HTTP_KNOWN_HEADER KnownHeaders_24;
                internal HTTP_KNOWN_HEADER KnownHeaders_25;
                internal HTTP_KNOWN_HEADER KnownHeaders_26;
                internal HTTP_KNOWN_HEADER KnownHeaders_27;
                internal HTTP_KNOWN_HEADER KnownHeaders_28;
                internal HTTP_KNOWN_HEADER KnownHeaders_29;
                internal HTTP_KNOWN_HEADER KnownHeaders_30;
                internal HTTP_KNOWN_HEADER KnownHeaders_31;
                internal HTTP_KNOWN_HEADER KnownHeaders_32;
                internal HTTP_KNOWN_HEADER KnownHeaders_33;
                internal HTTP_KNOWN_HEADER KnownHeaders_34;
                internal HTTP_KNOWN_HEADER KnownHeaders_35;
                internal HTTP_KNOWN_HEADER KnownHeaders_36;
                internal HTTP_KNOWN_HEADER KnownHeaders_37;
                internal HTTP_KNOWN_HEADER KnownHeaders_38;
                internal HTTP_KNOWN_HEADER KnownHeaders_39;
                internal HTTP_KNOWN_HEADER KnownHeaders_40;
                internal HTTP_KNOWN_HEADER KnownHeaders_41;
            }

            internal enum HTTP_VERB : int {
                HttpVerbUnparsed = 0,
                HttpVerbUnknown = 1,
                HttpVerbInvalid = 2,
                HttpVerbOPTIONS = 3,
                HttpVerbGET = 4,
                HttpVerbHEAD = 5,
                HttpVerbPOST = 6,
                HttpVerbPUT = 7,
                HttpVerbDELETE = 8,
                HttpVerbTRACE = 9,
                HttpVerbCONNECT = 10,
                HttpVerbTRACK = 11,
                HttpVerbMOVE = 12,
                HttpVerbCOPY = 13,
                HttpVerbPROPFIND = 14,
                HttpVerbPROPPATCH = 15,
                HttpVerbMKCOL = 16,
                HttpVerbLOCK = 17,
                HttpVerbUNLOCK = 18,
                HttpVerbSEARCH = 19,
                HttpVerbMaximum = 20,
            }

            internal static readonly string[] HttpVerbs = new string[] {
                null,
                "Unknown",
                "Invalid",
                "OPTIONS",
                "GET",
                "HEAD",
                "POST",
                "PUT",
                "DELETE",
                "TRACE",
                "CONNECT",
                "TRACK",
                "MOVE",
                "COPY",
                "PROPFIND",
                "PROPPATCH",
                "MKCOL",
                "LOCK",
                "UNLOCK",
                "SEARCH",
            };

            internal enum HTTP_DATA_CHUNK_TYPE : int {
                HttpDataChunkFromMemory = 0,
                HttpDataChunkFromFileHandle = 1,
                HttpDataChunkFromFragmentCache = 2,
                HttpDataChunkMaximum = 3,
            }

            [StructLayout(LayoutKind.Sequential)]
            internal struct HTTP_RESPONSE_INFO {
                internal HTTP_RESPONSE_INFO_TYPE Type;
                internal uint Length;
                internal void* pInfo;
            }

            [StructLayout(LayoutKind.Sequential)]
            internal struct HTTP_RESPONSE {
                internal uint Flags;
                internal HTTP_VERSION Version;
                internal ushort StatusCode;
                internal ushort ReasonLength;
                internal sbyte* pReason;
                internal HTTP_RESPONSE_HEADERS Headers;
                internal ushort EntityChunkCount;
                internal HTTP_DATA_CHUNK* pEntityChunks;
                internal ushort ResponseInfoCount;
                internal HTTP_RESPONSE_INFO* pResponseInfo;
            }

            [StructLayout(LayoutKind.Sequential)]
            internal struct HTTP_REQUEST_INFO {
                internal HTTP_REQUEST_INFO_TYPE InfoType;
                internal uint InfoLength;
                internal void* pInfo;
            }

            [StructLayout(LayoutKind.Sequential)]
            internal struct HTTP_REQUEST {
                internal uint Flags;
                internal ulong ConnectionId;
                internal ulong RequestId;
                internal ulong UrlContext;
                internal HTTP_VERSION Version;
                internal HTTP_VERB Verb;
                internal ushort UnknownVerbLength;
                internal ushort RawUrlLength;
                internal sbyte* pUnknownVerb;
                internal sbyte* pRawUrl;
                internal HTTP_COOKED_URL CookedUrl;
                internal HTTP_TRANSPORT_ADDRESS Address;
                internal HTTP_REQUEST_HEADERS Headers;
                internal ulong BytesReceived;
                internal ushort EntityChunkCount;
                internal HTTP_DATA_CHUNK* pEntityChunks;
                internal ulong RawConnectionId;
                internal HTTP_SSL_INFO* pSslInfo;
                internal ushort RequestInfoCount;
                internal HTTP_REQUEST_INFO* pRequestInfo;
            }

            [StructLayout(LayoutKind.Sequential)]
            internal struct HTTP_TIMEOUT_LIMIT_INFO {
                internal HTTP_FLAGS Flags;
                internal ushort EntityBody;
                internal ushort DrainEntityBody;
                internal ushort RequestQueue;
                internal ushort IdleConnection;
                internal ushort HeaderWait;
                internal uint MinSendRate;
            }

            [StructLayout(LayoutKind.Sequential)]
            internal struct HTTP_BINDING_INFO {
                internal HTTP_FLAGS Flags;
                internal IntPtr RequestQueueHandle;                
            }

            // see http.w for definitions
            [Flags]
            internal enum HTTP_FLAGS : uint {
                NONE                                = 0x00000000,
                HTTP_RECEIVE_REQUEST_FLAG_COPY_BODY = 0x00000001,
                HTTP_RECEIVE_SECURE_CHANNEL_TOKEN   = 0x00000001,
                HTTP_SEND_RESPONSE_FLAG_DISCONNECT  = 0x00000001,
                HTTP_SEND_RESPONSE_FLAG_MORE_DATA   = 0x00000002,
                HTTP_SEND_RESPONSE_FLAG_BUFFER_DATA = 0x00000004,
                HTTP_SEND_RESPONSE_FLAG_RAW_HEADER  = 0x00000004,
                HTTP_SEND_REQUEST_FLAG_MORE_DATA    = 0x00000001,
                HTTP_PROPERTY_FLAG_PRESENT          = 0x00000001,
                HTTP_INITIALIZE_SERVER              = 0x00000001,
                HTTP_INITIALIZE_CBT                 = 0x00000004,
                HTTP_SEND_RESPONSE_FLAG_OPAQUE      = 0x00000040,
            }

            const int HttpHeaderRequestMaximum  = (int)HttpRequestHeader.UserAgent + 1;
            const int HttpHeaderResponseMaximum = (int)HttpResponseHeader.WwwAuthenticate + 1;

            internal static class HTTP_REQUEST_HEADER_ID {
                internal static string ToString(int position) {
                    return m_Strings[position];
                }

                private static string[] m_Strings = {
                    "Cache-Control",
                    "Connection",
                    "Date",
                    "Keep-Alive",
                    "Pragma",
                    "Trailer",
                    "Transfer-Encoding",
                    "Upgrade",
                    "Via",
                    "Warning",

                    "Allow",
                    "Content-Length",
                    "Content-Type",
                    "Content-Encoding",
                    "Content-Language",
                    "Content-Location",
                    "Content-MD5",
                    "Content-Range",
                    "Expires",
                    "Last-Modified",

                    "Accept",
                    "Accept-Charset",
                    "Accept-Encoding",
                    "Accept-Language",
                    "Authorization",
                    "Cookie",
                    "Expect",
                    "From",
                    "Host",
                    "If-Match",

                    "If-Modified-Since",
                    "If-None-Match",
                    "If-Range",
                    "If-Unmodified-Since",
                    "Max-Forwards",
                    "Proxy-Authorization",
                    "Referer",
                    "Range",
                    "Te",
                    "Translate",
                    "User-Agent",
                };
            }

            internal static class HTTP_RESPONSE_HEADER_ID {
                private static Hashtable m_Hashtable;

                static HTTP_RESPONSE_HEADER_ID() {
                    m_Hashtable = new Hashtable((int)Enum.HttpHeaderResponseMaximum);
                    for (int i = 0; i < (int)Enum.HttpHeaderResponseMaximum; i++) {
                        m_Hashtable.Add(m_Strings[i], i);
                    }
                }

                internal static int IndexOfKnownHeader(string HeaderName) {
                    object index = m_Hashtable[HeaderName];
                    return index==null ? -1 : (int)index;
    }

                internal static string ToString(int position) {
                    return m_Strings[position];
}

                internal enum Enum {
                    HttpHeaderCacheControl          = 0,    // general-header [section 4.5]
                    HttpHeaderConnection            = 1,    // general-header [section 4.5]
                    HttpHeaderDate                  = 2,    // general-header [section 4.5]
                    HttpHeaderKeepAlive             = 3,    // general-header [not in rfc]
                    HttpHeaderPragma                = 4,    // general-header [section 4.5]
                    HttpHeaderTrailer               = 5,    // general-header [section 4.5]
                    HttpHeaderTransferEncoding      = 6,    // general-header [section 4.5]
                    HttpHeaderUpgrade               = 7,    // general-header [section 4.5]
                    HttpHeaderVia                   = 8,    // general-header [section 4.5]
                    HttpHeaderWarning               = 9,    // general-header [section 4.5]

                    HttpHeaderAllow                 = 10,   // entity-header  [section 7.1]
                    HttpHeaderContentLength         = 11,   // entity-header  [section 7.1]
                    HttpHeaderContentType           = 12,   // entity-header  [section 7.1]
                    HttpHeaderContentEncoding       = 13,   // entity-header  [section 7.1]
                    HttpHeaderContentLanguage       = 14,   // entity-header  [section 7.1]
                    HttpHeaderContentLocation       = 15,   // entity-header  [section 7.1]
                    HttpHeaderContentMd5            = 16,   // entity-header  [section 7.1]
                    HttpHeaderContentRange          = 17,   // entity-header  [section 7.1]
                    HttpHeaderExpires               = 18,   // entity-header  [section 7.1]
                    HttpHeaderLastModified          = 19,   // entity-header  [section 7.1]


                    // Response Headers

                    HttpHeaderAcceptRanges          = 20,   // response-header [section 6.2]
                    HttpHeaderAge                   = 21,   // response-header [section 6.2]
                    HttpHeaderEtag                  = 22,   // response-header [section 6.2]
                    HttpHeaderLocation              = 23,   // response-header [section 6.2]
                    HttpHeaderProxyAuthenticate     = 24,   // response-header [section 6.2]
                    HttpHeaderRetryAfter            = 25,   // response-header [section 6.2]
                    HttpHeaderServer                = 26,   // response-header [section 6.2]
                    HttpHeaderSetCookie             = 27,   // response-header [not in rfc]
                    HttpHeaderVary                  = 28,   // response-header [section 6.2]
                    HttpHeaderWwwAuthenticate       = 29,   // response-header [section 6.2]

                    HttpHeaderResponseMaximum       = 30,


                    HttpHeaderMaximum               = 41
                }

                private static string[] m_Strings = {
                    "Cache-Control",
                    "Connection",
                    "Date",
                    "Keep-Alive",
                    "Pragma",
                    "Trailer",
                    "Transfer-Encoding",
                    "Upgrade",
                    "Via",
                    "Warning",

                    "Allow",
                    "Content-Length",
                    "Content-Type",
                    "Content-Encoding",
                    "Content-Language",
                    "Content-Location",
                    "Content-MD5",
                    "Content-Range",
                    "Expires",
                    "Last-Modified",

                    "Accept-Ranges",
                    "Age",
                    "ETag",
                    "Location",
                    "Proxy-Authenticate",
                    "Retry-After",
                    "Server",
                    "Set-Cookie",
                    "Vary",
                    "WWW-Authenticate",
                };
            }

            private static HTTPAPI_VERSION version;
            private static volatile bool extendedProtectionSupported;

            //
            // This property is used by HttpListener to pass the version structure to the native layer in API
            // calls. 
            //
            internal static HTTPAPI_VERSION Version {
                get {
                    return version;
                }
            }

            //
            // This property is used by HttpListener to get the Api version in use so that it uses appropriate 
            // Http APIs.
            //
            internal static HTTP_API_VERSION ApiVersion {
                get {
                    if (version.HttpApiMajorVersion == 2 && version.HttpApiMinorVersion == 0) {
                        return HTTP_API_VERSION.Version20;
                    } 
                    else if (version.HttpApiMajorVersion == 1 && version.HttpApiMinorVersion == 0) {
                        return HTTP_API_VERSION.Version10;
                    } 
                    else {
                        return HTTP_API_VERSION.Invalid;
                    }
                }
            }

            //
            // returns 'true' if http.sys supports CBT: either the system is Win7+, or http.sys was patched.
            //
            internal static bool ExtendedProtectionSupported {
                get {
                    return extendedProtectionSupported;
                }
            }

            static HttpApi() {
                InitHttpApi(2, 0);
            }

            private static void InitHttpApi(ushort majorVersion, ushort minorVersion) {
                version.HttpApiMajorVersion = majorVersion;
                version.HttpApiMinorVersion = minorVersion;

                GlobalLog.Print("HttpApi::.ctor() calling HttpApi.HttpInitialize() for Version " + majorVersion + "." + minorVersion);

                // For pre-Win7 OS versions, we need to check whether http.sys contains the CBT patch.
                // We do so by passing HTTP_INITIALIZE_CBT flag to HttpInitialize. If the flag is not 
                // supported, http.sys is not patched. Note that http.sys will return invalid parameter
                // also on Win7, even though it shipped with CBT support. Therefore we must not pass
                // the flag on Win7 and later.
                uint statusCode = ErrorCodes.ERROR_SUCCESS;
                extendedProtectionSupported = true;

                if (ComNetOS.IsWin7orLater) {
                    // on Win7 and later, we don't pass the CBT flag. CBT is always supported.
                    statusCode = HttpApi.HttpInitialize(version, (uint)HTTP_FLAGS.HTTP_INITIALIZE_SERVER, null);
                }
                else {
                    statusCode = HttpApi.HttpInitialize(version,
                        (uint)(HTTP_FLAGS.HTTP_INITIALIZE_SERVER | HTTP_FLAGS.HTTP_INITIALIZE_CBT), null);

                    // if the status code is INVALID_PARAMETER, http.sys does not support CBT.
                    if (statusCode == ErrorCodes.ERROR_INVALID_PARAMETER) {
                        if (Logging.On) Logging.PrintWarning(Logging.HttpListener, SR.GetString(SR.net_listener_cbt_not_supported));
                       
                        // try again without CBT flag: HttpListener can still be used, but doesn't support EP
                        extendedProtectionSupported = false;
                        statusCode = HttpApi.HttpInitialize(version, (uint)HTTP_FLAGS.HTTP_INITIALIZE_SERVER, null);
                    }
                }

                supported = statusCode == ErrorCodes.ERROR_SUCCESS;

                GlobalLog.Print("HttpApi::.ctor() call to HttpApi.HttpInitialize() returned:" + statusCode + " supported:" + supported);
            }

            static volatile bool supported;
            internal static bool Supported {
                get {
                    return supported;
                }
            }

            // Server API

            internal static WebHeaderCollection GetHeaders(byte[] memoryBlob, IntPtr originalAddress)
            {
                GlobalLog.Enter("HttpApi::GetHeaders()");

                // Return value.
                WebHeaderCollection headerCollection = new WebHeaderCollection(WebHeaderCollectionType.HttpListenerRequest);
                fixed (byte* pMemoryBlob = memoryBlob)
                {
                    HTTP_REQUEST* request = (HTTP_REQUEST*) pMemoryBlob;
                    long fixup = pMemoryBlob - (byte*) originalAddress;
                    int index;

                    // unknown headers
                    if (request->Headers.UnknownHeaderCount != 0)
                    {
                        HTTP_UNKNOWN_HEADER* pUnknownHeader = (HTTP_UNKNOWN_HEADER*) (fixup + (byte*) request->Headers.pUnknownHeaders);
                        for (index = 0; index < request->Headers.UnknownHeaderCount; index++)
                        {
                            // For unknown headers, when header value is empty, RawValueLength will be 0 and 
                            // pRawValue will be null.
                            if (pUnknownHeader->pName != null && pUnknownHeader->NameLength > 0)
                            {
                                string headerName = new string(pUnknownHeader->pName + fixup, 0, pUnknownHeader->NameLength);
                                string headerValue;
                                if (pUnknownHeader->pRawValue != null && pUnknownHeader->RawValueLength > 0) {
                                    headerValue = new string(pUnknownHeader->pRawValue + fixup, 0, pUnknownHeader->RawValueLength);
                                }
                                else {
                                    headerValue = string.Empty;
                                }
                                headerCollection.AddInternal(headerName, headerValue);
                            }
                            pUnknownHeader++;
                        }
                    }

                    // known headers
                    HTTP_KNOWN_HEADER* pKnownHeader = &request->Headers.KnownHeaders;
                    for (index = 0; index < HttpHeaderRequestMaximum; index++)
                    {
                        // For known headers, when header value is empty, RawValueLength will be 0 and 
                        // pRawValue will point to empty string ("\0")
                        if (pKnownHeader->pRawValue != null)
                        {
                            string headerValue = new string(pKnownHeader->pRawValue + fixup, 0, pKnownHeader->RawValueLength);
                            headerCollection.AddInternal(HTTP_REQUEST_HEADER_ID.ToString(index), headerValue);
                        }
                        pKnownHeader++;
                    }
                }

                GlobalLog.Leave("HttpApi::GetHeaders()");
                return headerCollection;
            }

            private static string GetKnownHeader(HTTP_REQUEST* request, long fixup, int headerIndex)
            {
                GlobalLog.Enter("HttpApi::GetKnownHeader()");
                string header = null;

                HTTP_KNOWN_HEADER* pKnownHeader = (&request->Headers.KnownHeaders) + headerIndex;
                GlobalLog.Print("HttpApi::GetKnownHeader() pKnownHeader:0x" + ((IntPtr) pKnownHeader).ToString("x"));
                GlobalLog.Print("HttpApi::GetKnownHeader() pRawValue:0x" + ((IntPtr) pKnownHeader->pRawValue).ToString("x") + " RawValueLength:" + pKnownHeader->RawValueLength.ToString());
                // For known headers, when header value is empty, RawValueLength will be 0 and 
                // pRawValue will point to empty string ("\0")
                if (pKnownHeader->pRawValue != null)
                {
                    header = new string(pKnownHeader->pRawValue + fixup, 0, pKnownHeader->RawValueLength);
                }

                GlobalLog.Leave("HttpApi::GetKnownHeader() return:" + ValidationHelper.ToString(header));
                return header;
            }

            internal static string GetKnownHeader(HTTP_REQUEST* request, int headerIndex)
            {
                return GetKnownHeader(request, 0, headerIndex);
            }

            internal static string GetKnownHeader(byte[] memoryBlob, IntPtr originalAddress, int headerIndex)
            {
                fixed (byte* pMemoryBlob = memoryBlob)
                {
                    return GetKnownHeader((HTTP_REQUEST*) pMemoryBlob, pMemoryBlob - (byte*) originalAddress, headerIndex);
                }
            }

            private unsafe static string GetVerb(HTTP_REQUEST* request, long fixup)
            {
                GlobalLog.Enter("HttpApi::GetVerb()");
                string verb = null;

                if ((int) request->Verb > (int) HTTP_VERB.HttpVerbUnknown && (int) request->Verb < (int) HTTP_VERB.HttpVerbMaximum)
                {
                    verb = HttpVerbs[(int) request->Verb];
                }
                else if (request->Verb == HTTP_VERB.HttpVerbUnknown && request->pUnknownVerb != null)
                {
                    verb = new string(request->pUnknownVerb + fixup, 0, request->UnknownVerbLength);
                }

                GlobalLog.Leave("HttpApi::GetVerb() return:" + ValidationHelper.ToString(verb));
                return verb;
            }

            internal unsafe static string GetVerb(HTTP_REQUEST* request)
            {
                return GetVerb(request, 0);
            }

            internal unsafe static string GetVerb(byte[] memoryBlob, IntPtr originalAddress)
            {
                fixed (byte* pMemoryBlob = memoryBlob)
                {
                    return GetVerb((HTTP_REQUEST*) pMemoryBlob, pMemoryBlob - (byte*) originalAddress);
                }
            }

            internal static HTTP_VERB GetKnownVerb(byte[] memoryBlob, IntPtr originalAddress)
            {
                GlobalLog.Enter("HttpApi::GetKnownVerb()");

                // Return value.
                HTTP_VERB verb = HTTP_VERB.HttpVerbUnknown;
                fixed (byte* pMemoryBlob = memoryBlob)
                {
                    HTTP_REQUEST* request = (HTTP_REQUEST*) pMemoryBlob;
                    if ((int)request->Verb > (int)HTTP_VERB.HttpVerbUnparsed && (int)request->Verb < (int)HTTP_VERB.HttpVerbMaximum)
                    {
                        verb = request->Verb;
                    }
                }

                GlobalLog.Leave("HttpApi::GetKnownVerb()");
                return verb;
            }

            internal static uint GetChunks(byte[] memoryBlob, IntPtr originalAddress, ref int dataChunkIndex, ref uint dataChunkOffset, byte[] buffer, int offset, int size)
            {
                GlobalLog.Enter("HttpApi::GetChunks() memoryBlob:" + ValidationHelper.ToString(memoryBlob));

                // Return value.
                uint dataRead = 0;
                fixed(byte* pMemoryBlob = memoryBlob)
                {
                    HTTP_REQUEST* request = (HTTP_REQUEST*) pMemoryBlob;
                    long fixup = pMemoryBlob - (byte*) originalAddress;

                    if (request->EntityChunkCount > 0 && dataChunkIndex < request->EntityChunkCount && dataChunkIndex != -1)
                    {
                        HTTP_DATA_CHUNK* pDataChunk = (HTTP_DATA_CHUNK*) (fixup + (byte*) &request->pEntityChunks[dataChunkIndex]);

                        fixed(byte* pReadBuffer = buffer)
                        {
                            byte* pTo = &pReadBuffer[offset];

                            while (dataChunkIndex < request->EntityChunkCount && dataRead < size){
                                if(dataChunkOffset >= pDataChunk->BufferLength){
                                    dataChunkOffset = 0;
                                    dataChunkIndex ++;
                                    pDataChunk++;
                                }
                                else{
                                    byte* pFrom = pDataChunk->pBuffer + dataChunkOffset + fixup;

                                    uint bytesToRead =  pDataChunk->BufferLength - (uint)dataChunkOffset;
                                    if (bytesToRead  > (uint)size){
                                        bytesToRead = (uint)size;
                                    }
                                    for (uint i=0;i<bytesToRead;i++)
                                    {
                                        *(pTo++) = *(pFrom++);
                                    }
                                    dataRead+=bytesToRead;
                                    dataChunkOffset += bytesToRead;
                                }
                            }
                        }
                    }
                    //we're finished.
                    if(dataChunkIndex ==  request->EntityChunkCount){
                        dataChunkIndex = -1;
                    }
                }

                GlobalLog.Leave("HttpApi::GetChunks()");
                return dataRead;
            }

            internal static IPEndPoint GetRemoteEndPoint(byte[] memoryBlob, IntPtr originalAddress)
            {
                GlobalLog.Enter("HttpApi::GetRemoteEndPoint()");

                SocketAddress v4address = new SocketAddress(AddressFamily.InterNetwork, SocketAddress.IPv4AddressSize);
                SocketAddress v6address = new SocketAddress(AddressFamily.InterNetworkV6, SocketAddress.IPv6AddressSize);

                fixed (byte* pMemoryBlob = memoryBlob)
                {
                    HTTP_REQUEST* request = (HTTP_REQUEST*) pMemoryBlob;
                    IntPtr address = request->Address.pRemoteAddress != null ? (IntPtr) (pMemoryBlob - (byte*) originalAddress + (byte*) request->Address.pRemoteAddress) : IntPtr.Zero;
                    CopyOutAddress(address, ref v4address, ref v6address);
                }

                IPEndPoint endpoint = null;
                if (v4address != null)
                {
                    endpoint = IPEndPoint.Any.Create(v4address) as IPEndPoint;
                }
                else if (v6address != null)
                {
                    endpoint = IPEndPoint.IPv6Any.Create(v6address) as IPEndPoint;
                }

                GlobalLog.Leave("HttpApi::GetRemoteEndPoint()");
                return endpoint;
            }

            internal static IPEndPoint GetLocalEndPoint(byte[] memoryBlob, IntPtr originalAddress)
            {
                GlobalLog.Enter("HttpApi::GetLocalEndPoint()");

                SocketAddress v4address = new SocketAddress(AddressFamily.InterNetwork, SocketAddress.IPv4AddressSize);
                SocketAddress v6address = new SocketAddress(AddressFamily.InterNetworkV6, SocketAddress.IPv6AddressSize);

                fixed (byte* pMemoryBlob = memoryBlob)
                {
                    HTTP_REQUEST* request = (HTTP_REQUEST*) pMemoryBlob;
                    IntPtr address = request->Address.pLocalAddress != null ? (IntPtr) (pMemoryBlob - (byte*) originalAddress + (byte*) request->Address.pLocalAddress) : IntPtr.Zero;
                    CopyOutAddress(address, ref v4address, ref v6address);
                }

                IPEndPoint endpoint = null;
                if (v4address != null)
                {
                    endpoint = IPEndPoint.Any.Create(v4address) as IPEndPoint;
                }
                else if (v6address != null)
                {
                    endpoint = IPEndPoint.IPv6Any.Create(v6address) as IPEndPoint;
                }

                GlobalLog.Leave("HttpApi::GetLocalEndPoint()");
                return endpoint;
            }

            [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
            private static void CopyOutAddress(IntPtr address, ref SocketAddress v4address, ref SocketAddress v6address)
            {
                if (address != IntPtr.Zero)
                {
                    ushort addressFamily = *((ushort*) address);
                    if (addressFamily == (ushort) AddressFamily.InterNetwork)
                    {
                        v6address = null;
                        fixed (byte* pBuffer = v4address.m_Buffer)
                        {
                            for (int index = 2; index < SocketAddress.IPv4AddressSize; index++)
                            {
                                pBuffer[index] = ((byte*) address)[index];
                            }
                        }
                        return;
                    }
                    if (addressFamily == (ushort) AddressFamily.InterNetworkV6)
                    {
                        v4address = null;
                        fixed (byte* pBuffer = v6address.m_Buffer)
                        {
                            for (int index = 2; index < SocketAddress.IPv6AddressSize; index++)
                            {
                                pBuffer[index] = ((byte*) address)[index];
                            }
                        }
                        return;
                    }
                }

                v4address = null;
                v6address = null;
            }
        }

        [SuppressUnmanagedCodeSecurity]
        internal unsafe static class SecureStringHelper
        {
#if DEBUG
            // this method is only called as part of an assert
            internal static bool AreEqualValues(SecureString secureString1, SecureString secureString2)
            {
                IntPtr bstr1 = IntPtr.Zero;
                IntPtr bstr2 = IntPtr.Zero;
                bool result = false;

                if (secureString1 == null)
                {
                    if (secureString2 == null)
                        return true;
                    else
                        return false;
                }
                else if (secureString2 == null)
                {
                    return false;
                }

                // strings are non-null at this point

                if ((object)secureString1 == (object)secureString2)
                    return true;  // same objects

                if (secureString1.Length != secureString2.Length)
                    return false;

                // strings are same length.  decrypt to unmanaged memory and compare them.

                try
                {
                    bstr1 = Marshal.SecureStringToBSTR(secureString1);
                    bstr2 = Marshal.SecureStringToBSTR(secureString2);
                    result = true;
                    for (int i = 0; i < secureString1.Length; i++)
                    {
                        if (*((char*)bstr1 + i) != *((char*)bstr2 + i))
                        {
                            result = false;
                            break;
                        }
                    }
                }
                finally
                {
                    if (bstr1 != IntPtr.Zero)
                        Marshal.ZeroFreeBSTR(bstr1);
                    if (bstr2 != IntPtr.Zero)
                        Marshal.ZeroFreeBSTR(bstr2);
                }
                return result;
            }
#endif

            internal static string CreateString(SecureString secureString)
            {
                string plainString;
                IntPtr bstr = IntPtr.Zero;

                if (secureString == null || secureString.Length == 0)
                    return String.Empty;

                try
                {
                    bstr = Marshal.SecureStringToBSTR(secureString);
                    plainString = Marshal.PtrToStringBSTR(bstr);
                }
                finally
                {
                    if (bstr != IntPtr.Zero)
                        Marshal.ZeroFreeBSTR(bstr);
                }
                return plainString;
            }

            internal static SecureString CreateSecureString(string plainString)
            {
                SecureString secureString;

                if (plainString == null || plainString.Length == 0)
                    return new SecureString();

                fixed (char* pch = plainString)
                {
                    secureString = new SecureString(pch, plainString.Length);
                }

                return secureString;
            }
        }
#endif // !FEATURE_PAL

#if !FEATURE_PAL
        internal const int CLSCTX_SERVER = 0x15;
        [DllImport(OLE32, PreserveSig=false)] 
        public static extern void CoCreateInstance(
            [In] ref Guid clsid,
            IntPtr pUnkOuter,
            int context,
            [In] ref Guid iid,
            [MarshalAs(UnmanagedType.IUnknown)] out Object o );
#endif // !FEATURE_PAL
    
        // Used to support Windows Store apps.  
        // This code was provided by Immo Landwerth from the CLR team.
        [FriendAccessAllowed]
        internal class AppXHelper
        {
            [SecuritySafeCritical]
            internal static Lazy<IntPtr> PrimaryWindowHandle = new Lazy<IntPtr>(() => GetPrimaryWindowHandle());

            [SecuritySafeCritical]
            [SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", 
                MessageId = "System.Net.UnsafeNclNativeMethods+AppXHelper.GetWindowThreadProcessId(System.IntPtr,System.Int32@)",
                Justification = "The return value of is the thread ID that created the window, not an error code.")]
            private static IntPtr GetPrimaryWindowHandle()
            {
                IntPtr primaryWindow = IntPtr.Zero;
                GuiThreadInfo info = new GuiThreadInfo();
                info.cbSize = Marshal.SizeOf(info);
                // Find the current active window.
                if (GetGUIThreadInfo(0, ref info) != 0 && info.hwndActive != IntPtr.Zero)
                {
                    int processId;
                    // Find the process for that window.
                    GetWindowThreadProcessId(info.hwndActive, out processId);
                    // Make sure the current active window belongs to our process.
                    if (processId == Process.GetCurrentProcess().Id)
                    {
                        primaryWindow = info.hwndActive;
                    }
                }
                return primaryWindow;
            }
            
            [DllImport(USER32, SetLastError=true, ExactSpelling=true)]
            private static extern uint GetGUIThreadInfo(int threadId, ref GuiThreadInfo info);

            [DllImport(USER32, ExactSpelling=true)]
            private static extern uint GetWindowThreadProcessId(IntPtr hwnd, out int processId);

            private struct GuiThreadInfo
            {
                public int cbSize; // Must be set to Marshal.SizeOf(GuiThreadInfo) before using.
                public int flags;
                public IntPtr hwndActive;
                public IntPtr hwndFocus;
                public IntPtr hwndCapture;
                public IntPtr hwndMenuOwner;
                public IntPtr hwndMoveSize;
                public IntPtr hwndCaret;
                // RECT
                public int left;
                public int top;
                public int right;
                public int bottom;
            }
        }
    }
}
