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
using System.ComponentModel;
using System.Data;
using System.Data.Common;

namespace System.Data.OleDb
{
	[ListBindableAttribute ( false)]	

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
		
		public void CopyTo (Array array, int index) 
		{
		        if (array == null)
                                throw new ArgumentNullException("array");
                                                                                                    
                        if ((index < array.GetLowerBound (0)) || (index > array.GetUpperBound (0)))
                                throw new ArgumentOutOfRangeException("index");
                                                                                                    
                        // is the check for IsFixedSize required?
                        if ((array.IsFixedSize) || (index + this.Count > array.GetUpperBound (0)))
                                throw new ArgumentException("array");
                                                                                                    
                        ((OleDbError[])(list.ToArray ())).CopyTo (array, index);
                                                                                                    

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
