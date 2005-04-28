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

// COMPLETE

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
		private BindingMemberInfo value_member;
		private string display_member;
		protected CurrencyManager data_manager;

		protected ListControl ()
		{			
			data_source = null;
			value_member = new BindingMemberInfo (string.Empty);
			display_member = string.Empty;
			data_manager = null;
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

				if (data_source == value)
					return;

				data_source = value;
				ConnectToDataSource ();
				OnDataSourceChanged (EventArgs.Empty);
			}
		}

		[DefaultValue("")]
		[Editor("System.Windows.Forms.Design.DataMemberFieldEditor, " + Consts.AssemblySystem_Design, typeof(System.Drawing.Design.UITypeEditor))]
		[TypeConverter("System.Windows.Forms.Design.DataMemberFieldConverter, System.Design, Version=1.0.5000.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
		public string DisplayMember {
			get { 
				return display_member;				
			}
			set {
				if (display_member == value) {
					return;
				}

				display_member = value;
				ConnectToDataSource ();				
				OnDisplayMemberChanged (EventArgs.Empty);
			}
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
			get {
				if (data_manager == null) {
					return null;
				}				
				
				object item = data_manager.GetItem (SelectedIndex);
				object fil = FilterItemOnProperty (item, ValueMember);
				return fil;
			}
			set {
				if (data_manager != null) {
					
					PropertyDescriptorCollection col = data_manager.GetItemProperties ();
					PropertyDescriptor prop = col.Find (ValueMember, true);
										
					for (int i = 0; i < data_manager.Count; i++) {
						 if (prop.GetValue (data_manager.GetItem (i)) == value) {
						 	SelectedIndex = i;
						 	return;
						}
					}
					
				}
			}
		}

		[DefaultValue("")]
		[Editor("System.Windows.Forms.Design.DataMemberFieldEditor, " + Consts.AssemblySystem_Design, typeof(System.Drawing.Design.UITypeEditor))]
		public string ValueMember  {
			get { return value_member.BindingMember; }
			set {
				BindingMemberInfo new_value = new BindingMemberInfo (value);
				
				if (value_member.Equals (new_value)) {
					return;
				}
				
				value_member = new_value;
				
				if (display_member == string.Empty) {
					DisplayMember = value_member.BindingMember;					
				}
				
				ConnectToDataSource ();
				OnValueMemberChanged (EventArgs.Empty);
			}
		}

		#endregion Public Properties

		#region Public Methods

		protected object FilterItemOnProperty (object item)
		{
			return FilterItemOnProperty (item, string.Empty);
		}

		protected object FilterItemOnProperty (object item, string field)
		{
			if (item == null)
				return null;

			if (field == null || field == string.Empty)
				return item;

			PropertyDescriptor prop = null;

			if (data_manager != null) {
				PropertyDescriptorCollection col = data_manager.GetItemProperties ();
				prop = col.Find (field, true);				
			}
			
			if (prop == null)
				return item;
			
			return prop.GetValue (item);
		}

		public string GetItemText (object item)
		{
			if (data_manager != null) {
				object fil = FilterItemOnProperty (item, DisplayMember);
				if (fil != null) {
					return fil.ToString ();
				}
			}
								
			return item.ToString ();			
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
			ConnectToDataSource ();

			if (DataManager != null) {
				SetItemsCore (DataManager.List);
				SelectedIndex = DataManager.Position;
			}
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
		
		#endregion Public Methods
		
		#region Private Methods

		internal void BindDataItems (IList items)
		{
			items.Clear ();

			if (data_manager != null) {
				SetItemsCore (data_manager.List);
			}
		}

		private void ConnectToDataSource ()
		{
			if (data_source == null) {
				data_manager = null;
				return;
			}

			if (BindingContext == null) {
				return;
			}
			
			data_manager = (CurrencyManager) BindingContext [data_source, ValueMember];
			data_manager.PositionChanged += new EventHandler (OnPositionChanged);			
		}		
		
		private void OnPositionChanged (object sender, EventArgs e)
		{			
			SelectedIndex = data_manager.Position;
		}

		#endregion Private Methods	
	}

}

