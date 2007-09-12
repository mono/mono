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
// Copyright (c) 2007 Novell, Inc.
//
// Authors:
//	Andreia Gaita (avidigal@novell.com)
//

using System;
using System.Text;
using System.Collections;
using System.Runtime.InteropServices;
using System.Diagnostics;
using Mono.WebBrowser;

namespace Mono.Mozilla
{
	internal class Base
	{
		private static Hashtable boundControls;
		private class BindingInfo
		{
			public CallbackBinder callback;
			public IntPtr xulbrowser;
		}

		static Base ()
		{
			boundControls = new Hashtable ();
		}

		public Base () { }

		public static void DebugStartup ()
		{
			xulbrowser_debug_startup ();
			Trace.Listeners.Add (new TextWriterTraceListener (@"log"));
			Trace.AutoFlush = true;
		}

		public static void Init (Mono.WebBrowser.WebBrowser control)
		{
			BindingInfo info = new BindingInfo ();
			info.callback = new CallbackBinder (control);
			IntPtr ptrCallback = Marshal.AllocHGlobal (Marshal.SizeOf (info.callback));
			Marshal.StructureToPtr (info.callback, ptrCallback, true);
			info.xulbrowser = xulbrowser_init (ptrCallback, Environment.CurrentDirectory);
			boundControls.Add (control as IWebBrowser, info);
			DebugStartup ();
		}

		public static void Shutdown (Mono.WebBrowser.WebBrowser control)
		{
			if (!boundControls.ContainsKey (control))
				throw new ArgumentException ();
			BindingInfo info = boundControls[control] as BindingInfo;

			xulbrowser_shutdown (info.xulbrowser);
		}

		public static void Bind (IWebBrowser control, IntPtr handle, int width, int height)
		{
			if (!boundControls.ContainsKey (control))
				throw new ArgumentException ();
			BindingInfo info = boundControls[control] as BindingInfo;

			xulbrowser_createBrowserWindow (info.xulbrowser, handle, width, height);
		}

		// layout
		public static void Focus (IWebBrowser control)
		{
			if (!boundControls.ContainsKey (control))
				throw new ArgumentException ();
			BindingInfo info = boundControls[control] as BindingInfo;

			xulbrowser_focus (info.xulbrowser);
		}

		public static void Blur (IWebBrowser control)
		{
			if (!boundControls.ContainsKey (control))
				throw new ArgumentException ();
			BindingInfo info = boundControls[control] as BindingInfo;

			xulbrowser_blur (info.xulbrowser);
		}

		public static void Activate (IWebBrowser control)
		{
			if (!boundControls.ContainsKey (control))
				throw new ArgumentException ();
			BindingInfo info = boundControls[control] as BindingInfo;

			xulbrowser_activate (info.xulbrowser);
		}

		public static void Deactivate (IWebBrowser control)
		{
			if (!boundControls.ContainsKey (control))
				throw new ArgumentException ();
			BindingInfo info = boundControls[control] as BindingInfo;

			xulbrowser_deactivate (info.xulbrowser);
		}

		public static void Resize (IWebBrowser control, int width, int height)
		{
			if (!boundControls.ContainsKey (control))
				throw new ArgumentException ();
			BindingInfo info = boundControls[control] as BindingInfo;

			xulbrowser_resize (info.xulbrowser, width, height);
		}

		// navigation
		public static void Navigate (IWebBrowser control, string uri)
		{
			if (!boundControls.ContainsKey (control))
				throw new ArgumentException ();
			BindingInfo info = boundControls[control] as BindingInfo;

			xulbrowser_navigate (info.xulbrowser, uri);
		}


		public static bool Forward (IWebBrowser control)
		{
			if (!boundControls.ContainsKey (control))
				throw new ArgumentException ();
			BindingInfo info = boundControls[control] as BindingInfo;

			return xulbrowser_forward (info.xulbrowser);
		}

		public static bool Back (IWebBrowser control)
		{
			if (!boundControls.ContainsKey (control))
				throw new ArgumentException ();
			BindingInfo info = boundControls[control] as BindingInfo;

			return xulbrowser_back (info.xulbrowser);
		}

		public static void Home (IWebBrowser control)
		{
			if (!boundControls.ContainsKey (control))
				throw new ArgumentException ();
			BindingInfo info = boundControls[control] as BindingInfo;

			xulbrowser_home (info.xulbrowser);
		}

		public static void Stop (IWebBrowser control)
		{
			if (!boundControls.ContainsKey (control))
				throw new ArgumentException ();
			BindingInfo info = boundControls[control] as BindingInfo;

			xulbrowser_stop (info.xulbrowser);
		}

		public static void Reload (IWebBrowser control, ReloadOption option)
		{
			if (!boundControls.ContainsKey (control))
				throw new ArgumentException ();
			BindingInfo info = boundControls[control] as BindingInfo;

			xulbrowser_reload (info.xulbrowser, option);
		}


		#region pinvokes
		[DllImport("xulbrowser")]
		private static extern void xulbrowser_debug_startup();

		[DllImport("xulbrowser")]
		private static extern IntPtr xulbrowser_init (IntPtr events, string startDir);

		[DllImport ("xulbrowser")]
		private static extern IntPtr xulbrowser_shutdown (IntPtr instance);

		[DllImport ("xulbrowser")]
		private static extern int xulbrowser_createBrowserWindow (IntPtr instance, IntPtr hwnd, Int32 width, Int32 height);

		// layout
		[DllImport ("xulbrowser")]
		private static extern int xulbrowser_focus (IntPtr instance);
		[DllImport ("xulbrowser")]
		private static extern int xulbrowser_blur (IntPtr instance);
		[DllImport ("xulbrowser")]
		private static extern int xulbrowser_activate (IntPtr instance);
		[DllImport ("xulbrowser")]
		private static extern int xulbrowser_deactivate (IntPtr instance);
		[DllImport ("xulbrowser")]
		private static extern int xulbrowser_resize (IntPtr instance, Int32 width, Int32 height);

		// navigation
		[DllImport("xulbrowser")]
		private static extern int xulbrowser_navigate (IntPtr instance, string uri);
		[DllImport ("xulbrowser")]
		private static extern bool xulbrowser_forward (IntPtr instance);
		[DllImport ("xulbrowser")]
		private static extern bool xulbrowser_back (IntPtr instance);
		[DllImport ("xulbrowser")]
		private static extern int xulbrowser_home (IntPtr instance);
		[DllImport ("xulbrowser")]
		private static extern int xulbrowser_stop (IntPtr instance);
		[DllImport ("xulbrowser")]
		private static extern int xulbrowser_reload (IntPtr instance, ReloadOption option);
		#endregion
	}
}
