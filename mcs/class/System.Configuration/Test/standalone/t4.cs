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

			Console.WriteLine ("Count: {0}", appsettings.Settings.AllKeys.Length);
			Console.WriteLine ("AppSettings.Count: {0}", AppSettings.Count);
		}
		catch (Exception e)
		{
			// Error.
			Console.WriteLine(e.ToString());
		}
	}
}
