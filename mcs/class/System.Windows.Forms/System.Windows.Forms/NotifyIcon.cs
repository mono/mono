//
// System.Windows.Forms.NotifyIcon.cs
//
// Author:
//   stubbed out by Paul Osman (paul.osman@sympatico.ca)
//   Dennis Hayes (dennish@Raytek.com)
//
// (C) 2002 Ximian, Inc
//
using System.ComponentModel;
using System.Drawing;
using System.Runtime.Remoting;

namespace System.Windows.Forms {

	// <summary>
	//	This is only a template.  Nothing is implemented yet.
	//
	// </summary>
    public sealed class NotifyIcon : Component {

		//
		//  --- Constructor
		//
		[MonoTODO]
		public NotifyIcon()
		{
			throw new NotImplementedException ();
		}

		//
		//  --- Public Properties
		//
		//inherited
		//public IContainer Container {
		//	get {
		//		throw new NotImplementedException ();
		//	}
		//	set {
		//		throw new NotImplementedException ();
		//	}
		//}
		[MonoTODO]
		public ContextMenu ContextMenu {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public Icon Icon {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		//[MonoTODO]
		//public override ISite Site {
		//	get {
		//		throw new NotImplementedException ();
		//	}
		//	set {
		//		throw new NotImplementedException ();
		//	}
		//}
		[MonoTODO]
		public string Text {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public bool Visible {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		//
		//  --- Public Methods
		//
		[MonoTODO]
		public override ObjRef CreateObjRef(Type requestedType)
		{
			throw new NotImplementedException ();
		}
		//inherited
		//public void Dispose()
		//{
		//	throw new NotImplementedException ();
		//}
		//public object GetLifetimeService()
		//{
		//	throw new NotImplementedException ();
		//}
		//inherited
		//public virtual object InitializeLifetimeService()
		//{
		//	throw new NotImplementedException ();
		//}
		[MonoTODO]
		public override bool Equals(object o)
		{
			throw new NotImplementedException ();
		}
		//inherited
		//public static bool Equals(object o1, object o2)
		//{
		//	throw new NotImplementedException ();
		//}
		[MonoTODO]
		public override int GetHashCode() {
			//FIXME add our proprities
			return base.GetHashCode();
		}

		//inherited
		//public Type GetType()
		//{
		//	throw new NotImplementedException ();
		//}
		[MonoTODO]
		public override string ToString()
		{
			throw new NotImplementedException ();
		}

		//
		//  --- Public Events
		//
		[MonoTODO]
		public event EventHandler Click;
		//public event EventHandler Disposed;
		public event EventHandler DoubleClick;
		public event MouseEventHandler MouseDown;
		public event MouseEventHandler MouseMove;
		public event MouseEventHandler MouseUp;

		//
		//  --- Protected Properties
		//
		//[MonoTODO]
		//protected bool DesignMode {
		//	get {
		//		throw new NotImplementedException ();
		//	}
		//}
		//protected EventHandlerList Events {
		//	get {
		//		throw new NotImplementedException ();
		//	}
		//}

		//
		//  --- Protected Methods
		//
		//[MonoTODO]
		//protected virtual void Dispose(bool disposing)
		//{
		//	throw new NotImplementedException ();
		//}
		//[MonoTODO]
		//protected virtual object GetService(Type service)
		//{
		//	throw new NotImplementedException ();
		//}
		//[MonoTODO]
		//protected object MemberwiseClone()
		//{
		//	throw new NotImplementedException ();
		//}

		//
		//  --- DeConstructor
		//
		[MonoTODO]
		~NotifyIcon()
		{
			throw new NotImplementedException ();
		}
	 }
}
