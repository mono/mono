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
using System.Collections;

namespace NUnit.Util
{
	/// <summary>
	/// RecentAssemblySettings holds settings for recent assemblies
	/// </summary>
	public class RecentAssemblySettings : SettingsGroup
	{
		private static readonly string NAME = "Recent-Assemblies";
		
		private static string[] valueNames = {	"RecentAssembly1", 
												"RecentAssembly2", 
												"RecentAssembly3", 
												"RecentAssembly4", 
												"RecentAssembly5" };

		private IList assemblyEntries;

		public RecentAssemblySettings( ) : base ( NAME, UserSettings.GetStorageImpl( NAME ) )
		{
			LoadAssemblies();
		}

		public RecentAssemblySettings( SettingsStorage storage ) : base( NAME, storage ) 
		{
			LoadAssemblies();
		}

		public RecentAssemblySettings( SettingsGroup parent ) : base( NAME, parent ) 
		{ 
			LoadAssemblies();
		}

		private void LoadAssemblies()
		{
			assemblyEntries = new ArrayList();
			foreach( string valueName in valueNames )
			{
				string assemblyName = LoadStringSetting(valueName);
				if(assemblyName != null)
					assemblyEntries.Add(assemblyName);
			}
		}

		public override void Clear()
		{
			base.Clear();
			assemblyEntries = new ArrayList();
		}

		public IList GetAssemblies()
		{
			return assemblyEntries;
		}
		
		public string RecentAssembly
		{
			get 
			{ 
				if(assemblyEntries.Count > 0)
					return (string)assemblyEntries[0];

				return null;
			}
			set
			{
				int index = assemblyEntries.IndexOf(value);

				if(index == 0) return;

				if(index != -1)
				{
					assemblyEntries.RemoveAt(index);
				}

				assemblyEntries.Insert(0, value);
				if(assemblyEntries.Count > valueNames.Length)
					assemblyEntries.RemoveAt(valueNames.Length);

				SaveSettings();			
			}
		}

		public void Remove(string assemblyName)
		{
			assemblyEntries.Remove(assemblyName);
			SaveSettings();
		}

		private void SaveSettings()
		{
			for ( int index = 0; 
				  index < valueNames.Length;
				  index++)
			{
				if ( index < assemblyEntries.Count )
					SaveSetting( valueNames[index], assemblyEntries[index] );
				else
					RemoveSetting( valueNames[index] );
			}
		}
	}
}
