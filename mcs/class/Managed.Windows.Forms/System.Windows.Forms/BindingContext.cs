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
		private object null_data_source = new object ();

		private class DataSourceEntry {

			private object source;
			private Hashtable members;
			// private BindingManagerBase default_manager;
			
			public DataSourceEntry (object source)
			{
				this.source = source;
				members = new Hashtable ();
			}

			public BindingManagerBase AddMember (string member)
			{
				if (member == null)
					member = String.Empty;
				BindingManagerBase res = members [member] as BindingManagerBase;
				if (res != null)
					return res;
				res = CreateBindingManager (source, member);
				Console.WriteLine ("CREATED BINDING MANAGER:   {0}", res);
				if (res == null)
					return null;
				members [member] = res;
				return res;
			}

			public void AddMember (string member, BindingManagerBase manager)
			{
				members [member] = manager;
			}

			public bool Contains (string member)
			{
				return members.Contains (member);
			}
		}

		public BindingContext () 
		{
			managers = new Hashtable ();
		}

		public bool IsReadOnly {
			get {
				return false;
			}
		}

		public BindingManagerBase this [object dataSource] {
			get {
				return this [dataSource, String.Empty];
			}
		}

		public BindingManagerBase this [object data_source, string data_member] {
			get {
				DataSourceEntry ds = GetEntry (data_source, data_member, true);
				return ds.AddMember (data_member);
			}
		}

		private DataSourceEntry GetEntry (object data_source, string data_member, bool create)
		{
			if (data_source == null)
				throw new ArgumentNullException ("data_source");
				
			DataSourceEntry ds = managers [data_source] as DataSourceEntry;
			if (ds == null && create) {
				ds = new DataSourceEntry (data_source);
				managers [data_source] = ds;
			}

			return ds;
		}

		private static BindingManagerBase CreateBindingManager (object data_source, 
			string data_member)
		{
			if (data_source is IList || 
				data_source is IListSource ||
				data_source is IBindingList) {
				return CreateCurrencyManager (data_source, data_member);
			}

			return new PropertyManager (data_source, data_member);
		}

		private static CurrencyManager CreateCurrencyManager (object data_source, string data_member)
		{
			IList list = null;

			if (data_source is IList) {
				list = (IList) data_source;
			} else if (data_source is IListSource) {
				list = ((IListSource) data_source).GetList ();
			} else {
				throw new Exception ("Attempted to create currency manager " +
					"from invalid type: " + data_source.GetType ());
			}

			DataTable table = data_source as DataTable;
			if (table == null && data_source is DataView)
				table = ((DataView) data_source).Table;

			DataSet dataset = data_source as DataSet;
			if (table == null && dataset != null) {
				string table_name = data_member;
				int sp = data_member != null ? data_member.IndexOf ('.') : -1;
				if (sp != -1) {
					table_name = data_member.Substring (0, sp);
					data_member = data_member.Substring (sp + 1);
				}
				if (dataset != null && table_name != String.Empty) {
					Console.WriteLine ("TABLE NAME:  {0}   data member:   {1}", table_name, data_member);
					table = dataset.Tables [table_name];
					if (table == null)
						throw new ArgumentException (String.Format ("Specified data member table {0} does not exist in the data source DataSet", data_member));
					if (data_member != table_name) {
						/*
						Console.WriteLine ("CHECKING FOR COLUMN:   {0}", data_member);
						DataColumn col = table.Columns [data_member];
						DataRelation rel = (col == null ? dataset.Relations [data_member] : null);
						Console.WriteLine ("COLUMN:   {0}    RELATION:  {1}", col, rel);
						if (rel == null && col == null)
							throw new ArgumentException (String.Format ("Specified data member {0} does not exist in the data table {1}", data_member, table_name));

						// FIXME: hmm, in such case what should we do?
						*/
						table = null;
						list = null;
					}
				}
			}

			Console.WriteLine ("DATA TABLE:   {0}", table);
			
			if (table != null) {
				Console.WriteLine ("CREATING VIEW ON:  {0}", table.TableName);
				list = new DataView (table);
			}

			if (list == null)
				return null;
			return new CurrencyManager (list);
		}

		#region Public Instance Methods
		public bool Contains(object dataSource)
		{
			return Contains (dataSource, String.Empty);
		}

		public bool Contains (object dataSource, string dataMember)
		{
			DataSourceEntry ds = GetEntry (dataSource, dataMember, false);
			if (ds == null)
				return false;
			return ds.Contains (dataMember);

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
			DataSourceEntry ds = GetEntry (dataSource, String.Empty, true);
			ds.AddMember (String.Empty, listManager);
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

		protected virtual void OnCollectionChanged(System.ComponentModel.CollectionChangeEventArgs ccevent)
		{
			if (CollectionChanged != null) {
				CollectionChanged (this, ccevent);
			}
		}

		protected internal void Remove (object dataSource)
		{
			RemoveCore (dataSource);
			OnCollectionChanged (new CollectionChangeEventArgs (CollectionChangeAction.Remove, dataSource));
		}

		protected virtual void RemoveCore (object dataSource)
		{
			managers.Remove (dataSource);
		}
		#endregion	// Protected Instance Methods

		#region Events
		public event CollectionChangeEventHandler CollectionChanged;
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
		[MonoTODO]
		IEnumerator IEnumerable.GetEnumerator() {
			throw new NotImplementedException();
		}
		#endregion	// IEnumerable Interfaces
	}
}
