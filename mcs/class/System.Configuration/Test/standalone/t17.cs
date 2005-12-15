using System;
using System.Configuration;

class T1
{
	static void Main(string[] args)
	{
		try
		{
			Configuration config = ConfigurationManager.OpenExeConfiguration (ConfigurationUserLevel.None);
			AppSettingsSection sect = (AppSettingsSection)config.GetSection("appSettings");

			foreach (string key in sect.Settings.AllKeys) {
				KeyValueConfigurationElement e = sect.Settings[key];
				Console.WriteLine ("{0} = {1}", e.Key, e.Value);
			}

			Console.WriteLine ("lockAllElementsExcept = '{0}'", sect.LockAllElementsExcept.AttributeList);
		}
		catch (Exception e)
		{
			Console.WriteLine ("Exception raised: {0}\n{1}", e.GetType(), e);
		}
	}
}
