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
// Copyright (c) 2004 - 2006 Novell, Inc.
//
// Authors:
//	Peter Bartok	pbartok@novell.com
//

// COMPLETE

#undef DebugRunLoop

using Microsoft.Win32;
using System;
using System.Drawing;
using System.ComponentModel;
using System.Collections;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;

namespace System.Windows.Forms {
	public sealed class Application {
		private static bool			browser_embedded	= false;
		private static InputLanguage		input_language		= InputLanguage.CurrentInputLanguage;
		private static bool			messageloop_started	= false;
		private static string			safe_caption_format	= "{1} - {0} - {2}";
		private static ArrayList		message_filters		= new ArrayList();

		private Application () {
		}

		#region Private Methods
		private static void CloseForms(Thread thread) {
			Control		c;
			IEnumerator	control;
			bool		all;

			#if DebugRunLoop
				Console.WriteLine("   CloseForms({0}) called", thread);
			#endif
			if (thread == null) {
				all = true;
			} else {
				all = false;
			}

			control = Control.controls.GetEnumerator();

			while (control.MoveNext()) {
				c = (Control)control.Current;
				if (c is Form) {
					if (all || (thread == c.creator_thread)) {
						if (c.IsHandleCreated) {
							XplatUI.PostMessage(c.Handle, Msg.WM_CLOSE_INTERNAL, IntPtr.Zero, IntPtr.Zero);
						}
						#if DebugRunLoop
							Console.WriteLine("      Closing form {0}", c);
						#endif
					}
				}
			}

		}
		#endregion	// Private methods

		#region Public Static Properties
		public static bool AllowQuit {
			get {
				return browser_embedded;
			}
		}

		public static string CommonAppDataPath {
			get {
				return Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
			}
		}

		public static RegistryKey CommonAppDataRegistry {
			get {
				RegistryKey	key;

				key = Registry.LocalMachine.OpenSubKey("Software\\" + Application.CompanyName + "\\" + Application.ProductName + "\\" + Application.ProductVersion, true);

				return key;
			}
		}

		public static string CompanyName {
			get {
				AssemblyCompanyAttribute[] attrs = (AssemblyCompanyAttribute[]) Assembly.GetEntryAssembly().GetCustomAttributes(typeof(AssemblyCompanyAttribute), true);
				
				if ((attrs != null) && attrs.Length>0) {
					return attrs[0].Company;
				}

				return Assembly.GetEntryAssembly().GetName().Name;
			}
		}

		public static CultureInfo CurrentCulture {
			get {
				return Thread.CurrentThread.CurrentUICulture;
			}

			set {
				
				Thread.CurrentThread.CurrentUICulture=value;
			}
		}

		public static InputLanguage CurrentInputLanguage {
			get {
				return input_language;
			}

			set {
				input_language=value;
			}
		}

		public static string ExecutablePath {
			get {
				return Assembly.GetEntryAssembly().Location;
			}
		}

		public static string LocalUserAppDataPath {
			get {
				return Path.Combine(Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), CompanyName), ProductName), ProductVersion);
			}
		}

		public static bool MessageLoop {
			get {
				return messageloop_started;
			}
		}

		public static string ProductName {
			get {
				AssemblyProductAttribute[] attrs = (AssemblyProductAttribute[]) Assembly.GetEntryAssembly().GetCustomAttributes(typeof(AssemblyProductAttribute), true);
				
				if ((attrs != null) && attrs.Length>0) {
					return attrs[0].Product;
				}

				return Assembly.GetEntryAssembly().GetName().Name;
			}
		}

		public static string ProductVersion {
			get {
				String version;

				version = Assembly.GetEntryAssembly().GetName().Version.ToString();

				if (version.StartsWith("0.")) {
					version="1." + version.Substring(2);
				}
				return version;
			}
		}

		public static string SafeTopLevelCaptionFormat {
			get {
				return safe_caption_format;
			}

			set {
				safe_caption_format=value;
			}
		}

		public static string StartupPath {
			get {
				return Path.GetDirectoryName(Application.ExecutablePath);
			}
		}

		public static string UserAppDataPath {
			get {
				return Path.Combine(Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), CompanyName), ProductName), ProductVersion);
			}
		}

		public static RegistryKey UserAppDataRegistry {
			get {
				RegistryKey	key;

				key = Registry.CurrentUser.OpenSubKey("Software\\" + Application.CompanyName + "\\" + Application.ProductName + "\\" + Application.ProductVersion, true);

				return key;
			}
		}
		#endregion

		#region Public Static Methods
		public static void AddMessageFilter(IMessageFilter value) {
			message_filters.Add(value);
		}

		public static void DoEvents() {
			XplatUI.DoEvents();
		}

		public static void EnableVisualStyles() {
			XplatUI.EnableThemes();
		}

#if NET_2_0
		public static void EnableRTLMirroring () 
		{
		}

		//
		// If true, it uses GDI+, performance reasons were quoted
		//
		static internal bool use_compatible_text_rendering = true;
		
		public static void SetCompatibleTextRenderingDefault (bool defaultValue)
		{
			use_compatible_text_rendering = defaultValue;
		}
#endif

		public static void Exit() {
			CloseForms(null);

			// FIXME - this needs to be fired when they're all closed
			// But CloseForms uses PostMessage, so it gets fired before
			// We need to wait on something...
			if (ApplicationExit != null) {
				ApplicationExit(null, EventArgs.Empty);
			}
		}

		public static void ExitThread() {
			CloseForms(Thread.CurrentThread);
		}

		public static ApartmentState OleRequired() {
			//throw new NotImplementedException("OLE Not supported by this System.Windows.Forms implementation");
			return ApartmentState.Unknown;
		}

		public static void OnThreadException(Exception t) {
			if (Application.ThreadException != null) {
				Application.ThreadException(null, new ThreadExceptionEventArgs(t));
				return;
			}

			if (SystemInformation.UserInteractive) {
				Form form = new ThreadExceptionDialog (t);
				form.ShowDialog ();
			} else {
				Console.WriteLine (t.ToString ());
			}
		}

		public static void RemoveMessageFilter(IMessageFilter filter) {
			message_filters.Remove(filter);
		}

		public static void Run() {
			RunLoop(false, new ApplicationContext());
		}

		public static void Run(Form mainForm) {
			RunLoop(false, new ApplicationContext(mainForm));
		}

		public static void Run(ApplicationContext context) {
			RunLoop(false, context);
		}

		internal static void RunLoop(bool Modal, ApplicationContext context) {
			Queue		toplevels;
			IEnumerator	control;
			MSG		msg;

			msg = new MSG();

			if (context == null) {
				context = new ApplicationContext();
			}

			if (context.MainForm != null) {
				context.MainForm.Show();
				// FIXME - do we need this?
				//context.MainForm.PerformLayout();
				context.MainForm.context = context;
				context.MainForm.Activate();
				context.MainForm.closing = false;
			}

			#if DebugRunLoop
				Console.WriteLine("Entering RunLoop(Modal={0}, Form={1})", Modal, context.MainForm != null ? context.MainForm.ToString() : "NULL");
			#endif

			if (Modal) {
				Control c;

				if (context.MainForm.Modal) {
					throw new Exception("fixme");
				}
				context.MainForm.is_modal = true;

				toplevels = new Queue();
				control = Control.controls.GetEnumerator();

				while (control.MoveNext()) {

					c = (Control)control.Current;
					if (c is Form && (c != context.MainForm)) {
						if (c.IsHandleCreated && XplatUI.IsEnabled(c.Handle)) {
							#if DebugRunLoop
								Console.WriteLine("      Disabling form {0}", c);
							#endif
							XplatUI.EnableWindow(c.Handle, false);
							toplevels.Enqueue(c);
						}
					}
				}
				// FIXME - need activate?

				XplatUI.SetModal(context.MainForm.Handle, true);
			} else {
				toplevels = null;
			}

			messageloop_started = true;

			while (XplatUI.GetMessage(ref msg, IntPtr.Zero, 0, 0)) {
				if ((message_filters != null) && (message_filters.Count > 0)) {
					Message	m;
					bool	drop;

					drop = false;
					m = new Message();
					m.Msg = (int)msg.message;
					m.HWnd = msg.hwnd;
					m.LParam = msg.lParam;
					m.WParam = msg.wParam;
					for (int i = 0; i < message_filters.Count; i++) {
						if (((IMessageFilter)message_filters[i]).PreFilterMessage(ref m)) {
							// we're dropping the message
							drop = true;
							break;
						}
					}
					if (drop) {
						continue;
					}
				}

				XplatUI.TranslateMessage(ref msg);
				XplatUI.DispatchMessage(ref msg);

				// Handle exit, Form might have received WM_CLOSE and set 'closing' in response
				if ((context.MainForm != null) && context.MainForm.closing) {
					if (!Modal) {
						XplatUI.PostQuitMessage(0);
					} else {
						break;
					}
				}
			}
			#if DebugRunLoop
				Console.WriteLine("   RunLoop loop left");
			#endif

			messageloop_started = false;

			if (Modal) {
				Control c;

				context.MainForm.Hide();
				context.MainForm.is_modal = false;

				while (toplevels.Count>0) {
					#if DebugRunLoop
						Console.WriteLine("      Re-Enabling form form {0}", toplevels.Peek());
					#endif
					c = (Control)toplevels.Dequeue();
					if (c.IsHandleCreated) {
						XplatUI.EnableWindow(c.window.Handle, true);
					}
				}
				#if DebugRunLoop
					Console.WriteLine("   Done with the re-enable");
				#endif
				if (context.MainForm.IsHandleCreated) {
					XplatUI.SetModal(context.MainForm.Handle, false);
				}
				#if DebugRunLoop
					Console.WriteLine("   Done with the SetModal");
				#endif
			}

			#if DebugRunLoop
				Console.WriteLine("Leaving RunLoop(Modal={0}, Form={1})", Modal, context.MainForm != null ? context.MainForm.ToString() : "NULL");
			#endif
			if (context.MainForm != null) {
				context.MainForm.context = null;
			}

			if (!Modal) {
				if (ThreadExit != null) {
					ThreadExit(null, EventArgs.Empty);
				}

				context.ExitThread();
			}
		}

		#endregion	// Public Static Methods

		#region Events
		public static event EventHandler	ApplicationExit;

		public static event EventHandler	Idle {
			add {
				XplatUI.Idle += value;
			}
			remove {
				XplatUI.Idle -= value;
			}
		}

		public static event EventHandler	ThreadExit;
		public static event ThreadExceptionEventHandler	ThreadException;
		#endregion	// Events
	}
}
