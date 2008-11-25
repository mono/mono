// ****************************************************************
// Copyright 2002-2003, Charlie Poole
// This is free software licensed under the NUnit license. You may
// obtain a copy of the license at http://nunit.org/?p=license&r=2.4
// ****************************************************************

using System;
using System.Diagnostics;
using Microsoft.Win32;

namespace NUnit.Util
{
	/// <summary>
	/// Implementation of SettingsStorage for NUnit user settings,
	/// based on storage of settings in the registry.
	/// 
	/// Setting names containing a dot are interpreted as a 
	/// reference to a subkey. Only the first dot is used
	/// in this way, since the feature is only intended
	/// to support legacy registry settings, which are not
	/// nested any deeper.
	/// </summary>
	public class RegistrySettingsStorage : ISettingsStorage
	{
		#region Instance Variables

		/// <summary>
		/// If not null, the registry key for this storage
		/// </summary>
		private RegistryKey storageKey;

		#endregion

		#region Construction and Disposal

		/// <summary>
		/// Construct a storage on top of a pre-created registry key
		/// </summary>
		/// <param name="storageKey"></param>
		public RegistrySettingsStorage( RegistryKey storageKey )
		{
			this.storageKey = storageKey;
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

		#endregion

		#region ISettingsStorage Members

		/// <summary>
		/// Load a setting from this storage
		/// </summary>
		/// <param name="settingName">Name of the setting to load</param>
		/// <returns>Value of the setting</returns>
		public object GetSetting( string settingName )
		{
			int dot = settingName.IndexOf( '.' );
			if ( dot < 0 )
				return storageKey.GetValue( settingName );

			using( RegistryKey subKey = storageKey.OpenSubKey( settingName.Substring( 0, dot ) ) )
			{
				if ( subKey != null )
					return subKey.GetValue( settingName.Substring( dot + 1 ) );
			}

			return null;
		}

		/// <summary>
		/// Remove a setting from the storage
		/// </summary>
		/// <param name="settingName">Name of the setting to remove</param>
		public void RemoveSetting( string settingName )
		{
			int dot = settingName.IndexOf( '.' );
			if ( dot < 0 )
				storageKey.DeleteValue( settingName, false );
			else
			{
				using( RegistryKey subKey = storageKey.OpenSubKey( settingName.Substring( 0, dot ), true ) )
				{
					if ( subKey != null )
						subKey.DeleteValue( settingName.Substring( dot + 1 ) );
				}
			}
		}

		public void RemoveGroup( string groupName )
		{
			storageKey.DeleteSubKeyTree( groupName );
		}

		/// <summary>
		/// Save a setting in this storage
		/// </summary>
		/// <param name="settingName">Name of the setting to save</param>
		/// <param name="settingValue">Value to be saved</param>
		public void SaveSetting( string settingName, object settingValue )
		{
			object val = settingValue;
			if ( val is bool )
				val = ((bool)val) ? 1 : 0;

			int dot = settingName.IndexOf( '.' );
			if ( dot < 0 )
				storageKey.SetValue( settingName, val );
			else
			{
				using(  RegistryKey subKey = storageKey.CreateSubKey( settingName.Substring( 0, dot ) ) )
				{
					subKey.SetValue( settingName.Substring( dot + 1 ), val );
				}
			}
		}

		/// <summary>
		/// Make a new child storage under this one
		/// </summary>
		/// <param name="storageName">Name of the child storage to make</param>
		/// <returns>New storage</returns>
		public ISettingsStorage MakeChildStorage( string storageName )
		{
			return new RegistrySettingsStorage( storageKey.CreateSubKey( storageName ) );
		}

		/// <summary>
		/// LoadSettings does nothing in this implementation, since the
		/// registry is accessed directly.
		/// </summary>
		public void LoadSettings()
		{
		}

		/// <summary>
		/// SaveSettings does nothing in this implementation, since the
		/// registry is accessed directly.
		/// </summary>
		public void SaveSettings()
		{
		}
		#endregion

		#region IDisposable Members
		/// <summary>
		/// Dispose of this object by closing the storage key, if any
		/// </summary>
		public void Dispose()
		{
			if ( storageKey != null )
				storageKey.Close();
		}

		#endregion
	}
}
