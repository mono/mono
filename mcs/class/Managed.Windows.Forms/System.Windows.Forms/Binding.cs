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
// Copyright (c) 2004 Novell, Inc.
//
// Authors:
//	Peter Bartok	pbartok@novell.com
//


// COMPLETE

namespace System.Windows.Forms {
	public class Binding {
		internal string			property_name;
		internal object			data_source;
		internal BindingManagerBase	binding_manager_base;
		internal BindingMemberInfo	binding_member_info;
		internal Control		control;
		internal bool			is_binding;

		#region Public Constructors
		public Binding(string propertyName, object dataSource, string dataMember) {
			this.property_name=propertyName;
			this.data_source=dataSource;
			this.binding_member_info=new BindingMemberInfo(dataMember);
		}
		#endregion	// Public Constructors

		#region Public Instance Properties
		public BindingManagerBase BindingManagerBase {
			get {
				return this.binding_manager_base;
			}
		}

		public BindingMemberInfo BindingMemberInfo {
			get {
				return this.binding_member_info;
			}
		}

		public Control Control {
			get {
				return this.control;
			}
		}

		public object DataSource {
			get {
				return this.data_source;
			}
		}

		public bool IsBinding {
			get {
				return this.is_binding;
			}
		}

		public string PropertyName {
			get {
				return this.property_name;
			}
		}
		#endregion	// Public Instance Properties

		#region Protected Instance Methods
		protected virtual void OnFormat(ConvertEventArgs cevent) {
			if (Format!=null) Format(this, cevent);
		}

		protected virtual void OnParse(ConvertEventArgs cevent) {
			if (Parse!=null) Parse(this, cevent);
		}
		#endregion	// Protected Instance Methods

		#region Events
		public event ConvertEventHandler Format;
		public event ConvertEventHandler Parse;
		#endregion	// Events
	}
}
