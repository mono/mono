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
//	Jackson Harper (jackson@ximian.com)
//

using System;
using System.Collections;
using System.ComponentModel;

namespace System.Windows.Forms {

	public class CurrencyManager : BindingManagerBase {

		protected Type finalType;
		protected int listposition;

		private IList list;
		private IBindingList binding_list;

		private bool binding_suspended;

		internal CurrencyManager (object data_source)
		{
			binding_list = data_source as IBindingList;
			if (data_source is IListSource) {
				list = ((IListSource) data_source).GetList ();
			} else if (data_source is IList) {
				list = (IList) data_source;
			} else {
				throw new Exception ("Attempted to create currency manager " +
					"from invalid type: " + data_source.GetType ());
			}
		}		

		public IList List {
			get { return list; }
		}

		public override object Current {
			get {
				return list [listposition];
			}
		}

		public override int Count {
			get { return list.Count; }
		}

		public override int Position {
			get {
				return listposition;
			} 
			set {
				if (value < 0)
					value = 0;
				if (value == list.Count)
					value = list.Count - 1;
				if (listposition == value)
					return;
				listposition = value;
				OnCurrentChanged (EventArgs.Empty);
				OnPositionChanged (EventArgs.Empty);
			}
		}

		public override PropertyDescriptorCollection GetItemProperties ()
		{
			ITypedList typed = list as ITypedList;

			if (typed == null)
				return null;
			return typed.GetItemProperties (null);
		}

		public override void RemoveAt (int index)
		{
			list.RemoveAt (index);
		}

		public override void SuspendBinding ()
		{
			binding_suspended = true;
		}
		
		public override void ResumeBinding ()
		{
			binding_suspended = false;
		}

                internal override bool IsSuspended {
                        get { return binding_suspended; }
                }

		public override void AddNew ()
		{
			if (binding_list == null)
				throw new NotSupportedException ();
			binding_list.AddNew ();
		}

		public override void CancelCurrentEdit ()
		{
			IEditableObject editable = Current as IEditableObject;

			if (editable == null)
				return;
			editable.CancelEdit ();
			OnItemChanged (new ItemChangedEventArgs (Position));
		}
		
		public override void EndCurrentEdit ()
		{
			IEditableObject editable = Current as IEditableObject;

			if (editable == null)
				return;
			editable.EndEdit ();
		}

		protected internal override void OnCurrentChanged (EventArgs e)
		{
			PullData ();

			if (onCurrentChangedHandler != null) {
				onCurrentChangedHandler (this, e);
			}
		}

		protected virtual void OnItemChanged (ItemChangedEventArgs e)
		{
			PushData ();

			if (ItemChanged != null)
				ItemChanged (this, e);
		}

		protected virtual void OnPositionChanged (EventArgs e)
		{
			if (onPositionChangedHandler == null)
				return;
			onPositionChangedHandler (this, e);
		}

		protected internal override string GetListName (ArrayList accessors)
		{
			if (list is ITypedList) {
				PropertyDescriptor [] pds;
				pds = new PropertyDescriptor [accessors.Count];
				accessors.CopyTo (pds, 0);
				return ((ITypedList) list).GetListName (pds);
			}
			return String.Empty;
		}

		[MonoTODO ("Not totally sure how this works, its doesn't seemt to do a pull/push like i originally assumed")]
		protected override void UpdateIsBinding ()
		{
			UpdateItem ();

			foreach (Binding binding in Bindings)
				binding.UpdateIsBinding ();
		}

		private void UpdateItem ()
		{
			// Probably should be validating or something here
			EndCurrentEdit ();
		}

		public event ItemChangedEventHandler ItemChanged;
	}
}

