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

		
		
