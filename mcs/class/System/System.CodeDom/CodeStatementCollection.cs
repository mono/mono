//
// System.CodeDom CodeStatementCollection Class implementation
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) 2001 Ximian, Inc.
//

namespace System.CodeDom {

	using System.Collections;
	
	[Serializable]
	public class CodeStatementCollection : IList, ICollection, IEnumerable {

		ArrayList statements;
		
		//
		// Constructors
		//
		public CodeStatementCollection ()
		{
			statements = new ArrayList ();
		}

		//
		// Properties
		//
		public int Count {
			get {
				return statements.Count;
			}
		}

                public bool IsFixedSize {
                        get {
                                return true;
                        }
                }

		//
		// Methods
		//
		public void Add (CodeStatement value)
		{
			statements.Add (value);
		}

		public void AddRange (CodeStatement [] values)
		{
			foreach (CodeStatement ca in values) 
				statements.Add (ca);

		}

		public void Clear ()
		{
			statements.Clear ();
		}

		private class Enumerator : IEnumerator {
			private CodeStatementCollection collection;
			private int currentIndex = -1;

			internal Enumerator (CodeStatementCollection collection)
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
			return new CodeStatementCollection.Enumerator (this);
		}

		//
		// IList method implementations
		//
		public int Add (object value)
		{
			return statements.Add (value);
		}

		public bool Contains (Object value)
		{
			return statements.Contains (value);
		}

		public int IndexOf (Object value)
		{
			return statements.IndexOf (value);
		}

		public void Insert (int index, Object value)
		{
			statements [index] = value;
		}

		public object this[int index] {
			get {
				return statements [index];
			}

			set {
				statements [index] = value;
			}
		}

		public void Remove (object value)
		{
			statements.Remove (value);
		}

		public void RemoveAt (int index)
		{
			statements.RemoveAt (index);
		}

		//
		// ICollection method implementations
		//
		public void CopyTo (Array array, int index)
		{
			statements.CopyTo (array, index);
		}

		public object SyncRoot {
			get {
				return statements.SyncRoot;
			}
		}

		public bool IsReadOnly {
			get {
				return false;
			}
		}

		public bool IsSynchronized {
			get {
				return statements.IsSynchronized;
			}
		}
	}
}
