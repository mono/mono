#region Copyright (c) 2002, James W. Newkirk, Michael C. Two, Alexei A. Vorontsov, Philip A. Craig
/************************************************************************************
'
' Copyright © 2002 James W. Newkirk, Michael C. Two, Alexei A. Vorontsov
' Copyright © 2000-2002 Philip A. Craig
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
' Portions Copyright © 2002 James W. Newkirk, Michael C. Two, Alexei A. Vorontsov 
' or Copyright © 2000-2002 Philip A. Craig
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

namespace NUnit.Util
{
	/// <summary>
	/// AssemblyWatcher keeps track of a single assembly to see if
	/// it has changed. It incorporates a delayed notification
	/// and uses a standard event to notify any interested parties
	/// about the change. The path to the assembly is provided as
	/// an argument to the event handler so that one routine can
	/// be used to handle events from multiple watchers.
	/// </summary>
	public class AssemblyWatcher
	{
		FileSystemWatcher fileWatcher;
		protected System.Timers.Timer timer;
		FileInfo fileInfo;

		public delegate void AssemblyChangedHandler(String fullPath);
		public event AssemblyChangedHandler AssemblyChangedEvent;

		public AssemblyWatcher(int delay, FileInfo file)
		{
			fileWatcher = new FileSystemWatcher();
			fileWatcher.Path = file.DirectoryName;
			fileWatcher.Filter = file.Name;
			fileWatcher.NotifyFilter = NotifyFilters.Size | NotifyFilters.LastWrite;
			fileWatcher.Changed+=new FileSystemEventHandler(OnChanged);
			fileWatcher.EnableRaisingEvents = false;

			fileInfo = file;
			
			timer = new System.Timers.Timer( delay );
			timer.AutoReset=false;
			timer.Enabled=false;
			timer.Elapsed+=new ElapsedEventHandler(OnTimer);
		}

		public string Name
		{
			get { return fileInfo.Name; }
		}

		public string DirectoryName
		{
			get { return fileInfo.DirectoryName; }
		}

		public string FullName
		{
			get { return fileInfo.FullName; }
		}


		public void Start()
		{
			fileWatcher.EnableRaisingEvents=true;
		}

		public void Stop()
		{
			fileWatcher.EnableRaisingEvents=false;
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
			if ( timer != null )
			{
				lock(this)
				{
					if(!timer.Enabled)
					{
						timer.Enabled=true;
					}
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
				AssemblyChangedEvent( fileInfo.FullName );
		}
	}
}