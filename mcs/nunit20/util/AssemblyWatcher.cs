#region Copyright (c) 2002-2003, James W. Newkirk, Michael C. Two, Alexei A. Vorontsov, Charlie Poole, Philip A. Craig
/************************************************************************************
'
' Copyright  2002-2003 James W. Newkirk, Michael C. Two, Alexei A. Vorontsov, Charlie Poole
' Copyright  2000-2002 Philip A. Craig
'
' This software is provided 'as-is', without any express or implied warranty. In no 
' event will the authors be held liable for any damages arising from the use of this 
' software.
' 
' Permission is granted to anyone to use this software for any purpose, including 
' commercial applications, and to alter it and redistribute it freely, subject to the 
' following restrictions:
'
' 1. The origin of this software must not be misrepresented; you must not claim that 
' you wrote the original software. If you use this software in a product, an 
' acknowledgment (see the following) in the product documentation is required.
'
' Portions Copyright  2002-2003 James W. Newkirk, Michael C. Two, Alexei A. Vorontsov, Charlie Poole
' or Copyright  2000-2002 Philip A. Craig
'
' 2. Altered source versions must be plainly marked as such, and must not be 
' misrepresented as being the original software.
'
' 3. This notice may not be removed or altered from any source distribution.
'
'***********************************************************************************/
#endregion

using System;
using System.IO;
using System.Text;
using System.Timers;
using System.Collections;
using NUnit.Core;

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