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

#if NET_2_0
		private DataSourceUpdateMode datasource_update_mode;
		private ControlUpdateMode control_update_mode;
		//private object null_value;
		//private string format_string;
		//private IFormatProvider format_info;
		private bool formatting_enabled;
#endif
		#region Public Constructors
#if NET_2_0
		public Binding (string propertyName, object dataSource, string dataMember) 
			: this (propertyName, dataSource, dataMember, false, DataSourceUpdateMode.OnValidation, null, string.Empty, null)
		{
		}
		
		public Binding (string propertyName, object dataSource, string dataMember, bool formattingEnabled)
			: this (propertyName, dataSource, dataMember, formattingEnabled, DataSourceUpdateMode.OnValidation, null, string.Empty, null)
		{
		}
		
		public Binding (string propertyName, object dataSource, string dataMember, bool formattingEnabled, DataSourceUpdateMode dataSourceUpdateMode)
			: this (propertyName, dataSource, dataMember, formattingEnabled, dataSourceUpdateMode, null, string.Empty, null)
		{
		}
		
		public Binding (string propertyName, object dataSource, string dataMember, bool formattingEnabled, DataSourceUpdateMode dataSourceUpdateMode, object nullValue)
			: this (propertyName, dataSource, dataMember, formattingEnabled, dataSourceUpdateMode, nullValue, string.Empty, null)
		{
		}

		public Binding (string propertyName, object dataSource, string dataMember, bool formattingEnabled, DataSourceUpdateMode dataSourceUpdateMode, object nullValue, string formatString)
			: this (propertyName, dataSource, dataMember, formattingEnabled, dataSourceUpdateMode, nullValue, formatString, null)
		{
		}

		public Binding (string propertyName, object dataSource, string dataMember, bool formattingEnabled, DataSourceUpdateMode dataSourceUpdateMode, object nullValue, string formatString, IFormatProvider formatInfo)
		{
			property_name = propertyName;
			data_source = dataSource;
			data_member = dataMember;
			binding_member_info = new BindingMemberInfo (dataMember);
			datasource_update_mode = dataSourceUpdateMode;
			//null_value = nullValue;
			//format_string = formatString;
			//format_info = formatInfo;

			ConnectPropertyChangedEvent ();
		}
#else
		public Binding (string propertyName, object dataSource, string dataMember)
		{
			property_name = propertyName;
			data_source = dataSource;
			data_member = dataMember;
			binding_member_info = new BindingMemberInfo (dataMember);
			ConnectPropertyChangedEvent ();
		}		
#endif
		#endregion	// Public Constructors

		#region Public Instance Properties
#if NET_2_0
		public IBindableComponent BindableComponent {
			get {
				return control;
			}
		}
#endif
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

#if NET_2_0
		[DefaultValue (ControlUpdateMode.OnPropertyChanged)]
		public ControlUpdateMode ControlUpdateMode {
			get {
				return control_update_mode;
			}
			set {
				control_update_mode = value;
			}
		}
#endif

		public object DataSource {
			get {
				return data_source;
			}
		}

#if NET_2_0
		[DefaultValue (DataSourceUpdateMode.OnValidation)]
		public DataSourceUpdateMode DataSourceUpdateMode {
			get {
				return datasource_update_mode;
			}
			set {
				datasource_update_mode = value;
			}
		}

		[DefaultValue (false)]
		public bool FormattingEnabled {
			get {
				return formatting_enabled;
			}
			set {
				if (formatting_enabled == value)
					return;

				formatting_enabled = value;
			}
		}
#endif

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

#if NET_2_0
		public void ReadValue ()
		{
			PushData (true);
		}

		public void WriteValue ()
		{
			PullData (true);
		}
#endif

		#region Protected Instance Methods
#if NET_2_0
		protected virtual void OnBindingComplete (BindingCompleteEventArgs args)
		{
			if (BindingComplete != null)
				BindingComplete (this, args);
		}
#endif

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

		internal string DataMember {
			get { return data_member; }
		}
		
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
			PullData (false);
		}

		void PullData (bool force)
		{
			if (IsBinding == false || manager.Current == null)
				return;
#if NET_2_0
			if (!force && datasource_update_mode == DataSourceUpdateMode.Never)
				return;
#endif

			data = control_property.GetValue (control);

			try {
				SetPropertyValue (data);
			} catch (Exception e) {
#if NET_2_0
				if (formatting_enabled) {
					FireBindingComplete (BindingCompleteContext.DataSourceUpdate, e, e.Message);
					return;
				}
#endif
				throw e;
			}

#if NET_2_0
			if (formatting_enabled)
				FireBindingComplete (BindingCompleteContext.DataSourceUpdate, null, null);
#endif
		}

		internal void PushData ()
		{
			PushData (false);
		}

		void PushData (bool force)
		{
			if (manager == null || manager.IsSuspended || manager.Current == null)
				return;
#if NET_2_0
			if (!force && control_update_mode == ControlUpdateMode.Never)
				return;
#endif

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

			try {
				data = FormatData (data);
				SetControlValue (data);
			} catch (Exception e) {
#if NET_2_0
				if (formatting_enabled) {
					FireBindingComplete (BindingCompleteContext.ControlUpdate, e, e.Message);
					return;
				}
#endif
				throw e;
			}

#if NET_2_0
			if (formatting_enabled)
				FireBindingComplete (BindingCompleteContext.ControlUpdate, null, null);
#endif
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

		void ConnectPropertyChangedEvent ()
		{
#if NET_2_0
			if (data_source == null || binding_member_info == new BindingMemberInfo (String.Empty) || binding_member_info.BindingField.Length == 0)
				return;
#else
			if (data_source == null || binding_member_info.Equals (new BindingMemberInfo (String.Empty)) || binding_member_info.BindingField.Length == 0)
				return;
#endif

			string event_name = binding_member_info.BindingField + "Changed";
			Type event_handler_type = typeof (EventHandler);

			EventDescriptor prop_changed_event = null;
			foreach (EventDescriptor event_desc in TypeDescriptor.GetEvents (data_source)) {
				if (event_desc.Name == event_name && event_desc.EventType == event_handler_type) {
					prop_changed_event = event_desc;
					break;
				}
			}

			// No event for property changes
			if (prop_changed_event == null)
				return;

			prop_changed_event.AddEventHandler (data_source, new EventHandler (PropertyChangedHandler));
		}

		void PropertyChangedHandler (object o, EventArgs args)
		{
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
			ConvertEventArgs e = new ConvertEventArgs (data, data_type);

			OnFormat (e);
			if (data_type.IsInstanceOfType (e.Value))
				return e.Value;

			if (e.Value == null && data_type == typeof (object))
				return Convert.DBNull;

			return ConvertData (data, data_type);
		}

		private object ConvertData (object data, Type data_type)
		{
			if (data == null)
				return null;

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
#if NET_2_0
		void FireBindingComplete (BindingCompleteContext context, Exception exc, string error_message)
		{
			BindingCompleteEventArgs args = new BindingCompleteEventArgs (this, 
					exc == null ? BindingCompleteState.Success : BindingCompleteState.Exception,
					context);
			if (exc != null) {
				args.SetException (exc);
				args.SetErrorText (error_message);
			}

			OnBindingComplete (args);
		}
#endif

		#region Events
		public event ConvertEventHandler Format;
		public event ConvertEventHandler Parse;
#if NET_2_0
		public event BindingCompleteEventHandler BindingComplete;
#endif
		#endregion	// Events
	}
}
