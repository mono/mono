//
// System.Windows.Forms.Application
//
// Author:
//   stubbed out by Jaak Simm (jaaksimm@firm.ee)
//   Miguel de Icaza (miguel@ximian.com)
//	Dennis hayes (dennish@raytek.com)
//   WINELib implementation started by John Sohn (jsohn@columbus.rr.com)
//
// (C) Ximian, Inc 2002
//

using System;
using System.Drawing;
using Microsoft.Win32;
using System.ComponentModel;
using System.Threading;
using System.Globalization;
using System.Reflection;
using System.Collections;
using System.Runtime.CompilerServices;

namespace System.Windows.Forms {

	/// <summary>
	/// Provides static methods and properties to manage an application, 
	/// such as methods to start and stop an application, to process 
	/// Windows messages, and properties to get information about an 
	/// application. This class cannot be inherited.
	/// </summary>

	[MonoTODO]
	public sealed class Application {

		//[MethodImplAttribute(MethodImplOptions.InternalCall)]
		//extern static int GetInstance();

		static private Form applicationForm;

		// --- (public) Properties ---
		[MonoTODO]
		public static bool AllowQuit {
			get { throw new NotImplementedException (); }
		}
	
		[MonoTODO]
		public static string CommonAppDataPath {
			get { throw new NotImplementedException (); }
		}
	
		[MonoTODO]
		// Registry key not yet defined (this should be interesting)
		//public static RegistryKey CommonAppDataRegistry {
		//	get { throw new NotImplementedException (); }
		//}
	
		[MonoTODO]
		public static string CompanyName {
			get { throw new NotImplementedException (); }
		}
	
		[MonoTODO]
		public static CultureInfo CurrentCulture {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}
	
		[MonoTODO]
		public static InputLanguage CurrentInputLanguage {
			get { throw new NotImplementedException (); }
		}
	
		[MonoTODO]
		public static string ExecutablePath {
			get { throw new NotImplementedException (); }
		}
	
		[MonoTODO]
		public static string LocalUserAppDataPath {
			get { throw new NotImplementedException (); }
		}
	
		[MonoTODO]
		public static bool MessageLoop {
			get { throw new NotImplementedException (); }
		}
	
		[MonoTODO]
		public static string ProductName {
			get { throw new NotImplementedException (); }
		}
	
		[MonoTODO]
		public static string ProductVersion {
			get { throw new NotImplementedException (); }
		}
	
		[MonoTODO]
		public static string SafeTopLevelCaptionFormat {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}
	
		[MonoTODO]
		public static string StartupPath {
			get { throw new NotImplementedException (); }
		}
	
		[MonoTODO]
		public static string UserAppDataPath {
			get { throw new NotImplementedException (); }
		}
	
		[MonoTODO]
		// Registry key not yet defined
		//public static RegistryKey UserAppDataRegistry {
		//	get { throw new NotImplementedException (); }
		//}
		

		private static ArrayList messageFilters = new ArrayList ();
	
		// --- Methods ---
		[MonoTODO]
		public static void AddMessageFilter (IMessageFilter value) 
		{
			messageFilters.Add (value);
		}
	
		[MonoTODO]
		public static void DoEvents () 
		{
			throw new NotImplementedException ();
		}
	
		[MonoTODO]
		public static void Exit () 
		{
			throw new NotImplementedException ();
		}
	
		[MonoTODO]
		public static void ExitThread () 
		{
			throw new NotImplementedException ();
		}
	
		[MonoTODO]
		public static ApartmentState OleRequired () 
		{
			throw new NotImplementedException ();
		}
	
		[MonoTODO]
		public static void OnThreadException (Exception t) 
		{
			throw new NotImplementedException ();
		}
	
		[MonoTODO]
		public static void RemoveMessageFilter (IMessageFilter value)
		{
			messageFilters.Remove (value);
		}

		public static void Run ()
		{

		}

		[MonoTODO]
		public static void Run (ApplicationContext context) 
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		//[TypeAttributes.BeforeFieldInit]
		public static void Run (Form context)
		// Documents say this parameter name should be mainform, 
		// but the verifier says context.
		{
			applicationForm = context;
			int msg = 0;
			context.Show ();

			while (Win32.GetMessageA (ref msg, 0, 0, 0) != 0) {
				Win32.TranslateMessage (ref msg);
				Win32.DispatchMessageA (ref msg);
			}
		}
		
		// --- Events ---
//		public static event EventHandler ApplicationExit;		
		public static event EventHandler Idle;
		public static event ThreadExceptionEventHandler ThreadException;
		public static event EventHandler ThreadExit;



		// The WndProc is initialized in the monostub and calls the 
		// WndProc defined here
		static private IntPtr _ApplicationWndProc (IntPtr hWnd, 
							   int msg, 
							   IntPtr wParam, 
							   IntPtr lParam)  {

 		        Console.WriteLine ("in _ApplicationWndProc");
 			Message message = new Message ();

 			message.Result = (IntPtr) 0;
			message.HWnd = hWnd;
			message.Msg = msg;
			message.WParam = wParam;
			message.LParam = lParam;

			IEnumerator e = messageFilters.GetEnumerator ();
			
			while (e.MoveNext()) {
				IMessageFilter filter = 
				    (IMessageFilter) e.Current;
				if (filter.PreFilterMessage (ref message))
					return message.Result;
			}

			//if (applicationForm != null)
			//	applicationForm.WndProc (ref message);

			return message.Result;
		}
	}
}
