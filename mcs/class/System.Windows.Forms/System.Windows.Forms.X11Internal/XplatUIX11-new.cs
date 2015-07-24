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
//	Chris Toshok	toshok@ximian.com
//
//

// NOTE:
//	This driver understands the following environment variables: (Set the var to enable feature)
//
//	MONO_XEXCEPTIONS	= throw an exception when a X11 error is encountered;
//				  by default a message is displayed but execution continues
//
//	MONO_XSYNC		= perform all X11 commands synchronous; this is slower but
//				  helps in debugging errors
//

// NOT COMPLETE

// define to log Window handles and relationships to stdout
#undef DriverDebug

// Extra detailed debug
#undef DriverDebugExtra
#undef DriverDebugParent
#undef DriverDebugCreate
#undef DriverDebugDestroy
#undef DriverDebugThreads
#undef DriverDebugXEmbed

using System;
using System.ComponentModel;
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

/// X11 Version
namespace System.Windows.Forms.X11Internal {
	internal class XplatUIX11_new : XplatUIDriver {
		#region Local Variables
		// General
		static volatile XplatUIX11_new Instance;
		static readonly object lockobj = new object ();
		static int RefCount;
		static bool themes_enabled;

		static Hashtable MessageQueues; // Holds our thread-specific X11ThreadQueues

		X11Display display;

		#endregion	// Local Variables
		#region Constructors
		private XplatUIX11_new() {
			// Handle singleton stuff first
			RefCount = 0;

			// Now regular initialization
			MessageQueues = Hashtable.Synchronized (new Hashtable(7));
			if (Xlib.XInitThreads() == 0) {
				Console.WriteLine ("Failed XInitThreads.  The X11 event loop will not function properly");
			}
		}

		private void InitializeDisplay ()
		{
			display = new X11Display (Xlib.XOpenDisplay(IntPtr.Zero));

			Graphics.FromHdcInternal (display.Handle);
		}

		~XplatUIX11_new() {
			// Remove our display handle from S.D
			Graphics.FromHdcInternal (IntPtr.Zero);
		}

		#endregion	// Constructors

		#region Singleton Specific Code
		public static XplatUIX11_new GetInstance() {
			lock (lockobj) {
				if (Instance == null) {
					Instance = new XplatUIX11_new ();

					Instance.InitializeDisplay ();
				}
				RefCount++;
			}
			return Instance;
		}

		public int Reference {
			get {
				return RefCount;
			}
		}
		#endregion

		#region Internal Methods
		internal static void Where() {
			Console.WriteLine("Here: {0}\n", GetInstance().display.WhereString());
		}

		#endregion	// Internal Methods

		#region Private Methods

		internal X11ThreadQueue ThreadQueue (Thread thread)
		{
			X11ThreadQueue	queue;

			queue = (X11ThreadQueue)MessageQueues[thread];
			if (queue == null) {
				queue = new X11ThreadQueue(thread);
				MessageQueues[thread] = queue;
			}

			return queue;
		}
		#endregion	// Private Methods


		#region Public Properties
		internal override int CaptionHeight {
			get { return 19; }
		}

		internal override Size CursorSize {
			get { return display.CursorSize; }
		}

		internal override bool DragFullWindows {
			get { return true; }
		} 

		internal override Size DragSize {
			get { return new Size(4, 4); }
		} 

		internal override Size FrameBorderSize { 
			get { return new Size (4, 4); }
		}

		internal override Size IconSize {
			get { return display.IconSize; }
		}

		internal override int KeyboardSpeed {
			get { return display.KeyboardSpeed; }
		}

		internal override int KeyboardDelay {
			get { return display.KeyboardSpeed; }
		}

		internal override Size MaxWindowTrackSize {
			get { return new Size (WorkingArea.Width, WorkingArea.Height); }
		}

		internal override bool MenuAccessKeysUnderlined {
			get {
				return false;
			}
		}

		internal override Size MinimizedWindowSpacingSize {
			get { return new Size(1, 1); }
		} 

		internal override Size MinimumWindowSize {
			get { return new Size(1, 1); }
		} 

		internal override Keys ModifierKeys {
			get { return display.ModifierKeys; }
		}

		internal override Size SmallIconSize {
			get { return display.SmallIconSize; }
		}

		internal override int MouseButtonCount {
			get { return 3; /* FIXME - should detect this somehow.. */}
		} 

		internal override bool MouseButtonsSwapped {
			get { return false; /*FIXME - how to detect? */}
		} 

		internal override Size MouseHoverSize {
			get { return new Size (1, 1); }
		}

		internal override int MouseHoverTime {
			get { return display.MouseHoverTime; }
		}

		internal override bool MouseWheelPresent {
			get { return true; /* FIXME - how to detect? */	}
		} 

		internal override Rectangle VirtualScreen {
			get { return display.VirtualScreen; }
		} 

		internal override Rectangle WorkingArea {
			get { return display.WorkingArea; }
		}

		internal override bool ThemesEnabled {
			get { return XplatUIX11_new.themes_enabled; }
		}
 

		#endregion	// Public properties

		#region Public Static Methods
		internal override void RaiseIdle (EventArgs e)
		{
			X11ThreadQueue queue = ThreadQueue (Thread.CurrentThread);
			queue.OnIdle (e);
		}

		internal override IntPtr InitializeDriver ()
		{
			lock (this) {
				if (display == null)
					display = new X11Display (Xlib.XOpenDisplay(IntPtr.Zero));
			}
			return IntPtr.Zero;
		}

		internal override void ShutdownDriver(IntPtr token)
		{
			lock (this) {
				if (display != null) {
					display.Close ();
					display = null;
				}
			}
		}

		internal override void EnableThemes()
		{
			themes_enabled = true;
		}

		internal override void Activate (IntPtr handle)
		{
			X11Hwnd hwnd = (X11Hwnd)Hwnd.ObjectFromHandle(handle);

			if (hwnd != null)
				hwnd.Activate ();
		}

		internal override void AudibleAlert()
		{
			display.AudibleAlert ();
		}


		internal override void CaretVisible (IntPtr handle, bool visible)
		{
			display.CaretVisible (handle, visible);
		}

		// XXX this implementation should probably be shared between all non-win32 backends
		internal override bool CalculateWindowRect (ref Rectangle ClientRect, CreateParams cp, Menu menu, out Rectangle WindowRect)
		{
			WindowRect = Hwnd.GetWindowRectangle (cp, menu, ClientRect);
			return true;
		}

		internal override void ClientToScreen (IntPtr handle, ref int x, ref int y)
		{
			X11Hwnd hwnd = (X11Hwnd)Hwnd.ObjectFromHandle(handle);

			if (hwnd != null)
				hwnd.ClientToScreen (ref x, ref y);
		}

		internal override int[] ClipboardAvailableFormats (IntPtr handle)
		{
			return display.ClipboardAvailableFormats (handle);
		}

		internal override void ClipboardClose (IntPtr handle)
		{
			display.ClipboardClose (handle);
		}

		internal override int ClipboardGetID (IntPtr handle, string format)
		{
			return display.ClipboardGetID (handle, format);
		}

		internal override IntPtr ClipboardOpen (bool primary_selection)
		{
			return display.ClipboardOpen (primary_selection);
		}

		internal override object ClipboardRetrieve (IntPtr handle, int type, XplatUI.ClipboardToObject converter)
		{
			return display.ClipboardRetrieve (handle, type, converter);
		}

		internal override void ClipboardStore (IntPtr handle, object obj, int type, XplatUI.ObjectToClipboard converter)
		{
			display.ClipboardStore (handle, obj, type, converter);
		}

		internal override void CreateCaret (IntPtr handle, int width, int height)
		{
			display.CreateCaret (handle, width, height);
		}

		internal override IntPtr CreateWindow (CreateParams cp)
		{
			X11Hwnd hwnd = new X11Hwnd (display);

			hwnd.CreateWindow (cp);

			return hwnd.Handle;
		}

		internal override IntPtr CreateWindow (IntPtr Parent, int X, int Y, int Width, int Height)
		{
			CreateParams create_params = new CreateParams();

			create_params.Caption = "";
			create_params.X = X;
			create_params.Y = Y;
			create_params.Width = Width;
			create_params.Height = Height;

			create_params.ClassName = XplatUI.DefaultClassName;
			create_params.ClassStyle = 0;
			create_params.ExStyle = 0;
			create_params.Parent = IntPtr.Zero;
			create_params.Param = 0;

			return CreateWindow (create_params);
		}

		internal override IntPtr DefineCursor (Bitmap bitmap, Bitmap mask, Color cursor_pixel, Color mask_pixel, int xHotSpot, int yHotSpot)
		{
			return display.DefineCursor (bitmap, mask, cursor_pixel, mask_pixel, xHotSpot, yHotSpot);
		}
		internal override Bitmap DefineStdCursorBitmap (StdCursor id) 
		{
			return display.DefineStdCursorBitmap (id);
		}
		internal override IntPtr DefineStdCursor (StdCursor id)
		{
			return display.DefineStdCursor (id);
		}

		internal override IntPtr DefWndProc(ref Message msg)
		{
			X11Hwnd hwnd = (X11Hwnd)Hwnd.GetObjectFromWindow(msg.HWnd);

			if (hwnd == null)
				return IntPtr.Zero;

			return hwnd.DefWndProc (ref msg);
		}

		internal override void DestroyCaret (IntPtr handle)
		{
			display.DestroyCaret (handle);
		}

		internal override void DestroyCursor(IntPtr cursor)
		{
			display.DestroyCursor (cursor);
		}

		internal override void DestroyWindow (IntPtr handle) {
			X11Hwnd hwnd;

			hwnd = (X11Hwnd)Hwnd.ObjectFromHandle(handle);

			if (hwnd == null) {
#if DriverDebug || DriverDebugDestroy
				Console.WriteLine("window {0:X} already destroyed", handle.ToInt32());
#endif
				return;
			}

#if DriverDebug || DriverDebugDestroy
			Console.WriteLine("Destroying window {0}", XplatUI.Window(hwnd.ClientWindow));
#endif

			display.DestroyWindow (hwnd);
		}

		internal override IntPtr DispatchMessage(ref MSG msg)
		{
			return display.DispatchMessage (ref msg);
		}

		internal override void DrawReversibleLine (Point start, Point end, Color backColor)
		{
			display.DrawReversibleLine (start, end, backColor);
		}

		internal override void FillReversibleRectangle (Rectangle rectangle, Color backColor)
		{
			display.FillReversibleRectangle (rectangle, backColor);
		}

		internal override void DrawReversibleFrame (Rectangle rectangle, Color backColor, FrameStyle style)
		{
			display.DrawReversibleFrame (rectangle, backColor, style);
		}

		internal override void DrawReversibleRectangle (IntPtr handle, Rectangle rect, int line_width)
		{
			X11Hwnd hwnd = (X11Hwnd)Hwnd.ObjectFromHandle(handle);

			if (hwnd != null)
				hwnd.DrawReversibleRectangle (rect, line_width);
		}

		internal override void DoEvents ()
		{
			X11ThreadQueue queue = ThreadQueue (Thread.CurrentThread);
			display.DoEvents (queue);
		}

		internal override void EnableWindow (IntPtr handle, bool Enable)
		{
			Hwnd	hwnd;

			hwnd = Hwnd.ObjectFromHandle(handle);
			if (hwnd != null)
				hwnd.Enabled = Enable;
		}

		internal override void EndLoop (Thread thread)
		{
			// This is where we one day will shut down the loop for the thread
		}


		internal override IntPtr GetActive()
		{
			X11Hwnd hwnd = display.GetActiveWindow ();

			return (hwnd == null) ? IntPtr.Zero : hwnd.Handle;
		}

		internal override Region GetClipRegion (IntPtr handle)
		{
			X11Hwnd hwnd = (X11Hwnd)Hwnd.ObjectFromHandle(handle);

			return (hwnd == null) ? null : hwnd.GetClipRegion ();
		}

		internal override void GetCursorInfo(IntPtr cursor, out int width, out int height, out int hotspot_x, out int hotspot_y)
		{
			width = 20;
			height = 20;
			hotspot_x = 0;
			hotspot_y = 0;
		}

		internal override void GetDisplaySize(out Size size)
		{
			display.GetDisplaySize (out size);
		}

		internal override SizeF GetAutoScaleSize (Font font)
		{
			return display.GetAutoScaleSize (font);
		}

		// XXX this should be someplace shareable by all non-win32 backends..  like in Hwnd itself.
		// maybe a Hwnd.ParentHandle property
		internal override IntPtr GetParent (IntPtr handle)
		{
			Hwnd	hwnd;

			hwnd = Hwnd.ObjectFromHandle(handle);
			if (hwnd != null && hwnd.parent != null) {
				return hwnd.parent.Handle;
			}
			return IntPtr.Zero;
		}

		// This is a nop on win32 and x11
		internal override IntPtr GetPreviousWindow(IntPtr handle) {
			return handle;
		}

		internal override void GetCursorPos (IntPtr handle, out int x, out int y)
		{
			display.GetCursorPos ((X11Hwnd)Hwnd.ObjectFromHandle(handle),
					      out x, out y);
		}

		internal override IntPtr GetFocus()
		{
			return display.GetFocus ();
		}

		// XXX this should be shared amongst non-win32 backends
		internal override bool GetFontMetrics (Graphics g, Font font, out int ascent, out int descent)
		{
			FontFamily ff = font.FontFamily;
			ascent = ff.GetCellAscent (font.Style);
			descent = ff.GetCellDescent (font.Style);
			return true;
		}


		// XXX this should be shared amongst non-win32 backends
		internal override Point GetMenuOrigin (IntPtr handle)
		{
			Hwnd	hwnd;

			hwnd = Hwnd.ObjectFromHandle(handle);

			if (hwnd != null)
				return hwnd.MenuOrigin;

			return Point.Empty;
		}

		internal override bool GetMessage (object queue_id, ref MSG msg, IntPtr handle, int wFilterMin, int wFilterMax)
		{
			return display.GetMessage (queue_id, ref msg, handle, wFilterMin, wFilterMax);
		}

		internal override bool GetText (IntPtr handle, out string text)
		{
			X11Hwnd	hwnd = (X11Hwnd) Hwnd.ObjectFromHandle(handle);

			text = "";
			return hwnd != null && hwnd.GetText (out text);
		}

		internal override void GetWindowPos (IntPtr handle, bool is_toplevel,
						     out int x, out int y,
						     out int width, out int height,
						     out int client_width, out int client_height)
		{
			X11Hwnd hwnd = (X11Hwnd)Hwnd.ObjectFromHandle(handle);

			if (hwnd != null) {
				hwnd.GetPosition (is_toplevel, out x, out y, out width, out height, out client_width, out client_height);
			}
			else {
				// Should we throw an exception or fail silently?
				// throw new ArgumentException("Called with an invalid window handle", "handle");

				x = 0;
				y = 0;
				width = 0;
				height = 0;
				client_width = 0;
				client_height = 0;
			}
		}

		internal override FormWindowState GetWindowState (IntPtr handle)
		{
			X11Hwnd hwnd = (X11Hwnd)Hwnd.ObjectFromHandle(handle);

			if (hwnd == null)
				return FormWindowState.Normal; // XXX should we throw an exception here?  probably

			return hwnd.GetWindowState ();
		}

		internal override void GrabInfo (out IntPtr handle, out bool GrabConfined, out Rectangle GrabArea)
		{
			display.GrabInfo (out handle, out GrabConfined, out GrabArea);
		}

		internal override void GrabWindow (IntPtr handle, IntPtr confine_to_handle)
		{
			X11Hwnd hwnd = (X11Hwnd)Hwnd.ObjectFromHandle(handle);
			X11Hwnd confine_to_hwnd = (X11Hwnd)Hwnd.ObjectFromHandle(confine_to_handle);

			display.GrabWindow (hwnd, confine_to_hwnd);
		}

		internal override void UngrabWindow (IntPtr handle)
		{
			X11Hwnd hwnd = (X11Hwnd)Hwnd.ObjectFromHandle(handle);

			display.UngrabWindow (hwnd);
		}

		internal override void HandleException(Exception e) {
			StackTrace st = new StackTrace(e, true);
			Console.WriteLine("Exception '{0}'", e.Message+st.ToString());
			Console.WriteLine("{0}{1}", e.Message, st.ToString());
		}

		internal override void Invalidate (IntPtr handle, Rectangle rc, bool clear)
		{
			X11Hwnd hwnd = (X11Hwnd)Hwnd.ObjectFromHandle(handle);

			hwnd.Invalidate (rc, clear);
		}

		internal override void InvalidateNC (IntPtr handle)
		{
			X11Hwnd hwnd = (X11Hwnd)Hwnd.ObjectFromHandle(handle);

			hwnd.InvalidateNC ();
		}

		internal override bool IsEnabled(IntPtr handle)
		{
			X11Hwnd hwnd = (X11Hwnd)Hwnd.ObjectFromHandle (handle);

			return hwnd != null && hwnd.Enabled;
		}
		
		internal override bool IsVisible(IntPtr handle)
		{
			X11Hwnd hwnd = (X11Hwnd)Hwnd.ObjectFromHandle (handle);

			return hwnd != null && hwnd.Visible;
		}

		internal override void KillTimer (Timer timer)
		{
			X11ThreadQueue queue = (X11ThreadQueue) MessageQueues [timer.thread];

			if (queue == null) {
				// This isn't really an error, MS doesn't start the timer if
				// it has no assosciated queue
				return;
			}
			queue.KillTimer (timer);
		}

		internal override void MenuToScreen (IntPtr handle, ref int x, ref int y)
		{
			X11Hwnd hwnd = (X11Hwnd)Hwnd.ObjectFromHandle(handle);

			if (hwnd != null)
				hwnd.MenuToScreen (ref x, ref y);
		}

		internal override void OverrideCursor (IntPtr cursor)
		{
			display.OverrideCursor = cursor;
		}

		internal override PaintEventArgs PaintEventStart (ref Message m, IntPtr handle, bool client)
		{
			return display.PaintEventStart (ref m, handle, client);
		}

		internal override void PaintEventEnd (ref Message m, IntPtr handle, bool client)
		{
			display.PaintEventEnd (ref m, handle, client);
		}


		internal override bool PeekMessage (object queue_id, ref MSG msg, IntPtr hWnd, int wFilterMin, int wFilterMax, uint flags)
		{
			return display.PeekMessage (queue_id, ref msg, hWnd, wFilterMin, wFilterMax, flags);
		}

		internal override bool PostMessage (IntPtr handle, Msg message, IntPtr wparam, IntPtr lparam)
		{
			return display.PostMessage (handle, message, wparam, lparam);
		}

		internal override void PostQuitMessage(int exitCode)
		{
			display.PostMessage (display.FosterParent.Handle, Msg.WM_QUIT, IntPtr.Zero, IntPtr.Zero);
			display.Flush ();
		}

		[MonoTODO]
		internal override void RequestAdditionalWM_NCMessages (IntPtr handle, bool hover, bool leave)
		{
			X11Hwnd hwnd = (X11Hwnd)Hwnd.ObjectFromHandle(handle);

			if (hwnd != null)
				hwnd.RequestAdditionalWM_NCMessages (hover, leave);
		}
		
		internal override void RequestNCRecalc (IntPtr handle)
		{
			X11Hwnd hwnd = (X11Hwnd)Hwnd.ObjectFromHandle(handle);

			if (hwnd != null)
				hwnd.RequestNCRecalc ();
		}

		internal override void ResetMouseHover (IntPtr handle)
		{
			X11Hwnd hwnd = (X11Hwnd)Hwnd.ObjectFromHandle(handle);

			display.ResetMouseHover (hwnd);
		}

		internal override void ScreenToClient(IntPtr handle, ref int x, ref int y)
		{
			X11Hwnd hwnd = (X11Hwnd)Hwnd.ObjectFromHandle(handle);

			if (hwnd != null)
				hwnd.ScreenToClient (ref x, ref y);
		}

		internal override void ScreenToMenu (IntPtr handle, ref int x, ref int y)
		{
			X11Hwnd hwnd = (X11Hwnd)Hwnd.ObjectFromHandle(handle);

			if (hwnd != null)
				hwnd.ScreenToMenu (ref x, ref y);
		}

		internal override void ScrollWindow (IntPtr handle, Rectangle area, int XAmount, int YAmount, bool with_children)
		{
			X11Hwnd hwnd = (X11Hwnd)Hwnd.ObjectFromHandle(handle);

			if (hwnd != null)
				hwnd.ScrollWindow (area, XAmount, YAmount, with_children);
		}

		internal override void ScrollWindow(IntPtr handle, int XAmount, int YAmount, bool with_children)
		{
			X11Hwnd hwnd = (X11Hwnd)Hwnd.GetObjectFromWindow(handle);

			if (hwnd != null) {
				Rectangle	rect;

				rect = hwnd.ClientRect;
				rect.X = 0;
				rect.Y = 0;
				hwnd.ScrollWindow (rect, XAmount, YAmount, with_children);
			}
		}

		internal override void SendAsyncMethod (AsyncMethodData method)
		{
			display.SendAsyncMethod (method);
		}

		// XXX this is likely shareable amongst other backends
		internal override IntPtr SendMessage (IntPtr handle, Msg message, IntPtr wParam, IntPtr lParam)
		{
			return display.SendMessage (handle, message, wParam, lParam);
		}

		internal override int SendInput(IntPtr handle, Queue keys) { 
			return display.SendInput(handle, keys);
		}


		internal override void SetAllowDrop (IntPtr handle, bool value)
		{
			// We allow drop on all windows
		}

		internal override DragDropEffects StartDrag (IntPtr handle, object data,
							     DragDropEffects allowed_effects)
		{
			return display.StartDrag (handle, data, allowed_effects);
		}

		internal override void SetBorderStyle (IntPtr handle, FormBorderStyle border_style)
		{
			X11Hwnd hwnd = (X11Hwnd)Hwnd.ObjectFromHandle(handle);

			if (hwnd != null)
				hwnd.SetBorderStyle (border_style);
		}

		internal override void SetCaretPos (IntPtr handle, int x, int y)
		{
			display.SetCaretPos (handle, x, y);
		}

		internal override void SetClipRegion (IntPtr handle, Region region)
		{
			X11Hwnd hwnd = (X11Hwnd)Hwnd.ObjectFromHandle(handle);

			if (hwnd != null)
				hwnd.SetClipRegion (region);
		}

		internal override void SetCursor (IntPtr handle, IntPtr cursor)
		{
			display.SetCursor (handle, cursor);
		}

		internal override void SetCursorPos (IntPtr handle, int x, int y)
		{
			if (handle == IntPtr.Zero) {
				display.SetCursorPos (x, y);
			}
			else {
				X11Hwnd hwnd = (X11Hwnd)Hwnd.ObjectFromHandle(handle);

				hwnd.SetCursorPos (x, y);
			}
		}

		internal override void SetFocus (IntPtr handle)
		{
			X11Hwnd hwnd = (X11Hwnd)Hwnd.ObjectFromHandle(handle);

			display.SetFocus (hwnd);
		}

		internal override void SetIcon(IntPtr handle, Icon icon)
		{
			X11Hwnd hwnd = (X11Hwnd)Hwnd.ObjectFromHandle(handle);;

			if (hwnd != null)
				hwnd.SetIcon (icon);
		}

		internal override void SetMenu(IntPtr handle, Menu menu)
		{
			X11Hwnd hwnd = (X11Hwnd)Hwnd.ObjectFromHandle(handle);

			hwnd.SetMenu (menu);
		}

		internal override void SetModal(IntPtr handle, bool Modal)
		{
			X11Hwnd hwnd = (X11Hwnd)Hwnd.ObjectFromHandle(handle);

			if (hwnd != null)
				display.SetModal (hwnd, Modal);
		}

		internal override IntPtr SetParent(IntPtr handle, IntPtr parent)
		{
			X11Hwnd hwnd = (X11Hwnd)Hwnd.ObjectFromHandle(handle);
			X11Hwnd parent_hwnd = (X11Hwnd)Hwnd.ObjectFromHandle(parent);

			if (hwnd != null)
				hwnd.SetParent (parent_hwnd);

			return IntPtr.Zero;
		}

		internal override void SetTimer (Timer timer)
		{
			X11ThreadQueue queue = (X11ThreadQueue) MessageQueues [timer.thread];

			if (queue == null) {
				// This isn't really an error, MS doesn't start the timer if
				// it has no assosciated queue
				return;
			}
			queue.SetTimer (timer);
		}

		internal override bool SetTopmost(IntPtr handle, bool enabled)
		{
			X11Hwnd hwnd = (X11Hwnd) Hwnd.ObjectFromHandle (handle);

			if (hwnd == null)
				return false;

			return hwnd.SetTopmost (enabled);
		}

		internal override bool SetOwner(IntPtr handle, IntPtr handle_owner)
		{
			X11Hwnd hwnd;

			hwnd = (X11Hwnd)Hwnd.ObjectFromHandle(handle);

			if (hwnd == null)
				return false;

			X11Hwnd hwnd_owner = (X11Hwnd)Hwnd.ObjectFromHandle(handle_owner);

			return hwnd.SetOwner (hwnd_owner);
		}

		internal override bool SetVisible (IntPtr handle, bool visible, bool activate)
		{
			X11Hwnd hwnd = (X11Hwnd)Hwnd.ObjectFromHandle(handle);

			return hwnd != null && hwnd.SetVisible (visible, activate);
		}

		internal override void SetWindowMinMax (IntPtr handle, Rectangle maximized, Size min, Size max)
		{
			X11Hwnd hwnd = (X11Hwnd)Hwnd.ObjectFromHandle(handle);

			if (hwnd == null)
				return;

			hwnd.SetMinMax (maximized, min, max);
		}

		internal override void SetWindowPos (IntPtr handle, int x, int y, int width, int height)
		{
			X11Hwnd hwnd = (X11Hwnd)Hwnd.ObjectFromHandle(handle);

			if (hwnd != null)
				hwnd.SetPosition (x, y, width, height);
		}

		internal override void SetWindowState (IntPtr handle, FormWindowState state)
		{
			X11Hwnd hwnd = (X11Hwnd)Hwnd.ObjectFromHandle(handle);

			if (hwnd != null)
				hwnd.SetWindowState (state);
		}

		internal override void SetWindowStyle (IntPtr handle, CreateParams cp)
		{
			X11Hwnd hwnd = (X11Hwnd)Hwnd.ObjectFromHandle(handle);

			if (hwnd != null) {
				hwnd.SetHwndStyles (cp);
				hwnd.SetWMStyles (cp);
			}
		}

		internal override double GetWindowTransparency (IntPtr handle)
		{
			X11Hwnd hwnd = (X11Hwnd)Hwnd.ObjectFromHandle(handle);

			if (hwnd != null)
				return hwnd.GetWindowTransparency ();
			else
				return 0.0;
		}

		internal override void SetWindowTransparency (IntPtr handle, double transparency, Color key)
		{
			X11Hwnd hwnd = (X11Hwnd)Hwnd.ObjectFromHandle(handle);

			if (hwnd != null)
				hwnd.SetWindowTransparency (transparency, key);
		}

		internal override bool SetZOrder (IntPtr handle, IntPtr after_handle, bool top, bool bottom)
		{
			X11Hwnd hwnd = (X11Hwnd)Hwnd.ObjectFromHandle(handle);

			if (hwnd == null || !hwnd.mapped)
				return false;

			X11Hwnd after_hwnd = (X11Hwnd)Hwnd.ObjectFromHandle(after_handle);

			return hwnd.SetZOrder (after_hwnd, top, bottom);
		}

		internal override void ShowCursor (bool show)
		{
			display.ShowCursor (show);
		}

		internal override object StartLoop(Thread thread)
		{
			return (object) ThreadQueue(thread);
		}

		internal override TransparencySupport SupportsTransparency()
		{
			return display.SupportsTransparency ();
		}

		internal override bool SystrayAdd (IntPtr handle, string tip, Icon icon, out ToolTip tt)
		{
			return display.SystrayAdd (handle, tip, icon, out tt);
		}

		internal override bool SystrayChange (IntPtr handle, string tip, Icon icon, ref ToolTip tt)
		{
			return display.SystrayChange (handle, tip, icon, ref tt);
		}

		internal override void SystrayRemove (IntPtr handle, ref ToolTip tt)
		{
			display.SystrayRemove (handle, ref tt);
		}

		NotifyIcon.BalloonWindow balloon_window;

		internal override void SystrayBalloon(IntPtr handle, int timeout, string title, string text, ToolTipIcon icon)
		{
			Control control = Control.FromHandle(handle);
			
			if (control == null)
				return;

			if (balloon_window != null) {
				balloon_window.Close ();
				balloon_window.Dispose ();
			}

			balloon_window = new NotifyIcon.BalloonWindow (handle);
			balloon_window.Title = title;
			balloon_window.Text = text;
			balloon_window.Timeout = timeout;
			balloon_window.Show ();
			
			SendMessage(handle, Msg.WM_USER, IntPtr.Zero, (IntPtr) Msg.NIN_BALLOONSHOW);	
		}

		internal override bool Text (IntPtr handle, string text)
		{
			X11Hwnd	hwnd = (X11Hwnd) Hwnd.ObjectFromHandle(handle);

			if (hwnd != null)
				hwnd.Text = text;

			return true;
		}

		internal override bool TranslateMessage (ref MSG msg)
		{
			return display.TranslateMessage (ref msg);
		}

		internal override void UpdateWindow (IntPtr handle)
		{
			X11Hwnd hwnd = (X11Hwnd)Hwnd.ObjectFromHandle(handle);

			if (hwnd != null)
				hwnd.Update ();
		}

		internal override void CreateOffscreenDrawable (IntPtr handle,
								int width, int height,
								out object offscreen_drawable)
		{
			display.CreateOffscreenDrawable (handle, width, height,
							 out offscreen_drawable);
		}

		internal override void DestroyOffscreenDrawable (object offscreen_drawable)
		{
			display.DestroyOffscreenDrawable (offscreen_drawable);
		}

		internal override Graphics GetOffscreenGraphics (object offscreen_drawable)
		{
			return display.GetOffscreenGraphics (offscreen_drawable);
		}

		internal override void BlitFromOffscreen (IntPtr dest_handle,
							  Graphics dest_dc,
							  object offscreen_drawable,
							  Graphics offscreen_dc,
							  Rectangle r)
		{
			display.BlitFromOffscreen (dest_handle, dest_dc, offscreen_drawable, offscreen_dc, r);
		}

		#endregion	// Public Static Methods

		#region Events
		internal override event EventHandler Idle {
			add {
				Console.WriteLine ("adding idle handler for thread {0}", Thread.CurrentThread.GetHashCode());
				X11ThreadQueue queue = ThreadQueue(Thread.CurrentThread);
				queue.Idle += value;
			}
			remove {
				X11ThreadQueue queue = ThreadQueue(Thread.CurrentThread);
				queue.Idle += value;
			}
		}
		#endregion	// Events
	}
}
