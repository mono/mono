//
// System.Data.Mapping.Condition
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2003
//

#if NET_1_2

using System.Data.SqlXml;

namespace System.Data.Mapping {
        public class Condition
        {
		#region Properties
	
		[MonoTODO]
		public Field LeftField {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public MappingConditionOperator Operator { 
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public Variable OwnerVariable { 
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public string RightConstant { 
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public Field RightField { 
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public MappingArgumentType RightOperandType { 
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public MappingParameter RightParameter { 
			get { throw new NotImplementedException (); }
		}

		#endregion // Properties
        }
}

#endif // NET_1_2
