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

namespace NUnit.Util
{
	using System;
	using System.IO;
	using System.Text;
	using System.Windows.Forms;
	using Microsoft.Win32;

	/// <summary>
	/// NUnitRegistry provides static properties for NUnit's
	/// CurrentUser and LocalMachine subkeys.
	/// </summary>
	public class NUnitRegistry
	{
		private static readonly string KEY = 
			@"Software\Nascent Software\Nunit\";

		private static bool testMode = false;
		private static string testKey = 
			@"Software\Nascent Software\Nunit-Test";


		/// <summary>
		/// Prevent construction of object
		/// </summary>
		private NUnitRegistry() { }

		public static bool TestMode
		{
			get { return testMode; }
			set { testMode = value; }
		}

		public static string TestKey
		{
			get { return testKey; }
			set { testKey = value; }
		}

		/// <summary>
		/// Registry subkey for the current user
		/// </summary>
		public static RegistryKey CurrentUser
		{
			get 
			{
				// Todo: Code can go here to migrate the registry
				// if we change our location.
				//	Try to open new key
				//	if ( key doesn't exist )
				//		create it
				//		open old key
				//		if ( it was opened )
				//			copy entries to new key
				//	return new key
				return Registry.CurrentUser.CreateSubKey( testMode ? testKey : KEY ); 
			}
		}

		/// <summary>
		/// Registry subkey for the local machine
		/// </summary>
		public static RegistryKey LocalMachine
		{
			get { return Registry.LocalMachine.CreateSubKey( testMode ? testKey : KEY ); }
		}

		public static void ClearTestKeys()
		{
			ClearSubKey( Registry.CurrentUser, testKey );
			ClearSubKey( Registry.LocalMachine, testKey );	
		}

		/// <summary>
		/// Static function that clears out the contents of a subkey
		/// </summary>
		/// <param name="baseKey">Base key for the subkey</param>
		/// <param name="subKey">Name of the subkey</param>
		public static void ClearSubKey( RegistryKey baseKey, string subKey )
		{
			using( RegistryKey key = baseKey.OpenSubKey( subKey, true ) )
			{
				if ( key != null ) ClearKey( key );
			}
		}

		/// <summary>
		/// Static function that clears out the contents of a key
		/// </summary>
		/// <param name="key">Key to be cleared</param>
		public static void ClearKey( RegistryKey key )
		{
			foreach( string name in key.GetValueNames() )
				key.DeleteValue( name );

			foreach( string name in key.GetSubKeyNames() )
				key.DeleteSubKeyTree( name );
		}
	}
}
