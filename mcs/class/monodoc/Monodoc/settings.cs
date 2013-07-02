using System;
using System.Configuration;
using System.Collections.Specialized;

namespace Monodoc
{
	public static class Config
	{
		static KeyValueConfigurationCollection libConfig;
		static KeyValueConfigurationCollection exeConfig;

		static Config ()
		{
			try {
				var config = ConfigurationManager.OpenExeConfiguration (System.Reflection.Assembly.GetExecutingAssembly ().Location);
				libConfig = config.AppSettings.Settings;
			} catch {}

			try {
				exeConfig = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None).AppSettings.Settings;
			} catch {}
		}

		public static string Get (string key)
		{
			KeyValueConfigurationElement element = null;
			// We check the configuration in order: app first and then library itself
			if (exeConfig != null)
				element = exeConfig[key];
			if (element == null && libConfig != null)
				element = libConfig[key];

			return element == null ? null : element.Value;
		}

		public static KeyValueConfigurationCollection AppSettings {
			get {
				return exeConfig;
			}
		}

		public static KeyValueConfigurationCollection LibSettings {
			get {
				return libConfig;
			}
		}
	}
}
