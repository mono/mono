// 
// System.IO.DefaultWatcher.cs: windows IFileWatcher
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (c) 2004 Novell, Inc. (http://www.novell.com)
//

namespace System.IO {
	class DefaultWatcher : IFileWatcher
	{
		private DefaultWatcher ()
		{
		}
		
		public static bool GetInstance (out IFileWatcher watcher)
		{
			throw new NotSupportedException ();
		}
		
		public void StartDispatching (FileSystemWatcher fsw)
		{
		}

		public void StopDispatching (FileSystemWatcher fsw)
		{
		}
	}
}

