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
	//	This is only a template.  Nothing is implemented yet.
	//
	// </summary>

	public unsafe class NativeWindow : MarshalByRefObject {

		// the window's HWND
		private IntPtr windowHandle;
		static private Hashtable windowCollection = new Hashtable ();

		//
		//  --- Constructor
		//
		public NativeWindow () {
			windowHandle = (IntPtr) 0;
		}

		//
		//  --- Public Properties
		//
		public IntPtr Handle {
			get {
				return windowHandle;
			}
		}

		//
		//  --- Public Methods
		//
		public void AssignHandle (IntPtr handle) {
			if (windowHandle != (IntPtr) 0)
				windowCollection.Remove (windowHandle);

			windowHandle = handle;
			windowCollection.Add (windowHandle, this);
			OnHandleChange ();
		}

		public virtual void CreateHandle (CreateParams cp) {
			IntPtr createdHWnd = (IntPtr) 0;

			windowHandle = Win32.CreateWindowExA (cp.ExStyle,
							      cp.ClassName,
							      cp.Caption,
							      cp.Style,
							      cp.X,
							      cp.Y,
							      cp.Width,
							      cp.Height,
							      cp.Parent,
							      (IntPtr) 0,
							      (IntPtr) 0,
							      null);
			
			if (windowHandle != (IntPtr) 0)
				windowCollection.Add (windowHandle, this);
		}

		[MonoTODO]
		public override ObjRef CreateObjRef (Type requestedType) {
			throw new NotImplementedException ();
		}

		public void DefWndProc (ref Message m) {
			m.Result = Win32.DefWindowProcA (m.HWnd, m.Msg, 
							 m.WParam, m.LParam);
		}

		public virtual void DestroyHandle () {
			windowCollection.Remove (windowHandle);
			Win32.DestroyWindow (windowHandle);
		}

		[MonoTODO]
		public override bool Equals (object o) {
			throw new NotImplementedException ();
		}

		//inherited
		//public static bool Equals(object o1, object o2)
		//{
		//	throw new NotImplementedException ();
		//}
		[MonoTODO]
		public override int GetHashCode () {
			//FIXME add our proprities
			return base.GetHashCode ();
		}

		public static NativeWindow FromHandle (IntPtr handle) {
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

		public virtual void ReleaseHandle () {
			windowHandle = (IntPtr) 0;
			OnHandleChange ();
		}

		[MonoTODO]
		public override string ToString () {
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
		protected virtual void OnHandleChange () {
			// to be overridden
		}
		[MonoTODO]
		protected virtual void OnThreadException (Exception e) {
			throw new NotImplementedException ();
		}

		protected virtual void WndProc (ref Message m) {
			Console.WriteLine ("NativeWindow.WndProc");
			DefWndProc (ref m);
		}

		//
		//  --- Destructor
		//
		[MonoTODO]
		~NativeWindow () {
		}

// obsolete: should probably be handled by Application class
// 		static private IntPtr _WndProc (IntPtr hWnd, int msg, 
// 					        IntPtr wParam, IntPtr lParam) {
// 		        Console.WriteLine ("in _WndProc");
// 			NativeWindow window = 
// 			    (NativeWindow) windowCollection[hWnd];
// 			Message message = new Message ();
// 			message.Result = (IntPtr) 0;
// 			if (window != null) {
// 				message.HWnd = hWnd;
// 				message.Msg = msg;
// 				message.WParam = wParam;
// 				message.LParam = lParam;
// 				window.WndProc(ref message);
// 			} else {
// 				// even though we are not managing the
// 				// window let the window get the message
// 				message.Result = Win32.DefWindowProcA (hWnd, 
// 								       msg, 
// 								       wParam, 
// 								       lParam);
// 			}
// 			return message.Result;
// 		}
	}
}
