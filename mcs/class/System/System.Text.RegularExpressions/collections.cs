//
// assembly:	System
// namespace:	System.Text.RegularExpressions
// file:	collections.cs
//
// author:	Dan Lewis (dlewis@gmx.co.uk)
// 		(c) 2002

using System;
using System.Collections;

namespace System.Text.RegularExpressions {
	public abstract class RegexCollectionBase : ICollection, IEnumerable {
		public int Count {
			get { return list.Count; }
		}

		public bool IsReadOnly {
			get { return true; }	// FIXME
		}

		public bool IsSynchronized {
			get { return false; }	// FIXME
		}

		public object SyncRoot {
			get { return list; }	// FIXME
		}

		public void CopyTo (Array array, int index) {
			foreach (Object o in list) {
				if (index > array.Length)
					break;
				
				array.SetValue (o, index ++);
			}
		}

		public IEnumerator GetEnumerator () {
			return new Enumerator (list);
		}

		// internal methods

		internal RegexCollectionBase () {
			list = new ArrayList ();
		}

		internal void Add (Object o) {
			list.Add (o);
		}

		// IEnumerator implementation

		private class Enumerator : IEnumerator {
			public Enumerator (IList list) {
				this.list = list;
				Reset ();
			}

			public object Current {
				get {
					if (ptr >= list.Count)
						throw new InvalidOperationException ();

					return list[ptr];
				}
			}

			public bool MoveNext () {
				if (ptr > list.Count)
					throw new InvalidOperationException ();
				
				return ++ ptr < list.Count;
			}

			public void Reset () {
				ptr = -1;
			}

			private IList list;
			private int ptr;
		}

		// protected fields

		protected ArrayList list;
	}

	[Serializable]
	public class CaptureCollection : RegexCollectionBase, ICollection, IEnumerable {
		public Capture this[int i] {
			get { return (Capture)list[i]; }
		}

		internal CaptureCollection (Capture cap) {
			while (cap != null) {
				if (cap.IsDefined)
					Add (cap);

				cap = cap.Previous;
			}
			
			list.Reverse ();
		}
	}

	public class GroupCollection : RegexCollectionBase, ICollection, IEnumerable {
		public Group this[int i] {
			get { return (Group)list[i]; }
		}
		
		internal GroupCollection () {
		}
	}

	public class MatchCollection : RegexCollectionBase, ICollection, IEnumerable {
		public Match this[int i] {
			get { return (Match)list[i]; }
		}

		internal MatchCollection () {
		}
	}
}
