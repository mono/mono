//
// System.CodeDom CodeParameterDeclarationExpressionCollection Class implementation
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) 2001 Ximian, Inc.
//

namespace System.CodeDom {

	using System.Collections;
	
	public class CodeParameterDeclarationExpressionCollection : IList, ICollection, IEnumerable {

		ArrayList parameterDeclExprs;
		
		//
		// Constructors
		//
		public CodeParameterDeclarationExpressionCollection ()
		{
			parameterDeclExprs = new ArrayList ();
		}

		//
		// Properties
		//
		public int Count {
			get {
				return parameterDeclExprs.Count;
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
		public void Add (CodeParameterDeclarationExpression value)
		{
			parameterDeclExprs.Add (value);
		}

		public void AddRange (CodeParameterDeclarationExpression [] values)
		{
			foreach (CodeParameterDeclarationExpression ca in values) 
				parameterDeclExprs.Add (ca);

		}

		public void Clear ()
		{
			parameterDeclExprs.Clear ();
		}

		private class Enumerator : IEnumerator {
			private CodeParameterDeclarationExpressionCollection collection;
			private int currentIndex = -1;

			internal Enumerator (CodeParameterDeclarationExpressionCollection collection)
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
			return new CodeParameterDeclarationExpressionCollection.Enumerator (this);
		}

		//
		// IList method implementations
		//
		public int Add (object value)
		{
			return parameterDeclExprs.Add (value);
		}

		public bool Contains (Object value)
		{
			return parameterDeclExprs.Contains (value);
		}

		public int IndexOf (Object value)
		{
			return parameterDeclExprs.IndexOf (value);
		}

		public void Insert (int index, Object value)
		{
			parameterDeclExprs [index] = value;
		}

		public object this[int index] {
			get {
				return parameterDeclExprs [index];
			}

			set {
				parameterDeclExprs [index] = value;
			}
		}

		public void Remove (object value)
		{
			parameterDeclExprs.Remove (value);
		}

		public void RemoveAt (int index)
		{
			parameterDeclExprs.RemoveAt (index);
		}

		//
		// ICollection method implementations
		//
		public void CopyTo (Array array, int index)
		{
			parameterDeclExprs.CopyTo (array, index);
		}

		public object SyncRoot {
			get {
				return parameterDeclExprs.SyncRoot;
			}
		}

		public bool IsReadOnly {
			get {
				return false;
			}
		}

		public bool IsSynchronized {
			get {
				return parameterDeclExprs.IsSynchronized;
			}
		}
	}
}
