//
// System.Configuration.ConfigurationSettings.cs
//
// Author:
//   Christopher Podurgiel (cpodurgiel@msn.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//

using System;
using System.Collections.Specialized;

namespace System.Configuration
{
	/// <summary>
	///		Component class.
	/// </summary>
	/// <remarks>
	///		Longer description
	/// </remarks>

	public sealed class ConfigurationSettings
	{

		private NameValueCollection appsettings;


		/// <summary>
		///		ConfigurationSettings Constructor.
		/// </summary>
		public ConfigurationSettings ()
		{
			appsettings = new NameValueCollection();
		}

		/// <summary>
		///		Returns configuration settings for a user-defined configuration section.
		/// </summary>
		/// <param name="sectionName"></param>
		/// <returns></returns>
		public static object GetConfig(	string sectionName)
		{
			//FIXME: Not sure how to determine the correct .config file to parse.
			return null;
		}

		/// <summary>
		///		Get the Application Configuration Settings.
		/// </summary>
		public NameValueCollection AppSettings
		{
			get
			{
				return appsettings;
			}
		}
	}
}


