//
// ObjectMaterializer.cs
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
using System.Data.Common;
using System.Linq;

namespace System.Data.Linq.SqlClient.Implementation
{
	public abstract class ObjectMaterializer<TDataReader> where TDataReader : DbDataReader
	{
        [MonoTODO]
        public ObjectMaterializer()
        {
            throw new NotImplementedException();
        }

		[MonoTODO]
		public static IEnumerable<TOutput> Convert<TOutput> (IEnumerable source)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static IGrouping<TKey, TElement> CreateGroup<TKey, TElement> (TKey key, IEnumerable<TElement> items)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static IOrderedEnumerable<TElement> CreateOrderedEnumerable<TElement> (IEnumerable<TElement> items)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static Exception ErrorAssignmentToNull (Type type)
		{
			throw new NotImplementedException ();
		}

		// instance members

		[MonoTODO]
		public object [] Arguments;

		[MonoTODO]
		public DbDataReader BufferReader;

		[MonoTODO]
		public TDataReader DataReader;

		[MonoTODO]
		public object [] Globals;

		[MonoTODO]
		public object [] Locals;

		[MonoTODO]
		public int[] Ordinals;

		[MonoTODO]
		public abstract bool CanDeferLoad { get; }

		public abstract IEnumerable ExecuteSubQuery (int iSubQuery, object [] args);
		public abstract IEnumerable<T> GetLinkSource<T> (int globalLink, int localFactory, object [] keyValues);
		public abstract IEnumerable<T> GetNestedLinkSource<T> (int globalLink, int localFactory, object instance);
		public abstract object InsertLookup (int globalMetaType, object instance);
		public abstract bool Read ();
		public abstract void SendEntityMaterialized (int globalMetaType, object instance);
	}
}
