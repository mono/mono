//
// System.Data.Mapping.RelationshipMapCollection
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
        public class RelationshipMapCollection : ReadOnlyCollectionBase
        {
		#region Properties
	
		[MonoTODO]
		public RelationshipMap this [int index] {
			get { throw new NotImplementedException (); }
		}
		
		#endregion // Properties

		#region Methods

		[MonoTODO]
		public bool Contains (RelationshipMap dataSource)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void CopyTo (RelationshipMap[] array, int index)
		{
			throw new NotImplementedException ();
		}

		#endregion // Methods
        }
}

#endif // NET_1_2
