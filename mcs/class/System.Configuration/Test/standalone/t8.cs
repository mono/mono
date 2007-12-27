using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using System.Configuration;
using System.Web;

class T1
{
	static void Main(string[] args)
	{
		try
		{
			NameValueCollection AppSettings = ConfigurationManager.AppSettings;
			Configuration config = ConfigurationManager.OpenExeConfiguration (ConfigurationUserLevel.None);
			AppSettingsSection appsettings = config.AppSettings;

			try {
				AppSettings.Add ("fromtest", "valuehere");
			}
			catch {
				Console.WriteLine ("ConfigurationManager.AppSettings.Add resulted in exception");
			}

			AppSettings = new NameValueCollection (AppSettings);
			foreach (string key in AppSettings.AllKeys) {
				Console.WriteLine ("AppSettings[{0}] = {1}", key, AppSettings[key]);
			}

			foreach (string key in appsettings.Settings.AllKeys) {
				Console.WriteLine ("settings[{0}] = {1}", appsettings.Settings[key].Key, appsettings.Settings[key].Value);
			}
		}
		catch (Exception e)
		{
			// Error.
			Console.WriteLine(e.ToString());
		}
	}
}
