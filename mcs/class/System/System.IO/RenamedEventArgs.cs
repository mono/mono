// 
// System.IO.RenamedEventArgs.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System;

namespace System.IO {
	public class RenamedEventArgs : FileSystemEventArgs {

		#region Fields

		string oldName;

		#endregion // Fields

		#region Constructors

		public RenamedEventArgs (WatcherChangeTypes changeType, string directory, string name, string oldName)
			: base (changeType, directory, name)
		{
			this.oldName = oldName;
		}
		
		#endregion // Constructors

		#region Properties

		public string OldFullPath {
			[MonoTODO]
			get { throw new NotImplementedException (); }
		}

		public string OldName {
			get { return oldName; }
		}

		#endregion // Properties
	}
}
