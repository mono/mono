//------------------------------------------------------------------------------
//------------------------------------------------------------------------------

namespace MS.Win32
{
    using Accessibility;
    using System;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Permissions;
    using System.Collections;
    using System.Diagnostics;
    using System.IO;
    using System.Text;
    using MS.Win32;
    using Microsoft.Win32.SafeHandles;


#if WINDOWS_BASE
    using MS.Internal.WindowsBase;
#elif PRESENTATION_CORE
    using MS.Internal.PresentationCore;
#elif PRESENTATIONFRAMEWORK
    using MS.Internal.PresentationFramework;
#elif UIAUTOMATIONTYPES
    using MS.Internal.UIAutomationTypes;
#elif DRT
    using MS.Internal.Drt;
#else
#error Attempt to use FriendAccessAllowedAttribute from an unknown assembly.
    using MS.Internal.YourAssemblyName;
#endif

    [FriendAccessAllowed]
    internal partial class NativeMethods
    {
        // Translates Win32 error codes into HRESULTs.
        public static int MakeHRFromErrorCode(int errorCode)
        {
            Debug.Assert((0xFFFF0000 & errorCode) == 0);
            return unchecked(((int)0x80070000) | errorCode);
        }

        public const int    FEATURE_OBJECT_CACHING = 0 ;
        public const int    FEATURE_ZONE_ELEVATION = 1;
        public const int    FEATURE_MIME_HANDLING = 2;
        public const int    FEATURE_MIME_SNIFFING = 3;
        public const int    FEATURE_WINDOW_RESTRICTIONS = 4;
        public const int    FEATURE_WEBOC_POPUPMANAGEMENT = 5;
        public const int    FEATURE_BEHAVIORS = 6;
        public const int    FEATURE_DISABLE_MK_PROTOCOL = 7;
        public const int    FEATURE_LOCALMACHINE_LOCKDOWN = 8;
        public const int    FEATURE_SECURITYBAND = 9;
        public const int    FEATURE_RESTRICT_ACTIVEXINSTALL = 10;
        public const int    FEATURE_VALIDATE_NAVIGATE_URL = 11;
        public const int    FEATURE_RESTRICT_FILEDOWNLOAD = 12;
        public const int    FEATURE_ADDON_MANAGEMENT = 13;
        public const int    FEATURE_PROTOCOL_LOCKDOWN = 14;
        public const int    FEATURE_HTTP_USERNAME_PASSWORD_DISABLE = 15;
        public const int    FEATURE_SAFE_BINDTOOBJECT = 16;
        public const int    FEATURE_UNC_SAVEDFILECHECK = 17;
        public const int    FEATURE_GET_URL_DOM_FILEPATH_UNENCODED = 18;

        // IE7 and higher
        public const int    FEATURE_TABBED_BROWSING = 19;
        public const int    FEATURE_SSLUX = 20;
        public const int    FEATURE_DISABLE_NAVIGATION_SOUNDS = 21;
        public const int    FEATURE_DISABLE_LEGACY_COMPRESSION = 22;
        public const int    FEATURE_FORCE_ADDR_AND_STATUS = 23;
        public const int    FEATURE_XMLHTTP = 24;
        public const int    FEATURE_DISABLE_TELNET_PROTOCOL = 25;
        public const int    FEATURE_FEEDS = 26;
        public const int    FEATURE_BLOCK_INPUT_PROMPTS = 27;

        public const int    GET_FEATURE_FROM_PROCESS = 0x00000002;
        public const int    SET_FEATURE_ON_PROCESS   = 0x00000002;

        public const int    URLZONE_LOCAL_MACHINE    = 0;
        public const int    URLZONE_INTRANET         = URLZONE_LOCAL_MACHINE + 1;
        public const int    URLZONE_TRUSTED          = URLZONE_INTRANET + 1;
        public const int    URLZONE_INTERNET         = URLZONE_TRUSTED + 1;
        public const int    URLZONE_UNTRUSTED        = URLZONE_INTERNET + 1;

        public const byte   URLPOLICY_ALLOW          = 0x00;
        public const byte   URLPOLICY_QUERY          = 0x01;
        public const byte   URLPOLICY_DISALLOW       = 0x03;

        public const int    URLACTION_FEATURE_ZONE_ELEVATION = 0x00002101;
        public const int    PUAF_NOUI                = 0x00000001;
        public const int    MUTZ_NOSAVEDFILECHECK    = 0x00000001;

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode )]
        internal sealed class OSVERSIONINFOEX
        {
            public int osVersionInfoSize = SizeOf();
            public int majorVersion = 0;
            public int minorVersion = 0;
            public int buildNumber = 0;
            public int platformId = 0;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst=128)]
            public string csdVersion = null;
            public short servicePackMajor = 0;
            public short servicePackMinor = 0;
            public short suiteMask = 0;
            public byte productType = 0;
            public byte reserved = 0;
            
            /// <SecurityNote>
            ///  Critical : Calls critical Marshal.SizeOf
            ///  Safe     : Calls method with trusted input (well known safe type)
            /// </SecurityNote>
            [SecuritySafeCritical]
            private static int SizeOf()
            {
                return Marshal.SizeOf(typeof(OSVERSIONINFOEX));
            }
        }
        [ComImport, Guid("79eac9ee-baf9-11ce-8c82-00aa004ba90b"), System.Runtime.InteropServices.InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        internal interface IInternetSecurityMgrSite
        {
            void GetWindow( /* [out] */ ref IntPtr phwnd) ;
            void EnableModeless( /* [in] */ bool fEnable) ;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal class OLECMD {
            [MarshalAs(UnmanagedType.U4)]
            public   int cmdID = 0;
            [MarshalAs(UnmanagedType.U4)]
            public   int cmdf = 0;

        }

        // Helper GUID type for nullability requirement in IOleCommandTarget.Exec.
        [StructLayout(LayoutKind.Sequential)]
        internal class GUID
        {
            public Guid guid;

            public GUID(Guid guid)
            {
                this.guid = guid;
            }
        }

        /// <SecurityNote>
        /// Critical - Applies SuppressUnmanagedCodeSecurity.
        /// </SecurityNote>
        [SecurityCritical]
        [SuppressUnmanagedCodeSecurity]
        [ComVisible(true), ComImport(), Guid("B722BCCB-4E68-101B-A2BC-00AA00404770")]
        [InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]        
        internal interface IOleCommandTarget
        {

	        [SecurityCritical]
            [return: MarshalAs(UnmanagedType.I4)]
            [PreserveSig]
            int QueryStatus(
                GUID pguidCmdGroup, /* nullable GUID */
                int cCmds,
                [In, Out]
                OLECMD prgCmds,
                [In, Out]
                IntPtr pCmdText);

	        [SecurityCritical]
            [return: MarshalAs(UnmanagedType.I4)]
            [PreserveSig]
            int Exec(
                GUID pguidCmdGroup, /* nullable GUID */
                int nCmdID,
                int nCmdexecopt,
                // we need to have this an array because callers need to be able to specify NULL or VT_NULL
                [In, MarshalAs(UnmanagedType.LPArray)]
                Object[] pvaIn,
                int pvaOut);
        }

        [ComVisible(true), StructLayout(LayoutKind.Sequential)]
        internal class DOCHOSTUIINFO {
            [MarshalAs(UnmanagedType.U4)]
            internal   int cbSize = SizeOf();
            [MarshalAs(UnmanagedType.I4)]
            internal   int dwFlags;
            [MarshalAs(UnmanagedType.I4)]
            internal   int dwDoubleClick;
            [MarshalAs(UnmanagedType.I4)]
            internal   int dwReserved1 = 0;
            [MarshalAs(UnmanagedType.I4)]
            internal   int dwReserved2 = 0;
            
            /// <SecurityNote>
            ///  Critical : Calls critical Marshal.SizeOf
            ///  Safe     : Calls method with trusted input (well known safe type)
            /// </SecurityNote>
            [SecuritySafeCritical]
            private static int SizeOf()
            {
                return Marshal.SizeOf(typeof(DOCHOSTUIINFO));
            }
        }


        public enum DOCHOSTUIFLAG {
            DIALOG = 0x1,
            DISABLE_HELP_MENU = 0x2,
            NO3DBORDER = 0x4,
            SCROLL_NO = 0x8,
            DISABLE_SCRIPT_INACTIVE = 0x10,
            OPENNEWWIN = 0x20,
            DISABLE_OFFSCREEN = 0x40,
            FLAT_SCROLLBAR = 0x80,
            DIV_BLOCKDEFAULT = 0x100,
            ACTIVATE_CLIENTHIT_ONLY = 0x200,
            NO3DOUTERBORDER = 0x00200000,
            ENABLE_FORMS_AUTOCOMPLETE = 0x00004000,
            ENABLE_INPLACE_NAVIGATION = 0x00010000,
            IME_ENABLE_RECONVERSION   = 0x00020000,
            THEME = 0x00040000,
            NOTHEME = 0x80000,
            DISABLE_COOKIE = 0x400,
            NOPICS                    = 0x100000,
            DISABLE_EDIT_NS_FIXUP     = 0x400000,
            LOCAL_MACHINE_ACCESS_CHECK= 0x800000,
            DISABLE_UNTRUSTEDPROTOCOL = 0x1000000,
            HOST_NAVIGATES            = 0x2000000,
            ENABLE_REDIRECT_NOTIFICATION = 0x4000000
        }

        public enum DOCHOSTUIDBLCLICK {
            DEFAULT = 0x0,
            SHOWPROPERTIES = 0x1,
            SHOWCODE = 0x2
        }
        
        /// <SecurityNote>
        /// Critical : Elevates to UnmanagedCode permissions
        /// </SecurityNote>
        [SecurityCritical]
        [DllImport(ExternDll.Gdi32, ExactSpelling = true, CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern IntPtr SetEnhMetaFileBits(uint cbBuffer, byte[] buffer);

        [StructLayout(LayoutKind.Sequential)]
        internal class ICONINFO
        {
            public bool fIcon = false;
            public int xHotspot = 0;
            public int yHotspot = 0;
            public BitmapHandle hbmMask = null;
            public BitmapHandle hbmColor = null;
        }

        internal abstract class WpfSafeHandle : SafeHandleZeroOrMinusOneIsInvalid
        {
            private int _collectorId;

            /// <SecurityNote>
            ///      Critical:This code calls into a base class which is protected by link demand and by inheritance demand
            /// </SecurityNote>
            [SecurityCritical]
            protected WpfSafeHandle(bool ownsHandle, int collectorId) : base(ownsHandle)
            {
                HandleCollector.Add(collectorId);
                _collectorId = collectorId;
            }

            /// <SecurityNote>
            /// Critical: Conceptually, this would be accessing critical data as it's in the destroy call path.
            /// TreatAsSafe: This is just destroying a handle that this object owns.
            /// </SecurityNote>
            [SecurityCritical, SecurityTreatAsSafe]
            protected override void Dispose(bool disposing)
            {
                HandleCollector.Remove(_collectorId);
                base.Dispose(disposing);
            }

            // ReleaseHandle implementation is deferred to derived classes.
            // protected override bool ReleaseHandle() {}
        }

        internal sealed class BitmapHandle : WpfSafeHandle
        {
            /// <SecurityNote>
            /// Critical: This code calls into a base class which is protected by a SecurityCritical constructor.
            /// </SecurityNote>
            [SecurityCritical]
            private BitmapHandle() : this(true)
            {
            }
            
            /// <SecurityNote>
            /// Critical: This code calls into a base class which is protected by a SecurityCritical constructor.
            /// </SecurityNote>
            [SecurityCritical]
            private BitmapHandle(bool ownsHandle) : base(ownsHandle, NativeMethods.CommonHandles.GDI)
            {
            }
            /// <SecurityNote>
            ///     Critical: This calls into DeleteObject
            /// </SecurityNote>
            [SecurityCritical]
            [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
            protected override bool ReleaseHandle()
            {
                return UnsafeNativeMethods.DeleteObject(handle);
            }

            /// <SecurityNote>
            ///     Critical: Accesses internal critical data.
            /// </SecurityNote>
            [SecurityCritical]
            internal HandleRef MakeHandleRef(object obj)
            {
                return new HandleRef(obj, handle);
            }

            /// <SecurityNote>
            ///     Critical: Creates a new BitmapHandle using Critical constructor.
            /// </SecurityNote>
            [SecurityCritical]
            internal static BitmapHandle CreateFromHandle(IntPtr hbitmap, bool ownsHandle=true)
            {
                return new BitmapHandle(ownsHandle)
                {
                    handle = hbitmap,
                };
            }
        }

        internal sealed class IconHandle : WpfSafeHandle
        {
            /// <SecurityNote>
            /// Critical: This code calls into a base class which is protected by a SecurityCritical constructor.
            /// </SecurityNote>
            [SecurityCritical]
            private IconHandle() : base(true, NativeMethods.CommonHandles.Icon)
            {
            }
            
            /// <SecurityNote>
            ///     Critical: This calls into DestroyIcon
            /// </SecurityNote>
            [SecurityCritical]
            [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
            protected override bool ReleaseHandle()
            {
                return UnsafeNativeMethods.DestroyIcon(handle);
            }

            /// <SecurityNote>
            ///     Critical: This creates a new SafeHandle, which has a critical constructor.
            ///     TreatAsSafe: The handle this creates is invalid.  It contains no critical data.
            /// </SecurityNote>
            [SecurityCritical, SecurityTreatAsSafe]
            internal static IconHandle GetInvalidIcon()
            {
                return new IconHandle();
            }

            /// <summary>
            /// Get access to the raw handle for native APIs that require it.
            /// </summary>
            /// <SecurityNote>
            ///     Critical: This accesses critical data for the safe handle.
            /// </SecurityNote>
            [SecurityCritical]
            internal IntPtr CriticalGetHandle()
            {
                return handle;
            }
        }

        internal sealed class CursorHandle : WpfSafeHandle
        {
            /// <SecurityNote>
            /// Critical: This code calls into a base class which is protected by a SecurityCritical constructor.
            /// </SecurityNote>
            [SecurityCritical]
            private CursorHandle() : base(true, NativeMethods.CommonHandles.Cursor)
            {
            }

            /// <SecurityNote>
            ///     Critical: This calls into DestroyCursor
            /// </SecurityNote>
            [SecurityCritical]
            [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
            protected override bool ReleaseHandle()
            {
                return UnsafeNativeMethods.DestroyCursor( handle );
            }

            /// <SecurityNote>
            ///     Critical: This creates a new SafeHandle, which has a critical constructor.
            ///     TreatAsSafe: The handle this creates is invalid.  It contains no critical data.
            /// </SecurityNote>
            [SecurityCritical, SecurityTreatAsSafe]
            internal static CursorHandle GetInvalidCursor()
            {
                return new CursorHandle();
            }
        }

        public static int SignedHIWORD(IntPtr intPtr)
        {
            return SignedHIWORD(IntPtrToInt32(intPtr));
        }

        public static int SignedLOWORD(IntPtr intPtr)
        {
            return SignedLOWORD(IntPtrToInt32(intPtr));
        }

        public const int SIZE_RESTORED = 0;
        public const int SIZE_MINIMIZED = 1;

        public const int WS_EX_NOACTIVATE = 0x08000000;
        public const int VK_LSHIFT = 0xA0;
        public const int VK_RMENU = 0xA5;
        public const int VK_LMENU = 0xA4;
        public const int VK_LCONTROL = 0xA2;
        public const int VK_RCONTROL = 0xA3;
        public const int VK_LBUTTON = 0x01;
        public const int VK_RBUTTON = 0x02;
        public const int VK_MBUTTON = 0x04;
        public const int VK_XBUTTON1 = 0x05;
        public const int VK_XBUTTON2 = 0x06;

        // We have this wrapper because casting IntPtr to int may
        // generate OverflowException when one of high 32 bits is set.
        public static int IntPtrToInt32(IntPtr intPtr)
        {
            return unchecked((int)intPtr.ToInt64());
        }

        public const int PM_QS_SENDMESSAGE = unchecked(QS_SENDMESSAGE << 16);
        public const int PM_QS_POSTMESSAGE = unchecked((QS_POSTMESSAGE | QS_HOTKEY | QS_TIMER) << 16);
        public const int MWMO_WAITALL = 0x0001;
        public const int MWMO_ALERTABLE = 0x0002;
        public const int MWMO_INPUTAVAILABLE = 0x0004;

        public static IntPtr HWND_MESSAGE = new IntPtr(-3);


        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public class WNDCLASSEX_I
        {
            public int cbSize = 0;

            public int style = 0;

            public IntPtr lpfnWndProc = IntPtr.Zero;

            public int cbClsExtra = 0;

            public int cbWndExtra = 0;

            public IntPtr hInstance = IntPtr.Zero;

            public IntPtr hIcon = IntPtr.Zero;

            public IntPtr hCursor = IntPtr.Zero;

            public IntPtr hbrBackground = IntPtr.Zero;

            public IntPtr lpszMenuName = IntPtr.Zero;

            public IntPtr lpszClassName = IntPtr.Zero;

            public IntPtr hIconSm = IntPtr.Zero;
        }

        // NOTE:  this replaces the RECT struct in NativeMethodsCLR.cs because it adds an extra method IsEmpty
        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;

            public RECT(int left, int top, int right, int bottom)
            {
                this.left = left;
                this.top = top;
                this.right = right;
                this.bottom = bottom;
            }

            public int Width
            {
                get { return right - left; }
            }

            public int Height
            {
                get { return bottom - top; }
            }

            public void Offset(int dx, int dy)
            {
                left += dx;
                top += dy;
                right += dx;
                bottom += dy;
            }

            public bool IsEmpty
            {
                get
                {
                    return left >= right || top >= bottom;
                }
            }
        }

        // Provided for interop scenarios that require passing a NULL RECT*.
        [StructLayout(LayoutKind.Sequential)]
        internal class RefRECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;

            public RefRECT()
            {}

            public RefRECT(int left, int top, int right, int bottom)
            {
                this.left = left;
                this.top = top;
                this.right = right;
                this.bottom = bottom;
            }

            public int Width
            {
                get { return right - left; }
            }

            public int Height
            {
                get { return bottom - top; }
            }

            public void Offset(int dx, int dy)
            {
                left += dx;
                top += dy;
                right += dx;
                bottom += dy;
            }

            public bool IsEmpty
            {
                get
                {
                    return left >= right || top >= bottom;
                }
            }
        }

        // NOTE:  this replaces the struct in NativeMethodsCLR.cs because it adds some additonal methods
        [StructLayout(LayoutKind.Sequential, Pack = 2)]
        public struct BITMAPINFO
        {
            // bmiHeader was a by-value BITMAPINFOHEADER structure
            public int bmiHeader_biSize;  // ndirect.DllLib.sizeOf( BITMAPINFOHEADER.class );

            public int bmiHeader_biWidth;

            public int bmiHeader_biHeight;

            public short bmiHeader_biPlanes;

            public short bmiHeader_biBitCount;

            public int bmiHeader_biCompression;

            public int bmiHeader_biSizeImage;

            public int bmiHeader_biXPelsPerMeter;

            public int bmiHeader_biYPelsPerMeter;

            public int bmiHeader_biClrUsed;

            public int bmiHeader_biClrImportant;


            // hamidm -- 03/08/2006
            // if the following RGBQUAD struct is added in this struct,
            // we need to update bmiHeader_biSize in the cctor to hard-coded 40
            // since it expects the size of the BITMAPINFOHEADER only
            //
            // bmiColors was an embedded array of RGBQUAD structures
            // public byte     bmiColors_rgbBlue = 0;
            // public byte     bmiColors_rgbGreen = 0;
            // public byte     bmiColors_rgbRed = 0;
            // public byte     bmiColors_rgbReserved = 0;
            public BITMAPINFO(int width, int height, short bpp)
            {
                bmiHeader_biSize = SizeOf();
                bmiHeader_biWidth = width;
                bmiHeader_biHeight = height;
                bmiHeader_biPlanes = 1;
                bmiHeader_biBitCount = bpp;
                bmiHeader_biCompression = 0;
                bmiHeader_biSizeImage = 0;
                bmiHeader_biXPelsPerMeter = 0;
                bmiHeader_biYPelsPerMeter = 0;
                bmiHeader_biClrUsed = 0;
                bmiHeader_biClrImportant = 0;
            }

            /// <SecurityNote>
            ///  Critical : Calls critical Marshal.SizeOf
            ///  Safe     : Calls method with trusted input (well known safe type)
            /// </SecurityNote>
            [SecuritySafeCritical]
            private static int SizeOf()
            {
                return Marshal.SizeOf(typeof(BITMAPINFO));
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        internal class SECURITY_ATTRIBUTES
        {
            /// <SecurityNote>
            /// Critical : Initializes critical SafeHandle field
            /// Safe     : Initializes handle to known safe value
            /// </SecurityNote>
            [SecuritySafeCritical]
            public SECURITY_ATTRIBUTES ()
            {
                lpSecurityDescriptor = new SafeLocalMemHandle();
            }

            public int nLength = SizeOf();

            /// <SecurityNote>
            /// Critical : Exposes critical SafeHandle
            /// </SecurityNote>
            [SecurityCritical]
            public SafeLocalMemHandle lpSecurityDescriptor = new SafeLocalMemHandle();

            public bool bInheritHandle = false;

            /// <SecurityNote>
            /// Critical : Disposes critical lpSecurityDescriptor field
            /// </SecurityNote>
            [SecurityCritical]
            public void Release()
            {
                if (lpSecurityDescriptor != null)
                {
                    lpSecurityDescriptor.Dispose();
                    
                    // we do not set the handle to null because .Net marshaling will throw an exception if we pinvoke with a structure containing a null SafeHandle field
                    lpSecurityDescriptor = new SafeLocalMemHandle();
                }
            }
            
            /// <SecurityNote>
            ///  Critical : Calls critical Marshal.SizeOf
            ///  Safe     : Calls method with trusted input (well known safe type)
            /// </SecurityNote>
            [SecuritySafeCritical]
            private static int SizeOf()
            {
                return Marshal.SizeOf(typeof(SECURITY_ATTRIBUTES));
            }
        }

        /// <SecurityNote>
        ///  Critical: Inherits from critical tyoe SafeHandleZeroOrMinusOneIsInvalid
        /// </SecurityNote>
        [SecurityCritical]
		[SuppressUnmanagedCodeSecurity]
		[HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort=true)]
		internal sealed class SafeLocalMemHandle : SafeHandleZeroOrMinusOneIsInvalid
		{
            /// <SecurityNote>
            ///  Critical: Calls critical SafeHandle ctor
            /// </SecurityNote>
            [SecurityCritical]
		    public SafeLocalMemHandle() : base(true)
		    {
		    }

            /// <SecurityNote>
            ///  Critical: Calls critical SafeHandle.SetHandle
            /// </SecurityNote>
            [SecurityCritical]
		    public SafeLocalMemHandle(IntPtr existingHandle, bool ownsHandle) : base(ownsHandle)
		    {
		        base.SetHandle(existingHandle);
		    }

            /// <SecurityNote>
            ///  Critical: Calls critical LocalFree
            /// </SecurityNote>
            [SecurityCritical]
		    protected override bool ReleaseHandle()
		    {
		        return (LocalFree(base.handle) == IntPtr.Zero);
		    }

            /// <SecurityNote>
            ///  Critical: Elevates to unmanaged code permissions
            /// </SecurityNote>
            [SecurityCritical]
            [SuppressUnmanagedCodeSecurity]
		    [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
            [DllImport("kernel32.dll")]
		    private static extern IntPtr LocalFree(IntPtr hMem);
		}


        internal const uint DELETE = 0x00010000, READ_CONTROL = 0x00020000, WRITE_DAC = 0x00040000, WRITE_OWNER = 0x00080000, SYNCHRONIZE = 0x00100000, STANDARD_RIGHTS_REQUIRED = 0x000F0000, STANDARD_RIGHTS_READ = READ_CONTROL, STANDARD_RIGHTS_WRITE = READ_CONTROL, STANDARD_RIGHTS_EXECUTE = READ_CONTROL, STANDARD_RIGHTS_ALL = 0x001F0000, SPECIFIC_RIGHTS_ALL = 0x0000FFFF, ACCESS_SYSTEM_SECURITY = 0x01000000, MAXIMUM_ALLOWED = 0x02000000, GENERIC_READ = 0x80000000, GENERIC_WRITE = 0x40000000, GENERIC_EXECUTE = 0x20000000, GENERIC_ALL = 0x10000000;

        internal const uint FILE_READ_DATA = 0x0001,    // file & pipe
            FILE_LIST_DIRECTORY = 0x0001,    // directory
            FILE_WRITE_DATA = 0x0002,    // file & pipe
            FILE_ADD_FILE = 0x0002,    // directory
            FILE_APPEND_DATA = 0x0004,    // file
            FILE_ADD_SUBDIRECTORY = 0x0004,    // directory
            FILE_CREATE_PIPE_INSTANCE = 0x0004,    // named pipe
            FILE_READ_EA = 0x0008,    // file & directory
            FILE_WRITE_EA = 0x0010,    // file & directory
            FILE_EXECUTE = 0x0020,    // file
            FILE_TRAVERSE = 0x0020,    // directory
            FILE_DELETE_CHILD = 0x0040,    // directory
            FILE_READ_ATTRIBUTES = 0x0080,    // all
            FILE_WRITE_ATTRIBUTES = 0x0100;    // all

        internal const uint FILE_ALL_ACCESS = STANDARD_RIGHTS_REQUIRED | SYNCHRONIZE | 0x1FF, FILE_GENERIC_READ = STANDARD_RIGHTS_READ | FILE_READ_DATA | FILE_READ_ATTRIBUTES | FILE_READ_EA | SYNCHRONIZE, FILE_GENERIC_WRITE = STANDARD_RIGHTS_WRITE | FILE_WRITE_DATA | FILE_WRITE_ATTRIBUTES | FILE_WRITE_EA | FILE_APPEND_DATA | SYNCHRONIZE, FILE_GENERIC_EXECUTE = STANDARD_RIGHTS_EXECUTE | FILE_READ_ATTRIBUTES | FILE_EXECUTE | SYNCHRONIZE;

        internal const uint FILE_SHARE_READ = 0x00000001, FILE_SHARE_WRITE = 0x00000002, FILE_SHARE_DELETE = 0x00000004;

        internal const int ERROR_ALREADY_EXISTS = 183;

        internal const int OPEN_EXISTING = 3;

        internal const int PAGE_READONLY = 0x02;

        internal const int SECTION_MAP_READ = 0x0004;

        internal const int FILE_ATTRIBUTE_NORMAL = 0x00000080;
        internal const int FILE_ATTRIBUTE_TEMPORARY = 0x00000100;
        internal const int FILE_FLAG_DELETE_ON_CLOSE = 0x04000000;

        internal const int CREATE_ALWAYS   = 2;

        internal const int PROCESS_ALL_ACCESS = (int)(STANDARD_RIGHTS_REQUIRED | SYNCHRONIZE | 0xFFF);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public class WNDCLASSEX_D
        {
            public int cbSize = 0;

            public int style = 0;

            public WndProc lpfnWndProc = null;

            public int cbClsExtra = 0;

            public int cbWndExtra = 0;

            public IntPtr hInstance = IntPtr.Zero;

            public IntPtr hIcon = IntPtr.Zero;

            public IntPtr hCursor = IntPtr.Zero;

            public IntPtr hbrBackground = IntPtr.Zero;

            public string lpszMenuName = null;

            public string lpszClassName = null;

            public IntPtr hIconSm = IntPtr.Zero;
        }


#if BASE_NATIVEMETHODS

        public const int QS_EVENT = 0x2000;

        public const int VK_CANCEL = 0x03;


        public const int VK_BACK = 0x08;

        public const int VK_CLEAR = 0x0C;

        public const int VK_RETURN = 0x0D;

        public const int VK_PAUSE = 0x13;

        public const int VK_CAPITAL = 0x14;

        public const int VK_KANA = 0x15;

        public const int VK_HANGEUL = 0x15;

        public const int VK_HANGUL = 0x15;

        public const int VK_JUNJA = 0x17;

        public const int VK_FINAL = 0x18;

        public const int VK_HANJA = 0x19;

        public const int VK_KANJI = 0x19;

        public const int VK_ESCAPE = 0x1B;

        public const int VK_CONVERT = 0x1C;

        public const int VK_NONCONVERT = 0x1D;

        public const int VK_ACCEPT = 0x1E;

        public const int VK_MODECHANGE = 0x1F;

        public const int VK_SPACE = 0x20;

        public const int VK_PRIOR = 0x21;

        public const int VK_NEXT = 0x22;

        public const int VK_END = 0x23;

        public const int VK_HOME = 0x24;

        public const int VK_LEFT = 0x25;

        public const int VK_UP = 0x26;

        public const int VK_RIGHT = 0x27;

        public const int VK_DOWN = 0x28;

        public const int VK_SELECT = 0x29;

        public const int VK_PRINT = 0x2A;

        public const int VK_EXECUTE = 0x2B;

        public const int VK_SNAPSHOT = 0x2C;

        public const int VK_INSERT = 0x2D;

        public const int VK_DELETE = 0x2E;

        public const int VK_HELP = 0x2F;

        public const int VK_0 = 0x30;

        public const int VK_1 = 0x31;

        public const int VK_2 = 0x32;

        public const int VK_3 = 0x33;

        public const int VK_4 = 0x34;

        public const int VK_5 = 0x35;

        public const int VK_6 = 0x36;

        public const int VK_7 = 0x37;

        public const int VK_8 = 0x38;

        public const int VK_9 = 0x39;

        public const int VK_A = 0x41;

        public const int VK_B = 0x42;

        public const int VK_C = 0x43;

        public const int VK_D = 0x44;

        public const int VK_E = 0x45;

        public const int VK_F = 0x46;

        public const int VK_G = 0x47;

        public const int VK_H = 0x48;

        public const int VK_I = 0x49;

        public const int VK_J = 0x4A;

        public const int VK_K = 0x4B;

        public const int VK_L = 0x4C;

        public const int VK_M = 0x4D;

        public const int VK_N = 0x4E;

        public const int VK_O = 0x4F;

        public const int VK_P = 0x50;

        public const int VK_Q = 0x51;

        public const int VK_R = 0x52;

        public const int VK_S = 0x53;

        public const int VK_T = 0x54;

        public const int VK_U = 0x55;

        public const int VK_V = 0x56;

        public const int VK_W = 0x57;

        public const int VK_X = 0x58;

        public const int VK_Y = 0x59;

        public const int VK_Z = 0x5A;

        public const int VK_LWIN = 0x5B;

        public const int VK_RWIN = 0x5C;

        public const int VK_APPS = 0x5D;

        public const int VK_POWER = 0x5E;

        public const int VK_SLEEP = 0x5F;

        public const int VK_NUMPAD0 = 0x60;

        public const int VK_NUMPAD1 = 0x61;

        public const int VK_NUMPAD2 = 0x62;

        public const int VK_NUMPAD3 = 0x63;

        public const int VK_NUMPAD4 = 0x64;

        public const int VK_NUMPAD5 = 0x65;

        public const int VK_NUMPAD6 = 0x66;

        public const int VK_NUMPAD7 = 0x67;

        public const int VK_NUMPAD8 = 0x68;

        public const int VK_NUMPAD9 = 0x69;

        public const int VK_MULTIPLY = 0x6A;

        public const int VK_ADD = 0x6B;

        public const int VK_SEPARATOR = 0x6C;

        public const int VK_SUBTRACT = 0x6D;

        public const int VK_DECIMAL = 0x6E;

        public const int VK_DIVIDE = 0x6F;

        public const int VK_F1 = 0x70;

        public const int VK_F2 = 0x71;

        public const int VK_F3 = 0x72;

        public const int VK_F4 = 0x73;

        public const int VK_F5 = 0x74;

        public const int VK_F6 = 0x75;

        public const int VK_F7 = 0x76;

        public const int VK_F8 = 0x77;

        public const int VK_F9 = 0x78;

        public const int VK_F10 = 0x79;

        public const int VK_F11 = 0x7A;

        public const int VK_F12 = 0x7B;

        public const int VK_F13 = 0x7C;

        public const int VK_F14 = 0x7D;

        public const int VK_F15 = 0x7E;

        public const int VK_F16 = 0x7F;

        public const int VK_F17 = 0x80;

        public const int VK_F18 = 0x81;

        public const int VK_F19 = 0x82;

        public const int VK_F20 = 0x83;

        public const int VK_F21 = 0x84;

        public const int VK_F22 = 0x85;

        public const int VK_F23 = 0x86;

        public const int VK_F24 = 0x87;

        public const int VK_NUMLOCK = 0x90;

        public const int VK_SCROLL = 0x91;


        public const int VK_RSHIFT = 0xA1;

        public const int VK_BROWSER_BACK = 0xA6;

        public const int VK_BROWSER_FORWARD = 0xA7;

        public const int VK_BROWSER_REFRESH = 0xA8;

        public const int VK_BROWSER_STOP = 0xA9;

        public const int VK_BROWSER_SEARCH = 0xAA;

        public const int VK_BROWSER_FAVORITES = 0xAB;

        public const int VK_BROWSER_HOME = 0xAC;

        public const int VK_VOLUME_MUTE = 0xAD;

        public const int VK_VOLUME_DOWN = 0xAE;

        public const int VK_VOLUME_UP = 0xAF;

        public const int VK_MEDIA_NEXT_TRACK = 0xB0;

        public const int VK_MEDIA_PREV_TRACK = 0xB1;

        public const int VK_MEDIA_STOP = 0xB2;

        public const int VK_MEDIA_PLAY_PAUSE = 0xB3;

        public const int VK_LAUNCH_MAIL = 0xB4;

        public const int VK_LAUNCH_MEDIA_SELECT = 0xB5;

        public const int VK_LAUNCH_APP1 = 0xB6;

        public const int VK_LAUNCH_APP2 = 0xB7;

        public const int VK_PROCESSKEY = 0xE5;

        public const int VK_PACKET = 0xE7;

        public const int VK_ATTN = 0xF6;

        public const int VK_CRSEL = 0xF7;

        public const int VK_EXSEL = 0xF8;

        public const int VK_EREOF = 0xF9;

        public const int VK_PLAY = 0xFA;

        public const int VK_ZOOM = 0xFB;

        public const int VK_NONAME = 0xFC;

        public const int VK_PA1 = 0xFD;

        public const int VK_OEM_CLEAR = 0xFE;
#endif

        /////////////////////
        // from Framework
        internal const int ENDSESSION_LOGOFF = (unchecked((int)0x80000000));

        internal const int
        ERROR_SUCCESS = 0;

        public const int LOCALE_FONTSIGNATURE = 0x00000058;

        public const int
            SWP_NOREDRAW = 0x0008,
            SWP_FRAMECHANGED = 0x0020,  // The frame changed: send WM_NCCALCSIZE
            SWP_NOCOPYBITS = 0x0100,
            SWP_NOOWNERZORDER = 0x0200,  // Don't do owner Z ordering
            SWP_NOSENDCHANGING = 0x0400,  // Don't send WM_WINDOWPOSCHANGING
            SWP_NOREPOSITION = SWP_NOOWNERZORDER,
            SWP_DEFERERASE = 0x2000,
            SWP_ASYNCWINDOWPOS = 0x4000,
            SPI_GETCURSORSHADOW = 0x101A,
            SPI_SETCURSORSHADOW = 0x101B,
            SPI_GETFOCUSBORDERWIDTH = 0x200E,
            SPI_SETFOCUSBORDERWIDTH = 0x200F,
            SPI_GETFOCUSBORDERHEIGHT = 0x2010,
            SPI_SETFOCUSBORDERHEIGHT = 0x2011,
            SPI_GETSTYLUSHOTTRACKING = 0x1010,
            SPI_SETSTYLUSHOTTRACKING = 0x1011,
            SPI_GETTOOLTIPFADE = 0x1018,
            SPI_SETTOOLTIPFADE = 0x1019,
            SPI_GETFOREGROUNDFLASHCOUNT = 0x2004,
            SPI_SETFOREGROUNDFLASHCOUNT = 0x2005,
            SPI_SETCARETWIDTH = 0x2007,
            SPI_SETMOUSEVANISH = 0x1021,
            SPI_SETHIGHCONTRAST = 0x0043,
            SPI_SETKEYBOARDPREF = 0x0045,
            SPI_SETFLATMENU = 0x1023,
            SPI_SETDROPSHADOW = 0x1025,
            SPI_SETWORKAREA = 0x002F,
            SPI_SETICONMETRICS = 0x002E,
            SPI_SETDRAGWIDTH = 0x004C,
            SPI_SETDRAGHEIGHT = 0x004D,
            SPI_SETPENWINDOWS = 0x0031,
            SPI_SETMOUSEBUTTONSWAP = 0x0021,
            SPI_SETSHOWSOUNDS = 0x0039,
            SPI_SETKEYBOARDCUES = 0x100B,
            SPI_SETKEYBOARDDELAY = 0x0017,
            SPI_SETSNAPTODEFBUTTON = 0x0060,
            SPI_SETWHEELSCROLLLINES = 0x0069,
            SPI_SETMOUSEHOVERWIDTH = 0x0063,
            SPI_SETMOUSEHOVERHEIGHT = 0x0065,
            SPI_SETMOUSEHOVERTIME = 0x0067,
            SPI_SETMENUDROPALIGNMENT = 0x001C,
            SPI_SETMENUFADE = 0x1013,
            SPI_SETMENUSHOWDELAY = 0x006B,
            SPI_SETCOMBOBOXANIMATION = 0x1005,
            SPI_SETCLIENTAREAANIMATION = 0x1043,
            SPI_SETGRADIENTCAPTIONS = 0x1009,
            SPI_SETHOTTRACKING = 0x100F,
            SPI_SETLISTBOXSMOOTHSCROLLING = 0x1007,
            SPI_SETMENUANIMATION = 0x1003,
            SPI_SETSELECTIONFADE = 0x1015,
            SPI_SETTOOLTIPANIMATION = 0x1017,
            SPI_SETUIEFFECTS = 0x103F,
            SPI_SETANIMATION = 0x0049,
            SPI_SETDRAGFULLWINDOWS = 0x0025,
            SPI_SETBORDER = 0x0006,
            SPI_SETNONCLIENTMETRICS = 0x002A;

        public const int LANG_KOREAN = 0x12;

#if NEVER
        public static int PRIMARYLANGID(int lgid)
        {
            return ((ushort)(lgid) & 0x3ff);
        }
#endif

        public const int
            MB_YESNO = 0x00000004,
            MB_SYSTEMMODAL = 0x00001000,
            IDYES = 6;

        public const int PM_QS_INPUT = unchecked(QS_INPUT << 16);
        public const int PM_QS_PAINT = unchecked(QS_PAINT << 16);


        public const int
        SW_PARENTCLOSING = 1,
        SW_PARENTOPENING = 3,
        SC_MOUSEMOVE = SC_MOVE + 0x02,
        SPI_SETKEYBOARDSPEED = 0x000B;

        internal const int TYMED_HGLOBAL = 1;
        internal const int TYMED_FILE = 2;
        internal const int TYMED_ISTREAM = 4;
        internal const int TYMED_ISTORAGE = 8;
        internal const int TYMED_GDI = 16;
        internal const int TYMED_MFPICT = 32;
        internal const int TYMED_ENHMF = 64;


        public const int WS_OVERLAPPEDWINDOW = (WS_OVERLAPPED | WS_CAPTION | WS_SYSMENU | WS_THICKFRAME | WS_MINIMIZEBOX | WS_MAXIMIZEBOX);

        public const int KEYEVENTF_EXTENDEDKEY = 0x0001;
        public const int KEYEVENTF_KEYUP = 0x0002;
        public const int KEYEVENTF_UNICODE = 0x0004;
        public const int KEYEVENTF_SCANCODE = 0x0008;

        public const int MOUSEEVENTF_MOVE = 0x0001;
        public const int MOUSEEVENTF_LEFTDOWN = 0x0002;
        public const int MOUSEEVENTF_LEFTUP = 0x0004;
        public const int MOUSEEVENTF_RIGHTDOWN = 0x0008;
        public const int MOUSEEVENTF_RIGHTUP = 0x0010;
        public const int MOUSEEVENTF_MIDDLEDOWN = 0x0020;
        public const int MOUSEEVENTF_MIDDLEUP = 0x0040;
        public const int MOUSEEVENTF_XDOWN = 0x0080;
        public const int MOUSEEVENTF_XUP = 0x0100;
        public const int MOUSEEVENTF_WHEEL = 0x00800;
        public const int MOUSEEVENTF_VIRTUALDESK = 0x04000;
        public const int MOUSEEVENTF_ABSOLUTE = 0x08000;
        public const int MOUSEEVENTF_ACTUAL = 0x10000;

        public const int GWL_HINSTANCE = -6;
        public const int GWL_USERDATA = -21;
        public const int GCL_MENUNAME = -8;
        public const int GCL_HBRBACKGROUND = -10;
        public const int GCL_HCURSOR = -12;
        public const int GCL_HICON = -14;
        public const int GCL_HMODULE = -16;
        public const int GCL_CBWNDEXTRA = -18;
        public const int GCL_CBCLSEXTRA = -20;
        public const int GCL_STYLE = -26;
        public const int GCW_ATOM = -32;
        public const int GCL_HICONSM = -34;

        public const int MONITOR_DEFAULTTONULL       = 0x00000000;
        public const int MONITOR_DEFAULTTOPRIMARY    = 0x00000001;
        public const int MONITOR_DEFAULTTONEAREST = 0x00000002;


        [StructLayout(LayoutKind.Sequential)]
        public class ANIMATIONINFO
        {
            public int cbSize = SizeOf();
            public int iMinAnimate = 0;
            
            /// <SecurityNote>
            ///  Critical : Calls critical Marshal.SizeOf
            ///  Safe     : Calls method with trusted input (well known safe type)
            /// </SecurityNote>
            [SecuritySafeCritical]
            private static int SizeOf()
            {
                return Marshal.SizeOf(typeof(ANIMATIONINFO));
            }
        }



        [StructLayout(LayoutKind.Sequential)]
        public sealed class STATDATA
        {
            [MarshalAs(UnmanagedType.U4)]
            public int advf = 0;
            [MarshalAs(UnmanagedType.U4)]
            public int dwConnection = 0;
        }

        public enum WINDOWTHEMEATTRIBUTETYPE
        {
            WTA_NONCLIENT = 1
        };

        public const uint WTNCA_NODRAWCAPTION = 0x00000001;   // don't draw the window caption
        public const uint WTNCA_NODRAWICON = 0x00000002;   // don't draw the system icon
        public const uint WTNCA_NOSYSMENU = 0x00000004;   // don't expose the system menu icon functionality
        public const uint WTNCA_VALIDBITS = (WTNCA_NODRAWCAPTION | WTNCA_NODRAWICON | WTNCA_NOSYSMENU);

#if WCP_SYSTEM_THEMES_ENABLED
        [StructLayout(LayoutKind.Sequential)]
        public class WTA_OPTIONS
        {
            public uint dwFlags = 0;
            public uint dwMask = 0;
        };
#endif // WCP_SYSTEM_THEMES_ENABLED


        internal const int NO_ERROR = 0;


        ///////////////////////////
        // Used by BASE

        public const int VK_OEM_1 = 0xBA;
        public const int VK_OEM_PLUS = 0xBB;
        public const int VK_OEM_COMMA = 0xBC;
        public const int VK_OEM_MINUS = 0xBD;
        public const int VK_OEM_PERIOD = 0xBE;
        public const int VK_OEM_2 = 0xBF;
        public const int VK_OEM_3 = 0xC0;
        public const int VK_C1 = 0xC1;   // Brazilian ABNT_C1 key (not defined in winuser.h).
        public const int VK_C2 = 0xC2;   // Brazilian ABNT_C2 key (not defined in winuser.h).
        public const int VK_OEM_4 = 0xDB;
        public const int VK_OEM_5 = 0xDC;
        public const int VK_OEM_6 = 0xDD;
        public const int VK_OEM_7 = 0xDE;
        public const int VK_OEM_8 = 0xDF;
        public const int VK_OEM_AX = 0xE1;
        public const int VK_OEM_102 = 0xE2;
        public const int VK_OEM_RESET = 0xE9;
        public const int VK_OEM_JUMP = 0xEA;
        public const int VK_OEM_PA1 = 0xEB;
        public const int VK_OEM_PA2 = 0xEC;
        public const int VK_OEM_PA3 = 0xED;
        public const int VK_OEM_WSCTRL = 0xEE;
        public const int VK_OEM_CUSEL = 0xEF;
        public const int VK_OEM_ATTN = 0xF0;
        public const int VK_OEM_FINISH = 0xF1;
        public const int VK_OEM_COPY = 0xF2;
        public const int VK_OEM_AUTO = 0xF3;
        public const int VK_OEM_ENLW = 0xF4;
        public const int VK_OEM_BACKTAB = 0xF5;

        ////////////////////////////
        // Needed by BASE
#if BASE_NATIVEMETHODS
        /// <summary>
        /// HWND.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct HWND
        {
            /// <summary>
            ///
            /// </summary>
            public IntPtr h;

            /// <summary>
            ///
            /// </summary>
            /// <param name="h"></param>
            /// <returns></returns>
            public static HWND Cast(IntPtr h)
            {
                HWND hTemp = new HWND();
                hTemp.h = h;
                return hTemp;
            }

            public HandleRef MakeHandleRef(object wrapper)
            {
                return new HandleRef(wrapper,h);
            }

            /// <summary>
            ///
            /// </summary>
            /// <param name="h"></param>
            /// <returns></returns>
            public static implicit operator IntPtr(HWND h)
            {
                return h.h;
            }

            /// <summary>
            ///
            /// </summary>
            /// <param name="hl"></param>
            /// <param name="hr"></param>
            /// <returns></returns>
            public static bool operator ==(HWND hl, HWND hr)
            {
                return (hl.h == hr.h);
            }

            /// <summary>
            ///
            /// </summary>
            /// <param name="hl"></param>
            /// <param name="hr"></param>
            /// <returns></returns>
            public static bool operator !=(HWND hl, HWND hr)
            {
                return (hl.h != hr.h);
            }

            /// <summary>
            ///
            /// </summary>
            /// <param name="oCompare"></param>
            /// <returns></returns>
            override public bool Equals(object oCompare)
            {
                HWND hr = Cast((HWND)oCompare);
                return (h == hr.h);
            }

            /// <summary>
            ///
            /// </summary>
            /// <returns></returns>
            public override int GetHashCode()
            {
                return (int)h;
            }
        }

        /// <summary>
        /// HDC.
        /// </summary>
        public struct HDC
        {
            /// <summary>
            ///
            /// </summary>
            public IntPtr h;

            /// <summary>
            ///
            /// </summary>
            /// <param name="h"></param>
            /// <returns></returns>
            public static HDC Cast(IntPtr h)
            {
                HDC hTemp = new HDC();
                hTemp.h = h;
                return hTemp;
            }

            public HandleRef MakeHandleRef( object wrapper)
            {
                return new HandleRef(wrapper, h);
            }

            /// <summary>
            ///
            /// </summary>
            public static HDC NULL
            {
                get
                {
                    HDC hTemp = new HDC();
                    hTemp.h = IntPtr.Zero;
                    return hTemp;
                }
            }
        }

        public const int DRAGDROP_S_DROP = 0x00040100;
        public const int DRAGDROP_S_CANCEL = 0x00040101;
        public const int DRAGDROP_S_USEDEFAULTCURSORS = 0x00040102;

        public const int TME_CANCEL = (unchecked((int)0x80000000));
        public const int IDC_HAND = 32649;

        /// <summary>
        /// End document printing
        /// </summary>
        /// <param name="hdc">Printer DC</param>
        /// <returns>More than 0 if succeeds, zero or less if fails</returns>
        /// <SecurityNote>
        ///  Critical: Elevates to unmanaged code permissions
        /// </SecurityNote>
        [SecurityCritical]
        [DllImport("gdi32.dll")]
        public static extern Int32 EndDoc(HDC hdc);

        public const int DM_ORIENTATION = 0x00000001;
        public const int DM_PAPERSIZE = 0x00000002;
        public const int DM_PAPERLENGTH = 0x00000004;
        public const int DM_PAPERWIDTH = 0x00000008;
        public const int DM_PRINTQUALITY = 0x00000400;
        public const int DM_YRESOLUTION = 0x00002000;

        /// <summary>
        /// Escape description for ExtEscape
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public unsafe struct PrinterEscape
        {
            public Int32 cbInput;
            public UInt32 cbOutput;
            public UInt32 opcode;
            public Int32 cbSize;
            
            /// <SecurityNote>
            ///  Critical: Exposes native pointer
            /// </SecurityNote>
            [SecurityCritical]
            public void* buffer;
        }

        /// <summary>
        /// Send Escape to DC (printer)
        /// </summary>
        /// <param name="hdc">Printer DC</param>
        /// <param name="nEscape">Escape code</param>
        /// <param name="cbInput"># bytes in lpvInData</param>
        /// <param name="lpvInData">Input data</param>
        /// <param name="cbOutput">size of lpvOutData in bytes</param>
        /// <param name="lpvOutData">Structure to receive data</param>
        /// <returns>0 if escape not implemented, negative if error, otherwise succeeds</returns>
        /// <SecurityNote>
        ///  Critical: Elevates to unmanaged code permissions
        /// </SecurityNote>
        [SecurityCritical]
        [DllImport("gdi32.dll")]
        public static unsafe extern Int32 ExtEscape(HDC hdc, Int32 nEscape, Int32 cbInput, PrinterEscape* lpvInData, Int32 cbOutput, [Out] void* lpvOutData);

        public const int MM_ISOTROPIC = 7;


        public const int DM_OUT_BUFFER = 2;


        /// <summary>
        /// Document info for printing
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public unsafe struct DocInfo
        {// (*)indicates must be specfied
            internal Int32 cbSize;                     // (*)size of this structure (20)
            internal String lpszName;                   // (*)Name of document
            internal String lpszOutput;                 // Name of output file (null)
            internal String lpszDatatype;               // Type of data ("raw" or "emf") can be null
            internal Int32 fwType;                     // Flags about print job (0)
        }

        /// <summary>
        /// Start document printing
        /// </summary>
        /// <param name="hdc">Printer DC</param>
        /// <param name="docInfo">Document information</param>
        /// <returns>More than zero if succeeded</returns>
        /// <SecurityNote>
        ///  Critical: Elevates to unmanaged code permissions
        /// </SecurityNote>
        [SecurityCritical]
        [DllImport("gdi32.dll")]
        public unsafe static extern Int32 StartDoc(HDC hdc, ref DocInfo docInfo);

        /// <summary>
        ///
        /// </summary>
        /// <param name="printerName"></param>
        /// <param name="phPrinter"></param>
        /// <param name="pDefaults"></param>
        /// <returns></returns>
        /// <SecurityNote>
        ///  Critical: Elevates to unmanaged code permissions
        /// </SecurityNote>
        [SecurityCritical]
        [DllImport("winspool.drv", BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public unsafe static extern Int32 OpenPrinterA(String printerName, IntPtr* phPrinter, void* pDefaults);

        /// <summary>
        ///
        /// </summary>
        /// <param name="hPrinter"></param>
        /// <returns></returns>
        /// <SecurityNote>
        ///  Critical: Elevates to unmanaged code permissions
        /// </SecurityNote>
        [SecurityCritical]
        [DllImport("winspool.drv")]//CASRemoval:
        public static extern Int32 ClosePrinter(IntPtr hPrinter);

        /// <summary>
        /// End document page
        /// </summary>
        /// <param name="hdc">Printer DC</param>
        /// <returns>More than 0 if succeeds, zero or less if fails</returns>
        /// <SecurityNote>
        ///  Critical: Elevates to unmanaged code permissions
        /// </SecurityNote>
        [SecurityCritical]
        [DllImport("gdi32.dll")]//CASRemoval:
        public static extern Int32 EndPage(HDC hdc);

        /// <summary>
        /// Start document page
        /// </summary>
        /// <param name="hdc">Printer DC</param>
        /// <returns>More than 0 if succeeds, zero or less if fails</returns>
        /// <SecurityNote>
        ///  Critical: Elevates to unmanaged code permissions
        /// </SecurityNote>
        [SecurityCritical]
        [DllImport("gdi32.dll")]//CASRemoval:
        public static extern Int32 StartPage(HDC hdc);

        /// <summary>Win32 constants</summary>
        public const int E_HANDLE = unchecked((int)0x80070006);

        /// <summary>wParam for WM_SETTINGCHANGE</summary>
        public const int SPI_SETFONTSMOOTHING = 0x004B;
        /// <summary>wParam for WM_SETTINGCHANGE</summary>
        public const int SPI_SETFONTSMOOTHINGTYPE = 0x200B;
        /// <summary>wParam for WM_SETTINGCHANGE</summary>
        public const int SPI_SETFONTSMOOTHINGCONTRAST = 0x200D;
        /// <summary>wParam for WM_SETTINGCHANGE</summary>
        public const int SPI_SETFONTSMOOTHINGORIENTATION = 0x2013;
        /// <summary>wParam for WM_SETTINGCHANGE</summary>
        public const int SPI_SETDISPLAYPIXELSTRUCTURE = 0x2015;
        /// <summary>wParam for WM_SETTINGCHANGE</summary>
        public const int SPI_SETDISPLAYGAMMA = 0x2017;
        /// <summary>wParam for WM_SETTINGCHANGE</summary>
        public const int SPI_SETDISPLAYCLEARTYPELEVEL = 0x2019;
        /// <summary>wParam for WM_SETTINGCHANGE</summary>
        public const int SPI_SETDISPLAYTEXTCONTRASTLEVEL = 0x201b;

        public const int GMMP_USE_DISPLAY_POINTS = 1;
        public const int GMMP_USE_HIGH_RESOLUTION_POINTS = 2;

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)] // For GetMouseMovePointsEx
        public struct MOUSEMOVEPOINT {
            public int    x ;                       //Specifies the x-coordinate of the mouse
            public int    y ;                       //Specifies the x-coordinate of the mouse
            public int    time ;                    //Specifies the time stamp of the mouse coordinate
            public IntPtr dwExtraInfo;              //Specifies extra information associated with this coordinate.
        }

        public const int ERROR_FILE_NOT_FOUND               = 2;
        public const int ERROR_PATH_NOT_FOUND               = 3;
        public const int ERROR_ACCESS_DENIED                = 5;
        public const int ERROR_INVALID_DRIVE                = 15;
        public const int ERROR_SHARING_VIOLATION            = 32;
        public const int ERROR_FILE_EXISTS                  = 80;
        public const int ERROR_INVALID_PARAMETER            = 87;
        public const int ERROR_FILENAME_EXCED_RANGE         = 206;
        public const int ERROR_NO_MORE_ITEMS                = 259;
        public const int ERROR_OPERATION_ABORTED            = 995;

#endif // BASE_NATIVEMETHODS


        public const int LR_DEFAULTCOLOR = 0x0000,
                         LR_MONOCHROME = 0x0001,
                         LR_COLOR = 0x0002,
                         LR_COPYRETURNORG = 0x0004,
                         LR_COPYDELETEORG = 0x0008,
                         LR_LOADFROMFILE = 0x0010,
                         LR_LOADTRANSPARENT = 0x0020,
                         LR_DEFAULTSIZE = 0x0040,
                         LR_VGACOLOR = 0x0080,
                         LR_LOADMAP3DCOLORS = 0x1000,
                         LR_CREATEDIBSECTION = 0x2000,
                         LR_COPYFROMRESOURCE = 0x4000,
                         LR_SHARED = unchecked((int)0x8000);

        internal enum Win32SystemColors
        {
            ActiveBorder = 0x0A,
            ActiveCaption = 0x02,
            ActiveCaptionText = 0x09,
            AppWorkspace = 0x0C,
            Control = 0x0F,
            ControlDark = 0x10,
            ControlDarkDark = 0x15,
            ControlLight = 0x16,
            ControlLightLight = 0x14,
            ControlText = 0x12,
            Desktop = 0x01,
            GradientActiveCaption = 0x1B,
            GradientInactiveCaption = 0x1C,
            GrayText = 0x11,
            Highlight = 0x0D,
            HighlightText = 0x0E,
            HotTrack = 0x1A,
            InactiveBorder = 0x0B,
            InactiveCaption = 0x03,
            InactiveCaptionText = 0x13,
            Info = 0x18,
            InfoText = 0x17,
            Menu = 0x04,
            MenuBar = 0x1E,
            MenuHighlight = 0x1D,
            MenuText = 0x07,
            ScrollBar = 0x00,
            Window = 0x05,
            WindowFrame = 0x06,
            WindowText = 0x08
        }

#if WINDOWS_BASE
        // Copied from winineti.h

        // Note: CachePath should be an array of size 260.
        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        internal struct InternetCacheConfigInfo
        {
            internal UInt32 dwStructSize;
            internal UInt32 dwContainer;
            internal UInt32 dwQuota;
            internal UInt32 dwReserved4;
            [MarshalAs(UnmanagedType.Bool)] internal bool fPerUser;
            internal UInt32 dwSyncMode;
            internal UInt32 dwNumCachePaths;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)] internal string CachePath;
            internal UInt32 dwCacheSize;
            internal UInt32 dwNormalUsage;
            internal UInt32 dwExemptUsage;
        }
#endif

        public const int WTS_CONSOLE_CONNECT    = 0x1;
        public const int WTS_CONSOLE_DISCONNECT = 0x2;
        public const int WTS_REMOTE_CONNECT     = 0x3;
        public const int WTS_REMOTE_DISCONNECT  = 0x4;
        public const int WTS_SESSION_LOCK       = 0x7;
        public const int WTS_SESSION_UNLOCK     = 0x8;
        
        public const uint NOTIFY_FOR_THIS_SESSION = 0;

        public const int PBT_APMSUSPEND         = 0x0004;
        public const int PBT_APMRESUMECRITICAL  = 0x0006;
        public const int PBT_APMRESUMESUSPEND   = 0x0007;
        public const int PBT_APMRESUMEAUTOMATIC = 0x0012;
        public const int PBT_POWERSETTINGCHANGE = 0x8013;

        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        public struct POWERBROADCAST_SETTING {
            public Guid PowerSetting;
            public int DataLength;
            public byte Data;
        }

        public static readonly Guid GUID_MONITOR_POWER_ON = new Guid(0x02731015, 0x4510, 0x4526, 0x99, 0xE6, 0xE5, 0xA1, 0x7E, 0xBD, 0x1A, 0xEA);



        public const uint PROFILE_READ = 1;

        //
        // <Windows Color System (WCS) types>
        //

        public enum ProfileType : uint
        {
            PROFILE_FILENAME = 1,
            PROFILE_MEMBUFFER = 2
        };

        public enum COLORTYPE : uint
        {
            COLOR_GRAY       =   1,
            COLOR_RGB,
            COLOR_XYZ,
            COLOR_Yxy,
            COLOR_Lab,
            COLOR_3_CHANNEL,
            COLOR_CMYK,
            COLOR_5_CHANNEL,
            COLOR_6_CHANNEL,
            COLOR_7_CHANNEL,
            COLOR_8_CHANNEL,
            COLOR_NAMED,

            // Not part of the real enum in icm.h but here for backwards compat
            COLOR_UNDEFINED = 255
        };

        public enum ColorSpace : uint
        {   
            SPACE_XYZ       = 0x58595A20,  // = 'XYZ '
            SPACE_Lab       = 0x4C616220,  // = 'Lab '
            SPACE_Luv       = 0x4C757620,  // = 'Luv '
            SPACE_YCbCr     = 0x59436272,  // = 'YCbr'
            SPACE_Yxy       = 0x59787920,  // = 'Yxy '
            SPACE_RGB       = 0x52474220,  // = 'RGB '
            SPACE_GRAY      = 0x47524159,  // = 'GRAY'
            SPACE_HSV       = 0x48535620,  // = 'HSV '
            SPACE_HLS       = 0x484C5320,  // = 'HLS '
            SPACE_CMYK      = 0x434D594B,  // = 'CMYK'
            SPACE_CMY       = 0x434D5920,  // = 'CMY '
            SPACE_2_CHANNEL = 0x32434C52,  // = '2CLR'
            SPACE_3_CHANNEL = 0x33434C52,  // = '3CLR'
            SPACE_4_CHANNEL = 0x34434C52,  // = '4CLR'
            SPACE_5_CHANNEL = 0x35434C52,  // = '5CLR'
            SPACE_6_CHANNEL = 0x36434C52,  // = '6CLR'
            SPACE_7_CHANNEL = 0x37434C52,  // = '7CLR'
            SPACE_8_CHANNEL = 0x38434C52,  // = '8CLR'

            // These are not in icm.h but were present in our original
            // implementation. We don't know if these actually exist 
            // but we're going to leave them anyway for compat.
            SPACE_9_CHANNEL = 0x39434C52,  // = '9CLR'
            SPACE_A_CHANNEL = 0x41434C52,  // = 'ACLR'
            SPACE_B_CHANNEL = 0x42434C52,  // = 'BCLR'
            SPACE_C_CHANNEL = 0x43434C52,  // = 'CCLR'
            SPACE_D_CHANNEL = 0x44434C52,  // = 'DCLR'
            SPACE_E_CHANNEL = 0x45434C52,  // = 'ECLR'
            SPACE_F_CHANNEL = 0x46434C52,  // = 'FCLR'
            SPACE_sRGB      = 0x73524742   // = 'sRGB'
        };
        
        //
        // </Windows Color System (WCS) types>
        //
    }
}

