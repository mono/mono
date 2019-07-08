//
// System.IO.DefaultWatcher.cs: default IFileWatcher
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (c) 2004 Novell, Inc. (http://www.novell.com)
//

//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace System.IO {
	class DefaultWatcherData {
		public FileSystemWatcher FSW;
		public string Directory;
		public string FileMask; // If NoWildcards, contains the full path to the file.
		public bool IncludeSubdirs;
		public bool Enabled;
		public bool NoWildcards;
		public DateTime DisabledTime;

		public object FilesLock = new object ();
		public Dictionary<string, FileData> Files;
	}

	class FileData {
		public string Directory;
		public FileAttributes Attributes;
		public bool NotExists;
		public DateTime CreationTime;
		public DateTime LastWriteTime;
	}

	class DefaultWatcher : IFileWatcher
	{
		static DefaultWatcher instance;
		static Thread thread;
		static Hashtable watches;

		private DefaultWatcher ()
		{
		}

		// Locked by caller
		public static bool GetInstance (out IFileWatcher watcher)
		{
			if (instance != null) {
				watcher = instance;
				return true;
			}

			instance = new DefaultWatcher ();
			watcher = instance;
			return true;
		}

		public void StartDispatching (object handle)
		{
			var fsw = handle as FileSystemWatcher;
			DefaultWatcherData data;
			lock (this) {
				if (watches == null)
					watches = new Hashtable ();

				if (thread == null) {
					thread = new Thread (new ThreadStart (Monitor));
					thread.IsBackground = true;
					thread.Start ();
				}
			}

			lock (watches) {
				data = (DefaultWatcherData) watches [fsw];
				if (data == null) {
					data = new DefaultWatcherData ();
					data.Files = new Dictionary<string, FileData>();
					watches [fsw] = data;
				}

				data.FSW = fsw;
				data.Directory = fsw.FullPath;
				data.NoWildcards = !fsw.Pattern.HasWildcard;
				if (data.NoWildcards)
					data.FileMask = Path.Combine (data.Directory, fsw.MangledFilter);
				else
					data.FileMask = fsw.MangledFilter;

				data.IncludeSubdirs = fsw.IncludeSubdirectories;
				data.Enabled = true;
				data.DisabledTime = DateTime.MaxValue;
				UpdateDataAndDispatch (data, false);
			}
		}

		public void StopDispatching (object handle)
		{
			var fsw = handle as FileSystemWatcher;
			DefaultWatcherData data;
			lock (this) {
				if (watches == null) return;
			}

			lock (watches) {
				data = (DefaultWatcherData) watches [fsw];
				if (data != null) {
					lock (data.FilesLock) {
						data.Enabled = false;
						data.DisabledTime = DateTime.UtcNow;
					}
				}
			}
		}

		public void Dispose (object handle)
		{
			// does nothing
		}


		void Monitor ()
		{
			int zeroes = 0;

			while (true) {
				Thread.Sleep (750);

				Hashtable my_watches;
				lock (watches) {
					if (watches.Count == 0) {
						if (++zeroes == 20)
							break;
						continue;
					}

					my_watches = (Hashtable) watches.Clone ();
				}

				if (my_watches.Count != 0) {
					zeroes = 0;
					foreach (DefaultWatcherData data in my_watches.Values) {
						bool remove = UpdateDataAndDispatch (data, true);
						if (remove)
							lock (watches)
								watches.Remove (data.FSW);
					}
				}
			}

			lock (this) {
				thread = null;
			}
		}

		bool UpdateDataAndDispatch (DefaultWatcherData data, bool dispatch)
		{
			if (!data.Enabled) {
				return (data.DisabledTime != DateTime.MaxValue &&
					(DateTime.UtcNow - data.DisabledTime).TotalSeconds > 5);
			}

			DoFiles (data, data.Directory, dispatch);
			return false;
		}

		static void DispatchEvents (FileSystemWatcher fsw, FileAction action, string filename)
		{
			RenamedEventArgs renamed = null;

			lock (fsw) {
				fsw.DispatchEvents (action, filename, ref renamed);
				if (fsw.Waiting) {
					fsw.Waiting = false;
					System.Threading.Monitor.PulseAll (fsw);
				}
			}
		}

		static string [] NoStringsArray = new string [0];
		void DoFiles (DefaultWatcherData data, string directory, bool dispatch)
		{
			bool direxists = Directory.Exists (directory);
			if (direxists && data.IncludeSubdirs) {
				foreach (string d in Directory.GetDirectories (directory))
					DoFiles (data, d, dispatch);
			}

			string [] files = null;
			if (!direxists) {
				files = NoStringsArray;
			} else if (!data.NoWildcards) {
				files = Directory.GetFileSystemEntries (directory, data.FileMask);
			} else {
				// The pattern does not have wildcards
				if (File.Exists (data.FileMask) || Directory.Exists (data.FileMask))
					files = new string [] { data.FileMask };
				else
					files = NoStringsArray;
			}

			lock (data.FilesLock) {
				if (data.Enabled)
					IterateAndModifyFilesData (data, directory, dispatch, files);
			}
		}

		void IterateAndModifyFilesData (DefaultWatcherData data, string directory, bool dispatch, string[] files)
		{
			/* Set all as untested */
			foreach (var entry in data.Files) {
				FileData fd = entry.Value;
				if (fd.Directory == directory)
					fd.NotExists = true;
			}

			/* New files */
			foreach (string filename in files) {
				if (!data.Files.TryGetValue (filename, out var fd)) {
					try {
						data.Files.Add (filename, CreateFileData (directory, filename));
					} catch {
						// The file might have been removed in the meanwhile
						data.Files.Remove (filename);
						continue;
					}

					if (dispatch)
						DispatchEvents (data.FSW, FileAction.Added, filename);
				} else if (fd.Directory == directory) {
					fd.NotExists = false;
				}
			}

			if (!dispatch) // We only initialize the file list
				return;

			/* Removed files */
			List<string> removed = null;
			foreach (var entry in data.Files) {
				var filename = entry.Key;
				FileData fd = entry.Value;
				if (fd.NotExists) {
					if (removed == null)
						removed = new List<string> ();

					removed.Add (filename);
					DispatchEvents (data.FSW, FileAction.Removed, filename);
				}
			}

			if (removed != null) {
				foreach (string filename in removed)
					data.Files.Remove (filename);

				removed = null;
			}

			/* Changed files */
			foreach (var entry in data.Files) {
				var filename = entry.Key;
				FileData fd = entry.Value;
				DateTime creation, write;
				try {
					creation = File.GetCreationTime (filename);
					write = File.GetLastWriteTime (filename);
				} catch {
					/* Deleted */
					if (removed == null)
						removed = new List<string> ();

					removed.Add (filename);
					DispatchEvents (data.FSW, FileAction.Removed, filename);
					continue;
				}

				if (creation != fd.CreationTime || write != fd.LastWriteTime) {
					fd.CreationTime = creation;
					fd.LastWriteTime = write;
					DispatchEvents (data.FSW, FileAction.Modified, filename);
				}
			}

			if (removed != null) {
				foreach (string filename in removed)
					data.Files.Remove (filename);
			}

		}

		static FileData CreateFileData (string directory, string filename)
		{
			FileData fd = new FileData ();
			string fullpath = Path.Combine (directory, filename);
			fd.Directory = directory;
			fd.Attributes = File.GetAttributes (fullpath);
			fd.CreationTime = File.GetCreationTime (fullpath);
			fd.LastWriteTime = File.GetLastWriteTime (fullpath);
			return fd;
		}
	}
}
