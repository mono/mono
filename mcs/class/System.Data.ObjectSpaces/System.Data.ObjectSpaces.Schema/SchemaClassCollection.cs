//
// System.Data.ObjectSpaces.Schema.SchemaClassCollection.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2003
//

#if NET_1_2

using System.Collections;

namespace System.Data.ObjectSpaces.Schema {
	public class SchemaClassCollection : CollectionBase
	{
		#region Properties

		public SchemaClass this [int index] {
			get { return (SchemaClass) List [index]; }
		}

		[MonoTODO]
		public SchemaClass this [string typeName] {
			get { throw new NotImplementedException (); }
		}

		#endregion // Properties

		#region Methods

		public void Add (SchemaClass schemaClass)
		{
			Insert (Count, schemaClass);
		}

		public bool Contains (SchemaClass schemaClass)
		{
			return List.Contains (schemaClass);
		}

		public void CopyTo (SchemaClass[] array, int index)
		{
			List.CopyTo (array, index);
		}

		public int IndexOf (SchemaClass schemaClass)
		{
			return List.IndexOf (schemaClass);
		}

		public void Insert (int index, SchemaClass schemaClass)
		{
			List.Insert (index, schemaClass);
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

		public void Remove (SchemaClass schemaClass)
		{
			List.Remove (schemaClass);
		}

		#endregion // Methods
	}
}

#endif // NET_1_2
