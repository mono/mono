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
using System.Data;
using System.Reflection;
using System.Collections;
using System.ComponentModel;

namespace System.Windows.Forms {
	[DefaultMember("Item")]
	public class CurrencyManager : BindingManagerBase {

		protected Type finalType;
		protected int listposition;

		private IList list;
		private bool binding_suspended;

		internal CurrencyManager (object data_source)
		{			
			if (data_source is IListSource) {
				list = ((IListSource) data_source).GetList ();
			} else if (data_source is IList) {
				list = (IList) data_source;
			} else {
				throw new Exception ("Attempted to create currency manager " +
					"from invalid type: " + data_source.GetType ());
			}

			if (data_source as ArrayList != null) {
				finalType = ((ArrayList)data_source).GetType ();
			} else {
				if (data_source as Array != null) {
					finalType = ((Array) data_source).GetType ();
				} else {
					finalType = null;
				}
			}

			DataTable table = data_source as DataTable;
			if (table == null && data_source is DataView)
				table = ((DataView) data_source).Table;

			if (table != null) {
				table.Columns.CollectionChanged  += new CollectionChangeEventHandler (MetaDataChangedHandler);
				table.ChildRelations.CollectionChanged  += new CollectionChangeEventHandler (MetaDataChangedHandler);
				table.ParentRelations.CollectionChanged  += new CollectionChangeEventHandler (MetaDataChangedHandler);
				table.Constraints.CollectionChanged += new CollectionChangeEventHandler (MetaDataChangedHandler);
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
		
		internal string ListName {
			get {
				ITypedList typed = list as ITypedList;
				
				if (typed == null) {
					return finalType.Name;
				} else {
					return typed.GetListName (null);
				}
			}		
		}

		public override PropertyDescriptorCollection GetItemProperties ()
		{
			ITypedList typed = list as ITypedList;

			if (list is Array) {
				Type element = list.GetType ().GetElementType ();
				return TypeDescriptor.GetProperties (element);
			}

			if (typed != null) {
				return typed.GetItemProperties (null);
			}

			PropertyInfo [] props = finalType.GetProperties ();
			for (int i = 0; i < props.Length; i++) {
				if (props [i].Name == "Item") {
					Type t = props [i].PropertyType;
					if (t == typeof (object))
						continue;
					return GetBrowsableProperties (t);
				}
			}

			if (list.Count > 0) {
				return GetBrowsableProperties (list [0].GetType ());
			}
			
			return new PropertyDescriptorCollection (null);
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
                
                internal bool CanAddRows {
                	get {
				if (list as IBindingList == null) {
					return false;
				}
				
				return true;
			}
		}

		public override void AddNew ()
		{
			if (list as IBindingList == null)
				throw new NotSupportedException ();
				
			(list as IBindingList).AddNew ();
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

		public void Refresh ()
		{
			PullData ();
		}

		[MonoTODO ("This is just a guess, as I can't figure out how to test this method")]
		protected void CheckEmpty ()
		{
			if (list == null || list.Count < 1)
				throw new Exception ("List is empty.");
				
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
		
		internal object GetItem (int index)
		{
			return list [index];
		}		
		
		private PropertyDescriptorCollection GetBrowsableProperties (Type t)
		{
			Attribute [] att = new System.Attribute [1];
			att [0] = new BrowsableAttribute (true);
			return TypeDescriptor.GetProperties (t, att);
		}

		private void MetaDataChangedHandler (object sender, CollectionChangeEventArgs e)
		{
			if (MetaDataChanged != null)
				MetaDataChanged (this, EventArgs.Empty);
		}

		public event ItemChangedEventHandler ItemChanged;
		public event EventHandler MetaDataChanged;
	}
}

