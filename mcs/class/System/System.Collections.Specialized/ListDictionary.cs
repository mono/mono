namespace System.Collections.Specialized
{
	[Serializable]
	public class ListDictionary : IDictionary, ICollection, IEnumerable
	{
		private int count;
		private int modCount;
		private ListEntry root;
		private IComparer comparer;
		
		
		public ListDictionary()
		{
			count = 0;
			modCount = 0;
			comparer = null;
			root = null;
		}
		
		public ListDictionary(IComparer comparer) : this()
		{
			this.comparer = comparer;
		}
		
		private bool AreEqual(object obj1, object obj2)
		{
			if (comparer != null) {
				if (comparer.Compare(obj1, obj2) == 0) {
					return true;
				}
			} else {
				if (obj1.Equals(obj2)) {
					return true;
				}
			}
			
			return false;
		}
		
		private ListEntry FindEntry(object key)
		{
			if (key == null) {
				throw new ArgumentNullException("Attempted lookup for a null key.");
			}
			
			if (root == null) {
				return null;
			} else {
				ListEntry entry = root;
				
				while (entry != null) {
					if (AreEqual(key, entry.key)) {
						return entry;
					}
					
					entry = entry.next;
				}
			}
			
			return null;
		}

		private void AddImpl(object key, object value)
		{
			if (key == null) {
				throw new ArgumentNullException("Attempted add with a null key.");
			}
			
			if (root == null) {
				root = new ListEntry();
				root.key = key;
				root.value = value;
			} else {
				ListEntry entry = root;
				
				while (entry != null) {
					if (AreEqual(key, entry.key)) {
						throw new ArgumentException("Duplicate key in add.");
					}
					
					if (entry.next == null) {
						break;
					}
					
					entry = entry.next;
				}
				
				entry.next = new ListEntry();
				entry.next.key = key;
				entry.next.value = value;
			}
			
			count++;
			modCount++;
		}
		
		// IEnumerable Interface
		IEnumerator IEnumerable.GetEnumerator()
		{
			return new ListEntryEnumerator(this);
		}
		
		// ICollection Interface
		public int Count {
			get {
				return count;
			}
		}
		
		public bool IsSynchronized {
			get {
				return false;
			}
		}
		
		public object SyncRoot {
			get {
				return this;
			}
		}

		public void CopyTo(Array array, int index)
		{
			int i = index;
			foreach ( DictionaryEntry entry in this )
				array.SetValue( entry, i++ );
		}
		
		// IDictionary Interface
		public bool IsFixedSize
		{
			get {
				return false;
			}
		}
		
		public bool IsReadOnly
		{
			get {
				return false;
			}
		}
		
		// Indexer
		public object this[object key]
		{
			get {
				ListEntry entry = FindEntry(key);
				return entry == null ? entry : entry.value;
			}
			
			set {
				ListEntry entry = FindEntry(key);
				if (entry != null)
					entry.value = value;
				else
					AddImpl(key, value);
			}
		}
		
		public ICollection Keys
		{
			get {
				return new ListEntryCollection(this, true);
			}
		}
		
		public ICollection Values
		{
			get {
				return new ListEntryCollection(this, false);
			}
		}
		
		public void Add(object key, object value)
		{
			AddImpl(key, value);
		}
		
		public void Clear()
		{
			root = null;
			count = 0;
			modCount++;
		}
		
		public bool Contains(object key)
		{
			return FindEntry(key) != null ? true : false;
		}
		
		public IDictionaryEnumerator GetEnumerator()
		{
			return new ListEntryEnumerator(this);
		}
		
		public void Remove(object key)
		{
			ListEntry entry = root;
			
			for (ListEntry prev = null; entry != null; prev = entry, entry = entry.next) {
				if (AreEqual(key, entry.key)) {
					if (prev != null) {
						prev.next = entry.next;
					} else {
						root = entry.next;
					}
					
					entry.value = null;
					count--;
					modCount++;
				}
			}
		}
		

		private class ListEntry
		{
			public object key = null;
			public object value = null;
			public ListEntry next = null;
		}


		private class ListEntryEnumerator : IEnumerator, IDictionaryEnumerator
		{
			private ListDictionary dict;
			private bool isAtStart;
			private ListEntry current;
			private int version;
			
			public ListEntryEnumerator(ListDictionary dict)
			{
				this.dict = dict;
				version = dict.modCount;
				Reset();
			}

			private void FailFast()
			{
				if (version != dict.modCount) {
					throw new InvalidOperationException(
						"The ListDictionary's contents changed after this enumerator was instantiated.");
				}
			}
				
			public bool MoveNext()
			{
				FailFast();
				
				if (isAtStart) {
					current = dict.root;
					isAtStart = false;
				} else {
					current = current.next;
				}
				
				return current != null ? true : false;	
			}
			
			public void Reset()
			{
				FailFast();

				isAtStart = true;
				current = null;
			}
			
			public object Current
			{
				get {
					FailFast();
					
					if (isAtStart || current == null) {
						throw new InvalidOperationException(
							"Enumerator is positioned before the collection's first element or after the last element.");
					}
					
					return new DictionaryEntry(current.key, current.value);
				}
			}
			
			// IDictionaryEnumerator
			public DictionaryEntry Entry
			{
				get {
					FailFast();
					return (DictionaryEntry) Current;
				}
			}
			
			public object Key
			{
				get {
					FailFast();
					
					if (isAtStart || current == null) {
						throw new InvalidOperationException(
							"Enumerator is positioned before the collection's first element or after the last element.");
					}
					
					return current.key;
				}
			}
			
			public object Value
			{
				get {
					FailFast();
					
					if (isAtStart || current == null) {
						throw new InvalidOperationException(
							"Enumerator is positioned before the collection's first element or after the last element.");
					}
					
					return current.value;
				}
			}
		}
		
		private class ListEntryCollection : ICollection
		{
			private ListDictionary dict;
			private bool isKeyList;
				
			public ListEntryCollection(ListDictionary dict, bool isKeyList)
			{
				this.dict = dict;
				this.isKeyList = isKeyList;
			}
			
			// ICollection Interface
			public int Count {
				get {
					return dict.Count;
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
					return dict.SyncRoot;
				}
			}

			public void CopyTo(Array array, int index)
			{
				int i = index;
				foreach ( object obj in this )
					array.SetValue( obj, i++ );
			}
			
			// IEnumerable Interface
			public IEnumerator GetEnumerator()
			{
				return new ListEntryCollectionEnumerator(dict, isKeyList);
			}
			
			private class ListEntryCollectionEnumerator : IEnumerator
			{
				private ListDictionary dict;
				private bool isKeyList;
				private bool isAtStart;
				private int version;
				private ListEntry current;
					
				public ListEntryCollectionEnumerator(ListDictionary dict, bool isKeyList)
				{
					this.dict = dict;
					this.isKeyList = isKeyList;
					isAtStart = true;
					version = dict.modCount;
				}

				private void FailFast()
				{
					if (version != dict.modCount) {
						throw new InvalidOperationException(
							"The Collection's contents changed after this " +
							"enumerator was instantiated.");
					}
				}
				
				public object Current
				{
					get {
						FailFast();
						
						if (isAtStart || current == null) {
							throw new InvalidOperationException(
								"Enumerator is positioned before the collection's " +
								"first element or after the last element.");
						}
						
						return isKeyList ? current.key : current.value;
					}
				}
				
				public bool MoveNext()
				{
					FailFast();
					
					if (isAtStart) {
						current = dict.root;
						isAtStart = false;
					} else {
						current = current.next;
					}
					
					return current != null ? true : false;
				}
				
				public void Reset()
				{
					FailFast();
					isAtStart = true;
					current = null;
				}
			}
		}
	}
}
