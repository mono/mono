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
using System.Diagnostics;
using Microsoft.Win32;

namespace NUnit.Util
{
	/// <summary>
	/// Implementation of SettingsStorage for NUnit user settings,
	/// based on storage of settings in the registry.
	/// </summary>
	public class RegistrySettingsStorage : SettingsStorage, IDisposable
	{
		#region Instance Variables

		/// <summary>
		/// If not null, the registry key for this storage
		/// </summary>
		private RegistryKey storageKey;

		#endregion

		#region Construction and Disposal

		/// <summary>
		/// Construct a storage as a child of another storage
		/// </summary>
		/// <param name="storageName">The name to give the storage</param>
		/// <param name="parentStorage">The parent in which the storage is to be created</param>
		public RegistrySettingsStorage( string storageName, RegistrySettingsStorage parentStorage ) 
			: base( storageName, parentStorage ) 
		{ 
			this.storageKey = parentStorage.StorageKey.CreateSubKey( storageName );
		}

		/// <summary>
		/// Construct a storage using a registry key. This constructor is
		/// intended for use at the top level of the hierarchy.
		/// </summary>
		/// <param name="storageName">The name to give the storage</param>
		/// <param name="parentKey">The registry Key under which the storage will be created</param>
		public RegistrySettingsStorage( string storageName, RegistryKey parentKey ) 
			: base ( storageName, null )
		{
			this.storageKey = parentKey.CreateSubKey( storageName );
		}

		/// <summary>
		/// Construct a storage on top of a given key, using the key's name
		/// </summary>
		/// <param name="storageKey"></param>
		public RegistrySettingsStorage( RegistryKey storageKey )
			: base( storageKey.Name, null )
		{
			this.storageKey = storageKey;
		}

		/// <summary>
		/// Dispose of this object by closing the storage key, if any
		/// </summary>
		public override void Dispose()
		{
			if ( storageKey != null )
				storageKey.Close();
		}

		#endregion

		#region Properties

		/// <summary>
		/// The registry key used to hold this storage
		/// </summary>
		public RegistryKey StorageKey
		{
			get { return storageKey; }
		}

		/// <summary>
		/// The count of settings in this storage
		/// </summary>
		public override int SettingsCount
		{
			get { return storageKey.ValueCount; }
		}

		#endregion

		#region Methods

		/// <summary>
		/// Find out if a child storage exists
		/// </summary>
		/// <param name="storageName">Name of the child storage</param>
		/// <returns>True if the child storage exists</returns>
		public override bool ChildStorageExists( string storageName )
		{
			using (RegistryKey key = storageKey.OpenSubKey( storageName ) )
			{
				return key != null;
			}
		}

		/// <summary>
		/// Make a new child storage under this one
		/// </summary>
		/// <param name="storageName">Name of the child storage to make</param>
		/// <returns>New storage</returns>
		public override SettingsStorage MakeChildStorage( string storageName )
		{
			return new RegistrySettingsStorage( storageName, this );
		}

		/// <summary>
		/// Load a setting from this storage
		/// </summary>
		/// <param name="settingName">Name of the setting to load</param>
		/// <returns>Value of the setting</returns>
		public override object LoadSetting( string settingName )
		{
			return storageKey.GetValue( settingName );
		}

		/// <summary>
		/// Load an int setting from this storage. Since int is a
		/// value type, we can't return null so zero is used to
		/// indicate that nothing was found - or the found value
		/// was zero. If you need to distinguish, use your own 
		/// default value or call LoadSetting and check for null.
		/// </summary>
		/// <param name="settingName">Name of the setting to load</param>
		/// <returns>Value of the setting or zero if missing</returns>
		public override int LoadIntSetting( string settingName )
		{
			return LoadIntSetting( settingName, 0 );
		}

		/// <summary>
		/// Load a string setting from this storage
		/// </summary>
		/// <param name="settingName">Name of the setting to load</param>
		/// <returns>Value of the setting</returns>
		public override string LoadStringSetting( string settingName )
		{
			object resultValue = storageKey.GetValue( settingName );
			if ( resultValue == null || resultValue is string )
				return (string) resultValue;

			return resultValue.ToString();
		}

		/// <summary>
		/// Load a setting from this storage or return a default value
		/// </summary>
		/// <param name="settingName">Name of setting to load</param>
		/// <param name="defaultValue">Value to return if the seeting is not present</param>
		/// <returns>Value of the setting or the default</returns>
		public override object LoadSetting( string settingName, object defaultValue )
		{
			return storageKey.GetValue( settingName, defaultValue );
		}

		/// <summary>
		/// Load an integer setting from this storage or return a default value
		/// </summary>
		/// <param name="settingName">Name of setting to load</param>
		/// <param name="defaultValue">Value to return if the seeting is not present</param>
		/// <returns>Value of the setting or the default</returns>
		public override int LoadIntSetting( string settingName, int defaultValue )
		{
			object resultValue = storageKey.GetValue( settingName, defaultValue );
			if ( resultValue is int )
				return (int)resultValue;
			
			return int.Parse( (string)resultValue );
		}

		/// <summary>
		/// Load a string setting from this storage or return a default value
		/// </summary>
		/// <param name="settingName">Name of setting to load</param>
		/// <param name="defaultValue">Value to return if the seeting is not present</param>
		/// <returns>Value of the setting or the default</returns>
		public override string LoadStringSetting( string settingName, string defaultValue )
		{
			object resultValue = storageKey.GetValue( settingName, defaultValue );
			if ( resultValue is string )
				return (string) resultValue;

			return resultValue.ToString();
		}

		/// <summary>
		/// Remove a setting from the storage
		/// </summary>
		/// <param name="settingName">Name of the setting to remove</param>
		public override void RemoveSetting( string settingName )
		{
			storageKey.DeleteValue( settingName, false );
		}

		/// <summary>
		/// Save a setting in this storage
		/// </summary>
		/// <param name="settingName">Name of the setting to save</param>
		/// <param name="settingValue">Value to be saved</param>
		public override void SaveSetting( string settingName, object settingValue )
		{
			storageKey.SetValue( settingName, settingValue );
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

		/// <summary>
		/// Clear all settings from the storage - empty storage remains
		/// </summary>
		public override void Clear()
		{
			ClearKey( storageKey );
		}

		#endregion
	}
}
