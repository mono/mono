//
// System.Data.Common.GroupByBehavior.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2003
//

#if NET_1_2

namespace System.Data.Common {
	public enum GroupByBehavior 
	{
		ExactMatch,
		MustContainAll,
		NotSupported,
		Unknown,
		Unrelated
	}
}

#endif
