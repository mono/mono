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
using System.Collections;

namespace NUnit.Util
{
	/// <summary>
	/// Base class for settings that hold lists of recent files
	/// </summary>
	public abstract class RecentFileSettings : SettingsGroup
	{
		// TODO: This class does more loading and
		// storing than it should but this is the
		// current simplest solution to having
		// multiple recentfiles objects around.
		// We can fix this by using a singleton.
		private IList fileEntries;

		public static readonly int MinSize = 1;

		public static readonly int MaxSize = 24;

		public static readonly int DefaultSize = 5;

		public RecentFileSettings( string name ) : base ( name, UserSettings.GetStorageImpl( name ) )
		{
			LoadFiles();
		}

		public RecentFileSettings( string name, SettingsStorage storage ) : base( name, storage ) 
		{
			LoadFiles();
		}

		public RecentFileSettings( string name, SettingsGroup parent ) : base( name, parent ) 
		{ 
			LoadFiles();
		}

		public int MaxFiles
		{
			get 
			{ 
				int size = LoadIntSetting( "MaxFiles", DefaultSize );
				
				if ( size < MinSize ) size = MinSize;
				if ( size > MaxSize ) size = MaxSize;
				
				return size;
			}
			set 
			{ 
				int oldSize = MaxFiles;
				int newSize = value;
				
				if ( newSize < MinSize ) newSize = MinSize;
				if ( newSize > MaxSize ) newSize = MaxSize;

				SaveIntSetting( "MaxFiles", newSize );
				if ( newSize < oldSize ) SaveSettings();
			}
		}

		protected void LoadFiles()
		{
			fileEntries = new ArrayList();
			for ( int index = 1; index <= MaxFiles; index++ )
			{
				string fileName = LoadStringSetting( ValueName( index ) );
				if ( fileName != null )
					fileEntries.Add( fileName );
			}
		}

		public override void Clear()
		{
			base.Clear();
			fileEntries = new ArrayList();
		}

		public IList GetFiles()
		{
			LoadFiles();
			return fileEntries;
		}
		
		public string RecentFile
		{
			get 
			{ 
				LoadFiles();
				if( fileEntries.Count > 0 )
					return (string)fileEntries[0];

				return null;
			}
			set
			{
				LoadFiles();

				int index = fileEntries.IndexOf(value);

				if(index == 0) return;

				if(index != -1)
				{
					fileEntries.RemoveAt(index);
				}

				fileEntries.Insert( 0, value );
				if( fileEntries.Count > MaxFiles )
					fileEntries.RemoveAt( MaxFiles );

				SaveSettings();			
			}
		}

		public void Remove( string fileName )
		{
			LoadFiles();
			fileEntries.Remove( fileName );
			SaveSettings();
		}

		private void SaveSettings()
		{
			while( fileEntries.Count > MaxFiles )
				fileEntries.RemoveAt( fileEntries.Count - 1 );

			for( int index = 0; index < MaxSize; index++ ) 
			{
				string valueName = ValueName( index + 1 );
				if ( index < fileEntries.Count )
					SaveSetting( valueName, fileEntries[index] );
				else
					RemoveSetting( valueName );
			}
		}

		private string ValueName( int index )
		{
			return string.Format( "File{0}", index );
		}
	}
}
