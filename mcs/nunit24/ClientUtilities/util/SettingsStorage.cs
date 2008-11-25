// ****************************************************************
// Copyright 2002-2003, Charlie Poole
// This is free software licensed under the NUnit license. You may
// obtain a copy of the license at http://nunit.org/?p=license&r=2.4
// ****************************************************************

namespace NUnit.Util
{
	using System;

	/// <summary>
	/// The ISettingsStorage interface is implemented by all
	/// types of backing storage for settings.
	/// </summary>
	public interface ISettingsStorage : IDisposable
	{
		/// <summary>
		/// Load a setting from the storage.
		/// </summary>
		/// <param name="settingName">Name of the setting to load</param>
		/// <returns>Value of the setting or null</returns>
		object GetSetting( string settingName );

		/// <summary>
		/// Remove a setting from the storage
		/// </summary>
		/// <param name="settingName">Name of the setting to remove</param>
		void RemoveSetting( string settingName );

		/// <summary>
		/// Remove a group of settings from the storae
		/// </summary>
		/// <param name="groupName">Name of the group to remove</param>
		void RemoveGroup( string groupName );

		/// <summary>
		/// Save a setting in the storage
		/// </summary>
		/// <param name="settingName">Name of the setting to save</param>
		/// <param name="settingValue">Value to be saved</param>
		void SaveSetting( string settingName, object settingValue );

		/// <summary>
		/// Create a child storage of the same type
		/// </summary>
		/// <param name="name">Name of the child storage</param>
		/// <returns>New child storage</returns>
		ISettingsStorage MakeChildStorage( string name );

		/// <summary>
		/// Load settings from external storage if required
		/// by the implementation.
		/// </summary>
		void LoadSettings();

		/// <summary>
		/// Save settings to external storage if required
		/// by the implementation.
		/// </summary>
		void SaveSettings();
	}
}
