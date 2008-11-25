// ****************************************************************
// This is free software licensed under the NUnit license. You
// may obtain a copy of the license as well as information regarding
// copyright ownership at http://nunit.org/?p=license&r=2.4.
// ****************************************************************

using System;
using System.IO;
using System.Text;
using System.Timers;
using System.Collections;

namespace NUnit.Util
{
	/// <summary>
	/// AssemblyWatcher keeps track of one or more assemblies to 
	/// see if they have changed. It incorporates a delayed notification
	/// and uses a standard event to notify any interested parties
	/// about the change. The path to the assembly is provided as
	/// an argument to the event handler so that one routine can
	/// be used to handle events from multiple watchers.
	/// </summary>
	public class AssemblyWatcher
	{
		FileSystemWatcher[] fileWatcher;
		FileInfo[] fileInfo;

		protected System.Timers.Timer timer;
		protected string changedAssemblyPath; 

		public delegate void AssemblyChangedHandler(String fullPath);
		public event AssemblyChangedHandler AssemblyChangedEvent;

		public AssemblyWatcher( int delay, string assemblyFileName )
			: this( delay, new string[]{ assemblyFileName } ) { }

		public AssemblyWatcher( int delay, IList assemblies )
		{
			fileInfo = new FileInfo[assemblies.Count];
			fileWatcher = new FileSystemWatcher[assemblies.Count];

			for( int i = 0; i < assemblies.Count; i++ )
			{
				fileInfo[i] = new FileInfo( (string)assemblies[i] );

				fileWatcher[i] = new FileSystemWatcher();
				fileWatcher[i].Path = fileInfo[i].DirectoryName;
				fileWatcher[i].Filter = fileInfo[i].Name;
				fileWatcher[i].NotifyFilter = NotifyFilters.Size | NotifyFilters.LastWrite;
				fileWatcher[i].Changed+=new FileSystemEventHandler(OnChanged);
				fileWatcher[i].EnableRaisingEvents = false;
			}

			timer = new System.Timers.Timer( delay );
			timer.AutoReset=false;
			timer.Enabled=false;
			timer.Elapsed+=new ElapsedEventHandler(OnTimer);
		}

		public FileInfo GetFileInfo( int index )
		{
			return fileInfo[index];
		}

		public void Start()
		{
			EnableWatchers( true );
		}

		public void Stop()
		{
			EnableWatchers( false );
		}

		private void EnableWatchers( bool enable )
		{
			foreach( FileSystemWatcher watcher in fileWatcher )
				watcher.EnableRaisingEvents = enable;
		}

		protected void OnTimer(Object source, ElapsedEventArgs e)
		{
			lock(this)
			{
				PublishEvent();
				timer.Enabled=false;
			}
		}
		
		protected void OnChanged(object source, FileSystemEventArgs e)
		{
			changedAssemblyPath = e.FullPath;
			if ( timer != null )
			{
				lock(this)
				{
					if(!timer.Enabled)
						timer.Enabled=true;
					timer.Start();
				}
			}
			else
			{
				PublishEvent();
			}
		}
	
		protected void PublishEvent()
		{
			if ( AssemblyChangedEvent != null )
				AssemblyChangedEvent( changedAssemblyPath );
		}
	}
}