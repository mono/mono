//
// System.Data.Mapping.RelationshipCollection
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2003
//

#if NET_1_2

using System.Data.SqlXml;
using System.Collections;

namespace System.Data.Mapping {
        public class RelationshipCollection : ReadOnlyCollectionBase
        {
		#region Properties
	
		[MonoTODO]
		public Relationship this [int index] {
			get { throw new NotImplementedException (); }
		}
		
		#endregion // Properties

		#region Methods

		[MonoTODO]
		public bool Contains (Relationship dataSource)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void CopyTo (Relationship[] array, int index)
		{
			throw new NotImplementedException ();
		}

		#endregion // Methods
        }
}

#endif // NET_1_2
