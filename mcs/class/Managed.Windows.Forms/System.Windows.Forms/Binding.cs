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
//	Peter Bartok	pbartok@novell.com
//	Jackson Harper	jackson@ximian.com
//


using System.ComponentModel;

namespace System.Windows.Forms {

	[TypeConverter (typeof (ListBindingConverter))]
	public class Binding {

		private string property_name;
		private object data_source;
		private string data_member;

		private bool is_binding;
		private bool checked_isnull;

		private BindingMemberInfo binding_member_info;
		private Control control;

		private BindingManagerBase manager;
		private PropertyDescriptor control_property;
		private PropertyDescriptor is_null_desc;

		private object data;
		private Type data_type;

		#region Public Constructors
		public Binding (string propertyName, object dataSource, string dataMember)
		{
			property_name = propertyName;
			data_source = dataSource;
			data_member = dataMember;
			binding_member_info = new BindingMemberInfo (dataMember);
		}
		#endregion	// Public Constructors

		#region Public Instance Properties
		public BindingManagerBase BindingManagerBase {
			get {
				return manager;
			}
		}

		public BindingMemberInfo BindingMemberInfo {
			get {
				return binding_member_info;
			}
		}

		[DefaultValue (null)]
		public Control Control {
			get {
				return control;
			}
		}

		public object DataSource {
			get {
				return data_source;
			}
		}

		public bool IsBinding {
			get {
				return is_binding;
			}
		}

		[DefaultValue ("")]
		public string PropertyName {
			get {
				return property_name;
			}
		}
		#endregion	// Public Instance Properties

		#region Protected Instance Methods
		protected virtual void OnFormat (ConvertEventArgs cevent)
		{
			if (Format!=null)
				Format (this, cevent);
		}

		protected virtual void OnParse (ConvertEventArgs cevent)
		{
			if (Parse!=null)
				Parse (this, cevent);
		}
		#endregion	// Protected Instance Methods

		
		internal void SetControl (Control control)
		{
			if (control == this.control)
				return;

			control_property = TypeDescriptor.GetProperties (control).Find (property_name, true);			

			if (control_property == null)
				throw new ArgumentException (String.Concat ("Cannot bind to property '", property_name, "' on target control."));
			if (control_property.IsReadOnly)
				throw new ArgumentException (String.Concat ("Cannot bind to property '", property_name, "' because it is read only."));
				
			data_type = control_property.PropertyType; // Getting the PropertyType is kinda slow and it should never change, so it is cached
			control.Validating += new CancelEventHandler (ControlValidatingHandler);

			this.control = control;
		}

		internal void Check ()
		{
			if (control == null || control.BindingContext == null)
				return;

			if (manager == null) {
				manager = control.BindingContext [data_source];
				manager.AddBinding (this);
				manager.PositionChanged += new EventHandler (PositionChangedHandler);
			}

			if (manager.Position == -1)
				return;

			if (!checked_isnull) {
				is_null_desc = TypeDescriptor.GetProperties (manager.Current).Find (property_name + "IsNull", false);
				checked_isnull = true;
			}

			PushData ();
		}

		internal void PullData ()
		{
			if (IsBinding == false || manager.Current == null)
				return;

			data = control_property.GetValue (control);
			SetPropertyValue (data);
		}

		internal void PushData ()
		{
			if (manager == null || manager.IsSuspended || manager.Current == null)
				return;

			if (is_null_desc != null) {
				bool is_null = (bool) is_null_desc.GetValue (manager.Current);
				if (is_null) {
					data = Convert.DBNull;
					return;
				}
			}

			PropertyDescriptor pd = TypeDescriptor.GetProperties (manager.Current).Find (binding_member_info.BindingField, true);
			if (pd == null) {
				data = manager.Current;
			} else {
				data = pd.GetValue (manager.Current);
			}

			data = FormatData (data);
			SetControlValue (data);
		}

		internal void UpdateIsBinding ()
		{
			is_binding = false;
			if (control == null || !control.Created)
				return;
			if (manager == null || manager.IsSuspended)
				return;

			is_binding = true;
			PushData ();
		}

		private void SetControlValue (object data)
		{
			control_property.SetValue (control, data);
		}

		private void SetPropertyValue (object data)
		{
			PropertyDescriptor pd = TypeDescriptor.GetProperties (manager.Current).Find (binding_member_info.BindingField, true);
			if (pd.IsReadOnly)
				return;
			data = ParseData (data, pd.PropertyType);
			pd.SetValue (manager.Current, data);
		}

		private void ControlValidatingHandler (object sender, CancelEventArgs e)
		{
			object old_data = data;

			// If the data doesn't seem to be valid (it can't be converted,
			// is the wrong type, etc, we reset to the old data value.
			try {
				PullData ();
			} catch {
				data = old_data;
				SetControlValue (data);
			}
		}

		private void PositionChangedHandler (object sender, EventArgs e)
		{
			Check ();
			PushData ();
		}

		private object ParseData (object data, Type data_type)
		{
			ConvertEventArgs e = new ConvertEventArgs (data, data_type);

			OnParse (e);
			if (data_type.IsInstanceOfType (e.Value))
				return e.Value;
			if (e.Value == Convert.DBNull)
				return e.Value;

			return ConvertData (e.Value, data_type);
		}

		private object FormatData (object data)
		{
			if (data_type == typeof (object)) 
				return data;

			ConvertEventArgs e = new ConvertEventArgs (data, data_type);

			OnFormat (e);
			if (data_type.IsInstanceOfType (e.Value))
				return e.Value;

			return ConvertData (data, data_type);
		}

		private object ConvertData (object data, Type data_type)
		{
			TypeConverter converter = TypeDescriptor.GetConverter (data.GetType ());
			if (converter != null && converter.CanConvertTo (data_type))
				return converter.ConvertTo (data, data_type);

			if (data is IConvertible) {
				object res = Convert.ChangeType (data, data_type);
				if (data_type.IsInstanceOfType (res))
					return res;
			}

			return null;
		}

		#region Events
		public event ConvertEventHandler Format;
		public event ConvertEventHandler Parse;
		#endregion	// Events
	}
}
