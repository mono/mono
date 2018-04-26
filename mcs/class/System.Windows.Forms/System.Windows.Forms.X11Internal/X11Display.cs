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
// Copyright (c) 2006 Novell, Inc. (http://www.novell.com)
//
//

using System;
using System.Collections;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Mono.Unix.Native;

namespace System.Windows.Forms.X11Internal {

	internal class X11Display {

		IntPtr display; /* our X handle */

		// XXX internal because X11Hwnd needs them
		internal IntPtr CustomVisual;    // Visual for window creation
		internal IntPtr CustomColormap;  // Colormap for window creation

		X11Keyboard Keyboard;
		internal X11Dnd Dnd; // XXX X11Hwnd needs it to enable Dnd
		bool detectable_key_auto_repeat;

		X11Atoms atoms;
		X11RootHwnd root_hwnd;
		X11Hwnd foster_hwnd;

		// Clipboard
		IntPtr 		ClipMagic;
		ClipboardStruct	Clipboard; // Our clipboard

		// Focus tracking
		internal X11Hwnd ActiveWindow;
		X11Hwnd FocusWindow;

		// Modality support
		Stack ModalWindows; // Stack of our modal windows

		// Caret
		CaretStruct Caret;

		// mouse hover message generation
		// XXX internal because X11Atoms needs to access it..
		internal HoverStruct HoverState;

		// double click message generation
		ClickStruct ClickPending;
		int DoubleClickInterval; // msec; max interval between clicks to count as double click

		// Support for mouse grab
		GrabStruct Grab;

		// Cursors
		IntPtr LastCursorWindow; // The last window we set the cursor on
		IntPtr LastCursorHandle; // The handle that was last set on LastCursorWindow
		IntPtr OverrideCursorHandle; // The cursor that is set to override any other cursors

		// State
		Point MousePosition;     // Last position of mouse, in screen coords
		MouseButtons MouseState; // Last state of mouse buttons

		XErrorHandler	ErrorHandler;		// Error handler delegate
		bool		ErrorExceptions;	// Throw exceptions on X errors

		Thread event_thread; // the background thread that just watches our X socket

		Pollfd[] pollfds;

		public X11Display (IntPtr display)
		{
			if (display == IntPtr.Zero) {
				throw new ArgumentNullException("Display",
							"Could not open display (X-Server required. Check your DISPLAY environment variable)");
			}

			this.display = display;

			// Debugging support
			if (Environment.GetEnvironmentVariable ("MONO_XSYNC") != null) {
				Xlib.XSynchronize (display, true);
			}

			if (Environment.GetEnvironmentVariable ("MONO_XEXCEPTIONS") != null) {
				ErrorExceptions = true;
			}

			atoms = new X11Atoms (this);

			DoubleClickInterval = 500;

			HoverState.Interval = 500;
			HoverState.Timer = new Timer();
			HoverState.Timer.Enabled = false;
			HoverState.Timer.Interval = HoverState.Interval;
			HoverState.Timer.Tick += new EventHandler(MouseHover);
			HoverState.Size = new Size(4, 4);
			HoverState.X = -1;
			HoverState.Y = -1;

			ActiveWindow = null;
			FocusWindow = null;
			ModalWindows = new Stack(3);

			MouseState = MouseButtons.None;
			MousePosition = new Point(0, 0);

			Caret.Timer = new Timer();
			Caret.Timer.Interval = 500;		// FIXME - where should this number come from?
			Caret.Timer.Tick += new EventHandler(CaretCallback);

			// XXX multiscreen work here
			root_hwnd = new X11RootHwnd (this, Xlib.XRootWindow (display, DefaultScreen));

			// XXX do we need a per-screen foster parent?
			// Create the foster parent
			foster_hwnd = new X11Hwnd (this,
						   Xlib.XCreateSimpleWindow (display, root_hwnd.WholeWindow,
									     0, 0, 1, 1, 4, UIntPtr.Zero, UIntPtr.Zero));

			pollfds = new Pollfd [1];
			pollfds [0] = new Pollfd ();
			pollfds [0].fd = Xlib.XConnectionNumber (display);
			pollfds [0].events = PollEvents.POLLIN;

			Keyboard = new X11Keyboard(display, foster_hwnd.Handle);
			Dnd = new X11Dnd (display, Keyboard);

			ErrorExceptions = false;

			// Handle any upcoming errors
			ErrorHandler = new XErrorHandler (HandleError);
			Xlib.XSetErrorHandler (ErrorHandler);

			X11DesktopColors.Initialize(); // XXX we need to figure out how to make this display specific?

			// Disable keyboard autorepeat
			try {
				Xlib.XkbSetDetectableAutoRepeat (display, true, IntPtr.Zero);
				detectable_key_auto_repeat = true;
			} catch {
				Console.Error.WriteLine ("Could not disable keyboard auto repeat, will attempt to disable manually.");
				detectable_key_auto_repeat = false;
			}

			// we re-set our error handler here, X11DesktopColor stuff might have stolen it (gtk does)
			Xlib.XSetErrorHandler (ErrorHandler);

			// create our event thread (just sits on the X socket waiting for events)
			event_thread = new Thread (new ThreadStart (XEventThread));
			event_thread.IsBackground = true;
			event_thread.Start ();
		}

		#region	Callbacks
		private void MouseHover(object sender, EventArgs e)
		{
			HoverState.Timer.Enabled = false;

			if (HoverState.Window != IntPtr.Zero) {
				X11Hwnd hwnd = (X11Hwnd)Hwnd.GetObjectFromWindow (HoverState.Window);
				if (hwnd != null) {
					XEvent xevent = new XEvent ();

					xevent.type = XEventName.ClientMessage;
					xevent.ClientMessageEvent.display = display;
					xevent.ClientMessageEvent.window = HoverState.Window;
					xevent.ClientMessageEvent.message_type = HoverState.Atom;
					xevent.ClientMessageEvent.format = 32;
					xevent.ClientMessageEvent.ptr1 = (IntPtr) (HoverState.Y << 16 | HoverState.X);

					hwnd.Queue.Enqueue (xevent);
				}
			}
		}

		private void CaretCallback (object sender, EventArgs e)
		{
			if (Caret.Paused) {
				return;
			}
			Caret.On = !Caret.On;

			Xlib.XDrawLine (display, Caret.Hwnd, Caret.gc, Caret.X, Caret.Y, Caret.X, Caret.Y + Caret.Height);
		}

		internal string WhereString ()
		{
			StackTrace	stack;
			StackFrame	frame;
			string		newline;
			string		unknown;
			StringBuilder	sb;
			MethodBase	method;

			newline = String.Format("{0}\t {1} ", Environment.NewLine, Locale.GetText("at"));
			unknown = Locale.GetText("<unknown method>");
			sb = new StringBuilder();
			stack = new StackTrace(true);

			for (int i = 0; i < stack.FrameCount; i++) {
				frame = stack.GetFrame (i);
				sb.Append(newline);

				method = frame.GetMethod();
				if (method != null) {
					if (frame.GetFileLineNumber() != 0)
						sb.AppendFormat ("{0}.{1} () [{2}:{3}]",
								 method.DeclaringType.FullName, method.Name,
								 Path.GetFileName(frame.GetFileName()), frame.GetFileLineNumber());
					else
						sb.AppendFormat ("{0}.{1} ()", method.DeclaringType.FullName, method.Name);
				} else { 
					sb.Append(unknown);
				}
			}
			return sb.ToString();
 		}

		private int HandleError (IntPtr display, ref XErrorEvent error_event)
		{
			if (ErrorExceptions)
				throw new X11Exception (error_event.display, error_event.resourceid,
							error_event.serial, error_event.error_code,
							error_event.request_code, error_event.minor_code);
			else
				Console.WriteLine ("X11 Error encountered: {0}{1}\n",
						   X11Exception.GetMessage(error_event.display, error_event.resourceid,
									   error_event.serial, error_event.error_code,
									   error_event.request_code, error_event.minor_code),
						   WhereString());
			return 0;
		}
		#endregion	// Callbacks

		private void ShowCaret()
		{
			if ((Caret.gc == IntPtr.Zero) || Caret.On) {
				return;
			}
			Caret.On = true;

			Xlib.XDrawLine (display, Caret.Window, Caret.gc, Caret.X, Caret.Y, Caret.X, Caret.Y + Caret.Height);
		}

		private void HideCaret()
		{
			if ((Caret.gc == IntPtr.Zero) || !Caret.On) {
				return;
			}
			Caret.On = false;

			Xlib.XDrawLine (display, Caret.Window, Caret.gc, Caret.X, Caret.Y, Caret.X, Caret.Y + Caret.Height);
		}

		public void CaretVisible (IntPtr handle, bool visible)
		{
			if (Caret.Hwnd == handle) {
				if (visible) {
					if (!Caret.Visible) {
						Caret.Visible = true;
						ShowCaret();
						Caret.Timer.Start();
					}
				} else {
					Caret.Visible = false;
					Caret.Timer.Stop();
					HideCaret();
				}
			}
		}

		public void AudibleAlert ()
		{
			Xlib.XBell (display, 0);
		}

		public void Flush ()
		{
			Xlib.XFlush (display);
		}

		public void Close ()
		{
			// XXX shut down the event_thread
			Xlib.XCloseDisplay (display);
		}

		public IntPtr XGetParent(IntPtr handle)
		{
			IntPtr	Root;
			IntPtr	Parent;
			IntPtr	Children;
			int	ChildCount;

			Xlib.XQueryTree (display, handle, out Root, out Parent, out Children, out ChildCount);

			if (Children!=IntPtr.Zero) {
				Xlib.XFree(Children);
			}

			return Parent;
		}

		public bool SystrayAdd(IntPtr handle, string tip, Icon icon, out ToolTip tt)
		{
			IntPtr SystrayMgrWindow;

			Xlib.XGrabServer (display);
			SystrayMgrWindow = Xlib.XGetSelectionOwner (display, Atoms._NET_SYSTEM_TRAY_S);
			Xlib.XUngrabServer (display);

			if (SystrayMgrWindow != IntPtr.Zero) {
				XSizeHints size_hints;
				X11Hwnd hwnd;

				hwnd = (X11Hwnd)Hwnd.ObjectFromHandle(handle);
#if DriverDebug
				Console.WriteLine("Adding Systray Whole:{0:X}, Client:{1:X}",
						  hwnd.WholeWindow.ToInt32(), hwnd.ClientWindow.ToInt32());
#endif

				// Oh boy.
				if (hwnd.ClientWindow != hwnd.WholeWindow) {
					Xlib.XDestroyWindow (display, hwnd.ClientWindow);
					hwnd.ClientWindow = hwnd.WholeWindow;

					try {
						hwnd.Queue.Lock ();

						/* by virtue of the way the tests are ordered when determining if it's PAINT
						   or NCPAINT, ClientWindow == WholeWindow will always be PAINT.  So, if we're
						   waiting on an nc_expose, drop it and remove the hwnd from the list (unless
						   there's a pending expose). */
						hwnd.PendingNCExpose = false;
					}
					finally {
						hwnd.Queue.Unlock ();
					}
				}

				size_hints = new XSizeHints();

				size_hints.flags = (IntPtr)(XSizeHintsFlags.PMinSize | XSizeHintsFlags.PMaxSize | XSizeHintsFlags.PBaseSize);

 				size_hints.min_width = 24;
 				size_hints.min_height = 24;
 				size_hints.max_width = 24;
 				size_hints.max_height = 24;
 				size_hints.base_width = 24;
 				size_hints.base_height = 24;

				Xlib.XSetWMNormalHints (display, hwnd.WholeWindow, ref size_hints);

				int[] atoms = new int[2];
				atoms [0] = 1;			// Version 1
				atoms [1] = 1;			// we want to be mapped

				// This line cost me 3 days...
				Xlib.XChangeProperty (display,
						      hwnd.WholeWindow, Atoms._XEMBED_INFO, Atoms._XEMBED_INFO, 32,
						      PropertyMode.Replace, atoms, 2);

				// Need to pick some reasonable defaults
				tt = new ToolTip();
				tt.AutomaticDelay = 100;
				tt.InitialDelay = 250;
				tt.ReshowDelay = 250;
				tt.ShowAlways = true;

				if ((tip != null) && (tip != string.Empty)) {
					tt.SetToolTip(Control.FromHandle(handle), tip);
					tt.Active = true;
				} else {
					tt.Active = false;
				}

				SendNetClientMessage (SystrayMgrWindow,
						      Atoms._NET_SYSTEM_TRAY_OPCODE,
						      IntPtr.Zero,
						      (IntPtr)SystrayRequest.SYSTEM_TRAY_REQUEST_DOCK,
						      hwnd.WholeWindow);

				return true;
			}

			tt = null;
			return false;
		}

		public bool SystrayChange (IntPtr handle, string tip, Icon icon, ref ToolTip tt)
		{
			Control	control;

			control = Control.FromHandle(handle);
			if (control != null && tt != null) {
				tt.SetToolTip(control, tip);
				tt.Active = true;
				return true;
			} else {
				return false;
			}
		}

		public void SystrayRemove(IntPtr handle, ref ToolTip tt)
		{
#if GTKSOCKET_SUPPORTS_REPARENTING
			X11Hwnd hwnd;

			hwnd = (X11Hwnd)Hwnd.ObjectFromHandle(handle);

			/* in the XEMBED spec, it mentions 3 ways for a client window to break the protocol with the embedder.
			 * 1. The embedder can unmap the window and reparent to the root window (we should probably handle this...)
			 * 2. The client can reparent its window out of the embedder window.
			 * 3. The client can destroy its window.
			 *
			 * this call to SetParent is case 2, but in
			 * the spec it also mentions that gtk doesn't
			 * support this at present.  Looking at HEAD
			 * gtksocket-x11.c jives with this statement.
			 *
			 * so we can't reparent.  we have to destroy.
			 */
			SetParent(hwnd.WholeWindow, FosterParent);
#else
			Control	control = Control.FromHandle(handle);
			if (control is NotifyIcon.NotifyIconWindow)
				((NotifyIcon.NotifyIconWindow)control).InternalRecreateHandle ();
#endif

			// The caller can now re-dock it later...
			if (tt != null) {
				tt.Dispose();
				tt = null;
			}
		}

		public void ResetMouseHover (X11Hwnd hovering)
		{
			HoverState.Timer.Enabled = hovering != null;
			HoverState.X = MousePosition.X;
			HoverState.Y = MousePosition.Y;
			HoverState.Window = hovering == null ? IntPtr.Zero : hovering.Handle;
		}

		public void ShowCursor (bool show)
		{
			;	// FIXME - X11 doesn't 'hide' the cursor. we could create an empty cursor
		}

		public void SetModal (X11Hwnd hwnd, bool Modal)
		{
			if (Modal) {
				ModalWindows.Push(hwnd);
			} else {
				// XXX do we need to pop until the
				// hwnd is off the stack?  or just the
				// most recently pushed hwnd?
				if (ModalWindows.Contains(hwnd)) {
					ModalWindows.Pop();
				}

				if (ModalWindows.Count > 0) {
					X11Hwnd top_hwnd = (X11Hwnd)ModalWindows.Peek();
					top_hwnd.Activate();
				}
			}
		}

		public TransparencySupport SupportsTransparency ()
		{
			// compiz adds _NET_WM_WINDOW_OPACITY to _NET_SUPPORTED on the root window, check for that
			return ((IList)root_hwnd._NET_SUPPORTED).Contains (Atoms._NET_WM_WINDOW_OPACITY) ? TransparencySupport.GetSet : TransparencySupport.None;
		}

		public void SendAsyncMethod (AsyncMethodData method)
		{
			X11Hwnd hwnd = (X11Hwnd)Hwnd.ObjectFromHandle(method.Handle);
			XEvent xevent = new XEvent ();

			xevent.type = XEventName.ClientMessage;
			xevent.ClientMessageEvent.display = display;
			xevent.ClientMessageEvent.window = method.Handle;
			xevent.ClientMessageEvent.message_type = Atoms.AsyncAtom;
			xevent.ClientMessageEvent.format = 32;
			xevent.ClientMessageEvent.ptr1 = (IntPtr) GCHandle.Alloc (method);

			hwnd.Queue.Enqueue (xevent);
		}

		delegate IntPtr WndProcDelegate (IntPtr hwnd, Msg message, IntPtr wParam, IntPtr lParam);

		public IntPtr SendMessage (IntPtr handle, Msg message, IntPtr wParam, IntPtr lParam)
		{
			X11Hwnd	hwnd = (X11Hwnd)Hwnd.ObjectFromHandle(handle);
			if (hwnd == null)
				return IntPtr.Zero;

			if (hwnd.Queue.Thread != Thread.CurrentThread) {
				AsyncMethodResult	result;
				AsyncMethodData		data;

				result = new AsyncMethodResult ();
				data = new AsyncMethodData ();

				data.Handle = hwnd.Handle;
				data.Method = new WndProcDelegate (NativeWindow.WndProc);
				data.Args = new object[] { hwnd.Handle, message, wParam, lParam };
				data.Result = result;
				
				SendAsyncMethod (data);
#if DriverDebug || DriverDebugThreads
				Console.WriteLine ("Sending {0} message across.", message);
#endif

				return IntPtr.Zero;
			}
			else {
				return NativeWindow.WndProc (hwnd.Handle, message, wParam, lParam);
			}
		}

		public int SendInput (IntPtr handle, Queue keys) {
			if (handle == IntPtr.Zero)
				return 0;

			int count = keys.Count;
			Hwnd hwnd = Hwnd.ObjectFromHandle(handle);

			while (keys.Count > 0) {
			
				MSG msg = (MSG)keys.Dequeue();

				XEvent xevent = new XEvent ();

				xevent.type = (msg.message == Msg.WM_KEYUP ? XEventName.KeyRelease : XEventName.KeyPress);
				xevent.KeyEvent.display = display;

				if (hwnd != null) {
					xevent.KeyEvent.window = hwnd.whole_window;
				} else {
					xevent.KeyEvent.window = IntPtr.Zero;
				}

				xevent.KeyEvent.keycode = Keyboard.ToKeycode((int)msg.wParam);

				hwnd.Queue.EnqueueLocked (xevent);
			}
			return count;
		}

		// FIXME - I think this should just enqueue directly
		public bool PostMessage (IntPtr handle, Msg message, IntPtr wparam, IntPtr lparam)
		{
			XEvent xevent = new XEvent ();
			X11Hwnd hwnd = (X11Hwnd)Hwnd.ObjectFromHandle(handle);

			xevent.type = XEventName.ClientMessage;
			xevent.ClientMessageEvent.display = display;

			if (hwnd != null) {
				xevent.ClientMessageEvent.window = hwnd.WholeWindow;
			} else {
				xevent.ClientMessageEvent.window = IntPtr.Zero;
			}

			xevent.ClientMessageEvent.message_type = Atoms.PostAtom;
			xevent.ClientMessageEvent.format = 32;
			xevent.ClientMessageEvent.ptr1 = handle;
			xevent.ClientMessageEvent.ptr2 = (IntPtr) message;
			xevent.ClientMessageEvent.ptr3 = wparam;
			xevent.ClientMessageEvent.ptr4 = lparam;

			hwnd.Queue.Enqueue (xevent);

			return true;
		}

		public void SendNetWMMessage (IntPtr window, IntPtr message_type, IntPtr l0, IntPtr l1, IntPtr l2)
		{
			XEvent	xev;

			xev = new XEvent();
			xev.ClientMessageEvent.type = XEventName.ClientMessage;
			xev.ClientMessageEvent.send_event = true;
			xev.ClientMessageEvent.window = window;
			xev.ClientMessageEvent.message_type = message_type;
			xev.ClientMessageEvent.format = 32;
			xev.ClientMessageEvent.ptr1 = l0;
			xev.ClientMessageEvent.ptr2 = l1;
			xev.ClientMessageEvent.ptr3 = l2;

			Xlib.XSendEvent (display, root_hwnd.Handle, false,
					 new IntPtr ((int) (EventMask.SubstructureRedirectMask | EventMask.SubstructureNotifyMask)), ref xev);
		}

		public void SendNetClientMessage (IntPtr window, IntPtr message_type, IntPtr l0, IntPtr l1, IntPtr l2)
		{
			XEvent	xev;

			xev = new XEvent();
			xev.ClientMessageEvent.type = XEventName.ClientMessage;
			xev.ClientMessageEvent.send_event = true;
			xev.ClientMessageEvent.window = window;
			xev.ClientMessageEvent.message_type = message_type;
			xev.ClientMessageEvent.format = 32;
			xev.ClientMessageEvent.ptr1 = l0;
			xev.ClientMessageEvent.ptr2 = l1;
			xev.ClientMessageEvent.ptr3 = l2;

			Xlib.XSendEvent (display, window, false, new IntPtr ((int)EventMask.NoEventMask), ref xev);
		}

		public bool TranslateMessage (ref MSG msg)
		{
			return Keyboard.TranslateMessage (ref msg);
		}

		public IntPtr DispatchMessage (ref MSG msg)
		{
			return NativeWindow.WndProc(msg.hwnd, msg.message, msg.wParam, msg.lParam);
		}

		private void QueryPointer (IntPtr w, out IntPtr root, out IntPtr child,
					   out int root_x, out int root_y, out int child_x, out int child_y,
					   out int mask)
		{
			/* this code was written with the help of
			   glance at gdk.  I never would have realized we
			   needed a loop in order to traverse down in the
			   hierarchy.  I would have assumed you'd get the
			   most deeply nested child and have to do
			   XQueryTree to move back up the hierarchy..
			   stupid me, of course. */
			IntPtr c;

			//			Xlib.XGrabServer (display);

			Xlib.XQueryPointer (display, w, out root, out c,
					    out root_x, out root_y, out child_x, out child_y,
					    out mask);

			if (root != w)
				c = root;

			IntPtr child_last = IntPtr.Zero;
			while (c != IntPtr.Zero) {
				child_last = c;
				Xlib.XQueryPointer (display, c, out root, out c,
						    out root_x, out root_y, out child_x, out child_y,
						    out mask);
			}

			//			Xlib.XUngrabServer (display);

			child = child_last;
		}

		public void SetCursorPos (int x, int y)
		{
			IntPtr root, child;
			int root_x, root_y, child_x, child_y, mask;

			/* we need to do a
			 * QueryPointer before warping
			 * because if the warp is on
			 * the RootWindow, the x/y are
			 * relative to the current
			 * mouse position
			 */
			QueryPointer (RootWindow.Handle,
				      out root,
				      out child,
				      out root_x, out root_y,
				      out child_x, out child_y,
				      out mask);

			Xlib.XWarpPointer (display, IntPtr.Zero, IntPtr.Zero, 0, 0, 0, 0, x - root_x, y - root_y);

			Xlib.XFlush (display);

			/* then we need to a
			 * QueryPointer after warping
			 * to manually generate a
			 * motion event for the window
			 * we move into.
			 */
			QueryPointer (RootWindow.Handle,
				      out root,
				      out child,
				      out root_x, out root_y,
				      out child_x, out child_y,
				      out mask);

			X11Hwnd child_hwnd = (X11Hwnd)Hwnd.ObjectFromHandle(child);
			if (child_hwnd == null)
				return;

			XEvent xevent = new XEvent ();

			xevent.type = XEventName.MotionNotify;
			xevent.MotionEvent.display = display;
			xevent.MotionEvent.window = child_hwnd.Handle;
			xevent.MotionEvent.root = RootWindow.Handle;
			xevent.MotionEvent.x = child_x;
			xevent.MotionEvent.y = child_y;
			xevent.MotionEvent.x_root = root_x;
			xevent.MotionEvent.y_root = root_y;
			xevent.MotionEvent.state = mask;

			child_hwnd.Queue.Enqueue (xevent);
		}

		public void SetFocus (X11Hwnd new_focus)
		{
			if (new_focus == FocusWindow)
				return;

			X11Hwnd prev_focus = FocusWindow;
			FocusWindow = new_focus;

			if (prev_focus != null)
				SendMessage (prev_focus.Handle, Msg.WM_KILLFOCUS,
					     FocusWindow == null ? IntPtr.Zero : FocusWindow.Handle, IntPtr.Zero);
			if (FocusWindow != null)
				SendMessage (FocusWindow.Handle, Msg.WM_SETFOCUS,
					     prev_focus == null ? IntPtr.Zero : prev_focus.Handle, IntPtr.Zero);

			//XSetInputFocus(DisplayHandle, Hwnd.ObjectFromHandle(handle).ClientWindow, RevertTo.None, IntPtr.Zero);
		}

		public IntPtr DefineCursor (Bitmap bitmap, Bitmap mask, Color cursor_pixel, Color mask_pixel, int xHotSpot, int yHotSpot)
		{
			IntPtr	cursor;
			Bitmap	cursor_bitmap;
			Bitmap	cursor_mask;
			Byte[]	cursor_bits;
			Byte[]	mask_bits;
			Color	c_pixel;
			Color	m_pixel;
			int	width;
			int	height;
			IntPtr	cursor_pixmap;
			IntPtr	mask_pixmap;
			XColor	fg;
			XColor	bg;
			bool	and;
			bool	xor;

			if (Xlib.XQueryBestCursor (display, RootWindow.Handle, bitmap.Width, bitmap.Height, out width, out height) == 0) {
				return IntPtr.Zero;
			}

			// Win32 only allows creation cursors of a certain size
			if ((bitmap.Width != width) || (bitmap.Width != height)) {
				cursor_bitmap = new Bitmap(bitmap, new Size(width, height));
				cursor_mask = new Bitmap(mask, new Size(width, height));
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
					c_pixel = cursor_bitmap.GetPixel(x, y);
					m_pixel = cursor_mask.GetPixel(x, y);

					and = c_pixel == cursor_pixel;
					xor = m_pixel == mask_pixel;

					if (!and && !xor) {
						// Black
						// cursor_bits[y * width / 8 + x / 8] &= (byte)~((1 << (x % 8)));	// The bit already is 0
						mask_bits[y * width / 8 + x / 8] |= (byte)(1 << (x % 8));
					} else if (and && !xor) {
						// White
						cursor_bits[y * width / 8 + x / 8] |= (byte)(1 << (x % 8));
						mask_bits[y * width / 8 + x / 8] |= (byte)(1 << (x % 8));
#if notneeded
					} else if (and && !xor) {
						// Screen
					} else if (and && xor) {
						// Inverse Screen

						// X11 doesn't know the 'reverse screen' concept, so we'll treat them the same
						// we want both to be 0 so nothing to be done
						//cursor_bits[y * width / 8 + x / 8] &= (byte)~((1 << (x % 8)));
						//mask_bits[y * width / 8 + x / 8] |= (byte)(01 << (x % 8));
#endif
					}
				}
			}

			cursor_pixmap = Xlib.XCreatePixmapFromBitmapData (display, RootWindow.Handle,
									  cursor_bits, width, height, (IntPtr)1, (IntPtr)0, 1);
			mask_pixmap = Xlib.XCreatePixmapFromBitmapData (display, RootWindow.Handle,
									mask_bits, width, height, (IntPtr)1, (IntPtr)0, 1);
			fg = new XColor();
			bg = new XColor();

			fg.pixel = Xlib.XWhitePixel (display, DefaultScreen);
			fg.red = (ushort)65535;
			fg.green = (ushort)65535;
			fg.blue = (ushort)65535;

			bg.pixel = Xlib.XBlackPixel (display, DefaultScreen);

			cursor = Xlib.XCreatePixmapCursor (display, cursor_pixmap, mask_pixmap, ref fg, ref bg, xHotSpot, yHotSpot);

			Xlib.XFreePixmap (display, cursor_pixmap);
			Xlib.XFreePixmap (display, mask_pixmap);

			return cursor;
		}
		
		public Bitmap DefineStdCursorBitmap (StdCursor id)
		{
			CursorFontShape shape;
			string name;
			IntPtr theme;
			int size;
			Bitmap bmp = null;

			try {
				shape = XplatUIX11.StdCursorToFontShape (id);
				name = shape.ToString ().Replace ("XC_", string.Empty);
				size = XplatUIX11.XcursorGetDefaultSize (Handle);
				theme = XplatUIX11.XcursorGetTheme (Handle);
				IntPtr images_ptr = XplatUIX11.XcursorLibraryLoadImages (name, theme, size);
#if debug
				Console.WriteLine ("DefineStdCursorBitmap, id={0}, #id={1}, name{2}, size={3}, theme: {4}, images_ptr={5}", id, (int) id, name, size, Marshal.PtrToStringAnsi (theme), images_ptr);
#endif

				if (images_ptr == IntPtr.Zero) {
					return null;
				}

				XcursorImages images = (XcursorImages)Marshal.PtrToStructure (images_ptr, typeof (XcursorImages));
#if debug
				Console.WriteLine ("DefineStdCursorBitmap, cursor has {0} images", images.nimage);
#endif

				if (images.nimage > 0) {
					// We only care about the first image.
					XcursorImage image = (XcursorImage)Marshal.PtrToStructure (Marshal.ReadIntPtr (images.images), typeof (XcursorImage));

#if debug
					Console.WriteLine ("DefineStdCursorBitmap, loaded image <size={0}, height={1}, width={2}, xhot={3}, yhot={4}, pixels={5}", image.size, image.height, image.width, image.xhot, image.yhot, image.pixels);
#endif
					// A sanity check
					if (image.width <= short.MaxValue && image.height <= short.MaxValue) {
						int [] pixels = new int [image.width * image.height];
						Marshal.Copy (image.pixels, pixels, 0, pixels.Length);
						bmp = new Bitmap (image.width, image.height);
						for (int w = 0; w < image.width; w++) {
							for (int h = 0; h < image.height; h++) {
								bmp.SetPixel (w, h, Color.FromArgb (pixels [h * image.width + w]));
							}
						}
					}
				}

				XplatUIX11.XcursorImagesDestroy (images_ptr);

			} catch (DllNotFoundException ex) {
				Console.WriteLine ("Could not load libXcursor: " + ex.Message + " (" + ex.GetType ().Name + ")");
				return null;
			}

			return bmp;
		}
		
		public IntPtr DefineStdCursor (StdCursor id)
		{
			CursorFontShape	shape;

			// FIXME - define missing shapes

			switch (id) {
			case StdCursor.AppStarting:
				shape = CursorFontShape.XC_watch;
				break;

			case StdCursor.Arrow:
				shape = CursorFontShape.XC_top_left_arrow;
				break;

			case StdCursor.Cross:
				shape = CursorFontShape.XC_crosshair;
				break;

			case StdCursor.Default:
				shape = CursorFontShape.XC_top_left_arrow;
				break;

			case StdCursor.Hand:
				shape = CursorFontShape.XC_hand1;
				break;

			case StdCursor.Help:
				shape = CursorFontShape.XC_question_arrow;
				break;
			
			case StdCursor.HSplit:
				shape = CursorFontShape.XC_sb_v_double_arrow; 
				break;

			case StdCursor.IBeam:
				shape = CursorFontShape.XC_xterm; 
				break;

			case StdCursor.No:
				shape = CursorFontShape.XC_circle; 
				break;

			case StdCursor.NoMove2D:
				shape = CursorFontShape.XC_fleur; 
				break;

			case StdCursor.NoMoveHoriz:
				shape = CursorFontShape.XC_fleur; 
				break;

			case StdCursor.NoMoveVert:
				shape = CursorFontShape.XC_fleur; 
				break;

			case StdCursor.PanEast:
				shape = CursorFontShape.XC_fleur; 
				break;

			case StdCursor.PanNE:
				shape = CursorFontShape.XC_fleur; 
				break;

			case StdCursor.PanNorth:
				shape = CursorFontShape.XC_fleur; 
				break;

			case StdCursor.PanNW:
				shape = CursorFontShape.XC_fleur; 
				break;

			case StdCursor.PanSE:
				shape = CursorFontShape.XC_fleur; 
				break;

			case StdCursor.PanSouth:
				shape = CursorFontShape.XC_fleur; 
				break;

			case StdCursor.PanSW:
				shape = CursorFontShape.XC_fleur; 
				break;

			case StdCursor.PanWest:
				shape = CursorFontShape.XC_sizing; 
				break;

			case StdCursor.SizeAll:
				shape = CursorFontShape.XC_fleur; 
				break;

			case StdCursor.SizeNESW:
				shape = CursorFontShape.XC_top_right_corner; 
				break;

			case StdCursor.SizeNS:
				shape = CursorFontShape.XC_sb_v_double_arrow;
				break;

			case StdCursor.SizeNWSE:
				shape = CursorFontShape.XC_top_left_corner; 
				break;

			case StdCursor.SizeWE:
				shape = CursorFontShape.XC_sb_h_double_arrow; 
				break;

			case StdCursor.UpArrow:
				shape = CursorFontShape.XC_center_ptr; 
				break;

			case StdCursor.VSplit:
				shape = CursorFontShape.XC_sb_h_double_arrow;
				break;

			case StdCursor.WaitCursor:
				shape = CursorFontShape.XC_watch; 
				break;

			default:
				return IntPtr.Zero;
			}

			return Xlib.XCreateFontCursor (display, shape);
		}

		// XXX this should take an X11Hwnd.
		public void CreateCaret (IntPtr handle, int width, int height)
		{
			XGCValues gc_values;
			X11Hwnd hwnd;

			hwnd = (X11Hwnd)Hwnd.ObjectFromHandle(handle);

			if (Caret.Hwnd != IntPtr.Zero)
				DestroyCaret(Caret.Hwnd);

			Caret.Hwnd = handle;
			Caret.Window = hwnd.ClientWindow;
			Caret.Width = width;
			Caret.Height = height;
			Caret.Visible = false;
			Caret.On = false;

			gc_values = new XGCValues();
			gc_values.line_width = width;

			Caret.gc = Xlib.XCreateGC (display, Caret.Window, new IntPtr ((int)GCFunction.GCLineWidth), ref gc_values);
			if (Caret.gc == IntPtr.Zero) {
				Caret.Hwnd = IntPtr.Zero;
				return;
			}

			Xlib.XSetFunction (display, Caret.gc, GXFunction.GXinvert);
		}


		// XXX this should take an X11Hwnd.
		public void DestroyCaret (IntPtr handle)
		{
			if (Caret.Hwnd == handle) {
				if (Caret.Visible == true) {
					Caret.Timer.Stop ();
				}
				if (Caret.gc != IntPtr.Zero) {
					Xlib.XFreeGC (display, Caret.gc);
					Caret.gc = IntPtr.Zero;
				}
				Caret.Hwnd = IntPtr.Zero;
				Caret.Visible = false;
				Caret.On = false;
			}
		}

		public void SetCaretPos (IntPtr handle, int x, int y)
		{
			if (Caret.Hwnd == handle) {
				Caret.Timer.Stop();
				HideCaret();

				Caret.X = x;
				Caret.Y = y;

				if (Caret.Visible == true) {
					ShowCaret();
					Caret.Timer.Start();
				}
			}
		}

		public void DestroyCursor (IntPtr cursor)
		{
			Xlib.XFreeCursor (display, cursor);
		}

		private void AccumulateDestroyedHandles (Control c, ArrayList list)
		{
			if (c != null) {
				Control[] controls = c.Controls.GetAllControls ();

				if (c.IsHandleCreated && !c.IsDisposed) {
					X11Hwnd hwnd = (X11Hwnd)Hwnd.ObjectFromHandle(c.Handle);

#if DriverDebug || DriverDebugDestroy
					Console.WriteLine (" + adding {0} to the list of zombie windows", XplatUI.Window (hwnd.Handle));
					Console.WriteLine (" + parent X window is {0:X}", XGetParent (hwnd.WholeWindow).ToInt32());
#endif

					list.Add (hwnd);
					CleanupCachedWindows (hwnd);
					hwnd.zombie = true;
				}

				for (int  i = 0; i < controls.Length; i ++) {
					AccumulateDestroyedHandles (controls[i], list);
				}
			}
			
		}

		void CleanupCachedWindows (X11Hwnd hwnd)
		{
			if (ActiveWindow == hwnd) {
				SendMessage (hwnd.ClientWindow, Msg.WM_ACTIVATE, (IntPtr)WindowActiveFlags.WA_INACTIVE, IntPtr.Zero);
				ActiveWindow = null;
			}

			if (FocusWindow == hwnd) {
				SendMessage (hwnd.ClientWindow, Msg.WM_KILLFOCUS, IntPtr.Zero, IntPtr.Zero);
				FocusWindow = null;
			}

			if (Grab.Hwnd == hwnd.Handle) {
				Grab.Hwnd = IntPtr.Zero;
				Grab.Confined = false;
			}

			DestroyCaret (hwnd.Handle);
		}


		public void DestroyWindow (X11Hwnd hwnd)
		{
			CleanupCachedWindows (hwnd);

			hwnd.SendParentNotify (Msg.WM_DESTROY, int.MaxValue, int.MaxValue);

			ArrayList windows = new ArrayList ();

			AccumulateDestroyedHandles (Control.ControlNativeWindow.ControlFromHandle(hwnd.Handle), windows);

			hwnd.DestroyWindow ();

			foreach (X11Hwnd h in windows) {
				SendMessage (h.Handle, Msg.WM_DESTROY, IntPtr.Zero, IntPtr.Zero);
			}
		}

		public X11Hwnd GetActiveWindow ()
		{
			IntPtr	actual_atom;
			int	actual_format;
			IntPtr	nitems;
			IntPtr	bytes_after;
			IntPtr	prop = IntPtr.Zero;
			IntPtr	active = IntPtr.Zero;

			Xlib.XGetWindowProperty (display, RootWindow.Handle,
						 Atoms._NET_ACTIVE_WINDOW, IntPtr.Zero, new IntPtr (1), false,
						 Atoms.XA_WINDOW, out actual_atom, out actual_format, out nitems, out bytes_after, ref prop);

			if (((long)nitems > 0) && (prop != IntPtr.Zero)) {
				active = (IntPtr)Marshal.ReadInt32(prop);
				Xlib.XFree(prop);
			}

			return (X11Hwnd)Hwnd.GetObjectFromWindow(active);
		}

		public void SetActiveWindow (X11Hwnd new_active_window)
		{
			if (new_active_window != ActiveWindow) {
				if (ActiveWindow != null)
					PostMessage (ActiveWindow.Handle, Msg.WM_ACTIVATE,
						     (IntPtr)WindowActiveFlags.WA_INACTIVE, IntPtr.Zero);

				ActiveWindow = new_active_window;

				if (ActiveWindow != null)
					PostMessage (ActiveWindow.Handle, Msg.WM_ACTIVATE,
						     (IntPtr)WindowActiveFlags.WA_ACTIVE, IntPtr.Zero);
                        }

			if (ModalWindows.Count > 0) {
				// Modality handling, if we are modal and the new active window is one
				// of ours but not the modal one, switch back to the modal window

				if (ActiveWindow != null &&
				    NativeWindow.FromHandle (ActiveWindow.Handle) != null) {
					if (ActiveWindow != (X11Hwnd)ModalWindows.Peek())
						((X11Hwnd)ModalWindows.Peek()).Activate ();
				}
			}
                }

		public void GetDisplaySize (out Size size)
		{
			XWindowAttributes attributes = new XWindowAttributes();

			// FIXME - use _NET_WM messages instead?
			Xlib.XGetWindowAttributes (display, RootWindow.Handle, ref attributes);

			size = new Size(attributes.width, attributes.height);
		}

		// XXX this method doesn't really fit well anywhere in the backend
		public SizeF GetAutoScaleSize (Font font)
		{
			Graphics	g;
			float		width;
			string		magic_string = "The quick brown fox jumped over the lazy dog.";
			double		magic_number = 44.549996948242189; // XXX my god, where did this number come from?

			g = Graphics.FromHwnd (FosterParent.Handle);

			width = (float) (g.MeasureString (magic_string, font).Width / magic_number);
			return new SizeF(width, font.Height);
		}

		public void GetCursorPos (X11Hwnd hwnd, out int x, out int y)
		{
			IntPtr	use_handle;
			IntPtr	root;
			IntPtr	child;
			int	root_x;
			int	root_y;
			int	win_x;
			int	win_y;
			int	keys_buttons;

			if (hwnd != null)
				use_handle = hwnd.Handle;
			else
				use_handle = RootWindow.Handle;

			QueryPointer (use_handle, out root, out child, out root_x, out root_y, out win_x, out win_y, out keys_buttons);

			if (hwnd != null) {
				x = win_x;
				y = win_y;
			} else {
				x = root_x;
				y = root_y;
			}
		}

		public IntPtr GetFocus ()
		{
			return FocusWindow.Handle;
		}

		public IntPtr GetMousewParam (int Delta)
		{
			int	result = 0;

			if ((MouseState & MouseButtons.Left) != 0) {
				result |= (int)MsgButtons.MK_LBUTTON;
			}

			if ((MouseState & MouseButtons.Middle) != 0) {
				result |= (int)MsgButtons.MK_MBUTTON;
			}

			if ((MouseState & MouseButtons.Right) != 0) {
				result |= (int)MsgButtons.MK_RBUTTON;
			}

			Keys mods = ModifierKeys;
			if ((mods & Keys.Control) != 0) {
				result |= (int)MsgButtons.MK_CONTROL;
			}

			if ((mods & Keys.Shift) != 0) {
				result |= (int)MsgButtons.MK_SHIFT;
			}

			result |= Delta << 16;

			return (IntPtr)result;
		}

		public void GrabInfo (out IntPtr handle, out bool GrabConfined, out Rectangle GrabArea)
		{
			handle = Grab.Hwnd;
			GrabConfined = Grab.Confined;
			GrabArea = Grab.Area;
		}

		public void GrabWindow (X11Hwnd hwnd, X11Hwnd confine_to)
		{
			IntPtr	confine_to_window;

			confine_to_window = IntPtr.Zero;

			if (confine_to != null) {
				Console.WriteLine (Environment.StackTrace);

				XWindowAttributes attributes = new XWindowAttributes();

				Xlib.XGetWindowAttributes (display, confine_to.ClientWindow, ref attributes);

				Grab.Area.X = attributes.x;
				Grab.Area.Y = attributes.y;
				Grab.Area.Width = attributes.width;
				Grab.Area.Height = attributes.height;
				Grab.Confined = true;
				confine_to_window = confine_to.ClientWindow;
			}

			Grab.Hwnd = hwnd.ClientWindow;

			Xlib.XGrabPointer (display, hwnd.ClientWindow, false, 
					   EventMask.ButtonPressMask | EventMask.ButtonMotionMask |
					   EventMask.ButtonReleaseMask | EventMask.PointerMotionMask,
					   GrabMode.GrabModeAsync, GrabMode.GrabModeAsync, confine_to_window, IntPtr.Zero, IntPtr.Zero);
		}

		public void UngrabWindow (X11Hwnd hwnd)
		{
			Xlib.XUngrabPointer (display, IntPtr.Zero);
			Xlib.XFlush (display);

			// XXX make sure hwnd is what should have the grab and throw if not
			Grab.Hwnd = IntPtr.Zero;
			Grab.Confined = false;
		}

#if notyet
		private void TranslatePropertyToClipboard (IntPtr property)
		{
			IntPtr actual_atom;
			int actual_format;
			IntPtr nitems;
			IntPtr bytes_after;
			IntPtr prop = IntPtr.Zero;

			Clipboard.Item = null;

			Xlib.XGetWindowProperty (display, FosterParent.Handle,
						 property, IntPtr.Zero, new IntPtr (0x7fffffff), true,
						 Atoms.AnyPropertyType, out actual_atom, out actual_format, out nitems, out bytes_after, ref prop);

			if ((long)nitems > 0) {
				if (property == Atoms.XA_STRING) {
					Clipboard.Item = Marshal.PtrToStringAnsi(prop);
				} else if (property == Atoms.XA_BITMAP) {
					// FIXME - convert bitmap to image
				} else if (property == Atoms.XA_PIXMAP) {
					// FIXME - convert pixmap to image
				} else if (property == Atoms.OEMTEXT) {
					Clipboard.Item = Marshal.PtrToStringAnsi(prop);
				} else if (property == Atoms.UNICODETEXT) {
					Clipboard.Item = Marshal.PtrToStringAnsi(prop);
				}

				Xlib.XFree(prop);
			}
		}
#endif

		// XXX should we be using @handle instead of Atoms.CLIPBOARD here?
		public int[] ClipboardAvailableFormats (IntPtr handle)
		{
			// XXX deal with the updatemessagequeue stuff
#if true
			return new int[0];
#else
			DataFormats.Format f;
			int[] result;

			f = DataFormats.Format.List;

			if (Xlib.XGetSelectionOwner (display, Atoms.CLIPBOARD) == IntPtr.Zero) {
				return null;
			}

			Clipboard.Formats = new ArrayList();

			while (f != null) {
				Xlib.XConvertSelection (display, Atoms.CLIPBOARD, (IntPtr)f.Id, (IntPtr)f.Id, FosterParent.Handle, IntPtr.Zero);

				Clipboard.Enumerating = true;
				while (Clipboard.Enumerating) {
					UpdateMessageQueue(null);
				}
				f = f.Next;
			}

			result = new int[Clipboard.Formats.Count];

			for (int i = 0; i < Clipboard.Formats.Count; i++) {
				result[i] = ((IntPtr)Clipboard.Formats[i]).ToInt32 ();
			}

			Clipboard.Formats = null;
			return result;
#endif
		}

		public void ClipboardClose (IntPtr handle)
		{
			if (handle != ClipMagic) {
				throw new ArgumentException("handle is not a valid clipboard handle");
			}
			return;
		}

		public int ClipboardGetID (IntPtr handle, string format)
		{
			if (handle != ClipMagic) {
				throw new ArgumentException("handle is not a valid clipboard handle");
			}

			if (format == "Text" ) return Atoms.XA_STRING.ToInt32();
			else if (format == "Bitmap" ) return Atoms.XA_BITMAP.ToInt32();
			//else if (format == "MetaFilePict" ) return 3;
			//else if (format == "SymbolicLink" ) return 4;
			//else if (format == "DataInterchangeFormat" ) return 5;
			//else if (format == "Tiff" ) return 6;
			else if (format == "OEMText" ) return Atoms.OEMTEXT.ToInt32();
			else if (format == "DeviceIndependentBitmap" ) return Atoms.XA_PIXMAP.ToInt32();
			else if (format == "Palette" ) return Atoms.XA_COLORMAP.ToInt32();	// Useless
			//else if (format == "PenData" ) return 10;
			//else if (format == "RiffAudio" ) return 11;
			//else if (format == "WaveAudio" ) return 12;
			else if (format == "UnicodeText" ) return Atoms.UNICODETEXT.ToInt32();
			//else if (format == "EnhancedMetafile" ) return 14;
			//else if (format == "FileDrop" ) return 15;
			//else if (format == "Locale" ) return 16;

			return Xlib.XInternAtom (display, format, false).ToInt32();
		}

		public IntPtr ClipboardOpen (bool primary_selection)
		{
			if (!primary_selection)
				ClipMagic = Atoms.CLIPBOARD;
			else
				ClipMagic = Atoms.PRIMARY;

			return ClipMagic;
		}

		// XXX @converter?
		public object ClipboardRetrieve (IntPtr handle, int type, XplatUI.ClipboardToObject converter)
		{
			// XXX deal with the UpdateMessageQueue stuff
#if true
			return null;
#else
			Xlib.XConvertSelection (display, handle, (IntPtr)type, (IntPtr)type, FosterParent, IntPtr.Zero);

			Clipboard.Retrieving = true;
			while (Clipboard.Retrieving) {
				UpdateMessageQueue(null);
			}

			return Clipboard.Item;
#endif
		}

		public void ClipboardStore (IntPtr handle, object obj, int type, XplatUI.ObjectToClipboard converter)
		{
			Clipboard.Item = obj;
			Clipboard.Type = type;
			Clipboard.Converter = converter;

			if (obj != null) {
				Xlib.XSetSelectionOwner (display, Atoms.CLIPBOARD, FosterParent.Handle, IntPtr.Zero);
			} else {
				// Clearing the selection
				Xlib.XSetSelectionOwner (display, Atoms.CLIPBOARD, IntPtr.Zero, IntPtr.Zero);
			}
		}


		public PaintEventArgs PaintEventStart (ref Message m, IntPtr handle, bool client)
		{
			X11Hwnd hwnd = (X11Hwnd)Hwnd.ObjectFromHandle(handle);

			if (Caret.Visible == true) {
				Caret.Paused = true;
				HideCaret();
			}

			return hwnd.PaintEventStart (ref m, client);
		}

		public void PaintEventEnd (ref Message m, IntPtr handle, bool client, PaintEventArgs pevent)
		{
			X11Hwnd hwnd = (X11Hwnd)Hwnd.ObjectFromHandle(handle);

			hwnd.PaintEventEnd (ref m, client, pevent);

			if (Caret.Visible == true) {
				ShowCaret();
				Caret.Paused = false;
			}
		}

		public void SetCursor (IntPtr handle, IntPtr cursor)
		{
			Hwnd	hwnd;

			if (OverrideCursorHandle == IntPtr.Zero) {
				if ((LastCursorWindow == handle) && (LastCursorHandle == cursor))
					return;

				LastCursorHandle = cursor;
				LastCursorWindow = handle;

				hwnd = Hwnd.ObjectFromHandle(handle);
				if (cursor != IntPtr.Zero)
					Xlib.XDefineCursor (display, hwnd.whole_window, cursor);
				else
					Xlib.XUndefineCursor (display, hwnd.whole_window);
				Xlib.XFlush (display);
			}
			else {
				hwnd = Hwnd.ObjectFromHandle(handle);
				Xlib.XDefineCursor (display, hwnd.whole_window, OverrideCursorHandle);
			}
		}

		public DragDropEffects StartDrag (IntPtr handle, object data,
						  DragDropEffects allowed_effects)
		{
			X11Hwnd hwnd = (X11Hwnd)Hwnd.ObjectFromHandle (handle);

			if (hwnd == null)
				throw new ArgumentException ("Attempt to begin drag from invalid window handle (" + handle.ToInt32 () + ").");

			return Dnd.StartDrag (hwnd.ClientWindow, data, allowed_effects);
		}

		public X11Atoms Atoms {
			get { return atoms; }
		}

		public int CurrentTimestamp {
			get {
				TimeSpan t = (DateTime.UtcNow - new DateTime(1970, 1, 1));

				return (int) t.TotalSeconds;
			}
		}

		public Size CursorSize {
			get {
				int	x;
				int	y;

				if (Xlib.XQueryBestCursor (display, RootWindow.Handle, 32, 32, out x, out y) != 0) {
					return new Size (x, y);
				} else {
					return new Size (16, 16);
				}
			}
		} 

		public IntPtr Handle {
			get { return display; }
		}

		public Size IconSize {
			get {
				IntPtr		list;
				XIconSize	size;
				int		count;

				if (Xlib.XGetIconSizes (display, RootWindow.Handle, out list, out count) != 0) {
					long		current;
					int		largest;

					current = (long)list;
					largest = 0;

					size = new XIconSize();

					for (int i = 0; i < count; i++) {
						size = (XIconSize)Marshal.PtrToStructure((IntPtr)current, size.GetType());
						current += Marshal.SizeOf(size);

						// Look for our preferred size
						if (size.min_width == 32) {
							Xlib.XFree(list);
							return new Size(32, 32);
						}

						if (size.max_width == 32) {
							Xlib.XFree(list);
							return new Size(32, 32);
						}

						if (size.min_width < 32 && size.max_width > 32) {
							int	x;

							// check if we can fit one
							x = size.min_width;
							while (x < size.max_width) {
								x += size.width_inc;
								if (x == 32) {
									Xlib.XFree(list);
									return new Size(32, 32);
								}
							}
						}

						if (largest < size.max_width) {
							largest = size.max_width;
						}
					}

					// We didn't find a match or we wouldn't be here
					return new Size(largest, largest);

				} else {
					return new Size(32, 32);
				}
			}
		} 

		public int KeyboardSpeed {
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

		public int KeyboardDelay {
			get {
				//
				// Return values must range from 0 to 4, 0 meaning 250ms,
				// and 4 meaning 1000 ms.
				//
				return 1; // ie, 500 ms
			}
		} 

		public int DefaultScreen {
			get { return Xlib.XDefaultScreen (display); }
		}

		public IntPtr DefaultColormap {
			// XXX multiscreen
			get { return Xlib.XDefaultColormap (display, DefaultScreen); }
		}

		public Keys ModifierKeys {
			get { return Keyboard.ModifierKeys; }
		}

		public IntPtr OverrideCursor {
			get { return OverrideCursorHandle; }
			set {
				if (Grab.Hwnd != IntPtr.Zero) {
					Xlib.XChangeActivePointerGrab (display,
								       EventMask.ButtonMotionMask |
								       EventMask.PointerMotionMask |
								       EventMask.ButtonPressMask |
								       EventMask.ButtonReleaseMask,
								       value, IntPtr.Zero);
					return;
				}

				OverrideCursorHandle = value;
			}
		}

		public X11RootHwnd RootWindow {
			get { return root_hwnd; }
		}

		public Size SmallIconSize {
			get {
				IntPtr		list;
				XIconSize	size;
				int		count;

				if (Xlib.XGetIconSizes (display, RootWindow.Handle, out list, out count) != 0) {
					long		current;
					int		smallest;

					current = (long)list;
					smallest = 0;

					size = new XIconSize();

					for (int i = 0; i < count; i++) {
						size = (XIconSize)Marshal.PtrToStructure((IntPtr)current, size.GetType());
						current += Marshal.SizeOf(size);

						// Look for our preferred size
						if (size.min_width == 16) {
							Xlib.XFree(list);
							return new Size(16, 16);
						}

						if (size.max_width == 16) {
							Xlib.XFree(list);
							return new Size(16, 16);
						}

						if (size.min_width < 16 && size.max_width > 16) {
							int	x;

							// check if we can fit one
							x = size.min_width;
							while (x < size.max_width) {
								x += size.width_inc;
								if (x == 16) {
									Xlib.XFree(list);
									return new Size(16, 16);
								}
							}
						}

						if (smallest == 0 || smallest > size.min_width) {
							smallest = size.min_width;
						}
					}

					// We didn't find a match or we wouldn't be here
					return new Size(smallest, smallest);

				} else {
					return new Size(16, 16);
				}
			}
		} 

		public X11Hwnd FosterParent {
			get { return foster_hwnd; }
		}

		public int MouseHoverTime {
			get { return HoverState.Interval; }
		}

		public Rectangle VirtualScreen {
			get {
				IntPtr actual_atom;
				int actual_format;
				IntPtr nitems;
				IntPtr bytes_after;
				IntPtr prop = IntPtr.Zero;
				int width;
				int height;

				Xlib.XGetWindowProperty (display, RootWindow.Handle,
							 Atoms._NET_DESKTOP_GEOMETRY, IntPtr.Zero, new IntPtr (256), false, Atoms.XA_CARDINAL,
							 out actual_atom, out actual_format, out nitems, out bytes_after, ref prop);

				if ((long)nitems < 2)
					goto failsafe;

				width = Marshal.ReadIntPtr(prop, 0).ToInt32();
				height = Marshal.ReadIntPtr(prop, IntPtr.Size).ToInt32();
				Xlib.XFree(prop);

				return new Rectangle(0, 0, width, height);

			failsafe:
				XWindowAttributes attributes = new XWindowAttributes();

				Xlib.XGetWindowAttributes (display, RootWindow.Handle, ref attributes);

				return new Rectangle(0, 0, attributes.width, attributes.height);
			}
		}

		public Rectangle WorkingArea {
			get {
				IntPtr actual_atom;
				int actual_format;
				IntPtr nitems;
				IntPtr bytes_after;
				IntPtr prop = IntPtr.Zero;
				int width;
				int height;
				int current_desktop;
				int x;
				int y;

				Xlib.XGetWindowProperty (display, RootWindow.Handle, 
							 Atoms._NET_CURRENT_DESKTOP, IntPtr.Zero, new IntPtr(1), false, Atoms.XA_CARDINAL,
							 out actual_atom, out actual_format, out nitems, out bytes_after, ref prop);

				if ((long)nitems < 1) {
					goto failsafe;
				}

				current_desktop = Marshal.ReadIntPtr(prop, 0).ToInt32();
				Xlib.XFree(prop);

				Xlib.XGetWindowProperty (display, RootWindow.Handle,
							 Atoms._NET_WORKAREA, IntPtr.Zero, new IntPtr (256), false, Atoms.XA_CARDINAL,
							 out actual_atom, out actual_format, out nitems, out bytes_after, ref prop);

				if ((long)nitems < 4 * current_desktop) {
					goto failsafe;
				}

				x = Marshal.ReadIntPtr(prop, IntPtr.Size * 4 * current_desktop).ToInt32();
				y = Marshal.ReadIntPtr(prop, IntPtr.Size * 4 * current_desktop + IntPtr.Size).ToInt32();
				width = Marshal.ReadIntPtr(prop, IntPtr.Size * 4 * current_desktop + IntPtr.Size * 2).ToInt32();
				height = Marshal.ReadIntPtr(prop, IntPtr.Size * 4 * current_desktop + IntPtr.Size * 3).ToInt32();
				Xlib.XFree(prop);

				return new Rectangle(x, y, width, height);

			failsafe:
				XWindowAttributes attributes = new XWindowAttributes();

				Xlib.XGetWindowAttributes (display, RootWindow.Handle, ref attributes);

				return new Rectangle(0, 0, attributes.width, attributes.height);
			}
		}

		private void XEventThread ()
		{
			while (true) {
				Syscall.poll (pollfds, 1U, -1);

				while (Xlib.XPending (display) > 0) {
					XEvent xevent = new XEvent ();
					Xlib.XNextEvent (display, ref xevent);

					// this is kind of a gross place to put this, but we don't know about the
					// key repeat state in X11ThreadQueue, nor to we want the queue code calling
					// XPeekEvent.
					if (!detectable_key_auto_repeat &&
					    xevent.type == XEventName.KeyRelease &&
					    Xlib.XPending (display) > 0) {

						XEvent nextevent = new XEvent ();
						Xlib.XPeekEvent (display, ref nextevent);

						if (nextevent.type == XEventName.KeyPress &&
						    nextevent.KeyEvent.keycode == xevent.KeyEvent.keycode &&
						    nextevent.KeyEvent.time == xevent.KeyEvent.time) {
							continue;
						}
					}

					X11Hwnd hwnd = (X11Hwnd)Hwnd.GetObjectFromWindow(xevent.AnyEvent.window);
					if (hwnd != null)
						hwnd.Queue.Enqueue (xevent);
				}
			}
		}

		private void RedirectMsgToEnabledAncestor (X11Hwnd hwnd, MSG msg, IntPtr window,
							   ref int event_x, ref int event_y)
		{
			int x, y;

			IntPtr dummy;
			msg.hwnd = hwnd.EnabledHwnd;
			Xlib.XTranslateCoordinates (display, window,
						    Hwnd.ObjectFromHandle(msg.hwnd).ClientWindow,
						    event_x, event_y,
						    out x, out y, out dummy);
			event_x = x;
			event_y = y;
			msg.lParam = (IntPtr)(MousePosition.Y << 16 | MousePosition.X);
		}


		// This is called from the thread owning the corresponding X11ThreadQueue
		[MonoTODO("Implement filtering")]
		public bool GetMessage (object queue_id, ref MSG msg, IntPtr handle, int wFilterMin, int wFilterMax)
		{
			X11ThreadQueue queue = (X11ThreadQueue)queue_id;
			XEvent xevent;
			bool client;
			bool got_xevent = false;

			X11Hwnd hwnd;

		ProcessNextMessage:
			do {
				got_xevent = queue.Dequeue (out xevent);

				if (!got_xevent) {
#if spew
					Console.WriteLine (">");
					Console.Out.Flush ();
#endif
					break;
				}

#if spew
				Console.Write ("-");
				Console.Out.Flush ();
#endif

				hwnd = (X11Hwnd)Hwnd.GetObjectFromWindow (xevent.AnyEvent.window);

				// Handle messages for windows that are already or are about to be destroyed.

				// we need a special block for this because unless we remove the hwnd from the paint
				// queue it will always stay there (since we don't handle the expose), and we'll
				// effectively loop infinitely trying to repaint a non-existant window.
				if (hwnd != null && hwnd.zombie && xevent.type == XEventName.Expose) {
					hwnd.PendingExpose = hwnd.PendingNCExpose = false;
					goto ProcessNextMessage;
				}

				// We need to make sure we only allow DestroyNotify events through for zombie
				// hwnds, since much of the event handling code makes requests using the hwnd's
				// ClientWindow, and that'll result in BadWindow errors if there's some lag
				// between the XDestroyWindow call and the DestroyNotify event.
				if (hwnd == null || hwnd.zombie) {
#if DriverDebug || DriverDebugDestroy
					Console.WriteLine("GetMessage(): Got message {0} for non-existent or already destroyed window {1:X}",
							  xevent.type, xevent.AnyEvent.window.ToInt32());
#endif
					goto ProcessNextMessage;
				}

				client = hwnd.ClientWindow == xevent.AnyEvent.window;

				msg.hwnd = hwnd.Handle;

				switch (xevent.type) {
				case XEventName.KeyPress:
					Keyboard.KeyEvent (FocusWindow.Handle, xevent, ref msg);
					return true;

				case XEventName.KeyRelease:
					Keyboard.KeyEvent (FocusWindow.Handle, xevent, ref msg);
					return true;

				case XEventName.ButtonPress: {
					switch(xevent.ButtonEvent.button) {
					case 1:
						MouseState |= MouseButtons.Left;
						if (client) {
							msg.message = Msg.WM_LBUTTONDOWN;
						} else {
							msg.message = Msg.WM_NCLBUTTONDOWN;
							hwnd.MenuToScreen (ref xevent.ButtonEvent.x, ref xevent.ButtonEvent.y);
						}
						// TODO: For WM_NCLBUTTONDOWN wParam specifies a hit-test value not the virtual keys down
						msg.wParam=GetMousewParam(0);
						break;

					case 2:
						MouseState |= MouseButtons.Middle;
						if (client) {
							msg.message = Msg.WM_MBUTTONDOWN;
						} else {
							msg.message = Msg.WM_NCMBUTTONDOWN;
							hwnd.MenuToScreen (ref xevent.ButtonEvent.x, ref xevent.ButtonEvent.y);
						}
						msg.wParam=GetMousewParam(0);
						break;

					case 3:
						MouseState |= MouseButtons.Right;
						if (client) {
							msg.message = Msg.WM_RBUTTONDOWN;
						} else {
							msg.message = Msg.WM_NCRBUTTONDOWN;
							hwnd.MenuToScreen (ref xevent.ButtonEvent.x, ref xevent.ButtonEvent.y);
						}
						msg.wParam=GetMousewParam(0);
						break;

					case 4:
						msg.hwnd = FocusWindow.Handle;
						msg.message=Msg.WM_MOUSEWHEEL;
						msg.wParam=GetMousewParam(120);
						break;

					case 5:
						msg.hwnd = FocusWindow.Handle;
						msg.message=Msg.WM_MOUSEWHEEL;
						msg.wParam=GetMousewParam(-120);
						break;
					}

					msg.lParam=(IntPtr) (xevent.ButtonEvent.y << 16 | xevent.ButtonEvent.x);
					MousePosition.X = xevent.ButtonEvent.x;
					MousePosition.Y = xevent.ButtonEvent.y;

					if (!hwnd.Enabled) {
						RedirectMsgToEnabledAncestor (hwnd, msg, xevent.AnyEvent.window,
									      ref xevent.ButtonEvent.x, ref xevent.ButtonEvent.y);
					}

					if (Grab.Hwnd != IntPtr.Zero)
						msg.hwnd = Grab.Hwnd;

					if (ClickPending.Pending &&
					    ((((long)xevent.ButtonEvent.time - ClickPending.Time) < DoubleClickInterval) &&
					     (msg.wParam == ClickPending.wParam) &&
					     (msg.lParam == ClickPending.lParam) &&
					     (msg.message == ClickPending.Message))) {
						// Looks like a genuine double click, clicked twice on the same spot with the same keys
						switch(xevent.ButtonEvent.button) {
						case 1:
							msg.message = client ? Msg.WM_LBUTTONDBLCLK : Msg.WM_NCLBUTTONDBLCLK;
							break;

						case 2:
							msg.message = client ? Msg.WM_MBUTTONDBLCLK : Msg.WM_NCMBUTTONDBLCLK;
							break;

						case 3:
							msg.message = client ? Msg.WM_RBUTTONDBLCLK : Msg.WM_NCRBUTTONDBLCLK;
							break;
						}

						ClickPending.Pending = false;

					}
					else {
						ClickPending.Pending = true;
						ClickPending.Hwnd = msg.hwnd;
						ClickPending.Message = msg.message;
						ClickPending.wParam = msg.wParam;
						ClickPending.lParam = msg.lParam;
						ClickPending.Time = (long)xevent.ButtonEvent.time;
					}

					if (msg.message == Msg.WM_LBUTTONDOWN || msg.message == Msg.WM_MBUTTONDOWN || msg.message == Msg.WM_RBUTTONDOWN) {
						hwnd.SendParentNotify (msg.message, MousePosition.X, MousePosition.Y);

						// Win32 splurts MouseMove events all over the place, regardless of whether the mouse is actually moving or
						// not, especially after mousedown and mouseup. To support apps relying on mousemove events between and after 
						// mouse clicks to repaint or whatever, we generate a mousemove event here. *sigh*
						XEvent motionEvent = new XEvent ();
						motionEvent.type = XEventName.MotionNotify;
						motionEvent.MotionEvent.display = display;
						motionEvent.MotionEvent.window = xevent.ButtonEvent.window;
						motionEvent.MotionEvent.x = xevent.ButtonEvent.x;
						motionEvent.MotionEvent.y = xevent.ButtonEvent.y;
						hwnd.Queue.Enqueue (motionEvent);
					}

					return true;
				}

				case XEventName.ButtonRelease:
					switch(xevent.ButtonEvent.button) {
					case 1:
						if (client) {
							msg.message = Msg.WM_LBUTTONUP;
						} else {
							msg.message = Msg.WM_NCLBUTTONUP;
							hwnd.MenuToScreen (ref xevent.ButtonEvent.x, ref xevent.ButtonEvent.y);
						}
						MouseState &= ~MouseButtons.Left;
						msg.wParam=GetMousewParam(0);
						break;

					case 2:
						if (client) {
							msg.message = Msg.WM_MBUTTONUP;
						} else {
							msg.message = Msg.WM_NCMBUTTONUP;
							hwnd.MenuToScreen (ref xevent.ButtonEvent.x, ref xevent.ButtonEvent.y);
						}
						MouseState &= ~MouseButtons.Middle;
						msg.wParam=GetMousewParam(0);
						break;

					case 3:
						if (client) {
							msg.message = Msg.WM_RBUTTONUP;
						} else {
							msg.message = Msg.WM_NCRBUTTONUP;
							hwnd.MenuToScreen (ref xevent.ButtonEvent.x, ref xevent.ButtonEvent.y);
						}
						MouseState &= ~MouseButtons.Right;
						msg.wParam=GetMousewParam(0);
						break;

					case 4:
						goto ProcessNextMessage;

					case 5:
						goto ProcessNextMessage;
					}

					if (!hwnd.Enabled) {
						RedirectMsgToEnabledAncestor (hwnd, msg, xevent.AnyEvent.window,
									      ref xevent.ButtonEvent.x, ref xevent.ButtonEvent.y);
					}

					if (Grab.Hwnd != IntPtr.Zero)
						msg.hwnd = Grab.Hwnd;

					msg.lParam=(IntPtr) (xevent.ButtonEvent.y << 16 | xevent.ButtonEvent.x);
					MousePosition.X = xevent.ButtonEvent.x;
					MousePosition.Y = xevent.ButtonEvent.y;

						// Win32 splurts MouseMove events all over the place, regardless of whether the mouse is actually moving or
						// not, especially after mousedown and mouseup. To support apps relying on mousemove events between and after 
						// mouse clicks to repaint or whatever, we generate a mousemove event here. *sigh*
					if (msg.message == Msg.WM_LBUTTONUP || msg.message == Msg.WM_MBUTTONUP || msg.message == Msg.WM_RBUTTONUP) {
						XEvent motionEvent = new XEvent ();
						motionEvent.type = XEventName.MotionNotify;
						motionEvent.MotionEvent.display = display;
						motionEvent.MotionEvent.window = xevent.ButtonEvent.window;
						motionEvent.MotionEvent.x = xevent.ButtonEvent.x;
						motionEvent.MotionEvent.y = xevent.ButtonEvent.y;
						hwnd.Queue.Enqueue (motionEvent);
					}
					return true;

				case XEventName.MotionNotify:
					/* XXX move the compression stuff here */

					if (client) {
#if DriverDebugExtra
						Console.WriteLine("GetMessage(): Window {0:X} MotionNotify x={1} y={2}",
								  client ? hwnd.ClientWindow.ToInt32() : hwnd.WholeWindow.ToInt32(),
								  xevent.MotionEvent.x, xevent.MotionEvent.y);
#endif

						if (Grab.Hwnd != IntPtr.Zero)
							msg.hwnd = Grab.Hwnd;
						else
							NativeWindow.WndProc(msg.hwnd, Msg.WM_SETCURSOR, msg.hwnd, (IntPtr)HitTest.HTCLIENT);

						msg.message = Msg.WM_MOUSEMOVE;
						msg.wParam = GetMousewParam(0);
						msg.lParam = (IntPtr) (xevent.MotionEvent.y << 16 | xevent.MotionEvent.x & 0xFFFF);

						if (!hwnd.Enabled) {
							RedirectMsgToEnabledAncestor (hwnd, msg, xevent.AnyEvent.window,
										      ref xevent.MotionEvent.x, ref xevent.MotionEvent.y);
						}

						MousePosition.X = xevent.MotionEvent.x;
						MousePosition.Y = xevent.MotionEvent.y;

						if ((HoverState.Timer.Enabled) &&
						    (((MousePosition.X + HoverState.Size.Width) < HoverState.X) ||
						     ((MousePosition.X - HoverState.Size.Width) > HoverState.X) ||
						     ((MousePosition.Y + HoverState.Size.Height) < HoverState.Y) ||
						     ((MousePosition.Y - HoverState.Size.Height) > HoverState.Y))) {

							HoverState.Timer.Stop();
							HoverState.Timer.Start();
							HoverState.X = MousePosition.X;
							HoverState.Y = MousePosition.Y;
						}
					}
					else {
						HitTest	ht;
						IntPtr dummy;
						int screen_x;
						int screen_y;

						#if DriverDebugExtra
						Console.WriteLine("GetMessage(): non-client area {0:X} MotionNotify x={1} y={2}",
								  client ? hwnd.ClientWindow.ToInt32() : hwnd.WholeWindow.ToInt32(),
								  xevent.MotionEvent.x, xevent.MotionEvent.y);
						#endif
						msg.message = Msg.WM_NCMOUSEMOVE;

						if (!hwnd.Enabled) {
							RedirectMsgToEnabledAncestor (hwnd, msg, xevent.AnyEvent.window,
										      ref xevent.MotionEvent.x, ref xevent.MotionEvent.y);
						}

						// The hit test is sent in screen coordinates
						Xlib.XTranslateCoordinates (display, xevent.AnyEvent.window, RootWindow.Handle,
									    xevent.MotionEvent.x, xevent.MotionEvent.y,
									    out screen_x, out screen_y, out dummy);

						msg.lParam = (IntPtr) (screen_y << 16 | screen_x & 0xFFFF);
						ht = (HitTest)NativeWindow.WndProc (hwnd.ClientWindow, Msg.WM_NCHITTEST,
										    IntPtr.Zero, msg.lParam).ToInt32 ();
						NativeWindow.WndProc(hwnd.ClientWindow, Msg.WM_SETCURSOR, msg.hwnd, (IntPtr)ht);

						MousePosition.X = xevent.MotionEvent.x;
						MousePosition.Y = xevent.MotionEvent.y;
					}

					return true;

				case XEventName.EnterNotify:
					if (!hwnd.Enabled)
						goto ProcessNextMessage;

					if (xevent.CrossingEvent.mode != NotifyMode.NotifyNormal)
						goto ProcessNextMessage;

					msg.message = Msg.WM_MOUSE_ENTER;
					HoverState.X = xevent.CrossingEvent.x;
					HoverState.Y = xevent.CrossingEvent.y;
					HoverState.Timer.Enabled = true;
					HoverState.Window = xevent.CrossingEvent.window;

					return true;

				case XEventName.LeaveNotify:
					if (!hwnd.Enabled)
						goto ProcessNextMessage;

					if ((xevent.CrossingEvent.mode != NotifyMode.NotifyNormal) ||
					    (xevent.CrossingEvent.window != hwnd.ClientWindow))
						goto ProcessNextMessage;

					msg.message=Msg.WM_MOUSELEAVE;
					HoverState.Timer.Enabled = false;
					HoverState.Window = IntPtr.Zero;

					return true;

				case XEventName.ReparentNotify:
					if (hwnd.parent == null) {	// Toplevel
						if ((xevent.ReparentEvent.parent != IntPtr.Zero) && (xevent.ReparentEvent.window == hwnd.WholeWindow)) {
							// We need to adjust x/y
							// This sucks ass, part 2
							// Every WM does the reparenting of toplevel windows different, so there's
							// no standard way of getting our adjustment considering frames/decorations
							// The code below is needed for metacity. KDE doesn't works just fine without this
							int	dummy_int;
							IntPtr	dummy_ptr;
							int	new_x;
							int	new_y;
							int	frame_left;
							int	frame_top;

							hwnd.Reparented = true;

							Xlib.XGetGeometry(display, XGetParent(hwnd.WholeWindow),
									  out dummy_ptr, out new_x, out new_y,
									  out dummy_int, out dummy_int, out dummy_int, out dummy_int);
							hwnd.FrameExtents(out frame_left, out frame_top);
							if ((frame_left != 0) && (frame_top != 0) && (new_x != frame_left) && (new_y != frame_top)) {
								hwnd.x = new_x;
								hwnd.y = new_y;
								hwnd.whacky_wm = true;
							}

							if (hwnd.opacity != 0xffffffff) {
								IntPtr opacity;

								opacity = (IntPtr)(Int32)hwnd.opacity;
								Xlib.XChangeProperty (display, XGetParent(hwnd.WholeWindow),
										      Atoms._NET_WM_WINDOW_OPACITY, Atoms.XA_CARDINAL, 32,
										      PropertyMode.Replace, ref opacity, 1);
							}
							SendMessage(msg.hwnd, Msg.WM_WINDOWPOSCHANGED, msg.wParam, msg.lParam);
							goto ProcessNextMessage;
						} else {
							hwnd.Reparented = false;
							goto ProcessNextMessage;
						}
					}
					goto ProcessNextMessage;

				case XEventName.ConfigureNotify:
					hwnd.HandleConfigureNotify (xevent);
					goto ProcessNextMessage;

				case XEventName.MapNotify: {
					if (client && (xevent.ConfigureEvent.xevent == xevent.ConfigureEvent.window)) { // Ignore events for children (SubstructureNotify) and client areas
						hwnd.Mapped = true;
						msg.message = Msg.WM_SHOWWINDOW;
						msg.wParam = (IntPtr) 1;
						// XXX we're missing the lParam..
						break;
					}
					goto ProcessNextMessage;
				}

				case XEventName.UnmapNotify: {
					if (client && (xevent.ConfigureEvent.xevent == xevent.ConfigureEvent.window)) { // Ignore events for children (SubstructureNotify) and client areas
						hwnd.Mapped = false;
						msg.message = Msg.WM_SHOWWINDOW;
						msg.wParam = (IntPtr) 0;
						// XXX we're missing the lParam..
						break;
					}
					goto ProcessNextMessage;
				}

				case XEventName.FocusIn:
					// We received focus. We use X11 focus only to know if the app window does or does not have focus
					// We do not track the actual focussed window via it. Instead, this is done via FocusWindow internally
					// Receiving focus means we've gotten activated and therefore we need to let the actual FocusWindow know 
					// about it having focus again
					if (xevent.FocusChangeEvent.detail != NotifyDetail.NotifyNonlinear)
						goto ProcessNextMessage;

					if (FocusWindow == null) {
						Control c = Control.FromHandle (hwnd.ClientWindow);
						if (c == null)
							goto ProcessNextMessage;
						Form form = c.FindForm ();
						if (form == null)
							goto ProcessNextMessage;
						X11Hwnd new_active = (X11Hwnd)Hwnd.ObjectFromHandle (form.Handle);
						if (ActiveWindow != new_active) {
							ActiveWindow = new_active;
							SendMessage (ActiveWindow.Handle, Msg.WM_ACTIVATE, (IntPtr) WindowActiveFlags.WA_ACTIVE, IntPtr.Zero);
						}
						goto ProcessNextMessage;
					}
					Keyboard.FocusIn(FocusWindow.Handle);
					SendMessage(FocusWindow.Handle, Msg.WM_SETFOCUS, IntPtr.Zero, IntPtr.Zero);
					goto ProcessNextMessage;

				case XEventName.FocusOut:
					// Se the comment for our FocusIn handler
					if (xevent.FocusChangeEvent.detail != NotifyDetail.NotifyNonlinear)
						goto ProcessNextMessage;

					if (FocusWindow == null)
						goto ProcessNextMessage;

					Keyboard.FocusOut(FocusWindow.Handle);

					while (Keyboard.ResetKeyState(FocusWindow.Handle, ref msg))
						SendMessage(FocusWindow.Handle, msg.message, msg.wParam, msg.lParam);

					SendMessage(FocusWindow.Handle, Msg.WM_KILLFOCUS, IntPtr.Zero, IntPtr.Zero);
					goto ProcessNextMessage;

				case XEventName.Expose:
					if (!hwnd.Mapped) {
						hwnd.PendingExpose = hwnd.PendingNCExpose = false;
						continue;
					}

					msg.hwnd = hwnd.Handle;

					if (client) {
#if DriverDebugExtra
						Console.WriteLine("GetMessage(): Window {0:X} Exposed area {1},{2} {3}x{4}",
								  hwnd.client_window.ToInt32(),
								  xevent.ExposeEvent.x, xevent.ExposeEvent.y,
								  xevent.ExposeEvent.width, xevent.ExposeEvent.height);
#endif
						msg.message = Msg.WM_PAINT;
					}
					else {
						Graphics g;

						switch (hwnd.border_style) {
						case FormBorderStyle.Fixed3D:
							g = Graphics.FromHwnd(hwnd.WholeWindow);
							ControlPaint.DrawBorder3D(g, new Rectangle(0, 0, hwnd.Width, hwnd.Height),
										  Border3DStyle.Sunken);
							g.Dispose();
							break;

						case FormBorderStyle.FixedSingle:
							g = Graphics.FromHwnd(hwnd.WholeWindow);
							ControlPaint.DrawBorder(g, new Rectangle(0, 0, hwnd.Width, hwnd.Height),
										Color.Black, ButtonBorderStyle.Solid);
							g.Dispose();
						break;
						}
#if DriverDebugExtra
						Console.WriteLine("GetMessage(): Window {0:X} Exposed non-client area {1},{2} {3}x{4}",
								  hwnd.ClientWindow.ToInt32(),
								  xevent.ExposeEvent.x, xevent.ExposeEvent.y,
								  xevent.ExposeEvent.width, xevent.ExposeEvent.height);
#endif

						Rectangle rect = new Rectangle (xevent.ExposeEvent.x, xevent.ExposeEvent.y,
										xevent.ExposeEvent.width, xevent.ExposeEvent.height);
						Region region = new Region (rect);
						IntPtr hrgn = region.GetHrgn (null); // Graphics object isn't needed
						msg.message = Msg.WM_NCPAINT;
						msg.wParam = hrgn == IntPtr.Zero ? (IntPtr)1 : hrgn;
						msg.refobject = region;
					}

					return true;
						
				case XEventName.DestroyNotify:

					// This is a bit tricky, we don't receive our own DestroyNotify, we only get those for our children
					hwnd = (X11Hwnd)Hwnd.ObjectFromHandle(xevent.DestroyWindowEvent.window);

					// We may get multiple for the same window, act only one the first (when Hwnd still knows about it)
					if ((hwnd != null) && (hwnd.ClientWindow == xevent.DestroyWindowEvent.window)) {
						CleanupCachedWindows (hwnd);

						#if DriverDebugDestroy
						Console.WriteLine("Received X11 Destroy Notification for {0}", XplatUI.Window(hwnd.ClientWindow));
						#endif

						msg.hwnd = hwnd.ClientWindow;
						msg.message=Msg.WM_DESTROY;
						hwnd.Dispose();
					}
					else
						goto ProcessNextMessage;

					return true;

				case XEventName.ClientMessage:
					if (Dnd.HandleClientMessage (ref xevent))
						goto ProcessNextMessage;

					if (xevent.ClientMessageEvent.message_type == Atoms.AsyncAtom) {
						XplatUIDriverSupport.ExecuteClientMessage((GCHandle)xevent.ClientMessageEvent.ptr1);
						goto ProcessNextMessage;
					}

					if (xevent.ClientMessageEvent.message_type == HoverState.Atom) {
						msg.message = Msg.WM_MOUSEHOVER;
						msg.wParam = GetMousewParam(0);
						msg.lParam = (IntPtr) (xevent.ClientMessageEvent.ptr1);
						return true;
					}

					if (xevent.ClientMessageEvent.message_type == Atoms.PostAtom) {
						msg.hwnd = xevent.ClientMessageEvent.ptr1;
						msg.message = (Msg) xevent.ClientMessageEvent.ptr2.ToInt32 ();
						msg.wParam = xevent.ClientMessageEvent.ptr3;
						msg.lParam = xevent.ClientMessageEvent.ptr4;

						// if we posted a WM_QUIT message, make sure we return
						// false here as well.
						if (msg.message == (Msg)Msg.WM_QUIT)
							return false;
						else
							return true;
					}

					if (xevent.ClientMessageEvent.message_type == Atoms._XEMBED) {
#if DriverDebugXEmbed
						Console.WriteLine("GOT EMBED MESSAGE {0:X}, detail {1:X}",
								  xevent.ClientMessageEvent.ptr2.ToInt32(), xevent.ClientMessageEvent.ptr3.ToInt32());
#endif

						if (xevent.ClientMessageEvent.ptr2.ToInt32() == (int)XEmbedMessage.EmbeddedNotify) {
							XSizeHints hints = new XSizeHints();
							IntPtr dummy;

							Xlib.XGetWMNormalHints (display, hwnd.WholeWindow, ref hints, out dummy);

							hwnd.width = hints.max_width;
							hwnd.height = hints.max_height;
							hwnd.ClientRect = Rectangle.Empty;
							SendMessage(msg.hwnd, Msg.WM_WINDOWPOSCHANGED, IntPtr.Zero, IntPtr.Zero);
						}
					}

					if (xevent.ClientMessageEvent.message_type == Atoms.WM_PROTOCOLS) {
						if (xevent.ClientMessageEvent.ptr1 == Atoms.WM_DELETE_WINDOW) {
							msg.message = Msg.WM_CLOSE;
							return true;
						}

						// We should not get this, but I'll leave the code in case we need it in the future
						if (xevent.ClientMessageEvent.ptr1 == Atoms.WM_TAKE_FOCUS) {
							goto ProcessNextMessage;
						}
					}

					goto ProcessNextMessage;

				case XEventName.PropertyNotify:
					// The Hwnd's themselves handle this
					hwnd.PropertyChanged (xevent);
					goto ProcessNextMessage;
				}
			} while (true);

			msg.hwnd= IntPtr.Zero;
			msg.message = Msg.WM_ENTERIDLE;
			return true;
		}

		[MonoTODO("Implement filtering and PM_NOREMOVE")]
		public bool PeekMessage (object queue_id, ref MSG msg, IntPtr hWnd, int wFilterMin, int wFilterMax, uint flags)
		{
			X11ThreadQueue queue = (X11ThreadQueue) queue_id;
			bool	pending;

			if ((flags & (uint)PeekMessageFlags.PM_REMOVE) == 0) {
				throw new NotImplementedException("PeekMessage PM_NOREMOVE is not implemented yet");	// FIXME - Implement PM_NOREMOVE flag
			}

			try {
				queue.Lock ();
				pending = false;
				if (queue.CountUnlocked > 0)
					pending = true;
			}
			catch {
				return false;
			}
			finally {
				queue.Unlock ();
			}

			queue.CheckTimers ();

			if (!pending)
				return false;

			return GetMessage(queue_id, ref msg, hWnd, wFilterMin, wFilterMax);
		}

		public void DoEvents (X11ThreadQueue queue)
		{
			MSG	msg = new MSG ();

			if (OverrideCursorHandle != IntPtr.Zero)
				OverrideCursorHandle = IntPtr.Zero;

			queue.DispatchIdle = false;

			while (PeekMessage(queue, ref msg, IntPtr.Zero, 0, 0, (uint)PeekMessageFlags.PM_REMOVE)) {
				TranslateMessage (ref msg);
				DispatchMessage (ref msg);
			}

			queue.DispatchIdle = true;
		}

		// double buffering support
		public void CreateOffscreenDrawable (IntPtr handle,
						     int width, int height,
						     out object offscreen_drawable)
		{
			IntPtr root_out;
			int x_out, y_out, width_out, height_out, border_width_out, depth_out;

			Xlib.XGetGeometry (display, handle,
					   out root_out,
					   out x_out, out y_out,
					   out width_out, out height_out,
					   out border_width_out, out depth_out);

			IntPtr pixmap = Xlib.XCreatePixmap (display, handle, width, height, depth_out);

			offscreen_drawable = pixmap;
		}

		public void DestroyOffscreenDrawable (object offscreen_drawable)
		{
			Xlib.XFreePixmap (display, (IntPtr)offscreen_drawable);
		}

		public Graphics GetOffscreenGraphics (object offscreen_drawable)
		{
			return Graphics.FromHwnd ((IntPtr) offscreen_drawable);
		}

		public void BlitFromOffscreen (IntPtr dest_handle,
					       Graphics dest_dc,
					       object offscreen_drawable,
					       Graphics offscreen_dc,
					       Rectangle r)
		{
			XGCValues gc_values;
			IntPtr gc;

			gc_values = new XGCValues();

			gc = Xlib.XCreateGC (display, dest_handle, IntPtr.Zero, ref gc_values);

			Xlib.XCopyArea (display, (IntPtr)offscreen_drawable, dest_handle,
					gc, r.X, r.Y, r.Width, r.Height, r.X, r.Y);

			Xlib.XFreeGC (display, gc);
		}


		// reversible screen-level drawing
		IntPtr GetReversibleScreenGC (Color backColor)
		{
			XGCValues	gc_values;
			IntPtr		gc;
			uint pixel;

			XColor xcolor = new XColor();
			xcolor.red = (ushort)(backColor.R * 257);
			xcolor.green = (ushort)(backColor.G * 257);
			xcolor.blue = (ushort)(backColor.B * 257);
			Xlib.XAllocColor (display, DefaultColormap, ref xcolor);
			pixel = (uint)xcolor.pixel.ToInt32();


			gc_values = new XGCValues();

			gc_values.subwindow_mode = GCSubwindowMode.IncludeInferiors;
			gc_values.foreground = (IntPtr)pixel;

			gc = Xlib.XCreateGC (display, RootWindow.Handle, new IntPtr ((int) (GCFunction.GCSubwindowMode | GCFunction.GCForeground)), ref gc_values);
			Xlib.XSetForeground (display, gc, (UIntPtr)pixel);
			Xlib.XSetFunction (display,   gc, GXFunction.GXxor);

			return gc;
		}

		public void DrawReversibleLine (Point start, Point end, Color backColor)
		{
			if (backColor.GetBrightness() < 0.5)
				backColor = Color.FromArgb(255 - backColor.R, 255 - backColor.G, 255 - backColor.B);

			IntPtr gc = GetReversibleScreenGC (backColor);

			Xlib.XDrawLine (display, RootWindow.Handle, gc, start.X, start.Y, end.X, end.Y);

			Xlib.XFreeGC (display, gc);
		}

		public void FillReversibleRectangle (Rectangle rectangle, Color backColor)
		{
			if (backColor.GetBrightness() < 0.5)
				backColor = Color.FromArgb(255 - backColor.R, 255 - backColor.G, 255 - backColor.B);

			IntPtr gc = GetReversibleScreenGC (backColor);

			if (rectangle.Width < 0) {
				rectangle.X += rectangle.Width;
				rectangle.Width = -rectangle.Width;
			}
			if (rectangle.Height < 0) {
				rectangle.Y += rectangle.Height;
				rectangle.Height = -rectangle.Height;
			}

			Xlib.XFillRectangle (display, RootWindow.Handle, gc, rectangle.Left, rectangle.Top, rectangle.Width, rectangle.Height);

			Xlib.XFreeGC (display, gc);
		}

		public void DrawReversibleFrame (Rectangle rectangle, Color backColor, FrameStyle style)
		{
			if (backColor.GetBrightness() < 0.5)
				backColor = Color.FromArgb(255 - backColor.R, 255 - backColor.G, 255 - backColor.B);

			IntPtr gc = GetReversibleScreenGC (backColor);

			if (rectangle.Width < 0) {
				rectangle.X += rectangle.Width;
				rectangle.Width = -rectangle.Width;
			}
			if (rectangle.Height < 0) {
				rectangle.Y += rectangle.Height;
				rectangle.Height = -rectangle.Height;
			}

			int line_width = 1;
			GCLineStyle line_style = GCLineStyle.LineSolid;
			GCCapStyle cap_style = GCCapStyle.CapButt;
			GCJoinStyle join_style = GCJoinStyle.JoinMiter;

			switch (style) {
			case FrameStyle.Dashed:
				line_style = GCLineStyle.LineOnOffDash;
				break;
			case FrameStyle.Thick:
				line_width = 2;
				break;
			}

			Xlib.XSetLineAttributes (display, gc, line_width, line_style, cap_style, join_style);

			Xlib.XDrawRectangle (display, RootWindow.Handle, gc, rectangle.Left, rectangle.Top, rectangle.Width, rectangle.Height);

			Xlib.XFreeGC (display, gc);
		}
	}
}
