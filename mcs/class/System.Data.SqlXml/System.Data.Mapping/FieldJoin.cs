//
// System.Data.Mapping.FieldJoin
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2003
//

#if NET_1_2

using System.Data.SqlXml;

namespace System.Data.Mapping {
        public class FieldJoin
        {
		#region Properties
	
		[MonoTODO]
		public Field FromField {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public string FromFieldName { 
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public Relationship OwnerRelationship { 
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public Field ToField {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public string ToFieldName { 
			get { throw new NotImplementedException (); }
		}

		#endregion // Properties
        }
}

#endif // NET_1_2
