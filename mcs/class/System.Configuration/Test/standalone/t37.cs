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

			ConnectionStringSettings cssc = ((ConnectionStringsSection)connStrings).ConnectionStrings["LocalSqlServer"];
			Console.WriteLine ("connStrings[LocalSqlServer] = {0}", (cssc == null ? "null" : cssc.ConnectionString));

			ConnectionStringSettings cssc2 = ((ConnectionStringsSection)connStrings).ConnectionStrings["AccessFileName"];
			Console.WriteLine ("connStrings[AccessFileName] = {0}", (cssc2 == null ? "null" : cssc2.ConnectionString));
		}
		catch (Exception e) {
			Console.WriteLine ("{0} raised", e.GetType());
		}
	}
}
