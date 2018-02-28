// 
// System.IO.FAM.cs: interface with libfam
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
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace System.IO {
	struct FAMConnection {
		public int FD;
		public IntPtr opaque;
	}

	struct FAMRequest {
		public int ReqNum;
	}

	enum FAMCodes {
		Changed = 1,
		Deleted = 2,
		StartExecuting = 3, 
		StopExecuting = 4,
		Created = 5,
		Moved = 6, 
		Acknowledge = 7,
		Exists = 8,
		EndExist = 9
	};

	class FAMData {
		public FileSystemWatcher FSW;
		public string Directory;
		public string FileMask;
		public bool IncludeSubdirs;
		public bool Enabled;
		public FAMRequest Request;
		public Hashtable SubDirs;
	}

	class FAMWatcher : IFileWatcher
	{
		static bool failed;
		static FAMWatcher instance;
		static Hashtable watches;
		static Hashtable requests;
		static FAMConnection conn;
		static Thread thread;
		static bool stop;
		static bool use_gamin;
		
		private FAMWatcher ()
		{
		}
		
		// Locked by caller
		public static bool GetInstance (out IFileWatcher watcher, bool gamin)
		{
			if (failed == true) {
				watcher = null;
				return false;
			}

			if (instance != null) {
				watcher = instance;
				return true;
			}

			use_gamin = gamin;
			watches = Hashtable.Synchronized (new Hashtable ());
			requests = Hashtable.Synchronized (new Hashtable ());
			if (FAMOpen (out conn) == -1) {
				failed = true;
				watcher = null;
				return false;
			}

			instance = new FAMWatcher ();
			watcher = instance;
			return true;
		}
		
		public void StartDispatching (FileSystemWatcher fsw)
		{
			FAMData data;
			lock (this) {
				if (thread == null) {
					thread = new Thread (new ThreadStart (Monitor));
					thread.IsBackground = true;
					thread.Start ();
				}

				data = (FAMData) watches [fsw];
			}

			if (data == null) {
				data = new FAMData ();
				data.FSW = fsw;
				data.Directory = fsw.FullPath;
				data.FileMask = fsw.MangledFilter;
				data.IncludeSubdirs = fsw.IncludeSubdirectories;
				if (data.IncludeSubdirs)
					data.SubDirs = new Hashtable ();

				data.Enabled = true;
				StartMonitoringDirectory (data, false);
				lock (this) {
					watches [fsw] = data;
					requests [data.Request.ReqNum] = data;
					stop = false;
				}
			}
		}

		static void StartMonitoringDirectory (FAMData data, bool justcreated)
		{
			FAMRequest fr;
			FileSystemWatcher fsw;
			if (FAMMonitorDirectory (ref conn, data.Directory, out fr, IntPtr.Zero) == -1)
				throw new Win32Exception ();

			fsw = data.FSW;
			data.Request = fr;

			if (data.IncludeSubdirs) {
				foreach (string directory in Directory.GetDirectories (data.Directory)) {
					FAMData fd = new FAMData ();
					fd.FSW = data.FSW;
					fd.Directory = directory;
					fd.FileMask = data.FSW.MangledFilter;
					fd.IncludeSubdirs = true;
					fd.SubDirs = new Hashtable ();
					fd.Enabled = true;

					if (justcreated) {
						lock (fsw) {
							RenamedEventArgs renamed = null;

							fsw.DispatchEvents (FileAction.Added, directory, ref renamed);

							if (fsw.Waiting) {
								fsw.Waiting = false;
								System.Threading.Monitor.PulseAll (fsw);
							}
						}
					}

					StartMonitoringDirectory (fd, justcreated);
					data.SubDirs [directory] = fd;
					requests [fd.Request.ReqNum] = fd;
				}
			}

			if (justcreated) {
				foreach (string filename in Directory.GetFiles(data.Directory)) {
					lock (fsw) {
						RenamedEventArgs renamed = null;

						fsw.DispatchEvents (FileAction.Added, filename, ref renamed);
						/* If a file has been created, then it has been written to */
						fsw.DispatchEvents (FileAction.Modified, filename, ref renamed);

						if (fsw.Waiting) {
							fsw.Waiting = false;
							System.Threading.Monitor.PulseAll(fsw);
						}
					}
				}
			}
		}

		public void StopDispatching (FileSystemWatcher fsw)
		{
			FAMData data;
			lock (this) {
				data = (FAMData) watches [fsw];
				if (data == null)
					return;

				StopMonitoringDirectory (data);
				watches.Remove (fsw);
				requests.Remove (data.Request.ReqNum);
				if (watches.Count == 0)
					stop = true;

				if (!data.IncludeSubdirs)
					return;

				foreach (FAMData fd in data.SubDirs.Values) {
					StopMonitoringDirectory (fd);
					requests.Remove (fd.Request.ReqNum);
				}
			}
		}

		static void StopMonitoringDirectory (FAMData data)
		{
			if (FAMCancelMonitor (ref conn, ref data.Request) == -1)
				throw new Win32Exception ();
		}

		void Monitor ()
		{
			while (!stop) {
				int haveEvents;
				lock (this) {
					haveEvents = FAMPending (ref conn);
				}

				if (haveEvents > 0) {
					ProcessEvents ();
				} else {
					Thread.Sleep (500);
				}
			}

			lock (this) {
				thread = null;
				stop = false;
			}
		}

		const NotifyFilters changed = 	NotifyFilters.Attributes |
						NotifyFilters.LastAccess |
						NotifyFilters.Size	|
						NotifyFilters.LastWrite;

		void ProcessEvents ()
		{
			ArrayList newdirs = null;
			lock (this) {
				do {
					int code;
					string filename;
					int requestNumber;
					FileSystemWatcher fsw;

					if (InternalFAMNextEvent (ref conn, out filename,
								  out code, out requestNumber) != 1)
						return;

					bool found = false;
					switch ((FAMCodes) code) {
					case FAMCodes.Changed:
					case FAMCodes.Deleted:
					case FAMCodes.Created:
						found = requests.ContainsKey (requestNumber);
						break;
					case FAMCodes.Moved:
					case FAMCodes.StartExecuting:
					case FAMCodes.StopExecuting:
					case FAMCodes.Acknowledge:
					case FAMCodes.Exists:
					case FAMCodes.EndExist:
					default:
						found = false;
						break;
					}

					if (!found)
						continue;
					
					FAMData data = (FAMData) requests [requestNumber];
					if (!data.Enabled)
						continue;

					fsw = data.FSW;
					NotifyFilters flt = fsw.NotifyFilter;
					RenamedEventArgs renamed = null;
					FileAction fa = 0;
					if (code == (int) FAMCodes.Changed && (flt & changed) != 0)
						fa = FileAction.Modified;
					else if (code == (int) FAMCodes.Deleted)
						fa = FileAction.Removed;
					else if (code == (int) FAMCodes.Created)
						fa = FileAction.Added;

					if (fa == 0)
						continue;

					if (fsw.IncludeSubdirectories) {
						string full = fsw.FullPath;
						string datadir = data.Directory;
						if (datadir != full) {
							int len = full.Length;
							int slash = 1;
							if (len > 1 && full [len - 1] == Path.DirectorySeparatorChar)
								slash = 0;
							string reldir = datadir.Substring (full.Length + slash);
							datadir = Path.Combine (datadir, filename);
							filename = Path.Combine (reldir, filename);
						} else {
							datadir = Path.Combine (full, filename);
						}

						if (fa == FileAction.Added && Directory.Exists (datadir)) {
							if (newdirs == null)
								newdirs = new ArrayList (4);

							FAMData fd = new FAMData ();
							fd.FSW = fsw;
							fd.Directory = datadir;
							fd.FileMask = fsw.MangledFilter;
							fd.IncludeSubdirs = true;
							fd.SubDirs = new Hashtable ();
							fd.Enabled = true;
							newdirs.Add (fd);
							newdirs.Add (data);
						}
					}

					if (filename != data.Directory && !fsw.Pattern.IsMatch (filename))
						continue;

					lock (fsw) {
						fsw.DispatchEvents (fa, filename, ref renamed);
						if (fsw.Waiting) {
							fsw.Waiting = false;
							System.Threading.Monitor.PulseAll (fsw);
						}
					}
				} while (FAMPending (ref conn) > 0);
			}


			if (newdirs != null) {
				int count = newdirs.Count;
				for (int n = 0; n < count; n += 2) {
					FAMData newdir = (FAMData) newdirs [n];
					FAMData parent = (FAMData) newdirs [n + 1];
					StartMonitoringDirectory (newdir, true);
					requests [newdir.Request.ReqNum] = newdir;
					lock (parent) {
						parent.SubDirs [newdir.Directory] = newdir;
					}
				}
				newdirs.Clear ();
			}
		}

		~FAMWatcher ()
		{
			FAMClose (ref conn);
		}

		static int FAMOpen (out FAMConnection fc)
		{
			if (use_gamin)
				return gamin_Open (out fc);
			return fam_Open (out fc);
		}

		static int FAMClose (ref FAMConnection fc)
		{
			if (use_gamin)
				return gamin_Close (ref fc);
			return fam_Close (ref fc);
		}

		static int FAMMonitorDirectory (ref FAMConnection fc, string filename, out FAMRequest fr, IntPtr user_data)
		{
			if (use_gamin)
				return gamin_MonitorDirectory (ref fc, filename, out fr, user_data);
			return fam_MonitorDirectory (ref fc, filename, out fr, user_data);
		}

		static int FAMCancelMonitor (ref FAMConnection fc, ref FAMRequest fr)
		{
			if (use_gamin)
				return gamin_CancelMonitor (ref fc, ref fr);
			return fam_CancelMonitor (ref fc, ref fr);
		}

		static int FAMPending (ref FAMConnection fc)
		{
			if (use_gamin)
				return gamin_Pending (ref fc);
			return fam_Pending (ref fc);
		}


		
		[DllImport ("libfam.so.0", EntryPoint="FAMOpen")]
		extern static int fam_Open (out FAMConnection fc);

		[DllImport ("libfam.so.0", EntryPoint="FAMClose")]
		extern static int fam_Close (ref FAMConnection fc);

		[DllImport ("libfam.so.0", EntryPoint="FAMMonitorDirectory")]
		extern static int fam_MonitorDirectory (ref FAMConnection fc, string filename,
							out FAMRequest fr, IntPtr user_data);

		[DllImport ("libfam.so.0", EntryPoint="FAMCancelMonitor")]
		extern static int fam_CancelMonitor (ref FAMConnection fc, ref FAMRequest fr);

		[DllImport ("libfam.so.0", EntryPoint="FAMPending")]
		extern static int fam_Pending (ref FAMConnection fc);

		[DllImport ("libgamin-1.so.0", EntryPoint="FAMOpen")]
		extern static int gamin_Open (out FAMConnection fc);

		[DllImport ("libgamin-1.so.0", EntryPoint="FAMClose")]
		extern static int gamin_Close (ref FAMConnection fc);

		[DllImport ("libgamin-1.so.0", EntryPoint="FAMMonitorDirectory")]
		extern static int gamin_MonitorDirectory (ref FAMConnection fc, string filename,
							out FAMRequest fr, IntPtr user_data);

		[DllImport ("libgamin-1.so.0", EntryPoint="FAMCancelMonitor")]
		extern static int gamin_CancelMonitor (ref FAMConnection fc, ref FAMRequest fr);

		[DllImport ("libgamin-1.so.0", EntryPoint="FAMPending")]
		extern static int gamin_Pending (ref FAMConnection fc);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		extern static int InternalFAMNextEvent (ref FAMConnection fc, out string filename,
							out int code, out int reqnum);

	}
}

