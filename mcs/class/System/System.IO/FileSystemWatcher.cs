// 
// System.IO.FileSystemWatcher.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System;
using System.ComponentModel;

namespace System.IO {
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
			: this (String.Empty, String.Empty)
		{
		}

		public FileSystemWatcher (string path)
			: this (path, String.Empty)
		{
		}

		[MonoTODO]
		public FileSystemWatcher (string path, string filter)
		{
			if (path == null)
				throw new ArgumentNullException ();
			if (filter == null)
				throw new ArgumentNullException ();
			if (path == String.Empty)
				throw new ArgumentException ();

			// if the path does not exist throw an ArgumentException

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

		public bool EnableRaisingEvents {
			get { return enableRaisingEvents; }
			set { enableRaisingEvents = value; }
		}

		public string Filter {
			get { return filter; }
			set { filter = value; }
		}

		public bool IncludeSubdirectories {
			get { return includeSubdirectories; }
			set { includeSubdirectories = value; }
		}

		public int InternalBufferSize {
			get { return internalBufferSize; }
			set { internalBufferSize = value; }
		}

		public NotifyFilters NotifyFilter {
			get { return notifyFilter; }
			[MonoTODO ("Perform validation.")]
			set { notifyFilter = value; }
		}

		public string Path {
			get { return path; }
			[MonoTODO ("Perform validation.")]
			set { path = value; }
		}

		public override ISite Site {
			get { return site; }
			set { site = value; }
		}

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

		[MonoTODO]
		protected void OnChanged (FileSystemEventArgs e)
		{
			throw new NotImplementedException (); 
		}

		[MonoTODO]
		protected void OnCreated (FileSystemEventArgs e)
		{
			throw new NotImplementedException (); 
		}

		[MonoTODO]
		protected void OnDeleted (FileSystemEventArgs e)
		{
			throw new NotImplementedException (); 
		}

		[MonoTODO]
		protected void OnError (ErrorEventArgs e)
		{
			throw new NotImplementedException (); 
		}

		[MonoTODO]
		protected void OnRenamed (RenamedEventArgs e)
		{
			throw new NotImplementedException (); 
		}

		[MonoTODO]
		public WaitForChangedResult WaitForChanged (WatcherChangeTypes changeType)
		{
			throw new NotImplementedException (); 
		}

		[MonoTODO]
		public WaitForChangedResult WaitForChanged (WatcherChangeTypes changeType, int timeout)
		{
			throw new NotImplementedException (); 
		}


		#endregion // Methods

		#region Events and Delegates

		public event FileSystemEventHandler Changed;
		public event FileSystemEventHandler Created;
		public event FileSystemEventHandler Deleted;
		public event ErrorEventHandler Error;
		public event RenamedEventHandler Renamed;

		#endregion // Events and Delegates
	}
}
