//
// System.Data.Mapping.Map
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2003
//

#if NET_1_2

using System.Data.SqlXml;

namespace System.Data.Mapping {
        public class Map  
        {
		#region Properties
	
		[MonoTODO]
		public MappingAccess Access { 
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public Field ContentsMap { 
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public FieldMapCollection FieldMaps { 
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public Field OverflowMap {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public MappingSchema OwnerMappingSchema {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public Variable SourceVariable {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public string TargetSelect {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public IDomainStructure TargetStructure {
			get { throw new NotImplementedException (); }
		}

		#endregion // Properties
        }
}

#endif // NET_1_2
