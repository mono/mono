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
using System.Runtime.InteropServices;

namespace System.Windows.Forms
{
	[ClassInterface (ClassInterfaceType.AutoDispatch)]
	[ComVisible (true)]
	[LookupBindingProperties ("DataSource", "DisplayMember", "ValueMember", "SelectedValue")]
	public abstract class ListControl : Control
	{
		private object data_source;
		private BindingMemberInfo value_member;
		private string display_member;
		private CurrencyManager data_manager;
		private BindingContext last_binding_context;
		private IFormatProvider format_info;
		private string format_string = string.Empty;
		private bool formatting_enabled;

		protected ListControl ()
		{
			value_member = new BindingMemberInfo (string.Empty);
			display_member = string.Empty;
			SetStyle (ControlStyles.StandardClick | ControlStyles.UserPaint | ControlStyles.UseTextForAccessibility, false);
		}

		#region Events
		static object DataSourceChangedEvent = new object ();
		static object DisplayMemberChangedEvent = new object ();
		static object FormatEvent = new object ();
		static object FormatInfoChangedEvent = new object ();
		static object FormatStringChangedEvent = new object ();
		static object FormattingEnabledChangedEvent = new object ();
		static object SelectedValueChangedEvent = new object ();
		static object ValueMemberChangedEvent = new object ();

		public event EventHandler DataSourceChanged {
			add { Events.AddHandler (DataSourceChangedEvent, value); }
			remove { Events.RemoveHandler (DataSourceChangedEvent, value); }
		}

		public event EventHandler DisplayMemberChanged {
			add { Events.AddHandler (DisplayMemberChangedEvent, value); }
			remove { Events.RemoveHandler (DisplayMemberChangedEvent, value); }
		}

		public event ListControlConvertEventHandler Format {
			add { Events.AddHandler (FormatEvent, value); }
			remove { Events.RemoveHandler (FormatEvent, value); }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Advanced)]
		public event EventHandler FormatInfoChanged {
			add { Events.AddHandler (FormatInfoChangedEvent, value); }
			remove { Events.RemoveHandler (FormatInfoChangedEvent, value); }
		}
		
		public event EventHandler FormatStringChanged {
			add { Events.AddHandler (FormatStringChangedEvent, value); }
			remove { Events.RemoveHandler (FormatStringChangedEvent, value); }
		}
		
		public event EventHandler FormattingEnabledChanged {
			add { Events.AddHandler (FormattingEnabledChangedEvent, value); }
			remove { Events.RemoveHandler (FormattingEnabledChangedEvent, value); }
		}

		public event EventHandler SelectedValueChanged {
			add { Events.AddHandler (SelectedValueChangedEvent, value); }
			remove { Events.RemoveHandler (SelectedValueChangedEvent, value); }
		}

		public event EventHandler ValueMemberChanged {
			add { Events.AddHandler (ValueMemberChangedEvent, value); }
			remove { Events.RemoveHandler (ValueMemberChangedEvent, value); }
		}

		#endregion // Events

		#region .NET 2.0 Public Properties
		[Browsable (false)]
		[DefaultValue (null)]
		[EditorBrowsable (EditorBrowsableState.Advanced)]
		public IFormatProvider FormatInfo {
			get { return format_info; }
			set {
				if (format_info != value) {
					format_info = value;
					RefreshItems ();
					OnFormatInfoChanged (EventArgs.Empty);
				}
			}
		}
		
		[DefaultValue ("")]
		[MergableProperty (false)]
		[Editor ("System.Windows.Forms.Design.FormatStringEditor, " + Consts.AssemblySystem_Design, typeof (System.Drawing.Design.UITypeEditor))]
		public string FormatString {
			get { return format_string; }
			set {
				if (format_string != value) {
					format_string = value;
					RefreshItems ();
					OnFormatStringChanged (EventArgs.Empty);
				}
			}
		}
		
		[DefaultValue (false)]
		public bool FormattingEnabled {
			get { return formatting_enabled; }
			set { 
				if (formatting_enabled != value) {
					formatting_enabled = value;
					RefreshItems ();
					OnFormattingEnabledChanged (EventArgs.Empty);
				}
			}
		}
		#endregion

		#region Public Properties

		[DefaultValue(null)]
		[RefreshProperties(RefreshProperties.Repaint)]
		[AttributeProvider (typeof (IListSource))]
		[MWFCategory("Data")]
		public object DataSource {
			get { return data_source; }
			set {
				if (data_source == value)
					return;

				if (value == null)
					display_member = String.Empty;
				else if (!(value is IList || value is IListSource))
					throw new Exception ("Complex DataBinding accepts as a data source " +
						"either an IList or an IListSource");

				data_source = value;
				ConnectToDataSource ();
				OnDataSourceChanged (EventArgs.Empty);
			}
		}

		[DefaultValue("")]
		[Editor("System.Windows.Forms.Design.DataMemberFieldEditor, " + Consts.AssemblySystem_Design, typeof(System.Drawing.Design.UITypeEditor))]
		[TypeConverter("System.Windows.Forms.Design.DataMemberFieldConverter, " + Consts.AssemblySystem_Design)]
		[MWFCategory("Data")]
		public string DisplayMember {
			get { 
				return display_member;
			}
			set {
				if (value == null)
					value = String.Empty;

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
				if (data_manager == null || SelectedIndex == -1)
					return null;
				
				object item = data_manager [SelectedIndex];
				object fil = FilterItemOnProperty (item, ValueMember);
				return fil;
			}
			set {
				if (data_manager == null)
					return;

				if (value == null)
					throw new ArgumentNullException ("value");

				PropertyDescriptorCollection col = data_manager.GetItemProperties ();
				PropertyDescriptor prop = col.Find (ValueMember, true);

				for (int i = 0; i < data_manager.Count; i++) {
					if (value.Equals (prop.GetValue (data_manager [i]))) {
						SelectedIndex = i;
						return;
					}
				}
				SelectedIndex = -1;
			}
		}

		[DefaultValue("")]
		[Editor("System.Windows.Forms.Design.DataMemberFieldEditor, " + Consts.AssemblySystem_Design, typeof(System.Drawing.Design.UITypeEditor))]
		[MWFCategory("Data")]
		public string ValueMember {
			get { return value_member.BindingMember; }
			set {
				BindingMemberInfo new_value = new BindingMemberInfo (value);
				
				if (value_member.Equals (new_value))
					return;
				
				value_member = new_value;
				
				if (display_member == string.Empty)
					DisplayMember = value_member.BindingMember;

				ConnectToDataSource ();
				OnValueMemberChanged (EventArgs.Empty);
			}
		}

		protected virtual bool AllowSelection {
			get { return true; }
		}

		#endregion Public Properties

		#region Private Properties

		internal override bool ScaleChildrenInternal {
			get { return false; }
		}

		#endregion Private Properties

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
			} else {
				PropertyDescriptorCollection properties = TypeDescriptor.GetProperties (item);
				prop = properties.Find (field, true);
			}
			
			if (prop == null)
				return item;
			
			return prop.GetValue (item);
		}

		public string GetItemText (object item)
		{
			object o = FilterItemOnProperty (item, DisplayMember);
			
			if (o == null)
				o = item;

			string retval = o.ToString ();
			
			if (FormattingEnabled) {
				ListControlConvertEventArgs e = new ListControlConvertEventArgs (o, typeof (string), item);
				OnFormat (e);
				
				// The user provided their own value
				if (e.Value.ToString () != retval)
					return e.Value.ToString ();
					
				if (o is IFormattable)
					return ((IFormattable)o).ToString (string.IsNullOrEmpty (FormatString) ? null : FormatString, FormatInfo);
			}
				
			return retval;
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

		// Since this event is fired twice for the same binding context instance
		// (when the control is added to the form and when the form is shown), 
		// we only take into account the first time it happens
		protected override void OnBindingContextChanged (EventArgs e)
		{
			base.OnBindingContextChanged (e);
			if (last_binding_context == BindingContext)
				return;

			last_binding_context = BindingContext;
			ConnectToDataSource ();

			if (DataManager != null) {
				SetItemsCore (DataManager.List);
				if (AllowSelection)
					SelectedIndex = DataManager.Position;
			}
		}

		protected virtual void OnDataSourceChanged (EventArgs e)
		{
			EventHandler eh = (EventHandler)(Events [DataSourceChangedEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected virtual void OnDisplayMemberChanged (EventArgs e)
		{
			EventHandler eh = (EventHandler)(Events [DisplayMemberChangedEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected virtual void OnFormat (ListControlConvertEventArgs e)
		{
			ListControlConvertEventHandler eh = (ListControlConvertEventHandler)(Events[FormatEvent]);
			if (eh != null)
				eh (this, e);
		}
		
		protected virtual void OnFormatInfoChanged (EventArgs e)
		{
			EventHandler eh = (EventHandler)(Events[FormatInfoChangedEvent]);
			if (eh != null)
				eh (this, e);
		}
		
		protected virtual void OnFormatStringChanged (EventArgs e)
		{
			EventHandler eh = (EventHandler)(Events[FormatStringChangedEvent]);
			if (eh != null)
				eh (this, e);
		}
		
		protected virtual void OnFormattingEnabledChanged (EventArgs e)
		{
			EventHandler eh = (EventHandler)(Events[FormattingEnabledChangedEvent]);
			if (eh != null)
				eh (this, e);
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
			EventHandler eh = (EventHandler)(Events [SelectedValueChangedEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected virtual void OnValueMemberChanged (EventArgs e)
		{
			EventHandler eh = (EventHandler)(Events [ValueMemberChangedEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected abstract void RefreshItem (int index);
		
		protected virtual void RefreshItems ()
		{
		}

		protected virtual void SetItemCore (int index,	object value)
		{
		}

		protected abstract void SetItemsCore (IList items);
		
		#endregion Public Methods
		
		#region Private Methods

		internal void BindDataItems ()
		{
			SetItemsCore (data_manager != null ? data_manager.List : new object [0]);
		}

		private void ConnectToDataSource ()
		{
			if (BindingContext == null)
				return;

			CurrencyManager newDataMgr = null;
			if (data_source != null)
				newDataMgr = (CurrencyManager) BindingContext [data_source];
			if (newDataMgr != data_manager) {
				if (data_manager != null) {
					// Disconnect handlers from previous manager
					data_manager.PositionChanged -= new EventHandler (OnPositionChanged);
					data_manager.ItemChanged -= new ItemChangedEventHandler (OnItemChanged);
				}
				if (newDataMgr != null) {
					newDataMgr.PositionChanged += new EventHandler (OnPositionChanged);
					newDataMgr.ItemChanged += new ItemChangedEventHandler (OnItemChanged);
				}
				data_manager = newDataMgr;
			}
		}

		private void OnItemChanged (object sender, ItemChangedEventArgs e)
		{
			/* if the list has changed, tell our subclass to re-bind */
			if (e.Index == -1)
				SetItemsCore (data_manager.List);
			else
				RefreshItem (e.Index);

			/* For the first added item, ItemChanged is fired _after_ PositionChanged,
			 * so we need to set Index _only_ for that case - normally we would do that
			 * in PositionChanged handler */
			if (AllowSelection && SelectedIndex == -1 && data_manager.Count == 1)
				SelectedIndex = data_manager.Position;
		}

		private void OnPositionChanged (object sender, EventArgs e)
		{
			/* For the first added item, PositionChanged is fired
			 * _before_ ItemChanged (items not yet added), which leave us in a temporary
			 * invalid state */
			if (AllowSelection && data_manager.Count > 1)
				SelectedIndex = data_manager.Position;
		}

		#endregion Private Methods
	}
}
