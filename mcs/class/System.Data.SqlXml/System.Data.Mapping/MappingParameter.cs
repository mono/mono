//
// System.Data.Mapping.MappingParameter
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2003
//

#if NET_1_2

using System.Data.SqlXml;
using System.Xml.Schema;

namespace System.Data.Mapping {
        public class MappingParameter  
        {
		#region Properties
	
		[MonoTODO]
		public XmlSchemaBuiltInType DataType {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public string Default { 
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public string Name { 
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public string NullValue { 
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public MappingSchema OwnerMappingSchema { 
			get { throw new NotImplementedException (); }
		}

		#endregion // Properties
        }
}

#endif // NET_1_2
