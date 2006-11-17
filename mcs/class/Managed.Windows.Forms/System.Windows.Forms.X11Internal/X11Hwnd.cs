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
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace System.Windows.Forms.X11Internal {

	internal class X11Hwnd : Hwnd
	{
		X11Display display;
		int[] wm_state = new int[0];
		int[] window_type = new int[0];
		string text;
		X11ThreadQueue queue;

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
			WholeWindow = ClientWindow = handle;
			if (handle == IntPtr.Zero) {
				Console.WriteLine("X11Hwnd crated with null handle");
			}
		}

		// XXX this needs to be here so we don't have to change Hwnd.
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
			int[]			atoms;

			Attributes = new XSetWindowAttributes();
			x = cp.X;
			y = cp.Y;
			width = cp.Width;
			height = cp.Height;

			if (width<1) width=1;
			if (height<1) height=1;

			if (cp.Parent != IntPtr.Zero) {
				ParentHandle = Hwnd.ObjectFromHandle(cp.Parent).ClientWindow;
			} else {
				if (StyleSet (cp.Style, WindowStyles.WS_CHILD)) {
					// We need to use our foster parent window until this poor child gets it's parent assigned
					ParentHandle = display.FosterParent.Handle;
				} else if (StyleSet (cp.Style, WindowStyles.WS_POPUP)) {
					ParentHandle = display.RootWindow.Handle;
				} else {
					// Default position on screen, if window manager doesn't place us somewhere else
					if (x<0) x = 50;
					if (y<0) y = 50;
					ParentHandle = display.RootWindow.Handle;
				}
			}

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

			X = x;
			Y = y;
			Width = width;
			Height = height;
			Parent = Hwnd.ObjectFromHandle (cp.Parent);

			if (StyleSet (cp.Style, WindowStyles.WS_DISABLED)) {
				Enabled = false;
			}

			ClientWindow = IntPtr.Zero;

			WholeWindow = Xlib.XCreateWindow (display.Handle, ParentHandle,
							  X, Y, Width, Height, 0,
							  (int)CreateWindowArgs.CopyFromParent, (int)CreateWindowArgs.InputOutput,
							  IntPtr.Zero, new UIntPtr ((uint)ValueMask), ref Attributes);

			if (WholeWindow != IntPtr.Zero) {
				ValueMask &= ~(SetWindowValuemask.OverrideRedirect | SetWindowValuemask.SaveUnder);

				if (display.CustomVisual != IntPtr.Zero && display.CustomColormap != IntPtr.Zero) {
					ValueMask = SetWindowValuemask.ColorMap;
					Attributes.colormap = display.CustomColormap;
				}

				ClientWindow = Xlib.XCreateWindow (display.Handle, WholeWindow,
								   ClientRect.X, ClientRect.Y, ClientRect.Width, ClientRect.Height, 0,
								   (int)CreateWindowArgs.CopyFromParent, (int)CreateWindowArgs.InputOutput,
								   display.CustomVisual, new UIntPtr ((uint)ValueMask), ref Attributes);
			}

			if ((WholeWindow == IntPtr.Zero) || (ClientWindow == IntPtr.Zero)) {
				throw new Exception("Could not create X11 windows");
			}

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

			if (StyleSet (cp.Style, WindowStyles.WS_VISIBLE)) {
				Map ();
				Visible = true;
			}

			if (ExStyleSet (cp.ExStyle, WindowExStyles.WS_EX_TOPMOST)) {
				atoms = new int[2];
				atoms[0] = display.Atoms._NET_WM_WINDOW_TYPE_NORMAL.ToInt32();
				Xlib.XChangeProperty (display.Handle, WholeWindow,
						      display.Atoms._NET_WM_WINDOW_TYPE, display.Atoms.XA_ATOM, 32,
						      PropertyMode.Replace, atoms, 1);

				Xlib.XSetTransientForHint (display.Handle, WholeWindow, display.RootWindow.Handle);
			}

			SetWMStyles (cp);
			
			// set the group leader
			XWMHints wm_hints = new XWMHints ();
			
			wm_hints.flags = (IntPtr)(XWMHintsFlags.InputHint | XWMHintsFlags.StateHint | XWMHintsFlags.WindowGroupHint);
			wm_hints.input = !StyleSet (cp.Style, WindowStyles.WS_DISABLED);
			wm_hints.initial_state = StyleSet (cp.Style, WindowStyles.WS_MINIMIZE) ? XInitialState.IconicState : XInitialState.NormalState;
			
			if (ParentHandle != display.RootWindow.Handle) {
				wm_hints.window_group = WholeWindow;
			} else {
				wm_hints.window_group = ParentHandle;
			}
			
			Xlib.XSetWMHints (display.Handle, WholeWindow, ref wm_hints );

			if (StyleSet (cp.Style, WindowStyles.WS_MINIMIZE)) {
				SetWindowState (FormWindowState.Minimized);
			} else if (StyleSet (cp.Style, WindowStyles.WS_MAXIMIZE)) {
				SetWindowState (FormWindowState.Maximized);
			}

			// for now make all windows dnd enabled
			display.Dnd.SetAllowDrop (this, true);

			// Set caption/window title
			Text = cp.Caption;
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
			if (true /* the window manager supports NET_ACTIVE_WINDOW */) {
				display.SendNetWMMessage (WholeWindow, display.Atoms._NET_ACTIVE_WINDOW, (IntPtr)1, IntPtr.Zero, IntPtr.Zero);
			}
// 			else {
// 				XRaiseWindow(DisplayHandle, handle);
// 			}
		}

		public void Update ()
		{
			if (!Visible || !expose_pending || !Mapped)
				return;

			display.SendMessage (ClientWindow, Msg.WM_PAINT, IntPtr.Zero, IntPtr.Zero);
			Queue.RemovePaint (this);
		}

		public void MenuToScreen (ref int x, ref int y)
		{
			int	dest_x_return;
			int	dest_y_return;
			IntPtr	child;

			Xlib.XTranslateCoordinates (display.Handle, WholeWindow, display.RootWindow.Handle,
						    x, y, out dest_x_return, out dest_y_return, out child);

			x = dest_x_return;
			y = dest_y_return;
		}

		public virtual void PropertyChanged (XEvent xevent)
		{
			if (xevent.PropertyEvent.atom == display.Atoms._NET_WM_WINDOW_TYPE) {
				// we need to recache our WINDOW_TYPE on the next query
			}
			else if (xevent.PropertyEvent.atom == display.Atoms._NET_WM_STATE) {
				// we need to recache our WM_STATE on the next query
			}
			else if (xevent.PropertyEvent.atom == display.Atoms._NET_WM_NAME) {
				// update our Text property
			}
			else if (xevent.PropertyEvent.atom == display.Atoms._NET_WM_WINDOW_OPACITY) {
				// update the Hwnd's opacity
			}
			// else we don't care about it
		}

		public void SetIcon (Icon icon)
		{
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

		public void AddExpose (bool client, int x, int y, int width, int height)
		{
			// Don't waste time
			if ((x > Width) || (y > Height) || ((x + width) < 0) || ((y + height) < 0)) {
				return;
			}

			// Keep the invalid area as small as needed
			if ((x + width) > Width) {
				width = Width - x;
			}

			if ((y + height) > Height) {
				height = Height - y;
			}

			if (client) {
				AddInvalidArea(x, y, width, height);
				if (!expose_pending) {
					if (!nc_expose_pending) {
						Queue.AddPaint (this);
					}
					expose_pending = true;
				}
			}
			else {
				AddNcInvalidArea (x, y, width, height);
				
				if (!nc_expose_pending) {
					if (!expose_pending) {
						Queue.AddPaint (this);
					}
					nc_expose_pending = true;
				}
			}
		}

		public void AddConfigureNotify (XEvent xevent)
		{
			if ((xevent.ConfigureEvent.window == WholeWindow) && (xevent.ConfigureEvent.window == xevent.ConfigureEvent.xevent)) {
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
					Queue.Enqueue(xevent);
					configure_pending = true;
				}
			}
			// We drop configure events for Client windows
		}

		public void Invalidate (Rectangle rc, bool clear)
		{
			if (clear) {
				AddExpose (true, X, Y, Width, Height);
			} else {
				AddExpose (true, rc.X, rc.Y, rc.Width, rc.Height);
			}
		}

		private void InvalidateWholeWindow ()
		{
			InvalidateWholeWindow (new Rectangle(0, 0, Width, Height));
		}

		private void InvalidateWholeWindow (Rectangle rectangle)
		{
			AddExpose (false, rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height);
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

			if ((rect.Width > 0) && (rect.Height > 0)) {
				Xlib.XDrawRectangle (display.Handle, ClientWindow, gc, rect.Left, rect.Top, rect.Width, rect.Height);
			} else {
				if (rect.Width > 0) {
					Xlib.XDrawLine (display.Handle, ClientWindow, gc, rect.X, rect.Y, rect.Right, rect.Y);
				} else {
					Xlib.XDrawLine (display.Handle, ClientWindow, gc, rect.X, rect.Y, rect.X, rect.Bottom);
				}
			}

			Xlib.XFreeGC (display.Handle, gc);
		}

		public void Map ()
		{
			// FIXME why do we set this here and also in the MapNotify event handling?
			mapped = true;
			Xlib.XMapWindow (display.Handle, WholeWindow);
			if (WholeWindow != ClientWindow)
				Xlib.XMapWindow (display.Handle, ClientWindow);
		}

		public void Unmap ()
		{
			// FIXME why do we set this here and also in the UnmapNotify event handling?
			mapped = false;
			Xlib.XUnmapWindow (display.Handle, WholeWindow);
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
				if ((rect.Width < 1) || (rect.Height < 1)) {
					Xlib.XMoveResizeWindow (display.Handle, ClientWindow, -5, -5, 1, 1);
				} else {
					Xlib.XMoveResizeWindow (display.Handle, ClientWindow, rect.X, rect.Y, rect.Width, rect.Height);
				}
			}

			InvalidateWholeWindow ();
		}

		public void RequestNCRecalc ()
		{
			PerformNCCalc ();
			display.SendMessage (Handle, Msg.WM_WINDOWPOSCHANGED, IntPtr.Zero, IntPtr.Zero);
			InvalidateWholeWindow ();
		}

		public void FrameExtents (out int left, out int top)
		{
			IntPtr			actual_atom;
			int			actual_format;
			IntPtr			nitems;
			IntPtr			bytes_after;
			IntPtr			prop = IntPtr.Zero;

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
		public static void DeriveStyles(int Style, int ExStyle,
						out FormBorderStyle border_style, out TitleStyle title_style, out int caption_height, out int tool_caption_height)
		{

			// Only MDI windows get caption_heights
			caption_height = 0;
			tool_caption_height = 19;

			if (StyleSet (Style, WindowStyles.WS_CHILD)) {
				if (ExStyleSet (ExStyle, WindowExStyles.WS_EX_CLIENTEDGE)) {
					border_style = FormBorderStyle.Fixed3D;
				} else if (!StyleSet (Style, WindowStyles.WS_BORDER)) {
					border_style = FormBorderStyle.None;
				} else {
					border_style = FormBorderStyle.FixedSingle;
				}
				title_style = TitleStyle.None;

				if (ExStyleSet (ExStyle, WindowExStyles.WS_EX_MDICHILD)) {
					caption_height = 26;

					if (StyleSet (Style, WindowStyles.WS_CAPTION)) {
						if (ExStyleSet (ExStyle, WindowExStyles.WS_EX_TOOLWINDOW)) {
							title_style = TitleStyle.Tool;
						} else {
							title_style = TitleStyle.Normal;
						}
					}

					if (StyleSet (Style, WindowStyles.WS_OVERLAPPEDWINDOW) ||
					    ExStyleSet (ExStyle, WindowExStyles.WS_EX_TOOLWINDOW)) {
						border_style = (FormBorderStyle) 0xFFFF;
					} else {
						border_style = FormBorderStyle.None;
					}
				}
			}
			else {
				title_style = TitleStyle.None;
				if (StyleSet (Style, WindowStyles.WS_CAPTION)) {
					if (ExStyleSet (ExStyle, WindowExStyles.WS_EX_TOOLWINDOW)) {
						title_style = TitleStyle.Tool;
					} else {
						title_style = TitleStyle.Normal;
					}
				}

				border_style = FormBorderStyle.None;

				if (StyleSet (Style, WindowStyles.WS_THICKFRAME)) {
					if (ExStyleSet (ExStyle, WindowExStyles.WS_EX_TOOLWINDOW)) {
						border_style = FormBorderStyle.SizableToolWindow;
					} else {
						border_style = FormBorderStyle.Sizable;
					}
				} else {
					if (StyleSet (Style, WindowStyles.WS_CAPTION)) {
						if (ExStyleSet (ExStyle, WindowExStyles.WS_EX_CLIENTEDGE)) {
							border_style = FormBorderStyle.Fixed3D;
						} else if (ExStyleSet (ExStyle, WindowExStyles.WS_EX_DLGMODALFRAME)) {
							border_style = FormBorderStyle.FixedDialog;
						} else if (ExStyleSet (ExStyle, WindowExStyles.WS_EX_TOOLWINDOW)) {
							border_style = FormBorderStyle.FixedToolWindow;
						} else if (StyleSet (Style, WindowStyles.WS_BORDER)) {
							border_style = FormBorderStyle.FixedSingle;
						}
					} else {
						if (StyleSet (Style, WindowStyles.WS_BORDER)) {
							border_style = FormBorderStyle.FixedSingle;
						}
					}
				}
			}
		}

		public void SetHwndStyles (CreateParams cp)
		{
			DeriveStyles(cp.Style, cp.ExStyle, out this.border_style, out this.title_style, out this.caption_height, out this.tool_caption_height);
		}

		public void SetWMStyles (CreateParams cp)
		{
			MotifWmHints		mwmHints;
			MotifFunctions		functions;
			MotifDecorations	decorations;
			int[]			atoms;
			int			atom_count;
			Rectangle		client_rect;

			// Child windows don't need WM window styles
			if (StyleSet (cp.Style, WindowStyles.WS_CHILDWINDOW)) {
				return;
			}

			atoms = new int[8];
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
				atoms [0] = display.Atoms._NET_WM_WINDOW_TYPE_UTILITY.ToInt32 ();
				Xlib.XChangeProperty (display.Handle, WholeWindow,
						      display.Atoms._NET_WM_WINDOW_TYPE, display.Atoms.XA_ATOM, 32,
						      PropertyMode.Replace, atoms, 1);

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
				atoms[0] = display.Atoms._NET_WM_WINDOW_TYPE_NORMAL.ToInt32();
				Xlib.XChangeProperty (display.Handle, WholeWindow,
						      display.Atoms._NET_WM_WINDOW_TYPE, display.Atoms.XA_ATOM, 32,
						      PropertyMode.Replace, atoms, 1);

				Xlib.XSetTransientForHint(display.Handle, WholeWindow, parent.WholeWindow);
			} else if (!ExStyleSet (cp.ExStyle, WindowExStyles.WS_EX_APPWINDOW)) {
				/* this line keeps the window from showing up in gnome's taskbar */
				atoms[atom_count++] = display.Atoms._NET_WM_STATE_SKIP_TASKBAR.ToInt32();
			}
			if ((client_rect.Width < 1) || (client_rect.Height < 1)) {
				Xlib.XMoveResizeWindow (display.Handle, ClientWindow, -5, -5, 1, 1);
			} else {
				Xlib.XMoveResizeWindow (display.Handle, ClientWindow, client_rect.X, client_rect.Y, client_rect.Width, client_rect.Height);
			}

			if (ExStyleSet (cp.ExStyle, WindowExStyles.WS_EX_TOOLWINDOW)) {
				atoms[atom_count++] = display.Atoms._NET_WM_STATE_SKIP_TASKBAR.ToInt32();
			}
			/* we need to add these atoms in the
			 * event we're maximized, since we're
			 * replacing the existing
			 * _NET_WM_STATE here.  If we don't
			 * add them, future calls to
			 * GetWindowState will return Normal
			 * for a window which is maximized. */
			if (current_state == FormWindowState.Maximized) {
				atoms[atom_count++] = display.Atoms._NET_WM_STATE_MAXIMIZED_HORZ.ToInt32();
				atoms[atom_count++] = display.Atoms._NET_WM_STATE_MAXIMIZED_VERT.ToInt32();
			}

			Xlib.XChangeProperty (display.Handle, WholeWindow,
					      display.Atoms._NET_WM_STATE, display.Atoms.XA_ATOM, 32,
					      PropertyMode.Replace, atoms, atom_count);

			atom_count = 0;
			IntPtr[] atom_ptrs = new IntPtr[2];
			atom_ptrs[atom_count++] = display.Atoms.WM_DELETE_WINDOW;
			if (ExStyleSet (cp.ExStyle, WindowExStyles.WS_EX_CONTEXTHELP)) {
				atom_ptrs[atom_count++] = display.Atoms._NET_WM_CONTEXT_HELP;
			}

			Xlib.XSetWMProtocols (display.Handle, WholeWindow, atom_ptrs, atom_count);
		}

		public void ClientToScreen (ref int x, ref int y)
		{
			int	dest_x_return;
			int	dest_y_return;
			IntPtr	child;

			Xlib.XTranslateCoordinates (display.Handle, ClientWindow, display.RootWindow.Handle, x, y, out dest_x_return, out dest_y_return, out child);

			x = dest_x_return;
			y = dest_y_return;
		}

		public void ScreenToClient (ref int x, ref int y)
		{
			int	dest_x_return;
			int	dest_y_return;
			IntPtr	child;

			Xlib.XTranslateCoordinates (display.Handle, display.RootWindow.Handle, ClientWindow, x, y, out dest_x_return, out dest_y_return, out child);

			x = dest_x_return;
			y = dest_y_return;
		}


		public void ScreenToMenu (ref int x, ref int y)
		{
			int	dest_x_return;
			int	dest_y_return;
			IntPtr	child;

			Xlib.XTranslateCoordinates (display.Handle, display.RootWindow.Handle, WholeWindow, x, y, out dest_x_return, out dest_y_return, out child);

			x = dest_x_return;
			y = dest_y_return;
		}

		public void ScrollWindow (Rectangle area, int XAmount, int YAmount, bool with_children)
		{
			IntPtr		gc;
			XGCValues	gc_values;
			Rectangle	r;

			r = Invalid;
			if (r != Rectangle.Empty) {
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

				ClearInvalidArea();
				AddInvalidArea(r);
			}

			gc_values = new XGCValues();

			if (with_children) {
				gc_values.subwindow_mode = GCSubwindowMode.IncludeInferiors;
			}

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
				form.window_manager = new InternalWindowManager (form);
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

			if (hints.flags != IntPtr.Zero) {
				Xlib.XSetWMNormalHints (display.Handle, WholeWindow, ref hints);
			}

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

			if (!zero_sized) {
				//Hack?
				X = x;
				Y = y;
				Width = width;
				Height = height;
				display.SendMessage (Handle, Msg.WM_WINDOWPOSCHANGED, IntPtr.Zero, IntPtr.Zero);

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
		
		public bool SetTopmost (X11Hwnd owner, bool enabled)
		{
			if (enabled) {
				int[]	atoms;

				atoms = new int[8];

				atoms[0] = display.Atoms._NET_WM_WINDOW_TYPE_NORMAL.ToInt32();
				Xlib.XChangeProperty (display.Handle, WholeWindow,
						      display.Atoms._NET_WM_WINDOW_TYPE, display.Atoms.XA_ATOM, 32,
						      PropertyMode.Replace, atoms, 1);

				if (owner != null) {
					Xlib.XSetTransientForHint (display.Handle, WholeWindow, owner.WholeWindow);
				} else {
					Xlib.XSetTransientForHint (display.Handle, WholeWindow, display.RootWindow.WholeWindow);
				}
			} else {
				Xlib.XDeleteProperty (display.Handle, WholeWindow, display.Atoms.XA_WM_TRANSIENT_FOR);
			}

			return true;
		}

		public bool SetVisible (bool visible, bool activate)
		{
			Visible = visible;

			if (visible) {
				if (Control.FromHandle (Handle) is Form) {
					FormWindowState	s;

					s = ((Form)Control.FromHandle(Handle)).WindowState;

					Map ();

					switch(s) {
					case FormWindowState.Minimized:	SetWindowState (FormWindowState.Minimized); break;
					case FormWindowState.Maximized:	SetWindowState (FormWindowState.Maximized); break;
					}

				} else {
					Map ();
				}

				display.SendMessage (Handle, Msg.WM_WINDOWPOSCHANGED, IntPtr.Zero, IntPtr.Zero);
			} else {
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
					if ((atom == display.Atoms._NET_WM_STATE_MAXIMIZED_HORZ) || (atom == display.Atoms._NET_WM_STATE_MAXIMIZED_VERT)) {
						maximized++;
					} else if (atom == display.Atoms._NET_WM_STATE_HIDDEN) {
						minimized = true;
					}
				}
				Xlib.XFree(prop);
			}

			if (minimized) {
				return FormWindowState.Minimized;
			} else if (maximized == 2) {
				return FormWindowState.Maximized;
			}

			attributes = new XWindowAttributes();
			Xlib.XGetWindowAttributes (display.Handle, ClientWindow, ref attributes);
			if (attributes.map_state == MapState.IsUnmapped) {
				return (FormWindowState)(-1);
			}


			return FormWindowState.Normal;
		}


		public void SetWindowState (FormWindowState state)
		{
			FormWindowState	current_state;

			current_state = GetWindowState ();

			if (current_state == state)
				return;

			switch (state) {
				case FormWindowState.Normal: {
					if (current_state == FormWindowState.Minimized) {
						Map ();
					} else if (current_state == FormWindowState.Maximized) {
						display.SendNetWMMessage (WholeWindow,
									  display.Atoms._NET_WM_STATE, (IntPtr)2 /* toggle */,
									  display.Atoms._NET_WM_STATE_MAXIMIZED_HORZ,
									  display.Atoms._NET_WM_STATE_MAXIMIZED_VERT);
					}
					Activate ();
					break;
				}

				case FormWindowState.Minimized: {
					if (current_state == FormWindowState.Maximized) {
						display.SendNetWMMessage (WholeWindow,
									  display.Atoms._NET_WM_STATE, (IntPtr)2 /* toggle */,
									  display.Atoms._NET_WM_STATE_MAXIMIZED_HORZ,
									  display.Atoms._NET_WM_STATE_MAXIMIZED_VERT);
					}

					// FIXME multiscreen support
					Xlib.XIconifyWindow (display.Handle, WholeWindow, display.DefaultScreen);
					break;
				}

				case FormWindowState.Maximized: {
					if (current_state == FormWindowState.Minimized) {
						Map ();
					}

					display.SendNetWMMessage (WholeWindow,
								  display.Atoms._NET_WM_STATE, (IntPtr)1 /* Add */,
								  display.Atoms._NET_WM_STATE_MAXIMIZED_HORZ,
								  display.Atoms._NET_WM_STATE_MAXIMIZED_VERT);
					Activate ();
					break;
				}
			}
		}

		public bool SetZOrder (X11Hwnd after_hwnd, bool top, bool bottom)
		{
			if (top) {
				Xlib.XRaiseWindow (display.Handle, WholeWindow);
				return true;
			} else if (!bottom) {
				XWindowChanges	values = new XWindowChanges();

				if (after_hwnd == null) {
					// Work around metacity 'issues'
					int[]	atoms;

					atoms = new int[2];
					atoms[0] = display.CurrentTimestamp;
					Xlib.XChangeProperty (display.Handle, WholeWindow,
							      display.Atoms._NET_WM_USER_TIME, display.Atoms.XA_CARDINAL, 32,
							      PropertyMode.Replace, atoms, 1);

					Xlib.XRaiseWindow (display.Handle, WholeWindow);
					display.SendNetWMMessage (WholeWindow, display.Atoms._NET_ACTIVE_WINDOW, (IntPtr)1, IntPtr.Zero, IntPtr.Zero);
					return true;
				}

				values.sibling = after_hwnd.WholeWindow;
				values.stack_mode = StackMode.Below;

				Xlib.XConfigureWindow (display.Handle, WholeWindow, ChangeWindowFlags.CWStackMode | ChangeWindowFlags.CWSibling, ref values);
			} else {
				// Bottom
				Xlib.XLowerWindow (display.Handle, WholeWindow);
				return true;
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

		public int WINDOW_TYPE {
			get { return window_type.Length > 0 ? window_type[0] : 0; }
			set {
				Set_WINDOW_TYPE (new int[] {value}, 1);
			}
		}

		public void Set_WINDOW_TYPE (int[] value, int count)
		{
			int[] foo = new int[count];
			Array.Copy (value, foo, count);
			Array.Sort (foo);

			if (ArrayDifferent (window_type, foo)) {
				window_type = value;
				Xlib.XChangeProperty (display.Handle, WholeWindow,
						      display.Atoms._NET_WM_WINDOW_TYPE, display.Atoms.XA_ATOM, 32,
						      PropertyMode.Replace, window_type, window_type.Length);
			}
		}

		public void Set_WM_STATE (int[] value, int count)
		{
			int[] foo = new int[count];
			Array.Copy (value, foo, count);
			Array.Sort (foo);

			if (ArrayDifferent (wm_state, foo)) {
				wm_state = value;
				Xlib.XChangeProperty (display.Handle, WholeWindow,
						      display.Atoms._NET_WM_STATE, display.Atoms.XA_ATOM, 32,
						      PropertyMode.Replace, wm_state, wm_state.Length);
			}
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

		bool ArrayDifferent (int[] a, int[] b)
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
