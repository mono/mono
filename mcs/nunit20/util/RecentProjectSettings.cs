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

namespace NUnit.Util
{
	/// <summary>
	/// Holds list of most recent projects
	/// 
	/// NOTE: An earlier version had separate settings for
	/// RecentProjects and RecentAssemblies. Currently we
	/// currently only have RecentProjects, displayed in
	/// the user interface as "Recent Files" and containing
	/// all types of projects and assemblies that have
	/// been opened. We retained the separation into two
	/// classes in case we should need another recent list
	/// at some time in the future. The UI component
	/// RecentFilesMenuHandler can deal with any class
	/// derived from RecentFileSettings.
	/// </summary>
	public class RecentProjectSettings : RecentFileSettings
	{
		private static readonly string NAME = "Recent-Projects";
		
		public RecentProjectSettings( ) : base ( NAME ) { }

		public RecentProjectSettings( SettingsStorage storage ) 
			: base( NAME, storage ) { }

		public RecentProjectSettings( SettingsGroup parent ) 
			: base( NAME, parent ) { }
	}
}
