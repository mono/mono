//------------------------------------------------------------------------------
// <copyright file="NativeMethods.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.Win32 {
    using System;
    using System.Runtime.InteropServices;
#if !SILVERLIGHT    
    using System.Text;
    using System.Threading;
    using System.Globalization;
    using System.Runtime.Remoting;
    using System.Runtime.CompilerServices;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.Versioning;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.ComponentModel;
    using System.Security.Permissions;
    using Microsoft.Win32.SafeHandles;

    // not public!
    [HostProtection(MayLeakOnAbort = true)]
#endif

    internal static class NativeMethods {

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        internal class TEXTMETRIC {
            public int tmHeight = 0;
            public int tmAscent = 0;
            public int tmDescent = 0;
            public int tmInternalLeading = 0;
            public int tmExternalLeading = 0;
            public int tmAveCharWidth = 0;
            public int tmMaxCharWidth = 0;
            public int tmWeight = 0;
            public int tmOverhang = 0;
            public int tmDigitizedAspectX = 0;
            public int tmDigitizedAspectY = 0;
            public char tmFirstChar = '\0';
            public char tmLastChar = '\0';
            public char tmDefaultChar = '\0';
            public char tmBreakChar = '\0';
            public byte tmItalic = 0;
            public byte tmUnderlined = 0;
            public byte tmStruckOut = 0;
            public byte tmPitchAndFamily = 0;
            public byte tmCharSet = 0;
        }

        public const int DEFAULT_GUI_FONT = 17;
        public const int SM_CYSCREEN = 1;

#if !SILVERLIGHT
        public readonly static HandleRef NullHandleRef = new HandleRef(null, IntPtr.Zero);

        public static readonly IntPtr INVALID_HANDLE_VALUE = new IntPtr(-1);

        public const int GENERIC_READ = unchecked(((int)0x80000000));
        public const int GENERIC_WRITE = (0x40000000);

        public const int FILE_SHARE_READ = 0x00000001;
        public const int FILE_SHARE_WRITE = 0x00000002;
        public const int FILE_SHARE_DELETE = 0x00000004;

        public const int S_OK = 0x0;
        public const int E_ABORT = unchecked ((int)0x80004004);
        public const int E_NOTIMPL = unchecked((int)0x80004001);

        public const int CREATE_ALWAYS = 2;

        public const int FILE_ATTRIBUTE_NORMAL = 0x00000080;

        public const int STARTF_USESTDHANDLES = 0x00000100;

        public const int STD_INPUT_HANDLE = -10;
        public const int STD_OUTPUT_HANDLE = -11;
        public const int STD_ERROR_HANDLE = -12;

        public const int STILL_ACTIVE = 0x00000103;
        public const int SW_HIDE = 0;

        public const int WAIT_OBJECT_0    = 0x00000000;
        public const int WAIT_FAILED      = unchecked((int)0xFFFFFFFF);
        public const int WAIT_TIMEOUT     = 0x00000102;
        public const int WAIT_ABANDONED   = 0x00000080;
        public const int WAIT_ABANDONED_0 = WAIT_ABANDONED;

        // MoveFile Parameter
        public const int MOVEFILE_REPLACE_EXISTING = 0x00000001;

        // copied from winerror.h
        public const int ERROR_CLASS_ALREADY_EXISTS = 1410;
        public const int ERROR_NONE_MAPPED = 1332;
        public const int ERROR_INSUFFICIENT_BUFFER      = 122;
#endif // !SILVERLIGHT
        public const int ERROR_INVALID_NAME             = 0x7B; //123
#if !SILVERLIGHT
        public const int ERROR_PROC_NOT_FOUND           = 127;
        public const int ERROR_BAD_EXE_FORMAT           = 193;
        public const int ERROR_EXE_MACHINE_TYPE_MISMATCH= 216;
        public const int MAX_PATH                       = 260;


        [StructLayout(LayoutKind.Sequential)]
        internal class STARTUPINFO {
            public int cb;
            public IntPtr lpReserved = IntPtr.Zero;
            public IntPtr lpDesktop = IntPtr.Zero;
            public IntPtr lpTitle = IntPtr.Zero;
            public int dwX = 0;
            public int dwY = 0;
            public int dwXSize = 0;
            public int dwYSize = 0;
            public int dwXCountChars = 0;
            public int dwYCountChars = 0;
            public int dwFillAttribute = 0;
            public int dwFlags = 0;
            public short wShowWindow = 0;
            public short cbReserved2 = 0;
            public IntPtr lpReserved2 = IntPtr.Zero;
            public SafeFileHandle hStdInput = new SafeFileHandle(IntPtr.Zero, false);
            public SafeFileHandle hStdOutput = new SafeFileHandle(IntPtr.Zero, false);
            public SafeFileHandle hStdError = new SafeFileHandle(IntPtr.Zero, false);

            public STARTUPINFO() {
                cb = Marshal.SizeOf(this);
            } 
            
            public void Dispose() {                
                // close the handles created for child process
                if(hStdInput != null && !hStdInput.IsInvalid) {
                    hStdInput.Close();
                    hStdInput = null;
                }

                if(hStdOutput != null && !hStdOutput.IsInvalid) {
                    hStdOutput.Close();
                    hStdOutput = null;
                }

                if(hStdError != null && !hStdError.IsInvalid) {
                    hStdError.Close();
                    hStdError = null;
                }                
            }
        }
#endif // !SILVERLIGHT

        //
        // DACL related stuff
        //
        [StructLayout(LayoutKind.Sequential)]
        internal class SECURITY_ATTRIBUTES {
#if !SILVERLIGHT
            // We don't support ACL's on Silverlight nor on CoreSystem builds in our API's.  
            // But, we need P/Invokes to occasionally take these as parameters.  We can pass null.
            public int nLength = 12;
            public SafeLocalMemHandle lpSecurityDescriptor = new SafeLocalMemHandle(IntPtr.Zero, false);
            public bool bInheritHandle = false;
#endif // !SILVERLIGHT
        }

#if !SILVERLIGHT
        [DllImport(ExternDll.Kernel32, CharSet=System.Runtime.InteropServices.CharSet.Auto, SetLastError=true)]
        [ResourceExposure(ResourceScope.None)]
        public static extern bool GetExitCodeProcess(SafeProcessHandle processHandle, out int exitCode);

        [DllImport(ExternDll.Kernel32, CharSet=System.Runtime.InteropServices.CharSet.Auto, SetLastError=true)]
        [ResourceExposure(ResourceScope.None)]
        public static extern bool GetProcessTimes(SafeProcessHandle handle, out long creation, out long exit, out long kernel, out long user);

        [DllImport(ExternDll.Kernel32, CharSet=System.Runtime.InteropServices.CharSet.Auto, SetLastError=true)]
        [ResourceExposure(ResourceScope.None)]
        public static extern bool GetThreadTimes(SafeThreadHandle handle, out long creation, out long exit, out long kernel, out long user);

        [DllImport(ExternDll.Kernel32, CharSet=System.Runtime.InteropServices.CharSet.Ansi, SetLastError=true)]
        [ResourceExposure(ResourceScope.Process)]
        public static extern IntPtr GetStdHandle(int whichHandle);

        [DllImport(ExternDll.Kernel32, CharSet=System.Runtime.InteropServices.CharSet.Auto, SetLastError=true)]
        [ResourceExposure(ResourceScope.Process)]
        public static extern bool CreatePipe(out SafeFileHandle hReadPipe, out SafeFileHandle hWritePipe, SECURITY_ATTRIBUTES lpPipeAttributes, int nSize);

        [DllImport(ExternDll.Kernel32, CharSet=System.Runtime.InteropServices.CharSet.Auto, SetLastError=true, BestFitMapping=false)]
        [ResourceExposure(ResourceScope.Process)]
        public static extern bool CreateProcess(
            [MarshalAs(UnmanagedType.LPTStr)]
            string lpApplicationName,                   // LPCTSTR
            StringBuilder lpCommandLine,                // LPTSTR - note: CreateProcess might insert a null somewhere in this string
            SECURITY_ATTRIBUTES lpProcessAttributes,    // LPSECURITY_ATTRIBUTES
            SECURITY_ATTRIBUTES lpThreadAttributes,     // LPSECURITY_ATTRIBUTES
            bool bInheritHandles,                        // BOOL
            int dwCreationFlags,                        // DWORD
            IntPtr lpEnvironment,                       // LPVOID
            [MarshalAs(UnmanagedType.LPTStr)]           
            string lpCurrentDirectory,                  // LPCTSTR
            STARTUPINFO lpStartupInfo,                  // LPSTARTUPINFO
            SafeNativeMethods.PROCESS_INFORMATION lpProcessInformation    // LPPROCESS_INFORMATION
        );
           
        [DllImport(ExternDll.Kernel32, CharSet=System.Runtime.InteropServices.CharSet.Auto, SetLastError=true)]
        [ResourceExposure(ResourceScope.Machine)]
        public static extern bool TerminateProcess(SafeProcessHandle processHandle, int exitCode);


        [DllImport(ExternDll.Kernel32, CharSet=System.Runtime.InteropServices.CharSet.Auto)]
        [ResourceExposure(ResourceScope.Process)]
        public static extern int GetCurrentProcessId();
        [DllImport(ExternDll.Kernel32, CharSet=System.Runtime.InteropServices.CharSet.Ansi, SetLastError=true)]
        [ResourceExposure(ResourceScope.Process)]
        public static extern IntPtr GetCurrentProcess();

#if !FEATURE_PAL
        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        internal static string GetLocalPath(string fileName) {
            System.Diagnostics.Debug.Assert(fileName != null && fileName.Length > 0, "Cannot get local path, fileName is not valid");

            Uri uri = new Uri(fileName);
            return uri.LocalPath + uri.Fragment;
        }

        [DllImport(ExternDll.Advapi32, CharSet=System.Runtime.InteropServices.CharSet.Auto, SetLastError=true, BestFitMapping=false)]
        [System.Security.SuppressUnmanagedCodeSecurityAttribute()]
        [ResourceExposure(ResourceScope.Machine)]
        public extern static bool CreateProcessAsUser(SafeHandle hToken,
            string lpApplicationName,
            string lpCommandLine,
            SECURITY_ATTRIBUTES lpProcessAttributes,
            SECURITY_ATTRIBUTES lpThreadAttributes,
            bool bInheritHandles,
            int dwCreationFlags,
            HandleRef lpEnvironment,
            string lpCurrentDirectory,
            STARTUPINFO lpStartupInfo,
            SafeNativeMethods.PROCESS_INFORMATION lpProcessInformation
        );

        [DllImport(ExternDll.Advapi32, CharSet=CharSet.Unicode, ExactSpelling=true, SetLastError=true, BestFitMapping=false)]
        [ResourceExposure(ResourceScope.Machine)]
        internal static extern bool CreateProcessWithLogonW(
            string userName,
            string domain,
            IntPtr password,
            LogonFlags logonFlags,
            [MarshalAs(UnmanagedType.LPTStr)]            
            string appName,
            StringBuilder cmdLine,
            int creationFlags,
            IntPtr environmentBlock,
            [MarshalAs(UnmanagedType.LPTStr)]           
            string lpCurrentDirectory,                  // LPCTSTR            
            STARTUPINFO lpStartupInfo,
            SafeNativeMethods.PROCESS_INFORMATION lpProcessInformation);        

        [Flags]
        internal enum LogonFlags {
            LOGON_WITH_PROFILE         = 0x00000001,
            LOGON_NETCREDENTIALS_ONLY  = 0x00000002        
        }
        
        public const int UIS_SET = 1,
        WSF_VISIBLE = 0x0001,
        UIS_CLEAR = 2,
        UISF_HIDEFOCUS = 0x1,
        UISF_HIDEACCEL = 0x2,
        USERCLASSTYPE_FULL = 1,
        UOI_FLAGS = 1;

        public const int COLOR_WINDOW = 5;
        public const int WS_POPUP = unchecked((int)0x80000000);
        public const int WS_VISIBLE = 0x10000000;
        public const int WM_SETTINGCHANGE = 0x001A;
        public const int WM_SYSCOLORCHANGE = 0x0015;
        public const int WM_QUERYENDSESSION = 0x0011;
        public const int WM_QUIT = 0x0012;
        public const int WM_ENDSESSION = 0x0016;
        public const int WM_POWERBROADCAST = 0x0218;
        public const int WM_COMPACTING = 0x0041;
        public const int WM_DISPLAYCHANGE = 0x007E;
        public const int WM_FONTCHANGE = 0x001D;
        public const int WM_PALETTECHANGED = 0x0311;
        public const int WM_TIMECHANGE = 0x001E;
        public const int WM_THEMECHANGED = 0x031A;
        public const int WM_WTSSESSION_CHANGE = 0x02B1;

        public const int ENDSESSION_LOGOFF = unchecked((int)0x80000000);
        public const int WM_TIMER = 0x0113;
        public const int WM_USER = 0x0400;
        public const int WM_CREATETIMER = WM_USER + 1;
        public const int WM_KILLTIMER = WM_USER + 2;
        public const int WM_REFLECT = WM_USER + 0x1C00;

        public const int WTS_CONSOLE_CONNECT              =  0x1;
        public const int WTS_CONSOLE_DISCONNECT           =  0x2;
        public const int WTS_REMOTE_CONNECT               =  0x3;
        public const int WTS_REMOTE_DISCONNECT            =  0x4;
        public const int WTS_SESSION_LOGON                =  0x5;
        public const int WTS_SESSION_LOGOFF               =  0x6;
        public const int WTS_SESSION_LOCK                 =  0x7;
        public const int WTS_SESSION_UNLOCK               =  0x8;
        public const int WTS_SESSION_REMOTE_CONTROL       =  0x9;

        public const int NOTIFY_FOR_THIS_SESSION          =  0x0;

        public const int CTRL_C_EVENT        = 0;
        public const int CTRL_BREAK_EVENT    = 1;
        public const int CTRL_CLOSE_EVENT    = 2;
        public const int CTRL_LOGOFF_EVENT   = 5;
        public const int CTRL_SHUTDOWN_EVENT = 6;

        public const int SPI_GETBEEP                          =   1;
        public const int SPI_SETBEEP                          =   2;
        public const int SPI_GETMOUSE                         =   3;
        public const int SPI_SETMOUSE                         =   4;
        public const int SPI_GETBORDER                        =   5;
        public const int SPI_SETBORDER                        =   6;
        public const int SPI_GETKEYBOARDSPEED                 =  10;
        public const int SPI_SETKEYBOARDSPEED                 =  11;
        public const int SPI_LANGDRIVER                       =  12;
        public const int SPI_ICONHORIZONTALSPACING            =  13;
        public const int SPI_GETSCREENSAVETIMEOUT             =  14;
        public const int SPI_SETSCREENSAVETIMEOUT             =  15;
        public const int SPI_GETSCREENSAVEACTIVE              =  16;
        public const int SPI_SETSCREENSAVEACTIVE              =  17;
        public const int SPI_GETGRIDGRANULARITY               =  18;
        public const int SPI_SETGRIDGRANULARITY               =  19;
        public const int SPI_SETDESKWALLPAPER                 =  20;
        public const int SPI_SETDESKPATTERN                   =  21;
        public const int SPI_GETKEYBOARDDELAY                 =  22;
        public const int SPI_SETKEYBOARDDELAY                 =  23;
        public const int SPI_ICONVERTICALSPACING              =  24;
        public const int SPI_GETICONTITLEWRAP                 =  25;
        public const int SPI_SETICONTITLEWRAP                 =  26;
        public const int SPI_GETMENUDROPALIGNMENT             =  27;
        public const int SPI_SETMENUDROPALIGNMENT             =  28;
        public const int SPI_SETDOUBLECLKWIDTH                =  29;
        public const int SPI_SETDOUBLECLKHEIGHT               =  30;
        public const int SPI_GETICONTITLELOGFONT              =  31;
        public const int SPI_SETDOUBLECLICKTIME               =  32;
        public const int SPI_SETMOUSEBUTTONSWAP               =  33;
        public const int SPI_SETICONTITLELOGFONT              =  34;
        public const int SPI_GETFASTTASKSWITCH                =  35;
        public const int SPI_SETFASTTASKSWITCH                =  36;
        public const int SPI_SETDRAGFULLWINDOWS               =  37;
        public const int SPI_GETDRAGFULLWINDOWS               =  38;
        public const int SPI_GETNONCLIENTMETRICS              =  41;
        public const int SPI_SETNONCLIENTMETRICS              =  42;
        public const int SPI_GETMINIMIZEDMETRICS              =  43;
        public const int SPI_SETMINIMIZEDMETRICS              =  44;
        public const int SPI_GETICONMETRICS                   =  45;
        public const int SPI_SETICONMETRICS                   =  46;
        public const int SPI_SETWORKAREA                      =  47;
        public const int SPI_GETWORKAREA                      =  48;
        public const int SPI_SETPENWINDOWS                    =  49;
        public const int SPI_GETHIGHCONTRAST                  =  66;
        public const int SPI_SETHIGHCONTRAST                  =  67;
        public const int SPI_GETKEYBOARDPREF                  =  68;
        public const int SPI_SETKEYBOARDPREF                  =  69;
        public const int SPI_GETSCREENREADER                  =  70;
        public const int SPI_SETSCREENREADER                  =  71;
        public const int SPI_GETANIMATION                     =  72;
        public const int SPI_SETANIMATION                     =  73;
        public const int SPI_GETFONTSMOOTHING                 =  74;
        public const int SPI_SETFONTSMOOTHING                 =  75;
        public const int SPI_SETDRAGWIDTH                     =  76;
        public const int SPI_SETDRAGHEIGHT                    =  77;
        public const int SPI_SETHANDHELD                      =  78;
        public const int SPI_GETLOWPOWERTIMEOUT               =  79;
        public const int SPI_GETPOWEROFFTIMEOUT               =  80;
        public const int SPI_SETLOWPOWERTIMEOUT               =  81;
        public const int SPI_SETPOWEROFFTIMEOUT               =  82;
        public const int SPI_GETLOWPOWERACTIVE                =  83;
        public const int SPI_GETPOWEROFFACTIVE                =  84;
        public const int SPI_SETLOWPOWERACTIVE                =  85;
        public const int SPI_SETPOWEROFFACTIVE                =  86;
        public const int SPI_SETCURSORS                       =  87;
        public const int SPI_SETICONS                         =  88;
        public const int SPI_GETDEFAULTINPUTLANG              =  89;
        public const int SPI_SETDEFAULTINPUTLANG              =  90;
        public const int SPI_SETLANGTOGGLE                    =  91;
        public const int SPI_GETWINDOWSEXTENSION              =  92;
        public const int SPI_SETMOUSETRAILS                   =  93;
        public const int SPI_GETMOUSETRAILS                   =  94;
        public const int SPI_SETSCREENSAVERRUNNING            =  97;
        public const int SPI_SCREENSAVERRUNNING               =  SPI_SETSCREENSAVERRUNNING;
        public const int SPI_GETFILTERKEYS                    =  50;
        public const int SPI_SETFILTERKEYS                    =  51;
        public const int SPI_GETTOGGLEKEYS                    =  52;
        public const int SPI_SETTOGGLEKEYS                    =  53;
        public const int SPI_GETMOUSEKEYS                     =  54;
        public const int SPI_SETMOUSEKEYS                     =  55;
        public const int SPI_GETSHOWSOUNDS                    =  56;
        public const int SPI_SETSHOWSOUNDS                    =  57;
        public const int SPI_GETSTICKYKEYS                    =  58;
        public const int SPI_SETSTICKYKEYS                    =  59;
        public const int SPI_GETACCESSTIMEOUT                 =  60;
        public const int SPI_SETACCESSTIMEOUT                 =  61;
        public const int SPI_GETSERIALKEYS                    =  62;
        public const int SPI_SETSERIALKEYS                    =  63;
        public const int SPI_GETSOUNDSENTRY                   =  64;
        public const int SPI_SETSOUNDSENTRY                   =  65;
        public const int SPI_GETSNAPTODEFBUTTON               =  95;
        public const int SPI_SETSNAPTODEFBUTTON               =  96;
        public const int SPI_GETMOUSEHOVERWIDTH               =  98;
        public const int SPI_SETMOUSEHOVERWIDTH               =  99;
        public const int SPI_GETMOUSEHOVERHEIGHT              = 100;
        public const int SPI_SETMOUSEHOVERHEIGHT              = 101;
        public const int SPI_GETMOUSEHOVERTIME                = 102;
        public const int SPI_SETMOUSEHOVERTIME                = 103;
        public const int SPI_GETWHEELSCROLLLINES              = 104;
        public const int SPI_SETWHEELSCROLLLINES              = 105;
        public const int SPI_GETMENUSHOWDELAY                 = 106;
        public const int SPI_SETMENUSHOWDELAY                 = 107;
        public const int SPI_GETSHOWIMEUI                     = 110;
        public const int SPI_SETSHOWIMEUI                     = 111;
        public const int SPI_GETMOUSESPEED                    = 112;
        public const int SPI_SETMOUSESPEED                    = 113;
        public const int SPI_GETSCREENSAVERRUNNING            = 114;
        public const int SPI_GETDESKWALLPAPER                 = 115;
        public const int SPI_GETACTIVEWINDOWTRACKING          = 0x1000;
        public const int SPI_SETACTIVEWINDOWTRACKING          = 0x1001;
        public const int SPI_GETMENUANIMATION                 = 0x1002;
        public const int SPI_SETMENUANIMATION                 = 0x1003;
        public const int SPI_GETCOMBOBOXANIMATION             = 0x1004;
        public const int SPI_SETCOMBOBOXANIMATION             = 0x1005;
        public const int SPI_GETLISTBOXSMOOTHSCROLLING        = 0x1006;
        public const int SPI_SETLISTBOXSMOOTHSCROLLING        = 0x1007;
        public const int SPI_GETGRADIENTCAPTIONS              = 0x1008;
        public const int SPI_SETGRADIENTCAPTIONS              = 0x1009;
        public const int SPI_GETKEYBOARDCUES                  = 0x100A;
        public const int SPI_SETKEYBOARDCUES                  = 0x100B;
        public const int SPI_GETMENUUNDERLINES                = SPI_GETKEYBOARDCUES;
        public const int SPI_SETMENUUNDERLINES                = SPI_SETKEYBOARDCUES;
        public const int SPI_GETACTIVEWNDTRKZORDER            = 0x100C;
        public const int SPI_SETACTIVEWNDTRKZORDER            = 0x100D;
        public const int SPI_GETHOTTRACKING                   = 0x100E;
        public const int SPI_SETHOTTRACKING                   = 0x100F;
        public const int SPI_GETMENUFADE                      = 0x1012;
        public const int SPI_SETMENUFADE                      = 0x1013;
        public const int SPI_GETSELECTIONFADE                 = 0x1014;
        public const int SPI_SETSELECTIONFADE                 = 0x1015;
        public const int SPI_GETTOOLTIPANIMATION              = 0x1016;
        public const int SPI_SETTOOLTIPANIMATION              = 0x1017;
        public const int SPI_GETTOOLTIPFADE                   = 0x1018;
        public const int SPI_SETTOOLTIPFADE                   = 0x1019;
        public const int SPI_GETCURSORSHADOW                  = 0x101A;
        public const int SPI_SETCURSORSHADOW                  = 0x101B;
        public const int SPI_GETUIEFFECTS                     = 0x103E;
        public const int SPI_SETUIEFFECTS                     = 0x103F;
        public const int SPI_GETFOREGROUNDLOCKTIMEOUT         = 0x2000;
        public const int SPI_SETFOREGROUNDLOCKTIMEOUT         = 0x2001;
        public const int SPI_GETACTIVEWNDTRKTIMEOUT           = 0x2002;
        public const int SPI_SETACTIVEWNDTRKTIMEOUT           = 0x2003;
        public const int SPI_GETFOREGROUNDFLASHCOUNT          = 0x2004;
        public const int SPI_SETFOREGROUNDFLASHCOUNT          = 0x2005;
        public const int SPI_GETCARETWIDTH                    = 0x2006;
        public const int SPI_SETCARETWIDTH                    = 0x2007;

        public const uint STATUS_INFO_LENGTH_MISMATCH   =  0xC0000004;

        public const int PBT_APMQUERYSUSPEND           = 0x0000;
        public const int PBT_APMQUERYSTANDBY           = 0x0001;
        public const int PBT_APMQUERYSUSPENDFAILED     = 0x0002;
        public const int PBT_APMQUERYSTANDBYFAILED     = 0x0003;
        public const int PBT_APMSUSPEND                = 0x0004;
        public const int PBT_APMSTANDBY                = 0x0005;
        public const int PBT_APMRESUMECRITICAL         = 0x0006;
        public const int PBT_APMRESUMESUSPEND          = 0x0007;
        public const int PBT_APMRESUMESTANDBY          = 0x0008;
        public const int PBT_APMBATTERYLOW             = 0x0009;
        public const int PBT_APMPOWERSTATUSCHANGE      = 0x000A;
        public const int PBT_APMOEMEVENT               = 0x000B;

        public const int STARTF_USESHOWWINDOW = 0x00000001;
        public const int FILE_MAP_WRITE = 0x00000002;
        public const int FILE_MAP_READ = 0x00000004;
        public const int PAGE_READWRITE = 0x00000004;
        public const int GENERIC_EXECUTE = (0x20000000);
        public const int GENERIC_ALL = (0x10000000);
        public const int ERROR_NOT_READY  = 21;
        public const int ERROR_LOCK_FAILED = 167;
        public const int ERROR_BUSY        = 170;

        public const int IMPERSONATION_LEVEL_SecurityAnonymous = 0;
        public const int IMPERSONATION_LEVEL_SecurityIdentification = 1;
        public const int IMPERSONATION_LEVEL_SecurityImpersonation = 2;
        public const int IMPERSONATION_LEVEL_SecurityDelegation = 3;

        public const int TOKEN_TYPE_TokenPrimary = 1;
        public const int TOKEN_TYPE_TokenImpersonation = 2;

        public const int TOKEN_ALL_ACCESS   = 0x000f01ff;
        public const int TOKEN_EXECUTE      = 0x00020000;
        public const int TOKEN_READ         = 0x00020008;
        public const int TOKEN_IMPERSONATE  = 0x00000004;

        public const int PIPE_ACCESS_INBOUND = 0x00000001;
        public const int PIPE_ACCESS_OUTBOUND = 0x00000002;
        public const int PIPE_ACCESS_DUPLEX = 0x00000003;

        public const int PIPE_WAIT = 0x00000000;
        public const int PIPE_NOWAIT = 0x00000001;
        public const int PIPE_READMODE_BYTE = 0x00000000;
        public const int PIPE_READMODE_MESSAGE = 0x00000002;
        public const int PIPE_TYPE_BYTE = 0x00000000;
        public const int PIPE_TYPE_MESSAGE = 0x00000004;

        public const int PIPE_SINGLE_INSTANCES = 1;
        public const int PIPE_UNLIMITED_INSTANCES = 255;

        public const int FILE_FLAG_OVERLAPPED = 0x40000000;

        public const int PM_REMOVE = 0x0001;
        
        public const int QS_KEY = 0x0001,
        QS_MOUSEMOVE = 0x0002,
        QS_MOUSEBUTTON = 0x0004,
        QS_POSTMESSAGE = 0x0008,
        QS_TIMER = 0x0010,
        QS_PAINT = 0x0020,
        QS_SENDMESSAGE = 0x0040,
        QS_HOTKEY = 0x0080,
        QS_ALLPOSTMESSAGE = 0x0100,
        QS_MOUSE = QS_MOUSEMOVE | QS_MOUSEBUTTON,
        QS_INPUT = QS_MOUSE | QS_KEY,
        QS_ALLEVENTS = QS_INPUT | QS_POSTMESSAGE | QS_TIMER | QS_PAINT | QS_HOTKEY,
        QS_ALLINPUT = QS_INPUT | QS_POSTMESSAGE | QS_TIMER | QS_PAINT | QS_HOTKEY | QS_SENDMESSAGE;
 
        public const int MWMO_INPUTAVAILABLE = 0x0004;  // don't use MWMO_WAITALL, see ddb#176342

        // The following are unique to the SerialPort/SerialStream classes
        internal const byte ONESTOPBIT = 0;
        internal const byte ONE5STOPBITS = 1;
        internal const byte TWOSTOPBITS = 2;

        internal const int DTR_CONTROL_DISABLE = 0x00;
        internal const int DTR_CONTROL_ENABLE = 0x01;
        internal const int DTR_CONTROL_HANDSHAKE = 0x02;

        internal const int RTS_CONTROL_DISABLE = 0x00;
        internal const int RTS_CONTROL_ENABLE = 0x01;
        internal const int RTS_CONTROL_HANDSHAKE = 0x02;
        internal const int RTS_CONTROL_TOGGLE = 0x03;

        internal const int  MS_CTS_ON = 0x10;
        internal const int  MS_DSR_ON = 0x20;
        internal const int  MS_RING_ON = 0x40;
        internal const int  MS_RLSD_ON  = 0x80;

        internal const byte EOFCHAR = (byte) 26;

        // Since C# does not provide access to bitfields and the native DCB structure contains
        // a very necessary one, these are the positional offsets (from the right) of areas
        // of the 32-bit integer used in SerialStream's SetDcbFlag() and GetDcbFlag() methods.
        internal const int FBINARY = 0;
        internal const int FPARITY = 1;
        internal const int FOUTXCTSFLOW = 2;
        internal const int FOUTXDSRFLOW = 3;
        internal const int FDTRCONTROL = 4;
        internal const int FDSRSENSITIVITY = 6;
        internal const int FTXCONTINUEONXOFF = 7;
        internal const int FOUTX = 8;
        internal const int FINX = 9;
        internal const int FERRORCHAR = 10;
        internal const int FNULL = 11;
        internal const int FRTSCONTROL = 12;
        internal const int FABORTONOERROR = 14;
        internal const int FDUMMY2 = 15;

        internal const int PURGE_TXABORT     =  0x0001;  // Kill the pending/current writes to the comm port.
        internal const int PURGE_RXABORT     =  0x0002;  // Kill the pending/current reads to the comm port.
        internal const int PURGE_TXCLEAR     =  0x0004;  // Kill the transmit queue if there.
        internal const int PURGE_RXCLEAR     =  0x0008;  // Kill the typeahead buffer if there.

        internal const byte DEFAULTXONCHAR = (byte) 17;
        internal const byte DEFAULTXOFFCHAR = (byte) 19;

        internal const int SETRTS = 3;       // Set RTS high
        internal const int CLRRTS = 4;       // Set RTS low
        internal const int SETDTR = 5;       // Set DTR high
        internal const int CLRDTR = 6;

        internal const int EV_RXCHAR = 0x01;
        internal const int EV_RXFLAG = 0x02;
        internal const int EV_CTS = 0x08;
        internal const int EV_DSR = 0x10;
        internal const int EV_RLSD = 0x20;
        internal const int EV_BREAK = 0x40;
        internal const int EV_ERR = 0x80;
        internal const int EV_RING = 0x100;
        internal const int ALL_EVENTS = 0x1fb;  // don't use EV_TXEMPTY

        internal const int CE_RXOVER = 0x01;
        internal const int CE_OVERRUN = 0x02;
        internal const int CE_PARITY = 0x04;
        internal const int CE_FRAME = 0x08;
        internal const int CE_BREAK = 0x10;
        internal const int CE_TXFULL = 0x100;

        internal const int MAXDWORD = -1;   // note this is 0xfffffff, or UInt32.MaxValue, here used as an int

        internal const int NOPARITY          = 0;
        internal const int ODDPARITY         = 1;
        internal const int EVENPARITY        = 2;
        internal const int MARKPARITY        = 3;
        internal const int SPACEPARITY       = 4;

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto)]
        internal class WNDCLASS_I {
            public int      style;
            public IntPtr   lpfnWndProc;
            public int      cbClsExtra = 0;
            public int      cbWndExtra = 0;
            public IntPtr   hInstance = IntPtr.Zero;
            public IntPtr   hIcon = IntPtr.Zero;
            public IntPtr   hCursor = IntPtr.Zero;
            public IntPtr   hbrBackground = IntPtr.Zero;
            public IntPtr   lpszMenuName = IntPtr.Zero;
            public IntPtr   lpszClassName = IntPtr.Zero;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto)]
        internal class WNDCLASS {
            public int      style;
            public WndProc  lpfnWndProc;
            public int      cbClsExtra = 0;
            public int      cbWndExtra = 0;
            public IntPtr   hInstance = IntPtr.Zero;
            public IntPtr   hIcon = IntPtr.Zero;
            public IntPtr   hCursor = IntPtr.Zero;
            public IntPtr   hbrBackground = IntPtr.Zero;
            public string   lpszMenuName = null;
            public string   lpszClassName = null;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MSG {
            public IntPtr   hwnd;
            public int      message;
            public IntPtr   wParam;
            public IntPtr   lParam;
            public int      time;
            public int      pt_x;
            public int      pt_y;
        }

        public enum StructFormatEnum {
            Ansi = 1,
            Unicode = 2,
            Auto = 3,
        }

        internal const int SDDL_REVISION_1 = 1;

        public enum StructFormat {
            Ansi = 1,
            Unicode = 2,
            Auto = 3,
        }

        public delegate IntPtr WndProc(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

        public delegate int ConHndlr(int signalType);

        // file src\services\monitoring\system\diagnosticts\nativemethods.cs
        public const int SECURITY_DESCRIPTOR_REVISION = 1;
        public const int HKEY_PERFORMANCE_DATA = unchecked((int)0x80000004);
        public const int DWORD_SIZE = 4;
        public const int LARGE_INTEGER_SIZE = 8;

        public const int PERF_NO_INSTANCES     =      -1;  // no instances (see NumInstances above)

        public const int PERF_SIZE_DWORD        = 0x00000000;
        public const int PERF_SIZE_LARGE        = 0x00000100;
        public const int PERF_SIZE_ZERO         = 0x00000200;  // for Zero Length fields
        public const int PERF_SIZE_VARIABLE_LEN = 0x00000300;  // length is In CounterLength field

        public const int PERF_NO_UNIQUE_ID = -1;

        //
        //  select one of the following values to indicate the counter field usage
        //
        public const int PERF_TYPE_NUMBER       = 0x00000000;  // a number (not a counter)
        public const int PERF_TYPE_COUNTER      = 0x00000400;  // an increasing numeric value
        public const int PERF_TYPE_TEXT         = 0x00000800;  // a text field
        public const int PERF_TYPE_ZERO         = 0x00000C00;  // displays a zero

        //
        //  If the PERF_TYPE_NUMBER field was selected, then select one of the
        //  following to describe the Number
        //
        public const int PERF_NUMBER_HEX        = 0x00000000;  // display as HEX value
        public const int PERF_NUMBER_DECIMAL    = 0x00010000;  // display as a decimal integer
        public const int PERF_NUMBER_DEC_1000   = 0x00020000;  // display as a decimal/1000

        //
        //  If the PERF_TYPE_COUNTER value was selected then select one of the
        //  following to indicate the type of counter
        //
        public const int PERF_COUNTER_VALUE     = 0x00000000;  // display counter value
        public const int PERF_COUNTER_RATE      = 0x00010000;  // divide ctr / delta time
        public const int PERF_COUNTER_FRACTION  = 0x00020000;  // divide ctr / base
        public const int PERF_COUNTER_BASE      = 0x00030000;  // base value used In fractions
        public const int PERF_COUNTER_ELAPSED   = 0x00040000;  // subtract counter from current time
        public const int PERF_COUNTER_QUEUELEN  = 0x00050000;  // Use Queuelen processing func.
        public const int PERF_COUNTER_HISTOGRAM = 0x00060000;  // Counter begins or ends a histogram
        public const int PERF_COUNTER_PRECISION = 0x00070000;  // divide ctr / private clock

        //
        //  If the PERF_TYPE_TEXT value was selected, then select one of the
        //  following to indicate the type of TEXT data.
        //
        public const int PERF_TEXT_UNICODE      = 0x00000000;  // type of text In text field
        public const int PERF_TEXT_ASCII        = 0x00010000;  // ASCII using the CodePage field

        //
        //  Timer SubTypes
        //
        public const int PERF_TIMER_TICK        = 0x00000000;  // use system perf. freq for base
        public const int PERF_TIMER_100NS       = 0x00100000;  // use 100 NS timer time base units
        public const int PERF_OBJECT_TIMER      = 0x00200000;  // use the object timer freq

        //
        //  Any types that have calculations performed can use one or more of
        //  the following calculation modification flags listed here
        //
        public const int PERF_DELTA_COUNTER     = 0x00400000;  // compute difference first
        public const int PERF_DELTA_BASE        = 0x00800000;  // compute base diff as well
        public const int PERF_INVERSE_COUNTER   = 0x01000000;  // show as 1.00-value (assumes:
        public const int PERF_MULTI_COUNTER     = 0x02000000;  // sum of multiple instances

        //
        //  Select one of the following values to indicate the display suffix (if any)
        //
        public const int PERF_DISPLAY_NO_SUFFIX = 0x00000000;  // no suffix
        public const int PERF_DISPLAY_PER_SEC   = 0x10000000;  // "/sec"
        public const int PERF_DISPLAY_PERCENT   = 0x20000000;  // "%"
        public const int PERF_DISPLAY_SECONDS   = 0x30000000;  // "secs"
        public const int PERF_DISPLAY_NOSHOW    = 0x40000000;  // value is not displayed

        //
        //  Predefined counter types
        //

        // 32-bit Counter.  Divide delta by delta time.  Display suffix: "/sec"
        public const int PERF_COUNTER_COUNTER  =
                (PERF_SIZE_DWORD | PERF_TYPE_COUNTER | PERF_COUNTER_RATE |
                 PERF_TIMER_TICK | PERF_DELTA_COUNTER | PERF_DISPLAY_PER_SEC);


        // 64-bit Timer.  Divide delta by delta time.  Display suffix: "%"
        public const int PERF_COUNTER_TIMER =
                (PERF_SIZE_LARGE | PERF_TYPE_COUNTER | PERF_COUNTER_RATE |
                PERF_TIMER_TICK | PERF_DELTA_COUNTER | PERF_DISPLAY_PERCENT);

        // Queue Length Space-Time Product. Divide delta by delta time. No Display Suffix.
        public const int PERF_COUNTER_QUEUELEN_TYPE =
                (PERF_SIZE_DWORD | PERF_TYPE_COUNTER | PERF_COUNTER_QUEUELEN |
                PERF_TIMER_TICK | PERF_DELTA_COUNTER | PERF_DISPLAY_NO_SUFFIX);

        // Queue Length Space-Time Product. Divide delta by delta time. No Display Suffix.
        public const int PERF_COUNTER_LARGE_QUEUELEN_TYPE =
                (PERF_SIZE_LARGE | PERF_TYPE_COUNTER | PERF_COUNTER_QUEUELEN |
                PERF_TIMER_TICK | PERF_DELTA_COUNTER | PERF_DISPLAY_NO_SUFFIX);

        // Queue Length Space-Time Product using 100 Ns timebase.
        // Divide delta by delta time. No Display Suffix.
        public const int PERF_COUNTER_100NS_QUEUELEN_TYPE =
                    (PERF_SIZE_LARGE | PERF_TYPE_COUNTER | PERF_COUNTER_QUEUELEN |
                    PERF_TIMER_100NS | PERF_DELTA_COUNTER | PERF_DISPLAY_NO_SUFFIX);

        // Queue Length Space-Time Product using Object specific timebase.
        // Divide delta by delta time. No Display Suffix.
        public const int PERF_COUNTER_OBJ_TIME_QUEUELEN_TYPE =
                    (PERF_SIZE_LARGE | PERF_TYPE_COUNTER | PERF_COUNTER_QUEUELEN |
                    PERF_OBJECT_TIMER | PERF_DELTA_COUNTER | PERF_DISPLAY_NO_SUFFIX);

        // 64-bit Counter.  Divide delta by delta time. Display Suffix: "/sec"
        public const int PERF_COUNTER_BULK_COUNT =
                (PERF_SIZE_LARGE | PERF_TYPE_COUNTER | PERF_COUNTER_RATE |
                PERF_TIMER_TICK | PERF_DELTA_COUNTER | PERF_DISPLAY_PER_SEC);

        // Indicates the counter is not a  counter but rather Unicode text Display as text.
        public const int PERF_COUNTER_TEXT =
                (PERF_SIZE_VARIABLE_LEN | PERF_TYPE_TEXT | PERF_TEXT_UNICODE |
                PERF_DISPLAY_NO_SUFFIX);

        // Indicates the data is a counter  which should not be
        // time averaged on display (such as an error counter on a serial line)
        // Display as is.  No Display Suffix.
        public const int PERF_COUNTER_RAWCOUNT =
                (PERF_SIZE_DWORD | PERF_TYPE_NUMBER | PERF_NUMBER_DECIMAL |
                PERF_DISPLAY_NO_SUFFIX);

        // Same as PERF_COUNTER_RAWCOUNT except its size is a large integer
        public const int PERF_COUNTER_LARGE_RAWCOUNT =
                (PERF_SIZE_LARGE | PERF_TYPE_NUMBER | PERF_NUMBER_DECIMAL |
                PERF_DISPLAY_NO_SUFFIX);

        // Special case for RAWCOUNT that want to be displayed In hex
        // Indicates the data is a counter  which should not be
        // time averaged on display (such as an error counter on a serial line)
        // Display as is.  No Display Suffix.
        public const int PERF_COUNTER_RAWCOUNT_HEX =
                (PERF_SIZE_DWORD | PERF_TYPE_NUMBER | PERF_NUMBER_HEX |
                PERF_DISPLAY_NO_SUFFIX);

        // Same as PERF_COUNTER_RAWCOUNT_HEX except its size is a large integer
        public const int PERF_COUNTER_LARGE_RAWCOUNT_HEX =
                (PERF_SIZE_LARGE | PERF_TYPE_NUMBER | PERF_NUMBER_HEX |
                PERF_DISPLAY_NO_SUFFIX);

        // A count which is either 1 or 0 on each sampling interrupt (% busy)
        // Divide delta by delta base. Display Suffix: "%"
        public const int PERF_SAMPLE_FRACTION =
                (PERF_SIZE_DWORD | PERF_TYPE_COUNTER | PERF_COUNTER_FRACTION |
                PERF_DELTA_COUNTER | PERF_DELTA_BASE | PERF_DISPLAY_PERCENT);

        // A count which is sampled on each sampling interrupt (queue length)
        // Divide delta by delta time. No Display Suffix.
        public const int PERF_SAMPLE_COUNTER =
                (PERF_SIZE_DWORD | PERF_TYPE_COUNTER | PERF_COUNTER_RATE |
                PERF_TIMER_TICK | PERF_DELTA_COUNTER | PERF_DISPLAY_NO_SUFFIX);

        // A label: no data is associated with this counter (it has 0 length)
        // Do not display.
        public const int PERF_COUNTER_NODATA =
                (PERF_SIZE_ZERO | PERF_DISPLAY_NOSHOW);

        // 64-bit Timer inverse (e.g., idle is measured, but display busy %)
        // Display 100 - delta divided by delta time.  Display suffix: "%"
        public const int PERF_COUNTER_TIMER_INV =
                (PERF_SIZE_LARGE | PERF_TYPE_COUNTER | PERF_COUNTER_RATE |
                PERF_TIMER_TICK | PERF_DELTA_COUNTER | PERF_INVERSE_COUNTER |
                PERF_DISPLAY_PERCENT);

        // The divisor for a sample, used with the previous counter to form a
        // sampled %.  You must check for >0 before dividing by this!  This
        // counter will directly follow the  numerator counter.  It should not
        // be displayed to the user.
        public const int PERF_SAMPLE_BASE =
                (PERF_SIZE_DWORD | PERF_TYPE_COUNTER | PERF_COUNTER_BASE |
                PERF_DISPLAY_NOSHOW |
                0x00000001);  // for compatibility with pre-beta versions

        // A timer which, when divided by an average base, produces a time
        // In seconds which is the average time of some operation.  This
        // timer times total operations, and  the base is the number of opera-
        // tions.  Display Suffix: "sec"
        public const int PERF_AVERAGE_TIMER =
                (PERF_SIZE_DWORD | PERF_TYPE_COUNTER | PERF_COUNTER_FRACTION |
                PERF_DISPLAY_SECONDS);

        // Used as the denominator In the computation of time or count
        // averages.  Must directly follow the numerator counter.  Not dis-
        // played to the user.
        public const int PERF_AVERAGE_BASE =
                (PERF_SIZE_DWORD | PERF_TYPE_COUNTER | PERF_COUNTER_BASE |
                PERF_DISPLAY_NOSHOW |
                0x00000002);  // for compatibility with pre-beta versions


        // 64-bit Timer in object specific units. Display delta divided by
        // delta time as returned in the object type header structure.  Display suffix: "%"
        public const int PERF_OBJ_TIME_TIMER =
                    (PERF_SIZE_LARGE   | PERF_TYPE_COUNTER  | PERF_COUNTER_RATE |
                     PERF_OBJECT_TIMER | PERF_DELTA_COUNTER | PERF_DISPLAY_PERCENT);

        // A bulk count which, when divided (typically) by the number of
        // operations, gives (typically) the number of bytes per operation.
        // No Display Suffix.
        public const int PERF_AVERAGE_BULK =
                (PERF_SIZE_LARGE | PERF_TYPE_COUNTER | PERF_COUNTER_FRACTION  |
                PERF_DISPLAY_NOSHOW);

        // 64-bit Timer in object specific units. Display delta divided by
        // delta time as returned in the object type header structure.  Display suffix: "%"
        public const int PERF_OBJ_TIME_TIME =
                    (PERF_SIZE_LARGE   | PERF_TYPE_COUNTER  | PERF_COUNTER_RATE |
                     PERF_OBJECT_TIMER | PERF_DELTA_COUNTER | PERF_DISPLAY_PERCENT);

        // 64-bit Timer In 100 nsec units. Display delta divided by
        // delta time.  Display suffix: "%"
        public const int PERF_100NSEC_TIMER =
                (PERF_SIZE_LARGE | PERF_TYPE_COUNTER | PERF_COUNTER_RATE |
                PERF_TIMER_100NS | PERF_DELTA_COUNTER | PERF_DISPLAY_PERCENT);

        // 64-bit Timer inverse (e.g., idle is measured, but display busy %)
        // Display 100 - delta divided by delta time.  Display suffix: "%"
        public const int PERF_100NSEC_TIMER_INV =
                (PERF_SIZE_LARGE | PERF_TYPE_COUNTER | PERF_COUNTER_RATE |
                PERF_TIMER_100NS | PERF_DELTA_COUNTER | PERF_INVERSE_COUNTER  |
                PERF_DISPLAY_PERCENT);

        // 64-bit Timer.  Divide delta by delta time.  Display suffix: "%"
        // Timer for multiple instances, so result can exceed 100%.
        public const int PERF_COUNTER_MULTI_TIMER =
                (PERF_SIZE_LARGE | PERF_TYPE_COUNTER | PERF_COUNTER_RATE |
                PERF_DELTA_COUNTER | PERF_TIMER_TICK | PERF_MULTI_COUNTER |
                PERF_DISPLAY_PERCENT);

        // 64-bit Timer inverse (e.g., idle is measured, but display busy %)
        // Display 100 * _MULTI_BASE - delta divided by delta time.
        // Display suffix: "%" Timer for multiple instances, so result
        // can exceed 100%.  Followed by a counter of type _MULTI_BASE.
        public const int PERF_COUNTER_MULTI_TIMER_INV =
                (PERF_SIZE_LARGE | PERF_TYPE_COUNTER | PERF_COUNTER_RATE |
                PERF_DELTA_COUNTER | PERF_MULTI_COUNTER | PERF_TIMER_TICK |
                PERF_INVERSE_COUNTER | PERF_DISPLAY_PERCENT);

        // Number of instances to which the preceding _MULTI_..._INV counter
        // applies.  Used as a factor to get the percentage.
        public const int PERF_COUNTER_MULTI_BASE =
                (PERF_SIZE_LARGE | PERF_TYPE_COUNTER | PERF_COUNTER_BASE |
                PERF_MULTI_COUNTER | PERF_DISPLAY_NOSHOW);

        // 64-bit Timer In 100 nsec units. Display delta divided by delta time.
        // Display suffix: "%" Timer for multiple instances, so result can exceed 100%.
        public const int PERF_100NSEC_MULTI_TIMER =
                (PERF_SIZE_LARGE | PERF_TYPE_COUNTER | PERF_DELTA_COUNTER  |
                PERF_COUNTER_RATE | PERF_TIMER_100NS | PERF_MULTI_COUNTER |
                PERF_DISPLAY_PERCENT);

        // 64-bit Timer inverse (e.g., idle is measured, but display busy %)
        // Display 100 * _MULTI_BASE - delta divided by delta time.
        // Display suffix: "%" Timer for multiple instances, so result
        // can exceed 100%.  Followed by a counter of type _MULTI_BASE.
        public const int PERF_100NSEC_MULTI_TIMER_INV =
                (PERF_SIZE_LARGE | PERF_TYPE_COUNTER | PERF_DELTA_COUNTER  |
                PERF_COUNTER_RATE | PERF_TIMER_100NS | PERF_MULTI_COUNTER |
                PERF_INVERSE_COUNTER | PERF_DISPLAY_PERCENT);

        // Indicates the data is a fraction of the following counter  which
        // should not be time averaged on display (such as free space over
        // total space.) Display as is.  Display the quotient as "%".
        public const int PERF_RAW_FRACTION =
                (PERF_SIZE_DWORD | PERF_TYPE_COUNTER | PERF_COUNTER_FRACTION |
                PERF_DISPLAY_PERCENT);

        public const int PERF_LARGE_RAW_FRACTION =
                    (PERF_SIZE_LARGE | PERF_TYPE_COUNTER | PERF_COUNTER_FRACTION |
                    PERF_DISPLAY_PERCENT);
        
        // Indicates the data is a base for the preceding counter which should
        // not be time averaged on display (such as free space over total space.)
        public const int PERF_RAW_BASE =
                (PERF_SIZE_DWORD | PERF_TYPE_COUNTER | PERF_COUNTER_BASE |
                PERF_DISPLAY_NOSHOW |
                0x00000003);  // for compatibility with pre-beta versions

        public const int PERF_LARGE_RAW_BASE =
                    (PERF_SIZE_LARGE | PERF_TYPE_COUNTER | PERF_COUNTER_BASE |
                    PERF_DISPLAY_NOSHOW );
        
        // The data collected In this counter is actually the start time of the
        // item being measured. For display, this data is subtracted from the
        // sample time to yield the elapsed time as the difference between the two.
        // In the definition below, the PerfTime field of the Object contains
        // the sample time as indicated by the PERF_OBJECT_TIMER bit and the
        // difference is scaled by the PerfFreq of the Object to convert the time
        // units into seconds.
        public const int PERF_ELAPSED_TIME =
                (PERF_SIZE_LARGE | PERF_TYPE_COUNTER | PERF_COUNTER_ELAPSED |
                PERF_OBJECT_TIMER | PERF_DISPLAY_SECONDS);

        //
        //  The following counter type can be used with the preceding types to
        //  define a range of values to be displayed In a histogram.
        //

        //
        //  This counter is used to display the difference from one sample
        //  to the next. The counter value is a constantly increasing number
        //  and the value displayed is the difference between the current
        //  value and the previous value. Negative numbers are not allowed
        //  which shouldn't be a problem as long as the counter value is
        //  increasing or unchanged.
        //
        public const int PERF_COUNTER_DELTA =
                (PERF_SIZE_DWORD | PERF_TYPE_COUNTER | PERF_COUNTER_VALUE |
                PERF_DELTA_COUNTER | PERF_DISPLAY_NO_SUFFIX);

        public const int PERF_COUNTER_LARGE_DELTA =
                (PERF_SIZE_LARGE | PERF_TYPE_COUNTER | PERF_COUNTER_VALUE |
                PERF_DELTA_COUNTER | PERF_DISPLAY_NO_SUFFIX);

        // The timer used has the same frequency as the System Performance Timer
        public const int PERF_PRECISION_SYSTEM_TIMER =
                (PERF_SIZE_LARGE    | PERF_TYPE_COUNTER     | PERF_COUNTER_PRECISION    | 
                 PERF_TIMER_TICK    | PERF_DELTA_COUNTER    | PERF_DISPLAY_PERCENT   );

        //
        // The timer used has the same frequency as the 100 NanoSecond Timer
        public const int PERF_PRECISION_100NS_TIMER  =
                (PERF_SIZE_LARGE    | PERF_TYPE_COUNTER     | PERF_COUNTER_PRECISION    | 
                 PERF_TIMER_100NS   | PERF_DELTA_COUNTER    | PERF_DISPLAY_PERCENT   );
        //
        // The timer used is of the frequency specified in the Object header's
        //  PerfFreq field (PerfTime is ignored)
        public const int PERF_PRECISION_OBJECT_TIMER =
                (PERF_SIZE_LARGE    | PERF_TYPE_COUNTER     | PERF_COUNTER_PRECISION    | 
                 PERF_OBJECT_TIMER  | PERF_DELTA_COUNTER    | PERF_DISPLAY_PERCENT   );
        
        public const uint PDH_FMT_DOUBLE =  0x00000200;
        public const uint PDH_FMT_NOSCALE   =   0x00001000;
        public const uint PDH_FMT_NOCAP100  =   0x00008000;

        
        
        [StructLayout(LayoutKind.Sequential)]
        public class PDH_RAW_COUNTER {
            public int CStatus = 0;
            public long TimeStamp = 0;
            public long FirstValue = 0;
            public long SecondValue = 0;
            public int MultiCount = 0;
        }
        
        [StructLayout(LayoutKind.Sequential)]
        public class PDH_FMT_COUNTERVALUE {  
            public int CStatus = 0;
            public double data = 0;
        }

        //
        //  The following are used to determine the level of detail associated
        //  with the counter.  The user will be setting the level of detail
        //  that should be displayed at any given time.
        //
        public const int PERF_DETAIL_NOVICE      =    100; // The uninformed can understand it
        public const int PERF_DETAIL_ADVANCED    =    200; // For the advanced user
        public const int PERF_DETAIL_EXPERT      =    300; // For the expert user
        public const int PERF_DETAIL_WIZARD      =    400; // For the system designer

        [StructLayout(LayoutKind.Sequential)]
        internal class PERF_COUNTER_BLOCK {
            public int ByteLength = 0;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal class PERF_COUNTER_DEFINITION {
            public int ByteLength = 0;
            public int CounterNameTitleIndex = 0;

            // this one is kind of weird. It is defined as in SDK:
            // #ifdef _WIN64
            //  DWORD           CounterNameTitle;
            // #else
            //  LPWSTR          CounterNameTitle;
            // #endif
            // so we can't use IntPtr here.

            public int CounterNameTitlePtr = 0;
            public int CounterHelpTitleIndex = 0;
            public int CounterHelpTitlePtr = 0;
            public int DefaultScale = 0;
            public int DetailLevel = 0;
            public int CounterType = 0;
            public int CounterSize = 0;
            public int CounterOffset = 0;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal class PERF_DATA_BLOCK {
            public int Signature1 = 0;
            public int Signature2 = 0;
            public int LittleEndian = 0;
            public int Version = 0;
            public int Revision = 0;
            public int TotalByteLength = 0;
            public int HeaderLength = 0;
            public int NumObjectTypes = 0;
            public int DefaultObject = 0;
            public SYSTEMTIME SystemTime = null;
            public int pad1 = 0;  // Need to pad the struct to get quadword alignment for the 'long' after SystemTime
            public long PerfTime = 0;
            public long PerfFreq = 0;
            public long PerfTime100nSec = 0;
            public int SystemNameLength = 0;
            public int SystemNameOffset = 0;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal class PERF_INSTANCE_DEFINITION {
            public int ByteLength = 0;
            public int ParentObjectTitleIndex = 0;
            public int ParentObjectInstance = 0;
            public int UniqueID = 0;
            public int NameOffset = 0;
            public int NameLength = 0;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal class PERF_OBJECT_TYPE {
            public int TotalByteLength = 0;
            public int DefinitionLength = 0;
            public int HeaderLength = 0;
            public int ObjectNameTitleIndex = 0;
            public int ObjectNameTitlePtr = 0;
            public int ObjectHelpTitleIndex = 0;
            public int ObjectHelpTitlePtr = 0;
            public int DetailLevel = 0;
            public int NumCounters = 0;
            public int DefaultCounter = 0;
            public int NumInstances = 0;
            public int CodePage = 0;
            public long PerfTime = 0;
            public long PerfFreq = 0;
        }

        [DllImport(ExternDll.Kernel32, CharSet=CharSet.Auto, SetLastError=true, BestFitMapping=false)]
        [ResourceExposure(ResourceScope.Machine)]
        internal static extern SafeFileMappingHandle CreateFileMapping(IntPtr hFile, NativeMethods.SECURITY_ATTRIBUTES lpFileMappingAttributes, int flProtect, int dwMaximumSizeHigh, int dwMaximumSizeLow, string lpName);

        [DllImport(ExternDll.Kernel32, CharSet=CharSet.Auto, SetLastError=true, BestFitMapping=false)]        
        [ResourceExposure(ResourceScope.Machine)]
        internal static extern SafeFileMappingHandle OpenFileMapping(int dwDesiredAccess, bool bInheritHandle, string lpName);

#endif // !FEATURE_PAL
#endif // !SILVERLIGHT

        // copied from winbase.h
        public const int FORMAT_MESSAGE_ALLOCATE_BUFFER = 0x00000100;
        public const int FORMAT_MESSAGE_IGNORE_INSERTS  = 0x00000200;
        public const int FORMAT_MESSAGE_FROM_STRING     = 0x00000400;
        public const int FORMAT_MESSAGE_FROM_HMODULE    = 0x00000800;
        public const int FORMAT_MESSAGE_FROM_SYSTEM     = 0x00001000;
        public const int FORMAT_MESSAGE_ARGUMENT_ARRAY  = 0x00002000;
        public const int FORMAT_MESSAGE_MAX_WIDTH_MASK  = 0x000000FF;

#if !SILVERLIGHT
#if !FEATURE_PAL
        public const int LOAD_WITH_ALTERED_SEARCH_PATH  = 0x00000008;
        public const int LOAD_LIBRARY_AS_DATAFILE       = 0x00000002;

        public const int SEEK_READ = 0x2;
        public const int FORWARDS_READ = 0x4;
        public const int BACKWARDS_READ = 0x8;
        public const int ERROR_EVENTLOG_FILE_CHANGED = 1503;

        [DllImport(ExternDll.User32, CharSet=System.Runtime.InteropServices.CharSet.Auto, SetLastError=true)]
        [ResourceExposure(ResourceScope.None)]
        public static extern int WaitForInputIdle(SafeProcessHandle handle, int milliseconds); 

        [DllImport(ExternDll.Kernel32, CharSet=System.Runtime.InteropServices.CharSet.Auto, SetLastError=true)]
        [ResourceExposure(ResourceScope.None)]
        public static extern SafeProcessHandle OpenProcess(int access, bool inherit, int processId);

        [DllImport(ExternDll.Psapi, CharSet=System.Runtime.InteropServices.CharSet.Auto, SetLastError=true)]
        [ResourceExposure(ResourceScope.None)]
        public static extern bool EnumProcessModules(SafeProcessHandle handle, IntPtr modules, int size, ref int needed);
    
        [DllImport(ExternDll.Psapi, CharSet=System.Runtime.InteropServices.CharSet.Auto, SetLastError=true)]
        [ResourceExposure(ResourceScope.Machine)]
        public static extern bool EnumProcesses(int[] processIds, int size, out int needed);

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        [DllImport(ExternDll.Psapi, CharSet=System.Runtime.InteropServices.CharSet.Auto, SetLastError=true, BestFitMapping=false)]
        [ResourceExposure(ResourceScope.Machine)]
        public static extern int GetModuleFileNameEx(HandleRef processHandle, HandleRef moduleHandle, StringBuilder baseName, int size);

        [DllImport(ExternDll.Psapi, CharSet=System.Runtime.InteropServices.CharSet.Auto, SetLastError=true)]
        [ResourceExposure(ResourceScope.Process)]
        public static extern bool GetModuleInformation(SafeProcessHandle processHandle, HandleRef moduleHandle, NtModuleInfo ntModuleInfo, int size);
        [DllImport(ExternDll.Psapi, CharSet=System.Runtime.InteropServices.CharSet.Auto, SetLastError=true, BestFitMapping=false)]
        [ResourceExposure(ResourceScope.Machine)]
        public static extern int GetModuleBaseName(SafeProcessHandle processHandle, HandleRef moduleHandle, StringBuilder baseName, int size);
        [DllImport(ExternDll.Psapi, CharSet=System.Runtime.InteropServices.CharSet.Auto, SetLastError=true, BestFitMapping=false)]
        [ResourceExposure(ResourceScope.Machine)]
        public static extern int GetModuleFileNameEx(SafeProcessHandle processHandle, HandleRef moduleHandle, StringBuilder baseName, int size);
        
        [DllImport(ExternDll.Kernel32, CharSet=System.Runtime.InteropServices.CharSet.Auto, SetLastError=true)]
        [ResourceExposure(ResourceScope.Machine)]
        public static extern bool SetProcessWorkingSetSize(SafeProcessHandle handle, IntPtr min, IntPtr max);        
        [DllImport(ExternDll.Kernel32, CharSet=System.Runtime.InteropServices.CharSet.Auto, SetLastError=true)]
        [ResourceExposure(ResourceScope.None)]
        public static extern bool GetProcessWorkingSetSize(SafeProcessHandle handle, out IntPtr min, out IntPtr max);
        [DllImport(ExternDll.Kernel32, CharSet=System.Runtime.InteropServices.CharSet.Auto, SetLastError=true)]
        [ResourceExposure(ResourceScope.Machine)]
        public static extern bool SetProcessAffinityMask(SafeProcessHandle handle, IntPtr mask);
        [DllImport(ExternDll.Kernel32, CharSet=System.Runtime.InteropServices.CharSet.Auto, SetLastError=true)]
        [ResourceExposure(ResourceScope.None)]
        public static extern bool GetProcessAffinityMask(SafeProcessHandle handle, out IntPtr processMask, out IntPtr systemMask);
        [DllImport(ExternDll.Kernel32, CharSet=System.Runtime.InteropServices.CharSet.Auto, SetLastError=true)]
        [ResourceExposure(ResourceScope.None)]
        public static extern bool GetThreadPriorityBoost(SafeThreadHandle handle, out bool disabled);
        [DllImport(ExternDll.Kernel32, CharSet=System.Runtime.InteropServices.CharSet.Auto, SetLastError=true)]
        [ResourceExposure(ResourceScope.None)]
        public static extern bool SetThreadPriorityBoost(SafeThreadHandle handle, bool disabled);        
        [DllImport(ExternDll.Kernel32, CharSet=System.Runtime.InteropServices.CharSet.Auto, SetLastError=true)]
        [ResourceExposure(ResourceScope.None)]
        public static extern bool GetProcessPriorityBoost(SafeProcessHandle handle, out bool disabled);
        [DllImport(ExternDll.Kernel32, CharSet=System.Runtime.InteropServices.CharSet.Auto, SetLastError=true)]
        [ResourceExposure(ResourceScope.None)]
        public static extern bool SetProcessPriorityBoost(SafeProcessHandle handle, bool disabled);
        [DllImport(ExternDll.Kernel32, CharSet=System.Runtime.InteropServices.CharSet.Auto, SetLastError=true)]
        [ResourceExposure(ResourceScope.Process)]
        public static extern SafeThreadHandle OpenThread(int access, bool inherit, int threadId);
        [DllImport(ExternDll.Kernel32, CharSet=System.Runtime.InteropServices.CharSet.Auto, SetLastError=true)]
        [ResourceExposure(ResourceScope.Process)]
        public static extern bool SetThreadPriority(SafeThreadHandle handle, int priority);
        [DllImport(ExternDll.Kernel32, CharSet=System.Runtime.InteropServices.CharSet.Auto, SetLastError=true)]
        [ResourceExposure(ResourceScope.None)]
        public static extern int GetThreadPriority(SafeThreadHandle handle);
        [DllImport(ExternDll.Kernel32, CharSet=System.Runtime.InteropServices.CharSet.Auto, SetLastError=true)]
        [ResourceExposure(ResourceScope.Process)]
        public static extern IntPtr SetThreadAffinityMask(SafeThreadHandle handle, HandleRef mask);
        [DllImport(ExternDll.Kernel32, CharSet=System.Runtime.InteropServices.CharSet.Auto, SetLastError=true)]
        [ResourceExposure(ResourceScope.Process)]
        public static extern int SetThreadIdealProcessor(SafeThreadHandle handle, int processor);
        
        [DllImport(ExternDll.Kernel32, CharSet=System.Runtime.InteropServices.CharSet.Auto, SetLastError=true)]
        [ResourceExposure(ResourceScope.Process)]
        public static extern IntPtr CreateToolhelp32Snapshot(int flags, int processId);
        [DllImport(ExternDll.Kernel32, CharSet=System.Runtime.InteropServices.CharSet.Auto, SetLastError=true)]
        [ResourceExposure(ResourceScope.None)]
        public static extern bool Process32First(HandleRef handle, IntPtr entry);
        [DllImport(ExternDll.Kernel32, CharSet=System.Runtime.InteropServices.CharSet.Auto, SetLastError=true)]
        [ResourceExposure(ResourceScope.None)]
        public static extern bool Process32Next(HandleRef handle, IntPtr entry);
        [DllImport(ExternDll.Kernel32, CharSet=System.Runtime.InteropServices.CharSet.Auto, SetLastError=true)]
        [ResourceExposure(ResourceScope.None)]
        public static extern bool Thread32First(HandleRef handle, WinThreadEntry entry);
        [DllImport(ExternDll.Kernel32, CharSet=System.Runtime.InteropServices.CharSet.Auto, SetLastError=true)]
        [ResourceExposure(ResourceScope.None)]
        public static extern bool Thread32Next(HandleRef handle, WinThreadEntry entry);
        [DllImport(ExternDll.Kernel32, CharSet=System.Runtime.InteropServices.CharSet.Auto, SetLastError=true)]
        [ResourceExposure(ResourceScope.None)]
        public static extern bool Module32First(HandleRef handle, IntPtr entry);
        [DllImport(ExternDll.Kernel32, CharSet=System.Runtime.InteropServices.CharSet.Auto, SetLastError=true)]
        [ResourceExposure(ResourceScope.None)]
        public static extern bool Module32Next(HandleRef handle, IntPtr entry);
        [DllImport(ExternDll.Kernel32, CharSet=System.Runtime.InteropServices.CharSet.Auto, SetLastError=true)]
        [ResourceExposure(ResourceScope.None)]
        public static extern int GetPriorityClass(SafeProcessHandle handle);
        [DllImport(ExternDll.Kernel32, CharSet=System.Runtime.InteropServices.CharSet.Auto, SetLastError=true)]
        [ResourceExposure(ResourceScope.Machine)]
        public static extern bool SetPriorityClass(SafeProcessHandle handle, int priorityClass);        
        [DllImport(ExternDll.User32, CharSet=System.Runtime.InteropServices.CharSet.Auto, SetLastError=true)]
        [ResourceExposure(ResourceScope.Process)]
        public static extern bool EnumWindows(EnumThreadWindowsCallback callback, IntPtr extraData);
        [DllImport(ExternDll.User32, CharSet=System.Runtime.InteropServices.CharSet.Auto, SetLastError=true)]
        [ResourceExposure(ResourceScope.Machine)]
        public static extern int GetWindowThreadProcessId(HandleRef handle, out int processId);
        [DllImport(ExternDll.Shell32, CharSet=System.Runtime.InteropServices.CharSet.Auto, SetLastError=true)]
        [ResourceExposure(ResourceScope.Machine)]
        public static extern bool ShellExecuteEx(ShellExecuteInfo info);
        [DllImport(ExternDll.Ntdll, CharSet=System.Runtime.InteropServices.CharSet.Auto)]
        [ResourceExposure(ResourceScope.Machine)]
        public static extern int NtQueryInformationProcess(SafeProcessHandle processHandle, int query, NtProcessBasicInfo info, int size, int[] returnedSize);
        [DllImport(ExternDll.Ntdll, CharSet=System.Runtime.InteropServices.CharSet.Auto)]
        [ResourceExposure(ResourceScope.None)]
        public static extern int NtQuerySystemInformation(int query, IntPtr dataPtr, int size, out int returnedSize);
        [DllImport(ExternDll.Kernel32, CharSet=System.Runtime.InteropServices.CharSet.Auto, BestFitMapping=false)]
        [ResourceExposure(ResourceScope.Machine)]
        public static extern SafeFileHandle CreateFile(string lpFileName,int dwDesiredAccess,int dwShareMode, SECURITY_ATTRIBUTES lpSecurityAttributes, int dwCreationDisposition,int dwFlagsAndAttributes, SafeFileHandle hTemplateFile);


#endif // !FEATURE_PAL

        [DllImport(ExternDll.Kernel32, CharSet=System.Runtime.InteropServices.CharSet.Ansi, SetLastError=true, BestFitMapping=false)]    
        [ResourceExposure(ResourceScope.Machine)]
        public static extern bool DuplicateHandle(
            HandleRef hSourceProcessHandle,
            SafeHandle hSourceHandle,
            HandleRef hTargetProcess,
            out SafeFileHandle targetHandle,
            int dwDesiredAccess,
            bool bInheritHandle,
            int dwOptions
        );

        [DllImport(ExternDll.Kernel32, CharSet=System.Runtime.InteropServices.CharSet.Ansi, SetLastError=true, BestFitMapping=false)]    
        [ResourceExposure(ResourceScope.Machine)]
        public static extern bool DuplicateHandle(
            HandleRef hSourceProcessHandle,
            SafeHandle hSourceHandle,
            HandleRef hTargetProcess,
            out SafeWaitHandle targetHandle,
            int dwDesiredAccess,
            bool bInheritHandle,
            int dwOptions
        );
        

#if !FEATURE_PAL
        /* Unused
        [DllImport(ExternDll.Advapi32, CharSet=System.Runtime.InteropServices.CharSet.Auto, SetLastError=true, BestFitMapping=false)]
        [ResourceExposure(ResourceScope.Process)]
        public static extern bool LogonUser(
            [MarshalAs(UnmanagedType.LPTStr)]
            string lpszUsername,
            [MarshalAs(UnmanagedType.LPTStr)]
            string lpszDomain,
            [MarshalAs(UnmanagedType.LPTStr)]
            string lpszPassword,
            int dwLogonType,
            int dwLogonProvider,
            out IntPtr phToken
        );
        */
        [DllImport(ExternDll.Advapi32, CharSet=System.Runtime.InteropServices.CharSet.Auto, SetLastError=true)]
        [ResourceExposure(ResourceScope.Process)]
        public static extern bool OpenProcessToken(HandleRef ProcessHandle, int DesiredAccess, out IntPtr TokenHandle);
        [DllImport(ExternDll.Advapi32, CharSet=System.Runtime.InteropServices.CharSet.Auto, SetLastError=true, BestFitMapping=false)]
        [ResourceExposure(ResourceScope.None)]
        public static extern bool LookupPrivilegeValue([MarshalAs(UnmanagedType.LPTStr)] string lpSystemName, [MarshalAs(UnmanagedType.LPTStr)] string lpName, out LUID lpLuid);
        [DllImport(ExternDll.Advapi32, CharSet=System.Runtime.InteropServices.CharSet.Auto, SetLastError=true)]
        [ResourceExposure(ResourceScope.Process)]
        public static extern bool AdjustTokenPrivileges(
            HandleRef TokenHandle,
            bool DisableAllPrivileges,
            TokenPrivileges NewState,
            int BufferLength,
            IntPtr PreviousState,
            IntPtr ReturnLength
        );
        [DllImport(ExternDll.User32, CharSet=System.Runtime.InteropServices.CharSet.Auto, BestFitMapping=true)]
        [ResourceExposure(ResourceScope.None)]
        public static extern int GetWindowText(HandleRef hWnd, StringBuilder lpString, int nMaxCount);
        [DllImport(ExternDll.User32, CharSet=System.Runtime.InteropServices.CharSet.Auto)]
        [ResourceExposure(ResourceScope.None)]
        public static extern int GetWindowTextLength(HandleRef hWnd);
        [DllImport(ExternDll.User32, CharSet=System.Runtime.InteropServices.CharSet.Auto)]
        [ResourceExposure(ResourceScope.None)]
        public static extern bool IsWindowVisible(HandleRef hWnd);
        [DllImport(ExternDll.User32, CharSet=System.Runtime.InteropServices.CharSet.Auto)]
        [ResourceExposure(ResourceScope.None)]
        public static extern IntPtr SendMessageTimeout(HandleRef hWnd, int msg, IntPtr wParam, IntPtr lParam, int flags, int timeout, out IntPtr pdwResult);
        [DllImport(ExternDll.User32, CharSet=System.Runtime.InteropServices.CharSet.Auto)]
        [ResourceExposure(ResourceScope.None)]
        public static extern int GetWindowLong(HandleRef hWnd, int nIndex);
        [DllImport(ExternDll.User32, CharSet=System.Runtime.InteropServices.CharSet.Auto)]
        [ResourceExposure(ResourceScope.None)]
        public static extern int PostMessage(HandleRef hwnd, int msg, IntPtr wparam, IntPtr lparam);
        [DllImport(ExternDll.User32, ExactSpelling=true, CharSet=System.Runtime.InteropServices.CharSet.Auto)]
        [ResourceExposure(ResourceScope.Process)]
        public static extern IntPtr GetWindow(HandleRef hWnd, int uCmd);

        [StructLayout(LayoutKind.Sequential)]
        internal class NtModuleInfo {
            public IntPtr BaseOfDll = (IntPtr)0;
            public int SizeOfImage = 0;
            public IntPtr EntryPoint = (IntPtr)0;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal class WinProcessEntry {
            public int dwSize = 0;
            public int cntUsage = 0;
            public int th32ProcessID = 0;
            public IntPtr th32DefaultHeapID = (IntPtr)0;
            public int th32ModuleID = 0;
            public int cntThreads = 0;
            public int th32ParentProcessID = 0;
            public int pcPriClassBase = 0;
            public int dwFlags = 0;
            //[MarshalAs(UnmanagedType.ByValTStr, SizeConst=260)]
            //public string fileName;
            //byte fileName[260];
            public const int sizeofFileName = 260;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal class WinThreadEntry {
            public int dwSize = 0;
            public int cntUsage = 0;
            public int th32ThreadID = 0;
            public int th32OwnerProcessID = 0;
            public int tpBasePri = 0;
            public int tpDeltaPri = 0;
            public int dwFlags = 0;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal class WinModuleEntry {  // MODULEENTRY32
            public int dwSize = 0;
            public int th32ModuleID = 0;
            public int th32ProcessID = 0;
            public int GlblcntUsage = 0;
            public int ProccntUsage = 0;
            public IntPtr modBaseAddr = (IntPtr)0;
            public int modBaseSize = 0;
            public IntPtr hModule = (IntPtr)0;
            //byte moduleName[256];
            //[MarshalAs(UnmanagedType.ByValTStr, SizeConst=256)]
            //public string moduleName;
            //[MarshalAs(UnmanagedType.ByValTStr, SizeConst=260)]
            //public string fileName;
            //byte fileName[260];
            public const int sizeofModuleName = 256;
            public const int sizeofFileName = 260;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal class ShellExecuteInfo {
            public int cbSize = 0;
            public int fMask = 0;
            public IntPtr hwnd = (IntPtr)0;
            public IntPtr lpVerb = (IntPtr)0;
            public IntPtr lpFile = (IntPtr)0;
            public IntPtr lpParameters = (IntPtr)0;
            public IntPtr lpDirectory = (IntPtr)0;
            public int nShow = 0;
            public IntPtr hInstApp = (IntPtr)0;
            public IntPtr lpIDList = (IntPtr)0;
            public IntPtr lpClass = (IntPtr)0;
            public IntPtr hkeyClass = (IntPtr)0;
            public int dwHotKey = 0;
            public IntPtr hIcon = (IntPtr)0;
            public IntPtr hProcess = (IntPtr)0;

            [ResourceExposure(ResourceScope.Machine)]
            public ShellExecuteInfo() {
                cbSize = Marshal.SizeOf(this);
            }
        }

        // NT definition
        // typedef struct _PROCESS_BASIC_INFORMATION {
        //    NTSTATUS ExitStatus; (LONG)
        //    PPEB PebBaseAddress;
        //    ULONG_PTR AffinityMask;
        //    KPRIORITY BasePriority;  (LONG)
        //    ULONG_PTR UniqueProcessId;
        //    ULONG_PTR InheritedFromUniqueProcessId;
        //} PROCESS_BASIC_INFORMATION;

        [StructLayout(LayoutKind.Sequential)]
        internal class NtProcessBasicInfo {
            public int ExitStatus = 0;
            public IntPtr PebBaseAddress = (IntPtr)0;
            public IntPtr AffinityMask = (IntPtr)0;
            public int BasePriority = 0;
            public IntPtr UniqueProcessId = (IntPtr)0;
            public IntPtr InheritedFromUniqueProcessId = (IntPtr)0;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct LUID {
            public int LowPart;
            public int HighPart;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal class TokenPrivileges {
            public int PrivilegeCount = 1;
            public LUID Luid;
            public int Attributes = 0;
        }

        internal delegate bool EnumThreadWindowsCallback(IntPtr hWnd, IntPtr lParam);

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses")]
        [StructLayout(LayoutKind.Sequential)]
        internal class SYSTEMTIME {
            public short wYear;
            public short wMonth;
            public short wDayOfWeek;
            public short wDay;
            public short wHour;
            public short wMinute;
            public short wSecond;
            public short wMilliseconds;

            public override string ToString() {
                return "[SYSTEMTIME: "
                + wDay.ToString(CultureInfo.CurrentCulture) + "/" + wMonth.ToString(CultureInfo.CurrentCulture) + "/" + wYear.ToString(CultureInfo.CurrentCulture)
                + " " + wHour.ToString(CultureInfo.CurrentCulture) + ":" + wMinute.ToString(CultureInfo.CurrentCulture) + ":" + wSecond.ToString(CultureInfo.CurrentCulture)
                + "]";
            }
        }

        public const int NtPerfCounterSizeDword = 0x00000000;
        public const int NtPerfCounterSizeLarge = 0x00000100;

        public const int SHGFI_USEFILEATTRIBUTES = 0x000000010;  // use passed dwFileAttribute
        public const int SHGFI_TYPENAME = 0x000000400;

        public const int NtQueryProcessBasicInfo = 0;          
        public const int NtQuerySystemProcessInformation = 5;

        public const int SEE_MASK_CLASSNAME = 0x00000001;    // Note CLASSKEY overrides CLASSNAME
        public const int SEE_MASK_CLASSKEY = 0x00000003;
        public const int SEE_MASK_IDLIST = 0x00000004;    // Note INVOKEIDLIST overrides IDLIST
        public const int SEE_MASK_INVOKEIDLIST = 0x0000000c;
        public const int SEE_MASK_ICON = 0x00000010;
        public const int SEE_MASK_HOTKEY = 0x00000020;
        public const int SEE_MASK_NOCLOSEPROCESS = 0x00000040;
        public const int SEE_MASK_CONNECTNETDRV = 0x00000080;
        public const int SEE_MASK_FLAG_DDEWAIT = 0x00000100;
        public const int SEE_MASK_DOENVSUBST = 0x00000200;
        public const int SEE_MASK_FLAG_NO_UI = 0x00000400;
        public const int SEE_MASK_UNICODE = 0x00004000;
        public const int SEE_MASK_NO_CONSOLE = 0x00008000;
        public const int SEE_MASK_ASYNCOK = 0x00100000;

        public const int TH32CS_SNAPHEAPLIST = 0x00000001;
        public const int TH32CS_SNAPPROCESS = 0x00000002;
        public const int TH32CS_SNAPTHREAD = 0x00000004;
        public const int TH32CS_SNAPMODULE = 0x00000008;
        public const int TH32CS_INHERIT = unchecked((int)0x80000000);

#endif // !FEATURE_PAL

        public const int PROCESS_TERMINATE = 0x0001;
        public const int PROCESS_CREATE_THREAD = 0x0002;
        public const int PROCESS_SET_SESSIONID = 0x0004;
        public const int PROCESS_VM_OPERATION = 0x0008;
        public const int PROCESS_VM_READ = 0x0010;
        public const int PROCESS_VM_WRITE = 0x0020;
        public const int PROCESS_DUP_HANDLE = 0x0040;
        public const int PROCESS_CREATE_PROCESS = 0x0080;
        public const int PROCESS_SET_QUOTA = 0x0100;
        public const int PROCESS_SET_INFORMATION = 0x0200;
        public const int PROCESS_QUERY_INFORMATION = 0x0400;
        public const int PROCESS_QUERY_LIMITED_INFORMATION = 0x1000;
        public const int STANDARD_RIGHTS_REQUIRED = 0x000F0000;
        public const int SYNCHRONIZE = 0x00100000;
        public const int PROCESS_ALL_ACCESS = STANDARD_RIGHTS_REQUIRED | SYNCHRONIZE | 0xFFF;

#if !FEATURE_PAL

        public const int THREAD_TERMINATE = 0x0001;
        public const int THREAD_SUSPEND_RESUME = 0x0002;
        public const int THREAD_GET_CONTEXT = 0x0008;
        public const int THREAD_SET_CONTEXT = 0x0010;
        public const int THREAD_SET_INFORMATION = 0x0020;
        public const int THREAD_QUERY_INFORMATION = 0x0040;
        public const int THREAD_SET_THREAD_TOKEN = 0x0080;
        public const int THREAD_IMPERSONATE = 0x0100;
        public const int THREAD_DIRECT_IMPERSONATION = 0x0200;

        public static readonly IntPtr HKEY_LOCAL_MACHINE = unchecked((IntPtr)(int)0x80000002);
        public const int REG_BINARY = 3;
        public const int REG_MULTI_SZ = 7;

        public const int READ_CONTROL                    = 0x00020000;
        public const int STANDARD_RIGHTS_READ            = READ_CONTROL;

        public const int KEY_QUERY_VALUE        = 0x0001;
        public const int KEY_ENUMERATE_SUB_KEYS = 0x0008;
        public const int KEY_NOTIFY             = 0x0010;

        public const int KEY_READ               =((STANDARD_RIGHTS_READ |
                                                           KEY_QUERY_VALUE |
                                                           KEY_ENUMERATE_SUB_KEYS |
                                                           KEY_NOTIFY)
                                                          &
                                                          (~SYNCHRONIZE));

#endif // !FEATURE_PAL
#endif // !SILVERLIGHT

#if !SILVERLIGHT || FEATURE_NETCORE
        public const int ERROR_BROKEN_PIPE = 109;
        public const int ERROR_NO_DATA = 232;
        public const int ERROR_HANDLE_EOF = 38;
        public const int ERROR_IO_INCOMPLETE = 996;
        public const int ERROR_IO_PENDING = 997;
        public const int ERROR_FILE_EXISTS = 0x50;
        public const int ERROR_FILENAME_EXCED_RANGE = 0xCE;  // filename too long.
        public const int ERROR_MORE_DATA = 234;
        public const int ERROR_CANCELLED = 1223;
        public const int ERROR_FILE_NOT_FOUND = 2;
        public const int ERROR_PATH_NOT_FOUND = 3;
        public const int ERROR_ACCESS_DENIED = 5;
        public const int ERROR_INVALID_HANDLE = 6;
        public const int ERROR_NOT_ENOUGH_MEMORY = 8;
        public const int ERROR_BAD_COMMAND = 22;
        public const int ERROR_SHARING_VIOLATION = 32;
        public const int ERROR_OPERATION_ABORTED = 995;
        public const int ERROR_NO_ASSOCIATION = 1155;
        public const int ERROR_DLL_NOT_FOUND = 1157;
        public const int ERROR_DDE_FAIL = 1156;
        public const int ERROR_INVALID_PARAMETER = 87;
        public const int ERROR_PARTIAL_COPY = 299;
        public const int ERROR_SUCCESS = 0;
        public const int ERROR_ALREADY_EXISTS = 183;
        public const int ERROR_COUNTER_TIMEOUT = 1121;
#endif // !SILVERLIGHT || FEATURE_NETCORE

#if !SILVERLIGHT
        public const int DUPLICATE_CLOSE_SOURCE = 1;
        public const int DUPLICATE_SAME_ACCESS = 2;

#if !FEATURE_PAL
        public const int RPC_S_SERVER_UNAVAILABLE = 1722;
        public const int RPC_S_CALL_FAILED = 1726;

        public const int PDH_NO_DATA = unchecked((int) 0x800007D5);
        public const int PDH_CALC_NEGATIVE_DENOMINATOR = unchecked((int) 0x800007D6);
        public const int PDH_CALC_NEGATIVE_VALUE = unchecked((int) 0x800007D8);


        public const int SE_ERR_FNF = 2;
        public const int SE_ERR_PNF = 3;
        public const int SE_ERR_ACCESSDENIED = 5;
        public const int SE_ERR_OOM = 8;
        public const int SE_ERR_DLLNOTFOUND = 32;
        public const int SE_ERR_SHARE = 26;
        public const int SE_ERR_ASSOCINCOMPLETE = 27;
        public const int SE_ERR_DDETIMEOUT = 28;
        public const int SE_ERR_DDEFAIL = 29;
        public const int SE_ERR_DDEBUSY = 30;
        public const int SE_ERR_NOASSOC = 31;

        public const int SE_PRIVILEGE_ENABLED = 2;

        public const int LOGON32_LOGON_BATCH = 4;
        public const int LOGON32_PROVIDER_DEFAULT = 0;
        public const int LOGON32_LOGON_INTERACTIVE = 2;

        public const int TOKEN_ADJUST_PRIVILEGES = 0x20;
        public const int TOKEN_QUERY = 0x08;

        public const int CREATE_NO_WINDOW = 0x08000000;
        public const int CREATE_SUSPENDED = 0x00000004;
        public const int CREATE_UNICODE_ENVIRONMENT = 0x00000400;

        public const int SMTO_ABORTIFHUNG = 0x0002;
        public const int GWL_STYLE = (-16);
        public const int GCL_WNDPROC = (-24);
        public const int GWL_WNDPROC = (-4);
        public const int WS_DISABLED = 0x08000000;
        public const int WM_NULL = 0x0000;
        public const int WM_CLOSE = 0x0010;
        public const int SW_SHOWNORMAL = 1;
        public const int SW_NORMAL = 1;
        public const int SW_SHOWMINIMIZED = 2;
        public const int SW_SHOWMAXIMIZED = 3;
        public const int SW_MAXIMIZE = 3;
        public const int SW_SHOWNOACTIVATE = 4;
        public const int SW_SHOW = 5;
        public const int SW_MINIMIZE = 6;
        public const int SW_SHOWMINNOACTIVE = 7;
        public const int SW_SHOWNA = 8;
        public const int SW_RESTORE = 9;
        public const int SW_SHOWDEFAULT = 10;
        public const int SW_MAX = 10;
        public const int GW_OWNER = 4;
        public const int WHITENESS = 0x00FF0062;

        public const int
        VS_FILE_INFO = 16,
        VS_VERSION_INFO = 1,
        VS_USER_DEFINED = 100,
        VS_FFI_SIGNATURE = unchecked((int)0xFEEF04BD),
        VS_FFI_STRUCVERSION = 0x00010000,
        VS_FFI_FILEFLAGSMASK = 0x0000003F,
        VS_FF_DEBUG = 0x00000001,
        VS_FF_PRERELEASE = 0x00000002,
        VS_FF_PATCHED = 0x00000004,
        VS_FF_PRIVATEBUILD = 0x00000008,
        VS_FF_INFOINFERRED = 0x00000010,
        VS_FF_SPECIALBUILD = 0x00000020,
        VFT_UNKNOWN = 0x00000000,
        VFT_APP = 0x00000001,
        VFT_DLL = 0x00000002,
        VFT_DRV = 0x00000003,
        VFT_FONT = 0x00000004,
        VFT_VXD = 0x00000005,
        VFT_STATIC_LIB = 0x00000007,
        VFT2_UNKNOWN = 0x00000000,
        VFT2_DRV_PRINTER = 0x00000001,
        VFT2_DRV_KEYBOARD = 0x00000002,
        VFT2_DRV_LANGUAGE = 0x00000003,
        VFT2_DRV_DISPLAY = 0x00000004,
        VFT2_DRV_MOUSE = 0x00000005,
        VFT2_DRV_NETWORK = 0x00000006,
        VFT2_DRV_SYSTEM = 0x00000007,
        VFT2_DRV_INSTALLABLE = 0x00000008,
        VFT2_DRV_SOUND = 0x00000009,
        VFT2_DRV_COMM = 0x0000000A,
        VFT2_DRV_INPUTMETHOD = 0x0000000B,
        VFT2_FONT_RASTER = 0x00000001,
        VFT2_FONT_VECTOR = 0x00000002,
        VFT2_FONT_TRUETYPE = 0x00000003;

        // from Windows Forms nativemethods.cs
        [StructLayout(LayoutKind.Sequential)]
        internal class VS_FIXEDFILEINFO {
            public int dwSignature = 0;
            public int dwStructVersion = 0;
            public int dwFileVersionMS = 0;
            public int dwFileVersionLS = 0;
            public int dwProductVersionMS = 0;
            public int dwProductVersionLS = 0;
            public int dwFileFlagsMask = 0;
            public int dwFileFlags = 0;
            public int dwFileOS = 0;
            public int dwFileType = 0;
            public int dwFileSubtype = 0;
            public int dwFileDateMS = 0;
            public int dwFileDateLS = 0;
        }

        public const int
        GMEM_FIXED = 0x0000,
        GMEM_MOVEABLE = 0x0002,
        GMEM_NOCOMPACT = 0x0010,
        GMEM_NODISCARD = 0x0020,
        GMEM_ZEROINIT = 0x0040,
        GMEM_MODIFY = 0x0080,
        GMEM_DISCARDABLE = 0x0100,
        GMEM_NOT_BANKED = 0x1000,
        GMEM_SHARE = 0x2000,
        GMEM_DDESHARE = 0x2000,
        GMEM_NOTIFY = 0x4000,
        GMEM_LOWER = 0x1000,
        GMEM_VALID_FLAGS = 0x7F72,
        GMEM_INVALID_HANDLE = unchecked((int)0x8000),
        GHND = (0x0002|0x0040),
        GPTR = (0x0000|0x0040),
        GMEM_DISCARDED = 0x4000,
        GMEM_LOCKCOUNT = 0x00FF;

        public const int UOI_NAME      = 2;
        public const int UOI_TYPE      = 3;
        public const int UOI_USER_SID  = 4;

        [StructLayout(LayoutKind.Sequential)]
        internal class USEROBJECTFLAGS {
            public int fInherit = 0;
            public int fReserved = 0;
            public int dwFlags = 0;
        }

        public const int VER_PLATFORM_WIN32_NT = 2;

    internal static class Util {
        public static int HIWORD(int n) {
            return (n >> 16) & 0xffff;
        }

        public static int LOWORD(int n) {
            return n & 0xffff;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct MEMORY_BASIC_INFORMATION {
        internal IntPtr BaseAddress;
        internal IntPtr AllocationBase;
        internal uint AllocationProtect;
        internal UIntPtr RegionSize;
        internal uint State;
        internal uint Protect;
        internal uint Type;
    }

    [DllImport(ExternDll.Kernel32, SetLastError=true)]
    [ResourceExposure(ResourceScope.None)]
    unsafe internal static extern IntPtr VirtualQuery(SafeFileMapViewHandle address, ref MEMORY_BASIC_INFORMATION buffer, IntPtr sizeOfBuffer);

#endif // !FEATURE_PAL
#endif // !SILVERLIGHT

    }

}
