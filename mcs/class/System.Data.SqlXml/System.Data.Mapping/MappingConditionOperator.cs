//
// System.Data.Mapping.MappingConditionOperator
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2003
//

#if NET_1_2

namespace System.Data.Mapping {
        public enum MappingConditionOperator 
        {
		LessThen,
		GreaterThen,
		LessThenOrEqual,
		GreaterThenOrEqual,
		Equal,
		NotEqual,
		Like,
		NotLike,
		IsNull,
		IsNotNull
        }
}

#endif // NET_1_2
