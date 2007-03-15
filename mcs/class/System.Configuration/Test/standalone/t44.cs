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
			Configuration config = ConfigurationManager.OpenExeConfiguration (ConfigurationUserLevel.None);

			AppSettingsSection appSettings = (AppSettingsSection) config.GetSection ("appSettings");
			Console.Write ("appSettings is " + (appSettings == null ? "null" : "not null"));

			Console.Write (" , ");

			AppSettingsSection AppSettings = (AppSettingsSection) config.GetSection ("AppSettings");
			Console.Write ("AppSettings is " + (AppSettings == null ? "null" : "not null"));

			Console.WriteLine ();
		}
		catch (Exception e)
		{
			// Error.
			Console.WriteLine(e.ToString());
		}
	}
}
