// 
// System.IO.FileSystemEventArgs.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System;

namespace System.IO {
	public class FileSystemEventArgs : EventArgs {

		#region Fields

		WatcherChangeTypes changeType;
		string directory;
		string name;

		#endregion // Fields

		#region Constructors

		public FileSystemEventArgs (WatcherChangeTypes changeType, string directory, string name) 
		{
			this.changeType = changeType;
			this.directory = directory;
			this.name = name;
		}
		
		#endregion // Constructors

		#region Properties

		public WatcherChangeTypes ChangeType {
			get { return changeType; }
		}

		public string FullPath {
			get { return Path.Combine (directory, name); }
		}

		public string Name {
			get { return name; }
		}

		#endregion // Properties
	}
}
