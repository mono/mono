// 
// System.IO.WaitForChangedResult.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System;

namespace System.IO {
	public struct WaitForChangedResult {

		#region Fields

		WatcherChangeTypes changeType;
		string name;
		string oldName;
		bool timedOut;

		#endregion // Fields
		
		#region Properties

		public WatcherChangeTypes ChangeType {
			get { return changeType; }
			set { changeType = value; }
		}

		public string Name {
			get { return name; }
			set { name = value; }
		}

		public string OldName {
			get { return oldName; }
			set { oldName = value; }
		}

		public bool TimedOut {
			get { return timedOut; }
			set { timedOut = value; }
		}

		#endregion // Properties
	}
}
