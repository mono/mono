//
// System.Windows.Forms.ListControl.cs
//
// Author:
//   stubbed out by Daniel Carrera (dcarrera@math.toronto.edu)
//	Dennis Hayes (dennish@raytek.com)
// (C) 2002 Ximian, Inc
//
using System;
using System.Drawing;

namespace System.Windows.Forms {

	// <summary>
	//
	// </summary>

    public abstract class ListControl : Control {

		//ControlStyles controlStyles;
		//
		//  --- Public Properties
		//
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
		public string DisplayMember {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public abstract int SelectedIndex {get;set;}
//			get {
//				//throw new NotImplementedException ();
//			}
//			set {
//				//throw new NotImplementedException ();
//			}
//		}
		[MonoTODO]
		public object SelectedValue {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public string ValueMember {
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
		public override bool Equals(object o)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override int GetHashCode() {
			//FIXME add our proprities
			return base.GetHashCode();
		}
		[MonoTODO]
		public string GetItemText(object Item)
		{
			throw new NotImplementedException ();
		}

		//
		//  --- Public Events
		//
		[MonoTODO]
		public event EventHandler DataSourceChanged;
		[MonoTODO]
		public event EventHandler DisplayMemberChanged;

		//
		// --- Protected Constructor
		//
		[MonoTODO]
		protected ListControl()
		{
			
		}

		//
		//  --- Protected Properties
		//
		[MonoTODO]
		protected CurrencyManager DataManager {
			get {
				throw new NotImplementedException ();
			}
		}

		//
		//  --- Protected Methods
		//
		[MonoTODO]
		protected override bool IsInputKey(Keys keyData)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		protected virtual void OnDataSourceChanged(EventArgs e) {
			throw new NotImplementedException ();
		}
		[MonoTODO]
		protected virtual void OnDisplayMemberChanged(EventArgs e) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual void OnSelectedIndexChanged(EventArgs e) {
			throw new NotImplementedException ();
		}		
		
		[MonoTODO]
		protected virtual void OnSelectedValueChanged(EventArgs e){
			throw new NotImplementedException ();
		}

		
		[MonoTODO]
		protected override void OnBindingContextChanged(EventArgs e)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected abstract void RefreshItem(int index);

	 }
}
