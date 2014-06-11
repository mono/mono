// 
// System.IO.KeventWatcher.cs: interface with osx kevent
//
// Authors:
//	Geoff Norton (gnorton@customerdna.com)
//  Cody Russell (cody@xamarin.com)
//
// (c) 2004 Geoff Norton
// Copyright 2014 Xamarin Inc
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
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace System.IO {

        [Flags]
        enum EventFlags : ushort {
                Add         = 0x0001,
                Delete      = 0x0002,
                Enable      = 0x0004,
                Disable     = 0x0008,
                OneShot     = 0x0010,
                Clear       = 0x0020,
                Receipt     = 0x0040,
                Dispatch    = 0x0080,

                Flag0       = 0x1000,
                Flag1       = 0x2000,
                SystemFlags = unchecked (0xf000),
                        
                // Return values.
                EOF         = 0x8000,
                Error       = 0x4000,
        }
        
        enum EventFilter : short {
                Read = -1,
                Write = -2,
                Aio = -3,
                Vnode = -4,
                Proc = -5,
                Signal = -6,
                Timer = -7,
                MachPort = -8,
                FS = -9,
                User = -10,
                VM = -11
        }

	enum FilterFlags : uint {
                ReadPoll          = EventFlags.Flag0,
                ReadOutOfBand     = EventFlags.Flag1,
                ReadLowWaterMark  = 0x00000001,

                WriteLowWaterMark = ReadLowWaterMark,

                NoteTrigger       = 0x01000000,
                NoteFFNop         = 0x00000000,
                NoteFFAnd         = 0x40000000,
                NoteFFOr          = 0x80000000,
                NoteFFCopy        = 0xc0000000,
                NoteFFCtrlMask    = 0xc0000000,
                NoteFFlagsMask    = 0x00ffffff,
                                  
                VNodeDelete       = 0x00000001,
                VNodeWrite        = 0x00000002,
                VNodeExtend       = 0x00000004,
                VNodeAttrib       = 0x00000008,
                VNodeLink         = 0x00000010,
                VNodeRename       = 0x00000020,
                VNodeRevoke       = 0x00000040,
                VNodeNone         = 0x00000080,
                                  
                ProcExit          = 0x80000000,
                ProcFork          = 0x40000000,
                ProcExec          = 0x20000000,
                ProcReap          = 0x10000000,
                ProcSignal        = 0x08000000,
                ProcExitStatus    = 0x04000000,
                ProcResourceEnd   = 0x02000000,

                // iOS only
                ProcAppactive     = 0x00800000,
                ProcAppBackground = 0x00400000,
                ProcAppNonUI      = 0x00200000,
                ProcAppInactive   = 0x00100000,
                ProcAppAllStates  = 0x00f00000,

                // Masks
                ProcPDataMask     = 0x000fffff,
                ProcControlMask   = 0xfff00000,

                VMPressure        = 0x80000000,
                VMPressureTerminate = 0x40000000,
                VMPressureSuddenTerminate = 0x20000000,
                VMError           = 0x10000000,
                TimerSeconds      =    0x00000001,
                TimerMicroSeconds =   0x00000002,
                TimerNanoSeconds  =   0x00000004,
                TimerAbsolute     =   0x00000008,
        }

	[StructLayout(LayoutKind.Sequential)]
	struct kevent : IDisposable {
		public int ident;
		public EventFilter filter;
		public EventFlags flags;
		public FilterFlags fflags;
		public IntPtr data;
		public IntPtr udata;

		public void Dispose ()
		{
			if (udata != IntPtr.Zero)
				Marshal.FreeHGlobal (udata);
		}
	}

	struct timespec {
		public int tv_sec;
		public int tv_usec;
	}

	class KeventFileData {
		public FileSystemInfo fsi;
		public DateTime LastAccessTime;
		public DateTime LastWriteTime;

		public KeventFileData(FileSystemInfo fsi, DateTime LastAccessTime, DateTime LastWriteTime) {
			this.fsi = fsi;
			this.LastAccessTime = LastAccessTime;
			this.LastWriteTime = LastWriteTime;
		}
	}

	class KeventData {
                public FileSystemWatcher FSW;
                public string Path;
                public string FileMask;
                public bool IncludeSubdirs;
                public bool Enabled;
                public Hashtable DirEntries;
                public kevent ev;
                public int fd;
                public bool IsDirectory;
	}

	class KeventWatcher : IFileWatcher
	{
		static bool failed;
		static KeventWatcher instance;
		static Hashtable watches;  // <FileSystemWatcher, KeventData>
		static Thread thread;
		static int conn;
		static bool stop;

		readonly Dictionary<string, KeventData> filenamesDict = new Dictionary<string, KeventData> ();
		readonly Dictionary<int, KeventData> fdsDict = new Dictionary<int, KeventData> ();
		readonly List<int> removeQueue = new List<int> ();
		readonly List<int> rescanQueue = new List<int> ();
		
		private KeventWatcher ()
		{
		}
		
		// Locked by caller
		public static bool GetInstance (out IFileWatcher watcher)
		{
			if (failed == true) {
				watcher = null;
				return false;
			}

			if (instance != null) {
				watcher = instance;
				return true;
			}

			watches = Hashtable.Synchronized (new Hashtable ());
			conn = kqueue();
			if (conn == -1) {
				failed = true;
				watcher = null;
				return false;
			}

			instance = new KeventWatcher ();
			watcher = instance;
			return true;
		}
		
		public void StartDispatching (FileSystemWatcher fsw)
		{
			KeventData data;

			lock (this) {
				if (thread == null) {
					thread = new Thread (new ThreadStart (Monitor));
					thread.IsBackground = true;
					thread.Start ();
				}

				data = (KeventData) watches [fsw];
			}

			if (data == null) {
				data = new KeventData ();
				data.FSW = fsw;
				data.Path = fsw.FullPath;
				data.FileMask = fsw.MangledFilter;
				data.IncludeSubdirs = fsw.IncludeSubdirectories;

				data.Enabled = true;
				lock (this) {
					Scan (data);
					watches [fsw] = data;
					stop = false;
				}
			}
		}

		bool Add (KeventData data, bool postEvents = false)
		{
			var path = data.Path;

			if (filenamesDict.ContainsKey (path) || fdsDict.ContainsKey (data.fd) ) {
				return false;
			}

			var fd = open (path, 0x8000 /* O_EVTONLY */, 0);

			if (fd != -1) {
				data.fd = fd;
				filenamesDict.Add (path, data);
				fdsDict.Add (fd, data);

				var attrs = File.GetAttributes (data.Path);
				data.IsDirectory = ((attrs & FileAttributes.Directory) == FileAttributes.Directory);

				if (postEvents)
					PostEvent (path, data.FSW, FileAction.Added, path);

				return true;
			} else {
				return false;
			}
		}

		void Remove (int fd)
		{
			if (!fdsDict.ContainsKey (fd))
				return;

			var data = fdsDict [fd];
			fdsDict.Remove (fd);
			filenamesDict.Remove (data.Path);
			removeQueue.Remove (fd);

			close (fd);
		}

		void Remove (string path)
		{
			var data = filenamesDict [path];

			filenamesDict.Remove (path);
			fdsDict.Remove (data.fd);
			close (data.fd);
		}

		bool Scan (KeventData data, bool postEvents = false)
		{
			var path = data.Path;

			Add (data);
			if (!data.IncludeSubdirs) {
				return true;
			}

			if (data.IsDirectory && !Directory.Exists (path))
				return false;

			var attrs = File.GetAttributes (path);
			if ((attrs & FileAttributes.Directory) == FileAttributes.Directory) {
				var dirs_to_process = new List<string> ();
				dirs_to_process.Add (path);

				while (dirs_to_process.Count > 0) {
					var tmp_path = dirs_to_process [0];
					dirs_to_process.RemoveAt (0);
					var dirinfo = new DirectoryInfo (tmp_path);
					foreach (var fsi in dirinfo.GetFileSystemInfos ()) {
						var newdata = new KeventData {
							Path = fsi.FullName,
							FileMask = data.FileMask,
							FSW = data.FSW,
							IncludeSubdirs = data.IncludeSubdirs
						};

						if (!Add (newdata, postEvents))
							continue;

						var childAttrs = File.GetAttributes (fsi.FullName);
						if ((childAttrs & FileAttributes.Directory) == FileAttributes.Directory)
							dirs_to_process.Add (fsi.FullName);
					}
				}
			}

			return true;
		}

		public void StopDispatching (FileSystemWatcher fsw)
		{
			KeventData data;
			lock (this) {
				data = (KeventData) watches [fsw];
				if (data == null)
					return;

				StopMonitoringDirectory (data);
				watches.Remove (fsw);
				if (watches.Count == 0)
					stop = true;

				if (!data.IncludeSubdirs)
					return;

			}
		}

		static void StopMonitoringDirectory (KeventData data)
		{
			close(data.ev.ident);
		}

		void Monitor ()
		{
			bool firstRun = true;

			while (!stop) {
				removeQueue.ForEach (Remove);

				rescanQueue.ForEach (
					fd => {
						var data = fdsDict[fd];
						Scan (data, !firstRun);

						rescanQueue.Remove (fd);
					}
				);

				foreach (KeventData data in watches.Values) {
					Scan (data);
				}

				var changes = new List<kevent> ();
				var outEvents = new List<kevent> ();

				foreach (KeyValuePair<int, KeventData> kv in fdsDict) {
					var change = new kevent {
						ident = kv.Key,
						filter = EventFilter.Vnode,
						flags = EventFlags.Add | EventFlags.Enable | EventFlags.Clear,
						fflags = FilterFlags.VNodeDelete | FilterFlags.VNodeExtend | FilterFlags.VNodeRename | FilterFlags.VNodeAttrib | FilterFlags.VNodeLink | FilterFlags.VNodeRevoke | FilterFlags.VNodeWrite,
						data = IntPtr.Zero,
						udata = IntPtr.Zero
					};

					changes.Add (change);
					outEvents.Add (new kevent());
				}

				if (changes.Count > 0) {
					int numEvents = 0;
					var out_array = outEvents.ToArray ();

					lock (this) {
						kevent[] changes_array = changes.ToArray ();
						numEvents = kevent (conn, changes_array, changes_array.Length, out_array, out_array.Length, IntPtr.Zero);
					}

					for (var i = 0; i < numEvents; i++) {
						var kevt = out_array [i];
						if ((kevt.flags & EventFlags.Error) == EventFlags.Error)
							throw new Exception ("kevent error");

						if ((kevt.fflags & FilterFlags.VNodeDelete) != 0) {
							removeQueue.Add (kevt.ident);
							var data = fdsDict [kevt.ident];
							PostEvent (data.Path, data.FSW, FileAction.Removed, data.Path);
						} else if (((kevt.fflags & FilterFlags.VNodeRename) != 0) || ((kevt.fflags & FilterFlags.VNodeRevoke) != 0) || ((kevt.fflags & FilterFlags.VNodeWrite) != 0)) {
							var data = fdsDict [kevt.ident];
							if (data.IsDirectory && Directory.Exists (data.Path))
								rescanQueue.Add (kevt.ident);

							if ((kevt.fflags & FilterFlags.VNodeRename) != 0) {
								var newFilename = GetFilenameFromFd (data.fd);
								Remove (data.fd);
								PostEvent (data.Path, data.FSW, FileAction.RenamedNewName, data.Path, newFilename);

								var newEvent = new KeventData {
									Path = newFilename,
									FileMask = data.FileMask,
									FSW = data.FSW,
									IncludeSubdirs = data.IncludeSubdirs
								};

								Add (newEvent, false);
							}
						} else if ((kevt.fflags & FilterFlags.VNodeAttrib) != 0) {
							var data = fdsDict[kevt.ident];
							PostEvent (data.Path, data.FSW, FileAction.Modified, data.Path);
						}
					}
				} else {
					Thread.Sleep (500);
				}

				firstRun = false;
			}

			lock (this) {
				thread = null;
				stop = false;
			}
		}

		private void PostEvent (string filename, FileSystemWatcher fsw, FileAction fa, string fullname, string newname = null)
		{
			RenamedEventArgs renamed = null;

			if (fa == 0)
				return;

			if (fa == FileAction.RenamedNewName)
				renamed = new RenamedEventArgs (WatcherChangeTypes.Renamed, "", newname, fullname);

			lock (fsw) {
				fsw.DispatchEvents (fa, filename, ref renamed);
				if (fsw.Waiting) {
					fsw.Waiting = false;
					System.Threading.Monitor.PulseAll (fsw);
				}
			}
		}

		private string GetFilenameFromFd (int fd)
		{
			var sb = new StringBuilder (1024);

			if (fcntl (fd, 50 /* F_GETPATH */, sb) != -1) {
				return sb.ToString ();
			} else {
				return String.Empty;
			}
		}

		[DllImport("libc", EntryPoint="fcntl", CharSet=CharSet.Auto, SetLastError=true)]
		public static extern int fcntl (int file_names_by_descriptor, int cmd, StringBuilder sb);

		[DllImport ("libc")]
		extern static int open(string path, int flags, int mode_t);
		
		[DllImport ("libc")]
		extern static int close(int fd);

		[DllImport ("libc")]
		extern static int kqueue();

		[DllImport ("libc")]
		extern static int kevent(int kq, [In]kevent[] ev, int nchanges, [Out]kevent[] evtlist, int nevents, IntPtr time);
	}
}
