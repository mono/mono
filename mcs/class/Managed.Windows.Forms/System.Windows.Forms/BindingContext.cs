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


using System.Data;
using System.Collections;
using System.Globalization;
using System.ComponentModel;


namespace System.Windows.Forms {

	[DefaultEvent("CollectionChanged")]
	public class BindingContext : ICollection, IEnumerable {

		private Hashtable managers;
		private EventHandler onCollectionChangedHandler;

		private class HashKey {
			public object source;
			public string member;

			public HashKey (object source, string member)
			{
				this.source = source;
				this.member = member;
			}

			public override int GetHashCode ()
			{
				return source.GetHashCode() + member.GetHashCode ();
			}

			public override bool Equals (object o)
			{
				HashKey hk = o as HashKey;
				if (hk == null)
					return false;
				return hk.source == source && hk.member == member;
			}
		}

		public BindingContext () 
		{
			managers = new Hashtable ();
		}

		public bool IsReadOnly {
			get { return false; }
		}

		public BindingManagerBase this [object dataSource] {
			get { return this [dataSource, String.Empty]; }
		}

		public BindingManagerBase this [object data_source, string data_member] {
			get {
				if (data_source == null)
					throw new ArgumentNullException ("data_source");
				if (data_member == null)
					data_member = String.Empty;

				HashKey key = new HashKey (data_source, data_member);
				BindingManagerBase res = managers [key] as BindingManagerBase;

				if (res != null)
					return res;

				res = CreateBindingManager (data_source, data_member);
				Console.WriteLine ("CREATED: {0}", res);
				if (res == null)
					return null;
				managers [key] = res;
				return res;
			}
		}

		private BindingManagerBase CreateBindingManager (object data_source, string data_member)
		{
#if true
			/* the following is gross and special cased
			   and needs to die.  a more proper
			   implementation would be something like
			   what's down below in the #else section,
			   where the recursion over the nagivation
			   path is at the toplevel.
			*/

			DataTable table = data_source as DataTable;
			if (table == null && data_source is DataView)
				table = ((DataView) data_source).Table;

			DataSet dataset = data_source as DataSet;
			if (table != null) {
				return new CurrencyManager (new DataView (table));
			}
			else if (data_member != "" && dataset != null) {
				BindingMemberInfo info = new BindingMemberInfo (data_member);

				if (info.BindingPath == "") {
					table = dataset.Tables [info.BindingField];
					if (table == null)
						throw new ArgumentException (String.Format ("Specified data member table `{0}' does not exist in the data source DataSet", info.BindingField));

					return new CurrencyManager (new DataView (table));
				}
				else {
					Console.WriteLine ("Getting parent_manager for {0}", info.BindingPath);
					CurrencyManager parent_manager = (CurrencyManager) this[data_source, info.BindingPath];

					table = ((DataView)parent_manager.data_source).Table;

					DataColumn col = table.Columns [info.BindingField];
					DataRelation rel = dataset.Relations [info.BindingField];

					if (col != null) {
						Console.WriteLine ("+ creating related property manager for column {0}", info.BindingField);
						return new RelatedPropertyManager (parent_manager, info.BindingField);
					}
					else if (rel != null) {
						Console.WriteLine ("+ creating related currency manager for relation {0}", info.BindingField);
						return new RelatedCurrencyManager (parent_manager, rel);
					}
					else 
						throw new ArgumentException (String.Format ("Specified data member {0} does not exist in the data table {1}",
											    info.BindingField, table.TableName));

				}
			}
			else if (data_source is IList) {
				IList list = (IList)data_source;

				if (data_member == "") {
					return new CurrencyManager (list);
				}
				else {
					CurrencyManager parent_manager = (CurrencyManager) this[data_source, ""];

					if (parent_manager.Count == 0 ||
					    TypeDescriptor.GetProperties (parent_manager.GetItem (0)).Find (data_member, true) == null) {
						throw new ArgumentException ("Cannot create a child list for field {0}", data_member);
					}
						

					Console.WriteLine ("creating related property manager for column {0} on an IList source", data_member);
					return new RelatedPropertyManager (parent_manager, data_member);
				}
			} else if (data_source is IListSource) {
				return new CurrencyManager (((IListSource) data_source).GetList ());
			}
			else {
				/* must be a property */
				Console.WriteLine ("creating PropertyManager");
				return new PropertyManager (data_source, data_member);
			}
#else
			if (data_member == "") {
				if (data_source is DataSet) {
					return new CurrencyManager (new DataViewManager ((DataSet)data_source));
				}
				else if (data_source is DataTable) {
					return new CurrencyManager (new DataView ((DataTable)data_source));
				}
				else if (data_source is DataView) {
					return new CurrencyManager ((DataView)data_source);
				}
				else if (data_source is IListSource) {
					return new CurrencyManager (((IListSource) data_source).GetList ());
				}
				else if (data_source is IList) {
					return new CurrencyManager ((IList) data_source);
				}
				else {
					return new PropertyManager (data_source, data_member);
				}
			}
			else {
				BindingMemberInfo info = new BindingMemberInfo (data_member);

				Console.WriteLine ("Getting parent_manager for {0}", info.BindingPath);
				BindingManagerBase parent_manager = this[data_source, info.BindingPath];
				CurrencyManager cm = parent_manager as CurrencyManager;

				PropertyDescriptor pd = parent_manager == null ? null : parent_manager.GetItemProperties ().Find (info.BindingField, true);

				if (pd != null) {
					Console.WriteLine ("parent_manager.GetItemProperties returned property descriptor for {0}", pd.Name);
					if (cm != null) {
						if (cm.data_source is DataViewManager)
							return new RelatedCurrencyManager (cm, );
						else
							return new RelatedPropertyManager (cm, info.BindingField);
					}
				}
				else {
					/* null property.  extra checks here, for e.g. DataRelations */
					if (cm != null) {
						if (cm.data_source is DataViewManager) {
							DataSet ds = ((DataViewManager)cm.data_source).DataSet;
							DataRelation rel = ds.Relations [info.BindingField];

							if (rel != null) {
								Console.WriteLine ("+ creating related currency manager for relation {0}", info.BindingField);
								return new RelatedCurrencyManager (cm, rel);
							}
						}
					}
				}

				throw new ArgumentException (String.Format ("Cannot create a child list for field {0}", info.BindingField));
			}
#endif
		}

		#region Public Instance Methods
		public bool Contains(object dataSource)
		{
			return Contains (dataSource, String.Empty);
		}

		public bool Contains (object dataSource, string dataMember)
		{
			if (dataSource == null)
				throw new ArgumentNullException ("dataSource");
			if (dataMember == null)
				dataMember = String.Empty;

			HashKey key = new HashKey (dataSource, dataMember);
			return managers [key] != null;
		}
		#endregion	// Public Instance Methods

		#region Protected Instance Methods

		protected internal void Add (object dataSource, BindingManagerBase listManager)
		{
			AddCore (dataSource, listManager);
			OnCollectionChanged (new CollectionChangeEventArgs (CollectionChangeAction.Add, dataSource));
		}

		protected virtual void AddCore (object dataSource, BindingManagerBase listManager)
		{
			if (dataSource == null)
				throw new ArgumentNullException ("dataSource");
			if (listManager == null)
				throw new ArgumentNullException ("listManager");

			HashKey key = new HashKey (dataSource, String.Empty);
			managers [key] = listManager;
		}

		protected internal void Clear ()
		{
			ClearCore();
			OnCollectionChanged (new CollectionChangeEventArgs (CollectionChangeAction.Refresh, null));
		}

		protected virtual void ClearCore ()
		{
			managers.Clear ();
		}

		protected virtual void OnCollectionChanged (CollectionChangeEventArgs ccevent)
		{
			if (onCollectionChangedHandler != null) {
				onCollectionChangedHandler (this, ccevent);
			}
		}

		protected internal void Remove (object dataSource)
		{
			if (dataSource == null)
				throw new ArgumentNullException ("dataSource");

			RemoveCore (dataSource);
			OnCollectionChanged (new CollectionChangeEventArgs (CollectionChangeAction.Remove, dataSource));
		}

		protected virtual void RemoveCore (object dataSource)
		{
			HashKey[] keys = new HashKey [managers.Keys.Count];
			managers.Keys.CopyTo (keys, 0);

			for (int i = 0; i < keys.Length; i ++) {
				if (keys[i].source == dataSource)
					managers.Remove (keys[i]);
			}
		}
		#endregion	// Protected Instance Methods

		#region Events
		public event CollectionChangeEventHandler CollectionChanged {
			add { throw new NotImplementedException (); }
			remove { /* nothing to do here.. */ }
		}
		#endregion	// Events

		#region ICollection Interfaces
		void ICollection.CopyTo (Array array, int index)
		{
			managers.CopyTo (array, index);
		}

		int ICollection.Count {
			get {
				return managers.Count;
			}
		}

		bool ICollection.IsSynchronized {
			get {
				return false;
			}
		}

		object ICollection.SyncRoot {
			get {
				return null;
			}
		}

		#endregion	// ICollection Interfaces

		#region IEnumerable Interfaces
		[MonoTODO ("our enumerator is slightly different.  in MS's implementation the Values are WeakReferences to the managers.")]
		IEnumerator IEnumerable.GetEnumerator() {
			return managers.GetEnumerator ();
		}
		#endregion	// IEnumerable Interfaces
	}
}
