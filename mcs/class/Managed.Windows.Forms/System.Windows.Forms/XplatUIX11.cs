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

// NOT COMPLETE

using System;
using System.Threading;
using System.Drawing;
using System.ComponentModel;
using System.Collections;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Net;
using System.Net.Sockets;

// Only do the poll when building with mono for now
#if __MonoCS__
using Mono.Unix;
#endif

/// X11 Version
namespace System.Windows.Forms {
	internal class XplatUIX11 : XplatUIDriver {
		#region Local Variables
		private static XplatUIX11	instance;
		private static int		ref_count;
		private static bool		themes_enabled;

		private static IntPtr		DisplayHandle;		// X11 handle to display
		private static int		screen_num;		// Screen number used
		private static IntPtr		root_window;		// Handle of the root window for the screen/display
		private static IntPtr		FosterParent;		// Container to hold child windows until their parent exists
		private static int		wm_protocols;		// X Atom
		private static int		wm_delete_window;	// X Atom
		private static int		mwm_hints;		// X Atom
		private static int		wm_no_taskbar;		// X Atom
		private static int		wm_state_above;		// X Atom
		private static int		atom;			// X Atom
		private static int		net_wm_state;		// X Atom
		private static IntPtr		async_method;
		private static uint		default_colormap;	// X Colormap ID
		internal static Keys		key_state;
		internal static MouseButtons	mouse_state;
		internal static Point		mouse_position;
		internal static bool		grab_confined;		// Is the current grab (if any) confined to grab_area?
		internal static IntPtr		grab_hwnd;		// The window that is grabbed
		internal static Rectangle	grab_area;		// The area the current grab is confined to
		internal static IntPtr		click_pending_hwnd;	// 
		internal static Msg		click_pending_message;	// 
		internal static IntPtr		click_pending_lparam;	// 
		internal static IntPtr		click_pending_wparam;	// 
		internal static int		click_pending_time;	// Last time we received a mouse click
		internal static bool		click_pending;		// True if we haven't sent the last mouse click
		internal static int		double_click_interval;	// in milliseconds, how fast one has to click for a double click

		private static Hashtable	handle_data;
		private XEventQueue		message_queue;

		private ArrayList timer_list;
		private Thread timer_thread;
		private AutoResetEvent timer_wait;
		private Socket listen;
		private Socket wake;

#if __MonoCS__
		private Pollfd [] pollfds;
#endif

		private object xlib_lock = new object ();

		private static readonly EventMask  SelectInputMask = EventMask.ButtonPressMask | 
				EventMask.ButtonReleaseMask | 
				EventMask.KeyPressMask | 
				EventMask.KeyReleaseMask | 
				EventMask.EnterWindowMask | 
				EventMask.LeaveWindowMask |
				EventMask.ExposureMask |
				EventMask.PointerMotionMask | 
				EventMask.VisibilityChangeMask |
				EventMask.StructureNotifyMask;

		#endregion	// Local Variables

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
                // This is always called from a locked context
		private XplatUIX11() {
			// Handle singleton stuff first
			ref_count=0;

			message_queue = new XEventQueue ();
			timer_list = new ArrayList ();

			// Now regular initialization
			SetDisplay(XOpenDisplay(IntPtr.Zero));

			listen = new Socket (AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
			IPEndPoint ep = new IPEndPoint (IPAddress.Loopback, 0);
			listen.Bind (ep);
			listen.Listen (1);

			wake = new Socket (AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
			wake.Connect (listen.LocalEndPoint);

			double_click_interval = 500;

#if __MonoCS__
			pollfds = new Pollfd [2];
			pollfds [0] = new Pollfd ();
			pollfds [0].fd = XConnectionNumber (DisplayHandle);
			pollfds [0].events = PollEvents.POLLIN;

			pollfds [1] = new Pollfd ();
			pollfds [1].fd = wake.Handle.ToInt32 ();
			pollfds [1].events = PollEvents.POLLIN;
#endif
		}

		~XplatUIX11() {
			lock (this) {
				if (DisplayHandle!=IntPtr.Zero) {
					XCloseDisplay(DisplayHandle);
					DisplayHandle=IntPtr.Zero;
				}
			}
		}
		#endregion	// Constructor & Destructor

		#region Singleton Specific Code
		public static XplatUIX11 GetInstance() {
			lock (typeof (XplatUIX11)) {
				if (instance==null) {
					instance=new XplatUIX11();
				}
				ref_count++;
			}
			return instance;
		}

		public int Reference {
			get {
				return ref_count;
			}
		}
		#endregion

		internal override event EventHandler Idle;
		
		#region Public Static Methods
		internal override IntPtr InitializeDriver() {
			lock (this) {
				if (DisplayHandle==IntPtr.Zero) {
					DisplayHandle=XOpenDisplay(IntPtr.Zero);
					key_state=Keys.None;
					mouse_state=MouseButtons.None;
					mouse_position=Point.Empty;
				}
			}
			return IntPtr.Zero;
		}

		internal static void SetDisplay(IntPtr display_handle) {
			if (display_handle != IntPtr.Zero) {
				IntPtr	Screen;

				if (FosterParent != IntPtr.Zero) {
					XDestroyWindow(DisplayHandle, FosterParent);
				}
				if (DisplayHandle != IntPtr.Zero) {
					XCloseDisplay(DisplayHandle);
				}

				DisplayHandle=display_handle;

				// We need to tell System.Drawing our DisplayHandle. FromHdcInternal has
				// been hacked to do this for us.
				Graphics.FromHdcInternal (DisplayHandle);

				// Create a few things
				key_state = Keys.None;
				mouse_state = MouseButtons.None;
				mouse_position = Point.Empty;
				Screen = XDefaultScreenOfDisplay(DisplayHandle);
				//screen_num = XScreenNumberOfScreen(DisplayHandle, Screen);
				screen_num = 0;
				root_window = XRootWindow(display_handle, screen_num);
				default_colormap = XDefaultColormap(display_handle, screen_num);

				// Create the foster parent
				FosterParent=XCreateSimpleWindow(display_handle, root_window, 0, 0, 1, 1, 4, 0, 0);
				if (FosterParent==IntPtr.Zero) {
					Console.WriteLine("XplatUIX11 Constructor failed to create FosterParent");
				}

				// Prepare for shutdown
				wm_protocols=XInternAtom(display_handle, "WM_PROTOCOLS", false);
				wm_delete_window=XInternAtom(display_handle, "WM_DELETE_WINDOW", false);

				// handling decorations and such
				mwm_hints=XInternAtom(display_handle, "_MOTIF_WM_HINTS", false);
				net_wm_state=XInternAtom(display_handle, "_NET_WM_STATE", false);
				wm_no_taskbar=XInternAtom(display_handle, "_NET_WM_STATE_NO_TASKBAR", false);
				wm_state_above=XInternAtom(display_handle, "_NET_WM_STATE_ABOVE", false);
				atom=XInternAtom(display_handle, "ATOM", false);

				handle_data = new Hashtable ();
			} else {
				throw new ArgumentNullException("Display", "Could not open display (X-Server required. Check you DISPLAY environment variable)");
			}
		}

		internal override void ShutdownDriver(IntPtr token) {
			lock (this) {
				if (DisplayHandle!=IntPtr.Zero) {
					XCloseDisplay(DisplayHandle);
					DisplayHandle=IntPtr.Zero;
				}
			}
		}


		internal void Version() {
			Console.WriteLine("Xplat version $revision: $");
		}

		internal override void Exit() {
			Console.WriteLine("XplatUIX11.Exit");
		}

		internal override void GetDisplaySize(out Size size) {
			XWindowAttributes	attributes=new XWindowAttributes();

			lock (xlib_lock) {
				XGetWindowAttributes(DisplayHandle, XRootWindow(DisplayHandle, 0), ref attributes);
			}

			size = new Size(attributes.width, attributes.height);
		}

		internal override void EnableThemes() {
			themes_enabled=true;
		}

		internal override IntPtr CreateWindow(CreateParams cp) {
			IntPtr			WindowHandle;
			IntPtr			ParentHandle;
			int			X;
			int			Y;
			int			Width;
			int			Height;
			MotifWmHints		mwmHints;
			uint[]			atoms;
			int			atom_count;
			int			BorderWidth;
			int			protocols;
			XSetWindowAttributes	attr;

			ParentHandle=cp.Parent;

			X=cp.X;
			Y=cp.Y;
			Width=cp.Width;
			Height=cp.Height;
			BorderWidth=0;

			if (Width<1) Width=1;
			if (Height<1) Height=1;


			lock (xlib_lock) {
				if (ParentHandle==IntPtr.Zero) {
					if ((cp.Style & (int)(WindowStyles.WS_CHILD))!=0) {
						// We need to use our foster parent window until
						// this poor child gets it's parent assigned
						ParentHandle=FosterParent;
					} else if ((cp.Style & (int)(WindowStyles.WS_POPUP))!=0) {
						BorderWidth=0;
						ParentHandle=XRootWindow(DisplayHandle, 0);
					} else {
						if (X<1) X=50;
						if (Y<1) Y=50;
						BorderWidth=4;
						ParentHandle=XRootWindow(DisplayHandle, 0);
					}
				}

				attr = new XSetWindowAttributes();

				if ((cp.ExStyle & ((int)WindowStyles.WS_EX_TOOLWINDOW)) != 0) {
					attr.save_under = true;
				}

				attr.override_redirect = false;

				if ((cp.Style & ((int)WindowStyles.WS_POPUP)) != 0) {
					attr.override_redirect = true;
				}

				attr.bit_gravity = Gravity.NorthWestGravity;
				attr.win_gravity = Gravity.NorthWestGravity;

				WindowHandle=XCreateWindow(DisplayHandle, ParentHandle, X, Y, Width, Height, BorderWidth, (int)CreateWindowArgs.CopyFromParent, (int)CreateWindowArgs.InputOutput, IntPtr.Zero, SetWindowValuemask.BitGravity | SetWindowValuemask.WinGravity | SetWindowValuemask.SaveUnder | SetWindowValuemask.OverrideRedirect, ref attr);

				// Set the appropriate window manager hints
				if (((cp.Style & ((int)WindowStyles.WS_POPUP)) != 0)  && (ParentHandle != IntPtr.Zero)) {
					XSetTransientForHint(DisplayHandle, WindowHandle, ParentHandle);
				}

				mwmHints = new MotifWmHints();
				mwmHints.flags = MotifFlags.Functions | MotifFlags.Decorations;
				mwmHints.functions = 0;
				mwmHints.decorations = 0;
				
				if ((cp.Style & ((int)WindowStyles.WS_CAPTION)) != 0) {
					mwmHints.functions |= MotifFunctions.Move;
					mwmHints.decorations |= MotifDecorations.Title | MotifDecorations.Menu;
				}

				if ((cp.Style & ((int)WindowStyles.WS_THICKFRAME)) != 0) {
					mwmHints.functions |= MotifFunctions.Move | MotifFunctions.Resize;
					mwmHints.decorations |= MotifDecorations.Border | MotifDecorations.ResizeH;
				}

				if ((cp.Style & ((int)WindowStyles.WS_MINIMIZEBOX)) != 0) {
					mwmHints.functions |= MotifFunctions.Minimize;
					mwmHints.decorations |= MotifDecorations.Minimize;
				}

				if ((cp.Style & ((int)WindowStyles.WS_MAXIMIZEBOX)) != 0) {
					mwmHints.functions |= MotifFunctions.Maximize;
					mwmHints.decorations |= MotifDecorations.Maximize;
				}

				if ((cp.Style & ((int)WindowStyles.WS_SYSMENU)) != 0) {
					mwmHints.functions |= MotifFunctions.Close;
				}

				if ((cp.ExStyle & ((int)WindowStyles.WS_EX_DLGMODALFRAME)) != 0) {
					mwmHints.decorations |= MotifDecorations.Border;
				}

				if ((cp.Style & ((int)WindowStyles.WS_DLGFRAME)) != 0) {
					mwmHints.decorations |= MotifDecorations.Border;
				}

				if ((cp.Style & ((int)WindowStyles.WS_BORDER)) != 0) {
					mwmHints.decorations |= MotifDecorations.Border;
				}


				if ((cp.ExStyle & ((int)WindowStyles.WS_EX_TOOLWINDOW)) != 0) {
					mwmHints.functions = 0;
					mwmHints.decorations = 0;
				}

				XChangeProperty(DisplayHandle, WindowHandle, mwm_hints, mwm_hints, 32, PropertyMode.Replace, ref mwmHints, 5);

				atoms = new uint[8];
				atom_count = 0;

				if ((cp.ExStyle & ((int)WindowStyles.WS_EX_TOOLWINDOW)) != 0) {
					atoms[atom_count++] = (uint)wm_state_above;
					atoms[atom_count++] = (uint)wm_no_taskbar;
				}
				XChangeProperty(DisplayHandle, WindowHandle, net_wm_state, atom, 32, PropertyMode.Replace, ref atoms, atom_count);

				XMapWindow(DisplayHandle, WindowHandle);

				XSelectInput(DisplayHandle, WindowHandle, SelectInputMask);

				protocols=wm_delete_window;
				XSetWMProtocols(DisplayHandle, WindowHandle, ref protocols, 1);
			}
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
			lock (this) {
				HandleData data = (HandleData) handle_data [handle];
				if (data != null) {
					data.Dispose ();
					handle_data [handle] = null;
					XDestroyWindow(DisplayHandle, handle);
				}
			}
		}

		internal override void RefreshWindow(IntPtr handle) {
			XEvent	xevent = new XEvent();
			IntPtr	root;
			int	border_width;
			int	depth;
			int	x;
			int	y;
			int	width;
			int	height;

			lock (xlib_lock) {

				// We need info about our window to generate the expose 
				XGetGeometry(DisplayHandle, handle, out root, out x, out y,
						out width, out height, out border_width, out depth);

				xevent.type=XEventName.Expose;
				xevent.ExposeEvent.display=DisplayHandle;
				xevent.ExposeEvent.window=handle;
				xevent.ExposeEvent.x=0;
				xevent.ExposeEvent.y=0;
				xevent.ExposeEvent.width=width;
				xevent.ExposeEvent.height=height;

				XSendEvent(DisplayHandle, handle, false, EventMask.ExposureMask, ref xevent);
				XFlush(DisplayHandle);
			}
		}

		internal override void SetWindowBackground(IntPtr handle, Color color) {
			XColor	xcolor;

			xcolor = new XColor();

			xcolor.red = (ushort)(color.R * 257);
			xcolor.green = (ushort)(color.G * 257);
			xcolor.blue = (ushort)(color.B * 257);
			XAllocColor(DisplayHandle, default_colormap, ref xcolor);

			lock (xlib_lock) {
				XSetWindowBackground(DisplayHandle, handle, xcolor.pixel);
				XClearWindow(DisplayHandle, handle);
			}
		}

		[MonoTODO("Add support for internal table of windows/DCs for looking up paint area and cleanup")]
		internal override PaintEventArgs PaintEventStart(IntPtr handle) {
			PaintEventArgs	paint_event;

			HandleData data = (HandleData) handle_data [handle];
			if (data == null) {
				throw new Exception ("null data on paint event start: " + handle);

			}

			data.DeviceContext = Graphics.FromHwnd (handle);
			paint_event = new PaintEventArgs((Graphics)data.DeviceContext, data.InvalidArea);

			return paint_event;
		}

		internal override void PaintEventEnd(IntPtr handle) {
			HandleData data = (HandleData) handle_data [handle];
			if (data == null)
				throw new Exception ("null data on PaintEventEnd");
			data.ClearInvalidArea ();
			Graphics g = (Graphics) data.DeviceContext;
			g.Flush ();
			g.Dispose ();
		}

		internal override void SetWindowPos(IntPtr handle, int x, int y, int width, int height) {
			// X requires a sanity check for width & height; otherwise it dies
			if (width < 1) {
				width = 1;
			}

			if (height < 1) {
				height = 1;
			}

			lock (xlib_lock) {
				XMoveResizeWindow(DisplayHandle, handle, x, y, width, height);
			}
			return;
		}

		internal override void GetWindowPos(IntPtr handle, out int x, out int y, out int width, out int height, out int client_width, out int client_height) {
			IntPtr	root;
			int	border_width;
			int	depth;

			lock (xlib_lock) {
				
				XGetGeometry(DisplayHandle, handle, out root, out x,
						out y, out width, out height, out border_width, out depth);
			}

			client_width = width;
			client_height = height;
			return;
		}

		internal override void Activate(IntPtr handle) {

			lock (xlib_lock) {
				// Not sure this is the right method, but we don't use ICs either...	
				XRaiseWindow(DisplayHandle, handle);
			}
			return;
		}

		internal override void EnableWindow(IntPtr handle, bool Enable) {
			// We do nothing; On X11 SetModal is used to create modal dialogs, on Win32 this function is used (see comment there)
		}

		internal override void SetModal(IntPtr handle, bool Modal) {
			// We need to use the Motif window manager hints to build modal stuff; see freedesktop.org
			throw new NotImplementedException("Finish me");
		}

		internal override void Invalidate (IntPtr handle, Rectangle rc, bool clear) {
			if (clear) {
				XClearArea (DisplayHandle, handle, rc.Left, rc.Top, (uint)rc.Width, (uint)rc.Height, true);
			} else {
				XEvent xevent = new XEvent ();
				xevent.type = XEventName.Expose;
				xevent.ExposeEvent.display = DisplayHandle;
				xevent.ExposeEvent.window = handle;
				xevent.ExposeEvent.x = rc.X;
				xevent.ExposeEvent.y = rc.Y;
				xevent.ExposeEvent.width = rc.Width;
				xevent.ExposeEvent.height = rc.Height;

				AddExpose (xevent);
			}
		}

		internal override IntPtr DefWndProc(ref Message msg) {
			switch((Msg)msg.Msg) {
				case Msg.WM_ERASEBKGND: {
					XClearArea(DisplayHandle, msg.HWnd, 0, 0, 0, 0, false);

					return IntPtr.Zero;
				}
			}
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

			lock (xlib_lock) {
				len = XLookupString(ref xevent, buffer, 24, out keysym, IntPtr.Zero);
			}

			if ((keysym==XKeySym.XK_Control_L) || (keysym==XKeySym.XK_Control_R)) {
				if (xevent.type==XEventName.KeyPress) {
					key_state |= Keys.Control;
				} else {
					key_state &= ~Keys.Control;
				}
			}

			if ((keysym==XKeySym.XK_Shift_L) || (keysym==XKeySym.XK_Shift_R)) {
				if (xevent.type==XEventName.KeyPress) {
					key_state |= Keys.Shift;
				} else {
					key_state &= ~Keys.Shift;
				}
			}

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

			if ((msg.wParam == IntPtr.Zero) && (keys.Length>0)) {
				char[] keychars;				
				keychars=keys.ToCharArray(0, 1);
				msg.wParam=(IntPtr)keychars[0];
				Console.WriteLine("Got key {0} {1:x} ", keysym, keysym);
			}

			Marshal.FreeHGlobal (buffer);
			msg.lParam = (IntPtr) 1;
		}

		private IntPtr GetMousewParam(int Delta) {
			int	result = 0;

			if ((mouse_state & MouseButtons.Left) != 0) {
				result |= (int)MsgButtons.MK_LBUTTON;
			}

			if ((mouse_state & MouseButtons.Middle) != 0) {
				result |= (int)MsgButtons.MK_MBUTTON;
			}

			if ((mouse_state & MouseButtons.Right) != 0) {
				result |= (int)MsgButtons.MK_RBUTTON;
			}

			if ((key_state & Keys.Control) != 0) {
				result |= (int)MsgButtons.MK_CONTROL;
			}

			if ((key_state & Keys.Shift) != 0) {
				result |= (int)MsgButtons.MK_SHIFT;
			}

			result |= Delta << 16;

			return (IntPtr)result;
		}

		private int NextTimeout (DateTime now)
		{
			int timeout = Int32.MaxValue; 
			lock (timer_list) {
				foreach (Timer timer in timer_list) {
					int next = (int) (timer.Expires - now).TotalMilliseconds;
					if (next < 0)
						return 0; // Have a timer that has already expired
					if (next < timeout)
						timeout = next;
				}
			}
			if (timeout < Timer.Minimum)
				timeout = Timer.Minimum;
			return timeout;
		}

		private void CheckTimers (DateTime now)
		{
			lock (timer_list) {
				int count = timer_list.Count;
				if (count == 0)
					return;
				for (int i = 0; i < count; i++) {
                                        Timer timer = (Timer) timer_list [i];
                                        if (timer.Enabled && timer.Expires <= now) {
                                                timer.FireTick ();
                                                timer.Update (now);
                                        }
                                }
			}
		}

		private void AddExpose (XEvent xevent)
		{
			HandleData data = (HandleData) handle_data [xevent.AnyEvent.window];
			if (data == null) {
				data = new HandleData ();
				handle_data [xevent.AnyEvent.window] = data;
			}

			if (!data.IsVisible) {
				return;
			}

			data.AddToInvalidArea (xevent.ExposeEvent.x, xevent.ExposeEvent.y,
					xevent.ExposeEvent.width, xevent.ExposeEvent.height);
				   
			if (!data.HasExpose) {
				message_queue.Enqueue (xevent);
				data.HasExpose = true;
			}
		}

		private void UpdateMessageQueue ()
		{
			DateTime now = DateTime.Now;

			int pending;
			lock (xlib_lock) {
				pending = XPending (DisplayHandle);
			}
			if (pending == 0) {
				if (Idle != null) {
					Idle (this, EventArgs.Empty);
				}
				lock (xlib_lock) {
					pending = XPending (DisplayHandle);
				}
			}

			if (pending == 0) {
				int timeout = NextTimeout (now);
				if (timeout > 0) {
#if __MonoCS__
					Syscall.poll (pollfds, (uint) pollfds.Length, timeout);
#endif
					pending = XPending (DisplayHandle);
				}
			}

			CheckTimers (now);

			if (pending == 0) {
				lock (xlib_lock) {
					pending = XPending (DisplayHandle);
				}
			}

			while (pending > 0) {
				XEvent xevent = new XEvent ();

				lock (xlib_lock) {
					XNextEvent (DisplayHandle, ref xevent);
				}
				
				switch (xevent.type) {
				case XEventName.Expose:
					AddExpose (xevent);
					break;
				case XEventName.KeyPress:
				case XEventName.KeyRelease:
				case XEventName.ButtonPress:
				case XEventName.ButtonRelease:
				case XEventName.MotionNotify:
				case XEventName.EnterNotify:
				case XEventName.LeaveNotify:
				case XEventName.ConfigureNotify:
				case XEventName.DestroyNotify:
				case XEventName.FocusIn:
				case XEventName.FocusOut:
				case XEventName.ClientMessage:
					message_queue.Enqueue (xevent);
					break;
				}

				lock (xlib_lock) {
					pending = XPending (DisplayHandle);
				}
			}
		}

		internal override bool GetMessage(ref MSG msg, IntPtr hWnd, int wFilterMin, int wFilterMax) {
			XEvent	xevent;

			if (message_queue.Count > 0) {
				xevent = (XEvent) message_queue.Dequeue ();
			} else {
				UpdateMessageQueue ();
				if (message_queue.Count > 0) {
					xevent = (XEvent) message_queue.Dequeue ();
				} else {
					msg.hwnd= IntPtr.Zero;
					msg.message = Msg.WM_ENTERIDLE;
					return true;
				}
			}

			msg.hwnd=xevent.AnyEvent.window;

			//
			// If you add a new event to this switch make sure to add it in
			// UpdateMessage also unless it is not coming through the X event system.
			//
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
					switch(xevent.ButtonEvent.button) {
						case 1: {
							mouse_state |= MouseButtons.Left;
							msg.message=Msg.WM_LBUTTONDOWN;
							msg.wParam=GetMousewParam(0);
							break;
						}

						case 2: {
							mouse_state |= MouseButtons.Middle;
							msg.message=Msg.WM_MBUTTONDOWN;
							msg.wParam=GetMousewParam(0);
							break;
						}

						case 3: {
							mouse_state |= MouseButtons.Right;
							msg.message=Msg.WM_RBUTTONDOWN;
							msg.wParam=GetMousewParam(0);
							break;
						}

						case 4: {
							msg.message=Msg.WM_MOUSEWHEEL;
							msg.wParam=GetMousewParam(120);
							break;
						}

						case 5: {
							msg.message=Msg.WM_MOUSEWHEEL;
							msg.wParam=GetMousewParam(-120);
							break;
						}

					}

					msg.lParam=(IntPtr) (xevent.ButtonEvent.y << 16 | xevent.ButtonEvent.x);
					mouse_position.X=xevent.ButtonEvent.x;
					mouse_position.Y=xevent.ButtonEvent.y;

					if (!click_pending) {
						click_pending = true;
						click_pending_hwnd = msg.hwnd;
						click_pending_message = msg.message;
						click_pending_wparam = msg.wParam;
						click_pending_lparam = msg.lParam;
						click_pending_time = xevent.ButtonEvent.time;
					} else {
						if (((xevent.ButtonEvent.time - click_pending_time)<double_click_interval) && (msg.wParam == click_pending_wparam) && (msg.lParam == click_pending_lparam) && (msg.message == click_pending_message)) {
							// Looks like a genuine double click, clicked twice on the same spot with the same keys
							switch(xevent.ButtonEvent.button) {
								case 1: {
									msg.message=Msg.WM_LBUTTONDBLCLK;
									break;
								}

								case 2: {
									msg.message=Msg.WM_MBUTTONDBLCLK;
									break;
								}

								case 3: {
									msg.message=Msg.WM_RBUTTONDBLCLK;
									break;
								}
							}
						}
						click_pending = false;
					}

					break;
				}

				case XEventName.ButtonRelease: {
					switch(xevent.ButtonEvent.button) {
						case 1: {
							mouse_state &= ~MouseButtons.Left;
							msg.message=Msg.WM_LBUTTONUP;
							msg.wParam=GetMousewParam(0);
							break;
						}

						case 2: {
							mouse_state &= ~MouseButtons.Middle;
							msg.message=Msg.WM_MBUTTONUP;
							msg.wParam=GetMousewParam(0);
							break;
						}

						case 3: {
							mouse_state &= ~MouseButtons.Right;
							msg.message=Msg.WM_RBUTTONUP;
							msg.wParam=GetMousewParam(0);
							break;
						}

						case 4: {
							return true;
						}

						case 5: {
							return true;
						}
					}

					msg.lParam=(IntPtr) (xevent.ButtonEvent.y << 16 | xevent.ButtonEvent.x);
					mouse_position.X=xevent.ButtonEvent.x;
					mouse_position.Y=xevent.ButtonEvent.y;
					break;
				}

				case XEventName.MotionNotify: {
					msg.message=Msg.WM_MOUSEMOVE;
					msg.wParam=GetMousewParam(0);
					msg.lParam=(IntPtr) (xevent.MotionEvent.y << 16 | xevent.MotionEvent.x);
					mouse_position.X=xevent.MotionEvent.x;
					mouse_position.Y=xevent.MotionEvent.y;
					break;
				}

				case XEventName.EnterNotify: {
					if (xevent.CrossingEvent.mode != NotifyMode.NotifyNormal) {
						return true;
					}
					msg.message=Msg.WM_MOUSE_ENTER;
					break;
				}

				case XEventName.LeaveNotify: {
					if (xevent.CrossingEvent.mode != NotifyMode.NotifyNormal) {
						return true;
					}
					msg.message=Msg.WM_MOUSE_LEAVE;
					break;
				}

				case XEventName.ConfigureNotify: {
					msg.message=Msg.WM_WINDOWPOSCHANGED;
					msg.wParam=IntPtr.Zero;
					msg.lParam=IntPtr.Zero;

					break;
				}

				case XEventName.FocusIn: {
					msg.message=Msg.WM_ACTIVATE;
					msg.wParam=(IntPtr)WindowActiveFlags.WA_ACTIVE;
					msg.lParam=IntPtr.Zero;
					break;
				}

				case XEventName.FocusOut: {
					msg.message=Msg.WM_ACTIVATE;
					msg.wParam=(IntPtr)WindowActiveFlags.WA_INACTIVE;
					msg.lParam=IntPtr.Zero;
					break;
				}

				case XEventName.Expose: {
					NativeWindow.WndProc(msg.hwnd, Msg.WM_ERASEBKGND, msg.hwnd, IntPtr.Zero);

					msg.message=Msg.WM_PAINT;
					msg.wParam=IntPtr.Zero;
					msg.lParam=IntPtr.Zero;
					break;
				}

				case XEventName.DestroyNotify: {
					msg.message=Msg.WM_DESTROY;
					msg.wParam=IntPtr.Zero;
					msg.lParam=IntPtr.Zero;
					break;
				}

				case XEventName.ClientMessage: {
					if (xevent.ClientMessageEvent.message_type == async_method) {
						GCHandle handle = (GCHandle)xevent.ClientMessageEvent.ptr1;
						AsyncMethodData data = (AsyncMethodData) handle.Target;
						AsyncMethodResult result = data.Result.Target as AsyncMethodResult;
						object ret = data.Method.DynamicInvoke (data.Args);
						if (result != null)
							result.Complete (ret);
						handle.Free ();
					} else {
						msg.message=Msg.WM_QUIT;
						msg.wParam=IntPtr.Zero;
						msg.lParam=IntPtr.Zero;
						return false;
					}
					break;
				}

				case XEventName.TimerNotify: {
					xevent.TimerNotifyEvent.handler (this, EventArgs.Empty);
					break;
				}
		                        
				default: {
					msg.message = Msg.WM_NULL;
					break;
				}
			}

			NativeWindow.WndProc(msg.hwnd, msg.message, msg.wParam, msg.lParam);

			return true;
		}

		internal override bool TranslateMessage(ref MSG msg) {
			return true;
		}

		internal override bool DispatchMessage(ref MSG msg) {
			return true;
		}

		internal override bool SetZOrder(IntPtr hWnd, IntPtr AfterhWnd, bool Top, bool Bottom) {
			if (Top) {
				XRaiseWindow(DisplayHandle, hWnd);
				return true;
			} else if (!Bottom) {
				XWindowChanges	values = new XWindowChanges();

				values.sibling = AfterhWnd;
				values.stack_mode = StackMode.Below;
				XConfigureWindow(DisplayHandle, hWnd, ChangeWindowFlags.CWStackMode, ref values);
			} else {
				XLowerWindow(DisplayHandle, hWnd);
				return true;
			}
			return false;
		}

		internal override bool SetTopmost(IntPtr hWnd, IntPtr hWndOwner, bool Enabled) {
			if (Enabled) {
				if (hWndOwner == IntPtr.Zero) {
					hWndOwner = FosterParent;
				}
				XSetTransientForHint(DisplayHandle, hWnd, hWndOwner);
			} else {
				int	trans_prop;

				trans_prop = XInternAtom(DisplayHandle, "WM_TRANSIENT_FOR", false);
				XDeleteProperty(DisplayHandle, hWnd, trans_prop);
			}
			return true;
		}

		internal override bool Text(IntPtr handle, string text) {
#if notdef
			XTextProperty	property = new XTextProperty();

			property.encoding=
			XSetWMName(DisplayHandle, handle, ref property);
#else
			lock (xlib_lock) {
				XStoreName(DisplayHandle, handle, text);
			}
#endif
			return true;
		}

		internal override bool GetText(IntPtr handle, out string text) {
			IntPtr	textptr;

			textptr = IntPtr.Zero;

			lock (xlib_lock) {
				XFetchName(DisplayHandle, handle, ref textptr);
			}
			if (textptr != IntPtr.Zero) {
				text = Marshal.PtrToStringAnsi(textptr);
				XFree(textptr);
				return true;
			} else {
				text = "";
				return false;
			}
		}

		internal override bool SetVisible(IntPtr handle, bool visible) {
			HandleData data = (HandleData) handle_data [handle];

			if (data == null) {
				data = new HandleData ();
				handle_data [handle] = data;
			}

			data.IsVisible = visible;

			lock (xlib_lock) {
				if (visible) {
					XMapWindow(DisplayHandle, handle);
				} else {
					XUnmapWindow(DisplayHandle, handle);
				}
			}
			return true;
		}

		internal override bool IsVisible(IntPtr handle) {
			HandleData data = (HandleData) handle_data [handle];

			if (data == null || data.IsVisible == true) {
				return true;
			}
			return false;
		}

		internal override IntPtr SetParent(IntPtr handle, IntPtr parent) {
			XWindowAttributes	attributes=new XWindowAttributes();

			lock (xlib_lock) {
				XGetWindowAttributes(DisplayHandle, handle, ref attributes);
				XReparentWindow(DisplayHandle, handle, parent, attributes.x, attributes.y);
			}
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

			lock (xlib_lock) {
				XQueryTree(DisplayHandle, handle, ref Root, ref Parent, ref Children, ref ChildCount);
			}

			if (Children!=IntPtr.Zero) {
				lock (xlib_lock) {
					XFree(Children);
				}
			}
			return Parent;
		}

		internal override void GrabWindow(IntPtr hWnd, IntPtr confine_hwnd) {
			if (confine_hwnd != IntPtr.Zero) {
				XWindowAttributes	attributes = new XWindowAttributes();

				lock (xlib_lock) {
					XGetWindowAttributes(DisplayHandle, confine_hwnd, ref attributes);
				}
				grab_area.X = attributes.x;
				grab_area.Y = attributes.y;
				grab_area.Width = attributes.width;
				grab_area.Height = attributes.height;
				grab_confined = true;
			}
			grab_hwnd = hWnd;
			lock (xlib_lock) {
				XGrabPointer(DisplayHandle, hWnd, false,
					EventMask.ButtonPressMask | EventMask.ButtonMotionMask |
					EventMask.ButtonReleaseMask | EventMask.PointerMotionMask,
					GrabMode.GrabModeAsync, GrabMode.GrabModeAsync, confine_hwnd, 0, 0);
			}
		}

		internal override void GrabInfo(out IntPtr hWnd, out bool GrabConfined, out Rectangle GrabArea) {
			hWnd = grab_hwnd;
			GrabConfined = grab_confined;
			GrabArea = grab_area;
		}

		internal override void ReleaseWindow(IntPtr hWnd) {
			lock (xlib_lock) {
				XUngrabPointer(DisplayHandle, 0);
				grab_hwnd = IntPtr.Zero;
				grab_confined = false;
			}
		}

		internal override bool CalculateWindowRect(IntPtr hWnd, ref Rectangle ClientRect, int Style, bool HasMenu, out Rectangle WindowRect) {
			WindowRect = new Rectangle(ClientRect.Left, ClientRect.Top, ClientRect.Width, ClientRect.Height);
			return true;
		}

		internal override void SetCursorPos(IntPtr handle, int x, int y) {
			lock (xlib_lock) {
				XWarpPointer(DisplayHandle, IntPtr.Zero, (handle!=IntPtr.Zero) ? handle : IntPtr.Zero, 0, 0, 0, 0, x, y);
			}
		}

		internal override void GetCursorPos(IntPtr handle, out int x, out int y) {
			IntPtr	root;
			IntPtr	child;
			int	root_x;
			int	root_y;
			int	win_x;
			int	win_y;
			int	keys_buttons;

			lock (xlib_lock) {
				XQueryPointer(DisplayHandle, (handle!=IntPtr.Zero) ? handle : root_window,
						out root, out child, out root_x, out root_y,
						out win_x, out win_y, out keys_buttons);
			}

			if (handle != IntPtr.Zero) {
				x = win_x;
				y = win_y;
			} else {
				x = root_x;
				y = root_y;
			}
		}

		internal override void ScreenToClient(IntPtr handle, ref int x, ref int y)
		{
			int	dest_x_return;
			int	dest_y_return;
			IntPtr	child;

			lock (xlib_lock) {
				XTranslateCoordinates (DisplayHandle, root_window,
						handle, x, y, out dest_x_return, out dest_y_return, out child);
			}

			x = dest_x_return;
			y = dest_y_return;
		}

		internal override void ClientToScreen(IntPtr handle, ref int x, ref int y) {
			int	dest_x_return;
			int	dest_y_return;
			IntPtr	child;

			lock (xlib_lock) {
				XTranslateCoordinates (DisplayHandle, handle, root_window,
					x, y, out dest_x_return, out dest_y_return, out child);
			}

			x = dest_x_return;
			y = dest_y_return;
		}

		internal override void SendAsyncMethod (AsyncMethodData method)
		{
			XEvent xevent = new XEvent ();
			
			xevent.type = XEventName.ClientMessage;
			xevent.ClientMessageEvent.display = DisplayHandle;
			xevent.ClientMessageEvent.window = IntPtr.Zero;
			xevent.ClientMessageEvent.message_type = async_method;
			xevent.ClientMessageEvent.format = 32;
			xevent.ClientMessageEvent.ptr1 = (IntPtr) GCHandle.Alloc (method);

			message_queue.EnqueueLocked (xevent);

			WakeupMain ();
		}

		private void WakeupMain ()
		{
			wake.BeginSend (new byte [] { 0xFF }, 0, 1, SocketFlags.None, null, null);
		}

		internal override void SetTimer (Timer timer)
		{
			lock (timer_list) {
				timer_list.Add (timer);
			}
			WakeupMain ();
		}

		internal override void KillTimer (Timer timer)
		{
			lock (timer_list) {
				timer_list.Remove (timer);
			}
		}

		// Santa's little helper
		static void Where() 
		{
			Console.WriteLine("Here: {0}", new StackTrace().ToString());
		}
		#endregion	// Public Static Methods

		internal struct X11ToWin32KeyMapping {
			internal XKeySym	X11Key;
			internal VirtualKeys	Win32Key;	

			internal X11ToWin32KeyMapping (XKeySym x11, VirtualKeys win32)
			{
				X11Key = x11;
				Win32Key = win32;
			} 
		}

		/* X11 to Win32 VK mapping */
		static readonly X11ToWin32KeyMapping[] KeyMapping = new X11ToWin32KeyMapping[] 
		{
			/* Cursor navigation*/
			new X11ToWin32KeyMapping (XKeySym.XK_Left, VirtualKeys.VK_LEFT),
			new X11ToWin32KeyMapping (XKeySym.XK_Right, VirtualKeys.VK_RIGHT),
			new X11ToWin32KeyMapping (XKeySym.XK_Up, VirtualKeys.VK_UP),
			new X11ToWin32KeyMapping (XKeySym.XK_Down, VirtualKeys.VK_DOWN),

			new X11ToWin32KeyMapping (XKeySym.XK_Page_Up, VirtualKeys.VK_PRIOR),
			new X11ToWin32KeyMapping (XKeySym.XK_Page_Down, VirtualKeys.VK_NEXT),
			new X11ToWin32KeyMapping (XKeySym.XK_End, VirtualKeys.VK_END),
			new X11ToWin32KeyMapping (XKeySym.XK_Home, VirtualKeys.VK_HOME),

			/* Modifiers*/
			new X11ToWin32KeyMapping (XKeySym.XK_Shift_R, VirtualKeys.VK_SHIFT),
			new X11ToWin32KeyMapping (XKeySym.XK_Shift_L, VirtualKeys.VK_SHIFT),
			new X11ToWin32KeyMapping (XKeySym.XK_Control_R, VirtualKeys.VK_CONTROL),
			new X11ToWin32KeyMapping (XKeySym.XK_Control_L, VirtualKeys.VK_CONTROL),			

			/* Others */
			new X11ToWin32KeyMapping (XKeySym.XK_Return, VirtualKeys.VK_RETURN),
			new X11ToWin32KeyMapping (XKeySym.XK_Tab, VirtualKeys.VK_TAB),
			new X11ToWin32KeyMapping (XKeySym.XK_Menu, VirtualKeys.VK_MENU),
			new X11ToWin32KeyMapping (XKeySym.XK_BackSpace, VirtualKeys.VK_BACK),
			new X11ToWin32KeyMapping (XKeySym.XK_Clear, VirtualKeys.VK_CLEAR),
			
		};

		

		#region X11 Imports
		[DllImport ("libX11", EntryPoint="XOpenDisplay")]
		internal extern static IntPtr XOpenDisplay(IntPtr display);
		[DllImport ("libX11", EntryPoint="XCloseDisplay")]
		internal extern static void XCloseDisplay(IntPtr display);						    

		[DllImport ("libX11", EntryPoint="XCreateWindow")]
		internal extern static IntPtr XCreateWindow(IntPtr display, IntPtr parent, int x, int y, int width, int height, int border_width, int depth, int xclass, IntPtr visual, SetWindowValuemask valuemask, ref XSetWindowAttributes attributes);
		[DllImport ("libX11", EntryPoint="XCreateSimpleWindow")]
		internal extern static IntPtr XCreateSimpleWindow(IntPtr display, IntPtr parent, int x, int y, int width, int height, int border_width, int border, int background);
		[DllImport ("libX11", EntryPoint="XMapWindow")]
		internal extern static int XMapWindow(IntPtr display, IntPtr window);
		[DllImport ("libX11", EntryPoint="XUnmapWindow")]
		internal extern static int XUnmapWindow(IntPtr display, IntPtr window);
		[DllImport ("libX11", EntryPoint="XMapSubwindows")]
		internal extern static int XMapSubindows(IntPtr display, IntPtr window);
		[DllImport ("libX11", EntryPoint="XUnmapSubwindows")]
		internal extern static int XUnmapSubwindows(IntPtr display, IntPtr window);
		[DllImport ("libX11", EntryPoint="XRootWindow")]
		internal extern static IntPtr XRootWindow(IntPtr display, int screen_number);
		[DllImport ("libX11", EntryPoint="XNextEvent")]
		internal extern static IntPtr XNextEvent(IntPtr display, ref XEvent xevent);
		[DllImport ("libX11")]
		internal extern static int XConnectionNumber (IntPtr diplay);
		[DllImport ("libX11")]
		internal extern static int XPending (IntPtr diplay);
		[DllImport ("libX11")]
		internal extern static bool XCheckWindowEvent (IntPtr display, IntPtr window, EventMask mask, ref XEvent xevent);
		[DllImport ("libX11")]
		internal extern static bool XCheckMaskEvent (IntPtr display, EventMask mask, ref XEvent xevent);
		[DllImport ("libX11", EntryPoint="XSelectInput")]
		internal extern static IntPtr XSelectInput(IntPtr display, IntPtr window, EventMask mask);
		[DllImport ("libX11", EntryPoint="XLookupString")]
		internal extern static int XLookupString(ref XEvent xevent, IntPtr buffer, int num_bytes, out XKeySym keysym, IntPtr status);

		[DllImport ("libX11", EntryPoint="XDestroyWindow")]
		internal extern static int XDestroyWindow(IntPtr display, IntPtr window);

		[DllImport ("libX11", EntryPoint="XReparentWindow")]
		internal extern static int XReparentWindow(IntPtr display, IntPtr window, IntPtr parent, int x, int y);
		[DllImport ("libX11", EntryPoint="XMoveResizeWindow")]
		internal extern static int XMoveResizeWindow(IntPtr display, IntPtr window, int x, int y, int width, int height);

		[DllImport ("libX11", EntryPoint="XResizeWindow")]
		internal extern static int XResizeWindow(IntPtr display, IntPtr window, int width, int height);

		[DllImport ("libX11", EntryPoint="XGetWindowAttributes")]
		internal extern static int XGetWindowAttributes(IntPtr display, IntPtr window, ref XWindowAttributes attributes);

		[DllImport ("libX11", EntryPoint="XFlush")]
		internal extern static int XFlush(IntPtr display);

		[DllImport ("libX11", EntryPoint="XSetWMName")]
		internal extern static int XSetWMName(IntPtr display, IntPtr window, ref XTextProperty text_prop);

		[DllImport ("libX11", EntryPoint="XStoreName")]
		internal extern static int XStoreName(IntPtr display, IntPtr window, string window_name);

		[DllImport ("libX11", EntryPoint="XFetchName")]
		internal extern static int XFetchName(IntPtr display, IntPtr window, ref IntPtr window_name);

		[DllImport ("libX11", EntryPoint="XSendEvent")]
		internal extern static int XSendEvent(IntPtr display, IntPtr window, bool propagate, EventMask event_mask, ref XEvent send_event);

		[DllImport ("libX11", EntryPoint="XQueryTree")]
		internal extern static int XQueryTree(IntPtr display, IntPtr window, ref IntPtr root_return, ref IntPtr parent_return, ref IntPtr children_return, ref int nchildren_return);

		[DllImport ("libX11", EntryPoint="XFree")]
		internal extern static int XFree(IntPtr data);

		[DllImport ("libX11", EntryPoint="XRaiseWindow")]
		internal extern static int XRaiseWindow(IntPtr display, IntPtr window);

		[DllImport ("libX11", EntryPoint="XLowerWindow")]
		internal extern static uint XLowerWindow(IntPtr display, IntPtr window);

		[DllImport ("libX11", EntryPoint="XConfigureWindow")]
		internal extern static uint XConfigureWindow(IntPtr display, IntPtr window, ChangeWindowFlags value_mask, ref XWindowChanges values);

		[DllImport ("libX11", EntryPoint="XInternAtom")]
		internal extern static int XInternAtom(IntPtr display, string atom_name, bool only_if_exists);

		[DllImport ("libX11", EntryPoint="XSetWMProtocols")]
		internal extern static int XSetWMProtocols(IntPtr display, IntPtr window, ref int protocols, int count);

		[DllImport ("libX11", EntryPoint="XGrabPointer")]
		internal extern static int XGrabPointer(IntPtr display, IntPtr window, bool owner_events, EventMask event_mask, GrabMode pointer_mode, GrabMode keyboard_mode, IntPtr confine_to, uint cursor, uint timestamp);

		[DllImport ("libX11", EntryPoint="XUngrabPointer")]
		internal extern static int XUngrabPointer(IntPtr display, uint timestamp);

		[DllImport ("libX11", EntryPoint="XQueryPointer")]
		internal extern static bool XQueryPointer(IntPtr display, IntPtr window, out IntPtr root, out IntPtr child, out int root_x, out int root_y, out int win_x, out int win_y, out int keys_buttons);

		[DllImport ("libX11", EntryPoint="XTranslateCoordinates")]
		internal extern static bool XTranslateCoordinates (IntPtr display, IntPtr src_w, IntPtr dest_w, int src_x, int src_y, out int intdest_x_return,	 out int dest_y_return, out IntPtr child_return);

		[DllImport ("libX11", EntryPoint="XGetGeometry")]
		internal extern static bool XGetGeometry(IntPtr display, IntPtr window, out IntPtr root, out int x, out int y, out int width, out int height, out int border_width, out int depth);

		[DllImport ("libX11", EntryPoint="XWarpPointer")]
		internal extern static uint XWarpPointer(IntPtr display, IntPtr src_w, IntPtr dest_w, int src_x, int src_y, uint src_width, uint src_height, int dest_x, int dest_y);

		[DllImport ("libX11", EntryPoint="XClearWindow")]
		internal extern static int XClearWindow(IntPtr display, IntPtr window);

		[DllImport ("libX11", EntryPoint="XClearArea")]
		internal extern static int XClearArea(IntPtr display, IntPtr window, int x, int y, uint width, uint height, bool exposures);

		// Colormaps
		[DllImport ("libX11", EntryPoint="XDefaultScreenOfDisplay")]
		internal extern static IntPtr XDefaultScreenOfDisplay(IntPtr display);

		[DllImport ("libX11", EntryPoint="XScreenNumberOfScreen")]
		internal extern static int XScreenNumberOfScreen(IntPtr display, IntPtr Screen);

		[DllImport ("libX11", EntryPoint="XDefaultVisual")]
		internal extern static uint XDefaultVisual(IntPtr display, int screen_number);

		[DllImport ("libX11", EntryPoint="XDefaultDepth")]
		internal extern static uint XDefaultDepth(IntPtr display, int screen_number);

		[DllImport ("libX11", EntryPoint="XDefaultColormap")]
		internal extern static uint XDefaultColormap(IntPtr display, int screen_number);

		[DllImport ("libX11", EntryPoint="XLookupColor")]
		internal extern static int XLookupColor(IntPtr display, uint Colormap, string Coloranem, ref XColor exact_def_color, ref XColor screen_def_color);

		[DllImport ("libX11", EntryPoint="XAllocColor")]
		internal extern static int XAllocColor(IntPtr display, uint Colormap, ref XColor colorcell_def);

		[DllImport ("libX11.so", EntryPoint="XSetTransientForHint")]
		internal extern static int XSetTransientForHint(IntPtr display, IntPtr window, IntPtr prop_window);

		[DllImport ("libX11.so", EntryPoint="XChangeProperty")]
		internal extern static int XChangeProperty(IntPtr display, IntPtr window, int property, int type, int format, PropertyMode  mode, ref MotifWmHints data, int nelements);

		[DllImport ("libX11.so", EntryPoint="XChangeProperty")]
		internal extern static int XChangeProperty(IntPtr display, IntPtr window, int property, int format, int type, PropertyMode  mode, ref uint[] atoms, int nelements);

		[DllImport ("libX11.so", EntryPoint="XChangeProperty")]
		internal extern static int XChangeProperty(IntPtr display, IntPtr window, int property, int format, int type, PropertyMode  mode, IntPtr data, int nelements);

		[DllImport ("libX11.so", EntryPoint="XDeleteProperty")]
		internal extern static int XDeleteProperty(IntPtr display, IntPtr window, int property);

		// Drawing
		[DllImport ("libX11", EntryPoint="XCreateGC")]
		internal extern static IntPtr XCreateGC(IntPtr display, IntPtr window, int valuemask, IntPtr values);
		[DllImport ("libX11", EntryPoint="XFreeGC")]
		internal extern static int XFreeGC(IntPtr display, IntPtr gc);
		[DllImport ("libX11", EntryPoint="XDrawLine")]
		internal extern static int XDrawLine(IntPtr display, IntPtr drawable, IntPtr gc, int x1, int y1, int x2, int y2);
		[DllImport ("libX11", EntryPoint="XSetWindowBackground")]
		internal extern static int XSetWindowBackground(IntPtr display, IntPtr window, uint background);

		// Keyboard

		[StructLayout (LayoutKind.Sequential)]
		internal struct XKeyBoardState {
			int key_click_percent;
			int bell_percent;
			uint bell_pitch, bell_duration;
			uint led_mask;
			int global_auto_repeat;
			AutoRepeats auto_repeats;

			[StructLayout (LayoutKind.Explicit)]
			struct AutoRepeats {
				[FieldOffset (0)]
					byte first;
				
				[FieldOffset (31)]
				byte last;
			}
		}
		
		[DllImport ("libX11", EntryPoint="XGetKeyboardControl")]
		internal extern static int XGetKeyboardControl (IntPtr display, out XKeyBoardState state);

		internal override int KeyboardSpeed {
			get {
				//
				// A lot harder: need to do:
				// XkbQueryExtension(0x08051008, 0xbfffdf4c, 0xbfffdf50, 0xbfffdf54, 0xbfffdf58)       = 1
				// XkbAllocKeyboard(0x08051008, 0xbfffdf4c, 0xbfffdf50, 0xbfffdf54, 0xbfffdf58)        = 0x080517a8
				// XkbGetControls(0x08051008, 1, 0x080517a8, 0xbfffdf54, 0xbfffdf58)                   = 0
				//
				// And from that we can tell the repetition rate
				//
				// Notice, the values must map to:
				//   [0, 31] which maps to 2.5 to 30 repetitions per second.
				//
				return 0;
			}
		}

		internal override int KeyboardDelay {
			get {
				//
				// Return values must range from 0 to 4, 0 meaning 250ms,
				// and 4 meaning 1000 ms.
				//
				return 1; // ie, 500 ms
			}
		}
#endregion

	}
}
