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
	/// Summary description for OptionSettings.
	/// </summary>
	public class OptionSettings : SettingsGroup
	{
		private static readonly string NAME = "Options";

		public OptionSettings( ) : base( NAME, UserSettings.GetStorageImpl( NAME ) ) { }

		public OptionSettings( SettingsStorage storage ) : base( NAME, storage ) { }

		public OptionSettings( SettingsGroup parent ) : base( NAME, parent ) { }

		public bool LoadLastProject
		{
			get { return LoadIntSetting( "LoadLastProject", 1 ) != 0; }
			set { SaveIntSetting( "LoadLastProject", value ? 1 : 0 ); }
		}

		public int InitialTreeDisplay
		{
			get { return LoadIntSetting( "InitialTreeDisplay", 0 ); }
			set { SaveIntSetting( "InitialTreeDisplay", value ); }
		}

		public bool ReloadOnRun
		{
			get { return LoadIntSetting( "ReloadOnRun", 1 ) != 0; }
			set { SaveIntSetting( "ReloadOnRun", value ? 1 : 0 ); }
		}

		public bool ShowCheckBoxes
		{
			get { return LoadIntSetting( "ShowCheckBoxes", 0 ) != 0; }
			set { SaveIntSetting( "ShowCheckBoxes", value ? 1 : 0 ); }
		}

		public bool ReloadOnChange
		{
			get
			{
				if ( Environment.OSVersion.Platform != System.PlatformID.Win32NT )
					return false;

				return LoadIntSetting( "ReloadOnChange", 1 ) != 0; 
			}

			set 
			{
				if ( Environment.OSVersion.Platform != System.PlatformID.Win32NT )
					return;

				SaveIntSetting( "ReloadOnChange", value ? 1 : 0 ); 
			}
		}

		public bool ClearResults
		{
			get { return LoadIntSetting( "ClearResults", 1 ) != 0; }
			set { SaveIntSetting( "ClearResults", value ? 1 : 0 ); }
		}

		public bool TestLabels
		{
			get { return LoadIntSetting( "TestLabels", 0 ) != 0; }
			set { SaveIntSetting( "TestLabels", value ? 1 : 0 ); }
		}

		public bool VisualStudioSupport
		{
			get { return LoadIntSetting( "VisualStudioSupport", 0 ) != 0; }
			set { SaveIntSetting( "VisualStudioSupport", value ? 1 : 0 ); }
		}
	}
}
