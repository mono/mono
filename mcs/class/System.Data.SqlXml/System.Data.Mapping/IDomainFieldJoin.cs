//
// System.Data.Mapping.IDomainFieldJoin
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2003
//

#if NET_2_0

namespace System.Data.Mapping {
        public interface IDomainFieldJoin
        {
		#region Properties

		IDomainField FromDomainField { get; } 
		IDomainField ToDomainField { get; } 

		#endregion // Properties
        }
}

#endif // NET_2_0
