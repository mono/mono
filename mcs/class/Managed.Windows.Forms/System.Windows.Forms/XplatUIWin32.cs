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
// Copyright (c) 2004 Novell, Inc.
//
// Authors:
//	Peter Bartok	pbartok@novell.com
//
//
// $Revision: 1.43 $
// $Modtime: $
// $Log: XplatUIWin32.cs,v $
// Revision 1.43  2004/11/08 20:55:33  pbartok
// - Added argument to SetTopmost method
// - Fixed broken ClientToScreen function
//
// Revision 1.42  2004/10/20 03:58:05  pbartok
// - Small sanity check
//
// Revision 1.41  2004/10/18 05:17:32  pbartok
// - Removed VirtualKeys to XplatUIStructs
// - Implemented SetTopMost method
// - Implemented EnableWindow method
// - Bugfix in ScreenToClient()
// - Bugfixes in ClientToScreen()
//
// Revision 1.40  2004/10/13 20:18:21  pbartok
// - Added code to destroy a window
//
// Revision 1.39  2004/10/06 09:59:05  jordi
// removes warnings from compilation
//
// Revision 1.38  2004/10/02 19:08:38  pbartok
// - Added Win32EnableWindow method (test for implementing modal dialogs)
// - Added ClientToScreen method and imports
//
// Revision 1.37  2004/09/21 04:14:29  pbartok
// - Fixed accessibility level for Idle handler
//
// Revision 1.36  2004/09/21 00:54:15  jackson
// New message loop that uses poll so we don't get a busy loop
//
// Revision 1.35  2004/09/16 23:45:09  pbartok
// - Fixed sending a window to the front
// - Added overload for SetWindowPos to avoid casting
//
// Revision 1.34  2004/09/13 21:18:32  pbartok
// - Added Z-Ordering methods
//
// Revision 1.33  2004/09/11 00:57:35  pbartok
// - Added method to retrieve text from window
//
// Revision 1.32  2004/08/25 18:33:08  pbartok
// - Fixed timer handling, now seems to work
// - Improved error message for window creation
//
// Revision 1.31  2004/08/24 17:17:27  pbartok
// - Implemented SetTimer() and KillTimer()
//
// Revision 1.30  2004/08/24 11:29:44  jackson
// Move timers to the driver level. On X they are queued by the driver and checked on idle.
//
// Revision 1.29  2004/08/24 00:19:36  ravindra
// Removed the unwanted destructor.
//
// Revision 1.28  2004/08/23 19:39:30  pbartok
// - Added method to move mouse cursor
//
// Revision 1.27  2004/08/21 20:51:27  pbartok
// - Added method to get default display size
//
// Revision 1.26  2004/08/21 20:23:56  pbartok
// - Added method to query current grab state
// - Added argument to allow confining a grab to a window
//
// Revision 1.25  2004/08/21 18:35:38  pbartok
// - Fixed bug with Async message handling
// - Implemented getting the ModifierKeys
//
// Revision 1.24  2004/08/21 17:31:21  pbartok
// - Drivers now return proper mouse state
//
// Revision 1.23  2004/08/20 20:39:07  pbartok
// - Added jackson's Async code from X11 to Win32
//
// Revision 1.22  2004/08/20 20:02:45  pbartok
// - Added method for setting the background color
// - Added handling for erasing the window background
//
// Revision 1.21  2004/08/20 19:14:35  jackson
// Expose functionality to send async messages through the driver
//
// Revision 1.20  2004/08/20 01:37:47  pbartok
// - Added generation of MouseEnter, MouseLeave and MouseHover events
// - Added cleanup on EndPaint
//
// Revision 1.19  2004/08/18 19:16:53  jordi
// Move colors to a table
//
// Revision 1.18  2004/08/17 21:24:03  pbartok
// - Finished IsVisible
// - Added Win32GetWindowPlacement
//
// Revision 1.17  2004/08/13 21:42:15  pbartok
// - Changed signature for GetCursorPos
//
// Revision 1.16  2004/08/13 19:00:15  jordi
// implements PointToClient (ScreenToClient)
//
// Revision 1.15  2004/08/13 18:53:57  pbartok
// - Changed GetWindowPos to also provide client area size
// - Fixed broken prototypes for several win32 functions
//
// Revision 1.14  2004/08/12 22:59:03  pbartok
// - Implemented method to get current mouse position
//
// Revision 1.13  2004/08/11 22:20:59  pbartok
// - Signature fixes
//
// Revision 1.12  2004/08/11 19:41:38  jordi
// Fixes ClientRect
//
// Revision 1.11  2004/08/11 19:19:44  pbartok
// - We had SetWindowPos and MoveWindow to set window positions and size,
//   removed MoveWindow. We have GetWindowPos, so it made sense to keep
//   SetWindowPos as matching counterpart
// - Added some X11 sanity checking
//
// Revision 1.10  2004/08/11 18:55:46  pbartok
// - Added method to calculate difference between decorated window and raw
//   client area
//
// Revision 1.9  2004/08/10 18:47:16  jordi
// Calls InvalidateRect before UpdateWindow
//
// Revision 1.8  2004/08/10 17:36:17  pbartok
// - Implemented several methods
//
// Revision 1.7  2004/08/09 20:55:59  pbartok
// - Removed Run method, was only required for initial development
//
// Revision 1.6  2004/08/09 20:51:25  pbartok
// - Implemented GrabWindow/ReleaseWindow methods to allow pointer capture
//
// Revision 1.5  2004/08/09 16:05:16  jackson
// These properties are handled by the theme now.
//
// Revision 1.4  2004/08/06 15:53:39  jordi
// X11 keyboard navigation
//
// Revision 1.3  2004/08/04 20:11:24  pbartok
// - Added Invalidate handling
//
// Revision 1.2  2004/07/21 16:19:17  jordi
// LinkLabel control implementation
//
// Revision 1.1  2004/07/09 05:21:25  pbartok
// - Initial check-in
//
//

// NOT COMPLETE

using System;
using System.Drawing;
using System.ComponentModel;
using System.Collections;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

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

		internal static bool		themes_enabled;
		private static Hashtable	handle_data;
		private Hashtable		timer_list;
		#endregion	// Local Variables

		#region Private Structs
		[StructLayout(LayoutKind.Sequential)]
		private struct WNDCLASS {
			internal int		style;
			internal WndProc	lpfnWndProc;
			internal int		cbClsExtra;
			internal int		cbWndExtra;
			internal IntPtr		hInstance;
			internal IntPtr		hIcon;
			internal IntPtr		hCursor;
			internal IntPtr		hbrBackground;
			internal string		lpszMenuName;
			internal string		lpszClassName;
		}

		[StructLayout(LayoutKind.Sequential)]
		private struct RECT {
			internal int		left;
			internal int		top;
			internal int		right;
			internal int		bottom;
		}

		[StructLayout(LayoutKind.Sequential)]
		private struct POINT {
			internal int		x;
			internal int		y;
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

		[Flags]
		private enum TMEFlags {
			TME_HOVER		= 0x00000001,
			TME_LEAVE		= 0x00000002,
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
			CS_IME				= 0x00010000
		}

		internal enum PeekMessageFlags {
			PM_NOREMOVE			= 0x00000000,
			PM_REMOVE			= 0x00000001,
			PM_NOYIELD			= 0x00000002
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
			COLOR_SCROLLBAR			=0,
			COLOR_BACKGROUND		=1,
			COLOR_ACTIVECAPTION		=2,
			COLOR_INACTIVECAPTION		=3,
			COLOR_MENU			=4,
			COLOR_WINDOW			=5,
			COLOR_WINDOWFRAME		=6,
			COLOR_MENUTEXT			=7,
			COLOR_WINDOWTEXT		=8,
			COLOR_CAPTIONTEXT		=9,
			COLOR_ACTIVEBORDER		=10,
			COLOR_INACTIVEBORDER		=11,
			COLOR_APPWORKSPACE		=12,
			COLOR_HIGHLIGHT			=13,
			COLOR_HIGHLIGHTTEXT		=14,
			COLOR_BTNFACE			=15,
			COLOR_BTNSHADOW			=16,
			COLOR_GRAYTEXT			=17,
			COLOR_BTNTEXT			=18,
			COLOR_INACTIVECAPTIONTEXT	=19,
			COLOR_BTNHIGHLIGHT		=20,
			COLOR_3DDKSHADOW		=21,
			COLOR_3DLIGHT			=22,
			COLOR_INFOTEXT			=23,
			COLOR_INFOBK			=24,
			COLOR_DESKTOP			=1,
			COLOR_3DFACE			=16,
			COLOR_3DSHADOW			=16,
			COLOR_3DHIGHLIGHT		=20,
			COLOR_3DHILIGHT			=20,
			COLOR_BTNHILIGHT		=20,
			COLOR_MAXVALUE			=24,/* Maximum value */
		}       

		private enum LoadCursorType {
			IDC_ARROW			=32512,
			IDC_IBEAM			=32513,
			IDC_WAIT			=32514,
			IDC_CROSS			=32515,
			IDC_UPARROW			=32516,
			IDC_SIZE			=32640,
			IDC_ICON			=32641,
			IDC_SIZENWSE			=32642,
			IDC_SIZENESW			=32643,
			IDC_SIZEWE			=32644,
			IDC_SIZENS			=32645,
			IDC_SIZEALL			=32646,
			IDC_NO				=32648,
			IDC_HAND			=32649,
			IDC_APPSTARTING			=32650,
			IDC_HELP			=32651
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

		private struct COLORREF {
			internal byte			B;
			internal byte			G;
			internal byte			R;
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
			internal byte			tmFirstChar; 
			internal byte			tmLastChar; 
			internal byte			tmDefaultChar; 
			internal byte			tmBreakChar; 
			internal byte			tmItalic; 
			internal byte			tmUnderlined; 
			internal byte			tmStruckOut; 
			internal byte			tmPitchAndFamily; 
			internal byte			tmCharSet; 
		} 

		#endregion

		#region Constructor & Destructor
		private XplatUIWin32() {
			WNDCLASS	wndClass;
			bool		result;

			// Handle singleton stuff first
			ref_count=0;

			// Now regular initialization
			mouse_state = MouseButtons.None;
			mouse_position = Point.Empty;

			themes_enabled = false;

			// Prepare 'our' window class
			wnd_proc = new WndProc(NativeWindow.WndProc);
			wndClass.style = (int)(ClassStyle.CS_OWNDC | ClassStyle.CS_DBLCLKS);
			wndClass.lpfnWndProc = wnd_proc;
			wndClass.cbClsExtra = 0;
			wndClass.cbWndExtra = 0;
			wndClass.hbrBackground = IntPtr.Zero;
			wndClass.hCursor = Win32LoadCursor(IntPtr.Zero, LoadCursorType.IDC_ARROW);
			wndClass.hIcon = IntPtr.Zero;
			wndClass.hInstance = IntPtr.Zero;
			wndClass.lpszClassName = XplatUI.DefaultClassName;
			wndClass.lpszMenuName = "";

			result=Win32RegisterClass(ref wndClass);
			if (result==false) {
				Win32MessageBox(IntPtr.Zero, "Could not register the "+XplatUI.DefaultClassName+" window class, win32 error " + Win32GetLastError().ToString(), "Oops", 0);
			}

			FosterParent=Win32CreateWindow(0, "static", "Foster Parent Window", (int)WindowStyles.WS_OVERLAPPEDWINDOW, 0, 0, 0, 0, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);

			if (FosterParent==IntPtr.Zero) {
				Win32MessageBox(IntPtr.Zero, "Could not create foster window, win32 error " + Win32GetLastError().ToString(), "Oops", 0);
			}

			handle_data = new Hashtable ();
			timer_list = new Hashtable ();
		}
		#endregion	// Constructor & Destructor

		#region Private Support Methods
		private static IntPtr DefWndProc(IntPtr hWnd, Msg msg, IntPtr wParam, IntPtr lParam) {
			return Win32DefWindowProc(hWnd, msg, wParam, lParam);
		}

		private void EraseWindowBackground(IntPtr hWnd, IntPtr hDc) {
			IntPtr		hbr;
			LOGBRUSH	lb;
			uint		argb;
			RECT		rect;
						
			//msg.wParam
			argb = (uint)Win32GetWindowLong(hWnd, WindowLong.GWL_USERDATA);
			lb = new LOGBRUSH();
						
			lb.lbColor.B = (byte)((argb & 0xff0000)>>16);
			lb.lbColor.G = (byte)((argb & 0xff00)>>8);
			lb.lbColor.R = (byte)(argb & 0xff);

			lb.lbStyle = LogBrushStyle.BS_SOLID;
			hbr = Win32CreateBrushIndirect(ref lb);
			Win32GetClientRect(hWnd, out rect);
			Win32FillRect(hDc, ref rect, hbr);
			Win32DeleteObject(hbr);
		}

		#endregion	// Private Support Methods

		#region Static Properties
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

		internal override bool DropTarget {
			get {
				return false;
			}

			set {
				if (value) {
					throw new NotImplementedException("Need to figure out D'n'D for Win32");
				}
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
			mouse_state=MouseButtons.None;
			mouse_position=Point.Empty;

			Console.WriteLine("#region #line XplatUI Win32 Constructor called");

			return IntPtr.Zero;
		}

		internal override void ShutdownDriver(IntPtr token) {
			Console.WriteLine("XplatUIWin32 ShutdownDriver called");
		}


		internal void Version() {
			Console.WriteLine("Xplat version $revision: $");
		}

		internal override void Exit() {
			Win32PostQuitMessage(0);
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

			ParentHandle=cp.Parent;

			if ((ParentHandle==IntPtr.Zero) && (cp.Style & (int)(WindowStyles.WS_CHILD))!=0) {
				// We need to use our foster parent window until this poor child gets it's parent assigned
				ParentHandle=FosterParent;
			}
			WindowHandle = Win32CreateWindow((uint)cp.ExStyle, cp.ClassName, cp.Caption, (uint)cp.Style, cp.X, cp.Y, cp.Width, cp.Height, ParentHandle, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);

			if (WindowHandle==IntPtr.Zero) {
				uint error = Win32GetLastError();

				Win32MessageBox(IntPtr.Zero, "Error : " + error.ToString(), "Failed to create window, class '"+cp.ClassName+"'", 0);
			}

			Win32SetWindowLong(WindowHandle, WindowLong.GWL_USERDATA, (IntPtr)ThemeEngine.Current.DefaultControlBackColor.ToArgb());

			return WindowHandle;
		}

		internal override IntPtr CreateWindow(IntPtr Parent, int X, int Y, int Width, int Height) {
			CreateParams create_params = new CreateParams();

			create_params.Caption = "";
			create_params.X = X;
			create_params.Y = Y;
			create_params.Width = Width;
			create_params.Height = Height;

			create_params.ClassName=XplatUI.DefaultClassName;
			create_params.ClassStyle = 0;
			create_params.ExStyle=0;
			create_params.Parent=IntPtr.Zero;
			create_params.Param=0;

			return CreateWindow(create_params);
		}

		internal override void DestroyWindow(IntPtr handle) {
			Win32DestroyWindow(handle);
			return;
		}

		internal override void RefreshWindow(IntPtr handle) {			
			Win32InvalidateRect(handle, IntPtr.Zero, true);
			Win32UpdateWindow(handle);
		}

		internal override void SetWindowBackground(IntPtr handle, Color color) {
			Win32SetWindowLong(handle, WindowLong.GWL_USERDATA, (IntPtr)color.ToArgb());
		}

		[MonoTODO("Add support for internal table of windows/DCs for cleanup")]
		internal override PaintEventArgs PaintEventStart(IntPtr handle) {
			IntPtr		hdc;
			PAINTSTRUCT	ps;
			PaintEventArgs	paint_event;
			RECT		rect;
			Rectangle	clip_rect;

			clip_rect = new Rectangle();
			rect = new RECT();
			ps = new PAINTSTRUCT();

			if (Win32GetUpdateRect(handle, ref rect, false)) {
				HandleData	data;

				hdc = Win32BeginPaint(handle, ref ps);

				data = (HandleData) handle_data [0];
				if (data == null) {
					data = new HandleData();
					handle_data[0] = data;
				}

				data.DeviceContext=(Object)ps;

				// FIXME: Figure out why the rectangle is always 0 size
				clip_rect = new Rectangle(ps.rcPaint.left, ps.rcPaint.top, ps.rcPaint.right-ps.rcPaint.left, ps.rcPaint.bottom-ps.rcPaint.top);
//				clip_rect = new Rectangle(rect.top, rect.left, rect.right-rect.left, rect.bottom-rect.top);

				if (ps.fErase!=0) {
					EraseWindowBackground(handle, hdc);
				}
			} else {
				hdc = Win32GetDC(handle);
				// FIXME: Add the DC to internal list
				clip_rect = new Rectangle(rect.top, rect.left, rect.right-rect.left, rect.bottom-rect.top);
			}

			paint_event = new PaintEventArgs(Graphics.FromHdc(hdc), clip_rect);

			return paint_event;
		}

		internal override void PaintEventEnd(IntPtr handle) {
			HandleData	data;
			PAINTSTRUCT	ps;			

			data = (HandleData) handle_data [0];
			if (data == null) {
				data = new HandleData();
				handle_data[0] = data;
			}

			//paint_event.Graphics.Dispose();
			if (data.DeviceContext != null) {
				ps = (PAINTSTRUCT)data.DeviceContext;
				Win32EndPaint(handle, ref ps);
			}
		}


		internal override void SetWindowPos(IntPtr handle, int x, int y, int width, int height) {
			Win32MoveWindow(handle, x, y, width, height, true);
			return;
		}

		internal override void GetWindowPos(IntPtr handle, out int x, out int y, out int width, out int height, out int client_width, out int client_height) {
			RECT	rect;
			POINT	pt;

			Win32GetWindowRect(handle, out rect);
			width = rect.right - rect.left;
			height = rect.bottom - rect.top;

			pt.x=rect.left;
			pt.y=rect.top;
			Win32ScreenToClient(Win32GetParent(handle), ref pt);
			x = pt.x;
			y = pt.y;

			Win32GetClientRect(handle, out rect);
			client_width = rect.right - rect.left;
			client_height = rect.bottom - rect.top;
			return;
		}

		internal override void Activate(IntPtr handle) {
			Win32SetActiveWindow(handle);
		}

		internal override void Invalidate(IntPtr handle, Rectangle rc, bool clear) {
			RECT rect;

			rect.left=rc.Left;
			rect.top=rc.Top;
			rect.right=rc.Right;
			rect.bottom=rc.Bottom;
			Win32InvalidateRect(handle, ref rect, clear);
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

			while (Win32PeekMessage(ref msg, IntPtr.Zero, 0, 0, (uint)PeekMessageFlags.PM_REMOVE)!=true) {
				if (msg.message==Msg.WM_PAINT) {
					XplatUI.TranslateMessage(ref msg);
					XplatUI.DispatchMessage(ref msg);
				}
			}
		}

		internal override bool PeekMessage(ref MSG msg, IntPtr hWnd, int wFilterMin, int wFilterMax, uint flags) {
			return Win32PeekMessage(ref msg, hWnd, wFilterMin, wFilterMax, flags);
		}

		internal override bool GetMessage(ref MSG msg, IntPtr hWnd, int wFilterMin, int wFilterMax) {
			HandleData	data;
			bool		result;
			data = (HandleData) handle_data [0];
			if ((data!=null) && data.GetMessage(ref msg)) {
				return true;
			}

			result = Win32GetMessage(ref msg, hWnd, wFilterMin, wFilterMax);

			// We need to fake WM_MOUSE_ENTER/WM_MOUSE_LEAVE
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

				case Msg.WM_ERASEBKGND: {
					EraseWindowBackground(msg.hwnd, msg.wParam);
					break;
				}

				case Msg.WM_ASYNC_MESSAGE: {
					GCHandle handle = (GCHandle)msg.lParam;
					AsyncMethodData asyncdata = (AsyncMethodData) handle.Target;
					AsyncMethodResult asyncresult = asyncdata.Result.Target as AsyncMethodResult;
					object ret = asyncdata.Method.DynamicInvoke (asyncdata.Args);
					if (asyncresult != null) {
						asyncresult.Complete (ret);
					}
					handle.Free ();
					break;
				}

				case Msg.WM_MOUSEMOVE: {
					if (msg.hwnd != prev_mouse_hwnd) {
						TRACKMOUSEEVENT	tme;

						if (data == null) {
							data = new HandleData();
							handle_data[0] = data;
						}

						// The current message will be sent out next time around
						data.StoreMessage(ref msg);

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

				case Msg.WM_MOUSELEAVE: {
					prev_mouse_hwnd = IntPtr.Zero;
					msg.message=Msg.WM_MOUSE_LEAVE;
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

		internal override bool SetTopmost(IntPtr hWnd, IntPtr hWndOwner, bool Enabled) {
			if (Enabled) {
				Win32SetWindowPos(hWnd, SetWindowPosZOrder.HWND_TOPMOST, 0, 0, 0, 0, SetWindowPosFlags.SWP_NOMOVE | SetWindowPosFlags.SWP_NOSIZE);
				return true;
			} else {
				Win32SetWindowPos(hWnd, SetWindowPosZOrder.HWND_NOTOPMOST, 0, 0, 0, 0, SetWindowPosFlags.SWP_NOMOVE | SetWindowPosFlags.SWP_NOSIZE);
				return true;
			}
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

		internal override bool SetVisible(IntPtr handle, bool visible) {
			if (visible) {
				Win32ShowWindow(handle, WindowPlacementFlags.SW_SHOWNORMAL);
			} else {
				Win32ShowWindow(handle, WindowPlacementFlags.SW_HIDE);
			}
			return true;
		}

		internal override bool IsVisible(IntPtr handle) {
			WINDOWPLACEMENT	wndpl;

			wndpl = new WINDOWPLACEMENT();
			wndpl.length=(uint)Marshal.SizeOf(wndpl);
			Win32GetWindowPlacement(handle, ref wndpl);
			if ((wndpl.showCmd == WindowPlacementFlags.SW_SHOWMINIMIZED)) {
				return false;
			}
			return true;
		}

		internal override IntPtr SetParent(IntPtr handle, IntPtr parent) {
			return Win32SetParent(handle, parent);
		}

		internal override IntPtr GetParent(IntPtr handle) {
			return Win32GetParent(handle);
		}

		internal override void GrabWindow(IntPtr hWnd, IntPtr ConfineToHwnd) {
			grab_hwnd = hWnd;
			Win32SetCapture(hWnd);
		}

		internal override void GrabInfo(out IntPtr hWnd, out bool GrabConfined, out Rectangle GrabArea) {
			hWnd = grab_hwnd;
			GrabConfined = grab_confined;
			GrabArea = grab_area;
		}

		internal override void ReleaseWindow(IntPtr hWnd) {
			Win32ReleaseCapture();
			grab_hwnd = IntPtr.Zero;
		}

		internal override bool CalculateWindowRect(IntPtr hWnd, ref Rectangle ClientRect, int Style, bool HasMenu, out Rectangle WindowRect) {
			RECT	rect;

			rect.left=ClientRect.Left;
			rect.top=ClientRect.Top;
			rect.right=ClientRect.Right;
			rect.bottom=ClientRect.Bottom;

			if (!Win32AdjustWindowRectEx(ref rect, Style, HasMenu, 0)) {
				WindowRect = new Rectangle(ClientRect.Left, ClientRect.Top, ClientRect.Width, ClientRect.Height);
				return false;
			}

			WindowRect = new Rectangle(rect.left, rect.top, rect.right-rect.left, rect.bottom-rect.top);
			return true;
		}

		internal override void SetCursorPos(IntPtr handle, int x, int y) {
			Win32SetCursorPos(x, y);
		}

		internal override void EnableWindow(IntPtr handle, bool Enable) {
			Win32EnableWindow(handle, Enable);
		}

		internal override void SetModal(IntPtr handle, bool Modal) {
			// we do nothing on Win32; Application.cs simulates modal dialogs by disabling all toplevel windows
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

			Win32SetTimer(FosterParent, index, (uint)timer.Interval, IntPtr.Zero);
		}

		internal override void KillTimer (Timer timer)
		{
			int	index;

			index = timer.GetHashCode();

			Win32KillTimer(FosterParent, index);

			lock (timer_list) {
				timer_list.Remove(index);
			}
		}


		private void CaretCallback(object sender, EventArgs e) {
			Console.WriteLine("CaretCallback hit");
		}

		internal override void CreateCaret(IntPtr hwnd, int width, int height) {
			Win32CreateCaret(hwnd, IntPtr.Zero, width, height);
		}

		internal override void DestroyCaret(IntPtr hwnd) {
			Win32DestroyCaret();
		}

		internal override void SetCaretPos(IntPtr hwnd, int x, int y) {
			Win32SetCaretPos(x, y);
		}

		internal override void CaretVisible(IntPtr hwnd, bool visible) {
			if (visible) {
				Win32ShowCaret(hwnd);
			} else {
				Win32HideCaret(hwnd);
			}
		}

		internal override bool GetFontMetrics(Graphics g, Font font, out int ascent, out int descent) {
			IntPtr		dc;
			TEXTMETRIC	tm;

			tm = new TEXTMETRIC();

			dc = Win32GetDC(IntPtr.Zero);
			Win32SelectObject(dc, font.ToHfont());
			if (Win32GetTextMetrics(dc, ref tm) == false) {
				Win32ReleaseDC(IntPtr.Zero, dc);
				ascent = 0;
				descent = 0;
				return false;
			}
			Win32ReleaseDC(IntPtr.Zero, dc);

			ascent = tm.tmAscent;
			descent = tm.tmDescent;

			return true;
		}

		internal override int KeyboardSpeed {
			get {
				Console.WriteLine ("KeyboardSpeed: need to query Windows");

				//
				// Return values range from 0 to 31 which map to 2.5 to 30 repetitions per second.
				//
				return 0;
			}
		}

		internal override int KeyboardDelay {
			get {
				Console.WriteLine ("KeyboardDelay: need to query Windows");

				//
				// Return values must range from 0 to 4, 0 meaning 250ms,
				// and 4 meaning 1000 ms.
				//
				return 1;
			}
		}
		
		internal override event EventHandler Idle;

		// Santa's little helper
		static void Where() {
			Console.WriteLine("Here: {0}", new StackTrace().ToString());
		}
		#endregion	// Public Static Methods

		#region Win32 Imports
		[DllImport ("kernel32.dll", EntryPoint="GetLastError", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.StdCall)]
		private extern static uint Win32GetLastError();

		[DllImport ("user32.dll", EntryPoint="CreateWindowExA", CallingConvention=CallingConvention.StdCall)]
		internal extern static IntPtr Win32CreateWindow(uint dwExStyle, string lpClassName, string lpWindowName, uint dwStyle, int x, int y, int nWidth, int nHeight, IntPtr hWndParent, IntPtr hMenu, IntPtr hInstance, IntPtr lParam);

		[DllImport ("user32.dll", EntryPoint="DestroyWindow", CallingConvention=CallingConvention.StdCall)]
		internal extern static bool Win32DestroyWindow(IntPtr hWnd);

		[DllImport ("user32.dll", EntryPoint="PeekMessageA", CallingConvention=CallingConvention.StdCall)]
		internal extern static bool Win32PeekMessage(ref MSG msg, IntPtr hWnd, int wFilterMin, int wFilterMax, uint flags);

		[DllImport ("user32.dll", EntryPoint="GetMessageA", CallingConvention=CallingConvention.StdCall)]
		internal extern static bool Win32GetMessage(ref MSG msg, IntPtr hWnd, int wFilterMin, int wFilterMax);

		[DllImport ("user32.dll", EntryPoint="TranslateMessage", CallingConvention=CallingConvention.StdCall)]
		internal extern static bool Win32TranslateMessage(ref MSG msg);

		[DllImport ("user32.dll", EntryPoint="DispatchMessageA", CallingConvention=CallingConvention.StdCall)]
		internal extern static IntPtr Win32DispatchMessage(ref MSG msg);

		[DllImport ("user32.dll", EntryPoint="MoveWindow", CallingConvention=CallingConvention.StdCall)]
		internal extern static bool Win32MoveWindow(IntPtr hWnd, int x, int y, int width, int height, bool repaint);

		[DllImport ("user32.dll", EntryPoint="SetWindowPos", CallingConvention=CallingConvention.StdCall)]
		internal extern static bool Win32SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, SetWindowPosFlags Flags);

		[DllImport ("user32.dll", EntryPoint="SetWindowPos", CallingConvention=CallingConvention.StdCall)]
		internal extern static bool Win32SetWindowPos(IntPtr hWnd, SetWindowPosZOrder pos, int x, int y, int cx, int cy, SetWindowPosFlags Flags);

		[DllImport ("user32.dll", EntryPoint="SetWindowTextA", CallingConvention=CallingConvention.StdCall)]
		internal extern static bool Win32SetWindowText(IntPtr hWnd, string lpString);

		[DllImport ("user32.dll", EntryPoint="GetWindowTextA", CallingConvention=CallingConvention.StdCall)]
		internal extern static bool Win32GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

		[DllImport ("user32.dll", EntryPoint="SetParent", CallingConvention=CallingConvention.StdCall)]
		internal extern static IntPtr Win32SetParent(IntPtr hWnd, IntPtr hParent);

		[DllImport ("user32.dll", EntryPoint="RegisterClassA", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.StdCall)]
		private extern static bool Win32RegisterClass(ref WNDCLASS wndClass);

		[DllImport ("user32.dll", EntryPoint="LoadCursorA", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.StdCall)]
		private extern static IntPtr Win32LoadCursor(IntPtr hInstance, LoadCursorType type);

		[DllImport ("user32.dll", EntryPoint="DefWindowProcA", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.StdCall)]
		private extern static IntPtr Win32DefWindowProc(IntPtr hWnd, Msg Msg, IntPtr wParam, IntPtr lParam);

		[DllImport ("user32.dll", EntryPoint="DefDlgProcA", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.StdCall)]
		private extern static IntPtr Win32DefDlgProc(IntPtr hWnd, Msg Msg, IntPtr wParam, IntPtr lParam);

		[DllImport ("user32.dll", EntryPoint="PostQuitMessage", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.StdCall)]
		private extern static IntPtr Win32PostQuitMessage(int nExitCode);

		[DllImport ("user32.dll", EntryPoint="UpdateWindow", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.StdCall)]
		private extern static IntPtr Win32UpdateWindow(IntPtr hWnd);

		[DllImport ("user32.dll", EntryPoint="GetUpdateRect", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.StdCall)]
		private extern static bool Win32GetUpdateRect(IntPtr hWnd, ref RECT rect, bool erase);

		[DllImport ("user32.dll", EntryPoint="BeginPaint", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.StdCall)]
		private extern static IntPtr Win32BeginPaint(IntPtr hWnd, ref PAINTSTRUCT ps);

		[DllImport ("user32.dll", EntryPoint="EndPaint", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.StdCall)]
		private extern static bool Win32EndPaint(IntPtr hWnd, ref PAINTSTRUCT ps);

		[DllImport ("user32.dll", EntryPoint="GetDC", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.StdCall)]
		private extern static IntPtr Win32GetDC(IntPtr hWnd);

		[DllImport ("user32.dll", EntryPoint="ReleaseDC", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.StdCall)]
		private extern static IntPtr Win32ReleaseDC(IntPtr hWnd, IntPtr hDC);

		[DllImport ("user32.dll", EntryPoint="MessageBoxA", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.StdCall)]
		private extern static IntPtr Win32MessageBox(IntPtr hParent, string pText, string pCaption, uint uType);

		[DllImport ("user32.dll", EntryPoint="InvalidateRect", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.StdCall)]
		private extern static IntPtr Win32InvalidateRect(IntPtr hWnd, ref RECT lpRect, bool bErase);

		[DllImport ("user32.dll", EntryPoint="InvalidateRect", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.StdCall)]
		private extern static IntPtr Win32InvalidateRect(IntPtr hWnd, IntPtr lpRect, bool bErase);

		[DllImport ("user32.dll", EntryPoint="SetCapture", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.StdCall)]
		private extern static IntPtr Win32SetCapture(IntPtr hWnd);

		[DllImport ("user32.dll", EntryPoint="ReleaseCapture", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.StdCall)]
		private extern static IntPtr Win32ReleaseCapture();

		[DllImport ("user32.dll", EntryPoint="GetWindowRect", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.StdCall)]
		private extern static IntPtr Win32GetWindowRect(IntPtr hWnd, out RECT rect);

		[DllImport ("user32.dll", EntryPoint="GetClientRect", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.StdCall)]
		private extern static IntPtr Win32GetClientRect(IntPtr hWnd, out RECT rect);

		[DllImport ("user32.dll", EntryPoint="ScreenToClient", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.StdCall)]
		private extern static bool Win32ScreenToClient(IntPtr hWnd, ref POINT pt);

		[DllImport ("user32.dll", EntryPoint="ClientToScreen", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.StdCall)]
		private extern static bool Win32ClientToScreen(IntPtr hWnd, ref POINT pt);

		[DllImport ("user32.dll", EntryPoint="GetParent", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.StdCall)]
		private extern static IntPtr Win32GetParent(IntPtr hWnd);

		[DllImport ("user32.dll", EntryPoint="SetActiveWindow", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.StdCall)]
		private extern static IntPtr Win32SetActiveWindow(IntPtr hWnd);

		[DllImport ("user32.dll", EntryPoint="PostQuitMessage", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.StdCall)]
		private extern static IntPtr Win32PostQuitMessage(IntPtr hWnd);

		[DllImport ("user32.dll", EntryPoint="AdjustWindowRectEx", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.StdCall)]
		private extern static bool Win32AdjustWindowRectEx(ref RECT lpRect, int dwStyle, bool bMenu, int dwExStyle);

		[DllImport ("user32.dll", EntryPoint="GetCursorPos", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.StdCall)]
		private extern static bool Win32GetCursorPos(out POINT lpPoint);

		[DllImport ("user32.dll", EntryPoint="SetCursorPos", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.StdCall)]
		private extern static bool Win32SetCursorPos(int x, int y);

		[DllImport ("user32.dll", EntryPoint="GetWindowPlacement", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.StdCall)]
		private extern static bool Win32GetWindowPlacement(IntPtr hWnd, ref WINDOWPLACEMENT lpwndpl);

		[DllImport ("user32.dll", EntryPoint="TrackMouseEvent", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.StdCall)]
		private extern static bool Win32TrackMouseEvent(ref TRACKMOUSEEVENT tme);

		[DllImport ("gdi32.dll", EntryPoint="CreateBrushIndirect", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.StdCall)]
		private extern static IntPtr Win32CreateBrushIndirect(ref LOGBRUSH lb);

		[DllImport ("user32.dll", EntryPoint="FillRect", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.StdCall)]
		private extern static int Win32FillRect(IntPtr hdc, ref RECT rect, IntPtr hbr);

		[DllImport ("user32.dll", EntryPoint="SetWindowLong", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.StdCall)]
		private extern static IntPtr Win32SetWindowLong(IntPtr hwnd, WindowLong index, IntPtr value);

		[DllImport ("user32.dll", EntryPoint="GetWindowLong", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.StdCall)]
		private extern static IntPtr Win32GetWindowLong(IntPtr hwnd, WindowLong index);

		[DllImport ("gdi32.dll", EntryPoint="DeleteObject", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.StdCall)]
		private extern static bool Win32DeleteObject(IntPtr o);

		[DllImport ("user32.dll", EntryPoint="PostMessage", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.StdCall)]
		private extern static bool Win32PostMessage(IntPtr hwnd, Msg msg, IntPtr wParam, IntPtr lParam);

		[DllImport ("user32.dll", EntryPoint="GetKeyState", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.StdCall)]
		private extern static short Win32GetKeyState(VirtualKeys nVirtKey);

		[DllImport ("user32.dll", EntryPoint="GetDesktopWindow", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.StdCall)]
		private extern static IntPtr Win32GetDesktopWindow();

		[DllImport ("user32.dll", EntryPoint="SetTimer", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.StdCall)]
		private extern static IntPtr Win32SetTimer(IntPtr hwnd, int nIDEvent, uint uElapse, IntPtr timerProc);

		[DllImport ("user32.dll", EntryPoint="KillTimer", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.StdCall)]
		private extern static IntPtr Win32KillTimer(IntPtr hwnd, int nIDEvent);

		[DllImport ("user32.dll", EntryPoint="ShowWindow", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.StdCall)]
		private extern static IntPtr Win32ShowWindow(IntPtr hwnd, WindowPlacementFlags nCmdShow);

		[DllImport ("user32.dll", EntryPoint="EnableWindow", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.StdCall)]
		private extern static IntPtr Win32EnableWindow(IntPtr hwnd, bool Enabled);

		[DllImport ("user32.dll", EntryPoint="SetFocus", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.StdCall)]
		internal extern static IntPtr Win32SetFocus(IntPtr hwnd);

		[DllImport ("user32.dll", EntryPoint="CreateCaret", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.StdCall)]
		internal extern static bool Win32CreateCaret(IntPtr hwnd, IntPtr hBitmap, int nWidth, int nHeight);

		[DllImport ("user32.dll", EntryPoint="DestroyCaret", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.StdCall)]
		internal extern static bool Win32DestroyCaret();

		[DllImport ("user32.dll", EntryPoint="ShowCaret", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.StdCall)]
		internal extern static bool Win32ShowCaret(IntPtr hwnd);

		[DllImport ("user32.dll", EntryPoint="HideCaret", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.StdCall)]
		internal extern static bool Win32HideCaret(IntPtr hwnd);

		[DllImport ("user32.dll", EntryPoint="SetCaretPos", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.StdCall)]
		internal extern static bool Win32SetCaretPos(int X, int Y);

		[DllImport ("user32.dll", EntryPoint="GetCaretBlinkTime", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.StdCall)]
		internal extern static uint Win32GetCaretBlinkTime();

		[DllImport ("gdi32.dll", EntryPoint="GetTextMetricsA", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.StdCall)]
		internal extern static bool Win32GetTextMetrics(IntPtr hdc, ref TEXTMETRIC tm);

		[DllImport ("gdi32.dll", EntryPoint="SelectObject", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.StdCall)]
		internal extern static bool Win32SelectObject(IntPtr hdc, IntPtr hgdiobject);
		#endregion
	}
}
