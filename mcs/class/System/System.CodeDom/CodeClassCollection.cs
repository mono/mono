//
// System.CodeDOM CodeClassCollection Class implementation
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) 2001 Ximian, Inc.
//

namespace System.CodeDOM {

	using System.Collections;
	
	public class CodeClassCollection : IList, ICollection, IEnumerable {

		ArrayList classes;
		
		//
		// Constructors
		//
		public CodeClassCollection ()
		{
			classes = new ArrayList ();
		}

		//
		// Properties
		//
		public int Count {
			get {
				return classes.Count;
			}
		}

		//
		// Methods
		//
		public void Add (CodeClass value)
		{
			classes.Add (value);
		}

		public void AddRange (CodeClass [] values)
		{
			foreach (CodeClass ca in values) 
				classes.Add (ca);

		}

		public void Clear ()
		{
			classes.Clear ();
		}

		private class Enumerator : IEnumerator {
			private CodeClassCollection collection;
			private int currentIndex = -1;

			internal Enumerator (CodeClassCollection collection)
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
			return new CodeClassCollection.Enumerator (this);
		}

		//
		// IList method implementations
		//
		public int Add (object value)
		{
			return classes.Add (value);
		}

		public bool Contains (Object value)
		{
			return classes.Contains (value);
		}

		public int IndexOf (Object value)
		{
			return classes.IndexOf (value);
		}

		public void Insert (int index, Object value)
		{
			classes [index] = value;
		}

		public object this[int index] {
			get {
				return classes [index];
			}

			set {
				classes [index] = value;
			}
		}

		public void Remove (object value)
		{
			classes.Remove (value);
		}

		public void RemoveAt (int index)
		{
			classes.RemoveAt (index);
		}

		//
		// ICollection method implementations
		//
		public void CopyTo (Array array, int index)
		{
			classes.CopyTo (array, index);
		}

		public object SyncRoot {
			get {
				return classes.SyncRoot;
			}
		}

		public bool IsReadOnly {
			get {
				return false;
			}
		}

		public bool IsSynchronized {
			get {
				return classes.IsSynchronized;
			}
		}
	}
}
