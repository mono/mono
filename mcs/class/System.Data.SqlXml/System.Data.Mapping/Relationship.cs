//
// System.Data.Mapping.Relationship
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2003
//

#if NET_1_2

using System.Data.SqlXml;

namespace System.Data.Mapping {
        public class Relationship  
        {
		#region Properties
	
		[MonoTODO]
		public bool CascadeDelete {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public string ConstraintName { 
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public IDomainConstraint DomainConstraint { 
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public FieldJoinCollection FieldJoins { 
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public Variable FromVariable { 
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public string Name { 
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public RelationshipCollection NestedRelationships { 
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public DataSource OwnerDataSource { 
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public Variable ToVariable { 
			get { throw new NotImplementedException (); }
		}

		#endregion // Properties
        }
}

#endif // NET_1_2
