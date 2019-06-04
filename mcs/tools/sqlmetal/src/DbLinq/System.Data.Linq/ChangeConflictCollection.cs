//
// ChangeConflictCollection.cs
//
// Author:
//   Atsushi Enomoto  <atsushi@ximian.com>
//
// Copyright (C) 2008 Novell, Inc.
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
using System.Collections.Generic;

namespace System.Data.Linq
{
	public sealed class ChangeConflictCollection : ICollection<ObjectChangeConflict>, ICollection, IEnumerable<ObjectChangeConflict>, IEnumerable

	{
		internal ChangeConflictCollection ()
		{
		}

		[MonoTODO]
		public int Count {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public ObjectChangeConflict this [int index] {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		bool ICollection<ObjectChangeConflict>.IsReadOnly {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		bool ICollection.IsSynchronized {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		object ICollection.SyncRoot {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public void Clear ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public bool Contains (ObjectChangeConflict item)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void CopyTo (ObjectChangeConflict [] array, int arrayIndex)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public IEnumerator<ObjectChangeConflict> GetEnumerator ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public bool Remove (ObjectChangeConflict item)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void ResolveAll (RefreshMode mode)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void ResolveAll (RefreshMode mode, bool autoResolveDeletes)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		void ICollection<ObjectChangeConflict>.Add (ObjectChangeConflict item)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		void ICollection.CopyTo (Array array, int index)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		IEnumerator IEnumerable.GetEnumerator ()
		{
			throw new NotImplementedException ();
		}

	}
}
