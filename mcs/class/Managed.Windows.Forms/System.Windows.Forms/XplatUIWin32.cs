// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
// Copyright (c) 2004-2006 Novell, Inc.
//
// Authors:
//	Peter Bartok	pbartok@novell.com
//
//

// NOT COMPLETE

using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.ComponentModel;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;


/// Win32 Version
namespace System.Windows.Forms {
	internal class XplatUIWin32 : XplatUIDriver {
		#region Local Variables
		private static XplatUIWin32	instance;
		private static int		ref_count;
		private static IntPtr		FosterParent;

		internal static MouseButtons	mouse_state;
		internal static Point		mouse_position;
		internal static bool		grab_confined;
		internal static IntPtr		grab_hwnd;
		internal static Rectangle	grab_area;
		internal static WndProc		wnd_proc;
		internal static IntPtr		prev_mouse_hwnd;
		internal static bool		caret_visible;

		internal static bool		themes_enabled;
		private Hashtable		timer_list;
		private static Queue		message_queue;
		private static IntPtr 		clip_magic = new IntPtr(27051977);
		private static int		scroll_width;
		private static int		scroll_height;
		private static Hashtable	wm_nc_registered;
		private static RECT		clipped_cursor_rect;
		private Hashtable		registered_classes;
		private Hwnd HwndCreating; // the Hwnd we are currently creating (see CreateWindow)

		#endregion	// Local Variables

		#region Private Structs
		[StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
		private struct WNDCLASS {
			internal int		style;
			internal WndProc	lpfnWndProc;
			internal int		cbClsExtra;
			internal int		cbWndExtra;
			internal IntPtr		hInstance;
			internal IntPtr		hIcon;
			internal IntPtr		hCursor;
			internal IntPtr		hbrBackground;
			[MarshalAs(UnmanagedType.LPWStr)]
			internal string		lpszMenuName;
			[MarshalAs(UnmanagedType.LPWStr)]
			internal string		lpszClassName;
		}

		[StructLayout(LayoutKind.Sequential)]
		internal struct RECT {
			internal int		left;
			internal int		top;
			internal int		right;
			internal int		bottom;

			public RECT (int left, int top, int right, int bottom)
			{
				this.left = left;
				this.top = top;
				this.right = right;
				this.bottom = bottom;
			}

			#region Instance Properties
			public int Height { get { return bottom - top; } }
			public int Width { get { return right - left; } }
			public Size Size { get { return new Size (Width, Height); } }
			public Point Location { get { return new Point (left, top); } }
			#endregion

			#region Instance Methods
			public Rectangle ToRectangle ()
			{
				return Rectangle.FromLTRB (left, top, right, bottom);
			}

			public static RECT FromRectangle (Rectangle rectangle)
			{
				return new RECT (rectangle.Left, rectangle.Top, rectangle.Right, rectangle.Bottom);
			}

			public override int GetHashCode ()
			{
				return left ^ ((top << 13) | (top >> 0x13))
				  ^ ((Width << 0x1a) | (Width >> 6))
				  ^ ((Height << 7) | (Height >> 0x19));
			}
			
			public override string ToString ()
			{
				return String.Format("RECT left={0}, top={1}, right={2}, bottom={3}, width={4}, height={5}", left, top, right, bottom, right-left, bottom-top);
			}
			#endregion

			#region Operator overloads
			public static implicit operator Rectangle (RECT rect)
			{
				return Rectangle.FromLTRB (rect.left, rect.top, rect.right, rect.bottom);
			}

			public static implicit operator RECT (Rectangle rect)
			{
				return new RECT (rect.Left, rect.Top, rect.Right, rect.Bottom);
			}
			#endregion
		}

		internal enum SPIAction {
			SPI_GETACTIVEWINDOWTRACKING = 0x1000,
			SPI_GETACTIVEWNDTRKTIMEOUT = 0x2002,
			SPI_GETANIMATION = 0x0048,
			SPI_GETCARETWIDTH = 0x2006,
			SPI_GETCOMBOBOXANIMATION = 0x1004,
			SPI_GETDRAGFULLWINDOWS	= 0x0026,
			SPI_GETDROPSHADOW = 0x1024,
			SPI_GETFONTSMOOTHING = 0x004A,
			SPI_GETFONTSMOOTHINGCONTRAST = 0x200C,
			SPI_GETFONTSMOOTHINGTYPE = 0x200A,
			SPI_GETGRADIENTCAPTIONS = 0x1008,
			SPI_GETHOTTRACKING = 0x100E,
			SPI_GETICONTITLEWRAP = 0x0019,
			SPI_GETKEYBOARDSPEED = 0x000A,
			SPI_GETKEYBOARDDELAY	= 0x0016,
			SPI_GETKEYBOARDCUES		= 0x100A,
			SPI_GETKEYBOARDPREF = 0x0044,
			SPI_GETLISTBOXSMOOTHSCROLLING = 0x1006,
			SPI_GETMENUANIMATION = 0x1002,
			SPI_GETMENUDROPALIGNMENT = 0x001B,
			SPI_GETMENUFADE = 0x1012,
			SPI_GETMENUSHOWDELAY = 0x006A,
			SPI_GETMOUSESPEED = 0x0070,
			SPI_GETSELECTIONFADE = 0x1014,
			SPI_GETSNAPTODEFBUTTON = 0x005F,
			SPI_GETTOOLTIPANIMATION = 0x1016,
			SPI_GETWORKAREA = 0x0030,
			SPI_GETMOUSEHOVERWIDTH	= 0x0062,
			SPI_GETMOUSEHOVERHEIGHT	= 0x0064,
			SPI_GETMOUSEHOVERTIME	= 0x0066,
			SPI_GETUIEFFECTS = 0x103E,
			SPI_GETWHEELSCROLLLINES = 0x0068
		}

		internal enum WindowPlacementFlags {
			SW_HIDE			= 0,
			SW_SHOWNORMAL       	= 1,
			SW_NORMAL           	= 1,
			SW_SHOWMINIMIZED    	= 2,
			SW_SHOWMAXIMIZED    	= 3,
			SW_MAXIMIZE         	= 3,
			SW_SHOWNOACTIVATE   	= 4,
			SW_SHOW             	= 5,
			SW_MINIMIZE         	= 6,
			SW_SHOWMINNOACTIVE  	= 7,
			SW_SHOWNA           	= 8,
			SW_RESTORE          	= 9,
			SW_SHOWDEFAULT      	= 10,
			SW_FORCEMINIMIZE    	= 11,
			SW_MAX              	= 11
		}

		[StructLayout(LayoutKind.Sequential)]
		private struct WINDOWPLACEMENT {
			internal uint			length;
			internal uint			flags;
			internal WindowPlacementFlags	showCmd;
			internal POINT			ptMinPosition;
			internal POINT			ptMaxPosition;
			internal RECT			rcNormalPosition;
		}

		[StructLayout(LayoutKind.Sequential)]
		internal struct NCCALCSIZE_PARAMS {
			internal RECT		rgrc1;
			internal RECT		rgrc2;
			internal RECT		rgrc3;
			internal IntPtr		lppos;
		}

		[Flags]
		private enum TMEFlags {
			TME_HOVER		= 0x00000001,
			TME_LEAVE		= 0x00000002,
			TME_NONCLIENT		= 0x00000010,
			TME_QUERY		= unchecked((int)0x40000000),
			TME_CANCEL		= unchecked((int)0x80000000)
		}

		[StructLayout(LayoutKind.Sequential)]
		private struct TRACKMOUSEEVENT {
			internal int		size;
			internal TMEFlags	dwFlags;
			internal IntPtr		hWnd;
			internal int		dwHoverTime;
		}

		[StructLayout(LayoutKind.Sequential)]
		private struct PAINTSTRUCT {
			internal IntPtr		hdc;
			internal int		fErase;
			internal RECT		rcPaint;
			internal int		fRestore;
			internal int		fIncUpdate;
			internal int		Reserved1;
			internal int		Reserved2;
			internal int		Reserved3;
			internal int		Reserved4;
			internal int		Reserved5;
			internal int		Reserved6;
			internal int		Reserved7;
			internal int		Reserved8;
		}

		[StructLayout(LayoutKind.Sequential)]
		internal struct KEYBDINPUT {
			internal short wVk;
			internal short wScan;
			internal Int32 dwFlags;
			internal Int32 time;
			internal UIntPtr dwExtraInfo;
		}

		[StructLayout(LayoutKind.Sequential)]
		internal struct MOUSEINPUT {
			internal Int32 dx;
			internal Int32 dy;
			internal Int32 mouseData;
			internal Int32 dwFlags;
			internal Int32 time;
			internal UIntPtr dwExtraInfo;
		}

		[StructLayout(LayoutKind.Sequential)]
		internal struct HARDWAREINPUT {
			internal Int32 uMsg;
			internal short wParamL;
			internal short wParamH;
		}

		[StructLayout (LayoutKind.Sequential)]
		internal struct ICONINFO {
			internal bool fIcon;
			internal Int32 xHotspot;
			internal Int32 yHotspot;
			internal IntPtr hbmMask;
			internal IntPtr hbmColor;
		}    
		
		[StructLayout(LayoutKind.Explicit)]
		internal struct INPUT {
			[FieldOffset(0)]
			internal Int32 type;

			[FieldOffset(4)]
			internal MOUSEINPUT mi;

			[FieldOffset(4)]
			internal KEYBDINPUT ki;

			[FieldOffset(4)]
			internal HARDWAREINPUT hi;
		}

		[StructLayout (LayoutKind.Sequential)]
		public struct ANIMATIONINFO {
			internal uint cbSize;
			internal int iMinAnimate;
		}
		
		internal enum InputFlags {
			KEYEVENTF_EXTENDEDKEY	= 0x0001,
			KEYEVENTF_KEYUP			= 0x0002,
			KEYEVENTF_SCANCODE		= 0x0003,
			KEYEVENTF_UNICODE		= 0x0004,
		}

		internal enum ClassStyle {
			CS_VREDRAW			= 0x00000001,
			CS_HREDRAW			= 0x00000002,
			CS_KEYCVTWINDOW			= 0x00000004,
			CS_DBLCLKS			= 0x00000008,
			CS_OWNDC			= 0x00000020,
			CS_CLASSDC			= 0x00000040,
			CS_PARENTDC			= 0x00000080,
			CS_NOKEYCVT			= 0x00000100,
			CS_NOCLOSE			= 0x00000200,
			CS_SAVEBITS			= 0x00000800,
			CS_BYTEALIGNCLIENT		= 0x00001000,
			CS_BYTEALIGNWINDOW		= 0x00002000,
			CS_GLOBALCLASS			= 0x00004000,
			CS_IME				= 0x00010000,
			// Windows XP+
			CS_DROPSHADOW			= 0x00020000
		}

		internal enum SetWindowPosZOrder {
			HWND_TOP			= 0,
			HWND_BOTTOM			= 1,
			HWND_TOPMOST			= -1,
			HWND_NOTOPMOST			= -2
		}

		[Flags]
		internal enum SetWindowPosFlags {
			SWP_ASYNCWINDOWPOS		= 0x4000, 
			SWP_DEFERERASE			= 0x2000,
			SWP_DRAWFRAME			= 0x0020,
			SWP_FRAMECHANGED		= 0x0020,
			SWP_HIDEWINDOW			= 0x0080,
			SWP_NOACTIVATE			= 0x0010,
			SWP_NOCOPYBITS			= 0x0100,
			SWP_NOMOVE			= 0x0002,
			SWP_NOOWNERZORDER		= 0x0200,
			SWP_NOREDRAW			= 0x0008,
			SWP_NOREPOSITION		= 0x0200,
			SWP_NOENDSCHANGING		= 0x0400,
			SWP_NOSIZE			= 0x0001,
			SWP_NOZORDER			= 0x0004,
			SWP_SHOWWINDOW			= 0x0040
		}

		internal enum GetSysColorIndex {
			COLOR_SCROLLBAR			= 0,
			COLOR_BACKGROUND		= 1,
			COLOR_ACTIVECAPTION		= 2,
			COLOR_INACTIVECAPTION		= 3,
			COLOR_MENU			= 4,
			COLOR_WINDOW			= 5,
			COLOR_WINDOWFRAME		= 6,
			COLOR_MENUTEXT			= 7,
			COLOR_WINDOWTEXT		= 8,
			COLOR_CAPTIONTEXT		= 9,
			COLOR_ACTIVEBORDER		= 10,
			COLOR_INACTIVEBORDER		= 11,
			COLOR_APPWORKSPACE		= 12,
			COLOR_HIGHLIGHT			= 13,
			COLOR_HIGHLIGHTTEXT		= 14,
			COLOR_BTNFACE			= 15,
			COLOR_BTNSHADOW			= 16,
			COLOR_GRAYTEXT			= 17,
			COLOR_BTNTEXT			= 18,
			COLOR_INACTIVECAPTIONTEXT	= 19,
			COLOR_BTNHIGHLIGHT		= 20,
			COLOR_3DDKSHADOW		= 21,
			COLOR_3DLIGHT			= 22,
			COLOR_INFOTEXT			= 23,
			COLOR_INFOBK			= 24,
			
			COLOR_HOTLIGHT			= 26,
			COLOR_GRADIENTACTIVECAPTION	= 27,
			COLOR_GRADIENTINACTIVECAPTION	= 28,
			COLOR_MENUHIGHLIGHT		= 29,
			COLOR_MENUBAR			= 30,

			COLOR_DESKTOP			= 1,
			COLOR_3DFACE			= 16,
			COLOR_3DSHADOW			= 16,
			COLOR_3DHIGHLIGHT		= 20,
			COLOR_3DHILIGHT			= 20,
			COLOR_BTNHILIGHT		= 20,
			COLOR_MAXVALUE			= 24,/* Maximum value */
		}       

		private enum LoadCursorType {
			First				= 32512,
			IDC_ARROW			= 32512,
			IDC_IBEAM			= 32513,
			IDC_WAIT			= 32514,
			IDC_CROSS			= 32515,
			IDC_UPARROW			= 32516,
			IDC_SIZE			= 32640,
			IDC_ICON			= 32641,
			IDC_SIZENWSE			= 32642,
			IDC_SIZENESW			= 32643,
			IDC_SIZEWE			= 32644,
			IDC_SIZENS			= 32645,
			IDC_SIZEALL			= 32646,
			IDC_NO				= 32648,
			IDC_HAND			= 32649,
			IDC_APPSTARTING			= 32650,
			IDC_HELP			= 32651,
			Last				= 32651
		}

		private enum AncestorType {
			GA_PARENT = 1,
			GA_ROOT = 2, 
			GA_ROOTOWNER = 3
		}

		[Flags]
		private enum WindowLong {
			GWL_WNDPROC     		= -4,
			GWL_HINSTANCE			= -6,
			GWL_HWNDPARENT      		= -8,
			GWL_STYLE           		= -16,
			GWL_EXSTYLE         		= -20,
			GWL_USERDATA			= -21,
			GWL_ID				= -12
		}

		[Flags]
		private enum LogBrushStyle {
			BS_SOLID			= 0,
			BS_NULL             		= 1,
			BS_HATCHED          		= 2,
			BS_PATTERN          		= 3,
			BS_INDEXED          		= 4,
			BS_DIBPATTERN       		= 5,
			BS_DIBPATTERNPT     		= 6,
			BS_PATTERN8X8       		= 7,
			BS_DIBPATTERN8X8    		= 8,
			BS_MONOPATTERN      		= 9
		}

		[Flags]
		private enum LogBrushHatch {
			HS_HORIZONTAL			= 0,       /* ----- */
			HS_VERTICAL         		= 1,       /* ||||| */
			HS_FDIAGONAL        		= 2,       /* \\\\\ */
			HS_BDIAGONAL        		= 3,       /* ///// */
			HS_CROSS            		= 4,       /* +++++ */
			HS_DIAGCROSS        		= 5,       /* xxxxx */
		}

		internal struct COLORREF {
			internal byte			R;
			internal byte			G;
			internal byte			B;
			internal byte			A;
		}

		[StructLayout(LayoutKind.Sequential)]
		private struct LOGBRUSH {
			internal LogBrushStyle		lbStyle;
			internal COLORREF		lbColor;
			internal LogBrushHatch		lbHatch;
		}

		[StructLayout(LayoutKind.Sequential)]
		internal struct TEXTMETRIC { 
			internal int			tmHeight;
			internal int			tmAscent;
			internal int			tmDescent;
			internal int			tmInternalLeading;
			internal int			tmExternalLeading;
			internal int			tmAveCharWidth;
			internal int			tmMaxCharWidth;
			internal int			tmWeight;
			internal int			tmOverhang;
			internal int			tmDigitizedAspectX;
			internal int			tmDigitizedAspectY;
			internal short			tmFirstChar; 
			internal short			tmLastChar; 
			internal short			tmDefaultChar; 
			internal short			tmBreakChar; 
			internal byte			tmItalic; 
			internal byte			tmUnderlined; 
			internal byte			tmStruckOut; 
			internal byte			tmPitchAndFamily; 
			internal byte			tmCharSet; 
		}

		public enum TernaryRasterOperations : uint
		{
			SRCCOPY = 0x00CC0020,
			SRCPAINT = 0x00EE0086,
			SRCAND = 0x008800C6,
			SRCINVERT = 0x00660046,
			SRCERASE = 0x00440328,
			NOTSRCCOPY = 0x00330008,
			NOTSRCERASE = 0x001100A6,
			MERGECOPY = 0x00C000CA,
			MERGEPAINT = 0x00BB0226,
			PATCOPY = 0x00F00021,
			PATPAINT = 0x00FB0A09,
			PATINVERT = 0x005A0049,
			DSTINVERT = 0x00550009,
			BLACKNESS = 0x00000042,
			WHITENESS = 0x00FF0062
		}

		[Flags]
		private enum ScrollWindowExFlags {
			SW_NONE				= 0x0000,
			SW_SCROLLCHILDREN		= 0x0001,
			SW_INVALIDATE			= 0x0002,
			SW_ERASE			= 0x0004,
			SW_SMOOTHSCROLL			= 0x0010
		}

		internal enum SystemMetrics {
			SM_CXSCREEN			= 0,
			SM_CYSCREEN             	= 1,
			SM_CXVSCROLL            	= 2,
			SM_CYHSCROLL            	= 3,
			SM_CYCAPTION            	= 4,
			SM_CXBORDER             	= 5,
			SM_CYBORDER             	= 6,
			SM_CXDLGFRAME           	= 7,
			SM_CYDLGFRAME           	= 8,
			SM_CYVTHUMB             	= 9,
			SM_CXHTHUMB             	= 10,
			SM_CXICON               	= 11,
			SM_CYICON               	= 12,
			SM_CXCURSOR             	= 13,
			SM_CYCURSOR             	= 14,
			SM_CYMENU               	= 15,
			SM_CXFULLSCREEN         	= 16,
			SM_CYFULLSCREEN         	= 17,
			SM_CYKANJIWINDOW        	= 18,
			SM_MOUSEPRESENT         	= 19,
			SM_CYVSCROLL            	= 20,
			SM_CXHSCROLL            	= 21,
			SM_DEBUG                	= 22,
			SM_SWAPBUTTON           	= 23,
			SM_RESERVED1            	= 24,
			SM_RESERVED2            	= 25,
			SM_RESERVED3            	= 26,
			SM_RESERVED4            	= 27,
			SM_CXMIN                	= 28,
			SM_CYMIN                	= 29,
			SM_CXSIZE               	= 30,
			SM_CYSIZE               	= 31,
			SM_CXFRAME              	= 32,
			SM_CYFRAME              	= 33,
			SM_CXMINTRACK			= 34,
			SM_CYMINTRACK           	= 35,
			SM_CXDOUBLECLK          	= 36,
			SM_CYDOUBLECLK          	= 37,
			SM_CXICONSPACING        	= 38,
			SM_CYICONSPACING        	= 39,
			SM_MENUDROPALIGNMENT    	= 40,
			SM_PENWINDOWS           	= 41,
			SM_DBCSENABLED          	= 42,
			SM_CMOUSEBUTTONS        	= 43,
			SM_CXFIXEDFRAME			= SM_CXDLGFRAME,
			SM_CYFIXEDFRAME			= SM_CYDLGFRAME,
			SM_CXSIZEFRAME			= SM_CXFRAME,
			SM_CYSIZEFRAME			= SM_CYFRAME,
			SM_SECURE               	= 44,
			SM_CXEDGE               	= 45,
			SM_CYEDGE               	= 46,
			SM_CXMINSPACING         	= 47,
			SM_CYMINSPACING         	= 48,
			SM_CXSMICON             	= 49,
			SM_CYSMICON             	= 50,
			SM_CYSMCAPTION          	= 51,
			SM_CXSMSIZE             	= 52,
			SM_CYSMSIZE             	= 53,
			SM_CXMENUSIZE           	= 54,
			SM_CYMENUSIZE           	= 55,
			SM_ARRANGE              	= 56,
			SM_CXMINIMIZED          	= 57,
			SM_CYMINIMIZED          	= 58,
			SM_CXMAXTRACK           	= 59,
			SM_CYMAXTRACK           	= 60,
			SM_CXMAXIMIZED          	= 61,
			SM_CYMAXIMIZED          	= 62,
			SM_NETWORK              	= 63,
			SM_CLEANBOOT            	= 67,
			SM_CXDRAG               	= 68,
			SM_CYDRAG               	= 69,
			SM_SHOWSOUNDS           	= 70,
			SM_CXMENUCHECK          	= 71,
			SM_CYMENUCHECK          	= 72,
			SM_SLOWMACHINE          	= 73,
			SM_MIDEASTENABLED       	= 74,
			SM_MOUSEWHEELPRESENT    	= 75,
			SM_XVIRTUALSCREEN       	= 76,
			SM_YVIRTUALSCREEN       	= 77,
			SM_CXVIRTUALSCREEN      	= 78,
			SM_CYVIRTUALSCREEN      	= 79,
			SM_CMONITORS            	= 80,
			SM_SAMEDISPLAYFORMAT    	= 81,
			SM_IMMENABLED           	= 82,
			SM_CXFOCUSBORDER        	= 83,
			SM_CYFOCUSBORDER        	= 84,
			SM_TABLETPC             	= 86,
			SM_MEDIACENTER          	= 87,
			SM_CMETRICS             	= 88
		}

		// We'll only support _WIN32_IE < 0x0500 for now
		internal enum NotifyIconMessage {
			NIM_ADD				= 0x00000000,
			NIM_MODIFY			= 0x00000001,
			NIM_DELETE			= 0x00000002,
		}

		[Flags]
		internal enum NotifyIconFlags {
			NIF_MESSAGE			= 0x00000001,
			NIF_ICON			= 0x00000002,
			NIF_TIP				= 0x00000004,
			NIF_STATE			= 0x00000008,
			NIF_INFO			= 0x00000010			
		}

		[StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
		internal struct NOTIFYICONDATA {
			internal uint				cbSize;
			internal IntPtr				hWnd;
			internal uint				uID;
			internal NotifyIconFlags	uFlags;
			internal uint				uCallbackMessage;
			internal IntPtr				hIcon;
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst=128)]
			internal string				szTip;
			internal int				dwState;
			internal int				dwStateMask;
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst=256)]
			internal string				szInfo;
			internal int				uTimeoutOrVersion;
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst=64)]
			internal string				szInfoTitle;
#if NET_2_0
			internal ToolTipIcon		dwInfoFlags;
#else
			internal int				dwInfoFlags;
#endif
		}

		[Flags]
		internal enum DCExFlags {
			DCX_WINDOW			= 0x00000001,
			DCX_CACHE			= 0x00000002,
			DCX_NORESETATTRS     		= 0x00000004,
			DCX_CLIPCHILDREN     		= 0x00000008,
			DCX_CLIPSIBLINGS     		= 0x00000010,
			DCX_PARENTCLIP       		= 0x00000020,
			DCX_EXCLUDERGN       		= 0x00000040,
			DCX_INTERSECTRGN     		= 0x00000080,
			DCX_EXCLUDEUPDATE    		= 0x00000100,
			DCX_INTERSECTUPDATE  		= 0x00000200,
			DCX_LOCKWINDOWUPDATE 		= 0x00000400,
			DCX_USESTYLE			= 0x00010000,
			DCX_VALIDATE         		= 0x00200000
		}

		[StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
		internal struct CLIENTCREATESTRUCT {
			internal IntPtr			hWindowMenu;
			internal uint			idFirstChild;
		}

		private enum ClassLong : int {
			GCL_MENUNAME			= -8,
			GCL_HBRBACKGROUND		= -10,
			GCL_HCURSOR         		= -12,
			GCL_HICON            		= -14,
			GCL_HMODULE          		= -16,
			GCL_CBWNDEXTRA       		= -18,
			GCL_CBCLSEXTRA       		= -20,
			GCL_WNDPROC          		= -24,
			GCL_STYLE            		= -26,
			GCW_ATOM             		= -32,
			GCL_HICONSM			= -34
		}

		[Flags]
		internal enum GAllocFlags : uint {
			GMEM_FIXED			= 0x0000,
			GMEM_MOVEABLE			= 0x0002,
			GMEM_NOCOMPACT			= 0x0010,
			GMEM_NODISCARD			= 0x0020,
			GMEM_ZEROINIT			= 0x0040,
			GMEM_MODIFY			= 0x0080,
			GMEM_DISCARDABLE		= 0x0100,
			GMEM_NOT_BANKED			= 0x1000,
			GMEM_SHARE          		= 0x2000,
			GMEM_DDESHARE			= 0x2000,
			GMEM_NOTIFY			= 0x4000,
			GMEM_LOWER			= GMEM_NOT_BANKED,
			GMEM_VALID_FLAGS		= 0x7F72,
			GMEM_INVALID_HANDLE 		= 0x8000,
			GHND                		= (GMEM_MOVEABLE | GMEM_ZEROINIT),
			GPTR                		= (GMEM_FIXED | GMEM_ZEROINIT)
		}

		internal enum ROP2DrawMode : int {
			R2_BLACK			= 1,
			R2_NOTMERGEPEN      		= 2,
			R2_MASKNOTPEN       		= 3,
			R2_NOTCOPYPEN       		= 4,
			R2_MASKPENNOT       		= 5,
			R2_NOT              		= 6,
			R2_XORPEN           		= 7,
			R2_NOTMASKPEN       		= 8,
			R2_MASKPEN          		= 9,
			R2_NOTXORPEN        		= 10,
			R2_NOP              		= 11,
			R2_MERGENOTPEN      		= 12,
			R2_COPYPEN          		= 13,
			R2_MERGEPENNOT      		= 14,
			R2_MERGEPEN         		= 15,
			R2_WHITE            		= 16,
			R2_LAST             		= 16
		}

		internal enum PenStyle : int {
			PS_SOLID			= 0,
			PS_DASH             		= 1,
			PS_DOT              		= 2,
			PS_DASHDOT          		= 3,
			PS_DASHDOTDOT       		= 4,
			PS_NULL             		= 5,
			PS_INSIDEFRAME      		= 6,
			PS_USERSTYLE        		= 7,
			PS_ALTERNATE        		= 8
		}

		internal enum PatBltRop : int {
			PATCOPY   = 0xf00021,
			PATINVERT = 0x5a0049,
			DSTINVERT = 0x550009,
			BLACKNESS = 0x000042,
			WHITENESS = 0xff0062,
		}

		internal enum StockObject : int {
			WHITE_BRUSH			= 0,
			LTGRAY_BRUSH        		= 1,
			GRAY_BRUSH          		= 2,
			DKGRAY_BRUSH        		= 3,
			BLACK_BRUSH         		= 4,
			NULL_BRUSH          		= 5,
			HOLLOW_BRUSH        		= NULL_BRUSH,
			WHITE_PEN   			= 6,
			BLACK_PEN           		= 7,
			NULL_PEN            		= 8,
			OEM_FIXED_FONT      		= 10,
			ANSI_FIXED_FONT     		= 11,
			ANSI_VAR_FONT       		= 12,
			SYSTEM_FONT         		= 13,
			DEVICE_DEFAULT_FONT 		= 14,
			DEFAULT_PALETTE     		= 15,
			SYSTEM_FIXED_FONT  		= 16
		}

		internal enum HatchStyle : int {
			HS_HORIZONTAL			= 0,
			HS_VERTICAL         		= 1,
			HS_FDIAGONAL        		= 2,
			HS_BDIAGONAL        		= 3,
			HS_CROSS            		= 4,
			HS_DIAGCROSS        		= 5
		}

		[Flags]
		internal enum SndFlags : int {
			SND_SYNC			= 0x0000,
			SND_ASYNC			= 0x0001,
			SND_NODEFAULT			= 0x0002,
			SND_MEMORY			= 0x0004,
			SND_LOOP			= 0x0008,
			SND_NOSTOP			= 0x0010,
			SND_NOWAIT     			= 0x00002000,
			SND_ALIAS			= 0x00010000,
			SND_ALIAS_ID			= 0x00110000,
			SND_FILENAME			= 0x00020000,
			SND_RESOURCE			= 0x00040004,
			SND_PURGE			= 0x0040,
			SND_APPLICATION			= 0x0080,
		}

		[Flags]
		internal enum LayeredWindowAttributes : int {
			LWA_COLORKEY		= 0x1,
			LWA_ALPHA			= 0x2,
		}

		public enum ACLineStatus : byte {
			Offline = 0,
			Online = 1,
			Unknown = 255
		}

		public enum BatteryFlag : byte {
			High = 1,
			Low = 2,
			Critical = 4,
			Charging = 8,
			NoSystemBattery = 128,
			Unknown = 255
		}

		[StructLayout (LayoutKind.Sequential)]
		public class SYSTEMPOWERSTATUS {
			public ACLineStatus _ACLineStatus;
			public BatteryFlag _BatteryFlag;
			public Byte _BatteryLifePercent;
			public Byte _Reserved1;
			public Int32 _BatteryLifeTime;
			public Int32 _BatteryFullLifeTime;
		}
		#endregion

		#region Constructor & Destructor
		private XplatUIWin32() {
			// Handle singleton stuff first
			ref_count=0;

			mouse_state = MouseButtons.None;
			mouse_position = Point.Empty;

			grab_confined = false;
			grab_area = Rectangle.Empty;

			message_queue = new Queue();

			themes_enabled = false;

			wnd_proc = new WndProc(InternalWndProc);

			FosterParent=Win32CreateWindow(WindowExStyles.WS_EX_TOOLWINDOW, "static", "Foster Parent Window", WindowStyles.WS_OVERLAPPEDWINDOW, 0, 0, 0, 0, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);

			if (FosterParent==IntPtr.Zero) {
				Win32MessageBox(IntPtr.Zero, "Could not create foster window, win32 error " + Win32GetLastError().ToString(), "Oops", 0);
			}

			scroll_height = Win32GetSystemMetrics(SystemMetrics.SM_CYHSCROLL);
			scroll_width = Win32GetSystemMetrics(SystemMetrics.SM_CXVSCROLL);

			timer_list = new Hashtable ();
			registered_classes = new Hashtable ();
		}
		#endregion	// Constructor & Destructor

		#region Private Support Methods

		private string RegisterWindowClass (int classStyle)
		{
			string class_name;

			lock (registered_classes) {
				class_name = (string)registered_classes[classStyle];

				if (class_name != null)
					return class_name;

				class_name = string.Format ("Mono.WinForms.{0}.{1}", System.Threading.Thread.GetDomainID ().ToString (), classStyle);

				WNDCLASS wndClass;

				wndClass.style = classStyle;
				wndClass.lpfnWndProc = wnd_proc;
				wndClass.cbClsExtra = 0;
				wndClass.cbWndExtra = 0;
				wndClass.hbrBackground = (IntPtr)(GetSysColorIndex.COLOR_WINDOW + 1);
				wndClass.hCursor = Win32LoadCursor (IntPtr.Zero, LoadCursorType.IDC_ARROW);
				wndClass.hIcon = IntPtr.Zero;
				wndClass.hInstance = IntPtr.Zero;
				wndClass.lpszClassName = class_name;
				wndClass.lpszMenuName = "";

				bool result = Win32RegisterClass (ref wndClass);

				if (result == false)
					Win32MessageBox (IntPtr.Zero, "Could not register the window class, win32 error " + Win32GetLastError ().ToString (), "Oops", 0);

				registered_classes[classStyle] = class_name;
			}
			
			return class_name;
		}

		private static bool RetrieveMessage(ref MSG msg) {
			MSG	message;

			if (message_queue.Count == 0) {
				return false;
			}

			message = (MSG)message_queue.Dequeue();
			msg = message;

			return true;
		}

		private static bool StoreMessage(ref MSG msg) {
			MSG message = new MSG();

			message = msg;
			message_queue.Enqueue(message);

			return true;
		}

		internal static String AnsiToString(IntPtr ansi_data) {
			return (string)Marshal.PtrToStringAnsi(ansi_data);
		}

		internal static String UnicodeToString(IntPtr unicode_data) {
			return (string)Marshal.PtrToStringUni(unicode_data);
		}

		internal static Image DIBtoImage(IntPtr dib_data) {
			BITMAPINFOHEADER	bmi;
			int			ncolors;
			int			imagesize;
			//int			palettesize;
			Bitmap			bmp;
			BitmapData		bits;
			ColorPalette		pal;
			int[]			palette;
			byte[]			imagebits;
			int			bytesPerLine;

			bmi = (BITMAPINFOHEADER)Marshal.PtrToStructure(dib_data, typeof(BITMAPINFOHEADER));

			ncolors = (int)bmi.biClrUsed;
			if (ncolors == 0) {
				if (bmi.biBitCount < 24) {
					ncolors = (int)(1 << bmi.biBitCount);
				}
			}
			//palettesize = ncolors * 4;

			imagesize = (int)bmi.biSizeImage;
			if (imagesize == 0) {
				imagesize = (int)(((((bmi.biWidth * bmi.biBitCount) + 31) & ~31) >> 3) * bmi.biHeight);
			}

			switch(bmi.biBitCount) {
				case 1: {	// Monochrome
					bmp = new Bitmap(bmi.biWidth, bmi.biHeight, PixelFormat.Format1bppIndexed);
					palette = new int[2];
					break;
				}

				case 4: {	// 4bpp
					bmp = new Bitmap(bmi.biWidth, bmi.biHeight, PixelFormat.Format4bppIndexed);
					palette = new int[16];
					break;
				}

				case 8: {	// 8bpp
					bmp = new Bitmap(bmi.biWidth, bmi.biHeight, PixelFormat.Format8bppIndexed);
					palette = new int[256];
					break;
				}

				case 24:
				case 32: {	// 32bpp
					bmp = new Bitmap(bmi.biWidth, bmi.biHeight, PixelFormat.Format32bppArgb);
					palette = new int[0];
					break;
				}

				default: {
					throw new Exception("Unexpected number of bits:" + bmi.biBitCount.ToString());
				}
			}

			if (bmi.biBitCount < 24) {
				pal = bmp.Palette;				// Managed palette
				Marshal.Copy((IntPtr)((int)dib_data + Marshal.SizeOf(typeof(BITMAPINFOHEADER))), palette, 0, palette.Length);

				for (int i = 0; i < ncolors; i++) {
					pal.Entries[i] = Color.FromArgb(palette[i] | unchecked((int)0xff000000));
				}
				bmp.Palette = pal;
			}

			bytesPerLine = (int)((((bmi.biWidth * bmi.biBitCount) + 31) & ~31) >> 3);
			bits = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.WriteOnly, bmp.PixelFormat);

			imagebits = new byte[bytesPerLine];

			for (int y = 0; y < bmi.biHeight; y++) {
				// Copy from source to managed
				Marshal.Copy((IntPtr)((int)dib_data + Marshal.SizeOf(typeof(BITMAPINFOHEADER)) + palette.Length * 4 + bytesPerLine * y), imagebits, 0, bytesPerLine);

				// Copy from managed to dest
				Marshal.Copy(imagebits, 0, (IntPtr)((int)bits.Scan0 + bits.Stride * (bmi.biHeight - 1 - y)), imagebits.Length);
			}

			bmp.UnlockBits(bits);

			return bmp;
		}

		internal static byte[] ImageToDIB(Image image) {
			MemoryStream	ms;
			byte[]		buffer;
			byte[]		retbuf;

			ms = new MemoryStream();
			image.Save(ms, ImageFormat.Bmp);
			buffer = ms.GetBuffer();

			// Filter out the file header
			retbuf = new byte[buffer.Length];
			Array.Copy(buffer, 14, retbuf, 0, buffer.Length - 14);
			return retbuf;
		}

		internal static IntPtr DupGlobalMem(IntPtr mem) {
			IntPtr	dup;
			IntPtr	dup_ptr;
			IntPtr	mem_ptr;
			uint	len;

			len = Win32GlobalSize(mem);
			mem_ptr = Win32GlobalLock(mem);

			dup = Win32GlobalAlloc(GAllocFlags.GMEM_MOVEABLE, (int)len);
			dup_ptr = Win32GlobalLock(dup);

			Win32CopyMemory(dup_ptr, mem_ptr, (int)len);

			Win32GlobalUnlock(mem);
			Win32GlobalUnlock(dup);

			return dup;
		}
		
		private int GetSystemParametersInfoInt (SPIAction spi)
		{
			int value = 0;
			
			Win32SystemParametersInfo (spi, 0, ref value, 0);
			
			return value;
		}

		private bool GetSystemParametersInfoBool (SPIAction spi)
		{
			bool value = false;
			
			Win32SystemParametersInfo (spi, 0, ref value, 0);

			return value;
		}
		#endregion	// Private Support Methods

		#region Static Properties
		internal override int ActiveWindowTrackingDelay {
			get { return GetSystemParametersInfoInt (SPIAction.SPI_GETACTIVEWNDTRKTIMEOUT); }
		}

		internal override int CaretWidth {
			get { 
				// Supported on 2k, XP, 2k3 +
				if (Environment.OSVersion.Version.Major < 5)
					throw new NotSupportedException ();
					
				return GetSystemParametersInfoInt (SPIAction.SPI_GETCARETWIDTH);
			}
		}

		internal override int FontSmoothingContrast {
			get {
				// Supported on XP, 2k3 +
				if (Environment.OSVersion.Version.Major < 5 || (Environment.OSVersion.Version.Major == 5 && Environment.OSVersion.Version.Minor == 0))
					throw new NotSupportedException ();
					
				return GetSystemParametersInfoInt (SPIAction.SPI_GETFONTSMOOTHINGCONTRAST);
			}
		}

		internal override int FontSmoothingType {
			get {
				// Supported on XP, 2k3 +
				if (Environment.OSVersion.Version.Major < 5 || (Environment.OSVersion.Version.Major == 5 && Environment.OSVersion.Version.Minor == 0))
					throw new NotSupportedException ();

				return GetSystemParametersInfoInt (SPIAction.SPI_GETFONTSMOOTHINGTYPE);
			}
		}

		internal override int HorizontalResizeBorderThickness {
			get { return Win32GetSystemMetrics (SystemMetrics.SM_CXSIZEFRAME); }
		}

		internal override bool IsActiveWindowTrackingEnabled {
			get { return GetSystemParametersInfoBool (SPIAction.SPI_GETACTIVEWINDOWTRACKING); }
		}

		internal override bool IsComboBoxAnimationEnabled {
			get { return GetSystemParametersInfoBool (SPIAction.SPI_GETCOMBOBOXANIMATION); }
		}

		internal override bool IsDropShadowEnabled {
			get {
				// Supported on XP, 2k3 +
				if (Environment.OSVersion.Version.Major < 5 || (Environment.OSVersion.Version.Major == 5 && Environment.OSVersion.Version.Minor == 0))
					throw new NotSupportedException ();

				return GetSystemParametersInfoBool (SPIAction.SPI_GETDROPSHADOW);
			}
		}

		internal override bool IsFontSmoothingEnabled {
			get { return GetSystemParametersInfoBool (SPIAction.SPI_GETFONTSMOOTHING); }
		}

		internal override bool IsHotTrackingEnabled {
			get { return GetSystemParametersInfoBool (SPIAction.SPI_GETHOTTRACKING); }
		}

		internal override bool IsIconTitleWrappingEnabled {
			get { return GetSystemParametersInfoBool (SPIAction.SPI_GETICONTITLEWRAP); }
		}

		internal override bool IsKeyboardPreferred {
			get { return GetSystemParametersInfoBool (SPIAction.SPI_GETKEYBOARDPREF); }
		}

		internal override bool IsListBoxSmoothScrollingEnabled {
			get { return GetSystemParametersInfoBool (SPIAction.SPI_GETLISTBOXSMOOTHSCROLLING); }
		}

		internal override bool IsMenuAnimationEnabled {
			get { return GetSystemParametersInfoBool (SPIAction.SPI_GETMENUANIMATION); }
		}

		internal override bool IsMenuFadeEnabled {
			get { return GetSystemParametersInfoBool (SPIAction.SPI_GETMENUFADE); }
		}

		internal override bool IsMinimizeRestoreAnimationEnabled {
			get {
				ANIMATIONINFO ai = new ANIMATIONINFO ();
				ai.cbSize = (uint)Marshal.SizeOf (ai);
				
				Win32SystemParametersInfo (SPIAction.SPI_GETANIMATION, 0, ref ai, 0);
				return ai.iMinAnimate == 0 ? false : true;
			}
		}

		internal override bool IsSelectionFadeEnabled {
			get { return GetSystemParametersInfoBool (SPIAction.SPI_GETSELECTIONFADE); }
		}

		internal override bool IsSnapToDefaultEnabled {
			get { return GetSystemParametersInfoBool (SPIAction.SPI_GETSNAPTODEFBUTTON); }
		}

		internal override bool IsTitleBarGradientEnabled {
			get { return GetSystemParametersInfoBool (SPIAction.SPI_GETGRADIENTCAPTIONS); }
		}

		internal override bool IsToolTipAnimationEnabled {
			get { return GetSystemParametersInfoBool (SPIAction.SPI_GETTOOLTIPANIMATION); }
		}

		internal override Size MenuBarButtonSize {
			get {
				return new Size (Win32GetSystemMetrics (SystemMetrics.SM_CXMENUSIZE),
					Win32GetSystemMetrics (SystemMetrics.SM_CYMENUSIZE));
			}
		}

		public override Size MenuButtonSize {
			get {
				return new Size (
					Win32GetSystemMetrics (SystemMetrics.SM_CXMENUSIZE),
					Win32GetSystemMetrics (SystemMetrics.SM_CYMENUSIZE));
			}
		}

		internal override int MenuShowDelay {
			get { return GetSystemParametersInfoInt (SPIAction.SPI_GETMENUSHOWDELAY); }
		}

		internal override int MouseSpeed {
			get { return GetSystemParametersInfoInt (SPIAction.SPI_GETMOUSESPEED); }
		}

		internal override LeftRightAlignment PopupMenuAlignment {
			get { return GetSystemParametersInfoBool (SPIAction.SPI_GETMENUDROPALIGNMENT) == true ? LeftRightAlignment.Left : LeftRightAlignment.Right; }
		}

#if NET_2_0
		internal override PowerStatus PowerStatus {
			get {
				SYSTEMPOWERSTATUS p = new SYSTEMPOWERSTATUS ();
				
				Win32GetSystemPowerStatus (p);
				
				PowerStatus ps = new PowerStatus ((BatteryChargeStatus)p._BatteryFlag, p._BatteryFullLifeTime, (float)p._BatteryLifePercent / 255f, p._BatteryLifeTime, (PowerLineStatus)p._ACLineStatus);
				
				return ps;
			}
		}
#endif

		internal override int SizingBorderWidth {
			get { return Win32GetSystemMetrics (SystemMetrics.SM_CXSIZEFRAME); }
		}

		internal override Size SmallCaptionButtonSize {
			get {
				return new Size (Win32GetSystemMetrics (SystemMetrics.SM_CXSMSIZE),
					Win32GetSystemMetrics (SystemMetrics.SM_CYSMSIZE));
			}
		}

		internal override bool UIEffectsEnabled {
			get { return GetSystemParametersInfoBool (SPIAction.SPI_GETUIEFFECTS); }
		}

		internal override int VerticalResizeBorderThickness {
			get { return Win32GetSystemMetrics (SystemMetrics.SM_CYSIZEFRAME); }
		}

		internal override void RaiseIdle (EventArgs e)
		{
			if (Idle != null)
				Idle (this, e);
		}

		internal override Keys ModifierKeys {
			get {
				short	state;
				Keys	key_state;

				key_state = Keys.None;

				state = Win32GetKeyState(VirtualKeys.VK_SHIFT);
				if ((state & 0x8000) != 0) {
					key_state |= Keys.Shift;
				}
				state = Win32GetKeyState(VirtualKeys.VK_CONTROL);
				if ((state & 0x8000) != 0) {
					key_state |= Keys.Control;
				}

				state = Win32GetKeyState(VirtualKeys.VK_MENU);
				if ((state & 0x8000) != 0) {
					key_state |= Keys.Alt;
				}

				return key_state;
			}
		}

		internal override MouseButtons MouseButtons {
			get {
				return mouse_state;
			}
		}

		internal override Point MousePosition {
			get {
				return mouse_position;
			}
		}

		internal override Size MouseHoverSize {
			get {
				int	width = 4;
				int	height = 4;

				Win32SystemParametersInfo(SPIAction.SPI_GETMOUSEHOVERWIDTH, 0, ref width, 0);
				Win32SystemParametersInfo(SPIAction.SPI_GETMOUSEHOVERWIDTH, 0, ref height, 0);
				return new Size(width, height);
			}
		}

		internal override int MouseHoverTime {
			get {
				int time = 500;

				Win32SystemParametersInfo(SPIAction.SPI_GETMOUSEHOVERTIME, 0, ref time, 0);
				return time;
			}
		}

		internal override int MouseWheelScrollDelta {
			get {
				int delta = 120;
				Win32SystemParametersInfo(SPIAction.SPI_GETWHEELSCROLLLINES, 0, ref delta, 0);
				return delta;
			}
		}
		
		internal override int HorizontalScrollBarHeight {
			get {
				return scroll_height;
			}
		}

		internal override bool UserClipWontExposeParent {
			get {
				return false;
			}
		}


		internal override int VerticalScrollBarWidth {
			get {
				return scroll_width;
			}
		}

		internal override int MenuHeight {
			get {
				return Win32GetSystemMetrics(SystemMetrics.SM_CYMENU);
			}
		}

		internal override Size Border3DSize {
			get {
				return new Size (Win32GetSystemMetrics (SystemMetrics.SM_CXEDGE),
					Win32GetSystemMetrics (SystemMetrics.SM_CYEDGE));
			}
		}

		internal override Size BorderSize {
			get {
				return new Size (Win32GetSystemMetrics (SystemMetrics.SM_CXBORDER),
					Win32GetSystemMetrics (SystemMetrics.SM_CYBORDER));
			}
		}

		internal override bool DropTarget {
			get {
				return false;
			}

			set {
				if (value) {
					//throw new NotImplementedException("Need to figure out D'n'D for Win32");
				}
			}
		}

		internal override Size CaptionButtonSize {
			get {
				return new Size (Win32GetSystemMetrics (SystemMetrics.SM_CXSIZE),
					Win32GetSystemMetrics (SystemMetrics.SM_CYSIZE));
			}
		}

		internal override int CaptionHeight {
			get {
				return Win32GetSystemMetrics(SystemMetrics.SM_CYCAPTION);
			}
		}

		internal override Size CursorSize {
			get {
				return new Size(Win32GetSystemMetrics(SystemMetrics.SM_CXCURSOR), Win32GetSystemMetrics(SystemMetrics.SM_CYCURSOR));
			}
		}

		internal override bool DragFullWindows {
			get {
				int full = 0;
				Win32SystemParametersInfo (SPIAction.SPI_GETDRAGFULLWINDOWS, 0, ref full, 0);
				return (full != 0);
			}
		}

		internal override Size DragSize {
			get {
				return new Size(Win32GetSystemMetrics(SystemMetrics.SM_CXDRAG), Win32GetSystemMetrics(SystemMetrics.SM_CYDRAG));
			}
		}

		internal override Size DoubleClickSize {
			get {
				return new Size (Win32GetSystemMetrics (SystemMetrics.SM_CXDOUBLECLK),
					Win32GetSystemMetrics (SystemMetrics.SM_CYDOUBLECLK));
			}
		}

		internal override int DoubleClickTime {
			get {
				return Win32GetDoubleClickTime ();
			}
		}

		internal override Size FixedFrameBorderSize {
			get {
				return new Size (Win32GetSystemMetrics (SystemMetrics.SM_CXFIXEDFRAME),
					Win32GetSystemMetrics (SystemMetrics.SM_CYFIXEDFRAME));
			}
		}

		internal override Size FrameBorderSize { 
			get {
				return new Size(Win32GetSystemMetrics(SystemMetrics.SM_CXFRAME), Win32GetSystemMetrics(SystemMetrics.SM_CYFRAME));
			}
		}

		internal override Size IconSize {
			get {
				return new Size(Win32GetSystemMetrics(SystemMetrics.SM_CXICON), Win32GetSystemMetrics(SystemMetrics.SM_CYICON));
			}
		}

		internal override Size MaxWindowTrackSize {
			get {
				return new Size(Win32GetSystemMetrics(SystemMetrics.SM_CXMAXTRACK), Win32GetSystemMetrics(SystemMetrics.SM_CYMAXTRACK));
			}
		}

		internal override bool MenuAccessKeysUnderlined {
			get {
				int underlined = 0;
				Win32SystemParametersInfo (SPIAction.SPI_GETKEYBOARDCUES, 0, ref underlined, 0);
				return (underlined != 0);
			}
		}

		internal override Size MinimizedWindowSize {
			get {
				return new Size(Win32GetSystemMetrics(SystemMetrics.SM_CXMINIMIZED), Win32GetSystemMetrics(SystemMetrics.SM_CYMINIMIZED));
			}
		}

		internal override Size MinimizedWindowSpacingSize {
			get {
				return new Size(Win32GetSystemMetrics(SystemMetrics.SM_CXMINSPACING), Win32GetSystemMetrics(SystemMetrics.SM_CYMINSPACING));
			}
		}

		internal override Size MinimumWindowSize {
			get {
				return new Size(Win32GetSystemMetrics(SystemMetrics.SM_CXMIN), Win32GetSystemMetrics(SystemMetrics.SM_CYMIN));
			}
		}

		internal override Size MinWindowTrackSize {
			get {
				return new Size(Win32GetSystemMetrics(SystemMetrics.SM_CXMINTRACK), Win32GetSystemMetrics(SystemMetrics.SM_CYMINTRACK));
			}
		}

		internal override Size SmallIconSize {
			get {
				return new Size(Win32GetSystemMetrics(SystemMetrics.SM_CXSMICON), Win32GetSystemMetrics(SystemMetrics.SM_CYSMICON));
			}
		}

		internal override int MouseButtonCount {
			get {
				return Win32GetSystemMetrics(SystemMetrics.SM_CMOUSEBUTTONS);
			}
		}

		internal override bool MouseButtonsSwapped {
			get {
				return Win32GetSystemMetrics(SystemMetrics.SM_SWAPBUTTON) != 0;
			}
		}

		internal override bool MouseWheelPresent {
			get {
				return Win32GetSystemMetrics(SystemMetrics.SM_MOUSEWHEELPRESENT) != 0;
			}
		}

		internal override Rectangle VirtualScreen {
			get {
				return new Rectangle(	Win32GetSystemMetrics(SystemMetrics.SM_XVIRTUALSCREEN), Win32GetSystemMetrics(SystemMetrics.SM_YVIRTUALSCREEN),
							Win32GetSystemMetrics(SystemMetrics.SM_CXVIRTUALSCREEN), Win32GetSystemMetrics(SystemMetrics.SM_CYVIRTUALSCREEN));
			}
		}

		internal override Rectangle WorkingArea {
			get {
				RECT	rect;

				rect = new RECT();
				Win32SystemParametersInfo(SPIAction.SPI_GETWORKAREA, 0, ref rect, 0);
				return new Rectangle(rect.left, rect.top, rect.right - rect.left, rect.bottom - rect.top);
				//return new Rectangle(0, 0, Win32GetSystemMetrics(SystemMetrics.SM.SM_CXSCREEN), Win32GetSystemMetrics(SystemMetrics.SM_CYSCREEN));
			}
		}

		internal override bool ThemesEnabled {
			get {
				return XplatUIWin32.themes_enabled;
			}
		}

		internal override bool RequiresPositiveClientAreaSize {
			get {
				return false;
			}
		}

		public override int ToolWindowCaptionHeight {
			get {
				return Win32GetSystemMetrics (SystemMetrics.SM_CYSMCAPTION);
			}
		}

		public override Size ToolWindowCaptionButtonSize {
			get {
				return new Size (
					Win32GetSystemMetrics (SystemMetrics.SM_CXSMSIZE),
					Win32GetSystemMetrics (SystemMetrics.SM_CYSMSIZE));
			}
		}
		#endregion	// Static Properties

		#region Singleton Specific Code
		public static XplatUIWin32 GetInstance() {
			if (instance==null) {
				instance=new XplatUIWin32();
			}
			ref_count++;
			return instance;
		}

		public int Reference {
			get {
				return ref_count;
			}
		}
		#endregion

		#region Public Static Methods
		internal override IntPtr InitializeDriver() {
			return IntPtr.Zero;
		}

		internal override void ShutdownDriver(IntPtr token) {
			Console.WriteLine("XplatUIWin32 ShutdownDriver called");
		}


		internal void Version() {
			Console.WriteLine("Xplat version $revision: $");
		}

		string GetSoundAlias (AlertType alert)
		{
			switch (alert) {
				case AlertType.Error:
					return "SystemHand";
				case AlertType.Question:
					return "SystemQuestion";
				case AlertType.Warning:
					return "SystemExclamation";
				case AlertType.Information:
					return "SystemAsterisk";
				default:
					return "SystemDefault";
			}
		}

		internal override void AudibleAlert(AlertType alert) {
			Win32PlaySound(GetSoundAlias (alert), IntPtr.Zero, SndFlags.SND_ALIAS_ID | SndFlags.SND_ASYNC | SndFlags.SND_NOSTOP | SndFlags.SND_NOWAIT);
		}

		internal override void BeginMoveResize (IntPtr handle) {
		}

		internal override void GetDisplaySize(out Size size) {
			RECT	rect;

			Win32GetWindowRect(Win32GetDesktopWindow(), out rect);

			size = new Size(rect.right - rect.left, rect.bottom - rect.top);
		}

		internal override void EnableThemes() {
			themes_enabled=true;
		}

		internal override IntPtr CreateWindow(CreateParams cp) {
			IntPtr	WindowHandle;
			IntPtr	ParentHandle;
			Hwnd	hwnd;

			hwnd = new Hwnd();

			ParentHandle=cp.Parent;

			if ((ParentHandle==IntPtr.Zero) && (cp.Style & (int)(WindowStyles.WS_CHILD))!=0) {
				// We need to use our foster parent window until this poor child gets it's parent assigned
				ParentHandle = FosterParent;
			}

			if ( ((cp.Style & (int)(WindowStyles.WS_CHILD | WindowStyles.WS_POPUP))==0) && ((cp.ExStyle & (int)WindowExStyles.WS_EX_APPWINDOW) == 0)) {
				// If we want to be hidden from the taskbar we need to be 'owned' by 
				// something not on the taskbar. FosterParent is just that
				ParentHandle = FosterParent;
			}

			Point location;
			if (cp.HasWindowManager) {
				location = Hwnd.GetNextStackedFormLocation (cp, Hwnd.ObjectFromHandle (cp.Parent));
			} else {
				location = new Point (cp.X, cp.Y);
			}

			string class_name = RegisterWindowClass (cp.ClassStyle);
			HwndCreating = hwnd;

			// We cannot actually send the WS_EX_MDICHILD flag to Windows because we
			// are faking MDI, not uses Windows' version.
			if ((cp.WindowExStyle & WindowExStyles.WS_EX_MDICHILD) == WindowExStyles.WS_EX_MDICHILD)
				cp.WindowExStyle ^= WindowExStyles.WS_EX_MDICHILD;
				
			WindowHandle = Win32CreateWindow (cp.WindowExStyle, class_name, cp.Caption, cp.WindowStyle, location.X, location.Y, cp.Width, cp.Height, ParentHandle, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);

			HwndCreating = null;

			if (WindowHandle==IntPtr.Zero) {
				int error = Marshal.GetLastWin32Error ();

				Win32MessageBox(IntPtr.Zero, "Error : " + error.ToString(), "Failed to create window, class '"+cp.ClassName+"'", 0);
			}

			hwnd.ClientWindow = WindowHandle;
			hwnd.Mapped = true;
			Win32SetWindowLong(WindowHandle, WindowLong.GWL_USERDATA, (uint)ThemeEngine.Current.DefaultControlBackColor.ToArgb());

			return WindowHandle;
		}

		internal override IntPtr CreateWindow(IntPtr Parent, int X, int Y, int Width, int Height) {
			CreateParams create_params = new CreateParams();

			create_params.Caption = "";
			create_params.X = X;
			create_params.Y = Y;
			create_params.Width = Width;
			create_params.Height = Height;

			create_params.ClassName=XplatUI.GetDefaultClassName (GetType ());
			create_params.ClassStyle = 0;
			create_params.ExStyle=0;
			create_params.Parent=IntPtr.Zero;
			create_params.Param=0;

			return CreateWindow(create_params);
		}

		internal override void DestroyWindow(IntPtr handle) {
			Hwnd	hwnd;

			hwnd = Hwnd.ObjectFromHandle(handle);
			Win32DestroyWindow(handle);
			hwnd.Dispose();
			return;
		}

		internal override void SetWindowMinMax(IntPtr handle, Rectangle maximized, Size min, Size max) {
			// We do nothing, Form has to handle WM_GETMINMAXINFO
		}


		internal override FormWindowState GetWindowState(IntPtr handle) {
			uint style;

			style = Win32GetWindowLong(handle, WindowLong.GWL_STYLE);
			if ((style & (uint)WindowStyles.WS_MAXIMIZE) != 0) {
				return FormWindowState.Maximized;
			} else if ((style & (uint)WindowStyles.WS_MINIMIZE) != 0) {
				return FormWindowState.Minimized;
			}
			return FormWindowState.Normal;
		}

		internal override void SetWindowState(IntPtr hwnd, FormWindowState state) {
			switch(state) {
				case FormWindowState.Normal: {
					Win32ShowWindow(hwnd, WindowPlacementFlags.SW_RESTORE);
					return;
				}

				case FormWindowState.Minimized: {
					Win32ShowWindow(hwnd, WindowPlacementFlags.SW_MINIMIZE);
					return;
				}

				case FormWindowState.Maximized: {
					Win32ShowWindow(hwnd, WindowPlacementFlags.SW_MAXIMIZE);
					return;
				}
			}
		}

		internal override void SetWindowStyle(IntPtr handle, CreateParams cp) {

			Win32SetWindowLong(handle, WindowLong.GWL_STYLE, (uint)cp.Style);
			Win32SetWindowLong(handle, WindowLong.GWL_EXSTYLE, (uint)cp.ExStyle);

			// From MSDN:
			// Certain window data is cached, so changes you make using SetWindowLong
			// will not take effect until you call the SetWindowPos function. Specifically, 
			// if you change any of the frame styles, you must call SetWindowPos with
			// the SWP_FRAMECHANGED flag for the cache to be updated properly.
			if (cp.control is Form)
				XplatUI.RequestNCRecalc (handle);
		}

		internal override double GetWindowTransparency(IntPtr handle)
		{
			LayeredWindowAttributes lwa;
			COLORREF clrRef;
			byte alpha;

			if (0 == Win32GetLayeredWindowAttributes (handle, out clrRef, out alpha, out lwa))
				return 1.0;

			return ((double)alpha) / 255.0;
		}

		internal override void SetWindowTransparency(IntPtr handle, double transparency, Color key) {
			LayeredWindowAttributes lwa = LayeredWindowAttributes.LWA_ALPHA;
			byte opacity = (byte)(transparency*255);
			COLORREF clrRef = new COLORREF();
			if (key != Color.Empty) {
				clrRef.R = key.R;
				clrRef.G = key.G;
				clrRef.B = key.B;
				lwa = (LayeredWindowAttributes)( (int)lwa | (int)LayeredWindowAttributes.LWA_COLORKEY );
			}
			RECT rc;
			rc.right = 1000;
			rc.bottom = 1000;
			Win32SetLayeredWindowAttributes(handle, clrRef, opacity, lwa);
		}

		TransparencySupport support;
		bool queried_transparency_support;
		internal override TransparencySupport SupportsTransparency() {
			if (queried_transparency_support)
				return support;

			bool flag;
			support = TransparencySupport.None;

			flag = true;
			try {
				Win32SetLayeredWindowAttributes (IntPtr.Zero, new COLORREF (), 255, LayeredWindowAttributes.LWA_ALPHA);
			}
			catch (EntryPointNotFoundException) { flag = false; }
			catch { /* swallow everything else */ }

			if (flag) support |= TransparencySupport.Set;

			flag = true;
			try {
				LayeredWindowAttributes lwa;
				COLORREF clrRef;
				byte alpha;

				Win32GetLayeredWindowAttributes (IntPtr.Zero, out clrRef, out alpha, out lwa);
			}
			catch (EntryPointNotFoundException) { flag = false; }
			catch { /* swallow everything else */ }

			if (flag) support |= TransparencySupport.Get;

			queried_transparency_support = true;
			return support;
		}

		internal override void UpdateWindow(IntPtr handle) {
			Win32UpdateWindow(handle);
		}

		internal override PaintEventArgs PaintEventStart(ref Message msg, IntPtr handle, bool client) {
			IntPtr		hdc;
			PAINTSTRUCT	ps;
			PaintEventArgs	paint_event;
			RECT		rect;
			Rectangle	clip_rect;
			Hwnd		hwnd;

			clip_rect = new Rectangle();
			rect = new RECT();
			ps = new PAINTSTRUCT();

			hwnd = Hwnd.ObjectFromHandle(msg.HWnd);
			
			if (client) {
				if (Win32GetUpdateRect(msg.HWnd, ref rect, false)) {
					if (handle != msg.HWnd) {
						// We need to validate the window where the paint message
						// was generated, otherwise we'll never stop getting paint 
						// messages.
						Win32GetClientRect (msg.HWnd, out rect);
						Win32ValidateRect (msg.HWnd, ref rect);
						hdc = Win32GetDC (handle);
					} else {
						hdc = Win32BeginPaint (handle, ref ps);
						rect = ps.rcPaint;
					}
				} else {
					hdc = Win32GetDC(handle);
				}
				clip_rect = rect.ToRectangle ();
			} else {
				hdc = Win32GetWindowDC (handle);

				// HACK this in for now
				Win32GetWindowRect (handle, out rect);
				clip_rect = new Rectangle (0, 0, rect.Width, rect.Height);
			}

			// If we called BeginPaint, store the PAINTSTRUCT,
			// otherwise store hdc, so that PaintEventEnd can know
			// whether to call EndPaint or ReleaseDC.
			if (ps.hdc != IntPtr.Zero) {
				hwnd.drawing_stack.Push (ps);
			} else {
				hwnd.drawing_stack.Push (hdc);
			}
			
			Graphics dc = Graphics.FromHdc(hdc);
			hwnd.drawing_stack.Push (dc);

			paint_event = new PaintEventArgs(dc, clip_rect);

			return paint_event;
		}

		internal override void PaintEventEnd(ref Message m, IntPtr handle, bool client) {
			Hwnd		hwnd;

			hwnd = Hwnd.ObjectFromHandle(m.HWnd);

			Graphics dc = (Graphics)hwnd.drawing_stack.Pop();
			dc.Dispose ();

			object o = hwnd.drawing_stack.Pop();
			if (o is IntPtr) {
				IntPtr hdc = (IntPtr) o;
				Win32ReleaseDC (handle, hdc);
			} else if (o is PAINTSTRUCT) {
				PAINTSTRUCT ps = (PAINTSTRUCT) o;
				Win32EndPaint (handle, ref ps);
			}
		}


		internal override void SetWindowPos(IntPtr handle, int x, int y, int width, int height) {
			Win32MoveWindow(handle, x, y, width, height, true);
			return;
		}

		internal override void GetWindowPos(IntPtr handle, bool is_toplevel, out int x, out int y, out int width, out int height, out int client_width, out int client_height) {
			IntPtr	parent;
			RECT	rect;
			POINT	pt;

			Win32GetWindowRect(handle, out rect);
			width = rect.right - rect.left;
			height = rect.bottom - rect.top;

			pt.x=rect.left;
			pt.y=rect.top;

			parent = Win32GetAncestor (handle, AncestorType.GA_PARENT);
			if (parent != IntPtr.Zero && parent != Win32GetDesktopWindow ())
				Win32ScreenToClient(parent, ref pt);

			x = pt.x;
			y = pt.y;

			Win32GetClientRect(handle, out rect);
			client_width = rect.right - rect.left;
			client_height = rect.bottom - rect.top;
			return;
		}

		internal override void Activate(IntPtr handle) {
			Win32SetActiveWindow(handle);
			// delayed timer enabled
			lock (timer_list) {
				foreach (Timer t in timer_list.Values) {
					if (t.Enabled && t.window == IntPtr.Zero) {
						t.window = handle;
						int id = t.GetHashCode ();
						Win32SetTimer(handle, id, (uint)t.Interval, IntPtr.Zero);
					}
				}
			}
		}

		internal override void Invalidate(IntPtr handle, Rectangle rc, bool clear) {
			RECT rect;

			rect.left=rc.Left;
			rect.top=rc.Top;
			rect.right=rc.Right;
			rect.bottom=rc.Bottom;
			Win32InvalidateRect(handle, ref rect, clear);
		}


		internal override void InvalidateNC (IntPtr handle)
		{
			// found this gem at
			// http://www.dotnet247.com/247reference/msgs/58/292037.aspx
			Win32SetWindowPos(handle, IntPtr.Zero,
					  0, 0, 0, 0,
					  SetWindowPosFlags.SWP_NOMOVE |
					  SetWindowPosFlags.SWP_NOSIZE |
					  SetWindowPosFlags.SWP_NOZORDER |
					  SetWindowPosFlags.SWP_NOACTIVATE |
					  SetWindowPosFlags.SWP_DRAWFRAME);
		}

		private IntPtr InternalWndProc (IntPtr hWnd, Msg msg, IntPtr wParam, IntPtr lParam)
		{
			if (HwndCreating != null && HwndCreating.ClientWindow == IntPtr.Zero)
				HwndCreating.ClientWindow = hWnd;
			return NativeWindow.WndProc (hWnd, msg, wParam, lParam);
		}

		internal override IntPtr DefWndProc(ref Message msg) {
			msg.Result=Win32DefWindowProc(msg.HWnd, (Msg)msg.Msg, msg.WParam, msg.LParam);
			return msg.Result;
		}

		internal override void HandleException(Exception e) {
			StackTrace st = new StackTrace(e);
			Win32MessageBox(IntPtr.Zero, e.Message+st.ToString(), "Exception", 0);
			Console.WriteLine("{0}{1}", e.Message, st.ToString());
		}

		internal override void DoEvents() {
			MSG msg = new MSG();

			while (GetMessage(ref msg, IntPtr.Zero, 0, 0, false)) {
				Message m = Message.Create (msg.hwnd, (int)msg.message, msg.wParam, msg.lParam);

				if (Application.FilterMessage (ref m))
					continue;

				XplatUI.TranslateMessage(ref msg);
				XplatUI.DispatchMessage(ref msg);
			}
		}

		internal override bool PeekMessage(Object queue_id, ref MSG msg, IntPtr hWnd, int wFilterMin, int wFilterMax, uint flags) {
			return Win32PeekMessage(ref msg, hWnd, wFilterMin, wFilterMax, flags);
		}

		internal override void PostQuitMessage(int exitCode) {
			Win32PostQuitMessage(exitCode);
		}

		internal override void RequestAdditionalWM_NCMessages(IntPtr hwnd, bool hover, bool leave)
		{
			if (wm_nc_registered == null)
				wm_nc_registered = new Hashtable ();
				
			TMEFlags flags = TMEFlags.TME_NONCLIENT;
			if (hover)
				flags |= TMEFlags.TME_HOVER;
			if (leave)
				flags |= TMEFlags.TME_LEAVE;

			if (flags == TMEFlags.TME_NONCLIENT) {
				if (wm_nc_registered.Contains (hwnd)) {
					wm_nc_registered.Remove (hwnd);
				}
			} else {
				if (!wm_nc_registered.Contains (hwnd)) {
					wm_nc_registered.Add (hwnd, flags);
				} else {
					wm_nc_registered [hwnd] = flags;
				}
			}
		}

		internal override void RequestNCRecalc(IntPtr handle) {
			Win32SetWindowPos(handle, IntPtr.Zero, 0, 0, 0, 0, SetWindowPosFlags.SWP_FRAMECHANGED | SetWindowPosFlags.SWP_NOOWNERZORDER | SetWindowPosFlags.SWP_NOSIZE | SetWindowPosFlags.SWP_NOMOVE | SetWindowPosFlags.SWP_NOZORDER | SetWindowPosFlags.SWP_NOACTIVATE);
		}

		internal override void ResetMouseHover(IntPtr handle) {
			TRACKMOUSEEVENT	tme;

			tme = new TRACKMOUSEEVENT();
			tme.size = Marshal.SizeOf(tme);
			tme.hWnd = handle;
			tme.dwFlags = TMEFlags.TME_LEAVE | TMEFlags.TME_HOVER;
			Win32TrackMouseEvent(ref tme);
		}


		internal override bool GetMessage(Object queue_id, ref MSG msg, IntPtr hWnd, int wFilterMin, int wFilterMax) {
			return GetMessage(ref msg, hWnd, wFilterMin, wFilterMax, true);
		}

		private bool GetMessage(ref MSG msg, IntPtr hWnd, int wFilterMin, int wFilterMax, bool blocking) {
			bool		result;

			msg.refobject = 0;
			if (RetrieveMessage(ref msg)) {
				return true;
			}

			if (blocking) {
				result = Win32GetMessage(ref msg, hWnd, wFilterMin, wFilterMax);
			} else {
				result = Win32PeekMessage(ref msg, hWnd, wFilterMin, wFilterMax, (uint)PeekMessageFlags.PM_REMOVE);
				if (!result) {
					return false;
				}
			}

			// We need to fake WM_MOUSE_ENTER
			switch (msg.message) {
				case Msg.WM_LBUTTONDOWN: {
					mouse_state |= MouseButtons.Left;
					break;
				}

				case Msg.WM_MBUTTONDOWN: {
					mouse_state |= MouseButtons.Middle;
					break;
				}

				case Msg.WM_RBUTTONDOWN: {
					mouse_state |= MouseButtons.Right;
					break;
				}

				case Msg.WM_LBUTTONUP: {
					mouse_state &= ~MouseButtons.Left;
					break;
				}

				case Msg.WM_MBUTTONUP: {
					mouse_state &= ~MouseButtons.Middle;
					break;
				}

				case Msg.WM_RBUTTONUP: {
					mouse_state &= ~MouseButtons.Right;
					break;
				}

				case Msg.WM_ASYNC_MESSAGE: {
					XplatUIDriverSupport.ExecuteClientMessage((GCHandle)msg.lParam);
					break;
				}

				case Msg.WM_MOUSEMOVE: {
					if (msg.hwnd != prev_mouse_hwnd) {
						TRACKMOUSEEVENT	tme;

						mouse_state = Control.FromParamToMouseButtons ((int)msg.lParam.ToInt32());

						// The current message will be sent out next time around
						StoreMessage(ref msg);

						// This is the message we want to send at this point
						msg.message = Msg.WM_MOUSE_ENTER;

						prev_mouse_hwnd = msg.hwnd;

						tme = new TRACKMOUSEEVENT();
						tme.size = Marshal.SizeOf(tme);
						tme.hWnd = msg.hwnd;
						tme.dwFlags = TMEFlags.TME_LEAVE | TMEFlags.TME_HOVER;
						Win32TrackMouseEvent(ref tme);
						return result;
					}
					break;
				}

				case Msg.WM_NCMOUSEMOVE: {
					if (wm_nc_registered == null || !wm_nc_registered.Contains (msg.hwnd))
						break;

					mouse_state = Control.FromParamToMouseButtons ((int)msg.lParam.ToInt32 ());

					TRACKMOUSEEVENT tme;

					tme = new TRACKMOUSEEVENT ();
					tme.size = Marshal.SizeOf(tme);
					tme.hWnd = msg.hwnd;
					tme.dwFlags = (TMEFlags)wm_nc_registered[msg.hwnd];
					Win32TrackMouseEvent (ref tme);
					return result;
				}

				case Msg.WM_DROPFILES: {
					return Win32DnD.HandleWMDropFiles(ref msg);
				}

				case Msg.WM_MOUSELEAVE: {
					prev_mouse_hwnd = IntPtr.Zero;
					break;
				}

				case Msg.WM_TIMER: {
					Timer timer=(Timer)timer_list[(int)msg.wParam];

					if (timer != null) {
						timer.FireTick();
					}
					break;
				}
			}

			return result;
		}

		internal override bool TranslateMessage(ref MSG msg) {
			return Win32TranslateMessage(ref msg);
		}

		internal override IntPtr DispatchMessage(ref MSG msg) {
			return Win32DispatchMessage(ref msg);
		}

		internal override bool SetZOrder(IntPtr hWnd, IntPtr AfterhWnd, bool Top, bool Bottom) {
			if (Top) {
				Win32SetWindowPos(hWnd, SetWindowPosZOrder.HWND_TOP, 0, 0, 0, 0, SetWindowPosFlags.SWP_NOMOVE | SetWindowPosFlags.SWP_NOSIZE);
				return true;
			} else if (!Bottom) {
				Win32SetWindowPos(hWnd, AfterhWnd, 0, 0, 0, 0, SetWindowPosFlags.SWP_NOMOVE | SetWindowPosFlags.SWP_NOSIZE);
			} else {
				Win32SetWindowPos(hWnd, (IntPtr)SetWindowPosZOrder.HWND_BOTTOM, 0, 0, 0, 0, SetWindowPosFlags.SWP_NOMOVE | SetWindowPosFlags.SWP_NOSIZE);
				return true;
			}
			return false;
		}

		internal override bool SetTopmost(IntPtr hWnd, bool Enabled) {
			if (Enabled) {
				Win32SetWindowPos(hWnd, SetWindowPosZOrder.HWND_TOPMOST, 0, 0, 0, 0, SetWindowPosFlags.SWP_NOMOVE | SetWindowPosFlags.SWP_NOSIZE | SetWindowPosFlags.SWP_NOACTIVATE);
				return true;
			} else {
				Win32SetWindowPos(hWnd, SetWindowPosZOrder.HWND_NOTOPMOST, 0, 0, 0, 0, SetWindowPosFlags.SWP_NOMOVE | SetWindowPosFlags.SWP_NOSIZE | SetWindowPosFlags.SWP_NOACTIVATE);
				return true;
			}
		}
		
		internal override bool SetOwner(IntPtr hWnd, IntPtr hWndOwner) {
			Win32SetWindowLong(hWnd, WindowLong.GWL_HWNDPARENT, (uint) hWndOwner);
			return true;
		}

		internal override bool Text(IntPtr handle, string text) {
			Win32SetWindowText(handle, text);
			return true;
		}

		internal override bool GetText(IntPtr handle, out string text) {
			StringBuilder sb;

			sb = new StringBuilder(256);
			Win32GetWindowText(handle, sb, sb.Capacity);
			text = sb.ToString();
			return true;
		}

		internal override bool SetVisible (IntPtr handle, bool visible, bool activate)
		{
			if (visible) {
				Control c = Control.FromHandle (handle);
				if (c is Form) {
					Form f;

					f = (Form)Control.FromHandle (handle);
					WindowPlacementFlags flags = WindowPlacementFlags.SW_SHOWNORMAL;
					switch (f.WindowState) {
						case FormWindowState.Normal: flags = WindowPlacementFlags.SW_SHOWNORMAL; break;
						case FormWindowState.Minimized: flags = WindowPlacementFlags.SW_MINIMIZE; break;
						case FormWindowState.Maximized: flags = WindowPlacementFlags.SW_MAXIMIZE; break;
					}
					
					if (!f.ActivateOnShow)
						flags = WindowPlacementFlags.SW_SHOWNOACTIVATE;
						
					Win32ShowWindow (handle, flags);
				}
				else {
					if (c.ActivateOnShow)
						Win32ShowWindow (handle, WindowPlacementFlags.SW_SHOWNORMAL);
					else
						Win32ShowWindow (handle, WindowPlacementFlags.SW_SHOWNOACTIVATE);
				}
			}
			else {
				Win32ShowWindow (handle, WindowPlacementFlags.SW_HIDE);
			}
			return true;
		}

		internal override bool IsEnabled(IntPtr handle) {
			return IsWindowEnabled (handle);
		}

		internal override bool IsKeyLocked (VirtualKeys key)
		{
			return (Win32GetKeyState (key) & 1) == 1;
		}
		
		internal override bool IsVisible(IntPtr handle) {
			return IsWindowVisible (handle);
		}

		internal override IntPtr SetParent(IntPtr handle, IntPtr parent) {
			Control c = Control.FromHandle (handle);
			if (parent == IntPtr.Zero) {
				if (!(c is Form)) {
					Win32ShowWindow(handle, WindowPlacementFlags.SW_HIDE);
				}
			} else {
				if (!(c is Form)) {
					SetVisible (handle, c.is_visible, true);
				}
			}	
			// The Win32SetParent is lame, it can very well move the window
			// ref: http://groups.google.com/group/microsoft.public.vb.winapi/browse_thread/thread/1b82ccc54231ecee/afa82835bfc0422a%23afa82835bfc0422a
			// Here we save the position before changing the parent, and if it has changed afterwards restore it.
			// Another possibility would be to intercept WM_WINDOWPOSCHANGING and restore the coords there, but this would require plumbing in weird places
			// (either inside Control or add handling to InternalWndProc)
			// We also need to remove WS_CHILD if making the window parent-less, and add it if we're parenting it.
			RECT rect, rect2;
			IntPtr result;
			WindowStyles style, new_style;
			
			Win32GetWindowRect (handle, out rect);
			style = (WindowStyles) Win32GetWindowLong (handle, WindowLong.GWL_STYLE);
			
			if (parent == IntPtr.Zero) {
				new_style = style & ~WindowStyles.WS_CHILD;
				result = Win32SetParent (handle, FosterParent);
			} else {
				new_style = style | WindowStyles.WS_CHILD;
				result = Win32SetParent (handle, parent);
			}
			if (style != new_style && c is Form) {
				Win32SetWindowLong (handle, WindowLong.GWL_STYLE, (uint) new_style);
			}
			Win32GetWindowRect (handle, out rect2);
			if (rect.top != rect2.top && rect.left != rect2.left && c is Form) {
				Win32SetWindowPos (handle, IntPtr.Zero, rect.top, rect.left, rect.Width, rect.Height, SetWindowPosFlags.SWP_NOZORDER |  SetWindowPosFlags.SWP_NOREDRAW | SetWindowPosFlags.SWP_NOOWNERZORDER | SetWindowPosFlags.SWP_NOENDSCHANGING | SetWindowPosFlags.SWP_NOACTIVATE);
			}
			return result;
		}

		// If we ever start using this, we should probably replace FosterParent with IntPtr.Zero
		internal override IntPtr GetParent(IntPtr handle) {
			return Win32GetParent(handle);
		}

		// This is a nop on win32 and x11
		internal override IntPtr GetPreviousWindow(IntPtr handle) {
			return handle;
		}

		internal override void GrabWindow(IntPtr hWnd, IntPtr ConfineToHwnd) {
			grab_hwnd = hWnd;
			Win32SetCapture(hWnd);
			
			if (ConfineToHwnd != IntPtr.Zero) {
				RECT window_rect;
				Win32GetWindowRect (ConfineToHwnd, out window_rect);
				Win32GetClipCursor (out clipped_cursor_rect);
				Win32ClipCursor (ref window_rect);
			}
		}

		internal override void GrabInfo(out IntPtr hWnd, out bool GrabConfined, out Rectangle GrabArea) {
			hWnd = grab_hwnd;
			GrabConfined = grab_confined;
			GrabArea = grab_area;
		}

		internal override void UngrabWindow(IntPtr hWnd) {
			if (!(clipped_cursor_rect.top == 0 && clipped_cursor_rect.bottom == 0 && clipped_cursor_rect.left == 0 && clipped_cursor_rect.right == 0)) {
				Win32ClipCursor (ref clipped_cursor_rect);
				clipped_cursor_rect = new RECT ();
			}
			
			Win32ReleaseCapture();
			grab_hwnd = IntPtr.Zero;
		}

		internal override bool CalculateWindowRect(ref Rectangle ClientRect, CreateParams cp, Menu menu, out Rectangle WindowRect) {
			RECT	rect;

			rect.left=ClientRect.Left;
			rect.top=ClientRect.Top;
			rect.right=ClientRect.Right;
			rect.bottom=ClientRect.Bottom;

			if (!Win32AdjustWindowRectEx(ref rect, cp.Style, menu != null, cp.ExStyle)) {
				WindowRect = new Rectangle(ClientRect.Left, ClientRect.Top, ClientRect.Width, ClientRect.Height);
				return false;
			}

			WindowRect = new Rectangle(rect.left, rect.top, rect.right-rect.left, rect.bottom-rect.top);
			return true;
		}

		internal override void SetCursor(IntPtr window, IntPtr cursor) {
			Win32SetCursor(cursor);
			return;
		}

		internal override void ShowCursor(bool show) {
			Win32ShowCursor(show);
		}

		internal override void OverrideCursor(IntPtr cursor) {
			Win32SetCursor(cursor);
		}

		internal override IntPtr DefineCursor(Bitmap bitmap, Bitmap mask, Color cursor_pixel, Color mask_pixel, int xHotSpot, int yHotSpot) {
			IntPtr	cursor;
			Bitmap	cursor_bitmap;
			Bitmap	cursor_mask;
			Byte[]	cursor_bits;
			Byte[]	mask_bits;
			Color	pixel;
			int	width;
			int	height;

			// Win32 only allows creation cursors of a certain size
			if ((bitmap.Width != Win32GetSystemMetrics(SystemMetrics.SM_CXCURSOR)) || (bitmap.Width != Win32GetSystemMetrics(SystemMetrics.SM_CXCURSOR))) {
				cursor_bitmap = new Bitmap(bitmap, new Size(Win32GetSystemMetrics(SystemMetrics.SM_CXCURSOR), Win32GetSystemMetrics(SystemMetrics.SM_CXCURSOR)));
				cursor_mask = new Bitmap(mask, new Size(Win32GetSystemMetrics(SystemMetrics.SM_CXCURSOR), Win32GetSystemMetrics(SystemMetrics.SM_CXCURSOR)));
			} else {
				cursor_bitmap = bitmap;
				cursor_mask = mask;
			}

			width = cursor_bitmap.Width;
			height = cursor_bitmap.Height;

			cursor_bits = new Byte[(width / 8) * height];
			mask_bits = new Byte[(width / 8) * height];

			for (int y = 0; y < height; y++) {
				for (int x = 0; x < width; x++) {
					pixel = cursor_bitmap.GetPixel(x, y);

					if (pixel == cursor_pixel) {
						cursor_bits[y * width / 8 + x / 8] |= (byte)(0x80 >> (x % 8));
					}

					pixel = cursor_mask.GetPixel(x, y);

					if (pixel == mask_pixel) {
						mask_bits[y * width / 8 + x / 8] |= (byte)(0x80 >> (x % 8));
					}
				}
			}

			cursor = Win32CreateCursor(IntPtr.Zero, xHotSpot, yHotSpot, width, height, mask_bits, cursor_bits);

			return cursor;
		}

		internal override Bitmap DefineStdCursorBitmap (StdCursor id)
		{
			// We load the cursor, create a bitmap, draw the cursor onto the bitmap and return the bitmap.
			IntPtr cursor = DefineStdCursor (id);
			// Windows only have one possible cursor size!
			int width = Win32GetSystemMetrics (SystemMetrics.SM_CXCURSOR);
			int height = Win32GetSystemMetrics (SystemMetrics.SM_CYCURSOR);
			Bitmap bmp = new Bitmap (width, height);
			Graphics gc = Graphics.FromImage (bmp);
			IntPtr hdc = gc.GetHdc ();
			Win32DrawIcon (hdc, 0, 0, cursor);
			gc.ReleaseHdc (hdc);
			gc.Dispose ();
			return bmp;
		}

		[MonoTODO("Define the missing cursors")]
		internal override IntPtr DefineStdCursor(StdCursor id) {
			switch(id) {
				case StdCursor.AppStarting:	return Win32LoadCursor(IntPtr.Zero, LoadCursorType.IDC_APPSTARTING);
				case StdCursor.Arrow:		return Win32LoadCursor(IntPtr.Zero, LoadCursorType.IDC_ARROW);
				case StdCursor.Cross:		return Win32LoadCursor(IntPtr.Zero, LoadCursorType.IDC_CROSS);
				case StdCursor.Default:		return Win32LoadCursor(IntPtr.Zero, LoadCursorType.IDC_ARROW);
				case StdCursor.Hand:		return Win32LoadCursor(IntPtr.Zero, LoadCursorType.IDC_HAND);
				case StdCursor.Help:		return Win32LoadCursor(IntPtr.Zero, LoadCursorType.IDC_HELP);
				case StdCursor.HSplit:		return Win32LoadCursor(IntPtr.Zero, LoadCursorType.IDC_ARROW);		// FIXME
				case StdCursor.IBeam:		return Win32LoadCursor(IntPtr.Zero, LoadCursorType.IDC_IBEAM);
				case StdCursor.No:		return Win32LoadCursor(IntPtr.Zero, LoadCursorType.IDC_NO);
				case StdCursor.NoMove2D:	return Win32LoadCursor(IntPtr.Zero, LoadCursorType.IDC_ARROW);		// FIXME
				case StdCursor.NoMoveHoriz:	return Win32LoadCursor(IntPtr.Zero, LoadCursorType.IDC_ARROW);		// FIXME
				case StdCursor.NoMoveVert:	return Win32LoadCursor(IntPtr.Zero, LoadCursorType.IDC_ARROW);		// FIXME
				case StdCursor.PanEast:		return Win32LoadCursor(IntPtr.Zero, LoadCursorType.IDC_ARROW);		// FIXME
				case StdCursor.PanNE:		return Win32LoadCursor(IntPtr.Zero, LoadCursorType.IDC_ARROW);		// FIXME
				case StdCursor.PanNorth:	return Win32LoadCursor(IntPtr.Zero, LoadCursorType.IDC_ARROW);		// FIXME
				case StdCursor.PanNW:		return Win32LoadCursor(IntPtr.Zero, LoadCursorType.IDC_ARROW);		// FIXME
				case StdCursor.PanSE:		return Win32LoadCursor(IntPtr.Zero, LoadCursorType.IDC_ARROW);		// FIXME
				case StdCursor.PanSouth:	return Win32LoadCursor(IntPtr.Zero, LoadCursorType.IDC_ARROW);		// FIXME
				case StdCursor.PanSW:		return Win32LoadCursor(IntPtr.Zero, LoadCursorType.IDC_ARROW);		// FIXME
				case StdCursor.PanWest:		return Win32LoadCursor(IntPtr.Zero, LoadCursorType.IDC_ARROW);		// FIXME
				case StdCursor.SizeAll:		return Win32LoadCursor(IntPtr.Zero, LoadCursorType.IDC_SIZEALL);
				case StdCursor.SizeNESW:	return Win32LoadCursor(IntPtr.Zero, LoadCursorType.IDC_SIZENESW);
				case StdCursor.SizeNS:		return Win32LoadCursor(IntPtr.Zero, LoadCursorType.IDC_SIZENS);
				case StdCursor.SizeNWSE:	return Win32LoadCursor(IntPtr.Zero, LoadCursorType.IDC_SIZENWSE);
				case StdCursor.SizeWE:		return Win32LoadCursor(IntPtr.Zero, LoadCursorType.IDC_SIZEWE);
				case StdCursor.UpArrow:		return Win32LoadCursor(IntPtr.Zero, LoadCursorType.IDC_UPARROW);
				case StdCursor.VSplit:		return Win32LoadCursor(IntPtr.Zero, LoadCursorType.IDC_ARROW);		// FIXME
				case StdCursor.WaitCursor:	return Win32LoadCursor(IntPtr.Zero, LoadCursorType.IDC_WAIT);
			}
			throw new NotImplementedException ();
		}

		internal override void DestroyCursor(IntPtr cursor) {
			if ((cursor.ToInt32() < (int)LoadCursorType.First) || (cursor.ToInt32() > (int)LoadCursorType.Last)) {
				Win32DestroyCursor(cursor);
			}
		}

		[MonoTODO]
		internal override void GetCursorInfo(IntPtr cursor, out int width, out int height, out int hotspot_x, out int hotspot_y) {
			ICONINFO ii = new ICONINFO ();
			
			if (!Win32GetIconInfo (cursor, out ii))
				throw new Win32Exception ();
				
			width = 20;
			height = 20;
			hotspot_x = ii.xHotspot;
			hotspot_y = ii.yHotspot;
		}

		internal override void SetCursorPos(IntPtr handle, int x, int y) {
			Win32SetCursorPos(x, y);
		}

		internal override Region GetClipRegion(IntPtr hwnd) {
			Region region;

			region = new Region();

			Win32GetWindowRgn(hwnd, region.GetHrgn(Graphics.FromHwnd(hwnd)));

			return region;
		}

		internal override void SetClipRegion(IntPtr hwnd, Region region) {
			if (region == null)
				Win32SetWindowRgn (hwnd, IntPtr.Zero, true);
			else
				Win32SetWindowRgn(hwnd, region.GetHrgn(Graphics.FromHwnd(hwnd)), true);
		}

		internal override void EnableWindow(IntPtr handle, bool Enable) {
			Win32EnableWindow(handle, Enable);
		}

		internal override void EndLoop(System.Threading.Thread thread) {
			// Nothing to do
		}

		internal override object StartLoop(System.Threading.Thread thread) {
			return null;
		}

		internal override void SetModal(IntPtr handle, bool Modal) {
			// we do nothing on Win32
		}

		internal override void GetCursorPos(IntPtr handle, out int x, out int y) {
			POINT	pt;

			Win32GetCursorPos(out pt);

			if (handle!=IntPtr.Zero) {
				Win32ScreenToClient(handle, ref pt);
			}

			x=pt.x;
			y=pt.y;
		}

		internal override void ScreenToClient(IntPtr handle, ref int x, ref int y)
		{
			POINT pnt = new POINT();			

			pnt.x = x;
			pnt.y = y;
			Win32ScreenToClient (handle, ref pnt);

			x = pnt.x;
			y = pnt.y;
		}

		internal override void ClientToScreen(IntPtr handle, ref int x, ref int y) {			
			POINT	pnt = new POINT();			

			pnt.x = x;
			pnt.y = y;

			Win32ClientToScreen(handle, ref pnt);

			x = pnt.x;
			y = pnt.y;
		}

  		internal override void ScreenToMenu(IntPtr handle, ref int x, ref int y) {
 			RECT	rect;
 
 			Win32GetWindowRect(handle, out rect);
 			x -= rect.left + SystemInformation.FrameBorderSize.Width;
 			y -= rect.top + SystemInformation.FrameBorderSize.Height;

 			WindowStyles style = (WindowStyles) Win32GetWindowLong (handle, WindowLong.GWL_STYLE);
 			if (CreateParams.IsSet (style, WindowStyles.WS_CAPTION)) {
 				y -= ThemeEngine.Current.CaptionHeight;
 			}
  		}
  
  		internal override void MenuToScreen(IntPtr handle, ref int x, ref int y) {			
 			RECT	rect;
 
 			Win32GetWindowRect(handle, out rect);
 			x += rect.left + SystemInformation.FrameBorderSize.Width;
 			y += rect.top + SystemInformation.FrameBorderSize.Height + ThemeEngine.Current.CaptionHeight;
 			return;
  		}

		internal override void SendAsyncMethod (AsyncMethodData method)
		{
			Win32PostMessage(FosterParent, Msg.WM_ASYNC_MESSAGE, IntPtr.Zero, (IntPtr)GCHandle.Alloc (method));
		}

		internal override void SetTimer (Timer timer)
		{
			int	index;

			index = timer.GetHashCode();

			lock (timer_list) {
				timer_list[index]=timer;
			}

			if (Win32SetTimer(FosterParent, index, (uint)timer.Interval, IntPtr.Zero) != IntPtr.Zero)
				timer.window = FosterParent;
			else
				timer.window = IntPtr.Zero;
		}

		internal override void KillTimer (Timer timer)
		{
			int	index;

			index = timer.GetHashCode();

			Win32KillTimer(timer.window, index);

			lock (timer_list) {
				timer_list.Remove(index);
			}
		}
		
		internal override void CreateCaret(IntPtr hwnd, int width, int height) {
			Win32CreateCaret(hwnd, IntPtr.Zero, width, height);
			caret_visible = false;
		}

		internal override void DestroyCaret(IntPtr hwnd) {
			Win32DestroyCaret();
		}

		internal override void SetCaretPos(IntPtr hwnd, int x, int y) {
			Win32SetCaretPos(x, y);
		}

		internal override void CaretVisible(IntPtr hwnd, bool visible) {
			if (visible) {
				if (!caret_visible) {
					Win32ShowCaret(hwnd);
					caret_visible = true;
				}
			} else {
				if (caret_visible) {
					Win32HideCaret(hwnd);
					caret_visible = false;
				}
			}
		}

		internal override IntPtr GetFocus() {
			return Win32GetFocus();
		}

		internal override void SetFocus(IntPtr hwnd) {
			Win32SetFocus(hwnd);
		}

		internal override IntPtr GetActive() {
			return Win32GetActiveWindow();
		}

		internal override bool GetFontMetrics(Graphics g, Font font, out int ascent, out int descent) {
			IntPtr		dc;
			IntPtr		prevobj;
			TEXTMETRIC	tm;

			tm = new TEXTMETRIC();

			dc = Win32GetDC (IntPtr.Zero);
			prevobj = Win32SelectObject (dc, font.ToHfont ());
			
			if (Win32GetTextMetrics (dc, ref tm) == false) {
				prevobj = Win32SelectObject (dc, prevobj);
				Win32DeleteObject (prevobj);
				Win32ReleaseDC (IntPtr.Zero, dc);
				ascent = 0;
				descent = 0;
				return false;
			}
			prevobj = Win32SelectObject (dc, prevobj);
			Win32DeleteObject (prevobj);
			Win32ReleaseDC (IntPtr.Zero, dc);

			ascent = tm.tmAscent;
			descent = tm.tmDescent;

			return true;
		}

		internal override void ScrollWindow(IntPtr hwnd, Rectangle rectangle, int XAmount, int YAmount, bool with_children) {
			RECT	rect;

			rect = new RECT();
			rect.left = rectangle.X;
			rect.top = rectangle.Y;
			rect.right = rectangle.Right;
			rect.bottom = rectangle.Bottom;

			Win32ScrollWindowEx(hwnd, XAmount, YAmount, IntPtr.Zero, ref rect, IntPtr.Zero, IntPtr.Zero, ScrollWindowExFlags.SW_INVALIDATE | ScrollWindowExFlags.SW_ERASE | (with_children ? ScrollWindowExFlags.SW_SCROLLCHILDREN : ScrollWindowExFlags.SW_NONE));
			Win32UpdateWindow(hwnd);
		}

		internal override void ScrollWindow(IntPtr hwnd, int XAmount, int YAmount, bool with_children) {
			Win32ScrollWindowEx(hwnd, XAmount, YAmount, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, ScrollWindowExFlags.SW_INVALIDATE | ScrollWindowExFlags.SW_ERASE | (with_children ? ScrollWindowExFlags.SW_SCROLLCHILDREN : ScrollWindowExFlags.SW_NONE));
		}

		internal override bool SystrayAdd(IntPtr hwnd, string tip, Icon icon, out ToolTip tt) {
			NOTIFYICONDATA	nid;

			nid = new NOTIFYICONDATA();

			nid.cbSize = (uint)Marshal.SizeOf(nid);
			nid.hWnd = hwnd;
			nid.uID = 1;
			nid.uCallbackMessage = (uint)Msg.WM_USER;
			nid.uFlags = NotifyIconFlags.NIF_MESSAGE;

			if (tip != null) {	 
				nid.szTip = tip;
				nid.uFlags |= NotifyIconFlags.NIF_TIP;
			}

			if (icon != null) {
				nid.hIcon = icon.Handle;
				nid.uFlags |= NotifyIconFlags.NIF_ICON;
			}

			tt = null;

			return Win32Shell_NotifyIcon(NotifyIconMessage.NIM_ADD, ref nid);
		}

		internal override bool SystrayChange(IntPtr hwnd, string tip, Icon icon, ref ToolTip tt) {
			NOTIFYICONDATA	nid;

			nid = new NOTIFYICONDATA();

			nid.cbSize = (uint)Marshal.SizeOf(nid);
			nid.hIcon = icon.Handle;
			nid.hWnd = hwnd;
			nid.uID = 1;
			nid.uCallbackMessage = (uint)Msg.WM_USER;
			nid.uFlags = NotifyIconFlags.NIF_MESSAGE;

			if (tip != null) {
				nid.szTip = tip;
				nid.uFlags |= NotifyIconFlags.NIF_TIP;
			}

			if (icon != null) {
				nid.hIcon = icon.Handle;
				nid.uFlags |= NotifyIconFlags.NIF_ICON;
			}

			return Win32Shell_NotifyIcon(NotifyIconMessage.NIM_MODIFY, ref nid);
		}

		internal override void SystrayRemove(IntPtr hwnd, ref ToolTip tt) {
			NOTIFYICONDATA	nid;

			nid = new NOTIFYICONDATA();

			nid.cbSize = (uint)Marshal.SizeOf(nid);
			nid.hWnd = hwnd;
			nid.uID = 1;
			nid.uFlags = 0;

			Win32Shell_NotifyIcon(NotifyIconMessage.NIM_DELETE, ref nid);
		}

#if NET_2_0
		internal override void SystrayBalloon(IntPtr hwnd, int timeout, string title, string text, ToolTipIcon icon)
		{
			NOTIFYICONDATA	nid;

			nid = new NOTIFYICONDATA();

			nid.cbSize = (uint)Marshal.SizeOf(nid);
			nid.hWnd = hwnd;
			nid.uID = 1;
			nid.uFlags = NotifyIconFlags.NIF_INFO;
			nid.uTimeoutOrVersion = timeout;
			nid.szInfoTitle = title;
			nid.szInfo = text;
			nid.dwInfoFlags = icon;
			
			Win32Shell_NotifyIcon(NotifyIconMessage.NIM_MODIFY, ref nid);
		}
#endif

		internal override void SetBorderStyle(IntPtr handle, FormBorderStyle border_style) {
			// Nothing to do on Win32
		}

		internal override void SetMenu(IntPtr handle, Menu menu) {
			// Trigger WM_NCCALC
			Win32SetWindowPos(handle, IntPtr.Zero, 0, 0, 0, 0, SetWindowPosFlags.SWP_FRAMECHANGED | SetWindowPosFlags.SWP_NOMOVE | SetWindowPosFlags.SWP_NOSIZE);
		}

		internal override Point GetMenuOrigin(IntPtr handle) {
			Form form = Control.FromHandle (handle) as Form;
			
			if (form != null) {
				if (form.FormBorderStyle == FormBorderStyle.None)
					return Point.Empty;
					
				int bordersize = (form.Width - form.ClientSize.Width) / 2;
					
				if (form.FormBorderStyle == FormBorderStyle.FixedToolWindow || form.FormBorderStyle == FormBorderStyle.SizableToolWindow)
					return new Point (bordersize, bordersize + SystemInformation.ToolWindowCaptionHeight);
				else
					return new Point (bordersize, bordersize + SystemInformation.CaptionHeight);
			}
			
			return new Point(SystemInformation.FrameBorderSize.Width, SystemInformation.FrameBorderSize.Height + ThemeEngine.Current.CaptionHeight);
		}

		internal override void SetIcon(IntPtr hwnd, Icon icon) {
			Win32SendMessage(hwnd, Msg.WM_SETICON, (IntPtr)1, icon == null ? IntPtr.Zero : icon.Handle);	// 1 = large icon (0 would be small)
		}

		internal override void ClipboardClose(IntPtr handle) {
			if (handle != clip_magic) {
				throw new ArgumentException("handle is not a valid clipboard handle");
			}
			Win32CloseClipboard();
		}

		internal override int ClipboardGetID(IntPtr handle, string format) {
			if (handle != clip_magic) {
				throw new ArgumentException("handle is not a valid clipboard handle");
			}
			if (format == "Text" ) return 1;
			else if (format == "Bitmap" ) return 2;
			else if (format == "MetaFilePict" ) return 3;
			else if (format == "SymbolicLink" ) return 4;
			else if (format == "DataInterchangeFormat" ) return 5;
			else if (format == "Tiff" ) return 6;
			else if (format == "OEMText" ) return 7;
			else if (format == "DeviceIndependentBitmap" ) return 8;
			else if (format == "Palette" ) return 9;
			else if (format == "PenData" ) return 10;
			else if (format == "RiffAudio" ) return 11;
			else if (format == "WaveAudio" ) return 12;
			else if (format == "UnicodeText" ) return 13;
			else if (format == "EnhancedMetafile" ) return 14;
			else if (format == "FileDrop" ) return 15;
			else if (format == "Locale" ) return 16;

			return (int)Win32RegisterClipboardFormat(format);
		}

		internal override IntPtr ClipboardOpen(bool primary_selection) {
			// Win32 does not have primary selection
			Win32OpenClipboard(FosterParent);
			return clip_magic;
		}

		internal override int[] ClipboardAvailableFormats(IntPtr handle) {
			uint	format;
			int[]	result;
			int	count;

			if (handle != clip_magic) {
				return null;
			}

			// Count first
			count = 0;
			format = 0;
			do {
				format = Win32EnumClipboardFormats(format);
				if (format != 0) {
					count++;
				}
			} while (format != 0);

			// Now assign
			result = new int[count];
			count = 0;
			format = 0;
			do {
				format = Win32EnumClipboardFormats(format);
				if (format != 0) {
					result[count++] = (int)format;
				}
			} while (format != 0);

			return result;
		}


		internal override object ClipboardRetrieve(IntPtr handle, int type, XplatUI.ClipboardToObject converter) {
			IntPtr	hmem;
			IntPtr	data;
			object	obj;

			if (handle != clip_magic) {
				throw new ArgumentException("handle is not a valid clipboard handle");
			}

			hmem = Win32GetClipboardData((uint)type);
			if (hmem == IntPtr.Zero) {
				return null;
			}

			data = Win32GlobalLock(hmem);
			if (data == IntPtr.Zero) {
				uint error = Win32GetLastError();
				Console.WriteLine("Error: {0}", error);
				return null;
			}

			obj = null;

			if (type == DataFormats.GetFormat(DataFormats.Rtf).Id) {
				obj = AnsiToString(data);
			} else switch ((ClipboardFormats)type) {
				case ClipboardFormats.CF_TEXT: {
					obj = AnsiToString(data);
					break;
				}

				case ClipboardFormats.CF_DIB: {
					obj = DIBtoImage(data);
					break;
				}

				case ClipboardFormats.CF_UNICODETEXT: {
					obj = UnicodeToString(data);
					break;
				}

				default: {
					if (converter != null && !converter(type, data, out obj)) {
						obj = null;
					}
					break;
				}
			}
			Win32GlobalUnlock(hmem);

			return obj;

		}

		internal override void ClipboardStore(IntPtr handle, object obj, int type, XplatUI.ObjectToClipboard converter) {
			byte[]	data = null;

			if (handle != clip_magic) {
				throw new ArgumentException("handle is not a valid clipboard handle");
			}

			if (obj == null) {
				// Just clear it
				if (!Win32EmptyClipboard())
					throw new ExternalException("Win32EmptyClipboard");
				return;
			}

			if (type == -1) {
				if (obj is string) {
					type = (int)ClipboardFormats.CF_UNICODETEXT;
				} else if (obj is Image) {
					type = (int)ClipboardFormats.CF_DIB;
				}
			}

			if (type == DataFormats.GetFormat(DataFormats.Rtf).Id) {
				data = StringToAnsi ((string)obj);
			} else switch((ClipboardFormats)type) {
				case ClipboardFormats.CF_UNICODETEXT: {
					data = StringToUnicode ((string)obj);
					break;
				}

				case ClipboardFormats.CF_TEXT: {
					data = StringToAnsi ((string)obj);
					break;
				}

				case ClipboardFormats.CF_BITMAP:
				case ClipboardFormats.CF_DIB: {
					data = ImageToDIB ((Image)obj);
					type = (int)ClipboardFormats.CF_DIB;
					break;
				}

				default: {
					if (converter != null && !converter(ref type, obj, out data)) {
						data = null; // ensure that a failed conversion leaves null.
					}
					break;
				}
			}
			if (data != null) {
				SetClipboardData ((uint)type, data);
			}
		}

		internal static byte[] StringToUnicode (string text)
		{
			return Encoding.Unicode.GetBytes (text + "\0");
		}

		internal static byte[] StringToAnsi (string text)
		{
			// FIXME, follow the behaviour of the previous code using UTF-8,
			// but this should be 'ANSI' on Windows, i.e. the current code page.
			// Does Encoding.Default work on Windows?
			return Encoding.UTF8.GetBytes (text + "\0");
		}

		private void SetClipboardData (uint type, byte[] data)
		{
			if (data.Length == 0)
				// Shouldn't call Win32SetClipboard with NULL, as, from MSDN:
				// "This parameter can be NULL, indicating that the window provides data 
				//  in the specified clipboard format (renders the format) upon request."
				// and I don't think we support that...
				// Note this is unrelated to the fact that passing a null obj to 
				// ClipboardStore is actually a request to empty the clipboard!
				return;
			IntPtr hmem = CopyToMoveableMemory (data);
			if (hmem == IntPtr.Zero)
				// As above, should not call with null.
				// (Not that CopyToMoveableMemory should ever return null!)
				throw new ExternalException ("CopyToMoveableMemory failed.");
			if (Win32SetClipboardData (type, hmem) == IntPtr.Zero)
				throw new ExternalException ("Win32SetClipboardData");
		}

		/// <summary>
		/// Creates a memory block with GlobalAlloc(GMEM_MOVEABLE), copies the data 
		/// into it, and returns the handle to the memory.
		/// </summary>
		/// -
		/// <param name="data">The data.  Must not be null or zero-length &#x2014; 
		/// see the exception notes.</param>
		/// -
		/// <returns>The *handle* to the allocated GMEM_MOVEABLE block.</returns>
		/// -
		/// <exception cref="T:System.ArgumentException">The data was null or zero 
		/// length.  This is disallowed since a zero length allocation can't be made
		/// </exception>
		/// <exception cref="T:System.ComponentModel.Win32Exception">The allocation, 
		/// or locking (handle->pointer) failed.
		/// Either out of memory or the handle table is full (256 max currently).
		/// Note Win32Exception is a subclass of ExternalException so this is OK in 
		/// the documented Clipboard interface.
		/// </exception>
		internal static IntPtr CopyToMoveableMemory (byte[] data)
		{
			if (data == null || data.Length == 0)
				// detect this before GlobalAlloc does.
				throw new ArgumentException ("Can't create a zero length memory block.");

			IntPtr hmem = Win32GlobalAlloc (GAllocFlags.GMEM_MOVEABLE | GAllocFlags.GMEM_DDESHARE, data.Length);
			if (hmem == IntPtr.Zero)
				throw new Win32Exception ();
			IntPtr hmem_ptr = Win32GlobalLock (hmem);
			if (hmem_ptr == IntPtr.Zero) // If the allocation was valid this shouldn't occur.
				throw new Win32Exception ();
			Marshal.Copy (data, 0, hmem_ptr, data.Length);
			Win32GlobalUnlock (hmem);
			return hmem;
		}


		internal override void SetAllowDrop(IntPtr hwnd, bool allowed) {
			if (allowed) {
				Win32DnD.RegisterDropTarget(hwnd);
			} else {
				Win32DnD.UnregisterDropTarget(hwnd);
			}
		}

		internal override DragDropEffects StartDrag(IntPtr hwnd, object data, DragDropEffects allowedEffects) {
			return Win32DnD.StartDrag(hwnd, data, allowedEffects);
		}

		// XXX this doesn't work at all for FrameStyle.Dashed - it draws like Thick, and in the Thick case
		// the corners are drawn incorrectly.
		internal override void DrawReversibleFrame (Rectangle rectangle, Color backColor, FrameStyle style) {
			IntPtr		hdc;
			IntPtr		pen;
			IntPtr		oldpen;
			COLORREF        clrRef = new COLORREF();

			// If we want the standard hatch pattern we would
			// need to create a brush

			clrRef.R = backColor.R;
			clrRef.G = backColor.G;
			clrRef.B = backColor.B;

			// Grab a pen
			pen = Win32CreatePen (style == FrameStyle.Thick ? PenStyle.PS_SOLID : PenStyle.PS_DASH,
					      style == FrameStyle.Thick ? 4 : 2, ref clrRef);

			hdc = Win32GetDC(IntPtr.Zero);
			Win32SetROP2(hdc, ROP2DrawMode.R2_NOT);
			oldpen = Win32SelectObject(hdc, pen);

			Win32MoveToEx(hdc, rectangle.Left, rectangle.Top, IntPtr.Zero);
			if ((rectangle.Width > 0) && (rectangle.Height > 0)) {
				Win32LineTo(hdc, rectangle.Right, rectangle.Top);
				Win32LineTo(hdc, rectangle.Right, rectangle.Bottom);
				Win32LineTo(hdc, rectangle.Left, rectangle.Bottom);
				Win32LineTo(hdc, rectangle.Left, rectangle.Top);
			} else {
				if (rectangle.Width > 0) {
					Win32LineTo(hdc, rectangle.Right, rectangle.Top);
				} else {
					Win32LineTo(hdc, rectangle.Left, rectangle.Bottom);
				}
			}

			Win32SelectObject(hdc, oldpen);
			Win32DeleteObject(pen);

			Win32ReleaseDC(IntPtr.Zero, hdc);
		}

		internal override void DrawReversibleLine(Point start, Point end, Color backColor) {
			IntPtr		hdc;
			IntPtr		pen;
			IntPtr		oldpen;
			POINT		pt;
			COLORREF        clrRef = new COLORREF();

			pt = new POINT();
			pt.x = 0;
			pt.y = 0;
			Win32ClientToScreen(IntPtr.Zero, ref pt);

			// If we want the standard hatch pattern we would
			// need to create a brush

			clrRef.R = backColor.R;
			clrRef.G = backColor.G;
			clrRef.B = backColor.B;

			// Grab a pen
			pen = Win32CreatePen(PenStyle.PS_SOLID, 1, ref clrRef);

			hdc = Win32GetDC(IntPtr.Zero);
			Win32SetROP2(hdc, ROP2DrawMode.R2_NOT);
			oldpen = Win32SelectObject(hdc, pen);

			Win32MoveToEx(hdc, pt.x + start.X, pt.y + start.Y, IntPtr.Zero);
			Win32LineTo(hdc, pt.x + end.X, pt.y + end.Y);

			Win32SelectObject(hdc, oldpen);
			Win32DeleteObject(pen);

			Win32ReleaseDC(IntPtr.Zero, hdc);
		}

		internal override void FillReversibleRectangle (Rectangle rectangle, Color backColor)
		{
			RECT	rect;

			rect = new RECT();
			rect.left = rectangle.Left;
			rect.top = rectangle.Top;
			rect.right = rectangle.Right;
			rect.bottom = rectangle.Bottom;

			IntPtr		hdc;
			IntPtr		brush;
			IntPtr		oldbrush;
			COLORREF        clrRef = new COLORREF();

			clrRef.R = backColor.R;
			clrRef.G = backColor.G;
			clrRef.B = backColor.B;

			// Grab a brush
			brush = Win32CreateSolidBrush (clrRef);

			hdc = Win32GetDC(IntPtr.Zero);
			oldbrush = Win32SelectObject(hdc, brush);

			Win32PatBlt (hdc, rectangle.Left, rectangle.Top, rectangle.Width, rectangle.Height, PatBltRop.DSTINVERT);

			Win32SelectObject(hdc, oldbrush);
			Win32DeleteObject(brush);

			Win32ReleaseDC(IntPtr.Zero, hdc);
		}

		internal override void DrawReversibleRectangle(IntPtr handle, Rectangle rect, int line_width) {
			IntPtr		hdc;
			IntPtr		pen;
			IntPtr		oldpen;
			POINT		pt;

			pt = new POINT();
			pt.x = 0;
			pt.y = 0;
			Win32ClientToScreen(handle, ref pt);

			// If we want the standard hatch pattern we would
			// need to create a brush

			// Grab a pen
			pen = Win32CreatePen(PenStyle.PS_SOLID, line_width, IntPtr.Zero);

			hdc = Win32GetDC(IntPtr.Zero);
			Win32SetROP2(hdc, ROP2DrawMode.R2_NOT);
			oldpen = Win32SelectObject(hdc, pen);

			Control c = Control.FromHandle (handle);
			if (c != null) {
				RECT window_rect;
				Win32GetWindowRect (c.Handle, out window_rect);
				Region r = new Region (new Rectangle(window_rect.left, window_rect.top, window_rect.right - window_rect.left, window_rect.bottom - window_rect.top));
				Win32ExtSelectClipRgn(hdc, r.GetHrgn (Graphics.FromHdc (hdc)), (int) ClipCombineMode.RGN_AND);
			}

			Win32MoveToEx(hdc, pt.x + rect.Left, pt.y + rect.Top, IntPtr.Zero);
			if ((rect.Width > 0) && (rect.Height > 0)) {
				Win32LineTo(hdc, pt.x + rect.Right, pt.y + rect.Top);
				Win32LineTo(hdc, pt.x + rect.Right, pt.y + rect.Bottom);
				Win32LineTo(hdc, pt.x + rect.Left, pt.y + rect.Bottom);
				Win32LineTo(hdc, pt.x + rect.Left, pt.y + rect.Top);
			} else {
				if (rect.Width > 0) {
					Win32LineTo(hdc, pt.x + rect.Right, pt.y + rect.Top);
				} else {
					Win32LineTo(hdc, pt.x + rect.Left, pt.y + rect.Bottom);
				}
			}

			Win32SelectObject(hdc, oldpen);
			Win32DeleteObject(pen);
			if (c != null)
				Win32ExtSelectClipRgn(hdc, IntPtr.Zero, (int) ClipCombineMode.RGN_COPY);

			Win32ReleaseDC(IntPtr.Zero, hdc);
		}

		internal override SizeF GetAutoScaleSize(Font font) {
			Graphics	g;
			float		width;
			string		magic_string = "The quick brown fox jumped over the lazy dog.";
			double		magic_number = 44.549996948242189;

			g = Graphics.FromHwnd(FosterParent);

			width = (float) (g.MeasureString (magic_string, font).Width / magic_number);
			return new SizeF(width, font.Height);
		}

		internal override IntPtr SendMessage (IntPtr hwnd, Msg message, IntPtr wParam, IntPtr lParam) {
			return Win32SendMessage(hwnd, message, wParam, lParam);
		}

		internal override bool PostMessage (IntPtr hwnd, Msg message, IntPtr wParam, IntPtr lParam) {
			return Win32PostMessage(hwnd, message, wParam, lParam);
		}

		internal override int SendInput (IntPtr hwnd, Queue keys) {
			INPUT[] inputs = new INPUT[keys.Count];
			const Int32 INPUT_KEYBOARD = 1;
			uint returns = 0;
			int i = 0;
			while (keys.Count > 0) {
				MSG msg = (MSG)keys.Dequeue();

				
				inputs[i].ki.wScan = 0;
				inputs[i].ki.time = 0;
				inputs[i].ki.dwFlags = (Int32)(msg.message == Msg.WM_KEYUP ? InputFlags.KEYEVENTF_KEYUP : 0);
				inputs[i].ki.wVk = (short)msg.wParam.ToInt32();
				inputs[i].type = INPUT_KEYBOARD;
				i++;
			}
			returns = Win32SendInput((UInt32)inputs.Length, inputs, Marshal.SizeOf(typeof(INPUT)));

			return (int) returns;
		}

		internal override int KeyboardSpeed {
			get {
				int speed = 0;
				Win32SystemParametersInfo(SPIAction.SPI_GETKEYBOARDSPEED, 0, ref speed, 0);
				//
				// Return values range from 0 to 31 which map to 2.5 to 30 repetitions per second.
				//
				return speed;
			}
		}

		internal override int KeyboardDelay {
			get {
				int delay = 1;
				Win32SystemParametersInfo(SPIAction.SPI_GETKEYBOARDDELAY, 0, ref delay, 0);
				//
				// Return values must range from 0 to 4, 0 meaning 250ms,
				// and 4 meaning 1000 ms.
				//
				return delay;
			}
		}

		private class WinBuffer
		{
			public IntPtr hdc;
			public IntPtr bitmap;

			public WinBuffer (IntPtr hdc, IntPtr bitmap)
			{
				this.hdc = hdc;
				this.bitmap = bitmap;
			}
		}

		internal override void CreateOffscreenDrawable (IntPtr handle, int width, int height, out object offscreen_drawable)
		{
			Graphics destG = Graphics.FromHwnd (handle);
			IntPtr destHdc = destG.GetHdc ();

			IntPtr srcHdc = Win32CreateCompatibleDC (destHdc);
			IntPtr srcBmp = Win32CreateCompatibleBitmap (destHdc, width, height);
			Win32SelectObject (srcHdc, srcBmp);

			offscreen_drawable = new WinBuffer (srcHdc, srcBmp);

			destG.ReleaseHdc (destHdc);
		}

		internal override Graphics GetOffscreenGraphics (object offscreen_drawable)
		{
			return Graphics.FromHdc (((WinBuffer)offscreen_drawable).hdc);
		}

		internal override void BlitFromOffscreen (IntPtr dest_handle, Graphics dest_dc, object offscreen_drawable, Graphics offscreen_dc, Rectangle r)
		{
			WinBuffer wb = (WinBuffer)offscreen_drawable;

			IntPtr destHdc = dest_dc.GetHdc ();
			Win32BitBlt (destHdc, r.Left, r.Top, r.Width, r.Height, wb.hdc, r.Left, r.Top, TernaryRasterOperations.SRCCOPY);
			dest_dc.ReleaseHdc (destHdc);
		}

		internal override void DestroyOffscreenDrawable (object offscreen_drawable)
		{
			WinBuffer wb = (WinBuffer)offscreen_drawable;

			Win32DeleteObject (wb.bitmap);
			Win32DeleteDC (wb.hdc);
		}

		internal override void SetForegroundWindow (IntPtr handle)
		{
			Win32SetForegroundWindow(handle);
		}

		internal override event EventHandler Idle;
		#endregion	// Public Static Methods

		#region Win32 Imports
		[DllImport ("kernel32.dll", EntryPoint="GetLastError", CallingConvention=CallingConvention.StdCall)]
		private extern static uint Win32GetLastError();

		[DllImport ("user32.dll", EntryPoint="CreateWindowExW", CharSet=CharSet.Unicode, CallingConvention=CallingConvention.StdCall)]
		internal extern static IntPtr Win32CreateWindow(WindowExStyles dwExStyle, string lpClassName, string lpWindowName, WindowStyles dwStyle, int x, int y, int nWidth, int nHeight, IntPtr hWndParent, IntPtr hMenu, IntPtr hInstance, IntPtr lParam);

		[DllImport ("user32.dll", EntryPoint="DestroyWindow", CallingConvention=CallingConvention.StdCall)]
		internal extern static bool Win32DestroyWindow(IntPtr hWnd);

		[DllImport ("user32.dll", EntryPoint="PeekMessageW", CharSet=CharSet.Unicode, CallingConvention=CallingConvention.StdCall)]
		internal extern static bool Win32PeekMessage(ref MSG msg, IntPtr hWnd, int wFilterMin, int wFilterMax, uint flags);

		[DllImport ("user32.dll", EntryPoint="GetMessageW", CharSet=CharSet.Unicode, CallingConvention=CallingConvention.StdCall)]
		internal extern static bool Win32GetMessage(ref MSG msg, IntPtr hWnd, int wFilterMin, int wFilterMax);

		[DllImport ("user32.dll", EntryPoint="TranslateMessage", CallingConvention=CallingConvention.StdCall)]
		internal extern static bool Win32TranslateMessage(ref MSG msg);

		[DllImport ("user32.dll", EntryPoint="DispatchMessageW", CharSet=CharSet.Unicode, CallingConvention=CallingConvention.StdCall)]
		internal extern static IntPtr Win32DispatchMessage(ref MSG msg);

		[DllImport ("user32.dll", EntryPoint="MoveWindow", CallingConvention=CallingConvention.StdCall)]
		internal extern static bool Win32MoveWindow(IntPtr hWnd, int x, int y, int width, int height, bool repaint);

		[DllImport ("user32.dll", EntryPoint="SetWindowPos", CallingConvention=CallingConvention.StdCall)]
		internal extern static bool Win32SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, SetWindowPosFlags Flags);

		[DllImport ("user32.dll", EntryPoint="SetWindowPos", CallingConvention=CallingConvention.StdCall)]
		internal extern static bool Win32SetWindowPos(IntPtr hWnd, SetWindowPosZOrder pos, int x, int y, int cx, int cy, SetWindowPosFlags Flags);

		[DllImport ("user32.dll", EntryPoint="SetWindowTextW", CharSet=CharSet.Unicode, CallingConvention=CallingConvention.StdCall)]
		internal extern static bool Win32SetWindowText(IntPtr hWnd, string lpString);

		[DllImport ("user32.dll", EntryPoint="GetWindowTextW", CharSet=CharSet.Unicode, CallingConvention=CallingConvention.StdCall)]
		internal extern static bool Win32GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

		[DllImport ("user32.dll", EntryPoint="SetParent", CallingConvention=CallingConvention.StdCall)]
		internal extern static IntPtr Win32SetParent(IntPtr hWnd, IntPtr hParent);

		[DllImport ("user32.dll", EntryPoint="RegisterClassW", CharSet=CharSet.Unicode, CallingConvention=CallingConvention.StdCall)]
		private extern static bool Win32RegisterClass(ref WNDCLASS wndClass);

		[DllImport ("user32.dll", EntryPoint="LoadCursorW", CharSet=CharSet.Unicode, CallingConvention=CallingConvention.StdCall)]
		private extern static IntPtr Win32LoadCursor(IntPtr hInstance, LoadCursorType type);

		[DllImport ("user32.dll", EntryPoint="ShowCursor", CallingConvention=CallingConvention.StdCall)]
		private extern static IntPtr Win32ShowCursor(bool bShow);

		[DllImport ("user32.dll", EntryPoint="SetCursor", CallingConvention=CallingConvention.StdCall)]
		private extern static IntPtr Win32SetCursor(IntPtr hCursor);

		[DllImport ("user32.dll", EntryPoint="CreateCursor", CallingConvention=CallingConvention.StdCall)]
		private extern static IntPtr Win32CreateCursor(IntPtr hInstance, int xHotSpot, int yHotSpot, int nWidth, int nHeight, Byte[] pvANDPlane, Byte[] pvORPlane);

		[DllImport ("user32.dll", EntryPoint="DestroyCursor", CallingConvention=CallingConvention.StdCall)]
		private extern static bool Win32DestroyCursor(IntPtr hCursor);

		[DllImport ("user32.dll", EntryPoint = "DrawIcon", CallingConvention = CallingConvention.StdCall)]
		private extern static bool Win32DrawIcon (IntPtr hDC, int X, int Y, IntPtr hIcon);
		
		[DllImport ("user32.dll", EntryPoint="DefWindowProcW", CharSet=CharSet.Unicode, CallingConvention=CallingConvention.StdCall)]
		private extern static IntPtr Win32DefWindowProc(IntPtr hWnd, Msg Msg, IntPtr wParam, IntPtr lParam);

		//[DllImport ("user32.dll", EntryPoint="DefDlgProcW", CharSet=CharSet.Unicode, CallingConvention=CallingConvention.StdCall)]
		//private extern static IntPtr Win32DefDlgProc(IntPtr hWnd, Msg Msg, IntPtr wParam, IntPtr lParam);

		[DllImport ("user32.dll", EntryPoint="PostQuitMessage", CallingConvention=CallingConvention.StdCall)]
		private extern static IntPtr Win32PostQuitMessage(int nExitCode);

		[DllImport ("user32.dll", EntryPoint="UpdateWindow", CallingConvention=CallingConvention.StdCall)]
		private extern static IntPtr Win32UpdateWindow(IntPtr hWnd);

		[DllImport ("user32.dll", EntryPoint="GetUpdateRect", CallingConvention=CallingConvention.StdCall)]
		private extern static bool Win32GetUpdateRect(IntPtr hWnd, ref RECT rect, bool erase);

		[DllImport ("user32.dll", EntryPoint="BeginPaint", CallingConvention=CallingConvention.StdCall)]
		private extern static IntPtr Win32BeginPaint(IntPtr hWnd, ref PAINTSTRUCT ps);

		[DllImport ("user32.dll", EntryPoint = "ValidateRect", CallingConvention = CallingConvention.StdCall)]
		private extern static IntPtr Win32ValidateRect (IntPtr hWnd, ref RECT rect);
		
		[DllImport ("user32.dll", EntryPoint="EndPaint", CallingConvention=CallingConvention.StdCall)]
		private extern static bool Win32EndPaint(IntPtr hWnd, ref PAINTSTRUCT ps);

		[DllImport ("user32.dll", EntryPoint="GetDC", CallingConvention=CallingConvention.StdCall)]
		private extern static IntPtr Win32GetDC(IntPtr hWnd);

		[DllImport ("user32.dll", EntryPoint="GetWindowDC", CallingConvention=CallingConvention.StdCall)]
		private extern static IntPtr Win32GetWindowDC(IntPtr hWnd);

		//[DllImport ("user32.dll", EntryPoint="GetDCEx", CallingConvention=CallingConvention.StdCall)]
		//private extern static IntPtr Win32GetDCEx(IntPtr hWnd, IntPtr hRgn, DCExFlags flags);

		[DllImport ("user32.dll", EntryPoint="ReleaseDC", CallingConvention=CallingConvention.StdCall)]
		private extern static IntPtr Win32ReleaseDC(IntPtr hWnd, IntPtr hDC);

		[DllImport ("user32.dll", EntryPoint="MessageBoxW", CharSet=CharSet.Unicode, CallingConvention=CallingConvention.StdCall)]
		private extern static IntPtr Win32MessageBox(IntPtr hParent, string pText, string pCaption, uint uType);

		[DllImport ("user32.dll", EntryPoint="InvalidateRect", CallingConvention=CallingConvention.StdCall)]
		private extern static IntPtr Win32InvalidateRect(IntPtr hWnd, ref RECT lpRect, bool bErase);

		//[DllImport ("user32.dll", EntryPoint="InvalidateRect", CallingConvention=CallingConvention.StdCall)]
		//private extern static IntPtr Win32InvalidateRect(IntPtr hWnd, IntPtr lpRect, bool bErase);

		[DllImport ("user32.dll", EntryPoint="SetCapture", CallingConvention=CallingConvention.StdCall)]
		private extern static IntPtr Win32SetCapture(IntPtr hWnd);

		[DllImport ("user32.dll", EntryPoint="ReleaseCapture", CallingConvention=CallingConvention.StdCall)]
		private extern static IntPtr Win32ReleaseCapture();

		[DllImport ("user32.dll", EntryPoint="GetWindowRect", CallingConvention=CallingConvention.StdCall)]
		private extern static IntPtr Win32GetWindowRect(IntPtr hWnd, out RECT rect);

		[DllImport ("user32.dll", EntryPoint="GetClientRect", CallingConvention=CallingConvention.StdCall)]
		private extern static IntPtr Win32GetClientRect(IntPtr hWnd, out RECT rect);

		[DllImport ("user32.dll", EntryPoint="ScreenToClient", CallingConvention=CallingConvention.StdCall)]
		private extern static bool Win32ScreenToClient(IntPtr hWnd, ref POINT pt);

		[DllImport ("user32.dll", EntryPoint="ClientToScreen", CallingConvention=CallingConvention.StdCall)]
		private extern static bool Win32ClientToScreen(IntPtr hWnd, ref POINT pt);

		// This function returns the parent OR THE OWNER!
		// Use GetAncestor to only get the parent.
		[DllImport ("user32.dll", EntryPoint="GetParent", CallingConvention=CallingConvention.StdCall)]
		private extern static IntPtr Win32GetParent(IntPtr hWnd);

		[DllImport ("user32.dll", EntryPoint = "GetAncestor", CallingConvention = CallingConvention.StdCall)]
		private extern static IntPtr Win32GetAncestor (IntPtr hWnd, AncestorType flags);

		[DllImport ("user32.dll", EntryPoint="SetActiveWindow", CallingConvention=CallingConvention.StdCall)]
		private extern static IntPtr Win32SetActiveWindow(IntPtr hWnd);

		[DllImport ("user32.dll", EntryPoint="AdjustWindowRectEx", CallingConvention=CallingConvention.StdCall)]
		private extern static bool Win32AdjustWindowRectEx(ref RECT lpRect, int dwStyle, bool bMenu, int dwExStyle);

		[DllImport ("user32.dll", EntryPoint="GetCursorPos", CallingConvention=CallingConvention.StdCall)]
		private extern static bool Win32GetCursorPos(out POINT lpPoint);

		[DllImport ("user32.dll", EntryPoint="SetCursorPos", CallingConvention=CallingConvention.StdCall)]
		private extern static bool Win32SetCursorPos(int x, int y);

		//[DllImport ("user32.dll", EntryPoint="GetWindowPlacement", CallingConvention=CallingConvention.StdCall)]
		//private extern static bool Win32GetWindowPlacement(IntPtr hWnd, ref WINDOWPLACEMENT lpwndpl);

		[DllImport ("user32.dll", EntryPoint="TrackMouseEvent", CallingConvention=CallingConvention.StdCall)]
		private extern static bool Win32TrackMouseEvent(ref TRACKMOUSEEVENT tme);

		//[DllImport ("gdi32.dll", EntryPoint="CreateBrushIndirect", CallingConvention=CallingConvention.StdCall)]
		//private extern static IntPtr Win32CreateBrushIndirect(ref LOGBRUSH lb);

		[DllImport ("gdi32.dll", EntryPoint="CreateSolidBrush", CallingConvention=CallingConvention.StdCall)]
		private extern static IntPtr Win32CreateSolidBrush(COLORREF clrRef);

		[DllImport ("gdi32.dll", EntryPoint="PatBlt", CallingConvention=CallingConvention.StdCall)]
		private extern static int Win32PatBlt(IntPtr hdc, int nXLeft, int nYLeft, int nWidth, int nHeight, PatBltRop dwRop);

		[DllImport ("user32.dll", EntryPoint="SetWindowLong", CallingConvention=CallingConvention.StdCall)]
		private extern static uint Win32SetWindowLong(IntPtr hwnd, WindowLong index, uint value);

		[DllImport ("user32.dll", EntryPoint="GetWindowLong", CallingConvention=CallingConvention.StdCall)]
		private extern static uint Win32GetWindowLong(IntPtr hwnd, WindowLong index);

		[DllImport ("user32.dll", EntryPoint="SetLayeredWindowAttributes", CallingConvention=CallingConvention.StdCall)]
		private extern static uint Win32SetLayeredWindowAttributes (IntPtr hwnd, COLORREF crKey, byte bAlpha, LayeredWindowAttributes dwFlags);

		[DllImport ("user32.dll", EntryPoint="GetLayeredWindowAttributes", CallingConvention=CallingConvention.StdCall)]
		private extern static uint Win32GetLayeredWindowAttributes (IntPtr hwnd, out COLORREF pcrKey, out byte pbAlpha, out LayeredWindowAttributes pwdFlags);

		[DllImport ("gdi32.dll", EntryPoint="DeleteObject", CallingConvention=CallingConvention.StdCall)]
		public extern static bool Win32DeleteObject(IntPtr o);

		[DllImport ("user32.dll", EntryPoint="GetKeyState", CallingConvention=CallingConvention.StdCall)]
		private extern static short Win32GetKeyState(VirtualKeys nVirtKey);

		[DllImport ("user32.dll", EntryPoint="GetDesktopWindow", CallingConvention=CallingConvention.StdCall)]
		private extern static IntPtr Win32GetDesktopWindow();

		[DllImport ("user32.dll", EntryPoint="SetTimer", CallingConvention=CallingConvention.StdCall)]
		private extern static IntPtr Win32SetTimer(IntPtr hwnd, int nIDEvent, uint uElapse, IntPtr timerProc);

		[DllImport ("user32.dll", EntryPoint="KillTimer", CallingConvention=CallingConvention.StdCall)]
		private extern static IntPtr Win32KillTimer(IntPtr hwnd, int nIDEvent);

		[DllImport ("user32.dll", EntryPoint="ShowWindow", CallingConvention=CallingConvention.StdCall)]
		private extern static IntPtr Win32ShowWindow(IntPtr hwnd, WindowPlacementFlags nCmdShow);

		[DllImport ("user32.dll", EntryPoint="EnableWindow", CallingConvention=CallingConvention.StdCall)]
		private extern static IntPtr Win32EnableWindow(IntPtr hwnd, bool Enabled);

		[DllImport ("user32.dll", EntryPoint="SetFocus", CallingConvention=CallingConvention.StdCall)]
		internal extern static IntPtr Win32SetFocus(IntPtr hwnd);

		[DllImport ("user32.dll", EntryPoint="GetFocus", CallingConvention=CallingConvention.StdCall)]
		internal extern static IntPtr Win32GetFocus();

		[DllImport ("user32.dll", EntryPoint="CreateCaret", CallingConvention=CallingConvention.StdCall)]
		internal extern static bool Win32CreateCaret(IntPtr hwnd, IntPtr hBitmap, int nWidth, int nHeight);

		[DllImport ("user32.dll", EntryPoint="DestroyCaret", CallingConvention=CallingConvention.StdCall)]
		private  extern static bool Win32DestroyCaret();

		[DllImport ("user32.dll", EntryPoint="ShowCaret", CallingConvention=CallingConvention.StdCall)]
		private  extern static bool Win32ShowCaret(IntPtr hwnd);

		[DllImport ("user32.dll", EntryPoint="HideCaret", CallingConvention=CallingConvention.StdCall)]
		private  extern static bool Win32HideCaret(IntPtr hwnd);

		[DllImport ("user32.dll", EntryPoint="SetCaretPos", CallingConvention=CallingConvention.StdCall)]
		private  extern static bool Win32SetCaretPos(int X, int Y);

		//[DllImport ("user32.dll", EntryPoint="GetCaretBlinkTime", CallingConvention=CallingConvention.StdCall)]
		//private  extern static uint Win32GetCaretBlinkTime();

		[DllImport ("gdi32.dll", EntryPoint="GetTextMetricsW", CharSet=CharSet.Unicode, CallingConvention=CallingConvention.StdCall)]
		internal extern static bool Win32GetTextMetrics(IntPtr hdc, ref TEXTMETRIC tm);

		[DllImport ("gdi32.dll", EntryPoint="SelectObject", CallingConvention=CallingConvention.StdCall)]
		internal extern static IntPtr Win32SelectObject(IntPtr hdc, IntPtr hgdiobject);

		//[DllImport ("user32.dll", EntryPoint="ScrollWindowEx", CallingConvention=CallingConvention.StdCall)]
		//private extern static bool Win32ScrollWindowEx(IntPtr hwnd, int dx, int dy, ref RECT prcScroll, ref RECT prcClip, IntPtr hrgnUpdate, out RECT prcUpdate, ScrollWindowExFlags flags);

		//[DllImport ("user32.dll", EntryPoint="ScrollWindowEx", CallingConvention=CallingConvention.StdCall)]
		//private extern static bool Win32ScrollWindowEx(IntPtr hwnd, int dx, int dy, IntPtr prcScroll, ref RECT prcClip, IntPtr hrgnUpdate, out RECT prcUpdate, ScrollWindowExFlags flags);

		//[DllImport ("user32.dll", EntryPoint="ScrollWindowEx", CallingConvention=CallingConvention.StdCall)]
		//private extern static bool Win32ScrollWindowEx(IntPtr hwnd, int dx, int dy, ref RECT prcScroll, IntPtr prcClip, IntPtr hrgnUpdate, out RECT prcUpdate, ScrollWindowExFlags flags);

		[DllImport ("user32.dll", EntryPoint="ScrollWindowEx", CallingConvention=CallingConvention.StdCall)]
		private extern static bool Win32ScrollWindowEx(IntPtr hwnd, int dx, int dy, IntPtr prcScroll, ref RECT prcClip, IntPtr hrgnUpdate, IntPtr prcUpdate, ScrollWindowExFlags flags);

		//[DllImport ("user32.dll", EntryPoint="ScrollWindowEx", CallingConvention=CallingConvention.StdCall)]
		//private extern static bool Win32ScrollWindowEx(IntPtr hwnd, int dx, int dy, ref RECT prcScroll, IntPtr prcClip, IntPtr hrgnUpdate, IntPtr prcUpdate, ScrollWindowExFlags flags);

		//[DllImport ("user32.dll", EntryPoint="ScrollWindowEx", CallingConvention=CallingConvention.StdCall)]
		//private extern static bool Win32ScrollWindowEx(IntPtr hwnd, int dx, int dy, ref RECT prcScroll, ref RECT prcClip, IntPtr hrgnUpdate, IntPtr prcUpdate, ScrollWindowExFlags flags);

		[DllImport ("user32.dll", EntryPoint="ScrollWindowEx", CallingConvention=CallingConvention.StdCall)]
		private extern static bool Win32ScrollWindowEx(IntPtr hwnd, int dx, int dy, IntPtr prcScroll, IntPtr prcClip, IntPtr hrgnUpdate, IntPtr prcUpdate, ScrollWindowExFlags flags);

		[DllImport ("user32.dll", EntryPoint="GetActiveWindow", CallingConvention=CallingConvention.StdCall)]
		private extern static IntPtr Win32GetActiveWindow();

		[DllImport ("user32.dll", EntryPoint="GetSystemMetrics", CallingConvention=CallingConvention.StdCall)]
		private extern static int Win32GetSystemMetrics(SystemMetrics nIndex);

		[DllImport ("shell32.dll", EntryPoint="Shell_NotifyIconW", CharSet=CharSet.Unicode, CallingConvention=CallingConvention.StdCall)]
		private extern static bool Win32Shell_NotifyIcon(NotifyIconMessage dwMessage, ref NOTIFYICONDATA lpData);

		[DllImport ("gdi32.dll", EntryPoint="CreateRectRgn", CallingConvention=CallingConvention.StdCall)]
		internal extern static IntPtr Win32CreateRectRgn(int nLeftRect, int nTopRect, int nRightRect, int nBottomRect);

		[DllImport ("user32.dll", EntryPoint="IsWindowEnabled", CallingConvention=CallingConvention.StdCall)]
		private extern static bool IsWindowEnabled(IntPtr hwnd);

		[DllImport ("user32.dll", EntryPoint="IsWindowVisible", CallingConvention=CallingConvention.StdCall)]
		private extern static bool IsWindowVisible(IntPtr hwnd);

		//[DllImport ("user32.dll", EntryPoint="SetClassLong", CallingConvention=CallingConvention.StdCall)]
		//private extern static bool Win32SetClassLong(IntPtr hwnd, ClassLong nIndex, IntPtr dwNewLong);

		[DllImport ("user32.dll", EntryPoint="SendMessageW", CharSet=CharSet.Unicode, CallingConvention=CallingConvention.StdCall)]
		private extern static IntPtr Win32SendMessage(IntPtr hwnd, Msg msg, IntPtr wParam, IntPtr lParam);

		[DllImport ("user32.dll", EntryPoint="PostMessageW", CharSet=CharSet.Unicode, CallingConvention=CallingConvention.StdCall)]
		private extern static bool Win32PostMessage(IntPtr hwnd, Msg msg, IntPtr wParam, IntPtr lParam);

		[DllImport ("user32.dll", EntryPoint="SendInput", CharSet=CharSet.Unicode, CallingConvention=CallingConvention.StdCall)]
		private extern static UInt32 Win32SendInput(UInt32 nInputs, [MarshalAs(UnmanagedType.LPArray)] INPUT[] inputs, Int32 cbSize);

		[DllImport ("user32.dll", EntryPoint="SystemParametersInfoW", CharSet=CharSet.Unicode, CallingConvention=CallingConvention.StdCall)]
		private extern static bool Win32SystemParametersInfo(SPIAction uiAction, uint uiParam, ref RECT rect, uint fWinIni);
		
		//[DllImport ("user32.dll", EntryPoint="SystemParametersInfoW", CharSet=CharSet.Unicode, CallingConvention=CallingConvention.StdCall)]
		//private extern static bool Win32SystemParametersInfo(SPIAction uiAction, uint uiParam, ref uint value, uint fWinIni);

		[DllImport ("user32.dll", EntryPoint = "SystemParametersInfoW", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
		private extern static bool Win32SystemParametersInfo (SPIAction uiAction, uint uiParam, ref int value, uint fWinIni);

		[DllImport ("user32.dll", EntryPoint = "SystemParametersInfoW", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
		private extern static bool Win32SystemParametersInfo (SPIAction uiAction, uint uiParam, ref bool value, uint fWinIni);

		[DllImport ("user32.dll", EntryPoint = "SystemParametersInfoW", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
		private extern static bool Win32SystemParametersInfo (SPIAction uiAction, uint uiParam, ref ANIMATIONINFO value, uint fWinIni);

		[DllImport ("user32.dll", EntryPoint="OpenClipboard", CallingConvention=CallingConvention.StdCall)]
		private extern static bool Win32OpenClipboard(IntPtr hwnd);

		[DllImport ("user32.dll", EntryPoint="EmptyClipboard", CallingConvention=CallingConvention.StdCall)]
		private extern static bool Win32EmptyClipboard();

		[DllImport ("user32.dll", EntryPoint="RegisterClipboardFormatW", CharSet=CharSet.Unicode, CallingConvention=CallingConvention.StdCall)]
		private extern static uint Win32RegisterClipboardFormat(string format);

		[DllImport ("user32.dll", EntryPoint="CloseClipboard", CallingConvention=CallingConvention.StdCall)]
		private extern static bool Win32CloseClipboard();

		[DllImport ("user32.dll", EntryPoint="EnumClipboardFormats", CallingConvention=CallingConvention.StdCall)]
		private extern static uint Win32EnumClipboardFormats(uint format);

		[DllImport ("user32.dll", EntryPoint="GetClipboardData", CallingConvention=CallingConvention.StdCall)]
		private extern static IntPtr Win32GetClipboardData(uint format);

		[DllImport ("user32.dll", EntryPoint="SetClipboardData", CallingConvention=CallingConvention.StdCall)]
		private extern static IntPtr Win32SetClipboardData(uint format, IntPtr handle);

		[DllImport ("kernel32.dll", EntryPoint="GlobalAlloc", CallingConvention=CallingConvention.StdCall)]
		internal extern static IntPtr Win32GlobalAlloc(GAllocFlags Flags, int dwBytes);

		[DllImport ("kernel32.dll", EntryPoint="CopyMemory", CallingConvention=CallingConvention.StdCall)]
		internal extern static void Win32CopyMemory(IntPtr Destination, IntPtr Source, int length);

		[DllImport ("kernel32.dll", EntryPoint="GlobalFree", CallingConvention=CallingConvention.StdCall)]
		internal extern static IntPtr Win32GlobalFree(IntPtr hMem);

		[DllImport ("kernel32.dll", EntryPoint="GlobalSize", CallingConvention=CallingConvention.StdCall)]
		internal extern static uint Win32GlobalSize(IntPtr hMem);

		[DllImport ("kernel32.dll", EntryPoint="GlobalLock", CallingConvention=CallingConvention.StdCall)]
		internal extern static IntPtr Win32GlobalLock(IntPtr hMem);

		[DllImport ("kernel32.dll", EntryPoint="GlobalUnlock", CallingConvention=CallingConvention.StdCall)]
		internal extern static IntPtr Win32GlobalUnlock(IntPtr hMem);

		[DllImport ("gdi32.dll", EntryPoint="SetROP2", CallingConvention=CallingConvention.StdCall)]
		internal extern static int Win32SetROP2(IntPtr hdc, ROP2DrawMode fnDrawMode);

		[DllImport ("gdi32.dll", EntryPoint="MoveToEx", CallingConvention=CallingConvention.StdCall)]
		internal extern static bool Win32MoveToEx(IntPtr hdc, int x, int y, ref POINT lpPoint);

		[DllImport ("gdi32.dll", EntryPoint="MoveToEx", CallingConvention=CallingConvention.StdCall)]
		internal extern static bool Win32MoveToEx(IntPtr hdc, int x, int y, IntPtr lpPoint);

		[DllImport ("gdi32.dll", EntryPoint="LineTo", CallingConvention=CallingConvention.StdCall)]
		internal extern static bool Win32LineTo(IntPtr hdc, int x, int y);

		[DllImport ("gdi32.dll", EntryPoint="CreatePen", CallingConvention=CallingConvention.StdCall)]
		internal extern static IntPtr Win32CreatePen(PenStyle fnPenStyle, int nWidth, ref COLORREF color);

		[DllImport ("gdi32.dll", EntryPoint="CreatePen", CallingConvention=CallingConvention.StdCall)]
		internal extern static IntPtr Win32CreatePen(PenStyle fnPenStyle, int nWidth, IntPtr color);

		[DllImport ("gdi32.dll", EntryPoint="GetStockObject", CallingConvention=CallingConvention.StdCall)]
		internal extern static IntPtr Win32GetStockObject(StockObject fnObject);

		[DllImport ("gdi32.dll", EntryPoint="CreateHatchBrush", CallingConvention=CallingConvention.StdCall)]
		internal extern static IntPtr Win32CreateHatchBrush(HatchStyle fnStyle, IntPtr color);

		[DllImport ("gdi32.dll", EntryPoint="CreateHatchBrush", CallingConvention=CallingConvention.StdCall)]
		internal extern static IntPtr Win32CreateHatchBrush(HatchStyle fnStyle, ref COLORREF color);

		[DllImport("gdi32.dll", EntryPoint = "ExcludeClipRect", CallingConvention = CallingConvention.StdCall)]
		internal extern static int Win32ExcludeClipRect (IntPtr hdc, int left, int top,  int right, int bottom);

		[DllImport ("gdi32.dll", EntryPoint="ExtSelectClipRgn", CallingConvention=CallingConvention.StdCall)]
		internal extern static int Win32ExtSelectClipRgn(IntPtr hdc, IntPtr hrgn, int mode);

		[DllImport ("winmm.dll", EntryPoint="PlaySoundW", CallingConvention=CallingConvention.StdCall, CharSet=CharSet.Unicode)]
		internal extern static IntPtr Win32PlaySound(string pszSound, IntPtr hmod, SndFlags fdwSound);

		[DllImport ("user32.dll", EntryPoint="GetDoubleClickTime", CallingConvention=CallingConvention.StdCall, CharSet=CharSet.Unicode)]
		private extern static int Win32GetDoubleClickTime ();

		[DllImport ("user32.dll", EntryPoint="SetWindowRgn", CallingConvention=CallingConvention.StdCall, CharSet=CharSet.Unicode)]
		internal extern static int Win32SetWindowRgn(IntPtr hWnd, IntPtr hRgn, bool redraw);

		[DllImport ("user32.dll", EntryPoint="GetWindowRgn", CallingConvention=CallingConvention.StdCall, CharSet=CharSet.Unicode)]
		internal extern static IntPtr Win32GetWindowRgn(IntPtr hWnd, IntPtr hRgn);

		[DllImport ("user32.dll", EntryPoint="ClipCursor", CallingConvention=CallingConvention.StdCall)]
		internal extern static bool Win32ClipCursor (ref RECT lpRect);

		[DllImport ("user32.dll", EntryPoint="GetClipCursor", CallingConvention=CallingConvention.StdCall)]
		internal extern static bool Win32GetClipCursor (out RECT lpRect);

		[DllImport ("gdi32.dll", EntryPoint="BitBlt", CallingConvention=CallingConvention.StdCall)]
		internal static extern bool Win32BitBlt (IntPtr hObject, int nXDest, int nYDest, int nWidth,
		   int nHeight, IntPtr hObjSource, int nXSrc, int nYSrc, TernaryRasterOperations dwRop);

		[DllImport ("gdi32.dll", EntryPoint="CreateCompatibleDC", CallingConvention=CallingConvention.StdCall, ExactSpelling = true, SetLastError = true)]
		internal static extern IntPtr Win32CreateCompatibleDC (IntPtr hdc);

		[DllImport ("gdi32.dll", EntryPoint="DeleteDC", CallingConvention=CallingConvention.StdCall, ExactSpelling = true, SetLastError = true)]
		internal static extern bool Win32DeleteDC (IntPtr hdc);

		[DllImport ("gdi32.dll", EntryPoint="CreateCompatibleBitmap", CallingConvention=CallingConvention.StdCall)]
		internal static extern IntPtr Win32CreateCompatibleBitmap (IntPtr hdc, int nWidth, int nHeight);

		[DllImport ("kernel32.dll", EntryPoint = "GetSystemPowerStatus", CallingConvention = CallingConvention.StdCall)]
		internal static extern Boolean Win32GetSystemPowerStatus (SYSTEMPOWERSTATUS sps);

		[DllImport ("user32.dll", EntryPoint = "GetIconInfo", CallingConvention = CallingConvention.StdCall)]
		internal static extern bool Win32GetIconInfo (IntPtr hIcon, out ICONINFO piconinfo);

		[DllImport ("user32.dll", EntryPoint="SetForegroundWindow", CallingConvention=CallingConvention.StdCall)]
		extern static bool Win32SetForegroundWindow(IntPtr hWnd);
		#endregion
	}
}
