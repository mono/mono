//
// System.CodeDom CodeAttributeArgumentCollection Class implementation
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) 2001 Ximian, Inc.
//

namespace System.CodeDom {

	using System.Collections;
	
	public class CodeAttributeArgumentCollection : IList, ICollection, IEnumerable {

		ArrayList attributeArgs;
		
		//
		// Constructors
		//
		public CodeAttributeArgumentCollection ()
		{
			attributeArgs = new ArrayList ();
		}

		//
		// Properties
		//
		public int Count {
			get {
				return attributeArgs.Count;
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

		public void Add (CodeAttributeArgument value)
		{
			attributeArgs.Add (value);
		}

		public void AddRange (CodeAttributeArgument [] values)
		{
			foreach (CodeAttributeArgument ca in values) 
				attributeArgs.Add (ca);

		}

		public void Clear ()
		{
			attributeArgs.Clear ();
		}

		private class Enumerator : IEnumerator {
			private CodeAttributeArgumentCollection collection;
			private int currentIndex = -1;

			internal Enumerator (CodeAttributeArgumentCollection collection)
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
			return new CodeAttributeArgumentCollection.Enumerator (this);
		}

		//
		// IList method implementations
		//
		public int Add (object value)
		{
			return attributeArgs.Add (value);
		}

		public bool Contains (Object value)
		{
			return attributeArgs.Contains (value);
		}

		public int IndexOf (Object value)
		{
			return attributeArgs.IndexOf (value);
		}

		public void Insert (int index, Object value)
		{
			attributeArgs [index] = value;
		}

		public object this[int index] {
			get {
				return attributeArgs [index];
			}

			set {
				attributeArgs [index] = value;
			}
		}

		public void Remove (object value)
		{
			attributeArgs.Remove (value);
		}

		public void RemoveAt (int index)
		{
			attributeArgs.RemoveAt (index);
		}

		//
		// ICollection method implementations
		//
		public void CopyTo (Array array, int index)
		{
			attributeArgs.CopyTo (array, index);
		}

		public object SyncRoot {
			get {
				return attributeArgs.SyncRoot;
			}
		}

		public bool IsReadOnly {
			get {
				return false;
			}
		}

		public bool IsSynchronized {
			get {
				return attributeArgs.IsSynchronized;
			}
		}
	}
}
