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
// $Revision: 1.9 $
// $Modtime: $
// $Log: XplatUIX11.cs,v $
// Revision 1.9  2004/08/08 21:08:10  jordi
// fixes keyboard crash
//
// Revision 1.8  2004/08/06 23:46:56  pbartok
// - Implemented GetParent
//
// Revision 1.7  2004/08/06 23:17:44  pbartok
// - Fixed Refresh and Invalidate
//
// Revision 1.6  2004/08/06 21:30:56  pbartok
// - Fixed recursive loop when resizing
// - Improved/fixed redrawing on expose messages
//
// Revision 1.5  2004/08/06 15:53:39  jordi
// X11 keyboard navigation
//
// Revision 1.4  2004/08/06 14:02:33  pbartok
// - Fixed reparenting
// - Fixed window border creation
//
// Revision 1.3  2004/08/05 21:38:02  pbartok
// - Attempted fix for reparenting problems
//
// Revision 1.2  2004/08/04 20:11:24  pbartok
// - Added Invalidate handling
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

/// X11 Version
namespace System.Windows.Forms {
	public class XplatUIX11 : XplatUIDriver {
		#region Local Variables
		private static XplatUIX11	instance;
		private static int		ref_count;

		private static IntPtr		DisplayHandle;		// X11 handle to display
		private static IntPtr		FosterParent;		// Container to hold child windows until their parent exists
		internal static Keys		key_state;
		internal static MouseButtons	mouse_state;
		internal static Point		mouse_position;
		internal static Rectangle	paint_area;
		internal static	bool		is_visible;
		#endregion	// Local Variables

		internal override Color BackColor {
			get {
				return Color.DarkCyan;
			}
		}

		internal override Color ForeColor {
			get {
				return Color.White;
			}
		}

		internal override Font Font {
			get {
				return new Font("Arial", 12);
			}
		}

		internal override Keys ModifierKeys {
			get {
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
					throw new NotImplementedException("Need to figure out D'n'D for X11");
				}
			}
		}

		#region Constructor & Destructor
		private XplatUIX11() {
			// Handle singleton stuff first
			ref_count=0;

			// Now regular initialization
			DisplayHandle=XOpenDisplay(IntPtr.Zero);
			key_state=Keys.None;
			mouse_state=MouseButtons.None;
			mouse_position=Point.Empty;
			paint_area=new Rectangle(0, 0, 0, 0);

			// Create the foster parent
			FosterParent=XCreateSimpleWindow(DisplayHandle, XRootWindow(DisplayHandle, 0), 0, 0, 1, 1, 4, 0, 0);
			if (FosterParent==IntPtr.Zero) {
				Console.WriteLine("XplatUIX11 Constructor failed to create FosterParent");
			}
		}

		~XplatUIX11() {
			if (DisplayHandle!=IntPtr.Zero) {
				XCloseDisplay(DisplayHandle);
				DisplayHandle=IntPtr.Zero;
			}
		}
		#endregion	// Constructor & Destructor

		#region Singleton Specific Code
		public static XplatUIX11 GetInstance() {
			if (instance==null) {
				instance=new XplatUIX11();
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
			if (DisplayHandle==IntPtr.Zero) {
				DisplayHandle=XOpenDisplay(IntPtr.Zero);
				key_state=Keys.None;
				mouse_state=MouseButtons.None;
				mouse_position=Point.Empty;
			}

			return IntPtr.Zero;
		}

		internal override void ShutdownDriver(IntPtr token) {
			if (DisplayHandle!=IntPtr.Zero) {
				XCloseDisplay(DisplayHandle);
				DisplayHandle=IntPtr.Zero;
			}
		}


		internal void Version() {
			Console.WriteLine("Xplat version $revision: $");
		}

		internal override void Exit() {
			Console.WriteLine("XplatUIX11.Exit");
		}

		internal override IntPtr CreateWindow(CreateParams cp) {
			IntPtr	WindowHandle;
			IntPtr	ParentHandle;
			int	X;
			int	Y;
			int	Width;
			int	Height;
			int	BorderWidth;

			ParentHandle=cp.Parent;

			X=cp.X;
			Y=cp.Y;
			Width=cp.Width;
			Height=cp.Height;
			BorderWidth=0;

			if (X<1) X=50;
			if (Y<1) Y=50;
			if (Width<1) Width=100;
			if (Height<1) Height=100;

			if (ParentHandle==IntPtr.Zero) {
				if ((cp.Style & (int)WindowStyles.WS_CHILD)!=0) {
					// We need to use our foster parent window until this poor child gets it's parent assigned
					ParentHandle=FosterParent;
				} else {
					BorderWidth=4;
					ParentHandle=XRootWindow(DisplayHandle, 0);
				}
			}

			WindowHandle=XCreateSimpleWindow(DisplayHandle, ParentHandle, X, Y, Width, Height, BorderWidth, 0, 0);
			XMapWindow(DisplayHandle, WindowHandle);

			XSelectInput(DisplayHandle, WindowHandle, 
				EventMask.ButtonPressMask | 
				EventMask.ButtonReleaseMask | 
				EventMask.KeyPressMask | 
				EventMask.KeyReleaseMask | 
				EventMask.EnterWindowMask | 
				EventMask.LeaveWindowMask |
				EventMask.ExposureMask |
				EventMask.PointerMotionMask | 
				EventMask.ResizeRedirectMask |
				EventMask.VisibilityChangeMask |
				EventMask.StructureNotifyMask);
			XSetWindowBackground(DisplayHandle, WindowHandle, (uint)this.BackColor.ToArgb());
			is_visible=true;
			return(WindowHandle);
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
			XDestroyWindow(DisplayHandle, handle);
			return;
		}

		internal override void RefreshWindow(IntPtr handle) {
			XWindowAttributes	attributes=new XWindowAttributes();
			XEvent			xevent = new XEvent();

			// We need info about our window to generate the expose
			XGetWindowAttributes(DisplayHandle, handle, ref attributes);

			xevent.type=XEventName.Expose;
			xevent.ExposeEvent.display=DisplayHandle;
			xevent.ExposeEvent.window=handle;
			xevent.ExposeEvent.x=0;
			xevent.ExposeEvent.y=0;
			xevent.ExposeEvent.width=attributes.width;
			xevent.ExposeEvent.height=attributes.height;

			XSendEvent(DisplayHandle, handle, false, EventMask.ExposureMask, ref xevent);
			XFlush(DisplayHandle);
		}

		[MonoTODO("Add support for internal table of windows/DCs for looking up paint area and cleanup")]
		internal override PaintEventArgs PaintEventStart(IntPtr handle) {
			PaintEventArgs	paint_event;
			Rectangle	update_area;

			// FIXME: Assign proper values
			update_area = new Rectangle();
			paint_event = new PaintEventArgs(Graphics.FromHwnd(handle), update_area);

			return paint_event;
		}

		internal override void PaintEventEnd(IntPtr handle) {
			// FIXME: Lookup in the internal list how to clean
			;
		}

		internal override void SetWindowPos(IntPtr handle, Rectangle rc) {
			SetWindowPos(handle, rc.X, rc.Y, rc.Width, rc.Height);
			return;
		}

		internal override bool MoveWindow(IntPtr hWnd, int x, int y, int width, int height) {
			XMoveResizeWindow(DisplayHandle, hWnd, x, y, width, height);
			return true;
		}

		internal override void SetWindowPos(IntPtr handle, int x, int y, int width, int height) {
			XMoveResizeWindow(DisplayHandle, handle, x, y, width, height);
			return;
		}

		internal override void Activate(IntPtr handle) {
			Console.WriteLine("XplatUIX11.Activate");
			return;
		}

		internal override void Invalidate(IntPtr handle, Rectangle rc, bool clear) {
			XEvent			xevent = new XEvent();

			xevent.type=XEventName.Expose;
			xevent.ExposeEvent.display=DisplayHandle;
			xevent.ExposeEvent.window=handle;
			xevent.ExposeEvent.count=0;

			if (clear) {
				// Need to clear the whole window, so we force a redraw for the whole window
				XWindowAttributes	attributes=new XWindowAttributes();

				// We need info about our window to generate the expose
				XGetWindowAttributes(DisplayHandle, handle, ref attributes);

				xevent.ExposeEvent.x=0;
				xevent.ExposeEvent.y=0;
				xevent.ExposeEvent.width=attributes.width;
				xevent.ExposeEvent.height=attributes.height;
			} else {
				xevent.ExposeEvent.x=rc.Left;
				xevent.ExposeEvent.y=rc.Top;
				xevent.ExposeEvent.width=rc.Width;
				xevent.ExposeEvent.height=rc.Height;
			}

			XSendEvent(DisplayHandle, handle, false, EventMask.ExposureMask, ref xevent);
			// Flush is not needed, invalidate does not guarantee an immediate effect
			//XFlush(DisplayHandle);
			return;
		}

		internal override IntPtr DefWndProc(ref Message msg) {
#if not
			switch (msg.Msg) {
				case (int)Msg.WM_PAINT: {
					IntPtr	gc;

					if (msg.Hwnd!=IntPtr.Zero) {
						gc=XCreateGC(DisplayHandle, msg.Hwnd, 0, IntPtr.Zero);
						XSetBackground(DisplayHandle, gc, this.BackColor.ToArgb());
						XFreeGC(DisplayHandle, gc);
					}
					break;
				}
			}
#endif

#if debug
			Console.WriteLine("XplatUIX11.DefWndProc");
#endif
			return IntPtr.Zero;
		}

		internal override void HandleException(Exception e) {
			StackTrace st = new StackTrace(e);
			Console.WriteLine("Exception '{0}'", e.Message+st.ToString());
			Console.WriteLine("{0}{1}", e.Message, st.ToString());
		}

		internal override void DoEvents() {
			Console.WriteLine("XplatUIX11.DoEvents");
		}

		internal override bool PeekMessage(ref MSG msg, IntPtr hWnd, int wFilterMin, int wFilterMax, uint flags) {
			Console.WriteLine("XplatUIX11.PeekMessage");
			return true;
		}

		private void CreateKeyBoardMsg (XEvent xevent, ref MSG msg)
		{
			IntPtr	buffer = Marshal.AllocHGlobal(24);
			XKeySym	keysym;
			string	keys;
			int	len;
			msg.wParam = IntPtr.Zero;
			
			len = XLookupString(ref xevent, buffer, 24, out keysym, IntPtr.Zero);

			if (len>0) /* String is not zero terminated*/
				Marshal.WriteByte (buffer, len, 0);

			keys=Marshal.PtrToStringAuto(buffer);

			for (int i = 0; i < KeyMapping.Length; i++) {
				if (KeyMapping[i].X11Key == keysym) {
					msg.wParam = (IntPtr) KeyMapping[i].Win32Key;
					Console.WriteLine("Got special key {0} {1:x} ", keysym, keysym);
					break;
				}							
			}

			if (msg.wParam == IntPtr.Zero) {
				char[] keychars;				
				keychars=keys.ToCharArray(0, 1);
				msg.wParam=(IntPtr)keychars[0];
				Console.WriteLine("Got key {0} {1:x} ", keysym, keysym);
			}

			Marshal.FreeHGlobal (buffer);
			msg.lParam = (IntPtr) 1;
		}

		internal override bool GetMessage(ref MSG msg, IntPtr hWnd, int wFilterMin, int wFilterMax) {
			XEvent	xevent = new XEvent();

			XNextEvent(DisplayHandle, ref xevent);
			msg.hwnd=xevent.AnyEvent.window;

			switch(xevent.type) {
				case XEventName.KeyPress: {
					msg.message = Msg.WM_KEYDOWN;
					CreateKeyBoardMsg (xevent, ref msg);
					break;
				}

				case XEventName.KeyRelease: {
					msg.message=Msg.WM_KEYUP;
					CreateKeyBoardMsg (xevent, ref msg);
					break;
				}

				case XEventName.ButtonPress: {
					msg.message=Msg.WM_LBUTTONDOWN;
					msg.lParam=(IntPtr) (xevent.ButtonEvent.y << 16 | xevent.ButtonEvent.x);
					break;
				}

				case XEventName.ButtonRelease: {
					msg.message=Msg.WM_LBUTTONUP;
					msg.lParam=(IntPtr) (xevent.ButtonEvent.y << 16 | xevent.ButtonEvent.x);
					break;
				}

				case XEventName.MotionNotify: {
					msg.message=Msg.WM_MOUSEMOVE;
					msg.lParam=(IntPtr) (xevent.ButtonEvent.y << 16 | xevent.ButtonEvent.x);
					break;
				}

				case XEventName.Expose: {
					msg.message=Msg.WM_PAINT;
					paint_area.X=xevent.ExposeEvent.x;
					paint_area.Y=xevent.ExposeEvent.y;
					paint_area.Width=xevent.ExposeEvent.width;
					paint_area.Height=xevent.ExposeEvent.height;
					break;
				}

				case XEventName.ResizeRequest: {
					msg.message=Msg.WM_SIZE;
					msg.wParam=IntPtr.Zero;
					msg.lParam=(IntPtr) (xevent.ResizeRequestEvent.width<<16 | xevent.ResizeRequestEvent.width);
					break;
				}
			}

			NativeWindow.WndProc(msg.hwnd, msg.message, msg.wParam, msg.lParam);

			return true;
		}

		internal override bool TranslateMessage(ref MSG msg) {
//			Console.WriteLine("XplatUIX11 TranslateMessage");
			return true;
		}

		internal override bool DispatchMessage(ref MSG msg) {
//			Console.WriteLine("XplatUIX11 DispatchMessage");
			return true;
		}

		internal override bool Text(IntPtr handle, string text) {
#if notdef
			XTextProperty	property = new XTextProperty();

			property.encoding=
			XSetWMName(DisplayHandle, handle, ref property);
#else
			XStoreName(DisplayHandle, handle, text);
#endif
			return true;
		}

		internal override bool SetVisible(IntPtr handle, bool visible) {
			if (visible) {
				XMapWindow(DisplayHandle, handle);
				is_visible=true;
			} else {
				XUnmapWindow(DisplayHandle, handle);
				is_visible=false;
			}
			Console.WriteLine("Setting window visibility: {0}", visible);
			return true;
		}

		internal override bool IsVisible(IntPtr handle) {
			return is_visible;
		}

		internal override IntPtr SetParent(IntPtr handle, IntPtr parent) {
			XWindowAttributes	attributes=new XWindowAttributes();

			XGetWindowAttributes(DisplayHandle, handle, ref attributes);
			XReparentWindow(DisplayHandle, handle, parent, attributes.x, attributes.y);
			return IntPtr.Zero;
		}

		internal override IntPtr GetParent(IntPtr handle) {
			IntPtr	Root;
			IntPtr	Parent;
			IntPtr	Children;
			int	ChildCount;

			Root=IntPtr.Zero;
			Parent=IntPtr.Zero;
			Children=IntPtr.Zero;
			ChildCount=0;

			XQueryTree(DisplayHandle, handle, ref Root, ref Parent, ref Children, ref ChildCount);

			if (Children!=IntPtr.Zero) {
				XFree(Children);
			}
			return Parent;
		}

		// Santa's little helper
		static void Where() {
			Console.WriteLine("Here: {0}", new StackTrace().ToString());
		}

		internal override void Run() {
			XEvent	xevent = new XEvent();

			//Console.WriteLine("Size of XEvent: {0}", Marshal.SizeOf(typeof(XEvent)));
			//Where();
			//XNextEvent(DisplayHandle, ref xevent);
			//Where();

			while (true==true) {
				XNextEvent(DisplayHandle, ref xevent);

				switch(xevent.type) {
					case XEventName.KeyPress: {
						IntPtr	buffer = Marshal.AllocHGlobal(24);
						XKeySym	keysym;
						string	keys;
						int	len;

						len=XLookupString(ref xevent, buffer, 24, out keysym, IntPtr.Zero);
						if (len>0) {
							keys=Marshal.PtrToStringAuto(buffer);
							Console.WriteLine("Got char {0}", keys);
						} else {
							Console.WriteLine("Got special key {0}", keysym);
						}
						Marshal.FreeHGlobal(buffer);
						break;
					}

					case XEventName.Expose: {
						Rectangle	r = new Rectangle(xevent.ExposeEvent.x, xevent.ExposeEvent.y, xevent.ExposeEvent.width, xevent.ExposeEvent.height);
						Graphics	g = Graphics.FromHwnd (xevent.ExposeEvent.window );
						Font		f = new Font("Bitstream Vera Sans Mono", 20);
						SolidBrush	b = new SolidBrush(Color.Red);
						Rectangle	r2 = new Rectangle(0, 0, 300, 300);

						g.FillRectangle(SystemBrushes.Window, r2);
						g.DrawString("TestString", f, b, 0, 0);

						Console.WriteLine("XplatUI.Run(): Exposed {0}", r);
 						break;
					}

					case XEventName.ButtonPress: {
						Console.WriteLine("XplatUI.Run(): leaving loop");
						return;
					}

					default: {
						Console.WriteLine("Received event {0}", xevent.type);
						break;
					}
				}
			}
		}
		#endregion	// Public Static Methods

		internal struct X11ToWin32KeyMapping {
			internal XKeySym			X11Key;
			internal XplatUIWin32.VirtualKeys	Win32Key;	

			internal X11ToWin32KeyMapping (XKeySym x11, XplatUIWin32.VirtualKeys win32)
			{
				X11Key = x11;
				Win32Key = win32;
			} 
		}

		/* X11 to Win32 VK mapping */
		static readonly X11ToWin32KeyMapping[] KeyMapping = new X11ToWin32KeyMapping[] 
		{
			/* Cursor navigation*/
			new X11ToWin32KeyMapping (XKeySym.XK_Left, XplatUIWin32.VirtualKeys.VK_LEFT),
			new X11ToWin32KeyMapping (XKeySym.XK_Right, XplatUIWin32.VirtualKeys.VK_RIGHT),
			new X11ToWin32KeyMapping (XKeySym.XK_Up, XplatUIWin32.VirtualKeys.VK_UP),
			new X11ToWin32KeyMapping (XKeySym.XK_Down, XplatUIWin32.VirtualKeys.VK_DOWN),

			new X11ToWin32KeyMapping (XKeySym.XK_Page_Up, XplatUIWin32.VirtualKeys.VK_PRIOR),
			new X11ToWin32KeyMapping (XKeySym.XK_Page_Down, XplatUIWin32.VirtualKeys.VK_NEXT),
			new X11ToWin32KeyMapping (XKeySym.XK_End, XplatUIWin32.VirtualKeys.VK_END),
			new X11ToWin32KeyMapping (XKeySym.XK_Home, XplatUIWin32.VirtualKeys.VK_HOME),

			/* Modifiers*/
			new X11ToWin32KeyMapping (XKeySym.XK_Shift_R, XplatUIWin32.VirtualKeys.VK_SHIFT),
			new X11ToWin32KeyMapping (XKeySym.XK_Shift_L, XplatUIWin32.VirtualKeys.VK_SHIFT),
			new X11ToWin32KeyMapping (XKeySym.XK_Control_R, XplatUIWin32.VirtualKeys.VK_CONTROL),
			new X11ToWin32KeyMapping (XKeySym.XK_Control_L, XplatUIWin32.VirtualKeys.VK_CONTROL),			

			/* Others */
			new X11ToWin32KeyMapping (XKeySym.XK_Return, XplatUIWin32.VirtualKeys.VK_RETURN),
			new X11ToWin32KeyMapping (XKeySym.XK_Tab, XplatUIWin32.VirtualKeys.VK_TAB),
			new X11ToWin32KeyMapping (XKeySym.XK_Menu, XplatUIWin32.VirtualKeys.VK_MENU),
			new X11ToWin32KeyMapping (XKeySym.XK_BackSpace, XplatUIWin32.VirtualKeys.VK_BACK),
			new X11ToWin32KeyMapping (XKeySym.XK_Clear, XplatUIWin32.VirtualKeys.VK_CLEAR),
			
		};

		

		#region X11 Imports
		[DllImport ("libX11.so", EntryPoint="XOpenDisplay")]
		internal extern static IntPtr XOpenDisplay(IntPtr display);
		[DllImport ("libX11.so", EntryPoint="XCloseDisplay")]
		internal extern static void XCloseDisplay(IntPtr display);						    

		[DllImport ("libX11.so", EntryPoint="XCreateWindow")]
		internal extern static IntPtr XCreateWindow(IntPtr display, IntPtr parent, int x, int y, int width, int height, int border_width, int depth, int xclass, IntPtr visual, IntPtr attributes);
		[DllImport ("libX11.so", EntryPoint="XCreateSimpleWindow")]
		internal extern static IntPtr XCreateSimpleWindow(IntPtr display, IntPtr parent, int x, int y, int width, int height, int border_width, int border, int background);
		[DllImport ("libX11.so", EntryPoint="XMapWindow")]
		internal extern static int XMapWindow(IntPtr display, IntPtr window);
		[DllImport ("libX11.so", EntryPoint="XUnmapWindow")]
		internal extern static int XUnmapWindow(IntPtr display, IntPtr window);
		[DllImport ("libX11.so", EntryPoint="XRootWindow")]
		internal extern static IntPtr XRootWindow(IntPtr display, int screen_number);
		[DllImport ("libX11.so", EntryPoint="XNextEvent")]
		internal extern static IntPtr XNextEvent(IntPtr display, ref XEvent xevent);
		[DllImport ("libX11.so", EntryPoint="XSelectInput")]
		internal extern static IntPtr XSelectInput(IntPtr display, IntPtr window, EventMask mask);
		[DllImport ("libX11.so", EntryPoint="XLookupString")]
		internal extern static int XLookupString(ref XEvent xevent, IntPtr buffer, int num_bytes, out XKeySym keysym, IntPtr status);

		[DllImport ("libX11.so", EntryPoint="XDestroyWindow")]
		internal extern static int XDestroyWindow(IntPtr display, IntPtr window);

		[DllImport ("libX11.so", EntryPoint="XReparentWindow")]
		internal extern static int XReparentWindow(IntPtr display, IntPtr window, IntPtr parent, int x, int y);
		[DllImport ("libX11.so", EntryPoint="XMoveResizeWindow")]
		internal extern static int XMoveResizeWindow(IntPtr display, IntPtr window, int x, int y, int width, int height);

		[DllImport ("libX11.so", EntryPoint="XGetWindowAttributes")]
		internal extern static int XGetWindowAttributes(IntPtr display, IntPtr window, ref XWindowAttributes attributes);

		[DllImport ("libX11.so", EntryPoint="XFlush")]
		internal extern static int XFlush(IntPtr display);

		[DllImport ("libX11.so", EntryPoint="XSetWMName")]
		internal extern static int XSetWMName(IntPtr display, IntPtr window, ref XTextProperty text_prop);

		[DllImport ("libX11.so", EntryPoint="XStoreName")]
		internal extern static int XStoreName(IntPtr display, IntPtr window, string window_name);

		[DllImport ("libX11.so", EntryPoint="XSendEvent")]
		internal extern static int XSendEvent(IntPtr display, IntPtr window, bool propagate, EventMask event_mask, ref XEvent send_event);

		[DllImport ("libX11.so", EntryPoint="XQueryTree")]
		internal extern static int XQueryTree(IntPtr display, IntPtr window, ref IntPtr root_return, ref IntPtr parent_return, ref IntPtr children_return, ref int nchildren_return);

		[DllImport ("libX11.so", EntryPoint="XFree")]
		internal extern static int XFree(IntPtr data);

		// Drawing
		[DllImport ("libX11.so", EntryPoint="XCreateGC")]
		internal extern static IntPtr XCreateGC(IntPtr display, IntPtr window, int valuemask, IntPtr values);
		[DllImport ("libX11.so", EntryPoint="XFreeGC")]
		internal extern static int XFreeGC(IntPtr display, IntPtr gc);
		[DllImport ("libX11.so", EntryPoint="XSetWindowBackground")]
		internal extern static int XSetWindowBackground(IntPtr display, IntPtr window, uint background);
		#endregion

	}
}
