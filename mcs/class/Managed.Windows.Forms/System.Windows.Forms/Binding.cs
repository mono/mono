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
		private BindingMemberInfo binding_member_info;
		private Control control;

		private BindingManagerBase manager;
		private PropertyDescriptor prop_desc;
		private object data;

		private EventDescriptor changed_event;
		private EventHandler property_value_changed_handler;
		private object event_current; // The manager.Current as far as the changed_event knows

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

		[MonoTODO]
		public bool IsBinding {
			get {
				return false;
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

			prop_desc = TypeDescriptor.GetProperties (control).Find (property_name, false);

			if (prop_desc == null)
				throw new ArgumentException (String.Concat ("Cannot bind to property '", property_name, "' on target control."));
			if (prop_desc.IsReadOnly)
				throw new ArgumentException (String.Concat ("Cannot bind to property '", property_name, "' because it is read only."));
			this.control = control;
		}

		internal void Check (BindingContext binding_context)
		{
			if (control == null)
				return;

			manager = control.BindingContext [data_source, property_name];
			manager.AddBinding (this);

			WirePropertyValueChangedEvent ();

			PullData ();
			PushData ();
		}

		internal void PushData ()
		{
			prop_desc.SetValue (control, data);
		}

		internal void PullData ()
		{
			PropertyDescriptor pd = TypeDescriptor.GetProperties (manager.Current).Find (data_member, true);
			data = pd.GetValue (manager.Current);
		}

		internal void UpdateIsBinding ()
		{
			PushData ();
		}

		private void CurrentChangedHandler ()
		{
			if (changed_event != null) {
				changed_event.RemoveEventHandler (event_current, property_value_changed_handler);
				WirePropertyValueChangedEvent ();
			}
		}

		private void WirePropertyValueChangedEvent ()
		{
			EventDescriptor changed_event = TypeDescriptor.GetEvents (manager.Current).Find (property_name + "Changed", false);
			if (changed_event == null)
				return;
			property_value_changed_handler = new EventHandler (PropertyValueChanged);
			changed_event.AddEventHandler (manager.Current, property_value_changed_handler);

			event_current = manager.Current;
		}

		private void PropertyValueChanged (object sender, EventArgs e)
		{
			PullData ();
			PushData ();
		}

		#region Events
		public event ConvertEventHandler Format;
		public event ConvertEventHandler Parse;
		#endregion	// Events
	}
}
