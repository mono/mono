//
// System.CodeDOM CodeClassMemberCollection Class implementation
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) 2001 Ximian, Inc.
//

namespace System.CodeDOM {

	using System.Collections;
	
	public class CodeClassMemberCollection : IList, ICollection, IEnumerable {

		ArrayList classMembers;
		
		//
		// Constructors
		//
		public CodeClassMemberCollection ()
		{
			classMembers = new ArrayList ();
		}

		//
		// Properties
		//
		public int Count {
			get {
				return classMembers.Count;
			}
		}

		//
		// Methods
		//
		public void Add (CodeClassMember value)
		{
			classMembers.Add (value);
		}

		public void AddRange (CodeClassMember [] values)
		{
			foreach (CodeClassMember ca in values) 
				classMembers.Add (ca);

		}

		public void Clear ()
		{
			classMembers.Clear ();
		}

		private class Enumerator : IEnumerator {
			private CodeClassMemberCollection collection;
			private int currentIndex = -1;

			internal Enumerator (CodeClassMemberCollection collection)
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
			return new CodeClassMemberCollection.Enumerator (this);
		}

		//
		// IList method implementations
		//
		public int Add (object value)
		{
			return classMembers.Add (value);
		}

		public bool Contains (Object value)
		{
			return classMembers.Contains (value);
		}

		public int IndexOf (Object value)
		{
			return classMembers.IndexOf (value);
		}

		public void Insert (int index, Object value)
		{
			classMembers [index] = value;
		}

		public object this[int index] {
			get {
				return classMembers [index];
			}

			set {
				classMembers [index] = value;
			}
		}

		public void Remove (object value)
		{
			classMembers.Remove (value);
		}

		public void RemoveAt (int index)
		{
			classMembers.RemoveAt (index);
		}

		//
		// ICollection method implementations
		//
		public void CopyTo (Array array, int index)
		{
			classMembers.CopyTo (array, index);
		}

		public object SyncRoot {
			get {
				return classMembers.SyncRoot;
			}
		}

		public bool IsReadOnly {
			get {
				return false;
			}
		}

		public bool IsSynchronized {
			get {
				return classMembers.IsSynchronized;
			}
		}
	}
}
