//
// System.Management.QualifierDataCollection
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
	public class QualifierDataCollection : ICollection, IEnumerable
	{
		internal QualifierDataCollection ()
		{
		}

		[MonoTODO]
		public virtual void Add (string qualifierName, object qualifierValue)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual void Add (string qualifierName,
					 object qualifierValue,
					 bool isAmended,
					 bool propagatesToInstance,
					 bool propagatesToSubclass,
					 bool isOverridable)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void CopyTo (QualifierData [] qualifierArray, int index)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void CopyTo (Array array, int index)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public QualifierDataEnumerator GetEnumerator ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual void Remove (string qualifierName)
		{
			throw new NotImplementedException ();
		}

		public int Count {
			[MonoTODO]
			get {
				throw new NotImplementedException ();
			}
		}

		public bool IsSynchronized {
			[MonoTODO]
			get {
				throw new NotImplementedException ();
			}
		}

		public virtual QualifierData this [string qualifierName] {
			[MonoTODO]
			get {
				throw new NotImplementedException ();
			}
		}

		public object SyncRoot {
			[MonoTODO]
			get {
				throw new NotImplementedException ();
			}
		}

		IEnumerator IEnumerable.GetEnumerator ()
		{
			throw new NotImplementedException ();
		}

		public class QualifierDataEnumerator : IEnumerator
		{
			internal QualifierDataEnumerator ()
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

			public QualifierData Current {
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

