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
//	Jackson Harper (jackson@ximian.com)


using System;
using System.Collections;
using System.ComponentModel;
using System.Reflection;


namespace System.Windows.Forms {
	[DefaultEvent("CollectionChanged")]
	[Editor("System.Drawing.Design.UITypeEditor, " + Consts.AssemblySystem_Drawing, typeof(System.Drawing.Design.UITypeEditor))]
	[TypeConverter("System.Windows.Forms.Design.ControlBindingsConverter, " + Consts.AssemblySystem_Design)]
	public class ControlBindingsCollection : BindingsCollection {
		#region	Fields
		private Control control;
		private IBindableComponent bindable_component;
		private DataSourceUpdateMode default_datasource_update_mode;
		#endregion	// Fields

		#region Constructors
		internal ControlBindingsCollection (Control control) {
			this.control = control;
			bindable_component = control as IBindableComponent;
			default_datasource_update_mode = DataSourceUpdateMode.OnValidation;
		}

		public ControlBindingsCollection (IBindableComponent control)
		{
			bindable_component = control;
			control = control as Control;
			default_datasource_update_mode = DataSourceUpdateMode.OnValidation;
		}
		#endregion	// Constructors

		#region Public Instance Properties
		public Control Control {
			get {
				return control;
			}
		}

		public Binding this[string propertyName] {
			get {
				foreach (Binding b in base.List) {
					if (b.PropertyName == propertyName) {
						return b;
					}
				}
				return null;
			}
		}

		public IBindableComponent BindableComponent {
			get {
				return bindable_component;
			}
		}

		public DataSourceUpdateMode DefaultDataSourceUpdateMode {
			get {
				return default_datasource_update_mode;
			}
			set {
				default_datasource_update_mode = value;
			}
		}
		#endregion	// Public Instance Properties

		#region Public Instance Methods
		public new void Add (Binding binding)
		{
			AddCore (binding);
			OnCollectionChanged (new CollectionChangeEventArgs (CollectionChangeAction.Add, binding));
		}

		public Binding Add (string propertyName, object dataSource, string dataMember)
		{
			if (dataSource == null)
				throw new ArgumentNullException ("dataSource");

			Binding res = new Binding (propertyName, dataSource, dataMember);
			res.DataSourceUpdateMode = default_datasource_update_mode;
			Add (res);
			return res;
		}

		public Binding Add (string propertyName, object dataSource, string dataMember, bool formattingEnabled)
		{
			return Add (propertyName, dataSource, dataMember, formattingEnabled, default_datasource_update_mode, null, String.Empty, null);
		}

		public Binding Add (string propertyName, object dataSource, string dataMember, bool formattingEnabled, DataSourceUpdateMode updateMode)
		{
			return Add (propertyName, dataSource, dataMember, formattingEnabled, updateMode, null, String.Empty, null);
		}

		public Binding Add (string propertyName, object dataSource, string dataMember, bool formattingEnabled, DataSourceUpdateMode updateMode,
			object nullValue)
		{
			return Add (propertyName, dataSource, dataMember, formattingEnabled, updateMode, nullValue, String.Empty, null);
		}

		public Binding Add (string propertyName, object dataSource, string dataMember, bool formattingEnabled, DataSourceUpdateMode updateMode,
			object nullValue, string formatString)
		{
			return Add (propertyName, dataSource, dataMember, formattingEnabled, updateMode, nullValue, formatString, null);
		}

		public Binding Add (string propertyName, object dataSource, string dataMember, bool formattingEnabled, DataSourceUpdateMode updateMode,
			object nullValue, string formatString, IFormatProvider formatInfo)
		{
			if (dataSource == null)
				throw new ArgumentNullException ("dataSource");

			Binding res = new Binding (propertyName, dataSource, dataMember);
			res.FormattingEnabled = formattingEnabled;
			res.DataSourceUpdateMode = updateMode;
			res.NullValue = nullValue;
			res.FormatString = formatString;
			res.FormatInfo = formatInfo;

			Add (res);
			return res;
		}

		public new void Clear() {
			base.Clear();
		}

		public new void Remove (Binding binding) {
			if (binding == null)
				throw new NullReferenceException("The binding is null");

			base.Remove(binding);
		}

		public new void RemoveAt(int index) {
			if (index < 0 || index >= base.List.Count)
				throw new ArgumentOutOfRangeException("index");

			base.RemoveAt(index);
		}
		#endregion	// Public Instance Methods

		#region Protected Instance Methods
		protected override void AddCore (Binding dataBinding)
		{
			if (dataBinding == null)
				throw new ArgumentNullException ("dataBinding");

			if (dataBinding.Control != null && dataBinding.BindableComponent != bindable_component)
				throw new ArgumentException ("dataBinding belongs to another BindingsCollection");

			for (int i = 0; i < Count; i++) {
				Binding bnd = this [i];
				if (bnd == null || bnd.PropertyName.Length == 0 || dataBinding.PropertyName.Length == 0) {
					continue;
				}

				if (String.Compare (bnd.PropertyName, dataBinding.PropertyName, true) == 0) {
					throw new ArgumentException ("The binding is already in the collection");
				}
			}

			dataBinding.SetControl (bindable_component);
			dataBinding.Check ();
			base.AddCore (dataBinding);
		}

		protected override void ClearCore() {
			base.ClearCore ();
		}

		protected override void RemoveCore (Binding dataBinding) {
			if (dataBinding == null)
				throw new ArgumentNullException ("dataBinding");

			base.RemoveCore (dataBinding);
		}
		#endregion	// Protected Instance Methods
	}
}

