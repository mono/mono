// ****************************************************************
// Copyright 2007, Charlie Poole
// This is free software licensed under the NUnit license. You may
// obtain a copy of the license at http://nunit.org/?p=license&r=2.4
// ****************************************************************

using System;

namespace NUnit.Util
{
	public delegate void SettingsEventHandler( object sender, SettingsEventArgs args );

	public class SettingsEventArgs : EventArgs
	{
		private string settingName;

		public SettingsEventArgs( string settingName )
		{
			this.settingName = settingName;
		}

		public string SettingName
		{
			get { return settingName; }
		}
	}

	/// <summary>
	/// The ISettings interface is used to access all user
	/// settings and options.
	/// </summary>
	public interface ISettings
	{
		event SettingsEventHandler Changed;

		/// <summary>
		/// Load a setting from the storage.
		/// </summary>
		/// <param name="settingName">Name of the setting to load</param>
		/// <returns>Value of the setting or null</returns>
		object GetSetting( string settingName );

		/// <summary>
		/// Load a setting from the storage or return a default value
		/// </summary>
		/// <param name="settingName">Name of the setting to load</param>
		/// <param name="settingName">Value to return if the setting is missing</param>
		/// <returns>Value of the setting or the default value</returns>
		object GetSetting( string settingName, object defaultValue );

		/// <summary>
		/// Load an integer setting from the storage or return a default value
		/// </summary>
		/// <param name="settingName">Name of the setting to load</param>
		/// <param name="defaultValue">Value to return if the setting is missing</param>
		/// <returns>Value of the setting or the default value</returns>
		int GetSetting( string settingName, int defaultValue );

		/// <summary>
		/// Load a boolean setting or return a default value
		/// </summary>
		/// <param name="settingName">Name of setting to load</param>
		/// <param name="defaultValue">Value to return if the setting is missing</param>
		/// <returns>Value of the setting or the default value</returns>
		bool GetSetting( string settingName, bool defaultValue );

		/// <summary>
		/// Load a string setting from the storage or return a default value
		/// </summary>
		/// <param name="settingName">Name of the setting to load</param>
		/// <param name="defaultValue">Value to return if the setting is missing</param>
		/// <returns>Value of the setting or the default value</returns>
		string GetSetting( string settingName, string defaultValue );

		/// <summary>
		/// Load an enum setting from the storage or return a default value
		/// </summary>
		/// <param name="settingName">Name of the setting to load</param>
		/// <param name="defaultValue">Value to return if the setting is missing</param>
		/// <returns>Value of the setting or the default value</returns>
		System.Enum GetSetting( string settingName, System.Enum defaultValue );

		/// <summary>
		/// Remove a setting from the storage
		/// </summary>
		/// <param name="settingName">Name of the setting to remove</param>
		void RemoveSetting( string settingName );

		/// <summary>
		/// Remove an entire group of settings from the storage
		/// </summary>
		/// <param name="groupName">Name of the group to remove</param>
		void RemoveGroup( string groupName );

		/// <summary>
		/// Save a setting in the storage
		/// </summary>
		/// <param name="settingName">Name of the setting to save</param>
		/// <param name="settingValue">Value to be saved</param>
		void SaveSetting( string settingName, object settingValue );
	}
}
