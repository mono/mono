//
// System.CodeDom CodeNamespaceImportCollection Class implementation
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) 2001 Ximian, Inc.
//

namespace System.CodeDom {

	using System.Collections;
	
	public class CodeNamespaceImportCollection : IList, ICollection, IEnumerable {

		ArrayList namespaceImports;
		
		//
		// Constructors
		//
		public CodeNamespaceImportCollection ()
		{
			namespaceImports = new ArrayList ();
		}

		//
		// Properties
		//
		public int Count {
			get {
				return namespaceImports.Count;
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
		public void Add (CodeNamespaceImport value)
		{
			namespaceImports.Add (value);
		}

		public void AddRange (CodeNamespaceImport [] values)
		{
			foreach (CodeNamespaceImport ca in values) 
				namespaceImports.Add (ca);

		}

		public void Clear ()
		{
			namespaceImports.Clear ();
		}

		private class Enumerator : IEnumerator {
			private CodeNamespaceImportCollection collection;
			private int currentIndex = -1;

			internal Enumerator (CodeNamespaceImportCollection collection)
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
			return new CodeNamespaceImportCollection.Enumerator (this);
		}

		//
		// IList method implementations
		//
		public int Add (object value)
		{
			return namespaceImports.Add (value);
		}

		public bool Contains (Object value)
		{
			return namespaceImports.Contains (value);
		}

		public int IndexOf (Object value)
		{
			return namespaceImports.IndexOf (value);
		}

		public void Insert (int index, Object value)
		{
			namespaceImports [index] = value;
		}

		public object this[int index] {
			get {
				return namespaceImports [index];
			}

			set {
				namespaceImports [index] = value;
			}
		}

		public void Remove (object value)
		{
			namespaceImports.Remove (value);
		}

		public void RemoveAt (int index)
		{
			namespaceImports.RemoveAt (index);
		}

		//
		// ICollection method implementations
		//
		public void CopyTo (Array array, int index)
		{
			namespaceImports.CopyTo (array, index);
		}

		public object SyncRoot {
			get {
				return namespaceImports.SyncRoot;
			}
		}

		public bool IsReadOnly {
			get {
				return false;
			}
		}

		public bool IsSynchronized {
			get {
				return namespaceImports.IsSynchronized;
			}
		}
	}
}
