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
using System.Reflection;

namespace System.Windows.Forms {

	// <summary>
	//
	// </summary>

    public abstract class ListControl : Control {

		protected string DisplayMember_ = String.Empty;

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
				//FIXME:
			}
		}
		[MonoTODO]
		public string DisplayMember {
			get {
				return DisplayMember_;
			}
			set {
				if( DisplayMember_ != value) {
					DisplayMember_ = value;
					OnDisplayMemberChanged(new EventArgs());
				}
			}
		}
		internal string getDisplayMemberOfObj( object obj) {
			string objectString = String.Empty;
			Type t = obj.GetType();
			if( DisplayMember != String.Empty) {
				if( t != null) {
					PropertyInfo prop = t.GetProperty(DisplayMember);
					if( prop != null) {
						objectString = prop.GetValue(obj, null).ToString();
					}
				}
			}
			if( objectString == String.Empty) {
				objectString = obj.ToString();
			}
			return objectString;
		}

		[MonoTODO]
		public abstract int SelectedIndex {get;set;}

		[MonoTODO]
		public object SelectedValue {
			get {
				throw new NotImplementedException ();
			}
			set {
				//FIXME:
			}
		}
		[MonoTODO]
		public string ValueMember {
			get {
				throw new NotImplementedException ();
			}
			set {
				//FIXME:
			}
		}

		//
		//  --- Public Methods
		//

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
			//FIXME:
			return base.IsInputKey(keyData);
		}
		[MonoTODO]
		protected virtual void OnDataSourceChanged(EventArgs e) {
			//FIXME:
		}
		[MonoTODO]
		protected virtual void OnDisplayMemberChanged(EventArgs e) {
			//FIXME:
		}

		[MonoTODO]
		protected virtual void OnSelectedIndexChanged(EventArgs e) {
			//FIXME:
		}		
		
		[MonoTODO]
		protected virtual void OnSelectedValueChanged(EventArgs e){
			//FIXME:
		}

		
		[MonoTODO]
		protected override void OnBindingContextChanged(EventArgs e)
		{
			//FIXME:
			base.OnBindingContextChanged(e);
		}

		[MonoTODO]
		protected abstract void RefreshItem(int index);

	 }
}
