using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using System.Configuration;
using System.Web;

// test to see how we react when the default value for an attribute is
// of the wrong type.
public class CustomSection :  ConfigurationSection
{
	public CustomSection()
	{
	}
  
	[LongValidator(MinValue = 1, MaxValue = 1000000,
		       ExcludeRange = false)]
	[ConfigurationProperty ("longSetting", DefaultValue="wrong type")]
	public long LongSetting
	{
		get { return (long)this["longSetting"]; }
		set { this["longSetting"] = value; }
	}
}

class T1
{
	static void Main(string[] args)
	{
		try
		{
			Console.WriteLine ("1");
			Configuration config = ConfigurationManager.OpenExeConfiguration (ConfigurationUserLevel.None);
			Console.WriteLine ("2");
			CustomSection sect = (CustomSection)config.GetSection("customSection");

			Console.WriteLine ("longSetting = {0}", sect.LongSetting);
		}
		catch (Exception e)
		{
			Console.WriteLine ("Exception raised: {0}", e.GetType());
		}
	}
}
