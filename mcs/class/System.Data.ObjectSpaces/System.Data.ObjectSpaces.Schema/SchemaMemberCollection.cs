//
// System.Data.ObjectSpaces.Schema.SchemaMemberCollection.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2003
//

#if NET_1_2

using System.Collections;

namespace System.Data.ObjectSpaces.Schema {
	public class SchemaMemberCollection : CollectionBase
	{
		#region Properties

		public SchemaMember this [int index] {
			get { return (SchemaMember) List [index]; }
		}

		[MonoTODO]
		public SchemaMember this [string typeName] {
			get { throw new NotImplementedException (); }
		}

		#endregion // Properties

		#region Methods

		public void Add (SchemaMember member)
		{
			Insert (Count, member);
		}

		public bool Contains (SchemaMember member)
		{
			return List.Contains (member);
		}

		public void CopyTo (SchemaMember[] array, int index)
		{
			List.CopyTo (array, index);
		}

		public int IndexOf (SchemaMember member)
		{
			return List.IndexOf (member);
		}

		public void Insert (int index, SchemaMember member)
		{
			List.Insert (index, member);
		}

		[MonoTODO]
		protected override void OnInsert (int index, object value)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected override void OnRemove (int index, object value)
		{
			throw new NotImplementedException ();
		}

		public void Remove (SchemaMember member)
		{
			List.Remove (member);
		}

		#endregion // Methods
	}
}

#endif // NET_1_2
