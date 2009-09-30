// 
// System.IO.Inotify.cs: interface with inotify
//
// Authors:
//	Gonzalo Paniagua (gonzalo@novell.com)
//	Anders Rune Jensen (anders@iola.dk)
//
// (c) 2006 Novell, Inc. (http://www.novell.com)

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

	[Flags]
	enum InotifyMask : uint {
		Access = 1 << 0,
		Modify = 1 << 1,
		Attrib = 1 << 2,
		CloseWrite = 1 << 3,
		CloseNoWrite = 1 << 4,
		Open = 1 << 5,
		MovedFrom = 1 << 6,
		MovedTo = 1 << 7,
		Create = 1 << 8,
		Delete = 1 << 9,
		DeleteSelf = 1 << 10,
		MoveSelf = 1 << 11,
		BaseEvents = 0x00000fff,
		// Can be sent at any time
		Umount = 0x0002000,
		Overflow = 0x0004000,
		Ignored = 0x0008000,

		// Special flags.
		OnlyDir = 0x01000000,
		DontFollow = 0x02000000,
		AddMask = 0x20000000,
		Directory = 0x40000000,
		OneShot = 0x80000000
	}

	struct InotifyEvent { // Our internal representation for the data returned by the kernel
		public static readonly InotifyEvent Default = new InotifyEvent ();
		public int WatchDescriptor;
		public InotifyMask Mask;
		public string Name;

		public override string ToString ()
		{
			return String.Format ("[Descriptor: {0} Mask: {1} Name: {2}]", WatchDescriptor, Mask, Name);
		}
	}

	class ParentInotifyData
	{
		public bool IncludeSubdirs;
		public bool Enabled;
	        public ArrayList children; // InotifyData
	        public InotifyData data;
	}

	class InotifyData {
		public FileSystemWatcher FSW;
		public string Directory;
		public int Watch;
	}

	class InotifyWatcher : IFileWatcher
	{
		static bool failed;
		static InotifyWatcher instance;
		static Hashtable watches; // FSW to ParentInotifyData
		static Hashtable requests; // FSW to InotifyData
		static IntPtr FD;
		static Thread thread;
		static bool stop;
		
		private InotifyWatcher ()
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

			FD = GetInotifyInstance ();
			if ((long) FD == -1) {
				failed = true;
				watcher = null;
				return false;
			}

			watches = Hashtable.Synchronized (new Hashtable ());
			requests = Hashtable.Synchronized (new Hashtable ());
			instance = new InotifyWatcher ();
			watcher = instance;
			return true;
		}
		
		public void StartDispatching (FileSystemWatcher fsw)
		{
			ParentInotifyData parent;
			lock (this) {
				if ((long) FD == -1)
					FD = GetInotifyInstance ();

				if (thread == null) {
					thread = new Thread (new ThreadStart (Monitor));
					thread.IsBackground = true;
					thread.Start ();
				}

				parent = (ParentInotifyData) watches [fsw];
			}

			if (parent == null) {
				InotifyData data = new InotifyData ();
				data.FSW = fsw;
				data.Directory = fsw.FullPath;

				parent = new ParentInotifyData();
				parent.IncludeSubdirs = fsw.IncludeSubdirectories;
				parent.Enabled = true;
				parent.children = new ArrayList();
				parent.data = data;

				watches [fsw] = parent;

				try {
					StartMonitoringDirectory (data, false);
					lock (this) {
						AppendRequestData (data);
						stop = false;
					}
				} catch {} // ignore the directory if StartMonitoringDirectory fails.
			}
		}
		
		static void AppendRequestData (InotifyData data)
		{
			int wd = data.Watch;

			object obj = requests [wd];
			ArrayList list = null;
			if (obj == null) {
				requests [data.Watch] = data;
			} else if (obj is InotifyData) {
				list = new ArrayList ();
				list.Add (obj);
				list.Add (data);
				requests [data.Watch] = list;
			} else {
				list = (ArrayList) obj;
				list.Add (data);
			}
		}

		static bool RemoveRequestData (InotifyData data)
		{
			int wd = data.Watch;
			object obj = requests [wd];
			if (obj == null)
				return true;

			if (obj is InotifyData) {
				if (obj == data) {
					requests.Remove (wd);
					return true;
				}
				return false;
			}

			ArrayList list = (ArrayList) obj;
			list.Remove (data);
			if (list.Count == 0) {
				requests.Remove (wd);
				return true;
			}
			return false;
		}

		// Attempt to match MS and linux behavior.
		static InotifyMask GetMaskFromFilters (NotifyFilters filters)
		{
			InotifyMask mask = InotifyMask.Create | InotifyMask.Delete | InotifyMask.DeleteSelf | InotifyMask.AddMask;
			if ((filters & NotifyFilters.Attributes) != 0)
				mask |= InotifyMask.Attrib;

			if ((filters & NotifyFilters.Security) != 0)
				mask |= InotifyMask.Attrib;

			if ((filters & NotifyFilters.Size) != 0) {
				mask |= InotifyMask.Attrib;
				mask |= InotifyMask.Modify;
			}

			if ((filters & NotifyFilters.LastAccess) != 0) {
				mask |= InotifyMask.Attrib;
				mask |= InotifyMask.Access;
				mask |= InotifyMask.Modify;
			}

			if ((filters & NotifyFilters.LastWrite) != 0) {
				mask |= InotifyMask.Attrib;
				mask |= InotifyMask.Modify;
			}

			if ((filters & NotifyFilters.FileName) != 0) {
				mask |= InotifyMask.MovedFrom;
				mask |= InotifyMask.MovedTo;
			}

			if ((filters & NotifyFilters.DirectoryName) != 0) {
				mask |= InotifyMask.MovedFrom;
				mask |= InotifyMask.MovedTo;
			}

			return mask;
		}

		static void StartMonitoringDirectory (InotifyData data, bool justcreated)
		{
			InotifyMask mask = GetMaskFromFilters (data.FSW.NotifyFilter);
			int wd = AddDirectoryWatch (FD, data.Directory, mask);
			if (wd == -1) {
				int error = Marshal.GetLastWin32Error ();
				if (error == 4) { // Too many open watches
					string nr_watches = "(unknown)";
					try {
						using (StreamReader reader = new StreamReader ("/proc/sys/fs/inotify/max_user_watches")) {
							nr_watches = reader.ReadLine ();
						}
					} catch {}

					string msg = String.Format ("The per-user inotify watches limit of {0} has been reached. " +
								"If you're experiencing problems with your application, increase that limit " +
								"in /proc/sys/fs/inotify/max_user_watches.", nr_watches);
					
					throw new Win32Exception (error, msg);
				}
				throw new Win32Exception (error);
			}

			FileSystemWatcher fsw = data.FSW;
			data.Watch = wd;

			ParentInotifyData parent = (ParentInotifyData) watches[fsw];

			if (parent.IncludeSubdirs) {
				foreach (string directory in Directory.GetDirectories (data.Directory)) {
					InotifyData fd = new InotifyData ();
					fd.FSW = fsw;
					fd.Directory = directory;

					if (justcreated) {
						lock (fsw) {
							RenamedEventArgs renamed = null;
							if (fsw.Pattern.IsMatch (directory)) {
								fsw.DispatchEvents (FileAction.Added, directory, ref renamed);
								if (fsw.Waiting) {
									fsw.Waiting = false;
									System.Threading.Monitor.PulseAll (fsw);
								}
							}
						}
					}

					try {
						StartMonitoringDirectory (fd, justcreated);
						AppendRequestData (fd);
					        parent.children.Add(fd);
					} catch {} // ignore errors and don't add directory.
				}
			}

			if (justcreated) {
				foreach (string filename in Directory.GetFiles (data.Directory)) {
					lock (fsw) {
						RenamedEventArgs renamed = null;
						if (fsw.Pattern.IsMatch (filename)) {
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
		}

		public void StopDispatching (FileSystemWatcher fsw)
		{
			ParentInotifyData parent;
			lock (this) {
				parent = (ParentInotifyData) watches [fsw];
				if (parent == null)
					return;

				if (RemoveRequestData (parent.data)) {
					StopMonitoringDirectory (parent.data);
				}
				watches.Remove (fsw);
				if (watches.Count == 0) {
					stop = true;
					IntPtr fd = FD;
					FD = (IntPtr) (-1);
					Close (fd);
				}

				if (!parent.IncludeSubdirs)
					return;

				foreach (InotifyData idata in parent.children)
				{
				    if (RemoveRequestData (idata)) {
					StopMonitoringDirectory (idata);
				    }
				}
			}
		}

		static void StopMonitoringDirectory (InotifyData data)
		{
			RemoveWatch (FD, data.Watch);
		}

		void Monitor ()
		{
			byte [] buffer = new byte [4096];
			int nread;
			while (!stop) {
				nread = ReadFromFD (FD, buffer, (IntPtr) buffer.Length);
				if (nread == -1)
					continue;

				lock (this) {
					ProcessEvents (buffer, nread);

				}
			}

			lock (this) {
				thread = null;
				stop = false;
			}
		}
		/*
		struct inotify_event {
			__s32           wd;
			__u32           mask;
			__u32           cookie;
			__u32           len;		// Includes any trailing null in 'name'
			char            name[0];
		};
		*/

		static int ReadEvent (byte [] source, int off, int size, out InotifyEvent evt)
		{
			evt = new InotifyEvent ();
			if (size <= 0 || off > size - 16) {
				return -1;
			}

			int len;
			if (BitConverter.IsLittleEndian) {
				evt.WatchDescriptor = source [off] + (source [off + 1] << 8) +
							(source [off + 2] << 16) + (source [off + 3] << 24);
				evt.Mask = (InotifyMask) (source [off + 4] + (source [off + 5] << 8) +
							(source [off + 6] << 16) + (source [off + 7] << 24));
				// Ignore Cookie -> +4
				len = source [off + 12] + (source [off + 13] << 8) +
					(source [off + 14] << 16) + (source [off + 15] << 24);
			} else {
				evt.WatchDescriptor = source [off + 3] + (source [off + 2] << 8) +
							(source [off + 1] << 16) + (source [off] << 24);
				evt.Mask = (InotifyMask) (source [off + 7] + (source [off + 6] << 8) +
							(source [off + 5] << 16) + (source [off + 4] << 24));
				// Ignore Cookie -> +4
				len = source [off + 15] + (source [off + 14] << 8) +
					(source [off + 13] << 16) + (source [off + 12] << 24);
			}

			if (len > 0) {
				if (off > size - 16 - len)
					return -1;
				string name = Encoding.UTF8.GetString (source, off + 16, len);
				evt.Name = name.Trim ('\0');
			} else {
				evt.Name = null;
			}

			return 16 + len;
		}

		static IEnumerable GetEnumerator (object source)
		{
			if (source == null)
				yield break;

			if (source is InotifyData)
				yield return source;

			if (source is ArrayList) {
				ArrayList list = (ArrayList) source;
				for (int i = 0; i < list.Count; i++)
					yield return list [i];
			}
		}

		/* Interesting events:
			* Modify
			* Attrib
			* MovedFrom
			* MovedTo
			* Create
			* Delete
			* DeleteSelf
		*/
		static InotifyMask Interesting = InotifyMask.Modify | InotifyMask.Attrib | InotifyMask.MovedFrom |
							InotifyMask.MovedTo | InotifyMask.Create | InotifyMask.Delete |
							InotifyMask.DeleteSelf;

		void ProcessEvents (byte [] buffer, int length)
		{
			ArrayList newdirs = null;
			InotifyEvent evt;
			int nread = 0;
			RenamedEventArgs renamed = null;
			while (length > nread) {
				int bytes_read = ReadEvent (buffer, nread, length, out evt);
				if (bytes_read <= 0)
					break;

				nread += bytes_read;

				InotifyMask mask = evt.Mask;
				bool is_directory = (mask & InotifyMask.Directory) != 0;
				mask = (mask & Interesting); // Clear out all the bits that we don't need
				if (mask == 0)
					continue;

				foreach (InotifyData data in GetEnumerator (requests [evt.WatchDescriptor])) {
				        ParentInotifyData parent = (ParentInotifyData) watches[data.FSW];

					if (data == null || parent.Enabled == false)
						continue;

					string directory = data.Directory;
					string filename = evt.Name;
					if (filename == null)
						filename = directory;

					FileSystemWatcher fsw = data.FSW;
					FileAction action = 0;
					if ((mask & (InotifyMask.Modify | InotifyMask.Attrib)) != 0) {
						action = FileAction.Modified;
					} else if ((mask & InotifyMask.Create) != 0) {
						action = FileAction.Added;
					} else if ((mask & InotifyMask.Delete) != 0) {
						action = FileAction.Removed;
					} else if ((mask & InotifyMask.DeleteSelf) != 0) {
						if (data.Watch != parent.data.Watch) {
							// To avoid duplicate events handle DeleteSelf only for the top level directory.
							continue;
						}
						action = FileAction.Removed;
					} else if ((mask & InotifyMask.MoveSelf) != 0) {
						//action = FileAction.Removed;
						continue; // Ignore this one
					} else if ((mask & InotifyMask.MovedFrom) != 0) {
						InotifyEvent to;
						int i = ReadEvent (buffer, nread, length, out to);
						if (i == -1 || (to.Mask & InotifyMask.MovedTo) == 0 || evt.WatchDescriptor != to.WatchDescriptor) {
							action = FileAction.Removed;
						} else {
							nread += i;
							action = FileAction.RenamedNewName;
							renamed = new RenamedEventArgs (WatcherChangeTypes.Renamed, data.Directory, to.Name, evt.Name);
							if (evt.Name != data.Directory && !fsw.Pattern.IsMatch (evt.Name))
								filename = to.Name;
						}
					} else if ((mask & InotifyMask.MovedTo) != 0) {
						action = FileAction.Added;
					}
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

						if (action == FileAction.Added && is_directory) {
							if (newdirs == null)
								newdirs = new ArrayList (2);

							InotifyData fd = new InotifyData ();
							fd.FSW = fsw;
							fd.Directory = datadir;
							newdirs.Add (fd);
						}

						if (action == FileAction.RenamedNewName && is_directory) {
							string renamedOldFullPath = renamed.OldFullPath;
							string renamedFullPath = renamed.FullPath;
							int renamedOldFullPathLength = renamedOldFullPath.Length;
							
							foreach (InotifyData child in parent.children) {
									
								if (child.Directory.StartsWith (renamedOldFullPath
#if NET_2_0
												, StringComparison.Ordinal
#endif
								    )) {
									child.Directory = renamedFullPath +
										child.Directory.Substring (renamedOldFullPathLength);
								}
							}
						}
					}

					if (action == FileAction.Removed && filename == data.Directory) {
						int idx = parent.children.IndexOf (data);
						if (idx != -1) {
							parent.children.RemoveAt (idx);
							if (!fsw.Pattern.IsMatch (Path.GetFileName (filename))) {
								continue;
							}
						}
					}

					if (filename != data.Directory && !fsw.Pattern.IsMatch (Path.GetFileName (filename))) {
						continue;
					}

					lock (fsw) {
						fsw.DispatchEvents (action, filename, ref renamed);
						if (action == FileAction.RenamedNewName)
							renamed = null;
						if (fsw.Waiting) {
							fsw.Waiting = false;
							System.Threading.Monitor.PulseAll (fsw);
						}
					}
				}
			}

			if (newdirs != null) {
			        foreach (InotifyData newdir in newdirs) {
					try {
						StartMonitoringDirectory (newdir, true);
						AppendRequestData (newdir);
					        ((ParentInotifyData) watches[newdir.FSW]).children.Add(newdir);
					} catch {} // ignore the given directory
				}
				newdirs.Clear ();
			}
		}

		static int AddDirectoryWatch (IntPtr fd, string directory, InotifyMask mask)
		{
			mask |= InotifyMask.Directory;
			return AddWatch (fd, directory, mask);
		}

		[DllImport ("libc", EntryPoint="close")]
		internal extern static int Close (IntPtr fd);

		[DllImport ("libc", EntryPoint = "read")]
		extern static int ReadFromFD (IntPtr fd, byte [] buffer, IntPtr length);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		extern static IntPtr GetInotifyInstance ();

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		extern static int AddWatch (IntPtr fd, string name, InotifyMask mask);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		extern static IntPtr RemoveWatch (IntPtr fd, int wd);
	}
}

