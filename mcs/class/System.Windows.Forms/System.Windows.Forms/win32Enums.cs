/*
 * Copyright (C) 5/11/2002 Carlos Harvey Perez 
 * 
 *  Minor additons for project Mono by
 *  John Sohn (jsohn@columbus.rr.com)
 *	Dennis Hayes (dennish@raytek.com)
 *
 * Permission is hereby granted, free of charge, to any person obtaining
 * a copy of this software and associated documentation files (the
 * "Software"), to deal in the Software without restriction, including
 * without limitation the rights to use, copy, modify, merge, publish,
 * distribute, sublicense, and/or sell copies of the Software, and to
 * permit persons to whom the Software is furnished to do so, subject
 * to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be
 * included in all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
 * EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
 * OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
 * NONINFRINGEMENT.
 * IN NO EVENT SHALL CARLOS HARVEY PEREZ BE LIABLE FOR ANY CLAIM,
 * DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR
 * OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR
 * THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 * 
 * Except as contained in this notice, the name of Carlos Harvey Perez
 * shall not be used in advertising or otherwise to promote the sale,
 * use or other dealings in this Software without prior written
 * authorization from Carlos Harvey Perez.
 */

using System;
using System.Runtime.InteropServices;
using System.Drawing;


//namespace UtilityLibrary.Win32
namespace System.Windows.Forms{
	/// <summary>
	/// Window API enumerations
	/// </summary>
	
	#region Peek Message Flags
	internal enum PeekMessageFlags {
		PM_NOREMOVE		= 0,
		PM_REMOVE		= 1,
		PM_NOYIELD		= 2
	}
	#endregion
	
	#region Windows Messages
	public enum Msg {
		WM_NULL                   = 0x0000,
		WM_CREATE                 = 0x0001,
		WM_DESTROY                = 0x0002,
		WM_MOVE                   = 0x0003,
		WM_SIZE                   = 0x0005,
		WM_ACTIVATE               = 0x0006,
		WM_SETFOCUS               = 0x0007,
		WM_KILLFOCUS              = 0x0008,
		//		public const uint WM_SETVISIBLE           = 0x0009;
		WM_ENABLE                 = 0x000A,
		WM_SETREDRAW              = 0x000B,
		WM_SETTEXT                = 0x000C,
		WM_GETTEXT                = 0x000D,
		WM_GETTEXTLENGTH          = 0x000E,
		WM_PAINT                  = 0x000F,
		WM_CLOSE                  = 0x0010,
		WM_QUERYENDSESSION        = 0x0011,
		WM_QUIT                   = 0x0012,
		WM_QUERYOPEN              = 0x0013,
		WM_ERASEBKGND             = 0x0014,
		WM_SYSCOLORCHANGE         = 0x0015,
		WM_ENDSESSION             = 0x0016,
		//		public const uint WM_SYSTEMERROR          = 0x0017;
		WM_SHOWWINDOW             = 0x0018,
		WM_CTLCOLOR               = 0x0019,
		WM_WININICHANGE           = 0x001A,
		WM_SETTINGCHANGE          = 0x001A,
		WM_DEVMODECHANGE          = 0x001B,
		WM_ACTIVATEAPP            = 0x001C,
		WM_FONTCHANGE             = 0x001D,
		WM_TIMECHANGE             = 0x001E,
		WM_CANCELMODE             = 0x001F,
		WM_SETCURSOR              = 0x0020,
		WM_MOUSEACTIVATE          = 0x0021,
		WM_CHILDACTIVATE          = 0x0022,
		WM_QUEUESYNC              = 0x0023,
		WM_GETMINMAXINFO          = 0x0024,
		WM_PAINTICON              = 0x0026,
		WM_ICONERASEBKGND         = 0x0027,
		WM_NEXTDLGCTL             = 0x0028,
		//		public const uint WM_ALTTABACTIVE         = 0x0029;
		WM_SPOOLERSTATUS          = 0x002A,
		WM_DRAWITEM               = 0x002B,
		WM_MEASUREITEM            = 0x002C,
		WM_DELETEITEM             = 0x002D,
		WM_VKEYTOITEM             = 0x002E,
		WM_CHARTOITEM             = 0x002F,
		WM_SETFONT                = 0x0030,
		WM_GETFONT                = 0x0031,
		WM_SETHOTKEY              = 0x0032,
		WM_GETHOTKEY              = 0x0033,
		//		public const uint WM_FILESYSCHANGE        = 0x0034;
		//		public const uint WM_ISACTIVEICON         = 0x0035;
		//		public const uint WM_QUERYPARKICON        = 0x0036;
		WM_QUERYDRAGICON          = 0x0037,
		WM_COMPAREITEM            = 0x0039,
		//		public const uint WM_TESTING              = 0x003a;
		//		public const uint WM_OTHERWINDOWCREATED	= 0x003c;
		WM_GETOBJECT              = 0x003D,
		//			public const uint WM_ACTIVATESHELLWINDOW	= 0x003e;
		WM_COMPACTING             = 0x0041,
		WM_COMMNOTIFY             = 0x0044 ,
		WM_WINDOWPOSCHANGING      = 0x0046,
		WM_WINDOWPOSCHANGED       = 0x0047,
		WM_POWER                  = 0x0048,
		WM_COPYDATA               = 0x004A,
		WM_CANCELJOURNAL          = 0x004B,
		WM_NOTIFY                 = 0x004E,
		WM_INPUTLANGCHANGEREQUEST = 0x0050,
		WM_INPUTLANGCHANGE        = 0x0051,
		WM_TCARD                  = 0x0052,
		WM_HELP                   = 0x0053,
		WM_USERCHANGED            = 0x0054,
		WM_NOTIFYFORMAT           = 0x0055,
		WM_CONTEXTMENU            = 0x007B,
		WM_STYLECHANGING          = 0x007C,
		WM_STYLECHANGED           = 0x007D,
		WM_DISPLAYCHANGE          = 0x007E,
		WM_GETICON                = 0x007F,
		WM_SETICON                = 0x0080,
		WM_NCCREATE               = 0x0081,
		WM_NCDESTROY              = 0x0082,
		WM_NCCALCSIZE             = 0x0083,
		WM_NCHITTEST              = 0x0084,
		WM_NCPAINT                = 0x0085,
		WM_NCACTIVATE             = 0x0086,
		WM_GETDLGCODE             = 0x0087,
		WM_SYNCPAINT              = 0x0088,
		//		public const uint WM_SYNCTASK	    = 0x0089;
		WM_NCMOUSEMOVE            = 0x00A0,
		WM_NCLBUTTONDOWN          = 0x00A1,
		WM_NCLBUTTONUP            = 0x00A2,
		WM_NCLBUTTONDBLCLK        = 0x00A3,
		WM_NCRBUTTONDOWN          = 0x00A4,
		WM_NCRBUTTONUP            = 0x00A5,
		WM_NCRBUTTONDBLCLK        = 0x00A6,
		WM_NCMBUTTONDOWN          = 0x00A7,
		WM_NCMBUTTONUP            = 0x00A8,
		WM_NCMBUTTONDBLCLK        = 0x00A9,
		//		public const uint WM_NCXBUTTONDOWN    = 0x00ab;
		//		public const uint WM_NCXBUTTONUP      = 0x00ac;
		//		public const uint WM_NCXBUTTONDBLCLK  = 0x00ad;
		WM_KEYDOWN                = 0x0100,
		WM_KEYFIRST               = 0x0100,
		WM_KEYUP                  = 0x0101,
		WM_CHAR                   = 0x0102,
		WM_DEADCHAR               = 0x0103,
		WM_SYSKEYDOWN             = 0x0104,
		WM_SYSKEYUP               = 0x0105,
		WM_SYSCHAR                = 0x0106,
		WM_SYSDEADCHAR            = 0x0107,
		WM_KEYLAST                = 0x0108,
		WM_IME_STARTCOMPOSITION   = 0x010D,
		WM_IME_ENDCOMPOSITION     = 0x010E,
		WM_IME_COMPOSITION        = 0x010F,
		WM_IME_KEYLAST            = 0x010F,
		WM_INITDIALOG             = 0x0110,
		WM_COMMAND                = 0x0111,
		WM_SYSCOMMAND             = 0x0112,
		WM_TIMER                  = 0x0113,
		WM_HSCROLL                = 0x0114,
		WM_VSCROLL                = 0x0115,
		WM_INITMENU               = 0x0116,
		WM_INITMENUPOPUP          = 0x0117,
		//		public const uint WM_SYSTIMER	    = 0x0118;
		WM_MENUSELECT             = 0x011F,
		WM_MENUCHAR               = 0x0120,
		WM_ENTERIDLE              = 0x0121,
		WM_MENURBUTTONUP          = 0x0122,
		WM_MENUDRAG               = 0x0123,
		WM_MENUGETOBJECT          = 0x0124,
		WM_UNINITMENUPOPUP        = 0x0125,
		WM_MENUCOMMAND            = 0x0126,
		//		public const uint WM_CHANGEUISTATE    = 0x0127;
		//		public const uint WM_UPDATEUISTATE    = 0x0128;
		//		public const uint WM_QUERYUISTATE     = 0x0129;

		//		public const uint WM_LBTRACKPOINT     = 0x0131;
		WM_CTLCOLORMSGBOX         = 0x0132,
		WM_CTLCOLOREDIT           = 0x0133,
		WM_CTLCOLORLISTBOX        = 0x0134,
		WM_CTLCOLORBTN            = 0x0135,
		WM_CTLCOLORDLG            = 0x0136,
		WM_CTLCOLORSCROLLBAR      = 0x0137,
		WM_CTLCOLORSTATIC         = 0x0138,
		WM_MOUSEMOVE              = 0x0200,
		WM_MOUSEFIRST			  = 0x0200, 
		WM_LBUTTONDOWN            = 0x0201,
		WM_LBUTTONUP              = 0x0202,
		WM_LBUTTONDBLCLK          = 0x0203,
		WM_RBUTTONDOWN            = 0x0204,
		WM_RBUTTONUP              = 0x0205,
		WM_RBUTTONDBLCLK          = 0x0206,
		WM_MBUTTONDOWN            = 0x0207,
		WM_MBUTTONUP              = 0x0208,
		WM_MBUTTONDBLCLK          = 0x0209,
		WM_MOUSEWHEEL             = 0x020A,
		WM_MOUSELAST             = 0x020D,
		//		public const uint WM_XBUTTONDOWN      = 0x020B;
		//		public const uint WM_XBUTTONUP        = 0x020C;
		//		public const uint WM_XBUTTONDBLCLK    = 0x020D;
		WM_PARENTNOTIFY           = 0x0210,
		WM_ENTERMENULOOP          = 0x0211,
		WM_EXITMENULOOP           = 0x0212,
		WM_NEXTMENU               = 0x0213,
		WM_SIZING                 = 0x0214,
		WM_CAPTURECHANGED         = 0x0215,
		WM_MOVING                 = 0x0216,
		//		public const uint WM_POWERBROADCAST   = 0x0218;
		WM_DEVICECHANGE           = 0x0219,
		WM_MDICREATE              = 0x0220,
		WM_MDIDESTROY             = 0x0221,
		WM_MDIACTIVATE            = 0x0222,
		WM_MDIRESTORE             = 0x0223,
		WM_MDINEXT                = 0x0224,
		WM_MDIMAXIMIZE            = 0x0225,
		WM_MDITILE                = 0x0226,
		WM_MDICASCADE             = 0x0227,
		WM_MDIICONARRANGE         = 0x0228,
		WM_MDIGETACTIVE           = 0x0229,
		/* D&D messages */
		//		public const uint WM_DROPOBJECT	    = 0x022A;
		//		public const uint WM_QUERYDROPOBJECT  = 0x022B;
		//		public const uint WM_BEGINDRAG	    = 0x022C;
		//		public const uint WM_DRAGLOOP	    = 0x022D;
		//		public const uint WM_DRAGSELECT	    = 0x022E;
		//		public const uint WM_DRAGMOVE	    = 0x022F;
		WM_MDISETMENU             = 0x0230,
		WM_ENTERSIZEMOVE          = 0x0231,
		WM_EXITSIZEMOVE           = 0x0232,
		WM_DROPFILES              = 0x0233,
		WM_MDIREFRESHMENU         = 0x0234,
		WM_IME_SETCONTEXT         = 0x0281,
		WM_IME_NOTIFY             = 0x0282,
		WM_IME_CONTROL            = 0x0283,
		WM_IME_COMPOSITIONFULL    = 0x0284,
		WM_IME_SELECT             = 0x0285,
		WM_IME_CHAR               = 0x0286,
		WM_IME_REQUEST            = 0x0288,
		WM_IME_KEYDOWN            = 0x0290,
		WM_IME_KEYUP              = 0x0291,
		WM_MOUSEHOVER             = 0x02A1,
		WM_MOUSELEAVE             = 0x02A3,
		WM_CUT                    = 0x0300,
		WM_COPY                   = 0x0301,
		WM_PASTE                  = 0x0302,
		WM_CLEAR                  = 0x0303,
		WM_UNDO                   = 0x0304,
		WM_RENDERFORMAT           = 0x0305,
		WM_RENDERALLFORMATS       = 0x0306,
		WM_DESTROYCLIPBOARD       = 0x0307,
		WM_DRAWCLIPBOARD          = 0x0308,
		WM_PAINTCLIPBOARD         = 0x0309,
		WM_VSCROLLCLIPBOARD       = 0x030A,
		WM_SIZECLIPBOARD          = 0x030B,
		WM_ASKCBFORMATNAME        = 0x030C,
		WM_CHANGECBCHAIN          = 0x030D,
		WM_HSCROLLCLIPBOARD       = 0x030E,
		WM_QUERYNEWPALETTE        = 0x030F,
		WM_PALETTEISCHANGING      = 0x0310,
		WM_PALETTECHANGED         = 0x0311,
		WM_HOTKEY                 = 0x0312,
		WM_PRINT                  = 0x0317,
		WM_PRINTCLIENT            = 0x0318,
		WM_HANDHELDFIRST          = 0x0358,
		WM_HANDHELDLAST           = 0x035F,
		WM_AFXFIRST               = 0x0360,
		WM_AFXLAST                = 0x037F,
		WM_PENWINFIRST            = 0x0380,
		WM_PENWINLAST             = 0x038F,
		WM_APP                    = 0x8000,
		WM_USER                   = 0x0400,
		WM_REFLECT                = WM_USER + 0x1c00
	}
	#endregion

	#region Window Styles
	[Flags]
	public enum WindowStyles : long {
		WS_OVERLAPPED       = 0x00000000,
		WS_POPUP            = 0x80000000,
		WS_CHILD            = 0x40000000,
		WS_MINIMIZE         = 0x20000000,
		WS_VISIBLE          = 0x10000000,
		WS_DISABLED         = 0x08000000,
		WS_CLIPSIBLINGS     = 0x04000000,
		WS_CLIPCHILDREN     = 0x02000000,
		WS_MAXIMIZE         = 0x01000000,
		WS_CAPTION          = 0x00C00000,
		WS_BORDER           = 0x00800000,
		WS_DLGFRAME         = 0x00400000,
		WS_VSCROLL          = 0x00200000,
		WS_HSCROLL          = 0x00100000,
		WS_SYSMENU          = 0x00080000,
		WS_THICKFRAME       = 0x00040000,
		WS_GROUP            = 0x00020000,
		WS_TABSTOP          = 0x00010000,
		WS_MINIMIZEBOX      = 0x00020000,
		WS_MAXIMIZEBOX      = 0x00010000,
		WS_TILED            = 0x00000000,
		WS_ICONIC           = 0x20000000,
		WS_SIZEBOX          = 0x00040000,
		WS_POPUPWINDOW      = 0x80880000,
		WS_OVERLAPPEDWINDOW = WS_OVERLAPPED | WS_CAPTION | WS_SYSMENU | WS_THICKFRAME | WS_MINIMIZEBOX | WS_MAXIMIZEBOX,
		WS_TILEDWINDOW      = WS_OVERLAPPEDWINDOW,
		WS_CHILDWINDOW      = WS_CHILD
	}
	#endregion

	#region Window Extended Styles
	public enum WindowExStyles {
		WS_EX_DLGMODALFRAME     = 0x00000001,
		WS_EX_NOPARENTNOTIFY    = 0x00000004,
		WS_EX_TOPMOST           = 0x00000008,
		WS_EX_ACCEPTFILES       = 0x00000010,
		WS_EX_TRANSPARENT       = 0x00000020,
		WS_EX_MDICHILD          = 0x00000040,
		WS_EX_TOOLWINDOW        = 0x00000080,
		WS_EX_WINDOWEDGE        = 0x00000100,
		WS_EX_CLIENTEDGE        = 0x00000200,
		WS_EX_CONTEXTHELP       = 0x00000400,
		WS_EX_RIGHT             = 0x00001000,
		WS_EX_LEFT              = 0x00000000,
		WS_EX_RTLREADING        = 0x00002000,
		WS_EX_LTRREADING        = 0x00000000,
		WS_EX_LEFTSCROLLBAR     = 0x00004000,
		WS_EX_RIGHTSCROLLBAR    = 0x00000000,
		WS_EX_CONTROLPARENT     = 0x00010000,
		WS_EX_STATICEDGE        = 0x00020000,
		WS_EX_APPWINDOW         = 0x00040000,
		WS_EX_OVERLAPPEDWINDOW  = 0x00000300,
		WS_EX_PALETTEWINDOW     = 0x00000188,
		WS_EX_LAYERED			= 0x00080000
	}
	#endregion

	#region ShowWindow Styles
	public enum ShowWindowStyles : short {
		SW_HIDE             = 0,
		SW_SHOWNORMAL       = 1,
		SW_NORMAL           = 1,
		SW_SHOWMINIMIZED    = 2,
		SW_SHOWMAXIMIZED    = 3,
		SW_MAXIMIZE         = 3,
		SW_SHOWNOACTIVATE   = 4,
		SW_SHOW             = 5,
		SW_MINIMIZE         = 6,
		SW_SHOWMINNOACTIVE  = 7,
		SW_SHOWNA           = 8,
		SW_RESTORE          = 9,
		SW_SHOWDEFAULT      = 10,
		SW_FORCEMINIMIZE    = 11,
		SW_MAX              = 11
		//		public const uint SW_NORMALNA         = 0xCC;
	}
	#endregion

	#region SetWindowPos Z Order
	public enum SetWindowPosZOrder {
		HWND_TOP        = 0,
		HWND_BOTTOM     = 1,
		HWND_TOPMOST    = -1,
		HWND_NOTOPMOST  = -2
		//		public const int HWND_MESSAGE = -3;
	}
	#endregion

	#region SetWindowPosFlags
	[Flags]
	public enum SetWindowPosFlags {
		SWP_NOSIZE          = 0x0001,
		SWP_NOMOVE          = 0x0002,
		SWP_NOZORDER        = 0x0004,
		SWP_NOREDRAW        = 0x0008,
		SWP_NOACTIVATE      = 0x0010,
		SWP_FRAMECHANGED    = 0x0020,
		SWP_SHOWWINDOW      = 0x0040,
		SWP_HIDEWINDOW      = 0x0080,
		SWP_NOCOPYBITS      = 0x0100,
		SWP_NOOWNERZORDER   = 0x0200, 
		SWP_NOSENDCHANGING  = 0x0400,
		SWP_DRAWFRAME       = SWP_FRAMECHANGED,
		SWP_NOREPOSITION    = SWP_NOOWNERZORDER,
		SWP_DEFERERASE      = 0x2000,
		SWP_ASYNCWINDOWPOS  = 0x4000
	}
	#endregion

	#region Virtual Keys
	public enum VirtualKeys {
		VK_LBUTTON		= 0x01,
		VK_CANCEL		= 0x03,
		VK_BACK			= 0x08,
		VK_TAB			= 0x09,
		VK_CLEAR		= 0x0C,
		VK_RETURN		= 0x0D,
		VK_SHIFT		= 0x10,
		VK_CONTROL		= 0x11,
		VK_MENU			= 0x12,
		VK_CAPITAL		= 0x14,
		VK_ESCAPE		= 0x1B,
		VK_SPACE		= 0x20,
		VK_PRIOR		= 0x21,
		VK_NEXT			= 0x22,
		VK_END			= 0x23,
		VK_HOME			= 0x24,
		VK_LEFT			= 0x25,
		VK_UP			= 0x26,
		VK_RIGHT		= 0x27,
		VK_DOWN			= 0x28,
		VK_SELECT		= 0x29,
		VK_EXECUTE		= 0x2B,
		VK_SNAPSHOT		= 0x2C,
		VK_HELP			= 0x2F,
		VK_0			= 0x30,
		VK_1			= 0x31,
		VK_2			= 0x32,
		VK_3			= 0x33,
		VK_4			= 0x34,
		VK_5			= 0x35,
		VK_6			= 0x36,
		VK_7			= 0x37,
		VK_8			= 0x38,
		VK_9			= 0x39,
		VK_A			= 0x41,
		VK_B			= 0x42,
		VK_C			= 0x43,
		VK_D			= 0x44,
		VK_E			= 0x45,
		VK_F			= 0x46,
		VK_G			= 0x47,
		VK_H			= 0x48,
		VK_I			= 0x49,
		VK_J			= 0x4A,
		VK_K			= 0x4B,
		VK_L			= 0x4C,
		VK_M			= 0x4D,
		VK_N			= 0x4E,
		VK_O			= 0x4F,
		VK_P			= 0x50,
		VK_Q			= 0x51,
		VK_R			= 0x52,
		VK_S			= 0x53,
		VK_T			= 0x54,
		VK_U			= 0x55,
		VK_V			= 0x56,
		VK_W			= 0x57,
		VK_X			= 0x58,
		VK_Y			= 0x59,
		VK_Z			= 0x5A,
		VK_NUMPAD0		= 0x60,
		VK_NUMPAD1		= 0x61,
		VK_NUMPAD2		= 0x62,
		VK_NUMPAD3		= 0x63,
		VK_NUMPAD4		= 0x64,
		VK_NUMPAD5		= 0x65,
		VK_NUMPAD6		= 0x66,
		VK_NUMPAD7		= 0x67,
		VK_NUMPAD8		= 0x68,
		VK_NUMPAD9		= 0x69,
		VK_MULTIPLY		= 0x6A,
		VK_ADD			= 0x6B,
		VK_SEPARATOR	= 0x6C,
		VK_SUBTRACT		= 0x6D,
		VK_DECIMAL		= 0x6E,
		VK_DIVIDE		= 0x6F,
		VK_ATTN			= 0xF6,
		VK_CRSEL		= 0xF7,
		VK_EXSEL		= 0xF8,
		VK_EREOF		= 0xF9,
		VK_PLAY			= 0xFA,  
		VK_ZOOM			= 0xFB,
		VK_NONAME		= 0xFC,
		VK_PA1			= 0xFD,
		VK_OEM_CLEAR	= 0xFE,
		VK_LWIN			= 0x5B,
		VK_RWIN			= 0x5C,
		VK_APPS			= 0x5D,   
		VK_LSHIFT		= 0xA0,   
		VK_RSHIFT		= 0xA1,   
		VK_LCONTROL		= 0xA2,   
		VK_RCONTROL		= 0xA3,   
		VK_LMENU		= 0xA4,   
		VK_RMENU		= 0xA5
	}
	#endregion
		
	#region PatBlt Types
	public enum PatBltTypes {
		SRCCOPY          =   0x00CC0020,
		SRCPAINT         =   0x00EE0086,
		SRCAND           =   0x008800C6,
		SRCINVERT        =   0x00660046,
		SRCERASE         =   0x00440328,
		NOTSRCCOPY       =   0x00330008,
		NOTSRCERASE      =   0x001100A6,
		MERGECOPY        =   0x00C000CA,
		MERGEPAINT       =   0x00BB0226,
		PATCOPY          =   0x00F00021,
		PATPAINT         =   0x00FB0A09,
		PATINVERT        =   0x005A0049,
		DSTINVERT        =   0x00550009,
		BLACKNESS        =   0x00000042,
		WHITENESS        =   0x00FF0062
	}
	#endregion
	
	#region Clipboard Formats
	public enum ClipboardFormats {			
		CF_TEXT             = 1,
		CF_BITMAP           = 2,
		CF_METAFILEPICT     = 3,
		CF_SYLK             = 4,
		CF_DIF              = 5,
		CF_TIFF             = 6,
		CF_OEMTEXT          = 7,
		CF_DIB              = 8,
		CF_PALETTE          = 9,
		CF_PENDATA          = 10,
		CF_RIFF             = 11,
		CF_WAVE             = 12,
		CF_UNICODETEXT      = 13,
		CF_ENHMETAFILE      = 14,
		CF_HDROP            = 15,
		CF_LOCALE           = 16,
		CF_MAX              = 17,
		CF_OWNERDISPLAY     = 0x0080,
		CF_DSPTEXT          = 0x0081,
		CF_DSPBITMAP        = 0x0082,
		CF_DSPMETAFILEPICT  = 0x0083,
		CF_DSPENHMETAFILE   = 0x008E,
		CF_PRIVATEFIRST     = 0x0200,
		CF_PRIVATELAST      = 0x02FF,
		CF_GDIOBJFIRST      = 0x0300,
		CF_GDIOBJLAST       = 0x03FF
	}
	#endregion

	#region Common Controls Initialization flags
	public enum CommonControlInitFlags : int {
		ICC_LISTVIEW_CLASSES   = 0x00000001, 
		ICC_TREEVIEW_CLASSES   = 0x00000002, 
		ICC_BAR_CLASSES        = 0x00000004, 
		ICC_TAB_CLASSES        = 0x00000008, 
		ICC_UPDOWN_CLASS       = 0x00000010, 
		ICC_PROGRESS_CLASS     = 0x00000020, 
		ICC_HOTKEY_CLASS       = 0x00000040, 
		ICC_ANIMATE_CLASS      = 0x00000080, 
		ICC_WIN95_CLASSES      = 0x000000FF,
		ICC_DATE_CLASSES       = 0x00000100, 
		ICC_USEREX_CLASSES     = 0x00000200,
		ICC_COOL_CLASSES       = 0x00000400, 
		ICC_INTERNET_CLASSES   = 0x00000800,
		ICC_PAGESCROLLER_CLASS = 0x00001000, 
		ICC_NATIVEFNTCTL_CLASS = 0x00002000  
	}
	#endregion

	#region Common Controls Styles
	public  enum CommonControlStyles {
		CCS_TOP                 = 0x00000001,
		CCS_NOMOVEY             = 0x00000002,
		CCS_BOTTOM              = 0x00000003,
		CCS_NORESIZE            = 0x00000004,
		CCS_NOPARENTALIGN       = 0x00000008,
		CCS_ADJUSTABLE          = 0x00000020,
		CCS_NODIVIDER           = 0x00000040,
		CCS_VERT                = 0x00000080,
		CCS_LEFT                = (CCS_VERT | CCS_TOP),
		CCS_RIGHT               = (CCS_VERT | CCS_BOTTOM),
		CCS_NOMOVEX             = (CCS_VERT | CCS_NOMOVEY)
	}
	#endregion

	#region ToolBar Styles
	public enum ToolBarStyles {
		TBSTYLE_BUTTON          = 0x0000,
		TBSTYLE_SEP             = 0x0001,
		TBSTYLE_CHECK           = 0x0002,
		TBSTYLE_GROUP           = 0x0004,
		TBSTYLE_CHECKGROUP      = (TBSTYLE_GROUP | TBSTYLE_CHECK),
		TBSTYLE_DROPDOWN        = 0x0008,
		TBSTYLE_AUTOSIZE        = 0x0010,
		TBSTYLE_NOPREFIX        = 0x0020, 
		TBSTYLE_TOOLTIPS        = 0x0100,
		TBSTYLE_WRAPABLE        = 0x0200,
		TBSTYLE_ALTDRAG         = 0x0400,
		TBSTYLE_FLAT            = 0x0800,
		TBSTYLE_LIST            = 0x1000,
		TBSTYLE_CUSTOMERASE     = 0x2000,
		TBSTYLE_REGISTERDROP    = 0x4000,
		TBSTYLE_TRANSPARENT     = 0x8000,
		TBSTYLE_EX_DRAWDDARROWS = 0x00000001
	}
	#endregion

	#region ToolBar Ex Styles
	public enum ToolBarExStyles {
		TBSTYLE_EX_DRAWDDARROWS			= 0x1,
		TBSTYLE_EX_HIDECLIPPEDBUTTONS	= 0x10,
		TBSTYLE_EX_DOUBLEBUFFER			= 0x80
	}
	#endregion

	#region ToolBar Messages
	public enum ToolBarMessages {
		WM_USER                 =  0x0400,
		TB_ENABLEBUTTON         = (WM_USER + 1),
		TB_CHECKBUTTON          = (WM_USER + 2),
		TB_PRESSBUTTON          = (WM_USER + 3),
		TB_HIDEBUTTON           = (WM_USER + 4),
		TB_INDETERMINATE        = (WM_USER + 5),
		TB_MARKBUTTON           = (WM_USER + 6),
		TB_ISBUTTONENABLED      = (WM_USER + 9),
		TB_ISBUTTONCHECKED      = (WM_USER + 10),
		TB_ISBUTTONPRESSED      = (WM_USER + 11),
		TB_ISBUTTONHIDDEN       = (WM_USER + 12),
		TB_ISBUTTONINDETERMINATE= (WM_USER + 13),
		TB_ISBUTTONHIGHLIGHTED  = (WM_USER + 14),
		TB_SETSTATE             = (WM_USER + 17),
		TB_GETSTATE             = (WM_USER + 18),
		TB_ADDBITMAP            = (WM_USER + 19),
		TB_ADDBUTTONSA          = (WM_USER + 20),
		TB_INSERTBUTTONA        = (WM_USER + 21),
		TB_ADDBUTTONS           = (WM_USER + 20),
		TB_INSERTBUTTON         = (WM_USER + 21),
		TB_DELETEBUTTON         = (WM_USER + 22),
		TB_GETBUTTON            = (WM_USER + 23),
		TB_BUTTONCOUNT          = (WM_USER + 24),
		TB_COMMANDTOINDEX       = (WM_USER + 25),
		TB_SAVERESTOREA         = (WM_USER + 26),
		TB_CUSTOMIZE            = (WM_USER + 27),
		TB_ADDSTRINGA           = (WM_USER + 28),
		TB_GETITEMRECT          = (WM_USER + 29),
		TB_BUTTONSTRUCTSIZE     = (WM_USER + 30),
		TB_SETBUTTONSIZE        = (WM_USER + 31),
		TB_SETBITMAPSIZE        = (WM_USER + 32),
		TB_AUTOSIZE             = (WM_USER + 33),
		TB_GETTOOLTIPS          = (WM_USER + 35),
		TB_SETTOOLTIPS          = (WM_USER + 36),
		TB_SETPARENT            = (WM_USER + 37),
		TB_SETROWS              = (WM_USER + 39),
		TB_GETROWS              = (WM_USER + 40),
		TB_GETBITMAPFLAGS       = (WM_USER + 41),
		TB_SETCMDID             = (WM_USER + 42),
		TB_CHANGEBITMAP         = (WM_USER + 43),
		TB_GETBITMAP            = (WM_USER + 44),
		TB_GETBUTTONTEXTA       = (WM_USER + 45),
		TB_GETBUTTONTEXTW       = (WM_USER + 75),
		TB_REPLACEBITMAP        = (WM_USER + 46),
		TB_SETINDENT            = (WM_USER + 47),
		TB_SETIMAGELIST         = (WM_USER + 48),
		TB_GETIMAGELIST         = (WM_USER + 49),
		TB_LOADIMAGES           = (WM_USER + 50),
		TB_GETRECT              = (WM_USER + 51),
		TB_SETHOTIMAGELIST      = (WM_USER + 52),
		TB_GETHOTIMAGELIST      = (WM_USER + 53),
		TB_SETDISABLEDIMAGELIST = (WM_USER + 54),
		TB_GETDISABLEDIMAGELIST = (WM_USER + 55),
		TB_SETSTYLE             = (WM_USER + 56),
		TB_GETSTYLE             = (WM_USER + 57),
		TB_GETBUTTONSIZE        = (WM_USER + 58),
		TB_SETBUTTONWIDTH       = (WM_USER + 59),
		TB_SETMAXTEXTROWS       = (WM_USER + 60),
		TB_GETTEXTROWS          = (WM_USER + 61),
		TB_GETOBJECT            = (WM_USER + 62), 
		TB_GETBUTTONINFOW       = (WM_USER + 63),
		TB_SETBUTTONINFOW       = (WM_USER + 64),
		TB_GETBUTTONINFOA       = (WM_USER + 65),
		TB_SETBUTTONINFOA       = (WM_USER + 66),
		TB_INSERTBUTTONW        = (WM_USER + 67),
		TB_ADDBUTTONSW          = (WM_USER + 68),
		TB_HITTEST              = (WM_USER + 69),
		TB_SETDRAWTEXTFLAGS     = (WM_USER + 70),
		TB_GETHOTITEM           = (WM_USER + 71),
		TB_SETHOTITEM           = (WM_USER + 72), 
		TB_SETANCHORHIGHLIGHT   = (WM_USER + 73),  
		TB_GETANCHORHIGHLIGHT   = (WM_USER + 74),
		TB_SAVERESTOREW         = (WM_USER + 76),
		TB_ADDSTRINGW           = (WM_USER + 77),
		TB_MAPACCELERATORA      = (WM_USER + 78), 
		TB_GETINSERTMARK        = (WM_USER + 79), 
		TB_SETINSERTMARK        = (WM_USER + 80), 
		TB_INSERTMARKHITTEST    = (WM_USER + 81),  
		TB_MOVEBUTTON           = (WM_USER + 82),
		TB_GETMAXSIZE           = (WM_USER + 83),  
		TB_SETEXTENDEDSTYLE     = (WM_USER + 84),  
		TB_GETEXTENDEDSTYLE     = (WM_USER + 85),  
		TB_GETPADDING           = (WM_USER + 86),
		TB_SETPADDING           = (WM_USER + 87),
		TB_SETINSERTMARKCOLOR   = (WM_USER + 88),
		TB_GETINSERTMARKCOLOR   = (WM_USER + 89)
	}
	#endregion

	#region ToolBar Notifications
	public enum ToolBarNotifications {
		TTN_NEEDTEXTA		= ((0-520)-0),
		TTN_NEEDTEXTW		= ((0-520)-10),
		TBN_QUERYINSERT		= ((0-700)-6),
		TBN_DROPDOWN		= ((0-700)-10),
		TBN_HOTITEMCHANGE	= ((0 - 700) - 13)
	}
	#endregion

	#region Reflected Messages
	public enum ReflectedMessages {
		OCM__BASE				= (Msg.WM_USER+0x1c00),
		OCM_COMMAND				= (OCM__BASE + Msg.WM_COMMAND),
		OCM_CTLCOLORBTN			= (OCM__BASE + Msg.WM_CTLCOLORBTN),
		OCM_CTLCOLOREDIT		= (OCM__BASE + Msg.WM_CTLCOLOREDIT),
		OCM_CTLCOLORDLG			= (OCM__BASE + Msg.WM_CTLCOLORDLG),
		OCM_CTLCOLORLISTBOX		= (OCM__BASE + Msg.WM_CTLCOLORLISTBOX),
		OCM_CTLCOLORMSGBOX		= (OCM__BASE + Msg.WM_CTLCOLORMSGBOX),
		OCM_CTLCOLORSCROLLBAR   = (OCM__BASE + Msg.WM_CTLCOLORSCROLLBAR),
		OCM_CTLCOLORSTATIC		= (OCM__BASE + Msg.WM_CTLCOLORSTATIC),
		OCM_CTLCOLOR			= (OCM__BASE + Msg.WM_CTLCOLOR),
		OCM_DRAWITEM			= (OCM__BASE + Msg.WM_DRAWITEM),
		OCM_MEASUREITEM			= (OCM__BASE + Msg.WM_MEASUREITEM),
		OCM_DELETEITEM          = (OCM__BASE + Msg.WM_DELETEITEM),
		OCM_VKEYTOITEM          = (OCM__BASE + Msg.WM_VKEYTOITEM),
		OCM_CHARTOITEM          = (OCM__BASE + Msg.WM_CHARTOITEM),
		OCM_COMPAREITEM         = (OCM__BASE + Msg.WM_COMPAREITEM),
		OCM_HSCROLL             = (OCM__BASE + Msg.WM_HSCROLL),
		OCM_VSCROLL             = (OCM__BASE + Msg.WM_VSCROLL),
		OCM_PARENTNOTIFY        = (OCM__BASE + Msg.WM_PARENTNOTIFY),
		OCM_NOTIFY              = (OCM__BASE + Msg.WM_NOTIFY)
	}
	#endregion

	#region Notification Messages
	public enum NotificationMessages {
		NM_FIRST      = (0-0),
		NM_CUSTOMDRAW = (NM_FIRST-12),
		NM_NCHITTEST  = (NM_FIRST-14) 
	}
	#endregion

	#region ToolTip Flags
	public enum ToolTipFlags {
		TTF_IDISHWND            = 0x0001,
		TTF_CENTERTIP           = 0x0002,
		TTF_RTLREADING          = 0x0004,
		TTF_SUBCLASS            = 0x0010,
		TTF_TRACK               = 0x0020,
		TTF_ABSOLUTE            = 0x0080,
		TTF_TRANSPARENT         = 0x0100,
		TTF_DI_SETITEM          = 0x8000   
	}
	#endregion

	public enum ToolTipNotifications : int {
		TTN_FIRST               = 0-520,
		TTN_GETDISPINFO        = (TTN_FIRST - 10),
		TTN_SHOW                = (TTN_FIRST - 1),
		TTN_POP                 = (TTN_FIRST - 2)
	}

	#region Custom Draw Return Flags
	public enum CustomDrawReturnFlags {
		CDRF_DODEFAULT          = 0x00000000,
		CDRF_NEWFONT            = 0x00000002,
		CDRF_SKIPDEFAULT        = 0x00000004,
		CDRF_NOTIFYPOSTPAINT    = 0x00000010,
		CDRF_NOTIFYITEMDRAW     = 0x00000020,
		CDRF_NOTIFYSUBITEMDRAW  = 0x00000020, 
		CDRF_NOTIFYPOSTERASE    = 0x00000040
	}
	#endregion

	#region Custom Draw Item State Flags
	public enum CustomDrawItemStateFlags {
		CDIS_SELECTED       = 0x0001,
		CDIS_GRAYED         = 0x0002,
		CDIS_DISABLED       = 0x0004,
		CDIS_CHECKED        = 0x0008,
		CDIS_FOCUS          = 0x0010,
		CDIS_DEFAULT        = 0x0020,
		CDIS_HOT            = 0x0040,
		CDIS_MARKED         = 0x0080,
		CDIS_INDETERMINATE  = 0x0100
	}
	#endregion

	#region Custom Draw Draw State Flags
	public enum CustomDrawDrawStateFlags {
		CDDS_PREPAINT           = 0x00000001,
		CDDS_POSTPAINT          = 0x00000002,
		CDDS_PREERASE           = 0x00000003,
		CDDS_POSTERASE          = 0x00000004,
		CDDS_ITEM               = 0x00010000,
		CDDS_ITEMPREPAINT       = (CDDS_ITEM | CDDS_PREPAINT),
		CDDS_ITEMPOSTPAINT      = (CDDS_ITEM | CDDS_POSTPAINT),
		CDDS_ITEMPREERASE       = (CDDS_ITEM | CDDS_PREERASE),
		CDDS_ITEMPOSTERASE      = (CDDS_ITEM | CDDS_POSTERASE),
		CDDS_SUBITEM            = 0x00020000
	}
	#endregion

	#region Toolbar button info flags
	public enum ToolBarButtonInfoFlags {
		TBIF_IMAGE             = 0x00000001,
		TBIF_TEXT              = 0x00000002,
		TBIF_STATE             = 0x00000004,
		TBIF_STYLE             = 0x00000008,
		TBIF_LPARAM            = 0x00000010,
		TBIF_COMMAND           = 0x00000020,
		TBIF_SIZE              = 0x00000040,
		I_IMAGECALLBACK        = -1,
		I_IMAGENONE            = -2
	}
	#endregion

	#region Toolbar button styles
	public enum ToolBarButtonStyles {
		TBSTYLE_BUTTON          = 0x0000,
		TBSTYLE_SEP             = 0x0001,
		TBSTYLE_CHECK           = 0x0002,
		TBSTYLE_GROUP           = 0x0004,
		TBSTYLE_CHECKGROUP      = (TBSTYLE_GROUP | TBSTYLE_CHECK),
		TBSTYLE_DROPDOWN        = 0x0008,
		TBSTYLE_AUTOSIZE        = 0x0010,
		TBSTYLE_NOPREFIX        = 0x0020, 
		TBSTYLE_TOOLTIPS        = 0x0100,
		TBSTYLE_WRAPABLE        = 0x0200,
		TBSTYLE_ALTDRAG         = 0x0400,
		TBSTYLE_FLAT            = 0x0800,
		TBSTYLE_LIST            = 0x1000,
		TBSTYLE_CUSTOMERASE     = 0x2000,
		TBSTYLE_REGISTERDROP    = 0x4000,
		TBSTYLE_TRANSPARENT     = 0x8000,
		TBSTYLE_EX_DRAWDDARROWS = 0x00000001
	}
	#endregion

	#region Toolbar button state
	public enum ToolBarButtonStates {
		TBSTATE_CHECKED         = 0x01,
		TBSTATE_PRESSED         = 0x02,
		TBSTATE_ENABLED         = 0x04,
		TBSTATE_HIDDEN          = 0x08,
		TBSTATE_INDETERMINATE   = 0x10,
		TBSTATE_WRAP            = 0x20,
		TBSTATE_ELLIPSES        = 0x40,
		TBSTATE_MARKED          = 0x80
	}
	#endregion

	#region Windows Hook Codes
	public enum WindowsHookCodes {
		WH_MSGFILTER        = (-1),
		WH_JOURNALRECORD    = 0,
		WH_JOURNALPLAYBACK  = 1,
		WH_KEYBOARD         = 2,
		WH_GETMESSAGE       = 3,
		WH_CALLWNDPROC      = 4,
		WH_CBT              = 5,
		WH_SYSMSGFILTER     = 6,
		WH_MOUSE            = 7,
		WH_HARDWARE         = 8,
		WH_DEBUG            = 9,
		WH_SHELL            = 10,
		WH_FOREGROUNDIDLE   = 11,
		WH_CALLWNDPROCRET   = 12,
		WH_KEYBOARD_LL      = 13,
		WH_MOUSE_LL         = 14
	}
		  
	#endregion

	#region Mouse Hook Filters
	public enum MouseHookFilters {
		MSGF_DIALOGBOX      = 0,
		MSGF_MESSAGEBOX     = 1,
		MSGF_MENU           = 2,
		MSGF_SCROLLBAR      = 5,
		MSGF_NEXTWINDOW     = 6
	}

	#endregion

	#region Draw Text format flags
	public enum DrawTextFormatFlags {
		DT_TOP              = 0x00000000,
		DT_LEFT             = 0x00000000,
		DT_CENTER           = 0x00000001,
		DT_RIGHT            = 0x00000002,
		DT_VCENTER          = 0x00000004,
		DT_BOTTOM           = 0x00000008,
		DT_WORDBREAK        = 0x00000010,
		DT_SINGLELINE       = 0x00000020,
		DT_EXPANDTABS       = 0x00000040,
		DT_TABSTOP          = 0x00000080,
		DT_NOCLIP           = 0x00000100,
		DT_EXTERNALLEADING  = 0x00000200,
		DT_CALCRECT         = 0x00000400,
		DT_NOPREFIX         = 0x00000800,
		DT_INTERNAL         = 0x00001000,
		DT_EDITCONTROL      = 0x00002000,
		DT_PATH_ELLIPSIS    = 0x00004000,
		DT_END_ELLIPSIS     = 0x00008000,
		DT_MODIFYSTRING     = 0x00010000,
		DT_RTLREADING       = 0x00020000,
		DT_WORD_ELLIPSIS    = 0x00040000
	}

	#endregion

	#region Rebar Styles
	public enum RebarStyles {
		RBS_TOOLTIPS        = 0x0100,
		RBS_VARHEIGHT       = 0x0200,
		RBS_BANDBORDERS     = 0x0400,
		RBS_FIXEDORDER      = 0x0800,
		RBS_REGISTERDROP    = 0x1000,
		RBS_AUTOSIZE        = 0x2000,
		RBS_VERTICALGRIPPER = 0x4000, 
		RBS_DBLCLKTOGGLE    = 0x8000,
	}
	#endregion

	#region Rebar Notifications
	public enum RebarNotifications {
		RBN_FIRST           = (0-831),
		RBN_HEIGHTCHANGE    = (RBN_FIRST - 0),
		RBN_GETOBJECT       = (RBN_FIRST - 1),
		RBN_LAYOUTCHANGED   = (RBN_FIRST - 2),
		RBN_AUTOSIZE        = (RBN_FIRST - 3),
		RBN_BEGINDRAG       = (RBN_FIRST - 4),
		RBN_ENDDRAG         = (RBN_FIRST - 5),
		RBN_DELETINGBAND    = (RBN_FIRST - 6),   
		RBN_DELETEDBAND     = (RBN_FIRST - 7),    
		RBN_CHILDSIZE       = (RBN_FIRST - 8),
		RBN_CHEVRONPUSHED   = (RBN_FIRST - 10)
	}
	#endregion

	#region Rebar Messages
	public enum RebarMessages {
		CCM_FIRST           =    0x2000,
		WM_USER             =    0x0400,
		RB_INSERTBANDA		=	(WM_USER +  1),
		RB_DELETEBAND       =	(WM_USER +  2),
		RB_GETBARINFO		=	(WM_USER +  3),
		RB_SETBARINFO		=	(WM_USER +  4),
		RB_GETBANDINFO		=	(WM_USER +  5),
		RB_SETBANDINFOA		=	(WM_USER +  6),
		RB_SETPARENT		=	(WM_USER +  7),
		RB_HITTEST			=	(WM_USER +  8),
		RB_GETRECT			=	(WM_USER +  9),
		RB_INSERTBANDW		=	(WM_USER +  10),
		RB_SETBANDINFOW		=	(WM_USER +  11),
		RB_GETBANDCOUNT		=	(WM_USER +  12),
		RB_GETROWCOUNT		=	(WM_USER +  13),
		RB_GETROWHEIGHT		=	(WM_USER +  14),
		RB_IDTOINDEX		=	(WM_USER +  16),
		RB_GETTOOLTIPS		=	(WM_USER +  17),
		RB_SETTOOLTIPS		=	(WM_USER +  18),
		RB_SETBKCOLOR		=	(WM_USER +  19),
		RB_GETBKCOLOR		=	(WM_USER +  20), 
		RB_SETTEXTCOLOR		=	(WM_USER +  21),
		RB_GETTEXTCOLOR		=	(WM_USER +  22),
		RB_SIZETORECT		=	(WM_USER +  23), 
		RB_SETCOLORSCHEME	=	(CCM_FIRST + 2),  
		RB_GETCOLORSCHEME	=	(CCM_FIRST + 3), 
		RB_BEGINDRAG		=	(WM_USER + 24),
		RB_ENDDRAG			=	(WM_USER + 25),
		RB_DRAGMOVE			=	(WM_USER + 26),
		RB_GETBARHEIGHT		=	(WM_USER + 27),
		RB_GETBANDINFOW		=	(WM_USER + 28),
		RB_GETBANDINFOA		=	(WM_USER + 29),
		RB_MINIMIZEBAND		=	(WM_USER + 30),
		RB_MAXIMIZEBAND		=	(WM_USER + 31),
		RB_GETDROPTARGET	=	(CCM_FIRST + 4),
		RB_GETBANDBORDERS	=	(WM_USER + 34),  
		RB_SHOWBAND			=	(WM_USER + 35),      
		RB_SETPALETTE		=	(WM_USER + 37),
		RB_GETPALETTE		=	(WM_USER + 38),
		RB_MOVEBAND			=	(WM_USER + 39),
		RB_SETUNICODEFORMAT =   (CCM_FIRST + 5),
		RB_GETUNICODEFORMAT =   (CCM_FIRST + 6)
	}
	#endregion

	#region Rebar Info Mask
	public enum RebarInfoMask {
		RBBIM_STYLE         = 0x00000001,
		RBBIM_COLORS        = 0x00000002,
		RBBIM_TEXT          = 0x00000004,
		RBBIM_IMAGE         = 0x00000008,
		RBBIM_CHILD         = 0x00000010,
		RBBIM_CHILDSIZE     = 0x00000020,
		RBBIM_SIZE          = 0x00000040,
		RBBIM_BACKGROUND    = 0x00000080,
		RBBIM_ID            = 0x00000100,
		RBBIM_IDEALSIZE     = 0x00000200,
		RBBIM_LPARAM        = 0x00000400,
		BBIM_HEADERSIZE     = 0x00000800  
	}
	#endregion

	#region Rebar Styles
	public enum RebarStylesEx {
		RBBS_BREAK			=	0x1,
		RBBS_CHILDEDGE		=	0x4,
		RBBS_FIXEDBMP		=	0x20,
		RBBS_GRIPPERALWAYS	=	0x80,
		RBBS_USECHEVRON		=	0x200
	}
	#endregion

	#region Object types
	public enum ObjectTypes {
		OBJ_PEN             = 1,
		OBJ_BRUSH           = 2,
		OBJ_DC              = 3,
		OBJ_METADC          = 4,
		OBJ_PAL             = 5,
		OBJ_FONT            = 6,
		OBJ_BITMAP          = 7,
		OBJ_REGION          = 8,
		OBJ_METAFILE        = 9,
		OBJ_MEMDC           = 10,
		OBJ_EXTPEN          = 11,
		OBJ_ENHMETADC       = 12,
		OBJ_ENHMETAFILE     = 13
	}
	#endregion

	#region WM_MENUCHAR return values
	public enum MenuCharReturnValues {
		MNC_IGNORE  = 0,
		MNC_CLOSE   = 1,
		MNC_EXECUTE = 2,
		MNC_SELECT  = 3
	}
	#endregion

	#region Background Mode
	public enum BackgroundMode {
		TRANSPARENT = 1,
		OPAQUE = 2
	}
	#endregion

	#region ListView Messages
	public enum ListViewMessages {
		LVM_FIRST           =    0x1000,
		LVM_GETSUBITEMRECT  = (LVM_FIRST + 56),
		LVM_GETITEMSTATE    = (LVM_FIRST + 44),
		LVM_GETITEMTEXTW    = (LVM_FIRST + 115)
	}
	#endregion

	#region Header Control Messages
	public enum HeaderControlMessages {
		HDM_FIRST        =  0x1200,
		HDM_GETITEMRECT  = (HDM_FIRST + 7),
		HDM_HITTEST      = (HDM_FIRST + 6),
		HDM_SETIMAGELIST = (HDM_FIRST + 8),
		HDM_GETITEMW     = (HDM_FIRST + 11),
		HDM_ORDERTOINDEX = (HDM_FIRST + 15)
	}
	#endregion

	#region Header Control Notifications
	public enum HeaderControlNotifications {
		HDN_FIRST			 =	(0-300),
		HDN_ITEMCLICKW		 =	(HDN_FIRST-22),
		HDN_ITEMDBLCLICKW	 =	(HDN_FIRST-23),
		HDN_DIVIDERDBLCLICKW =   (HDN_FIRST-25),
		HDN_BEGINTRACKW		 =	(HDN_FIRST-26),
		HDN_ENDTRACKW		 =	(HDN_FIRST-27)
	}
	#endregion

	#region Header Control HitTest Flags
	public enum HeaderControlHitTestFlags {
		HHT_NOWHERE             = 0x0001,
		HHT_ONHEADER            = 0x0002,
		HHT_ONDIVIDER           = 0x0004,
		HHT_ONDIVOPEN           = 0x0008,
		HHT_ABOVE               = 0x0100,
		HHT_BELOW               = 0x0200,
		HHT_TORIGHT             = 0x0400,
		HHT_TOLEFT              = 0x0800
	}
	#endregion

	#region List View sub item portion
	public enum SubItemPortion {
		LVIR_BOUNDS = 0,
		LVIR_ICON   = 1,
		LVIR_LABEL  = 2
	}
	#endregion

	#region Cursor Type
	public enum CursorType {
		IDC_ARROW		= 32512,
		IDC_IBEAM       = 32513,
		IDC_WAIT        = 32514,
		IDC_CROSS       = 32515,
		IDC_UPARROW     = 32516,
		IDC_SIZE        = 32640,
		IDC_ICON        = 32641,
		IDC_SIZENWSE    = 32642,
		IDC_SIZENESW    = 32643,
		IDC_SIZEWE      = 32644,
		IDC_SIZENS      = 32645,
		IDC_SIZEALL     = 32646,
		IDC_NO          = 32648,
		IDC_HAND        = 32649,
		IDC_APPSTARTING = 32650,
		IDC_HELP        = 32651
	}
	#endregion
	
	#region Tracker Event Flags
	public enum TrackerEventFlags : long {
		TME_HOVER	= 0x00000001,
		TME_LEAVE	= 0x00000002,
		TME_QUERY	= 0x40000000,
		TME_CANCEL	= 0x80000000
	}
	#endregion

	#region Mouse Activate Flags
	public enum MouseActivateFlags {
		MA_ACTIVATE			= 1,
		MA_ACTIVATEANDEAT   = 2,
		MA_NOACTIVATE       = 3,
		MA_NOACTIVATEANDEAT = 4
	}
	#endregion

	#region Dialog Codes
	public enum DialogCodes {
		DLGC_WANTARROWS			= 0x0001,
		DLGC_WANTTAB			= 0x0002,
		DLGC_WANTALLKEYS		= 0x0004,
		DLGC_WANTMESSAGE		= 0x0004,
		DLGC_HASSETSEL			= 0x0008,
		DLGC_DEFPUSHBUTTON		= 0x0010,
		DLGC_UNDEFPUSHBUTTON	= 0x0020,
		DLGC_RADIOBUTTON		= 0x0040,
		DLGC_WANTCHARS			= 0x0080,
		DLGC_STATIC				= 0x0100,
		DLGC_BUTTON				= 0x2000
	}
	#endregion

	#region Update Layered Windows Flags
	public enum UpdateLayeredWindowFlags {
		ULW_COLORKEY = 0x00000001,
		ULW_ALPHA    = 0x00000002,
		ULW_OPAQUE   = 0x00000004
	}
	#endregion

	#region Blend Flags
	public enum BlendFlags : byte {
		AC_SRC_OVER  = 0x00,
		AC_SRC_ALPHA = 0x01
	}
	#endregion

	#region ComboBox styles
	internal enum ComboBoxStyles : uint {
		CBS_SIMPLE            =0x0001,
		CBS_DROPDOWN          =0x0002,
		CBS_DROPDOWNLIST      =0x0003,
		CBS_OWNERDRAWFIXED    =0x0010,
		CBS_OWNERDRAWVARIABLE =0x0020,
		CBS_AUTOHSCROLL       =0x0040,
		CBS_OEMCONVERT        =0x0080,
		CBS_SORT              =0x0100,
		CBS_HASSTRINGS        =0x0200,
		CBS_NOINTEGRALHEIGHT  =0x0400,
		CBS_DISABLENOSCROLL   =0x0800,
		CBS_UPPERCASE         =0x2000,
		CBS_LOWERCASE         =0x4000 
	}
	#endregion

	#region ComboBox messages
	public enum ComboBoxMessages {
		CB_GETEDITSEL            =0x140,
		CB_LIMITTEXT             =0x141,
		CB_SETEDITSEL            =0x142,
		CB_ADDSTRING             =0x143,
		CB_DELETESTRING          =0x144,
		CB_DIR                   =0x145,
		CB_GETCOUNT              =0x146,
		CB_GETCURSEL             =0x147,
		CB_GETLBTEXT             =0x148,
		CB_GETLBTEXTLEN          =0x149,
		CB_INSERTSTRING          =0x14A,
		CB_RESETCONTENT          =0x14B,
		CB_FINDSTRING            =0x14C,
		CB_SELECTSTRING          =0x14D,
		CB_SETCURSEL             =0x14E,
		CB_SHOWDROPDOWN          =0x14F,
		CB_GETITEMDATA           =0x150,
		CB_SETITEMDATA           =0x151,
		CB_GETDROPPEDCONTROLRECT =0x152,
		CB_SETITEMHEIGHT         =0x153,
		CB_GETITEMHEIGHT         =0x154,
		CB_SETEXTENDEDUI         =0x155,
		CB_GETEXTENDEDUI         =0x156,
		CB_GETDROPPEDSTATE       =0x157,
		CB_FINDSTRINGEXACT       =0x158,
		CB_SETLOCALE             =0x159,
		CB_GETLOCALE             =0x15A,
		CB_GETTOPINDEX           =0x15b,
		CB_SETTOPINDEX           =0x15c,
		CB_GETHORIZONTALEXTENT   =0x15d,
		CB_SETHORIZONTALEXTENT   =0x15e,
		CB_GETDROPPEDWIDTH       =0x15f,
		CB_SETDROPPEDWIDTH       =0x160,
		CB_INITSTORAGE           =0x161,
		CB_MSGMAX                =0x162,
		CB_MSGMAX_351            =0x15B
	}
	#endregion

	#region TreeView Messages
	public enum TreeViewMessages : int {
		TV_FIRST	    =  0x1100,
		TVM_INSERTITEMA     =  ( TV_FIRST + 0 ),
		TVM_DELETEITEM      =  ( TV_FIRST + 1 ),
		TVM_EXPAND          =  ( TV_FIRST + 2 ),
		TVM_GETITEMRECT	    =  ( TV_FIRST + 4 ),
		TVM_GETCOUNT        =  ( TV_FIRST + 5 ),
		TVM_GETINDENT       =  ( TV_FIRST + 6 ),
		TVM_SETINDENT       =  ( TV_FIRST + 7 ),
		TVM_GETIMAGELIST    =  ( TV_FIRST + 8 ),
		TVM_SETIMAGELIST    =  ( TV_FIRST + 9 ),
		TVM_GETNEXTITEM     =  ( TV_FIRST + 10 ),
		TVM_SELECTITEM      =  ( TV_FIRST + 11 ),
		TVM_GETITEMA        =  ( TV_FIRST + 12 ),
		TVM_SETITEMA        =  ( TV_FIRST + 13 ),
		TVM_EDITLABELA      =  ( TV_FIRST + 14 ),
		TVM_GETEDITCONTROL  =  ( TV_FIRST + 15 ),
		TVM_GETVISIBLECOUNT =  ( TV_FIRST + 16 ),
		TVM_HITTEST         =  ( TV_FIRST + 17 ),
		TVM_CREATEDRAGIMAGE =  ( TV_FIRST + 18 ),
		TVM_SORTCHILDREN    =  ( TV_FIRST + 19 ),
		TVM_ENSUREVISIBLE   =  ( TV_FIRST + 20 ),
		TVM_SORTCHILDRENCB  =  ( TV_FIRST + 21 ),
		TVM_SETITEMHEIGHT   =  ( TV_FIRST + 27 ),
		TVM_GETITEMHEIGHT   =  ( TV_FIRST + 28 ),
		TVM_SETBKCOLOR      =  ( TV_FIRST + 29 ),
		TVM_SETTEXTCOLOR    =  ( TV_FIRST + 30 ),
		TVM_GETITEMW	    =  ( TV_FIRST + 62 ),
		TVM_SETITEMW        =  ( TV_FIRST + 63 ),
		TVM_INSERTITEMW     =  ( TV_FIRST + 50 )
	}
	#endregion

	#region TreeViewImageListFlags
	public enum TreeViewImageListFlags {
		TVSIL_NORMAL  = 0,
		TVSIL_STATE   = 2
	}
	#endregion

	#region TreeViewItem Flags
	[Flags]
	public enum TreeViewItemFlags {
		TVIF_NONE               = 0x0000,
		TVIF_TEXT               = 0x0001,
		TVIF_IMAGE              = 0x0002,
		TVIF_PARAM              = 0x0004,
		TVIF_STATE              = 0x0008,
		TVIF_HANDLE             = 0x0010,
		TVIF_SELECTEDIMAGE      = 0x0020,
		TVIF_CHILDREN           = 0x0040,
		TVIF_INTEGRAL           = 0x0080,
		I_CHILDRENCALLBACK      = -1,
		LPSTR_TEXTCALLBACK      = -1,
		I_IMAGECALLBACK			= -1,
		I_IMAGENONE				= -2
	}
	#endregion

	public enum TreeViewItemSelFlags : int {
		TVGN_ROOT               = 0x0000,
		TVGN_NEXT               = 0x0001,
		TVGN_PREVIOUS           = 0x0002,
		TVGN_PARENT             = 0x0003,
		TVGN_CHILD              = 0x0004,
		TVGN_FIRSTVISIBLE       = 0x0005,
		TVGN_NEXTVISIBLE        = 0x0006,
		TVGN_PREVIOUSVISIBLE    = 0x0007,
		TVGN_DROPHILITE         = 0x0008,
		TVGN_CARET              = 0x0009,
		TVGN_LASTVISIBLE        = 0x000A
	}

	#region ListViewItem flags
	public enum ListViewItemFlags {
		LVIF_TEXT               = 0x0001,
		LVIF_IMAGE              = 0x0002,
		LVIF_PARAM              = 0x0004,
		LVIF_STATE              = 0x0008,
		LVIF_INDENT             = 0x0010,
		LVIF_NORECOMPUTE        = 0x0800
	}
	#endregion

	#region HeaderItem flags
	public enum HeaderItemFlags {
		HDI_WIDTH               = 0x0001,
		HDI_HEIGHT              = HDI_WIDTH,
		HDI_TEXT                = 0x0002,
		HDI_FORMAT              = 0x0004,
		HDI_LPARAM              = 0x0008,
		HDI_BITMAP              = 0x0010,
		HDI_IMAGE               = 0x0020,
		HDI_DI_SETITEM          = 0x0040,
		HDI_ORDER               = 0x0080
	}
	#endregion

	#region GetDCExFlags
	public enum GetDCExFlags {
		DCX_WINDOW           = 0x00000001,
		DCX_CACHE            = 0x00000002,
		DCX_NORESETATTRS     = 0x00000004,
		DCX_CLIPCHILDREN     = 0x00000008,
		DCX_CLIPSIBLINGS     = 0x00000010,
		DCX_PARENTCLIP       = 0x00000020,
		DCX_EXCLUDERGN       = 0x00000040,
		DCX_INTERSECTRGN     = 0x00000080,
		DCX_EXCLUDEUPDATE    = 0x00000100,
		DCX_INTERSECTUPDATE  = 0x00000200,
		DCX_LOCKWINDOWUPDATE = 0x00000400,
		DCX_VALIDATE         = 0x00200000
	}
	#endregion

	#region HitTest 
	public enum HitTest {
		HTERROR             = (-2),
		HTTRANSPARENT       = (-1),
		HTNOWHERE           =   0,
		HTCLIENT            =   1,
		HTCAPTION           =   2,
		HTSYSMENU           =   3,
		HTGROWBOX           =   4,
		HTSIZE              =   HTGROWBOX,
		HTMENU              =   5,
		HTHSCROLL           =   6,
		HTVSCROLL           =   7,
		HTMINBUTTON         =   8,
		HTMAXBUTTON         =   9,
		HTLEFT              =   10,
		HTRIGHT             =   11,
		HTTOP               =   12,
		HTTOPLEFT           =   13,
		HTTOPRIGHT          =   14,
		HTBOTTOM            =   15,
		HTBOTTOMLEFT        =   16,
		HTBOTTOMRIGHT       =   17,
		HTBORDER            =   18,
		HTREDUCE            =   HTMINBUTTON,
		HTZOOM              =   HTMAXBUTTON,
		HTSIZEFIRST         =   HTLEFT,
		HTSIZELAST          =   HTBOTTOMRIGHT,
		HTOBJECT            =   19,
		HTCLOSE             =   20,
		HTHELP              =   21
	}
	#endregion

	#region ActivateFlags
	public enum ActivateState {
		WA_INACTIVE     = 0,
		WA_ACTIVE       = 1,
		WA_CLICKACTIVE  = 2
	}
	#endregion

	#region StrechModeFlags
	public enum StrechModeFlags {
		BLACKONWHITE		= 1,
		WHITEONBLACK        = 2,
		COLORONCOLOR        = 3,
		HALFTONE            = 4,
		MAXSTRETCHBLTMODE   = 4
	}
	#endregion

	#region ScrollBarFlags
	public enum ScrollBarFlags {
		SBS_HORZ                    = 0x0000,
		SBS_VERT                    = 0x0001,
		SBS_TOPALIGN                = 0x0002,
		SBS_LEFTALIGN               = 0x0002,
		SBS_BOTTOMALIGN             = 0x0004,
		SBS_RIGHTALIGN              = 0x0004,
		SBS_SIZEBOXTOPLEFTALIGN     = 0x0002,
		SBS_SIZEBOXBOTTOMRIGHTALIGN = 0x0004,
		SBS_SIZEBOX                 = 0x0008,
		SBS_SIZEGRIP                = 0x0010
	}
	#endregion

	#region ScrollBarMessages
	public enum ScrollBarMessages 
	{
		SBM_SETPOS                  = 0x00E0,
		SBM_GETPOS                  = 0x00E1,
		SBM_SETRANGE                = 0x00E2,
		SBM_GETRANGE                = 0x00E3
	}
	#endregion

	#region System Metrics Codes
	public enum SystemMetricsCodes {
		SM_CXSCREEN             = 0,
		SM_CYSCREEN             = 1,
		SM_CXVSCROLL            = 2,
		SM_CYHSCROLL            = 3,
		SM_CYCAPTION            = 4,
		SM_CXBORDER             = 5,
		SM_CYBORDER             = 6,
		SM_CXDLGFRAME           = 7,
		SM_CYDLGFRAME           = 8,
		SM_CYVTHUMB             = 9,
		SM_CXHTHUMB             = 10,
		SM_CXICON               = 11,
		SM_CYICON               = 12,
		SM_CXCURSOR             = 13,
		SM_CYCURSOR             = 14,
		SM_CYMENU               = 15,
		SM_CXFULLSCREEN         = 16,
		SM_CYFULLSCREEN         = 17,
		SM_CYKANJIWINDOW        = 18,
		SM_MOUSEPRESENT         = 19,
		SM_CYVSCROLL            = 20,
		SM_CXHSCROLL            = 21,
		SM_DEBUG                = 22,
		SM_SWAPBUTTON           = 23,
		SM_RESERVED1            = 24,
		SM_RESERVED2            = 25,
		SM_RESERVED3            = 26,
		SM_RESERVED4            = 27,
		SM_CXMIN                = 28,
		SM_CYMIN                = 29,
		SM_CXSIZE               = 30,
		SM_CYSIZE               = 31,
		SM_CXFRAME              = 32,
		SM_CYFRAME              = 33,
		SM_CXMINTRACK           = 34,
		SM_CYMINTRACK           = 35,
		SM_CXDOUBLECLK          = 36,
		SM_CYDOUBLECLK          = 37,
		SM_CXICONSPACING        = 38,
		SM_CYICONSPACING        = 39,
		SM_MENUDROPALIGNMENT    = 40,
		SM_PENWINDOWS           = 41,
		SM_DBCSENABLED          = 42,
		SM_CMOUSEBUTTONS        = 43,
		SM_CXFIXEDFRAME         = SM_CXDLGFRAME, 
		SM_CYFIXEDFRAME         = SM_CYDLGFRAME,  
		SM_CXSIZEFRAME          = SM_CXFRAME,    
		SM_CYSIZEFRAME          = SM_CYFRAME,    
		SM_SECURE               = 44,
		SM_CXEDGE               = 45,
		SM_CYEDGE               = 46,
		SM_CXMINSPACING         = 47,
		SM_CYMINSPACING         = 48,
		SM_CXSMICON             = 49,
		SM_CYSMICON             = 50,
		SM_CYSMCAPTION          = 51,
		SM_CXSMSIZE             = 52,
		SM_CYSMSIZE             = 53,
		SM_CXMENUSIZE           = 54,
		SM_CYMENUSIZE           = 55,
		SM_ARRANGE              = 56,
		SM_CXMINIMIZED          = 57,
		SM_CYMINIMIZED          = 58,
		SM_CXMAXTRACK           = 59,
		SM_CYMAXTRACK           = 60,
		SM_CXMAXIMIZED          = 61,
		SM_CYMAXIMIZED          = 62,
		SM_NETWORK              = 63,
		SM_CLEANBOOT            = 67,
		SM_CXDRAG               = 68,
		SM_CYDRAG               = 69,
		SM_SHOWSOUNDS           = 70,
		SM_CXMENUCHECK          = 71,  
		SM_CYMENUCHECK          = 72,
		SM_SLOWMACHINE          = 73,
		SM_MIDEASTENABLED       = 74,
		SM_MOUSEWHEELPRESENT    = 75,
		SM_XVIRTUALSCREEN       = 76,
		SM_YVIRTUALSCREEN       = 77,
		SM_CXVIRTUALSCREEN      = 78,
		SM_CYVIRTUALSCREEN      = 79,
		SM_CMONITORS            = 80,
		SM_SAMEDISPLAYFORMAT    = 81,
		SM_CMETRICS             = 83
	}
	#endregion

	#region ScrollBarTypes
	public enum ScrollBarTypes {
		SB_HORZ  = 0,
		SB_VERT  = 1,
		SB_CTL   = 2,
		SB_BOTH  = 3
	}
	#endregion

	#region SrollBarInfoFlags
	public enum ScrollBarInfoFlags {
		SIF_RANGE           = 0x0001,
		SIF_PAGE            = 0x0002,
		SIF_POS             = 0x0004,
		SIF_DISABLENOSCROLL = 0x0008,
		SIF_TRACKPOS        = 0x0010,
		SIF_ALL             = (SIF_RANGE | SIF_PAGE | SIF_POS | SIF_TRACKPOS)
	}
	#endregion

	#region Enable ScrollBar flags
	public enum EnableScrollBarFlags {
		ESB_ENABLE_BOTH     = 0x0000,
		ESB_DISABLE_BOTH    = 0x0003,
		ESB_DISABLE_LEFT    = 0x0001,
		ESB_DISABLE_RIGHT   = 0x0002,
		ESB_DISABLE_UP      = 0x0001,
		ESB_DISABLE_DOWN    = 0x0002,
		ESB_DISABLE_LTUP    = ESB_DISABLE_LEFT,
		ESB_DISABLE_RTDN    = ESB_DISABLE_RIGHT
	}
	#endregion

	#region Scroll Requests
	public enum ScrollBarRequests {
		SB_LINEUP           = 0,
		SB_LINELEFT         = 0,
		SB_LINEDOWN         = 1,
		SB_LINERIGHT        = 1,
		SB_PAGEUP           = 2,
		SB_PAGELEFT         = 2,
		SB_PAGEDOWN         = 3,
		SB_PAGERIGHT        = 3,
		SB_THUMBPOSITION    = 4,
		SB_THUMBTRACK       = 5,
		SB_TOP              = 6,
		SB_LEFT             = 6,
		SB_BOTTOM           = 7,
		SB_RIGHT            = 7,
		SB_ENDSCROLL        = 8
	}
	#endregion

	#region SrollWindowEx flags
	public enum ScrollWindowExFlags {
		SW_SCROLLCHILDREN   = 0x0001,  
		SW_INVALIDATE       = 0x0002,  
		SW_ERASE            = 0x0004,  
		SW_SMOOTHSCROLL     = 0x0010  
	}
	#endregion

	#region ImageListFlags
	public enum  ImageListFlags {
		ILC_MASK             = 0x0001,
		ILC_COLOR            = 0x0000,
		ILC_COLORDDB         = 0x00FE,
		ILC_COLOR4           = 0x0004,
		ILC_COLOR8           = 0x0008,
		ILC_COLOR16          = 0x0010,
		ILC_COLOR24          = 0x0018,
		ILC_COLOR32          = 0x0020,
		ILC_PALETTE          = 0x0800      
	}
	#endregion

	#region ImageListDrawFlags
	public enum ImageListDrawFlags {
		ILD_NORMAL              = 0x0000,
		ILD_TRANSPARENT         = 0x0001,
		ILD_MASK                = 0x0010,
		ILD_IMAGE               = 0x0020,
		ILD_ROP                 = 0x0040,
		ILD_BLEND25             = 0x0002,
		ILD_BLEND50             = 0x0004,
		ILD_OVERLAYMASK         = 0x0F00
	}
	#endregion

	#region List View Notifications
	public enum ListViewNotifications {
		LVN_FIRST             =  (0-100),
		LVN_GETDISPINFOW      = (LVN_FIRST-77),
		LVN_SETDISPINFOA      = (LVN_FIRST-51)
	}
	#endregion

	#region Drive Type
	public enum DriveType {
		DRIVE_UNKNOWN     = 0,
		DRIVE_NO_ROOT_DIR = 1,
		DRIVE_REMOVABLE   = 2,
		DRIVE_FIXED       = 3,
		DRIVE_REMOTE      = 4,
		DRIVE_CDROM       = 5,
		DRIVE_RAMDISK     = 6
	}
	#endregion

	#region Shell File Info Flags
	public enum ShellFileInfoFlags {
		SHGFI_ICON              = 0x000000100,
		SHGFI_DISPLAYNAME       = 0x000000200,     
		SHGFI_TYPENAME          = 0x000000400,    
		SHGFI_ATTRIBUTES        = 0x000000800,   
		SHGFI_ICONLOCATION      = 0x000001000,     
		SHGFI_EXETYPE           = 0x000002000,     
		SHGFI_SYSICONINDEX      = 0x000004000,    
		SHGFI_LINKOVERLAY       = 0x000008000,     
		SHGFI_SELECTED          = 0x000010000,     
		SHGFI_ATTR_SPECIFIED    = 0x000020000,     
		SHGFI_LARGEICON         = 0x000000000,     
		SHGFI_SMALLICON         = 0x000000001,     
		SHGFI_OPENICON          = 0x000000002,     
		SHGFI_SHELLICONSIZE     = 0x000000004,     
		SHGFI_PIDL              = 0x000000008,     
		SHGFI_USEFILEATTRIBUTES = 0x000000010     
	}
	#endregion

	#region Shell Special Folder
	public enum ShellSpecialFolder {
		CSIDL_DESKTOP                   = 0x0000,
		CSIDL_INTERNET                  = 0x0001,
		CSIDL_PROGRAMS                  = 0x0002,
		CSIDL_CONTROLS                  = 0x0003,
		CSIDL_PRINTERS                  = 0x0004,
		CSIDL_PERSONAL                  = 0x0005,
		CSIDL_FAVORITES                 = 0x0006,
		CSIDL_STARTUP                   = 0x0007,
		CSIDL_RECENT                    = 0x0008,
		CSIDL_SENDTO                    = 0x0009,
		CSIDL_BITBUCKET                 = 0x000a,
		CSIDL_STARTMENU                 = 0x000b,
		CSIDL_DESKTOPDIRECTORY          = 0x0010,
		CSIDL_DRIVES                    = 0x0011,
		CSIDL_NETWORK                   = 0x0012,
		CSIDL_NETHOOD                   = 0x0013,
		CSIDL_FONTS                     = 0x0014,
		CSIDL_TEMPLATES                 = 0x0015,
		CSIDL_COMMON_STARTMENU          = 0x0016,
		CSIDL_COMMON_PROGRAMS           = 0X0017,
		CSIDL_COMMON_STARTUP            = 0x0018,
		CSIDL_COMMON_DESKTOPDIRECTORY   = 0x0019,
		CSIDL_APPDATA                   = 0x001a,
		CSIDL_PRINTHOOD                 = 0x001b,
		CSIDL_ALTSTARTUP                = 0x001d,         
		CSIDL_COMMON_ALTSTARTUP         = 0x001e,         
		CSIDL_COMMON_FAVORITES          = 0x001f,
		CSIDL_INTERNET_CACHE            = 0x0020,
		CSIDL_COOKIES                   = 0x0021,
		CSIDL_HISTORY                   = 0x0022
	}
	#endregion

	#region ShellEnumFlags
	[Flags]
	public enum ShellEnumFlags {
		SHCONTF_FOLDERS         = 32,      // for shell browser
		SHCONTF_NONFOLDERS      = 64,      // for default view
		SHCONTF_INCLUDEHIDDEN   = 128,     // for hidden/system objects
	}
	#endregion

	#region ShellGetDisplayNameOfFlags
	[Flags]
	public enum ShellGetDisplayNameOfFlags {
		SHGDN_NORMAL            = 0,        // default (display purpose)
		SHGDN_INFOLDER          = 1,        // displayed under a folder (relative)
		SHGDN_INCLUDE_NONFILESYS = 0x2000,  // if not set, display names for shell name space items that are not in the file system will fail.
		SHGDN_FORADDRESSBAR     = 0x4000,   // for displaying in the address (drives dropdown) bar
		SHGDN_FORPARSING        = 0x8000,   // for ParseDisplayName or path
	}
	#endregion

	#region STRRETFlags
	[Flags]
	public enum STRRETFlags {
		STRRET_WSTR     = 0x0000,			// Use STRRET.pOleStr
		STRRET_OFFSET   = 0x0001,			// Use STRRET.uOffset to Ansi
		STRRET_CSTR     = 0x0002			// Use STRRET.cStr
	}
	#endregion

	#region GetAttributeOfFlags
	[Flags]
	public enum GetAttributeOfFlags : long {
		DROPEFFECT_NONE	       = 0,
		DROPEFFECT_COPY	       = 1,
		DROPEFFECT_MOVE		   = 2,
		DROPEFFECT_LINK		   = 4,
		DROPEFFECT_SCROLL	   = 0x80000000,
		SFGAO_CANCOPY          = DROPEFFECT_COPY,	// Objects can be copied
		SFGAO_CANMOVE          = DROPEFFECT_MOVE,	// Objects can be moved
		SFGAO_CANLINK          = DROPEFFECT_LINK,	// Objects can be linked
		SFGAO_CANRENAME        = 0x00000010,		// Objects can be renamed
		SFGAO_CANDELETE        = 0x00000020,		// Objects can be deleted
		SFGAO_HASPROPSHEET     = 0x00000040,		// Objects have property sheets
		SFGAO_DROPTARGET       = 0x00000100,		// Objects are drop target
		SFGAO_CAPABILITYMASK   = 0x00000177,
		SFGAO_LINK             = 0x00010000,		// Shortcut (link)
		SFGAO_SHARE            = 0x00020000,		// shared
		SFGAO_READONLY         = 0x00040000,		// read-only
		SFGAO_GHOSTED          = 0x00080000,		// ghosted icon
		SFGAO_HIDDEN           = 0x00080000,		// hidden object
		SFGAO_DISPLAYATTRMASK  = 0x000F0000,
		SFGAO_FILESYSANCESTOR  = 0x10000000,		// It contains file system folder
		SFGAO_FOLDER           = 0x20000000,		// It's a folder.
		SFGAO_FILESYSTEM       = 0x40000000,		// is a file system thing (file/folder/root)
		SFGAO_HASSUBFOLDER     = 0x80000000,		// Expandable in the map pane
		SFGAO_CONTENTSMASK     = 0x80000000,
		SFGAO_VALIDATE         = 0x01000000,		// invalidate cached information
		SFGAO_REMOVABLE        = 0x02000000,		// is this removeable media?
		SFGAO_COMPRESSED       = 0x04000000,		// Object is compressed (use alt color)
		SFGAO_BROWSABLE        = 0x08000000,		// is in-place browsable
		SFGAO_NONENUMERATED    = 0x00100000,		// is a non-enumerated object
		SFGAO_NEWCONTENT       = 0x00200000			// should show bold in explorer tree
	}
	#endregion

	#region ListViewItemState
	public enum ListViewItemState {
		LVIS_FOCUSED            = 0x0001,
		LVIS_SELECTED           = 0x0002,
		LVIS_CUT                = 0x0004,
		LVIS_DROPHILITED        = 0x0008,
		LVIS_ACTIVATING         = 0x0020,
		LVIS_OVERLAYMASK        = 0x0F00,
		LVIS_STATEIMAGEMASK     = 0xF000
	}
	#endregion

	#region TreeViewItemInsertPosition
	public enum TreeViewItemInsertPosition : uint {
		TVI_ROOT                = 0xFFFF0000,
		TVI_FIRST               = 0xFFFF0001,
		TVI_LAST                = 0xFFFF0002,
		TVI_SORT                = 0xFFFF0003
	}
	#endregion

	#region TreeViewNotifications
	public enum TreeViewNotifications {
		TVN_FIRST               =  -400, 
		TVN_SELCHANGINGA        =(TVN_FIRST-1),
		TVN_SELCHANGINGW        =(TVN_FIRST-50),
		TVN_SELCHANGEDA         =(TVN_FIRST-2),
		TVN_SELCHANGEDW         =(TVN_FIRST-51),
		TVN_GETDISPINFOA        =(TVN_FIRST-3),
		TVN_GETDISPINFOW        =(TVN_FIRST-52),
		TVN_SETDISPINFOA        =(TVN_FIRST-4),
		TVN_SETDISPINFOW        =(TVN_FIRST-53),
		TVN_ITEMEXPANDINGA      = (TVN_FIRST-5),
		TVN_ITEMEXPANDINGW      = (TVN_FIRST-54),
		TVN_ITEMEXPANDEDA       = (TVN_FIRST-6),
		TVN_ITEMEXPANDEDW       = (TVN_FIRST-55),
		TVN_BEGINDRAGA          = (TVN_FIRST-7),
		TVN_BEGINDRAGW          = (TVN_FIRST-56),
		TVN_BEGINRDRAGA         = (TVN_FIRST-8),
		TVN_BEGINRDRAGW         = (TVN_FIRST-57),
		TVN_DELETEITEMA         = (TVN_FIRST-9),
		TVN_DELETEITEMW         = (TVN_FIRST-58),
		TVN_BEGINLABELEDITA     = (TVN_FIRST-10),
		TVN_BEGINLABELEDITW     = (TVN_FIRST-59),
		TVN_ENDLABELEDITA       = (TVN_FIRST-11),
		TVN_ENDLABELEDITW       = (TVN_FIRST-60),
		TVN_KEYDOWN             = (TVN_FIRST-12),
		TVN_GETINFOTIPA         = (TVN_FIRST-13),
		TVN_GETINFOTIPW         = (TVN_FIRST-14),
		TVN_SINGLEEXPAND        = (TVN_FIRST-15)
	}
	#endregion

	#region TreeViewItemExpansion
	public enum TreeViewItemExpansion {
		TVE_COLLAPSE            = 0x0001,
		TVE_EXPAND              = 0x0002,
		TVE_TOGGLE              = 0x0003,
		TVE_EXPANDPARTIAL       = 0x4000,
		TVE_COLLAPSERESET       = 0x8000
	}
	#endregion

	#region WinErrors
	public enum WinErrors {
		NOERROR = 0x00000000
	}
	#endregion

	#region TreeViewHitTest
	public enum TreeViewHitTestFlags {
		TVHT_NOWHERE           = 0x0001,
		TVHT_ONITEMICON        = 0x0002,
		TVHT_ONITEMLABEL       = 0x0004,
		TVHT_ONITEM            = (TVHT_ONITEMICON | TVHT_ONITEMLABEL | TVHT_ONITEMSTATEICON),
		TVHT_ONITEMINDENT      = 0x0008,
		TVHT_ONITEMBUTTON      = 0x0010,
		TVHT_ONITEMRIGHT       = 0x0020,
		TVHT_ONITEMSTATEICON   = 0x0040,
		TVHT_ABOVE             = 0x0100,
		TVHT_BELOW             = 0x0200,
		TVHT_TORIGHT           = 0x0400,
		TVHT_TOLEFT            = 0x0800
	}
	#endregion

	#region TreeViewItemState
	public enum TreeViewItemState {
		TVIS_SELECTED           = 0x0002,
		TVIS_CUT                = 0x0004,
		TVIS_DROPHILITED        = 0x0008,
		TVIS_BOLD               = 0x0010,
		TVIS_EXPANDED           = 0x0020,
		TVIS_EXPANDEDONCE       = 0x0040,
		TVIS_EXPANDPARTIAL      = 0x0080,
		TVIS_OVERLAYMASK        = 0x0F00,
		TVIS_STATEIMAGEMASK     = 0xF000,
		TVIS_USERMASK           = 0xF000
	}
	#endregion

	public enum TreeViewStyles : int {
		TVS_HASBUTTONS          = 0x0001,
		TVS_HASLINES            = 0x0002,
		TVS_LINESATROOT         = 0x0004,
		TVS_EDITLABELS          = 0x0008,
		TVS_DISABLEDRAGDROP     = 0x0010,
		TVS_SHOWSELALWAYS       = 0x0020,
		TVS_RTLREADING          = 0x0040,
		TVS_NOTOOLTIPS          = 0x0080,
		TVS_CHECKBOXES          = 0x0100,
		TVS_TRACKSELECT         = 0x0200,
		TVS_SINGLEEXPAND        = 0x0400,
		TVS_INFOTIP             = 0x0800,
		TVS_FULLROWSELECT       = 0x1000,
		TVS_NOSCROLL            = 0x2000,
		TVS_NONEVENHEIGHT       = 0x4000
	}

	#region Windows System Objects
	public enum SystemObject : long {
		// Reserved IDs for system objects
		OBJID_WINDOW        = 0x00000000,
		OBJID_SYSMENU       = 0xFFFFFFFF,
		OBJID_TITLEBAR      = 0xFFFFFFFE,
		OBJID_MENU          = 0xFFFFFFFD,
		OBJID_CLIENT        = 0xFFFFFFFC,
		OBJID_VSCROLL       = 0xFFFFFFFB,
		OBJID_HSCROLL       = 0xFFFFFFFA,
		OBJID_SIZEGRIP      = 0xFFFFFFF9,
		OBJID_CARET         = 0xFFFFFFF8,
		OBJID_CURSOR        = 0xFFFFFFF7,
		OBJID_ALERT         = 0xFFFFFFF6,
		OBJID_SOUND         = 0xFFFFFFF5
	}
	#endregion

	#region SystemState
	public enum SystemState {
		STATE_SYSTEM_UNAVAILABLE        = 0x00000001, // Disabled
		STATE_SYSTEM_SELECTED           = 0x00000002,
		STATE_SYSTEM_FOCUSED            = 0x00000004,
		STATE_SYSTEM_PRESSED            = 0x00000008,
		STATE_SYSTEM_CHECKED            = 0x00000010,
		STATE_SYSTEM_MIXED              = 0x00000020, // 3-state checkbox or toolbar button
		STATE_SYSTEM_READONLY           = 0x00000040,
		STATE_SYSTEM_HOTTRACKED         = 0x00000080,
		STATE_SYSTEM_DEFAULT            = 0x00000100,
		STATE_SYSTEM_EXPANDED           = 0x00000200,
		STATE_SYSTEM_COLLAPSED          = 0x00000400,
		STATE_SYSTEM_BUSY               = 0x00000800,
		STATE_SYSTEM_FLOATING           = 0x00001000, // Children "owned" not "contained" by parent
		STATE_SYSTEM_MARQUEED           = 0x00002000,
		STATE_SYSTEM_ANIMATED           = 0x00004000,
		STATE_SYSTEM_INVISIBLE          = 0x00008000,
		STATE_SYSTEM_OFFSCREEN          = 0x00010000,
		STATE_SYSTEM_SIZEABLE           = 0x00020000,
		STATE_SYSTEM_MOVEABLE           = 0x00040000,
		STATE_SYSTEM_SELFVOICING        = 0x00080000,
		STATE_SYSTEM_FOCUSABLE          = 0x00100000,
		STATE_SYSTEM_SELECTABLE         = 0x00200000,
		STATE_SYSTEM_LINKED             = 0x00400000,
		STATE_SYSTEM_TRAVERSED          = 0x00800000,
		STATE_SYSTEM_MULTISELECTABLE    = 0x01000000,  // Supports multiple selection
		STATE_SYSTEM_EXTSELECTABLE      = 0x02000000,  // Supports extended selection
		STATE_SYSTEM_ALERT_LOW          = 0x04000000,  // This information is of low priority
		STATE_SYSTEM_ALERT_MEDIUM       = 0x08000000,  // This information is of medium priority
		STATE_SYSTEM_ALERT_HIGH         = 0x10000000,  // This information is of high priority
		STATE_SYSTEM_VALID              = 0x1FFFFFFF
	}
	#endregion

	#region QueryContextMenuFlags
	public enum QueryContextMenuFlags : long {
		CMF_NORMAL              = 0x00000000,
		CMF_DEFAULTONLY         = 0x00000001,
		CMF_VERBSONLY           = 0x00000002,
		CMF_EXPLORE             = 0x00000004,
		CMF_NOVERBS             = 0x00000008,
		CMF_CANRENAME           = 0x00000010,
		CMF_NODEFAULT           = 0x00000020,
		CMF_INCLUDESTATIC       = 0x00000040,
		CMF_RESERVED            = 0xffff0000  
	}
	#endregion

	#region GetWindowLongFlag
	public enum GetWindowLongFlag {
		GWL_WNDPROC         = (-4),
		GWL_HINSTANCE       = (-6),
		GWL_HWNDPARENT      = (-8),
		GWL_STYLE           = (-16),
		GWL_EXSTYLE         = (-20),
		GWL_USERDATA        = (-21),
		GWL_ID              = (-12),
		DWL_MSGRESULT       = 0,
		DWL_DLGPROC         = 4,
		DWL_USER            = 8
	}
	#endregion

	#region CommonControlMessages
	public enum CommonControlMessages {
		CCM_FIRST               = 0x2000,			// Common control shared messages
		CCM_SETBKCOLOR          = (CCM_FIRST + 1),	// lParam is bkColor
		CCM_SETCOLORSCHEME      = (CCM_FIRST + 2),	// lParam is color scheme
		CCM_GETCOLORSCHEME      = (CCM_FIRST + 3),	// fills in COLORSCHEME pointed to by lParam
		CCM_GETDROPTARGET       = (CCM_FIRST + 4),
		CCM_SETUNICODEFORMAT    = (CCM_FIRST + 5),
		CCM_GETUNICODEFORMAT    = (CCM_FIRST + 6),
		CCM_SETVERSION          = (CCM_FIRST + 7)
	}
	#endregion

	#region PenStyle
	public enum PenStyle {
		PS_SOLID            = 0,
		PS_DASH             = 1,      
		PS_DOT              = 2,     
		PS_DASHDOT          = 3,      
		PS_DASHDOTDOT       = 4,       
		PS_NULL             = 5,
		PS_INSIDEFRAME      = 6,
		PS_USERSTYLE        = 7,
		PS_ALTERNATE        = 8,
		PS_STYLE_MASK       = 0x0000000F,
		PS_ENDCAP_ROUND     = 0x00000000,
		PS_ENDCAP_SQUARE    = 0x00000100,
		PS_ENDCAP_FLAT      = 0x00000200,
		PS_ENDCAP_MASK      = 0x00000F00,
		PS_JOIN_ROUND       = 0x00000000,
		PS_JOIN_BEVEL       = 0x00001000,
		PS_JOIN_MITER       = 0x00002000,
		PS_JOIN_MASK        = 0x0000F000,
		PS_COSMETIC         = 0x00000000,
		PS_GEOMETRIC        = 0x00010000,
		PS_TYPE_MASK        = 0x000F0000
	}
	#endregion

	#region ImageListDrawColor
	public enum ImageListDrawColor {
		None,		
		Default     
	}
	#endregion

	#region ImageList Draw Colors
	internal enum ImageListDrawColors : uint {
		CLR_NONE		= 0xFFFFFFFF,
		CLR_DEFAULT     = 0xFF000000
	}
	#endregion

	#region WM_SIZE_Param_Values
	public enum WM_SIZE_Param_Values {
		SIZE_RESTORED       = 0,
		SIZE_MINIMIZED      = 1,
		SIZE_MAXIMIZED      = 2,
		SIZE_MAXSHOW        = 3,
		SIZE_MAXHIDE        = 4
	}
	#endregion

	#region WMSizingMessage
	public enum WM_Sizing_Message_Param_Values {	
		/* wParam for WM_SIZING message */
		WMSZ_LEFT           = 1,
		WMSZ_RIGHT          = 2,
		WMSZ_TOP            = 3,
		WMSZ_TOPLEFT        = 4,
		WMSZ_TOPRIGHT       = 5,
		WMSZ_BOTTOM         = 6,
		WMSZ_BOTTOMLEFT     = 7,
		WMSZ_BOTTOMRIGHT    = 8
	}
	#endregion WMSizingMessage

	/* Win32 4.0 messages for IME */
	//	internal const uint IME_SETCONTEXT           = 0x0281;
	//	internal const uint IME_NOTIFY               = 0x0282;
	//	internal const uint IME_CONTROL              = 0x0283;
	//	internal const uint IME_COMPOSITIONFULL      = 0x0284;
	//	internal const uint IME_SELECT               = 0x0285;
	//	internal const uint IME_CHAR                 = 0x0286;
	//	/* Win32 5.0 messages for IME */
	//	internal const uint IME_REQUEST              = 0x0288;
	//
	//	/* Win32 4.0 messages for IME */
	//	internal const uint IME_KEYDOWN              = 0x0290;
	//	internal const uint IME_KEYUP                = 0x0291;

	#region CS_Types
	[Flags]
	internal enum CS_ {
		CS_VREDRAW          = 0x0001,
		CS_HREDRAW          = 0x0002,
		CS_KEYCVTWINDOW     = 0x0004,
		CS_DBLCLKS          = 0x0008,
		CS_OWNDC            = 0x0020,
		CS_CLASSDC          = 0x0040,
		CS_PARENTDC         = 0x0080,
		CS_NOKEYCVT         = 0x0100,
		CS_NOCLOSE          = 0x0200,
		CS_SAVEBITS         = 0x0800,
		CS_BYTEALIGNCLIENT  = 0x1000,
		CS_BYTEALIGNWINDOW  = 0x2000,
		CS_GLOBALCLASS      = 0x4000,
		CS_IME              = 0x00010000
	}
	#endregion 
	
	#region PeekMessage
	internal enum PeekMessage {
		// PeekMessage types
		PM_NOREMOVE     = 0x0000,
		PM_REMOVE       = 0x0001,
		PM_NOYIELD      = 0x0002
	}
	#endregion 

	#region WB_MessageBox_Types
	internal enum WB_MessageBox_Types {
		// MessageBox types
		MB_OK                   = 0x00000000,
		MB_OKCANCEL             = 0x00000001,
		MB_ABORTRETRYIGNORE     = 0x00000002,
		MB_YESNOCANCEL          = 0x00000003,
		MB_YESNO                = 0x00000004,
		MB_RETRYCANCEL          = 0x00000005,
		MB_TYPEMASK             = 0x0000000F,
		MB_APPLMODAL			= 0x00000000,
		MB_SYSTEMMODAL			= 0x00001000,
		MB_TASKMODAL			= 0x00002000,
		MB_MODEMASK				= 0x00003000
	}
	#endregion 

	#region Static Contreol Types
	internal enum SS_Static_Control_Types {
		// Static Control Styles
		SS_LEFT             = 0x00000000,
		SS_CENTER           = 0x00000001,
		SS_RIGHT            = 0x00000002,
		SS_ICON             = 0x00000003,
		SS_BLACKRECT        = 0x00000004,
		SS_GRAYRECT         = 0x00000005,
		SS_WHITERECT        = 0x00000006,
		SS_BLACKFRAME       = 0x00000007,
		SS_GRAYFRAME        = 0x00000008,
		SS_WHITEFRAME       = 0x00000009,

		SS_SIMPLE           = 0x0000000B,
		SS_LEFTNOWORDWRAP   = 0x0000000C,
		
		SS_OWNERDRAW        = 0x0000000D,
		SS_BITMAP           = 0x0000000E,
		SS_ENHMETAFILE      = 0x0000000F,

		SS_ETCHEDHORZ       = 0x00000010,
		SS_ETCHEDVERT       = 0x00000011,
		SS_ETCHEDFRAME      = 0x00000012,
		SS_TYPEMASK         = 0x0000001F,
		
		SS_NOPREFIX         = 0x00000080,
		SS_NOTIFY           = 0x00000100,
		SS_CENTERIMAGE      = 0x00000200,
		SS_RIGHTJUST        = 0x00000400,
		SS_REALSIZEIMAGE    = 0x00000800,
		SS_SUNKEN           = 0x00001000
	}
	#endregion 

	#region WF_
	internal enum MF_ {
		MF_INSERT           = 0x0,
		MF_CHANGE           = 0x80,
		MF_APPEND           = 0x100,
		MF_DELETE           = 0x200,
		MF_REMOVE           = 0x1000,
		MF_BYCOMMAND        = 0,
		MF_BYPOSITION       = 0x400,
		MF_SEPARATOR        = 0x800,
		MF_ENABLED          = 0, 
		MF_GRAYED           = 1,
		MF_DISABLED         = 2,
		MF_UNCHECKED        = 0,
		MF_CHECKED          = 8,
		MF_USECHECKBITMAPS  = 0x200,
		MF_STRING           = 0,
		MF_BITMAP           = 4,
		MF_OWNERDRAW        = 0x100,
		MF_POPUP            = 0x10,
		MF_MENUBARBREAK     = 0x20,
		MF_MENUBREAK        = 0x40,
		MF_UNHILITE         = 0,
		MF_HILITE           = 0x80,
		MF_DEFAULT          = 0x1000,
		MF_SYSMENU          = 0x2000,
		MF_HELP             = 0x4000,
		MF_RIGHTJUSTIFY     = 0x4000
	}
	#endregion 

	internal enum FM_ {
		FORMAT_MESSAGE_ALLOCATE_BUFFER  = 0x100,
		FORMAT_MESSAGE_IGNORE_INSERTS   = 0x200,
		FORMAT_MESSAGE_FROM_STRING      = 0x400,
		FORMAT_MESSAGE_FROM_HMODULE     = 0x800,
		FORMAT_MESSAGE_FROM_SYSTEM      = 0x1000,
		FORMAT_MESSAGE_ARGUMENT_ARRAY   = 0x2000,
		FORMAT_MESSAGE_MAX_WIDTH_MASK   = 0xFF
	}
	
	internal enum LC_ {
		IDC_ARROW           =32512,
		IDC_IBEAM           =32513,
		IDC_WAIT            =32514,
		IDC_CROSS           =32515,
		IDC_UPARROW         =32516,
		IDC_SIZE            =32640, 
		IDC_ICON            =32641, 
		IDC_SIZENWSE        =32642,
		IDC_SIZENESW        =32643,
		IDC_SIZEWE          =32644,
		IDC_SIZENS          =32645,
		IDC_SIZEALL         =32646,
		IDC_NO              =32648, 
		IDC_HAND            =32649,
		IDC_APPSTARTING     =32650, 
		IDC_HELP            =32651 
	}
	internal enum GSO_ {
		WHITE_BRUSH         =0,
		LTGRAY_BRUSH        =1,
		GRAY_BRUSH          =2,
		DKGRAY_BRUSH        =3,
		BLACK_BRUSH         =4,
		NULL_BRUSH          =5,
		HOLLOW_BRUSH        =5,
		WHITE_PEN           =6,
		BLACK_PEN           =7,
		NULL_PEN            =8,
		OEM_FIXED_FONT      =10,
		ANSI_FIXED_FONT     =11,
		ANSI_VAR_FONT       =12,
		SYSTEM_FONT         =13,
		DEVICE_DEFAULT_FONT =14,
		DEFAULT_PALETTE     =15,
		SYSTEM_FIXED_FONT   =16,
		DEFAULT_GUI_FONT    =17
	}

	internal enum GetSysColorIndex {
		COLOR_SCROLLBAR         =0,
		COLOR_BACKGROUND        =1,
		COLOR_ACTIVECAPTION     =2,
		COLOR_INACTIVECAPTION   =3,
		COLOR_MENU              =4,
		COLOR_WINDOW            =5,
		COLOR_WINDOWFRAME       =6,
		COLOR_MENUTEXT          =7,
		COLOR_WINDOWTEXT        =8,
		COLOR_CAPTIONTEXT       =9,
		COLOR_ACTIVEBORDER      =10,
		COLOR_INACTIVEBORDER    =11,
		COLOR_APPWORKSPACE      =12,
		COLOR_HIGHLIGHT         =13,
		COLOR_HIGHLIGHTTEXT     =14,
		COLOR_BTNFACE           =15,
		COLOR_BTNSHADOW         =16,
		COLOR_GRAYTEXT          =17,
		COLOR_BTNTEXT           =18,
		COLOR_INACTIVECAPTIONTEXT =19,
		COLOR_BTNHIGHLIGHT      =20,
		COLOR_3DDKSHADOW        =21,
		COLOR_3DLIGHT           =22,
		COLOR_INFOTEXT          =23,
		COLOR_INFOBK            =24,
		COLOR_DESKTOP           =1,
		COLOR_3DFACE            =16,
		COLOR_3DSHADOW          =16,
		COLOR_3DHIGHLIGHT       =20,
		COLOR_3DHILIGHT         =20,
		COLOR_BTNHILIGHT        =20
	}
	
	internal enum ButtonStyles : uint {
		BS_PUSHBUTTON       =0,
		BS_DEFPUSHBUTTON    =1,
		BS_CHECKBOX         =2,
		BS_AUTOCHECKBOX     =3,
		BS_RADIOBUTTON      =4,
		BS_3STATE           =5,
		BS_AUTO3STATE       =6,
		BS_GROUPBOX         =7,
		BS_USERBUTTON       =8,
		BS_AUTORADIOBUTTON  =9,
		BS_OWNERDRAW        =11,
		BS_LEFTTEXT         =0x20,
		BS_TEXT             =0,
		BS_ICON             =0x40,
		BS_BITMAP           =0x80,
		BS_LEFT             =0x100,
		BS_RIGHT            =0x200,
		BS_CENTER           =0x300,
		BS_TOP              =0x400,
		BS_BOTTOM           =0x800,
		BS_VCENTER          =0xC00,
		BS_PUSHLIKE         =0x1000,
		BS_MULTILINE        =0x2000,
		BS_NOTIFY           =0x4000,
		BS_FLAT             =0x8000,
		BS_RIGHTBUTTON      =0x20
	}

	internal enum ButtonNotification : uint {
		BN_CLICKED          =0,
		BN_PAINT            =1,
		BN_HILITE           =2,
		BN_UNHILITE         =3,
		BN_DISABLE          =4,
		BN_DOUBLECLICKED    =5,
		BN_PUSHED           =2,
		BN_UNPUSHED         =3,
		BN_DBLCLK           =5,
		BN_SETFOCUS         =6,
		BN_KILLFOCUS        =7
	}

	internal enum ButtonMessages : uint {
		BM_GETCHECK          =0x00f0,
		BM_SETCHECK          =0x00f1,
		BM_GETSTATE          =0x00f2,
		BM_SETSTATE          =0x00f3,
		BM_SETSTYLE          =0x00f4,
		BM_CLICK             =0x00f5,
		BM_GETIMAGE          =0x00f6,
		BM_SETIMAGE          =0x00f7
	}

	internal enum NativeButtonState : int {
		BST_UNCHECKED        = 0x0000,
		BST_CHECKED          = 0x0001,
		BST_INDETERMINATE    = 0x0002,
		BST_PUSHED           = 0x0004,
		BST_FOCUS            = 0x0008
	}

	internal enum ComboBoxNotification : int {
		CBN_ERRSPACE        =-1,
		CBN_SELCHANGE       =1,
		CBN_DBLCLK          =2,
		CBN_SETFOCUS        =3,
		CBN_KILLFOCUS       =4,
		CBN_EDITCHANGE      =5,
		CBN_EDITUPDATE      =6,
		CBN_DROPDOWN        =7,
		CBN_CLOSEUP         =8,
		CBN_SELENDOK        =9,
		CBN_SELENDCANCEL    =10
	}

	internal enum ListBoxMessages {
		LB_ADDSTRING            =0x0180,
		LB_INSERTSTRING         =0x0181,
		LB_DELETESTRING         =0x0182,
		LB_SELITEMRANGEEX       =0x0183,
		LB_RESETCONTENT         =0x0184,
		LB_SETSEL               =0x0185,
		LB_SETCURSEL            =0x0186,
		LB_GETSEL               =0x0187,
		LB_GETCURSEL            =0x0188,
		LB_GETTEXT              =0x0189,
		LB_GETTEXTLEN           =0x018A,
		LB_GETCOUNT             =0x018B,
		LB_SELECTSTRING         =0x018C,
		LB_DIR                  =0x018D,
		LB_GETTOPINDEX          =0x018E,
		LB_FINDSTRING           =0x018F,
		LB_GETSELCOUNT          =0x0190,
		LB_GETSELITEMS          =0x0191,
		LB_SETTABSTOPS          =0x0192,
		LB_GETHORIZONTALEXTENT  =0x0193,
		LB_SETHORIZONTALEXTENT  =0x0194,
		LB_SETCOLUMNWIDTH       =0x0195,
		LB_ADDFILE              =0x0196,
		LB_SETTOPINDEX          =0x0197,
		LB_GETITEMRECT          =0x0198,
		LB_GETITEMDATA          =0x0199,
		LB_SETITEMDATA          =0x019A,
		LB_SELITEMRANGE         =0x019B,
		LB_SETANCHORINDEX       =0x019C,
		LB_GETANCHORINDEX       =0x019D,
		LB_SETCARETINDEX        =0x019E,
		LB_GETCARETINDEX        =0x019F,
		LB_SETITEMHEIGHT        =0x01A0,
		LB_GETITEMHEIGHT        =0x01A1,
		LB_FINDSTRINGEXACT      =0x01A2,
		LB_SETLOCALE            =0x01A5,
		LB_GETLOCALE            =0x01A6,
		LB_SETCOUNT             =0x01A7,
		LB_INITSTORAGE          =0x01A8,
		LB_ITEMFROMPOINT        =0x01A9,
		LB_MSGMAX               =0x01B0,
		LB_MSGMAX_351           =0x01A8
	}

	internal enum ListBoxStyles : long {
		LBS_NOTIFY            = 0x0001,
		LBS_SORT              = 0x0002,
		LBS_NOREDRAW          = 0x0004,
		LBS_MULTIPLESEL       = 0x0008,
		LBS_OWNERDRAWFIXED    = 0x0010,
		LBS_OWNERDRAWVARIABLE = 0x0020,
		LBS_HASSTRINGS        = 0x0040,
		LBS_USETABSTOPS       = 0x0080,
		LBS_NOINTEGRALHEIGHT  = 0x0100,
		LBS_MULTICOLUMN       = 0x0200,
		LBS_WANTKEYBOARDINPUT = 0x0400,
		LBS_EXTENDEDSEL       = 0x0800,
		LBS_DISABLENOSCROLL   = 0x1000,
		LBS_NODATA            = 0x2000,
		LBS_NOSEL             = 0x4000,
		LBS_STANDARD          = 0x03 | WindowStyles.WS_VSCROLL | WindowStyles.WS_BORDER
	}

	internal enum ListBoxNotifications : int {
		LB_OKAY             =0,
		LB_ERR              =-1,
		LBN_ERRSPACE        =-2,
		LBN_SELCHANGE       =1,
		LBN_DBLCLK          =2,
		LBN_SELCANCEL       =3,
		LBN_SETFOCUS        =4,
		LBN_KILLFOCUS       =5
	}

	#region Trackbar Control Messages
	internal enum TrackbarMessages {
		TBM_GETPOS          =  ( Msg.WM_USER ),
		TBM_GETRANGEMIN     =  ( Msg.WM_USER + 1),
		TBM_GETRANGEMAX     =  ( Msg.WM_USER + 2),
		TBM_GETTIC          =  ( Msg.WM_USER + 3),
		TBM_SETTIC          =  ( Msg.WM_USER + 4),
		TBM_SETPOS          =  ( Msg.WM_USER + 5 ),
		TBM_SETRANGE        =  ( Msg.WM_USER + 6 ),
		TBM_SETRANGEMIN     =  ( Msg.WM_USER + 7 ),
		TBM_SETRANGEMAX     =  ( Msg.WM_USER + 8 ),
		TBM_CLEARTICS       =  ( Msg.WM_USER + 9 ),
		TBM_SETSEL          =  ( Msg.WM_USER + 10 ),
		TBM_SETSELSTART     =  ( Msg.WM_USER + 11 ),
		TBM_SETSELEND       =  ( Msg.WM_USER + 12 ),
		TBM_GETPTICS        =  ( Msg.WM_USER + 14 ),
		TBM_GETTICPOS       =  ( Msg.WM_USER + 15 ),
		TBM_GETNUMTICS      =  ( Msg.WM_USER + 16 ),
		TBM_GETSELSTART     =  ( Msg.WM_USER + 17 ),
		TBM_GETSELEND       =  ( Msg.WM_USER + 18 ),
		TBM_CLEARSEL        =  ( Msg.WM_USER + 19 ),
		TBM_SETTICFREQ      =  ( Msg.WM_USER + 20 ),
		TBM_SETPAGESIZE     =  ( Msg.WM_USER + 21 ),
		TBM_GETPAGESIZE     =  ( Msg.WM_USER + 22 ),
		TBM_SETLINESIZE     =  ( Msg.WM_USER + 23 ),
		TBM_GETLINESIZE     =  ( Msg.WM_USER + 24 ),
		TBM_GETTHUMBRECT    =  ( Msg.WM_USER + 25 ),
		TBM_GETCHANNELRECT  =  ( Msg.WM_USER + 26 ),
		TBM_SETTHUMBLENGTH  =  ( Msg.WM_USER + 27 ),
		TBM_GETTHUMBLENGTH  =  ( Msg.WM_USER + 28 ),
		TBM_SETTOOLTIPS     =  ( Msg.WM_USER + 29 ),
		TBM_GETTOOLTIPS     =  ( Msg.WM_USER + 30 ),
		TBM_SETTIPSIDE      =  ( Msg.WM_USER + 31 )
	}
	#endregion
		
	#region Trackbar Control Styles
	public enum TrackbarControlStyles {
		TBS_AUTOTICKS       =    0x0001,
		TBS_VERT            =    0x0002,
		TBS_HORZ            =    0x0000,
		TBS_TOP             =    0x0004,
		TBS_BOTTOM          =    0x0000,
		TBS_LEFT            =    0x0004,
		TBS_RIGHT           =    0x0000,
		TBS_BOTH            =    0x0008,
		TBS_NOTICKS         =    0x0010,
		TBS_ENABLESELRANGE  =    0x0020,
		TBS_FIXEDLENGTH     =    0x0040,
		TBS_NOTHUMB         =    0x0080,
		TBS_TOOLTIPS        =    0x0100
	}
	#endregion

	internal enum TrackbarNotifications : int {
		TB_LINEUP           =    0,
		TB_LINEDOWN         =    1,
		TB_PAGEUP           =    2,
		TB_PAGEDOWN         =    3,
		TB_THUMBPOSITION    =    4,
		TB_THUMBTRACK       =    5,
		TB_TOP              =    6,
		TB_BOTTOM           =    7,
		TB_ENDTRACK         =    8
	}

	#region StatusBar Control Styles
	public enum StatusbarControlStyles {
		SBARS_SIZEGRIP      =    0x0100,
		SBT_TOOLTIPS	    =	 0x0800
	}
	#endregion

	#region StatusBar notifications
	internal enum StatusbarNotifications : int {
		SBN_FIRST	= (0 - 880),
		SBN_LAST	= (0 - 899),
		SBN_SIMPLEMODECHANGE = (SBN_FIRST - 0)
	}
	#endregion

	#region Statusbar Control Messages
	internal enum StatusbarMessages {
		SB_SETTEXT	= ( Msg.WM_USER+1 ),
		SB_GETTEXT	= ( Msg.WM_USER+2 ),
		SB_GETTEXTLENGTH= ( Msg.WM_USER+3 ),
		SB_SETPARTS	= ( Msg.WM_USER+4 ),
		SB_GETPARTS	= ( Msg.WM_USER+6 ),
		SB_GETBORDERS	= ( Msg.WM_USER+7 ),
		SB_SETMINHEIGHT	= ( Msg.WM_USER+8 ),
		SB_SIMPLE	= ( Msg.WM_USER+9 ),
		SB_GETRECT	= ( Msg.WM_USER+10),
		SB_ISSIMPLE	= ( Msg.WM_USER+14),
		SB_SETICON	= ( Msg.WM_USER+15),
		SB_SETTIPTEXT	= ( Msg.WM_USER+16),
		SB_GETTIPTEXT	= ( Msg.WM_USER+18),
		SB_GETICON	= ( Msg.WM_USER+20),
		SB_SETBKCOLOR	= CommonControlMessages.CCM_SETBKCOLOR
	}
	#endregion

	#region Statusbar Drawing Operations Types
	internal enum StatusbarDrawType {
		SBT_OWNERDRAW   = 0x1000,
		SBT_NOBORDERS   = 0x0100,
		SBT_POPOUT      = 0x0200,
		SBT_RTLREADING  = 0x0400
	}
	#endregion

	#region DateTimePicker Control Styles
	public enum DateTimePickerControlStyles 
	{
		DTS_UPDOWN    =       0x0001,
		DTS_SHOWNONE  =       0x0002,
		DTS_SHORTDATEFORMAT = 0x0000,
		DTS_LONGDATEFORMAT  = 0x0004,
		DTS_TIMEFORMAT      = 0x0009,
		DTS_APPCANPARSE     = 0x0010,
		DTS_RIGHTALIGN      = 0x0020
	}
	#endregion

	#region DateTimePicker Control Messages
	internal enum DateTimePickerMessages 
	{
		DTM_FIRST		=  0x1000,
		DTM_GETSYSTEMTIME	= (DTM_FIRST + 1),
		DTM_SETSYSTEMTIME	= (DTM_FIRST + 2),
		DTM_GETRANGE		= (DTM_FIRST + 3),
		DTM_SETRANGE		= (DTM_FIRST + 4),
		DTM_SETFORMATA		= (DTM_FIRST + 5),
		DTM_SETMCCOLOR		= (DTM_FIRST + 6),
		DTM_GETMCCOLOR		= (DTM_FIRST + 7),
		DTM_GETMONTHCAL		= (DTM_FIRST + 8),
		DTM_SETMCFONT		= (DTM_FIRST + 9),
		DTM_GETMCFONT		= (DTM_FIRST + 10),
		DTM_SETFORMATW		= (DTM_FIRST + 50)
	}
	#endregion

	#region DateTimePicker Control Flags
	internal enum DateTimePickerFlags
	{
		GDT_ERROR    = -1,
		GDT_VALID    = 0,
		GDT_NONE     = 1,
		GDTR_MIN     = 0x0001,
		GDTR_MAX     = 0x0002
	}
	#endregion

	#region DateTimePicker Notifications
	internal enum DateTimePickerNotifications
	{
		DTN_FIRST		= (0 - 760),
		DTN_DATETIMECHANGE	= (DTN_FIRST + 1),
		DTN_USERSTRINGA		= (DTN_FIRST + 2),
		DTN_WMKEYDOWNA		= (DTN_FIRST + 3),
		DTN_FORMATA		= (DTN_FIRST + 4),
		DTN_FORMATQUERYA	= (DTN_FIRST + 5),
		DTN_DROPDOWN		= (DTN_FIRST + 6),
		DTN_CLOSEUP		= (DTN_FIRST + 7),
		DTN_USERSTRINGW		= (DTN_FIRST + 15),
		DTN_WMKEYDOWNW		= (DTN_FIRST + 16),
		DTN_FORMATW		= (DTN_FIRST + 17),
		DTN_FORMATQUERYW	= (DTN_FIRST + 18)
	}
	#endregion

	#region ToolTipControl Messages
	internal enum ToolTipControlMessages
	{
		TTM_ACTIVATE            = (Msg.WM_USER + 1),
		TTM_SETDELAYTIME	= (Msg.WM_USER + 3),
		TTM_SETMAXTIPWIDTH      = (Msg.WM_USER + 24),
		TTM_ADDTOOL		= (Msg.WM_USER + 4),
		TTM_DELTOOL		= (Msg.WM_USER + 5),
		TTM_UPDATETIPTEXT	= (Msg.WM_USER + 12)

	}
	#endregion

	#region ToolTipControl Styles
	internal enum ToolTipControlStyles
	{
		TTS_ALWAYSTIP           = 0x01,
		TTS_NOPREFIX            = 0x02
	}
	#endregion

	#region ToolTipControlDelay Flags
	internal enum ToolTipControlDelayFlags {
		TTDT_AUTOMATIC          = 0,
		TTDT_RESHOW             = 1,
		TTDT_AUTOPOP            = 2,
		TTDT_INITIAL            = 3
	}
	#endregion
	
	#region MonthCal Colors
	internal enum MonthCalColors
	{
		MCSC_BACKGROUND   = 0,
		MCSC_TEXT         = 1,
		MCSC_TITLEBK      = 2,
		MCSC_TITLETEXT    = 3,
		MCSC_MONTHBK      = 4,
		MCSC_TRAILINGTEXT = 5
	}
	#endregion

	internal enum OwnerDrawTypes : int 
	{
		ODT_COMBOBOX = 3,
		ODT_LISTBOX = 2,
		ODT_LISTVIEW = 102,
		ODT_MENU = 1,
		ODT_BUTTON = 4,
		ODT_STATIC = 5,
		ODT_TAB = 101,
		ODT_HEADER = 100
	}

	internal enum PBS_ : int {
		PBS_MARQUEE,
		PBS_SMOOTH,
		PBS_VERTICAL
	}

	internal enum ProgressBarMessages : int {
		WM_USER            = 0x0400,
		PBM_SETRANGE       = WM_USER+1,
		PBM_SETPOS         = WM_USER+2,
		PBM_DELTAPOS       = WM_USER+3,
		PBM_SETSTEP        = WM_USER+4,
		PBM_STEPIT         = WM_USER+5,
		PBM_SETRANGE32     = WM_USER+6,
		PBM_GETRANGE       = WM_USER+7,
		PBM_GETPOS         = WM_USER+8,
		PBM_SETBARCOLOR    = WM_USER+9,
		PBM_SETBKCOLOR     = CommonControlMessages.CCM_SETBKCOLOR
	}

	internal enum TabControlMessages : int {
		TCM_FIRST          =  0x1300,
		TCM_SETIMAGELIST   =  (TCM_FIRST + 3),
		TCM_GETITEM        =  (TCM_FIRST + 60),
		TCM_SETITEM        =  (TCM_FIRST + 6),
		TCM_INSERTITEM     =  (TCM_FIRST + 7),
		TCM_DELETEITEM     =  (TCM_FIRST + 8),
		TCM_DELETEALLITEMS =  (TCM_FIRST + 9),
		TCM_GETITEMRECT    =  (TCM_FIRST + 10),
		TCM_GETCURSEL      =  (TCM_FIRST + 11),
		TCM_SETCURSEL      =  (TCM_FIRST + 12),
		TCM_HITTEST        =  (TCM_FIRST + 13),
		TCM_ADJUSTRECT     =  (TCM_FIRST + 40),
		TCM_SETITEMSIZE    =  (TCM_FIRST + 41),
		TCM_REMOVEIMAGE    =  (TCM_FIRST + 42),
		TCM_SETPADDING     =  (TCM_FIRST + 43),
		TCM_GETROWCOUNT    =  (TCM_FIRST + 44),
		TCM_GETTOOLTIPS    =  (TCM_FIRST + 45),
		TCM_SETTOOLTIPS    =  (TCM_FIRST + 46),
		TCM_GETCURFOCUS    =  (TCM_FIRST + 47),
		TCM_SETCURFOCUS    =  (TCM_FIRST + 48),
		TCM_SETMINTABWIDTH =  (TCM_FIRST + 49),
		TCM_DESELECTALL    =  (TCM_FIRST + 50),
		TCM_HIGHLIGHTITEM  =  (TCM_FIRST + 51)
	}

	internal enum TabControlStyles : int {
		TCS_SCROLLOPPOSITE     = 0x0001,
		TCS_BOTTOM             = 0x0002,
		TCS_RIGHT              = 0x0002,
		TCS_MULTISELECT        = 0x0004,
		TCS_FLATBUTTONS        = 0x0008,
		TCS_FORCEICONLEFT      = 0x0010,
		TCS_FORCELABELLEFT     = 0x0020,
		TCS_HOTTRACK           = 0x0040,
		TCS_VERTICAL           = 0x0080,
		TCS_TABS               = 0x0000,
		TCS_BUTTONS            = 0x0100,
		TCS_SINGLELINE         = 0x0000,
		TCS_MULTILINE          = 0x0200,
		TCS_RIGHTJUSTIFY       = 0x0000,
		TCS_FIXEDWIDTH         = 0x0400,
		TCS_RAGGEDRIGHT        = 0x0800,
		TCS_FOCUSONBUTTONDOWN  = 0x1000,
		TCS_OWNERDRAWFIXED     = 0x2000,
		TCS_TOOLTIPS           = 0x4000,
		TCS_FOCUSNEVER         = 0x8000
	}

	internal enum TabControlNotifications : int {
		TCN_FIRST              = 0 - 550,
		TCN_SELCHANGE          = (TCN_FIRST - 1),
		TCN_SELCHANGING        = (TCN_FIRST - 2)
	}

	internal enum CreateWindowCoordinates : int {
		CW_USEDEFAULT = -2147483648
	}

	internal enum DrawFrameControl : uint {
		DFC_CAPTION             =1,      
		DFC_MENU                =2,      
		DFC_SCROLL              =3,      
		DFC_BUTTON              =4,
		DFCS_BUTTONCHECK        =0x0000, 
		DFCS_BUTTONRADIOIMAGE   =0x0001, 
		DFCS_BUTTONRADIOMASK    =0x0002,	
		DFCS_BUTTONRADIO        =0x0004, 
		DFCS_BUTTON3STATE       =0x0008, 
		DFCS_BUTTONPUSH         =0x0010, 
	}
/*
		DFCS_CAPTIONCLOSE       =0x0000, 
		DFCS_CAPTIONMIN         =0x0001, 
		DFCS_CAPTIONMAX         =0x0002, 
		DFCS_CAPTIONRESTORE     =0x0003, 
		DFCS_CAPTIONHELP        =0x0004,	
		DFCS_MENUARROW          =0x0000, 
		DFCS_MENUCHECK          =0x0001, 
		DFCS_MENUBULLET         =0x0002, 
		DFCS_MENUARROWRIGHT     =0x0004, 
		DFCS_SCROLLUP            =0x0000,
		DFCS_SCROLLDOWN          =0x0001,
		DFCS_SCROLLLEFT          =0x0002,
		DFCS_SCROLLRIGHT         =0x0003,
		DFCS_SCROLLCOMBOBOX      =0x0005,
		DFCS_SCROLLSIZEGRIP      =0x0008,
		DFCS_SCROLLSIZEGRIPRIGHT =0x0010,
		DFCS_INACTIVE           =0x0100, 
		DFCS_PUSHED             =0x0200, 
		DFCS_CHECKED            =0x0400, 
		DFCS_TRANSPARENT        =0x0800, 
		DFCS_HOT                =0x1000, 
		DFCS_ADJUSTRECT         =0x2000,	
		DFCS_FLAT               =0x4000, 
		DFCS_MONO               =0x8000 
	}
*/	
	internal enum KeyStatusFlags : int {
		MK_LBUTTON	    = 0x0001,
		MK_RBUTTON	    = 0x0002,
		MK_SHIFT	    = 0x0004,
		MK_CONTROL	    = 0x0008,
		MK_MBUTTON	    = 0x0010,
		MK_XBUTTON1     = 0x0020,
		MK_XBUTTON2     = 0x0040
	}

	internal enum ExtTextOutFlags : int {
		ETO_GRAYED          =0x0001,
		ETO_OPAQUE          =0x0002,
		ETO_CLIPPED         =0x0004,
		ETO_GLYPH_INDEX     =0x0010,
		ETO_RTLREADING      =0x0080,
		ETO_IGNORELANGUAGE  =0x1000
	}
	
	internal enum UpDownControlStyles : int {
		UDS_WRAP           =0x0001,
		UDS_SETBUDDYINT    =0x0002,
		UDS_ALIGNRIGHT     =0x0004,
		UDS_ALIGNLEFT      =0x0008,
		UDS_AUTOBUDDY      =0x0010,
		UDS_ARROWKEYS      =0x0020,
		UDS_HORZ           =0x0040,
		UDS_NOTHOUSANDS    =0x0080,
		UDS_HOTTRACK       =0x0100
	}
	
	internal enum UpDownControlMessages : int {
		WM_USER            =  0x0400,
		UDN_FIRST          =(0-721),
		UDN_LAST           =(0-740),
		UDN_DELTAPOS       =(UDN_FIRST-1),
		UDM_SETRANGE       =(WM_USER+101),
		UDM_GETRANGE       =(WM_USER+102),
		UDM_SETPOS         =(WM_USER+103),
		UDM_GETPOS         =(WM_USER+104),
		UDM_SETBUDDY       =(WM_USER+105),
		UDM_GETBUDDY       =(WM_USER+106),
		UDM_SETACCEL       =(WM_USER+107),
		UDM_GETACCEL       =(WM_USER+108),
		UDM_SETBASE        =(WM_USER+109),
		UDM_GETBASE        =(WM_USER+110),
		UDM_SETRANGE32     =(WM_USER+111),
		UDM_GETRANGE32     =(WM_USER+112),
		//UDM_SETUNICODEFORMAT    =CCM_SETUNICODEFORMAT,
		//UDM_GETUNICODEFORMAT    =CCM_GETUNICODEFORMAT,
		UDM_SETPOS32       =(WM_USER+113),
		UDM_GETPOS32       =(WM_USER+114)
	}

	internal enum GetWindowConstants : int {
		GW_HWNDFIRST       = 0,
		GW_HWNDLAST        = 1,
		GW_HWNDNEXT        = 2,
		GW_HWNDPREV        = 3,
		GW_OWNER           = 4,
		GW_CHILD           = 5
	}

	internal enum TabControlItemFlags : int {
		TCIF_TEXT          = 0x0001,
		TCIF_IMAGE         = 0x0002,
		TCIF_RTLREADING    = 0x0004,
		TCIF_PARAM         = 0x0008,
		TCIF_STATE         = 0x0010
	}

	internal enum EditControlStyles : int {
		ES_LEFT             = 0x0000,
		ES_CENTER           = 0x0001,
		ES_RIGHT            = 0x0002,
		ES_MULTILINE        = 0x0004,
		ES_UPPERCASE        = 0x0008,
		ES_LOWERCASE        = 0x0010,
		ES_PASSWORD         = 0x0020,
		ES_AUTOVSCROLL      = 0x0040,
		ES_AUTOHSCROLL      = 0x0080,
		ES_NOHIDESEL        = 0x0100,
		ES_OEMCONVERT       = 0x0400,
		ES_READONLY         = 0x0800,
		ES_WANTRETURN       = 0x1000
	}

	internal enum EditControlMessages : int {
		EM_GETSEL               = 0x00B0,
		EM_SETSEL               = 0x00B1,
		EM_GETRECT              = 0x00B2,
		EM_SETRECT              = 0x00B3,
		EM_SETRECTNP            = 0x00B4,
		EM_SCROLL               = 0x00B5,
		EM_LINESCROLL           = 0x00B6,
		EM_SCROLLCARET          = 0x00B7,
		EM_GETMODIFY            = 0x00B8,
		EM_SETMODIFY            = 0x00B9,
		EM_GETLINECOUNT         = 0x00BA,
		EM_LINEINDEX            = 0x00BB,
		EM_SETHANDLE            = 0x00BC,
		EM_GETHANDLE            = 0x00BD,
		EM_GETTHUMB             = 0x00BE,
		EM_LINELENGTH           = 0x00C1,
		EM_REPLACESEL           = 0x00C2,
		EM_GETLINE              = 0x00C4,
		EM_LIMITTEXT            = 0x00C5,
		EM_CANUNDO              = 0x00C6,
		EM_UNDO                 = 0x00C7,
		EM_FMTLINES             = 0x00C8,
		EM_LINEFROMCHAR         = 0x00C9,
		EM_SETTABSTOPS          = 0x00CB,
		EM_SETPASSWORDCHAR      = 0x00CC,
		EM_EMPTYUNDOBUFFER      = 0x00CD,
		EM_GETFIRSTVISIBLELINE  = 0x00CE,
		EM_SETREADONLY          = 0x00CF,
		EM_SETWORDBREAKPROC     = 0x00D0,
		EM_GETWORDBREAKPROC     = 0x00D1,
		EM_GETPASSWORDCHAR      = 0x00D2,
		EM_SETMARGINS           = 0x00D3,
		EM_GETMARGINS           = 0x00D4,
		EM_SETLIMITTEXT         = EM_LIMITTEXT,
		EM_GETLIMITTEXT         = 0x00D5,
		EM_POSFROMCHAR          = 0x00D6,
		EM_CHARFROMPOS          = 0x00D7
	}

	internal enum EditControlNotifications : int {
		EN_CHANGE           = 0x0300,
		EN_UPDATE           = 0x0400,
		EN_ERRSPACE         = 0x0500,
		EN_MAXTEXT          = 0x0501,
		EN_HSCROLL          = 0x0601,
		EN_VSCROLL          = 0x0602
	}	
	
	internal enum OpenFileDlgFlags : int {
		OFN_READONLY                 = 0x00000001,
		OFN_OVERWRITEPROMPT          = 0x00000002,
		OFN_HIDEREADONLY             = 0x00000004,
		OFN_NOCHANGEDIR              = 0x00000008,
		OFN_SHOWHELP                 = 0x00000010,
		OFN_ENABLEHOOK               = 0x00000020,
		OFN_ENABLETEMPLATE           = 0x00000040,
		OFN_ENABLETEMPLATEHANDLE     = 0x00000080,
		OFN_NOVALIDATE               = 0x00000100,
		OFN_ALLOWMULTISELECT         = 0x00000200,
		OFN_EXTENSIONDIFFERENT       = 0x00000400,
		OFN_PATHMUSTEXIST            = 0x00000800,
		OFN_FILEMUSTEXIST            = 0x00001000,
		OFN_CREATEPROMPT             = 0x00002000,
		OFN_SHAREAWARE               = 0x00004000,
		OFN_NOREADONLYRETURN         = 0x00008000,
		OFN_NOTESTFILECREATE         = 0x00010000,
		OFN_NONETWORKBUTTON          = 0x00020000,
		OFN_NOLONGNAMES              = 0x00040000,
		OFN_EXPLORER                 = 0x00080000,
		OFN_NODEREFERENCELINKS       = 0x00100000,
		OFN_LONGNAMES                = 0x00200000,
		OFN_ENABLEINCLUDENOTIFY      = 0x00400000,
		OFN_ENABLESIZING             = 0x00800000
	}

	internal enum CommDlgErrors : uint {
		FNERR_FILENAMECODES          = 0x3000,
		FNERR_SUBCLASSFAILURE        = 0x3001,
		FNERR_INVALIDFILENAME        = 0x3002,
		FNERR_BUFFERTOOSMALL         = 0x3003
	}

	internal enum CommDlgNotifications : int {
		CDN_FIRST                    = (0-601),
		CDN_INITDONE                 = (CDN_FIRST - 0x0000),
		CDN_SELCHANGE                = (CDN_FIRST - 0x0001),
		CDN_FOLDERCHANGE             = (CDN_FIRST - 0x0002),
		CDN_SHAREVIOLATION           = (CDN_FIRST - 0x0003),
		CDN_HELP                     = (CDN_FIRST - 0x0004),
		CDN_FILEOK                   = (CDN_FIRST - 0x0005),
		CDN_TYPECHANGE               = (CDN_FIRST - 0x0006),
		CDN_INCLUDEITEM              = (CDN_FIRST - 0x0007)
	}

	[Flags]
	internal enum TrackPopupMenuFlags : int {
		TPM_LEFTBUTTON      = 0x0000,
		TPM_RIGHTBUTTON     = 0x0002,
		TPM_LEFTALIGN       = 0x0000,
		TPM_CENTERALIGN     = 0x0004,
		TPM_RIGHTALIGN      = 0x0008,
		TPM_TOPALIGN        = 0x0000,
		TPM_VCENTERALIGN    = 0x0010,
		TPM_BOTTOMALIGN     = 0x0020,
		TPM_HORIZONTAL      = 0x0000,
		TPM_VERTICAL        = 0x0040,
		TPM_NONOTIFY        = 0x0080,
		TPM_RETURNCMD       = 0x0100,
		TPM_RECURSE         = 0x0001
	}

	internal enum ChooseColorFlags : int {
		CC_RGBINIT               = 0x00000001,
		CC_FULLOPEN              = 0x00000002,
		CC_PREVENTFULLOPEN       = 0x00000004,
		CC_SHOWHELP              = 0x00000008,
		CC_ENABLEHOOK            = 0x00000010,
		CC_ENABLETEMPLATE        = 0x00000020,
		CC_ENABLETEMPLATEHANDLE  = 0x00000040,
		CC_SOLIDCOLOR            = 0x00000080,
		CC_ANYCOLOR              = 0x00000100
	}
}
