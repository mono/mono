//
// System.Windows.Forms.Win32
//
// Provides easy access to commonly used Win32/WINELib contants and functions
//
// Author:
//   John Sohn (jsohn@columbus.rr.com)
//	Dennis Hayes (dennish@raytek.com)
//

using System.Runtime.InteropServices;
using System.Drawing;
namespace System.Windows.Forms {

	// holder for DLL Win32 DllImports 
	public class Win32 {

		[StructLayout(LayoutKind.Sequential)]
		public class POINT {
			public long  x;
			public long  y;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct RECT {
			public long left; 
			public long top; 
			public long right; 
			public long bottom;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct WINDOWPLACEMENT { 
			public uint  length; 
			public uint  flags; 
			public uint  showCmd; 
			public POINT ptMinPosition; 
			public POINT ptMaxPosition; 
			public RECT  rcNormalPosition; 
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct MSG {
			public IntPtr   hwnd;
			public uint  message; 
			public IntPtr wParam; 
			public IntPtr lParam; 
			public uint  time; 
			public POINT  pt;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct WNDCLASS {
			public int style;
			public WndProc lpfnWndProc;
			public int cbClsExtra;
			public int cbWndExtra;
			public IntPtr hInstance;
			public IntPtr hIcon;
			public IntPtr hCursor;
			public IntPtr hbrBackground;
			public string lpszMenuName;
			public string lpszClassName;
		}

		// helper function for calling RegisterClass
		public delegate IntPtr WndProc (IntPtr hwnd, int msg,
						IntPtr wParam, IntPtr lParam);

		[DllImport ("monostub.exe", 
			    CallingConvention = CallingConvention.StdCall,
			    CharSet = CharSet.Auto)]
		public static extern int MonoRegisterClass (
			int style, WndProc lpfnWndProc, int cbClsExtra,
			int cbWndExtra, IntPtr hInstance, IntPtr hIcon,
			IntPtr hCursor,	IntPtr hbrBackground,
			string lpszMenuName, string lpszClassName);

		#region Added by Dennis hayes 10-20-2002
		//correct?
		[DllImport ("user32.dll", 
			    CallingConvention = CallingConvention.StdCall,
			    CharSet = CharSet.Auto)]
		public static extern uint SendMessage(
			IntPtr hWnd, uint Msg,
			IntPtr wParam, IntPtr lParam);

		[DllImport ("user32.dll", 
			 CallingConvention = CallingConvention.StdCall,
			 CharSet = CharSet.Auto)]
		public static extern bool GetWindowPlacement(
			IntPtr hWnd,
			ref  WINDOWPLACEMENT  lpwndpl  // position data
			);
		#endregion

		[DllImport ("user32.dll", 
			    CallingConvention = CallingConvention.StdCall,
			    CharSet = CharSet.Auto)]
		public static extern IntPtr CreateWindowExA (
			uint dwExStyle, string lpClassName, 
			string lpWindowName, uint dwStyle, 
			int x, int y, int nWidth, int nHeight,
			IntPtr hWndParent, IntPtr hMenu, IntPtr hInstance,
			ref object lpParam);

		[DllImport ("user32.dll", 
			    CallingConvention = CallingConvention.StdCall, 
			    CharSet = CharSet.Auto)]
		public static extern IntPtr CreateMenu ();

		[DllImport ("user32.dll", 
			    CallingConvention = CallingConvention.StdCall, 
			    CharSet = CharSet.Ansi)]
		public static extern bool AppendMenuA(IntPtr hMenu, uint uflags, IntPtr NewItem, string item);

		[DllImport ("user32.dll", 
			    CallingConvention = CallingConvention.StdCall)]
		public static extern bool DestroyMenu (IntPtr hMenu);
		
		[DllImport ("user32.dll", CallingConvention = 
			    CallingConvention.StdCall, CharSet = CharSet.Auto)]
		public extern static IntPtr DefWindowProcA (
			IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);

		[DllImport ("user32.dll", CallingConvention = 
			    CallingConvention.StdCall, CharSet = CharSet.Auto)]
		public extern static int DestroyWindow (IntPtr hWnd);

		[DllImport ("user32.dll", CallingConvention = 
			    CallingConvention.StdCall, CharSet = CharSet.Auto)]
		public static extern int ShowWindow (IntPtr hWnd, 
						     uint nCmdShow);

		[DllImport ("user32.dll", CallingConvention = 
			    CallingConvention.StdCall,CharSet = CharSet.Auto)]
		public static extern int GetMessageA (ref MSG msg, int hwnd, 
						      int msgFrom,  int msgTo);

		[DllImport ("user32.dll", CallingConvention = 
			    CallingConvention.StdCall, CharSet = CharSet.Auto)]
		public static extern int  TranslateMessage (ref MSG msg);

		[DllImport ("user32.dll", CallingConvention = 
			    CallingConvention.StdCall, CharSet = CharSet.Auto)]
		public static extern int DispatchMessageA (ref MSG msg);

		[DllImport ("user32.dll", CallingConvention = 
			    CallingConvention.StdCall, CharSet = CharSet.Auto)]
		public static extern int PeekMessageA (
			ref MSG msg, IntPtr hWnd, uint wMsgFilterMin, 
			uint wMsgFilterMax, uint wRemoveMsg);

		[DllImport ("user32.dll", CallingConvention = 
			    CallingConvention.StdCall,
			    CharSet = CharSet.Auto)]
		public extern static void PostQuitMessage (int nExitCode);

		[DllImport ("user32.dll", CallingConvention = 
			    CallingConvention.StdCall,
			    CharSet = CharSet.Auto)]
		public extern static IntPtr SetActiveWindow (IntPtr hWnd);

		[DllImport ("user32.dll", CallingConvention = 
			    CallingConvention.StdCall,
			    CharSet = CharSet.Auto)]
		public extern static int CloseWindow (IntPtr hWnd);

		[DllImport ("user32.dll", CallingConvention = 
			    CallingConvention.StdCall,
			    CharSet = CharSet.Auto)]
		public extern static int SetWindowPos (
			IntPtr hWnd, IntPtr hWndInsertAfter, 
			int X, int Y, int cx, int cy, int uFlags);

		[DllImport ("user32.dll", 
			    CallingConvention = CallingConvention.StdCall, 
			    CharSet = CharSet.Auto)]
		public static extern int MessageBoxA (
			IntPtr hWnd, string pText, string pCaption, uint uType);

		[DllImport ("user32.dll", 
			    CallingConvention = CallingConvention.StdCall, 
			    CharSet = CharSet.Auto)]
		public static extern IntPtr SetParent (
			IntPtr hWndChild, IntPtr hWndNewParent);

		[DllImport ("user32.dll", 
			    CallingConvention = CallingConvention.StdCall, 
			    CharSet = CharSet.Auto)]
		public static extern IntPtr GetParent (IntPtr hWnd);

		[DllImport ("user32.dll", 
			    CallingConvention = CallingConvention.StdCall, 
			    CharSet = CharSet.Auto)]
		public static extern bool SetWindowTextA (
			IntPtr hWnd, string lpString);

		[DllImport ("user32.dll", 
			    CallingConvention = CallingConvention.StdCall, 
			    CharSet = CharSet.Auto)]
		public static extern bool UpdateWindow (IntPtr hWnd);

		[DllImport ("user32.dll", 
			    CallingConvention = CallingConvention.StdCall, 
			    CharSet = CharSet.Auto)]
		public static extern uint GetBkColor (IntPtr hdc);

		[DllImport ("user32.dll", 
			    CallingConvention = CallingConvention.StdCall, 
			    CharSet = CharSet.Auto)]
		public static extern uint SetBkColor (IntPtr hdc, uint crColor);

		[DllImport ("user32.dll", 
			    CallingConvention = CallingConvention.StdCall, 
			    CharSet = CharSet.Auto)]
		public static extern IntPtr GetDC (IntPtr hWnd);

		[DllImport ("user32.dll", 
			    CallingConvention = CallingConvention.StdCall, 
			    CharSet = CharSet.Auto)]
		public static extern int ReleaseDC (IntPtr hWnd, IntPtr hDC);

		[DllImport ("user32.dll", 
			    CallingConvention = CallingConvention.StdCall, 
			    CharSet = CharSet.Auto)]
		public static extern IntPtr GetFocus();

		[DllImport ("user32.dll", 
			    CallingConvention = CallingConvention.StdCall, 
			    CharSet = CharSet.Auto)]
		public static extern IntPtr SetFocus (IntPtr hWnd);

		[DllImport ("user32.dll", 
			    CallingConvention = CallingConvention.StdCall, 
			    CharSet = CharSet.Auto)]
		public static extern bool IsWindowEnabled (IntPtr hWnd);

		[DllImport ("user32.dll", 
			    CallingConvention = CallingConvention.StdCall, 
			    CharSet = CharSet.Auto)]
		public static extern bool IsMenu (IntPtr hWnd);

		
		[DllImport ("user32.dll", 
			    CallingConvention = CallingConvention.StdCall, 
			    CharSet = CharSet.Auto)]
		public static extern bool EnableWindow (
			IntPtr hWnd, bool bEnable);

		[DllImport ("user32.dll", 
			    CallingConvention = CallingConvention.StdCall, 
			    CharSet = CharSet.Auto)]
		public static extern bool GetWindowRect (
			IntPtr hWnd, ref RECT lpRect);

		[DllImport ("user32.dll", 
			    CallingConvention = CallingConvention.StdCall, 
			    CharSet = CharSet.Auto)]
		public static extern bool GetClientRect (
			IntPtr hWnd, ref RECT lpRect);

		[DllImport ("user32.dll", 
			    CallingConvention = CallingConvention.StdCall, 
			    CharSet = CharSet.Auto)]
		public static extern bool InvalidateRect (
			IntPtr hWnd, ref RECT lpRect, bool bErase); 

		[DllImport ("user32.dll", 
			    CallingConvention = CallingConvention.StdCall, 
			    CharSet = CharSet.Auto)]
		public static extern IntPtr GetCapture ();

		[DllImport ("user32.dll", 
			    CallingConvention = CallingConvention.StdCall, 
			    CharSet = CharSet.Auto)]
		public static extern IntPtr SetCapture (IntPtr hWnd);

		[DllImport ("user32.dll", 
			    CallingConvention = CallingConvention.StdCall, 
			    CharSet = CharSet.Auto)]
		public static extern bool ReleaseCapture ();

		[DllImport ("user32.dll", 
			    CallingConvention = CallingConvention.StdCall, 
			    CharSet = CharSet.Auto)]
		public static extern int GetWindowTextA (
			IntPtr hWnd, ref String lpString, int nMaxCount);

		[DllImport ("user32.dll", 
			    CallingConvention = CallingConvention.StdCall, 
			    CharSet = CharSet.Auto)]
		public static extern int GetWindowTextLengthA (IntPtr hWnd);

		[DllImport ("user32.dll", 
			    CallingConvention = CallingConvention.StdCall, 
			    CharSet = CharSet.Auto)]
		public static extern bool GetCursorPos (ref POINT lpPoint);



		// Window Messages
		public const uint WM_NULL                 = 0x0000;
		public const uint WM_CREATE               = 0x0001;
		public const uint WM_DESTROY              = 0x0002;
		public const uint WM_MOVE                 = 0x0003;
		public const uint WM_SIZEWAIT             = 0x0004;
		public const uint WM_SIZE                 = 0x0005;
		public const uint WM_ACTIVATE             = 0x0006;
		public const uint WM_SETFOCUS             = 0x0007;
		public const uint WM_KILLFOCUS            = 0x0008;
		public const uint WM_SETVISIBLE           = 0x0009;
		public const uint WM_ENABLE               = 0x000a;
		public const uint WM_SETREDRAW            = 0x000b;
		public const uint WM_SETTEXT              = 0x000c;
		public const uint WM_GETTEXT              = 0x000d;
		public const uint WM_GETTEXTLENGTH        = 0x000e;
		public const uint WM_PAINT                = 0x000f;
		public const uint WM_CLOSE                = 0x0010;
		public const uint WM_QUERYENDSESSION      = 0x0011;
		public const uint WM_QUIT                 = 0x0012;
		public const uint WM_QUERYOPEN            = 0x0013;
		public const uint WM_ERASEBKGND           = 0x0014;
		public const uint WM_SYSCOLORCHANGE       = 0x0015;
		public const uint WM_ENDSESSION           = 0x0016;
		public const uint WM_SYSTEMERROR          = 0x0017;
		public const uint WM_SHOWWINDOW           = 0x0018;
		public const uint WM_CTLCOLOR             = 0x0019;
		public const uint WM_WININICHANGE         = 0x001a;
		public const uint WM_SETTINGCHANGE        = WM_WININICHANGE;
		public const uint WM_DEVMODECHANGE        = 0x001b;
		public const uint WM_ACTIVATEAPP          = 0x001c;
		public const uint WM_FONTCHANGE           = 0x001d;
		public const uint WM_TIMECHANGE           = 0x001e;
		public const uint WM_CANCELMODE           = 0x001f;
		public const uint WM_SETCURSOR            = 0x0020;
		public const uint WM_MOUSEACTIVATE        = 0x0021;
		public const uint WM_CHILDACTIVATE        = 0x0022;
		public const uint WM_QUEUESYNC            = 0x0023;
		public const uint WM_GETMINMAXINFO        = 0x0024;

		public const uint WM_PAINTICON            = 0x0026;
		public const uint WM_ICONERASEBKGND       = 0x0027;
		public const uint WM_NEXTDLGCTL           = 0x0028;
		public const uint WM_ALTTABACTIVE         = 0x0029;
		public const uint WM_SPOOLERSTATUS        = 0x002a;
		public const uint WM_DRAWITEM             = 0x002b;
		public const uint WM_MEASUREITEM          = 0x002c;
		public const uint WM_DELETEITEM           = 0x002d;
		public const uint WM_VKEYTOITEM           = 0x002e;
		public const uint WM_CHARTOITEM           = 0x002f;
		public const uint WM_SETFONT              = 0x0030;
		public const uint WM_GETFONT              = 0x0031;
		public const uint WM_SETHOTKEY            = 0x0032;
		public const uint WM_GETHOTKEY            = 0x0033;
		public const uint WM_FILESYSCHANGE        = 0x0034;
		public const uint WM_ISACTIVEICON         = 0x0035;
		public const uint WM_QUERYPARKICON        = 0x0036;
		public const uint WM_QUERYDRAGICON        = 0x0037;
		public const uint WM_QUERYSAVESTATE       = 0x0038;
		public const uint WM_COMPAREITEM          = 0x0039;
		public const uint WM_TESTING              = 0x003a;

		public const uint WM_OTHERWINDOWCREATED	= 0x003c;
		public const uint WM_OTHERWINDOWDESTROYED	= 0x003d;
		public const uint WM_ACTIVATESHELLWINDOW	= 0x003e;

		public const uint WM_COMPACTING		= 0x0041;

		public const uint WM_COMMNOTIFY		= 0x0044;
		public const uint WM_WINDOWPOSCHANGING 	= 0x0046;
		public const uint WM_WINDOWPOSCHANGED 	= 0x0047;
		public const uint WM_POWER		= 0x0048;

		/* Win32 4.0 messages */
		public const uint WM_COPYDATA		= 0x004a;
		public const uint WM_CANCELJOURNAL	= 0x004b;
		public const uint WM_NOTIFY		= 0x004e;
		public const uint WM_INPUTLANGCHANGEREQUEST       = 0x0050;
		public const uint WM_INPUTLANGCHANGE              = 0x0051;
		public const uint WM_TCARD                = 0x0052;
		public const uint WM_HELP			= 0x0053;
		public const uint WM_USERCHANGED		= 0x0054;
		public const uint WM_NOTIFYFORMAT		= 0x0055;

		public const uint WM_CONTEXTMENU		= 0x007b;
		public const uint WM_STYLECHANGING 	= 0x007c;
		public const uint WM_STYLECHANGED		= 0x007d;
		public const uint WM_DISPLAYCHANGE        = 0x007e;
		public const uint WM_GETICON		= 0x007f;
		public const uint WM_SETICON		= 0x0080;

		/* Non-client system messages */
		public const uint WM_NCCREATE         = 0x0081;
		public const uint WM_NCDESTROY        = 0x0082;
		public const uint WM_NCCALCSIZE       = 0x0083;
		public const uint WM_NCHITTEST        = 0x0084;
		public const uint WM_NCPAINT          = 0x0085;
		public const uint WM_NCACTIVATE       = 0x0086;

		public const uint WM_GETDLGCODE	    = 0x0087;
		public const uint WM_SYNCPAINT	    = 0x0088;
		public const uint WM_SYNCTASK	    = 0x0089;

		/* Non-client mouse messages */
		public const uint WM_NCMOUSEMOVE      = 0x00a0;
		public const uint WM_NCLBUTTONDOWN    = 0x00a1;
		public const uint WM_NCLBUTTONUP      = 0x00a2;
		public const uint WM_NCLBUTTONDBLCLK  = 0x00a3;
		public const uint WM_NCRBUTTONDOWN    = 0x00a4;
		public const uint WM_NCRBUTTONUP      = 0x00a5;
		public const uint WM_NCRBUTTONDBLCLK  = 0x00a6;
		public const uint WM_NCMBUTTONDOWN    = 0x00a7;
		public const uint WM_NCMBUTTONUP      = 0x00a8;
		public const uint WM_NCMBUTTONDBLCLK  = 0x00a9;

		public const uint WM_NCXBUTTONDOWN    = 0x00ab;
		public const uint WM_NCXBUTTONUP      = 0x00ac;
		public const uint WM_NCXBUTTONDBLCLK  = 0x00ad;

		/* Keyboard messages */
		public const uint WM_KEYDOWN          = 0x0100;
		public const uint WM_KEYUP            = 0x0101;
		public const uint WM_CHAR             = 0x0102;
		public const uint WM_DEADCHAR         = 0x0103;
		public const uint WM_SYSKEYDOWN       = 0x0104;
		public const uint WM_SYSKEYUP         = 0x0105;
		public const uint WM_SYSCHAR          = 0x0106;
		public const uint WM_SYSDEADCHAR      = 0x0107;
		public const uint WM_KEYFIRST         = WM_KEYDOWN;
		public const uint WM_KEYLAST          = 0x0108;

		/* Win32 4.0 messages for IME */
		public const uint IME_STARTCOMPOSITION     = 0x010d;
		public const uint IME_ENDCOMPOSITION       = 0x010e;
		public const uint IME_COMPOSITION          = 0x010f;
		public const uint IME_KEYLAST              = 0x010f;

		public const uint WM_INITDIALOG       = 0x0110;
		public const uint WM_COMMAND          = 0x0111;
		public const uint WM_SYSCOMMAND       = 0x0112;
		public const uint WM_TIMER	    = 0x0113;
		public const uint WM_SYSTIMER	    = 0x0118;

		/* scroll messages */
		public const uint WM_HSCROLL          = 0x0114;
		public const uint WM_VSCROLL          = 0x0115;

		/* Menu messages */
		public const uint WM_INITMENU         = 0x0116;
		public const uint WM_INITMENUPOPUP    = 0x0117;

		public const uint WM_MENUSELECT       = 0x011F;
		public const uint WM_MENUCHAR         = 0x0120;
		public const uint WM_ENTERIDLE        = 0x0121;

		public const uint WM_MENURBUTTONUP    = 0x0122;
		public const uint WM_MENUDRAG         = 0x0123;
		public const uint WM_MENUGETOBJECT    = 0x0124;
		public const uint WM_UNINITMENUPOPUP  = 0x0125;
		public const uint WM_MENUCOMMAND      = 0x0126;

		public const uint WM_CHANGEUISTATE    = 0x0127;
		public const uint WM_UPDATEUISTATE    = 0x0128;
		public const uint WM_QUERYUISTATE     = 0x0129;

		public const uint WM_LBTRACKPOINT     = 0x0131;

		/* Win32 CTLCOLOR messages */
		public const uint WM_CTLCOLORMSGBOX    = 0x0132;
		public const uint WM_CTLCOLOREDIT      = 0x0133;
		public const uint WM_CTLCOLORLISTBOX   = 0x0134;
		public const uint WM_CTLCOLORBTN       = 0x0135;
		public const uint WM_CTLCOLORDLG       = 0x0136;
		public const uint WM_CTLCOLORSCROLLBAR = 0x0137;
		public const uint WM_CTLCOLORSTATIC    = 0x0138;

		/* Mouse messages */
		public const uint WM_MOUSEMOVE	    = 0x0200;
		public const uint WM_LBUTTONDOWN	    = 0x0201;
		public const uint WM_LBUTTONUP	    = 0x0202;
		public const uint WM_LBUTTONDBLCLK    = 0x0203;
		public const uint WM_RBUTTONDOWN	    = 0x0204;
		public const uint WM_RBUTTONUP	    = 0x0205;
		public const uint WM_RBUTTONDBLCLK    = 0x0206;
		public const uint WM_MBUTTONDOWN	    = 0x0207;
		public const uint WM_MBUTTONUP	    = 0x0208;
		public const uint WM_MBUTTONDBLCLK    = 0x0209;
		public const uint WM_MOUSEWHEEL       = 0x020A;
		public const uint WM_XBUTTONDOWN      = 0x020B;
		public const uint WM_XBUTTONUP        = 0x020C;
		public const uint WM_XBUTTONDBLCLK    = 0x020D;

		public const uint WM_MOUSEFIRST       = 0x0200;
		public const uint WM_MOUSELAST        = 0x020D;

		public const uint WM_WHEEL_DELTA      = 120;
		public const uint WM_WHEEL_PAGESCROLL  = 65535;
		public const uint WM_PARENTNOTIFY     = 0x0210;
		public const uint WM_ENTERMENULOOP    = 0x0211;
		public const uint WM_EXITMENULOOP     = 0x0212;
		public const uint WM_NEXTMENU	    = 0x0213;

		/* Win32 4.0 messages */
		public const uint WM_SIZING	    = 0x0214;
		public const uint WM_CAPTURECHANGED   = 0x0215;
		public const uint WM_MOVING	    = 0x0216;
		public const uint WM_POWERBROADCAST   = 0x0218;
		public const uint WM_DEVICECHANGE     = 0x0219;

		/* wParam for WM_SIZING message */
		public const uint WMSZ_LEFT           = 1;
		public const uint WMSZ_RIGHT          = 2;
		public const uint WMSZ_TOP            = 3;
		public const uint WMSZ_TOPLEFT        = 4;
		public const uint WMSZ_TOPRIGHT       = 5;
		public const uint WMSZ_BOTTOM         = 6;
		public const uint WMSZ_BOTTOMLEFT     = 7;
		public const uint WMSZ_BOTTOMRIGHT    = 8;

		/* MDI messages */
		public const uint WM_MDICREATE	    = 0x0220;
		public const uint WM_MDIDESTROY	    = 0x0221;
		public const uint WM_MDIACTIVATE	    = 0x0222;
		public const uint WM_MDIRESTORE	    = 0x0223;
		public const uint WM_MDINEXT	    = 0x0224;
		public const uint WM_MDIMAXIMIZE	    = 0x0225;
		public const uint WM_MDITILE	    = 0x0226;
		public const uint WM_MDICASCADE	    = 0x0227;
		public const uint WM_MDIICONARRANGE   = 0x0228;
		public const uint WM_MDIGETACTIVE     = 0x0229;
		public const uint WM_MDIREFRESHMENU   = 0x0234;

		/* D&D messages */
		public const uint WM_DROPOBJECT	    = 0x022A;
		public const uint WM_QUERYDROPOBJECT  = 0x022B;
		public const uint WM_BEGINDRAG	    = 0x022C;
		public const uint WM_DRAGLOOP	    = 0x022D;
		public const uint WM_DRAGSELECT	    = 0x022E;
		public const uint WM_DRAGMOVE	    = 0x022F;
		public const uint WM_MDISETMENU	    = 0x0230;

		public const uint WM_ENTERSIZEMOVE    = 0x0231;
		public const uint WM_EXITSIZEMOVE     = 0x0232;
		public const uint WM_DROPFILES	    = 0x0233;


		/* Win32 4.0 messages for IME */
		public const uint IME_SETCONTEXT           = 0x0281;
		public const uint IME_NOTIFY               = 0x0282;
		public const uint IME_CONTROL              = 0x0283;
		public const uint IME_COMPOSITIONFULL      = 0x0284;
		public const uint IME_SELECT               = 0x0285;
		public const uint IME_CHAR                 = 0x0286;
		/* Win32 5.0 messages for IME */
		public const uint IME_REQUEST              = 0x0288;

		/* Win32 4.0 messages for IME */
		public const uint IME_KEYDOWN              = 0x0290;
		public const uint IME_KEYUP                = 0x0291;

		/* Clipboard command messages */
		public const uint WM_CUT               = 0x0300;
		public const uint WM_COPY              = 0x0301;
		public const uint WM_PASTE             = 0x0302;
		public const uint WM_CLEAR             = 0x0303;
		public const uint WM_UNDO              = 0x0304;

		/* Clipboard owner messages */
		public const uint WM_RENDERFORMAT      = 0x0305;
		public const uint WM_RENDERALLFORMATS  = 0x0306;
		public const uint WM_DESTROYCLIPBOARD  = 0x0307;

		/* Clipboard viewer messages */
		public const uint WM_DRAWCLIPBOARD     = 0x0308;
		public const uint WM_PAINTCLIPBOARD    = 0x0309;
		public const uint WM_VSCROLLCLIPBOARD  = 0x030A;
		public const uint WM_SIZECLIPBOARD     = 0x030B;
		public const uint WM_ASKCBFORMATNAME   = 0x030C;
		public const uint WM_CHANGECBCHAIN     = 0x030D;
		public const uint WM_HSCROLLCLIPBOARD  = 0x030E;

		public const uint WM_QUERYNEWPALETTE   = 0x030F;
		public const uint WM_PALETTEISCHANGING = 0x0310;
		public const uint WM_PALETTECHANGED    = 0x0311;
		public const uint WM_HOTKEY	     = 0x0312;

		public const uint WM_PRINT             = 0x0317;
		public const uint WM_PRINTCLIENT       = 0x0318;

		public const uint WM_PENWINFIRST      = 0x0380;
		public const uint WM_PENWINLAST       = 0x038F;


		public const uint WM_APP               = 0x8000;

		public const int WM_MOUSEHOVER                   = 0x02A1;
		public const int WM_MOUSELEAVE                   = 0x02A3;

		// Window Styles
		public const uint WS_OVERLAPPED = 0x00000000;
		public const uint WS_POPUP = 0x80000000;
		public const uint WS_CHILD = 0x40000000;
		public const uint WS_MINIMIZE = 0x20000000;
		public const uint WS_VISIBLE = 0x10000000;
		public const uint WS_DISABLED = 0x08000000;
		public const uint WS_CLIPSIBLINGS = 0x04000000;
		public const uint WS_CLIPCHILDREN = 0x02000000;
		public const uint WS_MAXIMIZE = 0x01000000;
		public const uint WS_CAPTION = 0x00C00000;
		public const uint WS_BORDER = 0x00800000;
		public const uint WS_DLGFRAME = 0x00400000;
		public const uint WS_VSCROLL = 0x00200000;
		public const uint WS_HSCROLL = 0x00100000;
		public const uint WS_SYSMENU = 0x00080000;
		public const uint WS_THICKFRAME = 0x00040000;
		public const uint WS_GROUP = 0x00020000;
		public const uint WS_TABSTOP = 0x00010000;
		public const uint WS_MINIMIZEBOX = 0x00020000;
		public const uint WS_MAXIMIZEBOX = 0x00010000;
		public const uint WS_OVERLAPPEDWINDOW = WS_OVERLAPPED | 
		  WS_CAPTION | WS_SYSMENU | WS_THICKFRAME| 
		  WS_MINIMIZEBOX | WS_MAXIMIZEBOX;

		// ShowWindow types
		public const uint SW_HIDE             = 0;
		public const uint SW_SHOWNORMAL       = 1;
		public const uint SW_NORMAL           = 1;
		public const uint SW_SHOWMINIMIZED    = 2;
		public const uint SW_SHOWMAXIMIZED    = 3;
		public const uint SW_MAXIMIZE         = 3;
		public const uint SW_SHOWNOACTIVATE   = 4;
		public const uint SW_SHOW             = 5;
		public const uint SW_MINIMIZE         = 6;
		public const uint SW_SHOWMINNOACTIVE  = 7;
		public const uint SW_SHOWNA           = 8;
		public const uint SW_RESTORE          = 9;
		public const uint SW_SHOWDEFAULT      = 10;
		public const uint SW_FORCEMINIMIZE    = 11;
		public const uint SW_MAX              = 11;
		public const uint SW_NORMALNA         = 0xCC;

		// SetWindowPos types
		public const uint SWP_NOSIZE          = 0x0001;
		public const uint SWP_NOMOVE          = 0x0002;
		public const uint SWP_NOZORDER        = 0x0004;
		public const uint SWP_NOREDRAW        = 0x0008;
		public const uint SWP_NOACTIVATE      = 0x0010;
		public const uint SWP_FRAMECHANGED    = 0x0020;
		public const uint SWP_SHOWWINDOW      = 0x0040;
		public const uint SWP_HIDEWINDOW      = 0x0080;
		public const uint SWP_NOCOPYBITS      = 0x0100;
		public const uint SWP_NOOWNERZORDER   = 0x0200;
		public const uint SWP_DRAWFRAME       = SWP_FRAMECHANGED;
		public const uint SWP_NOREPOSITION    = SWP_NOOWNERZORDER;
		public const uint SWP_NOSENDCHANGING  = 0x0400;
		public const uint SWP_DEFERERASE      = 0x2000;
		public const uint SWP_ASYNCWINDOWPOS  = 0x4000;


		public const uint CS_VREDRAW          = 0x0001;
		public const uint CS_HREDRAW          = 0x0002;
		public const uint CS_KEYCVTWINDOW     = 0x0004;
		public const uint CS_DBLCLKS          = 0x0008;
		public const uint CS_OWNDC            = 0x0020;
		public const uint CS_CLASSDC          = 0x0040;
		public const uint CS_PARENTDC         = 0x0080;
		public const uint CS_NOKEYCVT         = 0x0100;
		public const uint CS_NOCLOSE          = 0x0200;
		public const uint CS_SAVEBITS         = 0x0800;
		public const uint CS_BYTEALIGNCLIENT  = 0x1000;
		public const uint CS_BYTEALIGNWINDOW  = 0x2000;
		public const uint CS_GLOBALCLASS      = 0x4000;
		public const uint CS_IME              = 0x00010000;

		// PeekMessage types
		public const uint PM_NOREMOVE     = 0x0000;
		public const uint PM_REMOVE       = 0x0001;
		public const uint PM_NOYIELD      = 0x0002;

		// MessageBox types
		public const uint MB_OK                   = 0x00000000;
		public const uint MB_OKCANCEL             = 0x00000001;
		public const uint MB_ABORTRETRYIGNORE     = 0x00000002;
		public const uint MB_YESNOCANCEL          = 0x00000003;
		public const uint MB_YESNO                = 0x00000004;
		public const uint MB_RETRYCANCEL          = 0x00000005;
		public const uint MB_TYPEMASK             = 0x0000000F;

		public const int HWND_TOP = 0;
		public const int HWND_BOTTOM = 1;
		public const int HWND_TOPMOST = -1;
		public const int HWND_NOTOPMOST = -2;
		public const int HWND_MESSAGE = -3;

		// Static Control Styles
		public const uint SS_LEFT             = 0x00000000;
		public const int SS_CENTER           = 0x00000001;
		public const int SS_RIGHT            = 0x00000002;
		public const int SS_ICON             = 0x00000003;
		public const int SS_BLACKRECT        = 0x00000004;
		public const int SS_GRAYRECT         = 0x00000005;
		public const int SS_WHITERECT        = 0x00000006;
		public const int SS_BLACKFRAME       = 0x00000007;
		public const int SS_GRAYFRAME        = 0x00000008;
		public const int SS_WHITEFRAME       = 0x00000009;

		public const int SS_SIMPLE           = 0x0000000B;
		public const int SS_LEFTNOWORDWRAP   = 0x0000000C;
		
		public const int SS_OWNERDRAW        = 0x0000000D;
		public const int SS_BITMAP           = 0x0000000E;
		public const int SS_ENHMETAFILE      = 0x0000000F;

		public const int SS_ETCHEDHORZ       = 0x00000010;
		public const int SS_ETCHEDVERT       = 0x00000011;
		public const int SS_ETCHEDFRAME      = 0x00000012;
		public const int SS_TYPEMASK         = 0x0000001F;
		
		public const int SS_NOPREFIX         = 0x00000080;
		public const int SS_NOTIFY           = 0x00000100;
		public const int SS_CENTERIMAGE      = 0x00000200;
		public const int SS_RIGHTJUST        = 0x00000400;
		public const int SS_REALSIZEIMAGE    = 0x00000800;
		public const int SS_SUNKEN           = 0x00001000;

		public const int MF_ENABLED          = 0;
		public const int MF_STRING           = 0;
		public const int MF_POPUP            = 0x10;
	}

}
