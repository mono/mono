// 
// System.IO.KeventWatcher.cs: interface with osx kevent
//
// Authors:
//	Geoff Norton (gnorton@customerdna.com)
//	Cody Russell (cody@xamarin.com)
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

	class PathData
	{
		public string Path;
		public bool IsDirectory;
	}

	class KqueueMonitor : IDisposable
	{
		public int Connection
		{
			get { return conn; }
		}

		public KqueueMonitor (FileSystemWatcher fsw)
		{
			this.fsw = fsw;
			this.conn = -1;
		}

		public void Dispose ()
		{
			Stop ();
		}

		public void Start ()
		{
			conn = kqueue ();

			if (thread == null) {
				thread = new Thread (new ThreadStart (Monitor));
				thread.IsBackground = true;
				thread.Start ();
			}

			var pathData = Add (fsw.FullPath);

			Scan (pathData);
		}

		public void Stop ()
		{
			stop = true;

			if (thread != null)
				thread.Interrupt ();

			if (conn != -1)
				close (conn);

			conn = -1;
		}

		private PathData FindPath (string path)
		{
			foreach (KeyValuePair<PathData, int> kv in paths) {
				if (kv.Key.Path == path)
					return kv.Key;
			}

			return null;
		}

		private PathData FindPath (int fd)
		{
			foreach (KeyValuePair<PathData, int> kv in paths) {
				if (kv.Value == fd)
					return kv.Key;
			}

			return null;
		}

		private void Monitor ()
		{
			bool firstRun = true;

			while (!stop) {
				removeQueue.ForEach (Remove);

				var changes = new List<kevent> ();
				var outEvents = new List<kevent> ();

				rescanQueue.ForEach (fd => {
					var path = FindPath (fd);
					Scan (path, !firstRun);
				});
				rescanQueue.Clear ();

				foreach (KeyValuePair<PathData, int> kv in paths) {
					var change = new kevent {
						ident  = kv.Value,
						filter = EventFilter.Vnode,
						flags  = EventFlags.Add | EventFlags.Enable | EventFlags.Clear,
						fflags = FilterFlags.VNodeDelete | FilterFlags.VNodeExtend |
						         FilterFlags.VNodeRename | FilterFlags.VNodeAttrib |
						         FilterFlags.VNodeLink | FilterFlags.VNodeRevoke |
						         FilterFlags.VNodeWrite,
						data   = IntPtr.Zero,
						udata  = IntPtr.Zero
					};

					changes.Add (change);
					outEvents.Add (new kevent ());
				}

				if (changes.Count > 0) {
					var outArray = outEvents.ToArray ();
					var changesArray = changes.ToArray ();
					int numEvents = kevent (conn, changesArray, changesArray.Length, outArray, outArray.Length, IntPtr.Zero);

					for (var i = 0; i < numEvents; i++) {
						var kevt = outArray [i];
						var pathData = FindPath (kevt.ident);

						if ((kevt.fflags & FilterFlags.VNodeDelete) != 0) {
							removeQueue.Add (kevt.ident);
							PostEvent (FileAction.Removed, pathData.Path);
						} else if (((kevt.fflags & FilterFlags.VNodeRename) != 0) || ((kevt.fflags & FilterFlags.VNodeRevoke) != 0) || ((kevt.fflags & FilterFlags.VNodeWrite) != 0)) {
							if (pathData.IsDirectory && Directory.Exists (pathData.Path))
								rescanQueue.Add (kevt.ident);

							if ((kevt.fflags & FilterFlags.VNodeRename) != 0) {
								var fd = paths [pathData];
								var newFilename = GetFilenameFromFd (fd);
								var oldFilename = pathData.Path;

								Remove (pathData);
								PostEvent (FileAction.RenamedNewName, oldFilename, newFilename);

								Add (newFilename, false);
							}
						} else if ((kevt.fflags & FilterFlags.VNodeAttrib) != 0) {
							PostEvent (FileAction.Modified, pathData.Path);
						}
					}
				} else {
					Thread.Sleep (500);
				}

				firstRun = false;
			}
		}

		private PathData Add (string path, bool postEvents = false)
		{
			var fd = open (path, O_EVTONLY, 0);

			if (fd == -1)
				return null;

			var attrs = File.GetAttributes (path);
			bool isDir = false;
			if ((attrs & FileAttributes.Directory) == FileAttributes.Directory)
				isDir = true;

			var pathData = new PathData {
				Path = path,
				IsDirectory = isDir
			};

			if (FindPath (path) == null) {
				paths.Add (pathData, fd);

				if (postEvents)
					PostEvent (FileAction.Added, path);
			}

			return pathData;
		}

		private void Remove (int fd)
		{
			var path = FindPath (fd);
			paths.Remove (path);
			removeQueue.Remove (fd);

			close (fd);
		}

		private void Remove (PathData pathData)
		{
			var fd = paths [pathData];
			paths.Remove (pathData);
			removeQueue.Remove (fd);

			close (fd);
		}

		private void Scan (PathData pathData, bool postEvents = false)
		{
			var path = pathData.Path;

			Add (path, postEvents);

			if (!fsw.IncludeSubdirectories)
				return;

			var attrs = File.GetAttributes (path);
			if ((attrs & FileAttributes.Directory) == FileAttributes.Directory) {
				var dirsToProcess = new List<string> ();
				dirsToProcess.Add (path);

				while (dirsToProcess.Count > 0) {
					var tmp = dirsToProcess [0];
					dirsToProcess.RemoveAt (0);

					var info = new DirectoryInfo (tmp);
					foreach (var fsi in info.GetFileSystemInfos ()) {
						if (Add (fsi.FullName, postEvents) == null)
							continue;

						var childAttrs = File.GetAttributes (fsi.FullName);
						if ((childAttrs & FileAttributes.Directory) == FileAttributes.Directory)
							dirsToProcess.Add (fsi.FullName);
					}
				}
			}
		}

		private void PostEvent (FileAction action, string path, string newPath = null)
		{
			RenamedEventArgs renamed = null;

			if (action == 0)
				return;

			if (action == FileAction.RenamedNewName)
				renamed = new RenamedEventArgs (WatcherChangeTypes.Renamed, "", newPath, path);

			lock (fsw) {
				fsw.DispatchEvents (action, path, ref renamed);

				if (fsw.Waiting) {
					fsw.Waiting = false;
					System.Threading.Monitor.PulseAll (fsw);
				}
			}
		}

		private string GetFilenameFromFd (int fd)
		{
			var sb = new StringBuilder (1024);

			if (fcntl (fd, F_GETPATH, sb) != -1)
				return sb.ToString ();
			else
				return String.Empty;
		}

		private const int O_EVTONLY = 0x8000;
		private const int F_GETPATH = 50;
		private FileSystemWatcher fsw;
		private int conn;
		private Thread thread;
		private bool stop;
		private readonly List<int> removeQueue = new List<int> ();
		private readonly List<int> rescanQueue = new List<int> ();
		private readonly Dictionary<PathData, int> paths = new Dictionary<PathData, int> ();

		[DllImport ("libc", EntryPoint="fcntl", CharSet=CharSet.Auto, SetLastError=true)]
		static extern int fcntl (int file_names_by_descriptor, int cmd, StringBuilder sb);

		[DllImport ("libc")]
		extern static int open (string path, int flags, int mode_t);

		[DllImport ("libc")]
		extern static int close (int fd);

		[DllImport ("libc")]
		extern static int kqueue ();

		[DllImport ("libc")]
		extern static int kevent(int kq, [In]kevent[] ev, int nchanges, [Out]kevent[] evtlist, int nevents, IntPtr time);
	}

	class KeventWatcher : IFileWatcher
	{
		static bool failed;
		static KeventWatcher instance;
		static Hashtable watches;  // <FileSystemWatcher, KqueueMonitor>

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
			var conn = kqueue();
			if (conn == -1) {
				failed = true;
				watcher = null;
				return false;
			}
			close (conn);

			instance = new KeventWatcher ();
			watcher = instance;
			return true;
		}

		public void StartDispatching (FileSystemWatcher fsw)
		{
			KqueueMonitor monitor;

			if (watches.ContainsKey (fsw)) {
				monitor = (KqueueMonitor)watches [fsw];
			} else {
				monitor = new KqueueMonitor (fsw);
			}

			watches.Add (fsw, monitor);

			monitor.Start ();
		}

		public void StopDispatching (FileSystemWatcher fsw)
		{
			KqueueMonitor monitor = (KqueueMonitor)watches [fsw];
			if (monitor == null)
				return;

			monitor.Stop ();
		}


		[DllImport ("libc")]
		extern static int close (int fd);

		[DllImport ("libc")]
		extern static int kqueue ();
	}
}

