// 
// System.IO.WatcherChangeTypes.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System;

namespace System.IO {
	[Flags]
	[Serializable]
	public enum WatcherChangeTypes {
		All,
		Changed,
		Created,
		Deleted,
		Renamed
	}
}
