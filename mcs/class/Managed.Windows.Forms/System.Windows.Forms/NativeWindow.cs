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
// Copyright (c) 2004-2008 Novell, Inc.
//
// Authors:
//	Peter Dennis Bartok	pbartok@novell.com
//	Ivan N. Zlatev  	<contact@i-nz.net>
//


// COMPLETE

//#define ExternalExceptionHandler

using System.Runtime.Remoting;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Collections;
using System.Diagnostics;
using System.Drawing;

namespace System.Windows.Forms
{
	public class NativeWindow : MarshalByRefObject, IWin32Window
	{
		IntPtr window_handle = IntPtr.Zero;
		static Hashtable window_collection = new Hashtable();

		[ThreadStatic]
		static NativeWindow WindowCreating;

		#region Public Constructors
		public NativeWindow()
		{
			window_handle=IntPtr.Zero;
		}
		#endregion	// Public Constructors

		#region Public Instance Properties
		public IntPtr Handle {
			get {
				return window_handle;
			}
		}
		#endregion	// Public Instance Properties

		#region Public Static Methods
		public static NativeWindow FromHandle(IntPtr handle)
		{
			return FindFirstInTable (handle);
		}
		#endregion	// Public Static Methods

		#region Private and Internal Methods
		internal void InvalidateHandle()
		{
			RemoveFromTable (this);
			window_handle = IntPtr.Zero;
		}
		#endregion

		#region Public Instance Methods
		public void AssignHandle(IntPtr handle)
		{
			RemoveFromTable (this);
			window_handle = handle;
			AddToTable (this);
			OnHandleChange();
		}

		private static void AddToTable (NativeWindow window)
		{
			IntPtr handle = window.Handle;
			if (handle == IntPtr.Zero)
				return;

			lock (window_collection) {
				object current = window_collection[handle];
				if (current == null) {
					window_collection.Add (handle, window);
				} else {
					NativeWindow currentWindow = current as NativeWindow;
					if (currentWindow != null) {
						if (currentWindow != window) {
							ArrayList windows = new ArrayList ();
							windows.Add (currentWindow);
							windows.Add (window);
							window_collection[handle] = windows;
						}
					} else { // list of windows
						ArrayList windows = (ArrayList) window_collection[handle];
						if (!windows.Contains (window))
							windows.Add (window);
					}
				}
			}
		}

		private static void RemoveFromTable (NativeWindow window)
		{
			IntPtr handle = window.Handle;
			if (handle == IntPtr.Zero)
				return;

			lock (window_collection) {
				object current = window_collection[handle];
				if (current != null) {
					NativeWindow currentWindow = current as NativeWindow;
					if (currentWindow != null) {
						window_collection.Remove (handle);
					} else { // list of windows
						ArrayList windows = (ArrayList) window_collection[handle];
						windows.Remove (window);
						if (windows.Count == 0)
							window_collection.Remove (handle);
						else if (windows.Count == 1)
							window_collection[handle] = windows[0];
					}
				}
			}
		}

		private static NativeWindow FindFirstInTable (IntPtr handle)
		{
			if (handle == IntPtr.Zero)
				return null;

			NativeWindow window = null;
			lock (window_collection) {
				object current = window_collection[handle];
				if (current != null) {
					window = current as NativeWindow;
					if (window == null) {
						ArrayList windows = (ArrayList) current;
						if (windows.Count > 0)
							window = (NativeWindow) windows[0];
					}
				}
			}
			return window;
		}

		public virtual void CreateHandle(CreateParams cp)
		{
			if (cp != null) {
				WindowCreating = this;
				window_handle=XplatUI.CreateWindow(cp);
				WindowCreating = null;

				if (window_handle != IntPtr.Zero)
					AddToTable (this);
			}

		}

		public void DefWndProc(ref Message m)
		{
			m.Result=XplatUI.DefWndProc(ref m);
		}

		public virtual void DestroyHandle()
		{
			if (window_handle != IntPtr.Zero) {
				XplatUI.DestroyWindow(window_handle);
			}
		}

		public virtual void ReleaseHandle()
		{
			RemoveFromTable (this);
			window_handle=IntPtr.Zero;
			OnHandleChange();
		}

		#endregion	// Public Instance Methods

		#region Protected Instance Methods
		~NativeWindow()
		{
		}

		protected virtual void OnHandleChange()
		{
		}

		protected virtual void OnThreadException(Exception e)
		{
			Application.OnThreadException(e);
		}

		protected virtual void WndProc(ref Message m)
		{
			DefWndProc(ref m);
		}

		internal static IntPtr WndProc(IntPtr hWnd, Msg msg, IntPtr wParam, IntPtr lParam)
		{
			IntPtr result = IntPtr.Zero;
			Message	m = new Message();
			m.HWnd = hWnd;
			m.Msg = (int)msg;
			m.WParam = wParam;
			m.LParam = lParam;
			m.Result = IntPtr.Zero;
					
#if debug
			Console.WriteLine("NativeWindow.cs ({0}, {1}, {2}, {3}): result {4}", hWnd, msg, wParam, lParam, m.Result);
#endif
			NativeWindow window = null;
			
			try {
			object current = null;
			lock (window_collection) {
				current = window_collection[hWnd];
			}

			window = current as NativeWindow;
			if (current == null)
				window = EnsureCreated (window, hWnd);

			if (window != null) {
				window.WndProc (ref m);
				result = m.Result;
			} else if (current is ArrayList) {
				ArrayList windows = (ArrayList) current;
				lock (windows) {
					if (windows.Count > 0) {
						window = EnsureCreated ((NativeWindow)windows[0], hWnd);
						window.WndProc (ref m);
						// the first one is the control's one. all others are synthetic,
						// so we want only the result from the control
						result = m.Result;
						for (int i=1; i < windows.Count; i++)
							((NativeWindow)windows[i]).WndProc (ref m);
					}
				}
			} else {
				result = XplatUI.DefWndProc (ref m);
			}
			}
			catch (Exception ex) {
#if !ExternalExceptionHandler
				if (window != null) {
					if (msg == Msg.WM_PAINT && window is Control.ControlNativeWindow) {
						// Replace control with a red cross
						var control = ((Control.ControlNativeWindow)window).Owner;
						control.Hide ();
						var redCross = new Control (control.Parent, string.Empty);
						redCross.BackColor = Color.White;
						redCross.ForeColor = Color.Red;
						redCross.Bounds = control.Bounds;
						redCross.Paint += HandleRedCrossPaint;
					}
 					window.OnThreadException (ex);
				}
#else
				throw;
#endif
			}
			#if debug
				Console.WriteLine("NativeWindow.cs: Message {0}, result {1}", msg, m.Result);
			#endif

			return result;
		}

		private static void HandleRedCrossPaint (object sender, PaintEventArgs e)
		{
			var control = sender as Control;
			using (var pen = new Pen (control.ForeColor, 2)) {
				var paintRect = control.DisplayRectangle;
				e.Graphics.DrawRectangle (pen, paintRect.Left + 1,
					paintRect.Top + 1, paintRect.Width - 1, paintRect.Height - 1);
				// NOTE: .NET's drawing of the red cross seems to have a bug
				// that draws the bottom and right of the rectangle only 1 pixel
				// wide. We would get a nicer rectangle using the following code,
				// but that runs into a problem with libgdiplus.
				//var paintRect = control.DisplayRectangle;
				//paintRect.Inflate (-1, -1);
				//e.Graphics.DrawRectangle (pen, paintRect);
				e.Graphics.DrawLine (pen, paintRect.Location,
					paintRect.Location + paintRect.Size);
				e.Graphics.DrawLine (pen, new Point (paintRect.Left, paintRect.Bottom),
					new Point (paintRect.Right, paintRect.Top));
			}
		}

		private static NativeWindow EnsureCreated (NativeWindow window, IntPtr hWnd)
		{
			// we need to do this AssignHandle here instead of relying on
			// Control.WndProc to do it, because subclasses can override
			// WndProc, install their own WM_CREATE block, and look at
			// this.Handle, and it needs to be set.  Otherwise, we end up
			// recursively creating windows and emitting WM_CREATE.
			if (window == null && WindowCreating != null) {
				window = WindowCreating;
				WindowCreating = null;
				if (window.Handle == IntPtr.Zero)
					window.AssignHandle (hWnd);
			}
			return window;
		}
		#endregion	// Protected Instance Methods
	}
}
