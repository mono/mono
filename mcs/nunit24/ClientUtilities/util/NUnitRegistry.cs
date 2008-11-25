// ****************************************************************
// This is free software licensed under the NUnit license. You
// may obtain a copy of the license as well as information regarding
// copyright ownership at http://nunit.org/?p=license&r=2.4.
// ****************************************************************

namespace NUnit.Util
{
	using System;
	using System.IO;
	using System.Text;
	using Microsoft.Win32;

	/// <summary>
	/// NUnitRegistry provides static properties for NUnit's
	/// CurrentUser and LocalMachine subkeys.
	/// </summary>
	public class NUnitRegistry
	{
		public static readonly string KEY = 
			@"Software\nunit.org\Nunit\2.4";

		public static readonly string LEGACY_KEY = 
			@"Software\Nascent Software\Nunit\";

		private static bool testMode = false;
		public static readonly string TEST_KEY = 
			@"Software\nunit.org\Nunit-Test";


		/// <summary>
		/// Prevent construction of object
		/// </summary>
		private NUnitRegistry() { }

		public static bool TestMode
		{
			get { return testMode; }
			set { testMode = value; }
		}

		/// <summary>
		/// Registry subkey for the current user
		/// </summary>
		public static RegistryKey CurrentUser
		{
			get 
			{
				if ( testMode )
					return Registry.CurrentUser.CreateSubKey( TEST_KEY );
				
				RegistryKey newKey = Registry.CurrentUser.OpenSubKey( KEY, true );
				if (newKey == null)
				{
					newKey = Registry.CurrentUser.CreateSubKey( KEY );
					RegistryKey oldKey = Registry.CurrentUser.OpenSubKey( LEGACY_KEY );
					if ( oldKey != null )
					{
						CopyKey( oldKey, newKey );
						oldKey.Close();
					}
				}

				return newKey; 
			}
		}

		public static bool KeyExists( string subkey )
		{
			using ( RegistryKey key = Registry.CurrentUser.OpenSubKey( subkey, true ) )
			{
				return key != null;
			} 
		}

		/// <summary>
		/// Registry subkey for the local machine
		/// </summary>
		public static RegistryKey LocalMachine
		{
			get { return Registry.LocalMachine.CreateSubKey( testMode ? TEST_KEY : KEY ); }
		}

		public static void ClearTestKeys()
		{
			ClearSubKey( Registry.CurrentUser, TEST_KEY );
			//ClearSubKey( Registry.LocalMachine, TEST_KEY );	
		}

		/// <summary>
		/// Static helper method that clears out the contents of a subkey
		/// </summary>
		/// <param name="baseKey">Base key for the subkey</param>
		/// <param name="subKey">Name of the subkey</param>
		private static void ClearSubKey( RegistryKey baseKey, string subKey )
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

			// TODO: This throws under Mono - Restore when bug is fixed
			//foreach( string name in key.GetSubKeyNames() )
			//    key.DeleteSubKeyTree( name );

			foreach (string name in key.GetSubKeyNames())
			{
				ClearSubKey(key, name);
				key.DeleteSubKey( name );
			}
		}

		/// <summary>
		/// Static method that copies the contents of one key to another
		/// </summary>
		/// <param name="fromKey">The source key for the copy</param>
		/// <param name="toKey">The target key for the copy</param>
		public static void CopyKey( RegistryKey fromKey, RegistryKey toKey )
		{
			foreach( string name in fromKey.GetValueNames() )
				toKey.SetValue( name, fromKey.GetValue( name ) );

			foreach( string name in fromKey.GetSubKeyNames() )
				using( RegistryKey fromSubKey = fromKey.OpenSubKey( name ) )
				using( RegistryKey toSubKey = toKey.CreateSubKey( name ) )
				{
					CopyKey( fromSubKey, toSubKey );
				}
		}
	}
}
