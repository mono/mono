//
// System.Windows.Forms.ErrorProvider
//
// Author:
//   stubbed out by Paul Osman (paul.osman@sympatico.ca)
//	 Dennis Hayes(dennish@raytek.com)
// (C) 2002 Ximian, Inc
//
using System.Drawing;
using System.Runtime.Remoting;
namespace System.Windows.Forms {

	// <summary>
	//	This is only a template.  Nothing is implemented yet.
	//
	// </summary>
using System.ComponentModel;
	public class ErrorProvider : Component, IExtenderProvider {

		//
		//  --- Constructor
		//
		[MonoTODO]
		public ErrorProvider()
		{
			throw new NotImplementedException ();
		}

		//
		//  --- Public Properties
		//
		[MonoTODO]
		public override ISite  Site {
			set {
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public int BlinkRate {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public ErrorBlinkStyle BlinkStyle {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		//public IContainer Container {
		//	get {
		//		throw new NotImplementedException ();
		//	}
		//}
		[MonoTODO]
		public ContainerControl ContainerControl {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public string DataMember {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public object DataSource {
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

		//
		//  --- Public Methods
		//
		[MonoTODO]
		public void BindToDataAndErrors(object newDataSource, string newDataMember)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public bool CanExtend(object extendee)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public override ObjRef CreateObjRef(Type requestedType)
		{
			throw new NotImplementedException ();
		}
		//inheriated
		//public void Dispose()
		//{
		//	throw new NotImplementedException ();
		//}
		//public static bool Equals(object o1, object o2)
		//{
		//	throw new NotImplementedException ();
		//}
		[MonoTODO]
		public override bool Equals(object o)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public string GetError(Control control)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public override int GetHashCode()
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public ErrorIconAlignment GetIconAlignment(Control control)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public int GetIconPadding(Control control)
		{
			throw new NotImplementedException ();
		}
		//public object GetLifetimeService()
		//{
		//	throw new NotImplementedException ();
		//}
		//public Type GetType()
		//{
		//	throw new NotImplementedException ();
		//}
		[MonoTODO]
		public override object InitializeLifetimeService()
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public void SetError(Control control,string value)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public void SetIconAlignment(Control control, ErrorIconAlignment value)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public void SetIconPadding(Control control, int padding)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public override string ToString()
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public void UpdateBinding()
		{
			throw new NotImplementedException ();
		}

		//
		//  --- Public Events
		//
		//inherited
		//public event EventHandler Disposed;

		//
		//  --- Protected Properties
		//
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
		[MonoTODO]
		protected override void Dispose(bool disposing)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		protected override object GetService(Type service)
		{
			throw new NotImplementedException ();
		}
		//protected object MemberwiseClone()
		//{
		//	throw new NotImplementedException ();
		//}

		//
		//  --- Destructor
		//
		[MonoTODO]
		~ErrorProvider()
		{
			throw new NotImplementedException ();
		}
	 }
}
