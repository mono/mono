//
// System.Data.Mapping.DataSource
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2003
//

#if NET_1_2

using System.Data.SqlXml;

namespace System.Data.Mapping {
        public class DataSource  
        {
		#region Properties
	
		[MonoTODO]
		public MappingDirection Direction { 
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public string name { 
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public MappingSchema OwnerMappingSchema { 
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public RelationshipCollection Relationships { 
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public IDomainSchema Schema {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public string SourceUri {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public MappingDataSourceType Type {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public VariableCollection Variables {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public bool WriteInline {
			get { throw new NotImplementedException (); }
		}

		#endregion // Properties
        }
}

#endif // NET_1_2
