// 
// System.IO.FAM.cs: interface with libfam
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (c) 2004 Novell, Inc. (http://www.novell.com)
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
		IntPtr opaque;
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
		
		private FAMWatcher ()
		{
		}
		
		public static bool GetInstance (out IFileWatcher watcher)
		{
			lock (typeof (FAMWatcher)) {
				if (failed == true) {
					watcher = null;
					return false;
				}

				if (instance != null) {
					watcher = instance;
					return true;
				}

				watches = new Hashtable ();
				requests = new Hashtable ();
				if (FAMOpen (out conn) == -1) {
					failed = true;
					watcher = null;
					return false;
				}

				instance = new FAMWatcher ();
				watcher = instance;
				return true;
			}
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
				data.FileMask = fsw.Filter;
				data.IncludeSubdirs = fsw.IncludeSubdirectories;
				data.Enabled = true;
				lock (this) {
					StartMonitoringDirectory (data);
					watches [fsw] = data;
					requests [data.Request.ReqNum] = data;
				}
			}
		}

		static void StartMonitoringDirectory (FAMData data)
		{
			FAMRequest fr;
			if (FAMMonitorDirectory (ref conn, data.Directory, out fr, IntPtr.Zero) == -1)
				throw new Win32Exception ();

			data.Request = fr;
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
			}
		}

		static void StopMonitoringDirectory (FAMData data)
		{
			FAMRequest fr;
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
					
					if (fa != 0) {
						if (filename != data.Directory && !fsw.Pattern.IsMatch (filename))
							continue;

						lock (fsw) {
							fsw.DispatchEvents (fa, filename, ref renamed);
							if (fsw.Waiting) {
								fsw.Waiting = false;
								System.Threading.Monitor.PulseAll (fsw);
							}
						}
					}

				} while (FAMPending (ref conn) > 0);
			}
		}
		
		[DllImport ("fam")]
		extern static int FAMOpen (out FAMConnection fc);

		[DllImport ("fam")]
		extern static int FAMClose (ref FAMConnection fc);

		[DllImport ("fam")]
		extern static int FAMMonitorDirectory (ref FAMConnection fc, string filename,
							out FAMRequest fr, IntPtr user_data);

		[DllImport ("fam")]
		extern static int FAMCancelMonitor (ref FAMConnection fc, ref FAMRequest fr);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		extern static int InternalFAMNextEvent (ref FAMConnection fc, out string filename,
							out int code, out int reqnum);

		[DllImport ("fam")]
		extern static int FAMPending (ref FAMConnection fc);
	}
}

