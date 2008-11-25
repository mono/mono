// ****************************************************************
// Copyright 2007, Charlie Poole
// This is free software licensed under the NUnit license. You may
// obtain a copy of the license at http://nunit.org/?p=license&r=2.4
// ****************************************************************

using System;
using NUnit.Core;
using NUnit.Core.Extensibility;

namespace NUnit.Util
{
	/// <summary>
	/// Summary description for Services
	/// </summary>
	public class Services
	{
		#region AddinManager
		private static AddinManager addinManager;
		public static AddinManager AddinManager
		{
			get 
			{
				if (addinManager == null )
					addinManager = (AddinManager)ServiceManager.Services.GetService( typeof( AddinManager ) );

				return addinManager; 
			}
		}
		#endregion

		#region AddinRegistry
		private static IAddinRegistry addinRegistry;
		public static IAddinRegistry AddinRegistry
		{
			get 
			{
				if (addinRegistry == null)
					addinRegistry = (IAddinRegistry)ServiceManager.Services.GetService( typeof( IAddinRegistry ) );
                
				return addinRegistry;
			}
		}
		#endregion

		#region DomainManager
		private static DomainManager domainManager;
		public static DomainManager DomainManager
		{
			get
			{
				if ( domainManager == null )
					domainManager = (DomainManager)ServiceManager.Services.GetService( typeof( DomainManager ) );

				return domainManager;
			}
		}
		#endregion

		#region UserSettings
		private static SettingsService userSettings;
		public static SettingsService UserSettings
		{
			get 
			{ 
				if ( userSettings == null )
					userSettings = (SettingsService)ServiceManager.Services.GetService( typeof( SettingsService ) );

				// Temporary fix needed to run TestDomain tests in test AppDomain
				// TODO: Figure out how to set up the test domain correctly
				if ( userSettings == null )
					userSettings = new SettingsService();

				return userSettings; 
			}
		}
		
		#endregion

		#region RecentFilesService
		private static RecentFiles recentFiles;
		public static RecentFiles RecentFiles
		{
			get
			{
				if ( recentFiles == null )
					recentFiles = (RecentFiles)ServiceManager.Services.GetService( typeof( RecentFiles ) );

				return recentFiles;
			}
		}
		#endregion

		#region TestLoader
		private static TestLoader loader;
		public static TestLoader TestLoader
		{
			get
			{
				if ( loader == null )
					loader = (TestLoader)ServiceManager.Services.GetService( typeof( TestLoader ) );

				return loader;
			}
		}
		#endregion

		#region TestAgency
		private static TestAgency agency;
		public static TestAgency TestAgency
		{
			get
			{
				if ( agency == null )
					agency = (TestAgency)ServiceManager.Services.GetService( typeof( TestAgency ) );

				// Temporary fix needed to run ProcessRunner tests in test AppDomain
				// TODO: Figure out how to set up the test domain correctly
//				if ( agency == null )
//				{
//					agency = new TestAgency();
//					agency.Start();
//				}

				return agency;
			}
		}
		#endregion
	}
}
