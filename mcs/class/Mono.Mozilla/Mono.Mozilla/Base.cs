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
		internal static bool gluezillaInstalled;

		private class BindingInfo
		{
			public CallbackBinder callback;
			public IntPtr gluezilla;
		}

		private static bool isInitialized ()
		{
			if (!gluezillaInstalled)
				return false;
			return true;
		}

		private static BindingInfo getBinding (IWebBrowser control)
		{
			if (!boundControls.ContainsKey (control))
				return null;
			BindingInfo info = boundControls[control] as BindingInfo;
			return info;
		}

		static Base ()
		{
			boundControls = new Hashtable ();
		}

		public Base () { }

		public static void DebugStartup ()
		{
			gluezilla_debug_startup ();
			Trace.Listeners.Add (new TextWriterTraceListener (@"log"));
			Trace.AutoFlush = true;
		}

		public static bool Init (WebBrowser control)
		{
			BindingInfo info = new BindingInfo ();
			info.callback = new CallbackBinder (control);
			IntPtr ptrCallback = Marshal.AllocHGlobal (Marshal.SizeOf (info.callback));
			Marshal.StructureToPtr (info.callback, ptrCallback, true);

			string monoMozDir = System.IO.Path.Combine (
				System.IO.Path.Combine (
				Environment.GetFolderPath (Environment.SpecialFolder.LocalApplicationData),
				".mono"), "mozilla");

			if (!System.IO.Directory.Exists (monoMozDir))
				System.IO.Directory.CreateDirectory (monoMozDir);

			try {
				info.gluezilla = gluezilla_init (ptrCallback, Environment.CurrentDirectory, monoMozDir);
			}
			catch (DllNotFoundException) {
				Console.WriteLine ("libgluezilla not found. To have webbrowser support, you need libgluezilla installed");
				Marshal.FreeHGlobal (ptrCallback);
				gluezillaInstalled = false;
				return false;
			}
			gluezillaInstalled = true;
			boundControls.Add (control as IWebBrowser, info);
			DebugStartup ();
			return true;
		}

		public static void Shutdown (IWebBrowser control)
		{
			if (!isInitialized ())
				return;
			BindingInfo info = getBinding (control);

			gluezilla_shutdown (info.gluezilla);
		}

		public static void Bind (IWebBrowser control, IntPtr handle, int width, int height)
		{
			if (!isInitialized ())
				return;
			BindingInfo info = getBinding (control);

			gluezilla_createBrowserWindow (info.gluezilla, handle, width, height);
		}

		// layout
		public static void Focus (IWebBrowser control, FocusOption focus)
		{
			if (!isInitialized ())
				return;
			BindingInfo info = getBinding (control);

			gluezilla_focus (info.gluezilla, focus);
		}


		public static void Blur (IWebBrowser control)
		{
			if (!isInitialized ())
				return;
			BindingInfo info = getBinding (control);

			gluezilla_blur (info.gluezilla);
		}

		public static void Activate (IWebBrowser control)
		{
			if (!isInitialized ())
				return;
			BindingInfo info = getBinding (control);

			gluezilla_activate (info.gluezilla);
		}

		public static void Deactivate (IWebBrowser control)
		{
			if (!isInitialized ())
				return;
			BindingInfo info = getBinding (control);

			gluezilla_deactivate (info.gluezilla);
		}

		public static void Resize (IWebBrowser control, int width, int height)
		{
			if (!isInitialized ())
				return;
			BindingInfo info = getBinding (control);

			gluezilla_resize (info.gluezilla, width, height);
		}

		// navigation
		public static void Navigate (IWebBrowser control, string uri)
		{
			if (!isInitialized ())
				return;
			BindingInfo info = getBinding (control);

			gluezilla_navigate (info.gluezilla, uri);
		}


		public static bool Forward (IWebBrowser control)
		{
			if (!isInitialized ())
				return false;
			BindingInfo info = getBinding (control);

			return gluezilla_forward (info.gluezilla);
		}

		public static bool Back (IWebBrowser control)
		{
			if (!isInitialized ())
				return false;
			BindingInfo info = getBinding (control);

			return gluezilla_back (info.gluezilla);
		}

		public static void Home (IWebBrowser control)
		{
			if (!isInitialized ())
				return;
			BindingInfo info = getBinding (control);

			gluezilla_home (info.gluezilla);
		}

		public static void Stop (IWebBrowser control)
		{
			if (!isInitialized ())
				return;
			BindingInfo info = getBinding (control);

			gluezilla_stop (info.gluezilla);
		}

		public static void Reload (IWebBrowser control, ReloadOption option)
		{
			if (!isInitialized ())
				return;
			BindingInfo info = getBinding (control);

			gluezilla_reload (info.gluezilla, option);
		}

		public static nsIDOMHTMLDocument GetDOMDocument (IWebBrowser control)
		{
			if (!isInitialized ())
				return null;
			BindingInfo info = getBinding (control);

			return gluezilla_getDomDocument (info.gluezilla);
		}

		public static nsIWebNavigation GetWebNavigation (IWebBrowser control)
		{
			if (!isInitialized ())
				return null;
			BindingInfo info = getBinding (control);

			return gluezilla_getWebNavigation (info.gluezilla);
		}

		public static IntPtr StringInit ()
		{
			return gluezilla_stringInit ();
		}

		public static void StringFinish (HandleRef str)
		{
			gluezilla_stringFinish (str);
		}

		public static string StringGet (HandleRef str)
		{
			IntPtr p = gluezilla_stringGet (str);
			return Marshal.PtrToStringUni (p);
		}

		public static void StringSet (HandleRef str, string text)
		{
			gluezilla_stringSet (str, text);
		}
/*		
		public static nsIServiceManager GetServiceManager (IWebBrowser control)
		{
			if (!isInitialized ())
				return null;
			BindingInfo info = getBinding (control);

			return gluezilla_getServiceManager (info.gluezilla);
		}
*/
		#region pinvokes
		[DllImport("gluezilla")]
		private static extern void gluezilla_debug_startup();

		[DllImport("gluezilla")]
		private static extern IntPtr gluezilla_init (IntPtr events, string startDir, string dataDir);

		[DllImport ("gluezilla")]
		private static extern IntPtr gluezilla_shutdown (IntPtr instance);

		[DllImport ("gluezilla")]
		private static extern int gluezilla_createBrowserWindow (IntPtr instance, IntPtr hwnd, Int32 width, Int32 height);

		// layout
		[DllImport ("gluezilla")]
		private static extern int gluezilla_focus (IntPtr instance, FocusOption focus);
		[DllImport ("gluezilla")]
		private static extern int gluezilla_blur (IntPtr instance);
		[DllImport ("gluezilla")]
		private static extern int gluezilla_activate (IntPtr instance);
		[DllImport ("gluezilla")]
		private static extern int gluezilla_deactivate (IntPtr instance);
		[DllImport ("gluezilla")]
		private static extern int gluezilla_resize (IntPtr instance, Int32 width, Int32 height);

		// navigation
		[DllImport("gluezilla")]
		private static extern int gluezilla_navigate (IntPtr instance, string uri);
		[DllImport ("gluezilla")]
		private static extern bool gluezilla_forward (IntPtr instance);
		[DllImport ("gluezilla")]
		private static extern bool gluezilla_back (IntPtr instance);
		[DllImport ("gluezilla")]
		private static extern int gluezilla_home (IntPtr instance);
		[DllImport ("gluezilla")]
		private static extern int gluezilla_stop (IntPtr instance);
		[DllImport ("gluezilla")]
		private static extern int gluezilla_reload (IntPtr instance, ReloadOption option);

		// dom
		[DllImport ("gluezilla")]
		[return:MarshalAs(UnmanagedType.Interface)]
		private static extern nsIDOMHTMLDocument gluezilla_getDomDocument (IntPtr instance);
		[DllImport ("gluezilla")]
		[return:MarshalAs(UnmanagedType.Interface)]
		private static extern nsIWebNavigation gluezilla_getWebNavigation (IntPtr instance);

		[DllImport ("gluezilla")]
		private static extern IntPtr gluezilla_stringInit ();
		[DllImport ("gluezilla")]
		private static extern int gluezilla_stringFinish (HandleRef str);
		[DllImport ("gluezilla")]
		private static extern IntPtr gluezilla_stringGet (HandleRef str);
		[DllImport ("gluezilla")]
		private static extern void gluezilla_stringSet (HandleRef str, [MarshalAs (UnmanagedType.LPWStr)] string text);


//		[DllImport ("gluezilla")]
//		private static extern nsIServiceManager gluezilla_getServiceManager (IntPtr instance);
		#endregion
	}
}
