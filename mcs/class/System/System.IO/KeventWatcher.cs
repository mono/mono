// 
// System.IO.KeventWatcher.cs: interface with osx kevent
//
// Authors:
//	Geoff Norton (gnorton@customerdna.com)
//	Cody Russell (cody@xamarin.com)
//	Alexis Christoforides (lexas@xamarin.com)
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
using System.Reflection;

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

	[Flags]
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
		public UIntPtr ident;
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

	[StructLayout(LayoutKind.Sequential)]
	struct timespec {
		public IntPtr tv_sec;
		public IntPtr tv_nsec;
	}

	class PathData
	{
		public string Path;
		public bool IsDirectory;
		public int Fd;
	}

	class KqueueMonitor : IDisposable
	{
		static bool initialized;
		
		public int Connection
		{
			get { return conn; }
		}

		public KqueueMonitor (FileSystemWatcher fsw)
		{
			this.fsw = fsw;
			this.conn = -1;
			if (!initialized){
				int t;
				initialized = true;
				var maxenv = Environment.GetEnvironmentVariable ("MONO_DARWIN_WATCHER_MAXFDS");
				if (maxenv != null && Int32.TryParse (maxenv, out t))
					maxFds = t;
			}
		}

		public void Dispose ()
		{
			CleanUp ();
		}

		public void Start ()
		{
			lock (stateLock) {
				if (started)
					return;

				conn = kqueue ();

				if (conn == -1)
					throw new IOException (String.Format (
						"kqueue() error at init, error code = '{0}'", Marshal.GetLastWin32Error ()));
					
				thread = new Thread (() => DoMonitor ());
				thread.IsBackground = true;
				thread.Start ();

				startedEvent.WaitOne ();

				if (exc != null) {
					thread.Join ();
					CleanUp ();
					throw exc;
				}
 
				started = true;
			}
		}

		public void Stop ()
		{
			lock (stateLock) {
				if (!started)
					return;
					
				requestStop = true;

				if (inDispatch)
					return;
				// This will break the wait in Monitor ()
				lock (connLock) {
					if (conn != -1)
						close (conn);
					conn = -1;
				}

				while (!thread.Join (2000))
					thread.Interrupt ();

				requestStop = false;
				started = false;

				if (exc != null)
					throw exc;
			}
		}

		void CleanUp ()
		{
			lock (connLock) {
				if (conn != -1)
					close (conn);
				conn = -1;
			}

			foreach (int fd in fdsDict.Keys)
				close (fd); 

			fdsDict.Clear ();
			pathsDict.Clear ();
		}

		void DoMonitor ()
		{			
			try {
				Setup ();
			} catch (Exception e) {
				exc = e;
			} finally {
				startedEvent.Set ();
			}

			if (exc != null) {
				fsw.DispatchErrorEvents (new ErrorEventArgs (exc));
				return;
			}

			try {
				Monitor ();
			} catch (Exception e) {
				exc = e;
			} finally {
				CleanUp ();
				if (!requestStop) { // failure
					started = false;
					inDispatch = false;
					fsw.EnableRaisingEvents = false;
				}
				if (exc != null)
					fsw.DispatchErrorEvents (new ErrorEventArgs (exc));
				requestStop = false;
			}
		}

		void Setup ()
		{	
			var initialFds = new List<int> ();

			// fsw.FullPath may end in '/', see https://bugzilla.xamarin.com/show_bug.cgi?id=5747
			if (fsw.FullPath != "/" && fsw.FullPath.EndsWith ("/", StringComparison.Ordinal))
				fullPathNoLastSlash = fsw.FullPath.Substring (0, fsw.FullPath.Length - 1);
			else
				fullPathNoLastSlash = fsw.FullPath;
				
			// realpath() returns the *realpath* which can be different than fsw.FullPath because symlinks.
			// If so, introduce a fixup step.
			var sb = new StringBuilder (__DARWIN_MAXPATHLEN);
			if (realpath(fsw.FullPath, sb) == IntPtr.Zero) {
				var errMsg = String.Format ("realpath({0}) failed, error code = '{1}'", fsw.FullPath, Marshal.GetLastWin32Error ());
				throw new IOException (errMsg);
			}
			var resolvedFullPath = sb.ToString();

			if (resolvedFullPath != fullPathNoLastSlash)
				fixupPath = resolvedFullPath;
			else
				fixupPath = null;

			Scan (fullPathNoLastSlash, false, ref initialFds);

			var immediate_timeout = new timespec { tv_sec = (IntPtr)0, tv_nsec = (IntPtr)0 };
			var eventBuffer = new kevent[0]; // we don't want to take any events from the queue at this point
			var changes = CreateChangeList (ref initialFds);

			int numEvents;
			int errno = 0;
			do {
				numEvents = kevent (conn, changes, changes.Length, eventBuffer, eventBuffer.Length, ref immediate_timeout);
				if (numEvents == -1) {
					errno = Marshal.GetLastWin32Error ();
				}
			} while (numEvents == -1 && errno == EINTR);

			if (numEvents == -1) {
				var errMsg = String.Format ("kevent() error at initial event registration, error code = '{0}'", errno);
				throw new IOException (errMsg);
			}
		}

		kevent[] CreateChangeList (ref List<int> FdList)
		{
			if (FdList.Count == 0)
				return emptyEventList;

			var changes = new List<kevent> ();
			foreach (int fd in FdList) {
				var change = new kevent {

					ident = (UIntPtr)fd,
					filter = EventFilter.Vnode,
					flags = EventFlags.Add | EventFlags.Enable | EventFlags.Clear,
					fflags = FilterFlags.VNodeDelete | FilterFlags.VNodeExtend |
						FilterFlags.VNodeRename | FilterFlags.VNodeAttrib |
						FilterFlags.VNodeLink | FilterFlags.VNodeRevoke |
						FilterFlags.VNodeWrite,
					data = IntPtr.Zero,
					udata = IntPtr.Zero
				};

				changes.Add (change);
			}
			FdList.Clear ();

			return changes.ToArray ();
		}

		void Monitor ()
		{
			var eventBuffer = new kevent[32];
			var newFds = new List<int> ();
			List<PathData> removeQueue = new List<PathData> ();
			List<string> rescanQueue = new List<string> ();

			int retries = 0; 

			while (!requestStop) {
				var changes = CreateChangeList (ref newFds);

				// We are calling an icall, so have to marshal manually
				// Marshal in
				int ksize = Marshal.SizeOf<kevent> ();
				var changesNative = Marshal.AllocHGlobal (ksize * changes.Length);
				for (int i = 0; i < changes.Length; ++i)
					Marshal.StructureToPtr (changes [i], changesNative + (i * ksize), false);
				var eventBufferNative = Marshal.AllocHGlobal (ksize * eventBuffer.Length);

				int numEvents = kevent_notimeout (ref conn, changesNative, changes.Length, eventBufferNative, eventBuffer.Length);

				// Marshal out
				Marshal.FreeHGlobal (changesNative);
				for (int i = 0; i < numEvents; ++i)
					eventBuffer [i] = Marshal.PtrToStructure<kevent> (eventBufferNative + (i * ksize));
				Marshal.FreeHGlobal (eventBufferNative);

				if (numEvents == -1) {
					// Stop () signals us to stop by closing the connection
					if (requestStop)
						break;
					int errno = Marshal.GetLastWin32Error ();
					if (errno != EINTR && ++retries == 3)
						throw new IOException (String.Format (
							"persistent kevent() error, error code = '{0}'", errno));

					continue;
				}
				retries = 0;

				for (var i = 0; i < numEvents; i++) {
					var kevt = eventBuffer [i];

					if (!fdsDict.ContainsKey ((int)kevt.ident))
						// The event is for a file that was removed
						continue;

					var pathData = fdsDict [(int)kevt.ident];

					if ((kevt.flags & EventFlags.Error) == EventFlags.Error) {
						var errMsg = String.Format ("kevent() error watching path '{0}', error code = '{1}'", pathData.Path, kevt.data);
						fsw.DispatchErrorEvents (new ErrorEventArgs (new IOException (errMsg)));
						continue;
					}
						
					if ((kevt.fflags & FilterFlags.VNodeDelete) == FilterFlags.VNodeDelete || (kevt.fflags & FilterFlags.VNodeRevoke) == FilterFlags.VNodeRevoke) {
						if (pathData.Path == fullPathNoLastSlash)
							// The root path is deleted; exit silently
							return;
								
						removeQueue.Add (pathData);
						continue;
					}

					if ((kevt.fflags & FilterFlags.VNodeRename) == FilterFlags.VNodeRename) {
							UpdatePath (pathData);
					} 

					if ((kevt.fflags & FilterFlags.VNodeWrite) == FilterFlags.VNodeWrite) {
						if (pathData.IsDirectory) //TODO: Check if dirs trigger Changed events on .NET
							rescanQueue.Add (pathData.Path);
						else
							PostEvent (FileAction.Modified, pathData.Path);
					}
						
					if ((kevt.fflags & FilterFlags.VNodeAttrib) == FilterFlags.VNodeAttrib || (kevt.fflags & FilterFlags.VNodeExtend) == FilterFlags.VNodeExtend)
						PostEvent (FileAction.Modified, pathData.Path);
				}

				removeQueue.ForEach (Remove);
				removeQueue.Clear ();

				rescanQueue.ForEach (path => {
					Scan (path, true, ref newFds);
				});
				rescanQueue.Clear ();
			}
		}

		PathData Add (string path, bool postEvents, ref List<int> fds)
		{
			PathData pathData;
			pathsDict.TryGetValue (path, out pathData);

			if (pathData != null)
				return pathData;

			if (fdsDict.Count >= maxFds)
				throw new IOException ("kqueue() FileSystemWatcher has reached the maximum number of files to watch."); 

			var fd = open (path, O_EVTONLY, 0);

			if (fd == -1) {
				fsw.DispatchErrorEvents (new ErrorEventArgs (new IOException (String.Format (
					"open() error while attempting to process path '{0}', error code = '{1}'", path, Marshal.GetLastWin32Error ()))));
				return null;
			}

			try {
				fds.Add (fd);

				var attrs = File.GetAttributes (path);

				pathData = new PathData {
					Path = path,
					Fd = fd,
					IsDirectory = (attrs & FileAttributes.Directory) == FileAttributes.Directory
				};
				
				pathsDict.Add (path, pathData);
				fdsDict.Add (fd, pathData);

				if (postEvents)
					PostEvent (FileAction.Added, path);

				return pathData;
			} catch (Exception e) {
				close (fd);
				fsw.DispatchErrorEvents (new ErrorEventArgs (e));
				return null;
			}

		}

		void Remove (PathData pathData)
		{
			fdsDict.Remove (pathData.Fd);
			pathsDict.Remove (pathData.Path);
			close (pathData.Fd);
			PostEvent (FileAction.Removed, pathData.Path);
		}

		void RemoveTree (PathData pathData)
		{
			var toRemove = new List<PathData> ();

			toRemove.Add (pathData);

			if (pathData.IsDirectory) {
				var prefix = pathData.Path + Path.DirectorySeparatorChar;
				foreach (var path in pathsDict.Keys)
					if (path.StartsWith (prefix)) {
						toRemove.Add (pathsDict [path]);
					}
			}
			toRemove.ForEach (Remove);
		}

		void UpdatePath (PathData pathData)
		{
			var newRoot = GetFilenameFromFd (pathData.Fd);
			if (!newRoot.StartsWith (fullPathNoLastSlash)) { // moved outside of our watched path (so stop observing it)
				RemoveTree (pathData);
				return;
			}
				
			var toRename = new List<PathData> ();
			var oldRoot = pathData.Path;

			toRename.Add (pathData);
															
			if (pathData.IsDirectory) { // anything under the directory must have their paths updated
				var prefix = oldRoot + Path.DirectorySeparatorChar;
				foreach (var path in pathsDict.Keys)
					if (path.StartsWith (prefix))
						toRename.Add (pathsDict [path]);
			}
		
			foreach (var renaming in toRename) {
				var oldPath = renaming.Path;
				var newPath = newRoot + oldPath.Substring (oldRoot.Length);

				renaming.Path = newPath;
				pathsDict.Remove (oldPath);

				// destination may exist in our records from a Created event, take care of it
				if (pathsDict.ContainsKey (newPath)) {
					var conflict = pathsDict [newPath];
					if (GetFilenameFromFd (renaming.Fd) == GetFilenameFromFd (conflict.Fd))
						Remove (conflict);
					else
						UpdatePath (conflict);
				}
					
				pathsDict.Add (newPath, renaming);
			}
			
			PostEvent (FileAction.RenamedNewName, oldRoot, newRoot);
		}

		void Scan (string path, bool postEvents, ref List<int> fds)
		{
			if (requestStop)
				return;
				
			var pathData = Add (path, postEvents, ref fds);

			if (pathData == null)
				return;
				
			if (!pathData.IsDirectory)
				return;

			var dirsToProcess = new List<string> ();
			dirsToProcess.Add (path);

			while (dirsToProcess.Count > 0) {
				var tmp = dirsToProcess [0];
				dirsToProcess.RemoveAt (0);

				var info = new DirectoryInfo (tmp);
				FileSystemInfo[] fsInfos = null;
				try {
					fsInfos = info.GetFileSystemInfos ();
						
				} catch (IOException) {
					// this can happen if the directory has been deleted already.
					// that's okay, just keep processing the other dirs.
					fsInfos = new FileSystemInfo[0];
				}

				foreach (var fsi in fsInfos) {
					if ((fsi.Attributes & FileAttributes.Directory) == FileAttributes.Directory && !fsw.IncludeSubdirectories)
						continue;

					if ((fsi.Attributes & FileAttributes.Directory) != FileAttributes.Directory && !fsw.Pattern.IsMatch (fsi.FullName))
						continue;

					var currentPathData = Add (fsi.FullName, postEvents, ref fds);

					if (currentPathData != null && currentPathData.IsDirectory)
						dirsToProcess.Add (fsi.FullName);
				}
			}
		}
			
		void PostEvent (FileAction action, string path, string newPath = null)
		{
			RenamedEventArgs renamed = null;

			if (requestStop || action == 0)
				return;

			// e.Name
			string name = (path.Length > fullPathNoLastSlash.Length) ? path.Substring (fullPathNoLastSlash.Length + 1) : String.Empty;

			// only post events that match filter pattern. check both old and new paths for renames
			if (!fsw.Pattern.IsMatch (path) && (newPath == null || !fsw.Pattern.IsMatch (newPath)))
				return;
				
			if (action == FileAction.RenamedNewName) {
				string newName = (newPath.Length > fullPathNoLastSlash.Length) ? newPath.Substring (fullPathNoLastSlash.Length + 1) : String.Empty;
				renamed = new RenamedEventArgs (WatcherChangeTypes.Renamed, fsw.Path, newName, name);
			}
				
			fsw.DispatchEvents (action, name, ref renamed);

			if (fsw.Waiting) {
				lock (fsw) {
					fsw.Waiting = false;
					System.Threading.Monitor.PulseAll (fsw);
				}
			}
		}

		private string GetFilenameFromFd (int fd)
		{
			var sb = new StringBuilder (__DARWIN_MAXPATHLEN);

			if (fcntl (fd, F_GETPATH, sb) != -1) {
				if (fixupPath != null) 
					sb.Replace (fixupPath, fullPathNoLastSlash, 0, fixupPath.Length); // see Setup()

				return sb.ToString ();
			} else {
				fsw.DispatchErrorEvents (new ErrorEventArgs (new IOException (String.Format (
					"fcntl() error while attempting to get path for fd '{0}', error code = '{1}'", fd, Marshal.GetLastWin32Error ()))));
				return String.Empty;
			}
		}

		const int O_EVTONLY = 0x8000;
		const int F_GETPATH = 50;
		const int __DARWIN_MAXPATHLEN = 1024;
		const int EINTR = 4;
		static readonly kevent[] emptyEventList = new System.IO.kevent[0];
		int maxFds = Int32.MaxValue;

		FileSystemWatcher fsw;
		int conn;
		Thread thread;
		volatile bool requestStop = false;
		AutoResetEvent startedEvent = new AutoResetEvent (false);
		bool started = false;
		bool inDispatch = false;
		Exception exc = null;
		object stateLock = new object ();
		object connLock = new object ();

		readonly Dictionary<string, PathData> pathsDict = new Dictionary<string, PathData> ();
		readonly Dictionary<int, PathData> fdsDict = new Dictionary<int, PathData> ();
		string fixupPath = null;
		string fullPathNoLastSlash = null;

		[DllImport ("libc", CharSet=CharSet.Auto, SetLastError=true)]
		static extern int fcntl (int file_names_by_descriptor, int cmd, StringBuilder sb);

		[DllImport ("libc", CharSet=CharSet.Auto, SetLastError=true)]
		static extern IntPtr realpath (string pathname, StringBuilder sb);

		[DllImport ("libc", SetLastError=true)]
		extern static int open (string path, int flags, int mode_t);

		[DllImport ("libc")]
		extern static int close (int fd);

		[DllImport ("libc", SetLastError=true)]
		extern static int kqueue ();

		[DllImport ("libc", SetLastError=true)]
		extern static int kevent (int kq, [In]kevent[] ev, int nchanges, [Out]kevent[] evtlist, int nevents, [In] ref timespec time);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		extern static int kevent_notimeout (ref int kq, IntPtr ev, int nchanges, IntPtr evtlist, int nevents);
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

		public void StartDispatching (object handle)
		{
			var fsw = handle as FileSystemWatcher;
			KqueueMonitor monitor;

			if (watches.ContainsKey (fsw)) {
				monitor = (KqueueMonitor)watches [fsw];
			} else {
				monitor = new KqueueMonitor (fsw);
				watches.Add (fsw, monitor);
			}
				
			monitor.Start ();
		}

		public void StopDispatching (object handle)
		{
			var fsw = handle as FileSystemWatcher;
			KqueueMonitor monitor = (KqueueMonitor)watches [fsw];
			if (monitor == null)
				return;

			monitor.Stop ();
		}

		public void Dispose (object handle)
		{
			// does nothing
		}
			
		[DllImport ("libc")]
		extern static int close (int fd);

		[DllImport ("libc")]
		extern static int kqueue ();
	}
}

