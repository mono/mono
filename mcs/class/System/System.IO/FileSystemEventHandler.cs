// 
// System.IO.FileSystemEventHandler.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System;

namespace System.IO {
	[Serializable]
	public delegate void FileSystemEventHandler (object sender, FileSystemEventArgs e);
}
