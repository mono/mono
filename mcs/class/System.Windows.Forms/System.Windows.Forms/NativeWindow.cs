//
// System.Windows.Forms.NativeWindow.cs
//
// Author:
//   stubbed out by Paul Osman (paul.osman@sympatico.ca)
//   Dennis Hayes (dennish@Raytek.com)
//   WINELib implementation started by John Sohn (jsohn@columbus.rr.com)
//
// (C) 2002 Ximian, Inc
//

using System.Runtime.Remoting;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.Collections;

namespace System.Windows.Forms {

	// <summary>
	// Implementation started.
	//
	// </summary>

	public class NativeWindow : MarshalByRefObject {

		// the window's HWND
		private IntPtr windowHandle;
		static private Hashtable windowCollection = new Hashtable ();
		static bool registeredClass = false;

		//
		//  --- Constructor
		//
		public NativeWindow () 
		{
			windowHandle = (IntPtr) 0;
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
			IntPtr createdHWnd = (IntPtr) 0;
			Object lpParam = new Object();

			if (!registeredClass) {
				Win32.WndProc wp = new Win32.WndProc (WndProc);
				WNDCLASS wndClass = new WNDCLASS();

				wndClass.style = (int) (CS_.CS_OWNDC |
							CS_.CS_VREDRAW |
							CS_.CS_HREDRAW);
				wndClass.lpfnWndProc = wp;
				wndClass.cbClsExtra = 0;
				wndClass.cbWndExtra = 0;
				wndClass.hInstance = (IntPtr)0;
				wndClass.hIcon = (IntPtr)0;
				wndClass.hCursor = (IntPtr)0;
				wndClass.hbrBackground = (IntPtr)6;  // ???
				wndClass.lpszMenuName = "";
				wndClass.lpszClassName = "mono_native_window";

				if (Win32.RegisterClass(ref wndClass) != 0) {
					registeredClass = true;
				} else {
					windowHandle = (IntPtr)0;
					return;
				}
			}

			windowHandle = Win32.CreateWindowEx (
				(uint) cp.ExStyle, cp.ClassName,
				cp.Caption,(uint) cp.Style,
				cp.X, cp.Y, cp.Width, cp.Height,
				(IntPtr) cp.Parent, (IntPtr) 0,
				(IntPtr) 0, ref lpParam);
			
			if (windowHandle != (IntPtr) 0)
				windowCollection.Add (windowHandle, this);
			//debug
			//else {
			//	System.Console.WriteLine("Cannot create window {0}", Win32.FormatMessage(Win32.GetLastError()));
			//}
		}

		[MonoTODO]
		public override ObjRef CreateObjRef (Type requestedType) 
		{
			throw new NotImplementedException ();
		}

		public void DefWndProc (ref Message m) 
		{
			m.Result = Win32.DefWindowProcA (m.HWnd, m.Msg, 
							 m.WParam, m.LParam);
		}

		public virtual void DestroyHandle () 
		{
			windowCollection.Remove (windowHandle);
			Win32.DestroyWindow (windowHandle);
		}

		[MonoTODO]
		public override bool Equals (object o) 
		{
			throw new NotImplementedException ();
		}

		//inherited
		//public static bool Equals(object o1, object o2)
		//{
		//	throw new NotImplementedException ();
		//}
		[MonoTODO]
		public override int GetHashCode () 
		{
			//FIXME add our proprities
			return base.GetHashCode ();
		}

		public static NativeWindow FromHandle (IntPtr handle) 
		{
			NativeWindow window = new NativeWindow ();
			window.AssignHandle (handle);
			return window;
		}

		//inherited
		//public object GetLifetimeService() {
		//	throw new NotImplementedException ();
		//}

		//public Type GetType() {
		//	throw new NotImplementedException ();
		//}

		//public virtual object InitializeLifetimeService(){
		//	throw new NotImplementedException ();
		//}

		public virtual void ReleaseHandle () 
		{
			windowHandle = (IntPtr) 0;
			OnHandleChange ();
		}

		[MonoTODO]
		public override string ToString () 
		{
			throw new NotImplementedException ();
		}

		//
		//  --- Protected Methods
		//
		//inherited
		//protected object MemberwiseClone() {
		//	throw new NotImplementedException ();
		//}

		[MonoTODO]
		protected virtual void OnHandleChange () 
		{
			// to be overridden
		}

		[MonoTODO]
		protected virtual void OnThreadException (Exception e) 
		{
			throw new NotImplementedException ();
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
		[MonoTODO]
		~NativeWindow ()
		{
		}

 		static private IntPtr WndProc (
			IntPtr hWnd, Msg msg, IntPtr wParam, IntPtr lParam) 
		{
			// windowCollection is a collection of all the 
			// NativeWindow(s) that have been created.
			// Dispatch the current message to the approriate
			// window.
 			NativeWindow window = 
			        (NativeWindow) windowCollection[hWnd];
 			Message message = new Message ();
			message.HWnd = hWnd;
			message.Msg = msg;
			message.WParam = wParam;
			message.LParam = lParam;
 			message.Result = (IntPtr) 0;

			if (msg == Msg.WM_CREATE)
				Console.WriteLine ("WM_CREATE (static)");

 			if (window != null) {
			if (msg == Msg.WM_CREATE)
				Console.WriteLine ("WM_CREATE (static != null)");
 				window.WndProc(ref message);
 			} else {
				Console.WriteLine ("no window, defwndproc");
 				// even though we are not managing the
 				// window let the window get the message
 				message.Result = Win32.DefWindowProcA (
					hWnd, msg, wParam, lParam);
 			}

 			return message.Result;
 		}
	}
}
