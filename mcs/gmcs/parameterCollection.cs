//
// System.CodeDOM CodeParameterCollection Class implementation
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) 2001 Ximian, Inc.
//

namespace Mono.CSharp {

	using System.Collections;
	using System;
	
	public class ParameterCollection : IList, ICollection, IEnumerable {

		ArrayList parameters;
		
		//
		// Constructors
		//
		public ParameterCollection ()
		{
			parameters = new ArrayList ();
		}

		//
		// Properties
		//
		public int Count {
			get {
				return parameters.Count;
			}
		}

		//
		// Methods
		//
		public void Add (Parameter value)
		{
			parameters.Add (value);
		}

		public void AddRange (Parameter [] values)
		{
			foreach (Parameter ca in values) 
				parameters.Add (ca);

		}

		public void Clear ()
		{
			parameters.Clear ();
		}

		private class Enumerator : IEnumerator {
			private ParameterCollection collection;
			private int currentIndex = -1;

			internal Enumerator (ParameterCollection collection)
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
			return new ParameterCollection.Enumerator (this);
		}

		//
		// IList method implementations
		//
		public int Add (object value)
		{
			return parameters.Add (value);
		}

		public bool Contains (Object value)
		{
			return parameters.Contains (value);
		}

		public int IndexOf (Object value)
		{
			return parameters.IndexOf (value);
		}

		public void Insert (int index, Object value)
		{
			parameters [index] = value;
		}

		public object this[int index] {
			get {
				return parameters [index];
			}

			set {
				parameters [index] = value;
			}
		}

		public void Remove (object value)
		{
			parameters.Remove (value);
		}

		public void RemoveAt (int index)
		{
			parameters.RemoveAt (index);
		}

		//
		// ICollection method implementations
		//
		public void CopyTo (Array array, int index)
		{
			parameters.CopyTo (array, index);
		}

		public object SyncRoot {
			get {
				return parameters.SyncRoot;
			}
		}

		public bool IsReadOnly {
			get {
				return false;
			}
		}

		public bool IsSynchronized {
			get {
				return parameters.IsSynchronized;
			}
		}

		public bool IsFixedSize {
			get {
				return false;
			}
		}
	}
}
