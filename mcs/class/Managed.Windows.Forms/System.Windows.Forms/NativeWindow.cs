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
//	Peter Dennis Bartok	pbartok@novell.com
//


// COMPLETE

//#define ExternalExceptionHandler

using System.Runtime.Remoting;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.Collections;
using System.Diagnostics;

namespace System.Windows.Forms
{
	public class NativeWindow : MarshalByRefObject {
		internal IntPtr			window_handle;
		static internal Hashtable	window_collection = new Hashtable();

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
			NativeWindow window=new NativeWindow();

			window.AssignHandle(handle);
			return window;
		}
		#endregion	// Public Static Methods

		#region Private and Internal Methods
		internal static NativeWindow FindWindow(IntPtr handle)
		{
			NativeWindow rv;
			lock (window_collection) {
				rv = (NativeWindow)window_collection[handle];
			}
			return rv;
		}

		internal void InvalidateHandle()
		{
			lock (window_collection) {
				window_collection.Remove(window_handle);
			}
			window_handle = IntPtr.Zero;
		}
		#endregion

		#region Public Instance Methods
		public void AssignHandle(IntPtr handle)
		{
			lock (window_collection) {
				if (window_handle != IntPtr.Zero) {
					window_collection.Remove(window_handle);
				}
				window_handle=handle;
				window_collection.Add(window_handle, this);
			}
			OnHandleChange();
		}

		public virtual void CreateHandle(CreateParams create_params)
		{
			if (create_params != null) {
				window_handle=XplatUI.CreateWindow(create_params);

				if (window_handle != IntPtr.Zero) {
					lock (window_collection) {
						window_collection.Add(window_handle, this);
					}
				}
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
			lock (window_collection) {
				window_collection.Remove(window_handle);
			}
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
			Message		m = new Message();
			NativeWindow	window = null;

			try {
				lock (window_collection) {
					window = (NativeWindow)window_collection[hWnd];
				}
				m.HWnd=hWnd;
				m.Msg=(int)msg;
				m.WParam=wParam;
				m.LParam=lParam;
				m.Result=IntPtr.Zero;

				if (window != null)
					window.WndProc(ref m);
				else
					m.Result=XplatUI.DefWndProc(ref m);
			}
			catch (Exception ex) {
#if !ExternalExceptionHandler				
				if (window != null)
					window.OnThreadException(ex);
#else
				throw;
#endif
			}

			#if debug
				Console.WriteLine("NativeWindow.cs: Message {0}, result {1}", msg, m.Result);
			#endif

			return m.Result;
		}
		#endregion	// Protected Instance Methods
	}
}
