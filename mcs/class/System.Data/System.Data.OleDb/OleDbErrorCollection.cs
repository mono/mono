//
// System.Data.OleDb.OleDbErrorCollection
//
// Authors:
//   Rodrigo Moya (rodrigo@ximian.com)
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Rodrigo Moya, 2002
// Copyright (C) Tim Coleman, 2002
//

using System.Collections;
using System.Data;
using System.Data.Common;

namespace System.Data.OleDb
{
	public sealed class OleDbErrorCollection : ICollection, IEnumerable
	{
		#region Fields

		ArrayList list;
	
		#endregion // Fields

		#region Properties 

		public int Count {
			get {
				return list.Count;
			}
		}

		public OleDbError this[int index] {
			get {
				return (OleDbError) list[index];
			}
		}

		object ICollection.SyncRoot {
			get {
				return list.SyncRoot;
			}
		}

		bool ICollection.IsSynchronized {
			get {
				return list.IsSynchronized;
			}
		}

		#endregion // Properties

		#region Methods

		internal void Add (OleDbError error)
		{
			list.Add ((object) error);
		}
		
		[MonoTODO]
		public void CopyTo (Array array, int index) 
		{
			((OleDbError[])(list.ToArray ())).CopyTo (array, index);
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
