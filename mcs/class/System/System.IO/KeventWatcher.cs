// 
// System.IO.KeventWatcher.cs: interface with osx kevent
//
// Authors:
//	Geoff Norton (gnorton@customerdna.com)
//
// (c) 2004 Geoff Norton

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
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace System.IO {

	struct kevent : IDisposable {
		public int ident;
		public short filter;
		public ushort flags;
		public uint fflags;
		public int data;
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
                public string Directory;
                public string FileMask;
                public bool IncludeSubdirs;
                public bool Enabled;
		public Hashtable DirEntries;
		public kevent ev;
        }

	class KeventWatcher : IFileWatcher
	{
		static bool failed;
		static KeventWatcher instance;
		static Hashtable watches;
		static Hashtable requests;
		static Thread thread;
		static int conn;
		static bool stop;
		
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
			requests = Hashtable.Synchronized (new Hashtable ());
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
				data.Directory = fsw.FullPath;
				data.FileMask = fsw.MangledFilter;
				data.IncludeSubdirs = fsw.IncludeSubdirectories;

				data.Enabled = true;
				lock (this) {
					StartMonitoringDirectory (data);
					watches [fsw] = data;
					stop = false;
				}
			}
		}

		static void StartMonitoringDirectory (KeventData data)
		{
			DirectoryInfo dir = new DirectoryInfo (data.Directory);
			if(data.DirEntries == null) {
				data.DirEntries = new Hashtable();
				foreach (FileSystemInfo fsi in dir.GetFileSystemInfos() ) 
					data.DirEntries.Add(fsi.FullName, new KeventFileData(fsi, fsi.LastAccessTime, fsi.LastWriteTime));
			}

			int fd = open(data.Directory, 0, 0);
			kevent ev = new kevent();
			ev.udata = IntPtr.Zero;
			timespec nullts = new timespec();
			nullts.tv_sec = 0;
			nullts.tv_usec = 0;
			if (fd > 0) {
				ev.ident = fd;
				ev.filter = -4;
				ev.flags = 1 | 4 | 20;
				ev.fflags = 20 | 2 | 1 | 8;
				ev.data = 0;
				ev.udata = Marshal.StringToHGlobalAuto (data.Directory);
				kevent outev = new kevent();
				outev.udata = IntPtr.Zero;
				kevent (conn, ref ev, 1, ref outev, 0, ref nullts);
				data.ev = ev;
				requests [fd] = data;
			}
			
			if (!data.IncludeSubdirs)
				return;

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
		
			while (!stop) {
				kevent ev = new kevent();
				ev.udata = IntPtr.Zero;
				kevent nullev = new kevent();
				nullev.udata = IntPtr.Zero;
				timespec ts = new timespec();
				ts.tv_sec = 0;
				ts.tv_usec = 0;
				int haveEvents;
				lock (this) {
					haveEvents = kevent (conn, ref nullev, 0, ref ev, 1, ref ts);
				}

				if (haveEvents > 0) {
					// Restart monitoring
					KeventData data = (KeventData) requests [ev.ident];
					StopMonitoringDirectory (data);
					StartMonitoringDirectory (data);
					ProcessEvent (ev);
				} else {
					System.Threading.Thread.Sleep (500);
				}
			}

			lock (this) {
				thread = null;
				stop = false;
			}
		}

		void ProcessEvent (kevent ev)
		{
			lock (this) {
				KeventData data = (KeventData) requests [ev.ident];
				if (!data.Enabled)
					return;

				FileSystemWatcher fsw;
				string filename = "";

				fsw = data.FSW;
				FileAction fa = 0;
				DirectoryInfo dir = new DirectoryInfo (data.Directory);
				FileSystemInfo changedFsi = null;

				try {
					foreach (FileSystemInfo fsi in dir.GetFileSystemInfos() )
						if (data.DirEntries.ContainsKey (fsi.FullName) && (fsi is FileInfo)) {
							KeventFileData entry = (KeventFileData) data.DirEntries [fsi.FullName];
							if (entry.LastWriteTime != fsi.LastWriteTime) {
								filename = fsi.Name;
								fa = FileAction.Modified;
								data.DirEntries [fsi.FullName] = new KeventFileData(fsi, fsi.LastAccessTime, fsi.LastWriteTime);
								if (fsw.IncludeSubdirectories && fsi is DirectoryInfo) {
									data.Directory = filename;
									requests [ev.ident] = data;
									ProcessEvent(ev);
								}
								PostEvent(filename, fsw, fa, changedFsi);
							}
						}
				} catch (Exception) {
					// The file system infos were changed while we processed them
				}
				// Deleted
				try {
					bool deleteMatched = true;
					while(deleteMatched) {
						foreach (KeventFileData entry in data.DirEntries.Values) { 
							if (!File.Exists (entry.fsi.FullName) && !Directory.Exists (entry.fsi.FullName)) {
								filename = entry.fsi.Name;
								fa = FileAction.Removed;
								data.DirEntries.Remove (entry.fsi.FullName);
								PostEvent(filename, fsw, fa, changedFsi);
								break;
							}
						}
						deleteMatched = false;
					}
				} catch (Exception) {
					// The file system infos were changed while we processed them
				}
				// Added
				try {
					foreach (FileSystemInfo fsi in dir.GetFileSystemInfos()) 
						if (!data.DirEntries.ContainsKey (fsi.FullName)) {
							changedFsi = fsi;
							filename = fsi.Name;
							fa = FileAction.Added;
							data.DirEntries [fsi.FullName] = new KeventFileData(fsi, fsi.LastAccessTime, fsi.LastWriteTime);
							PostEvent(filename, fsw, fa, changedFsi);
						}
				} catch (Exception) {
					// The file system infos were changed while we processed them
				}
				

			}
		}

		private void PostEvent (string filename, FileSystemWatcher fsw, FileAction fa, FileSystemInfo changedFsi) {
			RenamedEventArgs renamed = null;
			if (fa == 0)
				return;
			
			if (fsw.IncludeSubdirectories && fa == FileAction.Added) {
				if (changedFsi is DirectoryInfo) {
					KeventData newdirdata = new KeventData ();
					newdirdata.FSW = fsw;
					newdirdata.Directory = changedFsi.FullName;
					newdirdata.FileMask = fsw.MangledFilter;
					newdirdata.IncludeSubdirs = fsw.IncludeSubdirectories;
	
					newdirdata.Enabled = true;
					lock (this) {
						StartMonitoringDirectory (newdirdata);
					}
				}
			}
		
			if (!fsw.Pattern.IsMatch(filename, true))
				return;

			lock (fsw) {
				fsw.DispatchEvents (fa, filename, ref renamed);
				if (fsw.Waiting) {
					fsw.Waiting = false;
					System.Threading.Monitor.PulseAll (fsw);
				}
			}
		}

		[DllImport ("libc")]
		extern static int open(string path, int flags, int mode_t);
		
		[DllImport ("libc")]
		extern static int close(int fd);

		[DllImport ("libc")]
		extern static int kqueue();

		[DllImport ("libc")]
		extern static int kevent(int kqueue, ref kevent ev, int nchanges, ref kevent evtlist,  int nevents, ref timespec ts);
	}
}

