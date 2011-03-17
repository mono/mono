//
// System.Web.UI.WebControls.OrderedDictionary.cs
//
// Authors:
//	Lluis Sanchez Gual (lluis@novell.com)
//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
//
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

using System.Runtime.Serialization;

namespace System.Collections.Specialized
{
	[Serializable]
	public class OrderedDictionary : IOrderedDictionary, IDictionary, ICollection, IEnumerable, ISerializable, IDeserializationCallback
	{
		ArrayList list;
		Hashtable hash;
		bool readOnly;
		int initialCapacity;
		SerializationInfo serializationInfo;
		IEqualityComparer comparer;
		
		public OrderedDictionary ()
		{
			list = new ArrayList ();
			hash = new Hashtable ();
		}
		
		public OrderedDictionary (int capacity)
		{
			initialCapacity = (capacity < 0) ? 0 : capacity;
			list = new ArrayList (initialCapacity);
			hash = new Hashtable (initialCapacity);
		}
		
		public OrderedDictionary (IEqualityComparer equalityComparer)
		{
			list = new ArrayList ();
			hash = new Hashtable (equalityComparer);
			comparer = equalityComparer;
		}

		public OrderedDictionary (int capacity, IEqualityComparer equalityComparer)
		{
			initialCapacity = (capacity < 0) ? 0 : capacity;
			list = new ArrayList (initialCapacity);
			hash = new Hashtable (initialCapacity, equalityComparer);
			comparer = equalityComparer;
		}

		protected OrderedDictionary (SerializationInfo info, StreamingContext context)
		{
			serializationInfo = info;
		}

		protected virtual void OnDeserialization (object sender)
		{
			((IDeserializationCallback) this).OnDeserialization (sender);
		}

		void IDeserializationCallback.OnDeserialization (object sender)
		{
			if (serializationInfo == null)
				return;

			comparer = (IEqualityComparer) serializationInfo.GetValue ("KeyComparer", typeof (IEqualityComparer));
			readOnly = serializationInfo.GetBoolean ("ReadOnly");
			initialCapacity = serializationInfo.GetInt32 ("InitialCapacity");

			if (list == null)
				list = new ArrayList ();
			else
				list.Clear ();

			hash = new Hashtable (comparer);
			object[] array = (object[]) serializationInfo.GetValue ("ArrayList", typeof(object[]));
			foreach (DictionaryEntry de in array) {
				hash.Add (de.Key, de.Value);
				list.Add (de);
			}
		}
		
		public virtual void GetObjectData (SerializationInfo info, StreamingContext context)
		{
			if (info == null)
				throw new ArgumentNullException ("info");

			info.AddValue ("KeyComparer", comparer, typeof (IEqualityComparer));
			info.AddValue ("ReadOnly", readOnly);
			info.AddValue ("InitialCapacity", initialCapacity);

			object[] array = new object [hash.Count];
			hash.CopyTo (array, 0);
			info.AddValue ("ArrayList", array);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return list.GetEnumerator ();
		}
		
		public int Count {
			get {
				return list.Count;
			}
		}

		bool ICollection.IsSynchronized {
			get {
				return list.IsSynchronized;
			}
		}

		object ICollection.SyncRoot {
			get {
				return list.SyncRoot;
			}
		}

		public void CopyTo (Array array, int index)
		{
			list.CopyTo (array, index);
		}

		bool IDictionary.IsFixedSize {
			get {
				return false;
			}
		}
		
		public bool IsReadOnly
		{
			get {
				return readOnly;
			}
		}
		
		public object this [object key]
		{
			get { return hash [key]; }
			set {
				WriteCheck ();
				if (hash.Contains (key)) {
					int i = FindListEntry (key);
					list [i] = new DictionaryEntry (key, value);
				} else
					list.Add (new DictionaryEntry (key, value));
				
				hash [key] = value;
			}
		}
		
		public object this [int index]
		{
			get { return ((DictionaryEntry) list [index]).Value; }
			set {
				WriteCheck ();
				DictionaryEntry de = (DictionaryEntry) list [index];
				de.Value = value;
				// update (even on the list) isn't automatic
				list [index] = de;
				hash [de.Key] = value;
			}
		}
		
		public ICollection Keys
		{
			get {
				return new OrderedCollection (list, true);
			}
		}
		
		public ICollection Values
		{
			get {
				return new OrderedCollection (list, false);
			}
		}

		public void Add (object key, object value)
		{
			WriteCheck ();
			hash.Add (key, value);
			list.Add (new DictionaryEntry (key, value));
		}
		
		public void Clear()
		{
			WriteCheck ();
			hash.Clear ();
			list.Clear ();
		}
		
		public bool Contains (object key)
		{
			return hash.Contains (key);
		}
		
		public virtual IDictionaryEnumerator GetEnumerator()
		{
			return new OrderedEntryCollectionEnumerator (list.GetEnumerator ());
		}
		
		public void Remove (object key)
		{
			WriteCheck ();

			if (hash.Contains (key)) {
				hash.Remove (key);
				int i = FindListEntry (key);
				list.RemoveAt (i);
			}
		}
		
		int FindListEntry (object key)
		{
			for (int n=0; n<list.Count; n++) {
				DictionaryEntry de = (DictionaryEntry) list [n];
				if (comparer != null ? comparer.Equals(de.Key, key) : de.Key.Equals(key))
					return n;
			}
			return -1;
		}
		
		void WriteCheck ()
		{
			if (readOnly)
				throw new NotSupportedException ("Collection is read only");
		}
		
		public OrderedDictionary AsReadOnly ()
		{
			OrderedDictionary od = new OrderedDictionary ();
			od.list = list;
			od.hash = hash;
			od.comparer = comparer;
			od.readOnly = true;
			return od;
		}
		
		public void Insert (int index, object key, object value)
		{
			WriteCheck ();
			hash.Add (key, value);
			list.Insert (index, new DictionaryEntry (key, value));
		}
		
		public void RemoveAt (int index)
		{
			WriteCheck ();
			DictionaryEntry entry = (DictionaryEntry) list [index];
			list.RemoveAt (index);
			hash.Remove (entry.Key);
		}
		
		private class OrderedEntryCollectionEnumerator : IEnumerator, IDictionaryEnumerator
		{
			IEnumerator listEnumerator;
			
			public OrderedEntryCollectionEnumerator (IEnumerator listEnumerator)
			{
				this.listEnumerator = listEnumerator;
			}

			public bool MoveNext()
			{
				return listEnumerator.MoveNext ();
			}
			
			public void Reset()
			{
				listEnumerator.Reset ();
			}
			
			public object Current
			{
				get { return listEnumerator.Current; }
			}
			
			public DictionaryEntry Entry
			{
				get { return (DictionaryEntry) listEnumerator.Current; }
			}
			
			public object Key
			{
				get { return Entry.Key; }
			}
			
			public object Value
			{
				get { return Entry.Value; }
			}
		}
		
		private class OrderedCollection : ICollection
		{
			private ArrayList list;
			private bool isKeyList;
				
			public OrderedCollection (ArrayList list, bool isKeyList)
			{
				this.list = list;
				this.isKeyList = isKeyList;
			}

			public int Count {
				get {
					return list.Count;
				}
			}
			
			public bool IsSynchronized
			{
				get {
					return false;
				}
			}
			
			public object SyncRoot
			{
				get {
					return list.SyncRoot;
				}
			}

			public void CopyTo (Array array, int index)
			{
				for (int n=0; n<list.Count; n++) {
					DictionaryEntry de = (DictionaryEntry) list [n];
					if (isKeyList) array.SetValue (de.Key, index + n);
					else array.SetValue (de.Value, index + n);
				}
			}
			
			public IEnumerator GetEnumerator()
			{
				return new OrderedCollectionEnumerator (list.GetEnumerator (), isKeyList);
			}
			
			private class OrderedCollectionEnumerator : IEnumerator
			{
				private bool isKeyList;
				IEnumerator listEnumerator;
					
				public OrderedCollectionEnumerator (IEnumerator listEnumerator, bool isKeyList)
				{
					this.listEnumerator = listEnumerator;
					this.isKeyList = isKeyList;
				}

				public object Current
				{
					get {
						DictionaryEntry entry = (DictionaryEntry) listEnumerator.Current;
						return isKeyList ? entry.Key : entry.Value;
					}
				}
				
				public bool MoveNext()
				{
					return listEnumerator.MoveNext ();
				}
				
				public void Reset()
				{
					listEnumerator.Reset ();
				}
			}
		}
	}
}

