//
// System.Text.RegularExpressions.CaptureCollection
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
	public class CaptureCollection: ICollection, IEnumerable
	{
		private ArrayList list;

		/* No public constructor */
		internal CaptureCollection () {
			list = new ArrayList ();
		}

		public virtual int Count {
			get {
				return(list.Count);
			}
		}

		public bool IsReadOnly {
			get {
				return(true);
			}
		}

		public virtual bool IsSynchronized {
			get {
				return(false);
			}
		}

		public Capture this[int i] {
			get {
				if (i < 0 ||
				    i > Count) {
					throw new ArgumentOutOfRangeException ("Index is out of range");
				}
				
				return((Capture)list[i]);
			}
		}

		public virtual object SyncRoot {
			get {
				return(list);
			}
		}

		public virtual void CopyTo (Array array, int index) {
			foreach (object o in list) {
				if (index > array.Length) {
					break;
				}

				array.SetValue (o, index++);
			}
		}

		public virtual IEnumerator GetEnumerator () {
			return(new Enumerator (list));
		}

		internal void Add (object o) {
			list.Add (o);
		}

		internal void Reverse () {
			list.Reverse ();
		}

		private class Enumerator: IEnumerator {
			private IList list;
			private int ptr;

			public Enumerator (IList list) {
				this.list = list;
				Reset ();
			}

			public object Current {
				get {
					if (ptr >= list.Count) {
						throw new InvalidOperationException ();
					}

					return(list[ptr]);
				}
			}

			public bool MoveNext () {
				if (ptr > list.Count) {
					throw new InvalidOperationException ();
				}

				return(++ptr < list.Count);
			}

			public void Reset () {
				ptr = -1;
			}
		}
	}
}

		
		
