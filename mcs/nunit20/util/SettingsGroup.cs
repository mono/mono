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
	using System.Collections;

	/// <summary>
	/// SettingsGroup is the base class representing a group
	/// of user or system settings. A pimpl idiom is used
	/// to provide implementation-independence.
	/// </summary>
	public class SettingsGroup : IDisposable
	{
		#region Instance Variables
		/// <summary>
		/// The name of this group of settings
		/// </summary>
		private string name;

		/// <summary>
		/// If not null, the storage implementation holding the group settings.
		/// </summary>
		private SettingsStorage storageImpl;
		
		/// <summary>
		/// If not null, the settings group that contains this one.
		/// </summary>
		private SettingsGroup parentSettings;

		#endregion

		#region Construction and Disposal

		/// <summary>
		/// Construct a settings group based on a storage implementation.
		/// </summary>
		/// <param name="name">Name of the group</param>
		/// <param name="storageImpl">Storage for the group settings</param>
		public SettingsGroup( string name, SettingsStorage storageImpl )
		{
			this.name = name;
			this.storageImpl = storageImpl;
		}

		/// <summary>
		/// Construct a settings group based on a parent group that contains it.
		/// </summary>
		/// <param name="name">Name of the group</param>
		/// <param name="parentSettings">Containing  group</param>
		public SettingsGroup( string name, SettingsGroup parentSettings )
		{
			this.name = name;
			this.parentSettings = parentSettings;
			this.storageImpl = parentSettings.Storage.MakeChildStorage( name );
		}

		/// <summary>
		/// Dispose of this group by disposing of it's storage implementation
		/// </summary>
		public void Dispose()
		{
			if ( storageImpl != null )
			{
				storageImpl.Dispose();
				storageImpl = null;
			}
		}

		#endregion

		#region Properties

		/// <summary>
		/// The name of the group
		/// </summary>
		public string Name
		{
			get { return name; }
		}

		/// <summary>
		/// The storage used for the group settings
		/// </summary>
		public SettingsStorage Storage
		{
			get { return storageImpl; }
		}

		/// <summary>
		/// The number of settings in this group
		/// </summary>
		public int SettingsCount
		{
			get { return storageImpl.SettingsCount; }
		}

		#endregion

		#region Methods

		/// <summary>
		/// Clear all settings and subgroups in this group
		/// </summary>
		public virtual void Clear()
		{
			storageImpl.Clear();
		}

		/// <summary>
		/// Load the value of one of the group's settings
		/// </summary>
		/// <param name="settingName">Name of setting to load</param>
		/// <returns>Value of the setting or null</returns>
		public object LoadSetting( string settingName )
		{
			return storageImpl.LoadSetting( settingName );
		}

		/// <summary>
		/// Load the value of one of the group's integer settings
		/// in a type-safe manner.
		/// </summary>
		/// <param name="settingName">Name of setting to load</param>
		/// <returns>Value of the setting or null</returns>
		public int LoadIntSetting( string settingName )
		{
			return storageImpl.LoadIntSetting( settingName );
		}

		/// <summary>
		/// Load the value of one of the group's string settings
		/// in a type-safe manner.
		/// </summary>
		/// <param name="settingName">Name of setting to load</param>
		/// <returns>Value of the setting or null</returns>
		public string LoadStringSetting( string settingName )
		{
			return storageImpl.LoadStringSetting( settingName );
		}

		/// <summary>
		/// Load the value of one of the group's settings or return a default value
		/// </summary>
		/// <param name="settingName">Name of setting to load</param>
		/// <param name="defaultValue">Value to return if the seeting is not present</param>
		/// <returns>Value of the setting or the default</returns>
		public object LoadSetting( string settingName, object defaultValue )
		{
			return storageImpl.LoadSetting( settingName, defaultValue );
		}

		/// <summary>
		/// Load the value of one of the group's integer settings
		/// in a type-safe manner or return a default value
		/// </summary>
		/// <param name="settingName">Name of setting to load</param>
		/// <param name="defaultValue">Value to return if the seeting is not present</param>
		/// <returns>Value of the setting or the default</returns>
		public int LoadIntSetting( string settingName, int defaultValue )
		{
			return storageImpl.LoadIntSetting( settingName, defaultValue );
		}

		/// <summary>
		/// Load the value of one of the group's string settings
		/// in a type-safe manner or return a default value
		/// </summary>
		/// <param name="settingName">Name of setting to load</param>
		/// <param name="defaultValue">Value to return if the seeting is not present</param>
		/// <returns>Value of the setting or the default</returns>
		public string LoadStringSetting( string settingName, string defaultValue )
		{
			return storageImpl.LoadStringSetting( settingName, defaultValue );
		}

		/// <summary>
		/// Remove a setting from the group
		/// </summary>
		/// <param name="settingName">Name of the setting to remove</param>
		public void RemoveSetting( string settingName )
		{
			storageImpl.RemoveSetting( settingName );
		}

		/// <summary>
		/// Save the value of one of the group's settings
		/// </summary>
		/// <param name="settingName">Name of the setting to save</param>
		/// <param name="settingValue">Value to be saved</param>
		public void SaveSetting( string settingName, object settingValue )
		{
			storageImpl.SaveSetting( settingName, settingValue );
		}

		/// <summary>
		/// Save the value of one of the group's integer settings
		/// in a type-safe manner.
		/// </summary>
		/// <param name="settingName">Name of the setting to save</param>
		/// <param name="settingValue">Value to be saved</param>
		public void SaveIntSetting( string settingName, int settingValue )
		{
			storageImpl.SaveSetting( settingName, settingValue );
		}

		/// <summary>
		/// Save the value of one of the group's string settings
		/// in a type-safe manner.
		/// </summary>
		/// <param name="settingName">Name of the setting to save</param>
		/// <param name="settingValue">Value to be saved</param>
		public void SaveStringSetting( string settingName, string settingValue )
		{
			storageImpl.SaveSetting( settingName, settingValue );
		}

		#endregion
	}
}
