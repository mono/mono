//
// System.CodeDOM Code@CONTAINEE@Collection Class implementation
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) 2001 Ximian, Inc.
//

namespace System.CodeDOM {

	using System.Collections;
	
	public class Code@CONTAINEE@Collection : IList, ICollection, IEnumerable {

		ArrayList @arrayname@;
		
		//
		// Constructors
		//
		public Code@CONTAINEE@Collection ()
		{
			@arrayname@ = new ArrayList ();
		}

		//
		// Properties
		//
		public int Count {
			get {
				return @arrayname@.Count;
			}
		}

		//
		// Methods
		//
		public void Add (Code@CONTAINEE@ value)
		{
			@arrayname@.Add (value);
		}

		public void AddRange (Code@CONTAINEE@ [] values)
		{
			foreach (Code@CONTAINEE@ ca in values) 
				@arrayname@.Add (ca);

		}

		public void Clear ()
		{
			@arrayname@.Clear ();
		}

		private class Enumerator : IEnumerator {
			private Code@CONTAINEE@Collection collection;
			private int currentIndex = -1;

			internal Enumerator (Code@CONTAINEE@Collection collection)
			{
				this.collection = collection;
			}

			public object Current {
				get {
					if (currentIndex == collection.Count)
						throw new InvalidOperationException ();
					return collection [currentIndex];
				}
			}

			public bool MoveNext ()
			{
				if (currentIndex > collection.Count)
					throw new InvalidOperationException ();
				return ++currentIndex < collection.Count;
			}

			public void Reset ()
			{
				currentIndex = -1;
			}
		}
		
		public IEnumerator GetEnumerator ()
		{
			return new Code@CONTAINEE@Collection.Enumerator (this);
		}

		//
		// IList method implementations
		//
		public int Add (object value)
		{
			return @arrayname@.Add (value);
		}

		public bool Contains (Object value)
		{
			return @arrayname@.Contains (value);
		}

		public int IndexOf (Object value)
		{
			return @arrayname@.IndexOf (value);
		}

		public void Insert (int index, Object value)
		{
			@arrayname@ [index] = value;
		}

		public object this[int index] {
			get {
				return @arrayname@ [index];
			}

			set {
				@arrayname@ [index] = value;
			}
		}

		public void Remove (object value)
		{
			@arrayname@.Remove (value);
		}

		public void RemoveAt (int index)
		{
			@arrayname@.RemoveAt (index);
		}

		//
		// ICollection method implementations
		//
		public void CopyTo (Array array, int index)
		{
			@arrayname@.CopyTo (array, index);
		}

		public object SyncRoot {
			get {
				return @arrayname@.SyncRoot;
			}
		}

		public bool IsReadOnly {
			get {
				return false;
			}
		}

		public bool IsSynchronized {
			get {
				return @arrayname@.IsSynchronized;
			}
		}
	}
}
