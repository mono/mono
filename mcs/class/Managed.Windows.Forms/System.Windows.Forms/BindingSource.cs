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
// Copyright (c) 2007 Novell, Inc.
//

#if NET_2_0

using System.ComponentModel;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace System.Windows.Forms {

	[ComplexBindingProperties ("DataSource", "DataMember")]
	[DefaultEvent ("CurrentChanged")]
	[DefaultProperty ("DataSource")]
	[Designer("System.Windows.Forms.Design.BindingSourceDesigner, " + Consts.AssemblySystem_Design, "System.ComponentModel.Design.IDesigner")]
	public class BindingSource : Component,
		ICancelAddNew, IDisposable, ISupportInitialize,
		IBindingListView, IBindingList, ITypedList,
		IList, ISupportInitializeNotification, ICollection,
		IComponent, ICurrencyManagerProvider, IEnumerable 
	{
		ISite site;

		IList list;
		CurrencyManager currency_manager;
		Dictionary<string,CurrencyManager> related_currency_managers = new Dictionary<string,CurrencyManager> ();
		//bool list_defaulted;
		Type item_type;
		bool item_has_default_ctor;
		bool list_is_ibinding;

		object datasource;
		string datamember;

		bool raise_list_changed_events;

		bool allow_new_set;
		bool allow_new;

		bool add_pending;
		int pending_add_index;

		public BindingSource (IContainer container) : this ()
		{
			container.Add (this);
		}

		public BindingSource (object dataSource, string dataMember)
		{
			datasource = dataSource;
			datamember = dataMember;

			raise_list_changed_events = true;

			ResetList ();
			ConnectCurrencyManager ();
		}

		public BindingSource () : this (null, String.Empty)
		{
		}

		IList GetListFromEnumerable (IEnumerable enumerable)
		{
			IList l = null;

			IEnumerator e = enumerable.GetEnumerator();

			if (enumerable is string) {
				/* special case this.. seems to be the only one .net special cases? */
				l = new BindingList<char> ();
			}
			else {
				/* try to figure out the type based on
				 * the first element, if there is
				 * one */
				object first = null;
				if (e.MoveNext ()) {
					first = e.Current;
				}

				if (first == null) {
					return null;
				}
				else {
					Type t = typeof (BindingList<>).MakeGenericType (new Type[] { first.GetType() });
					l = (IList)Activator.CreateInstance (t);
				}
			}

			e.Reset ();
			while (e.MoveNext ()) {
				l.Add (e.Current);
			}

			return l;
		}

		void ConnectCurrencyManager ()
		{
			currency_manager = new CurrencyManager (this);

			currency_manager.PositionChanged += delegate (object o, EventArgs args) { OnPositionChanged (args); };
			currency_manager.CurrentChanged += delegate (object o, EventArgs args) { OnCurrentChanged (args); };
			currency_manager.BindingComplete += delegate (object o, BindingCompleteEventArgs args) { OnBindingComplete (args); };
			currency_manager.DataError += delegate (object o, BindingManagerDataErrorEventArgs args) { OnDataError (args); };
			currency_manager.CurrentChanged += delegate (object o, EventArgs args) { OnCurrentChanged (args); };
			currency_manager.CurrentItemChanged += delegate (object o, EventArgs args) { OnCurrentItemChanged (args); };
		}

		void ResetList ()
		{
			IList l;
			object source = ListBindingHelper.GetList (datasource, datamember);

			// 
			// If original source is null, then create a new object list
			// Otherwise, try to infer the list item type
			//

			if (datasource == null) {
				l = new BindingList<object>();
				//list_defaulted = true;
			} else if (source == null) {
				//Infer type based on datasource and datamember,
				// where datasource is an empty IEnumerable
				// and need to find out the datamember type

				Type property_type = ListBindingHelper.GetListItemProperties (datasource) [datamember].PropertyType;
				Type t = typeof (BindingList<>).MakeGenericType (new Type [] { property_type } );
				l = (IList)Activator.CreateInstance (t);
			} else if (source is IList) {
				l = (IList)source;
			} else if (source is IEnumerable) {
				IList new_list = GetListFromEnumerable ((IEnumerable)source);
				l = new_list == null ? list : new_list;
			} else if (source is Type) {
				Type t = typeof (BindingList<>).MakeGenericType (new Type [] { (Type)source });
				l = (IList)Activator.CreateInstance (t);
			} else {
				Type t = typeof (BindingList<>).MakeGenericType (new Type[] { source.GetType() });
				l = (IList)Activator.CreateInstance (t);
				l.Add (source);
			}

			SetList (l);
		}

		void SetList (IList l)
		{
			if (list is IBindingList)
				((IBindingList) list).ListChanged -= IBindingListChangedHandler;

			list = l;
			item_type = ListBindingHelper.GetListItemType (list);
			item_has_default_ctor = item_type.GetConstructor (Type.EmptyTypes) != null;

			list_is_ibinding = list is IBindingList;
			if (list_is_ibinding)
				((IBindingList) list).ListChanged += IBindingListChangedHandler;

			if (raise_list_changed_events)
				OnListChanged (new ListChangedEventArgs (ListChangedType.Reset, -1));
		}

		void IBindingListChangedHandler (object o, ListChangedEventArgs args)
		{
			if (raise_list_changed_events)
				OnListChanged (args);
		}

		[Browsable (false)]
		public virtual bool AllowEdit {
			get {
				if (list == null)
					return false;

				if (list.IsReadOnly)
					return false;

				if (list is IBindingList)
					return ((IBindingList)list).AllowEdit;

				return true;
			}
		}

		public virtual bool AllowNew {
			get { 
				if (allow_new_set)
					return allow_new;

				if (list is IBindingList)
					return ((IBindingList)list).AllowNew;

				if (list.IsFixedSize || list.IsReadOnly || !item_has_default_ctor)
					return false;

				return true;
			}
			set {
				if (value == allow_new && allow_new_set)
					return;

				if (value && (list.IsReadOnly || list.IsFixedSize))
					throw new InvalidOperationException ();

				allow_new_set = true;
				allow_new = value;

				if (raise_list_changed_events)
					OnListChanged (new ListChangedEventArgs (ListChangedType.Reset, -1));
			}
		}

		bool IsAddingNewHandled {
			get {
				return Events [AddingNewEvent] != null;
			}
		}

		[Browsable (false)]
		public virtual bool AllowRemove {
			get {
				if (list == null)
					return false;

				if (list.IsFixedSize || list.IsReadOnly)
					return false;

				if (list is IBindingList)
					return ((IBindingList)list).AllowRemove;

				return true;
			}
		}

		[Browsable (false)]
		public virtual int Count {
			get {
				return list.Count;
			}
		}

		[Browsable (false)]
		public virtual CurrencyManager CurrencyManager {
			get {
				return currency_manager;
			}
		}

		[Browsable (false)]
		public object Current {
			get {
				return currency_manager.Current;
			}
		}

		[DefaultValue ("")]
		[Editor("System.Windows.Forms.Design.DataMemberListEditor, " + Consts.AssemblySystem_Design, typeof(System.Drawing.Design.UITypeEditor))]
		[RefreshProperties (RefreshProperties.Repaint)]
		public string DataMember {
			get { return datamember; }
			set {
				/* we don't allow null DataMembers */
				if (value == null)
					value = String.Empty;

				if (datamember != value) {
					this.datamember = value;

					ResetList ();

					OnDataMemberChanged (EventArgs.Empty);
				}
			}
		}

		[AttributeProvider (typeof(IListSource))]
		[RefreshProperties (RefreshProperties.Repaint)]
		[DefaultValue (null)]
		public object DataSource {
			get { return datasource; }
			set {
				if (datasource != value) {
					this.datasource = value;
					datamember = String.Empty;

					ResetList ();

					OnDataSourceChanged (EventArgs.Empty);
				}
			}
		}

		[DefaultValue (null)]
		public virtual string Filter {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		[Browsable (false)]
		public bool IsBindingSuspended {
			get { return currency_manager.IsBindingSuspended; }
		}

		[Browsable (false)]
		public virtual bool IsFixedSize {
			get { return list.IsFixedSize; }
		}

		[Browsable (false)]
		public virtual bool IsReadOnly {
			get { return list.IsReadOnly; }
		}

		[Browsable (false)]
		public virtual bool IsSorted {
			get { return (list is IBindingList) && ((IBindingList)list).IsSorted; }
		}

		[Browsable (false)]
		public virtual bool IsSynchronized {
			get { throw new NotImplementedException (); }
		}

		[Browsable (false)]
		public virtual object this [int index] {
			get { return list[index]; }
			set
			{
				list[index] = value;
			}
		}

		[Browsable (false)]
		public IList List {
			get { return list; }
		}

		[DefaultValue (-1)]
		[Browsable (false)]
		public int Position {
			get {
				return currency_manager.Position;
			}
			set {
				if (value >= Count) value = Count - 1;
				if (value < 0) value = 0;

				currency_manager.Position = value;
			}
		}

		[Browsable (false)]
		[DefaultValue (true)]
		public bool RaiseListChangedEvents {
			get { return raise_list_changed_events; }
			set { raise_list_changed_events = value; }
		}

		[DefaultValue (null)]
		public string Sort {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public virtual ListSortDescriptionCollection SortDescriptions {
			get {
				if (list is IBindingListView)
					return ((IBindingListView)list).SortDescriptions;

				return null;
			}
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public virtual ListSortDirection SortDirection {
			get {
				if (list is IBindingList)
					return ((IBindingList)list).SortDirection;

				return ListSortDirection.Ascending;
			}
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public virtual PropertyDescriptor SortProperty {
			get {
				if (list is IBindingList)
					return ((IBindingList)list).SortProperty;

				return null;
			}
		}

		[Browsable (false)]
		public virtual bool SupportsAdvancedSorting {
			get { return (list is IBindingListView) && ((IBindingListView)list).SupportsAdvancedSorting; }
		}

		[Browsable (false)]
		public virtual bool SupportsChangeNotification {
			get { return true; }
		}

		[Browsable (false)]
		public virtual bool SupportsFiltering {
			get { return (list is IBindingListView) && ((IBindingListView)list).SupportsFiltering; }
		}

		[Browsable (false)]
		public virtual bool SupportsSearching {
			get { 
				return (list is IBindingList) && ((IBindingList)list).SupportsSearching;
			}
		}

		[Browsable (false)]
		public virtual bool SupportsSorting {
			get { return (list is IBindingList) && ((IBindingList)list).SupportsSorting; }
		}

		[Browsable (false)]
		public virtual object SyncRoot {
			get { 
				return list.SyncRoot;
			}
		}

		static object AddingNewEvent = new object ();
		static object BindingCompleteEvent = new object ();
		static object CurrentChangedEvent = new object ();
		static object CurrentItemChangedEvent = new object ();
		static object DataErrorEvent = new object ();
		static object DataMemberChangedEvent = new object ();
		static object DataSourceChangedEvent = new object ();
		static object ListChangedEvent = new object ();
		static object PositionChangedEvent= new object ();

		public event AddingNewEventHandler AddingNew {
			add { Events.AddHandler (AddingNewEvent, value); }
			remove { Events.RemoveHandler (AddingNewEvent, value); }
		}

		public event BindingCompleteEventHandler BindingComplete {
			add { Events.AddHandler (BindingCompleteEvent, value); }
			remove { Events.RemoveHandler (BindingCompleteEvent, value); }
		}

		public event EventHandler CurrentChanged {
			add { Events.AddHandler (CurrentChangedEvent, value); }
			remove { Events.RemoveHandler (CurrentChangedEvent, value); }
		}

		public event EventHandler CurrentItemChanged {
			add { Events.AddHandler (CurrentItemChangedEvent, value); }
			remove { Events.RemoveHandler (CurrentItemChangedEvent, value); }
		}

		public event BindingManagerDataErrorEventHandler DataError {
			add { Events.AddHandler (DataErrorEvent, value); }
			remove { Events.RemoveHandler (DataErrorEvent, value); }
		}

		public event EventHandler DataMemberChanged {
			add { Events.AddHandler (DataMemberChangedEvent, value); }
			remove { Events.RemoveHandler (DataMemberChangedEvent, value); }
		}

		public event EventHandler DataSourceChanged {
			add { Events.AddHandler (DataSourceChangedEvent, value); }
			remove { Events.RemoveHandler (DataSourceChangedEvent, value); }
		}

		public event ListChangedEventHandler ListChanged {
			add { Events.AddHandler (ListChangedEvent, value); }
			remove { Events.RemoveHandler (ListChangedEvent, value); }
		}

		public event EventHandler PositionChanged {
			add { Events.AddHandler (PositionChangedEvent, value); }
			remove { Events.RemoveHandler (PositionChangedEvent, value); }
		}

		public virtual int Add (object value)
		{
			// 
			// First (re)create the BindingList<T> based on value
			// if datasource is null and the current list is empty
			//
			if (datasource == null && list.Count == 0 && value != null) {
				Type t = typeof (BindingList<>).MakeGenericType (new Type [] { value.GetType () } );
				IList l = (IList) Activator.CreateInstance (t);
				SetList (l);
			}

			if (value != null && !item_type.IsAssignableFrom (value.GetType ()))
				throw new InvalidOperationException ("Objects added to the list must all be of the same type.");
			if (list.IsReadOnly)
				throw new NotSupportedException ("Collection is read-only.");
			if (list.IsFixedSize)
				throw new NotSupportedException ("Collection has a fixed size.");

			int idx = list.Add (value);
			if (raise_list_changed_events && !list_is_ibinding)
				OnListChanged (new ListChangedEventArgs (ListChangedType.ItemAdded, idx));

			return idx;
		}

		public virtual object AddNew ()
		{
			if (!AllowEdit)
				throw new InvalidOperationException ("Item cannot be added to a read-only or fixed-size list.");
			if (!AllowNew || (!item_has_default_ctor && !IsAddingNewHandled)) 
				throw new InvalidOperationException ("AddNew cannot be called on '" + item_type.Name +
						", since it does not have a public default ctor. Set AllowNew to true " +
						", handling AddingNew and creating the appropriate object.");

			EndEdit ();

			AddingNewEventArgs args = new AddingNewEventArgs ();
			OnAddingNew (args);

			object new_object = args.NewObject;
			if (new_object != null) {
				if (!item_type.IsAssignableFrom (new_object.GetType ()))
					throw new InvalidOperationException ("Objects added to the list must all be of the same type.");
			} else if (list is IBindingList)
				return ((IBindingList)list).AddNew ();
			else if (!item_has_default_ctor)
				throw new InvalidOperationException ("AddNew cannot be called on '" + item_type.Name +
						", since it does not have a public default ctor. Set AllowNew to true " +
						", handling AddingNew and creating the appropriate object.");
			else // fallback to default .ctor
				new_object = Activator.CreateInstance (item_type);

			int idx = list.Add (new_object);
			if (raise_list_changed_events && !list_is_ibinding)
				OnListChanged (new ListChangedEventArgs (ListChangedType.ItemAdded, idx));

			add_pending = true;
			pending_add_index = idx;

			return new_object;
		}

		[EditorBrowsable (EditorBrowsableState.Never)]
		public virtual void ApplySort (PropertyDescriptor property, ListSortDirection sort)
		{
			throw new NotImplementedException ();
		}

		[EditorBrowsable (EditorBrowsableState.Never)]
		public virtual void ApplySort (ListSortDescriptionCollection sorts)
		{
			throw new NotImplementedException ();
		}

		public void CancelEdit ()
		{
			currency_manager.CancelCurrentEdit ();
		}

		public virtual void Clear ()
		{
			if (list.IsReadOnly)
				throw new NotSupportedException ("Collection is read-only.");

			list.Clear ();
			if (raise_list_changed_events && !list_is_ibinding)
				OnListChanged (new ListChangedEventArgs (ListChangedType.Reset, -1));
		}

		public virtual bool Contains (object value)
		{
			return list.Contains (value);
		}

		public virtual void CopyTo (Array arr, int index)
		{
			list.CopyTo (arr, index);
		}

		protected override void Dispose (bool disposing)
		{
			base.Dispose (disposing);
		}

		public void EndEdit ()
		{
			currency_manager.EndCurrentEdit ();
		}

		public int Find (string propertyName, object key)
		{
			throw new NotImplementedException ();
		}

		public virtual int Find (PropertyDescriptor prop, object key)
		{
			throw new NotImplementedException ();
		}

		public virtual IEnumerator GetEnumerator ()
		{
			return this.List.GetEnumerator ();
		}

		public virtual PropertyDescriptorCollection GetItemProperties (PropertyDescriptor[] listAccessors)
		{
			return ListBindingHelper.GetListItemProperties (list, listAccessors);
		}

		public virtual string GetListName (PropertyDescriptor[] listAccessors)
		{
			return ListBindingHelper.GetListName (list, listAccessors);
		}

		public virtual CurrencyManager GetRelatedCurrencyManager (string dataMember)
		{
			if (dataMember == null || dataMember.Length == 0)
				return currency_manager;

			if (related_currency_managers.ContainsKey (dataMember))
				return related_currency_managers [dataMember];

			// FIXME - Why passing invalid dataMembers containing a . return
			// a null value?
			if (dataMember.IndexOf ('.') != -1)
				return null;

			BindingSource source = new BindingSource (this, dataMember);
			related_currency_managers [dataMember] = source.CurrencyManager;

			return source.CurrencyManager;
		}

		public virtual int IndexOf (object value)
		{
			return list.IndexOf (value);
		}

		public virtual void Insert (int index, object value)
		{
			if (index < 0 || index > list.Count)
				throw new ArgumentOutOfRangeException ("index");
			if (list.IsReadOnly || list.IsFixedSize)
				throw new NotSupportedException ();
			if (!item_type.IsAssignableFrom (value.GetType ()))
				throw new ArgumentException ("value");

			list.Insert (index, value);
			if (raise_list_changed_events && !list_is_ibinding)
				OnListChanged (new ListChangedEventArgs (ListChangedType.ItemAdded, index));
		}

		public void MoveFirst ()
		{
			Position = 0;
		}

		public void MoveLast ()
		{
			Position = Count - 1;
		}

		public void MoveNext ()
		{
			Position ++;
		}

		public void MovePrevious ()
		{
			Position --;
		}

		protected virtual void OnAddingNew (AddingNewEventArgs e)
		{
			AddingNewEventHandler eh = (AddingNewEventHandler)Events[AddingNewEvent];
			if (eh != null)
				eh (this, e);
		}

		protected virtual void OnBindingComplete (BindingCompleteEventArgs e)
		{
			BindingCompleteEventHandler eh = (BindingCompleteEventHandler) Events[BindingCompleteEvent];
			if (eh != null)
				eh (this, e);
		}

		protected virtual void OnCurrentChanged (EventArgs e)
		{
			EventHandler eh = (EventHandler) Events[CurrentChangedEvent];
			if (eh != null)
				eh (this, e);
		}

		protected virtual void OnCurrentItemChanged (EventArgs e)
		{
			EventHandler eh = (EventHandler) Events[CurrentItemChangedEvent];
			if (eh != null)
				eh (this, e);
		}

		protected virtual void OnDataError (BindingManagerDataErrorEventArgs e)
		{
			BindingManagerDataErrorEventHandler eh = (BindingManagerDataErrorEventHandler) Events[DataErrorEvent];
			if (eh != null)
				eh (this, e);
		}

		protected virtual void OnDataMemberChanged (EventArgs e)
		{
			EventHandler eh = (EventHandler) Events[DataMemberChangedEvent];
			if (eh != null)
				eh (this, e);
		}

		protected virtual void OnDataSourceChanged (EventArgs e)
		{
			EventHandler eh = (EventHandler) Events[DataSourceChangedEvent];
			if (eh != null)
				eh (this, e);
		}

		protected virtual void OnListChanged (ListChangedEventArgs e)
		{
			ListChangedEventHandler eh = (ListChangedEventHandler) Events[ListChangedEvent];
			if (eh != null)
				eh (this, e);
		}

		protected virtual void OnPositionChanged (EventArgs e)
		{
			EventHandler eh = (EventHandler) Events[PositionChangedEvent];
			if (eh != null)
				eh (this, e);
		}

		public virtual void Remove (object value)
		{
			if (list.IsReadOnly)
				throw new NotSupportedException ("Collection is read-only.");
			if (list.IsFixedSize)
				throw new NotSupportedException ("Collection has a fixed size.");

			int idx = list_is_ibinding ? - 1 : list.IndexOf (value);
			list.Remove (value);

			if (idx != -1 && raise_list_changed_events)
				OnListChanged (new ListChangedEventArgs (ListChangedType.ItemDeleted, idx));
		}

		public virtual void RemoveAt (int index)
		{
			if (index < 0 || index > list.Count)
				throw new ArgumentOutOfRangeException ("index");
			if (list.IsReadOnly || list.IsFixedSize)
				throw new InvalidOperationException ();

			list.RemoveAt (index);
			if (raise_list_changed_events && !list_is_ibinding)
				OnListChanged (new ListChangedEventArgs (ListChangedType.ItemDeleted, index));
		}

		public void RemoveCurrent ()
		{
			if (Position < 0)
				throw new InvalidOperationException ("Cannot remove item because there is no current item.");
			if (!AllowRemove)
				throw new InvalidOperationException ("Cannot remove item because list does not allow removal of items.");

			RemoveAt (Position);
		}

		public virtual void RemoveFilter ()
		{
			throw new NotImplementedException ();
		}

		public virtual void RemoveSort ()
		{
			throw new NotImplementedException ();
		}

		[EditorBrowsable (EditorBrowsableState.Advanced)]
		public virtual void ResetAllowNew ()
		{
			allow_new_set = false;
		}

		public void ResetBindings (bool metadataChanged)
		{
			if (metadataChanged)
				OnListChanged (new ListChangedEventArgs (ListChangedType.PropertyDescriptorChanged, 0));

			OnListChanged (new ListChangedEventArgs (ListChangedType.Reset, -1, -1));
		}

		public void ResetCurrentItem ()
		{
			OnListChanged (new ListChangedEventArgs (ListChangedType.ItemChanged, Position, -1));
		}

		public void ResetItem (int itemIndex)
		{
			OnListChanged (new ListChangedEventArgs (ListChangedType.ItemChanged, itemIndex, -1));
		}

		public void ResumeBinding ()
		{
			currency_manager.ResumeBinding ();
		}

		public void SuspendBinding ()
		{
			currency_manager.SuspendBinding ();
		}

		/* explicit interface implementations */

		void ICancelAddNew.CancelNew (int position)
		{
			if (!add_pending)
				return;

			if (position != pending_add_index)
				return;

			add_pending = false;
			list.RemoveAt (position);

			if (raise_list_changed_events && !list_is_ibinding)
				OnListChanged (new ListChangedEventArgs (ListChangedType.ItemDeleted, position));
		}

		void ICancelAddNew.EndNew (int position)
		{
			if (!add_pending)
				return;

			if (position != pending_add_index)
				return;

			add_pending = false;
		}
		
		void ISupportInitialize.BeginInit ()
		{
		}

		void ISupportInitialize.EndInit ()
		{
		}

		void IBindingList.AddIndex (PropertyDescriptor property)
		{
			if (!(list is IBindingList))
				throw new NotSupportedException();

			((IBindingList)list).AddIndex (property);
		}

		void IBindingList.RemoveIndex (PropertyDescriptor prop)
		{
			if (!(list is IBindingList))
				throw new NotSupportedException();

			((IBindingList)list).RemoveIndex (prop);
		}

		bool ISupportInitializeNotification.IsInitialized {
			// XXX this is likely wrong, but i can't
			// figure out how to make it return false
			get { return true; }
		}

		static object InitializedEvent = new object ();

		event EventHandler ISupportInitializeNotification.Initialized {
			add { Events.AddHandler (InitializedEvent, value); }
			remove { Events.RemoveHandler (InitializedEvent, value); }
		}
	}
}

#endif
