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
// $Revision: 1.8 $
// $Modtime: $
// $Log: Application.cs,v $
// Revision 1.8  2004/10/20 03:33:33  pbartok
// - Fixed to deal with new Form subclasses for menus
//
// Revision 1.7  2004/10/18 04:14:14  pbartok
// - Added code to simulate modal dialogs on Win32
//
// Revision 1.6  2004/09/22 20:05:41  pbartok
// - Added message loop for modal dialogs
//
// Revision 1.5  2004/09/21 00:54:15  jackson
// New message loop that uses poll so we don't get a busy loop
//
// Revision 1.4  2004/08/23 22:45:19  pbartok
// - Removed debug output
// - Simplifications
//
// Revision 1.3  2004/08/23 22:09:29  pbartok
// - Added handling of Idle event
// - Added handling of form closing
// - Fixed reporting of MessageLoop property
// - Removed some unneeded code, should provide a bit of a speedup
//
// Revision 1.2  2004/08/11 22:16:50  pbartok
// - Fixed Signature
// - Added .Net 1.1 method
//
// Revision 1.1  2004/07/09 05:21:25  pbartok
// - Initial check-in
//
//

// COMPLETE

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
		private static bool			browser_embedded;
		private static bool			exiting;
		private static InputLanguage		input_language;
		private static bool			messageloop_started;
		private static string			safe_caption_format;
		private static ArrayList		message_filters;
		private static ApplicationContext	app_context;
		private static Form			main_form;

		private Application () {
			input_language	= InputLanguage.CurrentInputLanguage;
			message_filters	= new ArrayList();
			app_context	= null;
			browser_embedded= false;
			exiting		= false;
			messageloop_started = false;
			safe_caption_format = "{1} - {0} - {2}";
		}

		#region Private and Internal Methods
		internal static void ModalRun(Form form) {
			MSG	msg = new MSG();
			Queue	toplevels = new Queue();
			IEnumerator control = Control.controls.GetEnumerator();

			if (form == null) {
				return;
			}

			while (control.MoveNext()) {
				if ((((Control)control.Current).parent == null) && (((Control)control.Current).is_visible) && (((Control)control.Current).is_enabled)) {
					if ((control.Current is Form.FormParentWindow)  && (((Form.FormParentWindow)control.Current)!=form.form_parent_window)) {
						XplatUI.EnableWindow(((Control)control.Current).window.Handle, false);
						toplevels.Enqueue((Control)control.Current);
					}
				}
			}

			while (!exiting && !form.end_modal && XplatUI.GetMessage(ref msg, IntPtr.Zero, 0, 0)) {
				XplatUI.TranslateMessage(ref msg);
				XplatUI.DispatchMessage(ref msg);

				// Handle exit, Form might have received WM_CLOSE and set 'closing' in response
				if (form.closing) {
					form.end_modal = true;
				}
			}

			while (toplevels.Count>0) {
				XplatUI.EnableWindow(((Control)toplevels.Dequeue()).window.Handle, true);
			}
		}
		#endregion	// Private and Internal Methods

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
				StackTrace	st;

				if (Environment.OSVersion.Platform != (PlatformID)128) {
					RegistryKey	key;
					String		ret;

					key=Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion", false);
					ret=(String)key.GetValue("RegisteredOrganization");

					return ret;
					
				}

				st=new StackTrace();
				return st.GetFrame(st.FrameCount-1).GetMethod().DeclaringType.Namespace;
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
				return Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
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
				return Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
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

		public static void Exit() {
			XplatUI.Exit();
		}

		public static void ExitThread() {
			exiting=true;
		}

		private static void InternalExit(object sender, EventArgs e) {
			Application.Exit();
		}

		public static ApartmentState OleRequired() {
			//throw new NotImplementedException("OLE Not supported by this System.Windows.Forms implementation");
			return ApartmentState.Unknown;
		}

		public static void OnThreadException(Exception t) {
			if (Application.ThreadException != null) {
				Application.ThreadException(null, new ThreadExceptionEventArgs(t));
			} else {
				XplatUI.HandleException(t);
			}
		}

		public static void RemoveMessageFilter(IMessageFilter filter) {
			message_filters.Remove(filter);
		}

		public static void Run() {
			MSG	msg = new MSG();
			Form	form = null;

			if (app_context != null) {
				form = app_context.MainForm;
			}

			messageloop_started = true;

			while (!exiting && XplatUI.GetMessage(ref msg, IntPtr.Zero, 0, 0)) {
				XplatUI.TranslateMessage(ref msg);
				XplatUI.DispatchMessage(ref msg);

				// Handle exit, Form might have received WM_CLOSE and set 'closing' in response
				if ((form != null) && form.closing) {
					exiting = true;
				}
			}

			messageloop_started = false;

			if (ApplicationExit != null) {
				ApplicationExit(null, EventArgs.Empty);
			}
		}

		public static void Run(Form mainForm) {
			mainForm.CreateControl();
			Run(new ApplicationContext(mainForm));
		}

		public static void Run(ApplicationContext context) {
			app_context=context;
			if (app_context.MainForm!=null) {
				app_context.MainForm.Show();
				app_context.ThreadExit += new EventHandler(InternalExit);
			}
			Run();
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
