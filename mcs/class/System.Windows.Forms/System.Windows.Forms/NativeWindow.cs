//
// System.Windows.Forms.NativeWindow.cs
//
// Author:
//   stubbed out by Paul Osman (paul.osman@sympatico.ca)
//   Dennis Hayes (dennish@Raytek.com)
//   WINELib implementation started by John Sohn (jsohn@columbus.rr.com)
//
// (C) 2002/3 Ximian, Inc
//

using System.Runtime.Remoting;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.Collections;

namespace System.Windows.Forms {

	// <summary>
	// </summary>

	public class NativeWindow : MarshalByRefObject {

		// the window's HWND
		private IntPtr windowHandle;
		static private Hashtable windowCollection = new Hashtable ();
		static bool registeredClass = false;
		
		// Important!  If this variable was initialized and supplied to Windows API,
		// we cannot *free* (GC) a delegate until all our windows destroyed, or better 
		// keep it forever.
		static Win32.WndProc wp = null;

		//
		//  --- Constructor
		//
		public NativeWindow () 
		{
			windowHandle = (IntPtr) 0;
			// Important! Do not reinitialize wp, because this will *free* (GC) 
			// WindowProc delegate on every Control creation, but this delegate could
			// already be passed to RegisterClass for the Form and others windows.
			// We will get problems in Get/Translate/Dispatch Message loop
			// (like call to invalid address)
			// wp = null;
		}

		//
		//  --- Public Properties
		//
		public IntPtr Handle 
		{
			get {
				return windowHandle;
			}
		}

		//
		//  --- Public Methods
		//
		public void AssignHandle (IntPtr handle) 
		{
			if (windowHandle != (IntPtr) 0)
				windowCollection.Remove (windowHandle);

			windowHandle = handle;
			windowCollection.Add (windowHandle, this);
			OnHandleChange ();
		}

		public virtual void CreateHandle (CreateParams cp) 
		{
			if( cp != null ) {
				IntPtr createdHWnd = (IntPtr) 0;

				if (!registeredClass) {
					WNDCLASS wndClass = new WNDCLASS();

					wndClass.style = (int) (CS_.CS_OWNDC /*|
						CS_.CS_VREDRAW |
						CS_.CS_HREDRAW*/);
					wndClass.lpfnWndProc = GetWindowProc();
					wndClass.cbClsExtra = 0;
					wndClass.cbWndExtra = 0;
					wndClass.hInstance = (IntPtr)0;
					wndClass.hIcon = (IntPtr)0;
					wndClass.hCursor = Win32.LoadCursor( (IntPtr)0, LC_.IDC_ARROW);
					wndClass.hbrBackground = (IntPtr)((int)GetSysColorIndex.COLOR_BTNFACE + 1);
					wndClass.lpszMenuName = "";
					wndClass.lpszClassName = "mono_native_window";

					if (Win32.RegisterClass(ref wndClass) != 0) {
						registeredClass = true;
					} else {
						windowHandle = (IntPtr)0;
						return;
					}
				}

				IntPtr lParam = IntPtr.Zero;
				
				if ( cp.Param != null && cp.Param is CLIENTCREATESTRUCT ) {
					lParam = Marshal.AllocHGlobal ( Marshal.SizeOf ( cp.Param ) );
					Marshal.StructureToPtr ( cp.Param, lParam, false );
				}
				
				windowHandle = Win32.CreateWindowEx (
					(uint) cp.ExStyle, cp.ClassName,
					cp.Caption,(uint) cp.Style,
					cp.X, cp.Y, cp.Width, cp.Height,
					(IntPtr) cp.Parent, (IntPtr) 0,
					(IntPtr) 0, lParam);
					
				if ( lParam != IntPtr.Zero )
					Marshal.FreeHGlobal ( lParam );

				if (windowHandle != (IntPtr) 0) {
					windowCollection.Add (windowHandle, this);
					if( (cp.Style & (int)WindowStyles.WS_CHILD) != 0) {
						IntPtr curId = Win32.GetWindowLong( windowHandle, GetWindowLongFlag.GWL_ID);
						if( curId == IntPtr.Zero)
							Win32.SetWindowLong(windowHandle, GetWindowLongFlag.GWL_ID, (int)windowHandle);
					}
				}
				//debug
				else {
					System.Console.WriteLine("Cannot create window {0}", Win32.FormatMessage(Win32.GetLastError()));
				}
			}
		}

		public void DefWndProc (ref Message m) 
		{
			m.Result = Win32.DefWindowProcA (m.HWnd, m.Msg, 
							 m.WParam, m.LParam);
		}

		internal void DefMDIChildProc ( ref Message m ) {
			m.Result = Win32.DefMDIChildProc(m.HWnd, m.Msg, m.WParam, m.LParam);
		}

		internal void DefFrameProc ( ref Message m , Control MdiClient) {
			m.Result = Win32.DefFrameProc(m.HWnd, MdiClient != null ? MdiClient.Handle : IntPtr.Zero, 
							m.Msg, m.WParam, m.LParam);
		}

		public virtual void DestroyHandle () 
		{
			windowCollection.Remove (windowHandle);
			Win32.DestroyWindow (windowHandle);
			windowHandle = (IntPtr)0;
		}

		public static NativeWindow FromHandle (IntPtr handle) 
		{
			NativeWindow window = new NativeWindow ();
			window.AssignHandle (handle);
			return window;
		}

		public virtual void ReleaseHandle () 
		{
			windowHandle = (IntPtr) 0;
			OnHandleChange ();
		}

		//
		//  --- Protected Methods
		//


		[MonoTODO]
		protected virtual void OnHandleChange () 
		{
			// to be overridden
		}

		[MonoTODO]
		protected virtual void OnThreadException (Exception e) 
		{
			Application.OnThreadException(e);
			//Console.WriteLine(e.Message + "\n" + ex.StackTrace);
		}

		protected virtual void WndProc (ref Message m) 
		{
			if (m.Msg == Msg.WM_CREATE)
				Console.WriteLine ("NW WndProc WM_CREATE");
			DefWndProc (ref m);
		}

		//
		//  --- Destructor
		//
		~NativeWindow ()
		{
			if ( windowHandle != IntPtr.Zero )
				DestroyHandle ( );
		}

 		static private IntPtr WndProc (
			IntPtr hWnd, Msg msg, IntPtr wParam, IntPtr lParam) 
		{
//			Console.WriteLine("NativeWindow.Message {0}", msg);
 			Message message = new Message ();
 			NativeWindow window = null;
			// CHECKME: This try/catch is implemented to keep Message Handlers "Exception safe"
			try {
				// windowCollection is a collection of all the 
				// NativeWindow(s) that have been created.
				// Dispatch the current message to the approriate
				// window.
				window = (NativeWindow) windowCollection[hWnd];
				message.HWnd = hWnd;
				message.Msg = msg;
				message.WParam = wParam;
				message.LParam = lParam;
	 			message.Result = (IntPtr) 0;

#if false
				if (msg == Msg.WM_CREATE)
					Console.WriteLine ("WM_CREATE (static)");
#endif
				
	 			if (window != null) {
					if (msg == Msg.WM_CREATE) {
						// Console.WriteLine ("WM_CREATE (static != null)");
					}
	 				window.WndProc(ref message);
	 			} else {
					// Console.WriteLine ("no window, defwndproc");
	 				// even though we are not managing the
	 				// window let the window get the message
	 				message.Result = Win32.DefWindowProcA (
						hWnd, msg, wParam, lParam);
	 			}
			}
			catch( System.Exception ex) {
				if( window != null)
					window.OnThreadException(ex);
			}
 			return message.Result;
 		}
 		
		internal static Win32.WndProc GetWindowProc() {
			if( wp == null){
				wp = new Win32.WndProc (WndProc);
			}
			return wp;
		}
	}
}
