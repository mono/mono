//
// Mono.Data.MySql.MySqlErrorCollection
//
// Author:
//    Daniel Morgan <danmorg@sc.rr.com>
//
// (c)Copyright 2002 Daniel Morgan
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
using System.ComponentModel;
using System.Data;
using System.Data.Common;

namespace Mono.Data.MySql
{
	public sealed class MySqlErrorCollection : ICollection, IEnumerable
	{
		#region Fields

		ArrayList list = new ArrayList ();
	
		#endregion // Fields

		#region Properties 

		public int Count 
		{
			get 
			{
				return list.Count;
			}
		}

		public MySqlError this[int index] 
		{
			get 
			{
				return (MySqlError) list[index];
			}
		}

		object ICollection.SyncRoot 
		{
			get 
			{
				return list.SyncRoot;
			}
		}

		bool ICollection.IsSynchronized 
		{
			get 
			{
				return list.IsSynchronized;
			}
		}

		#endregion // Properties

		#region Methods

		public void Add (MySqlError error)
		{
			list.Add ((object) error);
		}
		
		[MonoTODO]
		public void CopyTo (Array array, int index) 
		{
			((MySqlError[])(list.ToArray ())).CopyTo (array, index);
		}

		public IEnumerator GetEnumerator ()
		{
			return list.GetEnumerator ();
		}

		IEnumerator IEnumerable.GetEnumerator ()
		{
			return GetEnumerator ();
		}

		#endregion // Methods
	}
}
