//
// System.Web.UI.DataBindingCollection.cs
//
// Duncan Mak  (duncan@ximian.com)
//
// (C) Ximian, Inc.
//

using System;
using System.Collections;

namespace System.Web.UI {

	public sealed class DataBindingCollection : ICollection, IEnumerable
	{
		Hashtable list;
		ArrayList removed;
		
		public DataBindingCollection ()
		{
			list = new Hashtable ();
			removed = new ArrayList ();
		}

		public int Count {
			get { return list.Count; }
		}

		public bool IsReadOnly {
			get { return list.IsReadOnly; }
		}

		public bool IsSynchronized {
			get { return list.IsSynchronized; }
		}

		public DataBinding this [string propertyName] {
			get { return list [propertyName] as DataBinding; }
		}

		public string [] RemovedBindings {
			get { return (string []) removed.ToArray (typeof (string)); }
		}

		public object SyncRoot {
			get { return list.SyncRoot; }
		}

		public void Add (DataBinding binding)
		{
			list.Add (binding.PropertyName, binding);
		}

		public void Clear ()
		{
			list.Clear ();
		}

		public void CopyTo (Array array, int index)
		{
			list.CopyTo (array, index);
		}

		public IEnumerator GetEnumerator ()
		{
			return list.GetEnumerator ();
		}

		public void Remove (DataBinding binding)
		{
			string key = binding.PropertyName;
			Remove (key);
		}

		public void Remove (string propertyName)
		{
			removed.Add (propertyName);
			list.Remove (propertyName);
		}

		public void Remove (string propertyName,
				    bool addToRemovedList)
		{
			if (addToRemovedList)
				removed.Add (String.Empty); // LAMESPEC
			else
				removed.Add (propertyName);

			list.Remove (propertyName);
		}
	}
}
