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
				return source.GetHashCode() ^ member.GetHashCode ();
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
			onCollectionChangedHandler = null;
		}

		public bool IsReadOnly {
			get { return false; }
		}

		public BindingManagerBase this [object dataSource] {
			get { return this [dataSource, String.Empty]; }
		}

		public BindingManagerBase this [object dataSource, string dataMember] {
			get {
				if (dataSource == null)
					throw new ArgumentNullException ("dataSource");
				if (dataMember == null)
					dataMember = String.Empty;

				ICurrencyManagerProvider cm_provider = dataSource as ICurrencyManagerProvider;
				if (cm_provider != null) {
					if (dataMember.Length == 0)
						return cm_provider.CurrencyManager;

					return cm_provider.GetRelatedCurrencyManager (dataMember);
				}

				HashKey key = new HashKey (dataSource, dataMember);
				BindingManagerBase res = managers [key] as BindingManagerBase;

				if (res != null)
					return res;

				res = CreateBindingManager (dataSource, dataMember);
				if (res == null)
					return null;
				managers [key] = res;
				return res;
			}
		}

		private BindingManagerBase CreateBindingManager (object data_source, string data_member)
		{
			if (data_member == "") {
				if (IsListType (data_source.GetType ()))
					return new CurrencyManager (data_source);
				else
					return new PropertyManager (data_source);
			}
			else {
				BindingMemberInfo info = new BindingMemberInfo (data_member);

				BindingManagerBase parent_manager = this[data_source, info.BindingPath];

				PropertyDescriptor pd = parent_manager == null ? null : parent_manager.GetItemProperties ().Find (info.BindingField, true);

				if (pd == null)
					throw new ArgumentException (String.Format ("Cannot create a child list for field {0}.", info.BindingField));

				if (IsListType (pd.PropertyType))
					return new RelatedCurrencyManager (parent_manager, pd);
				else
					return new RelatedPropertyManager (parent_manager, info.BindingField);
			}
		}

		bool IsListType (Type t)
		{
			return (typeof (IList).IsAssignableFrom (t)
				|| typeof (IListSource).IsAssignableFrom (t));
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
#if NET_2_0
		[MonoTODO ("Stub, does nothing")]
		public static void UpdateBinding (BindingContext newBindingContext, Binding binding)
		{
		}
#endif
		#endregion	// Protected Instance Methods

		#region Events
#if NET_2_0
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
#endif
		public event CollectionChangeEventHandler CollectionChanged {
			add { throw new NotImplementedException (); }
			remove { /* nothing to do here.. */ }
		}
		#endregion	// Events

		#region ICollection Interfaces
		void ICollection.CopyTo (Array ar, int index)
		{
			managers.CopyTo (ar, index);
		}

		int ICollection.Count {
			get { return managers.Count; }
		}

		bool ICollection.IsSynchronized {
			get { return false; }
		}

		object ICollection.SyncRoot {
			get { return null; }
		}

		#endregion	// ICollection Interfaces

		#region IEnumerable Interfaces
		[MonoInternalNote ("our enumerator is slightly different.  in MS's implementation the Values are WeakReferences to the managers.")]
		IEnumerator IEnumerable.GetEnumerator() {
			return managers.GetEnumerator ();
		}
		#endregion	// IEnumerable Interfaces
	}
}
