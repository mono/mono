//
// System.Data.Mapping.DataSourceCollection
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2003
//

#if NET_1_2

using System.Collections;
using System.Data.SqlXml;

namespace System.Data.Mapping {
        public class DataSourceCollection : ReadOnlyCollectionBase
        {
		#region Properties
	
		[MonoTODO]
		public DataSource this [int index] {
			get { throw new NotImplementedException (); }
		}
		
		[MonoTODO]
		public DataSource this [string name] {
			get { throw new NotImplementedException (); }
		}

		#endregion // Properties

		#region Methods

		[MonoTODO]
		public bool Contains (DataSource dataSource)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void CopyTo (DataSource[] array, int index)
		{
			throw new NotImplementedException ();
		}

		#endregion // Methods
        }
}

#endif // NET_1_2
