//
// System.Net.CookieCollection
//
// Author:
//   Lawrence Pit (loz@cable.a2000.nl)
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
using System.Runtime.Serialization;

namespace System.Net 
{
	[Serializable]
	public class CookieCollection : ICollection, IEnumerable
	{
		private ArrayList list = new ArrayList ();
		
		// ctor
		public CookieCollection () 
		{
		}

		// ICollection

		public int Count {
			get { return list.Count; }
		}

		public bool IsSynchronized {
			get { return false; }
		}

		public Object SyncRoot {
			get { return this; }
		}

		public void CopyTo (Array array, int arrayIndex)
		{
			list.CopyTo (array, arrayIndex);
		}


		// IEnumerable

		public IEnumerator GetEnumerator ()
		{
			return list.GetEnumerator ();
		}
		
		
		// This
		
		// LAMESPEC: So how is one supposed to create a writable CookieCollection 
		// instance?? We simply ignore this property, as this collection is always
		// writable.
		public bool IsReadOnly {
			get { return true; }
		}		
		
		// LAMESPEC: Which exception should we throw when the read only 
		// property is set to true??
		public void Add (Cookie cookie) 
		{
			if (cookie == null)
				throw new ArgumentNullException ("cookie");
			int pos = list.IndexOf (cookie);
			if (pos == -1)
				list.Add (cookie);
			else 
				list [pos] = cookie;
		}		
		
		// LAMESPEC: Which exception should we throw when the read only 
		// property is set to true??
		public void Add (CookieCollection cookies) 
		{
			if (cookies == null)
				throw new ArgumentNullException ("cookies");
				
			IEnumerator enumerator = cookies.list.GetEnumerator ();
			while (enumerator.MoveNext ())
				Add ((Cookie) enumerator.Current);
		}
		
		public Cookie this [int index] {
			get {
				if (index < 0 || index >= list.Count)
					throw new ArgumentOutOfRangeException ("index");
				return (Cookie) list [index];
			}
		}		
				
		public Cookie this [string name] {
			get {
				lock (this) {
					IEnumerator enumerator = list.GetEnumerator ();
					while (enumerator.MoveNext ())		
						if (String.Compare (((Cookie) enumerator.Current).Name, name, true) == 0)
							return (Cookie) enumerator.Current;
				}
				return null;
			}
		}		
		

	} // CookieCollection

} // System.Net

