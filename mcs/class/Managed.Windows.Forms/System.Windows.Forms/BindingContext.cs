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


using System.Collections;
using System.Globalization;
using System.ComponentModel;


namespace System.Windows.Forms {

	[DefaultEvent("CollectionChanged")]
	public class BindingContext : ICollection, IEnumerable {

		private Hashtable managers;

		private class ManagerEntry {

			private object source;
			private string member;

			private int member_hash;

			public ManagerEntry (object source, string member)
			{
				this.source = source;
				if (member == null)
					member = String.Empty;
				this.member = member.ToLower (CultureInfo.InvariantCulture);

				
				member_hash = this.member.GetHashCode ();
				if (member_hash == 0)
					member_hash = 1;
			}

			public override bool Equals (object b)
			{
				ManagerEntry o = (ManagerEntry) b;

				return (o.source == source && o.member_hash == member_hash);
			}

			public override int GetHashCode ()
			{
				return member_hash * source.GetHashCode ();
			}

			public override string ToString ()
			{
				return source.ToString () + " + " + member.ToString ();
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
				ManagerEntry e = CreateEntry (data_source, data_member);
				BindingManagerBase res = managers [e] as BindingManagerBase;

				if (res != null)
					return res;
				res = AddManager (data_source, data_member);
				return res;
			}
		}

		private BindingManagerBase AddManager (object data_source, string data_member)
		{
			BindingManagerBase res = CreateBindingManager (data_source, data_member);
			managers.Add (CreateEntry (data_source, data_member), res);
			managers.Add (CreateEntry (data_source, String.Empty), res);

			return res;
		}

		private BindingManagerBase CreateBindingManager (object data_source, 
			string data_member)
		{
			if (data_source is IList || 
				data_source is IListSource ||
				data_source is IBindingList) {
				CurrencyManager res = new CurrencyManager (data_source);
				return res;
			}

			return new PropertyManager (data_source);
		}

		#region Public Instance Methods
		public bool Contains(object dataSource)
		{
			return Contains (dataSource, String.Empty);
		}

		public bool Contains (object dataSource, string dataMember)
		{
			ManagerEntry entry = CreateEntry (dataSource, dataMember);

			return managers.ContainsKey (entry);
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
			managers.Add (CreateEntry (dataSource, String.Empty), listManager);
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
			managers.Remove (CreateEntry (dataSource, String.Empty));
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

		private ManagerEntry CreateEntry (object dataSource, string dataMember)
		{
			return new ManagerEntry (dataSource, dataMember);
		}
	}
}
