// 
// System.IO.FileSystemWatcher.cs
//
// Authors:
// 	Tim Coleman (tim@timcoleman.com)
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// Copyright (C) Tim Coleman, 2002 
// (c) 2003 Ximian, Inc. (http://www.ximian.com)
//

using System;
using System.ComponentModel;
using System.Threading;

namespace System.IO {
	[DefaultEvent("Changed")]
	public class FileSystemWatcher : Component, ISupportInitialize {

		#region Fields

		bool enableRaisingEvents;
		string filter;
		bool includeSubdirectories;
		int internalBufferSize;
		NotifyFilters notifyFilter;
		string path;
		ISite site;
		ISynchronizeInvoke synchronizingObject;

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
		}

		public FileSystemWatcher (string path)
			: this (path, String.Empty)
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
		}

		#endregion // Constructors

		#region Properties

		[DefaultValue(false)]
		[IODescription("Flag to indicate if this instance is active")]
		public bool EnableRaisingEvents {
			get { return enableRaisingEvents; }
			set { enableRaisingEvents = value; }
		}

		[DefaultValue("*.*")]
		[IODescription("File name filter pattern")]
		[RecommendedAsConfigurable(true)]
		[TypeConverter ("System.Diagnostics.Design.StringValueConverter, " + Consts.AssemblySystem_Design)]
		public string Filter {
			get { return filter; }
			set {
				filter = value;
				if (filter == null || filter == "")
					filter = "*.*";
			}
		}

		[DefaultValue(false)]
		[IODescription("Flag to indicate we want to watch subdirectories")]
		public bool IncludeSubdirectories {
			get { return includeSubdirectories; }
			set { includeSubdirectories = value; }
		}

		[Browsable(false)]
		[DefaultValue(8192)]
		public int InternalBufferSize {
			get { return internalBufferSize; }
			set { internalBufferSize = value; }
		}

		[DefaultValue(NotifyFilters.FileName | NotifyFilters.DirectoryName | NotifyFilters.LastWrite)]
		[IODescription("Flag to indicate which change event we want to monitor")]
		public NotifyFilters NotifyFilter {
			get { return notifyFilter; }
			[MonoTODO ("Perform validation.")]
			set { notifyFilter = value; }
		}

		[DefaultValue("")]
		[IODescription("The directory to monitor")]
		[RecommendedAsConfigurable(true)]
		[TypeConverter ("System.Diagnostics.Design.StringValueConverter, " + Consts.AssemblySystem_Design)]
		[Editor ("System.Diagnostics.Design.FSWPathEditor, " + Consts.AssemblySystem_Design, "System.Drawing.Design.UITypeEditor, " + Consts.AssemblySystem_Drawing)]
		public string Path {
			get { return path; }
			set {
				bool exists = false;
				Exception exc = null;

				try {
					exists = Directory.Exists (value);
				} catch (Exception e) {
					exists = false;
					exc = e;
				}

				if (exc != null)
					throw new ArgumentException ("Invalid directory name", "value", exc);

				if (!exists)
					throw new ArgumentException ("Directory does not exists", "value");

				path = value;
			}
		}

		[Browsable(false)]
		public override ISite Site {
			get { return site; }
			set { site = value; }
		}

		[DefaultValue(null)]
		[IODescription("The object used to marshal the event handler calls resulting from a directory change")]
		public ISynchronizeInvoke SynchronizingObject {
			get { return synchronizingObject; }
			set { synchronizingObject = value; }
		}

		#endregion // Properties

		#region Methods
	
		[MonoTODO]
		public void BeginInit ()
		{
			throw new NotImplementedException (); 
		}

		[MonoTODO]
		protected override void Dispose (bool disposing)
		{
			if (disposing) {
				// 
			}
			base.Dispose (disposing);
		}

		[MonoTODO]
		public void EndInit ()
		{
			throw new NotImplementedException (); 
		}

		private void RaiseEvent (Delegate ev, EventArgs arg)
		{
			if (ev == null)
				return;

			object [] args = new object [] {this, arg};

			if (synchronizingObject == null) {
				ev.DynamicInvoke (args);
				return;
			}
			
			synchronizingObject.BeginInvoke (ev, args);
		}

		protected void OnChanged (FileSystemEventArgs e)
		{
			RaiseEvent (Changed, e);
		}

		protected void OnCreated (FileSystemEventArgs e)
		{
			RaiseEvent (Created, e);
		}

		protected void OnDeleted (FileSystemEventArgs e)
		{
			RaiseEvent (Deleted, e);
		}

		protected void OnError (ErrorEventArgs e)
		{
			RaiseEvent (Error, e);
		}

		protected void OnRenamed (RenamedEventArgs e)
		{
			RaiseEvent (Renamed, e);
		}

		public WaitForChangedResult WaitForChanged (WatcherChangeTypes changeType)
		{
			return WaitForChanged (changeType, Timeout.Infinite);
		}

		[MonoTODO]
		public WaitForChangedResult WaitForChanged (WatcherChangeTypes changeType, int timeout)
		{
			throw new NotImplementedException (); 
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
	}
}
