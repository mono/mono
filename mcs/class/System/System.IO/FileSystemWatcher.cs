// 
// System.IO.FileSystemWatcher.cs
//
// Authors:
// 	Tim Coleman (tim@timcoleman.com)
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// Copyright (C) Tim Coleman, 2002 
// (c) 2003 Ximian, Inc. (http://www.ximian.com)
// Copyright (C) 2004, 2006 Novell, Inc (http://www.novell.com)
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

using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Threading;

namespace System.IO {
	[DefaultEvent("Changed")]
#if NET_2_0
	[IODescription ("")]
#endif
	public class FileSystemWatcher : Component, ISupportInitialize {

		#region Fields

		bool enableRaisingEvents;
		string filter;
		bool includeSubdirectories;
		int internalBufferSize;
		NotifyFilters notifyFilter;
		string path;
		string fullpath;
		ISynchronizeInvoke synchronizingObject;
		WaitForChangedResult lastData;
		bool waiting;
		SearchPattern2 pattern;
		bool disposed;
		string mangledFilter;
		static IFileWatcher watcher;
		static object lockobj = new object ();

		#endregion // Fields

		#region Constructors

		public FileSystemWatcher ()
		{
			this.notifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName;
			this.enableRaisingEvents = false;
			this.filter = "*.*";
			this.includeSubdirectories = false;
			this.internalBufferSize = 8192;
			this.path = "";
			InitWatcher ();
		}

		public FileSystemWatcher (string path)
			: this (path, "*.*")
		{
		}

		public FileSystemWatcher (string path, string filter)
		{
			if (path == null)
				throw new ArgumentNullException ("path");

			if (filter == null)
				throw new ArgumentNullException ("filter");

			if (path == String.Empty)
				throw new ArgumentException ("Empty path", "path");

			if (!Directory.Exists (path))
				throw new ArgumentException ("Directory does not exists", "path");

			this.enableRaisingEvents = false;
			this.filter = filter;
			this.includeSubdirectories = false;
			this.internalBufferSize = 8192;
			this.notifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName;
			this.path = path;
			this.synchronizingObject = null;
			InitWatcher ();
		}

		[EnvironmentPermission (SecurityAction.Assert, Read="MONO_MANAGED_WATCHER")]
		void InitWatcher ()
		{
			lock (lockobj) {
				if (watcher != null)
					return;

				string managed = Environment.GetEnvironmentVariable ("MONO_MANAGED_WATCHER");
				int mode = 0;
				if (managed == null)
					mode = InternalSupportsFSW ();
				
				bool ok = false;
				switch (mode) {
				case 1: // windows
					ok = DefaultWatcher.GetInstance (out watcher);
					//ok = WindowsWatcher.GetInstance (out watcher);
					break;
				case 2: // libfam
					ok = FAMWatcher.GetInstance (out watcher, false);
					break;
				case 3: // kevent
					ok = KeventWatcher.GetInstance (out watcher);
					break;
				case 4: // libgamin
					ok = FAMWatcher.GetInstance (out watcher, true);
					break;
				case 5: // inotify
					ok = InotifyWatcher.GetInstance (out watcher, true);
					break;
				}

				if (mode == 0 || !ok) {
					if (String.Compare (managed, "disabled", true) == 0)
						NullFileWatcher.GetInstance (out watcher);
					else
						DefaultWatcher.GetInstance (out watcher);
				}

				ShowWatcherInfo ();
			}
		}

		[Conditional ("DEBUG"), Conditional ("TRACE")]
		void ShowWatcherInfo ()
		{
			Console.WriteLine ("Watcher implementation: {0}", watcher != null ? watcher.GetType ().ToString () : "<none>");
		}
		
		#endregion // Constructors

		#region Properties

		/* If this is enabled, we Pulse this instance */
		internal bool Waiting {
			get { return waiting; }
			set { waiting = value; }
		}

		internal string MangledFilter {
			get {
				if (filter != "*.*")
					return filter;

				if (mangledFilter != null)
					return mangledFilter;

				string filterLocal = "*.*";
				if (!(watcher.GetType () == typeof (WindowsWatcher)))
					filterLocal = "*";

				return filterLocal;
			}
		}

		internal SearchPattern2 Pattern {
			get {
				if (pattern == null) {
					pattern = new SearchPattern2 (MangledFilter);
				}
				return pattern;
			}
		}

		internal string FullPath {
			get {
				if (fullpath == null) {
					if (path == null || path == "")
						fullpath = Environment.CurrentDirectory;
					else
						fullpath = System.IO.Path.GetFullPath (path);
				}

				return fullpath;
			}
		}

		[DefaultValue(false)]
		[IODescription("Flag to indicate if this instance is active")]
		public bool EnableRaisingEvents {
			get { return enableRaisingEvents; }
			set {
				if (value == enableRaisingEvents)
					return; // Do nothing

				enableRaisingEvents = value;
				if (value) {
					Start ();
				} else {
					Stop ();
				}
			}
		}

		[DefaultValue("*.*")]
		[IODescription("File name filter pattern")]
		[RecommendedAsConfigurable(true)]
		[TypeConverter ("System.Diagnostics.Design.StringValueConverter, " + Consts.AssemblySystem_Design)]
		public string Filter {
			get { return filter; }
			set {
				if (value == null || value == "")
					value = "*.*";

				if (filter != value) {
					filter = value;
					pattern = null;
					mangledFilter = null;
				}
			}
		}

		[DefaultValue(false)]
		[IODescription("Flag to indicate we want to watch subdirectories")]
		public bool IncludeSubdirectories {
			get { return includeSubdirectories; }
			set {
				if (includeSubdirectories == value)
					return;

				includeSubdirectories = value;
				if (value && enableRaisingEvents) {
					Stop ();
					Start ();
				}
			}
		}

		[Browsable(false)]
		[DefaultValue(8192)]
		public int InternalBufferSize {
			get { return internalBufferSize; }
			set {
				if (internalBufferSize == value)
					return;

				if (value < 4196)
					value = 4196;

				internalBufferSize = value;
				if (enableRaisingEvents) {
					Stop ();
					Start ();
				}
			}
		}

		[DefaultValue(NotifyFilters.FileName | NotifyFilters.DirectoryName | NotifyFilters.LastWrite)]
		[IODescription("Flag to indicate which change event we want to monitor")]
		public NotifyFilters NotifyFilter {
			get { return notifyFilter; }
			set {
				if (notifyFilter == value)
					return;
					
				notifyFilter = value;
				if (enableRaisingEvents) {
					Stop ();
					Start ();
				}
			}
		}

		[DefaultValue("")]
		[IODescription("The directory to monitor")]
		[RecommendedAsConfigurable(true)]
		[TypeConverter ("System.Diagnostics.Design.StringValueConverter, " + Consts.AssemblySystem_Design)]
		[Editor ("System.Diagnostics.Design.FSWPathEditor, " + Consts.AssemblySystem_Design, "System.Drawing.Design.UITypeEditor, " + Consts.AssemblySystem_Drawing)]
		public string Path {
			get { return path; }
			set {
				if (path == value)
					return;

				bool exists = false;
				Exception exc = null;

				try {
					exists = Directory.Exists (value);
				} catch (Exception e) {
					exc = e;
				}

				if (exc != null)
					throw new ArgumentException ("Invalid directory name", "value", exc);

				if (!exists)
					throw new ArgumentException ("Directory does not exists", "value");

				path = value;
				fullpath = null;
				if (enableRaisingEvents) {
					Stop ();
					Start ();
				}
			}
		}

		[Browsable(false)]
		public override ISite Site {
			get { return base.Site; }
			set { base.Site = value; }
		}

		[DefaultValue(null)]
		[IODescription("The object used to marshal the event handler calls resulting from a directory change")]
#if NET_2_0
		[Browsable (false)]
#endif
		public ISynchronizeInvoke SynchronizingObject {
			get { return synchronizingObject; }
			set { synchronizingObject = value; }
		}

		#endregion // Properties

		#region Methods
	
		public void BeginInit ()
		{
			// Not necessary in Mono
		}

		protected override void Dispose (bool disposing)
		{
			if (!disposed) {
				disposed = true;
				Stop ();
			}

			base.Dispose (disposing);
		}

		~FileSystemWatcher ()
		{
			disposed = true;
			Stop ();
		}
		
		public void EndInit ()
		{
			// Not necessary in Mono
		}

		enum EventType {
			FileSystemEvent,
			ErrorEvent,
			RenameEvent
		}
		private void RaiseEvent (Delegate ev, EventArgs arg, EventType evtype)
		{
			if (ev == null)
				return;

			if (synchronizingObject == null) {
				switch (evtype) {
				case EventType.RenameEvent:
					((RenamedEventHandler)ev).BeginInvoke (this, (RenamedEventArgs) arg, null, null);
					break;
				case EventType.ErrorEvent:
					((ErrorEventHandler)ev).BeginInvoke (this, (ErrorEventArgs) arg, null, null);
					break;
				case EventType.FileSystemEvent:
					((FileSystemEventHandler)ev).BeginInvoke (this, (FileSystemEventArgs) arg, null, null);
					break;
				}
				return;
			}
			
			synchronizingObject.BeginInvoke (ev, new object [] {this, arg});
		}

		protected void OnChanged (FileSystemEventArgs e)
		{
			RaiseEvent (Changed, e, EventType.FileSystemEvent);
		}

		protected void OnCreated (FileSystemEventArgs e)
		{
			RaiseEvent (Created, e, EventType.FileSystemEvent);
		}

		protected void OnDeleted (FileSystemEventArgs e)
		{
			RaiseEvent (Deleted, e, EventType.FileSystemEvent);
		}

		protected void OnError (ErrorEventArgs e)
		{
			RaiseEvent (Error, e, EventType.ErrorEvent);
		}

		protected void OnRenamed (RenamedEventArgs e)
		{
			RaiseEvent (Renamed, e, EventType.RenameEvent);
		}

		public WaitForChangedResult WaitForChanged (WatcherChangeTypes changeType)
		{
			return WaitForChanged (changeType, Timeout.Infinite);
		}

		public WaitForChangedResult WaitForChanged (WatcherChangeTypes changeType, int timeout)
		{
			WaitForChangedResult result = new WaitForChangedResult ();
			bool prevEnabled = EnableRaisingEvents;
			if (!prevEnabled)
				EnableRaisingEvents = true;

			bool gotData;
			lock (this) {
				waiting = true;
				gotData = Monitor.Wait (this, timeout);
				if (gotData)
					result = this.lastData;
			}

			EnableRaisingEvents = prevEnabled;
			if (!gotData)
				result.TimedOut = true;

			return result;
		}

		internal void DispatchEvents (FileAction act, string filename, ref RenamedEventArgs renamed)
		{
			if (waiting) {
				lastData = new WaitForChangedResult ();
			}

			switch (act) {
			case FileAction.Added:
				lastData.Name = filename;
				lastData.ChangeType = WatcherChangeTypes.Created;
				OnCreated (new FileSystemEventArgs (WatcherChangeTypes.Created, path, filename));
				break;
			case FileAction.Removed:
				lastData.Name = filename;
				lastData.ChangeType = WatcherChangeTypes.Deleted;
				OnDeleted (new FileSystemEventArgs (WatcherChangeTypes.Deleted, path, filename));
				break;
			case FileAction.Modified:
				lastData.Name = filename;
				lastData.ChangeType = WatcherChangeTypes.Changed;
				OnChanged (new FileSystemEventArgs (WatcherChangeTypes.Changed, path, filename));
				break;
			case FileAction.RenamedOldName:
				if (renamed != null) {
					OnRenamed (renamed);
				}
				lastData.OldName = filename;
				lastData.ChangeType = WatcherChangeTypes.Renamed;
				renamed = new RenamedEventArgs (WatcherChangeTypes.Renamed, path, filename, "");
				break;
			case FileAction.RenamedNewName:
				lastData.Name = filename;
				lastData.ChangeType = WatcherChangeTypes.Renamed;
				if (renamed == null) {
					renamed = new RenamedEventArgs (WatcherChangeTypes.Renamed, path, "", filename);
				}
				OnRenamed (renamed);
				renamed = null;
				break;
			default:
				break;
			}
		}

		void Start ()
		{
			watcher.StartDispatching (this);
		}

		void Stop ()
		{
			watcher.StopDispatching (this);
		}
		#endregion // Methods

		#region Events and Delegates

		[IODescription("Occurs when a file/directory change matches the filter")]
		public event FileSystemEventHandler Changed;

		[IODescription("Occurs when a file/directory creation matches the filter")]
		public event FileSystemEventHandler Created;

		[IODescription("Occurs when a file/directory deletion matches the filter")]
		public event FileSystemEventHandler Deleted;

		[Browsable(false)]
		public event ErrorEventHandler Error;

		[IODescription("Occurs when a file/directory rename matches the filter")]
		public event RenamedEventHandler Renamed;

		#endregion // Events and Delegates

		/* 0 -> not supported	*/
		/* 1 -> windows		*/
		/* 2 -> FAM		*/
		/* 3 -> Kevent		*/
		/* 4 -> gamin		*/
		/* 5 -> inotify		*/
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		static extern int InternalSupportsFSW ();
	}
}

