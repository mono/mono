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

			connStrings.SectionInformation.ProtectSection ("FooProvider");
		}
		catch (Exception e) {
			Console.WriteLine (e.Message);
		}
	}
}
