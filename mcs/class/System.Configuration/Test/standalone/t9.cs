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
			ConnectionStringsSection sect = config.ConnectionStrings;
			ConnectionStringSettingsCollection connectionstrings = sect.ConnectionStrings;

			connectionstrings.Add (new ConnectionStringSettings ("fromtest", "connectionstringhere"));

			foreach (ConnectionStringSettings cs in connectionstrings) {
				Console.WriteLine ("connectionstring[{0}] = `{1}',`{2}'", cs.Name, cs.ProviderName, cs.ConnectionString);
			}
		}
		catch (Exception e)
		{
			// Error.
			Console.WriteLine(e.ToString());
		}
	}
}
