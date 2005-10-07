using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using System.Configuration;
using System.Web;

public class CustomSection :  ConfigurationSection
{
	public CustomSection()
	{
	}
  
	[LongValidator(MinValue = 1, MaxValue = 1000000,
		       ExcludeRange = false)]
	[ConfigurationProperty ("longSetting", DefaultValue=1000)]
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
			Configuration config = ConfigurationManager.OpenExeConfiguration (ConfigurationUserLevel.None);
			CustomSection sect = (CustomSection)config.GetSection("customSection");

			Console.WriteLine ("longSetting = {0}", sect.LongSetting);
		}
		catch (ConfigurationErrorsException e)
		{
			Console.WriteLine ("ConfigurationErrorsException raised");
		}
	}
}
