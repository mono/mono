//
//	System.Windows.Forms.ListControl
//
//	Author:
//		Alberto Fernandez	(infjaf00@yahoo.es)
//



using System;
using System.Collections;

namespace System.Windows.Forms{

	public abstract class ListControl : Control{
		[MonoTODO]
		protected ListControl(){
		}
		[MonoTODO]
		protected CurrencyManager DataManager {
			get { throw new NotImplementedException(); }
		}
		[MonoTODO]
		public object DataSource {
			get {throw new NotImplementedException();}
			set {}
		}
		[MonoTODO]
		public string DisplayMember {
			get { throw new NotImplementedException();}			
			set {}
		}
		public abstract int SelectedIndex {get; set;}
		
		[MonoTODO]
		public object SelectedValue {
			get {throw new NotImplementedException();}
			set {}
		}
		[MonoTODO]
		public string ValueMember {
			get { throw new NotImplementedException(); } 
			set {}
		}

		// No usar
		protected object FilterItemOnProperty(object item){
			throw new NotImplementedException();
		}
		protected object FilterItemOnProperty( object item, string field){
			throw new NotImplementedException();
		}

		[MonoTODO]
		public string GetItemText(object item){
			throw new NotImplementedException();
		}
		[MonoTODO]
		protected override bool IsInputKey( Keys keyData){
			return base.IsInputKey (keyData);
		}
		[MonoTODO]
		protected override void OnBindingContextChanged(EventArgs e){
		
		}
		protected virtual void OnDataSourceChanged(EventArgs e){
			if (DataSourceChanged != null)
				DataSourceChanged (this,e);			
		}
		protected virtual void OnDisplayMemberChanged( EventArgs e){
			if (DisplayMemberChanged != null)
				DisplayMemberChanged (this,e);
		}
		protected virtual void OnSelectedIndexChanged(EventArgs e){
			if (SelectedValueChanged != null)
				SelectedValueChanged (this, e);
		}
		protected virtual void OnSelectedValueChanged(EventArgs e){
			if (SelectedValueChanged != null)
				SelectedValueChanged (this, e);
		}
		protected virtual void OnValueMemberChanged(EventArgs e){
			if (ValueMemberChanged != null)
				ValueMemberChanged (this, e);
		}
		protected abstract void RefreshItem(int index);
		
		// Don't use
		[MonoTODO]
		protected virtual void SetItemCore(int index, object value){
		}
		// Don't use
		protected abstract void SetItemsCore(IList items);
		

		public event EventHandler DataSourceChanged;
		public event EventHandler DisplayMemberChanged;
		public event EventHandler SelectedValueChanged;
		public event EventHandler ValueMemberChanged;
	}
}
