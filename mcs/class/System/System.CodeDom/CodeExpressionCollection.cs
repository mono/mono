//
// System.CodeDOM CodeExpressionCollection Class implementation
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) 2001 Ximian, Inc.
//

namespace System.CodeDOM {

	using System.Collections;
	
	public class CodeExpressionCollection : IList, ICollection, IEnumerable {

		ArrayList expressions;
		
		//
		// Constructors
		//
		public CodeExpressionCollection ()
		{
			expressions = new ArrayList ();
		}

		//
		// Properties
		//
		public int Count {
			get {
				return expressions.Count;
			}
		}

		//
		// Methods
		//
		public void Add (CodeExpression value)
		{
			expressions.Add (value);
		}

		public void AddRange (CodeExpression [] values)
		{
			foreach (CodeExpression ca in values) 
				expressions.Add (ca);

		}

		public void Clear ()
		{
			expressions.Clear ();
		}

		private class Enumerator : IEnumerator {
			private CodeExpressionCollection collection;
			private int currentIndex = -1;

			internal Enumerator (CodeExpressionCollection collection)
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
			return new CodeExpressionCollection.Enumerator (this);
		}

		//
		// IList method implementations
		//
		public int Add (object value)
		{
			return expressions.Add (value);
		}

		public bool Contains (Object value)
		{
			return expressions.Contains (value);
		}

		public int IndexOf (Object value)
		{
			return expressions.IndexOf (value);
		}

		public void Insert (int index, Object value)
		{
			expressions [index] = value;
		}

		public object this[int index] {
			get {
				return expressions [index];
			}

			set {
				expressions [index] = value;
			}
		}

		public void Remove (object value)
		{
			expressions.Remove (value);
		}

		public void RemoveAt (int index)
		{
			expressions.RemoveAt (index);
		}

		//
		// ICollection method implementations
		//
		public void CopyTo (Array array, int index)
		{
			expressions.CopyTo (array, index);
		}

		public object SyncRoot {
			get {
				return expressions.SyncRoot;
			}
		}

		public bool IsReadOnly {
			get {
				return false;
			}
		}

		public bool IsSynchronized {
			get {
				return expressions.IsSynchronized;
			}
		}
	}
}
