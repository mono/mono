//
// System.Data.ResultSetOptions.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2003
//

#if NET_1_2

namespace System.Data {
	public enum ResultSetOptions 
	{
		None,
		Updatable,
		Scrollable,
		Sensitive,
		Insensitive
	}
}

#endif // NET_1_2
