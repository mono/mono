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
		Attributes = 4,
		CreationTime = 64,
		DirectoryName = 2,
		FileName = 1,
		LastAccess = 32,
		LastWrite = 16,
		Security = 256,
		Size = 8
	}
}
