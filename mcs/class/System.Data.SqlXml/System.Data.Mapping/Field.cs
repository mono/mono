//
// System.Data.Mapping.Field
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2003
//

#if NET_1_2

using System.Data.SqlXml;

namespace System.Data.Mapping {
        public class Field  
        {
		#region Properties
	
		[MonoTODO]
		public IDomainField DomainField {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public string Name { 
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public Variable OwnerVariable { 
			get { throw new NotImplementedException (); }
		}

		#endregion // Properties
        }
}

#endif // NET_1_2
