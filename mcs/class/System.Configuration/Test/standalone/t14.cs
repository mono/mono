using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using System.Configuration;
using System.Xml;

public class CustomCollection : KeyValueConfigurationCollection
{
	public CustomCollection () 
	{
		AddElementName = "insert";
		ClearElementName = "removeall";
		RemoveElementName = "delete";
	}
}

public class CustomSection :  ConfigurationSection
{
	public CustomSection()
	{
	}
  
	[ConfigurationProperty ("", Options = ConfigurationPropertyOptions.IsDefaultCollection)]
	public KeyValueConfigurationCollection Settings {
		get {
			if (settings == null)
				settings = new CustomCollection();
			return settings;
		}
	}

	protected override void DeserializeElement (XmlReader reader, bool serializeCollectionKey)
	{
		Settings.DeserializeElement (reader, serializeCollectionKey);
	}

	KeyValueConfigurationCollection settings;
}

class T1
{
	static void Main(string[] args)
	{
		try
		{
			Configuration config = ConfigurationManager.OpenExeConfiguration (ConfigurationUserLevel.None);
			CustomSection sect = (CustomSection)config.GetSection("customSection");

			foreach (string key in sect.Settings.AllKeys) {
				KeyValueConfigurationElement e = sect.Settings[key];
				Console.WriteLine ("{0} = {1}", e.Key, e.Value);
			}
		}
		catch (Exception e)
		{
			Console.WriteLine ("Exception raised: {0}\n{1}", e.GetType(), e);
		}
	}
}
