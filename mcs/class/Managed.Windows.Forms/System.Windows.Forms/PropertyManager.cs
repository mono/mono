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
// Copyright (c) 2005 Novell, Inc.
//
// Authors:
//	Jackson Harper	jackson@ximian.com

using System;
using System.Collections;
using System.ComponentModel;

namespace System.Windows.Forms {

	public class PropertyManager : BindingManagerBase {

		private object data_source;
		private string property_name;
		private PropertyDescriptor prop_desc;
		private bool binding_suspended;

		internal PropertyManager (object data_source, string property_name)
		{
			this.data_source = data_source;
			this.property_name = property_name;

			prop_desc = TypeDescriptor.GetProperties (data_source).Find (property_name, true);

			if (prop_desc == null)
				return;

			prop_desc.AddValueChanged (data_source, new EventHandler (PropertyChangedHandler));
		}

		public override object Current {
			get { return data_source; }
		}

		public override int Position {
			get { return 0; }
			set { /* Doesn't do anything on MS" */ }
		}

		public override int Count {
			get { return 1; }
		}

		public override void AddNew ()
		{
			throw new NotSupportedException ("AddNew is not supported for property to property binding");
		}

		public override void CancelCurrentEdit ()
		{
			IEditableObject editable = data_source as IEditableObject;
			if (editable == null)
				return;
			editable.CancelEdit ();

			PushData ();
		}

		public override void EndCurrentEdit ()
		{
			PullData ();

			IEditableObject editable = data_source as IEditableObject;
			if (editable == null)
				return;
			editable.EndEdit ();
		}

		public override PropertyDescriptorCollection GetItemProperties ()
		{
			return TypeDescriptor.GetProperties (data_source);
		}

		public override void RemoveAt (int idx)
		{
			throw new NotSupportedException ("RemoveAt is not supported for property to property binding");
		}

		public override void ResumeBinding ()
		{
			binding_suspended = false;
		}

		public override void SuspendBinding ()
		{
			binding_suspended = true;
		}

		protected internal override string GetListName (ArrayList list)
		{
			return String.Empty;
		}

		[MonoTODO]
		protected override void UpdateIsBinding ()
		{
		}

		protected internal override void OnCurrentChanged (EventArgs e)
		{
			PullData ();

			if (onCurrentChangedHandler != null) {
				onCurrentChangedHandler (this, e);
			}
		}

		private void PropertyChangedHandler (object sender, EventArgs e)
		{
			OnCurrentChanged (EventArgs.Empty);
		}
	}
}

