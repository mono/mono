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
		private ArrayList list;

		/* No public constructor */
		internal MatchCollection (Match start)
		{
			current = start;
			list = new ArrayList ();
		}

		public virtual int Count {
			get {
				TryToGet (Int32.MaxValue);
				return list.Count;
			}
		}

		public bool IsReadOnly {
			get { return true; }
		}

		public virtual bool IsSynchronized {
			get { return false; }
		}

		private bool TryToGet (int i)
		{
			while (i >= list.Count && current.Success) {
				list.Add (current);
				current = current.NextMatch ();
			}
			return i < list.Count;
		}

		public Match this [int i] {
			get {
				if (i < 0 || !TryToGet (i))
					throw new ArgumentOutOfRangeException ("index out of range", "i");
				return (Match) list [i];
			}
		}

		public virtual object SyncRoot {
			get { return list; }
		}

		public virtual void CopyTo (Array array, int index)
		{
			TryToGet (Int32.MaxValue);
			list.CopyTo (array, index);
		}

		public virtual IEnumerator GetEnumerator ()
		{
			// If !current.Success, the list is fully populated.  So, just use it.
			return current.Success ? new Enumerator (this) : list.GetEnumerator ();
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
					if (index < 0 || index >= coll.list.Count)
						throw new InvalidOperationException ();
					return coll.list [index];
				}
			}

			bool IEnumerator.MoveNext ()
			{
				if (coll.TryToGet (++index))
					return true;
				index = coll.list.Count;
				return false;
			}
		}
	}
}
