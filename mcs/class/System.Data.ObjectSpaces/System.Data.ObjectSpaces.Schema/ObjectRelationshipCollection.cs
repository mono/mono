//
// System.Data.ObjectSpaces.Schema.ObjectRelationshipCollection.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2003
//

#if NET_1_2

using System.Collections;

namespace System.Data.ObjectSpaces.Schema {
	public class ObjectRelationshipCollection : CollectionBase
	{
		#region Properties

		[MonoTODO]
		public ObjectRelationship this [string name] {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public ObjectRelationship this [int index] {
			get { return (ObjectRelationship) List [index]; }
		}

		#endregion // Properties

		#region Methods

		public void Add (ObjectRelationship relationship)
		{
			Insert (Count, relationship);
		}

		public bool Contains (ObjectRelationship relationship)
		{
			return List.Contains (relationship);
		}

		[MonoTODO]
		public ObjectRelationship[] GetChildRelationships (Type type)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public ObjectRelationship[] GetParentRelationships (Type type)
		{
			throw new NotImplementedException ();
		}

		public int IndexOf (ObjectRelationship relationship)
		{
			return List.IndexOf (relationship);
		}

		public void Insert (int index, ObjectRelationship relationship)
		{
			List.Insert (index, relationship);
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

		public void Remove (ObjectRelationship relation)
		{
			List.Remove (relation);
		}

		#endregion // Methods
	}
}

#endif // NET_1_2
