// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
// Copyright (c) 2004-2005 Novell, Inc.
//
// Authors:
//	Jordi Mas i Hernandez, jordi@ximian.com
//
//

// NOT COMPLETE

using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Reflection;

namespace System.Windows.Forms 
{
	public abstract class ListControl : Control
	{
		private object data_source;
		private string display_member = String.Empty;
		private CurrencyManager data_manager;
		
		protected ListControl ()
		{
		}

		#region Events		
		public event EventHandler DataSourceChanged;		
		public event EventHandler DisplayMemberChanged;
		public event EventHandler SelectedValueChanged;
		public event EventHandler ValueMemberChanged;
		#endregion // Events

		#region Public Properties

		[DefaultValue(null)]
		[RefreshProperties(RefreshProperties.Repaint)]
		[TypeConverter("System.Windows.Forms.Design.DataSourceConverter, System.Design, Version=1.0.5000.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
		public object DataSource {
			get { return data_source; }
			set {
				if (!(value is IList || value is IListSource)) {
					throw new Exception ("Complex DataBinding accepts as a data source " +
							"either an IList or an IListSource");
				}

				data_source = value;

				CurrencyManager manager = (CurrencyManager) BindingContext [data_source, display_member];
				data_manager = manager;

				OnDataSourceChanged (EventArgs.Empty);
			}
		}
		
		[DefaultValue("")]
		[Editor("System.Windows.Forms.Design.DataMemberFieldEditor, " + Consts.AssemblySystem_Design, typeof(System.Drawing.Design.UITypeEditor))]
		[TypeConverter("System.Windows.Forms.Design.DataMemberFieldConverter, System.Design, Version=1.0.5000.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
		public string DisplayMember {
			get { return display_member; } 
			set { display_member = value; }
		}
				
		public abstract int SelectedIndex {
			get; 
			set;
		}

		[Bindable(BindableSupport.Yes)]
		[Browsable(false)]
		[DefaultValue(null)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public object SelectedValue {
			get {throw new NotImplementedException (); }			 
			set {throw new NotImplementedException (); }
		}

		[DefaultValue("")]
		[Editor("System.Windows.Forms.Design.DataMemberFieldEditor, " + Consts.AssemblySystem_Design, typeof(System.Drawing.Design.UITypeEditor))]
		public string ValueMember  {
			get { return null; }
			set { }
		}
		
		#endregion Public Properties

		#region Public Methods

		protected object FilterItemOnProperty (object item)
		{
			throw new NotImplementedException ();
		}

		protected object FilterItemOnProperty (object item, string field)
		{
			throw new NotImplementedException (); 
		}

		public string GetItemText (object item)
		{
			 throw new NotImplementedException ();
		}

		protected CurrencyManager DataManager {
			get { return data_manager; }
		}

		// Used only by ListBox to avoid to break Listbox's member signature
		protected override bool IsInputKey (Keys keyData)
		{
			switch (keyData) {
			case Keys.Up:
			case Keys.Down:
			case Keys.PageUp:
			case Keys.PageDown:
			case Keys.Right:
			case Keys.Left:
			case Keys.End:
			case Keys.Home:
			case Keys.ControlKey:
			case Keys.Space:
			case Keys.ShiftKey:
				return true;
			
			default:					
				return false;
			}
		}

		protected override void OnBindingContextChanged (EventArgs e)
		{
			base.OnBindingContextChanged (e);
		}

		protected virtual void OnDataSourceChanged (EventArgs e)
		{
			if (DataSourceChanged != null)
				DataSourceChanged (this,e);
		}

		protected virtual void OnDisplayMemberChanged (EventArgs e)
		{
			if (DisplayMemberChanged != null)
				DisplayMemberChanged (this, e);
		}

		protected virtual void OnSelectedIndexChanged (EventArgs e)
		{
			if (data_manager == null)
				return;
			if (data_manager.Position == SelectedIndex)
				return;
			data_manager.Position = SelectedIndex;
		}

		protected virtual void OnSelectedValueChanged (EventArgs e)
		{
			if (SelectedValueChanged != null)
				SelectedValueChanged (this, e);
		}

		protected virtual void OnValueMemberChanged (EventArgs e)
		{
			if (ValueMemberChanged != null)
				ValueMemberChanged (this, e);
		}

		protected abstract void RefreshItem (int index);

		protected virtual void SetItemCore (int index,	object value)
		{

		}

		protected abstract void SetItemsCore (IList items);

		internal void BindDataItems (IList items)
		{
			items.Clear ();
			SetItemsCore (data_manager.List);
		}
		
		#endregion Public Methods
	}	

}

