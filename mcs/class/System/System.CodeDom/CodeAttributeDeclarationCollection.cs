//
// System.CodeDom CodeAttributeDeclarationCollection Class implementation
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) 2001 Ximian, Inc.
//

namespace System.CodeDom {

	using System.Collections;
	
	public class CodeAttributeDeclarationCollection : IList, ICollection, IEnumerable {

		ArrayList attributeDecls;
		
		//
		// Constructors
		//
		public CodeAttributeDeclarationCollection ()
		{
			attributeDecls = new ArrayList ();
		}

		//
		// Properties
		//
		public int Count {
			get {
				return attributeDecls.Count;
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
		public void Add (CodeAttributeDeclaration value)
		{
			attributeDecls.Add (value);
		}

		public void AddRange (CodeAttributeDeclaration [] values)
		{
			foreach (CodeAttributeDeclaration ca in values) 
				attributeDecls.Add (ca);

		}

		public void Clear ()
		{
			attributeDecls.Clear ();
		}

		private class Enumerator : IEnumerator {
			private CodeAttributeDeclarationCollection collection;
			private int currentIndex = -1;

			internal Enumerator (CodeAttributeDeclarationCollection collection)
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
			return new CodeAttributeDeclarationCollection.Enumerator (this);
		}

		//
		// IList method implementations
		//
		public int Add (object value)
		{
			return attributeDecls.Add (value);
		}

		public bool Contains (Object value)
		{
			return attributeDecls.Contains (value);
		}

		public int IndexOf (Object value)
		{
			return attributeDecls.IndexOf (value);
		}

		public void Insert (int index, Object value)
		{
			attributeDecls [index] = value;
		}

		public object this[int index] {
			get {
				return attributeDecls [index];
			}

			set {
				attributeDecls [index] = value;
			}
		}

		public void Remove (object value)
		{
			attributeDecls.Remove (value);
		}

		public void RemoveAt (int index)
		{
			attributeDecls.RemoveAt (index);
		}

		//
		// ICollection method implementations
		//
		public void CopyTo (Array array, int index)
		{
			attributeDecls.CopyTo (array, index);
		}

		public object SyncRoot {
			get {
				return attributeDecls.SyncRoot;
			}
		}

		public bool IsReadOnly {
			get {
				return false;
			}
		}

		public bool IsSynchronized {
			get {
				return attributeDecls.IsSynchronized;
			}
		}
	}
}
