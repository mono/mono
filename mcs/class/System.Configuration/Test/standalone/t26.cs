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

			connStrings.SectionInformation.UnprotectSection ();
			connStrings.SectionInformation.ForceSave = true;
			config.SaveAs ("t27.exe.config", ConfigurationSaveMode.Full);

			if (connStrings.SectionInformation.IsProtected == true)
				Console.WriteLine ("Section {0} is now protected by {1}",
						   connStrings.SectionInformation.Name,
						   connStrings.SectionInformation.ProtectionProvider.Name);
			else
				Console.WriteLine ("Section {0} is not protected", connStrings.SectionInformation.Name);
		}
		catch (Exception e) {
			Console.WriteLine ("{0} raised", e.GetType());
		}
	}
}
