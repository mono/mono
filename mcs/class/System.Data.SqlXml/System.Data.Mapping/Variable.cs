//
// System.Data.Mapping.Variable
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2003
//

#if NET_1_2

using System.Data.SqlXml;

namespace System.Data.Mapping {
        public class Variable  
        {
		#region Properties
	
		[MonoTODO]
		public ConditionCollection Conditions {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public IDomainStructure DomainStructure { 
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public FieldCollection Fields { 
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public string Name { 
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public DataSource OwnerDataSource { 
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public string Select { 
			get { throw new NotImplementedException (); }
		}

		#endregion // Properties
        }
}

#endif // NET_1_2
