//
// System.Text.RegularExpressions.GroupCollection
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
	public class GroupCollection: ICollection, IEnumerable
	{
		private Group [] list;

		/* No public constructor */
		internal GroupCollection (int n)
		{
			list = new Group [n];
		}

		public virtual int Count {
			get { return list.Length; }
		}

		public bool IsReadOnly {
			get { return true; }
		}

		public virtual bool IsSynchronized {
			get { return false; }
		}

		public Group this [int i] {
			get {
				if (i < list.Length && i >= 0)
					return list [i];
				else
					return Group.Fail;
			}
		}

		internal void SetValue (Group g, int i)
		{
			list [i] = g;
		}

		public Group this [string groupName] {
			get {
				// The 0th group is the match.
				Match m = (Match) list [0];
				if (m != Match.Empty) {
					int index = m.Regex.GroupNumberFromName (groupName);
					if (index != -1)
						return this [index];
				}

				return Group.Fail;
			}
		}

		public virtual object SyncRoot {
			get { return list; }
		}

		public virtual void CopyTo (Array array, int index)
		{
			list.CopyTo (array, index);
		}

		public virtual IEnumerator GetEnumerator ()
		{
			return list.GetEnumerator ();
		}
	}
}

		
		
