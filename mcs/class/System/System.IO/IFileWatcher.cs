// 
// System.IO.IFileWatcher.cs
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (c) 2004 Novell, Inc. (http://www.novell.com)
//

namespace System.IO {
	interface IFileWatcher {
		void StartDispatching (FileSystemWatcher fsw);
		void StopDispatching (FileSystemWatcher fsw);
	}
}

