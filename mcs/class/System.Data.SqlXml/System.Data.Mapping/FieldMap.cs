//
// System.Data.Mapping.FieldMap
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2003
//

#if NET_1_2

using System.Data.SqlXml;

namespace System.Data.Mapping {
        public class FieldMap  
        {
		#region Properties
	
		[MonoTODO]
		public string NullValue {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public Map OwnerMap {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public string SourceConstant {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public Field SourceField {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public MappingParameter SourceParameter {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public MappingArgumentType SourceType {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public string TargetConstant {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public IDomainField TargetDomainField {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public MappingParameter TargetParameter {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public MappingArgumentType TargetType {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public bool UseForConcurrency {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public MappingAccess UseNull {
			get { throw new NotImplementedException (); }
		}

		#endregion // Properties
        }
}

#endif // NET_1_2
