//
// System.Management.MethodDataCollection
//
// Authors:
//	Gert Driesen (drieseng@users.sourceforge.net)
//
// (C) 2006 Gert Driesen
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

namespace System.Management
{
	[MonoTODO ("System.Management is not implemented")]
	public class MethodDataCollection : ICollection, IEnumerable
	{
		internal MethodDataCollection ()
		{
		}

		public int Count {
			get {
				throw new NotImplementedException ();
			}
		}

		public bool IsSynchronized {
			get {
				throw new NotImplementedException ();
			}
		}

		public virtual MethodData this [string methodName] {
			get {
				throw new NotImplementedException ();
			}
		}

		public object SyncRoot {
			get {
				throw new NotImplementedException ();
			}
		}

		public virtual void Add (string methodName)
		{
			throw new NotImplementedException ();
		}

		public virtual void Add (string methodName, ManagementBaseObject inParameters, ManagementBaseObject outParameters)
		{
			throw new NotImplementedException ();
		}

		public void CopyTo (MethodData [] methodArray, int index)
		{
			throw new NotImplementedException ();
		}

		public void CopyTo (Array array, int index)
		{
			throw new NotImplementedException ();
		}

		public MethodDataEnumerator GetEnumerator ()
		{
			throw new NotImplementedException ();
		}

		public virtual void Remove (string methodName)
		{
			throw new NotImplementedException ();
		}

		IEnumerator IEnumerable.GetEnumerator ()
		{
			return GetEnumerator ();
		}

		public class MethodDataEnumerator : IEnumerator
		{
			internal MethodDataEnumerator ()
			{
			}

			public bool MoveNext ()
			{
				throw new NotImplementedException ();
			}

			public void Reset ()
			{
				throw new NotImplementedException ();
			}

			public MethodData Current {
				get {
					throw new NotImplementedException ();
				}
			}

			object IEnumerator.Current {
				get {
					return Current;
				}
			}
		}
	}
}
