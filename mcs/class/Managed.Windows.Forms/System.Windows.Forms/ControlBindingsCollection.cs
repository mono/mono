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
	[Editor("System.Drawing.Design.UITypeEditor, System.Drawing, Version=1.0.5000.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(System.Drawing.Design.UITypeEditor))]
	[TypeConverter("System.Windows.Forms.Design.ControlBindingsConverter, System.Design, Version=1.0.5000.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
	public class ControlBindingsCollection : BindingsCollection {
		#region	Fields
		private Control control;
		#endregion	// Fields

		#region Constructors
		internal ControlBindingsCollection (Control control) {
			this.control = control;
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
		#endregion	// Public Instance Properties

		#region Public Instance Methods
		public new void Add (Binding binding)
		{
			AddCore (binding);
			OnCollectionChanged (new CollectionChangeEventArgs (CollectionChangeAction.Add, binding));
		}

		public Binding Add (string property_name, object data_source, string data_member)
		{
			Binding res = new Binding (property_name, data_source, data_member);
			Add (res);
			return res;
		}

		public void Clear() {
			base.Clear();
		}

		public void Remove(Binding binding) {
			if (binding == null) {
				throw new NullReferenceException("The binding is null");
			}

			base.Remove(binding);
		}

		public void RemoveAt(int index) {
			if (index < 0 || index >= base.List.Count) {
				throw new ArgumentOutOfRangeException("index");
			}

			base.RemoveAt(index);
		}
		#endregion	// Public Instance Methods

		#region Protected Instance Methods
		protected override void AddCore (Binding binding)
		{
			if (binding == null)
				throw new ArgumentNullException ("dataBinding");

			if (binding.Control != null && binding.Control != control)
				  throw new ArgumentException ("dataBinding belongs to another BindingsCollection");

			binding.SetControl (control);
			base.AddCore (binding);
		}

		protected override void ClearCore() {
			base.ClearCore ();
		}

		protected override void RemoveCore(Binding dataBinding) {
			if (dataBinding == null) {
				throw new ArgumentNullException ("dataBinding");
			}

			base.RemoveCore (dataBinding);
		}
		#endregion	// Protected Instance Methods
	}
}

