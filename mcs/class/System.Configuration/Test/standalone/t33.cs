using System;
using System.Configuration;
using System.Reflection;
using System.Xml;

class Test
{
	public static void Main ()
	{
		try {
			Configuration c = ConfigurationManager.OpenExeConfiguration (
				ConfigurationUserLevel.None);

			ConfigurationSectionGroup g = c.SectionGroups ["foo"];
			ConfigurationSection s = g.Sections ["mojoEncryption"];
			Console.WriteLine (s.GetType ());
		} catch (Exception e) {
			Console.WriteLine (e);
		}
	}
}

