//
// System.CodeDom CodeCatchClauseCollection Class implementation
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) 2001 Ximian, Inc.
//

namespace System.CodeDom {

	using System.Collections;
	
	[Serializable]
	public class CodeCatchClauseCollection : IList, ICollection, IEnumerable {

		ArrayList catchClauses;
		
		//
		// Constructors
		//
		public CodeCatchClauseCollection ()
		{
			catchClauses = new ArrayList ();
		}

		//
		// Properties
		//
		public int Count {
			get {
				return catchClauses.Count;
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
		public void Add (CodeCatchClause value)
		{
			catchClauses.Add (value);
		}

		public void AddRange (CodeCatchClause [] values)
		{
			foreach (CodeCatchClause ca in values) 
				catchClauses.Add (ca);

		}

		public void Clear ()
		{
			catchClauses.Clear ();
		}

		private class Enumerator : IEnumerator {
			private CodeCatchClauseCollection collection;
			private int currentIndex = -1;

			internal Enumerator (CodeCatchClauseCollection collection)
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
			return new CodeCatchClauseCollection.Enumerator (this);
		}

		//
		// IList method implementations
		//
		public int Add (object value)
		{
			return catchClauses.Add (value);
		}

		public bool Contains (Object value)
		{
			return catchClauses.Contains (value);
		}

		public int IndexOf (Object value)
		{
			return catchClauses.IndexOf (value);
		}

		public void Insert (int index, Object value)
		{
			catchClauses [index] = value;
		}

		public object this[int index] {
			get {
				return catchClauses [index];
			}

			set {
				catchClauses [index] = value;
			}
		}

		public void Remove (object value)
		{
			catchClauses.Remove (value);
		}

		public void RemoveAt (int index)
		{
			catchClauses.RemoveAt (index);
		}

		//
		// ICollection method implementations
		//
		public void CopyTo (Array array, int index)
		{
			catchClauses.CopyTo (array, index);
		}

		public object SyncRoot {
			get {
				return catchClauses.SyncRoot;
			}
		}

		public bool IsReadOnly {
			get {
				return false;
			}
		}

		public bool IsSynchronized {
			get {
				return catchClauses.IsSynchronized;
			}
		}
	}
}
