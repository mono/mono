//
// System.Text.RegularExpressions.MatchCollection
//
// Authors:
//	Dan Lewis (dlewis@gmx.co.uk)
//	Dick Porter (dick@ximian.com)
//
// (C) 2002 Dan Lewis
// (C) 2004 Novell, Inc.
//

//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Collections;

namespace System.Text.RegularExpressions
{
	[Serializable]
	public class MatchCollection: ICollection, IEnumerable {
		private Match current;

		// Stores all the matches before 'current'.  If !current.Success, it has all the successful matches.
		private ArrayList list;

		/* No public constructor */
		internal MatchCollection (Match start)
		{
			current = start;
			list = new ArrayList ();
		}

		public int Count {
			get { return FullList.Count; }
		}

		public bool IsReadOnly {
			get { return true; }
		}

		public bool IsSynchronized {
			get { return false; }
		}


		public virtual Match this [int i] {
			get {
				if (i < 0 || !TryToGet (i))
					throw new ArgumentOutOfRangeException ("i");
				return i < list.Count ? (Match) list [i] : current;
			}
		}

		public object SyncRoot {
			get { return list; }
		}

		public void CopyTo (Array array, int arrayIndex)
		{
			FullList.CopyTo (array, arrayIndex);
		}

		public IEnumerator GetEnumerator ()
		{
			// If !current.Success, the list is fully populated.  So, just use it.
			return current.Success ? new Enumerator (this) : list.GetEnumerator ();
		}

		// Returns true when: i < list.Count 			 => this [i] == list [i]
		//                    i == list.Count && current.Success => this [i] == current
		private bool TryToGet (int i)
		{
			while (i > list.Count && current.Success) {
				list.Add (current);
				current = current.NextMatch ();
			}
			// Here we have: !(i > list.Count && current.Success)
			// or in a slightly more useful form: i > list.Count => current.Success == false
			return i < list.Count || current.Success;
		}

		private ICollection FullList {
			get {
				if (TryToGet (Int32.MaxValue)) {
					// list.Count == Int32.MaxValue && current.Success
					// i.e., we have more than Int32.MaxValue matches.
					// We can't represent that number with Int32.
					throw new SystemException ("too many matches");
				}
				return list;
			}
		}

		class Enumerator : IEnumerator {
			int index;
			MatchCollection coll;

			internal Enumerator (MatchCollection coll)
			{
				this.coll = coll;
				index = -1;
			}

			void IEnumerator.Reset ()
			{
				index = -1;
			}

			object IEnumerator.Current {
				get {
					if (index < 0)
						throw new InvalidOperationException ("'Current' called before 'MoveNext()'");
					if (index > coll.list.Count)
						throw new SystemException ("MatchCollection in invalid state");
					if (index == coll.list.Count && !coll.current.Success)
						throw new InvalidOperationException ("'Current' called after 'MoveNext()' returned false");
					return index < coll.list.Count ? coll.list [index] : coll.current;
				}
			}

			bool IEnumerator.MoveNext ()
			{
				if (index > coll.list.Count)
					throw new SystemException ("MatchCollection in invalid state");
				if (index == coll.list.Count && !coll.current.Success)
					return false;
				return coll.TryToGet (++index);
			}
		}
	}
}
