//
// System.Data.Mapping.MapCollection
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
        public class MapCollection : ReadOnlyCollectionBase
        {
		#region Properties
	
		[MonoTODO]
		public Map this [int index] {
			get { throw new NotImplementedException (); }
		}
		
		#endregion // Properties

		#region Methods

		[MonoTODO]
		public bool Contains (Map dataSource)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void CopyTo (Map[] array, int index)
		{
			throw new NotImplementedException ();
		}

		#endregion // Methods
        }
}

#endif // NET_1_2
