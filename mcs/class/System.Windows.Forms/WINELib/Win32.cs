//
// System.Windows.Forms.Win32
//
// Author:
//   John Sohn (jsohn@columbus.rr.com)
//
// Provides easy access to commonly used Win32/WINELib contants and functions
//

using System.Runtime.InteropServices;

namespace System.Windows.Forms {
	// holder for DLL Win32 DllImports 
	public unsafe class Win32 {
		[DllImport ("user32.dll", 
			    CallingConvention = CallingConvention.StdCall,
			    CharSet = CharSet.Auto)]
		public static extern IntPtr CreateWindowExA(int dwExStyle,
							    string lpClassName,
							    string lpWindowName,
							    int dwStyle,
							    int x,
							    int y,
							    int nWidth,
							    int nHeight,
							    IntPtr hWndParent,
							    IntPtr hMenu,
							    IntPtr hInstance,
							    void* lpParam);

		[DllImport ("user32.dll", CallingConvention = 
			    CallingConvention.StdCall, CharSet = CharSet.Auto)]
		public extern static IntPtr DefWindowProcA(IntPtr hWnd, 
							   int Msg,
							   IntPtr wParam,
							   IntPtr lParam);

		[DllImport ("user32.dll", CallingConvention = 
			    CallingConvention.StdCall, CharSet = CharSet.Auto)]
		public extern static int DestroyWindow(IntPtr hWnd);

		[DllImport ("user32.dll", CallingConvention = 
			    CallingConvention.StdCall, CharSet = CharSet.Auto)]
		public static extern int ShowWindow(IntPtr hWnd, int nCmdShow);

		[DllImport ("user32.dll", CallingConvention = 
			    CallingConvention.StdCall,CharSet = CharSet.Auto)]
		public static extern int GetMessageA(int* msg, int hwnd, 
						     int msgFrom,  int msgTo);

		[DllImport ("user32.dll", CallingConvention = 
			    CallingConvention.StdCall, CharSet = CharSet.Auto)]
		public static extern int  TranslateMessage(int* msg);

		[DllImport ("user32.dll", CallingConvention = 
			    CallingConvention.StdCall, CharSet = CharSet.Auto)]
		public static extern int DispatchMessageA(int* msg);

		[DllImport ("user32.dll", CallingConvention = 
			    CallingConvention.StdCall,
			    CharSet = CharSet.Auto)]
		public extern static void PostQuitMessage(int nExitCode);

		[DllImport ("user32.dll", CallingConvention = 
			    CallingConvention.StdCall,
			    CharSet = CharSet.Auto)]
		public extern static IntPtr SetActiveWindow(IntPtr hWnd);

		[DllImport ("user32.dll", CallingConvention = 
			    CallingConvention.StdCall,
			    CharSet = CharSet.Auto)]
		public extern static int CloseWindow(IntPtr hWnd);

		[DllImport ("user32.dll", CallingConvention = 
			    CallingConvention.StdCall,
			    CharSet = CharSet.Auto)]
		public extern static int SetWindowPos(IntPtr hWnd,
						      IntPtr hWndInsertAfter,
						      int X,
						      int Y,
						      int cx,
						      int cy,
						      int uFlags);

		// Window Messages
		public const int WM_NULL                 = 0x0000;
		public const int WM_CREATE               = 0x0001;
		public const int WM_DESTROY              = 0x0002;
		public const int WM_MOVE                 = 0x0003;
		public const int WM_SIZEWAIT             = 0x0004;
		public const int WM_SIZE                 = 0x0005;
		public const int WM_ACTIVATE             = 0x0006;
		public const int WM_SETFOCUS             = 0x0007;
		public const int WM_KILLFOCUS            = 0x0008;
		public const int WM_SETVISIBLE           = 0x0009;
		public const int WM_ENABLE               = 0x000a;
		public const int WM_SETREDRAW            = 0x000b;
		public const int WM_SETTEXT              = 0x000c;
		public const int WM_GETTEXT              = 0x000d;
		public const int WM_GETTEXTLENGTH        = 0x000e;
		public const int WM_PAINT                = 0x000f;
		public const int WM_CLOSE                = 0x0010;
		public const int WM_QUERYENDSESSION      = 0x0011;
		public const int WM_QUIT                 = 0x0012;
		public const int WM_QUERYOPEN            = 0x0013;
		public const int WM_ERASEBKGND           = 0x0014;
		public const int WM_SYSCOLORCHANGE       = 0x0015;
		public const int WM_ENDSESSION           = 0x0016;
		public const int WM_SYSTEMERROR          = 0x0017;
		public const int WM_SHOWWINDOW           = 0x0018;
		public const int WM_CTLCOLOR             = 0x0019;
		public const int WM_WININICHANGE         = 0x001a;
		public const int WM_SETTINGCHANGE        = WM_WININICHANGE;
		public const int WM_DEVMODECHANGE        = 0x001b;
		public const int WM_ACTIVATEAPP          = 0x001c;
		public const int WM_FONTCHANGE           = 0x001d;
		public const int WM_TIMECHANGE           = 0x001e;
		public const int WM_CANCELMODE           = 0x001f;
		public const int WM_SETCURSOR            = 0x0020;
		public const int WM_MOUSEACTIVATE        = 0x0021;
		public const int WM_CHILDACTIVATE        = 0x0022;
		public const int WM_QUEUESYNC            = 0x0023;
		public const int WM_GETMINMAXINFO        = 0x0024;

		public const int WM_PAINTICON            = 0x0026;
		public const int WM_ICONERASEBKGND       = 0x0027;
		public const int WM_NEXTDLGCTL           = 0x0028;
		public const int WM_ALTTABACTIVE         = 0x0029;
		public const int WM_SPOOLERSTATUS        = 0x002a;
		public const int WM_DRAWITEM             = 0x002b;
		public const int WM_MEASUREITEM          = 0x002c;
		public const int WM_DELETEITEM           = 0x002d;
		public const int WM_VKEYTOITEM           = 0x002e;
		public const int WM_CHARTOITEM           = 0x002f;
		public const int WM_SETFONT              = 0x0030;
		public const int WM_GETFONT              = 0x0031;
		public const int WM_SETHOTKEY            = 0x0032;
		public const int WM_GETHOTKEY            = 0x0033;
		public const int WM_FILESYSCHANGE        = 0x0034;
		public const int WM_ISACTIVEICON         = 0x0035;
		public const int WM_QUERYPARKICON        = 0x0036;
		public const int WM_QUERYDRAGICON        = 0x0037;
		public const int WM_QUERYSAVESTATE       = 0x0038;
		public const int WM_COMPAREITEM          = 0x0039;
		public const int WM_TESTING              = 0x003a;

		public const int WM_OTHERWINDOWCREATED	= 0x003c;
		public const int WM_OTHERWINDOWDESTROYED	= 0x003d;
		public const int WM_ACTIVATESHELLWINDOW	= 0x003e;

		public const int WM_COMPACTING		= 0x0041;

		public const int WM_COMMNOTIFY		= 0x0044;
		public const int WM_WINDOWPOSCHANGING 	= 0x0046;
		public const int WM_WINDOWPOSCHANGED 	= 0x0047;
		public const int WM_POWER		= 0x0048;

		/* Win32 4.0 messages */
		public const int WM_COPYDATA		= 0x004a;
		public const int WM_CANCELJOURNAL	= 0x004b;
		public const int WM_NOTIFY		= 0x004e;
		public const int WM_INPUTLANGCHANGEREQUEST       = 0x0050;
		public const int WM_INPUTLANGCHANGE              = 0x0051;
		public const int WM_TCARD                = 0x0052;
		public const int WM_HELP			= 0x0053;
		public const int WM_USERCHANGED		= 0x0054;
		public const int WM_NOTIFYFORMAT		= 0x0055;

		public const int WM_CONTEXTMENU		= 0x007b;
		public const int WM_STYLECHANGING 	= 0x007c;
		public const int WM_STYLECHANGED		= 0x007d;
		public const int WM_DISPLAYCHANGE        = 0x007e;
		public const int WM_GETICON		= 0x007f;
		public const int WM_SETICON		= 0x0080;

		/* Non-client system messages */
		public const int WM_NCCREATE         = 0x0081;
		public const int WM_NCDESTROY        = 0x0082;
		public const int WM_NCCALCSIZE       = 0x0083;
		public const int WM_NCHITTEST        = 0x0084;
		public const int WM_NCPAINT          = 0x0085;
		public const int WM_NCACTIVATE       = 0x0086;

		public const int WM_GETDLGCODE	    = 0x0087;
		public const int WM_SYNCPAINT	    = 0x0088;
		public const int WM_SYNCTASK	    = 0x0089;

		/* Non-client mouse messages */
		public const int WM_NCMOUSEMOVE      = 0x00a0;
		public const int WM_NCLBUTTONDOWN    = 0x00a1;
		public const int WM_NCLBUTTONUP      = 0x00a2;
		public const int WM_NCLBUTTONDBLCLK  = 0x00a3;
		public const int WM_NCRBUTTONDOWN    = 0x00a4;
		public const int WM_NCRBUTTONUP      = 0x00a5;
		public const int WM_NCRBUTTONDBLCLK  = 0x00a6;
		public const int WM_NCMBUTTONDOWN    = 0x00a7;
		public const int WM_NCMBUTTONUP      = 0x00a8;
		public const int WM_NCMBUTTONDBLCLK  = 0x00a9;

		public const int WM_NCXBUTTONDOWN    = 0x00ab;
		public const int WM_NCXBUTTONUP      = 0x00ac;
		public const int WM_NCXBUTTONDBLCLK  = 0x00ad;

		/* Keyboard messages */
		public const int WM_KEYDOWN          = 0x0100;
		public const int WM_KEYUP            = 0x0101;
		public const int WM_CHAR             = 0x0102;
		public const int WM_DEADCHAR         = 0x0103;
		public const int WM_SYSKEYDOWN       = 0x0104;
		public const int WM_SYSKEYUP         = 0x0105;
		public const int WM_SYSCHAR          = 0x0106;
		public const int WM_SYSDEADCHAR      = 0x0107;
		public const int WM_KEYFIRST         = WM_KEYDOWN;
		public const int WM_KEYLAST          = 0x0108;

		/* Win32 4.0 messages for IME */
		public const int IME_STARTCOMPOSITION     = 0x010d;
		public const int IME_ENDCOMPOSITION       = 0x010e;
		public const int IME_COMPOSITION          = 0x010f;
		public const int IME_KEYLAST              = 0x010f;

		public const int WM_INITDIALOG       = 0x0110;
		public const int WM_COMMAND          = 0x0111;
		public const int WM_SYSCOMMAND       = 0x0112;
		public const int WM_TIMER	    = 0x0113;
		public const int WM_SYSTIMER	    = 0x0118;

		/* scroll messages */
		public const int WM_HSCROLL          = 0x0114;
		public const int WM_VSCROLL          = 0x0115;

		/* Menu messages */
		public const int WM_INITMENU         = 0x0116;
		public const int WM_INITMENUPOPUP    = 0x0117;

		public const int WM_MENUSELECT       = 0x011F;
		public const int WM_MENUCHAR         = 0x0120;
		public const int WM_ENTERIDLE        = 0x0121;

		public const int WM_MENURBUTTONUP    = 0x0122;
		public const int WM_MENUDRAG         = 0x0123;
		public const int WM_MENUGETOBJECT    = 0x0124;
		public const int WM_UNINITMENUPOPUP  = 0x0125;
		public const int WM_MENUCOMMAND      = 0x0126;

		public const int WM_CHANGEUISTATE    = 0x0127;
		public const int WM_UPDATEUISTATE    = 0x0128;
		public const int WM_QUERYUISTATE     = 0x0129;

		public const int WM_LBTRACKPOINT     = 0x0131;

		/* Win32 CTLCOLOR messages */
		public const int WM_CTLCOLORMSGBOX    = 0x0132;
		public const int WM_CTLCOLOREDIT      = 0x0133;
		public const int WM_CTLCOLORLISTBOX   = 0x0134;
		public const int WM_CTLCOLORBTN       = 0x0135;
		public const int WM_CTLCOLORDLG       = 0x0136;
		public const int WM_CTLCOLORSCROLLBAR = 0x0137;
		public const int WM_CTLCOLORSTATIC    = 0x0138;

		/* Mouse messages */
		public const int WM_MOUSEMOVE	    = 0x0200;
		public const int WM_LBUTTONDOWN	    = 0x0201;
		public const int WM_LBUTTONUP	    = 0x0202;
		public const int WM_LBUTTONDBLCLK    = 0x0203;
		public const int WM_RBUTTONDOWN	    = 0x0204;
		public const int WM_RBUTTONUP	    = 0x0205;
		public const int WM_RBUTTONDBLCLK    = 0x0206;
		public const int WM_MBUTTONDOWN	    = 0x0207;
		public const int WM_MBUTTONUP	    = 0x0208;
		public const int WM_MBUTTONDBLCLK    = 0x0209;
		public const int WM_MOUSEWHEEL       = 0x020A;
		public const int WM_XBUTTONDOWN      = 0x020B;
		public const int WM_XBUTTONUP        = 0x020C;
		public const int WM_XBUTTONDBLCLK    = 0x020D;

		public const int WM_MOUSEFIRST       = 0x0200;
		public const int WM_MOUSELAST        = 0x020D;

		public const int WM_WHEEL_DELTA      = 120;
		public const int WM_WHEEL_PAGESCROLL  = 65535;
		public const int WM_PARENTNOTIFY     = 0x0210;
		public const int WM_ENTERMENULOOP    = 0x0211;
		public const int WM_EXITMENULOOP     = 0x0212;
		public const int WM_NEXTMENU	    = 0x0213;

		/* Win32 4.0 messages */
		public const int WM_SIZING	    = 0x0214;
		public const int WM_CAPTURECHANGED   = 0x0215;
		public const int WM_MOVING	    = 0x0216;
		public const int WM_POWERBROADCAST   = 0x0218;
		public const int WM_DEVICECHANGE     = 0x0219;

		/* wParam for WM_SIZING message */
		public const int WMSZ_LEFT           = 1;
		public const int WMSZ_RIGHT          = 2;
		public const int WMSZ_TOP            = 3;
		public const int WMSZ_TOPLEFT        = 4;
		public const int WMSZ_TOPRIGHT       = 5;
		public const int WMSZ_BOTTOM         = 6;
		public const int WMSZ_BOTTOMLEFT     = 7;
		public const int WMSZ_BOTTOMRIGHT    = 8;

		/* MDI messages */
		public const int WM_MDICREATE	    = 0x0220;
		public const int WM_MDIDESTROY	    = 0x0221;
		public const int WM_MDIACTIVATE	    = 0x0222;
		public const int WM_MDIRESTORE	    = 0x0223;
		public const int WM_MDINEXT	    = 0x0224;
		public const int WM_MDIMAXIMIZE	    = 0x0225;
		public const int WM_MDITILE	    = 0x0226;
		public const int WM_MDICASCADE	    = 0x0227;
		public const int WM_MDIICONARRANGE   = 0x0228;
		public const int WM_MDIGETACTIVE     = 0x0229;
		public const int WM_MDIREFRESHMENU   = 0x0234;

		/* D&D messages */
		public const int WM_DROPOBJECT	    = 0x022A;
		public const int WM_QUERYDROPOBJECT  = 0x022B;
		public const int WM_BEGINDRAG	    = 0x022C;
		public const int WM_DRAGLOOP	    = 0x022D;
		public const int WM_DRAGSELECT	    = 0x022E;
		public const int WM_DRAGMOVE	    = 0x022F;
		public const int WM_MDISETMENU	    = 0x0230;

		public const int WM_ENTERSIZEMOVE    = 0x0231;
		public const int WM_EXITSIZEMOVE     = 0x0232;
		public const int WM_DROPFILES	    = 0x0233;


		/* Win32 4.0 messages for IME */
		public const int IME_SETCONTEXT           = 0x0281;
		public const int IME_NOTIFY               = 0x0282;
		public const int IME_CONTROL              = 0x0283;
		public const int IME_COMPOSITIONFULL      = 0x0284;
		public const int IME_SELECT               = 0x0285;
		public const int IME_CHAR                 = 0x0286;
		/* Win32 5.0 messages for IME */
		public const int IME_REQUEST              = 0x0288;

		/* Win32 4.0 messages for IME */
		public const int IME_KEYDOWN              = 0x0290;
		public const int IME_KEYUP                = 0x0291;

		/* Clipboard command messages */
		public const int WM_CUT               = 0x0300;
		public const int WM_COPY              = 0x0301;
		public const int WM_PASTE             = 0x0302;
		public const int WM_CLEAR             = 0x0303;
		public const int WM_UNDO              = 0x0304;

		/* Clipboard owner messages */
		public const int WM_RENDERFORMAT      = 0x0305;
		public const int WM_RENDERALLFORMATS  = 0x0306;
		public const int WM_DESTROYCLIPBOARD  = 0x0307;

		/* Clipboard viewer messages */
		public const int WM_DRAWCLIPBOARD     = 0x0308;
		public const int WM_PAINTCLIPBOARD    = 0x0309;
		public const int WM_VSCROLLCLIPBOARD  = 0x030A;
		public const int WM_SIZECLIPBOARD     = 0x030B;
		public const int WM_ASKCBFORMATNAME   = 0x030C;
		public const int WM_CHANGECBCHAIN     = 0x030D;
		public const int WM_HSCROLLCLIPBOARD  = 0x030E;

		public const int WM_QUERYNEWPALETTE   = 0x030F;
		public const int WM_PALETTEISCHANGING = 0x0310;
		public const int WM_PALETTECHANGED    = 0x0311;
		public const int WM_HOTKEY	     = 0x0312;

		public const int WM_PRINT             = 0x0317;
		public const int WM_PRINTCLIENT       = 0x0318;

		public const int WM_PENWINFIRST      = 0x0380;
		public const int WM_PENWINLAST       = 0x038F;


		public const int WM_APP               = 0x8000;

		// Window Styles
		public const int WS_OVERLAPPED = 0x00000000;
		public const int WS_POPUP = (int) 0x80000000;
		public const int WS_CHILD = 0x40000000;
		public const int WS_MINIMIZE = 0x20000000;
		public const int WS_VISIBLE = 0x10000000;
		public const int WS_DISABLED = 0x08000000;
		public const int WS_CLIPSIBLINGS = 0x04000000;
		public const int WS_CLIPCHILDREN = 0x02000000;
		public const int WS_MAXIMIZE = 0x01000000;
		public const int WS_CAPTION = 0x00C00000;
		public const int WS_BORDER = 0x00800000L;
		public const int WS_DLGFRAME = 0x00400000;
		public const int WS_VSCROLL = 0x00200000;
		public const int WS_HSCROLL = 0x00100000;
		public const int WS_SYSMENU = 0x00080000;
		public const int WS_THICKFRAME = 0x00040000;
		public const int WS_GROUP = 0x00020000;
		public const int WS_TABSTOP = 0x00010000;
		public const int WS_MINIMIZEBOX = 0x00020000;
		public const int WS_MAXIMIZEBOX = 0x00010000;
		public const int WS_OVERLAPPEDWINDOW = WS_OVERLAPPED | 
		  WS_CAPTION | WS_SYSMENU | WS_THICKFRAME| 
		  WS_MINIMIZEBOX | WS_MAXIMIZEBOX;

		// ShowWindow types
		public const int SW_SHOW = 5;

		// SetWindowPos types
		public const int SWP_NOSIZE          = 0x0001;
		public const int SWP_NOMOVE          = 0x0002;
		public const int SWP_NOZORDER        = 0x0004;
		public const int SWP_NOREDRAW        = 0x0008;
		public const int SWP_NOACTIVATE      = 0x0010;
		public const int SWP_FRAMECHANGED    = 0x0020;
		public const int SWP_SHOWWINDOW      = 0x0040;
		public const int SWP_HIDEWINDOW      = 0x0080;
		public const int SWP_NOCOPYBITS      = 0x0100;
		public const int SWP_NOOWNERZORDER   = 0x0200;
		public const int SWP_DRAWFRAME       = SWP_FRAMECHANGED;
		public const int SWP_NOREPOSITION    = SWP_NOOWNERZORDER;
		public const int SWP_NOSENDCHANGING  = 0x0400;
		public const int SWP_DEFERERASE      = 0x2000;
		public const int SWP_ASYNCWINDOWPOS  = 0x4000;

	}
}
