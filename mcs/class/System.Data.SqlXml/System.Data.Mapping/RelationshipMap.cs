//
// System.Data.Mapping.RelationshipMap
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2003
//

#if NET_2_0

using System.Data.SqlXml;

namespace System.Data.Mapping {
        public class RelationshipMap
        {
		#region Properties
	
		[MonoTODO]
		public MappingSchema OwnerMappingSchema {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public Relationship SourceRelationship {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public IDomainConstraint TargetConstraint { 
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public string TargetConstraintName { 
			get { throw new NotImplementedException (); }
		}

		#endregion // Properties
        }
}

#endif // NET_2_0
