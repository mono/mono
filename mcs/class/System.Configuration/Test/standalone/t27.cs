using System;
using System.Collections.Specialized;
using System.Configuration;
using System.Xml;

class T1
{
	static void Main(string[] args)
	{
		try {
			System.Configuration.Configuration config = ConfigurationManager.OpenExeConfiguration (ConfigurationUserLevel.None);

			ConfigurationSection connStrings = config.ConnectionStrings;

			Console.WriteLine ("connStrings[LocalSqlServer] = {0}", ((ConnectionStringsSection)connStrings).ConnectionStrings["LocalSqlServer"]);
		}
		catch (Exception e) {
			Console.WriteLine ("{0} raised", e.GetType());
		}
	}
}
