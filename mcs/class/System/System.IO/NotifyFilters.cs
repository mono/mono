// 
// System.IO.NotifyFilters.cs
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
	public enum NotifyFilters {
		Attributes,
		CreationTime,
		DirectoryName,
		FileName,
		LastAccess,
		LastWrite,
		Security,
		Size
	}
}
