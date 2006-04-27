using System;
using System.Configuration;
class TESt
{
	public static void Main ()
	{
		try {
			Configuration c = ConfigurationManager.OpenExeConfiguration (
				ConfigurationUserLevel.None);
			ApplicationSettingsGroup g = c.SectionGroups ["applicationSettings"]
				as ApplicationSettingsGroup;
			ConfigurationSection s = g.Sections ["test.Properties.Settings"];
			Console.WriteLine ("RequirePermission: {0}", s.SectionInformation.RequirePermission);
		} catch (Exception e) {
			Console.WriteLine (e);
		}
	}
}

