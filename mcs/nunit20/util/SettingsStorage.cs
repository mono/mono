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

	/// <summary>
	/// Abstract class representing a hierarchical storage used to hold
	/// application settings. The actual implementation is left to 
	/// derived classes, and may be based on the registry, isolated
	/// storage or any other mechanism.
	/// </summary>
	public abstract class SettingsStorage : IDisposable
	{
		#region Instance Variables

		/// <summary>
		/// The name of this storage
		/// </summary>
		private string storageName;
		
		/// <summary>
		/// The parent storage containing this storage
		/// </summary>
		private SettingsStorage parentStorage;
		#endregion

		#region Construction and Disposal

		/// <summary>
		/// Construct a SettingsStorage under a parent storage
		/// </summary>
		/// <param name="storageName">Name of the storage</param>
		/// <param name="parentStorage">The parent which contains the new storage</param>
		public SettingsStorage( string storageName, SettingsStorage parentStorage )
		{
			this.storageName = storageName;
			this.parentStorage = parentStorage;
		}

		/// <summary>
		/// Dispose of resources held by this storage
		/// </summary>
		public abstract void Dispose();

		#endregion

		#region Properties

		/// <summary>
		/// The number of settings in this group
		/// </summary>
		public abstract int SettingsCount
		{
			get;
		}

		/// <summary>
		/// The name of the storage
		/// </summary>
		public string StorageName
		{
			get { return storageName; }
		}

		/// <summary>
		/// The storage that contains this one
		/// </summary>
		public SettingsStorage ParentStorage
		{
			get { return parentStorage; }
		}

		#endregion

		#region Methods

		/// <summary>
		/// Find out if a substorage exists
		/// </summary>
		/// <param name="name">Name of the child storage</param>
		/// <returns>True if the storage exists</returns>
		public abstract bool ChildStorageExists( string name );

		/// <summary>
		/// Create a child storage of the same type
		/// </summary>
		/// <param name="name">Name of the child storage</param>
		/// <returns>New child storage</returns>
		public abstract SettingsStorage MakeChildStorage( string name );

		/// <summary>
		/// Clear all settings from the storage - empty storage remains
		/// </summary>
		public abstract void Clear();

		/// <summary>
		/// Load a setting from the storage.
		/// </summary>
		/// <param name="settingName">Name of the setting to load</param>
		/// <returns>Value of the setting or null</returns>
		public abstract object LoadSetting( string settingName );

		/// <summary>
		/// Load an integer setting from the storage
		/// </summary>
		/// <param name="settingName">Name of the setting to load</param>
		/// <returns>Value of the setting or null</returns>
		public abstract int LoadIntSetting( string settingName );

		/// <summary>
		/// Load a string setting from the storage
		/// </summary>
		/// <param name="settingName">Name of the setting to load</param>
		/// <returns>Value of the setting or null</returns>
		public abstract string LoadStringSetting( string settingName );

		/// <summary>
		/// Load a setting from the storage or return a default value
		/// </summary>
		/// <param name="settingName">Name of the setting to load</param>
		/// <param name="settingName">Value to return if the setting is missing</param>
		/// <returns>Value of the setting or the default value</returns>
		public abstract object LoadSetting( string settingName, object defaultValue );

		/// <summary>
		/// Load an integer setting from the storage or return a default value
		/// </summary>
		/// <param name="settingName">Name of the setting to load</param>
		/// <param name="settingName">Value to return if the setting is missing</param>
		/// <returns>Value of the setting or the default value</returns>
		public abstract int LoadIntSetting( string settingName, int defaultValue );

		/// <summary>
		/// Load a string setting from the storage or return a default value
		/// </summary>
		/// <param name="settingName">Name of the setting to load</param>
		/// <param name="settingName">Value to return if the setting is missing</param>
		/// <returns>Value of the setting or the default value</returns>
		public abstract string LoadStringSetting( string settingName, string defaultValue );

		/// <summary>
		/// Remove a setting from the storage
		/// </summary>
		/// <param name="settingName">Name of the setting to remove</param>
		public abstract void RemoveSetting( string settingName );

		/// <summary>
		/// Save a setting in the storage
		/// </summary>
		/// <param name="settingName">Name of the setting to save</param>
		/// <param name="settingValue">Value to be saved</param>
		public abstract void SaveSetting( string settingName, object settingValue );

		#endregion
	}
}
