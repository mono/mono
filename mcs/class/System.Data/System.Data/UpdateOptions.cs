//
// System.Data.UpdateOptions.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2003
//

#if NET_2_0

namespace System.Data {
	public enum UpdateOptions 
	{
		None,
		DoNotAcceptChanges,
		NotTransacted,
		UpdateChildren
	}
}

#endif // NET_2_0
