//
// System.Data.Common.GroupByBehavior.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2003
//

#if NET_2_0

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
