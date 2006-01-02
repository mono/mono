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

			Console.WriteLine (sect.SectionInformation.GetRawXml ());

			foreach (string key in sect.Settings.AllKeys) {
				Console.WriteLine ("settings[{0}] = {1}", sect.Settings[key].Key, sect.Settings[key].Value);
			}
		}
		catch (Exception e)
		{
			Console.WriteLine ("Exception raised: {0}", e);
		}
	}
}
