//
// System.Data.Odbc.OdbcErrorCollection
//
// Author:
//   Brian Ritchie (brianlritchie@hotmail.com) 
//
// Copyright (C) Brian Ritchie, 2002
//

using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Data.Common;

namespace System.Data.Odbc
{
	public sealed class OdbcErrorCollection : ICollection, IEnumerable
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

		public OdbcError this[int index] 
		{
			get 
			{
				return (OdbcError) list[index];
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

		public void Add (OdbcError error)
		{
			list.Add ((object) error);
		}
		
		[MonoTODO]
		public void CopyTo (Array array, int index) 
		{
			((OdbcError[])(list.ToArray ())).CopyTo (array, index);
			throw new NotImplementedException ();
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
