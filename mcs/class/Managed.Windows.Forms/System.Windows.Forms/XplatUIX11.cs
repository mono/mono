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

/// X11 Version
namespace System.Windows.Forms {
	internal class XplatUIX11 : XplatUIDriver {
		#region Local Variables
		private static XplatUIX11	instance;
		private static int		ref_count;
		private static bool		themes_enabled;

		private static IntPtr		DisplayHandle;		// X11 handle to display
		private static IntPtr		root_window;		// Handle of the root window for the screen/display
		private static IntPtr		FosterParent;		// Container to hold child windows until their parent exists
		private static int		wm_protocols;		// X Atom
		private static int		wm_delete_window;	// X Atom
		private static IntPtr		async_method;
		private static uint		default_colormap;	// X Colormap ID
		internal static Keys		key_state;
		internal static MouseButtons	mouse_state;
		internal static Point		mouse_position;
		internal static bool		grab_confined;		// Is the current grab (if any) confined to grab_area?
		internal static IntPtr		grab_hwnd;		// The window that is grabbed
		internal static Rectangle	grab_area;		// The area the current grab is confined to
		internal static	bool		is_visible;

		private static Hashtable	handle_data;
		private Queue message_queue;
		private ArrayList timer_list;

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

			message_queue = new Queue ();
			timer_list = new ArrayList ();

			// Now regular initialization
			SetDisplay(XOpenDisplay(IntPtr.Zero));
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
				if (FosterParent != IntPtr.Zero) {
					XDestroyWindow(DisplayHandle, FosterParent);
				}
				if (DisplayHandle != IntPtr.Zero) {
					XCloseDisplay(DisplayHandle);
				}

				DisplayHandle=display_handle;

				// Create a few things
				key_state = Keys.None;
				mouse_state = MouseButtons.None;
				mouse_position = Point.Empty;
				root_window = XRootWindow(display_handle, 0);
				default_colormap = XDefaultColormap(display_handle, 0);

				// Create the foster parent
				FosterParent=XCreateSimpleWindow(display_handle, root_window, 0, 0, 1, 1, 4, 0, 0);
				if (FosterParent==IntPtr.Zero) {
					Console.WriteLine("XplatUIX11 Constructor failed to create FosterParent");
				}

				// Prepare for shutdown
				wm_protocols=XInternAtom(display_handle, "WM_PROTOCOLS", false);
				wm_delete_window=XInternAtom(display_handle, "WM_DELETE_WINDOW", false);

				handle_data = new Hashtable ();
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

			lock (this) {
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
			int			BorderWidth;
			int			protocols;

			ParentHandle=cp.Parent;

			X=cp.X;
			Y=cp.Y;
			Width=cp.Width;
			Height=cp.Height;
			BorderWidth=0;

			if (Width<1) Width=1;
			if (Height<1) Height=1;


			lock (this) {
				if (ParentHandle==IntPtr.Zero) {
					if ((cp.Style & (int)WindowStyles.WS_CHILD)!=0) {
						// We need to use our foster parent window until
						// this poor child gets it's parent assigned
						ParentHandle=FosterParent;
					} else {
						if (X<1) X=50;
						if (Y<1) Y=50;
						BorderWidth=4;
						ParentHandle=XRootWindow(DisplayHandle, 0);
					}
				}

				WindowHandle=XCreateSimpleWindow(DisplayHandle, ParentHandle, X, Y, Width, Height, BorderWidth, 0, 0);
				XMapWindow(DisplayHandle, WindowHandle);

				XSelectInput(DisplayHandle, WindowHandle, SelectInputMask);
				is_visible=true;

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
				XDestroyWindow(DisplayHandle, handle);
				HandleData data = (HandleData) handle_data [handle];
				if (data != null) {
					data.Dispose ();
					handle_data [handle] = null;
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

			lock (this) {

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
			uint	backcolor;

//			backcolor = ((uint)(color.ToArgb() & 0xff0000)>>16) | (uint)(color.ToArgb() & 0xff00) | (uint)((color.ToArgb() & 0xff) << 16);
			backcolor = ((uint)(color.ToArgb() & 0xff0000)) | (uint)(color.ToArgb() & 0xff00) | (uint)((color.ToArgb() & 0xff) );
			lock (this) {
				XSetWindowBackground(DisplayHandle, handle, backcolor);
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
		}

		internal override void SetWindowPos(IntPtr handle, int x, int y, int width, int height) {
			// X requires a sanity check for width & height; otherwise it dies
			if (width < 1) {
				width = 1;
			}

			if (height < 1) {
				height = 1;
			}
			lock (this) {
				XMoveResizeWindow(DisplayHandle, handle, x, y, width, height);
			}
			return;
		}

		internal override void GetWindowPos(IntPtr handle, out int x, out int y, out int width, out int height, out int client_width, out int client_height) {
			IntPtr	root;
			int	border_width;
			int	depth;

			lock (this) {
				
				XGetGeometry(DisplayHandle, handle, out root, out x,
						out y, out width, out height, out border_width, out depth);
			}

			client_width = width;
			client_height = height;
			return;
		}

		internal override void Activate(IntPtr handle) {

			lock (this) {
				// Not sure this is the right method, but we don't use ICs either...	
				XRaiseWindow(DisplayHandle, handle);
			}
			return;
		}

		internal override void Invalidate(IntPtr handle, Rectangle rc, bool clear) {
			XEvent			xevent = new XEvent();

			xevent.type=XEventName.Expose;
			xevent.ExposeEvent.display=DisplayHandle;
			xevent.ExposeEvent.window=handle;
			xevent.ExposeEvent.count=0;

			lock (this) {

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
			}
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

			lock (this) {
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

		internal override bool GetMessage(ref MSG msg, IntPtr hWnd, int wFilterMin, int wFilterMax) {
			XEvent	xevent = new XEvent();
			bool queued_message = false;

			lock (message_queue) {
				if (message_queue.Count > 0) {
					xevent = (XEvent) message_queue.Dequeue ();
					queued_message = true;
				}
			}

			if (!queued_message) {
				lock (this) {
					if (!XCheckMaskEvent (DisplayHandle, SelectInputMask, ref xevent)) {
						for (int i = 0; i < timer_list.Count; i++) {
							Timer timer = (Timer) timer_list [i];
							DateTime now = DateTime.Now;
							if (timer.Enabled && timer.Expires <= now) {
								timer.FireTick ();
								timer.Update ();
							}
						}
						msg.message = Msg.WM_ENTERIDLE;
						return true;
					}
				}
			}

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

				case XEventName.Expose: {
					HandleData data = (HandleData) handle_data [xevent.AnyEvent.window];
					if (data == null) {
						data = new HandleData ();
						handle_data [xevent.AnyEvent.window] = data;
					}

					data.AddToInvalidArea (xevent.ExposeEvent.x, xevent.ExposeEvent.y,
							xevent.ExposeEvent.width, xevent.ExposeEvent.height);

					lock (this) {
						// Try combining expose events to reduce drawing	
						while (XCheckWindowEvent (DisplayHandle, xevent.AnyEvent.window,
								       EventMask.ExposureMask, ref xevent)) {
							data.AddToInvalidArea (xevent.ExposeEvent.x, xevent.ExposeEvent.y,
									xevent.ExposeEvent.width, xevent.ExposeEvent.height);
						}
					}	

					msg.message=Msg.WM_PAINT;
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

		internal override bool Text(IntPtr handle, string text) {
#if notdef
			XTextProperty	property = new XTextProperty();

			property.encoding=
			XSetWMName(DisplayHandle, handle, ref property);
#else
			lock (this) {
				XStoreName(DisplayHandle, handle, text);
			}
#endif
			return true;
		}

		internal override bool SetVisible(IntPtr handle, bool visible) {
			lock (this) {
				if (visible) {
					XMapWindow(DisplayHandle, handle);
					is_visible=true;
				} else {
					XUnmapWindow(DisplayHandle, handle);
					is_visible=false;
				}
			}
			return true;
		}

		internal override bool IsVisible(IntPtr handle) {
			return is_visible;
		}

		internal override IntPtr SetParent(IntPtr handle, IntPtr parent) {
			XWindowAttributes	attributes=new XWindowAttributes();

			lock (this) {
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

			lock (this) {
				XQueryTree(DisplayHandle, handle, ref Root, ref Parent, ref Children, ref ChildCount);
			}

			if (Children!=IntPtr.Zero) {
				lock (this) {
					XFree(Children);
				}
			}
			return Parent;
		}

		internal override void GrabWindow(IntPtr hWnd, IntPtr confine_hwnd) {
			if (confine_hwnd != IntPtr.Zero) {
				XWindowAttributes	attributes = new XWindowAttributes();

				lock (this) {
					XGetWindowAttributes(DisplayHandle, confine_hwnd, ref attributes);
				}
				grab_area.X = attributes.x;
				grab_area.Y = attributes.y;
				grab_area.Width = attributes.width;
				grab_area.Height = attributes.height;
				grab_confined = true;
			}
			grab_hwnd = hWnd;
			lock (this) {
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
			lock (this) {
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
			lock (this) {
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

			lock (this) {
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

			lock (this) {
				XTranslateCoordinates (DisplayHandle, root_window,
						handle, x, y, out dest_x_return, out dest_y_return, out child);
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

			lock (message_queue) {
                                message_queue.Enqueue (xevent);
			}
		}

		internal override void SetTimer (Timer timer)
		{
			lock (timer_list) {
				timer_list.Add (timer);
			}
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
		[DllImport ("libX11.so")]
		internal extern static bool XCheckWindowEvent (IntPtr display, IntPtr window, EventMask mask, ref XEvent xevent);
		[DllImport ("libX11.so")]
		internal extern static bool XCheckMaskEvent (IntPtr display, EventMask mask, ref XEvent xevent);
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

		[DllImport ("libX11.so", EntryPoint="XResizeWindow")]
		internal extern static int XResizeWindow(IntPtr display, IntPtr window, int width, int height);

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

		[DllImport ("libX11.so", EntryPoint="XRaiseWindow")]
		internal extern static int XRaiseWindow(IntPtr display, IntPtr window);

		[DllImport ("libX11.so", EntryPoint="XInternAtom")]
		internal extern static int XInternAtom(IntPtr display, string atom_name, bool only_if_exists);

		[DllImport ("libX11.so", EntryPoint="XSetWMProtocols")]
		internal extern static int XSetWMProtocols(IntPtr display, IntPtr window, ref int protocols, int count);

		[DllImport ("libX11.so", EntryPoint="XGrabPointer")]
		internal extern static int XGrabPointer(IntPtr display, IntPtr window, bool owner_events, EventMask event_mask, GrabMode pointer_mode, GrabMode keyboard_mode, IntPtr confine_to, uint cursor, uint timestamp);

		[DllImport ("libX11.so", EntryPoint="XUngrabPointer")]
		internal extern static int XUngrabPointer(IntPtr display, uint timestamp);

		[DllImport ("libX11.so", EntryPoint="XQueryPointer")]
		internal extern static bool XQueryPointer(IntPtr display, IntPtr window, out IntPtr root, out IntPtr child, out int root_x, out int root_y, out int win_x, out int win_y, out int keys_buttons);

		[DllImport ("libX11.so", EntryPoint="XTranslateCoordinates")]
		internal extern static bool XTranslateCoordinates (IntPtr display, IntPtr src_w, IntPtr dest_w, int src_x, int src_y, out int intdest_x_return,	 out int dest_y_return, out IntPtr child_return);

		[DllImport ("libX11.so", EntryPoint="XGetGeometry")]
		internal extern static bool XGetGeometry(IntPtr display, IntPtr window, out IntPtr root, out int x, out int y, out int width, out int height, out int border_width, out int depth);

		[DllImport ("libX11.so", EntryPoint="XAllocColor")]
		internal extern static int XAllocColor(IntPtr display, uint Colormap, ref XColor colorcell_def);

		[DllImport ("libX11.so", EntryPoint="XGetStandardColormap")]
		internal extern static int XGetStandardColormap(IntPtr display, IntPtr window, ref XStandardColormap cmap_info, Atom property);

		[DllImport ("libX11.so", EntryPoint="XSetRGBColormaps")]
		internal extern static int XSetRGBColormaps(IntPtr display, IntPtr window, ref XStandardColormap cmap_info, int count, Atom property);

		[DllImport ("libX11.so", EntryPoint="XInstallColormap")]
		internal extern static int XInstallColormap(IntPtr display, uint cmap);

		[DllImport ("libX11.so", EntryPoint="XDefaultColormap")]
		internal extern static uint XDefaultColormap(IntPtr display, int screen_number);

		[DllImport ("libX11.so", EntryPoint="XDefaultDepth")]
		internal extern static uint XDefaultDepth(IntPtr display, int screen_number);

		[DllImport ("libX11.so", EntryPoint="XDefaultVisual")]
		internal extern static uint XDefaultVisual(IntPtr display, int screen_number);

		[DllImport ("libX11.so", EntryPoint="XSetWindowColormap")]
		internal extern static uint XSetWindowColormap(IntPtr display, IntPtr window, uint cmap);

		[DllImport ("libX11.so", EntryPoint="XWarpPointer")]
		internal extern static uint XWarpPointer(IntPtr display, IntPtr src_w, IntPtr dest_w, int src_x, int src_y, uint src_width, uint src_height, int dest_x, int dest_y);

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
