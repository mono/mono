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
	//
	// </summary>
using System.ComponentModel;
	public class ErrorProvider : Component, IExtenderProvider {
		internal string dataMember;
		//
		//  --- Constructor
		//
		[MonoTODO]
		public ErrorProvider()
		{
			dataMember = "";
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

		internal ContainerControl cc;//FIXME: just to get it to run
		[MonoTODO]
		public ContainerControl ContainerControl {
			get {
				 return cc;
			}
			set {
				cc = value;
			}
		}

		[MonoTODO]
		public string DataMember {
			get {
				return dataMember;
			}
			set {
				dataMember = value;
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
		public string GetError(Control control)
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
		public void UpdateBinding()
		{
			throw new NotImplementedException ();
		}
	 }
}
