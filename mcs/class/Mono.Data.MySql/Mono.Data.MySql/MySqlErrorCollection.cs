//
// Mono.Data.MySql.MySqlErrorCollection
//
// Author:
//    Daniel Morgan <danmorg@sc.rr.com>
//
// (c)Copyright 2002 Daniel Morgan
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
