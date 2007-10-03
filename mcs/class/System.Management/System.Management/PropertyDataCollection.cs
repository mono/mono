//
// System.Management.PropertyDataCollection
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2003 Ximian, Inc (http://www.ximian.com)
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
	public class PropertyDataCollection : ICollection, IEnumerable
	{
		internal PropertyDataCollection ()
		{
		}

		[MonoTODO]
		public virtual void Add (string propertyName, object propertyValue)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Add (string propertyName, CimType propertyType, bool isArray)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Add (string propertyName, object propertyValue, CimType propertyType)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void CopyTo (PropertyData [] propertyArray, int index)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void CopyTo (Array array, int index)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public PropertyDataEnumerator GetEnumerator ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual void Remove (string propertyName)
		{
			throw new NotImplementedException ();
		}


		[MonoTODO]
		IEnumerator IEnumerable.GetEnumerator ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public int Count {
			get {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public bool IsSynchronized {
			get {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public virtual PropertyData this [string propertyName] {
			get {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public object SyncRoot {
			get {
				throw new NotImplementedException ();
			}
		}

		public class PropertyDataEnumerator : IEnumerator
		{
			internal PropertyDataEnumerator ()
			{
			}

			[MonoTODO]
			public bool MoveNext ()
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			public void Reset ()
			{
				throw new NotImplementedException ();
			}

			public PropertyData Current {
				[MonoTODO]
				get {
					throw new NotImplementedException ();
				}
			}

			object IEnumerator.Current {
				[MonoTODO]
				get {
					throw new NotImplementedException ();
				}
			}
		}
	}
}

