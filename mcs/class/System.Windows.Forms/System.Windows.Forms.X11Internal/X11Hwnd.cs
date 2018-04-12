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
// Copyright (c) 2006 Novell, Inc. (http://www.novell.com)
//
//

using System;
using System.Collections;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace System.Windows.Forms.X11Internal {

	internal class X11Hwnd : Hwnd
	{
		X11Display display;

		bool refetch_window_type = true;
		bool refetch_window_opacity = true;

		IntPtr[] wm_state = new IntPtr[0];
		IntPtr[] window_type = new IntPtr[0];
		double trans = 1.0;

		string text;
		new X11ThreadQueue queue;

		const EventMask SelectInputMask = (EventMask.ButtonPressMask | 
						   EventMask.ButtonReleaseMask | 
						   EventMask.KeyPressMask | 
						   EventMask.KeyReleaseMask | 
						   EventMask.EnterWindowMask | 
						   EventMask.LeaveWindowMask |
						   EventMask.ExposureMask |
						   EventMask.FocusChangeMask |
						   EventMask.PointerMotionMask | 
						   EventMask.SubstructureNotifyMask);

		public X11Hwnd (X11Display display)
		{
			this.display = display;
			Queue = XplatUIX11_new.GetInstance().ThreadQueue(Thread.CurrentThread);
		}

		public X11Hwnd (X11Display display, IntPtr handle) : this (display)
		{
			if (handle == IntPtr.Zero)
				throw new ArgumentNullException ("handle");
			WholeWindow = ClientWindow = handle;
		}

		// XXX this needs to be here so we don't have to
		// change Hwnd.  once we land, remove this and make
		// Hwnd.Queue virtual or abstract
		public new X11ThreadQueue Queue {
			get { return queue; }
			set { queue = value; }
		}

		public virtual void CreateWindow (CreateParams cp)
		{
			if (WholeWindow != IntPtr.Zero || ClientWindow != IntPtr.Zero)
				throw new Exception ("createwindow called a second time on live X11Hwnd");

			XSetWindowAttributes	Attributes;
			int			x;
			int			y;
			int			width;
			int			height;
			IntPtr			ParentHandle;
			SetWindowValuemask	ValueMask;

			Attributes = new XSetWindowAttributes();
			x = cp.X;
			y = cp.Y;
			width = cp.Width;
			height = cp.Height;
			initial_ex_style = (WindowExStyles) cp.ExStyle;

			/* Figure out our parent handle */
			if (cp.Parent != IntPtr.Zero)
				// the parent handle is specified in the CreateParams
				ParentHandle = Hwnd.ObjectFromHandle(cp.Parent).ClientWindow;
			else if (StyleSet (cp.Style, WindowStyles.WS_CHILD))
				// a child control with an unassigned parent gets created under the FosterParent
				ParentHandle = display.FosterParent.Handle;
			else
				// for all other cases, the parent is the root window
				ParentHandle = display.RootWindow.Handle;

			ValueMask = SetWindowValuemask.BitGravity | SetWindowValuemask.WinGravity;

			Attributes.bit_gravity = Gravity.NorthWestGravity;
			Attributes.win_gravity = Gravity.NorthWestGravity;

			// Save what's under the toolwindow
			if (ExStyleSet (cp.ExStyle, WindowExStyles.WS_EX_TOOLWINDOW)) {
				Attributes.save_under = true;
				ValueMask |= SetWindowValuemask.SaveUnder;
			}

			// If we're a popup without caption we override the WM
			if (StyleSet (cp.Style, WindowStyles.WS_POPUP) && !StyleSet (cp.Style, WindowStyles.WS_CAPTION)) {
				Attributes.override_redirect = true;
				ValueMask |= SetWindowValuemask.OverrideRedirect;
			}

			// Default position on screen, if window manager doesn't place us somewhere else
			if (!StyleSet (cp.Style, WindowStyles.WS_CHILD)
			    && !StyleSet (cp.Style, WindowStyles.WS_POPUP)) {
				if (x<0) x = 50;
				if (y<0) y = 50;
			}
			// minimum width/height
			if (width<1) width=1;
			if (height<1) height=1;

			X = x;
			Y = y;
			Width = width;
			Height = height;
			Parent = Hwnd.ObjectFromHandle (cp.Parent);

			Enabled = !StyleSet (cp.Style, WindowStyles.WS_DISABLED);

			ClientWindow = IntPtr.Zero;

			WholeWindow = Xlib.XCreateWindow (display.Handle, ParentHandle,
							  X, Y, Width, Height, 0,
							  (int)CreateWindowArgs.CopyFromParent, (int)CreateWindowArgs.InputOutput,
							  IntPtr.Zero, new UIntPtr ((uint)ValueMask), ref Attributes);
			if (WholeWindow == IntPtr.Zero)
				throw new Exception ("Coult not create X11 nc window");

			ValueMask &= ~(SetWindowValuemask.OverrideRedirect | SetWindowValuemask.SaveUnder);

			if (display.CustomVisual != IntPtr.Zero && display.CustomColormap != IntPtr.Zero) {
				ValueMask |= SetWindowValuemask.ColorMap;
				Attributes.colormap = display.CustomColormap;
			}

			ClientWindow = Xlib.XCreateWindow (display.Handle, WholeWindow,
							   ClientRect.X, ClientRect.Y, ClientRect.Width, ClientRect.Height, 0,
							   (int)CreateWindowArgs.CopyFromParent, (int)CreateWindowArgs.InputOutput,
							   display.CustomVisual, new UIntPtr ((uint)ValueMask), ref Attributes);

			if (ClientWindow == IntPtr.Zero)
				throw new Exception("Could not create X11 client window");

#if DriverDebug || DriverDebugCreate
			Console.WriteLine("Created window {0:X} / {1:X} parent {2:X}, Style {3}, ExStyle {4}", ClientWindow.ToInt32(), WholeWindow.ToInt32(), Parent != null ? Parent.Handle.ToInt32() : 0, (WindowStyles)cp.Style, (WindowExStyles)cp.ExStyle);
#endif

			if (!StyleSet (cp.Style, WindowStyles.WS_CHILD)) {
				if ((X != unchecked((int)0x80000000)) && (Y != unchecked((int)0x80000000))) {
					XSizeHints	hints;

					hints = new XSizeHints();
					hints.x = X;
					hints.y = Y;
					hints.flags = (IntPtr)(XSizeHintsFlags.USPosition | XSizeHintsFlags.PPosition);
					Xlib.XSetWMNormalHints (display.Handle, WholeWindow, ref hints);
				}
			}

			Xlib.XSelectInput (display.Handle, WholeWindow, new IntPtr ((int)(SelectInputMask | EventMask.StructureNotifyMask)));
			if (WholeWindow != ClientWindow)
				Xlib.XSelectInput (display.Handle, ClientWindow, new IntPtr ((int)SelectInputMask));

			if (ExStyleSet (cp.ExStyle, WindowExStyles.WS_EX_TOPMOST)) {
				WINDOW_TYPE = display.Atoms._NET_WM_WINDOW_TYPE_NORMAL;
				Xlib.XSetTransientForHint (display.Handle, WholeWindow, display.RootWindow.Handle);
			}

			SetWMStyles (cp);
			
			// set the group leader
			XWMHints wm_hints = new XWMHints ();
			
			wm_hints.flags = (IntPtr)(XWMHintsFlags.InputHint | XWMHintsFlags.StateHint | XWMHintsFlags.WindowGroupHint);
			wm_hints.input = !StyleSet (cp.Style, WindowStyles.WS_DISABLED);
			wm_hints.initial_state = StyleSet (cp.Style, WindowStyles.WS_MINIMIZE) ? XInitialState.IconicState : XInitialState.NormalState;
			wm_hints.window_group = ParentHandle == display.RootWindow.Handle ? ParentHandle : WholeWindow;
			
			Xlib.XSetWMHints (display.Handle, WholeWindow, ref wm_hints );

			if (StyleSet (cp.Style, WindowStyles.WS_MINIMIZE))
				SetWindowState (FormWindowState.Minimized);
			else if (StyleSet (cp.Style, WindowStyles.WS_MAXIMIZE))
				SetWindowState (FormWindowState.Maximized);

			// for now make all windows dnd enabled
			display.Dnd.SetAllowDrop (this, true);

			// Set caption/window title
			Text = cp.Caption;

			display.SendMessage (Handle, Msg.WM_CREATE, (IntPtr)1, IntPtr.Zero /* XXX unused */);
                        SendParentNotify (Msg.WM_CREATE, int.MaxValue, int.MaxValue);

			if (StyleSet (cp.Style, WindowStyles.WS_VISIBLE)) {
				visible = true;
				Map ();
				if (!(Control.FromHandle(Handle) is Form))
					display.SendMessage (Handle, Msg.WM_SHOWWINDOW, (IntPtr)1, IntPtr.Zero);
			}
		}

		public virtual void DestroyWindow ()
		{
			if (WholeWindow != IntPtr.Zero) {
#if DriverDebug || DriverDebugDestroy
				Console.WriteLine ("XDestroyWindow (whole_window = {0:X})", WholeWindow.ToInt32());
#endif
				Xlib.XDestroyWindow (display.Handle, WholeWindow);
			}
			else if (ClientWindow != IntPtr.Zero) {
#if DriverDebug || DriverDebugDestroy
				Console.WriteLine ("XDestroyWindow (client_window = {0:X})", ClientWindow.ToInt32());
#endif
				Xlib.XDestroyWindow (display.Handle, ClientWindow);
			}
		}

		public void Activate ()
		{
			if (((IList)display.RootWindow._NET_SUPPORTED).Contains (display.Atoms._NET_ACTIVE_WINDOW)) {
				display.SendNetWMMessage (WholeWindow, display.Atoms._NET_ACTIVE_WINDOW, (IntPtr)1, IntPtr.Zero, IntPtr.Zero);
			}
 			else {
				Xlib.XRaiseWindow (display.Handle, WholeWindow);
 			}
		}

		public void Update ()
		{
			try {
				Queue.Lock ();
				if (!Visible || !PendingExpose || !Mapped)
					return;

				// XXX this SendMessage call should probably not be inside the lock
				display.SendMessage (ClientWindow, Msg.WM_PAINT, IntPtr.Zero, IntPtr.Zero);
				
				PendingExpose = false;
			}
			finally {
				Queue.Unlock ();
			}
		}

		public void MenuToScreen (ref int x, ref int y)
		{
			int	dest_x_return;
			int	dest_y_return;
			IntPtr	child;

			Xlib.XTranslateCoordinates (display.Handle,
						    WholeWindow, display.RootWindow.Handle,
						    x, y, out dest_x_return, out dest_y_return, out child);

			x = dest_x_return;
			y = dest_y_return;
		}

		public virtual void PropertyChanged (XEvent xevent)
		{
			if (xevent.PropertyEvent.atom == display.Atoms._NET_WM_WINDOW_TYPE) {
				// we need to recache our WINDOW_TYPE on the next query
				refetch_window_type = true;
				window_type = null;
			}
			else if (xevent.PropertyEvent.atom == display.Atoms._NET_WM_STATE) {
				// we need to recache our WM_STATE on the next query
			}
			else if (xevent.PropertyEvent.atom == display.Atoms._NET_WM_NAME) {
				// update our Text property
			}
			else if (xevent.PropertyEvent.atom == display.Atoms._NET_WM_WINDOW_OPACITY) {
				// we need to recache our _NET_WM_WINDOW_OPACITY on the next query.
				refetch_window_opacity = true;
			}
			// else we don't care about it

		}

		public void SetIcon (Icon icon)
		{
			if (icon == null) {
				Xlib.XDeleteProperty (display.Handle, WholeWindow, display.Atoms._NET_WM_ICON);
			}
			else {
				Bitmap		bitmap;
				int		size;
				IntPtr[]	data;
				int		index;

				bitmap = icon.ToBitmap();
				index = 0;
				size = bitmap.Width * bitmap.Height + 2;
				data = new IntPtr[size];

				data[index++] = (IntPtr)bitmap.Width;
				data[index++] = (IntPtr)bitmap.Height;

				for (int y = 0; y < bitmap.Height; y++) {
					for (int x = 0; x < bitmap.Width; x++) {
						data[index++] = (IntPtr)bitmap.GetPixel(x, y).ToArgb();
					}
				}

				Xlib.XChangeProperty (display.Handle, WholeWindow,
						      display.Atoms._NET_WM_ICON, display.Atoms.XA_CARDINAL, 32,
						      PropertyMode.Replace, data, size);
			}
		}

		public double GetWindowTransparency ()
		{
			if (refetch_window_opacity) {
				trans = 1.0;

				IntPtr actual_atom;
				int actual_format;
				IntPtr nitems;
				IntPtr bytes_after;
				IntPtr prop = IntPtr.Zero;

				IntPtr w = WholeWindow;
				if (reparented)
					w = display.XGetParent (WholeWindow);

				Xlib.XGetWindowProperty (display.Handle, w,
							 display.Atoms._NET_WM_WINDOW_OPACITY, IntPtr.Zero, new IntPtr (16), false,
							 display.Atoms.XA_CARDINAL,
							 out actual_atom, out actual_format, out nitems, out bytes_after, ref prop);
				
				if (((long)nitems == 1) && (prop != IntPtr.Zero)) {
					uint x11_opacity = (uint)Marshal.ReadInt32(prop, 0);
					trans = ((double)x11_opacity) / (uint)0xffffffff;
				}

				if (prop != IntPtr.Zero) {
					Xlib.XFree(prop);
				}
			}

			return trans;
		}

		public void SetWindowTransparency (double transparency, Color key)
		{
			IntPtr	x11_opacity;

			opacity = (uint)(0xffffffff * transparency);
			x11_opacity = (IntPtr)((int)opacity);

			IntPtr w = WholeWindow;
			if (reparented)
				w = display.XGetParent (WholeWindow);
			Xlib.XChangeProperty (display.Handle, w,
					      display.Atoms._NET_WM_WINDOW_OPACITY, display.Atoms.XA_CARDINAL, 32,
					      PropertyMode.Replace, ref x11_opacity, 1);
		}

		public IntPtr DefWndProc (ref Message msg)
		{
			switch ((Msg)msg.Msg) {
			case Msg.WM_PAINT:
				Queue.Lock ();
				PendingExpose = false;
				Queue.Unlock ();
				return IntPtr.Zero;

			case Msg.WM_NCPAINT:
				Queue.Lock ();
				PendingNCExpose = false;
				Queue.Unlock ();
				return IntPtr.Zero;

			case Msg.WM_CONTEXTMENU:
				if (Parent != null)
					display.SendMessage (Parent.ClientWindow, Msg.WM_CONTEXTMENU, msg.WParam, msg.LParam);
				return IntPtr.Zero;

			case Msg.WM_MOUSEWHEEL:
				if (Parent != null) {
					display.SendMessage (Parent.ClientWindow, Msg.WM_MOUSEWHEEL, msg.WParam, msg.LParam);
					if (msg.Result == IntPtr.Zero)
						return IntPtr.Zero;
				}
				return IntPtr.Zero;

			case Msg.WM_SETCURSOR:
				X11Hwnd parent = (X11Hwnd)Parent;
				// Pass to parent window first
				while ((parent != null) && (msg.Result == IntPtr.Zero)) {
					msg.Result = NativeWindow.WndProc (parent.Handle, Msg.WM_SETCURSOR, msg.HWnd, msg.LParam);
					parent = (X11Hwnd)Parent;
				}

				if (msg.Result == IntPtr.Zero) {
					IntPtr handle;

					switch((HitTest)(msg.LParam.ToInt32() & 0xffff)) {
					case HitTest.HTBOTTOM:		handle = Cursors.SizeNS.handle; break;
					case HitTest.HTBORDER:		handle = Cursors.SizeNS.handle; break;
					case HitTest.HTBOTTOMLEFT:	handle = Cursors.SizeNESW.handle; break;
					case HitTest.HTBOTTOMRIGHT:	handle = Cursors.SizeNWSE.handle; break;
					case HitTest.HTERROR:
						if ((msg.LParam.ToInt32() >> 16) == (int)Msg.WM_LBUTTONDOWN)
							display.AudibleAlert();
						handle = Cursors.Default.handle;
						break;

					case HitTest.HTHELP:		handle = Cursors.Help.handle; break;
					case HitTest.HTLEFT:		handle = Cursors.SizeWE.handle; break;
					case HitTest.HTRIGHT:		handle = Cursors.SizeWE.handle; break;
					case HitTest.HTTOP:		handle = Cursors.SizeNS.handle; break;
					case HitTest.HTTOPLEFT:		handle = Cursors.SizeNWSE.handle; break;
					case HitTest.HTTOPRIGHT:	handle = Cursors.SizeNESW.handle; break;

#if SameAsDefault
					case HitTest.HTGROWBOX:
					case HitTest.HTSIZE:
					case HitTest.HTZOOM:
					case HitTest.HTVSCROLL:
					case HitTest.HTSYSMENU:
					case HitTest.HTREDUCE:
					case HitTest.HTNOWHERE:
					case HitTest.HTMAXBUTTON:
					case HitTest.HTMINBUTTON:
					case HitTest.HTMENU:
					case HitTest.HSCROLL:
					case HitTest.HTBOTTOM:
					case HitTest.HTCAPTION:
					case HitTest.HTCLIENT:
					case HitTest.HTCLOSE:
#endif
					default: handle = Cursors.Default.handle; break;
					}

					display.SetCursor (msg.HWnd, handle);
				}
				return (IntPtr)1;

			default:
				return IntPtr.Zero;
			}
		}


		public void AddExpose (bool client, int x, int y, int width, int height)
		{
			// Don't waste time
			if ((x > Width) || (y > Height) || ((x + width) <= 0) || ((y + height) <= 0))
				return;

			// Keep the invalid area as small as needed
			if ((x + width) > Width)
				width = Width - x;

			if ((y + height) > Height)
				height = Height - y;

			if (client) {
				AddInvalidArea(x, y, width, height);
				PendingExpose = true;
			}
			else {
				AddNcInvalidArea (x, y, width, height);
				PendingNCExpose = true;
			}
		}

		public void AddConfigureNotify (XEvent xevent)
		{
			// We drop configure events for Client windows
			if ((xevent.ConfigureEvent.window != WholeWindow) || (xevent.ConfigureEvent.window != xevent.ConfigureEvent.xevent))
				return;

			if (!reparented) {
				X = xevent.ConfigureEvent.x;
				Y = xevent.ConfigureEvent.y;
			} else {
				// This sucks ass, part 1
				// Every WM does the ConfigureEvents of toplevel windows different, so there's
				// no standard way of getting our adjustment. 
				// The code below is needed for KDE and FVWM, the 'whacky_wm' part is for metacity
				// Several other WMs do their decorations different yet again and we fail to deal 
				// with that, since I couldn't find any frigging commonality between them.
				// The only sane WM seems to be KDE

				if (!xevent.ConfigureEvent.send_event) {
					IntPtr	dummy_ptr;

					int trans_x;
					int trans_y;

					Xlib.XTranslateCoordinates (display.Handle, WholeWindow, display.RootWindow.Handle,
								    -xevent.ConfigureEvent.x, -xevent.ConfigureEvent.y,
								    out trans_x, out trans_y, out dummy_ptr);

					X = trans_x;
					Y = trans_y;
				} else {
					// This is a synthetic event, coordinates are in root space
					X = xevent.ConfigureEvent.x;
					Y = xevent.ConfigureEvent.y;
					if (whacky_wm) {
						int frame_left;
						int frame_top;

						FrameExtents (out frame_left, out frame_top);
						X -= frame_left;
						Y -= frame_top;
					}
				}
			}

			Width = xevent.ConfigureEvent.width;
			Height = xevent.ConfigureEvent.height;
			ClientRect = Rectangle.Empty;

			if (!configure_pending) {
				Queue.AddConfigure (this);
				configure_pending = true;
			}
		}

		public void HandleMapEvent (XEvent xevent)
		{
			if (xevent.type == XEventName.MapNotify) {
			}
			else {
			}
		}

		public void HandleConfigureNotify (XEvent xevent)
		{
			configure_pending = false;

			display.SendMessage (Handle, Msg.WM_WINDOWPOSCHANGED, IntPtr.Zero, IntPtr.Zero);

			// We need to adjust our client window to track the resize of whole_window
			if (WholeWindow != ClientWindow)
				PerformNCCalc ();
		}

		public void Invalidate (Rectangle rc, bool clear)
		{
			if (clear) {
				AddExpose (true, X, Y, Width, Height);
			} else {
				AddExpose (true, rc.X, rc.Y, rc.Width, rc.Height);
			}
		}

		public void InvalidateNC ()
		{
			AddExpose (false, 0, 0, Width, Height);
		}

		// XXX this assumes the queue lock is held
		public bool PendingNCExpose {
			get { return nc_expose_pending; }
			set {
				if (nc_expose_pending == value)
					return;
				nc_expose_pending = value;

				if (nc_expose_pending && !expose_pending)
					Queue.AddPaint (this);
				else if (!nc_expose_pending && !expose_pending)
					Queue.RemovePaint (this);
			}
		}

		// XXX this assumes the queue lock is held
		public bool PendingExpose {
			get { return expose_pending; }
			set {
				if (expose_pending == value)
					return;
				expose_pending = value;

				if (expose_pending && !nc_expose_pending)
					Queue.AddPaint (this);
				else if (!expose_pending && !nc_expose_pending)
					Queue.RemovePaint (this);
			}
		}

		public PaintEventArgs PaintEventStart (ref Message m, bool client)
		{
			PaintEventArgs paint_event;
			Graphics dc;

			if (client) {
				dc = Graphics.FromHwnd (ClientWindow);

				Region clip_region = new Region ();
				clip_region.MakeEmpty();

				foreach (Rectangle r in ClipRectangles)
					clip_region.Union (r);

				if (UserClip != null)
					clip_region.Intersect(UserClip);

				dc.Clip = clip_region;
				paint_event = new PaintEventArgs(dc, Invalid);
				PendingExpose = false;

				ClearInvalidArea();

				return paint_event;
			}
			else {
				dc = Graphics.FromHwnd (WholeWindow);

				if (!nc_invalid.IsEmpty) {
					dc.SetClip (nc_invalid);
					paint_event = new PaintEventArgs(dc, nc_invalid);
				}
				else {
					paint_event = new PaintEventArgs(dc, new Rectangle(0, 0, width, height));
				}
				PendingNCExpose = false;

				ClearNcInvalidArea ();

				return paint_event;
			}
		}

		public void PaintEventEnd (ref Message m, bool client, PaintEventEnd pevent)
		{
			if (pevent.Graphics != null)
				pevent.Graphics.Dispose();
			pevent.SetGraphics(null);
			pevent.Dispose();
		}

		public void DrawReversibleRectangle (Rectangle rect, int line_width)
		{
			XGCValues	gc_values;
			IntPtr		gc;

			gc_values = new XGCValues ();

			gc_values.subwindow_mode = GCSubwindowMode.IncludeInferiors;
			gc_values.line_width = line_width;

			// XXX multiscreen support
			gc_values.foreground = Xlib.XBlackPixel (display.Handle, display.DefaultScreen);

			// This logic will give us true rubber bands: (libsx, SANE_XOR)
			//mask = foreground ^ background; 
			//XSetForeground(DisplayHandle, gc, 0xffffffff);
			//XSetBackground(DisplayHandle, gc, background);
			//XSetFunction(DisplayHandle,   gc, GXxor);
			//XSetPlaneMask(DisplayHandle,  gc, mask);


			gc = Xlib.XCreateGC (display.Handle, ClientWindow,
					     new IntPtr ((int) (GCFunction.GCSubwindowMode | GCFunction.GCLineWidth | GCFunction.GCForeground)), ref gc_values);
			uint foreground;
			uint background;

			Control control;
			control = Control.FromHandle(Handle);

			XColor xcolor = new XColor();

			xcolor.red = (ushort)(control.ForeColor.R * 257);
			xcolor.green = (ushort)(control.ForeColor.G * 257);
			xcolor.blue = (ushort)(control.ForeColor.B * 257);
			Xlib.XAllocColor (display.Handle, display.DefaultColormap, ref xcolor);
			foreground = (uint)xcolor.pixel.ToInt32();

			xcolor.red = (ushort)(control.BackColor.R * 257);
			xcolor.green = (ushort)(control.BackColor.G * 257);
			xcolor.blue = (ushort)(control.BackColor.B * 257);
			Xlib.XAllocColor (display.Handle, display.DefaultColormap, ref xcolor);
			background = (uint)xcolor.pixel.ToInt32();

			uint mask = foreground ^ background; 

			Xlib.XSetForeground (display.Handle, gc, (UIntPtr)0xffffffff);
			Xlib.XSetBackground (display.Handle, gc, (UIntPtr)background);
			Xlib.XSetFunction (display.Handle,   gc, GXFunction.GXxor);
			Xlib.XSetPlaneMask (display.Handle,  gc, (IntPtr)mask);

			if ((rect.Width > 0) && (rect.Height > 0))
				Xlib.XDrawRectangle (display.Handle, ClientWindow, gc, rect.Left, rect.Top, rect.Width, rect.Height);
			else if (rect.Width > 0)
				Xlib.XDrawLine (display.Handle, ClientWindow, gc, rect.X, rect.Y, rect.Right, rect.Y);
			else
				Xlib.XDrawLine (display.Handle, ClientWindow, gc, rect.X, rect.Y, rect.X, rect.Bottom);

			Xlib.XFreeGC (display.Handle, gc);
		}

		private void WaitForMessage (Msg message)
		{
			MSG msg = new MSG ();

			queue.DispatchIdle = false;

			bool done = false;
			do {
				if (display.PeekMessage(queue, ref msg, IntPtr.Zero, 0, 0, (uint)PeekMessageFlags.PM_REMOVE)) {
					if ((Msg)msg.message == Msg.WM_QUIT) {
						// XXX this should live someplace else
						XplatUI.PostQuitMessage (0);
						done = true;
					}
					else {
						if ((msg.hwnd == Handle) &&
						    ((Msg)msg.message == message || (Msg)msg.message == Msg.WM_DESTROY))
							done = true;
						display.TranslateMessage (ref msg);
						display.DispatchMessage (ref msg);
					}
				}
			} while (!done);

			queue.DispatchIdle = true;
		}

		public void Map ()
		{
			// FIXME why do we set this here and also in the MapNotify event handling?
			if (!mapped) {

				Xlib.XMapWindow (display.Handle, WholeWindow);
				Xlib.XMapWindow (display.Handle, ClientWindow);

				mapped = true;

				if (Control.FromHandle(Handle) is Form)
					WaitForMessage (Msg.WM_SHOWWINDOW);
			}
		}

		public void Unmap ()
		{
			// FIXME why do we set this here and also in the UnmapNotify event handling?
			if (mapped) {
				Xlib.XUnmapWindow (display.Handle, ClientWindow);
				Xlib.XUnmapWindow (display.Handle, WholeWindow);

				mapped = false;

				if (Control.FromHandle(Handle) is Form)
					WaitForMessage (Msg.WM_SHOWWINDOW);
			}
		}

		public void PerformNCCalc ()
		{
			XplatUIWin32.NCCALCSIZE_PARAMS	ncp;
			IntPtr				ptr;
			Rectangle			rect;

			rect = DefaultClientRect;

			ncp = new XplatUIWin32.NCCALCSIZE_PARAMS ();
			ptr = Marshal.AllocHGlobal (Marshal.SizeOf(ncp));

			ncp.rgrc1.left = rect.Left;
			ncp.rgrc1.top = rect.Top;
			ncp.rgrc1.right = rect.Right;
			ncp.rgrc1.bottom = rect.Bottom;

			Marshal.StructureToPtr (ncp, ptr, true);
			NativeWindow.WndProc (ClientWindow, Msg.WM_NCCALCSIZE, (IntPtr)1, ptr);
			ncp = (XplatUIWin32.NCCALCSIZE_PARAMS)Marshal.PtrToStructure (ptr, typeof(XplatUIWin32.NCCALCSIZE_PARAMS));
			Marshal.FreeHGlobal(ptr);

			// FIXME - debug this with Menus

			rect = new Rectangle(ncp.rgrc1.left, ncp.rgrc1.top, ncp.rgrc1.right - ncp.rgrc1.left, ncp.rgrc1.bottom - ncp.rgrc1.top);
			ClientRect = rect;

			if (Visible) {
				if ((rect.Width < 1) || (rect.Height < 1))
					Xlib.XMoveResizeWindow (display.Handle, ClientWindow, -5, -5, 1, 1);
				else
					Xlib.XMoveResizeWindow (display.Handle, ClientWindow, rect.X, rect.Y, rect.Width, rect.Height);
			}

			InvalidateNC ();
		}

		public void RequestNCRecalc ()
		{
			PerformNCCalc ();
			display.SendMessage (Handle, Msg.WM_WINDOWPOSCHANGED, IntPtr.Zero, IntPtr.Zero);
			InvalidateNC ();
		}

		[MonoTODO]
		public void RequestAdditionalWM_NCMessages (bool hover, bool leave)
		{
			// Missing messages won't crash anything so just don't generate them for the moment.
			// throw new NotImplementedException( );
		}
		
		public void FrameExtents (out int left, out int top)
		{
			IntPtr actual_atom;
			int actual_format;
			IntPtr nitems;
			IntPtr bytes_after;
			IntPtr prop = IntPtr.Zero;

			Xlib.XGetWindowProperty (display.Handle, WholeWindow,
						 display.Atoms._NET_FRAME_EXTENTS, IntPtr.Zero, new IntPtr (16), false,
						 display.Atoms.XA_CARDINAL, out actual_atom, out actual_format, out nitems, out bytes_after, ref prop);
			if (((long)nitems == 4) && (prop != IntPtr.Zero)) {
				left = Marshal.ReadIntPtr(prop, 0).ToInt32();
				//right = Marshal.ReadIntPtr(prop, IntPtr.Size).ToInt32();
				top = Marshal.ReadIntPtr(prop, IntPtr.Size * 2).ToInt32();
				//bottom = Marshal.ReadIntPtr(prop, IntPtr.Size * 3).ToInt32();
			} else {
				left = 0;
				top = 0;
			}

			if (prop != IntPtr.Zero) {
				Xlib.XFree(prop);
			}
		}

		static bool StyleSet (int s, WindowStyles ws)
		{
			return (s & (int)ws) == (int)ws;
		}

		static bool ExStyleSet (int ex, WindowExStyles exws)
		{
			return (ex & (int)exws) == (int)exws;
		}

		// XXX this should be a static method on Hwnd so other backends can use it
		public static void DeriveStyles(int Style, int ExStyle,	out FormBorderStyle border_style, out bool border_static, 
				out TitleStyle title_style, out int caption_height, out int tool_caption_height)
		{

			// Only MDI windows get caption_heights
			caption_height = 0;
			tool_caption_height = 19;
			border_static = false;

			if (StyleSet (Style, WindowStyles.WS_CHILD)) {
				if (ExStyleSet (ExStyle, WindowExStyles.WS_EX_CLIENTEDGE)) {
					border_style = FormBorderStyle.Fixed3D;
				} else if (ExStyleSet (ExStyle, WindowExStyles.WS_EX_STATICEDGE)) {
					border_style = FormBorderStyle.Fixed3D;
					border_static = true;
				} else if (!StyleSet (Style, WindowStyles.WS_BORDER)) {
					border_style = FormBorderStyle.None;
				} else {
					border_style = FormBorderStyle.FixedSingle;
				}
				title_style = TitleStyle.None;

				if (ExStyleSet (ExStyle, WindowExStyles.WS_EX_MDICHILD)) {
					caption_height = 26;

					if (StyleSet (Style, WindowStyles.WS_CAPTION)) {
						if (ExStyleSet (ExStyle, WindowExStyles.WS_EX_TOOLWINDOW))
							title_style = TitleStyle.Tool;
						else
							title_style = TitleStyle.Normal;
					}

					if (StyleSet (Style, WindowStyles.WS_OVERLAPPEDWINDOW) ||
					    ExStyleSet (ExStyle, WindowExStyles.WS_EX_TOOLWINDOW))
						border_style = (FormBorderStyle) 0xFFFF;
					else
						border_style = FormBorderStyle.None;
				}
			}
			else {
				title_style = TitleStyle.None;
				if (StyleSet (Style, WindowStyles.WS_CAPTION)) {
					if (ExStyleSet (ExStyle, WindowExStyles.WS_EX_TOOLWINDOW))
						title_style = TitleStyle.Tool;
					else
						title_style = TitleStyle.Normal;
				}

				border_style = FormBorderStyle.None;

				if (StyleSet (Style, WindowStyles.WS_THICKFRAME)) {
					if (ExStyleSet (ExStyle, WindowExStyles.WS_EX_TOOLWINDOW))
						border_style = FormBorderStyle.SizableToolWindow;
					else
						border_style = FormBorderStyle.Sizable;
				} else {
					if (StyleSet (Style, WindowStyles.WS_CAPTION)) {
						if (ExStyleSet (ExStyle, WindowExStyles.WS_EX_CLIENTEDGE))
							border_style = FormBorderStyle.Fixed3D;
						else if (ExStyleSet (ExStyle, WindowExStyles.WS_EX_STATICEDGE)) {
							border_style = FormBorderStyle.Fixed3D;
							border_static = true;
						} else if (ExStyleSet (ExStyle, WindowExStyles.WS_EX_DLGMODALFRAME))
							border_style = FormBorderStyle.FixedDialog;
						else if (ExStyleSet (ExStyle, WindowExStyles.WS_EX_TOOLWINDOW))
							border_style = FormBorderStyle.FixedToolWindow;
						else if (StyleSet (Style, WindowStyles.WS_BORDER))
							border_style = FormBorderStyle.FixedSingle;
					} else if (StyleSet (Style, WindowStyles.WS_BORDER))
						border_style = FormBorderStyle.FixedSingle;
				}
			}
		}

		public void SetHwndStyles (CreateParams cp)
		{
			DeriveStyles(cp.Style, cp.ExStyle, out this.border_style, out this.border_static, out this.title_style, out this.caption_height, out this.tool_caption_height);
		}

		public void SetWMStyles (CreateParams cp)
		{
			MotifWmHints mwmHints;
			MotifFunctions functions;
			MotifDecorations decorations;
			IntPtr[] atoms;
			int atom_count;
			Rectangle client_rect;

			// Child windows don't need WM window styles
			if (StyleSet (cp.Style, WindowStyles.WS_CHILDWINDOW))
				return;

			atoms = new IntPtr[8];
			mwmHints = new MotifWmHints();
			functions = 0;
			decorations = 0;

			mwmHints.flags = (IntPtr)(MotifFlags.Functions | MotifFlags.Decorations);
			mwmHints.functions = (IntPtr)0;
			mwmHints.decorations = (IntPtr)0;

			if (ExStyleSet (cp.ExStyle, WindowExStyles.WS_EX_TOOLWINDOW)
			    || !StyleSet (cp.Style, WindowStyles.WS_CAPTION | WindowStyles.WS_BORDER | WindowStyles.WS_DLGFRAME)) {
				/* tool windows get no window manager
				   decorations, and neither do windows
				   which lack CAPTION/BORDER/DLGFRAME
				   styles.
				*/

				/* just because the window doesn't get any decorations doesn't
				   mean we should disable the functions.  for instance, without
				   MotifFunctions.Maximize, changing the windowstate to Maximized
				   is ignored by metacity. */
				functions |= MotifFunctions.Move | MotifFunctions.Resize | MotifFunctions.Minimize | MotifFunctions.Maximize;
			}
			else {
				if (StyleSet (cp.Style, WindowStyles.WS_CAPTION)) {
					functions |= MotifFunctions.Move;
					decorations |= MotifDecorations.Title | MotifDecorations.Menu;
				}

				if (StyleSet (cp.Style, WindowStyles.WS_THICKFRAME)) {
					functions |= MotifFunctions.Move | MotifFunctions.Resize;
					decorations |= MotifDecorations.Border | MotifDecorations.ResizeH;
				}

				if (StyleSet (cp.Style, WindowStyles.WS_MINIMIZEBOX)) {
					functions |= MotifFunctions.Minimize;
					decorations |= MotifDecorations.Minimize;
				}

				if (StyleSet (cp.Style, WindowStyles.WS_MAXIMIZEBOX)) {
					functions |= MotifFunctions.Maximize;
					decorations |= MotifDecorations.Maximize;
				}

				if (StyleSet (cp.Style, WindowStyles.WS_SIZEBOX)) {
					functions |= MotifFunctions.Resize;
					decorations |= MotifDecorations.ResizeH;
				}

				if (ExStyleSet (cp.ExStyle, WindowExStyles.WS_EX_DLGMODALFRAME)) {
					decorations |= MotifDecorations.Border;
				}

				if (StyleSet (cp.Style, WindowStyles.WS_BORDER)) {
					decorations |= MotifDecorations.Border;
				}
			
				if (StyleSet (cp.Style, WindowStyles.WS_DLGFRAME)) {
					decorations |= MotifDecorations.Border;
				}

				if (StyleSet (cp.Style, WindowStyles.WS_SYSMENU)) {
					functions |= MotifFunctions.Close;
				}
				else {
					functions &= ~(MotifFunctions.Maximize | MotifFunctions.Minimize | MotifFunctions.Close);
					decorations &= ~(MotifDecorations.Menu | MotifDecorations.Maximize | MotifDecorations.Minimize);
					if (cp.Caption == "") {
						functions &= ~MotifFunctions.Move;
						decorations &= ~(MotifDecorations.Title | MotifDecorations.ResizeH);
					}
				}
			}

			if ((functions & MotifFunctions.Resize) == 0) {
				fixed_size = true;
				SetMinMax (new Rectangle(cp.X, cp.Y, cp.Width, cp.Height), new Size(cp.Width, cp.Height), new Size(cp.Width, cp.Height));
			} else {
				fixed_size = false;
			}

			mwmHints.functions = (IntPtr)functions;
			mwmHints.decorations = (IntPtr)decorations;

			FormWindowState current_state = GetWindowState ();
			if (current_state == (FormWindowState)(-1))
				current_state = FormWindowState.Normal;

			client_rect = ClientRect;

			atom_count = 0;

			// needed! map toolwindows to _NET_WM_WINDOW_TYPE_UTILITY to make newer metacity versions happy
			// and get those windows in front of their parents
			if (ExStyleSet (cp.ExStyle, WindowExStyles.WS_EX_TOOLWINDOW)) {
				WINDOW_TYPE = display.Atoms._NET_WM_WINDOW_TYPE_UTILITY;

				Form f = Control.FromHandle(Handle) as Form;
				if (f != null && !reparented) {
					if (f.Owner != null && f.Owner.Handle != IntPtr.Zero) {
						Hwnd owner_hwnd = Hwnd.ObjectFromHandle(f.Owner.Handle);
						if (owner_hwnd != null)
							Xlib.XSetTransientForHint (display.Handle, WholeWindow,
										   owner_hwnd.WholeWindow);
					}
				}
			}
				
			Xlib.XChangeProperty (display.Handle, WholeWindow,
					      display.Atoms._MOTIF_WM_HINTS, display.Atoms._MOTIF_WM_HINTS, 32,
					      PropertyMode.Replace, ref mwmHints, 5);

			if (StyleSet (cp.Style, WindowStyles.WS_POPUP) && (parent != null) && (parent.WholeWindow != IntPtr.Zero)) {
				WINDOW_TYPE = display.Atoms._NET_WM_WINDOW_TYPE_NORMAL;
				Xlib.XSetTransientForHint(display.Handle, WholeWindow, parent.WholeWindow);
			} else if (!ExStyleSet (cp.ExStyle, WindowExStyles.WS_EX_APPWINDOW)) {
				/* this line keeps the window from showing up in gnome's taskbar */
				atoms[atom_count++] = display.Atoms._NET_WM_STATE_SKIP_TASKBAR;
			}
			if ((client_rect.Width < 1) || (client_rect.Height < 1)) {
				Xlib.XMoveResizeWindow (display.Handle, ClientWindow, -5, -5, 1, 1);
			} else {
				Xlib.XMoveResizeWindow (display.Handle, ClientWindow, client_rect.X, client_rect.Y, client_rect.Width, client_rect.Height);
			}

			if (ExStyleSet (cp.ExStyle, WindowExStyles.WS_EX_TOOLWINDOW))
				atoms[atom_count++] = display.Atoms._NET_WM_STATE_SKIP_TASKBAR;

			/* we need to add these atoms in the
			 * event we're maximized, since we're
			 * replacing the existing
			 * _NET_WM_STATE here.  If we don't
			 * add them, future calls to
			 * GetWindowState will return Normal
			 * for a window which is maximized. */
			if (current_state == FormWindowState.Maximized) {
				atoms[atom_count++] = display.Atoms._NET_WM_STATE_MAXIMIZED_HORZ;
				atoms[atom_count++] = display.Atoms._NET_WM_STATE_MAXIMIZED_VERT;
			}

			Set_WM_STATE (atoms, atom_count);

			atom_count = 0;
			atoms[atom_count++] = display.Atoms.WM_DELETE_WINDOW;
			if (ExStyleSet (cp.ExStyle, WindowExStyles.WS_EX_CONTEXTHELP))
				atoms[atom_count++] = display.Atoms._NET_WM_CONTEXT_HELP;

			Xlib.XSetWMProtocols (display.Handle, WholeWindow, atoms, atom_count);
		}

		public void ClientToScreen (ref int x, ref int y)
		{
			int	dest_x_return;
			int	dest_y_return;
			IntPtr	child;

			Xlib.XTranslateCoordinates (display.Handle,
						    ClientWindow, display.RootWindow.Handle,
						    x, y, out dest_x_return, out dest_y_return, out child);

			x = dest_x_return;
			y = dest_y_return;
		}

		public void ScreenToClient (ref int x, ref int y)
		{
			int	dest_x_return;
			int	dest_y_return;
			IntPtr	child;

			Xlib.XTranslateCoordinates (display.Handle,
						    display.RootWindow.Handle, ClientWindow,
						    x, y, out dest_x_return, out dest_y_return, out child);

			x = dest_x_return;
			y = dest_y_return;
		}


		public void ScreenToMenu (ref int x, ref int y)
		{
			int	dest_x_return;
			int	dest_y_return;
			IntPtr	child;

			Xlib.XTranslateCoordinates (display.Handle,
						    display.RootWindow.Handle, WholeWindow,
						    x, y, out dest_x_return, out dest_y_return, out child);

			x = dest_x_return;
			y = dest_y_return;
		}

		public void ScrollWindow (Rectangle area, int XAmount, int YAmount, bool with_children)
		{
			IntPtr		gc;
			XGCValues	gc_values;

			Rectangle r = Rectangle.Intersect (Invalid, area);
			if (!r.IsEmpty) {
				/* We have an invalid area in the window we're scrolling. 
				   Adjust our stored invalid rectangle to to match the scrolled amount */

				r.X += XAmount;
				r.Y += YAmount;

				if (r.X < 0) {
					r.Width += r.X;
					r.X =0;
				}

				if (r.Y < 0) {
					r.Height += r.Y;
					r.Y =0;
				}

				if (area.Contains (Invalid))
					ClearInvalidArea();
				AddInvalidArea(r);
			}

			gc_values = new XGCValues();

			gc_values.graphics_exposures = false;
			if (with_children)
				gc_values.subwindow_mode = GCSubwindowMode.IncludeInferiors;

			gc = Xlib.XCreateGC (display.Handle, ClientWindow, IntPtr.Zero, ref gc_values);

			int src_x, src_y;
			int dest_x, dest_y;
			int width, height;

			if (YAmount > 0) {
				src_y = area.Y;
				height = area.Height - YAmount;
				dest_y = area.Y + YAmount;
			}
			else {
				src_y = area.Y - YAmount;
				height = area.Height + YAmount;
				dest_y = area.Y;
			}

			if (XAmount > 0) {
				src_x = area.X;
				width = area.Width - XAmount;
				dest_x = area.X + XAmount;
			}
			else {
				src_x = area.X - XAmount;
				width = area.Width + XAmount;
				dest_x = area.X;
			}

			Xlib.XCopyArea (display.Handle, ClientWindow, ClientWindow, gc, src_x, src_y, width, height, dest_x, dest_y);

			// Generate an expose for the area exposed by the horizontal scroll
			// We don't use AddExpose since we're 
			if (XAmount > 0) {
				AddExpose (true, area.X, area.Y, XAmount, area.Height);
			} else if (XAmount < 0) {
				AddExpose (true, XAmount + area.X + area.Width, area.Y, -XAmount, area.Height);
			}

			// Generate an expose for the area exposed by the vertical scroll
			if (YAmount > 0) {
				AddExpose (true, area.X, area.Y, area.Width, YAmount);
			} else if (YAmount < 0) {
				AddExpose (true, area.X, YAmount + area.Y + area.Height, area.Width, -YAmount);
			}

			Xlib.XFreeGC (display.Handle, gc);
		}


		public void SetBorderStyle (FormBorderStyle border_style)
		{
			Form form = Control.FromHandle (Handle) as Form;
			if (form != null && form.window_manager == null && (border_style == FormBorderStyle.FixedToolWindow ||
									    border_style == FormBorderStyle.SizableToolWindow)) {
				form.window_manager = new ToolWindowManager (form);
			}
			
			BorderStyle = border_style;
			RequestNCRecalc ();
		}

		// XXX this should probably be in Hwnd
		public void SetClipRegion (Region region)
		{
			UserClip = region;
			Invalidate (new Rectangle(0, 0, Width, Height), false);
		}

		// XXX this should probably be in Hwnd
		public Region GetClipRegion ()
		{
			return UserClip;
		}

		public void SetMenu (Menu menu)
		{
			Menu = menu;

			RequestNCRecalc ();
		}

		public void SetMinMax (Rectangle maximized, Size min, Size max)
		{
			XSizeHints	hints;
			IntPtr		dummy;

			hints = new XSizeHints();

			Xlib.XGetWMNormalHints (display.Handle, WholeWindow, ref hints, out dummy);
			if ((min != Size.Empty) && (min.Width > 0) && (min.Height > 0)) {
				hints.flags = (IntPtr)((int)hints.flags | (int)XSizeHintsFlags.PMinSize);
				hints.min_width = min.Width;
				hints.min_height = min.Height;
			}

			if ((max != Size.Empty) && (max.Width > 0) && (max.Height > 0)) {
				hints.flags = (IntPtr)((int)hints.flags | (int)XSizeHintsFlags.PMaxSize);
				hints.max_width = max.Width;
				hints.max_height = max.Height;
			}

			if (hints.flags != IntPtr.Zero)
				Xlib.XSetWMNormalHints (display.Handle, WholeWindow, ref hints);

			if ((maximized != Rectangle.Empty) && (maximized.Width > 0) && (maximized.Height > 0)) {
				hints.flags = (IntPtr)XSizeHintsFlags.PPosition;
				hints.x = maximized.X;
				hints.y = maximized.Y;
				hints.width = maximized.Width;
				hints.height = maximized.Height;

				// Metacity does not seem to follow this constraint for maximized (zoomed) windows
				Xlib.XSetZoomHints (display.Handle, WholeWindow, ref hints);
			}
		}

		// For WM_LBUTTONDOWN, WM_MBUTTONDOWN, WM_RBUTTONDOWN, WM_XBUTTONDOWN
		//     WM_CREATE and WM_DESTROY causes
		public void SendParentNotify (Msg cause, int x, int y)
		{
			if (Handle == IntPtr.Zero)
				return;

			if (ExStyleSet ((int) initial_ex_style, WindowExStyles.WS_EX_NOPARENTNOTIFY))
				return;

			if (Parent == null || Parent.Handle == IntPtr.Zero)
				return;

			if (cause == Msg.WM_CREATE || cause == Msg.WM_DESTROY) {
				display.SendMessage(Parent.Handle, Msg.WM_PARENTNOTIFY, Control.MakeParam((int)cause, 0), Handle);
			} else {
				display.SendMessage(Parent.Handle, Msg.WM_PARENTNOTIFY, Control.MakeParam((int)cause, 0), Control.MakeParam(x, y));
			}
			
			((X11Hwnd)Parent).SendParentNotify (cause, x, y);
		}


		public void GetPosition (bool is_toplevel, out int x, out int y, out int width, out int height, out int client_width, out int client_height)
		{
			x = X;
			y = Y;
			width = Width;
			height = Height;

			PerformNCCalc ();

			client_width = ClientRect.Width;
			client_height = ClientRect.Height;
		}

		public void SetPosition (int x, int y, int width, int height)
		{
			// Win32 automatically changes negative width/height to 0.
			if (width < 0)
				width = 0;
			if (height < 0)
				height = 0;

			// X requires a sanity check for width & height; otherwise it dies
			if (zero_sized && width > 0 && height > 0) {
				if (Visible) {
					Map ();
				}
				zero_sized = false;
			}

			if ((width < 1) || (height < 1)) {
				zero_sized = true;
				Unmap ();
			}

			// Save a server roundtrip (and prevent a feedback loop)
			if ((X == x) && (Y == y) && 
				(Width == width) && (Height == height)) {
				return;
			}

			X = x;
			Y = y;
			Width = width;
			Height = height;
			display.SendMessage (Handle, Msg.WM_WINDOWPOSCHANGED, IntPtr.Zero, IntPtr.Zero);

			if (!zero_sized) {
				if (fixed_size) {
					SetMinMax (Rectangle.Empty, new Size(width, height), new Size(width, height));
				}

				Xlib.XMoveResizeWindow (display.Handle, WholeWindow, x, y, width, height);
				PerformNCCalc ();
			}

			// Update our position/size immediately, so
			// that future calls to SetWindowPos aren't
			// kept from calling XMoveResizeWindow (by the
			// "Save a server roundtrip" block above).
			X = x;
			Y = y;
			Width = width;
			Height = height;
			ClientRect = Rectangle.Empty;
		}

		public void SetParent (X11Hwnd parent_hwnd)
		{
			Parent = parent_hwnd;

#if DriverDebug || DriverDebugParent
			Console.WriteLine("Parent for window {0} = {1}", XplatUI.Window(Handle), XplatUI.Window(hwnd.parent != null ? parent_hwnd.Handle : IntPtr.Zero));
#endif
			Xlib.XReparentWindow (display.Handle, WholeWindow,
					      parent_hwnd == null ? display.FosterParent.ClientWindow : parent_hwnd.ClientWindow,
					      X, Y);
		}

		public void SetCursorPos (int x, int y)
		{
			Xlib.XWarpPointer (display.Handle, IntPtr.Zero, ClientWindow, 0, 0, 0, 0, x, y);
		}
		
		public bool SetTopmost (bool enabled)
		{
			if (enabled) {
				int[] atoms = new int[8];
				atoms[0] = display.Atoms._NET_WM_STATE_ABOVE.ToInt32();
				Xlib.XChangeProperty (display.Handle, WholeWindow, display.Atoms._NET_WM_STATE, (IntPtr)Atom.XA_ATOM, 32, PropertyMode.Replace, atoms, 1);
			}
			else {
				Xlib.XDeleteProperty (display.Handle, WholeWindow, display.Atoms._NET_WM_STATE);
			}

			return true;
		}

		public bool SetOwner (X11Hwnd owner)
		{
			this.owner = owner;
			if (owner != null) {
				WINDOW_TYPE = display.Atoms._NET_WM_WINDOW_TYPE_NORMAL;
				if (owner != null)
					Xlib.XSetTransientForHint (display.Handle, WholeWindow, owner.WholeWindow);
				else
					Xlib.XSetTransientForHint (display.Handle, WholeWindow, display.RootWindow.WholeWindow);
			}
			else {
				Xlib.XDeleteProperty (display.Handle, WholeWindow, display.Atoms.XA_WM_TRANSIENT_FOR);
			}

			return true;
		}

		public bool SetVisible (bool visible, bool activate)
		{
			Visible = visible;

			if (visible) {
				Map ();

				if (Control.FromHandle (Handle) is Form) {
					FormWindowState	s;

					s = ((Form)Control.FromHandle(Handle)).WindowState;

					switch(s) {
					case FormWindowState.Minimized:	SetWindowState (FormWindowState.Minimized); break;
					case FormWindowState.Maximized:	SetWindowState (FormWindowState.Maximized); break;
					}

				}

				display.SendMessage (Handle, Msg.WM_WINDOWPOSCHANGED, IntPtr.Zero, IntPtr.Zero);
			}
			else {
				Unmap ();
			}

			return true;
		}

		public FormWindowState GetWindowState ()
		{
			IntPtr			actual_atom;
			int			actual_format;
			IntPtr			nitems;
			IntPtr			bytes_after;
			IntPtr			prop = IntPtr.Zero;
			IntPtr			atom;
			int			maximized;
			bool			minimized;
			XWindowAttributes	attributes;

			maximized = 0;
			minimized = false;
			Xlib.XGetWindowProperty (display.Handle, WholeWindow,
						 display.Atoms._NET_WM_STATE, IntPtr.Zero, new IntPtr (256), false,
						 display.Atoms.XA_ATOM, out actual_atom, out actual_format, out nitems, out bytes_after, ref prop);

			if (((long)nitems > 0) && (prop != IntPtr.Zero)) {
				for (int i = 0; i < (long)nitems; i++) {
					// XXX 64 bit clean?
					atom = (IntPtr)Marshal.ReadInt32(prop, i * 4);
					if ((atom == display.Atoms._NET_WM_STATE_MAXIMIZED_HORZ) || (atom == display.Atoms._NET_WM_STATE_MAXIMIZED_VERT))
						maximized++;
					else if (atom == display.Atoms._NET_WM_STATE_HIDDEN)
						minimized = true;
				}
				Xlib.XFree(prop);
			}

			if (minimized)
				return FormWindowState.Minimized;
			else if (maximized == 2)
				return FormWindowState.Maximized;

			attributes = new XWindowAttributes();
			Xlib.XGetWindowAttributes (display.Handle, ClientWindow, ref attributes);
			if (attributes.map_state == MapState.IsUnmapped)
				return (FormWindowState)(-1);

			return FormWindowState.Normal;
		}


		public void SetWindowState (FormWindowState state)
		{
			FormWindowState	current_state;

			current_state = GetWindowState ();

			if (current_state == state)
				return;

			switch (state) {
			case FormWindowState.Normal:
				if (current_state == FormWindowState.Minimized)
					Map ();
				else if (current_state == FormWindowState.Maximized)
					display.SendNetWMMessage (WholeWindow,
								  display.Atoms._NET_WM_STATE, (IntPtr)2 /* toggle */,
								  display.Atoms._NET_WM_STATE_MAXIMIZED_HORZ,
								  display.Atoms._NET_WM_STATE_MAXIMIZED_VERT);
				Activate ();
				break;

			case FormWindowState.Minimized:
				if (current_state == FormWindowState.Maximized)
					display.SendNetWMMessage (WholeWindow,
								  display.Atoms._NET_WM_STATE, (IntPtr)2 /* toggle */,
								  display.Atoms._NET_WM_STATE_MAXIMIZED_HORZ,
								  display.Atoms._NET_WM_STATE_MAXIMIZED_VERT);

				// FIXME multiscreen support
				Xlib.XIconifyWindow (display.Handle, WholeWindow, display.DefaultScreen);
				break;

			case FormWindowState.Maximized:
				if (current_state == FormWindowState.Minimized)
					Map ();

				display.SendNetWMMessage (WholeWindow,
							  display.Atoms._NET_WM_STATE, (IntPtr)1 /* Add */,
							  display.Atoms._NET_WM_STATE_MAXIMIZED_HORZ,
							  display.Atoms._NET_WM_STATE_MAXIMIZED_VERT);
				Activate ();
				break;
			}
		}

		public bool SetZOrder (X11Hwnd after_hwnd, bool top, bool bottom)
		{
			if (top) {
				Xlib.XRaiseWindow (display.Handle, WholeWindow);
				return true;
			}
			else if (bottom) {
				Xlib.XLowerWindow (display.Handle, WholeWindow);
				return true;
			}
			else {
				if (after_hwnd == null) {
					Update_USER_TIME ();
					Xlib.XRaiseWindow (display.Handle, WholeWindow);
					display.SendNetWMMessage (WholeWindow, display.Atoms._NET_ACTIVE_WINDOW, (IntPtr)1, IntPtr.Zero, IntPtr.Zero);
					return true;
				}

				XWindowChanges values = new XWindowChanges();
				values.sibling = after_hwnd.WholeWindow;
				values.stack_mode = StackMode.Below;

				Xlib.XConfigureWindow (display.Handle, WholeWindow, ChangeWindowFlags.CWStackMode | ChangeWindowFlags.CWSibling, ref values);
			}
			return false;
		}

		public X11Display Display {
			get { return display; }
		}

		public string Text {
			get { return text; }
			set {
				if (value == null)
					value = "";

				if (value == text)
					return;

				text = value;

				Xlib.XChangeProperty(display.Handle, WholeWindow,
						     display.Atoms._NET_WM_NAME, display.Atoms.UNICODETEXT, 8,
						     PropertyMode.Replace, text, Encoding.UTF8.GetByteCount (text));

				// XXX this has problems with UTF8.
				// we need to either use the actual
				// text if it's latin-1, or convert it
				// to compound text if it's in a
				// different charset.
				Xlib.XStoreName(display.Handle, WholeWindow, text);
			}
		}

		public bool GetText (out string text)
		{
			IntPtr actual_atom;
			int actual_format;
			IntPtr nitems;
			IntPtr bytes_after;
			IntPtr prop = IntPtr.Zero;

			Xlib.XGetWindowProperty (display.Handle, WholeWindow,
						 display.Atoms._NET_WM_NAME, IntPtr.Zero, new IntPtr (1), false,
						 display.Atoms.UNICODETEXT, out actual_atom, out actual_format,
						 out nitems, out bytes_after, ref prop);

			if ((long)nitems > 0 && prop != IntPtr.Zero) {
				text = Marshal.PtrToStringUni (prop, (int)nitems);
				Xlib.XFree (prop);
				return true;
			}
			else {
				// fallback on the non-_NET property
				IntPtr	textptr;

				textptr = IntPtr.Zero;

				Xlib.XFetchName (display.Handle, WholeWindow, ref textptr);
				if (textptr != IntPtr.Zero) {
					text = Marshal.PtrToStringAnsi(textptr);
					Xlib.XFree(textptr);
					return true;
				} else {
					text = "";
					return false;
				}
			}
		}

		public IntPtr WINDOW_TYPE {
			get {
				if (refetch_window_type) {
					window_type = GetAtomListProperty (display.Atoms._NET_WM_WINDOW_TYPE);
					refetch_window_type = false;
				}

				return window_type.Length > 0 ? window_type[0] : IntPtr.Zero;
			}
			set {
				Set_WINDOW_TYPE (new IntPtr[] {value}, 1);
			}
		}

		public void Set_WINDOW_TYPE (IntPtr[] value, int count)
		{
			if (refetch_window_type) {
				window_type = GetAtomListProperty (display.Atoms._NET_WM_WINDOW_TYPE);
				refetch_window_type = false;
			}

			if (ArrayDifferent (window_type, value)) {
				window_type = value;
				Xlib.XChangeProperty (display.Handle, WholeWindow,
						      display.Atoms._NET_WM_WINDOW_TYPE, display.Atoms.XA_ATOM, 32,
						      PropertyMode.Replace, window_type, window_type.Length);
			}
		}

		public void Set_WM_STATE (IntPtr[] value, int count)
		{
			if (ArrayDifferent (wm_state, value)) {
				wm_state = value;
				Xlib.XChangeProperty (display.Handle, WholeWindow,
						      display.Atoms._NET_WM_STATE, display.Atoms.XA_ATOM, 32,
						      PropertyMode.Replace, wm_state, wm_state.Length);
			}
		}

		public void Update_USER_TIME ()
		{
			int[] args;

			args = new int[2];
			args[0] = display.CurrentTimestamp;
			Xlib.XChangeProperty (display.Handle, WholeWindow,
					      display.Atoms._NET_WM_USER_TIME, display.Atoms.XA_CARDINAL, 32,
					      PropertyMode.Replace, args, 1);
		}

		public IntPtr[] GetAtomListProperty (IntPtr atom)
		{
			IntPtr	actual_atom;
			int	actual_format;
			IntPtr	nitems;
			IntPtr	bytes_after;
			IntPtr	prop = IntPtr.Zero;

			Xlib.XGetWindowProperty (display.Handle, WholeWindow,
						 atom, IntPtr.Zero, new IntPtr (Int32.MaxValue), false,
						 display.Atoms.XA_ATOM, out actual_atom, out actual_format, out nitems, out bytes_after, ref prop);

			if (actual_atom != display.Atoms.XA_ATOM ||
			    (long)nitems == 0 ||
			    prop == IntPtr.Zero) {
				return new IntPtr[0];
			}

			IntPtr[] values = new IntPtr[(long)nitems];
			int ofs = 0;

			for (int i = 0; i < values.Length; i ++) {
				values[i] = Marshal.ReadIntPtr (prop, ofs);  ofs += IntPtr.Size;
			}

			Xlib.XFree (prop);

			return values;
		}

		bool ArrayDifferent (IntPtr[] a, IntPtr[] b)
		{
			if (a.Length != b.Length)
				return true;

			for (int i = 0; i < a.Length; i ++) {
				if (a[i] != b[i])
					return true;
			}

			return false;
		}
	}
}
