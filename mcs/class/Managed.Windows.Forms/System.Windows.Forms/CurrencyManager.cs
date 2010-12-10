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
// 	Ivan N. Zlatev (contact@i-nz.net)
//

using System;
using System.Data;
using System.Reflection;
using System.Collections;
using System.ComponentModel;

namespace System.Windows.Forms {
	public class CurrencyManager : BindingManagerBase {

		protected int listposition;
		protected Type finalType;

		private IList list;
		private bool binding_suspended;

		private object data_source;

		bool editing;

		internal CurrencyManager ()
		{
		}

		internal CurrencyManager (object data_source)
		{
			SetDataSource (data_source);
		}

		public IList List {
			get { return list; }
		}

		public override object Current {
			get {
				if (listposition == -1 || listposition >= list.Count) {
					// Console.WriteLine ("throwing exception from here");
					// Console.WriteLine (Environment.StackTrace);
					throw new IndexOutOfRangeException ("list position");
				}
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
				if (value >= list.Count)
					value = list.Count - 1;
				if (listposition == value)
					return;

				if (listposition != -1)
					EndCurrentEdit ();

				listposition = value;
				OnCurrentChanged (EventArgs.Empty);
				OnPositionChanged (EventArgs.Empty);
			}
		}

		internal void SetDataSource (object data_source)
		{
			if (this.data_source is IBindingList)
				((IBindingList)this.data_source).ListChanged -= new ListChangedEventHandler (ListChangedHandler);

			if (data_source is IListSource)
				data_source = ((IListSource)data_source).GetList();

			this.data_source = data_source;
			if (data_source != null)
				this.finalType = data_source.GetType();

			listposition = -1;
			if (this.data_source is IBindingList)
				((IBindingList)this.data_source).ListChanged += new ListChangedEventHandler (ListChangedHandler);

			list = (IList)data_source;

			// XXX this is wrong.  MS invokes OnItemChanged directly, which seems to call PushData.
			ListChangedHandler (null, new ListChangedEventArgs (ListChangedType.Reset, -1));
		}

		public override PropertyDescriptorCollection GetItemProperties ()
		{
			return ListBindingHelper.GetListItemProperties (list);
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
                        get {
				// Always return true if we don't have items
				if (Count == 0)
					return true;

				return binding_suspended;
			}
                }

                internal bool AllowNew {
                	get {
				if (list is IBindingList)
					return ((IBindingList)list).AllowNew;

				if (list.IsReadOnly)
					return false;

				return false;
			}
		}

		internal bool AllowRemove {
			get {
				if (list.IsReadOnly)
					return false;

				if (list is IBindingList)
					return ((IBindingList)list).AllowRemove;

				return false;
			}
		}

		internal bool AllowEdit {
			get {
				if (list is IBindingList)
					return ((IBindingList)list).AllowEdit;

				return false;
			}
		}

		public override void AddNew ()
		{
			IBindingList ibl = list as IBindingList;

			if (ibl == null)
				throw new NotSupportedException ();

			ibl.AddNew ();

			bool validate = (Position != (list.Count - 1));
			ChangeRecordState (list.Count - 1, validate, validate, true, true);
		}


		void BeginEdit ()
		{
			IEditableObject editable = Current as IEditableObject;

			if (editable != null) {
				try {
					editable.BeginEdit ();
					editing = true;
				}
				catch {
					/* swallow exceptions in IEditableObject.BeginEdit () */
				}
			}
		}

		public override void CancelCurrentEdit ()
		{
			if (listposition == -1)
				return;

			IEditableObject editable = Current as IEditableObject;

			if (editable != null) {
				editing = false;
				editable.CancelEdit ();
				OnItemChanged (new ItemChangedEventArgs (Position));
			}
#if NET_2_0
			if (list is ICancelAddNew)
				((ICancelAddNew)list).CancelNew (listposition);
#endif

		}
		
		public override void EndCurrentEdit ()
		{
			if (listposition == -1)
				return;

			IEditableObject editable = Current as IEditableObject;

			if (editable != null) {
				editing = false;
				editable.EndEdit ();
			}

#if NET_2_0
			if (list is ICancelAddNew)
				((ICancelAddNew)list).EndNew (listposition);
#endif
		}

		public void Refresh ()
		{
			ListChangedHandler (null, new ListChangedEventArgs (ListChangedType.Reset, -1));
		}

		protected void CheckEmpty ()
		{
			if (list == null || list.Count < 1)
				throw new Exception ("List is empty.");
				
		}

		protected internal override void OnCurrentChanged (EventArgs e)
		{
			if (onCurrentChangedHandler != null) {
				onCurrentChangedHandler (this, e);
			}

#if NET_2_0
			// don't call OnCurrentItemChanged here, as it can be overridden
			if (onCurrentItemChangedHandler != null) {
				onCurrentItemChangedHandler (this, e);
			}
#endif

		}

#if NET_2_0
		protected override void OnCurrentItemChanged (EventArgs e)
		{
			if (onCurrentItemChangedHandler != null) {
				onCurrentItemChangedHandler (this, e);
			}
		}
#endif

		protected virtual void OnItemChanged (ItemChangedEventArgs e)
		{
			if (ItemChanged != null)
				ItemChanged (this, e);

			transfering_data = true;
			PushData ();
			transfering_data = false;
		}

#if NET_2_0
		void OnListChanged (ListChangedEventArgs args)
		{
			if (ListChanged != null)
				ListChanged (this, args);
		}
#endif

		protected virtual void OnPositionChanged (EventArgs e)
		{
			if (onPositionChangedHandler != null)
				onPositionChangedHandler (this, e);
		}

		protected internal override string GetListName (ArrayList listAccessors)
		{
			if (list is ITypedList) {
				PropertyDescriptor [] pds = null;
				if (listAccessors != null) {
					pds = new PropertyDescriptor [listAccessors.Count];
					listAccessors.CopyTo (pds, 0);
				}
				return ((ITypedList) list).GetListName (pds);
			}
			else if (finalType != null) {
				return finalType.Name;
			}
			return String.Empty;
		}

		protected override void UpdateIsBinding ()
		{
			UpdateItem ();

			foreach (Binding binding in Bindings)
				binding.UpdateIsBinding ();

			ChangeRecordState (listposition, false, false, true, false);

			OnItemChanged (new ItemChangedEventArgs (-1));
		}

		private void ChangeRecordState (int newPosition,
						bool validating,
						bool endCurrentEdit,
						bool firePositionChanged,
						bool pullData)
		{
			if (endCurrentEdit)
				EndCurrentEdit ();

			int old_index = listposition;

			listposition = newPosition;

			if (listposition >= list.Count)
				listposition = list.Count - 1;

			if (old_index != -1 && listposition != -1)
				OnCurrentChanged (EventArgs.Empty);

			if (firePositionChanged)
				OnPositionChanged (EventArgs.Empty);
		}

		private void UpdateItem ()
		{
			// Probably should be validating or something here
			if (!transfering_data && listposition == -1 && list.Count > 0) {
				listposition = 0;
				BeginEdit ();
			}
		}
		
		internal object this [int index] {
			get { return list [index]; }
		}		
		
		private PropertyDescriptorCollection GetBrowsableProperties (Type t)
		{
			Attribute [] att = new System.Attribute [1];
			att [0] = new BrowsableAttribute (true);
			return TypeDescriptor.GetProperties (t, att);
		}

#if NET_2_0
		protected
#else
		private
#endif
		void OnMetaDataChanged (EventArgs e)
		{
			if (MetaDataChanged != null)
				MetaDataChanged (this, e);
		}

		private void ListChangedHandler (object sender, ListChangedEventArgs e)
		{
			switch (e.ListChangedType) {
			case ListChangedType.PropertyDescriptorAdded:
			case ListChangedType.PropertyDescriptorDeleted:
			case ListChangedType.PropertyDescriptorChanged:
				OnMetaDataChanged (EventArgs.Empty);
#if NET_2_0
				OnListChanged (e);
#endif
				break;
			case ListChangedType.ItemDeleted:
				if (list.Count == 0) {
					/* the last row was deleted */
					listposition = -1;
					UpdateIsBinding ();

#if NET_2_0
					OnPositionChanged (EventArgs.Empty);
					OnCurrentChanged (EventArgs.Empty);
#endif
				}
				else if (e.NewIndex <= listposition) {
					/* the deleted row was either the current one, or one earlier in the list.
					   Update the index and emit PositionChanged, CurrentChanged, and ItemChanged. */
					ChangeRecordState (e.NewIndex,
							   false, false, e.NewIndex != listposition, false);
				}
				else {
					/* the deleted row was after the current one, so we don't
					   need to update bound controls for Position/Current changed */
				}

				OnItemChanged (new ItemChangedEventArgs (-1));
#if NET_2_0
				OnListChanged (e);
#endif
				break;
			case ListChangedType.ItemAdded:
				if (list.Count == 1) {
					/* it's the first one we've added */
					ChangeRecordState (e.NewIndex,
							   false, false, true, false);

#if ONLY_1_1
					UpdateIsBinding ();
#else
					OnItemChanged (new ItemChangedEventArgs (-1));
					OnListChanged (e);
#endif					 	
				}
				else {
#if NET_2_0
					if (e.NewIndex <= listposition) {
						ChangeRecordState (e.NewIndex,
								   false, false, false, false);
						OnItemChanged (new ItemChangedEventArgs (-1));
						OnListChanged (e);
						OnPositionChanged (EventArgs.Empty);
					}
					else {
						OnItemChanged (new ItemChangedEventArgs (-1));
						OnListChanged (e);
					}
#else
					OnItemChanged (new ItemChangedEventArgs (-1));
#endif
				}

				break;
			case ListChangedType.ItemChanged:
				if (editing) {
#if NET_2_0
					if (e.NewIndex == listposition)
						OnCurrentItemChanged (EventArgs.Empty);
#endif
					OnItemChanged (new ItemChangedEventArgs (e.NewIndex));
				}
#if NET_2_0
				OnListChanged (e);
#endif					 	
				break;
			case ListChangedType.Reset:
				PushData();
				UpdateIsBinding();
#if NET_2_0	
				OnListChanged (e);
#endif
				break;
			default:
#if NET_2_0
				OnListChanged (e);
#endif
				break;
			}
		}

#if NET_2_0
		public event ListChangedEventHandler ListChanged;
#endif
		public event ItemChangedEventHandler ItemChanged;
		public event EventHandler MetaDataChanged;
	}
}

